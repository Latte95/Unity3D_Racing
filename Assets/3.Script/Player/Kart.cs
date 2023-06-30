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
    [Header("Status")]
    [SerializeField]
    public float torque = 400.0f;
    [SerializeField]
    public float maxSpeed = 10f;
    [SerializeField]
    public float accel = 100f;
    [SerializeField]
    public readonly float boostSpeed = 1.3f;
    [SerializeField]
    public float steerRotate = 10.0f;

    [Header("Tires")]
    [HideInInspector]
    public GameObject[] wheels_Mesh;
    [HideInInspector]
    public WheelCollider[] wheels_Col;
    [HideInInspector]
    public GameObject[] wheels_Col_Obj;

    public WheelFrictionCurve initForwardTireForwardFric;
    public WheelFrictionCurve initForwardTireSideFric;
    public WheelFrictionCurve initRearTireSideFric;
    public WheelFrictionCurve driftRearTireForwardFric;
    public WheelFrictionCurve driftRearTireSideFric;

    public List<AxleInfo> axleInfos;
    public float vehicleWidth { get; private set; }
    public float wheelBase { get; private set; }
    public float driftFriction = 0.75f;

    [Header("Components")]
    private Animator anim;

    private void Awake()
    {
        wheels_Mesh = GameObject.FindGameObjectsWithTag("WheelMesh");
        wheels_Col_Obj = GameObject.FindGameObjectsWithTag("WheelCollider");
        int lenth = wheels_Col_Obj.Length;
        wheels_Col = new WheelCollider[lenth];
        for (int i = 0; i < lenth; i++)
        {
            wheels_Col[i] = wheels_Col_Obj[i].GetComponent<WheelCollider>();
        }

        TryGetComponent(out anim);
        SetAxelInfo();
    }

    private void Start()
    {
        // 휠 콜라이더 위치를 바퀴 오브젝트(이미지) 위치로 동기화
        //for (int i = 0; i < wheels_Mesh.Length; i++)
        //{
        //    wheels[i].transform.position = wheels_Mesh[i].transform.position;
        //}
        vehicleWidth = Mathf.Abs(wheels_Col_Obj[0].transform.position.magnitude - wheels_Col_Obj[1].transform.position.magnitude);
        wheelBase = Mathf.Abs(wheels_Col_Obj[0].transform.position.magnitude - wheels_Col_Obj[2].transform.position.magnitude);

        // 휠 마찰력
        initForwardTireSideFric = wheels_Col[0].sidewaysFriction;
        initForwardTireForwardFric = wheels_Col[0].forwardFriction;
        initRearTireSideFric = wheels_Col[3].sidewaysFriction;
        driftRearTireForwardFric = wheels_Col[3].forwardFriction;
        driftRearTireForwardFric.stiffness = driftFriction;
        driftRearTireSideFric = wheels_Col[3].sidewaysFriction;
        driftRearTireSideFric.stiffness = driftFriction;
    }

    private void SetAxelInfo()
    {
        axleInfos.Add(new AxleInfo(wheels_Col[0], wheels_Col[1], true, true));
        axleInfos.Add(new AxleInfo(wheels_Col[2], wheels_Col[3], true, false));
    }
}
