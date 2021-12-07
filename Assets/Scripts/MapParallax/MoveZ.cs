using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveZ : MonoBehaviour
{
    public float speed = 1.0f;
    void Start()
    {
        
    }

    
    void Update()
    {  
        this.transform.position = new Vector3(0, 0, this.transform.position.z - (Time.deltaTime * speed));
        if (transform.position.z <= -250)
        {
            transform.position = new Vector3(0, 0, 500);
        }
    }

    // private void OnCollisionEnter(Collision other) {
    //     if (other.transform.tag == "Player")
    //     {
    //         Debug.Log("Collision");
    //     }
    // }

    private void OnTriggerEnter(Collider other) {
        if (other.transform.tag == "Player")
        {
            Debug.Log("OnTriggerEnter");
        }
    }
}
