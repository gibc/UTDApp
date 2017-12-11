using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;

namespace Metric.Controllers
{
    public class TestController : Controller
    {
        // GET: Test
        //public ActionResult Index()
        //{
        //    return View();
        //}
        [HttpPost]
        public ActionResult Index()
        {
            //return View();
            {
                foreach (string upload in Request.Files)
                {
                    if (Request.Files[upload].FileName != "")
                    {
                        string path = AppDomain.CurrentDomain.BaseDirectory + "App_Data/";
                        string filename = Path.GetFileName(Request.Files[upload].FileName);
                        Request.Files[upload].SaveAs(Path.Combine(path, filename));
                    }
                }
                return View("Upload");
            }
        }

        public ActionResult Downloads()
        {
            var dir = new System.IO.DirectoryInfo(Server.MapPath("~/App_Data"));
            System.IO.FileInfo[] fileNames = dir.GetFiles("*.*"); List<string> items = new List<string>();
            foreach (var file in fileNames)
            {
                items.Add(file.Name);
            }
            return View(items);
        }

        public FileResult Download(string ImageName)
        {
            var FileVirtualPath = "~/App_Data/" + ImageName;
            return File(FileVirtualPath, "application/force-download", Path.GetFileName(FileVirtualPath));
        }
    }
}