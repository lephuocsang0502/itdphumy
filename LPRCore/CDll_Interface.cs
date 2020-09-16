using System.Text;
using System.Runtime.InteropServices;

namespace LPRCore
{
    class CDll_Interface
    {
        private const string DetectLibraryName = "cvexternitd.dll";

        //load plate detection model
        [DllImport(DetectLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int LoadDetectionModel(string root_dir_path);

        // load car plate recognition model
        [DllImport(DetectLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int LoadCarPlateModel(string root_dir_path);

        // load motor plate recognition model
        [DllImport(DetectLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int LoadMotorPlateModel(string root_dir_path);

        // load car plate color classification model
        [DllImport(DetectLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int LoadClassificationModel(string root_dir_path);

        // detect only one plate with high confidence
        [DllImport(DetectLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void DetectPlateHighConfidence(byte[] img, long nSize, int inpWidth, int inpHeight, float threshold, StringBuilder result);


        // detect all of plates from image
        [DllImport(DetectLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void DetectPlates(byte[] img, long nSize, int inpWidth, int inpHeight, float threshold, StringBuilder result);


        // recognize number from detected plates
        [DllImport(DetectLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void RecognitionPlate(byte[] img, long nSize,  StringBuilder result, int plate_class, int w, int h, float recog_threshold);


        // recognize plate color from detected plates
        [DllImport(DetectLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int RecognitionPlateType(byte[] img, long nSize, float classfiication_threshold);
    }
}
