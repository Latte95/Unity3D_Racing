using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AxleInfo
{
    public WheelCollider[] leftWheel = new WheelCollider[2];
    public WheelCollider[] rightWheel = new WheelCollider[2];
    public bool motor;
    public bool steering;

    public AxleInfo(WheelCollider l1, WheelCollider l2, WheelCollider r1, WheelCollider r2, bool mo, bool st)
    {
        leftWheel[0] = l1;
        leftWheel[1] = l2;
        rightWheel[0] = r1;
        rightWheel[1] = r2;
        motor = mo;
        steering = st;
    }
}

public class Kart : MonoBehaviour
{
    [Header("Status")]
    [SerializeField]
    public  float speed = 400.0f;
    [SerializeField]
    public  float maxSpeed = 10f;
    [SerializeField]
    public readonly float boostSpeed = 1.3f;
    [SerializeField]
    public  float steerRotate = 10.0f;

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
        for (int i = 0; i < wheels_Mesh.Length; i++)
        {
            wheels[i].transform.position = wheels_Mesh[i].transform.position;
        }
        vehicleWidth = Mathf.Abs(wheels[0].transform.position.magnitude - wheels[1].transform.position.magnitude);
        wheelBase = Mathf.Abs(wheels[0].transform.position.magnitude - wheels[2].transform.position.magnitude);

        // 드리프트를 위한 바퀴 마찰력 세팅을 위한 변수 초기화
        nomalFriction_f = wheels_Col[0].forwardFriction;
        driftFriction_f = wheels_Col[0].forwardFriction;
        driftFriction_f.stiffness = driftFriction;
        nomalFriction_s = wheels_Col[0].sidewaysFriction;
        driftFriction_s = wheels_Col[0].sidewaysFriction;
        driftFriction_s.stiffness = driftFriction;
    }

    private void SetAxelInfo()
    {
        axleInfos.Add(new AxleInfo(wheels_Col[0], wheels_Col[1], wheels_Col[2], wheels_Col[3], true, true));
        axleInfos.Add(new AxleInfo(wheels_Col[4], wheels_Col[5], wheels_Col[6], wheels_Col[7], true, false));
    }
}
