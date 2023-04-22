using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Barracuda;
using UnityEngine;
using UnityEngine.Events;
using Filter;

namespace YamNetUnity
{
    public class YamNet : MonoBehaviour
    {
        private const int NumClasses = 521;
        private const int AudioBufferLengthSec = 3;

        public NNModel modelAsset;
        public UnityEvent<int, string, float> onResult;
        public int sampleRate;

        private Queue<float> samples;
        FilterButterworth hp; // high pass filter
        public float m_level;//=maximum in this buffer=old implementation; I later switched to avg of five consequitive buffers based on RMS
        public float rmsLevel, rmsLevelUnfilt;
        public float m_5buff;//=avg of five consequitive buffers
        private int updateCounter=0;

        private const float MINTHRESHOLD = 0.002f;//0.04f;
        private const int NUM_SECONDS = 5;//to calculate the threshold we need to average m_level over several seconds (the duration of vocalization).
        private const int NUM_POINTS = NUM_SECONDS * 50;//FixedUpdate occurres 50 times per second.
        public float m_threshold;
        private float[] prevLevels;
        private float max_mic_level_detected=MINTHRESHOLD;
        private bool bFirstDropWasDetected=false;
        private readonly float resonance=Mathf.Sqrt(2);
        private readonly float frequency=500;//highpass cutoff frequency (Hz)
        private readonly int sr=16000;//same as m_nRecordingHZ
        private bool bInitializePrevLevel = false;
        private int cid = 0;

        public void StartMicrophone()
        {
            int minFreq;
            int maxFreq;

            foreach (var device in Microphone.devices)
            {
                Microphone.GetDeviceCaps(device, out minFreq, out maxFreq);
                Debug.Log($"Name: {device} MinFreq: {minFreq} MaxFreq: {maxFreq}");
            }

            this.microphoneDeviceName = Microphone.devices[0];
            Microphone.GetDeviceCaps(microphoneDeviceName, out minFreq, out maxFreq);
            this.sampleRate = AudioFeatureBuffer.InputSamplingRate;
            if (minFreq != 0 && maxFreq != 0)
            {
                this.sampleRate = Mathf.Clamp(this.sampleRate, minFreq, maxFreq);
            }

            this.clip = Microphone.Start(microphoneDeviceName, true, AudioBufferLengthSec, this.sampleRate);
            this.audioOffset = 0;
            this.cid = Runnable.Run(DetectThreshold());
        }

        public void StopMicrophone() {
            if (this.clip != null) {
                Microphone.End(this.microphoneDeviceName);
                this.clip = null;
                this.microphoneDeviceName = null;
                Runnable.Stop(this.cid);
                this.cid = 0;
                Debug.Log("StopRecording() ");
            }
        }

        public void SendInput(float[] waveform)
        {
            waveform = featureBuffer.Resample(waveform, sampleRate);
            int offset = 0;
            while (offset < waveform.Length)
            {
                int written = this.featureBuffer.Write(waveform, offset, waveform.Length - offset);
                offset += written;
                while (this.featureBuffer.OutputCount >= 96 * 64)
                {
                    try
                    {
                        var features = new float[96 * 64];
                        Array.Copy(this.featureBuffer.OutputBuffer, 0, features, 0, 96 * 64);
                        this.OnPatchReceived(features);
                    }
                    finally
                    {
                        this.featureBuffer.ConsumeOutput(48 * 64);
                    }
                }
            }        
        }

        private Model model;
        private IWorker worker;
        private AudioClip clip;
        private string microphoneDeviceName;
        private int audioOffset;
        private AudioFeatureBuffer featureBuffer;
        private string[] classMap;

        private void Awake()
        {
            this.microphoneDeviceName = null;
            this.onResult = new UnityEvent<int, string, float>();
        }

        // Start is called before the first frame update
        void Start()
        {
            hp = new FilterButterworth(frequency, sr, 
                FilterButterworth.PassType.Highpass, resonance);
            samples = new Queue<float>();

            if (modelAsset)
            {
                model = ModelLoader.Load(modelAsset);
                worker = WorkerFactory.CreateWorker(model, WorkerFactory.Device.GPU);
            }

            this.classMap = new string[NumClasses];

            TextAsset classMapData = (TextAsset)Resources.Load("yamnet_class_map", typeof(TextAsset));
            using (var reader = new StringReader(classMapData.text))
            {
                string line = reader.ReadLine(); // Discard the first line.
                while ((line = reader.ReadLine()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        string[] parts = line.Split(',');
                        int classId = int.Parse(parts[0]);
                        this.classMap[classId] = parts[2];
                    }
                }
            }

            this.featureBuffer = new AudioFeatureBuffer();
        }

