using System.ComponentModel.DataAnnotations;

namespace InsecomCotizador.Models;

// empresas que venden los materiales
public class Proveedor
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150)]
    public string Nombre { get; set; } = "";

    [StringLength(150)]
    public string? Contacto { get; set; }

    public List<PrecioProveedor> Precios { get; set; } = new();
}
