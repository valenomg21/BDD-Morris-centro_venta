namespace SistemaGestionVentas
{
    // Herencia: Televisor "es un" Producto
    public class Televisor : Producto
    {
        public int Pulgadas { get; set; }
        public string TipoPantalla { get; set; }

        public Televisor(string codigo, string nombre, decimal precio, int stock, int idSucursal, int pulgadas, string tipoPantalla)
            : base(codigo, nombre, precio, stock, idSucursal)
        {
            Pulgadas = pulgadas;
            TipoPantalla = tipoPantalla;
        }

        // Polimorfismo: Las TVs tienen un recargo del 10% por costos aduaneros de tecnología
        public override decimal CalcularPrecioFinal()
        {
            return Precio * 1.10m;
        }
    }
}