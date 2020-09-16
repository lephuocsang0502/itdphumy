using System;
using System.Collections.Generic;
using System.Text;

namespace ITD.PhuMyPort.TCP
{
    public class Consts
    {
        #region Const
        /// <summary>
        /// Thoi gian cho khi goi lenh command
        /// </summary>
        public const int DelayTime = 50;
        /// <summary>
        /// Độ dài gói tin giao tiep voi plc:10//moi sua lai plc nam 2013
        /// Tính theo byte
        /// </summary>
        public const int PACKAGE_LEN = 8;
        /// <summary>
        /// Độ dài gói tin giao tiep voi plc:10//moi sua lai plc nam 2013
        /// Tính theo byte
        /// </summary>
        public const int TEMP_PACKAGE_LEN = 8;
        /// <summary>
        /// So luong bit trong goi tin 64bit(8byte)
        /// </summary>
        public const int BIT_NUMBER = 64;
        #region vi tri cac bit chua trang thai cua PLC va su thay doi cua cac bit do
        /// <summary>
        /// Trang thai nut uu tien 1 xe ở bàn phím cơ
        /// Nằm ở vi trí bit 0
        /// </summary>
        public const int STATUS_BIT_0_GREEN_BUTTON = 0;
        /// <summary>
        /// Trang thai cho biet gia tri cua nut co thay doi so voi truoc do ko
        /// Neu gia tri =0: ko thay doi, =1 co thay doi
        /// </summary>
        public const int STATUS_BIT_0_GREEN_BUTTON_TRACKING = 24;
        /// <summary>
        /// Trang thai nut uu tien doan xe ở bàn phím cơ
        /// Nằm ở vi trí bit 1
        /// </summary>
        public const int STATUS_BIT_1_BLUE_BUTTON = 1;
        public const int STATUS_BIT_1_BLUE_BUTTON_TRACKING = 25;
        /// <summary>
        /// Trang thai nut DONG BARRIER
        /// Nằm ở vi trí bit 2
        /// </summary>
        public const int STATUS_BIT_2_RED_BUTTON = 2;
        public const int STATUS_BIT_2_RED_BUTTON_TRACKING = 26;
        /// <summary>
        /// Vi tri bit thu 3, CHO BIET CO LENH MO LANE TU BAN PHIM CO
        /// </summary>
        public const int STATUS_BIT_3_LANE = 3;
        public const int STATUS_BIT_3_LANE_TRACKING = 27;
        /// <summary>
        /// gIA TRI CHO BIET Có lệnh mở khóa từ bàn cơ 28May2016
        /// </summary>
        public const int STATUS_BIT_4_MANUAL_BARRIER = 4;
        public const int STATUS_BIT_4_MANUAL_BARRIER_TRACKING = 28;
        /// <summary>
        /// gIA TRI CHO BIET Có lệnh mở khóa từ bàn cơ
        /// </summary>
        public const int STATUS_BIT_6_LOCK_LANE = 6;
        public const int STATUS_BIT_6_LOCK_LANE_TRACKING = 30;

        /// <summary>
        /// Loop A status: vi tri bit thu 8
        /// </summary>
        public const int STATUS_BIT_8_LOOPA = 8;
        public const int STATUS_BIT_8_LOOPA_TRACKING = 32;
        /// <summary>
        /// Loop B status: vi tri bit thu 9
        /// </summary>
        public const int STATUS_BIT_9_LOOPB = 9;
        public const int STATUS_BIT_9_LOOPB_TRACKING = 33;
        /// <summary>
        /// Loop C status: vi tri bit thu 10
        /// </summary>
        public const int STATUS_BIT_10_LOOPC = 10;
        public const int STATUS_BIT_10_LOOPC_TRACKING = 34;
        /// <summary>
        /// Loop D status (lock OBU): vi tri bit thu 11
        /// Vetc tin hieu loopD va sick ket hop
        /// </summary>
        public const int STATUS_BIT_11_LOOPLOCK_SIGNAL = 11;
        public const int STATUS_BIT_11_LOOPLOCK_SIGNAL_TRACKING = 35;

