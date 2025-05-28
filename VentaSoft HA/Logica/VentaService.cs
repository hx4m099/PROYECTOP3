using Datos;
using Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logica
{
    public class VentaService
    {
        private VentaRepository ventaRepository = new VentaRepository();

        public bool RestarStock(int idproducto, int cantidad)
        {
            return ventaRepository.RestarStock(idproducto, cantidad);
        }

        public bool SumarStock(int idproducto, int cantidad)
        {
            return ventaRepository.SumarStock(idproducto, cantidad);
        }

        public int ObtenerCorrelativo()
        {
            return ventaRepository.ObtenerCorrelativo();
        }

        public bool Registrar(Venta obj, DataTable DetalleVenta, out string Mensaje)
        {
            bool Respuesta = false;
            Mensaje = string.Empty;
            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    SqlCommand cmd = new SqlCommand("usp_RegistrarVenta", oconexion);
                    cmd.Parameters.AddWithValue("IdUsuario", obj.oUsuario.IdUsuario);
                    cmd.Parameters.AddWithValue("TipoDocumento", obj.TipoDocumento);
                    cmd.Parameters.AddWithValue("NumeroDocumento", obj.NumeroDocumento);
                    cmd.Parameters.AddWithValue("DocumentoCliente", obj.DocumentoCliente);
                    cmd.Parameters.AddWithValue("NombreCliente", obj.NombreCliente);
                    cmd.Parameters.AddWithValue("MontoPago", obj.MontoPago);
                    cmd.Parameters.AddWithValue("MontoCambio", obj.MontoCambio);
                    cmd.Parameters.AddWithValue("MontoTotal", obj.MontoTotal);
                    cmd.Parameters.AddWithValue("DescuentoAplicado", obj.DescuentoAplicado);
                    cmd.Parameters.AddWithValue("DetalleVenta", DetalleVenta);
                    cmd.Parameters.Add("Resultado", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("Mensaje", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;

                    oconexion.Open();
                    cmd.ExecuteNonQuery();

                    Respuesta = Convert.ToBoolean(cmd.Parameters["Resultado"].Value);
                    Mensaje = cmd.Parameters["Mensaje"].Value.ToString();
                }
            }
            catch (Exception ex)
            {
                Respuesta = false;
                Mensaje = ex.Message;
            }
            return Respuesta;
        }

        // Método para obtener una venta específica
        public Venta ObtenerVenta(string numero)
        {
            Venta venta = ventaRepository.ObtenerVenta(numero);

            // Si se encontró la venta, cargar sus detalles
            if (venta.IdVenta != 0)
            {
                venta.oDetalle_Venta = ventaRepository.ObtenerDetalleVenta(venta.IdVenta);
            }

            return venta;
        }

        public List<Detalle_Venta> ObtenerDetalleVenta(int idVenta)
        {
            return ventaRepository.ObtenerDetalleVenta(idVenta);
        }

        public decimal ObtenerTotalVentasDelMes(int mes, int año)
        {
            return ventaRepository.ObtenerTotalVentasDelMes(mes, año);
        }

        public int ObtenerTotalProductosVendidosDelMes(int mes, int año)
        {
            return ventaRepository.ObtenerTotalProductosVendidosDelMes(mes, año);
        }

        public int ObtenerClientesActivosDelMes(int mes, int año)
        {
            return ventaRepository.ObtenerClientesActivosDelMes(mes, año);
        }

        public List<Venta> ObtenerVentasRecientes(int cantidad = 10)
        {
            return ventaRepository.ObtenerVentasRecientes(cantidad);
        }

        public List<Producto> ObtenerProductosConStock()
        {
            return ventaRepository.ObtenerProductosConStock();
        }

        public Dictionary<string, decimal> ObtenerVentasPorMes(int año)
        {
            return ventaRepository.ObtenerVentasPorMes(año);
        }

        public List<ProductoVendido> ObtenerProductosMasVendidos(int cantidad = 10)
        {
            return ventaRepository.ObtenerProductosMasVendidos(cantidad);
        }
    }
}