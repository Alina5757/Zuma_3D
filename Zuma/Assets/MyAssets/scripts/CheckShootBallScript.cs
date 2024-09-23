using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckShootBallScript : MonoBehaviour
{
    static bool start_timer = false;
    static float time_timer = 0.7f;
    static float time;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (start_timer)
        {
            time -= Time.deltaTime;
            if (time <= 0)
            {
                start_timer = false;
                person_script.ready_shot = true;
            }
        }        
    }

    public static void StartTimer()
    {
        start_timer = true;
        time = time_timer;
    }
}
