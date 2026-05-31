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

        // muestra las sucursales
        public List<Sucursal> ObtenerSucursales()
        {
            List<Sucursal> sucursales = new List<Sucursal>();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT idSucursal, Nombre FROM sucursal";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sucursales.Add(new Sucursal(
                            Convert.ToInt32(reader["idSucursal"]),
                            reader["Nombre"].ToString()
                        ));
                    }
                }
            }
            return sucursales;
        }

        // obtiene el nombre de la sucursal
        public string ObtenerNombreSucursal(int id)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT Nombre FROM sucursal WHERE idSucursal = @id";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    object result = cmd.ExecuteScalar();
                    return result != null ? result.ToString() : null;
                }
            }
        }

        // guarda un producto
        public void GuardarProducto(Producto prod)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                
                // inserta en la tabla base 'producto' y obtiene el id generado
                string queryProducto = 
                    @"INSERT INTO producto 
                    (`Código`, Nombre, Precio, Stock, idSucursal, `Tipo producto`) 
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

                // se inserta en la tabla que corresponde según el tipo de producto
                if (prod is Televisor tv)
                {
                    string queryTv = "INSERT INTO televisor (idProducto, Pulgadas, `Tipo pantalla`) VALUES (@id, @pulgadas, @pantalla)";
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
                    string queryHel = "INSERT INTO heladera (idProducto, CapacidadLitros, Tipo) VALUES (@id, @capacidad, @tipo)";
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
                    string queryLav = "INSERT INTO lavarropas (idProducto, CargaKg, Tipo) VALUES (@id, @carga, @tipo)";
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

        // lista los productos de una sucursal
        public List<Producto> ObtenerProductosPorSucursal(int idSucursal)
        {
            List<Producto> lista = new List<Producto>();

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                
                string query = @"
                    SELECT p.idProducto, p.`Código`, p.Nombre, p.Precio, p.Stock, p.`Tipo producto`, p.idSucursal,
                           t.Pulgadas, t.`Tipo pantalla`,
                           h.CapacidadLitros, h.Tipo AS hel_tipo,
                           l.CargaKg, l.Tipo AS lav_tipo
                    FROM producto p
                    LEFT JOIN televisor t ON p.idProducto = t.idProducto
                    LEFT JOIN heladera h ON p.idProducto = h.idProducto
                    LEFT JOIN lavarropas l ON p.idProducto = l.idProducto
                    WHERE p.idSucursal = @id_sucursal";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id_sucursal", idSucursal);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string tipo = reader["Tipo producto"].ToString();
                            string codigo = reader["Código"].ToString();
                            string nombre = reader["Nombre"].ToString();
                            decimal precio = Convert.ToDecimal(reader["Precio"]);
                            int stock = Convert.ToInt32(reader["Stock"]);
                            int id = Convert.ToInt32(reader["idProducto"]);

                            Producto prod = null;

                            if (tipo == "Televisor")
                            {
                                int pulgadas = Convert.ToInt32(reader["Pulgadas"]);
                                string pant = reader["Tipo pantalla"].ToString();
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

        // modifica el precio y stock de un producto
        public bool ModificarProducto(int id, int idSucursal, decimal nuevoPrecio, int nuevoStock)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE producto SET Precio = @precio, Stock = @stock WHERE idProducto = @id AND idSucursal = @id_sucursal";
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

        // elimina un producto
        public bool EliminarProducto(int id, int idSucursal)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "DELETE FROM producto WHERE idProducto = @id AND idSucursal = @id_sucursal";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@id_sucursal", idSucursal);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // emula la venta
        public void RegistrarVentaConTransaccion(int idSucursal, Producto prod, int cantidad, decimal totalVenta)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                MySqlTransaction transaccion = conn.BeginTransaction();

                try
                {
                    // insertamos en la tabla 'venta' y obtenemos el idVenta que se generó
                    string queryVenta = "INSERT INTO venta (idSucursal, total) VALUES (@id_sucursal, @total); SELECT LAST_INSERT_ID();";
                    int idVentaGenerado = 0;

                    using (MySqlCommand cmdVenta = new MySqlCommand(queryVenta, conn, transaccion))
                    {
                        cmdVenta.Parameters.AddWithValue("@id_sucursal", idSucursal);
                        cmdVenta.Parameters.AddWithValue("@total", totalVenta);
                        idVentaGenerado = Convert.ToInt32(cmdVenta.ExecuteScalar());
                    }

                    // crea detallev de la venta
                    string queryDetail = "INSERT INTO detalleventa (idVenta, idProducto, Cantidad, PrecioUnitario) VALUES (@id_venta, @id_producto, @cantidad, @precio_unitario)";
                    using (MySqlCommand cmdDetalle = new MySqlCommand(queryDetail, conn, transaccion))
                    {
                        cmdDetalle.Parameters.AddWithValue("@id_venta", idVentaGenerado);
                        cmdDetalle.Parameters.AddWithValue("@id_producto", prod.Id);
                        cmdDetalle.Parameters.AddWithValue("@cantidad", cantidad);
                        cmdDetalle.Parameters.AddWithValue("@precio_unitario", prod.CalcularPrecioFinal());

                        cmdDetalle.ExecuteNonQuery();
                    }

                    // restamos el stock del producto vendido
                    string queryActualizarStock = "UPDATE producto SET Stock = Stock - @cantidad WHERE idProducto = @id";
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