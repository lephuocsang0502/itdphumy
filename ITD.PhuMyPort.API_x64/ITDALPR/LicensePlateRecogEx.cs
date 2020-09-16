using ITD.PhuMyPort.ANPR;
using ITD.PhuMyPort.Common;
using LPRCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ITD.PhuMyPort.API
{
    public class LicensePlateRecogEx
    {
        public static bool IsInit = false;

        public static bool Init()
        {
            bool bR = true;
            try
            {
                Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
                iAnprConf config = new iAnprConf();
                bR = config.LoadConf(iAnprConf.iAnprConf_enum.iAnprConf_enum_detection);
                bR = config.LoadConf(iAnprConf.iAnprConf_enum.iAnprConf_enum_car);
                bR = config.LoadConf(iAnprConf.iAnprConf_enum.iAnprConf_enum_motor);
                bR = config.LoadConf(iAnprConf.iAnprConf_enum.iAnprConf_enum_classification);
            }
            catch (Exception ex)
            {
                NLogHelper.Error("getplate - " + ex.Message);
            }
            return bR;
        }
        static List<string> _mListSeri = new List<string>();
        static Dictionary<string, string> _mListKeyMap = new Dictionary<string, string>();
        static bool IsVietNameseFormat(string pPlate)
        {
            Regex rg5 = new Regex("^[1-9]{0,1}[0-9]{0,1}[A-Z]{1,2}[0-9]{3,5}$"); //Viet Nam licence plate number
            return rg5.Match(pPlate).Success;
        }
        public static void GetPlate(byte[] imageData, ref PlateResult plateResult)
        {
            if (!IsInit)
            {
                IsInit = Init();
            }
            try
            {
                iAnpr anpr = new iAnpr();
                List<iAnprResult> iAnprResults = anpr.GetAllPlateFromMem(imageData);
                //found plate in main image
                if (iAnprResults != null)
                {
                    foreach (iAnprResult iAnprResult in iAnprResults)
                    {
                        if (IsVietNameseFormat(iAnprResult.GetAnprText()))
                        {
                            plateResult.Plate = iAnprResult.GetAnprText();
                            if (plateResult.Plate.Length > 0)
                            {
                                plateResult.PlateBox = iAnprResult.GetAnprFrame();
                            }
                            break;
                        }
                        else
                        {
                            string plate = iAnprResult.GetAnprText();

                            //remove fisrt and last number
                            Regex rg5 = new Regex("^[0-9]{3}[A-Z]{1,2}[0-9]{3,5}$");
                            if (rg5.Match(plate).Success)
                                plate = plate.Substring(1);
                            Regex rg6 = new Regex("^[0-9]{2}[A-Z]{1,2}[0-9]{6}$");
                            if (rg6.Match(plate).Success)
                                plate = plate.Substring(0, plate.Length - 1);
                            plateResult.Plate = plate;

                            if (plateResult.Plate.Length > 0)
                            {
                                plateResult.PlateBox = iAnprResult.GetAnprFrame();
                            }
                            break;
                        }
                    }
                }
                if (iAnprResults == null || iAnprResults.Count == 0)
                {
                    //recognization for part
                    using (var memoryStream = new MemoryStream(imageData))
                    {
                        Bitmap bitmap = new Bitmap(Image.FromStream(memoryStream));
                        Bitmap imagePart1 = bitmap.Clone(new Rectangle(0, 0, bitmap.Width / 2, bitmap.Height), bitmap.PixelFormat);
                        Bitmap imagePart2 = bitmap.Clone(new Rectangle(bitmap.Width / 2, 0, bitmap.Width / 2, bitmap.Height), bitmap.PixelFormat);

                        using (MemoryStream memoryStream1 = new MemoryStream())
                        {
                            imagePart1.Save(memoryStream1, ImageFormat.Jpeg);
                            byte[] imageDataPart1 = memoryStream1.ToArray();
                            iAnprResults = anpr.GetAllPlateFromMem(imageDataPart1);
                            //found plate in part 1 iamge
                            foreach (iAnprResult iAnprResult in iAnprResults)
                            {
                                if (IsVietNameseFormat(iAnprResult.GetAnprText()))
                                {
                                    plateResult.Plate = iAnprResult.GetAnprText();
                                    if (plateResult.Plate.Length > 0)
                                    {
                                        plateResult.PlateBox = iAnprResult.GetAnprFrame();
                                    }
                                    break;
                                }
                                else
                                {
                                    string plate = iAnprResult.GetAnprText();

                                    //remove fisrt and last number
                                    Regex rg5 = new Regex("^[0-9]{3}[A-Z]{1,2}[0-9]{3,5}$");
                                    if (rg5.Match(plate).Success)
                                        plate = plate.Substring(1);
                                    Regex rg6 = new Regex("^[0-9]{2}[A-Z]{1,2}[0-9]{6}$");
                                    if (rg6.Match(plate).Success)
                                        plate = plate.Substring(0, plate.Length - 1);
                                    plateResult.Plate = plate;

                                    if (plateResult.Plate.Length > 0)
                                    {
                                        plateResult.PlateBox = iAnprResult.GetAnprFrame();
                                    }
                                    break;
                                }
                            }
                        }
                        if (iAnprResults == null || iAnprResults.Count == 0)
                        {
                            using (MemoryStream memoryStream2 = new MemoryStream())
                            {
                                imagePart2.Save(memoryStream2, ImageFormat.Jpeg);
                                byte[] imageDataPart2 = memoryStream2.ToArray();

                                iAnprResults = anpr.GetAllPlateFromMem(imageDataPart2);
                                //found image in part 1 image
                                foreach (iAnprResult iAnprResult in iAnprResults)
                                {
                                    if (IsVietNameseFormat(iAnprResult.GetAnprText()))
                                    {
                                        plateResult.Plate = iAnprResult.GetAnprText();
                                        if (plateResult.Plate.Length > 0)
                                        {
                                            plateResult.PlateBox = iAnprResult.GetAnprFrame();
                                        }
                                        break;
                                    }
                                    else
                                    {
                                        string plate = iAnprResult.GetAnprText();

                                        //remove fisrt and last number
                                        Regex rg5 = new Regex("^[0-9]{3}[A-Z]{1,2}[0-9]{3,5}$");
                                        if (rg5.Match(plate).Success)
                                            plate = plate.Substring(1);
                                        Regex rg6 = new Regex("^[0-9]{2}[A-Z]{1,2}[0-9]{6}$");
                                        if (rg6.Match(plate).Success)
                                            plate = plate.Substring(0, plate.Length - 1);
                                        plateResult.Plate = plate;

                                        if (plateResult.Plate.Length > 0)
                                        {
                                            plateResult.PlateBox = iAnprResult.GetAnprFrame();
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NLogHelper.Error("getplate - " + ex.Message);
            }

        }
    }
}
