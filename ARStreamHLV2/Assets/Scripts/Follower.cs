using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Composites;

public class Follower : MonoBehaviour
{
    public Transform target;
    public float speed = 0.1f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float dt = Time.deltaTime*speed;
        if(dt > 1) { dt = 1; }

        transform.position = Vector3.Lerp(transform.position, target.position, dt);
        transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, dt);
    }
}
