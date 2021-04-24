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
                quotes.AddRange(System.IO.File.ReadAllLines(args[0]));
            else //Else load default quotes
                quotes.AddRange(System.IO.File.ReadAllLines("quotes.txt"));

            //Create new server object
            Server qotdServer = new Server(17, quotes);

            qotdServer.RunServer(); //We run our qotd server
        }

    }

    class Server
    {
        /// <summary>
        /// TcpListener that is waoting for clients to connect
        /// </summary>
        TcpListener server = null;

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
            server = new TcpListener(IPAddress.Any, port);
        }

        /// <summary>
        /// This method starts listening for new clients
        /// </summary>
        private void StartListening()
        {
            try
            {
                while (true)
                {
                    Console.WriteLine("Waiting for a connection...");

                    TcpClient client = server.AcceptTcpClient(); //Here we are waiting for a new connection

                    Console.WriteLine("Connected to IP: " + client.Client.LocalEndPoint);

                    //Creates new thread for a client and starts it
                    Thread t = new Thread(new ParameterizedThreadStart(HandleRequest));
                    t.Start(client);
                }
            }
            catch (SocketException e) //If anything goes wrong, then we print an error to the terminal and start listening again
            {
                Console.WriteLine("SocketException: {0}", e);
                StartListening();
            }
        }

        /// <summary>
        /// Method handles client's request
        /// </summary>
        /// <param name="obj">TcpClient in an object form</param>
        public void HandleRequest(object obj)
        {
            try
            {
                //Gets the client and the stream for the client
                TcpClient client = (TcpClient)obj;
                var stream = client.GetStream();

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
        /// Method runs a server
        /// </summary>
        public void RunServer()
        {
            server.Start();

            StartListening();
        }
    }
}