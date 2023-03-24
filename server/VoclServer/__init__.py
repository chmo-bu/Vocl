from flask import Flask, request, Response
from matplotlib.backends.backend_agg import FigureCanvasAgg as FigureCanvas
from matplotlib.figure import Figure
import io

app = Flask(__name__)

img = None

@app.route("/", methods=["GET"])
def index():
    return "<img src=\"/send\" alt=\"my plot\">"

@app.route("/data", methods=["POST"])
def handle_request():
    global img
    
    if (request.method == "POST"):
        fig = Figure()
        axis = fig.add_subplot(1, 1, 1)
        data = request.form["data"]
        peaks = request.form["peaks"]
        peaks = peaks.split(',')
        peaks = [int(d) for d in peaks]
        peaks = [p for p in peaks if p >= 0]
        y = data.split(',')
        y = [float(d) for d in y]
        t = range(0, len(y))
        axis.plot(t, y, label='audio')
        axis.plot(peaks, [y[p] for p in peaks], marker='x', ls="", label="peaks")
        axis.set_ylim(-1., 1.)
        axis.legend()
        axis.set_title("{} clap(s)".format(request.form["claps"]))
        output = io.BytesIO()
        FigureCanvas(fig).print_jpg(output)
        img = output.getvalue()
        img = (b'--frame\r\n'
               b'Content-Type: image/jpeg\r\n\r\n' + img + b'\r\n')
    
    return "nothing"

def _send_data():
    while True:
        if (img is None):
            continue
        yield img
    
@app.route("/send", methods=["GET"])
def send_data():
    return Response(_send_data(), mimetype='multipart/x-mixed-replace; boundary=frame')
