Imports System.Net
Imports System.Net.Sockets
Imports System.Text

Module Program

    Sub Main(args As String())
        'Sets the default host address for the server
        Dim host As String = "qotd.nikigre.si"

        'If we have no arguments, then we do a TCP request on the default sever
        If args.Length = 0 Then
            DoARequestTCP(host)
        End If

        'If first argument Is call for help, we print help
        If args(0).Trim() = "/?" Or args(0).Contains("help") Then
            handleHelp()
        End If

        If args(0).ToUpper() = "UDP" Then 'If first argument Is UDP, then the host Is second argument.
            If args.Length <= 1 Then
                Console.WriteLine("Second argument must be the hostname or IP!")
                Environment.Exit(1)
            End If

            'We check if host makes sense And then we do a UDP request
            host = args(1)
            checkIPhost(host)

            DoARequestUDP(host)

        ElseIf args(0).ToUpper() = "TCP" Then 'if first argument Is TCP, then the host Is second argument.
            If args.Length <= 1 Then
                Console.WriteLine("Second argument must be the hostname or IP!")
                Environment.Exit(1)
            End If

            'We check if host makes sense And then we do a UDP request
            host = args(1)
            checkIPhost(host)

            DoARequestTCP(host)

        Else 'If it Is Not UDP/TCP then we know, that this Is host
            'We check if host makes sense And then we do a UDP request
            host = args(0)
            DoARequestTCP(host)
        End If
    End Sub

    ''' <summary>
    ''' Method checks if IP/host makes sense
    ''' </summary>
    ''' <param name="host">Host we want to check</param>
    Private Sub checkIPhost(host As String)
        'If host Is given, we try to parse it. If it Is unsuccesful, then it terminates the program
        Dim tmp As IPAddress
        If (IPAddress.TryParse(host, tmp)) Then
            System.Console.WriteLine("IP address or hostname is not in the correct format!")
            Environment.Exit(1)
        End If
    End Sub

    '''<summary>
    ''' Method does TCP request to the host
    '''</summary>
    '''<param name="host">The host we want data from</param>
    Private Sub DoARequestTCP(host As String)
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

        Environment.Exit(0)
    End Sub

    ''' <summary>
    ''' Method sends a UDP request to the host
    ''' </summary>
    ''' <param name="host">The host we want data from</param>
    Private Sub DoARequestUDP(host As String)
        Try
            'We declare And initialice New UDP client
            Dim UdpClient As UdpClient = New UdpClient(host, 17)

            'And IP endpoint of server we are connecting to it
            Dim server As IPEndPoint = New IPEndPoint(IPAddress.Any, 17)

            'So that server knows where to send the quote firstly we need to send some data to the server
            Dim data As Byte() = New Byte() {1}

            UdpClient.Send(data, data.Length)

            'Here we wait And read the data that server returns
            Dim response As Byte() = UdpClient.Receive(server)

            'Decode array of bytes to ASCII string
            Dim stringResponse As String = Encoding.ASCII.GetString(response)

            'Print the response back
            System.Console.WriteLine(stringResponse)
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try

        Environment.Exit(0)
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

        Console.WriteLine("USAGE:" & vbNewLine & vbTab & "qotd [/? | [hostname] | TCP [hostname] |  UDP [hostname] ]")
        Console.WriteLine()
        Console.WriteLine("where:")
        Console.WriteLine(vbTab & "hostname" & vbTab & " Is the hostname of QOTD server you want to connect to")
        Console.WriteLine(vbNewLine & "Options:")
        Console.WriteLine(vbTab & "/? Display this help message")
        Console.WriteLine(vbTab & "TCP Uses a TCP connection to the host")
        Console.WriteLine(vbTab & "UDP Uses a UDP connection to the host")
        Console.WriteLine(vbNewLine & "Examples:")
        Console.WriteLine(vbTab & "qotd" & vbTab & vbTab & vbTab & "...Connects to the default server")
        Console.WriteLine(vbTab & "qotd 127.0.0.1" & vbTab & vbTab & "...Connects to the 127.0.0.1 server")
        Console.WriteLine(vbTab & "qotd TCP 127.0.0.1" & vbTab & "...Connects to the 127.0.0.1 server with TCP protocol")
        Console.WriteLine(vbTab & "qotd qotd.example.com" & vbTab & "...Connects to the qotd.example.com server")
        Environment.Exit(0)

    End Sub

End Module
