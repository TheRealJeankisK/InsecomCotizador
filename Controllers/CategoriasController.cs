using InsecomCotizador.Data;
using InsecomCotizador.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InsecomCotizador.Controllers;

// crud de categorias de materiales
public class CategoriasController : Controller
{
    private readonly AppDbContext _db;

    public CategoriasController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        // Include para contar cuantos materiales tiene cada categoria
        return View(await _db.Categorias.Include(c => c.Materiales).OrderBy(c => c.Nombre).ToListAsync());
    }

    public IActionResult Crear() => View("Formulario", new Categoria());

    public async Task<IActionResult> Editar(int id)
    {
        var categoria = await _db.Categorias.FindAsync(id);
        if (categoria == null) return NotFound();
        return View("Formulario", categoria);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Guardar(Categoria categoria)
    {
        if (!ModelState.IsValid)
            return View("Formulario", categoria);

        if (categoria.Id == 0)
            _db.Categorias.Add(categoria);
        else
            _db.Categorias.Update(categoria);

        await _db.SaveChangesAsync();
        TempData["Mensaje"] = "Categoría guardada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var categoria = await _db.Categorias.FindAsync(id);
        if (categoria == null) return NotFound();

        try
        {
            _db.Categorias.Remove(categoria);
            await _db.SaveChangesAsync();
            TempData["Mensaje"] = "Categoría eliminada.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "No se puede eliminar: la categoría tiene materiales. Cámbialos de categoría primero.";
        }
        return RedirectToAction(nameof(Index));
    }
}
