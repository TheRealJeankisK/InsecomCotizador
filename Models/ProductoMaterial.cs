using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsecomCotizador.Models;

// receta: cuanto de cada material lleva 1 unidad de producto
public class ProductoMaterial
{
    public int Id { get; set; }

    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }

    [Display(Name = "Material")]
    public int MaterialId { get; set; }
    public Material? Material { get; set; }

    [Range(0.01, 100000, ErrorMessage = "La cantidad debe ser mayor a 0.")]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Cantidad por unidad")]
    public decimal CantidadPorUnidad { get; set; }
}
