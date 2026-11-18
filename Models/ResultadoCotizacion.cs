namespace InsecomCotizador.Models;

// resultado del core (no es tabla, lo armo en el controlador para mostrarlo)
public class ResultadoCotizacion
{
    public List<LineaProducto> Productos { get; set; } = new();

    public decimal Costo { get; set; }
    public decimal MargenPct { get; set; }
    public decimal ValorMargen { get; set; }
    public decimal Subtotal { get; set; }
    public decimal IvaPct { get; set; }
    public decimal ValorIva { get; set; }
    public decimal Total { get; set; }
    public decimal ComisionPct { get; set; }
    public decimal Comision { get; set; }
    public int PlazoDias { get; set; }

    // cosas que impiden aprobar (material sin stock ni proveedor, producto sin receta...)
    public List<string> Problemas { get; set; } = new();
}

// un producto de la cotizacion con el desglose de sus materiales
public class LineaProducto
{
    public int DetalleId { get; set; }
    public int ProductoId { get; set; }
    public string Nombre { get; set; } = "";
    public int Cantidad { get; set; }
    public decimal Costo { get; set; }
    public decimal PrecioUnitario { get; set; }
    public List<LineaMaterial> Materiales { get; set; } = new();
}

// de donde sale cada material: bodega, proveedor o los dos
public class LineaMaterial
{
    public int MaterialId { get; set; }
    public string Nombre { get; set; } = "";
    public string Unidad { get; set; } = "";
    public decimal Necesito { get; set; }
    public decimal DeBodega { get; set; }
    public decimal AComprar { get; set; }
    public decimal CostoBodega { get; set; }
    public string? Proveedor { get; set; }
    public decimal PrecioProveedor { get; set; }
    public int DiasEntrega { get; set; }
    public decimal Subtotal { get; set; }
}
