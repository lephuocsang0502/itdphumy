using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ITD.PhuMyPort.API.Models;
using System.Net;
using System.Drawing;
using ITD.PhuMyPort.DataAccess.Models;
using ITD.PhuMyPort.DataAccess.Data;
using ITD.PhuMyPort.Common;
using ITD.PhyMyPort.API.ITDALPR;
using ITD.PhuMyPort.ANPR;
using System.Drawing.Imaging;

namespace ITD.PhuMyPort.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ITDANPRServiceController : ControllerBase
    {

        private readonly string encryptedKey = "p@SSword";
        private readonly ConfigWebContext _context;
        /// <summary>
        /// định danh request tăng dần
        /// </summary>
        private static int _squenceId = 0;
        /// <summary>
        /// kiểm tra thread đã bắt đầu hay chưa
        /// </summary>
        private static bool threadStarted = false;
        /// <summary>
        /// hàng đợi lệnh request nhận dạng
        /// </summary>
        private static Queue<ImageData> requuestQueue = new Queue<ImageData>();
        /// <summary>
        /// danh sách kết quả nhận dạng
        /// </summary>
        private static Dictionary<int, PlateResult> responseQueue = new Dictionary<int, PlateResult>();
        /// <summary>
        /// thời gian tối đa xử lý nhận dạng
        /// </summary>
        private static int recogTimeout = 500;
        /// <summary>
        /// 0: không lưu image temp, 1: lưu image
        /// </summary>
        private static int saveImageFlag = 0;
        /// <summary>
        /// testing flag
        /// </summary>
        private static int isTest = 0;
        /// <summary>
        /// thread xử lý nhận dạng
        /// </summary>
        private static Thread ThreadProcess = new Thread(RequestProcess) { IsBackground = true };
        public ITDANPRServiceController(IConfiguration configuration, ConfigWebContext context)
        {

            _context = context;
            if (threadStarted == false)
            {
                ThreadProcess.Start();
            }
            //get timeout from config
            var anprTimeout = configuration.GetSection("ANPRTimeout");
            if (anprTimeout != null && anprTimeout.Value != null)
            {
                int.TryParse(anprTimeout.Value, out recogTimeout);
            }

            //get save image flag
            var saveImage = configuration.GetSection("SaveImages");
            if (saveImage != null && saveImage.Value != null)
            {
                int.TryParse(saveImage.Value, out saveImageFlag);
            }

            //get istest flag
            var authen = configuration.GetSection("IsTest");
            if (authen != null && authen.Value != null)
            {
                int.TryParse(authen.Value, out isTest);
            }
        }
        private async Task<PlateResult> LicensePlateRecognize(byte[] ImageData)
        {
            PlateResult rs = new PlateResult();
            try
            {
                if (ImageData == null)
                    return rs;
                NLogHelper.Info("Received Image: byte[" + ImageData.Length + "]");// + Convert.ToBase64String(ImageData));
                string imagePath = "";

                //0. save image to temp folder
                try
                {
                    if (saveImageFlag == 1)
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
                    }
                }
                catch (Exception ex)
                {
                    NLogHelper.Error(ex);
                }


                //1. add image to queue
                ImageData imageData = new ImageData()
                {
                    SequenceID = _squenceId < int.MaxValue ? _squenceId++ : 0,
                    Data = ImageData
                };
                requuestQueue.Enqueue(imageData);

                //2. wait until done or timeout
                int sequenceId = imageData.SequenceID;
                int timeout = recogTimeout;//ms
                while (timeout > 0)//responseQueue.ContainsKey(sequenceId)
                {
                    //try to get plate from result list
                    //PlateResult plateResult;
                    if (responseQueue.TryGetValue(sequenceId, out rs))
                    {
                        //found
                        responseQueue.Remove(sequenceId);
                        break;
                    }
                    timeout -= 5;
                    Thread.Sleep(5);
                }
                if (timeout <= 0)
                {
                    NLogHelper.Info("Recognition timeout, SequenceId: " + sequenceId);
                }
                NLogHelper.Info("Recognition - SequenceId: " + sequenceId + ", Plate: " + rs.Plate);

                //write to csv
                try
                {
                    StreamWriter streamWriter = new StreamWriter("data_" + DateTime.Now.ToString("yyyyMMdd") + ".csv", true);
                    streamWriter.WriteLine(string.Format("{0},{1},{2}", DateTime.Now.ToString("yyyyMMddHHmmssfff"), rs.Plate, imagePath));
                    streamWriter.Close();
                }
                catch { }

                return rs;
            }
            catch (Exception ex)
            {
                NLogHelper.Error(ex);
                NLogHelper.Info(ex.Message);
            }
            return rs;
        }

        [HttpGet("GetLicensePlate/{WorkplaceCode}", Name = "GetLicensePlate")]
        public Response GetLicensePlate(string WorkplaceCode)
        {
            Response response = new Response()
            {
                ErrorCode = 0
            };
            try
            {
                NLogHelper.Info("GetLicensePlate - WorkplaceCode: " + WorkplaceCode);
                //1. get camera ip by workplacecode
                Camera camera = _context.Cameras.Where(c => c.WorkplaceCode == WorkplaceCode).Take(1).FirstOrDefault();
                if (camera != null)
                {
                    //2. decrypt password
                    camera.Password = Cipher.Decrypt(camera.Password, encryptedKey);
                    //3. get image from camera
                    byte[] imageData = Capture(camera);

                    if (isTest == 1)
                    {
                        try
                        {
                            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test.jpg");
                            Bitmap image = new Bitmap(path);
                            using (MemoryStream memoryStream = new MemoryStream())
                            {
                                image.Save(memoryStream, ImageFormat.Jpeg);
                                imageData = memoryStream.ToArray();
                            }
                        }
                        catch { }
                    }

                    if (imageData.Length > 0)
                    {
                        response.ResponseData = new ResponseData() { Image = Convert.ToBase64String(imageData) };
                        //4. get plate from image
                        PlateResult plateResult = LicensePlateRecognize(imageData).Result;

                        if (plateResult != null && plateResult.Plate != "")
                        {
                            response.ResponseData.Plate = plateResult.Plate;

                            //image to memorystream
                            try
                            {
                                //save image to byte array
                                using (var memoryStream = new MemoryStream(imageData))
                                {
                                    Bitmap bitmap = new Bitmap(Image.FromStream(memoryStream));
                                    Bitmap bmp1 = new Bitmap(bitmap.Clone(plateResult.PlateBox, bitmap.PixelFormat));
                                    byte[] plateImageArray = ImageHelper.ImageToArray(bmp1, ImageFormat.Jpeg);
                                    response.ResponseData.PlateImage = Convert.ToBase64String(plateImageArray);
                                }

                            }
                            catch (Exception ex)
                            {
                                NLogHelper.Error(ex);
                            }

                        }
                    }
                    else
                    {
                        NLogHelper.Info("GetLicensePlate - Cannot get image from camera, Workplace: " + WorkplaceCode);
                    }
                }
                else
                {
                    NLogHelper.Info("GetLicensePlate - Cannot get camera info, Workplace: " + WorkplaceCode);
                }
            }
            catch (Exception ex)
            {
                NLogHelper.Error(ex);
            }

            return response;
        }
        public string Test()
        {
            return "Welcome to ITDANPR Service";
        }
        private static void RequestProcess()
        {
            threadStarted = true;
            while (true)
            {
                if (requuestQueue.Count > 0)
                {
                    ImageData imageData = requuestQueue.Peek();
                    if (imageData != null)
                    {
                        try
                        {
                            NLogHelper.Info("Processing SequenceID: " + imageData.SequenceID);
                            PlateResult plateResult = new PlateResult()
                            {
                                SequenceID = imageData.SequenceID,
                                //Plate = plate
                            };
                            PlateResult plateResult2 = new PlateResult()
                            {
                                SequenceID = imageData.SequenceID,
                                //Plate = plate
                            };
                            //current ANPR
                            try
                            {

                                NLogHelper.Info("Processing Current SequenceID: " + imageData.SequenceID);
                                LicensePlateRecog.getPlate(imageData.Data, ref plateResult2);
                                NLogHelper.Info("Processing Current SequenceID: " + plateResult2.Plate);
                            }
                            catch (Exception ex)
                            {
                                NLogHelper.Error(ex);
                            }
                            //new ANPR
                            try
                            {
                                NLogHelper.Info("Processing new SequenceID: " + imageData.SequenceID);
                                LicensePlateRecogEx.GetPlate(imageData.Data, ref plateResult);
                                NLogHelper.Info("Processing new SequenceID: " + plateResult.Plate);
                            }
                            catch (Exception ex)
                            {
                                NLogHelper.Error(ex);
                            }
                            if (plateResult != null && plateResult2 != null)
                            {
                                if (plateResult.Plate != null)
                                {
                                    if (plateResult2.Plate == null || plateResult2.Plate.Length == 0)
                                    {
                                        responseQueue.Add(imageData.SequenceID, plateResult);
                                    }
                                    else if (plateResult2.Plate.Length > plateResult.Plate.Length && plateResult.Plate.Length<8)
                                    {
                                        responseQueue.Add(imageData.SequenceID, plateResult2);
                                    }
                                    else
                                    {
                                        responseQueue.Add(imageData.SequenceID, plateResult);
                                    }
                                }
                                else if (plateResult2 != null)
                                {
                                    responseQueue.Add(imageData.SequenceID, plateResult2);
                                }
                            }
                            NLogHelper.Info("Processing SequenceID: " + imageData.SequenceID + ", Result: " + plateResult.Plate);
                            NLogHelper.Info("----------------------------------------------------------------------------------");
                        }
                        catch (Exception ex)
                        {
                            NLogHelper.Error(ex);
                        }
                        requuestQueue.Dequeue();
                    }

                }
                else
                {
                    Thread.Sleep(5);
                }
            }
        }
        /// <summary>
        /// snapshot image form camera
        /// </summary>
        /// <param name="camera"></param>
        /// <returns></returns>
        private byte[] Capture(Camera camera)
        {
            //image data in bytes
            byte[] imageData = new byte[0];

            //web request
            HttpWebRequest ImageWebRequest;
            HttpWebResponse ImageWebResponse = null;
            Stream responseStream = null;

            //camera info
            string url = camera.IP;
            string password = camera.Password;
            string usename = camera.Username;
            try
            {
                //LastestImage = null;//reset
                ImageWebRequest = (HttpWebRequest)WebRequest.Create(url);// "http://10.0.8.100/cgi-bin/video.cgi?msubmenu=jpg");

                ImageWebRequest.AllowWriteStreamBuffering = true;
                ImageWebRequest.Timeout = 5000;//harcode temporary

                if (isTest == 0)
                {
                    ImageWebRequest.UseDefaultCredentials = true;
                    ImageWebRequest.Credentials = new NetworkCredential(usename, password);// "admin", "01S@ngtao");
                }

                ImageWebResponse = (HttpWebResponse)ImageWebRequest.GetResponse();
                responseStream = ImageWebResponse.GetResponseStream();

                //save image to byte array
                using (var memoryStream = new MemoryStream())
                {
                    responseStream.CopyTo(memoryStream);
                    imageData = memoryStream.ToArray();
                }
                ImageWebResponse.Close();
            }
            catch (Exception ex)
            {
                NLogHelper.Info(this.GetType() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + ex.Message);
                NLogHelper.Error(this.GetType() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
            finally
            {
                ImageWebRequest = null;
                ImageWebResponse = null;
            }
            return imageData;
        }
    }
}