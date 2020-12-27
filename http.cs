using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ToDoList
{
    class Program
    {
        private static Message[] ms = new Message[5];

        private static int count = 0;

        static void Main(string[] args)
        {
            HttpListener http = new HttpListener();
            http.Prefixes.Add("http://127.0.0.1:3000/");
            http.Start();  
            while (true)
            {
                HttpListenerContext context = http.GetContext();
                Task.Run(() => Handler(context.Request, context.Response));
            }
        }

        private static void Handler(HttpListenerRequest req, HttpListenerResponse res)
        {
            switch (req.RawUrl)
            {
                case "/":
                    SendPage(res, "index.html");
                    break;
                case "/msg":
                    if (req.HttpMethod == "POST")
                    {
                         byte[] buffer = new byte[1024];
                        for(int i = 0; ; i++)
                        {
                            int t = req.InputStream.ReadByte();
                            if (t == -1)
                            {
                                Array.Resize(ref buffer, i);
                                break;
                            }
                            buffer[i] = (byte)t;
                        }
                        Message msg = JsonConvert.DeserializeObject<Message>(Encoding.UTF8.GetString(buffer));
                        if (count == 5)
                        {
                            for (int i = ms.Length - 1; i > 0; i--)
                                ms[i] = ms[i - 1];
                            ms[0] = msg;
                        }
                        else
                            ms[count++] = msg;
                        res.StatusCode = 202;
                        res.Close();
                        break;
                    }
                    res.StatusCode = 200;
                    res.ContentType = "application/json";
                    Message[] temp = new Message[count];
                    Array.Copy(ms, temp, count);
                    res.OutputStream.Write(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(temp)));
                    res.Close();
                    break;              
                default:
                    res.StatusCode = 404;
                    res.Close();
                    break;
            }
        }
        private static void SendPage(HttpListenerResponse res, string path)
        {
            res.StatusCode = 200;
            res.ContentType = "text/html";
            res.AddHeader("Charset", "UTF-8");
            BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read));
            res.OutputStream.Write(reader.ReadBytes((int)reader.BaseStream.Length), 0, (int)reader.BaseStream.Length);
            reader.Close();
            res.Close();
        }

        class Message
        {
            public string text="";
        }
    }
}