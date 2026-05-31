using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace SistemaGestionVentas
{
    public class DatabaseManager
    {
        private string connectionString;

        public DatabaseManager(string connStr)
        {
            connectionString = connStr;
        }

        // lista las sucursales
        public List<Sucursal> ObtenerSucursales()
        {
            List<Sucursal> sucursales = new List<Sucursal>();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT IdSucursal, Nombre FROM Sucursal";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sucursales.Add(new Sucursal(
                            Convert.ToInt32(reader["IdSucursal"]),
                            reader["Nombre"].ToString()
                        ));
                    }
                }
            }
            return sucursales;
        }

        // obtiene nmbre de la sucursal
        public string ObtenerNombreSucursal(int id)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT Nombre FROM Sucursal WHERE IdSucursal = @id";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    object result = cmd.ExecuteScalar();
                    return result != null ? result.ToString() : null;
                }
            }
        }

        // guarda el producto en la bdd
        public void GuardarProducto(Producto prod)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                
                // inserta producto en la tabla general y obtiene el id generado
                string queryProducto = @"INSERT INTO Producto 
                    (Codigo, Nombre, Precio, Stock, IdSucursal, TipoProducto) 
                    VALUES (@codigo, @nombre, @precio, @stock, @id_sucursal, @tipo);
                    SELECT LAST_INSERT_ID();";

                int idProductoGenerado = 0;

                using (MySqlCommand cmd = new MySqlCommand(queryProducto, conn))
                {
                    cmd.Parameters.AddWithValue("@codigo", prod.Codigo);
                    cmd.Parameters.AddWithValue("@nombre", prod.Nombre);
                    cmd.Parameters.AddWithValue("@precio", prod.Precio);
                    cmd.Parameters.AddWithValue("@stock", prod.Stock);
                    cmd.Parameters.AddWithValue("@id_sucursal", prod.IdSucursal);

                    if (prod is Televisor) cmd.Parameters.AddWithValue("@tipo", "Televisor");
                    else if (prod is Heladera) cmd.Parameters.AddWithValue("@tipo", "Heladera");
                    else if (prod is Lavarropas) cmd.Parameters.AddWithValue("@tipo", "Lavarropas");

                    idProductoGenerado = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // inserta en la tabla específica según el tipo de producto
                if (prod is Televisor tv)
                {
                    string queryTv = "INSERT INTO Televisor (IdProducto, Pulgadas, TipoPantalla) VALUES (@id, @pulgadas, @pantalla)";
                    using (MySqlCommand cmdTv = new MySqlCommand(queryTv, conn))
                    {
                        cmdTv.Parameters.AddWithValue("@id", idProductoGenerado);
                        cmdTv.Parameters.AddWithValue("@pulgadas", tv.Pulgadas);
                        cmdTv.Parameters.AddWithValue("@pantalla", tv.TipoPantalla);
                        cmdTv.ExecuteNonQuery();
                    }
                }
                else if (prod is Heladera hel)
                {
                    string queryHel = "INSERT INTO Heladera (IdProducto, CapacidadLitros, Tipo) VALUES (@id, @capacidad, @tipo)";
                    using (MySqlCommand cmdHel = new MySqlCommand(queryHel, conn))
                    {
                        cmdHel.Parameters.AddWithValue("@id", idProductoGenerado);
                        cmdHel.Parameters.AddWithValue("@capacidad", hel.Capacidad);
                        cmdHel.Parameters.AddWithValue("@tipo", hel.Tipo);
                        cmdHel.ExecuteNonQuery();
                    }
                }
                else if (prod is Lavarropas lav)
                {
                    string queryLav = "INSERT INTO Lavarropas (IdProducto, CargaKg, Tipo) VALUES (@id, @carga, @tipo)";
                    using (MySqlCommand cmdLav = new MySqlCommand(queryLav, conn))
                    {
                        cmdLav.Parameters.AddWithValue("@id", idProductoGenerado);
                        cmdLav.Parameters.AddWithValue("@carga", lav.Carga);
                        cmdLav.Parameters.AddWithValue("@tipo", lav.Tipo);
                        cmdLav.ExecuteNonQuery();
                    }
                }
            }
        }

        // muestra los productos de la sucursal
        public List<Producto> ObtenerProductosPorSucursal(int idSucursal)
        {
            List<Producto> lista = new List<Producto>();

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                
                string query = @"
                    SELECT p.IdProducto, p.Codigo, p.Nombre, p.Precio, p.Stock, p.TipoProducto, p.IdSucursal,
                           t.Pulgadas, t.TipoPantalla,
                           h.CapacidadLitros, h.Tipo AS hel_tipo,
                           l.CargaKg, l.Tipo AS lav_tipo
                    FROM Producto p
                    LEFT JOIN Televisor t ON p.IdProducto = t.IdProducto
                    LEFT JOIN Heladera h ON p.IdProducto = h.IdProducto
                    LEFT JOIN Lavarropas l ON p.IdProducto = l.IdProducto
                    WHERE p.IdSucursal = @id_sucursal";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id_sucursal", idSucursal);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string tipo = reader["TipoProducto"].ToString();
                            string codigo = reader["Codigo"].ToString();
                            string nombre = reader["Nombre"].ToString();
                            decimal precio = Convert.ToDecimal(reader["Precio"]);
                            int stock = Convert.ToInt32(reader["Stock"]);
                            int id = Convert.ToInt32(reader["IdProducto"]);

                            Producto prod = null;

                            if (tipo == "Televisor")
                            {
                                int pulgadas = Convert.ToInt32(reader["Pulgadas"]);
                                string pant = reader["TipoPantalla"].ToString();
                                prod = new Televisor(codigo, nombre, precio, stock, idSucursal, pulgadas, pant);
                            }
                            else if (tipo == "Heladera")
                            {
                                int cap = Convert.ToInt32(reader["CapacidadLitros"]);
                                string tipoH = reader["hel_tipo"].ToString();
                                prod = new Heladera(codigo, nombre, precio, stock, idSucursal, cap, tipoH);
                            }
                            else if (tipo == "Lavarropas")
                            {
                                int carga = Convert.ToInt32(reader["CargaKg"]);
                                string tipoL = reader["lav_tipo"].ToString();
                                prod = new Lavarropas(codigo, nombre, precio, stock, idSucursal, carga, tipoL);
                            }

                            if (prod != null)
                            {
                                prod.Id = id;
                                lista.Add(prod);
                            }
                        }
                    }
                }
            }
            return lista;
        }

        // modifica el producto en la bdd
        public bool ModificarProducto(int id, int idSucursal, decimal nuevoPrecio, int nuevoStock)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE Producto SET Precio = @precio, Stock = @stock WHERE IdProducto = @id AND IdSucursal = @id_sucursal";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@precio", nuevoPrecio);
                    cmd.Parameters.AddWithValue("@stock", nuevoStock);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@id_sucursal", idSucursal);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // elimina el producto de la bdd
        public bool EliminarProducto(int id, int idSucursal)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "DELETE FROM Producto WHERE IdProducto = @id AND IdSucursal = @id_sucursal";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@id_sucursal", idSucursal);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // registra la venta con transaccion
        public void RegistrarVentaConTransaccion(int idSucursal, Producto prod, int cantidad, decimal totalVenta)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                MySqlTransaction transaccion = conn.BeginTransaction();

                try
                {
                    // inserta la venta en la tabla 'Venta' y obtiene el id generado
                    string queryVenta = "INSERT INTO Venta (IdSucursal) VALUES (@id_sucursal); SELECT LAST_INSERT_ID();";
                    int idVentaGenerado = 0;

                    using (MySqlCommand cmdVenta = new MySqlCommand(queryVenta, conn, transaccion))
                    {
                        cmdVenta.Parameters.AddWithValue("@id_sucursal", idSucursal);
                        idVentaGenerado = Convert.ToInt32(cmdVenta.ExecuteScalar());
                    }

                    // inserta el detalle de la venta en la tabla 'DetalleVenta'
                    string queryDetail = "INSERT INTO DetalleVenta (IdVenta, IdProducto, Cantidad, PrecioUnitario) VALUES (@id_venta, @id_producto, @cantidad, @precio_unitario)";
                    using (MySqlCommand cmdDetalle = new MySqlCommand(queryDetail, conn, transaccion))
                    {
                        cmdDetalle.Parameters.AddWithValue("@id_venta", idVentaGenerado);
                        cmdDetalle.Parameters.AddWithValue("@id_producto", prod.Id);
                        cmdDetalle.Parameters.AddWithValue("@cantidad", cantidad);
                        cmdDetalle.Parameters.AddWithValue("@precio_unitario", prod.CalcularPrecioFinal());

                        cmdDetalle.ExecuteNonQuery();
                    }

                    // resta el stock del producto vendido
                    string queryActualizarStock = "UPDATE Producto SET Stock = Stock - @cantidad WHERE IdProducto = @id";
                    using (MySqlCommand cmdStock = new MySqlCommand(queryActualizarStock, conn, transaccion))
                    {
                        cmdStock.Parameters.AddWithValue("@cantidad", cantidad);
                        cmdStock.Parameters.AddWithValue("@id", prod.Id);

                        cmdStock.ExecuteNonQuery();
                    }

                    transaccion.Commit();
                    Console.WriteLine("\n=================================");
                    Console.WriteLine(" ¡VENTA REALIZADA CON ÉXITO!");
                    Console.WriteLine($" Total final facturado: ${totalVenta}");
                    Console.WriteLine($" Nuevo stock del producto: {prod.Stock - cantidad}");
                    Console.WriteLine("=================================");
                }
                catch (Exception ex)
                {
                    transaccion.Rollback();
                    Console.WriteLine("\n[Error de Consistencia] Venta cancelada.");
                    Console.WriteLine("Detalle técnico: " + ex.Message);
                }
            }
        }
    }
}