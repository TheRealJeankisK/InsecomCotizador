using InsecomCotizador.Data;
using InsecomCotizador.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InsecomCotizador.Controllers;

// el core: arma la cotizacion, calcula el precio sugerido y al aprobar descuenta stock y saca la comision
public class CotizacionesController : Controller
{
    private readonly AppDbContext _db;

    public CotizacionesController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var lista = await _db.Cotizaciones
            .Include(c => c.Cliente).Include(c => c.Vendedor).Include(c => c.Detalles)
            .OrderByDescending(c => c.Fecha).ToListAsync();
        return View(lista);
    }

    // paso 1: elegir cliente y vendedor
    public async Task<IActionResult> Crear()
    {
        await CargarCombos();
        return View(new Cotizacion());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(Cotizacion cotizacion)
    {
        // valido en el back que existan (no confio en el dropdown)
        if (!await _db.Clientes.AnyAsync(c => c.Id == cotizacion.ClienteId))
            ModelState.AddModelError(nameof(Cotizacion.ClienteId), "El cliente no existe.");
        if (!await _db.Vendedores.AnyAsync(v => v.Id == cotizacion.VendedorId))
            ModelState.AddModelError(nameof(Cotizacion.VendedorId), "El vendedor no existe.");

        if (!ModelState.IsValid)
        {
            await CargarCombos();
            return View(cotizacion);
        }

        cotizacion.Fecha = DateTime.Now;
        cotizacion.Estado = EstadoCotizacion.Borrador;
        _db.Cotizaciones.Add(cotizacion);
        await _db.SaveChangesAsync();

        TempData["Mensaje"] = "Cotización creada. Ahora agrega los productos.";
        return RedirectToAction(nameof(Detalle), new { id = cotizacion.Id });
    }

    // paso 2: agregar productos y ver el calculo del core
    public async Task<IActionResult> Detalle(int id)
    {
        var cotizacion = await CargarCotizacion(id);
        if (cotizacion == null) return NotFound();

        // si es borrador calculo en vivo; si ya se aprobo muestro lo que se guardo
        if (cotizacion.Estado == EstadoCotizacion.Borrador)
            ViewBag.Resultado = await Calcular(cotizacion);

        ViewBag.Productos = new SelectList(
            await _db.Productos.Where(p => p.Receta.Any()).OrderBy(p => p.Nombre).ToListAsync(), "Id", "Nombre");
        return View(cotizacion);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarProducto(int cotizacionId, int productoId, int cantidad)
    {
        var cotizacion = await _db.Cotizaciones.Include(c => c.Detalles).FirstOrDefaultAsync(c => c.Id == cotizacionId);
        if (cotizacion == null) return NotFound();

        if (cotizacion.Estado != EstadoCotizacion.Borrador)
        {
            TempData["Error"] = "Solo se pueden cambiar las cotizaciones en borrador.";
        }
        else if (!await _db.Productos.AnyAsync(p => p.Id == productoId))
        {
            TempData["Error"] = "Elige un producto.";
        }
        else if (cantidad < 1 || cantidad > 10000)
        {
            TempData["Error"] = "La cantidad debe estar entre 1 y 10000.";
        }
        else
        {
            // si el producto ya esta, sumo la cantidad en vez de repetir la fila
            var existente = cotizacion.Detalles.FirstOrDefault(d => d.ProductoId == productoId);
            if (existente != null)
                existente.Cantidad += cantidad;
            else
                cotizacion.Detalles.Add(new DetalleCotizacion { ProductoId = productoId, Cantidad = cantidad });

            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Detalle), new { id = cotizacionId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuitarProducto(int id)
    {
        var detalle = await _db.DetallesCotizacion.Include(d => d.Cotizacion).FirstOrDefaultAsync(d => d.Id == id);
        if (detalle == null) return NotFound();

        if (detalle.Cotizacion!.Estado == EstadoCotizacion.Borrador)
        {
            _db.DetallesCotizacion.Remove(detalle);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Detalle), new { id = detalle.CotizacionId });
    }

    // paso 3: aprobar = guardo el resultado, descuento stock y queda la comision del vendedor
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Aprobar(int id)
    {
        var cotizacion = await CargarCotizacion(id);
        if (cotizacion == null) return NotFound();

        if (cotizacion.Estado != EstadoCotizacion.Borrador)
        {
            TempData["Error"] = "Esta cotización ya no está en borrador.";
            return RedirectToAction(nameof(Detalle), new { id });
        }

        // vuelvo a calcular aqui (el stock pudo cambiar desde que se abrio la pagina)
        var r = await Calcular(cotizacion);
        if (r.Problemas.Any())
        {
            TempData["Error"] = "No se puede aprobar: " + string.Join(" ", r.Problemas);
            return RedirectToAction(nameof(Detalle), new { id });
        }

        // descuento de bodega lo que el calculo dijo que sale de bodega
        foreach (var linea in r.Productos)
        {
            foreach (var m in linea.Materiales)
            {
                if (m.DeBodega > 0)
                {
                    var material = await _db.Materiales.FindAsync(m.MaterialId);
                    material!.Stock -= m.DeBodega;
                }
            }
        }

        // guardo el precio de cada producto para que no cambie si luego suben los costos
        foreach (var d in cotizacion.Detalles)
        {
            foreach (var linea in r.Productos)
            {
                if (linea.DetalleId == d.Id)
                    d.PrecioUnitario = linea.PrecioUnitario;
            }
        }

        cotizacion.Costo = r.Costo;
        cotizacion.Subtotal = r.Subtotal;
        cotizacion.Total = r.Total;
        cotizacion.Comision = r.Comision;
        cotizacion.PlazoDias = r.PlazoDias;
        cotizacion.Estado = EstadoCotizacion.Aprobada;
        await _db.SaveChangesAsync();

        TempData["Mensaje"] = $"Cotización aprobada. Se descontó el stock y la comisión de {cotizacion.Vendedor!.Nombre} es ${r.Comision:N2}.";
        return RedirectToAction(nameof(Detalle), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rechazar(int id)
    {
        var cotizacion = await _db.Cotizaciones.FindAsync(id);
        if (cotizacion == null) return NotFound();

        if (cotizacion.Estado == EstadoCotizacion.Borrador)
        {
            cotizacion.Estado = EstadoCotizacion.Rechazada;
            await _db.SaveChangesAsync();
            TempData["Mensaje"] = "Cotización rechazada.";
        }
        return RedirectToAction(nameof(Detalle), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var cotizacion = await _db.Cotizaciones.FindAsync(id);
        if (cotizacion == null) return NotFound();

        // una aprobada ya desconto stock, por eso no la dejo borrar
        if (cotizacion.Estado == EstadoCotizacion.Aprobada)
        {
            TempData["Error"] = "No se puede eliminar una cotización aprobada.";
        }
        else
        {
            _db.Cotizaciones.Remove(cotizacion);
            await _db.SaveChangesAsync();
            TempData["Mensaje"] = "Cotización eliminada.";
        }
        return RedirectToAction(nameof(Index));
    }

    // ================= CORE =================
    // por cada producto -> por cada material de su receta -> si hay stock uso bodega, si no el proveedor mas barato
    // luego costo + margen + iva y la comision del vendedor segun el rango
    private async Task<ResultadoCotizacion> Calcular(Cotizacion cotizacion)
    {
        var r = new ResultadoCotizacion();

        var config = await _db.Configuraciones.FirstOrDefaultAsync();
        r.MargenPct = config?.Margen ?? 0;
        r.IvaPct = config?.IVA ?? 0;

        if (!cotizacion.Detalles.Any())
            r.Problemas.Add("La cotización no tiene productos.");

        // stock que me queda libre mientras recorro (si 2 productos usan el mismo material no lo cuento 2 veces)
        var disponible = new Dictionary<int, decimal>();

        foreach (var detalle in cotizacion.Detalles)
        {
            var producto = detalle.Producto!;
            var linea = new LineaProducto
            {
                DetalleId = detalle.Id, ProductoId = producto.Id, Nombre = producto.Nombre, Cantidad = detalle.Cantidad
            };

            if (!producto.Receta.Any())
                r.Problemas.Add($"{producto.Nombre} no tiene receta.");

            foreach (var item in producto.Receta)
            {
                var material = item.Material!;
                if (!disponible.ContainsKey(material.Id))
                    disponible[material.Id] = material.Stock;

                var lm = new LineaMaterial
                {
                    MaterialId = material.Id, Nombre = material.Nombre, Unidad = material.Unidad,
                    Necesito = item.CantidadPorUnidad * detalle.Cantidad,
                    CostoBodega = material.CostoUnitario
                };

                if (disponible[material.Id] >= lm.Necesito)
                {
                    // alcanza la bodega
                    lm.DeBodega = lm.Necesito;
                }
                else
                {
                    // uso lo que haya y el resto se compra
                    lm.DeBodega = disponible[material.Id];
                    lm.AComprar = lm.Necesito - lm.DeBodega;

                    // busco el proveedor mas barato para este material
                    PrecioProveedor? mejor = null;
                    foreach (var precio in material.Precios)
                    {
                        if (mejor == null || precio.Precio < mejor.Precio)
                            mejor = precio;
                    }

                    if (mejor == null)
                    {
                        r.Problemas.Add($"No hay stock suficiente de {material.Nombre} y ningún proveedor lo vende.");
                    }
                    else
                    {
                        lm.Proveedor = mejor.Proveedor!.Nombre;
                        lm.PrecioProveedor = mejor.Precio;
                        lm.DiasEntrega = mejor.DiasEntrega;

                        // el plazo de la cotizacion es la entrega mas larga
                        if (mejor.DiasEntrega > r.PlazoDias)
                            r.PlazoDias = mejor.DiasEntrega;
                    }
                }

                disponible[material.Id] -= lm.DeBodega;
                lm.Subtotal = lm.DeBodega * lm.CostoBodega + lm.AComprar * lm.PrecioProveedor;
                linea.Costo += lm.Subtotal;
                linea.Materiales.Add(lm);
            }

            // precio de 1 unidad con el margen
            linea.PrecioUnitario = Math.Round(linea.Costo * (1 + r.MargenPct / 100) / detalle.Cantidad, 2);
            r.Costo += linea.Costo;
            r.Productos.Add(linea);
        }

        r.ValorMargen = Math.Round(r.Costo * r.MargenPct / 100, 2);
        r.Subtotal = Math.Round(r.Costo + r.ValorMargen, 2);
        r.ValorIva = Math.Round(r.Subtotal * r.IvaPct / 100, 2);
        r.Total = r.Subtotal + r.ValorIva;

        // comision: busco en que rango cae el subtotal (como en el minicore)
        var reglas = await _db.ReglasComision.OrderBy(x => x.MontoMinimo).ToListAsync();
        foreach (var regla in reglas)
        {
            if (r.Subtotal >= regla.MontoMinimo && (regla.MontoMaximo == null || r.Subtotal < regla.MontoMaximo))
                r.ComisionPct = regla.Porcentaje;
        }
        r.Comision = Math.Round(r.Subtotal * r.ComisionPct / 100, 2);

        return r;
    }

    // la cotizacion con todo lo que necesita el core (cliente, vendedor, productos, recetas, materiales y precios)
    private Task<Cotizacion?> CargarCotizacion(int id)
    {
        return _db.Cotizaciones
            .Include(c => c.Cliente)
            .Include(c => c.Vendedor)
            .Include(c => c.Detalles).ThenInclude(d => d.Producto).ThenInclude(p => p!.Receta).ThenInclude(r => r.Material).ThenInclude(m => m!.Precios).ThenInclude(p => p.Proveedor)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    private async Task CargarCombos()
    {
        ViewBag.Clientes = new SelectList(await _db.Clientes.OrderBy(c => c.RazonSocial).ToListAsync(), "Id", "RazonSocial");
        ViewBag.Vendedores = new SelectList(await _db.Vendedores.OrderBy(v => v.Nombre).ToListAsync(), "Id", "Nombre");
    }
}
