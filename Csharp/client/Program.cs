using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace client
{
    class Program
    {
        static void Main(string[] args)
        {
            //Sets the default host address for the server
            string host = "127.0.0.1";

            if (args.Length > 0)
            {
                if (args[0].Trim() == "/?" || args[0].Contains("help"))
                    handleHelp();
                //If host is given, we try to parse it. If it is unsuccesful, then it terminates the program
                IPAddress tmp;
                if (IPAddress.TryParse(args[0], out tmp))
                {
                    System.Console.WriteLine("IP address is not in the correct format!");
                    Environment.Exit(1);
                }

                host = args[0];
            }

            //We declare new tcpClient
            TcpClient tcpClient = null;
            NetworkStream nwStream = null;
            try
            {
                //We try to connect to the host
                tcpClient = new TcpClient(host, 17);

                //Gets stream from server
                nwStream = tcpClient.GetStream();
            }
            catch (SocketException ex) //If we fail, because server is not avaliable this will happen
            {
                Console.WriteLine(ex.Message);
                Environment.Exit(1);
            }
            catch (Exception ex) //If there is some other error, this will happen
            {
                Console.WriteLine("Unexpected error: " + ex.Message);
                Environment.Exit(1);
            }

            ReadTheResponse(nwStream);

            try
            {
                //Close the connection
                tcpClient.Close();
            }
            catch (Exception)
            {
                Environment.Exit(2);
            }
        }

        /// <summary>
        /// Metods print to console output from NetworkStream
        /// </summary>
        /// <param name="nwStream">NetworkStream to read from</param>
        private static void ReadTheResponse(NetworkStream nwStream)
        {
            //A variable for keeping bytes that are read
            List<byte> response = new List<byte> { };

            //temporary var
            int enbyte = 0;

            while (true)
            {
                //Reads one byte to temporary variable
                enbyte = nwStream.ReadByte();

                //If it is n -1 then it breaks loop
                if (enbyte == -1)
                    break;

                //Adds byte to the list
                response.Add((byte)enbyte);

            }

            //Decode array of bytes to ASCII string
            string stringResponse = Encoding.ASCII.GetString(response.ToArray());

            //Print the response back
            System.Console.WriteLine(stringResponse);
        }

        /// <summary>
        /// Method prints help to the console
        /// </summary>
        private static void handleHelp()
        {
            Console.WriteLine("USAGE:\n\tqotd [/? | [hostname] ]");
            Console.WriteLine();
            Console.WriteLine("where:");
            Console.WriteLine("\thostname\t Is the hostname of QOTD server you want to connect to");
            Console.WriteLine("\nOptions:");
            Console.WriteLine("\t/? Display this help message");
            Console.WriteLine("\nExamples:");
            Console.WriteLine("\tqotd\t\t\t...Connects to the default server");
            Console.WriteLine("\tqotd 127.0.0.1\t\t...Connects to the 127.0.0.1 server");
            Console.WriteLine("\tqotd qotd.example.com\t...Connects to the qotd.example.com server");

            Environment.Exit(0);

        }
    }
}
