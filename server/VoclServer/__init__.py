from flask import Flask, request, Response
from matplotlib.backends.backend_agg import FigureCanvasAgg as FigureCanvas
from matplotlib.figure import Figure
import io

app = Flask(__name__)

img = None

@app.route("/", methods=["GET"])
def index():
    return '<img src="/data" alt="my plot">'

@app.route("/data", methods=["POST", "GET"])
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
        FigureCanvas(fig).print_png(output)
        img = output
    
    if (img is not None):
        return Response(img.getvalue(), mimetype='image/png')
    
    return "nothing"