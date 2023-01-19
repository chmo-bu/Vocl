import numpy as np
from matplotlib.lines import Line2D
import matplotlib.pyplot as plt
import matplotlib.animation as animation
import pyaudio

class Scope:
    def __init__(self, ax, maxt=2, dt=0.02):
        self.ax = ax
        self.dt = dt
        self.maxt = maxt
        self.tdata = [0]
        self.ydata = [0]
        self.line = Line2D(self.tdata, self.ydata)
        self.ax.add_line(self.line)
        self.ax.set_ylim(-.1, 1.1)
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


def emitter(p=0.1):
    # PyAudio INIT:
    FORMAT = pyaudio.paFloat32
    CHANNELS = 1
    RATE = 16000
    CHUNK = 1024
    pa = pyaudio.PyAudio()

    stream = pa.open(format = FORMAT,
            channels = CHANNELS, 
            rate = RATE, 
            input = True,
            output = False,
            frames_per_buffer = CHUNK)

    """Return a random value in [0, 1) with probability p, else 0."""
    while True:
        ret = stream.read(CHUNK)
        yield np.frombuffer(ret, np.float32)

if __name__ == "__main__":
    fig, ax = plt.subplots()
    scope = Scope(ax, maxt=1000)

    # pass a generator in "emitter" to produce data for the update func
    ani = animation.FuncAnimation(fig, scope.update, emitter, interval=50,
                                blit=True)

    plt.show()