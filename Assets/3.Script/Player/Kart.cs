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
    public float maxSpeed = 150f;
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
    private WheelCollider[] wheels_Col;
    [HideInInspector]
    public GameObject[] wheels_Col_Obj;

    public WheelFrictionCurve initForwardTireForwardFric;
    public WheelFrictionCurve initForwardTireSideFric;
    public WheelFrictionCurve initRearTireForwardFric;
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
        SaveFriction();
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
    /// ÈÙ ¸¶Âû·Â ÀúÀå
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
        driftRearTireSideFric.stiffness = initRearTireSideFric.stiffness * driftFriction;
        driftRearTireSideFric.asymptoteValue = initRearTireSideFric.asymptoteValue * driftFriction;
    }
}
