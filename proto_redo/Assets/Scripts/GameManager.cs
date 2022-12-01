using System.Security.AccessControl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameManager : MonoBehaviour 
{

    // item1 is the correct choice, other two
    // are incorrect

    //public Material item1, item2, item3;

    private GameObject objectHit;
    public GameObject rabbit;
    public RuntimeAnimatorController celebrate;

    public GameObject prompt;
    public GameObject correct_prompt;
    public GameObject incorrect_prompt;
    
    void Start() {
        correct_prompt.SetActive(false);
        incorrect_prompt.SetActive(false);
        prompt.SetActive(true);
        
    }

    void Update() {

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
                        Animator currentAnimator = rabbit.GetComponent<Animator>();
                        currentAnimator.runtimeAnimatorController = celebrate;

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
}