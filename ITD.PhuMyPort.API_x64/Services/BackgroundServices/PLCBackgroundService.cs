using ITD.PhuMyPort.API.Models;
using ITD.PhuMyPort.API.Services;
using ITD.PhuMyPort.Common;
using ITD.PhuMyPort.TCP;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ITD.PhuMyPort.API
{
    public class PLCBackgroundService : BackgroundService
    {
        /// <summary>
        /// PLC communication Socket Server
        /// </summary>
        IOptions<PLCSettings> plcSettings;
        /// <summary>
        /// plc control functions
        /// </summary>
        PLCServices pLCServices;
        // private PLCSocket pLCSocket;
        PLCServerManager pLCServerManager;// = new PLCServerManager("10.0.3.176", 2000, 2020, 2001);
        public PLCBackgroundService(IOptions<PLCSettings> plcSettings, PLCServices pLCServices)
        {
            this.plcSettings = plcSettings;
            this.pLCServices = pLCServices;
        }
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            if (plcSettings.Value != null)
            {
                pLCServerManager = new PLCServerManager(plcSettings.Value.IPAddress, plcSettings.Value.ReceiveStatusChangePort, plcSettings.Value.ReceiveStatusResultPort, plcSettings.Value.SendPort);
            }
            return base.StartAsync(cancellationToken);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (pLCServices.requuestQueue.Count > 0)
                {
                    PLCData pLCData = pLCServices.requuestQueue.Dequeue();
                    NLogHelper.Info("----------------------------------------------------------------------------------");
                    NLogHelper.Info("Processing PLC - IP: " + pLCData.PLC.IP +", ProcessType: "+ pLCData.ControllType.ToString()+", Barrier: "+pLCData.BarrierNo);
                    bool bR = false;
                    //1. get PLC status
                    bR = pLCServerManager.GetPlcStatus(pLCData.PLC.IP);// pLCSocket.GetPlcStatus();

                    if (bR)
                    {
                        if (pLCData.ControllType == ControllType.Open)
                            OpenBarrierProcess(pLCData);
                        else if (pLCData.ControllType == ControllType.Close)
                            CloseBarrierProcess(pLCData);
                    }

                    pLCServices.responseQueue.Add(pLCData.SequenceID, pLCData);
                    Thread.Sleep(1);
                }
                else
                {
                    Thread.Sleep(5);
                    await Task.Yield();
                }
            }
        }
        private void OpenBarrierProcess(PLCData pLCData)
        {
            int barrier = pLCData.BarrierNo;
            //2. get barrier status
            BarrierStatus barrierStatus = pLCServerManager.GetBarrierStatus(pLCData.PLC.IP, barrier);// pLCSocket.GetBarrierStatus(1);

            if (barrierStatus == BarrierStatus.OpenAuto || barrierStatus == BarrierStatus.OpenManual)
            {
                pLCData.Result = true;
            }
            else
            {
                //3. send open 
                if (pLCData.ControllType == ControllType.Open)
                    pLCServerManager.OpenBarrier(barrier, pLCData.PLC.IP);

                int timeout = 1000;
                do
                {
                    //check barrier status
                    barrierStatus = pLCServerManager.GetBarrierStatus(pLCData.PLC.IP, barrier); ;// pLCSocket.GetBarrierStatus(1);
                    timeout -= 1;
                    Thread.Sleep(1);
                }
                while ((!(barrierStatus == BarrierStatus.OpenAuto || barrierStatus == BarrierStatus.OpenManual)) && timeout > 0);

                //4. set result
                if (timeout > 0)   //success
                {
                    pLCData.Result = true;
                }
                else
                {
                    pLCData.Result = false;
                }
            }
        }

        private void CloseBarrierProcess(PLCData pLCData)
        {
            int barrier = pLCData.BarrierNo;
            //2. get barrier status
            BarrierStatus barrierStatus = pLCServerManager.GetBarrierStatus(pLCData.PLC.IP, barrier);// pLCSocket.GetBarrierStatus(1);

            if (barrierStatus == BarrierStatus.Close)
            {
                pLCData.Result = true;
            }
            else
            {
                //3. send open 
                if (pLCData.ControllType == ControllType.Close)
                    pLCServerManager.CloseBarrier(pLCData.PLC.IP, barrier);

                int timeout = 1000;
                do
                {
                    //check barrier status
                    barrierStatus = pLCServerManager.GetBarrierStatus(pLCData.PLC.IP, barrier); ;// pLCSocket.GetBarrierStatus(1);
                    timeout -= 1;
                    Thread.Sleep(1);
                }
                while ((!(barrierStatus == BarrierStatus.Close) && timeout > 0));

                //4. set result
                if (timeout > 0)   //success
                {
                    pLCData.Result = true;
                }
                else
                {
                    pLCData.Result = false;
                }
            }
        }
    }
}
