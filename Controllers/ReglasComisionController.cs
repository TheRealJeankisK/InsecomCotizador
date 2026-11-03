using InsecomCotizador.Data;
using InsecomCotizador.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InsecomCotizador.Controllers;

// crud de reglas de comision (rangos)
public class ReglasComisionController : Controller
{
    private readonly AppDbContext _db;

    public ReglasComisionController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _db.ReglasComision.OrderBy(r => r.MontoMinimo).ToListAsync());
    }

    public IActionResult Crear() => View("Formulario", new ReglaComision());

    public async Task<IActionResult> Editar(int id)
    {
        var regla = await _db.ReglasComision.FindAsync(id);
        if (regla == null) return NotFound();
        return View("Formulario", regla);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Guardar(ReglaComision regla)
    {
        // el maximo tiene que ser mayor que el minimo
        if (regla.MontoMaximo != null && regla.MontoMaximo <= regla.MontoMinimo)
            ModelState.AddModelError(nameof(regla.MontoMaximo), "El monto máximo debe ser mayor que el mínimo.");

        // aqui reviso que el rango no se cruce con otra regla
        var otras = await _db.ReglasComision.Where(r => r.Id != regla.Id).ToListAsync();
        foreach (var otra in otras)
        {
            var finNueva = regla.MontoMaximo ?? decimal.MaxValue;
            var finOtra = otra.MontoMaximo ?? decimal.MaxValue;
            if (regla.MontoMinimo < finOtra && otra.MontoMinimo < finNueva)
            {
                ModelState.AddModelError("", $"El rango se cruza con la regla de {otra.MontoMinimo:N2} en adelante.");
                break;
            }
        }

        if (!ModelState.IsValid)
            return View("Formulario", regla);

        if (regla.Id == 0)
            _db.ReglasComision.Add(regla);
        else
            _db.ReglasComision.Update(regla);

        await _db.SaveChangesAsync();
        TempData["Mensaje"] = "Regla guardada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var regla = await _db.ReglasComision.FindAsync(id);
        if (regla == null) return NotFound();

        _db.ReglasComision.Remove(regla);
        await _db.SaveChangesAsync();
        TempData["Mensaje"] = "Regla eliminada.";
        return RedirectToAction(nameof(Index));
    }
}
