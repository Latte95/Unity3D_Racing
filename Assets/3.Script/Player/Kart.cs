using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AxleInfo
{
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor;
    public bool steering;

    public AxleInfo(WheelCollider lw, WheelCollider rw, bool mo, bool st)
    {
        leftWheel = lw;
        rightWheel = rw;
        motor = mo;
        steering = st;
    }
}

public class Kart : MonoBehaviour
{
    [Space]
    [Header("Status")]
    [SerializeField][Range(400,1600)][Tooltip("���� ������ ���� ��")]
    public float torque = 400.0f;
    [Tooltip("���� �ִ� �ӵ�")]
    public float maxSpeed = 150f;
    [Tooltip("���� �ν��� �ִ� �ӵ�")]
    public float boostSpeed = 180f;
    [Tooltip("���� �ν��� ��")]
    public float boostForce = 1.05f;
    [Tooltip("�ְ�ӵ����� �ش� ��ġ��ŭ ���� �ӵ����� ������ ����")]
    public float accelSpeedRatio = 0.8f;
    [Min(1)][Tooltip("������ �����ϱ� ���� ��")]
    public float accelForce = 100f;
    [Tooltip("���� �ִ� ȸ�� ����")]
    public float steerRotate = 45.0f;
    [Tooltip("���� ȸ�� ���ӵ�")]
    public float handleAccel = 45.0f;

    [Space]
    [Header("Tires")][Space(10)]
    [Tooltip("���� �޽� ������Ʈ")]
    public GameObject[] wheels_Mesh;
    [Tooltip("�帮��Ʈ�� �ĸ� Ÿ�̾� ������ ���")]
    private float driftFriction = 0.85f;
    private WheelCollider[] wheels_Col;
    [Tooltip("���ݶ��̴� ������Ʈ")]
    public GameObject[] wheels_Col_Obj;

    public List<AxleInfo> axleInfos;
    public float vehicleWidth { get; private set; }
    public float wheelBase { get; private set; }

    [Header("��ƼŬ")][Space(10)]
    public ParticleSystem[] boostPar;

    [HideInInspector]
    public Animator anim;

    public WheelFrictionCurve initForwardTireForwardFric;
    public WheelFrictionCurve initForwardTireSideFric;
    public WheelFrictionCurve initRearTireForwardFric;
    public WheelFrictionCurve initRearTireSideFric;
    public WheelFrictionCurve driftRearTireForwardFric;
    public WheelFrictionCurve driftRearTireSideFric;

    // �ִϸ��̼� ĳ��
    public readonly int BananaHitHash = Animator.StringToHash("BananaHit");
    public readonly int ShellHitHash = Animator.StringToHash("ShellHit");

    private void Awake()
    {
        TryGetComponent(out anim);

        wheels_Col_Obj = new GameObject[4];
        wheels_Col_Obj[0] = transform.parent.Find("LFWheel").gameObject;
        wheels_Col_Obj[1] = transform.parent.Find("RFWheel").gameObject;
        wheels_Col_Obj[2] = transform.parent.Find("LRWheel").gameObject;
        wheels_Col_Obj[3] = transform.parent.Find("RRWheel").gameObject;

        int lenth = wheels_Col_Obj.Length;
        wheels_Col = new WheelCollider[lenth];
        for (int i = 0; i < lenth; i++)
        {
            wheels_Col[i] = wheels_Col_Obj[i].GetComponent<WheelCollider>();
        }

        SetAxelInfo();
        SaveFriction();
        SetWheelColliderPosition();
    }

    private void Start()
    {
        vehicleWidth = Mathf.Abs((wheels_Col_Obj[0].transform.position - wheels_Col_Obj[1].transform.position).magnitude);
        wheelBase = Mathf.Abs((wheels_Col_Obj[0].transform.position - wheels_Col_Obj[2].transform.position).magnitude);
    }

    private void SetAxelInfo()
    {
        axleInfos.Add(new AxleInfo(wheels_Col[0], wheels_Col[1], false, true));
        axleInfos.Add(new AxleInfo(wheels_Col[2], wheels_Col[3], true, false));
    }

    /// <summary>
    /// �� ������ ����
    /// </summary>
    private void SaveFriction()
    {
        // �չ���
        // �Ϲ�
        initForwardTireForwardFric = axleInfos[0].leftWheel.forwardFriction;
        initForwardTireSideFric = axleInfos[0].leftWheel.sidewaysFriction;

        // �޹���
        // �Ϲ�
        initRearTireForwardFric = axleInfos[1].leftWheel.forwardFriction;
        initRearTireSideFric = axleInfos[1].leftWheel.sidewaysFriction;
        // �帮��Ʈ
        driftRearTireForwardFric = axleInfos[1].leftWheel.forwardFriction;
        driftRearTireForwardFric.stiffness = initForwardTireForwardFric.stiffness * 2;

        driftRearTireSideFric = axleInfos[1].leftWheel.sidewaysFriction;
        driftRearTireSideFric.stiffness = driftFriction;
        driftRearTireSideFric.asymptoteValue = initRearTireSideFric.asymptoteValue * driftFriction;
    }

    /// <summary>
    /// �� �ݶ��̴� ��ġ�� ���� �̹��� ������Ʈ ��ġ�� ����ȭ
    /// </summary>
    private void SetWheelColliderPosition()
    {
        for (int i = 0; i < wheels_Mesh.Length; i++)
        {
            wheels_Col_Obj[i].transform.position = wheels_Mesh[i].transform.position;
        }
    }


    #region �ν�����
    private void OnValidate()
    {
        torque = Mathf.Round(torque / 10) * 10;
        maxSpeed = Mathf.Round(maxSpeed);
        maxSpeed = Mathf.Clamp(maxSpeed, 100f, 250f);
        steerRotate = Mathf.Clamp(steerRotate, 0f, 60f);
        accelSpeedRatio = Mathf.Clamp(accelSpeedRatio, 0f, 1f);
    }
    #endregion �ν�����
}
