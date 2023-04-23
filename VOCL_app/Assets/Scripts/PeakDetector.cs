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

    void Start()
    {
        
    }

    private void StartMicrophone() {
        int minFreq;
        int maxFreq;

        thresh = new Threshold();
        peakCounter = new PeakCounter();
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
                bool result = peakCounter.countPeaks(samples.ToArray());
                Debug.Log("peakCounter: " + result);
                done = true;
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
                net.SendInput(data, this.sampleRate);
            }
        }
    }

    public void Listen() {
        net = GetComponent<YamNet>();
        net.onResult.AddListener(YamNetResultCallback);
        StartMicrophone();
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
        net.onResult.RemoveListener(YamNetResultCallback);
    }

    private void YamNetResultCallback(int bestClassId, string bestClassName, float bestScore)
    {
        float time = Time.time;
        string status = $"time: {time}, bestClassId: {bestClassId}, score: {bestScore}, bestClassName: {bestClassName}";
        Debug.Log(status);
    }
}
// }

