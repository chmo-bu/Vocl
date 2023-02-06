using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//TODO: try https://stackoverflow.com/questions/22583391/peak-signal-detection-in-realtime-timeseries-data/53614452#53614452

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
    private int clapCount = 0;
    private double clapDelay = 1000.0;

    // Start is called before the first frame update
    void Start()
    {
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

        // tempState = streamingMic.m_5buff > streamingMic.Threshold(1.5f);
        tempState = streamingMic.m_5buff > streamingMic.Threshold(4.0f);
        // tempState = streamingMic.m_5buff > 0.20;

        if (tempState && ((double)((TimeSpan)(System.DateTime.Now
         - lastTime)).TotalMilliseconds) > clapDelay) {
            lastTime = System.DateTime.Now;
            clapCount++;
            Debug.Log("clap count: " + clapCount);
        }

        // if (tempState != lastState) {
        //     lastTime = System.DateTime.Now;
        // }
        
        // double diff = ((TimeSpan)(System.DateTime.Now - lastTime)).TotalMilliseconds;

        // if (diff > clapDelay) {
        //     // Debug.Log(diff);
        //     if (tempState != currentState) {
        //         currentState = tempState;
        //     }

        //     if (currentState) {
        //         // Debug.Log("clap!");
        //         clapCount++;
        //         Debug.Log(clapCount);
        //     }
        // }

        // lastState = tempState;
        // if (streamingMic.m_5buff > streamingMic.Threshold(1.5f)) //&&     //calculate new value for m_threshold
        //     //System.DateTime.Now > streamingMic.playCollisionSoundStops)//do not trigger threshold crossing for the colision sound
        // {//above the treshold => force up. 
        //     Debug.Log("Crossed threshold");
        // }
    }
}
