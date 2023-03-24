using System.Collections;
using System.Collections.Generic;
using System;
using Filter;
using static MovingAverage.MovingAverage;
// using StreamingMic;
using UnityEngine;
using TCPSocket;
using UnityEngine.Networking;
//using static Game3.completeTask;

// namespace ClapDetector {
public class ClapDetector : MonoBehaviour
{
    public bool done;
    // public StreamingMic.StreamingMic streamingMic = new StreamingMic.StreamingMic();
    public StreamingMic streamingMic;
    
    // clap detection flags
    private bool tempState;
    private DateTime lastTime;
    private float resonance=Mathf.Sqrt(2);

    FilterButterworth hp;
    FilterButterworth lp;

    /* variables for peak counting */
    // to hold filtered audio data for analysis
    private float[] filtered = new float[48000];
    // calculate indices for audio data
    private int start;
    private int stop;
    private float [] tail;
    private float[] head;
    // array to store peaks
    private int[] m = new int[7];
    private float[] m_amp = new float[7];
    // create array to compare peaks
    private float[] sorted = new float[7];

    private int cid = 0; // runnable id

    private TCPClient client;

    // private byte[] byteData = new byte[48000*sizeof(float)];

    // Constructor
    // public ClapDetector()
    // {
    //     hp = new FilterButterworth(600, 16000, FilterButterworth.PassType.Highpass, resonance);
    //     lp = new FilterButterworth(3000, 16000, FilterButterworth.PassType.Lowpass, resonance);
    // }

    void Start()
    {
        // client = new TCPClient();
        hp = new FilterButterworth(600, 16000, FilterButterworth.PassType.Highpass, resonance);
        lp = new FilterButterworth(3000, 16000, FilterButterworth.PassType.Lowpass, resonance);
    }

    public void Listen() {
        done = false;
        streamingMic.StartRecording();
        lastTime = System.DateTime.Now;
        cid = Runnable.Run(CountPeaks());
    }

    public void Stop() {
        Runnable.Stop(cid);
        cid = 0;
        streamingMic.StopRecording();
    }

    public bool checkCount(int claps)
    {
        if (claps == 3)
        {
            return true;
        }
        else
        {
            return false;
        }
        
    }

    private IEnumerator CountPeaks() {
        while (true) {
        // while (!done) {
            
            // wait until 48000 samples
            yield return new WaitUntil(() => 
                (streamingMic.idx == 48000));

            // Debug.Log(streamingMic.idx);

            streamingMic.idx = 0;

            // Debug.Log(streamingMic._samples.Length);

            // get sample window
            Array.Copy(streamingMic._samples, filtered, filtered.Length);

            // initialize first two inputs of butterworth filters
            hp.filterInit(filtered[0], filtered[1]);
            lp.filterInit(filtered[0], filtered[1]);

            // high pass at 600hz and next low pass at 3000hz
            for (int i=2; i<48000; i++) {
                filtered[i] = hp.Update(filtered[i]);
                filtered[i] = lp.Update(filtered[i]);
            }

            // run a 5-point moving average
            filtered = MovingAverage.MovingAverage.run(filtered, 5);

            // Buffer.BlockCopy(filtered, 0, byteData, 0, byteData.Length);
            
            // client.SendMessage(byteData);

            // Debug.Log("data = " + String.Join(",",
            // new List<float>(filtered)
            // .ConvertAll(i => i.ToString())
            // .ToArray()));

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

            // Debug.Log(m_amp[0] + " " + m_amp[1] + " " + m_amp[2] + " " + 
            //     m_amp[3] + " " + m_amp[4] + " " + m_amp[5] + " " + m_amp[6]);

            // Debug.Log(m[0] + " " + m[1] + " " + m[2] + " " + 
            //     m[3] + " " + m[4] + " " + m[5] + " " + m[6]); 

            // copy values
            Array.Copy(m_amp, sorted, m_amp.Length);

            // sort values (largest -> smallest)
            qsortr(sorted, 0, sorted.Length-1);

            int clapCount = 1;
            
            // peak comparison procedure
            for (int i=2; i<6; i++) {
                // compute the average of the i largest peaks
                float avgMax = 0;
                for (int j=0; j<i; j++) {
                    avgMax += sorted[i];
                }
                avgMax /= i;

                // find the smallest amplitude of unsorted peaks
                float minAmp = m_amp[0];
                for (int j=0; j<i; j++) {
                    if (m_amp[i] < minAmp) {
                        minAmp = m_amp[i];
                    }
                }

                // Debug.Log("minimum: " + minAmp + " threshold: " + 0.7*avgMax);

                // if the smallest amplitude is greater than 0.7 * average of the i largest peaks increment clap count
                if (minAmp > (0.7 * avgMax)) {
                    clapCount++;
                } else {break;}
            }
            Debug.Log("clap count: " + clapCount);
            done = checkCount(clapCount);

            WWWForm formData = new WWWForm();
            formData.AddField("data", String.Join(",", filtered));
            formData.AddField("peaks", String.Join(",", m));
            formData.AddField("claps", clapCount.ToString());
            
            using (UnityWebRequest www = UnityWebRequest.Post("http://localhost:8052/data", formData)) {
                yield return www.SendWebRequest();
            }

            yield return new WaitForSeconds(0.02f);
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
// }

