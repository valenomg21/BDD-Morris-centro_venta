using System.Collections.Generic;

namespace SistemaGestionVentas
{
    // composición: una sucursal "tiene" productos
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