        // Update is called once per frame
        void Update()
        {
            if (this.microphoneDeviceName != null)
            {
                int pos = Microphone.GetPosition(microphoneDeviceName);
                if (pos < audioOffset)
                {
                    pos = clip.samples;
                }
                if (pos > audioOffset)
                {
                    int nsamplesarray = pos - audioOffset;
                    float[] data = new float[nsamplesarray];
                    this.clip.GetData(data, this.audioOffset);
                    this.audioOffset = pos;
                    if (this.audioOffset >= clip.samples)
                    {
                        this.audioOffset = 0;
                    }
                    for (int i=0; i<nsamplesarray; i++) { 
                        if (samples.Count == 48000) {
                            samples.Dequeue();
                        }
                        samples.Enqueue(data[i]);
                    }
                    this.SendInput(data);
                }
            }
        }

        private IEnumerator DetectThreshold() {
            while (cid != 0 && this.clip != null) {
                if (samples.Count == 48000) {
                    float[] filtered = samples.ToArray();

                    hp.filterInit(filtered[0], filtered[1]);
                    // StringBuilder sb = new StringBuilder();

                    for (int i=2; i<48000; i++) { 
                        filtered[i]=hp.Update(filtered[i]);
                    }
                    // Debug.Log(_samples._head);

                    //calculate root mean square - it is much more reliable than the maximum
                    float sum=0;
                    //for (int i=0; i<nsamplesarray; i++){sum += samples[i]*samples[i];} rmsLevelUnfilt = Mathf.Sqrt(sum/nsamplesarray); sum=0;//unfiltered rms = square root of average
                    for (int i=2; i<48000; i++){sum += filtered[i]*filtered[i];} rmsLevel = Mathf.Sqrt(sum/(48000-2)); //filtered rms = square root of average
                    rmsLevel = Mathf.Sqrt(sum/48000); // rms = square root of average
                    if(rmsLevel>max_mic_level_detected) max_mic_level_detected=rmsLevel;//keep an eye on the maximum rmsLevel ever reach, out threshold must be way lower 
                    if(max_mic_level_detected<MINTHRESHOLD*10) max_mic_level_detected=MINTHRESHOLD*10;//we cannot set max_mic_level_detected too low and at the start, when there were no vocalizations it could be quite low
                }
                yield return new WaitForSeconds(0.1f);
            }
            yield break;
        }

        private void OnPatchReceived(float[] features)
        {
            if (worker != null)
            {
                Tensor inputTensor = null;

                var shape = new int[4] { 1, 96, 64, 1 };
                var inputs = new Dictionary<string, Tensor>();

                string inputName = model.inputs[0].name;
                inputTensor = new Tensor(shape, features);
                inputs.Add(inputName, inputTensor);
                worker.Execute(inputs);

                try
                {
                    string outputName = model.outputs[0];
                    Tensor output = worker.PeekOutput(outputName);
                    float[] predictions = output.AsFloats();
                    int bestClassId = -1;
                    float bestScore = -1000;
                    for (int i = 0; i < predictions.Length; i++)
                    {
                        if (bestScore < output[0, 0, 0, i])
                        {
                            bestScore = output[0, 0, 0, i];
                            bestClassId = i;
                        }
                    }
                    string bestClassName = this.classMap[bestClassId];
                    this.onResult.Invoke(bestClassId, bestClassName, bestScore);
                }
                finally
                {
                    inputTensor?.Dispose();
                }
            }
        }

        public void OnDestroy()
        {
            worker?.Dispose();
        }

        public void InitializePrevLevel()
        {//
            prevLevels = new float[NUM_POINTS];
            float startingThreathold = Mathf.Max(2*rmsLevel, MINTHRESHOLD);
            for(int i=0; i<NUM_POINTS; i++){ prevLevels[i]=startingThreathold; }//=MINTHRESHOLD;//if we start with MINTHRESHOLD, the ball gets stuck on top until the threshold picks up 
        }

        public void FillPrevLevel()
        {//
            for(int i=0; i<NUM_POINTS-1; i++) prevLevels[i] = prevLevels[i+1];  
            prevLevels[NUM_POINTS-1] = rmsLevel;
            for(int i=NUM_POINTS-5; i<NUM_POINTS; i++){ m_5buff += prevLevels[i]; } m_5buff/=5;//the shortest words last for at least five buffers (usually longer), so we only need to react to prolonged crossing of a threshold
        }

