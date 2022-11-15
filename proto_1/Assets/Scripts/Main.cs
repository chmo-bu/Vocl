using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
//using System.Windows.Forms.Timer;

public class Main : MonoBehaviour
{
    // game objects for scenes
    public GameObject loadingObject;
    public GameObject menuObject;
    public GameObject inGameObject;

    // child objects
    public GameObject ClickableObject;
    public GameObject scoreObject;

    // tmpro text
    private TMP_Text scoreText;

    // start button flag
    private bool start = false;

    // loading time
    private float timeLeft = 7.0f;

    // Touch objects
    private Touch touch;
    
    // bounds
    private float minX, minY, maxX, maxY;

    // random object spawn position
    private Vector2 pos;
    
    // number of times object clicked
    private volatile uint score = 0;

    // Start is called before the first frame update
    void Start()
    {
        SetMinMax();
        StopGame();
        HideMenu();
        ShowLoadingScreen();

        ClickableObject = inGameObject.transform.Find("ClickableObject").gameObject;
        scoreObject = inGameObject.transform.Find("Score").gameObject;

        scoreText = scoreObject.transform.Find("ScoreText").gameObject.GetComponent<TMP_Text>();
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
            // ClickableObject.SetActive(true);

            // check touch inputs
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) {
                touch = Input.GetTouch(0);

                // change object position on click
                if (checkCircleIntersect(Camera.main.ScreenToWorldPoint(touch.position).x, Camera.main.ScreenToWorldPoint(touch.position).y, 
                    ClickableObject.transform.position.x, ClickableObject.transform.position.y, 
                    touch.radius, 250)) {
                    ChangePosition();
                    score++;
                    scoreText.text = "Score: " + score;
                    Debug.Log("score: " + score);
                }
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

    private void SetMinMax() {
        Vector2 Bounds = Camera.main.ScreenToWorldPoint(
            new Vector2(Screen.width, Screen.height));

        // Debug.Log(Bounds);
        
        minX = 0;
        maxX = Screen.width;
        minY = 0;
        maxY = Screen.height;
    }

    private void ChangePosition() {
        pos = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
        ClickableObject.transform.position = pos;
    }

    // https://www.geeksforgeeks.org/check-two-given-circles-touch-intersect/
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
