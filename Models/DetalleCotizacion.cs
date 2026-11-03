using System.ComponentModel.DataAnnotations;

namespace InsecomCotizador.Models;

// productos y cantidades de cada cotizacion
public class DetalleCotizacion
{
    public int Id { get; set; }

    public int CotizacionId { get; set; }
    public Cotizacion? Cotizacion { get; set; }

    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }

    [Range(1, 10000, ErrorMessage = "La cantidad debe ser mayor a 0.")]
    public int Cantidad { get; set; }
}
