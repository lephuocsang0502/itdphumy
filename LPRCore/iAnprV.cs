using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Emgu.CV;
using System.Net;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;

namespace LPRCore
{

    // class of ANPR from video
    public class iAnprV
    {
        public int nchar_min = 0;
        public int nchar_max = 0;
        public string root_dir = "";
        Mat input_image = new Mat();
        VideoCapture videoCapture = new VideoCapture();  // video capture for video or camera

        iAnprCallBack current_callback;  // call back function
        iAnprConf config = new iAnprConf();
        public iAnprV(int char_min, int char_max)
        {
            nchar_min = char_min;
            nchar_max = char_max;
            root_dir = AppDomain.CurrentDomain.BaseDirectory;
            // create temp folder to download video from url
            if (!Directory.Exists(root_dir + "/temp/")) Directory.CreateDirectory(root_dir + "/temp/");
            // read configuration parameters
            config.ReadParameters();
        }

        public iAnprV()
        {
            nchar_min = 5;
            nchar_max = 10;
            root_dir = AppDomain.CurrentDomain.BaseDirectory;
            if (!Directory.Exists(root_dir + "/temp/")) Directory.CreateDirectory(root_dir + "/temp/");
            config.ReadParameters();
        }

        // detect all of plates from capture image
        private void DetectLicensePlates(Mat input_img, iAnprCallBack callBack)
        {
            List<iAnprResult> anpr_results = new List<iAnprResult>();

            ImageFormat fmt = new ImageFormat(input_img.Bitmap.RawFormat.Guid);
            var imageCodecInfo = ImageCodecInfo.GetImageEncoders().FirstOrDefault(codec => codec.FormatID == input_img.Bitmap.RawFormat.Guid);
            //this is for situations, where the image is not read from disk, and is stored in the memort(e.g. image comes from a camera or snapshot)
            if (imageCodecInfo == null)
            {
                fmt = ImageFormat.Jpeg;
            }

            StringBuilder result = new StringBuilder(4096);
            using (MemoryStream ms = new MemoryStream())
            {
                input_img.Bitmap.Save(ms, fmt);
                byte[] image_byte_array = ms.ToArray();
                try
                {
                    CDll_Interface.DetectPlates(image_byte_array, ms.Length, config.GetPlateDetectionWidth(), config.GetPlateDetectionHeight(), config.GetPlateDetectionThreshold(), result);
                }
                catch (Exception)
                {
                    return;
                }
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

                Mat plate_image = new Mat(input_img, temp_result.GetAnprFrame());

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
            // send detected results to callback function

            if (anpr_results.Count > 0) callBack(anpr_results);
        }

        // capture images from video or camera
        private void ProcessFrame(object sender, EventArgs e)
        {
            if (videoCapture != null && videoCapture.Ptr != IntPtr.Zero)
            {
                videoCapture.Retrieve(input_image, 0);
                DetectLicensePlates(input_image, current_callback);
            }
        }

        public delegate void iAnprCallBack(List<iAnprResult> result);

        // recognize plates from video and send result to callback function
        public bool GetPlatefromUrl(string pUrlVideo, iAnprCallBack callback)
        {
            Uri uriResult;
            bool url_result = Uri.TryCreate(pUrlVideo, UriKind.Absolute, out uriResult)
                && uriResult.Scheme == Uri.UriSchemeHttp;
            if (url_result)  // from url
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(new Uri(pUrlVideo), root_dir + "/temp/temp.mp4");
                }
                videoCapture = new VideoCapture(root_dir + "/temp/temp.mp4");
            }
            else // from local storage
            {
                videoCapture = new VideoCapture(pUrlVideo);
            }
            if (videoCapture.IsOpened)
            {
                videoCapture.ImageGrabbed += ProcessFrame;
                input_image = new Mat();
                if (videoCapture != null)
                {
                    try
                    {
                        videoCapture.Start();
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }

            current_callback = callback;
            return true;
        }
    }
}
