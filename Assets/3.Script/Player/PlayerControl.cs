using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerControl : MonoBehaviour
{
    [Header("Player")]
    public Kart kart;
    private float targetSpeed = 0;
    private float currentSpeed = 0;

    [Header("Status")]
    // ������ ȸ�� ������
    public float radius = 600;
    // �ӵ��� ���� �з� ����ġ
    public float downForce = 1000.0f;

    [Header("State")]
    private PlayerState currentStarategy = new NormalMoveState();
    public bool isBoost { get; private set; }

    [Header("Components")]
    public PlayerInput input;
    private Animator anim;
    private Rigidbody rigid;
    [SerializeField]
    private Text speed_txt;

    private void Awake()
    {
        TryGetComponent(out anim);
        TryGetComponent(out rigid);
        TryGetComponent(out input);
        kart = transform.GetChild(0).GetComponentInChildren<Kart>();
    }

    private void Start()
    {
        // ������ ���� �����߽� ������ ����
        //rigid.centerOfMass = Vector3.down;

        temp = kart.frontWheelSideFriction;
    }

    private void FixedUpdate()
    {
        //Drift();
        Move();
        WheelPos();
        UpdateSpeed();
        //AddDownForce();
    }

    #region �̵�
    private void Move()
    {
        // �ӵ� ���
        //if (Mathf.Abs(targetSpeed) < kart.maxSpeed)
        //{
        //    targetSpeed += kart.speed * Time.deltaTime * input.move.y;
        //}
        targetSpeed = kart.speed * input.move.y;
        if (input.move.Equals(Vector2.zero))
        {
            targetSpeed = 0;
        }
        else if (isBoost)
        {
            targetSpeed *= kart.boostSpeed;
        }


        float steering = kart.steerRotate * input.move.x;
        float halfVehicleWidth = kart.vehicleWidth * 0.5f;

        foreach (AxleInfo axleInfo in kart.axleInfos)
        {
            // ���� ȸ�� (��Ŀ�� ��Ƽ�)
            if (axleInfo.steering)
            {
                axleInfo.leftWheel.steerAngle = Mathf.Rad2Deg * Mathf.Atan(kart.wheelBase / (radius + halfVehicleWidth * input.move.x)) * steering;
                axleInfo.rightWheel.steerAngle = Mathf.Rad2Deg * Mathf.Atan(kart.wheelBase / (radius - halfVehicleWidth * input.move.x)) * steering;
            }
            // ���� ����
            if (axleInfo.motor)
            {
                axleInfo.leftWheel.motorTorque = targetSpeed;
                axleInfo.rightWheel.motorTorque = targetSpeed;
                Debug.Log(axleInfo.leftWheel.rpm);
                Debug.Log(axleInfo.rightWheel.rpm);

            }
        }
    }
    private void UpdateSpeed()
    {
        currentSpeed = rigid.velocity.magnitude * 3.6f;
        if(currentSpeed < 0.9f)
        {
            currentSpeed = 0;
        }
        speed_txt.text = currentSpeed.ToString("F1");
    }
    private void Drift()
    {
        if (input.drift)
        {
            kart.axleInfos[1].leftWheel.forwardFriction = kart.driftFriction_f;
            kart.axleInfos[1].leftWheel.sidewaysFriction = kart.driftFriction_s;
            kart.axleInfos[1].rightWheel.forwardFriction = kart.driftFriction_f;
            kart.axleInfos[1].rightWheel.sidewaysFriction = kart.driftFriction_s;
        }
        else
        {
            kart.axleInfos[1].leftWheel.forwardFriction = kart.nomalFriction_f;
            kart.axleInfos[1].leftWheel.sidewaysFriction = kart.nomalFriction_s;
            kart.axleInfos[1].rightWheel.forwardFriction = kart.nomalFriction_f;
            kart.axleInfos[1].rightWheel.sidewaysFriction = kart.nomalFriction_s;
        }
    }
    #endregion �̵�

    #region ���� ����ȭ
    WheelFrictionCurve temp;
    private void AddDownForce()
    {
        float force = downForce * rigid.velocity.magnitude;
        rigid.AddForce(-transform.up * force);

        //for (int i = 0; i < 2; i++)
        //{
        //    if (force != 0)
        //    {
        //        temp.extremumSlip *= (1 + (force / rigid.mass));
        //        temp.asymptoteSlip *= (1 + (force / rigid.mass));
        //        kart.axleInfos[0].leftWheel.sidewaysFriction = temp;
        //        temp.extremumSlip *= (1 + (force / rigid.mass));
        //        temp.asymptoteSlip *= (1 + (force / rigid.mass));
        //        kart.axleInfos[0].rightWheel.sidewaysFriction = temp;
        //    }
        //    else
        //    {
        //        temp = kart.frontWheelSideFriction;
        //        kart.axleInfos[0].leftWheel.sidewaysFriction = kart.frontWheelSideFriction;
        //        kart.axleInfos[0].rightWheel.sidewaysFriction = kart.frontWheelSideFriction;
        //    }
        //}
    }
    #endregion


    /// <summary>
    /// ���� �̹����� �� �ݶ��̴� ��ġ�� ����ȭ ��Ű�� �޼ҵ�
    /// </summary>
    private void WheelPos()
    {
        Vector3 wheelPosition;
        Quaternion wheelRotation;

        for (int i = 0; i < 4; i++)
        {
            kart.wheels_Col[i].GetWorldPose(out wheelPosition, out wheelRotation);
            kart.wheels_Mesh[i].transform.position = wheelPosition;
            kart.wheels_Mesh[i].transform.rotation = wheelRotation;
        }
    }
}