using System.ComponentModel.DataAnnotations;

namespace InsecomCotizador.Models;

// empresa que pide la cotizacion
public class Cliente
{
    public int Id { get; set; }

    [Required(ErrorMessage = "La razón social es obligatoria.")]
    [StringLength(150)]
    [Display(Name = "Razón social")]
    public string RazonSocial { get; set; } = "";

    // ruc de ecuador (13 digitos)
    [Required(ErrorMessage = "El RUC es obligatorio.")]
    [RegularExpression(@"^\d{13}$", ErrorMessage = "El RUC debe tener 13 dígitos.")]
    public string RUC { get; set; } = "";

    [Required(ErrorMessage = "El sector es obligatorio.")]
    [StringLength(50)]
    public string Sector { get; set; } = "";

    [StringLength(100)]
    public string? Contacto { get; set; }

    [StringLength(20)]
    [Display(Name = "Teléfono")]
    public string? Telefono { get; set; }

    [EmailAddress(ErrorMessage = "El correo no es válido.")]
    [StringLength(150)]
    public string? Correo { get; set; }

    public List<Cotizacion> Cotizaciones { get; set; } = new();
}
