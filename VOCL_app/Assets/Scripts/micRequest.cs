using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.iOS;

namespace micRequest 
{
    public static class requestAccess
    {
        public static IEnumerator req()
        {
            findMics();
            yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
            if (Application.HasUserAuthorization(UserAuthorization.Microphone))
            {
                Debug.Log("Microphone found");
            }
            else
            {
                Debug.Log("Microphone not found");
            }
        }

        public static void findMics()
        {
            foreach (var device in Microphone.devices)
            {
                Debug.Log("Name: " + device);
            }
        }
    }
    
}
