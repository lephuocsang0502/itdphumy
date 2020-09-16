using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITD.PhuMyPort.API.Models
{
    public class PLCSettings
    {
        /// <summary>
        /// port to receive plc status change
        /// </summary>
        public int ReceiveStatusChangePort { get; set; }
        /// <summary>
        /// port to receive plc status reponse
        /// </summary>
        public int ReceiveStatusResultPort { get; set; }
        /// <summary>
        /// port to send control
        /// </summary>
        public int SendPort { get; set; }
        /// <summary>
        /// IP to host
        /// </summary>
        public string IPAddress { get; set; }
    }
}
