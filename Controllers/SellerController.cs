using House_Rental_System.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Windows;

namespace House_Rental_System.Controllers
{
    public class SellerController : Controller
    {
        // GET: Seller
        House_Rental Db = new House_Rental();
        public ActionResult Index()
        {
            int id = (int)Session["id"];
            List<Property_Details> pd = Db.Property_Details.Where(m => m.Seller_Id==id).ToList<Property_Details>();
            ViewBag.ps = pd;
            return View();
        }
        public ActionResult AddProperty()
        {
            List<string> type = new List<string>() {"House","Flat"};
            List<string> status = new List<string>() { "Available", "UnAvailable" };
            ViewBag.types = type;
            ViewBag.Status = status;
            return View();
        }
        [HttpPost]
        public ActionResult AddProperty(Property_Details pd,HttpPostedFileBase[] images)
        {
            if (ModelState.IsValid)
            {
                pd.Seller_Id =(int) Session["id"];
                Db.Property_Details.Add(pd);
                Db.SaveChanges();
                int id = Db.Property_Details.Max(p => p.Property_ID);
                if (images != null)
                {
                    foreach (var image in images)
                    {
                        BinaryReader binary = new BinaryReader(image.InputStream);
                        Property_Images pi = new Property_Images
                        {
                            Property_Id = id,
                            Image = binary.ReadBytes((int)image.ContentLength)
                        };
                        Db.Property_Images.Add(pi);
                    }
                    Db.SaveChanges();
                }
                
               
            }
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int? id)
        {
            List<string> type = new List<string>() { "House", "Flat" };
            List<string> status = new List<string>() { "Available", "UnAvailable" };
            ViewBag.types = type;
            ViewBag.Status = status;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Property_Details property_Details = Db.Property_Details.Find(id);
            if (property_Details == null)
            {
                return HttpNotFound();
            }
            ViewBag.Seller_Id = new SelectList(Db.Seller_Details, "Seller_ID", "Seller_Name", property_Details.Seller_Id);
            return View(property_Details);
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Property_ID,Property_Type,Property_Name,Property_Address,Property_State,Property_City,Property_Pin,Property_Status,Seller_Id")] Property_Details property_Details)
        {
            if (ModelState.IsValid)
            {
                property_Details.Seller_Id = (int)Session["id"];
                Db.Entry(property_Details).State = EntityState.Modified;
                Db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Seller_Id = new SelectList(Db.Seller_Details, "Seller_ID", "Seller_Name", property_Details.Seller_Id);
            return View(property_Details);
        }
        public ActionResult Profile()
        {
            int id =(int) Session["id"];
            var result = Db.Seller_Details.Where(m=>m.Seller_ID==id).FirstOrDefault();
            ViewBag.image =result.Seller_Photo ;
            return View(result);
        }
        [HttpGet]
        public ActionResult EditProfile(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Seller_Details sd = Db.Seller_Details.Find(id);
            if (sd == null)
            {
                return HttpNotFound();
            }
            return View(sd);
        }
        [HttpPost]
        public ActionResult EditProfile([Bind(Include = "Seller_ID,Seller_Name,Seller_Email,Seller_Phone,Seller_State,Seller_Password") ]Seller_Details sd, HttpPostedFileBase image)
        {
            if (image != null)
            {
                sd.Seller_Photo = new byte[image.ContentLength];
                image.InputStream.Read(sd.Seller_Photo, 0, image.ContentLength);
            }
            if (ModelState.IsValid)
            {
                sd.Seller_ID = (int)Session["id"];
                Db.Entry(sd).State = EntityState.Modified;
                Db.SaveChanges();
                return RedirectToAction("Profile");
            }
            return View(sd);
        }
        public ActionResult DeleteProperty(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Property_Details pd = Db.Property_Details.Find(id);
            if (pd == null)
            {
                return HttpNotFound();
            }
            return View(pd);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int? id)
        {
            Property_Details pd = Db.Property_Details.Find(id);
            Db.Property_Details.Remove(pd);
            Db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult RequestedProperty()
        {
            int id = (int)Session["id"];
            
           
            return View();
        }
    }
}