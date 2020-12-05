using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] [Range(-1, 1)] float xRot = 0f;
    [SerializeField] [Range(-1, 1)] float yRot = 0f;
    [SerializeField] [Range(-1, 1)] float zRot = 1f;

    [SerializeField] [Min(0)] int degPerSec = 180;

    void Update()
    {
        transform.RotateAround(transform.position, new Vector3(xRot, yRot, zRot), degPerSec * Time.deltaTime);
    }
}
