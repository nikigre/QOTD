import socket
import sys


def main(args):
    host = "127.0.0.1"

    do_a_request_tcp(host)


def do_a_request_tcp(host):
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.bind((host, 17))
        s.listen()


if __name__ == "__main__":
    main(sys.argv[1:])  # first argument is path of the file