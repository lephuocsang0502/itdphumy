using ITD.PhuMyPort.Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ITD.PhuMyPort.TCP
{
    public enum ServerType
    {
        ReceiveStatusChange = 0,  //listening port 2000
        ReceiveStatusResult = 1, //listening port 2020
        SendCommand = 2          //listening port 2001
    }
    public class Server
    {
        TcpListener server = null; ServerType serverType;
        int port = 2000;
        public Server(string ip, int port, ServerType serverType)
        {
            this.serverType = serverType;
            this.port = port;
            try
            {
                IPAddress localAddr = IPAddress.Parse(ip);
                server = new TcpListener(localAddr, port);
                server.Start();
                Thread t = new Thread(StartListener) { IsBackground = true };
                t.Start();
            }
            catch (Exception ex)
            {
                NLogHelper.Error(ex);
                NLogHelper.Info("Cannot start host: " + ip + ":" + port);
            }
        }
        public void StartListener()
        {
            try
            {
                while (true)
                {
                    Console.WriteLine("Waiting for a connection...");
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Connected!");
                    Thread t = new Thread(new ParameterizedThreadStart(HandleDeivce)) { IsBackground = true };
                    t.Start(client);

                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
                NLogHelper.Info(string.Format("SocketException: {0}", e.Message));
                NLogHelper.Error(e);
                server.Stop();
            }
        }
        public void HandleDeivce(Object obj)
        {
            //1. get tcp client
            TcpClient client = (TcpClient)obj;
            try
            {
                string ipaddress = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                PLCClient pLCClient = null;
                if (PLCServerManager.clients.ContainsKey(ipaddress))
                {
                    pLCClient = PLCServerManager.clients[ipaddress];
                }
                else
                {
                    pLCClient = new PLCClient()
                    {
                        IPAddress = client.Client.RemoteEndPoint.ToString()
                    };
                    //2. add TCP to list
                    PLCServerManager.clients.Add(ipaddress, pLCClient);
                }


                if (serverType == ServerType.ReceiveStatusChange)
                {
                    pLCClient.ReceiveStatusChangeClient = client;
                    pLCClient.StartListeningCP();
                }
                else if (serverType == ServerType.ReceiveStatusResult)
                {
                    pLCClient.ReceiveStatusResultClient = client;
                    pLCClient.StartListeningStatus();
                }
                else if (serverType == ServerType.SendCommand)
                {
                    pLCClient.SendCommandClient = client;
                }
                PLCServerManager.clients[ipaddress] = pLCClient;
                NLogHelper.Info("PLC Connected to port " + port + "IP: " + ipaddress);
            }
            catch (Exception ex1)
            {
                Console.WriteLine("Exception: {0}", ex1.ToString());
                NLogHelper.Info(string.Format("SocketException: {0}", ex1.Message));
                NLogHelper.Error(ex1);
                client.Close();
            }
        }
    }
}
