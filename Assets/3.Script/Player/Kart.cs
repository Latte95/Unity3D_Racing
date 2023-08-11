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
    [SerializeField][Range(400,1600)][Tooltip("바퀴 돌리는 엔진 힘")]
    public float torque = 400.0f;
    [Tooltip("차량 최대 속도")]
    public float maxSpeed = 150f;
    [Tooltip("차량 부스터 최대 속도")]
    public float boostSpeed = 180f;
    [Tooltip("차량 부스터 힘")]
    public float boostForce = 1.05f;
    [Tooltip("최고속도에서 해당 수치만큼 곱한 속도까지 빠르게 가속")]
    public float accelSpeedRatio = 0.8f;
    [Min(1)][Tooltip("빠르게 가속하기 위한 힘")]
    public float accelForce = 100f;
    [Tooltip("차량 최대 회전 각도")]
    public float steerRotate = 45.0f;
    [Tooltip("차량 회전 가속도")]
    public float handleAccel = 45.0f;

    [Space]
    [Header("Tires")][Space(10)]
    [Tooltip("바퀴 메쉬 오브젝트")]
    public GameObject[] wheels_Mesh;
    [Tooltip("드리프트시 후면 타이어 마찰력 계수")]
    private float driftFriction = 0.85f;
    private WheelCollider[] wheels_Col;
    [Tooltip("휠콜라이더 오브젝트")]
    public GameObject[] wheels_Col_Obj;

    public List<AxleInfo> axleInfos;
    public float vehicleWidth { get; private set; }
    public float wheelBase { get; private set; }

    [Header("파티클")][Space(10)]
    public ParticleSystem[] boostPar;

    [HideInInspector]
    public Animator anim;

    public WheelFrictionCurve initForwardTireForwardFric;
    public WheelFrictionCurve initForwardTireSideFric;
    public WheelFrictionCurve initRearTireForwardFric;
    public WheelFrictionCurve initRearTireSideFric;
    public WheelFrictionCurve driftRearTireForwardFric;
    public WheelFrictionCurve driftRearTireSideFric;

    // 애니메이션 캐싱
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
    /// 휠 마찰력 저장
    /// </summary>
    private void SaveFriction()
    {
        // 앞바퀴
        // 일반
        initForwardTireForwardFric = axleInfos[0].leftWheel.forwardFriction;
        initForwardTireSideFric = axleInfos[0].leftWheel.sidewaysFriction;

        // 뒷바퀴
        // 일반
        initRearTireForwardFric = axleInfos[1].leftWheel.forwardFriction;
        initRearTireSideFric = axleInfos[1].leftWheel.sidewaysFriction;
        // 드리프트
        driftRearTireForwardFric = axleInfos[1].leftWheel.forwardFriction;
        driftRearTireForwardFric.stiffness = initForwardTireForwardFric.stiffness * 2;

        driftRearTireSideFric = axleInfos[1].leftWheel.sidewaysFriction;
        driftRearTireSideFric.stiffness = driftFriction;
        driftRearTireSideFric.asymptoteValue = initRearTireSideFric.asymptoteValue * driftFriction;
    }

    /// <summary>
    /// 휠 콜라이더 위치를 바퀴 이미지 오브젝트 위치로 동기화
    /// </summary>
    private void SetWheelColliderPosition()
    {
        for (int i = 0; i < wheels_Mesh.Length; i++)
        {
            wheels_Col_Obj[i].transform.position = wheels_Mesh[i].transform.position;
        }
    }


    #region 인스펙터
    private void OnValidate()
    {
        torque = Mathf.Round(torque / 10) * 10;
        maxSpeed = Mathf.Round(maxSpeed);
        maxSpeed = Mathf.Clamp(maxSpeed, 100f, 250f);
        steerRotate = Mathf.Clamp(steerRotate, 0f, 60f);
        accelSpeedRatio = Mathf.Clamp(accelSpeedRatio, 0f, 1f);
    }
    #endregion 인스펙터
}
