using System.Collections;
using System.Collections.Generic;
using System;
using Filter;
using static MovingAverage.MovingAverage;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//TODO: try https://stackoverflow.com/questions/22583391/peak-signal-detection-in-realtime-timeseries-data/53614452#53614452
// TODO: bandpass filter? https://stackoverflow.com/questions/10373184/bandpass-butterworth-filter-implementation-in-c
public class AudioTest : MonoBehaviour
{
    public StreamingMic streamingMic;
    public TMP_Text status;

    private float waitTime = 1.5f;
    private float timer = 0.0f;
    private bool bInitializePrevLevel = false;
    
    // clap detection flags
    private bool lastState = false;
    private bool currentState = false;
    private bool tempState;
    private DateTime lastTime;
    private double clapDelay = 2000.0;
    private int half = (int) (1.5 * 16000);

    private float resonance=Mathf.Sqrt(2);

    FilterButterworth hp;
    FilterButterworth lp;

    private float[] window = new float[48000];

    // Start is called before the first frame update
    void Start()
    {
        hp = new FilterButterworth(600, 16000, FilterButterworth.PassType.Highpass, resonance);
        lp = new FilterButterworth(3000, 16000, FilterButterworth.PassType.Lowpass, resonance);
        lastTime = System.DateTime.Now;
        // status.text = "hello";
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.log("here");
    }

    void FixedUpdate ()
    {//0.02 seconds (50 calls per second) is the default time between calls
        // float forceUp = 0;
        // constForce.force = Vector3.zero;

        if(!bInitializePrevLevel){streamingMic.InitializePrevLevel(); bInitializePrevLevel=true; return;}
        timer += Time.deltaTime; if(timer < waitTime) {streamingMic.FillPrevLevel(); return; }//wait for 1.5 seconds before responding to sound. This will allow the threashold to settle. 

        tempState = streamingMic.m_5buff > streamingMic.Threshold(2.0f);
        // tempState = streamingMic.m_5buff > streamingMic.Threshold(4.0f);
        // tempState = streamingMic.m_5buff > 0.20;

        if (tempState && ((double)((TimeSpan)(System.DateTime.Now
         - lastTime)).TotalMilliseconds) > clapDelay) {
            lastTime = System.DateTime.Now;
            if (streamingMic._samples._length >= half) {
                // to hold filtered audio data for analysis
                float[] filtered = new float[48000];

                // calculate indices for audio data
                int start = streamingMic._samples._head - half;
                int stop = streamingMic._samples._head;

                // get 1.5 seconds before threshold detected
                float [] tail = streamingMic._samples.slice(start, stop);

                // wait for at least 1.5 seconds of audio data
                while ((streamingMic._samples._head - stop < half) && 
                    (streamingMic._samples._length - stop + streamingMic._samples._head < half));

                // get 1.5 latter seconds
                float[] head = streamingMic._samples.slice(stop, stop + half);

                Array.Copy(tail, 0, window, 0, half); // copy first 1.5 seconds
                Array.Copy(head, 0, window, half, half); // copy latter 1.5 seconds

                // initialize first two inputs of butterworth filters
                hp.filterInit(window[0], window[1]);
                lp.filterInit(window[0], window[1]);

                // high pass at 600hz and next low pass at 3000hz
                for (int i=2; i<48000; i++) {
                    filtered[i] = hp.Update(window[i]);
                    filtered[i] = lp.Update(filtered[i]);
                }

                // run a 5-point moving average
                filtered = MovingAverage.MovingAverage.run(filtered, 5);

                // array to store peaks
                int[] m = new int[7];
                float[] m_amp = new float[7];

                // create array to compare peaks
                float[] sorted = new float[7];

                // peak finding procedure:
                // NOTE: 0.25 * 3 seconds = 12000 samples b/c sample rate is 16000
                // first -> max of array
                // second -> max to left of (first - 0.25 * 3 seconds)
                // third -> max to right of (first + 0.25 * 3 seconds)
                // fourth -> max to left of (second - 0.25 * 3 seconds)
                // fifth -> between second and first (second + 0.25 * 3 seconds -> first - 0.25 * 3 seconds)
                // sixth -> between first and third (first + 0.25 * 3 seconds -> third - 0.25 * 3 seconds)
                // seventh -> right to third of (third + 0.25 * 3 seconds)

                m[0] = streamingMic.argmax(filtered);
                m[1] = streamingMic.argmax(filtered, 0, Math.Max(0, m[0] - 12000));
                m[2] = streamingMic.argmax(filtered, Math.Min(m[0] + 12000, 47999), 48000);
                m[3] = streamingMic.argmax(filtered, 0, Math.Max(0, m[1] - 12000));
                m[4] = -1;
                m[5] = -1;

                if (m[0] - m[1] < 1) {
                    m[4] = streamingMic.argmax(filtered,  Math.Min(m[1] + 12000, 47999), Math.Max(0, m[0] - 12000));
                }

                if (m[2] - m[0] < 1) {
                    m[5] = streamingMic.argmax(filtered,  Math.Min(m[0] + 12000, 47999), Math.Max(0, m[2] - 12000));
                }

                m[6] = streamingMic.argmax(filtered, Math.Min(m[2] + 12000, 47999), 48000);

                // convert indices to values
                for (int i=0; i<7; i++) {
                    int idx = m[i];
                    if (idx != -1) {
                        m_amp[i] = filtered[idx];
                    } else {m_amp[i] = 0;} // if m[4] or m[5] are negative make sure they are at the end when sorted
                }

                // copy values
                Array.Copy(m_amp, sorted, m_amp.Length);

                // sort values (largest -> smallest)
                qsortr(sorted, 0, sorted.Length-1);

                int clapCount = 1;
                
                // peak comparison procedure
                for (int i=1; i<6; i++) {
                    // compute the average of the i largest peaks
                    float avgMax = 0;
                    for (int j=0; j<i; j++) {
                        avgMax += sorted[i];
                    }
                    avgMax /= (i + 1);

                    // find the smallest amplitude of unsorted peaks
                    float minAmp = m_amp[0];
                    for (int j=0; j<i; j++) {
                        if (m_amp[i] < minAmp) {
                            minAmp = m_amp[i];
                        }
                    }

                    // if the smallest amplitude is greater than 0.7 * average of the i largest peaks increment clap count
                    if (minAmp > (0.7 * avgMax)) {
                        clapCount++;
                    }
                }
                Debug.Log("clap count: " + clapCount);
            }
        }
    }

