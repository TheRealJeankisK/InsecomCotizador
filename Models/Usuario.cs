using System.ComponentModel.DataAnnotations;

namespace InsecomCotizador.Models;

// usuarios del sistema (login)
public class Usuario
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    [Display(Name = "Usuario")]
    public string NombreUsuario { get; set; } = "";

    // aqui va el hash, nunca la contraseña en texto plano
    public string ContrasenaHash { get; set; } = "";

    // Administrador o Vendedor
    [Required, StringLength(20)]
    public string Rol { get; set; } = "Administrador";
}
