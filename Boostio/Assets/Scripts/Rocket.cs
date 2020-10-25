using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    [SerializeField] private float rcsThrust = 100f;
    [SerializeField] private float mainThrust = 100f;

    private Rigidbody rigidBody;
    private AudioSource audioSource;

    private void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        Thrust();
        Rotate();
    }

    private void OnCollisionEnter(Collision collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Friendly":
                // do nothing
                print("Chill."); // TODO: remove
                break;
            case "Fuel":
                // TODO: refuel
                print("Fuel."); // TODO: remove
                break;
            default:
                // TODO: you died
                print("F."); // TODO: remove
                break;
        }
    }

    private void Thrust()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            // thrust rocket 
            rigidBody.AddRelativeForce(Vector3.up*mainThrust);
            // play thrust sfx
            if (!audioSource.isPlaying) { audioSource.Play(); }
        }
        else if (audioSource.isPlaying) { audioSource.Stop(); }
    }

    private void Rotate()
    {
        rigidBody.freezeRotation = true; // take manual control of rotation

        float rotSpeed = rcsThrust * Time.deltaTime;
        if (Input.GetKey(KeyCode.A))
        {
            // rotate left
            transform.Rotate(Vector3.forward*rotSpeed);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            // rotate right 
            transform.Rotate(Vector3.back*rotSpeed);
        }

        rigidBody.freezeRotation = false; // resume physics control of rotation 
    }
}
