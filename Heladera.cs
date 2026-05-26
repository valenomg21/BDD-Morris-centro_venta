namespace SistemaGestionVentas
{
    // heladera hereda de producto
    public class Heladera : Producto
    {
        public int Capacidad { get; set; }
        public string Tipo { get; set; } // no frost / freezer

        public Heladera(string codigo, string nombre, decimal precio, int stock, int idSucursal, int capacidad, string tipo)
            : base(codigo, nombre, precio, stock, idSucursal)
        {
            Capacidad = capacidad;
            Tipo = tipo;
        }

        // modifica el método de la clase padre
        public override decimal CalcularPrecioFinal()
        {
            return Precio * 1.05m;
        }
    }
}