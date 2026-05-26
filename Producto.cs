namespace SistemaGestionVentas
{
    // Clase abstracta: No se puede instanciar directamente, solo sirve de base (Abstracción)
    public abstract class Producto
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public int IdSucursal { get; set; }

        // Constructor base para heredar a los hijos
        protected Producto(string codigo, string nombre, decimal precio, int stock, int idSucursal)
        {
            Codigo = codigo;
            Nombre = nombre;
            Precio = precio;
            Stock = stock;
            IdSucursal = idSucursal;
        }

        // Método abstracto: Obliga a cada hijo a calcular su precio final a su manera (Polimorfismo)
        public abstract decimal CalcularPrecioFinal();
    }
}