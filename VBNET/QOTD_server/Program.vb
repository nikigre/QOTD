Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Threading

Module Program

    Sub Main(args As String())
        Dim quotes As New List(Of String)() 'Variable For quotes

        'If we have first argument, then load custom quotes
        If (args.Length = 1) Then
            quotes.AddRange(System.IO.File.ReadAllLines(args(0)))
        Else 'Else load Default quotes
            quotes.AddRange(System.IO.File.ReadAllLines("quotes.txt"))
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
    Dim server As TcpListener = Nothing

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
        server = New TcpListener(IPAddress.Any, port)
    End Sub

    ''' <summary>
    ''' This method starts listening for new clients
    ''' </summary>
    Private Sub StartListening()
        Try
            While True
                Console.WriteLine("Waiting for a connection...")

                Dim client As TcpClient = server.AcceptTcpClient() 'Here we are waiting for a new connection

                Console.WriteLine("Connected to IP: " & client.Client.RemoteEndPoint.ToString())

                'Creates new thread for a client and starts it
                Dim t As Thread = New Thread(New ParameterizedThreadStart(AddressOf HandleRequest))
                t.Start(client)

            End While

        Catch ex As Exception 'If anything goes wrong, then we print an error to the terminal and start listening again
            Console.WriteLine("SocketException: {0}", ex)
            StartListening()
        End Try

    End Sub


    ''' <summary>
    ''' Method handles client's request
    ''' </summary>
    ''' <param name="obj">TcpClient in an object form</param>
    Public Sub HandleRequest(obj As Object)
        Try
            'Gets the client and the stream for the client
            Dim client As TcpClient = DirectCast(obj, TcpClient)
            Dim stream As NetworkStream = client.GetStream()

            'New byte[] array for out message
            Dim content As Byte() = Nothing

            'If we have enough quites for every year, then we just encode message that is saved in quotes
            If WeHaveQuoteForEveryDay Then
                content = Encoding.ASCII.GetBytes(quotes(DateTime.Today.DayOfYear))
            Else
                'If we don't have enough of them, then we randomly choose an element in the list
                Dim r As New Random()

                content = Encoding.ASCII.GetBytes(quotes(r.Next(0, quotes.Count)))
            End If

            'Writes content to the client
            stream.Write(content, 0, content.Length)

            'And closes the connection
            client.Close()

        Catch ex As Exception
            Console.WriteLine("Exception: {0}", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Method runs a server
    ''' </summary>
    Public Sub RunServer()
        server.Start()

        StartListening()
    End Sub

End Class