using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class AxleInfo
{
    public WheelCollider[] leftWheel = new WheelCollider[2];
    public WheelCollider[] rightWheel = new WheelCollider[2];
    public bool motor;
    public bool steering;
}

public class PlayerControl : MonoBehaviour
{
    [Header("Tires")]
    private GameObject[] wheels_Mesh;
    [SerializeField]
    private WheelCollider[] wheels_Col;
    private WheelFrictionCurve nomalFriction_f;
    private WheelFrictionCurve driftFriction_f;
    private WheelFrictionCurve nomalFriction_s;
    private WheelFrictionCurve driftFriction_s;
    public List<AxleInfo> axleInfos;
    private GameObject[] wheels;
    private float vehicleWidth;
    private float wheelBase;
    private float driftFriction = 0.75f;

    [Header("Player")]
    private float targetSpeed = 0;
    [SerializeField]
    private float speed = 400.0f;
    [SerializeField]
    private float maxSpeed = 10f;
    [SerializeField]
    private float boostSpeed = 1.3f;
    [SerializeField]
    private float steerRotate = 40.0f;

    [Header("State")]
    private PlayerState currentStarategy = new NormalMoveState();
    private bool isBoost = false;

    [Header("Components")]
    private Animator anim;
    private Rigidbody rigid;

    public PlayerInput input;

    private void Awake()
    {
        wheels_Mesh = GameObject.FindGameObjectsWithTag("WheelMesh");
        wheels = GameObject.FindGameObjectsWithTag("Wheel");
        TryGetComponent(out anim);
        TryGetComponent(out rigid);
        TryGetComponent(out input);
        rigid.centerOfMass = 0.3f * Vector3.down;
    }

    private void Start()
    {
        for (int i = 0; i < wheels_Mesh.Length; i++)
        {
            wheels[i].transform.position = wheels_Mesh[i].transform.position;
        }
        vehicleWidth = Mathf.Abs(wheels[0].transform.position.magnitude - wheels[1].transform.position.magnitude);
        wheelBase = Mathf.Abs(wheels[0].transform.position.magnitude - wheels[2].transform.position.magnitude);
        nomalFriction_f = wheels_Col[0].forwardFriction;
        driftFriction_f = wheels_Col[0].forwardFriction;
        driftFriction_f.stiffness = driftFriction;
        nomalFriction_s = wheels_Col[0].sidewaysFriction;
        driftFriction_s = wheels_Col[0].sidewaysFriction;
        driftFriction_s.stiffness = driftFriction;
    }

    private void FixedUpdate()
    {
        Move();
        Drift();
        WheelPos();
        AddDownForce();
    }

    #region 이동
    public float radius = 6000000000f;
    private void Move()
    {
        if (Mathf.Abs(targetSpeed) < maxSpeed)
        {
            targetSpeed += speed * Time.deltaTime * input.move.y;
        }
        if (input.move.Equals(Vector2.zero))
        {
            targetSpeed = 0;
        }
        else if (isBoost)
        {
            targetSpeed *= boostSpeed;
        }

        float targetRot = steerRotate;
        float steering = targetRot * input.move.x;

        foreach (AxleInfo axleInfo in axleInfos)
        {
            // 바퀴 회전
            if (axleInfo.steering)
            {
                float adjustment = vehicleWidth * 0.5f;

                for (int i = 0; i < 2; i++)
                {
                    axleInfo.leftWheel[i].steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (radius + adjustment * input.move.x)) * steering;
                    axleInfo.rightWheel[i].steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (radius - adjustment * input.move.x)) * steering;
                }
            }
            // 바퀴 굴림
            if (axleInfo.motor)
            {
                axleInfo.leftWheel[0].motorTorque = targetSpeed;
                axleInfo.leftWheel[1].motorTorque = targetSpeed;
                axleInfo.rightWheel[0].motorTorque = targetSpeed;
                axleInfo.rightWheel[1].motorTorque = targetSpeed;
            }
        }
    }
    /// <summary>
    /// 바퀴 이미지를 휠 콜라이더 위치랑 동기화 시키는 메소드
    /// </summary>
    private void WheelPos()
    {
        Vector3 wheelPosition;
        Quaternion wheelRotation;

        for (int i = 0; i < 4; i++)
        {
            Vector3 temp1, temp2;
            wheels_Col[i * 2].GetWorldPose(out temp1, out wheelRotation);
            wheels_Col[i * 2 + 1].GetWorldPose(out temp2, out wheelRotation);
            wheelPosition = (temp1 + temp2) * 0.5f;
            wheels_Mesh[i].transform.position = wheelPosition;
            wheels_Mesh[i].transform.rotation = wheelRotation;
        }
    }
    private void Drift()
    {
        for (int i = 0; i < 2; i++)
        {
            if (input.drift)
            {
                axleInfos[1].leftWheel[i].forwardFriction = driftFriction_f;
                axleInfos[1].leftWheel[i].sidewaysFriction = driftFriction_s;
                axleInfos[1].rightWheel[i].forwardFriction = driftFriction_f;
                axleInfos[1].rightWheel[i].sidewaysFriction = driftFriction_s;
            }
            else
            {
                axleInfos[1].leftWheel[i].forwardFriction = nomalFriction_f;
                axleInfos[1].leftWheel[i].sidewaysFriction = nomalFriction_s;
                axleInfos[1].rightWheel[i].forwardFriction = nomalFriction_f;
                axleInfos[1].rightWheel[i].sidewaysFriction = nomalFriction_s;
            }
        }
    }
    #endregion 이동

    #region 차량 안정화
    public float downForce = 100.0f;

    private void AddDownForce()
    {
        rigid.AddForce(-transform.up * downForce * rigid.velocity.magnitude);
    }
    #endregion
}