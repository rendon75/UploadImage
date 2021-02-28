using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models;
using UploadImage.Models;

namespace UploadImage.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _db;

        public HomeController(ILogger<HomeController> logger, AppDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult UploadImage()
        {
            foreach(var file in Request.Form.Files)
            {
                Image img = new Image();
                img.ImageTitle = file.FileName;

                MemoryStream ms = new MemoryStream();
                file.CopyTo(ms);
                img.ImageData = ms.ToArray();

                ms.Close();
                ms.Dispose();

                _db.Images.Add(img);
                _db.SaveChanges();                    
            }

            ViewBag.Message = "Image(s) stored in database!";
            return View("Index");
        }
         
        [HttpPost]
        public ActionResult RetrieveImage()
        {
            Image img = _db.Images.OrderByDescending(i=>i.Id).FirstOrDefault();
            string imageBase64Data = Convert.ToBase64String(img.ImageData);
            string imageDataURL = string.Format("data:image/jpg;base64,{0}", imageBase64Data);

            ViewBag.ImageTitle = img.ImageTitle;
            ViewBag.ImageDataUrl = imageDataURL;
            return View("Index");
        }         

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public ActionResult List()
        {
            var imageListViewModel = new ImageListViewModel
            {
                ImageCount = _db.Images.Count()
            };

            var imageList = new List<ImageViewModel>();

            foreach (var item in _db.Images)
            {
                string imageBase64Data = Convert.ToBase64String(item.ImageData);
                string imageDataURL = string.Format("data:image/jpg;base64,{0}", imageBase64Data);                
                var image = new ImageViewModel{
                    ImageTitle = item.ImageTitle,
                    ImageData = imageDataURL
                };
                imageList.Add(image);
            }
            imageListViewModel.Images = imageList;

            return View("List", imageListViewModel);
        }
    }
}
