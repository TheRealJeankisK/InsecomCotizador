using System.ComponentModel.DataAnnotations;

namespace InsecomCotizador.Models;

// datos del form de login
public class LoginViewModel
{
    [Required(ErrorMessage = "Escribe tu usuario.")]
    [Display(Name = "Usuario")]
    public string NombreUsuario { get; set; } = "";

    [Required(ErrorMessage = "Escribe tu contraseña.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Contrasena { get; set; } = "";

    public string? ReturnUrl { get; set; }
}
