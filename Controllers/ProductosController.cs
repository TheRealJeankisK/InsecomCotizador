using InsecomCotizador.Data;
using InsecomCotizador.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InsecomCotizador.Controllers;

// crud de productos + su receta de materiales
public class ProductosController : Controller
{
    private readonly AppDbContext _db;

    public ProductosController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        // Include para mostrar cuantos materiales tiene cada receta
        return View(await _db.Productos.Include(p => p.Receta).OrderBy(p => p.Nombre).ToListAsync());
    }

    public IActionResult Crear() => View("Formulario", new Producto());

    public async Task<IActionResult> Editar(int id)
    {
        var producto = await _db.Productos.FindAsync(id);
        if (producto == null) return NotFound();
        return View("Formulario", producto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Guardar(Producto producto)
    {
        if (!ModelState.IsValid)
            return View("Formulario", producto);

        var esNuevo = producto.Id == 0;
        if (esNuevo)
            _db.Productos.Add(producto);
        else
            _db.Productos.Update(producto);

        await _db.SaveChangesAsync();
        TempData["Mensaje"] = "Producto guardado.";

        // si es nuevo lo mando directo a armar su receta
        return esNuevo ? RedirectToAction(nameof(Receta), new { id = producto.Id }) : RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var producto = await _db.Productos.FindAsync(id);
        if (producto == null) return NotFound();

        try
        {
            // la receta se borra sola (cascade)
            _db.Productos.Remove(producto);
            await _db.SaveChangesAsync();
            TempData["Mensaje"] = "Producto eliminado.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "No se puede eliminar: el producto está en cotizaciones.";
        }
        return RedirectToAction(nameof(Index));
    }

    // ---------- receta ----------

    // ver la receta del producto y el form para agregar materiales
    public async Task<IActionResult> Receta(int id)
    {
        var producto = await _db.Productos
            .Include(p => p.Receta).ThenInclude(r => r.Material)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (producto == null) return NotFound();

        // primer dropdown: categorias que tienen materiales (el de materiales se llena segun la categoria)
        ViewBag.Categorias = new SelectList(await _db.Categorias.Where(c => c.Materiales.Any()).OrderBy(c => c.Nombre).ToListAsync(), "Id", "Nombre");
        return View(producto);
    }

    // dropdown dependiente: devuelve en json los materiales de una categoria (lo pide el fetch de la vista)
    public async Task<IActionResult> MaterialesPorCategoria(int categoriaId)
    {
        var materiales = await _db.Materiales
            .Where(m => m.CategoriaId == categoriaId)
            .OrderBy(m => m.Nombre)
            .Select(m => new { id = m.Id, nombre = m.Nombre + " (" + m.Unidad + ")" })
            .ToListAsync();
        return Json(materiales);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarMaterial(ProductoMaterial item)
    {
        // validacion en el back: el material tiene que existir (no confio en lo que manda el form)
        if (!await _db.Materiales.AnyAsync(m => m.Id == item.MaterialId))
            ModelState.AddModelError(nameof(item.MaterialId), "Material no válido.");

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Elige un material y una cantidad mayor a 0.";
            return RedirectToAction(nameof(Receta), new { id = item.ProductoId });
        }

        // si el material ya esta en la receta solo cambio la cantidad
        var existente = await _db.ProductoMateriales
            .FirstOrDefaultAsync(r => r.ProductoId == item.ProductoId && r.MaterialId == item.MaterialId);

        if (existente != null)
            existente.CantidadPorUnidad = item.CantidadPorUnidad;
        else
            _db.ProductoMateriales.Add(item);

        await _db.SaveChangesAsync();
        TempData["Mensaje"] = "Receta actualizada.";
        return RedirectToAction(nameof(Receta), new { id = item.ProductoId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuitarMaterial(int id)
    {
        var item = await _db.ProductoMateriales.FindAsync(id);
        if (item == null) return NotFound();

        _db.ProductoMateriales.Remove(item);
        await _db.SaveChangesAsync();
        TempData["Mensaje"] = "Material quitado de la receta.";
        return RedirectToAction(nameof(Receta), new { id = item.ProductoId });
    }
}
