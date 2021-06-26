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
            string host = "qotd.nikigre.si";

            //If we have no arguments, then we do a TCP request on the default sever
            if (args.Length == 0)
            {
                DoARequestTCP(host);
            }

            //If first argument is call for help, we print help
            if (args[0].Trim() == "/?" || args[0].Contains("help"))
                handleHelp();

            //If first argument is UDP, then the host is second argument.
            if (args[0].ToUpper() == "UDP")
            {
                if (args.Length <= 1)
                {
                    Console.WriteLine("Second argument must be the hostname or IP!");
                    Environment.Exit(1);
                }
                //We check if host makes sense and then we do a UDP request
                host = args[1];
                checkIPhost(host);

                DoARequestUDP(host);

            }
            //If first argument is TCP, then the host is second argument.
            else if (args[0].ToUpper() == "TCP")
            {
                if (args.Length <= 1)
                {
                    Console.WriteLine("Second argument must be the hostname or IP!");
                    Environment.Exit(1);
                }
                //We check if host makes sense and then we do a UDP request
                host = args[1];
                checkIPhost(host);

                DoARequestTCP(host);
            }
            //If it is not UDP/TCP then we know, that this is host
            else
            {
                //We check if host makes sense and then we do a UDP request
                host = args[0];
                DoARequestTCP(host);
            }

        }

        /// <summary>
        /// Method checks if IP/host makes sense
        /// </summary>
        /// <param name="host">Host we want to check</param>
        private static void checkIPhost(string host)
        {
            //If host is given, we try to parse it. If it is unsuccesful, then it terminates the program
            IPAddress tmp;
            if (!IPAddress.TryParse(host, out tmp))
            {
                System.Console.WriteLine("IP address or hostname is not in the correct format!");
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Method sends a UDP request to the host
        /// </summary>
        /// <param name="host">The host we want data from</param>
        private static void DoARequestUDP(string host)
        {
            try
            {
                //We declare and initialice new UDP client
                UdpClient udpClient = new UdpClient(host, 17);

                //And IP endpoint of server we are connecting to it
                IPEndPoint server = new IPEndPoint(IPAddress.Any, 17);

                //So that server knows where to send the quote firstly we need to send some data to the server
                byte[] data = new byte[] { 1 };
                int packet1 = udpClient.Send(data, data.Length);

                //Here we wait and read the data that server returns
                byte[] response = udpClient.Receive(ref server);

                //Decode array of bytes to ASCII string
                string stringResponse = Encoding.ASCII.GetString(response);

                //Print the response back
                System.Console.WriteLine(stringResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Environment.Exit(0);
        }

        /// <summary>
        /// Method does TCP request to the host
        /// </summary>
        /// <param name="host">The host we want data from</param>
        private static void DoARequestTCP(string host)
        {
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

            //Read the response from client
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

            Environment.Exit(0);
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
            Console.WriteLine("USAGE:\n\tqotd [/? | [hostname] | TCP [hostname] |  UDP [hostname] ]");
            Console.WriteLine();
            Console.WriteLine("where:");
            Console.WriteLine("\thostname\t Is the hostname of QOTD server you want to connect to");
            Console.WriteLine("\nOptions:");
            Console.WriteLine("\t/? Display this help message");
            Console.WriteLine("\tTCP Uses a TCP connection to the host");
            Console.WriteLine("\tUDP Uses a UDP connection to the host");
            Console.WriteLine("\nExamples:");
            Console.WriteLine("\tqotd\t\t\t...Connects to the default server");
            Console.WriteLine("\tqotd 127.0.0.1\t\t...Connects to the 127.0.0.1 server");
            Console.WriteLine("\tqotd TCP 127.0.0.1\t...Connects to the 127.0.0.1 server with TCP protocol");
            Console.WriteLine("\tqotd qotd.example.com\t...Connects to the qotd.example.com server");

            Environment.Exit(0);

        }
    }
}