        /// <summary>
        /// Loop E status: loop moi bo sung de xac dinh lock he thong
        /// </summary>
        public const int STATUS_BIT_13_LOOPE = 13;
        public const int STATUS_BIT_13_LOOPE_TRACKING = 37;
        /// <summary>
        /// Bit hu coi khi lock he thong: vi tri bit thu 13
        /// </summary>
        public const int STATUS_BIT_13_ALARM_HORN = 13;
        public const int STATUS_BIT_13_ALARM_HORN_TRACKING = 37;
        /// <summary>
        /// Bit thong bao trang thai loi cua he thong
        /// 1:Data bị lỗi,0: nguoc lai
        /// </summary>
        //public const int STATUS_BIT_13_DATAERROR = 13;
        //public const int STATUS_BIT_13_DATAERROR_TRACKING = 37;
        /// <summary>
        /// Bit thu 14 cho biet Có đoàn xe đang qua hay ko
        /// </summary>
        public const int STATUS_BIT_14_BARRIER_OPENMANUAL = 14;
        public const int STATUS_BIT_14_BARRIER_OPENMANUAL_TRACKING = 38;
        /// <summary>
        /// Bit thu 15 cho biet trang thai barrier
        /// </summary>
        public const int STATUS_BIT_15_BARRIER = 15;
        public const int STATUS_BIT_15_BARRIER_TRACKING = 39;
        /// <summary>
        /// bit trang thai cot hong ngoai1: vi tri 16
        /// </summary>
        //public const int STATUS_BIT_16_LOOPD = 16;
        //public const int STATUS_BIT_16_LOOPD_TRACKING = 40;
        /// <summary>
        /// trang thai cua reader A
        /// </summary>
        public const int STATUS_BIT_20_STATUS_READER_A = 20;//reader A
        public const int STATUS_BIT_20_STATUS_READERA_TRACKING = 44;
        /// <summary>
        /// trang thai cua reader b
        /// </summary>
        public const int STATUS_BIT_21_STATUS_READER_B = 21;//reader B
        public const int STATUS_BIT_21_STATUS_READERB_TRACKING = 45;
        /// <summary>
        /// cho biet reader nao dang duoc chon
        /// </summary>
        public const int STATUS_BIT_22_STATUS_SELECTED_READER = 22;//
        public const int STATUS_BIT_22_STATUS_READERB_TRACKING = 46;
        /// <summary>
        /// trang thai cua reader ma PLC giam sat duoc
        /// </summary>
        public const int STATUS_BIT_23_STATUS_OF_READER_PLC = 23;//
        public const int STATUS_BIT_23_STATUS_OF_READER_PLC_TRACKING = 47;
        /// <summary>
        /// 
        /// </summary>
        public const int STATUS_BIT_28_STATUS_READER = 13;//
        public const int STATUS_BIT_28_STATUS_OF_READERE_TRACKING = 37;
        /// <summary>
        /// loop chup hinh F
        /// </summary>
        public const int STATUS_BIT_13_LOOPF = 13;//
        public const int STATUS_BIT_13_LOOPF_TRACKING = 37;
        //      public const int STATUS_BIT_28_STATUS_READER = 21;//
        //public const int STATUS_BIT_28_STATUS_OF_READERE_TRACKING = 45;
        /// <summary>
        /// bit trang thai cot hong ngoai 2: vi tri 17
        /// </summary>
        //public const int STATUS_BIT_17_INFRARED2 = 17;//Bit hong ngoai
        //public const int STATUS_BIT_17_INFRARED2_TRACKING = 41;

        /// <summary>
        /// Vị trí bit 18 chứa trạng thái của loop phát hành thẻ 1
        /// </summary>
        //public const int STATUS_BIT_18_CARD_LOOP1 = 18;
        //public const int STATUS_BIT_18_CARD_LOOP1_TRACKING = 42;
        /// <summary>
        ///  Vị trí bit 19 chứa trạng thái loop D
        /// </summary>
        public const int STATUS_BIT_19_LOOP_D = 19;
        public const int STATUS_BIT_19_LOOP_D_TRACKING = 43;
        /// <summary>
        /// Vị trí bit chứa trạng thái cua SICK status
        /// </summary>
        public const int STATUS_BIT_20_SICK_STATUS = 20;
        public const int STATUS_BIT_20_SICK_STATUS_TRACKING = 44;
        /// <summary>
        ///  Vị trí bit chứa trạng thái sensor2 của card dispenser
        /// </summary>
        //public const int STATUS_BIT_21_CARD_SENSOR2 = 21;
        //public const int STATUS_BIT_21_CARD_SENSOR2_TRACKING = 45;

