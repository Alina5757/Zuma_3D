using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyBallScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Touch(string Napr, GameObject ballTouch)
    {
        balls_main_script script = GameObject.Find("Main").transform.GetComponent<balls_main_script>();
        Destroy(transform.GetChild(1).gameObject);
        Destroy(transform.GetComponent<FlyBallScript>());

        GameObject Cubes = Instantiate(script.Cubes, new Vector3(0, 0, 0), new Quaternion(), transform);
        Cubes.transform.position = transform.position;
        //Cubes.transform.position = new Vector3(0, 0, 0);

        GameObject.Find("Main").transform.GetComponent<balls_main_script>().AddFlyBall(transform.gameObject, ballTouch, Napr);
    }
}
