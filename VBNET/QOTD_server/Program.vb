Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Threading

Module Program

    Sub Main(args As String())
        Dim quotes As New List(Of String)() 'Variable For quotes

        'If we have first argument, then load custom quotes
        If (args.Length = 1) Then
            'We need to check, if file exists. And if it does, then we can load it up.
            If System.IO.File.Exists(args(0)) Then
                quotes.AddRange(System.IO.File.ReadAllLines(args(0)))
            Else
                Console.WriteLine("Requested file does not exists!")
                Environment.Exit(1)
            End If

        Else 'Else load Default quotes
            'We need to check, if file exists. And if it does, then we can load it up.
            If System.IO.File.Exists("quotes.txt") Then
                quotes.AddRange(System.IO.File.ReadAllLines("quotes.txt"))
            Else
                Console.WriteLine("Default file quotes.txt does not exists!")
                Environment.Exit(1)
            End If

        End If

        'Create New server object
        Dim qotdServer As New Server(17, quotes)

        qotdServer.RunServer() 'We run our qotd server
    End Sub

End Module

Public Class Server
    ''' <summary>
    ''' TcpListener that Is waoting for clients to connect
    ''' </summary>
    Dim serverTCP As TcpListener = Nothing

    ''' <summary>
    ''' UDPclient that is waiting for clients to connect
    ''' </summary>
    Dim serverUDP As UdpClient = Nothing

    ''' <summary>
    ''' If we have quote for every day, then this Is set to true
    ''' </summary>>
    Dim WeHaveQuoteForEveryDay As Boolean = False

    ''' <summary>
    ''' List of quotes
    ''' </summary>
    Dim quotes As New List(Of String)()

    ''' <summary>
    ''' Creates a New instance of QOTD server
    ''' </summary>
    ''' <param name="port">On which port you want to start server</param>
    ''' <param name="quotes">List of quotes</param>
    Public Sub New(ByVal port As Integer, ByVal quotes As List(Of String))
        Me.quotes = quotes

        'We check, if we have 365/366 quotes
        If quotes.Count = 365 Or quotes.Count = 366 Then
            WeHaveQuoteForEveryDay = True
        End If

        'Initialize variable
        serverTCP = New TcpListener(IPAddress.Any, port)


        Dim ipep As IPEndPoint = New IPEndPoint(IPAddress.Any, 17)
        serverUDP = New UdpClient(ipep)

    End Sub

    ''' <summary>
    ''' Method runs a server
    ''' </summary>
    Public Sub RunServer()
        Try
            'Starts New thread that listens for UDP clients
            Dim threadTCP As Thread = New Thread(New ThreadStart(AddressOf StartListeningUDP))
            threadTCP.Start()

            'Starts New thread that listens for TCP clients
            Dim threadUDP As Thread = New Thread(New ThreadStart(AddressOf StartListeningTCP))
            threadUDP.Start()

        Catch ex As Exception

            Console.WriteLine("There was a problem starting a web server. Is server on port 17 already running?\nError: " + ex.Message)
            Environment.Exit(1)
        End Try

    End Sub

#Region "MethodsThatListenForNewCLients"

    ''' <summary>
    ''' This method starts listening for New clients
    ''' </summary>
    Private Sub StartListeningUDP()
        Try
            While True
                'This variable will hold info about our client that sent And UDP packet
                Dim sender As IPEndPoint = New IPEndPoint(IPAddress.Any, 0)

                Console.WriteLine("Waiting for a UDP connection...")

                'Here we are waiting for a client to send us something. 
                'Notice, that we used ref sender. sender variable parameters will be changed depending on the New client
                Dim s As Byte() = serverUDP.Receive(sender) 'Here we are waiting For a New connection

                Console.WriteLine("Connected to UDP IP: " & sender.ToString())

                'Creates New thread for a client And starts it
                Dim t As Thread = New Thread(New ParameterizedThreadStart(AddressOf HandleRequestUDP))
                t.Start(sender)

            End While

        Catch e As SocketException  'If anything goes wrong, Then we print an Error To the terminal And start listening again
            Console.WriteLine("SocketException: {0}", e)
            StartListeningUDP()

        End Try
    End Sub

    ''' <summary>
    ''' This method starts listening for new clients
    ''' </summary>
    Private Sub StartListeningTCP()
        Try
            While True
                Console.WriteLine("Waiting for a connection...")

                Dim client As TcpClient = serverTCP.AcceptTcpClient() 'Here we are waiting for a new connection

                Console.WriteLine("Connected to IP: " & client.Client.RemoteEndPoint.ToString())

                'Creates new thread for a client and starts it
                Dim t As Thread = New Thread(New ParameterizedThreadStart(AddressOf HandleRequestTCP))
                t.Start(client)

            End While

        Catch ex As Exception 'If anything goes wrong, then we print an error to the terminal and start listening again
            Console.WriteLine("SocketException: {0}", ex)
            StartListeningTCP()
        End Try

    End Sub

#End Region

#Region "MethodsThatHandleNewClient"

    ''' <summary>
    ''' Method handles client's request
    ''' </summary>
    ''' <param name="obj">TcpClient in an object form</param>
    Public Sub HandleRequestTCP(obj As Object)
        Try
            'Gets the client and the stream for the client
            Dim client As TcpClient = DirectCast(obj, TcpClient)
            Dim stream As NetworkStream = client.GetStream()

            'New byte[] array for out message
            Dim content As Byte() = GetQuote()

            'Writes content to the client
            stream.Write(content, 0, content.Length)

            'And closes the connection
            client.Close()

        Catch ex As Exception
            Console.WriteLine("Exception: {0}", ex)
        End Try
    End Sub

    '''<summary>
    ''' Method handles client's request
    ''' </summary>
    '''<param name="obj">UdpClient in an object form</param>
    Private Sub HandleRequestUDP(obj As Object)

        Try
            'Gets the clients IP endpoint
            Dim sender As IPEndPoint = DirectCast(obj, IPEndPoint)

            'Gets the todays quote
            Dim content As Byte() = GetQuote()

            'Sends content to the client
            serverUDP.Send(content, content.Length, sender)

            'We do Not close the connection. Why?
            'Because UDP protocol doesn't have a hand shake when it opens the connection.
            'It just sends the data to a client.

        Catch ex As Exception
            Console.WriteLine("Exception: {0}", ex)
        End Try


    End Sub

#End Region

    ''' <summary>
    ''' Method returns quote of the day
    ''' </summary>
    ''' <returns>Reurns ASCII string in byte[] array</returns>
    Private Function GetQuote()

        'New byte[] array for out message
        Dim content As Byte() = Nothing

        'If we have enough quites for every year, then we just encode message that Is saved in quotes
        If WeHaveQuoteForEveryDay Then
            content = Encoding.ASCII.GetBytes(quotes(DateTime.Today.DayOfYear))
        Else

            'If we don't have enough of them, then we randomly choose an element in the list
            Dim r As Random = New Random()
            content = Encoding.ASCII.GetBytes(quotes(r.Next(0, quotes.Count)))
        End If

        Return content
    End Function
End Class