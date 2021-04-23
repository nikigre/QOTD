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
        Dim qotdServer As New Server("127.0.0.1", 17, quotes)

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

    Public Sub New(ByVal ip As String, ByVal port As Integer, ByVal quotes As List(Of String))
        Me.quotes = quotes
        If quotes.Count = 365 Or quotes.Count = 366 Then
            WeHaveQuoteForEveryDay = True
        End If

        Dim localAddr As IPAddress = IPAddress.Parse(ip)

        server = New TcpListener(localAddr, port)
    End Sub

    ''' <summary>
    ''' This method starts listening for new clients
    ''' </summary>
    Private Sub StartListening()
        Try
            While True
                Console.WriteLine("Waiting for a connection...")

                Dim client As TcpClient = server.AcceptTcpClient()

                Console.WriteLine("Connected to IP: " & client.Client.LocalEndPoint.ToString())

                Dim t As Thread = New Thread(New ParameterizedThreadStart(AddressOf HandleRequest))
                t.Start(client)

            End While

        Catch ex As Exception
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
            Dim client As TcpClient = DirectCast(obj, TcpClient)
            Dim stream As NetworkStream = client.GetStream()

            Dim content As Byte() = Nothing

            If WeHaveQuoteForEveryDay Then
                content = Encoding.ASCII.GetBytes(quotes(DateTime.Today.DayOfYear))
            Else
                Dim r As New Random()

                content = Encoding.ASCII.GetBytes(quotes(r.Next(0, quotes.Count)))
            End If

            stream.Write(content, 0, content.Length)

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