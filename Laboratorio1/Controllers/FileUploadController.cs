using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace Laboratorio1.Controllers
{
    public class FileUploadController : Controller
    {
       public static  string Message;
        // GET: FileUpload
        public ActionResult Index()
        {
            var items = FilesUploaded();
            ViewBag.Message = Message;
            return View(items);

        }


        [HttpPost]         
        public ActionResult Index(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
                    try
                    {
                        if (Path.GetExtension(file.FileName) == ".txt" || Path.GetExtension(file.FileName) == ".huff" || Path.GetExtension(file.FileName) == ".lzw")
                        {
                            string path = Path.Combine(Server.MapPath("~/Archivo"),
                                          Path.GetFileName(file.FileName));
                            file.SaveAs(path); 
                            ViewBag.Message = "File uploaded";
                        }
                    else
                    {
                        ViewBag.Message = "Invalid file, please upload a .txt";
                    }
                }
                    catch
                    {
                        ViewBag.Message = "ERROR. Try uploading other file";
                    }
                else
                {
                    ViewBag.Message = "Please upload a file";
                }
                var items = FilesUploaded();
                return View(items);
        }

        private List<string> FilesUploaded()
        {
            if (!System.IO.Directory.Exists(Server.MapPath("~/Archivo")))
            {
                System.IO.Directory.CreateDirectory(Server.MapPath("~/Archivo"));
            }
            var dir = new System.IO.DirectoryInfo(Server.MapPath("~/Archivo"));
            System.IO.FileInfo[] fileNames = dir.GetFiles("*.*");
            List<string> filesupld = new List<string>();
            foreach (var file in fileNames)
            {
                filesupld.Add(file.Name);
            }
            //Devuelvo la lista
            return filesupld;
        }

        public ActionResult Read(string TxtName)
        {
            if (Path.GetExtension(TxtName) == ".txt")
            {
                return RedirectToAction("Read", "ReadText", new { filename = TxtName });
            }
            else
            {
                Message = "No es un archivo comprimible";
            return RedirectToAction("Index", "FileUpload");
            }
            
        }

       
    }
}