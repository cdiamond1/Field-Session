using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class BoundingBoxManager : MonoBehaviour
{
    public Transform boxesParent;
    public TextMeshProUGUI tmp;

    private GameObject boxPrefab;
    private Camera cam;
    private Vector2 start;
    private Vector2 end;
    private int layerMask;

    public string example1;
    public string example2;
    public string example3;

    private List<string> queue = new List<string>();

    private int step = 0;
    private List<string> steps = new List<string>();



    // Start is called before the first frame update
    void Start()
    {
        boxPrefab = Resources.Load("Prefabs/BoundingBox") as GameObject;
        cam = Camera.main;
        layerMask = 1 << 31;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            exampleBox("aaa");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            exampleBox("bbb");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            exampleBox("ccc");
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            parseJson(example1);

        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            parseJson(example2);

        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            parseJson(example3);

        }

        foreach(string data in queue)
        {
            parseJson(data);
        }
        queue.Clear();
    }

    public void exampleBox(string label)
    {
        float minv = 0;
        float maxv = 1024;
        float midv = (minv + maxv)/2;
        start = new Vector2(UnityEngine.Random.Range(minv, midv), UnityEngine.Random.Range(minv, midv));
        end = new Vector2(UnityEngine.Random.Range(midv, maxv), UnityEngine.Random.Range(midv, maxv));
        applyBoundingBox(start, end, label, RandomHue());
    }

    public void addToJsonQueue(string data)
    {
        queue.Add(data);
    }

    public void addToSteps(string data)
    {
        steps.Add(data);
    }

    //advances, retreats, or updates step display
    public void stepChange(int delta)
    {
        step += delta;
        int endidx = steps.Count - 1;

        if (step < 0)
        {
            step = 0;
        }

        if (step > endidx)
        {
            step = endidx;
        }

        //only display if there is atleast 1 step
        if(endidx > 0)
        {
            string stepcounter = (step + 1) + "/" + (endidx + 1) + "\n";
            tmp.text = stepcounter + steps[step];
        }
    }

    //parses a json string to the various output types
    public void parseJson(string data)
    {
        //Logger.Log($"[remote] {data}\n");
        //convert input json to an object
        LabelData parsedData = LabelData.CreateFromJSON(data);

        //parse html color code to color
        Color parsedColor = Color.white;
        bool colorWasParsed = UnityEngine.ColorUtility.TryParseHtmlString(parsedData.color, out parsedColor);
        if (!colorWasParsed) { Logger.Log("Color " + parsedData.color + " is invalid."); }

        string type = parsedData.type;
        switch (type)
        {
            case "box":
                // Apply to bounding box system
                applyBoundingBox(new Vector2(parsedData.x1, parsedData.y1), new Vector2(parsedData.x2, parsedData.y2), parsedData.label, parsedColor);
                break;

            case "todo":
                // Apply to todo list system
                applyTodo(parsedData.label, parsedColor);
                break;

            default:
                // Send to the console
                Logger.Log(parsedData.label);
                break;
        }
    }

    //create or update an existing bounding box to support the arguments
    public void applyBoundingBox(Vector2 start, Vector2 end, string label, Color color)
    {
        float camw = 1504;
        float camh = 846;
        float screenscale = 1.725f;
        float resw = cam.pixelWidth;
        float resh = cam.pixelHeight;

        //remap to align the position to real world
        //remap pixel on image from -0.5 to 0.5
        start = new Vector2(map(start.x, 0, camw, -0.5f, 0.5f), map(start.y, 0, camh, -0.5f, 0.5f));
        end = new Vector2(map(end.x, 0, camw, -0.5f, 0.5f), map(end.y, 0, camh, -0.5f, 0.5f));

        //flip vertically since unity y is down and image y is up
        start.y = -start.y;
        end.y = -end.y;

        //multiply by scale to make objects align
        start *= screenscale;
        end *= screenscale;

        //remap to resolution of screen
        start = new Vector2(map(start.x, -0.5f, 0.5f, 0, resw), map(start.y, -0.5f, 0.5f, 0, resh));
        end = new Vector2(map(end.x, -0.5f, 0.5f, 0, resw), map(end.y, -0.5f, 0.5f, 0, resh));

        //add px offset to accound for camera being angled up
        float voffset = 100;
        start.y -= voffset;
        end.y -= voffset;

        GameObject newBoxObj = null;
        bool boxExists = false;

        //either find the existing box for this label, or create a new box
        foreach (Transform box in boxesParent)
        {
            if(box.gameObject.GetComponent<BoundingBox>().getLabel() == label)
            {
                boxExists = true;
                newBoxObj = box.gameObject;
                break;
            }
        }

        //create box if it wasn't found
        if (boxExists == false)
        {
            newBoxObj = Instantiate(boxPrefab, boxesParent);
        }

        BoundingBox newBox = newBoxObj.GetComponent<BoundingBox>();

        Vector2 center = new Vector2((end.x + start.x) / 2, (end.y + start.y) / 2);

        Ray startRay = cam.ScreenPointToRay(start);
        Ray endRay = cam.ScreenPointToRay(end);
        Ray centerRay = cam.ScreenPointToRay(center);

        float dist = 2.0f;

        if (Physics.SphereCast(centerRay.origin, 0.1f, centerRay.direction, out RaycastHit hitInfo, 10.0f, layerMask))
        {
            dist = hitInfo.distance-0.1f;
        }

        //calculate box endpoints
        Vector3 startPos = startRay.origin + startRay.direction * dist;
        Vector3 endPos = endRay.origin + endRay.direction * dist;
        Vector3 centerPos = centerRay.origin + centerRay.direction * dist;

        //calculate box size
        Vector3 startPosLocal = cam.transform.InverseTransformPoint(startPos);
        Vector3 endPosLocal = cam.transform.InverseTransformPoint(endPos);
        float w = endPosLocal.x - startPosLocal.x;
        float h = endPosLocal.y - startPosLocal.y;

        newBox.setUp(centerPos, cam.transform.rotation, new Vector3(w, h, 1.0f), color, label, boxExists==false);
    }

    //add the item to the todo list
    public void applyTodo(string label, Color color)
    {
        string newlabel = "\n<color=#" + color.ToHexString() + ">" + label;
        
        addToSteps(newlabel);
        stepChange(0);
    }


    private void OnDrawGizmos()
    {
        if (cam)
        {
            Vector2 center = new Vector2((end.x + start.x) / 2, (end.y + start.y) / 2);
            Ray centerRay = cam.ScreenPointToRay(center);

            Gizmos.DrawRay(centerRay.origin, centerRay.direction);
        }
    }

    Color RandomHue()
    {
        return Color.HSVToRGB(UnityEngine.Random.Range(0f, 1f), 1.0f, 1.0f);
    }

    float map(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }
}

[System.Serializable]
public class LabelData
{
    public float x1;
    public float y1;
    public float x2;
    public float y2;

    public string label;
    public string color;

    public string type;

    public static LabelData CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<LabelData>(jsonString);
    }
}
