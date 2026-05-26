using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace SistemaGestionVentas
{
    public class DatabaseManager
    {
        private string connectionString;

        // El constructor recibe la conexión de forma segura
        public DatabaseManager(string connStr)
        {
            connectionString = connStr;
        }

        // Método para listar las sucursales existentes
        public List<Sucursal> ObtenerSucursales()
        {
            List<Sucursal> sucursales = new List<Sucursal>();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT id, nombre FROM sucursal";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sucursales.Add(new Sucursal(
                            Convert.ToInt32(reader["id"]),
                            reader["nombre"].ToString()
                        ));
                    }
                }
            }
            return sucursales;
        }

        // obtiene el nombre de la sucursal por su id
        public string ObtenerNombreSucursal(int id)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT nombre FROM sucursal WHERE id = @id";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    object result = cmd.ExecuteScalar();
                    return result != null ? result.ToString() : null;
                }
            }
        }

        // guarda un producto en la bdd
        public void GuardarProducto(Producto prod)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = @"INSERT INTO producto 
                    (codigo, nombre, precio, stock, id_sucursal, tipo, tv_pulgadas, tv_tipo_pantalla, hel_capacidad, hel_tipo, lav_carga, lav_tipo) 
                    VALUES (@codigo, @nombre, @precio, @stock, @id_sucursal, @tipo, @tv_pulgadas, @tv_tipo_pantalla, @hel_capacidad, @hel_tipo, @lav_carga, @lav_tipo)";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@codigo", prod.Codigo);
                    cmd.Parameters.AddWithValue("@nombre", prod.Nombre);
                    cmd.Parameters.AddWithValue("@precio", prod.Precio);
                    cmd.Parameters.AddWithValue("@stock", prod.Stock);
                    cmd.Parameters.AddWithValue("@id_sucursal", prod.IdSucursal);

                    // determian el tipo de producto y asigna los parámetros específicos, dejando null los que no se usen
                    if (prod is Televisor tv)
                    {
                        cmd.Parameters.AddWithValue("@tipo", "Televisor");
                        cmd.Parameters.AddWithValue("@tv_pulgadas", tv.Pulgadas);
                        cmd.Parameters.AddWithValue("@tv_tipo_pantalla", tv.TipoPantalla);
                        cmd.Parameters.AddWithValue("@hel_capacidad", DBNull.Value);
                        cmd.Parameters.AddWithValue("@hel_tipo", DBNull.Value);
                        cmd.Parameters.AddWithValue("@lav_carga", DBNull.Value);
                        cmd.Parameters.AddWithValue("@lav_tipo", DBNull.Value);
                    }
                    else if (prod is Heladera hel)
                    {
                        cmd.Parameters.AddWithValue("@tipo", "Heladera");
                        cmd.Parameters.AddWithValue("@tv_pulgadas", DBNull.Value);
                        cmd.Parameters.AddWithValue("@tv_tipo_pantalla", DBNull.Value);
                        cmd.Parameters.AddWithValue("@hel_capacidad", hel.Capacidad);
                        cmd.Parameters.AddWithValue("@hel_tipo", hel.Tipo);
                        cmd.Parameters.AddWithValue("@lav_carga", DBNull.Value);
                        cmd.Parameters.AddWithValue("@lav_tipo", DBNull.Value);
                    }
                    else if (prod is Lavarropas lav)
                    {
                        cmd.Parameters.AddWithValue("@tipo", "Lavarropas");
                        cmd.Parameters.AddWithValue("@tv_pulgadas", DBNull.Value);
                        cmd.Parameters.AddWithValue("@tv_tipo_pantalla", DBNull.Value);
                        cmd.Parameters.AddWithValue("@hel_capacidad", DBNull.Value);
                        cmd.Parameters.AddWithValue("@hel_tipo", DBNull.Value);
                        cmd.Parameters.AddWithValue("@lav_carga", lav.Carga);
                        cmd.Parameters.AddWithValue("@lav_tipo", lav.Tipo);
                    }

                    cmd.ExecuteNonQuery();
                }
            }
        }

        // carga los productos de una sucursal específica
        public List<Producto> ObtenerProductosPorSucursal(int idSucursal)
        {
            List<Producto> lista = new List<Producto>();

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM producto WHERE id_sucursal = @id_sucursal";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id_sucursal", idSucursal);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string tipo = reader["tipo"].ToString();
                            string codigo = reader["codigo"].ToString();
                            string nombre = reader["nombre"].ToString();
                            decimal precio = Convert.ToDecimal(reader["precio"]);
                            int stock = Convert.ToInt32(reader["stock"]);
                            int id = Convert.ToInt32(reader["id"]);

                            Producto prod = null;

                            if (tipo == "Televisor")
                            {
                                int pulgadas = Convert.ToInt32(reader["tv_pulgadas"]);
                                string pant = reader["tv_tipo_pantalla"].ToString();
                                prod = new Televisor(codigo, nombre, precio, stock, idSucursal, pulgadas, pant);
                            }
                            else if (tipo == "Heladera")
                            {
                                int cap = Convert.ToInt32(reader["hel_capacidad"]);
                                string tipoH = reader["hel_tipo"].ToString();
                                prod = new Heladera(codigo, nombre, precio, stock, idSucursal, cap, tipoH);
                            }
                            else if (tipo == "Lavarropas")
                            {
                                int carga = Convert.ToInt32(reader["lav_carga"]);
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

        // modifica el precio y stock de los productos
        public bool ModificarProducto(int id, int idSucursal, decimal nuevoPrecio, int nuevoStock)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE producto SET precio = @precio, stock = @stock WHERE id = @id AND id_sucursal = @id_sucursal";
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

        // elimina un producto de la bdd
        public bool EliminarProducto(int id, int idSucursal)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "DELETE FROM producto WHERE id = @id AND id_sucursal = @id_sucursal";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@id_sucursal", idSucursal);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // realiza el registro de una venta 
        public void RegistrarVentaConTransaccion(int idSucursal, Producto prod, int cantidad, decimal totalVenta)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                // inicia la transaccion
                MySqlTransaction transaccion = conn.BeginTransaction();

                try
                {
                    // 1.crea la venta y obtiene su id generado automáticamente
                    string queryVenta = "INSERT INTO venta (id_sucursal, total) VALUES (@id_sucursal, @total); SELECT LAST_INSERT_ID();";
                    int idVentaGenerado = 0;

                    using (MySqlCommand cmdVenta = new MySqlCommand(queryVenta, conn, transaccion))
                    {
                        cmdVenta.Parameters.AddWithValue("@id_sucursal", idSucursal);
                        cmdVenta.Parameters.AddWithValue("@total", totalVenta);
                        idVentaGenerado = Convert.ToInt32(cmdVenta.ExecuteScalar());
                    }

                    // 2. crear detalle de la venta con el producto vendido
                    string queryDetalle = "INSERT INTO detalle_venta (id_venta, id_producto, cantidad, precio_unitario) VALUES (@id_venta, @id_producto, @cantidad, @precio_unitario)";
                    using (MySqlCommand cmdDetalle = new MySqlCommand(queryDetalle, conn, transaccion))
                    {
                        cmdDetalle.Parameters.AddWithValue("@id_venta", idVentaGenerado);
                        cmdDetalle.Parameters.AddWithValue("@id_producto", prod.Id);
                        cmdDetalle.Parameters.AddWithValue("@cantidad", cantidad);
                        cmdDetalle.Parameters.AddWithValue("@precio_unitario", prod.CalcularPrecioFinal());

                        cmdDetalle.ExecuteNonQuery();
                    }

                    // 3. descontar el stock del producto vendido
                    string queryActualizarStock = "UPDATE producto SET stock = stock - @cantidad WHERE id = @id";
                    using (MySqlCommand cmdStock = new MySqlCommand(queryActualizarStock, conn, transaccion))
                    {
                        cmdStock.Parameters.AddWithValue("@cantidad", cantidad);
                        cmdStock.Parameters.AddWithValue("@id", prod.Id);

                        cmdStock.ExecuteNonQuery();
                    }

                    // si no hubo excepciones se confirma la transaccion y guarda todo en la base de datos
                    transaccion.Commit();
                    Console.WriteLine("\n=================================");
                    Console.WriteLine(" ¡VENTA REALIZADA CON ÉXITO!");
                    Console.WriteLine($" Total final facturado: ${totalVenta}");
                    Console.WriteLine($" Nuevo stock del producto: {prod.Stock - cantidad}");
                    Console.WriteLine("=================================");
                }
                catch (Exception ex)
                {
                    // si algo falla en cualquiera de los pasos, se revierte todo lo hecho en la base de datos
                    transaccion.Rollback();
                    Console.WriteLine("\n[Error de Consistencia] Venta cancelada.");
                    Console.WriteLine("Detalle técnico: " + ex.Message);
                }
            }
        }
    }
}