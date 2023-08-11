using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControl : CharacterControl
{
    [Header("Status")]
    [Tooltip("������ ȸ�� ������")]
    public float radius = 60;
    [Tooltip("������ ��ȭ��")]
    private float stiffnessTransition = 0f;

    [Header("State")]
    private PlayerState currentState = new CantMoveState();
    public NormalState nomalState = new NormalState();
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

    [HideInInspector]
    public PlayerInput input;

    [Header("ETC")]
    [SerializeField]
    [Tooltip("�ӵ� ǥ�� �ؽ�Ʈ UI")]
    private Text speed_txt;
    [SerializeField]
    [Tooltip("�� ���� °���� ǥ�� �ϴ� �ؽ�Ʈ UI")]
    private Text currentLap_txt;
    public GameObject icon;

    [HideInInspector]
    public Inventory inven;
    private Transform currentCheckPoint;

    // ĳ��
    private GameManager gameManager;
    private TimeManager timeManager;

    // �ִϸ��̼� ĳ��
    private readonly int TurnLeftHash = Animator.StringToHash("TurnLeft");
    private readonly int TurnRightHash = Animator.StringToHash("TurnRight");
    private readonly int ThrowForwardHash = Animator.StringToHash("ThrowForward");
    private readonly int ThrowBackwardHash = Animator.StringToHash("ThrowBackward");
    private readonly int CurveHash = Animator.StringToHash("Curve");

    private new void Awake()
    {
        base.Awake();
        TryGetComponent(out input);
        TryGetComponent(out inven);
        speed_txt = GameObject.FindGameObjectWithTag("Speed").GetComponent<Text>();
        currentLap_txt = GameObject.FindGameObjectWithTag("CurrentLap").GetComponent<Text>();
        timeManager = FindObjectOfType<TimeManager>();
    }

    private void Start()
    {
        Init();
        carWeigth = rigid.mass * 9.8f;
        tireContactArea = carWeigth / tirePressure;

        SetState(cantMoveState);
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

    public void UseItem()
    {
        // PC
        if (input.useItem)
        {
            input.useItem = false;
            if (inven.items.Count > 0)
            {
                inven.items[0].UseItem(this);
                inven.RemoveItem();
            }
        }
    }
    public void UseItem(int index)
    {
        // �����
        if (inven.items.Count > index)
        {
            if (inven.items[index] != null)
            {
                inven.items[index].UseItem(this);
                inven.RemoveItem(index);
            }
        }
    }

    #region �̵�
    private void Drift()
    {
        LRTire.GetGroundHit(out WheelHit hit);
        // �޹����� ���� �������� �ٿ��� �帮��Ʈ ����
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
            }
            bool isMovingForward = input.move.y > 0 && !currentState.Equals(cantMoveState);
            if (isMovingForward && KPH < kart.maxSpeed)
            {
                Quaternion rotation = Quaternion.Euler(0, LFTire.steerAngle, 0);
                Vector3 direction = rotation * -LFTire.transform.up;
                rigid.AddForce(kart.accelForce * 100 * direction, ForceMode.Impulse);
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
        if (KPH < kart.maxSpeed * kart.accelSpeedRatio)
        {
            targetTorque = kart.torque * input.move.y;
            if (input.move.y > 0 && !currentState.Equals(cantMoveState))
            {
                rigid.AddForce(-kart.accelForce * kart.wheels_Col_Obj[0].transform.up, ForceMode.Impulse);
            }
        }
        // �ְ� �ӵ� �Ѿ�� ����x
        else if (KPH > kart.maxSpeed)
        {
            targetTorque = 0;
        }
        // ���� �ӵ�~�ְ�ӵ� ����
        else
        {
            targetTorque = kart.torque * input.move.y;
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
                    a.leftWheel.motorTorque = targetTorque * 0.5f;
                    a.rightWheel.motorTorque = targetTorque * 0.5f;
                }

                // �ݴ� ������ ���� ���� ���͸� ���� ������ �ٷ� �ݴ�� ���ư�������
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
        // ȸ��
        if (input.move.x != 0)
        {
            // �ӵ��� �������� �ڵ��� õõ�� ����
            float rotationSpeedFactor = Mathf.Clamp(kart.maxSpeed / KPH, 1f, 5f);
            float steerAngle = currentState.CurveDirection() * Mathf.Rad2Deg * Mathf.Atan(kart.wheelBase / (radius + kart.vehicleWidth * 0.5f * input.move.x)) * kart.steerRotate * input.move.x;

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
            // �ӵ��� �������� �ڵ��� õõ�� ���ƿ�
            //float returnSpeedFactor = Mathf.Clamp(kart.maxSpeed / KPH, 1f, 10f);
            //Mathf.Clamp(KPH / 10, 1f, 10f);   // => �ӵ� �������� ������ ���� ���

            float returnSpeedFactor = 10;

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
    #endregion �̵�

    #region ���
    /// <summary>
    /// ���� �ӵ��� ���� �з� (���⿪��) -> ������ ���ο� �پ� �ֵ��� ����
    /// </summary>
    private void DownForce()
    {
        float force;
        // ���� ���
        force = (0.5f * airDensity * carFrontalArea * dragCoefficient * rigid.velocity.sqrMagnitude) / tireContactArea;
        // ���Ӽ� ���� ������
        //force = rigid.mass * (-1 + KPH / 30);
        if (force < 0)
        {
            force = 0;
        }

        // rigid.AddForce(-transform.up * force); => ���� �߽ɿ� �� ���� ���
        rigid.AddForceAtPosition(-transform.up * force, rigid.worldCenterOfMass + 0.5f * transform.forward);

        // �ӵ��� ���� ���� ȸ���� sideSlip�� Ŀ�� => ������ ����
        // ��asymptoteSlip�� �ӵ��� ����ϰ� ��ȭ���� �̲����� ����
        //tempFric = kart.initForwardTireSideFric;
        //tempFric.extremumSlip *= (1 + force / rigid.mass);
        //tempFric.asymptoteSlip *= (1 + force / rigid.mass);
        //LFTire.sidewaysFriction = tempFric;
        //RFTire.sidewaysFriction = tempFric;
    }

    /// <summary>
    /// �ӵ� ��� �� ȭ�� ǥ��
    /// </summary>
    private void UpdateSpeed()
    {
        if (Mathf.Abs(rigid.velocity.x) > 1 || Mathf.Abs(rigid.velocity.z) > 1)
        {
            speed_txt.text = ((int)KPH).ToString();
        }
        // �������� �������⸸ �� ���� �ӵ� 0 (ù ����, ��ġ ���� ���)
        else
        {
            speed_txt.text = "0";
        }
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
            kart.wheels_Mesh[i * 2].transform.SetPositionAndRotation(wheelPosition, wheelRotation);
            kart.axleInfos[i].rightWheel.GetWorldPose(out wheelPosition, out wheelRotation);
            kart.wheels_Mesh[i * 2 + 1].transform.SetPositionAndRotation(wheelPosition, wheelRotation);
        }
    }

    /// <summary>
    /// ���߿��� �����ϱ� ���� DownForce�� ���� ������ ��ٶ����� �� �����ϴ� �޼ҵ�
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
            // ���� ��ġ ����
            currentCheckPoint = other.gameObject.transform;

            // ������ ��� ����
            int pathIndex = int.Parse(other.gameObject.name.Replace("Path", "")) - 1;
            gameManager.SetPathCheck(this.gameObject, pathIndex);
        }
    }
    #endregion ���

    public void SetState(PlayerState state)
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
            charAnim.SetFloat(CurveHash, curveBlend);
        }
        // ���� �� ����� x
        else
        {
            charAnim.SetBool(TurnRightHash, false);
            charAnim.SetBool(TurnLeftHash, false);
        }

        // ����
        // �̵� ����� �ٶ󺸴� ������ ������ 90�� �̻�(����)�̸� ����, ����(����)�� ���
        float dotProduct = Vector3.Dot(rigid.velocity.normalized, transform.forward.normalized);
        // �Ĺ� �ٶ󺸴� �ִϸ��̼��� mario�ۿ� ��� ����
    }
    #endregion �ִϸ��̼�

    #region ���� �� �ʱ� ������
    public int myIndex;
    public string myName;
    [PunRPC]
    public void SetMyIndex(int index, string name)
    {
        myIndex = index;
        myName = name;
    }
    private new void Init()
    {
        gameManager = GameManager.Instance;

        GameObject kartPrefab = Resources.Load<GameObject>("Kart/" + gameManager.kartName[myIndex]);
        if (kartPrefab != null)
        {
            // īƮ ����
            GameObject kartInstance = Instantiate(kartPrefab, transform);
            kartInstance.name = "Kart";
            kartInstance.transform.SetSiblingIndex(0);
            kartInstance.TryGetComponent(out kart);

            // ĳ���� ����
            GameObject characterPrefab = Resources.Load<GameObject>("Character/" + gameManager.charName[myIndex]);
            if (characterPrefab != null)
            {
                GameObject characterInstance = Instantiate(characterPrefab, kartInstance.transform);
                characterInstance.name = gameManager.charName[0];
                characterInstance.transform.SetSiblingIndex(1);
                characterInstance.TryGetComponent(out charAnim);
                gameObject.GetComponent<Minimap>().icon = characterInstance.transform.Find("MyIcon").GetComponent<Image>();
            }
        }
        base.Init();

        // ������ �����ϱ� �������� ���ο� �������� �̿��� �������� ������
        rigid.constraints = ~(RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX);
    }
    #endregion �ʱ� ����

    public override void HandleItem(IItem item)
    {
        inven.AddItem(item);
    }

    public override void LapIncrease()
    {
        currentLap_txt.text = currentLapCount.ToString();
        timeManager.SetBestTime();
    }
}
