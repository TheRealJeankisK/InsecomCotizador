using System.ComponentModel.DataAnnotations;

namespace InsecomCotizador.Models;

// tipo de material (laminas, breakers, cables...) para ordenar el inventario
public class Categoria
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100)]
    public string Nombre { get; set; } = "";

    public List<Material> Materiales { get; set; } = new();
}
