using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsecomCotizador.Models;

// productos y cantidades de cada cotizacion
public class DetalleCotizacion
{
    public int Id { get; set; }

    public int CotizacionId { get; set; }
    public Cotizacion? Cotizacion { get; set; }

    [Display(Name = "Producto")]
    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }

    [Range(1, 10000, ErrorMessage = "La cantidad debe ser mayor a 0.")]
    public int Cantidad { get; set; }

    // precio sugerido de 1 unidad (sin iva), se guarda al aprobar
    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecioUnitario { get; set; }
}
