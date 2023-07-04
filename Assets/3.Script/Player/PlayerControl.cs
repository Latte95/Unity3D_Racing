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
    // ������ ȸ�� ������
    public float radius = 60;
    private float targetRPM;

    [Header("State")]
    private PlayerState currentState = new CantMoveState();
    private NormalState nomalState = new NormalState();
    private CantMoveState cantMoveState = new CantMoveState();
    private ReverseState reverseState = new ReverseState();
    public bool isBoost { get; private set; }

    [Header("���⿪�� ���")]
    private float tireContactArea;          // Ÿ�̾� �� ���˸�
    private float tirePressure = 220000;    // Ÿ�̾� �����
    private float airDensity = 1.225f;      // ���� ����
    private float carFrontalArea = 2.5f;    // ���� �ܸ���
    private float carWeigth;                // ���� ����
    private float dragCoefficient = 0.3f;   // �׷� ���

    private WheelFrictionCurve tempFric;

    [Header("Components")]
    public PlayerInput input;
    [SerializeField]
    private Animator anim;
    Rigidbody rigid;
    [SerializeField]
    private Text speed_txt;

    [Header("�ִϸ��̼� ĳ��")]
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

        // ������ ���� �����߽� ������ ����
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

    #region �̵�
    float drift = 1;
    private void Drift()
    {
        // �޹����� ���� �������� �ٿ��� �帮��Ʈ ����
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

        // ��� rpm ���
        foreach (AxleInfo a in kart.axleInfos)
        {
            if (a.motor)
            {
                rpmAvg += a.leftWheel.rpm + a.rightWheel.rpm;
                tireNum += 2;
            }
        }
        rpmAvg /= tireNum;

        // ��ũ ��� (���� ��)
        // ���� �ӵ� ���������� ������ ����
        if (Mathf.Abs(rpmAvg) < targetRPM && KPH < kart.maxSpeed * 0.8f)
        {
            targetTorque = kart.torque * input.move.y * 0.5f * (1 + (1 - Mathf.Abs(rpmAvg / targetRPM)) * kart.accel);
        }
        // �ְ� �ӵ� �Ѿ�� ����x
        else if (KPH > kart.maxSpeed)
        {
            targetTorque = 0;
        }
        // ���� �ӵ�~�ְ�ӵ� ����
        else
        {
            targetTorque = kart.torque * input.move.y * 0.5f;
        }

        // ���� ����
        foreach (AxleInfo a in kart.axleInfos)
        {
            // ���� �ִ� ������ ����
            if (a.motor)
            {
                if (input.move.y >= 0)
                {
                    a.leftWheel.motorTorque = targetTorque;
                    a.rightWheel.motorTorque = targetTorque;
                }
                // ������ ���� ���ϰ�
                else
                {
                    a.leftWheel.motorTorque = targetTorque * 0.3f;
                    a.rightWheel.motorTorque = targetTorque * 0.3f;
                }

                // �ݴ� ������ ���� ���� ���͸� ���� ������ �ٷ� �ݴ�� ���ư�������
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
        // ȸ��
        if (input.move.x != 0)
        {
            // �ӵ��� �������� �ڵ��� õõ�� ����
            float rotationSpeedFactor = Mathf.Clamp(kart.maxSpeed / KPH, 1f, 5f);
            float steerAngle = drift * currentState.Curve() * Mathf.Rad2Deg * Mathf.Atan(kart.wheelBase / (radius + kart.vehicleWidth * 0.5f * input.move.x)) * kart.steerRotate * input.move.x;
            steerAngle = Mathf.Clamp(steerAngle, -kart.steerRotate, kart.steerRotate);
            kart.axleInfos[0].leftWheel.steerAngle = Mathf.Lerp(kart.axleInfos[0].leftWheel.steerAngle, steerAngle, Time.deltaTime * rotationSpeedFactor);
            kart.axleInfos[0].rightWheel.steerAngle = Mathf.Lerp(kart.axleInfos[0].rightWheel.steerAngle, steerAngle, Time.deltaTime * rotationSpeedFactor);
        }
        else
        {
            // �ӵ��� �������� �ڵ��� õõ�� ���ƿ�
            //float returnSpeedFactor = Mathf.Clamp(KPH / 10, 1f, 10f); => �ӵ� �������� ������ ���� ���
            float returnSpeedFactor = Mathf.Clamp(kart.maxSpeed / KPH, 1f, 10f);

            kart.axleInfos[0].leftWheel.steerAngle = Mathf.Lerp(kart.axleInfos[0].leftWheel.steerAngle, 0, Time.deltaTime * returnSpeedFactor);
            kart.axleInfos[0].rightWheel.steerAngle = Mathf.Lerp(kart.axleInfos[0].rightWheel.steerAngle, 0, Time.deltaTime * returnSpeedFactor);
        }
        curveBlend = (kart.axleInfos[0].leftWheel.steerAngle + kart.axleInfos[0].rightWheel.steerAngle) * 0.5f;
    }
    #endregion �̵�

    #region ���
    /// <summary>
    /// ���� �ӵ��� ���� �з� (���⿪��) -> ������ ���ο� �پ� �ֵ��� ����
    /// </summary>
    private void DownForce()
    {
        float force;
        // ���� ���
        // force = (0.5f * airDensity * carFrontalArea * dragCoefficient * rigid.velocity.sqrMagnitude) / tireContactArea;
        // ���Ӽ� ���� ������
        force = rigid.mass * (-1 + KPH / 30);
        if (force < 0)
        {
            force = 0;
        }

        // rigid.AddForce(-transform.up * force); => ���� �߽ɿ� �� ���� ���
        rigid.AddForceAtPosition(-transform.up * force, rigid.worldCenterOfMass + 0.5f * transform.forward);

        // �ӵ��� ���� ���� ȸ���� sideSlip�� Ŀ�� => ������ ����
        // ��asymptoteSlip�� �ӵ��� ����ϰ� ��ȭ�� �̲����� ����
        tempFric = kart.initForwardTireSideFric;
        tempFric.extremumSlip *= (1 + force / rigid.mass);
        tempFric.asymptoteSlip *= (1 + force / rigid.mass);
        kart.axleInfos[0].leftWheel.sidewaysFriction = tempFric;
        kart.axleInfos[0].rightWheel.sidewaysFriction = tempFric;
    }

    /// <summary>
    /// �ӵ� ��� �� ȭ�� ǥ��
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
    /// ���� �̹����� �� �ݶ��̴� ��ġ�� ����ȭ ��Ű�� �޼ҵ�
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

    #region �ִϸ��̼�
    /// <summary>
    /// �÷��̾� ĳ���� �ִϸ��̼� ����
    /// </summary>
    float curveBlend;
    private void SetAnimation()
    {
        // Ŀ��
        if (input.move.y >= 0)
        {
            anim.SetFloat("Curve", curveBlend);
        }
        // ���� �� ����� x
        else
        {
            anim.SetBool(TurnRightHash, false);
            anim.SetBool(TurnLeftHash, false);
        }

        // ����
        // �̵� ����� �ٶ󺸴� ������ ������ 90�� �̻�(����)�̸� ����, ����(����)�� ���
        float dotProduct = Vector3.Dot(rigid.velocity.normalized, transform.forward.normalized);
        // �� Ű�� ������ �ְ�, �ڷ� �̵����� �� �ڸ� �Ĵٺ�
        if (input.move.y < 0 && dotProduct < 0 && KPH > 5)
        {
            anim.SetBool(ReverseHash, true);
        }
        else
        {
            anim.SetBool(ReverseHash, false);
        }
    }
    #endregion �ִϸ��̼�
    #region ���� �� �ʱ� ������
    /// <summary>
    /// ù ���۽� ī��Ʈ�ٿ� ���� �̵� ����
    /// </summary>
    /// <param name="time">ī��Ʈ�ٿ� �ð�</param>
    /// <returns></returns>
    private void Init()
    {
        // ������ �����ϱ� �������� ���ο� �������� �̿��� �������� ������
        rigid.constraints = ~(RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX);
    }
    /// <summary>
    /// ī��Ʈ �ٿ��� ������ �÷��̾ �̵� �����ϵ��� �����ϴ� �ڷ�ƾ
    /// </summary>
    /// <param name="time">ī��Ʈ�ٿ� �ð�</param>
    /// <returns></returns>
    private IEnumerator CountDown_co(float time)
    {
        float preTime = time - 1;
        if (preTime < 0)
        {
            preTime = 0;
        }

        // ī��Ʈ�ٿ��� ������ ���� �̸� Freeze�� ���� �̵����� ����
        yield return new WaitForSeconds(preTime);
        rigid.constraints = RigidbodyConstraints.None;

        // ī��Ʈ �ٿ��� ������ �̵� ������ ���·� ��ȯ
        yield return new WaitForSeconds(time - preTime);
        SetState(nomalState);
    }
    #endregion �ʱ� ����
}
