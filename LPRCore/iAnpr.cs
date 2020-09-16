using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Net;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace LPRCore
{
    // Core class for ANPR from image
    public class iAnpr
    {
        public int nchar_min = 0; // minimum number of characters to recognize
        public int nchar_max = 0; // maximum number of characters to recognize
        public string root_dir = "";
        Mat input_image = new Mat();  // input image
        iAnprResult current_result = new iAnprResult(); // current object of reconition
        List<iAnprResult> anpr_total_results = new List<iAnprResult>();  // total recognition results 

        int current_index = 0;  // current index for get recognition result using FindFirst, FindNext function

        iAnprConf config = new iAnprConf();

        public iAnpr(int char_min, int char_max)
        {
            nchar_min = char_min;
            nchar_max = char_max;
            root_dir = AppDomain.CurrentDomain.BaseDirectory;
            // create temp folder to download image from url
            if (!Directory.Exists(root_dir + "/temp/")) Directory.CreateDirectory(root_dir + "/temp/");
            anpr_total_results.Clear();
            // read configuration parameters
            config.ReadParameters();
        }

        public iAnpr()
        {
            nchar_min = 5;
            nchar_max = 10;
            root_dir = AppDomain.CurrentDomain.BaseDirectory;
            if (!Directory.Exists(root_dir + "/temp/")) Directory.CreateDirectory(root_dir + "/temp/");
            anpr_total_results.Clear();
            config.ReadParameters();
        }

        // detect one plate with high confidence
        public bool GetPlatefromUrl(string pUrlImage)
        {
            Uri uriResult;
            bool url_result = Uri.TryCreate(pUrlImage, UriKind.Absolute, out uriResult)
                && uriResult.Scheme == Uri.UriSchemeHttp;  // check pUrlImage is url or local path
            if (url_result)  // from url
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(new Uri(pUrlImage), root_dir + "/temp/temp.jpg");
                }
                input_image = CvInvoke.Imread(root_dir + "/temp/temp.jpg");
            }
            else // from local storage
            {
                input_image = CvInvoke.Imread(pUrlImage);
            }
            // ******* convert Mat to byte array and detect plate with high confidence ***********
            ImageFormat fmt = new ImageFormat(input_image.Bitmap.RawFormat.Guid);
            var imageCodecInfo = ImageCodecInfo.GetImageEncoders().FirstOrDefault(codec => codec.FormatID == input_image.Bitmap.RawFormat.Guid);
            //this is for situations, where the image is not read from disk, and is stored in the memort(e.g. image comes from a camera or snapshot)
            if (imageCodecInfo == null)
            {
                fmt = ImageFormat.Jpeg;
            }

            StringBuilder result = new StringBuilder(4096);
            using (MemoryStream ms = new MemoryStream())
            {
                input_image.Bitmap.Save(ms, fmt);
                byte[] image_byte_array = ms.ToArray();
                try
                {
                    CDll_Interface.DetectPlateHighConfidence(image_byte_array, ms.Length, config.GetPlateDetectionWidth(), config.GetPlateDetectionHeight(), config.GetPlateDetectionThreshold(), result);
                }
                catch (Exception)
                {
                    return false;
                }
            }
            //****************************************************************
            String detection_result = result.ToString();
            int left, top, w, h, plate_class, confidence;
            String[] result_pos = Regex.Split(detection_result, " ");
            left = (int)(Convert.ToDouble(result_pos[0]));// * detect_scale);
            if (left < 1) left = 1;
            top = (int)(Convert.ToDouble(result_pos[1]));// * detect_scale);
            if (top < 1) top = 1;
            w = (int)(Convert.ToDouble(result_pos[2]));// * detect_scale);
            h = (int)(Convert.ToDouble(result_pos[3]));// * detect_scale);
            confidence = (int)(Convert.ToDouble(result_pos[4]));
            plate_class = (int)(Convert.ToDouble(result_pos[5]));

            current_result.SetAnprFrame(new Rectangle(left, top, w, h));
            // get detected plate image for recognition
            Mat plate_image = new Mat(input_image, current_result.GetAnprFrame());

            ImageFormat fmt1 = new ImageFormat(plate_image.Bitmap.RawFormat.Guid);
            var imageCodecInfo1 = ImageCodecInfo.GetImageEncoders().FirstOrDefault(codec => codec.FormatID == plate_image.Bitmap.RawFormat.Guid);
            //this is for situations, where the image is not read from disk, and is stored in the memort(e.g. image comes from a camera or snapshot)
            if (imageCodecInfo1 == null)
            {
                fmt1 = ImageFormat.Jpeg;
            }

            StringBuilder result_text = new StringBuilder(4096);
            int plate_type = -1;
            using (MemoryStream ms = new MemoryStream())
            {
                plate_image.Bitmap.Save(ms, fmt1);
                byte[] image_byte_array = ms.ToArray();
                // ********* get number and plate type from byte array using c++ dll engine **********
                try
                {
                    if (plate_class == 0)
                    {
                        CDll_Interface.RecognitionPlate(image_byte_array, ms.Length, result_text, plate_class, config.GetCarPlateRecognitionWidth(), config.GetCarPlateRecognitionHeight(), config.GetPlateRecognitionThreshold());
                    }
                    else
                    {
                        CDll_Interface.RecognitionPlate(image_byte_array, ms.Length, result_text, plate_class, config.GetMotorPlateRecognitionWidth(), config.GetMotorPlateRecognitionHeight(), config.GetPlateRecognitionThreshold());
                    }
                    plate_type = CDll_Interface.RecognitionPlateType(image_byte_array, ms.Length, config.GetColorClassificationThreshold());
                }
                catch (Exception)
                {
                    return false;
                }
                //**************************************************************************************
            }
            String recognition_result = result_text.ToString();
            string anpr_text = recognition_result.Split(' ')[0];
            // check recognition result is valid or not
            if (anpr_text.Length >= nchar_min && anpr_text.Length <= nchar_max)
            {
                current_result.SetAnprText(recognition_result.Split(' ')[0]);  // set plate number to current object
                current_result.SetAnprConfidence((float)(Convert.ToDouble(recognition_result.Split(' ')[1]))); // set recognition confidence to current object
                switch (plate_type)  // set plate color type to current object
                {
                    case 0:
                        current_result.SetAnprType(iAnprResult.iAnpr_enum.iAnpr_enum_blue);
                        break;
                    case 1:
                        current_result.SetAnprType(iAnprResult.iAnpr_enum.iAnpr_enum_red);
                        break;
                    case 2:
                        current_result.SetAnprType(iAnprResult.iAnpr_enum.iAnpr_enum_white);
                        break;
                    default:
                        current_result.SetAnprType(iAnprResult.iAnpr_enum.iAnpr_enum_error);
                        break;
                }

            }
            else
            {
                return false;
            }


            return true;
        }

        // detect one plate with high confidence
        public bool GetPlateFromMem(byte[] buffer)
        {
            // convert byte array to Mat 
            CvInvoke.Imdecode(buffer, ImreadModes.Color, input_image);
            StringBuilder result = new StringBuilder(4096);
            // detect plate from byte array using c++ dll
            try
            {
                CDll_Interface.DetectPlateHighConfidence(buffer, buffer.ToArray().Length, config.GetPlateDetectionWidth(), config.GetPlateDetectionHeight(), config.GetPlateDetectionThreshold(), result);
            }
            catch (Exception ex)
            {
                return false;
            }
            
            String detection_result = result.ToString();
            int left, top, w, h, plate_class=0, confidence;
            String[] result_pos = Regex.Split(detection_result, " ");
            left = (int)(Convert.ToDouble(result_pos[0]));// * detect_scale);
            if (left < 1) left = 1;
            top = (int)(Convert.ToDouble(result_pos[1]));// * detect_scale);
            if (top < 1) top = 1;
            w = (int)(Convert.ToDouble(result_pos[2]));// * detect_scale);
            h = (int)(Convert.ToDouble(result_pos[3]));// * detect_scale);
            confidence = (int)(Convert.ToDouble(result_pos[4]));
            try
            {
                plate_class = (int)(Convert.ToDouble(result_pos[5]));
            }
            catch { }

            current_result.SetAnprFrame(new Rectangle(left, top, w, h));

            Mat plate_image = new Mat(input_image, current_result.GetAnprFrame());

            ImageFormat fmt1 = new ImageFormat(plate_image.Bitmap.RawFormat.Guid);
            var imageCodecInfo1 = ImageCodecInfo.GetImageEncoders().FirstOrDefault(codec => codec.FormatID == plate_image.Bitmap.RawFormat.Guid);
            //this is for situations, where the image is not read from disk, and is stored in the memort(e.g. image comes from a camera or snapshot)
            if (imageCodecInfo1 == null)
            {
                fmt1 = ImageFormat.Jpeg;
            }

            StringBuilder result_text = new StringBuilder(4096);
            int plate_type = -1;
            using (MemoryStream ms = new MemoryStream())
            {
                plate_image.Bitmap.Save(ms, fmt1);
                byte[] image_byte_array = ms.ToArray();
                try
                {
                    if (plate_class == 0)
                    {
                        CDll_Interface.RecognitionPlate(image_byte_array, ms.Length, result_text, plate_class, config.GetCarPlateRecognitionWidth(), config.GetCarPlateRecognitionHeight(), config.GetPlateRecognitionThreshold());
                    }
                    else
                    {
                        CDll_Interface.RecognitionPlate(image_byte_array, ms.Length, result_text, plate_class, config.GetMotorPlateRecognitionWidth(), config.GetMotorPlateRecognitionHeight(), config.GetPlateRecognitionThreshold());
                    }
                    plate_type = CDll_Interface.RecognitionPlateType(image_byte_array, ms.Length, config.GetColorClassificationThreshold());
                }
                catch (Exception)
                {
                    return false;
                }
            }
            String recognition_result = result_text.ToString();
            string anpr_text = recognition_result.Split(' ')[0];
            if (anpr_text.Length >= nchar_min && anpr_text.Length <= nchar_max)
            {
                current_result.SetAnprText(recognition_result.Split(' ')[0]);
                current_result.SetAnprConfidence((float)(Convert.ToDouble(recognition_result.Split(' ')[1])));
                switch (plate_type)
                {
                    case 0:
                        current_result.SetAnprType(iAnprResult.iAnpr_enum.iAnpr_enum_blue);
                        break;
                    case 1:
                        current_result.SetAnprType(iAnprResult.iAnpr_enum.iAnpr_enum_red);
                        break;
                    case 2:
                        current_result.SetAnprType(iAnprResult.iAnpr_enum.iAnpr_enum_white);
                        break;
                    default:
                        current_result.SetAnprType(iAnprResult.iAnpr_enum.iAnpr_enum_error);
                        break;
                }

            }
            else
            {
                return false;
            }


            return true;
        }

        // detect all of plates from image url
        public List<iAnprResult> GetAllPlatefromUrl(string pUrlImage)
        {
            List<iAnprResult> anpr_results = new List<iAnprResult>();
            Uri uriResult;
            bool url_result = Uri.TryCreate(pUrlImage, UriKind.Absolute, out uriResult)
                && uriResult.Scheme == Uri.UriSchemeHttp;  // check pUrlImage is url or local storage path
            if (url_result)  // from url
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(new Uri(pUrlImage), root_dir + "/temp/temp.jpg");
                }
                input_image = CvInvoke.Imread(root_dir + "/temp/temp.jpg");
            }
            else // from local storage
            {
                input_image = CvInvoke.Imread(pUrlImage);
            }
            ImageFormat fmt = new ImageFormat(input_image.Bitmap.RawFormat.Guid);
            var imageCodecInfo = ImageCodecInfo.GetImageEncoders().FirstOrDefault(codec => codec.FormatID == input_image.Bitmap.RawFormat.Guid);
            //this is for situations, where the image is not read from disk, and is stored in the memort(e.g. image comes from a camera or snapshot)
            if (imageCodecInfo == null)
            {
                fmt = ImageFormat.Jpeg;
            }

            StringBuilder result = new StringBuilder(4096);
            using (MemoryStream ms = new MemoryStream())
            {
                input_image.Bitmap.Save(ms, fmt);
                byte[] image_byte_array = ms.ToArray();
                // ********* detect all of plates from byte array using c++ dll engine *********
                try
                {
                    CDll_Interface.DetectPlates(image_byte_array, ms.Length, config.GetPlateDetectionWidth(), config.GetPlateDetectionHeight(), config.GetPlateDetectionThreshold(), result);
                }
                catch (Exception)
                {
                    return anpr_results;
                }
                //********************************************************************************
            }
            String detection_result = result.ToString();
            int left, top, w, h, plate_class, confidence;
            string[] result_items = Regex.Split(detection_result, ":");
            for (int i = 0; i < result_items.Length - 1; i++)
            {
                iAnprResult temp_result = new iAnprResult();
                String[] result_pos = Regex.Split(result_items[i], " ");
                left = (int)(Convert.ToDouble(result_pos[0]));// * detect_scale);
                if (left < 1) left = 1;
                top = (int)(Convert.ToDouble(result_pos[1]));// * detect_scale);
                if (top < 1) top = 1;
                w = (int)(Convert.ToDouble(result_pos[2]));// * detect_scale);
                h = (int)(Convert.ToDouble(result_pos[3]));// * detect_scale);
                confidence = (int)(Convert.ToDouble(result_pos[4]));
                plate_class = (int)(Convert.ToDouble(result_pos[5]));

                temp_result.SetAnprFrame(new Rectangle(left, top, w, h));

                Mat plate_image = new Mat(input_image, temp_result.GetAnprFrame());

                ImageFormat fmt1 = new ImageFormat(plate_image.Bitmap.RawFormat.Guid);
                var imageCodecInfo1 = ImageCodecInfo.GetImageEncoders().FirstOrDefault(codec => codec.FormatID == plate_image.Bitmap.RawFormat.Guid);
                //this is for situations, where the image is not read from disk, and is stored in the memort(e.g. image comes from a camera or snapshot)
                if (imageCodecInfo1 == null)
                {
                    fmt1 = ImageFormat.Jpeg;
                }

                StringBuilder result_text = new StringBuilder(4096);
                int plate_type = -1;
                using (MemoryStream ms = new MemoryStream())
                {
                    plate_image.Bitmap.Save(ms, fmt1);
                    byte[] image_byte_array = ms.ToArray();
                    try
                    {
                        if (plate_class == 0)
                        {
                            CDll_Interface.RecognitionPlate(image_byte_array, ms.Length, result_text, plate_class, config.GetCarPlateRecognitionWidth(), config.GetCarPlateRecognitionHeight(), config.GetPlateRecognitionThreshold());
                        }
                        else
                        {
                            CDll_Interface.RecognitionPlate(image_byte_array, ms.Length, result_text, plate_class, config.GetMotorPlateRecognitionWidth(), config.GetMotorPlateRecognitionHeight(), config.GetPlateRecognitionThreshold());
                        }
                        plate_type = CDll_Interface.RecognitionPlateType(image_byte_array, ms.Length, config.GetColorClassificationThreshold());
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
                String recognition_result = result_text.ToString();
                string anpr_text = recognition_result.Split(' ')[0];
                if (anpr_text.Length >= nchar_min && anpr_text.Length <= nchar_max)
                {
                    temp_result.SetAnprText(recognition_result.Split(' ')[0]);
                    temp_result.SetAnprConfidence((float)(Convert.ToDouble(recognition_result.Split(' ')[1])));
                    switch (plate_type)
                    {
                        case 0:
                            temp_result.SetAnprType(iAnprResult.iAnpr_enum.iAnpr_enum_blue);
                            break;
                        case 1:
                            temp_result.SetAnprType(iAnprResult.iAnpr_enum.iAnpr_enum_red);
                            break;
                        case 2:
                            temp_result.SetAnprType(iAnprResult.iAnpr_enum.iAnpr_enum_white);
                            break;
                        default:
                            temp_result.SetAnprType(iAnprResult.iAnpr_enum.iAnpr_enum_error);
                            break;
                    }

                }
                else
                {
                    continue;
                }

                anpr_results.Add(temp_result);
            }
            return anpr_results;
        }

        // detect all plates from byte array of image
        public List<iAnprResult> GetAllPlateFromMem(byte[] buffer)
        {
            List<iAnprResult> anpr_results = new List<iAnprResult>();
            CvInvoke.Imdecode(buffer, ImreadModes.Color, input_image);
            StringBuilder result = new StringBuilder(4096);
            try
            {
                CDll_Interface.DetectPlates(buffer, buffer.ToArray().Length, config.GetPlateDetectionWidth(), config.GetPlateDetectionHeight(), config.GetPlateDetectionThreshold(), result);
            }
            catch (Exception)
            {
                return anpr_results;
            }
            
            String detection_result = result.ToString();
            int left, top, w, h, plate_class, confidence;
            string[] result_items = Regex.Split(detection_result, ":");
            for (int i = 0; i < result_items.Length - 1; i++)
            {
                iAnprResult temp_result = new iAnprResult();
                String[] result_pos = Regex.Split(result_items[i], " ");
                left = (int)(Convert.ToDouble(result_pos[0]));// * detect_scale);
                if (left < 1) left = 1;
                top = (int)(Convert.ToDouble(result_pos[1]));// * detect_scale);
                if (top < 1) top = 1;
                w = (int)(Convert.ToDouble(result_pos[2]));// * detect_scale);
                h = (int)(Convert.ToDouble(result_pos[3]));// * detect_scale);
                confidence = (int)(Convert.ToDouble(result_pos[4]));
                plate_class = (int)(Convert.ToDouble(result_pos[5]));

                temp_result.SetAnprFrame(new Rectangle(left, top, w, h));

                Mat plate_image = new Mat(input_image, temp_result.GetAnprFrame());

                ImageFormat fmt1 = new ImageFormat(plate_image.Bitmap.RawFormat.Guid);
                var imageCodecInfo1 = ImageCodecInfo.GetImageEncoders().FirstOrDefault(codec => codec.FormatID == plate_image.Bitmap.RawFormat.Guid);
                //this is for situations, where the image is not read from disk, and is stored in the memort(e.g. image comes from a camera or snapshot)
                if (imageCodecInfo1 == null)
                {
                    fmt1 = ImageFormat.Jpeg;
                }

                StringBuilder result_text = new StringBuilder(4096);
                int plate_type = -1;
                using (MemoryStream ms = new MemoryStream())
                {
                    plate_image.Bitmap.Save(ms, fmt1);
                    byte[] image_byte_array = ms.ToArray();
                    try
                    {
                        if (plate_class == 0)
                        {
                            CDll_Interface.RecognitionPlate(image_byte_array, ms.Length, result_text, plate_class, config.GetCarPlateRecognitionWidth(), config.GetCarPlateRecognitionHeight(), config.GetPlateRecognitionThreshold());
                        }
                        else
                        {
                            CDll_Interface.RecognitionPlate(image_byte_array, ms.Length, result_text, plate_class, config.GetMotorPlateRecognitionWidth(), config.GetMotorPlateRecognitionHeight(), config.GetPlateRecognitionThreshold());
                        }
                        plate_type = CDll_Interface.RecognitionPlateType(image_byte_array, ms.Length, config.GetColorClassificationThreshold());
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
                String recognition_result = result_text.ToString();
                string anpr_text = recognition_result.Split(' ')[0];
                if (anpr_text.Length >= nchar_min && anpr_text.Length <= nchar_max)
                {
                    temp_result.SetAnprText(recognition_result.Split(' ')[0]);
                    temp_result.SetAnprConfidence((float)(Convert.ToDouble(recognition_result.Split(' ')[1])));
                    switch (plate_type)
                    {
                        case 0:
                            temp_result.SetAnprType(iAnprResult.iAnpr_enum.iAnpr_enum_blue);
                            break;
                        case 1:
                            temp_result.SetAnprType(iAnprResult.iAnpr_enum.iAnpr_enum_red);
                            break;
                        case 2:
                            temp_result.SetAnprType(iAnprResult.iAnpr_enum.iAnpr_enum_white);
                            break;
                        default:
                            temp_result.SetAnprType(iAnprResult.iAnpr_enum.iAnpr_enum_error);
                            break;
                    }

                }
                else
                {
                    continue;
                }

                anpr_results.Add(temp_result);
            }
            return anpr_results;
        }

        // set first dected plate to current object from byte array of image
        public bool FindFirst(byte[] buffer)
        {
            CvInvoke.Imdecode(buffer, ImreadModes.Color, input_image);
            
            StringBuilder result = new StringBuilder(4096);
            
            try
            {
                CDll_Interface.DetectPlates(buffer, buffer.ToArray().Length, config.GetPlateDetectionWidth(), config.GetPlateDetectionHeight(), config.GetPlateDetectionThreshold(), result);
            }
            catch (Exception)
            {
                return false;
            }
            
            String detection_result = result.ToString();
            int left, top, w, h, plate_class, confidence;
            string[] result_items = Regex.Split(detection_result, ":");
            for (int i = 0; i < result_items.Length - 1; i++)
            {
                iAnprResult temp_result = new iAnprResult();
                String[] result_pos = Regex.Split(result_items[i], " ");
                left = (int)(Convert.ToDouble(result_pos[0]));// * detect_scale);
                if (left < 1) left = 1;
                top = (int)(Convert.ToDouble(result_pos[1]));// * detect_scale);
                if (top < 1) top = 1;
                w = (int)(Convert.ToDouble(result_pos[2]));// * detect_scale);
                h = (int)(Convert.ToDouble(result_pos[3]));// * detect_scale);
                confidence = (int)(Convert.ToDouble(result_pos[4]));
                plate_class = (int)(Convert.ToDouble(result_pos[5]));

                temp_result.SetAnprFrame(new Rectangle(left, top, w, h));

                Mat plate_image = new Mat(input_image, temp_result.GetAnprFrame());

                ImageFormat fmt1 = new ImageFormat(plate_image.Bitmap.RawFormat.Guid);
                var imageCodecInfo1 = ImageCodecInfo.GetImageEncoders().FirstOrDefault(codec => codec.FormatID == plate_image.Bitmap.RawFormat.Guid);
                //this is for situations, where the image is not read from disk, and is stored in the memort(e.g. image comes from a camera or snapshot)
                if (imageCodecInfo1 == null)
                {
                    fmt1 = ImageFormat.Jpeg;
                }

                StringBuilder result_text = new StringBuilder(4096);
                int plate_type = -1;
                using (MemoryStream ms = new MemoryStream())
                {
                    plate_image.Bitmap.Save(ms, fmt1);
                    byte[] image_byte_array = ms.ToArray();
                    try
                    {
                        if (plate_class == 0)
                        {
                            CDll_Interface.RecognitionPlate(image_byte_array, ms.Length, result_text, plate_class, config.GetCarPlateRecognitionWidth(), config.GetCarPlateRecognitionHeight(), config.GetPlateRecognitionThreshold());
                        }
                        else
                        {
                            CDll_Interface.RecognitionPlate(image_byte_array, ms.Length, result_text, plate_class, config.GetMotorPlateRecognitionWidth(), config.GetMotorPlateRecognitionHeight(), config.GetPlateRecognitionThreshold());
                        }
                        plate_type = CDll_Interface.RecognitionPlateType(image_byte_array, ms.Length, config.GetColorClassificationThreshold());
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
                String recognition_result = result_text.ToString();
                string anpr_text = recognition_result.Split(' ')[0];
                if (anpr_text.Length >= nchar_min && anpr_text.Length <= nchar_max)
                {
                    temp_result.SetAnprText(recognition_result.Split(' ')[0]);
                    temp_result.SetAnprConfidence((float)(Convert.ToDouble(recognition_result.Split(' ')[1])));
                    switch (plate_type)
                    {
                        case 0:
                            temp_result.SetAnprType(iAnprResult.iAnpr_enum.iAnpr_enum_blue);
                            break;
                        case 1:
                            temp_result.SetAnprType(iAnprResult.iAnpr_enum.iAnpr_enum_red);
                            break;
                        case 2:
                            temp_result.SetAnprType(iAnprResult.iAnpr_enum.iAnpr_enum_white);
                            break;
                        default:
                            temp_result.SetAnprType(iAnprResult.iAnpr_enum.iAnpr_enum_error);
                            break;
                    }

                }
                else
                {
                    continue;
                }

                anpr_total_results.Add(temp_result);
            }
            if (anpr_total_results.Count < 1) return false;
            current_result = anpr_total_results[0];
            current_index = 0;
            return true;
        }

        // set next detected result to current object
        public bool FindNext()
        {
            if (anpr_total_results.Count < 1) return false;
            current_index++;
            if (current_index > anpr_total_results.Count - 1)
            {
                return false;
            }
            current_result = anpr_total_results[current_index];
            return true;
        }

        // get current object of recognition
        public iAnprResult GetResult()
        {
            return current_result;
        }
    }
}
