using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ball_script : MonoBehaviour
{
    public bool is_go = true;
    public bool is_magneted = false;
    int zikl_povorot = 0;
    int max_zikl = 10;
    Quaternion need_povorot;

    // Start is called before the first frame update
    void Start()
    {       
    }

    //// Update is called once per frame
    void Update()
    {
        if (is_go)
        {
            transform.GetChild(0).Rotate(new Vector3((float)0.5, 0, 0), Space.Self);
        }
        else if (is_magneted)
        {
            transform.GetChild(0).Rotate(new Vector3((float)-0.5, 0, 0), Space.Self);
        }

        if(zikl_povorot <= max_zikl)
        {
            double mnoz = (double)((double)zikl_povorot / (double)max_zikl);
            transform.rotation = new Quaternion((float)(transform.rotation.x + need_povorot.x * mnoz), (float)(transform.rotation.y + need_povorot.y * mnoz), 
                (float)(transform.rotation.z + need_povorot.z * mnoz), (float)(transform.rotation.w + need_povorot.w * mnoz));
            zikl_povorot++;
        }
    }    

    public void Povorot(Vector3 need_pov)
    {
        zikl_povorot = 0;
        Quaternion temp = transform.rotation;
        transform.LookAt(need_pov);
        need_povorot = transform.rotation;
        transform.rotation = temp;
    }
}
