//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Oscillator : MonoBehaviour
{
    [SerializeField] Vector3 movementVector;

    // TODO: remove
    [SerializeField] [Range (0, 1)] float movementFactor; // 0 not moved, 1 fully moved

    private Vector3 startingPos;

    void Start() { startingPos = transform.position; }

    void Update()
    {
        Vector3 offset = movementVector*movementFactor;
        transform.position = startingPos + offset;
    }
}
