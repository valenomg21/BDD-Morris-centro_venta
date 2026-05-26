using System;
using System.Collections.Generic;

namespace SistemaGestionVentas
{
    class Program
    {
        // Recuerda ajustar la contraseña 'tu_contraseña' por la de tu MySQL local
        private static string connectionString = "Server=localhost;Database=gestion_ventas;Uid=root;Pwd=pokemon123;";
        private static DatabaseManager db;
        private static int sucursalActivaId = 0;
        private static string sucursalActivaNombre = "";

        static void Main(string[] args)
        {
            db = new DatabaseManager(connectionString);
            Console.Title = "UTN - Sistema de Control de Stock y Ventas";

            // Flujo obligatorio inicial: Seleccionar Sucursal
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

        static void CambiarDeSucursal()
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

                Console.Write("\nIngrese el ID numérico de la sucursal: ");
                if (int.TryParse(Console.ReadLine(), out int idSel))
                {
                    string nombre = db.ObtenerNombreSucursal(idSel);
                    if (nombre != null)
                    {
                        sucursalActivaId = idSel;
                        sucursalActivaNombre = nombre;
                    }
                    else
                    {
                        Console.WriteLine("\nID de sucursal no válido. Presione una tecla para reintentar...");
                        Console.ReadKey();
                        CambiarDeSucursal();
                    }
                }
                else
                {
                    Console.WriteLine("\nEntrada incorrecta. Presione una tecla para reintentar...");
                    Console.ReadKey();
                    CambiarDeSucursal();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nError al cargar sucursales: " + ex.Message);
                Console.WriteLine("¿Configuraste tu contraseña de MySQL en Program.cs?");
                Console.ReadKey();
                Environment.Exit(0);
            }
        }

        static void AgregarProductoMenu()
        {
            Console.Clear();
            Console.WriteLine("=== REGISTRAR NUEVO PRODUCTO ===");
            Console.Write("Código único: ");
            string codigo = Console.ReadLine();
            Console.Write("Nombre descriptivo: ");
            string nombre = Console.ReadLine();
            Console.Write("Precio Base: ");
            decimal precio = decimal.Parse(Console.ReadLine());
            Console.Write("Stock Inicial: ");
            int stock = int.Parse(Console.ReadLine());

            Console.WriteLine("\nTipo de electrodoméstico:");
            Console.WriteLine("1. Televisor");
            Console.WriteLine("2. Heladera");
            Console.WriteLine("3. Lavarropas");
            Console.Write("Seleccione opción: ");
            string tipo = Console.ReadLine();

            Producto nuevoProd = null;

            if (tipo == "1")
            {
                Console.Write("Pulgadas de pantalla: ");
                int pulgadas = int.Parse(Console.ReadLine());
                Console.Write("Tipo de pantalla (LED, Smart, etc.): ");
                string tipoPant = Console.ReadLine();
                nuevoProd = new Televisor(codigo, nombre, precio, stock, sucursalActivaId, pulgadas, tipoPant);
            }
            else if (tipo == "2")
            {
                Console.Write("Capacidad en Litros: ");
                int cap = int.Parse(Console.ReadLine());
                Console.Write("Tipo (no frost / freezer): ");
                string tipoH = Console.ReadLine();
                nuevoProd = new Heladera(codigo, nombre, precio, stock, sucursalActivaId, cap, tipoH);
            }
            else if (tipo == "3")
            {
                Console.Write("Carga máxima (kg): ");
                int carga = int.Parse(Console.ReadLine());
                Console.Write("Tipo (automatico / semi): ");
                string tipoL = Console.ReadLine();
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
            List<Producto> productos = db.ObtenerProductosPorSucursal(sucursalActivaId);

            if (productos.Count == 0)
            {
                Console.WriteLine("No hay productos cargados en esta sucursal.");
            }
            else
            {
                foreach (var p in productos)
                {
                    // Note que usamos CalcularPrecioFinal() para ver el precio dinámico calculado por Polimorfismo
                    Console.WriteLine($"ID: {p.Id} | Código: {p.Codigo} | {p.Nombre} | Base: ${p.Precio} | Final: ${p.CalcularPrecioFinal()} | Stock: {p.Stock}");
                }
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

            Console.Write("\nIngrese el ID del producto que desea modificar: ");
            if (int.TryParse(Console.ReadLine(), out int idSel))
            {
                Console.Write("Nuevo Precio Base: ");
                decimal precio = decimal.Parse(Console.ReadLine());
                Console.Write("Nuevo Stock: ");
                int stock = int.Parse(Console.ReadLine());

                bool exito = db.ModificarProducto(idSel, sucursalActivaId, precio, stock);
                if (exito)
                    Console.WriteLine("\n¡Producto actualizado con éxito!");
                else
                    Console.WriteLine("\nNo se encontró el producto o no pertenece a esta sucursal.");
            }
            else
            {
                Console.WriteLine("\nID inválido.");
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

            Console.Write("\nIngrese el ID del producto a eliminar: ");
            if (int.TryParse(Console.ReadLine(), out int idSel))
            {
                Console.Write("¿Está seguro de que desea eliminar permanentemente este producto? (S/N): ");
                if (Console.ReadLine().ToUpper() == "S")
                {
                    bool exito = db.EliminarProducto(idSel, sucursalActivaId);
                    if (exito)
                        Console.WriteLine("\n¡Producto eliminado exitosamente!");
                    else
                        Console.WriteLine("\nNo se pudo eliminar el producto.");
                }
            }
            else
            {
                Console.WriteLine("\nID inválido.");
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

            Console.Write("\nIngrese el ID del producto que va a vender: ");
            if (!int.TryParse(Console.ReadLine(), out int idSel)) return;

            Producto prodElegido = productos.Find(p => p.Id == idSel);
            if (prodElegido == null)
            {
                Console.WriteLine("\nProducto no encontrado.");
                Console.ReadKey();
                return;
            }

            Console.Write($"Ingrese cantidad a vender (Stock actual: {prodElegido.Stock}): ");
            if (int.TryParse(Console.ReadLine(), out int cant) && cant > 0)
            {
                if (cant > prodElegido.Stock)
                {
                    Console.WriteLine("\n¡Error! No hay suficiente stock para cubrir la demanda.");
                    Console.ReadKey();
                    return;
                }

                // Cálculo total multiplicando el precio calculado polimórficamente
                decimal totalVenta = prodElegido.CalcularPrecioFinal() * cant;

                // Llama al gestor de BD que ejecuta la transacción
                db.RegistrarVentaConTransaccion(sucursalActivaId, prodElegido, cant, totalVenta);
            }
            else
            {
                Console.WriteLine("\nCantidad inválida.");
            }

            Console.WriteLine("\nPresione cualquier tecla para continuar...");
            Console.ReadKey();
        }
    }
}