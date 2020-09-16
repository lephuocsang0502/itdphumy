using ITD.PhuMyPort.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITD.PhuMyPort.API.Models
{
    public enum ControllType
    {
        Open = 0,
        Close = 1
    }
    public class PLCData
    {
        /// <summary>
        /// thứ tự lệnh
        /// </summary>
        public int SequenceID { get; set; }
        /// <summary>
        /// kết quả xử lý
        /// </summary>
        public bool Result { get; set; }
        /// <summary>
        /// mã định danh PLC
        /// </summary>
        public PLC PLC { get; set; }
        /// <summary>
        /// loại điều khiển
        /// </summary>
        public ControllType ControllType { get; set; }
        public int BarrierNo { get; set; }
    }
}
