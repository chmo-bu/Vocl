using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public GameObject loadingObject;
    public GameObject prompt;
    public PeakDetector detector;

    
    public void LoadScene(int sceneID)
    {
       prompt.SetActive(false);
       StartCoroutine(LoadSceneAsync(sceneID));
       if (detector.isListening)
       {
        detector.Stop();
       }
    }

    IEnumerator LoadSceneAsync(int sceneID)
    {
       // loadingObject.SetActive(true);
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneID);
        loadingObject.SetActive(true);
        while (!operation.isDone)
        {
            yield return null;
        }

       // loadingObject.SetActive(false);
    }

    public void PlayAgain(int sceneID)
    {
        SceneManager.LoadScene(sceneID);
    }
    
}
