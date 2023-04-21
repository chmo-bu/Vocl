from flask import Flask, request, Response
from flask import Flask, request, Response, jsonify
from matplotlib.backends.backend_agg import FigureCanvasAgg as FigureCanvas
from matplotlib.figure import Figure
import tensorflow as tf
#import tensorflow_io as tfio
import numpy as np
import pandas as pd
import io
import csv

app = Flask(__name__)

img = None

@app.route("/", methods=["GET"])
def index():
    return "<img src=\"/send\" alt=\"my plot\">"

@app.route("/data", methods=["POST"])
def handle_request():
    global img
    interpreter = tf.lite.Interpreter('./audioML.tflite')

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
        
        input_details = interpreter.get_input_details()
        waveform_input_index = input_details[0]['index']
        output_details = interpreter.get_output_details()
        scores_output_index = output_details[0]['index']
        embeddings_output_index = output_details[1]['index']
        spectrogram_output_index = output_details[2]['index']

        waveform = np.array(y).astype('float32')
	
        interpreter.resize_tensor_input(waveform_input_index, [len(waveform)], strict=True)
        interpreter.allocate_tensors()
        interpreter.set_tensor(waveform_input_index, waveform)
        interpreter.invoke()
        scores, embeddings, spectrogram = (
            interpreter.get_tensor(scores_output_index),
            interpreter.get_tensor(embeddings_output_index),
            interpreter.get_tensor(spectrogram_output_index))
        max_score = scores.mean(axis=0).argmax()
        if max_score < 40:
            classification = "yelling"
        elif max_score > 55:
            classification = "clapping"
        else:
            classification = "stomping"
        # response = Flask.make_response(jsonify({"classification": classification}), 200)
        # response.headers['Content-Type'] = 'application/json'
        return jsonify({"classification": classification})
        # return Response(jsonify({"classification": classification}), mimetype='application/json')
    
    return "nothing"
    

def _send_data():
    while True:
        if (img is None):
            continue
        yield img
    
@app.route("/send", methods=["GET"])
def send_data():
    return Response(_send_data(), mimetype='multipart/x-mixed-replace; boundary=frame')