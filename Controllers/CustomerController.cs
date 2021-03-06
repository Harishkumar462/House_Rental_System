using House_Rental_System.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace House_Rental_System.Controllers
{
    public class CustomerController : Controller
    {
        House_Rental Db = new House_Rental();
        // GET: Customer
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Profile()
        {
            int id = (int)Session["id"];
            var result = Db.Customer_Details.Where(m => m.Customer_Id == id).FirstOrDefault();
            ViewBag.image = result.Customer_Profile;
            return View(result);
        }
        [HttpGet]
        public ActionResult EditProfile(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer_Details sd = Db.Customer_Details.Find(id);
            if (sd == null)
            {
                return HttpNotFound();
            }
            return View(sd);
        }
        [HttpPost]
        public ActionResult EditProfile([Bind(Include = "Customer_Id,Customer_Name,Customer_Email,Customer_State,Customer_Phone,Customer_City,Customer_Password")] Customer_Details cd, HttpPostedFileBase image)
        {
            if (image != null)
            {
                cd.Customer_Profile = new byte[image.ContentLength];
                image.InputStream.Read(cd.Customer_Profile, 0, image.ContentLength);
            }
            if (ModelState.IsValid)
            {
                cd.Customer_Id = (int)Session["id"];
                Db.Entry(cd).State = EntityState.Modified;
                Db.SaveChanges();
                return RedirectToAction("Profile");
            }
            return View(cd);
        }
    }
}