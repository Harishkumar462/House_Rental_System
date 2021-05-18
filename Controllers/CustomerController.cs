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
        [HttpPost]
        public JsonResult Autocomplete(string Prefix)
        {
            var property_city = (from p in Db.Property_Details
                                 where p.Property_City.StartsWith(Prefix)
                                 select new
                                 {
                                     label = p.Property_City,
                                     val = p.Property_City
                                 }).ToList();
            var prop = property_city.Distinct();
            return Json(prop);
            
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
            Session["search"] = search;
            if (Session["min"] != null)
            {
                int min = (int)Session["min"];
                int max = (int)Session["max"];
                List<Property_Details> property = Db.Property_Details.Where(m => m.Property_City == search && m.Property_Status == "Available" && m.Property_Information.ExpectedRent >= min && m.Property_Information.ExpectedRent <= max).ToList<Property_Details>(); 
                return View(property);
            }
            else
            {
                List<Property_Details> property = Db.Property_Details.Where(m => m.Property_City == search && m.Property_Status == "Available").ToList<Property_Details>();
                return View(property);
            }
            
            
        }
        public ActionResult PriceRange(int min,int max)
        {
            Session["min"] = min;
            Session["max"] = max;
            if (Session["search"] != null)
            {
                string city = (string)Session["search"];
                var result = Db.Property_Details.Where(m => m.Property_Information.ExpectedRent >= min && m.Property_Information.ExpectedRent <= max  && m.Property_City==city && m.Property_Status == "Available").ToList();
                return View(result);
            }
            else
            {
                var result = Db.Property_Details.Where(m => m.Property_Information.ExpectedRent >= min && m.Property_Information.ExpectedRent <= max && m.Property_Status == "Available").ToList();
                return View(result);
            }
            
        }
        public ActionResult TopSeller()
        {
            var sellers = from s in Db.Sold_Property group s by s.Seller_Id;
            var topseller = sellers.OrderByDescending(m => m.Count()).Take(5);
            var topsellerid = topseller.Select(m => m.Key);
            var seller = Db.Seller_Details.Where(m => topsellerid.Contains(m.Seller_ID)).ToList();
            List<int> count = new List<int>();
            foreach(var item in topsellerid)
            {
                count.Add(Db.Sold_Property.Count(m => m.Seller_Id == item));
            }
            ViewBag.count = count;
            return View(seller);
        }
    }
}