using InsecomCotizador.Data;
using InsecomCotizador.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InsecomCotizador.Controllers;

// crud de vendedores
public class VendedoresController : Controller
{
    private readonly AppDbContext _db;

    public VendedoresController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _db.Vendedores.OrderBy(v => v.Nombre).ToListAsync());
    }

    public IActionResult Crear() => View("Formulario", new Vendedor());

    public async Task<IActionResult> Editar(int id)
    {
        var vendedor = await _db.Vendedores.FindAsync(id);
        if (vendedor == null) return NotFound();
        return View("Formulario", vendedor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Guardar(Vendedor vendedor)
    {
        if (!ModelState.IsValid)
            return View("Formulario", vendedor);

        if (vendedor.Id == 0)
            _db.Vendedores.Add(vendedor);
        else
            _db.Vendedores.Update(vendedor);

        await _db.SaveChangesAsync();
        TempData["Mensaje"] = "Vendedor guardado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var vendedor = await _db.Vendedores.FindAsync(id);
        if (vendedor == null) return NotFound();

        try
        {
            _db.Vendedores.Remove(vendedor);
            await _db.SaveChangesAsync();
            TempData["Mensaje"] = "Vendedor eliminado.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "No se puede eliminar: el vendedor tiene cotizaciones.";
        }
        return RedirectToAction(nameof(Index));
    }
}
