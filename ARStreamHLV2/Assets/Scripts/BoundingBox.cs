using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

public class BoundingBox : MonoBehaviour
{
    private Transform cam;
    private float timeout;
    private Vector3 targetPos;
    private Quaternion targetRot;
    private Vector3 targetScale;
    private string label;

    public Transform boxTF;
    public Transform labelTF;
    public TextMeshPro tmp;
    
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main.transform;
        timeout = 10.0f;


    }

    // Update is called once per frame
    void Update()
    {
        float dt = Time.deltaTime;


        //animate the position rotation and scale of the box to the target
        transform.position = Vector3.Lerp(transform.position, targetPos, dt * 2.0f);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, dt * 2.0f);
        boxTF.localScale = Vector3.Lerp(boxTF.localScale, targetScale, dt * 2.0f);

        //set label position
        Vector3 ls = boxTF.localScale.Abs()*0.5f;
        labelTF.localPosition = new Vector3(-ls.x,ls.y,0);

        //update timeout to remove boxes that may not be valid anymore
        timeout -= dt;
        if (timeout < 0)
        {
            Destroy(gameObject);
        }
    }

    //set values and maybe perform first time setup
    public void setUp(Vector3 position, Quaternion rotation, Vector3 scale, Color color, string label, bool doFirstTimeSetup)
    {
        targetPos = position;
        targetRot = rotation;
        targetScale = scale;
        timeout = 10.0f;

        //perform first time setup
        if(doFirstTimeSetup == true)
        {
            //set variables
            boxTF = transform.Find("Box");
            labelTF = transform.Find("Label");
            tmp = labelTF.GetComponent<TextMeshPro>();

            //snap to targets
            transform.position = position;
            transform.rotation = rotation;
            boxTF.localScale = scale;
            //set visuals
            setColor(color);
            setLabel(label);
        }
    }

    public void setColor(Color color)
    {
        boxTF.GetComponent<Renderer>().material.SetColor("_BaseColor", color);
        tmp.color = color;
    }

    public void setLabel(string label)
    {
        this.label = label;
        tmp.text = label;
    }

    public string getLabel()
    {
        return label;
    }




}
