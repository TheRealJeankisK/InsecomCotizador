using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsecomCotizador.Models;

// inventario de bodega
public class Material
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150)]
    public string Nombre { get; set; } = "";

    // fk a la categoria (se elige con un dropdown, no se escribe el id)
    [Range(1, int.MaxValue, ErrorMessage = "Elige una categoría.")]
    [Display(Name = "Categoría")]
    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    // u, m, kg...
    [Required(ErrorMessage = "La unidad es obligatoria.")]
    [StringLength(20)]
    public string Unidad { get; set; } = "";

    [Range(0, 1000000, ErrorMessage = "El stock no puede ser negativo.")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Stock { get; set; }

    [Range(0, 1000000, ErrorMessage = "El costo no puede ser negativo.")]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Costo unitario")]
    public decimal CostoUnitario { get; set; }

    // precios de cada proveedor para este material
    public List<PrecioProveedor> Precios { get; set; } = new();
}
