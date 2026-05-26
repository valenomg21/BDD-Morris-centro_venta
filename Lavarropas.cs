namespace SistemaGestionVentas
{
    // hereda de producto
    public class Lavarropas : Producto
    {
        public int Carga { get; set; }
        public string Tipo { get; set; } // automatico, semi, etc

        public Lavarropas(string codigo, string nombre, decimal precio, int stock, int idSucursal, int carga, string tipo)
            : base(codigo, nombre, precio, stock, idSucursal)
        {
            Carga = carga;
            Tipo = tipo;
        }

        // polimorfismo, modifica el método de la clase padre
        public override decimal CalcularPrecioFinal()
        {
            return Precio;
        }
    }
}