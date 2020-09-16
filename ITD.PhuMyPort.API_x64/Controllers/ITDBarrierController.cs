using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ITD.PhuMyPort.API.Models;
using ITD.PhuMyPort.API.Services;
using ITD.PhuMyPort.Common;
using ITD.PhuMyPort.DataAccess.Data;
using ITD.PhuMyPort.DataAccess.Models;
using Microsoft.AspNetCore.Mvc;

namespace ITD.PhuMyPort.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ITDBarrierController : ControllerBase
    {
        private readonly ConfigWebContext _context;

        ///// <summary>
        ///// thời gian tối đa xử lý
        ///// </summary>
        private int openBarrierTimeout = 2000;
        // private static PLCSocket pLCSocket;
        private PLCServices pLCServices;
        public ITDBarrierController(ConfigWebContext context, PLCServices pLCServices)
        {
            _context = context;
            this.pLCServices = pLCServices;
        }

        private void PLCSocket_OnStatusPLCPakageReceived(object sender, EventArgs e)
        {
            // throw new NotImplementedException();
        }

        [HttpPost("BarrierOn/{WorkplaceCode}", Name = "BarrierOn")]
        public PLCResponse BarrierOn(string WorkplaceCode)
        {
            PLCResponse response = new PLCResponse()
            {
                ErrorCode = 0
            };

            NLogHelper.Info("BarrierOn - WorkplaceCode: " + WorkplaceCode);
            //1. get barrier info
            int barrier = 0;
            PLC plc = _context.PLCs.Where(p => p.WorkplaceCode == WorkplaceCode).Take(1).FirstOrDefault();
            if (plc != null)
            {
                int.TryParse(plc.Barrier, out barrier);
                //2. open barrier auto
                int iR = OpenBarrier(plc, barrier).Result;

                //0: success, 1: timeout, 2: failed
                response.Message = iR == 0 ? "Success" : (iR == 1 ? "Timeout" : "Failed");
            }
            return response;
        }

        [HttpPost("BarrierOff/{WorkplaceCode}", Name = "BarrierOff")]
        public PLCResponse BarrierOff(string WorkplaceCode)
        {
            PLCResponse response = new PLCResponse()
            {
                ErrorCode = 0
            };
            //1. get barrier info                                            
            NLogHelper.Info("BarrierOff - WorkplaceCode: " + WorkplaceCode);
            int barrier = 0;
            PLC plc = _context.PLCs.Where(p => p.WorkplaceCode == WorkplaceCode).Take(1).FirstOrDefault();
            if (plc != null)
            {
                int.TryParse(plc.Barrier, out barrier);
                //2. open barrier auto
                int iR = CloseBarrier(plc, barrier).Result;

                //0: success, 1: timeout, 2: failed
                response.Message = iR == 0 ? "Success" : (iR == 1 ? "Timeout" : "Failed");
            }
            return response;
        }

        static bool threadStarted = false;
        static int _squenceId = 0;
        private async Task<int> OpenBarrier(PLC pLC, int barrier)
        {
            //success
            int rs = 0;
            try
            {
                //1. add request to queue
                PLCData plcData = new PLCData()
                {
                    SequenceID = _squenceId < int.MaxValue ? _squenceId++ : 0,
                    PLC = pLC,
                    ControllType = ControllType.Open,
                    BarrierNo = barrier
                };
                pLCServices.requuestQueue.Enqueue(plcData);

                //2. wait until done or timeout
                int sequenceId = plcData.SequenceID;
                int timeout = openBarrierTimeout;//ms
                while (timeout > 0)//responseQueue.ContainsKey(sequenceId)
                {
                    //try to get data from result list
                    if (pLCServices.responseQueue.TryGetValue(sequenceId, out plcData))
                    {
                        //found
                        rs = plcData.Result ? 0 : 2;
                        pLCServices.responseQueue.Remove(sequenceId);
                        break;
                    }
                    timeout -= 10;
                    Thread.Sleep(10);
                }
                if (timeout <= 0)
                {
                    rs = 1;
                    NLogHelper.Info("Send Open barrier timeout, SequenceId: " + sequenceId);
                }
                else
                {
                    NLogHelper.Info("Send Open barrier success - SequenceId: " + sequenceId + ", Result: " + rs);
                }
            }
            catch (Exception ex)
            {
                NLogHelper.Error(ex);
                NLogHelper.Info(ex.Message);
            }
            return rs;
        }
        private async Task<int> CloseBarrier(PLC pLC, int barrier)
        {
            //success
            int rs = 0;
            try
            {
                //1. add request to queue
                PLCData plcData = new PLCData()
                {
                    SequenceID = _squenceId < int.MaxValue ? _squenceId++ : 0,
                    PLC = pLC,
                    ControllType = ControllType.Close,
                    BarrierNo = barrier
                };
                pLCServices.requuestQueue.Enqueue(plcData);

                //2. wait until done or timeout
                int sequenceId = plcData.SequenceID;
                int timeout = openBarrierTimeout;//ms
                while (timeout > 0)//responseQueue.ContainsKey(sequenceId)
                {
                    //try to get data from result list
                    if (pLCServices.responseQueue.TryGetValue(sequenceId, out plcData))
                    {
                        //found
                        rs = plcData.Result ? 0 : 2;
                        pLCServices.responseQueue.Remove(sequenceId);
                        break;
                    }
                    timeout -= 10;
                    Thread.Sleep(10);
                }
                if (timeout <= 0)
                {
                    rs = 1;
                    NLogHelper.Info("Send Close barrier timeout, SequenceId: " + sequenceId);
                }
                else
                {
                    NLogHelper.Info("Send Close barrier success - SequenceId: " + sequenceId + ", Result: " + rs);
                }
            }
            catch (Exception ex)
            {
                NLogHelper.Error(ex);
                NLogHelper.Info(ex.Message);
            }
            return rs;
        }
    }
}
