using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class TestMove : MonoBehaviour
{
    [SerializeField] Vector3 PerFrameMove = new Vector3(.05f, 0f, .05f);

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position += PerFrameMove;
    }
}
