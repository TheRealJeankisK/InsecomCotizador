using InsecomCotizador.Data;
using InsecomCotizador.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InsecomCotizador.Controllers;

// crud de proveedores
public class ProveedoresController : Controller
{
    private readonly AppDbContext _db;

    public ProveedoresController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        // Include para contar cuantos materiales tiene precio con cada proveedor
        return View(await _db.Proveedores.Include(p => p.Precios).OrderBy(p => p.Nombre).ToListAsync());
    }

    public IActionResult Crear() => View("Formulario", new Proveedor());

    public async Task<IActionResult> Editar(int id)
    {
        var proveedor = await _db.Proveedores.FindAsync(id);
        if (proveedor == null) return NotFound();
        return View("Formulario", proveedor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Guardar(Proveedor proveedor)
    {
        if (!ModelState.IsValid)
            return View("Formulario", proveedor);

        if (proveedor.Id == 0)
            _db.Proveedores.Add(proveedor);
        else
            _db.Proveedores.Update(proveedor);

        await _db.SaveChangesAsync();
        TempData["Mensaje"] = "Proveedor guardado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var proveedor = await _db.Proveedores.FindAsync(id);
        if (proveedor == null) return NotFound();

        try
        {
            _db.Proveedores.Remove(proveedor);
            await _db.SaveChangesAsync();
            TempData["Mensaje"] = "Proveedor eliminado.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "No se puede eliminar: el proveedor tiene precios registrados. Quítalos primero desde Materiales.";
        }
        return RedirectToAction(nameof(Index));
    }
}
