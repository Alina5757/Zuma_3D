using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckBalls : MonoBehaviour
{
    int count_inside = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        balls_main_script.ready_to_new_ball = false;
        count_inside++;
    }
    private void OnTriggerExit(Collider other)
    {
        count_inside--;
        if (count_inside == 0)
        {
            balls_main_script.ready_to_new_ball = true;
        }
    }
}
