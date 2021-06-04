using FSWebApp.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZXing;


namespace FSWebApp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult FSForm(Models.FinancialStatementFile fsFile)
        {
            byte[] img;
            ImageConverter converter = new ImageConverter();
            img = (byte[])converter.ConvertTo(Image.FromStream(fsFile.Photo.InputStream, true, true), typeof(byte[]));

            FinancialStatement fs = new FSController().ProcessFinancialStatement(fsFile.Name, fsFile.Surname, fsFile.Income, fsFile.Age, img);
            if(fs != null)
            {
                ViewBag.Name = fs.Firstname;
                ViewBag.Surname = fs.Lastname;
                ViewBag.Income = fs.Income;
                ViewBag.Age = fs.Age;
                ViewBag.Image = "data:image/png;base64," + Convert.ToBase64String(fs.Photo, 0, fs.Photo.Length);
                ViewBag.Code = "data:image/png;base64," + Convert.ToBase64String(fs.Code, 0, fs.Code.Length);
            }
            return View("Index");
        }

        
    }
}