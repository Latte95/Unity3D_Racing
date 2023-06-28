using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControl : MonoBehaviour
{
    [Header("Player")]
    public Kart kart;
    private float targetSpeed = 0;

    [Header("State")]
    private PlayerState currentStarategy = new NormalMoveState();
    public bool isBoost { get; private set; }

    [Header("Components")]
    private Animator anim;
    private Rigidbody rigid;

    public PlayerInput input;

    private void Awake()
    {
        TryGetComponent(out anim);
        TryGetComponent(out rigid);
        TryGetComponent(out input);
        kart = transform.GetChild(0).GetComponentInChildren<Kart>();
    }

    private void Start()
    {
        // 안정성 위해 무게중심 밑으로 설정
        rigid.centerOfMass = 0.3f * Vector3.down;
    }

    private void FixedUpdate()
    {
        Move();
        //Drift();
        WheelPos();
        AddDownForce();
    }

    #region 이동
    public float radius = 6;
    private void Move()
    {
        // 속도 계산
        targetSpeed = kart.speed * input.move.y;
        if (input.move.Equals(Vector2.zero))
        {
            targetSpeed = 0;
        }
        else if (isBoost)
        {
            targetSpeed *= kart.boostSpeed;
        }


        float steering = kart.steerRotate * input.move.x;
        float halfVehicleWidth = kart.vehicleWidth * 0.5f;

        foreach (AxleInfo axleInfo in kart.axleInfos)
        {
            // 바퀴 회전 (애커만 스티어링)
            if (axleInfo.steering)
            {
                for (int i = 0; i < 2; i++)
                {
                    axleInfo.leftWheel[i].steerAngle = Mathf.Rad2Deg * Mathf.Atan(kart.wheelBase / (radius + halfVehicleWidth * input.move.x)) * steering;
                    axleInfo.rightWheel[i].steerAngle = Mathf.Rad2Deg * Mathf.Atan(kart.wheelBase / (radius - halfVehicleWidth * input.move.x)) * steering;
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
            Debug.Log(axleInfo.leftWheel[1].rpm);
            Debug.Log(axleInfo.rightWheel[1].rpm);
        }
        Debug.Log($"{rigid.velocity.magnitude * 3.6f} km/h" );
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
            kart.wheels_Col[i * 2].GetWorldPose(out temp1, out wheelRotation);
            kart.wheels_Col[i * 2 + 1].GetWorldPose(out temp2, out wheelRotation);
            wheelPosition = (temp1 + temp2) * 0.5f;
            kart.wheels_Mesh[i].transform.position = wheelPosition;
            kart.wheels_Mesh[i].transform.rotation = wheelRotation;
        }
    }
    private void Drift()
    {
        for (int i = 0; i < 2; i++)
        {
            if (input.drift)
            {
                kart.axleInfos[1].leftWheel[i].forwardFriction = kart.driftFriction_f;
                kart.axleInfos[1].leftWheel[i].sidewaysFriction = kart.driftFriction_s;
                kart.axleInfos[1].rightWheel[i].forwardFriction = kart.driftFriction_f;
                kart.axleInfos[1].rightWheel[i].sidewaysFriction = kart.driftFriction_s;
            }
            else
            {
                kart.axleInfos[1].leftWheel[i].forwardFriction = kart.nomalFriction_f;
                kart.axleInfos[1].leftWheel[i].sidewaysFriction = kart.nomalFriction_s;
                kart.axleInfos[1].rightWheel[i].forwardFriction = kart.nomalFriction_f;
                kart.axleInfos[1].rightWheel[i].sidewaysFriction = kart.nomalFriction_s;
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