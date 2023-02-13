using System.Security.AccessControl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Game1 : MonoBehaviour 
{

    // item1 is the correct choice, other two
    // are incorrect

    //public Material item1, item2, item3;
    
    private GameObject objectHit;
    public GameObject rabbit;
    public RuntimeAnimatorController CelebrateRun;
    //public RuntimeAnimatorController run;
    public RuntimeAnimatorController idle;

    public GameObject prompt;
    public GameObject correct_prompt;
    public GameObject incorrect_prompt;
    public GameObject destination1;
    //public GameObject submitButton;
    private bool moving;
    private float timer;
    private bool complete;
    
    void Start() {
        correct_prompt.SetActive(false);
        incorrect_prompt.SetActive(false);
        prompt.SetActive(true);
        //submitButton.SetActive(false);
        moving = false;
        timer = 17f;
        complete = false;
    }

    public void move ()
    {
        moving = true;
    }

    void Update() {

        Animator currentAnimator = rabbit.GetComponent<Animator>();
        if (timer != 0f && complete == true)
        {
            timer = timer - .5f;
        }
        

        if (moving == true && timer == 0)
        {
            //currentAnimator.runtimeAnimatorController = run;
            //rabbit.transform.eulerAngles.y = 90;
            //rabbit.transform.Rotate(0,90,0);
            Vector3 rabbitLocation = rabbit.transform.position;
            Vector3 target = destination1.transform.position;
            var offset = target - rabbitLocation;
            //Get the difference.
            if (offset.magnitude > 3f && moving == true) {
                //If we're further away than 3 units, move towards the target.
                //The minimum allowable tolerance varies with the speed of the object and the framerate.
                // 2 * tolerance must be >= moveSpeed / framerate or the object will jump right over the stop.
                offset = offset.normalized * 5.0f;
                //normalize it and account for movement speed.
                rabbit.transform.position = rabbitLocation + (offset * Time.deltaTime);
                //playerController.Move (offset * Time.deltaTime);
                //actually move the character.
                //return false;
            } 
            else 
            {
                moving = false;
                timer = 17f; // reset timer
               // rabbit.transform.eulerAngles.y = 180;
               rabbit.transform.Rotate(0,90,0);
               currentAnimator.runtimeAnimatorController = idle;
                //return true;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            //Touch touch = Input.GetTouch(0);
           // Vector3 touchPosition = Camera.main.ScreenToWorldPoint(touch.position);
             
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo))
            {
                if (hitInfo.transform.gameObject.CompareTag("Correct"))
                {
                    var rig = hitInfo.collider.GetComponent<Rigidbody>();
                    if (rig != null)
                    {
                       // rig.GetComponent<SpriteRenderer>().material = item1;
                       prompt.SetActive(false);
                       incorrect_prompt.SetActive(false);
                       correct_prompt.SetActive(true);
                        rig.AddForceAtPosition(hitInfo.transform.up * 10f, hitInfo.point, ForceMode.VelocityChange);
                       // rig.AddForceAtPosition(ray.direction * 50f, hitInfo.point, ForceMode.VelocityChange);
                       // Animator currentAnimator = rabbit.GetComponent<Animator>();
                        currentAnimator.runtimeAnimatorController = CelebrateRun;
                        //StartCoroutine(wait(currentAnimator));
                        //wait();
                        complete = true;
                         rabbit.transform.Rotate(0,260,0);
                      moving = true;
                        /*
                        bool arrived = false;
                        while (arrived == false)
                        {
                            arrived = MoveToTarget();
                        }*/
                        
                    }
                }
                else if (hitInfo.transform.gameObject.CompareTag("Incorrect"))
                {
                    prompt.SetActive(false);
                    incorrect_prompt.SetActive(true);
                }
            
            }
        }


    } 

    IEnumerator wait(Animator currentAnimator)
    {
        //yield on a new YieldInstruction that waits for 5 seconds.
        currentAnimator.runtimeAnimatorController = CelebrateRun;
        yield return new WaitForSeconds(currentAnimator.GetCurrentAnimatorStateInfo(0).length+currentAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime);
        //yield return new WaitForSecondsRealtime(15);
    }

    
}
