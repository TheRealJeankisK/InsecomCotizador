using System.ComponentModel.DataAnnotations;

namespace InsecomCotizador.Models;

// catalogo de lo que fabrica insecom (tableros, gabinetes, etc)
public class Producto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150)]
    public string Nombre { get; set; } = "";

    // NEMA 3R, NEMA 12, MCC, PLC...
    [Required(ErrorMessage = "El tipo es obligatorio.")]
    [StringLength(50)]
    public string Tipo { get; set; } = "";

    [StringLength(500)]
    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }

    // receta: materiales que lleva 1 unidad
    public List<ProductoMaterial> Receta { get; set; } = new();
}
