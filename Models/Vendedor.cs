using System.ComponentModel.DataAnnotations;

namespace InsecomCotizador.Models;

// personas del area comercial que hacen las cotizaciones
public class Vendedor
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100)]
    public string Nombre { get; set; } = "";

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no es válido.")]
    [StringLength(150)]
    public string Correo { get; set; } = "";

    public List<Cotizacion> Cotizaciones { get; set; } = new();
}
