Imports System.Net
Imports System.Net.Sockets
Imports System.Text

Module Program

    Sub Main(args As String())
        'Sets the default host address for the server
        Dim host As String = "127.0.0.1"

        If args.Length > 0 Then
            If args(0).Trim() = "/?" Or args(0).Contains("help") Then
                handleHelp()
            End If

            'If host is given, we try to parse it. If it is unsuccesful, then it terminates the program
            Dim tmp As IPAddress

            If IPAddress.TryParse(args(0), tmp) Then
                Console.WriteLine("IP address is not in the correct format!")
                Environment.Exit(1)
            End If

            host = args(0)

        End If

        'We declare new tcpClient
        Dim tcpClient As TcpClient = Nothing
        Dim nwStream As NetworkStream = Nothing

        Try
            'We try to connect to the host
            tcpClient = New TcpClient(host, 17)

            'Gets stream from server
            nwStream = tcpClient.GetStream()
        Catch ex As SocketException 'If we fail, because server is not avaliable this will happen
            Console.WriteLine(ex.Message)
            Environment.Exit(1)

        Catch ex As Exception 'If there is some other error, this will happen
            Console.WriteLine("Unexpected error: " + ex.Message)
            Environment.Exit(1)
        End Try

        'Read the response from client
        ReadTheResponse(nwStream)

        Try
            'Close the connection
            tcpClient.Close()
        Catch ex As Exception
            Environment.Exit(2)
        End Try
    End Sub

    ''' <summary>
    ''' Metods print to console output from NetworkStream
    ''' </summary>
    ''' <param name="nwStream">NetworkStream to read from</param>
    Private Sub ReadTheResponse(nwStream As NetworkStream)
        'A variable for keeping bytes that are read
        Dim response As New List(Of Byte)()

        'temporary var
        Dim enbyte As Integer = 0

        While True
            'Reads one byte to temporary variable
            enbyte = nwStream.ReadByte()

            'If it is n -1 then it breaks loop
            If enbyte = -1 Then
                Exit While
            End If

            'Adds byte to the list
            response.Add(Convert.ToByte(enbyte))
        End While

        'Decode array of bytes to ASCII string
        Dim stringResponse As String = Encoding.ASCII.GetString(response.ToArray())

        'Print the response back
        Console.WriteLine(stringResponse)


    End Sub

    ''' <summary>
    ''' Method prints help to the console
    ''' </summary>
    Private Sub handleHelp()

        Console.WriteLine("USAGE:" & vbNewLine & vbTab & "qotd [/? | [hostname] ]")
        Console.WriteLine()
        Console.WriteLine("where:")
        Console.WriteLine(vbTab & "hostname" & vbTab & " Is the hostname of QOTD server you want to connect to")
        Console.WriteLine(vbNewLine & "Options:")
        Console.WriteLine(vbTab & "/? Display this help message")
        Console.WriteLine(vbNewLine & "Examples:")
        Console.WriteLine(vbTab & "qotd" & vbTab & vbTab & vbTab & "...Connects to the default server")
        Console.WriteLine(vbTab & "qotd 127.0.0.1" & vbTab & vbTab & "...Connects to the 127.0.0.1 server")
        Console.WriteLine(vbTab & "qotd qotd.example.com" & vbTab & "...Connects to the qotd.example.com server")
        Environment.Exit(0)

    End Sub

End Module
