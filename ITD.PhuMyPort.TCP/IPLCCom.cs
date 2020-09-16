using System;
using System.Collections.Generic;
using System.Text;

namespace ITD.PhuMyPort.TCP
{
    /// <summary>
    /// Loai loop
    /// </summary>
    public enum LoopTypeCarPark
    {
        /// <summary>
        /// Loop A1 (Loop xe ra)
        /// </summary>
        A1,
        A2,
        /// <summary>
        /// Loop B1 (loop close barrier)
        /// </summary>
        B1,
        B2,
        /// <summary>
        /// Loop C (Loop open barrier)
        /// </summary>
        C1,
        C2

    }

    public enum BarrierTypeCarPark
    {
        /// <summary>
        /// Loop A1 (Loop xe ra)
        /// </summary>
        B1,
        B2
    }

    /// <summary>
    /// Trang thai barrier
    /// </summary>
    public enum BarrierStatus
    {
        /// <summary>
        /// Barier dang mo tu dong dong
        /// </summary>
        OpenAuto,
        /// <summary>
        /// Barrier dang mo , ko tu dong dong
        /// </summary>
        OpenManual,
        /// <summary>
        /// Barier dong
        /// </summary>
        Close
    }

    public enum LoopStatus
    {
        /// <summary>
        /// Co xe trong loop
        /// </summary>
        Pressed,
        /// <summary>
        /// Khong co xe trong loop
        /// </summary>
        NonPressed
    }

    public enum LaneStatus
    {
        Open,
        Close,
    }

    public enum CardDispenserSensorStatus
    {
        HasCard,
        NoCard
    }
    /// <summary>
    /// Trang thai nut nhan ban phim co
    /// </summary>
    public enum ButtonStatus
    {
        Press,
        NonPress
    }
    /// <summary>
    /// trang thai cua 1 reader:active/standby
    /// </summary>
    public enum RFID_READER_STATUS
    {
        Active,
        Standby
    }

    /// <summary>
    /// 
    /// </summary>
    public enum RFID_Reader
    {
        A,
        B
    }
    /// <summary>
    /// cho biet reader nao dang duoc chon( kich hoat tu plc)
    /// </summary>
    public enum SELECTED_RFID_READER
    {
        A,
        B
    }
    /// <summary>
    /// trang thai giua reader va plc
    /// 1 plc co ket noi toi reader,0 mat ket noi
    /// </summary>
    public enum RFID_READER_PLC_STATUS
    {
        Connected,
        Disconnected
    }
    /// <summary>
    /// trang thai coi
    /// </summary>
    public enum ALARM_HORN_STATUS
    {
        On,
        Off
    }
    /// <summary>
    /// Loai loop
    /// </summary>
    public enum LoopType
    {
        /// <summary>
        /// Loop A (Loop xe ra)
        /// </summary>
        A,
        /// <summary>
        /// Loop B (Loop coi)
        /// </summary>
        B,
        /// <summary>
        /// Loop C (Loop nhan dang)
        /// </summary>
        C,

        /// <summary>
        /// Loop D ( vong tu duoi sick)
        /// </summary>
        D,
        /// <summary>
        /// (Loop lock cu)
        /// </summary>
        Lock,
        /// <summary>
        /// Loop E (Loop lock moi bo sung)
        /// </summary>
        E,
        /// <summary>
        /// Loop F (Loop nhan dang ETC)
        /// </summary>
        F,
        /// <summary>
        /// trang thai cua SICk
        /// </summary>
        SICK
        /// <summary>
        /// Loop bao ket qua lock
        /// </summary>
        //  Infrared1,
        /// <summary>
        /// Cot hong ngoai 2
        /// </summary>
        // Infrared2,
        /// <summary>
        /// Card Dispenser 1
        /// </summary>
        //   CardDispenser1,
        /// <summary>
        /// CardDispenser 2
        /// </summary>
        // CardDispenser2,
        //RRFID_READER_A,
        //RRFID_READER_B,
        //RRFID_SELETED_READER,
        //RRFID_STATUS_OF_READER
    }

    /// <summary>
    /// Nut bam ban phim co
    /// </summary>
    public enum ButtonType
    {
        /// <summary>
        /// Nut mo uu tien 1 xe
        /// </summary>
        Green,
        /// <summary>
        /// Nut mo uu tien doan xe
        /// </summary>
        Blue,
        /// <summary>
        /// Nut dong barrier
        /// </summary>
        Red
    }

    /// <summary>
    /// Interface cho việc giao tiếp với PLC điều khiển thiết bị
    /// </summary>

    public interface IPLCCom
    {
        #region Methods

        /// <summary>
        /// Lay trang thai cua Plc
        /// </summary>
        /// <returns></returns>
        bool GetPlcStatus();

        /// <summary>
        /// 
        /// </summary>
        BarrierStatus GetBarrierStatus(int barrier);

        /// <summary>
        /// 
        /// </summary>
        LaneStatus GetLaneStatus();


        /// <summary>
        /// Lệnh mở barrier bình thường, xe qua barrier tự đóng
        /// </summary>
        /// <returns></returns>
        bool OpenBarrierAuto(int barrier);

        /// <summary>
        /// Lenh mo barrier cho doan xe, barrier ko tu don'g
        /// </summary>
        /// <returns></returns>
        bool OpenBarrierManual(int barrier);

        /// <summary>
        /// Yêu cầu PLC ra lệnh đóng barrier.
        /// </summary>
        /// <returns></returns>
        bool CloseBarrier(int barrier);
        #endregion
        #region Event
        /// <summary>
        /// Sự kiện xảy ra khi có các lệnh trả về gói tin mặc định ("!"; "&"; "+"; "0")
        /// </summary>
        event EventHandler<EventArgs> OnStatusPLCPakageReceived;
        event EventHandler<EventArgs> OnCarCounterPakageReceived;
        #endregion
    }
}
