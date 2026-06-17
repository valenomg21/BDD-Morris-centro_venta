namespace SistemaGestionVentas
{
    // clase que representa un producro
    public abstract class Producto
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public int IdSucursal { get; set; }

        // constructor de clase producto
        protected Producto(string codigo, string nombre, decimal precio, int stock, int idSucursal)
        {
            Codigo = codigo;
            Nombre = nombre;
            Precio = precio;
            Stock = stock;
            IdSucursal = idSucursal;
        }

        // obliga a las clases hijas a usar el mismo metodo
        public abstract decimal CalcularPrecioFinal();
    }
}