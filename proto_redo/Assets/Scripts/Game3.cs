using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ClapAudio.Play;
using ClapDetector;
public class Game3 : MonoBehaviour
{
    private ClapDetector.ClapDetector clapDetector;
    public GameObject rabbit;
    public GameObject destination2;

    public Vector3 rabbitLocation;
    public Vector3 target2;

    public string microphone;
    AudioSource audioSource; 
    bool done;

    bool isListening;
    // Start is called before the first frame update
    void Start()
    {
        rabbitLocation = rabbit.transform.position;
        target2 = destination2.transform.position;
        audioSource = GetComponent<AudioSource>();
        done = false;
        isListening = false;
        clapDetector = new ClapDetector.ClapDetector();
        // get all available microphones
		foreach (string device in Microphone.devices) {
			if (microphone == null) {
				//set default mic to first mic found.
				microphone = device;
                Debug.Log(microphone);
			}
		}
    }

    // Update is called once per frame
    void Update()
    {
        rabbitLocation = rabbit.transform.position;
        float distFromTarget2 = Vector3.Distance(rabbitLocation, target2);

        if (distFromTarget2 <= 3 && !done)
        {
            // rabbit is at campfire location, begin game
            // playAudio();
            if (!isListening) {
                clapDetector.Listen();
                isListening = true;
            }
            // clapDetector.Listen();
            // Debug.Log("done playing");
        }
    }

    void playAudio()
    {
        //ClapAudio.Play.playAudio();
            
            audioSource.Stop();
            audioSource.clip = Microphone.Start(microphone, false, 10, 44100);
            Debug.Log(Microphone.IsRecording(microphone).ToString());
            if (Microphone.IsRecording (microphone)) { //check that the mic is recording, otherwise you'll get stuck in an infinite loop waiting for it to start
			while (!(Microphone.GetPosition (microphone) > 0)) {
			} // Wait until the recording has started. 
		
			Debug.Log ("recording started with " + microphone);

			// Start playing the audio source
			audioSource.Play (); 
		} else {
			//microphone doesn't work for some reason

			Debug.Log (microphone + " doesn't work!");
		}
       
        done = true;
    }
}
