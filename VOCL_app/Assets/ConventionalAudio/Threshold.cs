using System;
using UnityEngine;

namespace ConventionalAudio {
    public class Threshold {
        public float m_level;//=maximum in this buffer=old implementation; I later switched to avg of five consequitive buffers based on RMS
        public float rmsLevel;
        public float m_5buff;//=avg of five consequitive buffers
        private int updateCounter=0;
        private const float MINTHRESHOLD = 0.002f;//0.04f;
        private const int NUM_SECONDS = 5;//to calculate the threshold we need to average m_level over several seconds (the duration of vocalization).
        private const int NUM_POINTS = NUM_SECONDS * 50;//FixedUpdate occurres 50 times per second.
        public float m_threshold;
        private float[] prevLevels;
        private float max_mic_level_detected=MINTHRESHOLD;//never set the threshold above than 1/4 of max_mic_level_detected
        private bool bFirstDropWasDetected=false;
        private FilterButterworth hp; // high pass filter

        public Threshold() {
            hp = new FilterButterworth(DefaultParams.frequency, DefaultParams.sampleRate,
                FilterButterworth.PassType.Highpass, DefaultParams.resonance);
        }

        public void update(float[] data) {
            int nsamplesarray = data.Length;
            float[] filtered = new float[nsamplesarray];

            Array.Copy(data, 0, filtered, 0, nsamplesarray);

           
            // outputHistory[0]=inputHistory[0]=samples[0]; outputHistory[1]=inputHistory[1]=samples[1];
            hp.filterInit(filtered[0], filtered[1]);
            // StringBuilder sb = new StringBuilder();

            for (int i=2; i<nsamplesarray; i++) { 
                filtered[i]=hp.Update(filtered[i]);
            }
            // Debug.Log(_samples._head);

            //calculate root mean square - it is much more reliable than the maximum
            float sum=0;
            //for (int i=0; i<nsamplesarray; i++){sum += samples[i]*samples[i];} rmsLevelUnfilt = Mathf.Sqrt(sum/nsamplesarray); sum=0;//unfiltered rms = square root of average
            for (int i=2; i<nsamplesarray; i++){sum += filtered[i]*filtered[i];} rmsLevel = Mathf.Sqrt(sum/(nsamplesarray-2)); //filtered rms = square root of average
            rmsLevel = Mathf.Sqrt(sum/nsamplesarray); // rms = square root of average
            if(rmsLevel>max_mic_level_detected) max_mic_level_detected=rmsLevel;//keep an eye on the maximum rmsLevel ever reach, out threshold must be way lower 
            if(max_mic_level_detected<MINTHRESHOLD*10) max_mic_level_detected=MINTHRESHOLD*10;//we cannot set max_mic_level_detected too low and at the start, when there were no vocalizations it could be quite low
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

        public float threshold(float multiplier)
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
}