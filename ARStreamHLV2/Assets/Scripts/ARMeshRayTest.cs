using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARMeshRayTest : MonoBehaviour
{
    public GameObject vis;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float rayLength = 100f;
        int layerMask = 1 << 31;

        if (Physics.SphereCast(transform.position, 0.1f, transform.forward, out RaycastHit hitInfo, rayLength, layerMask))
        {
            vis.transform.position = hitInfo.point + (hitInfo.normal*0.05f);
        }
    }
}
