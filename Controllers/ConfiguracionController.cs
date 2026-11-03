using InsecomCotizador.Data;
using InsecomCotizador.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InsecomCotizador.Controllers;

// margen e iva del calculo (solo se edita, hay una sola fila)
public class ConfiguracionController : Controller
{
    private readonly AppDbContext _db;

    public ConfiguracionController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var config = await _db.Configuraciones.FirstAsync();
        return View(config);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(Configuracion config)
    {
        if (!ModelState.IsValid)
            return View(config);

        _db.Configuraciones.Update(config);
        await _db.SaveChangesAsync();
        TempData["Mensaje"] = "Configuración guardada.";
        return RedirectToAction(nameof(Index));
    }
}
