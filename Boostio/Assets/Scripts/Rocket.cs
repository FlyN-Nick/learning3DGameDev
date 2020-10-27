//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class Rocket : MonoBehaviour
{
    [SerializeField] private float rcsThrust = 100f;
    [SerializeField] private float mainThrust = 100f;

    [SerializeField] private AudioClip engineThrustSFX;
    [SerializeField] private AudioClip deathSFX;
    [SerializeField] private AudioClip successSFX;

    [SerializeField] private ParticleSystem engineVFX;
    [SerializeField] private ParticleSystem deathVFX;
    [SerializeField] private ParticleSystem successVFX;

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
    
    private void Thrust()
    {
        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.UpArrow))
        {
            // thrust rocket 
            rigidBody.AddRelativeForce(Vector3.up * mainThrust * Time.deltaTime);
            // play thrust sfx
            if (!audioSource.isPlaying) { audioSource.PlayOneShot(engineThrustSFX); }
            // play thrust vfx
            if (!engineVFX.isPlaying) { engineVFX.Play(); }
        }
        else if (audioSource.isPlaying) { audioSource.Stop(); engineVFX.Stop(); }
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
                state = State.transcending;
                //if (audioSource.isPlaying) { audioSource.Stop(); }
                audioSource.PlayOneShot(successSFX);
                successVFX.Play();
                Invoke("LoadNextLevel", 1f);
                break;
            default:
                // TODO: first level or level screen?
                state = State.dead;
                //if (audioSource.isPlaying) { audioSource.Stop(); }
                audioSource.PlayOneShot(deathSFX);
                deathVFX.Play();
                Invoke("LoadFirstLevel", 1f);
                break;
        }
    }

    void LoadNextLevel()
    {
        SceneManager.LoadScene(1); // TODO: make this work for more than 2 levels
    }

    void LoadFirstLevel() { SceneManager.LoadScene(0); }
}
