using System;

namespace Entidades
{
    public class Detalle_Venta
    {
        public int IdDetalleVenta { get; set; }
        public int IdVenta { get; set; }
        public int IdProducto { get; set; }
        public decimal PrecioVenta { get; set; }
        public int Cantidad { get; set; }
        public decimal SubTotal { get; set; }
        public string FechaRegistro { get; set; }

        public Producto oProducto { get; set; }

        // Constructor para inicializar la propiedad
        public Detalle_Venta()
        {
            oProducto = new Producto();
        }
    }
}