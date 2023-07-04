using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControl : MonoBehaviour
{
    [Header("Player")]
    public Kart kart;
    private float KPH;

    [Header("Status")]
    // 차량의 회전 반지름
    public float radius = 60;
    private float targetRPM;

    [Header("State")]
    private PlayerState currentState = new CantMoveState();
    private NormalState nomalState = new NormalState();
    private CantMoveState cantMoveState = new CantMoveState();
    private ReverseState reverseState = new ReverseState();
    public bool isBoost { get; private set; }

    [Header("공기역학 계산")]
    private float tireContactArea;          // 타이어 총 접촉면
    private float tirePressure = 220000;    // 타이어 공기압
    private float airDensity = 1.225f;      // 공기 저항
    private float carFrontalArea = 2.5f;    // 차량 단면적
    private float carWeigth;                // 차량 무게
    private float dragCoefficient = 0.3f;   // 항력 계수

    private WheelFrictionCurve tempFric;

    [Header("Components")]
    public PlayerInput input;
    [SerializeField]
    private Animator anim;
    Rigidbody rigid;
    [SerializeField]
    private Text speed_txt;

    [Header("애니메이션 캐싱")]
    private readonly int TurnLeftHash = Animator.StringToHash("TurnLeft");
    private readonly int TurnRightHash = Animator.StringToHash("TurnRight");
    private readonly int HasItemHash = Animator.StringToHash("HasItem");
    private readonly int HitBySomethingHash = Animator.StringToHash("HitBySomething");
    private readonly int ThrowForwardHash = Animator.StringToHash("ThrowForward");
    private readonly int ThrowBackwardHash = Animator.StringToHash("ThrowBackward");
    private readonly int HitItemHash = Animator.StringToHash("HitItem");
    private readonly int FirstPlaceHash = Animator.StringToHash("FirstPlace");
    private readonly int HurtHash = Animator.StringToHash("Hurt");
    private readonly int ReverseHash = Animator.StringToHash("Reverse");
    private readonly int LoseAnimHash = Animator.StringToHash("LoseAnim");
    private readonly int JumpTrick1Hash = Animator.StringToHash("JumpTrick1");
    private readonly int JumpTrick2Hash = Animator.StringToHash("JumpTrick2");


    private void Awake()
    {
        TryGetComponent(out rigid);
        TryGetComponent(out input);
        kart = transform.GetChild(0).GetComponentInChildren<Kart>();

        Init();
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
        rigid.centerOfMass = center + 0.1f * Vector3.down;// + 0.3f * Vector3.forward;

        SetState(cantMoveState);
        StartCoroutine(CountDown_co(3));
    }

    private void Update()
    {
        UpdateSpeed();
        WheelPos();
        SetAnimation();
    }

    private void FixedUpdate()
    {
        Drift();
        Move();
        Curve();
        //DownForce();
    }

    #region 이동
    float drift = 1;
    private void Drift()
    {
        // 뒷바퀴의 측면 마찰력을 줄여서 드리프트 구현
        if (input.drift && KPH > 40)
        {
            kart.axleInfos[1].leftWheel.sidewaysFriction = kart.driftRearTireSideFric;
            kart.axleInfos[1].rightWheel.sidewaysFriction = kart.driftRearTireSideFric;
        }
        else
        {
            kart.axleInfos[1].leftWheel.sidewaysFriction = kart.initRearTireSideFric;
            kart.axleInfos[1].rightWheel.sidewaysFriction = kart.initRearTireSideFric;
        }
        if(input.drift)
        {
            drift = 3.5f;
        }
        else
        {
            drift = 1;
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
        // 일정 속도 이전까지는 빠르게 가속
        if (Mathf.Abs(rpmAvg) < targetRPM && KPH < kart.maxSpeed * 0.8f)
        {
            targetTorque = kart.torque * input.move.y * 0.5f * (1 + (1 - Mathf.Abs(rpmAvg / targetRPM)) * kart.accel);
        }
        // 최고 속도 넘어가면 가속x
        else if (KPH > kart.maxSpeed)
        {
            targetTorque = 0;
        }
        // 일정 속도~최고속도 구간
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
                if (input.move.y >= 0)
                {
                    a.leftWheel.motorTorque = targetTorque;
                    a.rightWheel.motorTorque = targetTorque;
                }
                // 후진은 모터 약하게
                else
                {
                    a.leftWheel.motorTorque = targetTorque * 0.3f;
                    a.rightWheel.motorTorque = targetTorque * 0.3f;
                }

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
            float steerAngle = drift * currentState.Curve() * Mathf.Rad2Deg * Mathf.Atan(kart.wheelBase / (radius + kart.vehicleWidth * 0.5f * input.move.x)) * kart.steerRotate * input.move.x;
            steerAngle = Mathf.Clamp(steerAngle, -kart.steerRotate, kart.steerRotate);
            kart.axleInfos[0].leftWheel.steerAngle = Mathf.Lerp(kart.axleInfos[0].leftWheel.steerAngle, steerAngle, Time.deltaTime * rotationSpeedFactor);
            kart.axleInfos[0].rightWheel.steerAngle = Mathf.Lerp(kart.axleInfos[0].rightWheel.steerAngle, steerAngle, Time.deltaTime * rotationSpeedFactor);
        }
        else
        {
            // 속도가 빠를수록 핸들이 천천히 돌아옴
            //float returnSpeedFactor = Mathf.Clamp(KPH / 10, 1f, 10f); => 속도 빠를수록 빠르게 돌릴 경우
            float returnSpeedFactor = Mathf.Clamp(kart.maxSpeed / KPH, 1f, 10f);

            kart.axleInfos[0].leftWheel.steerAngle = Mathf.Lerp(kart.axleInfos[0].leftWheel.steerAngle, 0, Time.deltaTime * returnSpeedFactor);
            kart.axleInfos[0].rightWheel.steerAngle = Mathf.Lerp(kart.axleInfos[0].rightWheel.steerAngle, 0, Time.deltaTime * returnSpeedFactor);
        }
        curveBlend = (kart.axleInfos[0].leftWheel.steerAngle + kart.axleInfos[0].rightWheel.steerAngle) * 0.5f;
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

        // rigid.AddForce(-transform.up * force); => 차량 중심에 힘 가할 경우
        rigid.AddForceAtPosition(-transform.up * force, rigid.worldCenterOfMass + 0.5f * transform.forward);

        // 속도가 빠를 수록 회전시 sideSlip이 커짐 => 마찰력 감소
        // ㄴasymptoteSlip을 속도에 비례하게 변화켜 미끄러짐 방지
        tempFric = kart.initForwardTireSideFric;
        tempFric.extremumSlip *= (1 + force / rigid.mass);
        tempFric.asymptoteSlip *= (1 + force / rigid.mass);
        kart.axleInfos[0].leftWheel.sidewaysFriction = tempFric;
        kart.axleInfos[0].rightWheel.sidewaysFriction = tempFric;
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
    #endregion

    private void SetState(PlayerState state)
    {
        currentState = state;
        currentState.SetFriction(this);
    }

    #region 애니메이션
    /// <summary>
    /// 플레이어 캐릭터 애니메이션 설정
    /// </summary>
    float curveBlend;
    private void SetAnimation()
    {
        // 커브
        if (input.move.y >= 0)
        {
            anim.SetFloat("Curve", curveBlend);
        }
        // 후진 시 기울임 x
        else
        {
            anim.SetBool(TurnRightHash, false);
            anim.SetBool(TurnLeftHash, false);
        }

        // 후진
        // 이동 방향과 바라보는 방향의 각도가 90도 이상(후진)이면 음수, 이하(전진)면 양수
        float dotProduct = Vector3.Dot(rigid.velocity.normalized, transform.forward.normalized);
        // 뒤 키를 누르고 있고, 뒤로 이동중일 때 뒤를 쳐다봄
        if (input.move.y < 0 && dotProduct < 0 && KPH > 5)
        {
            anim.SetBool(ReverseHash, true);
        }
        else
        {
            anim.SetBool(ReverseHash, false);
        }
    }
    #endregion 애니메이션
    #region 시작 전 초기 설정들
    /// <summary>
    /// 첫 시작시 카운트다운 이후 이동 가능
    /// </summary>
    /// <param name="time">카운트다운 시간</param>
    /// <returns></returns>
    private void Init()
    {
        // 게임이 시작하기 전까지는 도로에 떨어지는 이외의 움직임을 제한함
        rigid.constraints = ~(RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX);
    }
    /// <summary>
    /// 카운트 다운이 끝나면 플레이어가 이동 가능하도록 제어하는 코루틴
    /// </summary>
    /// <param name="time">카운트다운 시간</param>
    /// <returns></returns>
    private IEnumerator CountDown_co(float time)
    {
        float preTime = time - 1;
        if (preTime < 0)
        {
            preTime = 0;
        }

        // 카운트다운이 끝나기 전에 미리 Freeze를 통한 이동제한 해제
        yield return new WaitForSeconds(preTime);
        rigid.constraints = RigidbodyConstraints.None;

        // 카운트 다운이 끝나면 이동 가능한 상태로 전환
        yield return new WaitForSeconds(time - preTime);
        SetState(nomalState);
    }
    #endregion 초기 설정
}
