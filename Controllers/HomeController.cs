using System.Diagnostics;
using InsecomCotizador.Data;
using InsecomCotizador.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsecomCotizador.Controllers;

// panel de inicio con el resumen de las tablas
public class HomeController : Controller
{
    private readonly AppDbContext _db;

    public HomeController(AppDbContext db)
    {
        _db = db;
    }

    public IActionResult Index()
    {
        ViewBag.Clientes = _db.Clientes.Count();
        ViewBag.Vendedores = _db.Vendedores.Count();
        ViewBag.Productos = _db.Productos.Count();
        ViewBag.Materiales = _db.Materiales.Count();
        ViewBag.Proveedores = _db.Proveedores.Count();
        ViewBag.Reglas = _db.ReglasComision.Count();
        // materiales que se acabaron (para avisar)
        ViewBag.SinStock = _db.Materiales.Count(m => m.Stock == 0);
        return View();
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
