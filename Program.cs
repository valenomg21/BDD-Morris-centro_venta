using System;
using System.Collections.Generic;

namespace SistemaGestionVentas
{
    class Program
    {
        //guarda la direccion de la bdd
        private static string connectionString = "Server=localhost;Database=electrodomesticosdb;Uid=root;Pwd=pokemon123;AllowPublicKeyRetrieval=True;SslMode=Disabled;";
        private static DatabaseManager db;
        private static int sucursalActivaId = 0;
        private static string sucursalActivaNombre = "";

        static void Main(string[] args)
        {
            Console.Title = "UTN - Sistema de Control de Stock y Ventas";

            //inicializa la conexion
            ConfigurarConexion();

            CambiarDeSucursal();

            bool ejecutar = true;
            while (ejecutar)
            {
                Console.Clear();
                Console.WriteLine("==================================================");
                Console.WriteLine($" SUCURSAL SELECCIONADA: {sucursalActivaNombre.ToUpper()}");
                Console.WriteLine("==================================================");
                Console.WriteLine("1. Cambiar de Sucursal");
                Console.WriteLine("2. Registrar un Producto");
                Console.WriteLine("3. Ver Lista de Productos");
                Console.WriteLine("4. Modificar Producto (Precio / Stock)");
                Console.WriteLine("5. Eliminar un Producto");
                Console.WriteLine("6. Registrar Venta (Transacción SQL)");
                Console.WriteLine("7. Salir del Sistema");
                Console.Write("\nSeleccione una opción: ");

                string opcion = Console.ReadLine();

                switch (opcion)
                {
                    case "1":
                        CambiarDeSucursal();
                        break;
                    case "2":
                        AgregarProductoMenu();
                        break;
                    case "3":
                        ListarProductosMenu();
                        break;
                    case "4":
                        ModificarProductoMenu();
                        break;
                    case "5":
                        EliminarProductoMenu();
                        break;
                    case "6":
                        RegistrarVentaMenu();
                        break;
                    case "7":
                        ejecutar = false;
                        Console.WriteLine("\nGracias por usar el sistema. Saliendo...");
                        break;
                    default:
                        Console.WriteLine("\nOpción inválida. Presione cualquier tecla para intentar de nuevo.");
                        Console.ReadKey();
                        break;
                }
            }
        }

        #region CONFIGURACIÓN DE CONEXIÓN

        static void ConfigurarConexion()
        {
            // instancia el admin de la bdd
            db = new DatabaseManager(connectionString);
        }

        #endregion

        #region METODOS DE VALIDACION DE ENTRADAS (TryParse básicos)

        static string LeerTexto(string mensaje)
        {
            while (true)
            {
                Console.Write(mensaje);
                string entrada = Console.ReadLine()?.Trim() ?? "";
                if (!string.IsNullOrEmpty(entrada))
                {
                    return entrada;
                }
                Console.WriteLine("[Error] Este campo no puede quedar vacío.");
            }
        }

        static int LeerEntero(string mensaje, int min = int.MinValue)
        {
            int resultado;
            while (true)
            {
                Console.Write(mensaje);
                if (int.TryParse(Console.ReadLine(), out resultado) && resultado >= min)
                {
                    return resultado;
                }
                Console.WriteLine($"[Error] Por favor, ingrese un número entero válido (Mínimo: {min}).");
            }
        }

        static decimal LeerDecimal(string mensaje, decimal min = 0)
        {
            decimal resultado;
            while (true)
            {
                Console.Write(mensaje);
                if (decimal.TryParse(Console.ReadLine(), out resultado) && resultado >= min)
                {
                    return resultado;
                }
                Console.WriteLine($"[Error] Por favor, ingrese un monto decimal válido (Mínimo: {min}).");
            }
        }

        #endregion

        #region MENUS DEL SISTEMA

