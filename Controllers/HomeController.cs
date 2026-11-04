using System.Diagnostics;
using InsecomCotizador.Data;
using InsecomCotizador.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InsecomCotizador.Controllers;

// panel de inicio con el resumen de las tablas
public class HomeController : Controller
{
    private readonly AppDbContext _db;

    // debajo de esta cantidad aviso que hay poco stock
    private const decimal StockMinimo = 5;

    public HomeController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var panel = new PanelViewModel
        {
            Clientes = await _db.Clientes.CountAsync(),
            Vendedores = await _db.Vendedores.CountAsync(),
            Productos = await _db.Productos.CountAsync(),
            Materiales = await _db.Materiales.CountAsync(),
            Proveedores = await _db.Proveedores.CountAsync(),
            Reglas = await _db.ReglasComision.CountAsync(),
            Categorias = await _db.Categorias.CountAsync()
        };

        var config = await _db.Configuraciones.FirstOrDefaultAsync();
        panel.Margen = config?.Margen ?? 0;
        panel.IVA = config?.IVA ?? 0;

        // inventario: valor total, stock bajo y resumen por categoria
        var materiales = await _db.Materiales.Include(m => m.Categoria).OrderBy(m => m.Stock).ToListAsync();
        var categorias = new Dictionary<string, ResumenCategoria>();
        foreach (var m in materiales)
        {
            decimal valor = m.Stock * m.CostoUnitario;
            panel.ValorInventario += valor;

            if (m.Stock <= StockMinimo)
                panel.StockBajo.Add(m);

            // acumulo por categoria
            string nombre = m.Categoria?.Nombre ?? "Sin categoría";
            if (!categorias.ContainsKey(nombre))
                categorias[nombre] = new ResumenCategoria { Nombre = nombre };
            categorias[nombre].Materiales++;
            categorias[nombre].Valor += valor;
        }
        panel.PorCategoria = categorias.Values.OrderByDescending(c => c.Valor).ToList();

        // costo de materiales de 1 unidad de cada producto (recorro su receta)
        var productos = await _db.Productos.Include(p => p.Receta).ThenInclude(r => r.Material).OrderBy(p => p.Nombre).ToListAsync();
        foreach (var p in productos)
        {
            decimal costo = 0;
            foreach (var r in p.Receta)
                costo += r.CantidadPorUnidad * (r.Material?.CostoUnitario ?? 0);

            panel.CostosProductos.Add(new ResumenProducto
            {
                Id = p.Id, Nombre = p.Nombre, Tipo = p.Tipo, NumMateriales = p.Receta.Count, Costo = costo
            });
        }

        // lo que hace falta registrar para que el core pueda cotizar
        bool hayRecetas = productos.Any(p => p.Receta.Count > 0);
        panel.Pasos = new()
        {
            ("Registrar categorías de materiales", panel.Categorias > 0, "Categorias"),
            ("Registrar materiales con stock y costo", panel.Materiales > 0, "Materiales"),
            ("Registrar proveedores y sus precios", panel.Proveedores > 0, "Proveedores"),
            ("Crear productos con su receta", hayRecetas, "Productos"),
            ("Registrar clientes", panel.Clientes > 0, "Clientes"),
            ("Registrar vendedores", panel.Vendedores > 0, "Vendedores"),
            ("Definir reglas de comisión", panel.Reglas > 0, "ReglasComision")
        };

        return View(panel);
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
