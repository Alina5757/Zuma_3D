using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyBall_Cube : MonoBehaviour
{
    bool firstTouch = true;
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
        if (firstTouch)
        {
            if ((other.gameObject).tag == "Back")
            {
                firstTouch = false;
                transform.parent.transform.GetComponent<FlyBallScript>().Touch("Back", (other.gameObject).transform.parent.parent.gameObject);
            }
            if ((other.gameObject).tag == "Forward")
            {
                firstTouch = false;
                transform.parent.transform.GetComponent<FlyBallScript>().Touch("Forward", (other.gameObject).transform.parent.parent.gameObject);
            }
        }
    }
}
