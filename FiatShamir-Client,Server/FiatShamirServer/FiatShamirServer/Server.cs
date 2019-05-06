using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Newtonsoft.Json;

namespace FiatShamirServer
{
    class Server
    {
        int port;
        TcpListener listener;
        public List<FancyTcp> clients = new List<FancyTcp>();

        public Server(int port = 1234)
        {
            this.port = port;
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
        }

        public TcpClient newCLient()
        {
            TcpClient client = listener.AcceptTcpClient();
            clients.Add(new FancyTcp() { client = client });
            return client;
        }

        public void SendToNew(TcpClient nc, BigInteger n)
        {
            Message m = new Message();
            m.text = n.ToString();
            m.user = "Server:";
            Envelope en = new Envelope("PublicN", m);
            string obj = JsonConvert.SerializeObject(en);
            byte[] msgToSend = ASCIIEncoding.ASCII.GetBytes(obj);
            nc.GetStream().Write(msgToSend, 0, msgToSend.Length);
        }

        public void sendToAll(string message)
        {
            byte[] bytes = ASCIIEncoding.ASCII.GetBytes(message);
            for (int i = 0; i < clients.Count; i++)
            {
                try
                {
                    clients[i].client.GetStream().Write(bytes, 0, bytes.Length);
                }
                catch
                {
                    clients.RemoveAt(i);
                    if (i - 1 < 0)
                    {
                        i--;
                    }
                }
            }
        }

        public string recieve(TcpClient client)
        {
            byte[] bytes = new byte[client.ReceiveBufferSize];
            int toRead = client.GetStream().Read(bytes, 0, client.ReceiveBufferSize);
            return ASCIIEncoding.ASCII.GetString(bytes, 0, toRead);
        }

        public void SendToOne(PrivateMessage message,TcpClient client)
        {
            Envelope en = new Envelope("PM", message);
            string obj = JsonConvert.SerializeObject(en);
            byte[] messageToSend = ASCIIEncoding.ASCII.GetBytes(obj);
            client.GetStream().Write(messageToSend, 0, messageToSend.Length);
        }
    }
}
