using InsecomCotizador.Data;
using InsecomCotizador.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InsecomCotizador.Controllers;

// crud de materiales (inventario) + precios de cada proveedor
public class MaterialesController : Controller
{
    private readonly AppDbContext _db;

    public MaterialesController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _db.Materiales.Include(m => m.Precios).Include(m => m.Categoria).OrderBy(m => m.Nombre).ToListAsync());
    }

    public async Task<IActionResult> Crear()
    {
        await CargarCategorias();
        return View("Formulario", new Material());
    }

    public async Task<IActionResult> Editar(int id)
    {
        var material = await _db.Materiales.FindAsync(id);
        if (material == null) return NotFound();
        await CargarCategorias();
        return View("Formulario", material);
    }

    // lleno el dropdown de categorias (la fk se elige, no se escribe)
    private async Task CargarCategorias()
    {
        ViewBag.Categorias = new SelectList(await _db.Categorias.OrderBy(c => c.Nombre).ToListAsync(), "Id", "Nombre");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Guardar(Material material)
    {
        // validacion en el back: la categoria tiene que existir de verdad en la bd
        if (!await _db.Categorias.AnyAsync(c => c.Id == material.CategoriaId))
            ModelState.AddModelError(nameof(material.CategoriaId), "Elige una categoría válida.");

        if (!ModelState.IsValid)
        {
            await CargarCategorias();
            return View("Formulario", material);
        }

        if (material.Id == 0)
            _db.Materiales.Add(material);
        else
            _db.Materiales.Update(material);

        await _db.SaveChangesAsync();
        TempData["Mensaje"] = "Material guardado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var material = await _db.Materiales.FindAsync(id);
        if (material == null) return NotFound();

        try
        {
            // sus precios se borran solos (cascade)
            _db.Materiales.Remove(material);
            await _db.SaveChangesAsync();
            TempData["Mensaje"] = "Material eliminado.";
        }
        catch (DbUpdateException)
        {
            // esta en la receta de algun producto (restrict)
            TempData["Error"] = "No se puede eliminar: el material está en la receta de un producto.";
        }
        return RedirectToAction(nameof(Index));
    }

    // ---------- precios por proveedor ----------

    public async Task<IActionResult> Precios(int id)
    {
        var material = await _db.Materiales
            .Include(m => m.Precios).ThenInclude(p => p.Proveedor)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (material == null) return NotFound();

        ViewBag.Proveedores = new SelectList(await _db.Proveedores.OrderBy(p => p.Nombre).ToListAsync(), "Id", "Nombre");
        return View(material);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarPrecio(PrecioProveedor nuevo)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Elige un proveedor, un precio mayor a 0 y los días de entrega.";
            return RedirectToAction(nameof(Precios), new { id = nuevo.MaterialId });
        }

        // si el proveedor ya tenia precio para este material lo actualizo
        var existente = await _db.PreciosProveedor
            .FirstOrDefaultAsync(p => p.MaterialId == nuevo.MaterialId && p.ProveedorId == nuevo.ProveedorId);

        if (existente != null)
        {
            existente.Precio = nuevo.Precio;
            existente.DiasEntrega = nuevo.DiasEntrega;
        }
        else
        {
            _db.PreciosProveedor.Add(nuevo);
        }

        await _db.SaveChangesAsync();
        TempData["Mensaje"] = "Precio guardado.";
        return RedirectToAction(nameof(Precios), new { id = nuevo.MaterialId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuitarPrecio(int id)
    {
        var precio = await _db.PreciosProveedor.FindAsync(id);
        if (precio == null) return NotFound();

        _db.PreciosProveedor.Remove(precio);
        await _db.SaveChangesAsync();
        TempData["Mensaje"] = "Precio eliminado.";
        return RedirectToAction(nameof(Precios), new { id = precio.MaterialId });
    }
}
