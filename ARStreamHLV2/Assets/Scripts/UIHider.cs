using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHider : MonoBehaviour
{
    private Transform cam;
    private Canvas canvas;
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main.transform;
        canvas = GetComponent<Canvas>();
    }

    // Update is called once per frame
    void Update()
    {
        float dot = Vector3.Dot(cam.forward, transform.forward);
        //Debug.Log(dot);
        if(dot < 0)
        {
            if(canvas.enabled == true) { canvas.enabled = false; }
            
        }
        else
        {
            if (canvas.enabled == false) { canvas.enabled = true; }
        }
    }
}
