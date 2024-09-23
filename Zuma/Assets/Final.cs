using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Final : MonoBehaviour
{
    public int wait;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(FinalVideo());
    }

    IEnumerator FinalVideo()
    {
        yield return new WaitForSeconds(wait);
        Application.Quit();
    }
}