    static int partition(float[] arr, int low, int high) {
        float temp;
        float pivot = arr[high];
  
        // index of larger element
        int i = (low - 1);
        for (int j = low; j <= high - 1; j++) {
            // If current element is greater
            // than or equal to pivot
            if (arr[j] >= pivot) {
                i++;
  
                // swap arr[i] and arr[j]
                temp = arr[i];
                arr[i] = arr[j];
                arr[j] = temp;
            }
        }
  
        // swap arr[i+1] and arr[high]
        // (or pivot)
  
        temp = arr[i + 1];
        arr[i + 1] = arr[high];
        arr[high] = temp;
  
        return i + 1;
    }
  
    /* A[] --> Array to be sorted, 
    l --> Starting index, 
    h --> Ending index */
    static void qsortr(float[] arr, int l, int h) {
        // Create an auxiliary stack
        int[] stack = new int[h - l + 1];
  
        // initialize top of stack
        int top = -1;
  
        // push initial values of l and h to
        // stack
        stack[++top] = l;
        stack[++top] = h;
  
        // Keep popping from stack while
        // is not empty
        while (top >= 0) {
            // Pop h and l
            h = stack[top--];
            l = stack[top--];
  
            // Set pivot element at its
            // correct position in
            // sorted array
            int p = partition(arr, l, h);
  
            // If there are elements on
            // left side of pivot, then
            // push left side to stack
            if (p - 1 > l) {
                stack[++top] = l;
                stack[++top] = p - 1;
            }
  
            // If there are elements on
            // right side of pivot, then
            // push right side to stack
            if (p + 1 < h) {
                stack[++top] = p + 1;
                stack[++top] = h;
            }
        }
    }
}
