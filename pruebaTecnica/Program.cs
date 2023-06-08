using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using pruebaTecnica.Models;

var context = new PruebaContext();

// Obtener la fecha límite para las ventas de los últimos 30 días
DateTime fechaLimite = DateTime.Now.AddDays(-30);

// Total de ventas en los últimos 30 días
var datos = context.Venta
    .Where(v => v.Fecha >= fechaLimite)
    .Select(v => v.Total)
    .Sum();

// Cantidad total de ventas en los últimos 30 días
var cantidadVentas = context.Venta
    .Where(v => v.Fecha >= fechaLimite)
    .Count();

// Venta con el monto más alto
var ventaMaxima = context.Venta
    .OrderByDescending(v => v.Total)
    .FirstOrDefault();
DateTime fechaVentaMaxima = ventaMaxima.Fecha;
decimal montoVentaMaxima = ventaMaxima.Total;

// Producto con el mayor monto total de ventas
var productoMasVendido = context.Productos
    .Join(context.VentaDetalles, p => p.IdProducto, vd => vd.IdProducto, (p, vd) => new { Producto = p, VentaDetalle = vd })
    .AsEnumerable()
    .GroupBy(x => x.Producto)
    .Select(g => new { Producto = g.Key, MontoTotalVentas = g.Sum(x => x.VentaDetalle.TotalLinea) })
    .OrderByDescending(x => x.MontoTotalVentas)
    .FirstOrDefault();

string nombreProductoMasVendido = productoMasVendido?.Producto.Nombre;
decimal montoTotalVentasProductoMasVendido = productoMasVendido?.MontoTotalVentas ?? 0;

// Local con el mayor monto de ventas
var localMayorMontoVentas = context.Locals
    .Join(context.Venta, l => l.IdLocal, v => v.IdLocal, (l, v) => new { Local = l, Venta = v })
    .AsEnumerable()
    .GroupBy(x => x.Local)
    .Select(g => new { Local = g.Key, MontoTotalVentas = g.Sum(x => x.Venta.Total) })
    .OrderByDescending(x => x.MontoTotalVentas)
    .FirstOrDefault();

string nombreLocalMayorMontoVentas = localMayorMontoVentas?.Local.Nombre;
decimal montoTotalVentasLocalMayor = localMayorMontoVentas?.MontoTotalVentas ?? 0;

// Marca con el mayor margen de ganancias
var marcaConMayorMargen = context.Productos
    .Join(context.VentaDetalles, p => p.IdProducto, vd => vd.IdProducto, (p, vd) => new { Producto = p, VentaDetalle = vd })
    .Join(context.Marcas, x => x.Producto.IdMarca, m => m.IdMarca, (x, m) => new { Producto = x.Producto, VentaDetalle = x.VentaDetalle, Marca = m })
    .GroupBy(x => x.Marca.Nombre)
    .Select(g => new { Marca = g.Key, MargenGanancias = g.Sum(x => x.VentaDetalle.TotalLinea - (x.Producto.CostoUnitario * x.VentaDetalle.Cantidad)) })
    .OrderByDescending(x => x.MargenGanancias)
    .FirstOrDefault()?.Marca;

// Producto que más se vende en cada local
var productosMasVendidosPorLocal = context.Locals
    .GroupJoin(context.Venta, l => l.IdLocal, v => v.IdLocal, (l, ventas) => new { Local = l, Ventas = ventas })
    .SelectMany(x => x.Ventas.DefaultIfEmpty(), (x, v) => new { Local = x.Local, Venta = v })
    .Join(context.VentaDetalles, x => x.Venta.IdVenta, vd => vd.IdVenta, (x, vd) => new { Local = x.Local, VentaDetalle = vd })
    .Join(context.Productos, x => x.VentaDetalle.IdProducto, p => p.IdProducto, (x, p) => new { Local = x.Local, Producto = p })
    .AsEnumerable()
    .GroupBy(x => new { x.Local.IdLocal, x.Local.Nombre })
    .Select(g => new { Local = g.Key, ProductoMasVendido = g.GroupBy(x => x.Producto).OrderByDescending(grp => grp.Count()).FirstOrDefault() })
    .Select(x => new { x.Local.Nombre, ProductoMasVendido = x.ProductoMasVendido.Key, CantidadVendida = x.ProductoMasVendido.Count() })
    .ToList();

// Mostrar los resultados en la consola
Console.WriteLine("--------Resultados---------");
Console.WriteLine("");
Console.WriteLine("Total de ventas en los últimos 30 días: " + datos);
Console.WriteLine("Cantidad total de ventas en los últimos 30 días: " + cantidadVentas);
Console.WriteLine("");
Console.WriteLine("Venta con monto más alto:");
Console.WriteLine("  Fecha: " + fechaVentaMaxima);
Console.WriteLine("  Monto: " + montoVentaMaxima);
Console.WriteLine("");
Console.WriteLine("Producto con mayor monto total de ventas:");
Console.WriteLine("  Nombre: " + nombreProductoMasVendido);
Console.WriteLine("  Monto total de ventas: " + montoTotalVentasProductoMasVendido);
Console.WriteLine("");
Console.WriteLine("Local con mayor monto de ventas:");
Console.WriteLine("  Nombre: " + nombreLocalMayorMontoVentas);
Console.WriteLine("  Monto total de ventas: " + montoTotalVentasLocalMayor);
Console.WriteLine("");
Console.WriteLine("Marca con mayor margen de ganancias: " + marcaConMayorMargen);
Console.WriteLine("");
Console.WriteLine("Productos que más se venden en cada local:");
foreach (var productoPorLocal in productosMasVendidosPorLocal)
{
    Console.WriteLine("  Local: " + productoPorLocal.Nombre);
    Console.WriteLine("    Producto más vendido: " + productoPorLocal.ProductoMasVendido.Nombre);
    Console.WriteLine("    Cantidad vendida: " + productoPorLocal.CantidadVendida);
}
