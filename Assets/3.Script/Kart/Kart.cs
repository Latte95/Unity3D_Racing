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
    public float speed = 400.0f;
    [SerializeField]
    public float maxSpeed = 10f;
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
    public WheelFrictionCurve nomalFriction_f { get; private set; }
    public WheelFrictionCurve driftFriction_f;
    public WheelFrictionCurve nomalFriction_s { get; private set; }
    public WheelFrictionCurve driftFriction_s;
    public WheelFrictionCurve frontWheelSideFriction;
    public List<AxleInfo> axleInfos;
    private GameObject[] wheels;
    public float vehicleWidth { get; private set; }
    public float wheelBase { get; private set; }
    private float driftFriction = 0.75f;

    [Header("Components")]
    private Animator anim;

    private void Awake()
    {
        wheels_Mesh = GameObject.FindGameObjectsWithTag("WheelMesh");
        wheels = GameObject.FindGameObjectsWithTag("Wheel");
        wheels_Col_Obj = GameObject.FindGameObjectsWithTag("WheelCollider");
        int lenth = wheels_Col_Obj.Length;
        wheels_Col = new WheelCollider[lenth];
        for (int i = 0; i < lenth; i++)
        {
            wheels_Col[i] = wheels_Col_Obj[i].GetComponent<WheelCollider>();
        }

        TryGetComponent(out anim);
    }

    private void Start()
    {
        SetAxelInfo();

        // 휠 콜라이더 위치를 바퀴 오브젝트(이미지) 위치로 동기화
        //for (int i = 0; i < wheels_Mesh.Length; i++)
        //{
        //    wheels[i].transform.position = wheels_Mesh[i].transform.position;
        //}
        vehicleWidth = Mathf.Abs(wheels[0].transform.position.magnitude - wheels[1].transform.position.magnitude);
        wheelBase = Mathf.Abs(wheels[0].transform.position.magnitude - wheels[2].transform.position.magnitude);

        // 드리프트를 위한 바퀴 마찰력 세팅을 위한 변수 초기화
        nomalFriction_f = wheels_Col[3].forwardFriction;
        driftFriction_f = wheels_Col[3].forwardFriction;
        driftFriction_f.stiffness = nomalFriction_f.stiffness * driftFriction;
        nomalFriction_s = wheels_Col[3].sidewaysFriction;
        driftFriction_s = wheels_Col[3].sidewaysFriction;
        driftFriction_s.stiffness = nomalFriction_s.stiffness * driftFriction;
        frontWheelSideFriction = wheels_Col[0].sidewaysFriction;
    }

    private void SetAxelInfo()
    {
        axleInfos.Add(new AxleInfo(wheels_Col[0], wheels_Col[1], false, true));
        axleInfos.Add(new AxleInfo(wheels_Col[2], wheels_Col[3], true, false));
    }
}
