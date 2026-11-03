using System.Security.Claims;
using InsecomCotizador.Data;
using InsecomCotizador.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InsecomCotizador.Controllers;

// login y logout
public class CuentaController : Controller
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<Usuario> _hasher;

    public CuentaController(AppDbContext db, IPasswordHasher<Usuario> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    // mostrar el form (AllowAnonymous porque todo lo demas pide login)
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (!string.IsNullOrEmpty(returnUrl))
            ViewBag.Aviso = "Inicia sesión para entrar a esa sección.";

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel modelo)
    {
        if (!ModelState.IsValid)
            return View(modelo);

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == modelo.NombreUsuario);

        // aqui comparo la contraseña escrita con el hash guardado
        var correcta = usuario != null &&
            _hasher.VerifyHashedPassword(usuario, usuario.ContrasenaHash, modelo.Contrasena) != PasswordVerificationResult.Failed;

        if (!correcta)
        {
            ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
            return View(modelo);
        }

        // datos que van en la cookie (el rol sirve para dar permisos despues)
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario!.Id.ToString()),
            new(ClaimTypes.Name, usuario.NombreUsuario),
            new(ClaimTypes.Role, usuario.Rol)
        };
        var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identidad));

        // IsLocalUrl para que no redirija a otra pagina externa
        if (Url.IsLocalUrl(modelo.ReturnUrl))
            return Redirect(modelo.ReturnUrl!);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }
}
