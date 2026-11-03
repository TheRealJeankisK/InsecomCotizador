using InsecomCotizador.Data;
using InsecomCotizador.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InsecomCotizador.Controllers;

// crud de clientes
public class ClientesController : Controller
{
    private readonly AppDbContext _db;

    public ClientesController(AppDbContext db)
    {
        _db = db;
    }

    // listar (con buscador por nombre o ruc)
    public async Task<IActionResult> Index(string? buscar)
    {
        var consulta = _db.Clientes.AsQueryable();
        if (!string.IsNullOrWhiteSpace(buscar))
            consulta = consulta.Where(c => c.RazonSocial.Contains(buscar) || c.RUC.Contains(buscar));

        ViewBag.Buscar = buscar;
        return View(await consulta.OrderBy(c => c.RazonSocial).ToListAsync());
    }

    // crear y editar usan la misma vista (Formulario)
    public IActionResult Crear() => View("Formulario", new Cliente());

    public async Task<IActionResult> Editar(int id)
    {
        var cliente = await _db.Clientes.FindAsync(id);
        if (cliente == null) return NotFound();
        return View("Formulario", cliente);
    }

    // guardar sirve para los dos (si Id es 0 es nuevo)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Guardar(Cliente cliente)
    {
        // validacion en el back del dato sensible: el ruc tiene que ser un ruc ecuatoriano valido
        // (aunque alguien se salte el html o el js, aqui no se graba si esta mal)
        var errorRuc = ValidarRuc(cliente.RUC);
        if (errorRuc != null)
            ModelState.AddModelError(nameof(cliente.RUC), errorRuc);

        // ruc repetido
        if (await _db.Clientes.AnyAsync(c => c.RUC == cliente.RUC && c.Id != cliente.Id))
            ModelState.AddModelError(nameof(cliente.RUC), "Ya existe un cliente con ese RUC.");

        if (!ModelState.IsValid)
            return View("Formulario", cliente);

        if (cliente.Id == 0)
            _db.Clientes.Add(cliente);
        else
            _db.Clientes.Update(cliente);

        await _db.SaveChangesAsync();
        TempData["Mensaje"] = "Cliente guardado.";
        return RedirectToAction(nameof(Index));
    }

    // borrar por post (un link get nunca deberia borrar)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var cliente = await _db.Clientes.FindAsync(id);
        if (cliente == null) return NotFound();

        try
        {
            _db.Clientes.Remove(cliente);
            await _db.SaveChangesAsync();
            TempData["Mensaje"] = "Cliente eliminado.";
        }
        catch (DbUpdateException)
        {
            // tiene cotizaciones (restrict en el AppDbContext)
            TempData["Error"] = "No se puede eliminar: el cliente tiene cotizaciones.";
        }
        return RedirectToAction(nameof(Index));
    }

    // reglas del ruc de ecuador: devuelve el error o null si esta bien
    private static string? ValidarRuc(string? ruc)
    {
        // 13 digitos, solo numeros
        if (string.IsNullOrEmpty(ruc) || ruc.Length != 13 || !ruc.All(char.IsDigit))
            return "El RUC debe tener 13 dígitos.";

        // los 2 primeros son la provincia (01 a 24, o 30 para extranjeros)
        int provincia = int.Parse(ruc.Substring(0, 2));
        if ((provincia < 1 || provincia > 24) && provincia != 30)
            return "Los 2 primeros dígitos del RUC no son una provincia válida.";

        // el tercero dice el tipo: 0-5 persona natural, 6 empresa publica, 9 empresa privada
        int tercero = ruc[2] - '0';
        if (tercero == 7 || tercero == 8)
            return "El tercer dígito del RUC no es válido.";

        // termina en el numero de establecimiento (no puede ser 000)
        if (ruc.EndsWith("000"))
            return "El RUC no puede terminar en 000.";

        // persona natural: los 10 primeros son la cedula y se comprueba su digito verificador (modulo 10)
        if (tercero < 6)
        {
            int suma = 0;
            for (int i = 0; i < 9; i++)
            {
                int digito = ruc[i] - '0';
                if (i % 2 == 0)
                {
                    digito *= 2;  // las posiciones impares se multiplican por 2
                    if (digito > 9) digito -= 9;
                }
                suma += digito;
            }
            int verificador = (10 - suma % 10) % 10;
            if (verificador != ruc[9] - '0')
                return "El RUC no es válido (falla el dígito verificador de la cédula).";
        }

        return null;
    }
}
