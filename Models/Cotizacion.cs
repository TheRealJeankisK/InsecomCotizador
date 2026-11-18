using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsecomCotizador.Models;

public enum EstadoCotizacion { Borrador, Enviada, Aprobada, Rechazada }

// cabecera de la cotizacion
// mientras es borrador el core la calcula en vivo; al aprobar guardo aqui el resultado
public class Cotizacion
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Elige un cliente.")]
    [Display(Name = "Cliente")]
    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Elige un vendedor.")]
    [Display(Name = "Vendedor")]
    public int VendedorId { get; set; }
    public Vendedor? Vendedor { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Now;

    public EstadoCotizacion Estado { get; set; } = EstadoCotizacion.Borrador;

    // costo de materiales (bodega + proveedores)
    [Column(TypeName = "decimal(18,2)")]
    public decimal Costo { get; set; }

    // costo + margen (sin iva), sobre esto se calcula la comision
    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }

    // subtotal + iva = precio sugerido final
    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }

    // comision del vendedor segun las reglas
    [Column(TypeName = "decimal(18,2)")]
    public decimal Comision { get; set; }

    // dias que tarda en llegar lo que hay que comprar (0 = todo de bodega)
    [Display(Name = "Plazo (días)")]
    public int PlazoDias { get; set; }

    public List<DetalleCotizacion> Detalles { get; set; } = new();
}
