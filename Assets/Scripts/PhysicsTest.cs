using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsTest : MonoBehaviour
{
    private Rigidbody rigidbody;
    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
    }
    
    private void FixedUpdate()
    {
        if(Input.GetKeyDown(KeyCode.A))
        {
            rigidbody.AddForce(-200,0,0,ForceMode.VelocityChange);
            //rigidbody.angularVelocity = new Vector3(0, 50,0);
            //rigidbody.velocity = new Vector3(-10,0 ,0);
        }
    }
}
