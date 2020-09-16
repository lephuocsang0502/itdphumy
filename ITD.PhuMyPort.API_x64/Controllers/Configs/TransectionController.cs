using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ITD.PhuMyPort.DataAccess.Data;
using ITD.PhuMyPort.DataAccess.Models;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using ITD.PhuMyPort.Common;
using ITD.PhuMyPort.ANPR;
using ITD.PhuMyPort.API.Models;
using System.Threading;
using System.Net.NetworkInformation;
using Microsoft.CodeAnalysis.Text;
using ITD.PhuMyPort.API_x64.ViewModels;
using Nancy.Json;
using Microsoft.CodeAnalysis.Differencing;
using Nancy.ViewEngines;
using Microsoft.EntityFrameworkCore.Internal;
using System.Security.Policy;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using CrystalDecisions.CrystalReports.Engine;
using System.Globalization;
using ITD.PhuMyPort.TCP;
using System.Text;
using System.Runtime.Serialization.Json;

namespace ITD.PhuMyPort.API_x64.Controllers.Configs
{
    public class TransectionController : Controller
    {
        private readonly ConfigWebContext _context;

        private static Dictionary<int, PlateResult> responseQueue = new Dictionary<int, PlateResult>();
        /// <summary>
        /// thời gian tối đa xử lý nhận dạng
        /// </summary>

        private static Queue<ImageData> requuestQueue = new Queue<ImageData>();
        public TransectionController(ConfigWebContext context)
        {
            _context = context;
        }
        static string path = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName;
        static string dbname = Path.Combine(path, "DB", "ConfigWebDb.db");
        string connectionString = @"Data Source=" + dbname + ";";
        // GET: CheckPoint
        public ActionResult Save()
        {
            return View();
        }
     
        [HttpPost]
    
