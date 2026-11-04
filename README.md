# INSECOM Cotizador — ASP.NET Core MVC

Proyecto de la materia **Ingeniería Web (ISWZ3101)** de la UDLA: sistema de gestión comercial para **INSECOM S.A.** (Quito), empresa que fabrica tableros eléctricos y envolventes metálicos NEMA.

El **core** (Progreso 2) calcula el precio sugerido de una cotización a partir de la receta de materiales de cada producto, el inventario y los precios de los proveedores, y luego la comisión del vendedor. Esta primera parte es la **administración** que alimenta ese cálculo.

**Demo en línea:** https://insecomcotizador.onrender.com (plan gratis de Render: la primera carga tarda ~1 min si estaba dormida).

## Qué incluye (Progreso 1: admin)

| Módulo | Qué se puede hacer |
|---|---|
| Login | Inicio y cierre de sesión con cookie; contraseña cifrada (PBKDF2). Todas las páginas piden sesión. |
| Clientes | CRUD con buscador. **El RUC se valida en el back-end** antes de grabar (ver abajo). |
| Vendedores | CRUD. |
| Productos y recetas | CRUD de productos y su receta: qué materiales lleva una unidad y en qué cantidad. El material se elige con **dropdowns en cascada** (categoría → materiales de esa categoría). |
| Categorías de materiales | CRUD (Breakers, Láminas, Cables…). Ordenan el inventario y filtran la receta. |
| Materiales (inventario) | CRUD con categoría (dropdown), stock y costo en bodega; precios y días de entrega de cada proveedor (marca el más barato). |
| Proveedores | CRUD. |
| Reglas de comisión | CRUD de rangos; valida que el máximo sea mayor que el mínimo y que los rangos no se crucen. |
| Margen e IVA | Parámetros de la fórmula del precio. |

**Integridad de datos:** no se puede borrar un material que está en una receta, un proveedor con precios, ni un cliente o vendedor con cotizaciones. Al borrar un producto se borra su receta, y al borrar un material, sus precios.

## Validación en el back-end (dato sensible: RUC)

El RUC identifica al cliente en las cotizaciones, así que se valida **en el controlador** (`ClientesController.ValidarRuc`) antes de guardar, aunque alguien se salte el HTML o el JavaScript:

1. 13 dígitos, solo números.
2. Los 2 primeros son una provincia válida (01–24, o 30).
3. El tercer dígito es un tipo válido (0–5 persona natural, 6 pública, 9 privada).
4. No termina en `000` (número de establecimiento).
5. Persona natural: se comprueba el dígito verificador de la cédula (módulo 10).
6. No se repite con otro cliente.

También se valida en el back que la categoría de un material y el material de una receta existan de verdad en la base de datos.

## Dropdowns en cascada (claves foráneas)

Ninguna clave foránea se escribe a mano: todas se eligen con un dropdown. En la **receta** de un producto se elige primero la **categoría** y, con `fetch`, se pide al controlador (`ProductosController.MaterialesPorCategoria`, devuelve JSON) la lista de materiales de esa categoría para llenar el segundo dropdown.

## Tablas

Usuario, Categoria, Cliente, Vendedor, Producto, Material, ProductoMaterial, Proveedor, PrecioProveedor, ReglaComision, Configuracion, Cotizacion y DetalleCotizacion.

## Ejecutar en local

Requisito: [.NET SDK 10](https://dotnet.microsoft.com/download).

```bash
dotnet run
```

La base de datos SQLite (`insecom.db`) se crea sola. El usuario inicial está en `appsettings.json` (sección `AdminInicial`). Todos los demás datos se registran desde la aplicación.

## Despliegue (Render)

El proyecto tiene `Dockerfile`. En Render: **New → Web Service → Docker**, conectado a este repositorio. La app lee el puerto de la variable `PORT`. Para no usar la contraseña de ejemplo, definir en Render las variables `AdminInicial__NombreUsuario` y `AdminInicial__Contrasena`.

## Tecnologías

ASP.NET Core MVC (.NET 10) · Entity Framework Core · SQLite · Bootstrap 5 · Bootstrap Icons

## Autor

**Jean Carlos G.** — Ingeniería de Software, UDLA.
