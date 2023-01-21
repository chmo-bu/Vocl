import numpy as np
from matplotlib.lines import Line2D
import matplotlib.pyplot as plt
import matplotlib.animation as animation
import pyaudio
import time

class HighPass:
    MINTHRESHOLD = 0.002
    NUM_SECONDS = 5 #to calculate the threshold we need to average m_level over several seconds (the duration of vocalization).
    NUM_POINTS = NUM_SECONDS * 50

    def __init__(self,
        resonance=np.sqrt(2),
        frequency=500,
        sampleRate=16000):

        self.resonance = resonance
        self.frequency = frequency
        self.sampleRate = sampleRate

        self.c = 0 
        self.a1 = 0
        self.a2 = 0
        self.a3 = 0 
        self.b1 = 0 
        self.b2 = 0

        self.inputHistory = np.zeros((2))
        self.outputHistory = np.zeros((3))
        self.prevLevels = np.zeros((HighPass.NUM_POINTS))
        self.rmsLevel = 0
        self.updateCounter = 0
        self.m_5buff = 0
        self.max_mic_level_detected = HighPass.MINTHRESHOLD
        self.bfirstDropWasDetected = False
        self.m_threshold = 0

    def filter(self):
        self.c = np.tan(np.pi * self.frequency / self.sampleRate)
        self.a1 = 1.0 / (1.0 + self.resonance * self.c + self.c * self.c)
        self.a2 = -2.0 * self.a1
        self.a3 = self.a1
        self.b1 = 2.0 * (self.c * self.c - 1.0) * self.a1
        self.b2 = (1.0 - self.resonance * self.c + self.c * self.c) * self.a1

    def update(self, newInput):
        newOutput = self.a1 * newInput + self.a2 * self.inputHistory[0] \
            + self.a3 * self.inputHistory[1] - self.b1 * self.outputHistory[0] \
            - self.b2 * self.outputHistory[1]

        self.inputHistory[1] = self.inputHistory[0]
        self.inputHistory[0] = newInput

        self.outputHistory[2] = self.outputHistory[1]
        self.outputHistory[1] = self.outputHistory[0]
        self.outputHistory[0] = newOutput

    def getValue(self):
        return self.outputHistory[0]

    def InitializePrevLevel(self):
        self.prevLevels = np.zeros(HighPass.NUM_POINTS)
        startingThreathold = max(2*self.rmsLevel, HighPass.MINTHRESHOLD)
        for i in range(HighPass.NUM_POINTS):
            self.prevLevels[i]=startingThreathold; #=MINTHRESHOLD; #if we start with MINTHRESHOLD, the ball gets stuck on top until the threshold picks up 

    def FillPrevLevel(self):
    #
        for i in range(HighPass.NUM_POINTS-1):
            self.prevLevels[i] = self.prevLevels[i+1]

        self.prevLevels[HighPass.NUM_POINTS-1] = self.rmsLevel

        for i in range(HighPass.NUM_POINTS-5, HighPass.NUM_POINTS):
            self.m_5buff += self.prevLevels[i]
            
        self.m_5buff /= 5;#the shortest words last for at least five buffers (usually longer), so we only need to react to prolonged crossing of a threshold
    

    def Threshold(self, multiplier) -> float:
    #Re-calculate current threshold and add the new self.rmsLevel to the array. The threshold cannot be constant as room can suddenly become noisy (TV is turned on). The fact that the ball will be stuck at the ceiling will be interpreted as an error. 
       #we enter this function every 20ms. There is really no need to enter it so often. We can do it every 100ms.
        fiveBuffers_BeforeLastSixBuffers=0

        for i in range(HighPass.NUM_POINTS-11, HighPass.NUM_POINTS-6):
            fiveBuffers_BeforeLastSixBuffers += self.prevLevels[i]  
        
        fiveBuffers_BeforeLastSixBuffers/=5
        self.updateCounter += 1

        if (self.m_5buff < fiveBuffers_BeforeLastSixBuffers/4): #sudden drop in level -> there must have been a vocalization
        #when vocalization stops (sharp drop in amplitude), we need to encourage a second vocalization by dropping the threshold.
            self.bfirstDropWasDetected = True #before the first drop was detected we shall not set threshold to max_mic_level_detected/4 since max_mic_level_detected is not a realistice max_mic_level_detected.
            #Debug.Log("Sudden drop in self.rmsLevel"+"; self.rmsLevelUnfilt="+(self.rmsLevelUnfilt*1000).ToString("0.0") + "; self.rmsLevel="+(self.rmsLevel*1000).ToString("0.0") +"; self.m_threshold="+(self.m_threshold*1000).ToString("0.0"));
            reduction_mult=0.5
            self.m_threshold *= reduction_mult
            if (self.m_threshold<HighPass.MINTHRESHOLD): 
                self.m_threshold = HighPass.MINTHRESHOLD
            for i in range(HighPass.NUM_POINTS):
                self.prevLevels[i]=self.m_threshold; #lower down all historic records
            #Debug.Log("Reduced self.prevLevels[i] by "+reduction_mult*100+"% to self.m_threshold="+(self.m_threshold*1000).ToString("0.0"));
        
        else:
        #to calculate the threshold we need to average self.rmsLevel over several seconds. 
            avg = 0
            for i in range(HighPass.NUM_POINTS):
                avg += self.prevLevels[i] 

            avg /= HighPass.NUM_POINTS#calculate the average

            if(not self.bfirstDropWasDetected):
                self.m_threshold=avg*multiplier*2;#until the fist vocalization was detected keep threshold twice as big as normal.
            else:
                self.m_threshold=avg*multiplier;# ==30% greater than average level for the ball

            if (self.m_threshold < HighPass.MINTHRESHOLD):
                self.m_threshold = HighPass.MINTHRESHOLD # Debug.Log(self.updateCounter+". m_5b="+(self.m_5buff*1000).ToString("0.0")+"; rmsUnfilt="+(self.rmsLevelUnfilt*1000).ToString("0.0") +"; rms="+(self.rmsLevel*1000).ToString("0.0")+"; avg="+(avg*1000).ToString("0.0")+"; self.m_threshold=MINTHRESHOLD="+(self.m_threshold*1000).ToString("0.0"));
            elif ((self.m_threshold > self.max_mic_level_detected/4) and self.bfirstDropWasDetected):
                self.m_threshold = self.max_mic_level_detected/4 #Debug.Log(self.updateCounter+". m_5b="+(self.m_5buff*1000).ToString("0.0")+"; rmsUnfilt="+(self.rmsLevelUnfilt*1000).ToString("0.0")+"; rms="+(self.rmsLevel*1000).ToString("0.0") +"; avg="+(avg*1000).ToString("0.0")+"; self.m_threshold=max_mic_level_detected/4="+(self.m_threshold*1000).ToString("0.0"));#the problem with child sayinh "UUUUUUUUUU". The threshold gets too high and it looks like an error with unresponsive game.
            #else Debug.Log(self.updateCounter+". m_5b="+(self.m_5buff*1000).ToString("0.0")+"; rmsUnfilt="+(self.rmsLevelUnfilt*1000).ToString("0.0") +"; rms="+(self.rmsLevel*1000).ToString("0.0") +"; avgAllHistoryBuffer="+(avg*1000).ToString("0.0")+"; self.m_threshold=avg over 5sec x MULTIPLIER="+(self.m_threshold*1000).ToString("0.0"));

            for i in range(HighPass.NUM_POINTS-1):
                self.prevLevels[i] = self.prevLevels[i+1]  
        
        self.prevLevels[HighPass.NUM_POINTS-1] = self.rmsLevel

        for i in range(HighPass.NUM_POINTS-5, HighPass.NUM_POINTS):
            self.m_5buff += self.prevLevels[i];  
        
        self.m_5buff /= 5#the shortest words last for at least five buffers (usually longer), so we only need to react to prolonged crossing of a threshold

        return self.m_threshold
       

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

        self.filter = HighPass(frequency=5000)
        self.filter.filter()
        self.waitTime = 1.5
        self.prev = time.time()
        self.bInitializePrevLevel = False

    def update(self, data):
        SUM = 0
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

            self.filter.update(y)
            res = self.filter.getValue()

            SUM += (res * res)

            self.tdata.append(t)
            self.ydata.append(res)
            self.line.set_data(self.tdata, self.ydata)
            
        self.filter.rmsLevel = np.sqrt(SUM / len(data))

        if(self.filter.rmsLevel>self.filter.max_mic_level_detected):
            self.filter.max_mic_level_detected=self.filter.rmsLevel #keep an eye on the maximum self.rmsLevel ever reach, out threshold must be way lower 
        if(self.filter.max_mic_level_detected<HighPass.MINTHRESHOLD*10):
            self.filter.max_mic_level_detected=HighPass.MINTHRESHOLD*10 #we cannot set max_mic_level_detected too low and at the start, when there were no vocalizations it could be quite low

        if(not self.bInitializePrevLevel):
            self.filter.InitializePrevLevel()
            self.bInitializePrevLevel=True
            return self.line, 

        timer = time.time()

        if(abs(timer - self.prev) < self.waitTime):
            self.filter.FillPrevLevel()
            return self.line,

        # print(self.filter.m_5buff, self.filter.Threshold(1.5))

        if (self.filter.m_5buff > self.filter.Threshold(1.5)):
            print("[{:.2f}]: detected!".format(timer))
            self.line.set_color("r")
        else:
            self.line.set_color('b')

        return self.line,


class Emitter:
    def __init__(self):
        # PyAudio INIT:
        FORMAT = pyaudio.paFloat32
        CHANNELS = 1
        RATE = 16000
        self.CHUNK = 1024
        pa = pyaudio.PyAudio()

        self.stream = pa.open(format = FORMAT,
            channels = CHANNELS, 
            rate = RATE, 
            input = True,
            output = False,
            frames_per_buffer = self.CHUNK)

    def emit(self):
        while True:
            ret = self.stream.read(self.CHUNK)
            yield np.frombuffer(ret, np.float32)

if __name__ == "__main__":
    fig, ax = plt.subplots()
    scope = Scope(ax, maxt=1000)
    emitter = Emitter()

    # pass a generator in "emitter" to produce data for the update func
    ani = animation.FuncAnimation(fig, scope.update, emitter.emit, interval=50,
                                blit=True)

    plt.show()