        public IActionResult Save(IFormFile file, byte[] fileData, ICollection<IFormFile> photos, byte [] photoData)
        {
            var ms = new MemoryStream();
            file.OpenReadStream().CopyTo(ms);
            fileData = ms.ToArray();        
            NLogHelper.Info("Received Image: byte[" + fileData.Length + "]");// + Convert.ToBase64String(ImageData));
            string filePath = "";      
            try
            {
               
                string fileRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
               
                if (!Directory.Exists(fileRoot))
                {
                    
                        Directory.CreateDirectory(fileRoot);
                }
               
                filePath = Path.Combine(fileRoot, file.FileName);
               
                System.IO.File.WriteAllBytes(filePath, fileData);
               
            }
          
            catch (Exception ex)
            {
                NLogHelper.Error(ex);
            }

            foreach (var photo in photos)
            {
                if (photo.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        photo.CopyTo(memoryStream);
                        photoData = memoryStream.ToArray();
                     
                        string imagePath = "";
                        try
                        {
                            string imageRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                            if (!Directory.Exists(imageRoot))
                            {

                                Directory.CreateDirectory(imageRoot);
                            }
                            imagePath = Path.Combine(imageRoot, photo.FileName);
                            System.IO.File.WriteAllBytes(imagePath, photoData);
                        }
                        catch (Exception ex)
                        {
                            NLogHelper.Error(ex);
                        }
                    }
                }
            }
            var lineNumber = 0;
            using (SqliteConnection conn = new SqliteConnection(connectionString))
            {
                
                conn.Open();
                //Put your file location here:
                using (StreamReader reader = new StreamReader(filePath))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (lineNumber != 0)
                        {
                            var values = line.Split(',');

                            int status = 0;
                            string stringImg = values[2].Substring(44, 22);
                            string img = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images") + stringImg;
                            string dateString = values[0].Substring(0, 4)+ "-"+values[0].Substring(4, 2) + "-" + values[0].Substring(6, 2) + " " + values[0].Substring(8, 2) + ":" + values[0].Substring(10, 2) + ":" + values[0].Substring(12, 2);
                            DateTime a = DateTime.ParseExact(dateString, "yyyy-MM-dd HH:mm:ss", null);
                            var b = a;
                            var c = dateString;

                            var sql = "INSERT INTO Transection VALUES ('" + values[0] + "','" + values[1] + "','" + img + "','" + status + "','" + dateString + "')";

                            var cmd = new SqliteCommand();
                            cmd.CommandText = sql;
                            cmd.CommandType = System.Data.CommandType.Text;
                            cmd.Connection = conn;
                            cmd.ExecuteNonQuery();
                           /* try
                            {
                                StreamWriter streamWriter = new StreamWriter("data_" + DateTime.Now.ToString("yyyyMMdd") + ".csv", true);
                                streamWriter.WriteLine(string.Format("{0},{1},{2},{3},{4}", values[0], values[1], img, status, dateString));
                                streamWriter.Close();
                            }
                            catch { }*/
                        }
                        lineNumber++;
                    }
                }
                conn.Close();
            }
          
            return RedirectToAction("Index");
        }

  

    
        private DateTime FormatDate(string date, string format)
        {
            IFormatProvider culture = new CultureInfo("en-US", true);
            return DateTime.ParseExact(date, format, culture);
        }

        DateTime dStart;
        DateTime dEnd;
        [HttpGet]
        public JsonResult Chart(string start_date, string end_date)
        {
            var query = _context.Transections

             .GroupBy(x => x.Result)
             .Select(g => new { name = g.Key, count = g.Count() }).ToList();

            if (!String.IsNullOrEmpty(start_date))
            {
                dStart = DateTime.ParseExact(start_date, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                query = _context.Transections
                    .Where(x => x.Time >= dStart)
                  .GroupBy(x => x.Result)
                  .Select(g => new { name = g.Key, count = g.Count() }).ToList();
            }
            if (!String.IsNullOrEmpty(start_date))
            {
                dEnd = DateTime.ParseExact(end_date, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                query = _context.Transections
              .Where(x => x.Time >= dEnd)
            .GroupBy(x => x.Result)
            .Select(g => new { name = g.Key, count = g.Count() }).ToList();

            }
            return Json(
                query

            );
        }
        public JsonResult GetAppoitmentByDate(string start_date, string end_date, int page, int pageSize)
        {
            var model = from p in _context.Transections
                        select new { p.Date, p.Images, p.Plate, p.Result, p.Time };



            int totalRow = model.Count();

            model = model
              .Skip((page - 1) * pageSize)
              .Take(pageSize);


            List<Transection> listTran = new List<Transection>();
            foreach (var item in model)
            {
                Transection tran = new Transection();
                tran.Date = item.Date;
                tran.Images = Convert.ToBase64String(System.IO.File.ReadAllBytes(item.Images));
                tran.Result = item.Result;
                tran.Plate = item.Plate;
                tran.Time = item.Time;
                listTran.Add(tran);
            }
          
            return Json(new
            {
                data = listTran,

                total = totalRow,
                status = true
            });
        }

    
        public IActionResult PingHost()
        {
            string nameOrAddress = "10.10.14.199";
            bool pingable = false;
            Ping pinger = null;

            try
            {
                pinger = new Ping();
                PingReply reply = pinger.Send(nameOrAddress);
                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                // Discard PingExceptions and return false;
            }
            finally
            {
                if (pinger != null)
                {
                    pinger.Dispose();
                }
            }

            return View(pingable);
        }

        public IActionResult Index()
        {
            return View();
        }

        // GET: Transection
        [HttpGet]
        public JsonResult LoadData(string name, string status, string cbPlate, string start_date, string end_date, int page, int pageSize)
        {
            var model = from p in _context.Transections
                        select new { p.Date, p.Images, p.Plate, p.Result,p.Time};
            var a = model.ToList();
            if (!string.IsNullOrEmpty(start_date))
            {
                DateTime dStart = DateTime.ParseExact(start_date, "dd/MM/yyyy HH:mm:ss", null);
               
                model=model.Where(x => x.Time >= dStart);
              
            }
            if (!string.IsNullOrEmpty(end_date))
            {
                DateTime dEnd = DateTime.ParseExact(end_date, "dd/MM/yyyy HH:mm:ss", null);
                model = model.Where(x => x.Time < dEnd);
              
            }
          
            if (!string.IsNullOrEmpty(status))
            {
                var statusBool = int.Parse(status);
                model = model.Where(x => x.Result == statusBool);
            }
        
            if (!string.IsNullOrEmpty(name))
            { 
                  model = model.Where(x => x.Plate.Contains(name));
            }
              

            if (!string.IsNullOrEmpty(cbPlate))
            {
                var statusBool = int.Parse(cbPlate);
                if (statusBool == 5)
                {
                    model = model.Where(x => x.Plate.Length < statusBool);
                }
                if (statusBool == 6)
                {
                    model = model.Where(x => x.Plate.Length >= statusBool);
                }

            }
            int totalRow = model.Count();

            model = model
              .Skip((page - 1) * pageSize)
              .Take(pageSize);


            List<Transection> listTran = new List<Transection>();
            foreach (var item in model)
            {
                Transection tran = new Transection();
                tran.Date = item.Date;
                tran.Images = Convert.ToBase64String(System.IO.File.ReadAllBytes(item.Images));
                tran.Result = item.Result;
                tran.Plate = item.Plate;
                tran.Time = item.Time;
                listTran.Add(tran);
            }
        
            return Json(new
            {
                data = listTran,

                total = totalRow,
                status = true
            });


        }


     
        // GET: Transection/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transection = await _context.Transections
                .FirstOrDefaultAsync(m => m.Date == id);
            if (transection == null)
            {
                return NotFound();
            }

            return View(transection);
        }

        // GET: Transection/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Transection/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Plate,Images")] Transection transection, IFormFile photo, byte[] ImageData)
        {
            if (ModelState.IsValid)
            {

                var ms = new MemoryStream();
                photo.OpenReadStream().CopyTo(ms);
                ImageData = ms.ToArray();



                NLogHelper.Info("Received Image: byte[" + ImageData.Length + "]");// + Convert.ToBase64String(ImageData));
                string imagePath = "";


                try
                {
                    string imageRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                    if (!Directory.Exists(imageRoot))
                    {
                        try
                        {
                            Directory.CreateDirectory(imageRoot);
                        }
                        catch (Exception ex)
                        {
                            NLogHelper.Error(ex);
                        }
                    }
                    imagePath = Path.Combine(imageRoot, DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".jpg");
                    System.IO.File.WriteAllBytes(imagePath, ImageData);
                    string a = Convert.ToBase64String(System.IO.File.ReadAllBytes(imagePath));
                    try
                    {
                        StreamWriter streamWriter = new StreamWriter("data_" + DateTime.Now.ToString("yyyyMMdd") + ".txt", true);
                        streamWriter.WriteLine(string.Format("{0},{1},{2}", DateTime.Now.ToString("yyyyMMddHHmmssfff"), transection.Plate, imagePath));
                        streamWriter.Close();
                    }

                    catch { }
                 
                    transection.Images = imagePath;
                    transection.Date = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    transection.Result = 0;
                    transection.Time = DateTime.Now;
                    _context.Add(transection);
                    _context.SaveChanges();
                    return RedirectToAction("Index", "Transection");
                }
                catch (Exception ex)
                {
                    NLogHelper.Error(ex);
                }

            }
            return View(transection);
        }

        // GET: Transection/Edit/5
        public Transection ShowDetail(string id)
        {
            var transection = _context.Transections.SingleOrDefault(x => x.Date == id);


            return transection;

        }
        [HttpPost]
        public JsonResult Edit(string id)
        {
            var result = ShowDetail(id);
            var list = new List<Transection>();

            return Json(new
            {
                status = result,
                a = Convert.ToBase64String(System.IO.File.ReadAllBytes(result.Images))

            });
        }

        [HttpGet]
        public JsonResult LoadToArray()
        {
            var result = _context.Transections.ToList();
            return Json(new
            {
                status = result
            });

        }
        [HttpGet]
        public JsonResult Load()
        {
            List<Transection> model = _context.Transections.ToList();
            return Json(new
            {
                data = model
            });
        }

        [HttpPost]
        public JsonResult Submit(string cartModel)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Transection tran = serializer.Deserialize<Transection>(cartModel);

            //save db
            var entity = _context.Transections.Find(tran.Date);
            entity.Result = tran.Result;
            _context.SaveChanges();
            return Json(new
            {
                status = true
            });
        }
        // POST: Transection/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transection = await _context.Transections
                .FirstOrDefaultAsync(m => m.Date == id);
            if (transection == null)
            {
                return NotFound();
            }

            return View(transection);
        }

        // POST: Transection/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var transection = await _context.Transections.FindAsync(id);
            _context.Transections.Remove(transection);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TransectionExists(string id)
        {
            return _context.Transections.Any(e => e.Date == id);
        }

        [HttpGet]
        public JsonResult Same()
        {
            var same = _context.Transections.Where(x => x.Result == 1).Count();
            var notsame = _context.Transections.Where(x => x.Result == 2).Count();
            var noimg = _context.Transections.Where(x => x.Result == 3).Count();
            var unapprove = _context.Transections.Where(x => x.Result == 0).Count();
            return Json(new
            {
                dataSame = same,
                dataNotSame=notsame,
                dataNoImg=noimg,
                dataUnapprove= unapprove
            });
        }

    }


}
