using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class person_script : MonoBehaviour
{
    public float SpeedLeftRight = 9000000f;
    public float SpeedTopBot = 100000f;

    private GameObject myObject;
    public GameObject Main;
    public GameObject LPlace;
    public GameObject RPlace;

    public static bool ready_shot = true;
    public GameObject column;
    int ready_zikl = 0; //от 0 до 10 анимация появления
    int max_zikl = 100;
    int select;
    int prev_select;
    int max_height = 400;
    int min_height = 130;


    GameObject new_ball;

    void Start()
    {
        select = GameObject.Find("Main").transform.GetComponent<balls_main_script>().SelectColor("");
        prev_select = GameObject.Find("Main").transform.GetComponent<balls_main_script>().SelectColor("");

        string findName = "Visual/TypeBall-" + (prev_select + 1).ToString();
        GameObject ballType = (GameObject)Resources.Load(findName);
        GameObject ball = Instantiate(ballType, LPlace.transform);
        ball.transform.localPosition = new Vector3(0, 0, 0);
        ball = Instantiate(ballType, RPlace.transform);
        ball.transform.localPosition = new Vector3(0, 0, 0);

        //myObject = GameObject.Find(this.name);
        Cursor.lockState = CursorLockMode.Locked;
    }

    // обратите внимание что все действия с физикой 
    // необходимо обрабатывать в FixedUpdate, а не в Update
    void FixedUpdate()
    {
        MovementLogic();
    }

    private void MovementLogic()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");

        float moveVertical = Input.GetAxis("Vertical");

        transform.position = new Vector3(transform.position.x, transform.position.y + moveVertical, transform.position.z);
        if(transform.position.y < min_height)
        {
            transform.position = new Vector3(transform.position.x, min_height, transform.position.z);
        }
        if (transform.position.y > max_height)
        {
            transform.position = new Vector3(transform.position.x, max_height, transform.position.z);
        }
        column.transform.position = new Vector3(column.transform.position.x, (float)(transform.position.y - 146.25), column.transform.position.z);
    }

    void Update()
    {
        float X = Input.GetAxis("Mouse X") * (SpeedLeftRight*SpeedLeftRight) * Time.deltaTime;
        float Y = -Input.GetAxis("Mouse Y") * (SpeedLeftRight * SpeedLeftRight) * Time.deltaTime;
        float eulerX = (transform.rotation.eulerAngles.x + Y);
        float eulerY = (transform.rotation.eulerAngles.y + X);
        transform.rotation = Quaternion.Euler(eulerX, eulerY, 0);

        //transform.rotation = Quaternion.Euler(Y, X, 0);
        //transform.rotation = new Quaternion(transform.rotation.x, transform.rotation.y, 0, transform.rotation.w);

        if (ready_shot) 
        {
            if(ready_zikl == 0)
            {
                Vector3 persons = transform.position;
                
                string findName = "FlyBalls/FlyBall-" + (select + 1).ToString();
                GameObject ballType = (GameObject)Resources.Load(findName);

                new_ball = Instantiate(ballType, new Vector3(persons.x, persons.y, persons.z), new Quaternion(0, 0, 0, 0), transform);
                new_ball.transform.localPosition = new Vector3(0, (float)-0.688, (float)0.546);
                new_ball.GetComponent<ball_script>().is_go = true;
                new_ball.transform.eulerAngles = transform.GetChild(0).eulerAngles;
                new_ball.transform.localScale = new Vector3((float)0.066, (float)0.066, (float)0.066) * ((float)1 / max_zikl);

                ready_zikl++;
            }

            else if (ready_zikl < max_zikl)
            {
                new_ball.transform.eulerAngles = transform.GetChild(0).eulerAngles;
                new_ball.transform.localScale = new_ball.transform.localScale / (((float)ready_zikl) / max_zikl) * ((float)(ready_zikl + 1) / max_zikl);
                ready_zikl++;
            }

            else if (ready_zikl == max_zikl)
            {
                new_ball.GetComponent<ball_script>().is_go = false;
                new_ball.GetComponent<ball_script>().is_magneted = false;
                ready_zikl++;
            }

            else
            {
                new_ball.transform.eulerAngles = transform.GetChild(0).eulerAngles;
            }

            if (Input.GetKeyUp(KeyCode.Mouse0) && ready_zikl > max_zikl)
            {
                ready_shot = false;
                Main.transform.GetComponent<balls_main_script>().ShotBall(transform.position, new_ball, select);
                CheckShootBallScript.StartTimer();

                select = prev_select;
                prev_select = GameObject.Find("Main").transform.GetComponent<balls_main_script>().SelectColor("");
                ready_zikl = 0;

                string findName = "Visual/TypeBall-" + (prev_select + 1).ToString();
                GameObject ballType = (GameObject)Resources.Load(findName);
                Destroy(LPlace.transform.GetChild(0).gameObject);
                Destroy(RPlace.transform.GetChild(0).gameObject);
                GameObject ball = Instantiate(ballType, LPlace.transform);
                ball.transform.localPosition = new Vector3(0, 0, 0);
                ball = Instantiate(ballType, RPlace.transform);
                ball.transform.localPosition = new Vector3(0, 0, 0);                
            }

            else if(Input.GetKeyUp(KeyCode.Mouse1) && ready_zikl > max_zikl)
            {
                ready_shot = true;

                int temp = select;
                select = prev_select;
                prev_select = temp;
                ready_zikl = 0;

                string findName = "Visual/TypeBall-" + (prev_select + 1).ToString();
                GameObject ballType = (GameObject)Resources.Load(findName);
                Destroy(LPlace.transform.GetChild(0).gameObject);
                Destroy(RPlace.transform.GetChild(0).gameObject);
                GameObject ball = Instantiate(ballType, LPlace.transform);
                ball.transform.localPosition = new Vector3(0, 0, 0);
                ball = Instantiate(ballType, RPlace.transform);
                ball.transform.localPosition = new Vector3(0, 0, 0);

                Destroy(new_ball.gameObject);
            }
        }

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
