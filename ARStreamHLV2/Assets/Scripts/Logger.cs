using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Logger : MonoBehaviour
{
    public static Logger Instance { get; private set; }
    public TextMeshProUGUI tmp;
    public List<string> queue;
    // Start is called before the first frame update
    void Start()
    {
        Logger.Instance = this;
        //tmp = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        foreach(string message in queue)
        {
            DoLog(message);
        }
        queue.Clear();
    }

    public static void Log(string output)
    {
        Logger.Instance.queue.Add(output);
    }

    void DoLog(string output)
    {
        //send text to textmeshpro
        Debug.Log(output);
        tmp.text = tmp.text + "\n" + output;
        //tmp.text = output;
    }
}
