using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Uplift.DataAccess.Data;
using Uplift.DataAccess.Data.Repository.IRepository;
using Uplift.Models;
using Uplift.Utility;

namespace Uplift.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class WebImageController : Controller
    {
        private readonly ApplicationDbContext _db;

        public WebImageController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upsert(int? id)
        {
            WebImages imgObj = new WebImages();
            if (id ==null)
            {

            }
            else
            {
                imgObj = _db.WebImages.SingleOrDefault(m => m.Id == id);
                if (imgObj == null)
                {
                    return NotFound();
                }
            }

            return View(imgObj);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(int id,WebImages imgObject)
        {
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                if (files.Count > 0)
                {
                    byte[] pic = null;
                    using (var fs = files[0].OpenReadStream())
                    {
                        using (var ms = new MemoryStream())
                        {
                            fs.CopyTo(ms);
                            pic = ms.ToArray();
                        }
                    }
                    imgObject.Picture = pic;

                }

                if (imgObject.Id == 0)
                {
                    _db.WebImages.Add(imgObject);
                }
                else
                {
                    var imgFromDB = _db.WebImages.Where(w => w.Id == id).FirstOrDefault();
                    imgFromDB.Name = imgObject.Name;
                    if (files.Count > 0)
                    {
                        imgFromDB.Picture = imgObject.Picture;
                    }
                }
                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(imgObject);
        }


        #region API Calls
        [HttpGet]
        public IActionResult GetAll()
        {
            return Json(new { data = _db.WebImages.ToList()});
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var objFromDb = _db.WebImages.Find(id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error while deleting." });
            }
            _db.WebImages.Remove(objFromDb);
            _db.SaveChanges();
            return Json(new { success = true, message = "Delete successful." });
        }

        #endregion
    }
}
