# echo-server.py

import socket
import struct
from matplotlib.lines import Line2D
import matplotlib.pyplot as plt
import matplotlib.animation as animation
import serial
import time

HOST = "localhost"  # Standard loopback interface address (localhost)
PORT = 8052  # Port to listen on (non-privileged ports are > 1023)

# def connect(sock, host, port):
#     sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
#     sock.bind((HOST, PORT))
#     sock.listen()
#     conn, addr = sock.accept()
#     return sock, conn, addr;

class Scope:
    def __init__(self, ax, maxt=2, dt=0.02):
        self.ax = ax
        self.dt = dt
        self.maxt = maxt
        self.tdata = [0]
        self.ydata = [0]
        self.line = Line2D(self.tdata, self.ydata)
        self.ax.add_line(self.line)
        self.ax.set_ylim(-1, 1)
        self.ax.set_xlim(0, self.maxt)

    def update(self, data):
        for y in data:

            lastt = self.tdata[-1]
            if lastt >= self.tdata[0] + self.maxt:  # reset the arrays
                self.tdata = [self.tdata[-1]]
                self.ydata = [self.ydata[-1]]
                self.ax.set_xlim(self.tdata[0], self.tdata[0] + self.maxt)
                self.ax.figure.canvas.draw()

            # This slightly more complex calculation avoids floating-point issues
            # from just repeatedly adding `self.dt` to the previous value.
            t = self.tdata[0] + len(self.tdata) * self.dt

            self.tdata.append(t)
            self.ydata.append(y)
            self.line.set_data(self.tdata, self.ydata)
        return self.line,

class emitter:
    def __init__(self):
        self.s = socket.socket(socket.AF_INET, 
                          socket.SOCK_STREAM)
        self.conn = None
        self.addr = None
        while (self.conn is None):
            self.s.bind((HOST, PORT))
            self.s.listen()
            self.conn, self.addr = self.s.accept()
        print(f"Connected by {self.addr}")    
        
    def emit(self):
        while True:
            time.sleep(0.1)
            data = self.conn.recv(16000)
            if not data:
                break
            
            # print(len(data) // 4, len(data))
            data = struct.unpack(f"<{len(data) // 4}f", data)
            
            # if (min(data) != -2.0):
            #     continue
                        
            # res = []
            # for d in data:
            #     if (d == -2.0):
            #         break
            #     res.append(d)
            res = data
            
            # print(res)
            
            self.conn.sendall(b"received")
            yield res

if __name__ == "__main__":
    # with serial.serial_for_url("http://localhost:8052") as s:
    #     while (True):
    #         print(len(s.inWaiting()))
    # e = emitter()
    # e.emit() 
    try:
        e = emitter()
        fig, ax = plt.subplots()
        scope = Scope(ax, maxt=2000)

        # pass a generator in "emitter" to produce data for the update func
        ani = animation.FuncAnimation(fig, scope.update, e.emit(), interval=50,
                                    blit=True, save_count=100)

        plt.show()
    except:
        print("exiting...")
        