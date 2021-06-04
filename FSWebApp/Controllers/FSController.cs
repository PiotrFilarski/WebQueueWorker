using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Drawing;
using FSWebApp.Models;
using System.IO;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using ZXing;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.ServiceBus;
using System.Configuration;

namespace FSWebApp.Controllers
{
    [RoutePrefix("api/Statements")]
    public class FSController : ApiController
    {
        //POST api/Statements/CreateStatement
        [Route("CreateStatement")]
        public async Task<IHttpActionResult> PostAsync(Models.FinancialStatement fsArg)
        {
            //direct serverless function http trigger-----------------------------------------------------------------------------------------------------------
            //using (var client = new HttpClient())
            //{
            //    var json = JsonConvert.SerializeObject(fsArg);
            //    var data = new StringContent(json, Encoding.UTF8, "application/json");
            //    var response = await client.PostAsync(ConfigurationManager.AppSettings["FunctionEndpoint"], data);
            //    var content = response.Content.ReadAsStringAsync().Result;
            //    if (response.StatusCode != HttpStatusCode.OK)
            //        return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, content));
            //}


            //putting request to the message queue-------------------------------------------------------------------------------------------------------------
            ServiceBusClient sbclient = new ServiceBusClient(ConfigurationManager.ConnectionStrings["ServiceBusConnection"].ConnectionString);
            ServiceBusSender sender = sbclient.CreateSender(ConfigurationManager.AppSettings["QueueName"]);
            string messageBody = JsonConvert.SerializeObject(fsArg);
            ServiceBusMessage message = new ServiceBusMessage(messageBody);
            await sender.SendMessageAsync(message);
            await sbclient.DisposeAsync();

            return Ok();    
        }

        public FinancialStatement ProcessFinancialStatement(string firstname, string lastname, int income, int age, byte[] photo)
        {
            FinancialStatement fs = new FinancialStatement();
            fs.Firstname = firstname;
            fs.Lastname = lastname;
            fs.Income = income;
            fs.Age = age;

            try
            {
                Image photoImage;
                using (var ms = new MemoryStream(photo))
                {
                    photoImage = Image.FromStream(ms);
                }

                Bitmap image = ResizeImage(photoImage, 100, 100);
                image = GrayScaleFilter(image);
                ImageConverter converter = new ImageConverter();
                fs.Photo = (byte[])converter.ConvertTo(image, typeof(byte[]));
            }
            catch
            {
                return null;
            }

            try
            {
                Bitmap qrcode = GenerateMyQCCode(fs.Firstname + "; " + fs.Lastname + "; " + fs.Income);
                ImageConverter converter = new ImageConverter();
                fs.Code = (byte[])converter.ConvertTo(qrcode, typeof(byte[]));
            }
            catch
            {
                return null;
            }

            for (int i = 0; i < 10; i++)
            {
                long fsFactor = FindPrimeNumber(100000);
            }

            FsDBContext db = new FsDBContext();
            db.FinancialStatements.Add(fs);
            int result = db.SaveChanges();

            return fs;
        }
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
        public Bitmap GrayScaleFilter(Bitmap image)
        {
            Bitmap grayScale = new Bitmap(image.Width, image.Height);

            for (Int32 y = 0; y < grayScale.Height; y++)
                for (Int32 x = 0; x < grayScale.Width; x++)
                {
                    Color c = image.GetPixel(x, y);

                    Int32 gs = (Int32)(c.R * 0.3 + c.G * 0.59 + c.B * 0.11);

                    grayScale.SetPixel(x, y, Color.FromArgb(gs, gs, gs));
                }
            return grayScale;
        }
        private Bitmap GenerateMyQCCode(string QCText)
        {
            var QCwriter = new BarcodeWriter();
            QCwriter.Format = BarcodeFormat.QR_CODE;
            var result = QCwriter.Write(QCText);
            var barcodeBitmap = new Bitmap(result);
            return barcodeBitmap;
        }
        public long FindPrimeNumber(int n)
        {
            int count = 0;
            long a = 2;
            while (count < n)
            {
                long b = 2;
                int prime = 1;// to check if found a prime
                while (b * b <= a)
                {
                    if (a % b == 0)
                    {
                        prime = 0;
                        break;
                    }
                    b++;
                }
                if (prime > 0)
                {
                    count++;
                }
                a++;
            }
            return (--a);
        }
    }
}
