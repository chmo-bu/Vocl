using UnityEngine;
using System.Collections;
using System;

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
    private int updateCounter=0, recBuffCounter=0;

    private const float MINTHRESHOLD = 0.002f;//0.04f;
    private const int NUM_SECONDS = 5;//to calculate the threshold we need to average m_level over several seconds (the duration of vocalization).
    private const int NUM_POINTS = NUM_SECONDS * 50;//FixedUpdate occurres 50 times per second.
    public float m_threshold;
    private float[] prevLevels;
    private float max_mic_level_detected=MINTHRESHOLD;//never set the threshold above than 1/4 of max_mic_level_detected
    private bool bFirstDropWasDetected=false;
    //public TimeSpan durAfterThresholdCrossing;
    public DateTime lastDetection;//this is used to calculate duration of silence. Reported by each scene
    public DateTime playCollisionSoundStops;//time when a collision sound being played stops
    public DateTime playEncourageSoundStops;//time when a encouraging sound being played (to encourage a child) stops

    //Highpass filter Butterworth  https://stackoverflow.com/questions/8079526/lowpass-and-high-pass-filter-in-c-sharp
    private readonly float resonance=Mathf.Sqrt(2);
    private readonly float frequency=500;//highpass cutoff frequency (Hz)
    private readonly int sampleRate=16000;//same as m_nRecordingHZ
    private float c, a1, a2, a3, b1, b2;
    private float[] inputHistory = new float[2];/// Array of input values, latest are in front
    private float[] outputHistory = new float[3];/// Array of output values, latest are in front


    public void FilterHighpassButterworth()
    {//highpass  https://stackoverflow.com/questions/8079526/lowpass-and-high-pass-filter-in-c-sharp
        c = (float)Math.Tan(Math.PI * frequency / sampleRate);
        a1 = 1.0f / (1.0f + resonance * c + c * c);
        a2 = -2f * a1;
        a3 = a1;
        b1 = 2.0f * (c * c - 1.0f) * a1;
        b2 = (1.0f - resonance * c + c * c) * a1;

        /*//I have tested this implementation with a cutoff freq 1000Hz by playing a tone into an externam microphone. (do not use internal Lenovo mic for testing. It is extremely noisy)
         * As you can see there is no decrease of RMS when frequency is greater than 1000Hz
        500Hz. m_5b=17.4; rmsUnfilt=55.7; rms=13.9; avgAllHistoryBuffer=13.9; m_threshold=avg over 5sec x MULTIPLIER=41.7
        700Hz: m_5b=41.4; rmsUnfilt=75.3; rms=33.1; avgAllHistoryBuffer=29.2; m_threshold=avg over 5sec x MULTIPLIER=87.7
        1000Hz. m_5b=68.5; rmsUnfilt=78.2; rms=54.8; avgAllHistoryBuffer=54.6; m_threshold=avg over 5sec x MULTIPLIER=163.8
        1300Hz. m_5b=14.4; rmsUnfilt=15.8; rms=11.6; avgAllHistoryBuffer=11.3; m_threshold=avg over 5sec x MULTIPLIER=33.9
        2000Hz. m_5b=43.3; rmsUnfilt=35.6; rms=34.5; avgAllHistoryBuffer=34.2; m_threshold=avg over 5sec x MULTIPLIER=102.7
        */
    }


    void Start()
    {
        FilterHighpassButterworth();
        StartRecording();   
    }

    private void StartRecording()
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
                m_level = Mathf.Max(samples);//no filtering is OK: 1) high frequency: since we are recording at 16,000Hz and high frequencies are naturally removed. 2) Low freq vibrations are filtered out by the microphone.
                
                //Highpass filter the samples array in order to filter out low frequency noise, adult voice, and motor sound in the driving game//https://stackoverflow.com/questions/8079526/lowpass-and-high-pass-filter-in-c-sharp
                float[] filtered = new float[nsamplesarray];//latest are in front
                outputHistory[0]=inputHistory[0]=samples[0]; outputHistory[1]=inputHistory[1]=samples[1];
                for (int i=2; i<nsamplesarray; i++){ filtered[i]=FilterButterworth(samples[i]);}

                //calculate root mean square - it is much more reliable than the maximum
                float sum=0;
                //for (int i=0; i<nsamplesarray; i++){sum += samples[i]*samples[i];} rmsLevelUnfilt = Mathf.Sqrt(sum/nsamplesarray); sum=0;//unfiltered rms = square root of average
                for (int i=2; i<nsamplesarray; i++){sum += filtered[i]*filtered[i];} rmsLevel = Mathf.Sqrt(sum/(nsamplesarray-2)); //filtered rms = square root of average
                rmsLevel = Mathf.Sqrt(sum/nsamplesarray); // rms = square root of average
                if(rmsLevel>max_mic_level_detected) max_mic_level_detected=rmsLevel;//keep an eye on the maximum rmsLevel ever reach, out threshold must be way lower 
                if(max_mic_level_detected<MINTHRESHOLD*10) max_mic_level_detected=MINTHRESHOLD*10;//we cannot set max_mic_level_detected too low and at the start, when there were no vocalizations it could be quite low

                //recBuffCounter++; Debug.Log("recBuffCounter="+recBuffCounter);
                //m_webSocketClient.OnListen(samples, 0, samples.Length, m_acRecording.channels);
            }
            else
            {
                samples = new float[m_acRecording.samples];
                m_acRecording.GetData(samples, 0);
                //m_webSocketClient.OnListen(samples, lastSample, samples.Length, m_acRecording.channels);
                //m_webSocketClient.OnListen(samples, 0, pos, m_acRecording.channels);
            }
            lastSample = pos;
            yield return new WaitForSeconds(0.1f);
        }

        yield break;
    }

    private float FilterButterworth(float newInput)
    {
        float newOutput = a1 * newInput + a2 * inputHistory[0] + a3 * inputHistory[1] - b1 * outputHistory[0] - b2 * outputHistory[1];

        inputHistory[1] = inputHistory[0];
        inputHistory[0] = newInput;

        outputHistory[2] = outputHistory[1];
        outputHistory[1] = outputHistory[0];
        outputHistory[0] = newOutput;

        return newOutput;
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
        
}

