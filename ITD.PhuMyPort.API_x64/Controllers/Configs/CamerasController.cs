using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ITD.PhuMyPort.API.Helpers;
using ITD.PhuMyPort.API.ViewModels;
using ITD.PhuMyPort.Common;
using ITD.PhuMyPort.DataAccess.Data;
using ITD.PhuMyPort.DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ITD.PhuMyPort.API.Controllers
{
    public class CamerasController : Controller
    {
        private readonly ConfigWebContext _context;
        private readonly string encryptedKey = "p@SSword";
        private readonly IWebHostEnvironment _env;
        public CamerasController(ConfigWebContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Cameras
     /*   [Authorize]*/
        public async Task<IActionResult> Index()
        {
            ViewBag.totalPlate = TotalPlate();
            ViewBag.same = Same();
            ViewBag.notsame = Notsame();
            ViewBag.noimg = Noimg();
            ViewBag.unapproved = Unapproved();


            var configWebContext = _context.Cameras.Include(c => c.Workplace);
            var list = await configWebContext.ToListAsync();
            NLogHelper.Info("Cameras - Show index: Found " + list.Count + " cameras.\n");
            return View(list);
        }

        // GET: Cameras/Create
        [NoDirectAccess]
        public async Task<IActionResult> Create()
        {
            CameraViewModel cameraViewModel = new CameraViewModel();
            cameraViewModel.Camera = new Camera();
            cameraViewModel.WorkplaceList = new SelectList(_context.Workplaces, "Code", "Code");
            NLogHelper.Info("Cameras - Create: Show Create popup");
            return View(cameraViewModel);
        }

        // POST: Cameras/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Code,Name,IP,Port,Username,Password,WorkplaceCode")] Camera camera)
        {
            NLogHelper.Info("Camera - Create: Try to create new camera");
            if (ModelState.IsValid)
            {
                var passwordEncryptred = Cipher.Encrypt(camera.Password, encryptedKey);
                camera.Password = passwordEncryptred;
                _context.Add(camera);
                await _context.SaveChangesAsync();
                NLogHelper.Info("Camera - Create: Valid infomation. Create new camera to database.\n");

                var configWebContext = _context.Cameras.Include(c => c.Workplace);
                return Json(new { isValid = true, html = Helper.RenderRazorViewToString(this, "_ViewAll", await configWebContext.ToListAsync()) });
            }

            NLogHelper.Info("Camera - Create: InValid infomation.");
            CameraViewModel cameraViewModel = new CameraViewModel();
            cameraViewModel.Camera = camera;
            cameraViewModel.WorkplaceList = new SelectList(_context.Workplaces, "Code", "Code");
            return Json(new { isValid = false, html = Helper.RenderRazorViewToString(this, "Create", cameraViewModel) });
        }


        // GET: Cameras/Edit/5
        [NoDirectAccess]
        public async Task<IActionResult> Edit(string id)
        {
            NLogHelper.Info("Cameras - Edit: Find camera base on id");
            if (id == null)
            {
                NLogHelper.Info("Cameras - Edit: Can not find camera with id: " + id);
                return NotFound();
            }

            var camera = await _context.Cameras.FindAsync(id);
            if (camera == null)
            {
                NLogHelper.Info("Cameras - Edit: Can not find camera with id: " + id);
                return NotFound();
            }

            NLogHelper.Info("Cameras - Edit: Found camera with id: " + id);
            var passwordDecrypted = Cipher.Decrypt(camera.Password, encryptedKey);
            camera.Password = passwordDecrypted;

            CameraViewModel cameraViewModel = new CameraViewModel();
            cameraViewModel.Camera = camera;
            cameraViewModel.WorkplaceList = new SelectList(_context.Workplaces, "Code", "Code", camera.WorkplaceCode);
            NLogHelper.Info("Cameras - Edit: Show Edit popup");
            return View(cameraViewModel);
        }

        // POST: Cameras/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Code,Name,IP,Port,Username,Password,WorkplaceCode")] Camera camera)
        {
            NLogHelper.Info("Camera - Edit: Try to edit camera");
            if (id != camera.Code)
            {
                return NotFound();
            }
            
            if (ModelState.IsValid)
            {
                try
                {
                    var passwordEncryptred = Cipher.Encrypt(camera.Password, encryptedKey);
                    camera.Password = passwordEncryptred;
                    _context.Update(camera);
                    await _context.SaveChangesAsync();
                    NLogHelper.Info("Camera - Edit: Valid infomation. Update camera information to database.\n");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CameraExists(camera.Code))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                var configWebContext = _context.Cameras.Include(c => c.Workplace);
                return Json(new { isValid = true, html = Helper.RenderRazorViewToString(this, "_ViewAll", await configWebContext.ToListAsync()) });
            }
            NLogHelper.Info("Camera - Edit: InValid infomation.");
            CameraViewModel cameraViewModel = new CameraViewModel();
            cameraViewModel.Camera = camera;
            cameraViewModel.WorkplaceList = new SelectList(_context.Workplaces, "Code", "Code", camera.WorkplaceCode);
            return Json(new { isValid = false, html = Helper.RenderRazorViewToString(this, "Edit", cameraViewModel) });
        }


        // POST: Cameras/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var camera = await _context.Cameras.FindAsync(id);
            _context.Cameras.Remove(camera);
            await _context.SaveChangesAsync();
            NLogHelper.Info("Camera - Delete: Removed camera with id: " + id + "\n");
            var configWebContext = _context.Cameras.Include(c => c.Workplace);
            return Json(new { html = Helper.RenderRazorViewToString(this, "_ViewAll", await configWebContext.ToListAsync()) });
        }

        private bool CameraExists(string id)
        {
            return _context.Cameras.Any(e => e.Code == id);
        }


        //Get SnapShot of camera and return to view
        public async Task<IActionResult> SnapShot(string id)
        {
            NLogHelper.Info("Camera - SnapShot: Try to get snapshot of camera with id: " + id);
            if (id == null)
            {
                NLogHelper.Info("Camera - SnapShot: Can not find camera with id: " + id);
                return NotFound();
            }

            var camera = await _context.Cameras.FindAsync(id);
            if (camera == null)
            {
                NLogHelper.Info("Camera - SnapShot: Can not find camera with id: " + id);
                return NotFound();
            }
            //Decrypt password
            var passwordDecrypted = Cipher.Decrypt(camera.Password, encryptedKey);
            camera.Password = passwordDecrypted;

            byte[] ImageData;
            ImageData = Capture(camera);
            if (!SafeEquals(ImageData, new byte[500000]))
            {
                //save image to temp folder
                try
                {
                    string folderPath = Path.Combine(_env.WebRootPath, "SnapshotImages");
                    if (!Directory.Exists(folderPath))
                    {
                        try
                        {
                            Directory.CreateDirectory(folderPath);
                        }
                        catch (Exception ex)
                        {
                            NLogHelper.Error(ex);
                        }
                    }

                    //delete old temp images files
                    string[] files = Directory.GetFiles(folderPath);
                    foreach (var item in files)
                    {
                        System.IO.File.Delete(item);
                    }

                    var fileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".jpg";
                    string imagePath = Path.Combine(folderPath, fileName);
                    System.IO.File.WriteAllBytes(imagePath, ImageData);
                    ViewBag.ImageName = fileName;
                    NLogHelper.Info("Camera - SnapShot: Get snapshot of camera with id: " + id + " success.\n");
                }
                catch (Exception ex)
                {
                    NLogHelper.Error(ex);
                }
            }
            else
            {
                NLogHelper.Info("Camera - SnapShot: Can not get snapshot of camera with id: " + id + "\n");
                ViewBag.ImageName = null;
            }
            return View();
        }


        /// <summary>
        /// Get byte[] of image from camera info
        /// </summary>
        /// <param name="camera"></param>
        /// <returns></returns>
        private byte[] Capture(Camera camera)
        {
            byte[] imageData = new byte[500000];
            HttpWebRequest ImageWebRequest;
            HttpWebResponse ImageWebResponse = null;
            Stream responseStream = null;
            // IsCaptureSuccess = false;
            try
            {
                //LastestImage = null;//reset
                ImageWebRequest = (HttpWebRequest)WebRequest.Create(camera.IP);

                ImageWebRequest.AllowWriteStreamBuffering = true;
                ImageWebRequest.Timeout = 500;//harcode temporary
                ImageWebRequest.UseDefaultCredentials = true;
                ImageWebRequest.Credentials = new NetworkCredential(camera.Username, camera.Password);

                ImageWebResponse = (HttpWebResponse)ImageWebRequest.GetResponse();
                responseStream = ImageWebResponse.GetResponseStream();

                //save image to byte array
                using (var memoryStream = new MemoryStream())
                {
                    responseStream.CopyTo(memoryStream);
                    imageData = memoryStream.ToArray();
                }

                //save image to image
                //Image LastestImage = Image.FromStream(responseStream);

                //System.IO.File.WriteAllBytes("test.jpg", imageData);
                ImageWebResponse.Close();
            }
            catch (Exception ex)
            {
                NLogHelper.Error(this.GetType() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
            finally
            {
                ImageWebRequest = null;
                ImageWebResponse = null;
            }
            return imageData;
        }

        /// <summary>
        /// Compare 2 byte[]
        /// </summary>
        /// <param name="strA"></param>
        /// <param name="strB"></param>
        /// <returns></returns>
        private static bool SafeEquals(byte[] strA, byte[] strB)
        {
            int length = strA.Length;
            if (length != strB.Length)
            {
                return false;
            }
            for (int i = 0; i < length; i++)
            {
                if (strA[i] != strB[i]) return false;
            }
            return true;
        }


        public int TotalPlate()
        {
            var result = from a in _context.Transections
                         select a.Plate;
            var b = result.Distinct().Count();

            return b;

        }

        public int Same()
        {
            int same = 0;
            foreach (var pro in _context.Transections)
            {
                Transection tempt = new Transection();

                var a = tempt.Result = pro.Result;
                if (a == 1)
                    same++;
            }
            return same;
        }

        public int Notsame()
        {
            int notsame = 0;
            foreach (var pro in _context.Transections)
            {
                Transection tempt = new Transection();

                var a = tempt.Result = pro.Result;

                if (a == 2)
                    notsame++;
            }
            return notsame;
        }

        public int Noimg()
        {
            int noimg = 0;
            foreach (var pro in _context.Transections)
            {
                Transection tempt = new Transection();

                var a = tempt.Result = pro.Result;
                if (a == 3)
                    noimg++;
            }
            return noimg;
        }


        public int Unapproved()
        {
            int unApproved = 0;

            foreach (var pro in _context.Transections)
            {
                Transection tempt = new Transection();

                var a = tempt.Result = pro.Result;
                if (a == 0)
                    unApproved++;
            }
            return unApproved;
        }


    }
}