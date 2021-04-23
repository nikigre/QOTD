using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace client
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpClient tcpClient = new TcpClient("localhost", 17);
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
