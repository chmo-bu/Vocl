using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ClapAudio.Play;
// using ClapDetector;
public class Game3 : MonoBehaviour
{
    // private ClapDetector.ClapDetector clapDetector;
    public ClapDetector clapDetector;
    public GameObject rabbit;
    public GameObject destination2;

    public Vector3 rabbitLocation;
    public Vector3 target2;

    public string microphone;
    AudioSource audioSource; 
    public bool complete;

    public GameObject correct_prompt;
    public GameObject clap_prompt;
    public RuntimeAnimatorController celebrate;
    public RuntimeAnimatorController idle;
    
    private float timer;

    bool isListening;
    // Start is called before the first frame update
    void Start()
    {
        clap_prompt.SetActive(true);
        rabbitLocation = rabbit.transform.position;
        target2 = destination2.transform.position;
        audioSource = GetComponent<AudioSource>();
        complete = false;
        timer = 9f;
        isListening = false;
        correct_prompt.SetActive(false);
        // clapDetector = new ClapDetector.ClapDetector();
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
        Animator currentAnimator = rabbit.GetComponent<Animator>();
        float distFromTarget2 = Vector3.Distance(rabbitLocation, target2);

        if (timer != 0f && complete == true)
        {
            timer = timer - .5f;
        }

        if (distFromTarget2 <= 3 && !complete)
        {
            // rabbit is at campfire location, begin game
            if (!isListening) {
                clapDetector.Listen();
                isListening = true;
            }
            
            // check if task is complete
            if (clapDetector.done == true)
            {
                completeTask();
                currentAnimator.runtimeAnimatorController = celebrate;
                clapDetector.Stop();
            }
        }
        else if (complete && timer == 0)
        {
            currentAnimator.runtimeAnimatorController = idle;
            timer = 9f; // reset timer
        }
    }

    public void completeTask()
    {
        complete = true;
        clap_prompt.SetActive(false);
        correct_prompt.SetActive(true);
    }
}
