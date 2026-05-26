namespace SistemaGestionVentas
{
    // televisor hereda de producto
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

        // modifica el metodo de la clase padre
        public override decimal CalcularPrecioFinal()
        {
            return Precio * 1.10m;
        }
    }
}