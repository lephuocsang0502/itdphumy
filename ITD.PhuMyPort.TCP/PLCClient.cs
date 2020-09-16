using ITD.PhuMyPort.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace ITD.PhuMyPort.TCP
{
    public class PLCClient : IPLCCom
    {
        //events 
        public event EventHandler<EventArgs> OnStatusPLCPakageReceived;
        public event EventHandler<EventArgs> OnStatusPLCPakageReceivedStatus;
        public event EventHandler<EventArgs> OnCarCounterPakageReceived;

        string ipaddress;
        /// <summary>
        /// tcp client
        /// </summary>
        TcpClient _receiveStatusChangeClient;
        TcpClient _receiveStatusResultClient;
        TcpClient _sendCommandClient;
        /// <summary>
        /// Gói tin chứa trạng thái của PLC khi có thay đổi
        /// </summary>
        private BitArray mPlcStatusPackage = new BitArray(Consts.BIT_NUMBER);
        /// <summary>
        /// Gói chứa thông tin trạng thái khi gửi lệnh
        /// </summary>
        private BitArray mPlcStatusPackageStatus = new BitArray(Consts.BIT_NUMBER);
        /// <summary>
        /// PLC client IP Address
        /// </summary>
        public string IPAddress { get => ipaddress; set => ipaddress = value; }
        public TcpClient ReceiveStatusChangeClient { get => _receiveStatusChangeClient; set => _receiveStatusChangeClient = value; }
        public BitArray MPlcStatusPackage { get => mPlcStatusPackage; set => mPlcStatusPackage = value; }
        public BitArray MPlcStatusPackageStatus { get => mPlcStatusPackageStatus; set => mPlcStatusPackageStatus = value; }
        public TcpClient ReceiveStatusResultClient { get => _receiveStatusResultClient; set => _receiveStatusResultClient = value; }
        public TcpClient SendCommandClient { get => _sendCommandClient; set => _sendCommandClient = value; }

        byte[] tempbufferCP = new byte[Consts.TEMP_PACKAGE_LEN];

        byte[] tempbufferStatus = new byte[Consts.TEMP_PACKAGE_LEN];

        public void StartListeningCP()
        {
            try
            {
                ReceiveStatusChangeClient.GetStream().BeginRead(tempbufferCP, 0, Consts.TEMP_PACKAGE_LEN, StreamReceivedCP, null);
            }
            catch (Exception ex)
            {
                NLogHelper.Info(ex.Message);
                NLogHelper.Error(ex);
                //Utility.WriteLogFile(this.GetType() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        public void StartListeningStatus()
        {
            try
            {
                ReceiveStatusResultClient.GetStream().BeginRead(tempbufferStatus, 0, Consts.TEMP_PACKAGE_LEN, StreamReceivedStatus, null);
            }
            catch (Exception ex1)
            {
                NLogHelper.Info(ex1.Message);
                NLogHelper.Error(ex1); // Utility.WriteLogFile(this.GetType() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, ex1.Message);
            }
        }
        private void StreamReceivedStatus(IAsyncResult result)
        {
            int bytes = 0;
            try
            {
                lock (ReceiveStatusResultClient.GetStream())
                {
                    bytes = ReceiveStatusResultClient.GetStream().EndRead(result);
                }
                if (bytes > 8)
                {
                    bytes = bytes + 1;
                }
                //Utility.WriteLogFile("Log PLC", string.Format("Data received: {0} bytes - '{1}'", bytes, BitConverter.ToString(tempbuffer)));
                byte[] buffer = new byte[Consts.PACKAGE_LEN];

                if (bytes >= 8)
                {//neu >=8 => lay 8 byte cuoi 
                    Buffer.BlockCopy(tempbufferStatus, tempbufferStatus.Length - Consts.PACKAGE_LEN, buffer, 0, Consts.PACKAGE_LEN);
                }
                else
                {
                    return;
                }
                //Utility.WriteLogFile("", "data plc:" + BitConverter.ToString(buffer));
                bool bCheckSum = CheckSum(buffer, bytes);
                //Kiểm tra gói tin loại trạng thái hay số xe
                string txtIndicator = Encoding.UTF8.GetString(buffer, 7, 1);//7??
                //Neu ket qua ko phai la goi tin lay so luong xe-> no' la goi tin trang thai plc
                if (txtIndicator == Consts.GET_PLC_STATUS_PACKAGE_INDICATOR)//Huy 28May 2016 tai sao ko goi lenh get plc status ma plc tra ve cho nay hoai vay
                {
                    if (bCheckSum)
                    {
                        MPlcStatusPackage = new BitArray(buffer);
                        // _mPlcStatusPackageStatus = new BitArray(buffer);
                        if (OnStatusPLCPakageReceivedStatus != null)
                            OnStatusPLCPakageReceivedStatus(this, null);
                    }
                    else
                    {
                        //Neu goi tin bi loi checksum->goi yeu cau lay lai goi tin plc status
                        // get len cung ko dung duoc 20-03-2017
                        //this.GetPlcStatus();//??
                        //Utility.WriteLogFile("COM PLC.GET_PLC_STATUS_PACKAGE_INDICATOR", "CheckSum fail");
                    }
                }
                else
                {
                    //Utility.WriteLogFile("Strange Package:", string.Format("Data received: {0} bytes - '{1}'", bytes, BitConverter.ToString(buffer)));
                    return;
                }
            }
            catch (Exception ex)
            {     
                NLogHelper.Info(ex.Message);
                NLogHelper.Error(ex);
                //Utility.WriteLogFile(this.GetType() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message);//this.GetType()+"PLC Constructor", ex.Message);
            }
            finally
            {
                StartListeningStatus();
            }
        }
        private void StreamReceivedCP(IAsyncResult result)
        {
            int bytes = 0;
            try
            {
                lock (ReceiveStatusChangeClient.GetStream())
                {
                    bytes = ReceiveStatusChangeClient.GetStream().EndRead(result);
                }
                if (bytes > 8)
                {
                    bytes = bytes + 1;
                }
                //Utility.WriteLogFile("Log PLC", string.Format("Data received: {0} bytes - '{1}'", bytes, BitConverter.ToString(tempbuffer)));
                byte[] buffer = new byte[Consts.PACKAGE_LEN];

                if (bytes >= 8)
                {//neu >=8 => lay 8 byte cuoi 
                    Buffer.BlockCopy(tempbufferCP, tempbufferCP.Length - Consts.PACKAGE_LEN, buffer, 0, Consts.PACKAGE_LEN);
                }
                else
                {
                    return;
                }
                //Utility.WriteLogFile("", "data plc:" + BitConverter.ToString(buffer));
                bool bCheckSum = CheckSum(buffer, bytes);
                //Kiểm tra gói tin loại trạng thái hay số xe
                string txtIndicator = Encoding.UTF8.GetString(buffer, 7, 1);//7??
                //Neu ket qua ko phai la goi tin lay so luong xe-> no' la goi tin trang thai plc
                if (txtIndicator == Consts.PLC_STATUS_PACKAGE_INDICATOR)
                {
                    if (bCheckSum)
                    {
                        MPlcStatusPackage = new BitArray(buffer);
                        if (OnStatusPLCPakageReceived != null)
                            OnStatusPLCPakageReceived(this, null);
                    }
                    else
                    {
                        //Neu goi tin bi loi checksum->goi yeu cau lay lai goi tin plc status
                        // Khi nhan status tu PLC neu loi thi thoi, Gui Plc Status thi cung ko cap nhat duoc _mPlcStatusPackage. 20-03-2017
                        //this.GetPlcStatus();//??
                        // Utility.WriteLogFile("COM PLC.PLC_STATUS_PACKAGE_INDICATOR", "CheckSum fail");
                    }
                }

                else if (txtIndicator == Consts.GET_PLC_STATUS_PACKAGE_INDICATOR)//Huy 28May 2016 tai sao ko goi lenh get plc status ma plc tra ve cho nay hoai vay
                {
                    if (bCheckSum)
                    {
                        MPlcStatusPackage = new BitArray(buffer);
                        if (OnStatusPLCPakageReceived != null)
                            OnStatusPLCPakageReceived(this, null);//Huy 28 May 2016
                    }
                    else
                    {
                        //Neu goi tin bi loi checksum->goi yeu cau lay lai goi tin plc status
                        // Khi nhan status tu PLC neu loi thi thoi, Gui Plc Status thi cung ko cap nhat duoc _mPlcStatusPackage. 20-03-2017
                        //this.GetPlcStatus();//??
                        // Utility.WriteLogFile("COM PLC.GET_PLC_STATUS_PACKAGE_INDICATOR", "CheckSum fail");
                    }
                }
                else
                {
                    //Utility.WriteLogFile("Strange Package:", string.Format("Data received: {0} bytes - '{1}'", bytes, BitConverter.ToString(buffer)));
                    return;
                }
            }
            catch (Exception ex)
            {

                NLogHelper.Info(ex.Message);
                NLogHelper.Error(ex);
                //Utility.WriteLogFile(this.GetType() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message);//this.GetType()+"PLC Constructor", ex.Message);
            }
            finally
            {
                StartListeningCP();
            }
        }
        /// <summary>
        /// Checksum goi tin nhan dc
        /// </summary>
        /// <param name="pBuffer"></param>
        /// <param name="pLen"></param>
        /// <returns></returns>
        private bool CheckSum(byte[] pBuffer, int pLen)
        {
            try
            {
                if (pBuffer == null || pBuffer.Length < 7) return false;
                byte byte1 = pBuffer[0];
                byte byte2 = pBuffer[1];
                byte byte3 = pBuffer[2];
                byte byte4 = pBuffer[3];
                byte byte5 = pBuffer[4];
                byte byte6 = pBuffer[5];
                byte byte7 = pBuffer[6];
                byte checksum = (byte)(byte1 ^ byte2 ^ byte3 ^ byte4 ^ byte5 ^ byte6);

                if (byte7 == checksum)//sua lai ngay 2/1/2013 cho plc moi
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {

                NLogHelper.Info(ex.Message);
                NLogHelper.Error(ex);
                // Utility.WriteLogFile(this.GetType() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message);//this.GetType()+"PLC Constructor", ex.Message);
                return false;
            }
        }
        public bool GetPlcStatus()
        {
            try
            {
                SendCommandCP(Consts.CMD_GET_PLC_STATUS, Consts.DelayTime);
                return true;
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {

                    NLogHelper.Info(ex.Message);
                    NLogHelper.Error(ex);//Utility.WriteLogFile(this.GetType() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message + ".InnerMessage:" + ex.InnerException.Message);
                }
                else
                {
                    NLogHelper.Info(ex.Message);
                    NLogHelper.Error(ex);//Utility.WriteLogFile(this.GetType() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message);
                }
            }
            return false;
        }

        public BarrierStatus GetBarrierStatus(int barrier)
        {
            BarrierStatus barierStatus = BarrierStatus.Close;
            // GetPlcStatus();
            try
            {
                if (MPlcStatusPackage != null)
                {
                    /* Kiem tra barier mo hay dong
                     * Neu mo thi kiem tra dang mo uu tien doan hay ko
                     */
                    if (barrier == 1)
                    {
                        if (MPlcStatusPackage[Consts.STATUS_BIT_13_BARRIER_1_OPENAUTO] == true)
                        {
                            if (MPlcStatusPackage[Consts.STATUS_BIT_12_BARRIER_1_OPENMANUAL] == true)
                            {
                                barierStatus = BarrierStatus.OpenManual;
                            }
                            else
                            {
                                barierStatus = BarrierStatus.OpenAuto;
                            }
                        }
                        else
                        {
                            return barierStatus;
                        }
                    }
                    else
                    {
                        if (MPlcStatusPackage[Consts.STATUS_BIT_15_BARRIER_2_OPENAUTO] == true)
                        {
                            if (MPlcStatusPackage[Consts.STATUS_BIT_14_BARRIER_2_OPENMANUAL] == true)
                            {
                                barierStatus = BarrierStatus.OpenManual;
                            }
                            else
                            {
                                barierStatus = BarrierStatus.OpenAuto;
                            }
                        }
                        else
                        {
                            return barierStatus;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NLogHelper.Info(ex.Message);
                NLogHelper.Error(ex);
                //Utility.WriteLogFile(this.GetType() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message);//this.GetType()+"PLC Constructor", ex.Message);
            }
            return barierStatus;
        }

        public LaneStatus GetLaneStatus()
        {
            return LaneStatus.Close;
            //throw new NotImplementedException();
        }

        public bool OpenBarrierAuto(int barrier)
        {
            try
            {
                if (barrier == 1)
                    SendCommandCP(Consts.CMD_OPEN_BARRIER_1_AUTO, Consts.DelayTime);
                else
                    SendCommandCP(Consts.CMD_OPEN_BARRIER_2_AUTO, Consts.DelayTime);

                return true;
            }
            catch (Exception ex)
            {
                NLogHelper.Info(ex.Message);
                NLogHelper.Error(ex);
                //Utility.WriteLogFile(this.GetType() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message);//this.GetType()+"PLC Constructor", ex.Message);
            }
            return false;
        }
        public bool OpenBarrierAuto(int barrier, int nRetry = 1)
        {
            try
            {
                for (int i = 0; i < nRetry; i++)
                {
                    if (barrier == 1)
                        SendCommandCP(Consts.CMD_OPEN_BARRIER_1_AUTO, Consts.DelayTime);
                    else
                        SendCommandCP(Consts.CMD_OPEN_BARRIER_2_AUTO, Consts.DelayTime);
                }
                return true;
            }
            catch (Exception ex)
            {
                NLogHelper.Info(ex.Message);
                NLogHelper.Error(ex);
                //Utility.WriteLogFile(this.GetType() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message);//this.GetType()+"PLC Constructor", ex.Message);
            }
            return false;
        }

        public bool OpenBarrierManual(int barrier)
        {
            try
            {
                SendCommandCP(Consts.CMD_OPEN_BARRIER_1_MANUAL, Consts.DelayTime);
                return true;
            }
            catch (Exception ex)
            {
                NLogHelper.Info(ex.Message);
                NLogHelper.Error(ex);
                //Utility.WriteLogFile(this.GetType() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message);//this.GetType()+"PLC Constructor", ex.Message);
            }
            return false;
        }

        public bool CloseBarrier(int barrier)
        {
            try
            {
                if (barrier == 1)
                    SendCommandCP(Consts.CMD_CLOSE_BARRIER_1, Consts.DelayTime);
                else
                    SendCommandCP(Consts.CMD_CLOSE_BARRIER_2, Consts.DelayTime);
                return true;
            }
            catch (Exception ex)
            {
                NLogHelper.Info(ex.Message);
                NLogHelper.Error(ex);
                // Utility.WriteLogFile(this.GetType() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message);//this.GetType()+"PLC Constructor", ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Ham goi lenh ra cong com
        /// </summary>
        /// <param name="Cmd"></param>
        /// <param name="Delay"></param>        
        private void SendCommandCP(string Cmd, int Delay)
        {
            try
            {
                int test = 5;
                if (Delay > 0)
                {
                    test = Delay;
                }
                //Utility.WriteLogFile(this.GetType() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "Step1");
                if (SendCommandClient.GetStream().CanWrite)
                {
                    //_mSerialPort.Write(Cmd);
                    byte[] data = Encoding.ASCII.GetBytes(Cmd);
                    //Utility.WriteLogFile(this.GetType() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "Step2");
                    this.SendDataCP(data);
                    //Utility.WriteLogFile(this.GetType() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "Step3");

                    //Utility.WriteLogFile(this.GetType() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, "Step3");
                    System.Threading.Thread.Sleep(test);
                    //countReSend = 1;
                }
            }
            catch (Exception ex)
            {
                NLogHelper.Info(ex.Message);
                NLogHelper.Error(ex);
            }
        }
        private void SendDataCP(byte[] data)
        {
            //Try to send the data.  If an exception is thrown, disconnect the client
            try
            {
                lock (SendCommandClient.GetStream())
                {
                    SendCommandClient.GetStream().BeginWrite(data, 0, data.Length, StreamSentCP, SendCommandClient.GetStream());
                }
            }
            catch (Exception ex)
            {
                NLogHelper.Info(ex.Message);
                NLogHelper.Error(ex);
                //Utility.WriteLogFile(this.GetType() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
        private void StreamSentCP(IAsyncResult result)
        {
            try
            {
                SendCommandClient.GetStream().EndWrite(result);
            }
            catch (Exception ex)
            {
                NLogHelper.Info(ex.Message);
                NLogHelper.Error(ex);
                //Utility.WriteLogFile(this.GetType() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
    }
}
