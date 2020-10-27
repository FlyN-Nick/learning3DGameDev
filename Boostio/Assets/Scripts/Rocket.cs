//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour
{
    [SerializeField] private float rcsThrust = 100f;
    [SerializeField] private float mainThrust = 100f;

    private Rigidbody rigidBody;
    private AudioSource audioSource;

    private enum State { alive, dead, transcending }
    State state = State.alive;

    private void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (state == State.alive)
        {
            Thrust();
            Rotate();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (state != State.alive) { return; }

        switch (collision.gameObject.tag)
        {
            case "Friendly":
                // do nothing
                // TODO: switch to if else?
                break;
            case "Fuel":
                // TODO: refuel
                break;
            case "Finish":
                // TODO: next level or level screen?
                // TODO: play next level sound?
                state = State.transcending;
                Invoke("LoadNextLevel", 1f);
                break;
            default:
                // TODO: first level or level screen?
                // TODO: stop sound on death?
                // TODO: play dying sound?
                state = State.dead;
                Invoke("LoadFirstLevel", 1f);
                break;
        }
    }

    void LoadNextLevel()
    {
        SceneManager.LoadScene(1); // TODO: make this work for more than 2 levels
    }

    void LoadFirstLevel() { SceneManager.LoadScene(0);  }
    
    private void Thrust()
    {
        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.UpArrow))
        {
            // thrust rocket 
            rigidBody.AddRelativeForce(Vector3.up * mainThrust);
            // play thrust sfx
            if (!audioSource.isPlaying) { audioSource.Play(); }
        }
        else if (audioSource.isPlaying) { audioSource.Stop(); }
    }

    private void Rotate()
    {
        rigidBody.freezeRotation = true; // take manual control of rotation

        float rotSpeed = rcsThrust * Time.deltaTime;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            // rotate left
            transform.Rotate(Vector3.forward*rotSpeed);
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            // rotate right 
            transform.Rotate(Vector3.back*rotSpeed);
        }

        rigidBody.freezeRotation = false; // resume physics control of rotation 
    }
}
