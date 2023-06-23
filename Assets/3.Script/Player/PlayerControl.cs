using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerControl : MonoBehaviour
{
    [Header("Tires")]
    [SerializeField]
    private GameObject[] wheels;
    [SerializeField]
    private WheelCollider[] wheels_Col;

    private PlayerState currentStarategy = new NormalMoveState();

    [Header("Components")]
    private Animator anim;
    private Rigidbody rigid;

    private void Awake()
    {
        wheels = GameObject.FindGameObjectsWithTag("Wheel");
        TryGetComponent(out anim);
        TryGetComponent(out rigid);
        rigid.centerOfMass = Vector3.zero + 0.3f * Vector3.down;
    }

    private void Start()
    {
        for (int i = 0; i < wheels.Length; i++)
        {
            wheels_Col[i].transform.position = wheels[i].transform.position;
        }
    }

    private void Update()
    {

    }

    private void FixedUpdate()
    {
        WheelPosAndAni();
    }

    void WheelPosAndAni()
    {
        Vector3 wheelPosition = Vector3.zero;
        Quaternion wheelRotation = Quaternion.identity;

        for (int i = 0; i < 4; i++)
        {
            wheels_Col[i].GetWorldPose(out wheelPosition, out wheelRotation);
            wheels[i].transform.position = wheelPosition;
            wheels[i].transform.rotation = wheelRotation;
        }
    }
}