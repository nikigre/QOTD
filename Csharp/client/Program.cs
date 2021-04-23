using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace client
{
    class Program
    {
        static void Main(string[] args)
        {
            string ip = "127.0.0.1";
            if (args.Length > 0)
            {
                ip = args[0];
            }

            TcpClient tcpClient = new TcpClient(ip, 17);

            List<byte> odgovor = new List<byte> { };

            NetworkStream nwStream = tcpClient.GetStream();

            int enbyte = 0;
            while (enbyte != -1)
            {
                enbyte = nwStream.ReadByte();
                if (enbyte != -1)
                    odgovor.Add((byte)enbyte);
            }

            System.Console.WriteLine(Encoding.ASCII.GetString(odgovor.ToArray()));
            tcpClient.Close();

        }
    }
}
