from flask import Flask, request
import struct

app = Flask(__name__)

@app.route("/", methods=["POST"])
def hello_world():
    print(request.form['data'])
    return "<p>Hello, World!</p>"
