-- Consulta para obtener el monto total y la cantidad total de ventas de los últimos 30 días
SELECT SUM(Total) AS Monto_Total, COUNT(*) AS Cantidad_Total
FROM Venta
WHERE Fecha >= DATEADD(DAY, -30, GETDATE());

-- Consulta para obtener la fecha y el monto de la venta más alta
SELECT TOP 1 Fecha, Total AS Monto_Venta_Maximo
FROM Venta
ORDER BY Total DESC;

-- Consulta para obtener el nombre del producto y el monto total de ventas, ordenados por el monto total de ventas de forma descendente
SELECT TOP 1 P.ID_Producto, P.Nombre, SUM(VD.TotalLinea) AS MontoTotalVentas
FROM  Producto P  
JOIN VentaDetalle VD ON P.ID_Producto = VD.ID_Producto
JOIN Venta V ON VD.ID_Venta = V.ID_Venta
GROUP BY 
  P.ID_Producto, 
  P.Nombre
ORDER BY 
  MontoTotalVentas DESC;

-- Consulta para obtener el nombre del local y el monto total de ventas, ordenados por el monto total de ventas de forma descendente
SELECT TOP 1 l.Nombre AS Nombre_Local, SUM(v.Total) AS Monto_Total_Ventas
FROM Local l
JOIN Venta v ON l.ID_Local = v.ID_Local
GROUP BY l.Nombre
ORDER BY Monto_Total_Ventas DESC;

-- Consulta la marca con mayor margen de ganacias 
SELECT TOP 1 m.Nombre AS Marca, SUM((vd.Precio_Unitario - p.Costo_Unitario) * vd.Cantidad) AS MargenGanancias
FROM Marca m
JOIN Producto p ON m.ID_Marca = p.ID_Marca
JOIN VentaDetalle vd ON p.ID_Producto = vd.ID_Producto
GROUP BY m.Nombre
ORDER BY MargenGanancias DESC;


-- Consulta para obtener el nombre del local, el nombre del producto y la cantidad vendida del producto más vendido en cada local
SELECT Nombre_Local, Nombre_Producto, Cantidad_Vendida
FROM (
    -- Subconsulta para obtener el nombre del local, el nombre del producto, la cantidad vendida y el número de fila
    SELECT l.Nombre AS Nombre_Local, p.Nombre AS Nombre_Producto, COUNT(*) AS Cantidad_Vendida,
        ROW_NUMBER() OVER(PARTITION BY l.ID_Local ORDER BY COUNT(*) DESC) AS RowNum
    FROM Local l
    JOIN Venta v ON l.ID_Local = v.ID_Local
    JOIN VentaDetalle vd ON v.ID_Venta = vd.ID_Venta
    JOIN Producto p ON vd.ID_Producto = p.ID_Producto
    GROUP BY l.ID_Local, l.Nombre, p.Nombre
) AS subquery
WHERE RowNum = 1;