        public float Threshold(float multiplier)
        {//Re-calculate current threshold and add the new rmsLevel to the array. The threshold cannot be constant as room can suddenly become noisy (TV is turned on). The fact that the ball will be stuck at the ceiling will be interpreted as an error. 
        //we enter this function every 20ms. There is really no need to enter it so often. We can do it every 100ms.
            float fiveBuffers_BeforeLastSixBuffers=0;
            for(int i=NUM_POINTS-11; i<NUM_POINTS-6; i++){ fiveBuffers_BeforeLastSixBuffers += prevLevels[i]; } fiveBuffers_BeforeLastSixBuffers/=5;
            updateCounter++;

            if (m_5buff < fiveBuffers_BeforeLastSixBuffers/4)//sudden drop in level -> there must have been a vocalization
            {//when vocalization stops (sharp drop in amplitude), we need to encourage a second vocalization by dropping the threshold.
                bFirstDropWasDetected = true;//before the first drop was detected we shall not set threshold to max_mic_level_detected/4 since max_mic_level_detected is not a realistice max_mic_level_detected.
                //Debug.Log("Sudden drop in rmsLevel"+"; rmsLevelUnfilt="+(rmsLevelUnfilt*1000).ToString("0.0") + "; rmsLevel="+(rmsLevel*1000).ToString("0.0") +"; m_threshold="+(m_threshold*1000).ToString("0.0"));
                float reduction_mult=0.5f;
                m_threshold *= reduction_mult; if(m_threshold<MINTHRESHOLD) m_threshold = MINTHRESHOLD;
                for(int i=0; i<NUM_POINTS; i++){ prevLevels[i]=m_threshold; }//lower down all historic records
                //Debug.Log("Reduced prevLevels[i] by "+reduction_mult*100+"% to m_threshold="+(m_threshold*1000).ToString("0.0"));
            }
            else
            {//to calculate the threshold we need to average rmsLevel over several seconds. 
                float avg=0;
                for(int i=0; i<NUM_POINTS; i++){ avg += prevLevels[i]; } avg/=NUM_POINTS;//calculate the average
                if(!bFirstDropWasDetected) m_threshold=avg*multiplier*2;//until the fist vocalization was detected keep threshold twice as big as normal.
                else m_threshold=avg*multiplier;// ==30% greater than average level for the ball

                if (m_threshold < MINTHRESHOLD){ m_threshold = MINTHRESHOLD; }// Debug.Log(updateCounter+". m_5b="+(m_5buff*1000).ToString("0.0")+"; rmsUnfilt="+(rmsLevelUnfilt*1000).ToString("0.0") +"; rms="+(rmsLevel*1000).ToString("0.0")+"; avg="+(avg*1000).ToString("0.0")+"; m_threshold=MINTHRESHOLD="+(m_threshold*1000).ToString("0.0"));}
                else if((m_threshold > max_mic_level_detected/4) && bFirstDropWasDetected){ m_threshold = max_mic_level_detected/4; }//Debug.Log(updateCounter+". m_5b="+(m_5buff*1000).ToString("0.0")+"; rmsUnfilt="+(rmsLevelUnfilt*1000).ToString("0.0")+"; rms="+(rmsLevel*1000).ToString("0.0") +"; avg="+(avg*1000).ToString("0.0")+"; m_threshold=max_mic_level_detected/4="+(m_threshold*1000).ToString("0.0"));}//the problem with child sayinh "UUUUUUUUUU". The threshold gets too high and it looks like an error with unresponsive game.
                //else Debug.Log(updateCounter+". m_5b="+(m_5buff*1000).ToString("0.0")+"; rmsUnfilt="+(rmsLevelUnfilt*1000).ToString("0.0") +"; rms="+(rmsLevel*1000).ToString("0.0") +"; avgAllHistoryBuffer="+(avg*1000).ToString("0.0")+"; m_threshold=avg over 5sec x MULTIPLIER="+(m_threshold*1000).ToString("0.0"));

                for(int i=0; i<NUM_POINTS-1; i++) prevLevels[i] = prevLevels[i+1];  
            }
            prevLevels[NUM_POINTS-1] = rmsLevel;
            for(int i=NUM_POINTS-5; i<NUM_POINTS; i++){ m_5buff += prevLevels[i]; } m_5buff/=5;//the shortest words last for at least five buffers (usually longer), so we only need to react to prolonged crossing of a threshold

            return m_threshold;
        }
        public float[] getSamples() {
            return samples.ToArray();
        }
    }
}
