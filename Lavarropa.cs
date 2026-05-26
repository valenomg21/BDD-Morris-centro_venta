namespace SistemaGestionVentas
{
    // Herencia: Lavarropas "es un" Producto
    public class Lavarropas : Producto
    {
        public int Carga { get; set; }
        public string Tipo { get; set; } // automatico / semi

        public Lavarropas(string codigo, string nombre, decimal precio, int stock, int idSucursal, int carga, string tipo)
            : base(codigo, nombre, precio, stock, idSucursal)
        {
            Carga = carga;
            Tipo = tipo;
        }

        // Polimorfismo: Los lavarropas mantienen su precio base (sin recargos)
        public override decimal CalcularPrecioFinal()
        {
            return Precio;
        }
    }
}