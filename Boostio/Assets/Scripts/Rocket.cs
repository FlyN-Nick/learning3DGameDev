using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    Rigidbody rigidbody;

    void Start() { rigidbody = GetComponent<Rigidbody>(); }

    void Update()
    {
        ProcessInput();
    }

    private void ProcessInput()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            //print("Thrusting");
            rigidbody.AddRelativeForce(Vector3.up);
        }

        if (Input.GetKey(KeyCode.A))
        {
            //print("Left");
            // TODO: make rocket rotate left 
        }
        else if (Input.GetKey(KeyCode.D))
        {
            //print("Right");
            // TODO: make rocket rotate right 
        }
    }
}
