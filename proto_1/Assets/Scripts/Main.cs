using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
//using System.Windows.Forms.Timer;

public class Main : MonoBehaviour
{
    // game objects
    public GameObject loadingObject;
    public GameObject menuObject;
    public GameObject inGameObject;
    public GameObject ClickableObject;

    // start button flag
    private bool start = false;

    // object click flag
    private bool objectClicked = false;

    // loading time
    private float timeLeft = 7.0f;

    // Touch objects
    private Touch touch;
    
    // bounds
    private float minX, minY, maxX, maxY;

    // random object spawn position
    private Vector2 pos;

    // Start is called before the first frame update
    void Start()
    {
        ClickableObject.SetActive(false);
        SetMinMax();
        StopGame();
        HideMenu();
        ShowLoadingScreen();
    }

    public void HideMenu()
    {
        menuObject.SetActive(false);
    }

    public void ShowLoadingScreen()
    {
        loadingObject.SetActive(true);
    }

    public void switchToMenu()
    {
        loadingObject.SetActive(false);
        menuObject.SetActive(true);
    }
    
    // Update is called once per frame
    void Update()
    {
        timeLeft -= Time.deltaTime;
        if(timeLeft < 0)
        {
            switchToMenu();
        }

        if (start) {
            // do start button task
            StartButtonTask();


            // make clickable active
            ClickableObject.SetActive(true);

            // // check touch inputs
            // if (Input.touchCount > 0) {
            //     touch = Input.GetTouch(0);

            //     // change object position on click
            //     if (checkCircleIntersect(touch.position.x, touch.position.y, 
            //         ClickableObject.transform.position.x, ClickableObject.transform.position.y, 
            //         touch.radius, 100)) {
            //         Debug.Log("intersect");
            //         // ChangePosition();
            //     }
            // }

            // TODO: change position on clicking on object

            if (objectClicked) {
                Debug.Log("object touched!");
                // objectClicked = false;
            }
        }
    }

    private void StartButtonTask() {
        // reveal game object and hide menu object
        StartGame();
        HideMenu();
    }

    private void StartGame() {
        // activate inGameObject
        inGameObject.SetActive(true);
    }

    private void StopGame() {
        // deactivate inGameObject
        inGameObject.SetActive(false);
    }

    public void onStartClick() {
        // change start flag
        start = true;
    }

    public void onObjectClick() {
        // change object click flag
        objectClicked = true;
        Debug.Log("clicked");
    }

    private void SetMinMax() {
        Vector2 Bounds = Camera.main.ScreenToWorldPoint(
            new Vector2(Screen.width, Screen.height));
        
        minX = -Bounds.x;
        maxX = Bounds.x;
        minY = -Bounds.y;
        maxX = Bounds.y;
    }

    private void ChangePosition() {
        pos = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
        ClickableObject.transform.position = pos;
    }

    public bool checkCircleIntersect(float x1, float y1, float x2, float y2, float r1, float r2) {
        double d = Mathf.Sqrt((x1 - x2) * (x1 - x2)
            + (y1 - y2) * (y1 - y2));
        
        if (d <= r1 - r2 || d <= r2 - r1
            || d < r1 + r2 || d == r1 + r2) {
            return true;
        }

        return false;
    }
}
