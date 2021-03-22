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
            int i = 0;
            int[] property_count = new int[pd.Count];
            foreach(var x in pd)
            {
                var result = Db.Booking_Details.Where(m => m.Property_Id == x.Property_ID).Count();
                if (result == 0)
                {
                    property_count[i] = 0;
                }
                else
                {
                    property_count[i] = 1;
                }
                i += 1;
            }
            ViewBag.pc = property_count;
            
            return View(pd);
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

        public ActionResult Requests(int id)
        {
            Session["propertyid"] = id;
            var customers_Id = Db.Booking_Details.Where(m=>m.Property_Id==id).Select(m => m.Customer_Id);
            List<Customer_Details> cd = Db.Customer_Details.Where(m => customers_Id.Contains(m.Customer_Id)).ToList<Customer_Details>();
            ViewBag.cds = cd;
            return View();
        }
        public ActionResult Confirmation(int cid)
        {
            Session["customerid"] = cid;
            return View();

        }
        [HttpPost]
        public ActionResult Confirmation(Sold_Property sp)
        {
            sp.Customer_Id = (int)Session["customerid"];
            sp.Property_Id = (int)Session["propertyid"];
            sp.Seller_Id = (int)Session["id"];
            sp.Date_of_Sale = DateTime.Now.ToString();
            Db.Sold_Property.Add(sp);
            Db.SaveChanges();
            int pid = (int)Session["propertyid"];
            int cid = (int)Session["customerid"];
            string cemail = Db.Customer_Details.Where(m => m.Customer_Id == cid).Select(m=>m.Customer_Email).FirstOrDefault();
            var customer = Db.Booking_Details.Where(m=>m.Customer_Id==cid&& m.Property_Id==pid).FirstOrDefault();
            string pname = Db.Property_Details.Where(m => m.Property_ID == pid).Select(m => m.Property_Name).FirstOrDefault();
            string paddress = Db.Property_Details.Where(m => m.Property_ID == pid).Select(m => m.Property_Address).FirstOrDefault();
            ConfirmationMail(cemail, pname,paddress);
            Db.Booking_Details.Remove(customer);
            Db.SaveChanges();
            List<Booking_Details> result = Db.Booking_Details.Where(m => m.Property_Id ==pid  && m.Customer_Id!=cid ).ToList<Booking_Details>();
            Property_Details bd = Db.Property_Details.Where(m => m.Property_ID == pid).FirstOrDefault();
            bd.Property_Status = "Not Available";
            Db.SaveChanges();
            if (result != null)
            {
                foreach (Booking_Details x in result)
                {
                    string email = Db.Customer_Details.Where(m => m.Customer_Id == x.Customer_Id).Select(m => m.Customer_Email).FirstOrDefault();
                    string propertyname = Db.Property_Details.Where(m => m.Property_ID == pid).Select(m => m.Property_Name).FirstOrDefault();
                    string propertyaddress = Db.Property_Details.Where(m => m.Property_ID == pid).Select(m => m.Property_Address).FirstOrDefault();
                    RejectionMail(email, propertyname, propertyaddress);
                    Db.Booking_Details.Remove(x);
                    Db.SaveChanges();
                }
            }
            
            Session.Remove("propertyid");
            Session.Remove("customerid");
            return RedirectToAction("Index");
        }

        public ActionResult SoldProperty()
        {
            int id = (int)Session["id"];
            List<Sold_Property> sold_Property = Db.Sold_Property.Where(m => m.Seller_Id == id).ToList<Sold_Property>();
            return View(sold_Property);
            
        }

        public void RejectionMail(string emailid,string propertyname,string propertyaddress)
        {
            MailMessage mailMessage = new MailMessage("harishsnape1999@gmail.com", emailid);
            mailMessage.IsBodyHtml = true;
            mailMessage.Subject = "House Rental";
            string link = "https://localhost:44376/Home/Index";
            mailMessage.Body = "<html><body><h1> The House/Flat " + propertyname+" "+ propertyaddress +
                "</h1><p>This property was Rented </p><p>Please search some other property</p><a href="+link+">Click Here</a></body></html>";
            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.Credentials = new System.Net.NetworkCredential()
            {
                UserName = "harishsnape1999@gmail.com",
                Password = "harish462"
            };

            smtpClient.EnableSsl = true;
            smtpClient.Send(mailMessage);
        }
        public void ConfirmationMail(string emailid, string propertyname, string propertyaddress)
        {
            MailMessage mailMessage = new MailMessage("harishsnape1999@gmail.com", emailid);
            mailMessage.IsBodyHtml = true;
            mailMessage.Subject = "House Rental";
            string link = "https://localhost:44376/Home/Index";
            mailMessage.Body = "<html><body><h1> The House/Flat " + propertyname + " " + propertyaddress +
                "</h1><p>This property was Rented to You </p><a href=" + link + ">Click Here</a></body></html>";
            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.Credentials = new System.Net.NetworkCredential()
            {
                UserName = "harishsnape1999@gmail.com",
                Password = "harish462"
            };

            smtpClient.EnableSsl = true;
            smtpClient.Send(mailMessage);
        }
    }
}