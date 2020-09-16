using System;
using System.IO;

namespace LPRCore
{
    // ******** class of confiurations for ANPR
    public class iAnprConf
    {
        public enum iAnprConf_enum
        {
            iAnprConf_enum_car,
            iAnprConf_enum_classification,
            iAnprConf_enum_detection,
            iAnprConf_enum_motor
        }
        private String root_dir = "";
        private String detectionModel = "detection";  // detection model name (detection.cfg and detection. weights)
        private String carPlateModel = "car_lp";      // car plate recognition model name(car_lp.cfg and car_lp.weights)
        private String motorPlateModel = "motor_lp";  // motor plate recognition model name (motor_lp.cfg and motor_lp.weights)
        private String classificationModel = "classification"; // plate color classification model name (classification.cfg and classification.weights)

        private float plate_detection_threshold = 0.3f;
        private int plate_detection_width = 416;
        private int plate_detection_height = 416;
        private float plate_recognition_threshold = 0.3f;
        private int car_plate_recognition_width = 160;
        private int car_plate_recognition_height = 40;
        private int motor_plate_recognition_width = 80;
        private int motor_plate_recognition_height = 80;
        private float classification_threshold = 0.8f;

        // structure
        public iAnprConf()
        {
            root_dir = AppDomain.CurrentDomain.BaseDirectory;
        }

        public bool ReadParameters()
        {
            try
            {
                // read parameters from configuration file
                using (var reader = new StreamReader(root_dir + "/settings.config"))
                {
                    string line;
                    // read plate detection threshold
                    if ((line = reader.ReadLine()) != null)
                    {
                        plate_detection_threshold = (float)(Convert.ToDouble(line.Split('=')[1]));
                    }
                    // read plate detection width
                    if ((line = reader.ReadLine()) != null)
                    {
                        plate_detection_width = (int)(Convert.ToDouble(line.Split('=')[1]));
                    }
                    // read plate detection height
                    if ((line = reader.ReadLine()) != null)
                    {
                        plate_detection_height = (int)(Convert.ToDouble(line.Split('=')[1]));
                    }
                    // read plate recognition threshold
                    if ((line = reader.ReadLine()) != null)
                    {
                        plate_recognition_threshold = (float)(Convert.ToDouble(line.Split('=')[1]));
                    }
                    // read car plate recognition width
                    if ((line = reader.ReadLine()) != null)
                    {
                        car_plate_recognition_width = (int)(Convert.ToDouble(line.Split('=')[1]));
                    }
                    // read car plate recognition height
                    if ((line = reader.ReadLine()) != null)
                    {
                        car_plate_recognition_height = (int)(Convert.ToDouble(line.Split('=')[1]));
                    }
                    // read motor plate recognition width
                    if ((line = reader.ReadLine()) != null)
                    {
                        motor_plate_recognition_width = (int)(Convert.ToDouble(line.Split('=')[1]));
                    }
                    // read motor plate recognition height
                    if ((line = reader.ReadLine()) != null)
                    {
                        motor_plate_recognition_height = (int)(Convert.ToDouble(line.Split('=')[1]));
                    }
                    // read plate color classification threshold
                    if ((line = reader.ReadLine()) != null)
                    {
                        classification_threshold = (float)(Convert.ToDouble(line.Split('=')[1]));
                    }
                }
            }
            catch(Exception)
            {
                return false;
            }
            return true;
            
        }

