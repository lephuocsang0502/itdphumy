using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using Emgu.Util;
using Emgu.CV.CvEnum;
using System.Diagnostics;
using Emgu.CV.Util;

namespace LPRCore
{
    // ****** class for recognition result **************    
    public class iAnprResult
    {
        private String AnprText = "";   // recognized number
        private Rectangle AnprFrame = new Rectangle(); // position of plate
        private float AnprConfidence = 0.0f; // confidence of recognition
        public enum iAnpr_enum
        {
            iAnpr_enum_error = -1,
            iAnpr_enum_white,
            iAnpr_enum_red,
            iAnpr_enum_blue
        }

        private iAnpr_enum AnprType;  // the color of plate
        private int AnprVPos = 0;

        public iAnprResult()
        {

        }

        public void SetAnprText(String txt)
        {
            AnprText = txt;
        }

        public String GetAnprText()
        {
            return AnprText;
        }

        public void SetAnprFrame(Rectangle rect)
        {
            AnprFrame = rect;
        }

        public Rectangle GetAnprFrame()
        {
            return AnprFrame;
        }

        public void SetAnprConfidence(float conf)
        {
            AnprConfidence = conf;
        }

        public float GetAnprConfidence()
        {
            return AnprConfidence;
        }

        public void SetAnprType(iAnpr_enum type)
        {
            AnprType = type;
        }

        public iAnpr_enum GetAnprType()
        {
            return AnprType;
        }

        public void SetAnprVPos(int pos)
        {
            AnprVPos = pos;
        }

        public int GetAnprVPos()
        {
            return AnprVPos;
        }
    }
}
