using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;

namespace FiatShamirServer
{
    class Program
    {
        static Server server = new Server(5432);
        static BigInteger p;
        static BigInteger q;
        static BigInteger n;

        static void Main(string[] args)
        {
            BigRND bgr = new BigRND();
            p = bgr.p;
            q = bgr.q;
            n = p * q;

            while(true)
            {
                TcpClient client = server.newCLient();
                Console.WriteLine("A user has entered");
                Task.Run(() =>
                {
                    server.SendToNew(client, n);
                    while (true)
                    {
                        try
                        {
                            string message = server.recieve(client);

                            int index = message.IndexOf("}}") + 1;
                            string objTemp = message.Substring(0, index + 1);
                            int i = server.clients.Count - 1;
                            Envelope unknown = JsonConvert.DeserializeObject<Envelope>(objTemp);
                            if (unknown.type == "Message")
                            {
                                Message m = JsonConvert.DeserializeObject<Message>(unknown.obj.ToString());

                                if (m.user == null)
                                {
                                    byte[] bytesAux = ASCIIEncoding.ASCII.GetBytes(m.text);
                                    string aux = ASCIIEncoding.ASCII.GetString(bytesAux);
                                    server.clients[i].V = BigInteger.Parse(aux);
                                    Console.WriteLine(server.clients[i].V.ToString());
                                    continue;
                                }
                                if (m.text == null)
                                {
                                    byte[] bytesAux = ASCIIEncoding.ASCII.GetBytes(m.user);
                                    server.clients[i].name = m.user;
                                    Console.WriteLine(server.clients[i].name.ToString());
                                    continue;
                                }
                            }
                            if(unknown.type=="VRequest")
                            {
                                PrivateMessage pm = JsonConvert.DeserializeObject<PrivateMessage>(unknown.obj.ToString());
                                byte[] bytesAux = ASCIIEncoding.ASCII.GetBytes(pm.sUser);
                                string SUser = ASCIIEncoding.ASCII.GetString(bytesAux);

                                bytesAux = ASCIIEncoding.ASCII.GetBytes(pm.dUser);
                                string DUser = ASCIIEncoding.ASCII.GetString(bytesAux);

                                BigInteger v=new BigInteger();
                                for(int j=0; j<server.clients.Count;j++)
                                {
                                    if(server.clients[j].name==DUser)
                                    {
                                        v = server.clients[j].V;
                                        break;
                                    }
                                }

                                for(int j=0;j<server.clients.Count;j++)
                                {
                                    if(server.clients[j].name==SUser)
                                    {
                                        Message pmToSend = new Message();
                                        pmToSend.text = v.ToString();
                                        pmToSend.user = DUser;
                                        Envelope en = new Envelope("VRequest", pmToSend);
                                        string obj = JsonConvert.SerializeObject(en);
                                        byte[] msgToSend = ASCIIEncoding.ASCII.GetBytes(obj);
                                        server.clients[j].client.GetStream().Write(msgToSend, 0, msgToSend.Length);
                                        break;
                                    }
                                }
                                continue;
                            }
                            Console.WriteLine("Recieved " + message);
                            ChecMessageType(message);
                        }
                        catch
                        {
                            break;
                        }
                    }
                });
            }
        }


        public static void ChecMessageType(string message)
        {
            int index = message.IndexOf("}}") + 1;
            string objTemp = message.Substring(0, index + 1);
            Envelope unknown = JsonConvert.DeserializeObject<Envelope>(objTemp);
            
            if(unknown.type=="Message")
            {
                server.sendToAll(message);
            }
            else if(unknown.type=="PM")
            {
                PrivateMessage PM = JsonConvert.DeserializeObject<PrivateMessage>(unknown.obj.ToString());
                TcpClient client=new TcpClient();
                string dest = PM.dUser;
                for(int i=0;i<server.clients.Count;i++)
                {
                    if(server.clients[i].name==dest)
                    {
                        client = server.clients[i].client;
                        break;
                    }
                }
                server.SendToOne(PM, client);
            }
        }
    }
}
