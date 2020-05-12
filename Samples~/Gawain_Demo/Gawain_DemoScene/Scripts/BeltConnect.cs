using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class BeltConnect : MonoBehaviour
{
    private CharacterJoint cj;
    private Rigidbody rb;
    private Rigidbody prb;


    // Start is called before the first frame update
    void Start()
    {
        rb.isKinematic = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Awake()
    {
        cj = (CharacterJoint)GetComponent(typeof(CharacterJoint));
        prb = (Rigidbody)transform.parent.GetComponent(typeof(Rigidbody));
        rb = (Rigidbody)transform.GetComponent(typeof(Rigidbody));
        cj.connectedBody = prb;
    }
}
