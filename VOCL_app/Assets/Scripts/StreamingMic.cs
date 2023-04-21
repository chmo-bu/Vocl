using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using RollArray;
using Filter;

// namespace StreamingMic {
public class StreamingMic : MonoBehaviour
{//AV 0401 2022: filtering with AudioSource //https://youtu.be/GHc9RF258VA?t=223
    private int m_nRecordingRoutine = 0;
    private string m_sMicrophoneID = null;
    private AudioClip m_acRecording = null;
    private int m_nRecordingBufferSize = 1;
    public int m_nRecordingHZ = 16000;
    public float m_level;//=maximum in this buffer=old implementation; I later switched to avg of five consequitive buffers based on RMS
    public float rmsLevel, rmsLevelUnfilt;
    public float m_5buff;//=avg of five consequitive buffers
    public float m_threshold;
    private float[] prevLevels;
    public float[] _samples = new float[48000];
    public int idx = 0;

    void Start() {
    }

    public void StartRecording()
    {
        if (m_nRecordingRoutine == 0)
        {
            //UnityObjectUtil.StartDestroyQueue();
            m_nRecordingRoutine = Runnable.Run(RecordingHandler());
        }
    }

    public void StopRecording()
    {
        if (m_nRecordingRoutine != 0)
        {
            Microphone.End(m_sMicrophoneID);
            Runnable.Stop(m_nRecordingRoutine);
            m_nRecordingRoutine = 0;
            Debug.Log("StopRecording() ");
        }
    }

    private void OnError(string error)
    {
        Debug.Log("StreamingMic Error! " + error);
    }

    public void ToggleMicrophone()
    {
    }

    private IEnumerator RecordingHandler()
    {
        Debug.Log("****StreamingMic devices: " + Microphone.devices);
        m_acRecording = Microphone.Start(m_sMicrophoneID, true, m_nRecordingBufferSize, m_nRecordingHZ);
        while (!(Microphone.GetPosition(null) > 0))
        {
        }
        yield return null;

        if (m_acRecording == null)
        {
            StopRecording();
            yield break;
        }

        float[] samples = null;
        int lastSample = 0;

        while (m_nRecordingRoutine != 0 && m_acRecording != null)
        {//we enter this function asyncronously approximately every 100ms (but buffers are uneven). It does not make any sense to re-calculate the threshold every millisend
            int pos = Microphone.GetPosition(m_sMicrophoneID);
            if (pos > m_acRecording.samples || !Microphone.IsRecording(m_sMicrophoneID))
            {
                //Debug.Log("MicrophoneWidget Microphone disconnected.");
                StopRecording();
                yield break;
            }

            int diff = pos - lastSample;
            //Debug.Log("pos=" + pos + ", lastSample=" + lastSample + ", diff=" + diff);

            if (diff > 0)
            {
                int nsamplesarray = diff * m_acRecording.channels;
                samples = new float[nsamplesarray];
                m_acRecording.GetData(samples, 0);//m_acRecording.GetData(samples, lastSample);

                for (int i=0; i<nsamplesarray; i++) { 
                    if (idx < 48000) {
                        _samples[idx] = (samples[i]);
                        idx++;
                    }
                }
            }
            lastSample = pos;
            yield return new WaitForSeconds(0.1f);
        }

        yield break;
    }

    public int argmax(float[] arr) {
        float ma = arr[0];
        int amax = 0;
        for (int i=1; i<arr.Length; i++) {
            float temp = arr[i];
            if (temp > ma) {
                amax = i;
                ma = temp;
            }
        }
        return amax;
    }

    public int argmax(float[] arr, int start, int end) {
        float ma = arr[start];
        int amax = start;
        for (int i=start+1; i<end; i++) {
            float temp = arr[i];
            if (temp > ma) {
                amax = i;
                ma = temp;
            }
        }
        return amax;
    }
        
}
// }