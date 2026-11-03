using System.Globalization;
using InsecomCotizador.Data;
using InsecomCotizador.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// render manda el puerto por la variable PORT (en local no existe)
var puerto = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(puerto))
    builder.WebHost.UseUrls($"http://0.0.0.0:{puerto}");

// decimales con punto (en es-EC el decimal es coma y los input number mandan punto)
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

// aqui pongo [Authorize] a todos los controladores de una (solo el login queda libre con AllowAnonymous)
builder.Services.AddControllersWithViews(o =>
    o.Filters.Add(new AuthorizeFilter(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build())));

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// cifrado de contraseñas (PBKDF2 con salt)
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

// login con cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Cuenta/Login";
        o.LogoutPath = "/Cuenta/Logout";
        o.ExpireTimeSpan = TimeSpan.FromHours(2);
        o.SlidingExpiration = true;
    });

var app = builder.Build();

// crear la bd, el admin inicial y la configuracion por defecto (todo lo demas se llena desde el admin)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Usuario>>();
    db.Database.EnsureCreated();

    if (!db.Usuarios.Any())
    {
        var datos = builder.Configuration.GetSection("AdminInicial");
        var admin = new Usuario { NombreUsuario = datos["NombreUsuario"]!, Rol = "Administrador" };
        admin.ContrasenaHash = hasher.HashPassword(admin, datos["Contrasena"]!);
        db.Usuarios.Add(admin);
    }

    if (!db.Configuraciones.Any())
        db.Configuraciones.Add(new Configuracion { Margen = 30, IVA = 15 });

    db.SaveChanges();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseRouting();

// primero authentication y luego authorization (el orden importa)
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
