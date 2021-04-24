import socket
import struct
import sys


def main(args):
    host = "127.0.0.1"
    do_a_request_tcp(host)


def receive_fixed_length_msg(sock, msglen):
    message = b''
    while len(message) < msglen:
        chunk = sock.recv(msglen - len(message))
        if chunk == b'':
            raise RuntimeError("socket connection broken")
        message = message + chunk

    return message


def receive_message(sock):
    header = receive_fixed_length_msg(sock, 2)
    message_length = struct.unpack("!H", header)[0]

    message = None
    if message_length > 0:
        message = receive_fixed_length_msg(sock, message_length)
        message = message.decode("utf-8")

    return message


def do_a_request_tcp(host):
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        try:
            s.connect((host, 17))

            while True:
                msg_received = receive_message(s)
                if len(msg_received) > 0:
                    print(msg_received)
                    sys.exit(0)

        except Exception as e:
            print(e)
            sys.exit(1)


if __name__ == "__main__":
    main(sys.argv[1:])  # first argument is path of the file
