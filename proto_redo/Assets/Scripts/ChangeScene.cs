using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public GameObject loadingObject;
    public GameObject prompt;
   // public GameObject menu;

    
    public void LoadScene(int sceneID)
    {
        //SceneManager.LoadScene(sceneID);
        //StartCoroutine(LoadSceneAsync(sceneID));
       // menu.SetActive(false);
       // StartCoroutine(LoadScene(sceneID));
       prompt.SetActive(false);
       StartCoroutine(LoadSceneAsync(sceneID));
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
    
}
