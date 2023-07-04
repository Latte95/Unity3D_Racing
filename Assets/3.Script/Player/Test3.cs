using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Test3 : MonoBehaviour
{
    public WheelCollider w1;
    public WheelCollider w2;
    public WheelCollider w3;
    public WheelCollider w4;

    PlayerInput input;

    private void Start()
    {
        TryGetComponent(out input);
    }

    private void FixedUpdate()
    {
        w3.motorTorque = 400;
        w4.motorTorque = 400;

        w1.steerAngle = 45 * input.move.x;
        w2.steerAngle = 45 * input.move.x;
    }
}