        // Load model files
        public bool LoadConf(iAnprConf_enum modelType)
        {
            try
            {
                // load plate detection model
                if (modelType == iAnprConf_enum.iAnprConf_enum_detection)
                {
                    int res1 = CDll_Interface.LoadDetectionModel(root_dir + "/models/" + detectionModel);
                    if (res1 == -1) return false;
                }
                // load car plate recognition model
                else if (modelType == iAnprConf_enum.iAnprConf_enum_car)
                {
                    int res2 = CDll_Interface.LoadCarPlateModel(root_dir + "/models/" + carPlateModel);
                    if (res2 == -1) return false;
                }
                // load motor plate recognition model
                else if (modelType == iAnprConf_enum.iAnprConf_enum_motor)
                {
                    int res3 = CDll_Interface.LoadMotorPlateModel(root_dir + "/models/" + motorPlateModel);
                    if (res3 == -1) return false;
                }
                // load plate color classification model
                else if (modelType == iAnprConf_enum.iAnprConf_enum_classification)
                {
                    int res = CDll_Interface.LoadClassificationModel(root_dir + "/models/" + classificationModel);
                    if (res == -1) return false;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception)
            {
                return false;
            }
            
            return true;
        }

        public bool UpdateConf()
        {
            return true;
        }

        // set detection model name
        public void SetDetectionModel(String model)
        {
            detectionModel = model;
        }

        // get detection model name
        public String GetDetectionModel()
        {
            return detectionModel;
        }

        // set car plate recognition model name
        public void SetCarPlateModel(String model)
        {
            carPlateModel = model;
        }
        // get car plate recognition model name
        public String GetCarPlateModel()
        {
            return carPlateModel;
        }
        // set motor plate recognition model name
        public void SetMotorPlateModel(String model)
        {
            motorPlateModel = model;
        }
        // get motor plate recognition model name
        public String GetMotorPlateModel()
        {
            return motorPlateModel;
        }
        // set plate color classification model name
        public void SetClassificationModel(String model)
        {
            classificationModel = model;
        }
        // get plate color classification model name
        public String GetClassificationModel()
        {
            return classificationModel;
        }

        // set plate detection threshold
        public void SetPlateDetectionThreshold(float thresh)
        {
            plate_detection_threshold = thresh;
        }

        // get plate detection threshold
        public float GetPlateDetectionThreshold()
        {
            return plate_detection_threshold;
        }

        // set plate detection model width
        public void SetPlateDetectionWidth(int width)
        {
            plate_detection_width = width;
        }
        // get plate detection model width
        public int GetPlateDetectionWidth()
        {
            return plate_detection_width;
        }
        // set plate detection model height
        public void SetPlateDetectionHeight(int height)
        {
            plate_detection_height = height;
        }
        // get plate detection model height
        public int GetPlateDetectionHeight()
        {
            return plate_detection_height;
        }
        // set plate recognition threshold
        public void SetPlateRecognitionThreshold(float thresh)
        {
            plate_recognition_threshold = thresh;
        }
        // get plate recognition threshold
        public float GetPlateRecognitionThreshold()
        {
            return plate_recognition_threshold;
        }
        // set car plate recognition model width
        public void SetCarPlateRecognitionWidth(int w)
        {
            car_plate_recognition_width = w;
        }
        // get car plate recognition model width
        public int GetCarPlateRecognitionWidth()
        {
            return car_plate_recognition_width;
        }
        // set car plate recognition model height
        public void SetCarPlateRecognitionHeight(int h)
        {
            car_plate_recognition_height = h;
        }
        // get car plate recognition model height
        public int GetCarPlateRecognitionHeight()
        {
            return car_plate_recognition_height;
        }
        // set motor plate recognition model width
        public void SetMotorPlateRecognitionWidth(int w)
        {
            motor_plate_recognition_width = w;
        }
        // get motor plate recognition model width
        public int GetMotorPlateRecognitionWidth()
        {
            return motor_plate_recognition_width;
        }
        // set motor plate recognition model height
        public void SetMotorPlateRecognitionHeight(int h)
        {
            motor_plate_recognition_height = h;
        }
        // get motor plate recognition model height
        public int GetMotorPlateRecognitionHeight()
        {
            return motor_plate_recognition_height;
        }
        // set color classification threshold
        public void SetColorClassificationThreshold(float thresh)
        {
            classification_threshold = thresh;
        }
        // get color classification threshold
        public float GetColorClassificationThreshold()
        {
            return classification_threshold;
        }
    }
}
