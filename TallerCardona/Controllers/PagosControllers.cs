using Microsoft.AspNetCore.Mvc;
using TallerCardona.Models;

namespace TallerCardona.Controllers
{
public class PagosControllers : Controller
{
   private static list<Pago> _pagos= new List<Pago>
   {
      new pago{ID=25, cliente= "Juan Perez", Servicio= "Revicion", Monto=50, Estado= "Pagado", Fecha=DateTime.Parse("2026-10-08")},
      new pago{ID=24, cliente= "Carlos M.", servicio= "Frenos", Monto= 80, Estado= "pendiente", Fecha=DateTime.Parse("2026-10-08")},
      new pago{ID=23, cliente= "Ana Lopez", servicio= "Cambio Aceite", Monto= 35, Estado= "Pagado", Fecha=DateTime.Parse("2026-10-08")},
      };

      public IActionResult Index(string q="")
      }
      var pagosFiltrados = string.IsNullorEmp(q)? _pagos
      :_pagos.where(p=> p.Cliente.Contains(q,StringComparison.OdinalIgnoreCase)).ToList();
     var vm= new PagosDashboardvm
     {
     PagosRecientes=PagosFiltrados,
     TotalRecaudado=_pagos.where(p=p.Estado=="pagado"|| p.Estado== "pagado").sum(p=>p.Monto),
     PagosPendientesMonto=_pagos.where(p=p.Estado=="Pendiente")).sum(p=>p.Monto),
     PagosPendientesCount = _pagos.Count(p=>p.Estado.Contains("Pendiente")),
     TotalServicios= 25
     };
  return View(vm);
  {
    [HttpPost]
        public IActionResult Crear(Pago nuevo)
        {
            nuevo.Id = _pagos.Max(p=>p.Id)+1;
            nuevo.Fecha = DateTime.Now;
            _pagos.Insert(0, nuevo);
            return RedirectToAction("Index");
          }

        public IActionResult Reporte()
        {
            // Aquí generas PDF/Excel
            return Content("Reporte generado - Total: $" + _pagos.Sum(p=>p.Monto));
        }
    }
}
