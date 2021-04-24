import random
import struct
from datetime import datetime
import sys
import threading
from os import path
import socket


def main(args):
    if len(args) == 1:  # If we have first argument, then load custom quotes
        # We need to check, if file exists. If it does, then we can load it up.
        if path.exists(args[0]):
            with open(args[0]) as file:
                quotes = file.readlines()
        else:
            print("Requested file does not exist!")
            sys.exit(1)
    else:  # load default quotes
        # We need to check, if file exists. If it does, then we can load it up.
        if path.exists("quotes.txt"):
            with open("quotes.txt") as file:
                quotes = file.readlines()
        else:
            print("Default file quotes.txt does not exist!")
            sys.exit(1)

    # Create a new server
    qotd_server = Server(17, quotes)
    qotd_server.run_server()


class Server:
    def __init__(self, port, quotes):
        self.port = port
        self.quotes = quotes
        self.we_have_quote_for_every_day = True if len(quotes) == 365 or len(quotes) == 366 else False

    def run_server(self):
        self.start_listening_tcp()

    def start_listening_tcp(self):
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
            s.bind(('localhost', self.port))
            s.listen()
            try:
                while True:
                    print("Waiting for a TCP connection...")
                    client_sock, client_addr = s.accept()
                    print(f"Connected to TCP IP: {client_addr[0]}:{client_addr[1]}")

                    thread = threading.Thread(target=self.handle_request_tcp, args=(client_sock, client_addr))
                    thread.daemon = True
                    thread.start()

            except Exception as e:
                print(f"Socket exception {e}")
                self.start_listening_tcp()

    def send_message(self, sock, message):
        encoded_message = message.encode("utf-8")
        header = struct.pack("!H", len(encoded_message))
        message = header + encoded_message
        sock.sendall(message)

    def handle_request_tcp(self, client_sock, client_addr):
        try:
            content = self.get_quote()
            self.send_message(client_sock, content)
        except Exception as e:
            print(f"Exception {e}")

    def get_quote(self):
        if self.we_have_quote_for_every_day:
            content = self.quotes[datetime.now().timetuple().tm_yday]
        else:
            content = self.quotes[random.randint(0, len(self.quotes))]
        return content


if __name__ == "__main__":
    main(sys.argv[1:])  # first argument is path of the file
