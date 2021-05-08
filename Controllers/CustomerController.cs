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
            int id = (int)Session["id"];
            var result=Db.Customer_Details.Where(m=>m.Customer_Id==id).FirstOrDefault();
            var propid = Db.Booking_Details.Where(m=>m.Customer_Id==id).Select(m => m.Property_Id);
            List<Property_Details> property = Db.Property_Details.Where(m => m.Property_City == result.Customer_City && !propid.Contains(m.Property_ID) &&m.Property_Status=="Available").ToList<Property_Details>();
            return View(property);
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
            else
            {
                int id = (int)Session["id"];
                var img = Db.Customer_Details.Where(m => m.Customer_Id == id).Select(m => m.Customer_Profile).FirstOrDefault();
                if (img != null)
                {
                    cd.Customer_Profile =img;
                }
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
        [HttpGet]
        public ActionResult Details(int? id)
        {
            var result = Db.Property_Details.Where(m => m.Property_ID == id).FirstOrDefault();
            return View(result);
        }

        public ActionResult PropertyRequest(int pid,int Sid)
        {
            int cid = (int)Session["id"];
            Booking_Details bd = new Booking_Details
            {
                Customer_Id = cid,
                Property_Id = pid,
                Seller_Id = Sid
            };
            Db.Booking_Details.Add(bd);
            Db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult Property()
        {
            int id =(int) Session["id"];
            var propertyids = Db.Booking_Details.Where(m => m.Customer_Id == id).Select(m => m.Property_Id);
            List<Property_Details> pd = Db.Property_Details.Where(m => propertyids.Contains(m.Property_ID)).ToList<Property_Details>();
            return View(pd);
        }
        public ActionResult DeleteRequest(int id)
        {
            int cid = (int)Session["id"];
            Booking_Details bd = Db.Booking_Details.Where(m => m.Customer_Id == cid && m.Property_Id == id).FirstOrDefault();
            Db.Booking_Details.Remove(bd);
            Db.SaveChanges();
            return RedirectToAction("Property");
        }
        public ActionResult RentedProperties()
        {
            int id = (int)Session["id"];
            List<Sold_Property> sp = Db.Sold_Property.Where(m => m.Customer_Id == id).ToList<Sold_Property>();
            return View(sp);
        }
        public ActionResult Search(string search)
        {
            List<Property_Details> property = Db.Property_Details.Where(m => m.Property_City == search && m.Property_Status == "Available").ToList<Property_Details>();
            return View(property);
        }
    }
}