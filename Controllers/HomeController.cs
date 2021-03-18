﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using House_Rental_System.Models;

namespace House_Rental_System.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult SellerLogin()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SellerLogin(string user,string pass)
        {
            House_Rental Db = new House_Rental();
            var result = Db.Seller_Details.Where(m => m.Seller_Email == user && m.Seller_Password == pass).FirstOrDefault();
            if (result != null)
            {
                Session["id"] = result.Seller_ID;
               
                return RedirectToAction("Index", "Seller");
            }
            else
            {
                return RedirectToAction("SellerLogin", "Home");
            }
        }
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(string user,string pass)
        {
            House_Rental Db = new House_Rental();
            var result = Db.Customer_Details.Where(m => m.Customer_Email == user && m.Customer_Password == pass).FirstOrDefault();
            if (result != null)
            {
                Session["id"] = result.Customer_Id;
                
                return RedirectToAction("Index", "Customer");
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
        }
        public ActionResult SellerRegistration()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SellerRegistration(Seller_Details sd,HttpPostedFileBase image)
        {
            House_Rental Db = new House_Rental();
            if (image != null)
            {
                sd.Seller_Photo = new byte[image.ContentLength];
                image.InputStream.Read(sd.Seller_Photo, 0, image.ContentLength);
            }
            if (ModelState.IsValid)
            {
                
                Db.Seller_Details.Add(sd);
                Db.SaveChanges();
                
            }
            return RedirectToAction("SellerLogin", "Home");
        }
        public ActionResult Registration()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Registration(Customer_Details cd, HttpPostedFileBase image)
        {
            House_Rental Db = new House_Rental();
            if (image != null)
            {
                cd.Customer_Profile = new byte[image.ContentLength];
                image.InputStream.Read(cd.Customer_Profile, 0, image.ContentLength);
            }
            if (ModelState.IsValid)
            {

                Db.Customer_Details.Add(cd);
                Db.SaveChanges();

            }
            //SendMail();
            return RedirectToAction("Login", "Home");
        }
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        public void SendMail(string emailid)
        {
            MailMessage mailMessage = new MailMessage("harishkumarskv@gmail.com", emailid);
            mailMessage.Subject = "Test";
            mailMessage.Body = "";
            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.Credentials = new System.Net.NetworkCredential()
            {
                UserName = "harishkumarskv@gmail.com",
                Password = "harish462"
            };
            
            smtpClient.EnableSsl= true;
            smtpClient.Send(mailMessage);
        }
    }
}