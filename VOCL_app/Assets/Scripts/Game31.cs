using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game31 : MonoBehaviour
{
    public GameObject rabbit;
    public GameObject destination1;
    public GameObject correct_prompt;
    public GameObject pink_candy;
    public GameObject green_candy;
    public GameObject blue_candy;
    public RuntimeAnimatorController celebrate;
    public RuntimeAnimatorController idle;
    public bool complete;
    public Vector3 rabbitLocation;
    public bool moving;
    public GameObject tap_prompt;
    public float timer;

     void Start()
    {
        rabbitLocation = rabbit.transform.position;
        complete = false;
        moving = false;
        correct_prompt.SetActive(false);
        tap_prompt.SetActive(false);
        timer = 500.0f;
    }

    void Update()
    {
        rabbitLocation = rabbit.transform.position;
        Animator currentAnimator = rabbit.GetComponent<Animator>();

        /* Moves rabbit to next destination if running, if not then idle */
        if (timer != 0f )
        {
            timer = timer - .5f;
        }

        if(timer == 0f)
        {
            completeTask();
        }


        if (moving == true)
        {
            Vector3 rabbitLocation = rabbit.transform.position;
            Vector3 target = destination1.transform.position;
            var offset = target - rabbitLocation;
            //Debug.Log(offset.magnitude);
            if (offset.magnitude > 100f && moving == true) {
                // If we're further away than 3 units, move towards the target.
                // The minimum allowable tolerance varies with the speed of the object and the framerate.
                // 2 * tolerance must be >= moveSpeed / framerate or the object will jump right over the stop.
                offset = offset.normalized * 400.0f;
                //normalize it and account for movement speed.
                rabbit.transform.position = rabbitLocation + (offset * Time.deltaTime);
                
            } 
        }

        if (!complete)
        {
            // at initial, start 
            //Debug.Log("here");
            tap_prompt.SetActive(true);
            if (!complete && !moving)
            {
                //Debug.Log("gamestart");
                // begin game
                if (Input.touchCount == 1)
                {
                    var ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
                    //Debug.Log(ray);
                    RaycastHit hitInfo;
                    if (Physics.Raycast(ray, out hitInfo))
                    {
                    Debug.Log(ray);

                        if (hitInfo.transform.gameObject.CompareTag("Correct"))
                        {
                            Debug.Log("pink");
                            completeTask();
                        }
                    }
                                                
                }
            }
        }
        
    }

    public void completeTask()
    {
        complete = true;
        moving = true;
        tap_prompt.SetActive(false);
        correct_prompt.SetActive(true);
    }
}
