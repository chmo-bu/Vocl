using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine;
using UnityEngine.Networking;

using YamNetUnity;
using ConventionalAudio;

[DisallowMultipleComponent]
[RequireComponent(typeof(YamNet))]
public class PeakDetector : MonoBehaviour
{
    private YamNet net;
    private Threshold thresh;
    private PeakCounter peakCounter;

    private AudioClip clip = null;
    private int sampleRate;
    private string microphoneDeviceName = null;
    private Queue<float> samples;
    private float waitTime = 1.5f;
    private float timer = 0.0f;
    private bool bInitializePrevLevel = false;
    private int audioOffset;
    private int cid = 0;
    public bool done = false;
    public int numToDetect;
    public int classID; // 1 - clap, 2 - yell, 3 - stomp

    void Start()
    {
        thresh = new Threshold();
    }

    private void StartMicrophone(int num) {
        int minFreq;
        int maxFreq;

       // thresh = new Threshold();
        peakCounter = new PeakCounter(num);
        samples = new Queue<float>();

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
            this.sampleRate = Math.Clamp(this.sampleRate, minFreq, maxFreq);
        }

        this.clip = Microphone.Start(microphoneDeviceName, true, net.getSeconds(), this.sampleRate);
        this.audioOffset = 0;
        Debug.Log("StartMicrophone() ");
    }

    private void StopMicrophone() {
        if (this.clip != null) {
            Microphone.End(this.microphoneDeviceName);
            this.clip = null;
            this.microphoneDeviceName = null;
            Debug.Log("StopMicrophone() ");
        }
    }

    private IEnumerator CountClaps() {
        while (this.cid == 0 || this.clip == null) {
            yield return null;
        }

        while (this.cid != 0 && this.clip != null) {
            float[] _samples = null;

            if(!bInitializePrevLevel){
                thresh.InitializePrevLevel(); 
                bInitializePrevLevel=true;
                yield return new WaitForSeconds(0.02f);
            
            }

            timer += Time.deltaTime; 
            if(timer < waitTime) {
                thresh.FillPrevLevel(); 
                yield return new WaitForSeconds(0.02f); 
            }

            if (thresh.m_5buff > thresh.threshold(3.0f)) {
                yield return new WaitForSeconds(1.5f);
                
                _samples = samples.ToArray();
                
                bool result = peakCounter.countPeaks(_samples);
                net.SendInput(_samples, this.sampleRate, result);
            }

            yield return new WaitForSeconds(0.02f);
        }
        yield break;
    }

    void Update() {
        if (this.microphoneDeviceName != null && this.clip != null)
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
                thresh.update(data);
            }
        }
    }

    public void Listen(int num, int id) {
        numToDetect = num;
        classID = id;
        net = GetComponent<YamNet>();
        net.onResult.AddListener(YamNetResultCallback);
        StartMicrophone(numToDetect);
        if (this.cid == 0) {
            this.cid = Runnable.Run(CountClaps());
        }
    }


    public void Stop() {
        StopMicrophone();
        Runnable.Stop(this.cid);
        if (this.cid != 0) {
            this.cid = 0;
        }
        samples.Clear();
        net.onResult.RemoveListener(YamNetResultCallback);
    }

    private void YamNetResultCallback(int bestClassId, string bestClassName, float bestScore, bool result)
    {
        float time = Time.time;
        string status = $"time: {time}, bestClassId: {bestClassId}, score: {bestScore}, bestClassName: {bestClassName}, threshold: {result}";
        Debug.Log(status);


        // clap 
        if (classID == 1)
        {
            done = (result == true)  && ((bestClassId >= 420 && bestClassId <= 432) || bestClassId == 57 || bestClassId == 58);

        }
        else if (classID == 2)
        {
            // yell
            done = (result == true)  && (bestClassId >= 6 && bestClassId <=11);

        }
        else if (classID == 3)
        {
            // stomp event
            done = (result == true) && (bestClassId > 45 && bestClassId < 49);
        }
    }
}