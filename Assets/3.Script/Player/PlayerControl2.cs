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
    WheelHit hit;

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
        Vector3 center = Vector3.zero;
        int tireNum = 0;
        foreach (AxleInfo a in kart.axleInfos)
        {
            center += a.leftWheel.transform.localPosition;
            center += a.rightWheel.transform.localPosition;
            tireNum++;
        }
        center /= tireNum;
        rigid.centerOfMass = center + 0.1f * Vector3.down + 0.3f * Vector3.forward;
    }

    private void Update()
    {
        UpdateSpeed();
        WheelPos();
    }

    private void FixedUpdate()
    {
        Drift();
        Move();
        Curve();
        DownForce();
        Test();
    }

    #region 이동

    int aaaa = 0;
    void Test()
    {
        //if (kart.axleInfos[1].leftWheel.GetGroundHit(out hit))
        //{
        //    Debug.Log("앞 : " + hit.forwardSlip);
        //    Debug.Log("뒤 : " + hit.sidewaysSlip);
        //}
        if (!kart.axleInfos[0].leftWheel.GetGroundHit(out var hit2))
        {
            Debug.Log(1);
        }
        if (!kart.axleInfos[1].leftWheel.GetGroundHit(out var hit3))
        {
            Debug.Log(2);
        }

    }

    private void Drift()
    {
        // 뒷바퀴의 측면 마찰력을 줄여서 드리프트 구현
        if (input.drift)
        {
            //kart.axleInfos[1].leftWheel.forwardFriction = kart.driftRearTireForwardFric;
            kart.axleInfos[1].leftWheel.sidewaysFriction = kart.driftRearTireSideFric;
            //kart.axleInfos[1].rightWheel.forwardFriction = kart.driftRearTireForwardFric;
            kart.axleInfos[1].rightWheel.sidewaysFriction = kart.driftRearTireSideFric;
        }
        else
        {
            //kart.axleInfos[1].leftWheel.forwardFriction = kart.initRearTireForwardFric;
            kart.axleInfos[1].leftWheel.sidewaysFriction = kart.initRearTireSideFric;
            //kart.axleInfos[1].rightWheel.forwardFriction = kart.initRearTireForwardFric;
            kart.axleInfos[1].rightWheel.sidewaysFriction = kart.initRearTireSideFric;
        }
    }

    private void Move()
    {
        float rpmAvg = 0;
        float targetTorque;
        float tireNum = 0;

        // 평균 rpm 계산
        foreach (AxleInfo a in kart.axleInfos)
        {
            if (a.motor)
            {
                rpmAvg += a.leftWheel.rpm + a.rightWheel.rpm;
                tireNum += 2;
            }
        }
        rpmAvg /= tireNum;

        // 토크 계산 (모터 힘)
        // 100km/h 이전까지는 빠르게 가속
        if (Mathf.Abs(rpmAvg) < targetRPM && KPH < kart.maxSpeed * 0.8f)
        {
            targetTorque = kart.torque * input.move.y * 0.5f * (1 + (1 - Mathf.Abs(rpmAvg / targetRPM)) * kart.accel);
        }
        // 최고 속도 넘어가면 가속x
        else if (KPH > kart.maxSpeed)
        {
            targetTorque = 0;
        }
        // 100~최고속도 구간
        else
        {
            targetTorque = kart.torque * input.move.y * 0.5f;
        }

        // 바퀴 굴림
        foreach (AxleInfo a in kart.axleInfos)
        {
            // 엔진 있는 바퀴만 굴림
            if (a.motor)
            {
                a.leftWheel.motorTorque = targetTorque;
                a.rightWheel.motorTorque = targetTorque;

                // 반대 방향을 누른 순간 모터를 멈춰 바퀴가 바로 반대로 돌아가도록함
                if (rpmAvg * input.move.y < 0)
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
    }

    private void Curve()
    {
        // 회전
        if (input.move.x != 0)
        {
            // 속도가 빠를수록 핸들이 천천히 꺾임
            float rotationSpeedFactor = Mathf.Clamp(kart.maxSpeed / KPH, 1f, 5f);

            kart.axleInfos[0].leftWheel.steerAngle = Mathf.Lerp(kart.axleInfos[0].leftWheel.steerAngle, Mathf.Rad2Deg * Mathf.Atan(kart.wheelBase / (radius + kart.vehicleWidth * 0.5f * input.move.x)) * kart.steerRotate * input.move.x, Time.deltaTime * rotationSpeedFactor);
            kart.axleInfos[0].rightWheel.steerAngle = Mathf.Lerp(kart.axleInfos[0].rightWheel.steerAngle, Mathf.Rad2Deg * Mathf.Atan(kart.wheelBase / (radius - kart.vehicleWidth * 0.5f * input.move.x)) * kart.steerRotate * input.move.x, Time.deltaTime * rotationSpeedFactor);

            //kart.axleInfos[0].leftWheel.steerAngle = Mathf.Lerp(kart.axleInfos[0].leftWheel.steerAngle, kart.steerRotate * input.move.x, Time.deltaTime * rotationSpeedFactor);
            //kart.axleInfos[0].rightWheel.steerAngle = Mathf.Lerp(kart.axleInfos[0].rightWheel.steerAngle, kart.steerRotate * input.move.x, Time.deltaTime * rotationSpeedFactor);
            Debug.Log(kart.axleInfos[0].rightWheel.steerAngle);
        }
        else
        {
            // 속도가 빠를수록 핸들이 빠르게 돌아옴
            //float returnSpeedFactor = KPH / 10;
            //float returnSpeedFactor = Mathf.Clamp(KPH / 10, 1f, 10f);
            float returnSpeedFactor = Mathf.Clamp(kart.maxSpeed / KPH, 1f, 10f);

            kart.axleInfos[0].leftWheel.steerAngle = Mathf.Lerp(kart.axleInfos[0].leftWheel.steerAngle, 0, Time.deltaTime * returnSpeedFactor);
            kart.axleInfos[0].rightWheel.steerAngle = Mathf.Lerp(kart.axleInfos[0].rightWheel.steerAngle, 0, Time.deltaTime * returnSpeedFactor);
        }
    }
    #endregion 이동

    #region 계산
    /// <summary>
    /// 차량 속도에 따른 압력 (공기역학) -> 차량에 도로에 붙어 있도록 만듦
    /// </summary>
    private void DownForce()
    {
        float force;
        // 현실 계산
        // force = (0.5f * airDensity * carFrontalArea * dragCoefficient * rigid.velocity.sqrMagnitude) / tireContactArea;
        // 게임성 위한 조정값
        force = rigid.mass * (-1 + KPH / 30);
        if (force < 0)
        {
            force = 0;
        }

        //rigid.AddForce(-transform.up * force);

        //Vector3 forcePosition = rigid.worldCenterOfMass + offset;
        rigid.AddForceAtPosition(-transform.up * force, rigid.worldCenterOfMass + 0.5f * transform.forward);

        // 
        temp = kart.initForwardTireSideFric;
        temp.extremumSlip *= (1 + force / rigid.mass);
        temp.asymptoteSlip *= (1 + force / rigid.mass);
        kart.axleInfos[0].leftWheel.sidewaysFriction = temp;
        kart.axleInfos[0].rightWheel.sidewaysFriction = temp;
    }

    /// <summary>
    /// 속도 계산 및 화면 표시
    /// </summary>
    private void UpdateSpeed()
    {
        KPH = rigid.velocity.magnitude * 3.6f;
        if (KPH < 0.9f)
        {
            KPH = 0f;
        }
        speed_txt.text = KPH.ToString("F1");
    }
    #endregion

    /// <summary>
    /// 바퀴 이미지를 휠 콜라이더 위치랑 동기화 시키는 메소드
    /// </summary>
    private void WheelPos()
    {
        Vector3 wheelPosition;
        Quaternion wheelRotation;
        int length = kart.axleInfos.Count;

        for (int i = 0; i < length; i++)
        {
            kart.axleInfos[i].leftWheel.GetWorldPose(out wheelPosition, out wheelRotation);
            kart.wheels_Mesh[i * 2].transform.position = wheelPosition;
            kart.wheels_Mesh[i * 2].transform.rotation = wheelRotation;
            kart.axleInfos[i].rightWheel.GetWorldPose(out wheelPosition, out wheelRotation);
            kart.wheels_Mesh[i * 2 + 1].transform.position = wheelPosition;
            kart.wheels_Mesh[i * 2 + 1].transform.rotation = wheelRotation;
        }
    }
}
