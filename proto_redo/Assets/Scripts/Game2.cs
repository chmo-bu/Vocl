using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game2 : MonoBehaviour
{
    // game objects
    public GameObject rabbit;
    public GameObject fruit1, fruit2, fruit3;

    // animators 
    public RuntimeAnimatorController CelebrateRun;
    public RuntimeAnimatorController idle;

    // prompts 
    public GameObject prompt;
    public GameObject correct_prompt;
    public GameObject incorrect_prompt;
    public GameObject destination2;
    public GameObject submitButton;

    private bool moving;
    private float timer;
    private bool complete;

    // Start is called before the first frame update
    void Start()
    {
        correct_prompt.SetActive(false);
        incorrect_prompt.SetActive(false);
        prompt.SetActive(true);
        submitButton.SetActive(true);
        moving = false;
        timer = 17f;
        complete = false;
    }

    public void move ()
    {
        moving = true;
    }

    // Update is called once per frame
    void Update()
    {
        Animator currentAnimator = rabbit.GetComponent<Animator>();
        if (timer != 0f && complete == true)
        {
            timer = timer - .5f;
        }

        /* Moves rabbit to next destination if running, if not then idle */

        if (moving == true && timer == 0)
        {
            Vector3 rabbitLocation = rabbit.transform.position;
            Vector3 target = destination2.transform.position;
            var offset = target - rabbitLocation;
        
            if (offset.magnitude > 3f && moving == true) {
                // If we're further away than 3 units, move towards the target.
                // The minimum allowable tolerance varies with the speed of the object and the framerate.
                // 2 * tolerance must be >= moveSpeed / framerate or the object will jump right over the stop.
                offset = offset.normalized * 5.0f;
                //normalize it and account for movement speed.
                rabbit.transform.position = rabbitLocation + (offset * Time.deltaTime);
                
            } 
            else 
            {
                moving = false;
                timer = 17f; // reset timer
                rabbit.transform.Rotate(0,90,0);
                currentAnimator.runtimeAnimatorController = idle;
            }
        }
    }
}
