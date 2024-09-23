using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatorBalls_trigger : MonoBehaviour
{
    bool exist_touch = false;
    float time_stop = 1;  //ms   40 standart
    bool start = true;
    int time_prev;
    void Start()
    {
        
    }

    
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        int time = System.DateTime.Now.Millisecond;
        if (!exist_touch && (start || (time - time_prev >= time_stop || (time_prev >= 900 && time + 1000 - time_prev >= time_stop))))
        {
            exist_touch = true;
            time_prev = System.DateTime.Now.Millisecond;
            //balls_main_script.ready_to_new_ball = true;
            start = false;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        exist_touch = false;
    }
}
