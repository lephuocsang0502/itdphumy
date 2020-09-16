using cm;
using gx;
using ITD.PhuMyPort.API.ITDALPR;
using ITD.PhuMyPort.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace ITD.PhuMyPort.ANPR
{
    public class PlateResult
    {
        /// <summary>
        /// thứ tự kết quả
        /// </summary>
        public int SequenceID { get; set; }

        /// <summary>
        /// biển số nhận được
        /// </summary>
        public string Plate { get; set; }
        /// <summary>
        /// vi tri bien so
        /// </summary>
        public Rectangle PlateBox { get; set; }
    }
    public class LicensePlateRecog
    {
        static cmAnpr anpr = new cmAnpr("default");
        static gxImage image = new gxImage("default");

        public static string getPlate(byte[] imageData, ref PlateResult plateResult)
        {
            try
            {
                string plate = "";
                bool bR = false;
                gxPG4 gxPG4Frame = new gxPG4();
                gxVariant gxVariant = new gxVariant();
                try
                {
                    bR = image.LoadFromMem(imageData, (int)GX_PIXELFORMATS.GX_GRAY);
                    bR = anpr.FindFirst(image);
                    plate = anpr.GetText();
                    gxPG4Frame = anpr.GetFrame();
                    if ((IsPass(plate)))
                    {
                        if (bR)
                        {
                            int bestConfidence = anpr.GetConfidence();
                            int timeout = 500;
                            Stopwatch stopwatch = new Stopwatch();
                            while (anpr.FindNext() && timeout > 0)
                            {
                                stopwatch.Start();
                                var confidence = anpr.GetConfidence();
                                bestConfidence = bestConfidence < confidence ? confidence : bestConfidence;
                                plate = bestConfidence < confidence ? anpr.GetText() : plate;
                                stopwatch.Stop();
                                timeout -= (int)stopwatch.ElapsedMilliseconds;
                                stopwatch.Reset();
                                plate = anpr.GetText();
                                gxPG4Frame = anpr.GetFrame();
                                if ((!IsPass(plate)))
                                    break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    anpr = new cmAnpr("default");
                    image = new gxImage("default");
                    //NNLogHelper.Error("getPlate: " + ex);
                    bR = false;
                }
                if (plate != "" && (!IsPass(plate)) && IsVietNameseFormat(plate))
                {
                    plate = plate.ToUpper();
                    if (plate.Contains("OC"))
                        plate = plate.Replace("OC", "0C");
                    if (plate.Contains("O"))
                        plate = plate.Replace("O", "0");
                    //plate = ComparePlateNew("", plate);
                    MatchTemplate(ref plate);
                    //remove fisrt and last number
                    Regex rg5 = new Regex("^[0-9]{3}[A-Z]{1,2}[0-9]{3,5}$");
                    if (rg5.Match(plate).Success)
                        plate = plate.Substring(1);
                    Regex rg6 = new Regex("^[0-9]{2}[A-Z]{1,2}[0-9]{6}$");
                    if (rg6.Match(plate).Success)
                        plate = plate.Substring(0, plate.Length - 1);
                    plateResult.Plate = plate;

                    if (gxPG4Frame.ToString().Length > 0)
                    {
                        int x = Math.Min(gxPG4Frame.x1, gxPG4Frame.x2);
                        int y = Math.Min(gxPG4Frame.y1, gxPG4Frame.y3);
                        int w = Math.Max(gxPG4Frame.x3, gxPG4Frame.x4) - x;
                        int h = Math.Max(gxPG4Frame.y2, gxPG4Frame.y4) - y;
                        plateResult.PlateBox = new Rectangle(x, y, w, h);
                    }

                }
                var c = Task.FromResult(plate);
                return plate;
            }
            catch (Exception e)
            {
                NLogHelper.Error("getplate - " + e.Message);
                return null;
            }

        }
        static bool CountContainChar(string plate, string character, int count)
        {
            bool rs = false;
            try
            {
                int countChar = 0;
                for (int i = 0; i < plate.Length; i++)
                {
                    if (plate[i].ToString() == character)
                    {
                        countChar++;
                    }
                }

                if (countChar >= count)
                {
                    rs = true;
                }
            }
            catch (Exception)
            {
                return rs;
            }
            return rs;
        }
        static bool IsContainString(string plate, string pStringForCompare)
        {
            bool rs = false;
            try
            {
                rs = plate.Contains(pStringForCompare);
            }
            catch (Exception)
            {
                return rs;
            }
            return rs;
        }
        static bool IsPass(string recogResult)
        {
            try
            {
                //KQND nhiều hơn 2 ký tự "I" thì bỏ qua không lấy
                bool isPass = CountContainChar(recogResult, "I", 2);
                if (!isPass)
                {
                    isPass = IsContainString(recogResult, "111111");
                }
                if (!isPass)
                {
                    isPass = IsContainString(recogResult, "11111111");
                }
                if (!isPass)
                {
                    isPass = IsContainString(recogResult, "VVV");
                }

                if (!isPass)
                {
                    if (getNumofChar(recogResult) > 3)
                    {
                        isPass = true;
                    }
                }
                if (!isPass)
                {
                    if (recogResult.Length < 5)
                    {
                        isPass = true;
                    }
                }
                return isPass;
            }
            catch (Exception ex)
            {
                NLogHelper.Error(ex);
                return false;
            }
        }
        static int getNumofChar(string plate)
        {
            if (string.IsNullOrEmpty(plate))
            {
                return 0;
            }

            int ret = 0;

            for (int i = 0; i < plate.Length; i++)
            {
                if ('A' <= plate[i] && 'Z' <= plate[i])
                {
                    ret++;
                }
            }

            return ret;
        }
        /// <summary>
        ///     Kiểm tra biển số ND có đúng format Việt Nam không
        /// </summary>
        /// <param name="pPlate"></param>
        /// <returns></returns>
        static bool IsVietNameseFormat(string pPlate)
        {
            Regex rg5 = new Regex("^[1-9]{0,1}[0-9]{0,1}[A-Z]{1,2}[0-9]{3,5}$"); //Viet Nam licence plate number
            return rg5.Match(pPlate).Success;
        }
        static List<string> _mListSeri = new List<string>();
        static Dictionary<string, string> _mListKeyMap = new Dictionary<string, string>();
        static int MatchTemplate(ref string inputPlate)
        {
            if (_mListSeri.Count == 0)
            {
                XmlDocument xmlDoc;
                if (File.Exists("PlateCheckRule.xml"))
                {
                    //loading here
                    xmlDoc = new XmlDocument();
                    xmlDoc.Load("PlateCheckRule.xml");
                    foreach (XmlNode child in xmlDoc.GetElementsByTagName("dictionaryrule"))
                    {
                        if (child.Attributes["name"].Value == "Seri")
                        {
                            foreach (XmlNode child1 in child.SelectNodes("code"))
                            {
                                _mListSeri.Add(child1.Attributes["value"].Value);
                            }
                            break;
                        }
                    }
                }
                else
                {
                    _mListSeri.AddRange(new string[]
                    {
                    "LD", "NG", "QT", "NN", "AA", "AB", "AC", "AD", "AT", "AA", "AP", "BBB", "BC", "BH", "BK", "BL", "BT",
                    "BP", "BS", "BV", "HA", "HB", "HC", "HD", "HE", "HT", "HQ", "HN", "HH", "KA", "KB", "KC", "KD", "KV",
                    "KP", "KH", "KK", "KT", "KN", "PA", "PP", "PK", "PT", "PQ", "PX", "PC", "HL", "QA", "QK", "QP", "QB",
                    "QH", "TC", "TH", "TK", "TT", "TM", "TN", "DB", "ND", "CH", "VB", "VK", "VT"
                    });
                }
                if (File.Exists("CharacterMapping.xml"))
                {
                    //loading here
                    xmlDoc = new XmlDocument();
                    xmlDoc.Load("CharacterMapping.xml");
                    foreach (XmlNode child in xmlDoc.GetElementsByTagName("key"))
                    {
                        _mListKeyMap.Add(child.Attributes["value"].Value, child.InnerText);
                    }
                }
                else
                {
                    _mListKeyMap.Add("0", "D,U,Q");
                    _mListKeyMap.Add("1", "L,J,T,I");
                    _mListKeyMap.Add("2", "Z");
                    _mListKeyMap.Add("3", "E");
                    _mListKeyMap.Add("4", "A");
                    _mListKeyMap.Add("5", "S");
                    _mListKeyMap.Add("6", "C,G,O,B");
                    _mListKeyMap.Add("7", "T,Y");
                    _mListKeyMap.Add("8", "B,A,O");
                    _mListKeyMap.Add("9", "B");
                    _mListKeyMap.Add("A", "4");
                    _mListKeyMap.Add("B", "8,6,0");
                    _mListKeyMap.Add("C", "6");
                    _mListKeyMap.Add("D", "0");
                    _mListKeyMap.Add("E", "3");
                    _mListKeyMap.Add("F", "5");
                    _mListKeyMap.Add("G", "6");
                    _mListKeyMap.Add("H", "4,7,1");
                    _mListKeyMap.Add("I", "1");
                    _mListKeyMap.Add("J", "1");
                    _mListKeyMap.Add("O", "0,6,9,8");
                    _mListKeyMap.Add("S", "5");
                    _mListKeyMap.Add("T", "7");
                    _mListKeyMap.Add("U", "0");
                    _mListKeyMap.Add("Z", "2");
                }
            }
            int iR = 0; //default passed
            Regex rg = new Regex("^[1-9][0-9][A-Z]{1,2}[0-9]{4,5}$"); //Viet Nam licence plate number
            Regex rg2 = new Regex("\\d[A-Z]{2,2}[0-9]{3,5}$"); //checking seri //type 3
            Regex rg3 = new Regex("^[0-9A-Z]{1,4}[0-9]{3,5}\\d$"); //last number failed //type 1
            Regex rg4 = new Regex("^[1-9][0-9][A-Z]{1,2}[0-9]{3,5}$"); //province code failed //type 2
            Regex rg5 = new Regex("^[1-9][A-Z]{1,4}[0-9]{3,5}$"); // sai 2 ky tu
            Match match = rg.Match(inputPlate);
            if (match.Success)
            {
                Match match2 = rg2.Match(inputPlate);
                if (match2.Success)
                {
                    if (_mListSeri.Contains(inputPlate.Substring(2, 2)))
                    {
                        iR = 0;
                    }
                    else
                    {
                        iR = 3;
                    }
                }
            }
            else
            {
                //failed   
                if (rg3.Match(inputPlate).Success)
                {
                    iR = 0;
                    if (rg4.Match(inputPlate).Success)
                    {
                        iR = 0;
                    }
                    else
                    {
                        iR = 2;
                    }
                }
                else
                    iR = 1;
                if (iR > 0)// && string.IsNullOrEmpty(_mRegisteredPlateNumber)) //retry here
                {
                    char[] chartemp = inputPlate.ToCharArray();
                    if (rg5.Match(inputPlate).Success)
                    {
                        if ('A' <= inputPlate[1] && 'Z' >= inputPlate[1])
                        {
                            string s = inputPlate[1].ToString();
                            if (_mListKeyMap.Keys.Contains(s))
                                chartemp[1] = _mListKeyMap[s][0];
                        }
                    }

                    for (int i = 4; i < inputPlate.Length; i++)
                    {
                        if ('A' <= inputPlate[i] && 'Z' >= inputPlate[i])
                        {
                            string s = inputPlate[i].ToString();
                            if (_mListKeyMap.Keys.Contains(s))
                                chartemp[i] = _mListKeyMap[s][0];
                            else
                                chartemp[i] = '0';
                        }
                    }

                    if (chartemp.Length > 0)
                        inputPlate = new string(chartemp);


                    if (inputPlate.Length > 3 && !_mListSeri.Contains(inputPlate.Substring(2, 2)))
                    {
                        if ('A' <= inputPlate[3] && 'Z' >= inputPlate[3])
                        {
                            string s = inputPlate[3].ToString();
                            if (_mListKeyMap.Keys.Contains(s))
                                chartemp[3] = _mListKeyMap[s][0];
                        }

                        if ('A' >= inputPlate[2] && 'Z' <= inputPlate[2])// ky tu thu 2 la ky tu so
                        {
                            string s = inputPlate[2].ToString();
                            if (_mListKeyMap.Keys.Contains(s))
                                chartemp[2] = _mListKeyMap[s][0];
                        }

                        if (chartemp.Length > 0)
                            inputPlate = new string(chartemp);
                    }
                }
            }
            return iR;
        }
        static byte[] Image2Bytes(Image img)
        {
            byte[] byteArray = new byte[0];
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, ImageFormat.Jpeg);
                stream.Close();

                byteArray = stream.ToArray();
            }
            return byteArray;
        }
        static Image Bytes2Image(byte[] imageData)
        {
            try
            {
                using (var ms = new MemoryStream(imageData))
                {
                    Image image = Image.FromStream(ms);
                    return image;
                }
            }
            catch
            {

            }
            return null;
        }
        static string ConvertToUnSign(string text)
        {
            try
            {
                if (string.IsNullOrEmpty(text)) return string.Empty;
                for (int i = 33; i < 48; i++)
                {
                    text = text.Replace(((char)i).ToString(), "");
                }

                for (int i = 58; i < 65; i++)
                {
                    text = text.Replace(((char)i).ToString(), "");
                }

                for (int i = 91; i < 97; i++)
                {
                    text = text.Replace(((char)i).ToString(), "");
                }
                for (int i = 123; i < 127; i++)
                {
                    text = text.Replace(((char)i).ToString(), "");
                }
                text = text.Replace(" ", "-");
                var regex = new Regex(@"\p{IsCombiningDiacriticalMarks}+");
                string strFormD = text.Normalize(NormalizationForm.FormD);
                return
                    regex.Replace(strFormD, String.Empty)
                        .Replace('\u0111', 'd')
                        .Replace('\u0110', 'D')
                        .Replace("-", "")
                        .ToUpper();
            }
            catch (Exception ex)
            {
                //NNLogHelper.Info("ConvertToUnSign: " + ex.Message);
                return text;
            }
        }
        public static string ComparePlateNew(string regisPlate, string recogPlate)
        {
            string correctPlate = recogPlate;
            if (correctPlate != null)
                correctPlate = correctPlate.Replace("Å", "A");
            try
            {
                var myCorrectPlate = new CorrectPlateNumber();
                myCorrectPlate.CorrectNumber(regisPlate, correctPlate);
                correctPlate = myCorrectPlate.ImprovedPlateNumber;
            }
            catch (Exception ex)
            {
                //NNLogHelper.Info("ComparePlateNew: " + ex.ToString());
            }

            return correctPlate;
        }
    }
}
