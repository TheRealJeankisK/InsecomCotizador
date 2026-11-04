namespace InsecomCotizador.Models;

// datos que muestra el panel de inicio (no es tabla, solo lo armo en el controlador)
public class PanelViewModel
{
    // contadores de cada tabla
    public int Clientes { get; set; }
    public int Vendedores { get; set; }
    public int Productos { get; set; }
    public int Materiales { get; set; }
    public int Proveedores { get; set; }
    public int Reglas { get; set; }
    public int Categorias { get; set; }

    // inventario
    public decimal ValorInventario { get; set; }
    public List<Material> StockBajo { get; set; } = new();
    public List<ResumenCategoria> PorCategoria { get; set; } = new();

    // costo de 1 unidad de cada producto segun su receta
    public List<ResumenProducto> CostosProductos { get; set; } = new();

    public decimal Margen { get; set; }
    public decimal IVA { get; set; }

    // pasos que hacen falta para poder cotizar (texto, si ya esta listo, controlador)
    public List<(string Texto, bool Listo, string Controlador)> Pasos { get; set; } = new();
}

public class ResumenCategoria
{
    public string Nombre { get; set; } = "";
    public int Materiales { get; set; }
    public decimal Valor { get; set; }
}

public class ResumenProducto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Tipo { get; set; } = "";
    public int NumMateriales { get; set; }
    public decimal Costo { get; set; }
}
