using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControl2 : MonoBehaviour
{
    [Header("Player")]
    public Kart kart;
    private float KPH;
    //public WheelCollider[] wheel;

    [Header("Status")]
    // 차량의 회전 반지름
    public float radius = 60;
    private float targetRPM;

    [Header("State")]
    private PlayerState currentStarategy = new NormalMoveState();
    public bool isBoost { get; private set; }

    [Header("공기역학 계산")]
    private float tireContactArea;          // 타이어 총 접촉면
    private float tirePressure = 220000;    // 타이어 공기압
    private float airDensity = 1.225f;      // 공기 저항
    private float carFrontalArea = 2.5f;    // 차량 단면적
    private float carWeigth;                // 차량 무게
    private float dragCoefficient = 0.3f;   // 항력 계수

    private WheelFrictionCurve temp;

    [Header("Components")]
    public PlayerInput input;
    private Animator anim;
    Rigidbody rigid;
    [SerializeField]
    private Text speed_txt;



    private void Awake()
    {
        TryGetComponent(out anim);
        TryGetComponent(out rigid);
        TryGetComponent(out input);
        kart = transform.GetChild(0).GetComponentInChildren<Kart>();
    }

    private void Start()
    {
        carWeigth = rigid.mass * 9.8f;
        tireContactArea = carWeigth / tirePressure;

        targetRPM = 100 * 16.6667f / (2 * Mathf.PI * kart.axleInfos[1].leftWheel.radius);

        // 안정성 위해 무게중심 밑으로 설정
        rigid.centerOfMass += 0.2f * Vector3.down;// - 0.3f * Vector3.forward;
        centerMass = rigid.centerOfMass;
    }

    private void Update()
    {
        UpdateSpeed();
        //Debug.Log(Vector3.Angle(transform.forward, rigid.velocity.normalized));
    }

    private void FixedUpdate()
    {
        //Drift();
        Move();
        Curve();
        DownForce();
    }
    private void UpdateSpeed()
    {
        KPH = rigid.velocity.magnitude * 3.6f;
        if (KPH < 0.9f)
        {
            KPH = 0f;
        }
        speed_txt.text = KPH.ToString("F1");
    }

    private void Drift()
    {
        if (input.drift)
        {
            kart.axleInfos[1].leftWheel.forwardFriction = kart.driftRearTireForwardFric;
            kart.axleInfos[1].leftWheel.sidewaysFriction = kart.driftRearTireSideFric;
            kart.axleInfos[1].rightWheel.forwardFriction = kart.driftRearTireForwardFric;
            kart.axleInfos[1].rightWheel.sidewaysFriction = kart.driftRearTireSideFric;
        }
        else
        {
            kart.axleInfos[1].leftWheel.forwardFriction = kart.initForwardTireForwardFric;
            kart.axleInfos[1].leftWheel.sidewaysFriction = kart.initRearTireSideFric;
            kart.axleInfos[1].rightWheel.forwardFriction = kart.initForwardTireForwardFric;
            kart.axleInfos[1].rightWheel.sidewaysFriction = kart.initRearTireSideFric;
        }
    }

    private void Move()
    {
        float rpm = (kart.axleInfos[1].leftWheel.rpm + kart.axleInfos[1].rightWheel.rpm) * 0.5f;
        float targetTorque;
        // 바퀴 굴림
        if (Mathf.Abs(rpm) < targetRPM && KPH < 100)
        {
            targetTorque = kart.torque * input.move.y * 0.5f * (1 + (1 - Mathf.Abs(rpm / targetRPM)) * kart.accel);
        }
        else if (KPH > kart.maxSpeed)
        {
            targetTorque = 0;
        }
        else
        {
            targetTorque = kart.torque * input.move.y * 0.5f;
        }

        foreach (AxleInfo a in kart.axleInfos)
        {
            // 엔진 있는 바퀴만 굴림
            if (!a.motor)
            {
                continue;
            }
            a.leftWheel.motorTorque = targetTorque;
            a.rightWheel.motorTorque = targetTorque;

            // 반대 방향을 누른 순간 모터를 멈춰 바퀴가 바로 반대로 돌아가도록함
            if (rpm * input.move.y < 0)
            {
                a.leftWheel.brakeTorque += kart.torque;
                a.rightWheel.brakeTorque += kart.torque;
            }
            else
            {
                a.leftWheel.brakeTorque = 0;
                a.rightWheel.brakeTorque = 0;
            }
        }
    }

    private void Curve()
    {
        // 회전
        if (input.move.x != 0)
        {
            // 속도가 빠를수록 핸들이 천천히 꺾임
            float rotationSpeedFactor = Mathf.Clamp(kart.maxSpeed / KPH, 1f, 10f);

            kart.axleInfos[0].leftWheel.steerAngle = Mathf.Lerp(kart.axleInfos[0].leftWheel.steerAngle, kart.steerRotate * input.move.x, Time.deltaTime * rotationSpeedFactor);
            kart.axleInfos[0].rightWheel.steerAngle = Mathf.Lerp(kart.axleInfos[0].rightWheel.steerAngle, kart.steerRotate * input.move.x, Time.deltaTime * rotationSpeedFactor);
        }
        else
        {
            // 속도가 빠를수록 핸들이 빠르게 돌아옴
            //float returnSpeedFactor = KPH / 10;
            float returnSpeedFactor = Mathf.Clamp(KPH / 10, 1f, 10f);

            kart.axleInfos[0].leftWheel.steerAngle = Mathf.Lerp(kart.axleInfos[0].leftWheel.steerAngle, 0, Time.deltaTime * returnSpeedFactor);
            kart.axleInfos[0].rightWheel.steerAngle = Mathf.Lerp(kart.axleInfos[0].rightWheel.steerAngle, 0, Time.deltaTime * returnSpeedFactor);
        }
    }
    private void DownForce()
    {
        // 공기역학
        float force;
        force = rigid.mass * (-1 + KPH / 30);
        if (force < 0)
        {
            force = 0;
        }

        //float force = (0.5f * airDensity * carFrontalArea * dragCoefficient * rigid.velocity.sqrMagnitude) / tireContactArea;
        rigid.AddForce(-transform.up * force);
        temp = kart.initForwardTireSideFric;
        temp.extremumSlip *= (1 + force / rigid.mass);
        temp.asymptoteSlip *= (1 + force / rigid.mass);
        kart.axleInfos[0].leftWheel.sidewaysFriction = temp;
        kart.axleInfos[0].rightWheel.sidewaysFriction = temp;
    }
}
