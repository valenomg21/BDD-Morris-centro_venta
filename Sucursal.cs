using System.Collections.Generic;

namespace SistemaGestionVentas
{
    // Composición: Una sucursal "tiene" una lista de productos
    public class Sucursal
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public List<Producto> Productos { get; set; }

        public Sucursal(int id, string nombre)
        {
            Id = id;
            Nombre = nombre;
            Productos = new List<Producto>();
        }
    }
}