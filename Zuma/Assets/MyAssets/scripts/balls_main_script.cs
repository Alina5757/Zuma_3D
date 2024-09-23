using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class balls_main_script : MonoBehaviour
{
    // Индекс шара - индивидуальное неизменное float число, требуется чтобы шары не могли путаться. Номер шара - динамическая характеристика, номер в цепочке начиная с ее головы

    public GameObject prefabParent;
    public GameObject flyPrefab;
    public GameObject Ball_collector;
    public GameObject Fly_collector;
    public GameObject Person;
    public GameObject Cubes;
    public GameObject winnerText;

    static float speed = 0.06f;
    float speed_fly = 4f;
    float speed_go = 0.07f;

    Vector3? start_ball_pos = null;    

    float time = 20;
    private float _timeLeft = 0f;

    public GameObject mainPointsObj;

    public static float razbr = 15;

    public static List<Transform> arrayPoints = new List<Transform>();

    List<Point> points = new List<Point>();
    public static GameObject prev_ball = null;

    List<(GameObject, int)> balls_colors = new List<(GameObject, int)>();  // массив шаров, и их индекс цвета
    int number_point_first_ball; // индекс путевой точки на которую направлен первый шар

    public static bool ready_to_new_ball = false;  // отслеживание шаров показывает что площадка пуста и можно сгенерировать еще шар
    public GameObject Creater_new_balls;    
    List<Vector3> prev_positions = new List<Vector3>();   // массив просчитанных точек    
    List<int> go_to = new List<int>();  // положение шаров относительно просчитанных точек
    List<(Vector3, GameObject, float, int)> fly_balls = new List<(Vector3, GameObject, float, int)>();  // шары которые в данный момент находятся в полете

    public GameObject Pointscount;
    public GameObject NeedPoints;
    int points_need_to_win = 2000; // количество очков для победы
    public int game_points = 0; // очки набранные за игру
    int point_for_ball = 100; // очки за один шар
    double mnoz = 1.5; // множитель за комбо
    int kombo = 0; // удалений шаров подряд
    bool flagWin = false;

    bool isAdded = false; // встраивается ли в данный момент шар
    bool isMagnet = false; // примагничиваются ли в данный момент шары (обратное движение если при разъединении цепочки на обоих ее концах шары одного цвета)

    int? indexFirstRunBall; // Первый шар который движется при вставке шара
    Vector3? need_point_index;  // Место куда он должен подвинуться
    GameObject addedBall;  // Добавляемый шар
    int addedBallIndex; // индекс вставки добавляемого шара
    string addedNapr; // Добавляемый шар прикрепляется по направлению forward или back
    Vector3 point_add;   // Место куда встанет шар
    int kount_zikl; // циклов движения до точки
    int razrezInd; // номер кусочка в который встраивается шар
    bool flagForward; // указывает встраивается ли шар в переднюю часть кусочка

    public bool isDeleted = false; // изменяется при попадании летящего шара в область очистки

    int rasstMBalls = 245; // расстояние в точках между соседними шарами
    int speed_add_ball = 5; // скорость добавления шарика в цепочку

    int magnet_zikl = 0; // шаг магнетизма, ускоряет процесс примагничивания
    int speed_magnet = 5; // начальная скорость перемещения шаров при эффекте примагничивания

    float? leftStoppedBall = null; //самый левый неподвижный шар оторванный от цепочки, номер шара
    int? rightGoBall = 0; // Самый первый движущийся шар, номер шара

    int kolvo_types_balls = 4; // количество цветов у шариков
    System.Random rand = new System.Random();
    int harder = 65;  //от 0 до 100, сложность игры

    /// <summary>
    /// начало и конец кусочка цепи, точка присоединения;  от минимальных индексов к максимальным
    /// </summary>    
    List<(int, int, int)> piecesChain = new List<(int, int, int)>();

    // хранение ожидания встраиваемого летящего шара, на случай если прошлый шар еще не успел встроиться
    bool flag_wait_fly_ball = false;
    List <GameObject> wait_fly_added_ball;
    List <GameObject> wait_ball_Touch;
    List <string> wait_napr;

    void Generate(List<int> colors, List<int> go_to_added, List<(int, int)> piece)
    {
        int need_point = 1;
        for (int ball = 0; ball < colors.Count; ball++)
        {
            int select = colors[ball];
            string findName = "TypesBalls/TypeBall-" + (select + 1).ToString();
            GameObject ballType = (GameObject)Resources.Load(findName);

            GameObject createdBall = Instantiate(ballType, (Vector3)start_ball_pos, new Quaternion(0, 0, 0, 0), Ball_collector.transform);
            if(ball != 0)
            {
                go_to.Insert(0, go_to[0] + 245 + go_to_added[colors.Count - ball - 1]);
                need_point = go_to[0] + 1;
            }
            else
            {
                go_to.Insert(0, 1);
            }
            
            balls_colors.Add((createdBall, select));            
        }

        for(int razr = 0; razr < piece.Count; razr++)
        {
            piecesChain.Add((piece[razr].Item1, piece[razr].Item2, 0));
        }

        need_point++;
        Vector3 new_pos = (Vector3)start_ball_pos;
        for (int p = 0; p < need_point; p++)
        {
            List<(float, float, float, bool)> position_new = NewPosition(new_pos.x, new_pos.y, new_pos.z,
                points[number_point_first_ball], points[number_point_first_ball + 1], speed);

            prev_positions.Add(new Vector3(position_new[0].Item1, position_new[0].Item2, position_new[0].Item3));
            new_pos = new Vector3(position_new[0].Item1, position_new[0].Item2, position_new[0].Item3);
            if (position_new[0].Item4)
            {
                number_point_first_ball++;
            }

            new_pos = new Vector3(position_new[0].Item1, position_new[0].Item2, position_new[0].Item3);
        }
        for(int ball = 0; ball < balls_colors.Count; ball++)
        {
            balls_colors[ball].Item1.transform.position = prev_positions[go_to[ball]];
            balls_colors[ball].Item1.transform.LookAt(prev_positions[go_to[ball] + 1]);
        }

        GetLeftRightBalls();
    }

    /// <summary>
    /// меньшее значение, большее значение, куда добавляется элемент, добавляемый элемент, используется ли перед добавлением
    /// </summary> 
    void TestCheck(int smallItem, int bigItem, int index_add, (int, int, int) elem_add, bool prev)
    {
        int prev_forward = -1;
        int prev_back = -1;
        for (int r = 0; r < piecesChain.Count; r++)
        {            
            //if (prev && r == index_add)
            //{
            //    prev_forward = elem_add.Item1;
            //    prev_back = elem_add.Item2;
            //    if (elem_add.Item1 < prev_back)
            //    {
            //        int j = 0;
            //    }
            //}

            if (prev_forward == -1)
            {
                prev_forward = piecesChain[r].Item1;
                prev_back = piecesChain[r].Item2;
            }
            else
            {
                if (piecesChain[r].Item1 < prev_back)
                {
                    int j = 0;
                }
                prev_forward = piecesChain[r].Item1;
                prev_back = piecesChain[r].Item2;
            }
        }
        if (smallItem > bigItem)
        {
            int j = 0;
        }
    }

    private IEnumerator StartTimer()
    {
        while (true)
        {
            _timeLeft = time;
            while (_timeLeft > 0)
            {
                _timeLeft -= Time.deltaTime;


                //////for (int number = 0; number < balls_points.Count; number++)
                //////{
                //////    List<(float, float, float, bool)> position_new = NewPosition(balls_points[number].Item1.transform.position.x, balls_points[number].Item1.transform.position.y,
                //////        balls_points[number].Item1.transform.position.z, points[balls_points[number].Item2], points[balls_points[number].Item2 + 1], speed);


                //////    balls_points[number].Item1.transform.position = new Vector3(position_new[0].Item1, position_new[0].Item2, position_new[0].Item3);
                //////    if (position_new[number].Item4)
                //////    {
                //////        int nnew_point = balls_points[number].Item2 + 1;
                //////        GameObject ball = balls_points[number].Item1;
                //////        balls_points.RemoveAt(number);
                //////        balls_points.Insert(number, (ball, nnew_point));
                //////    }
                //////    balls_points[0].Item1.transform.rotation.SetLookRotation(new Vector3(arrayPoints[balls_points[number].Item2].position.x,
                //////        arrayPoints[balls_points[number].Item2].position.y,
                //////        arrayPoints[balls_points[number].Item2].position.z));
                //////}
                //foreach (GameObject ball in arrayBalls){
                //    //((Rigidbody)prev_ball.GetComponent("Rigidbody")).AddForce(new Vector3((points[1].x - points[0].x),
                //    //    (points[1].y - points[0].y),
                //    //    (points[1].z - points[0].z)) * count_balls / del);
                //    //((Rigidbody)prev_ball.GetComponent("Rigidbody")).AddForce(new Vector3(0, 0, -1) * speed);
                //    //prev_ball.transform.rotation = Quaternion.LookRotation(arrayPoints[1].position);
                //    //((Rigidbody)prev_ball.GetComponent("Rigidbody")).AddTorque(new Vector3(arrayPoints[1].position.x - arrayPoints[0].position.x, 
                //    //    arrayPoints[1].position.y - arrayPoints[0].position.y,
                //    //    arrayPoints[1].position.z - arrayPoints[0].position.z) * speed);
                //    ((Rigidbody)prev_ball.GetComponent("Rigidbody")).AddTorque(new Vector3(arrayPoints[1].position.x - arrayPoints[0].position.x,
                //        arrayPoints[1].position.y - arrayPoints[0].position.y,
                //        arrayPoints[1].position.z - arrayPoints[0].position.z) * speed);


                //}
                ////if (prev_ball != null)
                ////{
                ////    ((Rigidbody)prev_ball.GetComponent("Rigidbody")).AddForce(new Vector3((points[1].x - points[0].x),
                ////        (points[1].y - points[0].y),
                ////        (points[1].z - points[0].z)) * count_balls * 2 / del);
                ////}

                //parent_obj.transform.rotation = Quaternion.LookRotation(arrayPoints[1].position);
                //parent_obj.transform.GetChild(0).GetComponent<Rigidbody>().AddTorque(new Vector3(0, 0, 1) * speed);

                yield return null;
            }

            //////balls_points.Add((Instantiate(prefabParent, new Vector3(start_point_x, start_point_y, start_point_z), new Quaternion(0, 0, 0, 0)), 0));
            
            //GameObject gameObj = Instantiate(prefabParent, new Vector3(start_point_x, start_point_y, start_point_z), new Quaternion(0, 0, 0, 0));


            //if (prev_ball)
            //{
            //    ConfigurableJoint ss = prev_ball.AddComponent<ConfigurableJoint>();
            //    ss.connectedBody = gameObj.GetComponent<Rigidbody>();
            //    ss.autoConfigureConnectedAnchor = false;
            //    ss.connectedAnchor = new Vector3((float)10, (float)0.5, (float)10);
            //    ss.xMotion = ConfigurableJointMotion.Limited;
            //    ss.zMotion = ConfigurableJointMotion.Limited;
            //    ss.enableCollision = true;
            //}
            //prev_ball = gameObj;

            //gameObj.transform.rotation.SetLookRotation(new Vector3(arrayPoints[1].position.x, arrayPoints[1].position.y, arrayPoints[1].position.z));


            //gameObj.transform.LookAt(arrayPoints[1]);
            //arrayBalls.Add(gameObj);
            //count_balls++;
            //if (!(prev_ball == null))
            //{
            //    //SpringJoint ss = prev_ball.AddComponent<SpringJoint>();
            //    //ss.connectedBody = (Rigidbody)gameObj.GetComponent("Rigidbody");
            //    //ss.spring = 60;
            //    //ss.damper = 1;
            //    //ss.autoConfigureConnectedAnchor = false;
            //    //ss.connectedAnchor = new Vector3((float)0.4, (float)0.4, (float)0.4);
            //    //ss.enableCollision = true;
            //}
            //prev_ball = gameObj;
        }        
    }

    List<(float, float, float, bool)> NewPosition(float x, float y, float z, Point next, Point nnext, float speed)  //x, y, z, and perehod to next_point
    {
        float razn_x = x - next.x;
        float razn_y = y - next.y;
        float razn_z = z - next.z;

        float zikls = Mathf.Sqrt(Mathf.Abs(razn_x * razn_x) + Mathf.Abs(razn_y * razn_y) + Mathf.Abs(razn_z * razn_z)) / speed;
        if(zikls > 1)
        {
            List<(float, float, float, bool)> list = new List<(float, float, float, bool)>();
            list.Add((x - razn_x / zikls, y - razn_y / zikls, z - razn_z / zikls, false));
            return list;
        }
        else
        {
            speed -= Mathf.Sqrt(Mathf.Abs(razn_x * razn_x) + Mathf.Abs(razn_y * razn_y) + Mathf.Abs(razn_z * razn_z));
            razn_x = next.x - nnext.x;
            razn_y = next.y - nnext.y;
            razn_z = next.z - nnext.z;
            zikls = Mathf.Sqrt(Mathf.Abs(razn_x * razn_x) + Mathf.Abs(razn_y * razn_y) + Mathf.Abs(razn_z * razn_z)) / speed;

            List<(float, float, float, bool)> list = new List<(float, float, float, bool)>();
            list.Add((next.x - razn_x / zikls, next.y - razn_y / zikls, next.z - razn_z / zikls, true));
            return list;
        }
    }

    void Start()
    {
        NeedPoints.GetComponent<Text>().text = points_need_to_win.ToString();
        _timeLeft = speed_go;
        bool parent = true;
        int point_t = 0;
        int temp = 0;
        GameObject points_obj = GameObject.Find("TracePoints");
        foreach (Transform child in points_obj.GetComponentsInChildren<Transform>())
        {
            if(temp < 2)
            {
                temp++;
                start_ball_pos = child.position;
            }
            if (parent)
            {
                parent = false;
            }
            else
            {
                arrayPoints.Add(child);
                points.Add(new Point(child.position.x, child.position.y, child.position.z));                
                point_t++;
            }            
        }
        // цвета - слева начало змейки
        // разрывы в точках - 0 1500 0 0...  значит что первые два шара стоят отдельно
        // разрывы должны соответствовать их описанию

        // для тестирования
        //List<int> colors = new List<int>() {           0, 0, 1, 1, 2,      0, 0,      1, 1, 2, 2,         3, 0, 1, 2 };
        //List<int> razriv_in_point = new List<int>() {  0, 0, 0, 0, 50,     0, 50,    0, 0, 0, 4000,       0, 0, 0, 0};
        //List<(int, int)> razriv = new List<(int, int)>() { (0, 4), (5, 6), (7, 10) };

        List<int> colors = new List<int>() { 0, 0, 1, 1, 2,  0, 0, 1, 1, 2,  2, 3, 0, 1, 2,  2, 1, 1, 0, 1,  2, 3, 3, 1, 3,  2, 0, 1, 1, 0 };
        List<int> razriv_in_point = new List<int>() { 0, 0, 0, 0, 0,  0, 0, 0, 0, 0,  0, 0, 0, 0, 0,  0, 0, 0, 0, 0,  0, 0, 0, 0, 0,  0, 0, 0, 0, 0 };
        List<(int, int)> razriv = new List<(int, int)>() {};
        Generate(colors, razriv_in_point, razriv);

        //parent_obj = Instantiate(prefabParent, new Vector3(1009, 103, 280), new Quaternion(0, 0, 0, 0));

        //_timeLeft = time;
        //StartCoroutine(StartTimer());
    }

    public int SelectColor(string ballIs) // main or person
    {
        int select;
        if (ballIs.Equals("main"))
        {
            int chanceCopy = rand.Next(0, 100);            
            if (balls_colors.Count > 0 && chanceCopy > harder)
            {
                select = balls_colors[balls_colors.Count - 1].Item2;
            }
            else
            {
                select = rand.Next(0, kolvo_types_balls);
                if (balls_colors.Count > 0 && balls_colors[balls_colors.Count - 1].Item2 == select)
                {
                    if (select == kolvo_types_balls - 1)
                    {
                        select--;
                    }
                    else if (select == 0)
                    {
                        select++;
                    }
                    else if (chanceCopy % 2 == 0)
                    {
                        select--;
                    }
                    else
                    {
                        select++;
                    }
                }
            }           
        }
        else
        {
            select = rand.Next(0, kolvo_types_balls);
        }
        return select;
    }

    /// <summary>
    /// Пересчет индексов кусочков цепочек при добавлении / удалении
    /// </summary>
    void PereschetChain(int start_pos, int smeshenie, int? prikreplBall)  // + индекс к какому шару прикрепляется
    {
        // позволяет отследить ситуацию присоединения к целой части цепи
        bool flag_added = false;
        // добавляется новый шарик
        if (prikreplBall != null && piecesChain.Count > 0)
        {
            if (addedNapr == "Back")
            {
                prikreplBall = start_pos - 1;
            }
            else
            {
                prikreplBall = start_pos;
            }

            for (int razr = 0; razr < piecesChain.Count; razr++)
            {
                // добавление в последующие кусочки
                if (piecesChain[razr].Item1 > prikreplBall)
                {
                    (int, int, int) elem = (piecesChain[razr].Item1 + 1, piecesChain[razr].Item2 + 1, piecesChain[razr].Item3);

                    piecesChain.RemoveAt(razr);
                    piecesChain.Insert(razr, elem);                   
                }
                // добавление в выбранный кусочек
                else if (piecesChain[razr].Item1 <= prikreplBall && piecesChain[razr].Item2 >= prikreplBall)
                {
                    flag_added = true;
                    (int, int, int) elem = (piecesChain[razr].Item1, piecesChain[razr].Item2 + 1, piecesChain[razr].Item3);
                    
                    piecesChain.RemoveAt(razr);
                    piecesChain.Insert(razr, elem);
                   
                    // добавляется в начало дальнего кусочка, проверка на примагничевание к заднему
                    if (balls_colors[piecesChain[razr].Item2].Item2 == balls_colors[piecesChain[razr].Item2 + 1].Item2)
                    {
                        isMagnet = true;
                    }
                    // добавляется в конец ближнего кусочка, проверка на примагничевание переднего кусочка
                    else if (piecesChain[razr].Item1 > 0 && balls_colors[piecesChain[razr].Item1].Item2 == balls_colors[piecesChain[razr].Item1 - 1].Item2)
                    {
                        isMagnet = true;
                    }
                }
            }

            // добавление в начало целой части
            if (!flag_added && balls_colors[piecesChain[piecesChain.Count - 1].Item2].Item2 == balls_colors[piecesChain[piecesChain.Count - 1].Item2 + 1].Item2)
            {
                isMagnet = true;
            }
        }

        // шарики удаляются
        else if (prikreplBall == null)
        {
            // удаление из целой цепочки
            if (piecesChain.Count == 0)
            {
                // удаляется кусок в центре
                if (start_pos != 0)
                {
                    int start_ind = 0;
                    
                    piecesChain.Add((start_ind, start_pos - 1, prev_positions.IndexOf(balls_colors[start_pos].Item1.transform.position)));
                    
                    for (int ball = start_ind; ball <= start_pos - 1; ball++)
                    {
                        balls_colors[ball].Item1.GetComponent<ball_script>().is_go = false;
                    }
                }
            }

            // цепь разделена  
            // удаление в хвосте целой цепи
            else if (piecesChain[piecesChain.Count - 1].Item2 < start_pos)
            {
                if(piecesChain.Count == 3)
                {
                    int o = 0;
                }
                // удаляется кусок в центре
                if (piecesChain[piecesChain.Count - 1].Item2 + 1 != start_pos)
                {
                    int start_ind;
                    start_ind = piecesChain[piecesChain.Count - 1].Item2 + 1;
                    
                    piecesChain.Add((start_ind, start_pos - 1, prev_positions.IndexOf(balls_colors[start_pos].Item1.transform.position)));
                   
                    for (int ball = start_ind; ball <= start_pos - 1; ball++)
                    {
                        balls_colors[ball].Item1.GetComponent<ball_script>().is_go = false;
                    }
                }
            }

            // удаление в кусочках
            else
            {
                bool flagDleted = true;
                int maxCount = piecesChain.Count;
                for (int razr = 0; razr < maxCount; razr++)
                {
                    // удаление в этом кусочке
                    if (flagDleted && (piecesChain[razr].Item1 <= start_pos && piecesChain[razr].Item2 > start_pos))
                    {
                        // удаление целого куска
                        if (piecesChain[razr].Item1 == start_pos && piecesChain[razr].Item2 == start_pos + (smeshenie - 1))
                        {
                            piecesChain.RemoveAt(razr);
                            maxCount--;
                            razr--;
                            flagDleted = false;
                        }
                        // удаление части в начале или в конце
                        else if (piecesChain[razr].Item1 == start_pos || piecesChain[razr].Item2 == start_pos + (smeshenie - 1))
                        {
                            (int, int, int) elem = (piecesChain[razr].Item1, piecesChain[razr].Item2 - smeshenie, piecesChain[razr].Item3);

                            piecesChain.RemoveAt(razr);
                            piecesChain.Insert(razr, elem);
                           
                            flagDleted = false;
                        }
                        // удаление части в середине
                        else
                        {
                            (int, int, int) elem_start = (piecesChain[razr].Item1, start_pos - 1, prev_positions.IndexOf(balls_colors[start_pos].Item1.transform.position));
                            (int, int, int) elem_end = (start_pos + smeshenie, piecesChain[razr].Item2, piecesChain[razr].Item3);

                            piecesChain.RemoveAt(razr);
                            piecesChain.Insert(razr, elem_end);
                            piecesChain.Insert(razr, elem_start);
                            
                            flagDleted = false;
                            maxCount++;
                        }
                    }
                    // удалено из прошлого кусочка
                    else if (!flagDleted || piecesChain[razr].Item1 > start_pos)
                    {
                        (int, int, int) elem = (piecesChain[razr].Item1 - smeshenie, piecesChain[razr].Item2 - smeshenie, piecesChain[razr].Item3);

                        piecesChain.RemoveAt(razr);
                        piecesChain.Insert(razr, elem);
                    }
                }
            }
        }
        if (piecesChain.Count > 0)
        {
            rightGoBall = piecesChain[piecesChain.Count - 1].Item2 + 1;
            leftStoppedBall = piecesChain[piecesChain.Count - 1].Item2;
        }
        else
        {
            rightGoBall = 0;
            leftStoppedBall = null;
        }
    }

    void BallsDeleter(int addedBallposition, int addedBallColor)
    {
        try
        {
            razrezInd = -1;
            int ind_razr = -1;
            int countBalls = 1;

            int forwardDelete = addedBallposition;
            int backDelete = addedBallposition;

            int globalIndForwardBall = -1;

            bool flagForward = true;
            bool flagBack = true;

            // если цепь целая
            if (piecesChain.Count == 0)
            {
                while (flagForward)
                {
                    forwardDelete--;
                    if (forwardDelete >= 0)
                    {
                        if (balls_colors[forwardDelete].Item2 == addedBallColor)
                        {
                            countBalls++;
                        }
                        else
                        {
                            globalIndForwardBall = forwardDelete;
                            flagForward = false;
                            forwardDelete++;
                        }
                    }
                    else
                    {
                        flagForward = false;
                        forwardDelete++;
                    }
                }
                while (flagBack)
                {
                    backDelete++;
                    if (backDelete < balls_colors.Count && balls_colors[backDelete].Item2 == addedBallColor)
                    {
                        countBalls++;
                    }
                    else
                    {
                        flagBack = false;
                        backDelete--;
                    }
                }
            }
            // цепь разделена
            else
            {
                for(int razr = piecesChain.Count - 1; razr >= 0; razr--)
                {
                    if(piecesChain[razr].Item1 <= addedBallposition && piecesChain[razr].Item2 >= addedBallposition)
                    {
                        razrezInd = razr;
                        break;
                    }
                }

                // удаление из целой части
                if (razrezInd == - 1)
                {
                    while (flagForward)
                    {
                        forwardDelete--;
                        if (forwardDelete >= piecesChain[piecesChain.Count - 1].Item2 + 1)
                        {
                            if (balls_colors[forwardDelete].Item2 == addedBallColor)
                            {
                                countBalls++;
                            }
                            else
                            {
                                globalIndForwardBall = forwardDelete;
                                flagForward = false;
                                forwardDelete++;
                            }
                        }
                        else
                        {
                            flagForward = false;
                            forwardDelete++;
                        }
                    }
                    while (flagBack)
                    {
                        backDelete++;
                        if (backDelete < balls_colors.Count && balls_colors[backDelete].Item2 == addedBallColor)
                        {
                            countBalls++;
                        }
                        else
                        {
                            flagBack = false;
                            backDelete--;
                        }
                    }
                }
                // удаление из кусочка
                else
                {
                    while (flagForward)
                    {
                        forwardDelete--;
                        if (forwardDelete >= piecesChain[razrezInd].Item1)
                        {
                            if (forwardDelete >= 0 && balls_colors[forwardDelete].Item2 == addedBallColor)
                            {
                                countBalls++;
                            }
                            else
                            {
                                globalIndForwardBall = forwardDelete;
                                flagForward = false;
                                forwardDelete++;
                            }
                        }
                        else
                        {
                            flagForward = false;
                            forwardDelete++;
                        }
                    }
                    while (flagBack)
                    {
                        backDelete++;
                        if (backDelete <= piecesChain[razrezInd].Item2 && balls_colors[backDelete].Item2 == addedBallColor)
                        {
                            countBalls++;
                        }
                        else
                        {
                            flagBack = false;
                            backDelete--;
                        }
                    }
                }
            }
            

            // само удаление шаров
            int point = -1;
            if (countBalls >= 3)
            {
                PereschetChain(forwardDelete, countBalls, null);
                for (int ball = backDelete; ball >= forwardDelete; ball--)
                {
                    point = prev_positions.IndexOf(balls_colors[ball].Item1.transform.position);
                    go_to.RemoveAt(ball);
                    Destroy(balls_colors[ball].Item1.gameObject);
                    balls_colors.RemoveAt(ball);
                }

                game_points += (int)(point_for_ball * countBalls * Math.Pow(mnoz, kombo));
                kombo++;

                if(game_points >= points_need_to_win) {
                    flagWin = true;
                }

                if (forwardDelete > 0 && balls_colors[forwardDelete - 1].Item2 == balls_colors[forwardDelete].Item2)
                {
                    isMagnet = true;
                }
            }
        }
        catch(Exception e)
        {
            int k = 0;
        }
    }

    // проверка не нужно ли возобновить движение шаров
    int? CheckContinueGo(Vector3 position, int LballIndex)
    {
        if (piecesChain.Count > 0)
        {
            int razr_ind = -1;
            int index_ball = - 1;

            // какой кусочек прикрепляется к LballIndex шару
            for (int razr = piecesChain.Count - 1; razr >= 0; razr--)
            {
                if(piecesChain[razr].Item2 == LballIndex - 1)
                {
                    index_ball = piecesChain[razr].Item2;
                    razr_ind = razr;
                }                
            }
            Vector3 ball_pos;

            if(index_ball != -1)
            {
                if (index_ball == 0 && go_to[0] >= prev_positions.Count)
                {
                    ball_pos = prev_positions[prev_positions.Count - 1];
                }
                else
                {
                    ball_pos = balls_colors[index_ball].Item1.transform.position;
                }

                if (prev_positions.IndexOf(position) >= prev_positions.IndexOf(ball_pos) - rasstMBalls)
                {
                    if(razr_ind == piecesChain.Count - 1)
                    {
                        for (int ball = piecesChain[piecesChain.Count - 1].Item1; ball <= piecesChain[piecesChain.Count - 1].Item2; ball++)
                        {
                            balls_colors[ball].Item1.GetComponent<ball_script>().is_go = true;
                        }
                        int t = piecesChain[razr_ind].Item1;
                        piecesChain.RemoveAt(razr_ind);
                        return t;
                    }
                    else
                    {
                        (int, int, int) new_razr = (piecesChain[razr_ind].Item1, piecesChain[razr_ind + 1].Item2, 0);
                        piecesChain.RemoveAt(razr_ind);
                        piecesChain.RemoveAt(razr_ind);
                        piecesChain.Insert(razr_ind, new_razr);
                        return new_razr.Item1;
                    }
                }
            }
        }
        return LballIndex;
    }

    void GetLeftRightBalls()
    {
        if (piecesChain.Count > 0)
        {
            rightGoBall = piecesChain[piecesChain.Count - 1].Item2 + 1;
            leftStoppedBall = rightGoBall - 1;
        }
        else
        {
            rightGoBall = 0;
            leftStoppedBall = null;
        }
    }

    /// <summary>
    /// Продвижение первого шара для рассчитанных точек и с рассчетом точек
    /// </summary>  
    void FirstBallGo()
    {        
        // точки просчитаны
        if (prev_positions.Count <= prev_positions.IndexOf(balls_colors[0].Item1.transform.position) + 1)
        {
            List<(float, float, float, bool)> position_new = NewPosition(balls_colors[0].Item1.transform.position.x, balls_colors[0].Item1.transform.position.y,
                balls_colors[0].Item1.transform.position.z, points[number_point_first_ball], points[number_point_first_ball + 1], speed);
            balls_colors[0].Item1.transform.LookAt(new Vector3(position_new[0].Item1, position_new[0].Item2, position_new[0].Item3));

            prev_positions.Add(new Vector3(position_new[0].Item1, position_new[0].Item2, position_new[0].Item3));
            balls_colors[0].Item1.transform.position = new Vector3(position_new[0].Item1, position_new[0].Item2, position_new[0].Item3);
            if (position_new[0].Item4)
            {
                number_point_first_ball++;
            }
            balls_colors[0].Item1.transform.rotation.SetLookRotation(new Vector3(arrayPoints[number_point_first_ball].position.x,
                arrayPoints[number_point_first_ball].position.y,
                arrayPoints[number_point_first_ball].position.z));
        }
        else
        {
            int indexPosition = prev_positions.IndexOf(balls_colors[0].Item1.transform.position) + 1;
            balls_colors[0].Item1.transform.position = new Vector3(prev_positions[indexPosition].x, prev_positions[indexPosition].y, prev_positions[indexPosition].z);

            if (prev_positions.Count > indexPosition + 1)
            {
                balls_colors[0].Item1.transform.rotation.SetLookRotation(new Vector3(prev_positions[indexPosition + 1].x,
                prev_positions[indexPosition + 1].y, prev_positions[indexPosition + 1].z));
            }
            else
            {
                balls_colors[0].Item1.transform.rotation.SetLookRotation(new Vector3(arrayPoints[number_point_first_ball].position.x,
               arrayPoints[number_point_first_ball].position.y, arrayPoints[number_point_first_ball].position.z));
            }
        }
        go_to[0]++;
    }

    // откат шаров обратно если их цвет совпадает
    void MagnetismBallsColors()
    {
        // есть ли подходящие шары для примагничевания
        bool isMagneted = false;
        int start_index = -1;
        int razr_num = -1;
        if (piecesChain.Count == 1)
        {
            if (balls_colors[piecesChain[0].Item2 + 1].Item2 == balls_colors[piecesChain[0].Item2].Item2)
            {
                isMagneted = true;
                start_index = piecesChain[0].Item2;
                razr_num = 0;
            }
        }
        else if (piecesChain.Count > 0)
        {
            for (int razr = piecesChain.Count - 1; razr >= 0; razr--)
            {
                if (balls_colors[piecesChain[razr].Item2 + 1].Item2 == balls_colors[piecesChain[razr].Item2].Item2)
                {
                    isMagneted = true;
                    start_index = piecesChain[razr].Item2;
                    razr_num = razr;
                    break;
                }
            }
        }

        if (!isMagneted)
        {
            for(int ball = balls_colors.Count - 1; ball >= 0; ball--)
            {
                ball_script script = balls_colors[ball].Item1.GetComponent<ball_script>();
                if (ball >= rightGoBall)
                {
                    script.is_go = true;
                }
                else
                {
                    script.is_go = false;
                }
            }

            magnet_zikl = 0;
            isMagnet = false;
        }
        else
        {
            // визуальное отображение остановки шаров
            for (int ball = balls_colors.Count - 1; ball > start_index; ball--)
            {
                ball_script script = balls_colors[ball].Item1.GetComponent<ball_script>();
                script.is_go = false;
            }
            // визуальное отображение откатывания шаров
            for (int ball = start_index; ball >= piecesChain[razr_num].Item1; ball--)
            {
                ball_script script = balls_colors[ball].Item1.GetComponent<ball_script>();
                script.is_magneted = true;
                script.is_go = false;
            }

            // флаг, необходим для определения было ли присоединение
            bool prisoed = false;
            for (int z = speed_magnet + (int)(magnet_zikl*0.4); z > 0; z--)
            {
                if (prisoed)
                {
                    break;
                }
                for (int ball = start_index; ball >= piecesChain[razr_num].Item1; ball--)
                {
                    // проверка на присоединение
                    if (ball == start_index)
                    {
                        if (go_to[ball + 1] == go_to[ball] - 245)
                        {
                            start_index -= (piecesChain[razr_num].Item2 - piecesChain[razr_num].Item1);
                            for(int b = piecesChain[razr_num].Item2; b >= piecesChain[razr_num].Item1; b--)
                            {
                                balls_colors[b].Item1.GetComponent<ball_script>().is_magneted = false;
                            }

                            int num_ball = piecesChain[razr_num].Item2;
                            if(razr_num == piecesChain.Count - 1)
                            {
                                piecesChain.RemoveAt(piecesChain.Count - 1);
                            }
                            else
                            {
                                (int, int, int) elem = (piecesChain[razr_num].Item1, piecesChain[razr_num + 1].Item2, 0);
                                piecesChain.RemoveAt(razr_num);
                                piecesChain.RemoveAt(razr_num);
                                piecesChain.Insert(razr_num, elem);

                            }
                            BallsDeleter(num_ball, balls_colors[num_ball].Item2);

                            GetLeftRightBalls();
                            prisoed = true;
                            break;
                        }
                        magnet_zikl = 0;
                    }

                    // отдельные действия для первого шара, так как есть проблемы с просчитанными точками
                    if (ball == 0 && go_to[0] >= prev_positions.Count)
                    {
                        balls_colors[0].Item1.transform.position = prev_positions[prev_positions.Count - 1];
                        go_to[0] = prev_positions.Count - 1;
                    }
                    // откат шаров назад
                    else
                    {
                        balls_colors[ball].Item1.transform.position = prev_positions[go_to[ball] - 1];
                        go_to[ball]--;
                    }
                }
            }
            magnet_zikl++;
        }
    }

    void BallsGoStandart()
    {
        // при условии что цепочка целая
        if (leftStoppedBall == null)
        {
            FirstBallGo();
        }

        // шарики после первого катятся по уже имеющимся точкам
        // если цепочка целая
        if (leftStoppedBall == null)
        {
            for (int number = 1; number < balls_colors.Count; number++)
            {
                balls_colors[number].Item1.transform.position = prev_positions[go_to[number] + 1];
                go_to[number]++;
                Vector3 vect_new = prev_positions[go_to[number] + 1];
                balls_colors[number].Item1.transform.LookAt(new Vector3(vect_new.x, vect_new.y, vect_new.z));
            }
        }
        // если в цепи есть промежутки
        else
        {
            GetLeftRightBalls();
            // Для самого первого движущегося шара
            balls_colors[(int)rightGoBall].Item1.transform.position = prev_positions[go_to[(int)rightGoBall] + 1];
            go_to[(int)rightGoBall]++;
            Vector3 vect_new = prev_positions[go_to[(int)rightGoBall] + 1];
            balls_colors[(int)rightGoBall].Item1.transform.LookAt(new Vector3(vect_new.x, vect_new.y, vect_new.z));
            CheckContinueGo(prev_positions[go_to[(int)rightGoBall]], (int)rightGoBall);
            
            // Для всех остальных шаров
            for (int number = (int)rightGoBall + 1; number < balls_colors.Count; number++)
            {
                balls_colors[number].Item1.transform.position = prev_positions[go_to[number] + 1];
                go_to[number]++;
                vect_new = prev_positions[go_to[number] + 1];
                balls_colors[number].Item1.transform.LookAt(new Vector3(vect_new.x, vect_new.y, vect_new.z));
            }
        }
    }

    void BallsGo_VstroenieFlyBall()
    {
        //kount_zikl = 245;
        //// позиция вставки добавляемого шара
        //point_add = prev_positions[prev_positions.IndexOf(balls_colors[(int)indexFirstRunBall].Item1.transform.position) + 245];
        //// необходимая позиция для продвижения шара к которому идет прикрепление и всех дальнейших шаров соответственно
        //need_point_index = prev_positions[prev_positions.IndexOf(balls_colors[(int)indexFirstRunBall].Item1.transform.position) + 245];
        //indexFirstRunBall = 0;

        // нужно ли начать крутится вставленному шарику
        bool ball_run = false;

        // если шар встает на первую позицию
        if(indexFirstRunBall == null)
        {
            if(kount_zikl == 245)
            {
                for (int ball = balls_colors.Count - 1; ball >= 0; ball--)
                {
                    balls_colors[ball].Item1.GetComponent<ball_script>().is_go = false;
                }
            }

            //встает на нужную позицию во главе цепочки
            if (addedBall.transform.position != point_add)
            {
                Vector3 pos = addedBall.transform.position;
                addedBall.transform.position = new Vector3(pos.x + (point_add.x - pos.x) / kount_zikl,
                    pos.y + (point_add.y - pos.y) / kount_zikl, pos.z + (point_add.z - pos.z) / kount_zikl);
                kount_zikl--;
            }
        }
        else
        {
            // цепь целая
            // flagForward позволяет не перескочить на этот метод вставки с метода вставки на первую позицию движущейся части в варианте, когда цепь разделена
            if(piecesChain.Count == 0 && !flagForward)
            {
                if (kount_zikl == 245)
                {
                    for (int ball = balls_colors.Count - 1; ball >= addedBallIndex; ball--)
                    {
                        balls_colors[ball].Item1.GetComponent<ball_script>().is_go = false;
                    }
                }

                //первый движущийся шар встает на нужную позицию
                if (balls_colors[(int)indexFirstRunBall].Item1.transform.position != need_point_index)
                {
                    if(indexFirstRunBall != 0)
                    {
                        for (int ball = (int)indexFirstRunBall; ball > 0; ball--)
                        {
                            balls_colors[ball].Item1.transform.position = prev_positions[go_to[ball] + 1];
                            go_to[ball]++;
                            Vector3 vect_new = prev_positions[go_to[ball] + 1];
                            balls_colors[ball].Item1.transform.LookAt(new Vector3(vect_new.x, vect_new.y, vect_new.z));
                        }
                    }

                    FirstBallGo();

                    Vector3 pos = addedBall.transform.position;
                    addedBall.transform.position = new Vector3(pos.x + (point_add.x - pos.x) / kount_zikl,
                        pos.y + (point_add.y - pos.y) / kount_zikl, pos.z + (point_add.z - pos.z) / kount_zikl);
                    kount_zikl--;
                }
                // заглушка на вариант зависания
                else
                {
                    kount_zikl = 0;
                }
            }
            // цепь поделена на сегменты
            else
            {
                // addedBallIndex начало движения, rightGoBall конец движения, indexFirstRunBall определение шарика привязки
                if (kount_zikl == 245)
                {
                    for (int ball = balls_colors.Count - 1; ball > leftStoppedBall; ball--)
                    {
                        balls_colors[ball].Item1.GetComponent<ball_script>().is_go = false;
                    }

                    flagForward = false;
                    rightGoBall = null;
                    razrezInd = -1;

                    // определение индекса шарика привязки
                    if(addedNapr == "Back")
                    {
                        indexFirstRunBall = addedBallIndex - 1;
                    }
                    else
                    {
                        indexFirstRunBall = addedBallIndex;
                    }

                    // поиск кусочка 
                    for (int razr = 0; razr < piecesChain.Count; razr++)
                    {
                        if (piecesChain[razr].Item1 <= indexFirstRunBall && piecesChain[razr].Item2 >= indexFirstRunBall)
                        {
                            razrezInd = razr;
                            rightGoBall = piecesChain[razr].Item1;
                            if (piecesChain[razr].Item1 == addedBallIndex)
                            {                                
                                flagForward = true;                                
                            }
                            break;
                        }
                    }

                    if(rightGoBall == null)
                    {
                        rightGoBall = piecesChain[piecesChain.Count - 1].Item2 + 1;

                        // шар добавляется в голову движения, перед первым движущимся шаром
                        if(addedBallIndex == rightGoBall)
                        {
                            // позиция вставки добавляемого шара
                            point_add = prev_positions[prev_positions.IndexOf(balls_colors[addedBallIndex].Item1.transform.position) + 245];
                            rightGoBall = null;
                            flagForward = true;
                        }                       
                    }
                }

                // встраивается в середину или конец кусочка
                if (!flagForward && razrezInd != -1)
                {
                    //первый движущийся шар встает на нужную позицию
                    if (balls_colors[(int)indexFirstRunBall].Item1.transform.position != need_point_index)
                    {
                        for (int ball = addedBallIndex - 1; ball > rightGoBall; ball--)
                        {
                            balls_colors[ball].Item1.transform.position = prev_positions[go_to[ball] + 1];
                            go_to[ball]++;
                            Vector3 vect_new = prev_positions[go_to[ball] + 1];
                            balls_colors[ball].Item1.transform.LookAt(new Vector3(vect_new.x, vect_new.y, vect_new.z));
                        }

                        // продвижение первого шара
                        // если это самый дальний кусочек
                        if (rightGoBall == 0)
                        {
                            FirstBallGo();
                        }
                        else
                        {
                            balls_colors[(int)rightGoBall].Item1.transform.position = prev_positions[go_to[(int)rightGoBall] + 1];
                            go_to[(int)rightGoBall]++;
                            Vector3 vect_new = prev_positions[go_to[(int)rightGoBall] + 1];
                            balls_colors[(int)rightGoBall].Item1.transform.LookAt(new Vector3(vect_new.x, vect_new.y, vect_new.z));
                        }

                        rightGoBall = CheckContinueGo(balls_colors[(int)rightGoBall].Item1.transform.position, (int)rightGoBall);
                        

                        Vector3 pos = addedBall.transform.position;
                        addedBall.transform.position = new Vector3(pos.x + (point_add.x - pos.x) / kount_zikl,
                            pos.y + (point_add.y - pos.y) / kount_zikl, pos.z + (point_add.z - pos.z) / kount_zikl);
                        kount_zikl--;
                    }
                    // заглушка на вариант зависания
                    else
                    {
                        kount_zikl = 0;
                    }
                }
                // встраивается во главу кусочка
                else if (razrezInd != -1)
                {
                    // добавляемая позиция перекрывает точку присоединения
                    int index_L_Ball = -1;
                    if (piecesChain.Count > razrezInd - 1)
                    {
                        index_L_Ball = piecesChain[razrezInd - 1].Item2;
                    }
                    if (index_L_Ball != -1 && prev_positions.IndexOf(point_add) > prev_positions.IndexOf(balls_colors[index_L_Ball].Item1.transform.position) - rasstMBalls)
                    {
                        //встает на нужную позицию
                        if (addedBall.transform.position != point_add)
                        {
                            for (int ball = (int)indexFirstRunBall - 1; ball >= rightGoBall; ball--)
                            {
                                if (ball == 0)
                                {
                                    FirstBallGo();
                                }
                                else
                                {
                                    balls_colors[ball].Item1.transform.position = prev_positions[go_to[ball] + 1];
                                    go_to[ball]++;
                                    Vector3 vect_new = prev_positions[go_to[ball] + 1];
                                    balls_colors[ball].Item1.transform.LookAt(new Vector3(vect_new.x, vect_new.y, vect_new.z));
                                }
                            }

                            Vector3 pos1 = addedBall.transform.position;
                            addedBall.transform.position = new Vector3(pos1.x + (point_add.x - pos1.x) / kount_zikl,
                                pos1.y + (point_add.y - pos1.y) / kount_zikl, pos1.z + (point_add.z - pos1.z) / kount_zikl);
                            kount_zikl--;
                            if(rightGoBall != 0)
                            {
                                int? t;
                                if (rightGoBall == indexFirstRunBall)
                                {
                                    t = CheckContinueGo(prev_positions[go_to[(int)rightGoBall] + (245 - kount_zikl)], (int)rightGoBall);
                                }
                                else
                                {
                                    t = CheckContinueGo(prev_positions[go_to[(int)rightGoBall]], (int)rightGoBall);
                                }
                                if (t != -1)
                                {
                                    rightGoBall = t;
                                }
                            }                            
                        }
                        // заглушка на вариант зависания
                        else
                        {
                            kount_zikl = 0;
                        }
                    }
                    else
                    {
                        //встает на нужную позицию во главе цепочки
                        if (addedBall.transform.position != point_add)
                        {
                            Vector3 pos1 = addedBall.transform.position;
                            addedBall.transform.position = new Vector3(pos1.x + (point_add.x - pos1.x) / kount_zikl,
                                pos1.y + (point_add.y - pos1.y) / kount_zikl, pos1.z + (point_add.z - pos1.z) / kount_zikl);
                            kount_zikl--;
                        }
                    }
                }
                // встраивается в цельную часть цепи
                else
                {
                    //встает на нужную позицию
                    if (addedBall.transform.position != point_add)
                    {
                        // шар встраивается в середину
                        if (!flagForward)
                        {
                            for (int ball = addedBallIndex - 1; ball >= rightGoBall; ball--)
                            {
                                balls_colors[ball].Item1.transform.position = prev_positions[go_to[ball] + 1];
                                go_to[ball]++;
                                Vector3 vect_new = prev_positions[go_to[ball] + 1];
                                balls_colors[ball].Item1.transform.LookAt(new Vector3(vect_new.x, vect_new.y, vect_new.z));
                            }

                            Vector3 pos1 = addedBall.transform.position;
                            addedBall.transform.position = new Vector3(pos1.x + (point_add.x - pos1.x) / kount_zikl,
                                pos1.y + (point_add.y - pos1.y) / kount_zikl, pos1.z + (point_add.z - pos1.z) / kount_zikl);
                            kount_zikl--;
                            GetLeftRightBalls();
                            CheckContinueGo(balls_colors[(int)rightGoBall].Item1.transform.position, (int)(rightGoBall));
                        }
                        //шар встраивается перед первым движущимся шариком
                        else
                        {
                            // добавляемая позиция перекрывает точку присоединения
                            // До присоединения, шар движется один
                            if (rightGoBall == null && go_to[addedBallIndex] + 245 >= piecesChain[piecesChain.Count - 1].Item3)
                            {
                                //встает на нужную позицию
                                if (addedBall.transform.position != point_add)
                                {
                                    Vector3 pos = addedBall.transform.position;
                                    addedBall.transform.position = new Vector3(pos.x + (point_add.x - pos.x) / kount_zikl,
                                        pos.y + (point_add.y - pos.y) / kount_zikl, pos.z + (point_add.z - pos.z) / kount_zikl);
                                    kount_zikl--;
                                    GetLeftRightBalls();
                                    CheckContinueGo(prev_positions[go_to[addedBallIndex] + (245 - kount_zikl)], addedBallIndex);                                    
                                }
                            }
                            // Добавляемая позиция не перекрывает точку присоединения
                            else if(rightGoBall == null)
                            {
                                //встает на нужную позицию
                                if (addedBall.transform.position != point_add)
                                {
                                    Vector3 pos = addedBall.transform.position;
                                    addedBall.transform.position = new Vector3(pos.x + (point_add.x - pos.x) / kount_zikl,
                                        pos.y + (point_add.y - pos.y) / kount_zikl, pos.z + (point_add.z - pos.z) / kount_zikl);
                                    kount_zikl--;
                                }
                            }
                            // Присоединение завершено
                            else
                            {
                                Vector3 pos = addedBall.transform.position;
                                addedBall.transform.position = new Vector3(pos.x + (point_add.x - pos.x) / kount_zikl,
                                    pos.y + (point_add.y - pos.y) / kount_zikl, pos.z + (point_add.z - pos.z) / kount_zikl);
                                kount_zikl--;

                                // собрана не единая цепь
                                if (rightGoBall != 0)
                                {
                                    for (int ball = addedBallIndex - 1; ball >= rightGoBall; ball--)
                                    {
                                        balls_colors[ball].Item1.transform.position = prev_positions[go_to[ball] + 1];
                                        go_to[ball]++;
                                        Vector3 vect_new = prev_positions[go_to[ball] + 1];
                                        balls_colors[ball].Item1.transform.LookAt(new Vector3(vect_new.x, vect_new.y, vect_new.z));
                                    }
                                    GetLeftRightBalls();
                                    CheckContinueGo(prev_positions[go_to[(int)rightGoBall] + (245 - kount_zikl)], addedBallIndex);
                                }
                                // цепь целая, без кусочков
                                else
                                {
                                    for (int ball = addedBallIndex - 1; ball > 0; ball--)
                                    {
                                        balls_colors[ball].Item1.transform.position = prev_positions[go_to[ball] + 1];
                                        go_to[ball]++;
                                        Vector3 vect_new = prev_positions[go_to[ball] + 1];
                                        balls_colors[ball].Item1.transform.LookAt(new Vector3(vect_new.x, vect_new.y, vect_new.z));
                                    }

                                    FirstBallGo();
                                }
                            }
                        }
                    }
                    // заглушка на вариант зависания
                    else
                    {
                        kount_zikl = 0;
                    }
                }
            }            
        }


        if (kount_zikl == 0)
        {
            string str = addedBall.name.Split('-')[1];
            int select = int.Parse(str[0].ToString()) - 1;
            balls_colors.Insert(addedBallIndex, (addedBall, select));
            addedBall.transform.position = point_add;
            if (ball_run)
            {
                addedBall.transform.GetComponent<ball_script>().is_go = true;
            }

            isAdded = false;
            if (indexFirstRunBall == null)
            {
                PereschetChain(addedBallIndex, 1, 0);
                go_to.Insert(0, prev_positions.IndexOf(balls_colors[1].Item1.transform.position) + rasstMBalls);
            }
            else
            {
                PereschetChain(addedBallIndex, 1, indexFirstRunBall);
                go_to.Insert(addedBallIndex, prev_positions.IndexOf(balls_colors[addedBallIndex].Item1.transform.position));
            }

            GetLeftRightBalls();
            //Проверка нужно ли присоединить цепочки
            CheckContinueGo(balls_colors[(int)rightGoBall].Item1.transform.position, (int)rightGoBall);
            BallsDeleter(addedBallIndex, select);

            

            for (int ball = balls_colors.Count - 1; ball >= rightGoBall; ball--)
            {
                balls_colors[ball].Item1.transform.GetComponent<ball_script>().is_go = true;
            }

            flagForward = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        try
        {
            if (flagWin)
            {
                winnerText.transform.GetComponent<Text>().enabled = true;
                StartCoroutine(Final());
            }

            else
            {
                Pointscount.GetComponent<Text>().text = game_points.ToString();
                // в данный момент ни один летящий шар не встраивается в цепочку, нет шаров в ожидании и нет магнетизма
                if (!isAdded && !isMagnet && !flag_wait_fly_ball)
                {
                    //отрисовка путевой линии
                    if (arrayPoints.Count > 1)
                    {
                        for (int i = 0; i < arrayPoints.Count - 1; i++)
                        {
                            Debug.DrawLine(arrayPoints[i].position,
                                arrayPoints[i + 1].position, Color.magenta);
                        }
                    }

                    //генерация цепочки шариков
                    if (ready_to_new_ball)
                    {
                        ready_to_new_ball = false;

                        int select = SelectColor("main");
                        string findName = "TypesBalls/TypeBall-" + (select + 1).ToString();
                        GameObject ballType = (GameObject)Resources.Load(findName);

                        GameObject createdBall = Instantiate(ballType, (Vector3)start_ball_pos, new Quaternion(0, 0, 0, 0), Ball_collector.transform);
                        go_to.Add(1);
                        balls_colors.Add((createdBall, select));
                        if (balls_colors.Count == 1)
                        {
                            createdBall.GetComponent<Rigidbody>().useGravity = false;
                            createdBall.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
                        }

                        prev_ball = balls_colors[balls_colors.Count - 1].Item1;
                    }

                    BallsGoStandart();
                }
                // есть ожидающий шар
                else if (flag_wait_fly_ball)
                {
                    AddFlyBall(wait_fly_added_ball[0], wait_ball_Touch[0], wait_napr[0]);
                    if (wait_fly_added_ball.Count == 1)
                    {
                        flag_wait_fly_ball = false;
                    }
                    wait_fly_added_ball.RemoveAt(0);
                    wait_ball_Touch.RemoveAt(0);
                    wait_napr.RemoveAt(0);
                }
                // в цепочку встраивается летящий шар
                else if (isAdded && !isDeleted)
                {
                    for (int i = 0; i < speed_add_ball; i++)
                    {
                        if (isAdded)
                        {
                            BallsGo_VstroenieFlyBall();
                        }
                    }
                }
                // магнетизм
                else if (isMagnet)
                {
                    if (isMagnet)
                    {
                        MagnetismBallsColors();
                    }
                }



                for (int number = 0; number < fly_balls.Count; number++)
                {
                    Transform ball = fly_balls[number].Item2.transform;
                    Vector3 eye = fly_balls[number].Item1;
                    fly_balls[number].Item2.transform.position = new Vector3(ball.position.x + eye.x, ball.position.y + eye.y, ball.position.z + eye.z);
                }

                if (go_to[balls_colors.Count - 1] >= 246)
                {
                    ready_to_new_ball = true;
                }
            }

            if (game_points >= points_need_to_win)
            {
                flagWin = true;
            }
        }

        catch (Exception e)
        {
            int i = 0;
        }
    }


    IEnumerator Final()
    {
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene(2);
    }

    public void ShotBall(Vector3 start_position, GameObject newBall, int select)
    {
        float koef_dvig = speed_fly / (Mathf.Abs(newBall.transform.forward.x) + Mathf.Abs(newBall.transform.forward.y) + Mathf.Abs(newBall.transform.forward.z));
        fly_balls.Add((newBall.transform.forward * koef_dvig, newBall, koef_dvig, select));
        newBall.transform.parent = Fly_collector.transform;
    }

    public void DeleteFlyBall(Collider collider)
    {
        for (int num = 0; num < fly_balls.Count; num++) {
            if(fly_balls[num].Item2 == collider.gameObject)
            {
                isDeleted = false;
                fly_balls.RemoveAt(num);
                Destroy(collider.gameObject);
                break;
            }
        }            
    }

    public void AddFlyBall(GameObject vstr_ball, GameObject ball_Touch, string napr)
    {
        // если прошлый шар еще не успел встроиться
        if (isAdded)
        {
            flag_wait_fly_ball = true;
            wait_fly_added_ball.Add(vstr_ball);
            wait_ball_Touch.Add(ball_Touch);
            wait_napr.Add(napr);
        }
        else
        {
            kombo = 0;
            isAdded = true;

            int ballPrivazInd = -1;
            addedBallIndex = -1;
            // найти индекс шара привязки
            for (int num = 0; num < balls_colors.Count; num++)
            {
                if (ball_Touch == balls_colors[num].Item1)
                {
                    ballPrivazInd = num;
                }
            }


            indexFirstRunBall = 0;
            if (napr == "Back")
            {
                addedNapr = "Back";
                addedBallIndex = ballPrivazInd + 1;
            }
            else
            {
                addedNapr = "Forward";
                addedBallIndex = ballPrivazInd;
            }
            if (addedBallIndex < 0)
            {
                addedBallIndex = 0;
            }
            if (addedBallIndex != 0)
            {
                indexFirstRunBall = addedBallIndex - 1;
            }


            if (addedBallIndex < 2)
            {
                // если точки нужно просчитать
                if (prev_positions.Count < prev_positions.IndexOf(balls_colors[(int)indexFirstRunBall].Item1.transform.position) + 245)
                {
                    int kount_zikl = 245;
                    Vector3 new_pos = new Vector3(balls_colors[(int)indexFirstRunBall].Item1.transform.position.x, balls_colors[(int)indexFirstRunBall].Item1.transform.position.y,
                        balls_colors[(int)indexFirstRunBall].Item1.transform.position.z);
                    while (kount_zikl > 0)
                    {
                        List<(float, float, float, bool)> position_new = NewPosition(new_pos.x, new_pos.y, new_pos.z,
                            points[number_point_first_ball], points[number_point_first_ball + 1], speed);

                        prev_positions.Add(new Vector3(position_new[0].Item1, position_new[0].Item2, position_new[0].Item3));
                        new_pos = new Vector3(position_new[0].Item1, position_new[0].Item2, position_new[0].Item3);
                        if (position_new[0].Item4)
                        {
                            number_point_first_ball++;
                        }
                        kount_zikl--;
                    }
                }

                // добавление шара на первую позицию
                if (addedBallIndex == 0)
                {
                    kount_zikl = 245;
                    // позиция вставки добавляемого шара
                    point_add = prev_positions[prev_positions.IndexOf(balls_colors[(int)indexFirstRunBall].Item1.transform.position) + 245];
                    // необходимая позиция для продвижения шара к которому идет прикрепление и всех дальнейших шаров соответственно
                    need_point_index = null;
                    indexFirstRunBall = null;
                }
                // добавление шара на вторую позицию
                else
                {
                    kount_zikl = 245;
                    // позиция вставки добавляемого шара
                    point_add = prev_positions[prev_positions.IndexOf(balls_colors[(int)indexFirstRunBall].Item1.transform.position)];
                    // необходимая позиция для продвижения шара к которому идет прикрепление и всех дальнейших шаров соответственно
                    need_point_index = prev_positions[prev_positions.IndexOf(balls_colors[(int)indexFirstRunBall].Item1.transform.position) + 245];
                    indexFirstRunBall = 0;
                }
            }
            else
            {
                if (addedNapr == "Forward" && piecesChain.Count > 0)
                {
                    // позиция вставки добавляемого шара
                    point_add = prev_positions[prev_positions.IndexOf(balls_colors[ballPrivazInd].Item1.transform.position) + rasstMBalls];
                    // необходимая позиция для продвижения шара к которому идет прикрепление и всех дальнейших шаров соответственно
                    need_point_index = balls_colors[addedBallIndex - 2].Item1.transform.position;
                    indexFirstRunBall = addedBallIndex - 1;
                }
                else
                {
                    // позиция вставки добавляемого шара
                    point_add = balls_colors[addedBallIndex - 1].Item1.transform.position;
                    // необходимая позиция для продвижения шара к которому идет прикрепление и всех дальнейших шаров соответственно
                    need_point_index = balls_colors[addedBallIndex - 2].Item1.transform.position;
                    indexFirstRunBall = addedBallIndex - 1;
                }
                kount_zikl = 245;
            }

            for (int fly_num = 0; fly_num < fly_balls.Count; fly_num++)
            {
                if (fly_balls[fly_num].Item2 == vstr_ball)
                {
                    fly_balls.RemoveAt(fly_num);
                    addedBall = vstr_ball;
                    break;
                }
            }
        }
    }
}

public class Point {
    public float x;
    public float y;
    public float z;

    public Point(float x, float y, float z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}