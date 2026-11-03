using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsecomCotizador.Models;

// rangos de venta y su porcentaje de comision
public class ReglaComision
{
    public int Id { get; set; }

    [Range(0, 100000000, ErrorMessage = "El monto no puede ser negativo.")]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Monto mínimo")]
    public decimal MontoMinimo { get; set; }

    // vacio = sin tope (la ultima regla)
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Monto máximo")]
    public decimal? MontoMaximo { get; set; }

    [Range(0, 100, ErrorMessage = "El porcentaje va de 0 a 100.")]
    [Column(TypeName = "decimal(5,2)")]
    public decimal Porcentaje { get; set; }
}
