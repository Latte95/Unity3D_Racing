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
    [SerializeField]
    [Tooltip("바퀴 돌리는 엔진 힘")]
    [Range(400,1600)]
    public float torque = 400.0f;
    [Tooltip("차량 최대 속도")]
    public float maxSpeed = 150f;
    [Min(1)]
    [Tooltip("속력이 낮을 때 더 빠르게 가속하기 위한 힘")]
    public float accel = 100f;
    public readonly float boostSpeed = 1.3f;
    [Tooltip("차량 최대 회전 각도")]
    public float steerRotate = 45.0f;
    [Tooltip("차량 회전 가속도")]
    public float handleRotate = 45.0f;

    [Space]
    [Header("Tires")]
    [Space(20)]
    [Tooltip("드리프트시 후면 타이어 마찰력 계수")]
    [Range(0, 1)]
    public float driftFriction = 0.75f;
    [Tooltip("바퀴 메쉬 오브젝트")]
    public GameObject[] wheels_Mesh;
    private WheelCollider[] wheels_Col;
    [Tooltip("휠콜라이더 오브젝트")]
    public GameObject[] wheels_Col_Obj;

    public List<AxleInfo> axleInfos;
    public float vehicleWidth { get; private set; }
    public float wheelBase { get; private set; }



    public WheelFrictionCurve initForwardTireForwardFric;
    public WheelFrictionCurve initForwardTireSideFric;
    public WheelFrictionCurve initRearTireForwardFric;
    public WheelFrictionCurve initRearTireSideFric;
    public WheelFrictionCurve driftRearTireForwardFric;
    public WheelFrictionCurve driftRearTireSideFric;


    [Header("Components")]
    private Animator anim;

    private void Awake()
    {
        int lenth = wheels_Col_Obj.Length;
        wheels_Col = new WheelCollider[lenth];
        for (int i = 0; i < lenth; i++)
        {
            wheels_Col[i] = wheels_Col_Obj[i].GetComponent<WheelCollider>();
        }

        TryGetComponent(out anim);

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
        initForwardTireForwardFric = axleInfos[0].leftWheel.forwardFriction;
        initForwardTireSideFric = axleInfos[0].leftWheel.sidewaysFriction;
        initRearTireForwardFric = axleInfos[1].leftWheel.forwardFriction;
        initRearTireSideFric = axleInfos[1].leftWheel.sidewaysFriction;
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
    }
    #endregion 인스펙터
}
