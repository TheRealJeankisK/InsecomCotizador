using InsecomCotizador.Models;
using Microsoft.EntityFrameworkCore;

namespace InsecomCotizador.Data;

// contexto de la bd (cada DbSet es una tabla)
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Vendedor> Vendedores => Set<Vendedor>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Material> Materiales => Set<Material>();
    public DbSet<ProductoMaterial> ProductoMateriales => Set<ProductoMaterial>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<PrecioProveedor> PreciosProveedor => Set<PrecioProveedor>();
    public DbSet<ReglaComision> ReglasComision => Set<ReglaComision>();
    public DbSet<Configuracion> Configuraciones => Set<Configuracion>();
    public DbSet<Cotizacion> Cotizaciones => Set<Cotizacion>();
    public DbSet<DetalleCotizacion> DetallesCotizacion => Set<DetalleCotizacion>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // campos unicos
        mb.Entity<Usuario>().HasIndex(u => u.NombreUsuario).IsUnique();
        mb.Entity<Cliente>().HasIndex(c => c.RUC).IsUnique();

        // un material solo una vez en la receta de un producto
        mb.Entity<ProductoMaterial>().HasIndex(pm => new { pm.ProductoId, pm.MaterialId }).IsUnique();
        // un proveedor solo un precio por material
        mb.Entity<PrecioProveedor>().HasIndex(p => new { p.ProveedorId, p.MaterialId }).IsUnique();

        // aqui cuido la integridad:
        // cascade = si borro el padre se borran sus hijos (la receta es del producto, los precios son del material)
        // restrict = no deja borrar si se esta usando (un material que esta en una receta, un cliente con cotizaciones)
        mb.Entity<ProductoMaterial>()
            .HasOne(pm => pm.Producto).WithMany(p => p.Receta)
            .HasForeignKey(pm => pm.ProductoId).OnDelete(DeleteBehavior.Cascade);
        mb.Entity<ProductoMaterial>()
            .HasOne(pm => pm.Material).WithMany()
            .HasForeignKey(pm => pm.MaterialId).OnDelete(DeleteBehavior.Restrict);

        // no deja borrar una categoria que tiene materiales
        mb.Entity<Material>()
            .HasOne(m => m.Categoria).WithMany(c => c.Materiales)
            .HasForeignKey(m => m.CategoriaId).OnDelete(DeleteBehavior.Restrict);

        mb.Entity<PrecioProveedor>()
            .HasOne(p => p.Material).WithMany(m => m.Precios)
            .HasForeignKey(p => p.MaterialId).OnDelete(DeleteBehavior.Cascade);
        mb.Entity<PrecioProveedor>()
            .HasOne(p => p.Proveedor).WithMany(pr => pr.Precios)
            .HasForeignKey(p => p.ProveedorId).OnDelete(DeleteBehavior.Restrict);

        mb.Entity<Cotizacion>()
            .HasOne(c => c.Cliente).WithMany(cl => cl.Cotizaciones)
            .HasForeignKey(c => c.ClienteId).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<Cotizacion>()
            .HasOne(c => c.Vendedor).WithMany(v => v.Cotizaciones)
            .HasForeignKey(c => c.VendedorId).OnDelete(DeleteBehavior.Restrict);

        mb.Entity<DetalleCotizacion>()
            .HasOne(d => d.Cotizacion).WithMany(c => c.Detalles)
            .HasForeignKey(d => d.CotizacionId).OnDelete(DeleteBehavior.Cascade);
        mb.Entity<DetalleCotizacion>()
            .HasOne(d => d.Producto).WithMany()
            .HasForeignKey(d => d.ProductoId).OnDelete(DeleteBehavior.Restrict);
    }
}
