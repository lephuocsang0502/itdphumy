using System;
using System.Collections.Generic;
using System.Text;

namespace ITD.PhuMyPort.TCP
{
    /// <summary>
    /// quản lý kết nối nhiều PLC
    /// </summary>
    public class PLCServerManager
    {
        /// <summary>
        /// server port 2000 nhận tín hiệu thay đổi trạng thái từ PLC
        /// </summary>
        Server _serverReceiveStatusChange;
        /// <summary>
        /// server port 2020 nhận tín hiệu kết quả request tới PLC
        /// </summary>
        Server _serverReceiveStatusResult;
        /// <summary>
        /// server port 2001 gửi tín hiệu lệnh tới PLC
        /// </summary>
        Server _serverSendCommand;

        /// <summary>
        /// port mặc định nhận tín hiệu thay đổi trạng thái từ PLC
        /// </summary>
        int _receiveStatusChangePort = 2000;
        /// <summary>
        /// port mặc định nhận tín hiệu kết quả request tới PLC
        /// </summary>
        int _receiveStatusResultPort = 2020;
        /// <summary>
        /// port mặc định gửi tín hiệu lệnh tới PLC
        /// </summary>
        int _sendCommandPort = 2001;

        /// <summary>
        /// các PLC đã kết nối vào hệ thống
        /// </summary>
        public static Dictionary<string, PLCClient> clients = new Dictionary<string, PLCClient>();

        /// <summary>
        /// khời tạo quản lý nhiều PLC
        /// </summary>
        /// <param name="ipHost"></param>
        /// <param name="receiveStatusChangePort"></param>
        /// <param name="receiveStatusResultPort"></param>
        /// <param name="sendCommandPort"></param>
        public PLCServerManager(string ipHost, int receiveStatusChangePort, int receiveStatusResultPort, int sendCommandPort)
        {
            _receiveStatusChangePort = receiveStatusChangePort;
            _receiveStatusResultPort = receiveStatusResultPort;
            _sendCommandPort = sendCommandPort;

            //server nhận thay đổi trạng thái PLC  port 2000
            _serverReceiveStatusChange = new Server(ipHost, _receiveStatusChangePort, ServerType.ReceiveStatusChange);

            //server nhận phản hổi thay đổi trạng thái PLC khi gửi lệnh port 2020
            _serverReceiveStatusResult = new Server(ipHost, _receiveStatusResultPort, ServerType.ReceiveStatusResult);

            //server gửi lệnh tới PLC port 2001
            _serverSendCommand = new Server(ipHost, _sendCommandPort, ServerType.SendCommand);
        }
        /// <summary>
        /// mở barrier tự động
        /// </summary>
        /// <param name="barrier"></param>
        /// <param name="ipaddress"></param>
        /// <returns></returns>
        public bool OpenBarrier(int barrier, string ipaddress)
        {
            if (clients.ContainsKey(ipaddress))
            {
                lock (clients)
                {
                    var client = clients[ipaddress];
                    if (client != null)
                    {
                        return client.OpenBarrierAuto(barrier);
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// lấy trạng thái PLC
        /// </summary>
        /// <param name="ipaddress"></param>
        /// <returns></returns>
        public bool GetPlcStatus(string ipaddress)
        {
            if (clients.ContainsKey(ipaddress))
            {
                lock (clients)
                {
                    var client = clients[ipaddress];
                    if (client != null)
                    {
                        return client.GetPlcStatus();
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// lấy trạng thái barrier
        /// </summary>
        /// <param name="ipaddress"></param>
        /// <param name="barrier"></param>
        /// <returns></returns>
        public BarrierStatus GetBarrierStatus(string ipaddress, int barrier)
        {
            if (clients.ContainsKey(ipaddress))
            {
                lock (clients)
                {
                    var client = clients[ipaddress];
                    if (client != null)
                    {
                        return client.GetBarrierStatus(barrier);
                    }
                }
            }
            return BarrierStatus.Close;
        }
        /// <summary>
        /// đóng barrier
        /// </summary>
        /// <param name="ipaddress"></param>
        /// <param name="barrier"></param>
        /// <returns></returns>
        public bool CloseBarrier(string ipaddress, int barrier)
        {
            if (clients.ContainsKey(ipaddress))
            {
                lock (clients)
                {
                    var client = clients[ipaddress];
                    if (client != null)
                    {
                        return client.CloseBarrier(barrier);
                    }
                }
            }
            return false;
        }
    }
}
