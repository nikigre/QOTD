using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace QOTD_server
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> quotes = new List<string> { }; //Variable for quotes

            //If we have first argument, then load custom quotes
            if (args.Length == 1)
            {
                //We need to check, if file exists. And if it does, then we can load it up.
                if (System.IO.File.Exists(args[0]))
                {
                    quotes.AddRange(System.IO.File.ReadAllLines(args[0]));
                }
                else
                {
                    Console.WriteLine("Requested file does not exists!");
                    Environment.Exit(1);
                }
            }
            else //Else load default quotes
            {
                //We need to check, if file exists. And if it does, then we can load it up.
                if (System.IO.File.Exists("quotes.txt"))
                {
                    quotes.AddRange(System.IO.File.ReadAllLines("quotes.txt"));
                }
                else
                {
                    Console.WriteLine("Default file quotes.txt does not exists!");
                    Environment.Exit(1);
                }
            }

            //Create new server object
            Server qotdServer = new Server(17, quotes);

            qotdServer.RunServer(); //We run our qotd server
        }
    }

    class Server
    {
        /// <summary>
        /// TcpListener that is waiting for clients to connect
        /// </summary>
        TcpListener serverTCP = null;

        /// <summary>
        /// UDPclient that is waiting for clients to connect
        /// </summary>
        UdpClient serverUDP = null;

        /// <summary>
        /// If we have quote for every day, then this is set to true
        /// </summary>
        bool WeHaveQuoteForEveryDay = false;

        /// <summary>
        /// List of quotes
        /// </summary>
        List<string> quotes = null;

        /// <summary>
        /// Creates a new instance of QOTD server
        /// </summary>
        /// <param name="port">On which port you want to start server</param>
        /// <param name="quotes">List of quotes</param>
        public Server(int port, List<string> quotes)
        {
            this.quotes = quotes;

            //We check, if we have 365/366 quotes
            if (quotes.Count == 365 || quotes.Count == 366)
                WeHaveQuoteForEveryDay = true;

            //Initialize variable
            serverTCP = new TcpListener(IPAddress.Any, port);

            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, port);
            serverUDP = new UdpClient(ipep);
        }

        /// <summary>
        /// Method runs a server
        /// </summary>
        public void RunServer()
        {
            try
            {
                //Starts new thread that listens for UDP clients
                Thread threadTCP = new Thread(new ThreadStart(StartListeningUDP));
                threadTCP.Start();

                //Starts new thread that listens for TCP clients
                Thread threadUDP = new Thread(new ThreadStart(StartListeningTCP));
                threadUDP.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("There was a problem starting a web server. Is server on port 17 already running?\nError: " + ex.Message);
                Environment.Exit(1);
            }
        }

        #region MethodsThatListenForNewCLients

        /// <summary>
        /// This method starts listening for new clients
        /// </summary>
        private void StartListeningUDP()
        {
            try
            {
                while (true)
                {
                    //This variable will hold info about our client that sent and UDP packet
                    IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);

                    Console.WriteLine("Waiting for a UDP connection...");

                    //Here we are waiting for a client to send us something. 
                    //Notice, that we used ref sender. sender variable parameters will be changed depending on the new client
                    byte[] s = serverUDP.Receive(ref sender); //Here we are waiting for a new connection

                    Console.WriteLine("Connected to UDP IP: " + sender);

                    //Creates new thread for a client and starts it
                    Thread t = new Thread(new ParameterizedThreadStart(HandleRequestUDP));
                    t.Start(sender);

                }
            }
            catch (SocketException e) //If anything goes wrong, then we print an error to the terminal and start listening again
            {
                Console.WriteLine("SocketException: {0}", e);
                StartListeningUDP();
            }

        }

        /// <summary>
        /// This method starts listening for new clients
        /// </summary>
        private void StartListeningTCP()
        {
            serverTCP.Start();

            try
            {
                while (true)
                {
                    Console.WriteLine("Waiting for a TCP connection...");

                    TcpClient client = serverTCP.AcceptTcpClient(); //Here we are waiting for a new connection

                    Console.WriteLine("Connected to TCP IP: " + client.Client.RemoteEndPoint);

                    //Creates new thread for a client and starts it
                    Thread t = new Thread(new ParameterizedThreadStart(HandleRequestTCP));
                    t.Start(client);
                }
            }
            catch (SocketException e) //If anything goes wrong, then we print an error to the terminal and start listening again
            {
                Console.WriteLine("SocketException: {0}", e);
                StartListeningTCP();
            }
        }

        #endregion

        #region MethodsThatHandleNewClient

        /// <summary>
        /// Method handles client's request
        /// </summary>
        /// <param name="obj">TcpClient in an object form</param>
        public void HandleRequestTCP(object obj)
        {
            try
            {
                //Gets the client and the stream for the client
                TcpClient client = (TcpClient)obj;
                var stream = client.GetStream();

                byte[] content = GetQuote();

                //Writes content to the client
                stream.Write(content, 0, content.Length);

                //And closes the connection
                client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex);
            }

        }

        /// <summary>
        /// Method handles client's request
        /// </summary>
        /// <param name="obj">UdpClient in an object form</param>
        private void HandleRequestUDP(object obj)
        {
            try
            {
                //Gets the clients IP endpoint
                IPEndPoint sender = (IPEndPoint)obj;

                //Gets the todays quote
                byte[] content = GetQuote();

                //Sends content to the client
                serverUDP.Send(content, content.Length, sender);

                //We do not close the connection. Why?
                //Because UDP protocol doesn't have a hand shake when it opens the connection.
                //It just sends the data to a client.

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex);
            }

        }

        #endregion

        /// <summary>
        /// Method returns quote of the day
        /// </summary>
        /// <returns>Reurns ASCII string in byte[] array</returns>
        private byte[] GetQuote()
        {
            //New byte[] array for out message
            byte[] content = null;

            //If we have enough quites for every year, then we just encode message that is saved in quotes
            if (WeHaveQuoteForEveryDay)
                content = Encoding.ASCII.GetBytes(quotes[DateTime.Today.DayOfYear]);
            else
            {
                //If we don't have enough of them, then we randomly choose an element in the list
                Random r = new Random();
                content = Encoding.ASCII.GetBytes(quotes[r.Next(0, quotes.Count)]);
            }

            return content;
        }
    }
}