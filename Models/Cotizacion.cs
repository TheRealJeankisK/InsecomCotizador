using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsecomCotizador.Models;

public enum EstadoCotizacion { Borrador, Enviada, Aprobada, Rechazada }

// cabecera de la cotizacion (el core la usa en el P2)
public class Cotizacion
{
    public int Id { get; set; }

    [Display(Name = "Cliente")]
    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    [Display(Name = "Vendedor")]
    public int VendedorId { get; set; }
    public Vendedor? Vendedor { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Now;

    public EstadoCotizacion Estado { get; set; } = EstadoCotizacion.Borrador;

    // precio sugerido sin iva (lo llena el core)
    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }

    public List<DetalleCotizacion> Detalles { get; set; } = new();
}