        #endregion
        #region BarrierPhuMy
        public const int STATUS_BIT_12_BARRIER_1_OPENMANUAL = 12;
        public const int STATUS_BIT_13_BARRIER_1_OPENAUTO = 13;
        public const int STATUS_BIT_14_BARRIER_2_OPENMANUAL = 14;
        public const int STATUS_BIT_15_BARRIER_2_OPENAUTO = 15;
        #endregion

        /// <summary>
        /// vi tri bit 0, cho nay xem lai
        /// </summary>
        public const int BIT_CAR_COUNTER = 0;
        /// <summary>
        /// vi tri bit 0, cho nay xem lai
        /// </summary>
        public const int BIT_TOTAL_CAR_COUNTER = 0;
        /// <summary>
        /// Dấu hiệu cho biết gói tin chứa trạng thái của PLC
        /// </summary>
        public const string PLC_STATUS_PACKAGE_INDICATOR = "1";
        /// <summary>
        /// Dau hieu cho biet goi tin nay do lenh GetPlc Status goi xuong plc
        /// </summary>
        public const string GET_PLC_STATUS_PACKAGE_INDICATOR = "2";
        /// <summary>
        /// Dấu hiệu cho biết gói tin chứa số xe đếm được từ PLC
        /// </summary>
        public const string CAR_COUNTER_PACKAGE_INDICATOR = "3";

        #region Tập mã lệnh giao tiếp với PLC
        /// <summary>
        /// Mã lệnh lấy số xe đếm đc từ PLC
        /// </summary>
        public const string CMD_GET_CAR_COUNTER = "o";
        /// <summary>
        /// Mã lệnh lấy trạng thái PLC
        /// </summary>
        public const string CMD_GET_PLC_STATUS = "@";
        /// <summary>
        /// Mã lệnh reset biến đếm số xe
        /// </summary>
        public const string CMD_RESET_CARCOUNTER = "0";
        /// <summary>
        /// Mã lệnh đóng barrier 1
        /// </summary>
        public const string CMD_CLOSE_BARRIER_1 = "+";
        /// <summary>
        /// Mã lệnh đóng barrier 2
        /// </summary>
        public const string CMD_CLOSE_BARRIER_2 = ">";
        /// <summary>
        /// Mã lệnh mở barrier 1 cho đòan xe
        /// </summary>
        public const string CMD_OPEN_BARRIER_1_MANUAL = "&";
        /// <summary>
        /// Mã lệnh mở barrier 2 cho đòan xe
        /// </summary>
        public const string CMD_OPEN_BARRIER_2_MANUAL = "=";
        /// <summary>
        /// chon reader A
        /// </summary>
        public const string CMD_SELECT_READER_A = "A";
        /// <summary>
        /// Chon reader B
        /// </summary>
        public const string CMD_SELECT_READER_B = "B";
        /// <summary>
        /// Mã lệnh mở barrier 1 cho 1 xe
        /// </summary>
        public const string CMD_OPEN_BARRIER_1_AUTO = "!";
        /// <summary>
        /// Mã lệnh mở barrier 2 cho 1 xe
        /// </summary>
        public const string CMD_OPEN_BARRIER_2_AUTO = "<";
        /// <summary>
        /// Mã lệnh đóng làn
        /// </summary>
        public const string CMD_CLOSE_LANE = "#";//???
        /// <summary>
        /// Mã lệnh mở làn
        /// </summary>
        public const string CMD_OPEN_LANE = "^";//???

        /// <summary>
        /// Mã lệnh lock he thong bao coi
        /// </summary>
        public const string CMD_LOCK_SOUND = "L";

        /// <summary>
        /// Mã lệnh unlock he thong tat coi
        /// </summary>
        public const string CMD_UNLOCK_SOUND = "U";

        #endregion

        #endregion
    }
}

