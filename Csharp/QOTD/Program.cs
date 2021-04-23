using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace QOTD
{
    class ClientContext
    {
        public TcpClient Client;
        public Stream Stream;
        public MemoryStream Message = new MemoryStream();
    }

    class Program
    {
        static List<string> quotes = new List<string> { };
        static bool imamoZaVsakDan = false;

        static void OnClientAccepted(IAsyncResult ar)
        {
            TcpListener listener = ar.AsyncState as TcpListener;
            if (listener == null)
                return;

            ClientContext context = new ClientContext();

            try
            {
                context.Client = listener.EndAcceptTcpClient(ar);
                context.Stream = context.Client.GetStream();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                try
                {
                    byte[] vsebina = null;
                    if (imamoZaVsakDan)
                        vsebina = Encoding.ASCII.GetBytes(quotes[DateTime.Today.DayOfYear]);
                    else
                    {
                        Random r = new Random();
                        vsebina = Encoding.ASCII.GetBytes(quotes[r.Next(0,quotes.Count)]);
                    }

                    context.Stream.Write(vsebina, 0, vsebina.Length);
                    context.Client.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                listener.BeginAcceptTcpClient(OnClientAccepted, listener);
            }

        }

        static void Main(string[] args)
        {
            if (args.Length == 1)
                quotes.AddRange(System.IO.File.ReadAllLines(args[0]));
            else
                quotes.AddRange(System.IO.File.ReadAllLines("quotes.txt"));

            if (quotes.Count == 365 || quotes.Count == 366)
                imamoZaVsakDan = true;

            TcpListener listener = new TcpListener(new IPEndPoint(IPAddress.Any, 17));
            listener.Start();

            listener.BeginAcceptTcpClient(OnClientAccepted, listener);

            Console.Write("Press enter to exit...");
            Console.ReadLine();
            listener.Stop();
        }
    }
}

//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;
//using System.Threading.Tasks;

//namespace QOTD
//{
//    class ClientContext
//    {
//        public TcpClient Client;
//        public Stream Stream;
//        public byte[] Buffer = new byte[4];
//        public MemoryStream Message = new MemoryStream();
//    }

//    class Program
//    {
//        static void OnMessageReceived(ClientContext context)
//        {
//            byte[] vsebina = Encoding.ASCII.GetBytes("hello");
//            context.Stream.Write(vsebina, 0, vsebina.Length);
//            context.Client.Close();
//        }

//        static void OnClientRead(IAsyncResult ar)
//        {
//            ClientContext context = ar.AsyncState as ClientContext;
//            if (context == null)
//                return;

//            try
//            {
//                int read = context.Stream.EndRead(ar);
//                context.Message.Write(context.Buffer, 0, read);

//                int length = BitConverter.ToInt32(context.Buffer, 0);
//                byte[] buffer = new byte[1024];
//                while (length > 0)
//                {
//                    read = context.Stream.Read(buffer, 0, Math.Min(buffer.Length, length));
//                    context.Message.Write(buffer, 0, read);
//                    length -= read;
//                }

//                OnMessageReceived(context);
//            }
//            catch (System.Exception)
//            {
//                context.Client.Close();
//                context.Stream.Dispose();
//                context.Message.Dispose();
//                context = null;
//            }
//            finally
//            {
//                if (context != null)
//                    context.Stream.BeginRead(context.Buffer, 0, context.Buffer.Length, OnClientRead, context);
//            }
//        }

//        static void OnClientAccepted(IAsyncResult ar)
//        {
//            TcpListener listener = ar.AsyncState as TcpListener;
//            if (listener == null)
//                return;

//            try
//            {
//                ClientContext context = new ClientContext();
//                context.Client = listener.EndAcceptTcpClient(ar);
//                context.Stream = context.Client.GetStream();
//                context.Stream.BeginRead(context.Buffer, 0, context.Buffer.Length, OnClientRead, context);
//            }
//            finally
//            {
//                listener.BeginAcceptTcpClient(OnClientAccepted, listener);
//            }
//        }

//        static void Main(string[] args)
//        {
//            TcpListener listener = new TcpListener(new IPEndPoint(IPAddress.Any, 17));
//            listener.Start();

//            listener.BeginAcceptTcpClient(OnClientAccepted, listener);

//            Console.Write("Press enter to exit...");
//            Console.ReadLine();
//            listener.Stop();
//        }
//    }
//}
