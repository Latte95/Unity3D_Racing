using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test2 : MonoBehaviour
{
    public WheelCollider[] wheel;
    Rigidbody rigid;
    WheelFrictionCurve temp;
    WheelFrictionCurve initFrontTireSideFric;

    float torque = 800;

    // 공기역학 계산
    float tireContactArea;          // 타이어 총 접촉면
    float tirePressure = 220000;    // 타이어 공기압
    float airDensity = 1.225f;      // 공기 저항
    float carFrontalArea = 2.5f;    // 차량 단면적
    float carWeigth;                // 차량 무게
    float dragCoefficient = 0.3f;   // 항력 계수

    float KPH;

    private void Start()
    {
        TryGetComponent(out rigid);
        initFrontTireSideFric = wheel[0].sidewaysFriction;
        carWeigth = rigid.mass * 9.8f;
        tireContactArea = carWeigth / tirePressure;
        rigid.centerOfMass += 0.3f * Vector3.down;
    }

    private void Update()
    {
        KPH = rigid.velocity.magnitude * 3.6f;
    }

    private void FixedUpdate()
    {
        Move();
        Curve();
        DownForce();
    }

    private void Move()
    {
        float v = Input.GetAxis("Vertical");
         // 이동
         wheel[2].motorTorque = torque * v * 0.5f;
        wheel[3].motorTorque = torque * v * 0.5f;
        float directForward = Vector3.Dot(rigid.velocity.normalized, transform.forward);
        if (directForward * v < 0)
        {
            wheel[2].brakeTorque += torque * Time.deltaTime;
            wheel[3].brakeTorque += torque * Time.deltaTime;
        }
        else
        {
            wheel[2].brakeTorque = 0;
            wheel[3].brakeTorque = 0;
        }
    }
    private void Curve()
    {
        float h = Input.GetAxis("Horizontal"); 
        // 회전
        if (h != 0)
        {
            wheel[0].steerAngle = 10 * h;
            wheel[1].steerAngle = 10 * h;
        }
        else
        {
            wheel[0].steerAngle = 0;
            wheel[1].steerAngle = 0;
        }
    }
    private void DownForce()
    {
        // 공기역학
        //float force = 50 * rigid.velocity.magnitude;
        float force = (0 + (0.5f * airDensity * carFrontalArea * dragCoefficient * rigid.velocity.sqrMagnitude)) / tireContactArea;
        rigid.AddForce(-transform.up * force);
        temp = initFrontTireSideFric;
        temp.extremumSlip = initFrontTireSideFric.extremumSlip * (1 + force / carWeigth);
        temp.asymptoteSlip = initFrontTireSideFric.asymptoteSlip * (1 + force / carWeigth);
        wheel[0].sidewaysFriction = temp;
        wheel[1].sidewaysFriction = temp;
    }
}
