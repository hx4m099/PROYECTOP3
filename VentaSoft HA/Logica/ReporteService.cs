using Datos;
using Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logica
{
    public class ReporteService
    {
        private ReporteRepository reporteRepository = new ReporteRepository();

        public List<ReporteCompra> Compra(string fechainicio, string fechafin, int idproveedor)
        {
            return reporteRepository.Compra(fechainicio, fechafin, idproveedor);
        }


        public List<ReporteVenta> Venta(string fechainicio, string fechafin)
        {
            return reporteRepository.Venta(fechainicio, fechafin);
        }
    }
}
