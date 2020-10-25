using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    private Rigidbody rigidbody;
    private AudioSource audioSource;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        ProcessInput();
    }

    private void ProcessInput()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            // thrust rocket 
            rigidbody.AddRelativeForce(Vector3.up);
            // play thrust sfx
            if (!audioSource.isPlaying) { audioSource.Play(); }
        }
        else if (audioSource.isPlaying) { audioSource.Stop(); }

        if (Input.GetKey(KeyCode.A))
        {
            // rotate left 
            transform.Rotate(Vector3.forward);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            // rotate right 
            transform.Rotate(-Vector3.forward);
        }
    }
}
