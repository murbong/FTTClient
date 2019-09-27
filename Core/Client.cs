using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FTTClient.Core
{
    public class Client
    {
        public TcpClient tcpClient;
        public bool Connected;
        public string ip;
        public int port;
        public delegate void ReceivedString(string str);
        public event ReceivedString ReceivedStr;
        public Client() { }

        public Client(string _ip, int _port) { ip = _ip; port = _port; }

        public void Clear()
        {
            foreach (Delegate d in ReceivedStr.GetInvocationList())
            {
                ReceivedStr -= (ReceivedString)d;
            }
        }

        public void Stop()
        {
            Connected = false;
            tcpClient.Close();
            tcpClient.Dispose();
        }
        public int Initialize()
        {
            return Initialize(ip, port);
        }

        public int Initialize(string _ip, int _port)
        {
            ip = _ip;
            port = _port;
            Connected = false;
            tcpClient = new TcpClient();
            try
            {
                var result = tcpClient.BeginConnect(_ip, _port, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(1000, true); // 1초간 대기
                if (success)
                {
                    tcpClient.EndConnect(result);
                    ReceivedStr("서버와 연결되었습니다.");
                    Connected = true;
                }
                else
                {
                    //ReceivedStr("오류.");
                    tcpClient.Close();
                    throw new SocketException(10060); // Connection timed out.
                }
                //Task.Run(() => BeginRead());
                tcpClient.SendTimeout = 5000;
                BeginRead();
                return 0;
            }
            catch
            {
                // 연결 실패 (연결 도중 오류가 발생함)
                tcpClient.Close();
                return -1;
            }
        }
        public void BeginRead()//Loop With Callback
        {
            if (Connected)
            {
                try
                {
                    var buffer = new byte[4096];
                    var ns = tcpClient.GetStream();
                    ns.BeginRead(buffer, 0, buffer.Length, EndRead, buffer);
                }
                catch
                {

                }
            }
        }
        public void EndRead(IAsyncResult result)//Callback
        {
            if (Connected)
            {
                try
                {
                    var buffer = (byte[])result.AsyncState;
                    var ns = tcpClient.GetStream();
                    var bytesAvailable = ns.EndRead(result);
                    if (bytesAvailable > 0)
                    {
                        string msg = Encoding.Unicode.GetString(buffer, 0, bytesAvailable);
                        ReceivedStr(msg);
                        BeginRead();
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                catch // 연결 재시도
                {
                    ReceivedStr("서버와의 연결이 끊겼습니다.");
                    for (int i = 0; i < 10000; i++)
                    {
                        if (Initialize(ip, port) == 0)
                        {
                            ReceivedStr("연결 성공");
                            return;
                        }
                        else
                        {
                            Task.WaitAll(Task.Delay(1000));
                            ReceivedStr("재시도중..." + i);
                        }
                    }
                    ReceivedStr("서버와 연결할 수 없습니다.");



                    tcpClient.Close();
                    tcpClient.Dispose();

                    Connected = false;
                }
            }
        }

        public void BeginSend(string xml)
        {
            if (Connected)
            {
                var bytes = Encoding.Unicode.GetBytes(xml);
                var ns = tcpClient.GetStream();
                ns.BeginWrite(bytes, 0, bytes.Length, EndSend, bytes);
            }
        }
        public void EndSend(IAsyncResult result)
        {
            if (Connected)
            {
                var bytes = (byte[])result.AsyncState;
                Console.WriteLine("Sent  {0} bytes to server.", bytes.Length);
                Console.WriteLine("Sent: {0}", Encoding.Unicode.GetString(bytes));
            }
        }
    }
}
