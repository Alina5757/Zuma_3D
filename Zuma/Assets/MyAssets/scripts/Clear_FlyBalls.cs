using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clear_FlyBalls : MonoBehaviour
{
    GameObject Main;
    // Start is called before the first frame update
    void Start()
    {
        Main = GameObject.Find("Main");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTriggerEnter(Collider other)
    {
        Main.GetComponent<balls_main_script>().isDeleted = true;
        Main.GetComponent<balls_main_script>().DeleteFlyBall(other);
    }
}