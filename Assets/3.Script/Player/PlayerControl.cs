using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControl : CharacterControl
{
    [Header("Status")]
    [Tooltip("차량의 회전 반지름")]
    public float radius = 60;
    [Tooltip("마찰력 변화율")]
    private float stiffnessTransition = 0f;

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

    [HideInInspector]
    public PlayerInput input;

    [Header("ETC")]
    [SerializeField]
    [Tooltip("속도 표시 텍스트 UI")]
    private Text speed_txt;
    [SerializeField]
    [Tooltip("몇 바퀴 째인지 표시 하는 텍스트 UI")]
    private Text currentLap_txt;

    [HideInInspector]
    public Inventory inven;
    private Transform currentCheckPoint;

    // 캐싱
    private GameManager gameManager;

    // 애니메이션 캐싱
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

    private new void Awake()
    {
        base.Awake();
        TryGetComponent(out input);
        TryGetComponent(out inven);
        Init();
    }

    private void Start()
    {
        carWeigth = rigid.mass * 9.8f;
        tireContactArea = carWeigth / tirePressure;

        //StartCoroutine(SetKart_co());

        SetState(cantMoveState);
        StartCoroutine(CountDown_co(1));
    }

    private new void Update()
    {
        base.Update();
        UpdateSpeed();
        WheelPos();
        SetAnimation();
        ResetPositon();
        UseItem();
    }

    private new void FixedUpdate()
    {
        Drift();
        Move();
        Curve();
        DownForce();
        AirBorne();
        if (input.move.y <= 0 && boostTime > 0)
        {
            boostTime = 0;
        }
        base.FixedUpdate();
    }

    private void UseItem()
    {
        if (input.useItem)
        {
            input.useItem = false;
            if (inven.items.Count > 0)
            {
                inven.items[0].behavior.UseItem(this);
                inven.RemoveItem();
            }
        }
    }

    #region 이동
    private void Drift()
    {
        LRTire.GetGroundHit(out WheelHit hit);
        // 뒷바퀴의 측면 마찰력을 줄여서 드리프트 구현
        if (input.drift && KPH > 40)
        {
            float sideSlip = Mathf.Abs(hit.sidewaysSlip);
            if (sideSlip < 0.5f)
            {
                stiffnessTransition = 0f;

                LRTire.sidewaysFriction = kart.driftRearTireSideFric;
                RRTire.sidewaysFriction = kart.driftRearTireSideFric;
            }
            else
            {
                stiffnessTransition += Time.deltaTime * 0.3f;

                WheelFrictionCurve newFrictionCurve = LRTire.sidewaysFriction;
                newFrictionCurve.asymptoteValue = Mathf.Lerp(kart.driftRearTireSideFric.asymptoteValue, kart.initRearTireSideFric.asymptoteValue, stiffnessTransition);
                newFrictionCurve.stiffness = Mathf.Lerp(kart.driftRearTireSideFric.stiffness, kart.initRearTireSideFric.stiffness, stiffnessTransition);

                LRTire.sidewaysFriction = newFrictionCurve;
                RRTire.sidewaysFriction = newFrictionCurve;

                bool isMovingForward = input.move.y > 0 && !currentState.Equals(cantMoveState);
                if (isMovingForward)
                {
                    Quaternion rotation = Quaternion.Euler(0, LFTire.steerAngle, 0);
                    Vector3 direction = rotation * -LFTire.transform.up;
                    rigid.AddForce(kart.accelForce * 3 * direction, ForceMode.Impulse);
                }
            }
        }
        else
        {
            stiffnessTransition = 0f;

            LRTire.sidewaysFriction = kart.initRearTireSideFric;
            RRTire.sidewaysFriction = kart.initRearTireSideFric;
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
        if (KPH < kart.maxSpeed * kart.accelSpeedRatio)
        {
            targetTorque = kart.torque * input.move.y;
            if (input.move.y > 0 && !currentState.Equals(cantMoveState))
            {
                rigid.AddForce(-kart.accelForce * kart.wheels_Col_Obj[0].transform.up, ForceMode.Impulse);
            }
        }
        // 최고 속도 넘어가면 가속x
        else if (KPH > kart.maxSpeed)
        {
            targetTorque = 0;
        }
        // 일정 속도~최고속도 구간
        else
        {
            targetTorque = kart.torque * input.move.y;
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
                if (rpmAvg * input.move.y < 0 || KPH > kart.maxSpeed)
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
            float steerAngle = currentState.Curve() * Mathf.Rad2Deg * Mathf.Atan(kart.wheelBase / (radius + kart.vehicleWidth * 0.5f * input.move.x)) * kart.steerRotate * input.move.x;

            steerAngle = Mathf.Clamp(steerAngle, -kart.steerRotate, kart.handleAccel);
            if (input.drift)
            {
                LFTire.steerAngle = kart.steerRotate * input.move.x;
                RFTire.steerAngle = kart.steerRotate * input.move.x;
            }
            else
            {
                LFTire.steerAngle = Mathf.Lerp(LFTire.steerAngle, steerAngle, Time.deltaTime * rotationSpeedFactor);
                RFTire.steerAngle = Mathf.Lerp(RFTire.steerAngle, steerAngle, Time.deltaTime * rotationSpeedFactor);
            }
        }
        else
        {
            // 속도가 빠를수록 핸들이 천천히 돌아옴
            float returnSpeedFactor = 10; //Mathf.Clamp(KPH / 10, 1f, 10f);   // => 속도 빠를수록 빠르게 돌릴 경우
            //float returnSpeedFactor = Mathf.Clamp(kart.maxSpeed / KPH, 1f, 10f);

            LFTire.steerAngle = Mathf.Lerp(LFTire.steerAngle, 0, Time.deltaTime * returnSpeedFactor);
            RFTire.steerAngle = Mathf.Lerp(RFTire.steerAngle, 0, Time.deltaTime * returnSpeedFactor);
        }
        curveBlend = (LFTire.steerAngle + RFTire.steerAngle) * 0.5f;
    }

    private void ResetPositon()
    {
        bool canReset = currentCheckPoint != null && input.resetPosition && !currentState.Equals(cantMoveState);
        if (canReset)
        {
            Quaternion rotation = Quaternion.Euler(currentCheckPoint.rotation.eulerAngles.x, currentCheckPoint.rotation.eulerAngles.y + 180, currentCheckPoint.rotation.eulerAngles.z);
            transform.SetPositionAndRotation(currentCheckPoint.position, rotation);
            //SetState(cantMoveState);
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
            LFTire.brakeTorque = 5000;
            RFTire.brakeTorque = 5000;
            LRTire.brakeTorque = 5000;
            RRTire.brakeTorque = 5000;
            rigid.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationY;
            StartCoroutine(ReMove_co());
        }
    }
    private IEnumerator ReMove_co()
    {
        yield return new WaitUntil(() => LRTire.GetGroundHit(out WheelHit hit));
        LFTire.brakeTorque = 0;
        RFTire.brakeTorque = 0;
        LRTire.brakeTorque = 0;
        RRTire.brakeTorque = 0;
        rigid.constraints = RigidbodyConstraints.None;
        //SetState(nomalState);
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
        //tempFric = kart.initForwardTireSideFric;
        //tempFric.extremumSlip *= (1 + force / rigid.mass);
        //tempFric.asymptoteSlip *= (1 + force / rigid.mass);
        //LFTire.sidewaysFriction = tempFric;
        //RFTire.sidewaysFriction = tempFric;
    }

    /// <summary>
    /// 속도 계산 및 화면 표시
    /// </summary>
    private void UpdateSpeed()
    {
        if (Mathf.Abs(rigid.velocity.x) > 1 || Mathf.Abs(rigid.velocity.z) > 1)
        {
            speed_txt.text = ((int)KPH).ToString();
        }
        // 수직으로 떨어지기만 할 때는 속도 0 (첫 시작, 위치 리셋 등등)
        else
        {
            speed_txt.text = "0";
        }
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
            kart.wheels_Mesh[i * 2].transform.SetPositionAndRotation(wheelPosition, wheelRotation);
            kart.axleInfos[i].rightWheel.GetWorldPose(out wheelPosition, out wheelRotation);
            kart.wheels_Mesh[i * 2 + 1].transform.SetPositionAndRotation(wheelPosition, wheelRotation);
        }
    }

    /// <summary>
    /// 공중에서 착지하기 전에 DownForce에 의해 앞으로 고꾸라지는 것 방지하는 메소드
    /// </summary>
    private void AirBorne()
    {
        if (!LRTire.isGrounded && !RRTire.isGrounded)
        {
            if (transform.rotation.x > 0)
            {
                rigid.angularVelocity = new Vector3(0, rigid.angularVelocity.y, rigid.angularVelocity.z);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Path"))
        {
            // 리셋 위치 저장
            currentCheckPoint = other.gameObject.transform;

            // 지나간 경로 저장
            int pathIndex = int.Parse(other.gameObject.name.Replace("Path", "")) - 1;
            gameManager.SetPathCheck(this.gameObject, pathIndex);
        }
    }
    #endregion 계산

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
            charAnim.SetFloat("Curve", curveBlend);
        }
        // 후진 시 기울임 x
        else
        {
            charAnim.SetBool(TurnRightHash, false);
            charAnim.SetBool(TurnLeftHash, false);
        }

        // 후진
        // 이동 방향과 바라보는 방향의 각도가 90도 이상(후진)이면 음수, 이하(전진)면 양수
        float dotProduct = Vector3.Dot(rigid.velocity.normalized, transform.forward.normalized);
        // 뒤 키를 누르고 있고, 뒤로 이동중일 때 뒤를 쳐다봄
        if (input.move.y < 0 && dotProduct < 0 && KPH > 5)
        {
            charAnim.SetBool(ReverseHash, true);
        }
        else
        {
            charAnim.SetBool(ReverseHash, false);
        }
    }
    #endregion 애니메이션

    #region 시작 전 초기 설정들
    private new void Init()
    {
        gameManager = GameManager.Instance;

        GameObject kartPrefab = Resources.Load<GameObject>("Kart/" + gameManager.kartName);
        if (kartPrefab != null)
        {
            // 카트 생성
            GameObject kartInstance = Instantiate(kartPrefab, transform);
            kartInstance.name = "Kart";
            kartInstance.transform.SetSiblingIndex(0);
            kartInstance.TryGetComponent(out kart);

            // 캐릭터 생성
            GameObject characterPrefab = Resources.Load<GameObject>("Character/" + gameManager.charName);
            if (characterPrefab != null)
            {
                GameObject characterInstance = Instantiate(characterPrefab, kartInstance.transform);
                characterInstance.name = gameManager.charName;
                characterInstance.transform.SetSiblingIndex(1);
                characterInstance.TryGetComponent(out charAnim);
            }
        }
        base.Init();

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
        gameManager.isPlay = true;
        SetState(nomalState);
    }
    #endregion 초기 설정

    public override void HandleItem(Item item)
    {
        inven.AddItem(item);
    }

    public override void LapIncrease()
    {
        currentLap_txt.text = currentLapCount.ToString();
    }
}
