using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BASIC_PROJECT.Models;

namespace BASIC_PROJECT.Controllers
{
    public class KhoController : Controller
    {
        NhaHangEntities3 db = new NhaHangEntities3();
        // GET: Kho
        public ActionResult Index()
        {
            return View(db.NguyenLieux.ToList());
        }
        public ActionResult NhapKho()
        {
            return View(db.CTNhapHangs.ToList());
        }
        public ActionResult NhapKho1()
        {
            return View(db.CTNhapHangs.ToList());
        }
    }
}