using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AudioTest : MonoBehaviour
{
    public StreamingMic streamingMic;
    public TMP_Text status;

    private float waitTime = 1.5f;
    private float timer = 0.0f;
    private bool bInitializePrevLevel = false;

    // Start is called before the first frame update
    void Start()
    {
        status.text = "hello";
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

        if (streamingMic.m_5buff > streamingMic.Threshold(1.5f)) //&&     //calculate new value for m_threshold
            //System.DateTime.Now > streamingMic.playCollisionSoundStops)//do not trigger threshold crossing for the colision sound
        {//above the treshold => force up. 
            Debug.Log("Crossed threshold");

            // if(System.DateTime.Now > streamingMic.playEncourageSoundStops) streamingMic.lastDetection = System.DateTime.Now;//this threshold crossing is the result of a playback sound. Do not use it for silence duration calculations.
            // else Debug.Log("Threshold crossing due to sound playback");

            // if (!audioSource.isPlaying)
            //     forceUp = this.upForce;
            // else
            //     forceUp = this.upForce / 2;

            // constForce.force = new Vector3(0, forceUp, 0);
        }
    }
}
