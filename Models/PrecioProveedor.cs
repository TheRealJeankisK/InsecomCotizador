using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsecomCotizador.Models;

// precio y tiempo de entrega de un material segun el proveedor
public class PrecioProveedor
{
    public int Id { get; set; }

    [Display(Name = "Proveedor")]
    public int ProveedorId { get; set; }
    public Proveedor? Proveedor { get; set; }

    public int MaterialId { get; set; }
    public Material? Material { get; set; }

    [Range(0.01, 1000000, ErrorMessage = "El precio debe ser mayor a 0.")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Precio { get; set; }

    [Range(0, 365, ErrorMessage = "Los días deben estar entre 0 y 365.")]
    [Display(Name = "Días de entrega")]
    public int DiasEntrega { get; set; }
}
