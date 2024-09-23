using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayStartVideo : MonoBehaviour
{
    // Start is called before the first frame update
    public int waitTime;
    void Start()
    {
        StartCoroutine(WaitVideo());
    }

    IEnumerator WaitVideo()
    {
        yield return new WaitForSeconds(waitTime);
        SceneManager.LoadScene(1);
    }
   
}
