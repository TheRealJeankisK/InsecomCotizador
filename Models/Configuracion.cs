using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsecomCotizador.Models;

// parametros del calculo (solo hay una fila)
public class Configuracion
{
    public int Id { get; set; }

    // en porcentaje (30 = 30 %)
    [Range(0, 500, ErrorMessage = "El margen va de 0 a 500 %.")]
    [Column(TypeName = "decimal(5,2)")]
    [Display(Name = "Margen de ganancia (%)")]
    public decimal Margen { get; set; } = 30;

    [Range(0, 100, ErrorMessage = "El IVA va de 0 a 100 %.")]
    [Column(TypeName = "decimal(5,2)")]
    [Display(Name = "IVA (%)")]
    public decimal IVA { get; set; } = 15;
}
