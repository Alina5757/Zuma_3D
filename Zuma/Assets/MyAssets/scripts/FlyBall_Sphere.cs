using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyBall_Sphere : MonoBehaviour
{
    GameObject first_ball = null;
    GameObject second_ball = null;
    bool full = false;

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
        if (!full && (other.gameObject).tag == "Ball")
        {
            if(first_ball == null)
            {
                first_ball = other.gameObject;
            }
            else
            {
                full = true;
                second_ball = other.gameObject;
            }            
        }
    }
}
