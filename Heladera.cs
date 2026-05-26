namespace SistemaGestionVentas
{
    // Herencia: Heladera "es un" Producto
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

        // Polimorfismo: Las heladeras tienen un 5% de recargo por flete especial de línea blanca
        public override decimal CalcularPrecioFinal()
        {
            return Precio * 1.05m;
        }
    }
}