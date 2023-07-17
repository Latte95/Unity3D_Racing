using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterTurn : MonoBehaviour
{
    public float rotationSpeed = 20f;

    private void Update()
    {
        transform.Rotate(rotationSpeed * Vector3.up * Time.deltaTime);
    }
}