        static void CambiarDeSucursal()
        {
            bool sucursalSeleccionada = false;

            while (!sucursalSeleccionada)
            {
                Console.Clear();
                Console.WriteLine("=== SELECCIONE UNA SUCURSAL DE OPERACIÓN ===");
                try
                {
                    List<Sucursal> lista = db.ObtenerSucursales();
                    foreach (var suc in lista)
                    {
                        Console.WriteLine($"{suc.Id} - {suc.Nombre}");
                    }

                    int idSel = LeerEntero("\nIngrese el ID de la sucursal: ");
                    string nombre = db.ObtenerNombreSucursal(idSel);

                    if (nombre != null)
                    {
                        sucursalActivaId = idSel;
                        sucursalActivaNombre = nombre;
                        sucursalSeleccionada = true; 
                    }
                    else
                    {
                        Console.WriteLine("\n[Error] ID de sucursal inexistente. Presione una tecla para reintentar...");
                        Console.ReadKey();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\n[Error] Falló la conexión: " + ex.Message);
                    Console.WriteLine("Presione una tecla para reintentar...");
                    Console.ReadKey();
                }
            }
        }

        static void AgregarProductoMenu()
        {
            Console.Clear();
            Console.WriteLine("=== REGISTRAR NUEVO PRODUCTO ===");
            
            string codigo = LeerTexto("Código único: ");
            string nombre = LeerTexto("Nombre descriptivo: ");
            decimal precio = LeerDecimal("Precio Base: ", min: 0.01m);
            int stock = LeerEntero("Stock Inicial: ", min: 0);

            Console.WriteLine("\nTipo de electrodoméstico:");
            Console.WriteLine("1. Televisor");
            Console.WriteLine("2. Heladera");
            Console.WriteLine("3. Lavarropas");
            string tipo = LeerTexto("Seleccione opción: ");

            Producto nuevoProd = null;

            if (tipo == "1")
            {
                int pulgadas = LeerEntero("Pulgadas de pantalla: ", min: 1);
                string tipoPant = LeerTexto("Tipo de pantalla (LED, Smart, etc.): ");
                nuevoProd = new Televisor(codigo, nombre, precio, stock, sucursalActivaId, pulgadas, tipoPant);
            }
            else if (tipo == "2")
            {
                int cap = LeerEntero("Capacidad en Litros: ", min: 1);
                string tipoH = LeerTexto("Tipo (no frost / freezer): ");
                nuevoProd = new Heladera(codigo, nombre, precio, stock, sucursalActivaId, cap, tipoH);
            }
            else if (tipo == "3")
            {
                int carga = LeerEntero("Carga máxima (kg): ", min: 1);
                string tipoL = LeerTexto("Tipo (automatico / semi): ");
                nuevoProd = new Lavarropas(codigo, nombre, precio, stock, sucursalActivaId, carga, tipoL);
            }

            if (nuevoProd != null)
            {
                try
                {
                    db.GuardarProducto(nuevoProd);
                    Console.WriteLine("\n¡Producto guardado exitosamente en la Base de Datos!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nError al guardar en BD: " + ex.Message);
                }
            }
            else
            {
                Console.WriteLine("\nOpción de tipo inválida. Registro cancelado.");
            }

            Console.WriteLine("\nPresione cualquier tecla para continuar...");
            Console.ReadKey();
        }

        static List<Producto> ListarProductosMenu(bool pausarAlFinal = true)
        {
            Console.Clear();
            Console.WriteLine($"=== INVENTARIO DE {sucursalActivaNombre.ToUpper()} ===");
            List<Producto> productos = new List<Producto>();

            try
            {
                productos = db.ObtenerProductosPorSucursal(sucursalActivaId);
                if (productos.Count == 0)
                {
                    Console.WriteLine("No hay productos cargados en esta sucursal.");
                }
                else
                {
                    foreach (var p in productos)
                    {
                        Console.WriteLine($"ID: {p.Id} | Código: {p.Codigo} | {p.Nombre} | Base: ${p.Precio} | Final: ${p.CalcularPrecioFinal()} | Stock: {p.Stock}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al recuperar el inventario: " + ex.Message);
            }

            if (pausarAlFinal)
            {
                Console.WriteLine("\nPresione cualquier tecla para continuar...");
                Console.ReadKey();
            }

            return productos;
        }

        static void ModificarProductoMenu()
        {
            List<Producto> productos = ListarProductosMenu(false);
            if (productos.Count == 0)
            {
                Console.ReadKey();
                return;
            }

            int idSel = LeerEntero("\nIngrese el ID del producto que desea modificar: ");
            decimal precio = LeerDecimal("Nuevo Precio Base: ", min: 0.01m);
            int stock = LeerEntero("Nuevo Stock: ", min: 0);

            try
            {
                bool exito = db.ModificarProducto(idSel, sucursalActivaId, precio, stock);
                if (exito)
                    Console.WriteLine("\n¡Producto actualizado con éxito!");
                else
                    Console.WriteLine("\nNo se encontró el producto o no pertenece a esta sucursal.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al modificar en la BD: " + ex.Message);
            }

            Console.WriteLine("\nPresione cualquier tecla para continuar...");
            Console.ReadKey();
        }

        static void EliminarProductoMenu()
        {
            List<Producto> productos = ListarProductosMenu(false);
            if (productos.Count == 0)
            {
                Console.ReadKey();
                return;
            }

            int idSel = LeerEntero("\nIngrese el ID del producto a eliminar: ");
            string confirmacion = LeerTexto("¿Está seguro de que desea eliminar permanentemente este producto? (S/N): ");
            
            if (confirmacion.ToUpper() == "S")
            {
                try
                {
                    bool exito = db.EliminarProducto(idSel, sucursalActivaId);
                    if (exito)
                        Console.WriteLine("\n¡Producto eliminado exitosamente!");
                    else
                        Console.WriteLine("\nNo se pudo eliminar el producto (verifique el ID).");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al eliminar de la BD: " + ex.Message);
                }
            }
            else
            {
                Console.WriteLine("\nOperación cancelada.");
            }

            Console.WriteLine("\nPresione cualquier tecla para continuar...");
            Console.ReadKey();
        }

        static void RegistrarVentaMenu()
        {
            List<Producto> productos = ListarProductosMenu(false);
            if (productos.Count == 0)
            {
                Console.ReadKey();
                return;
            }

            int idSel = LeerEntero("\nIngrese el ID del producto que va a vender: ");

            Producto prodElegido = productos.Find(p => p.Id == idSel);
            if (prodElegido == null)
            {
                Console.WriteLine("\nProducto no encontrado.");
                Console.ReadKey();
                return;
            }

            int cant = LeerEntero($"Ingrese cantidad a vender (Stock actual: {prodElegido.Stock}): ", min: 1);

            if (cant > prodElegido.Stock)
            {
                Console.WriteLine("\n¡Error! No hay suficiente stock para cubrir la demanda.");
                Console.ReadKey();
                return;
            }

            decimal totalVenta = prodElegido.CalcularPrecioFinal() * cant;

            db.RegistrarVentaConTransaccion(sucursalActivaId, prodElegido, cant, totalVenta);

            Console.WriteLine("\nPresione cualquier tecla para continuar...");
            Console.ReadKey();
        }

        #endregion
    }
}