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

    // ���⿪�� ���
    float tireContactArea;          // Ÿ�̾� �� ���˸�
    float tirePressure = 220000;    // Ÿ�̾� �����
    float airDensity = 1.225f;      // ���� ����
    float carFrontalArea = 2.5f;    // ���� �ܸ���
    float carWeigth;                // ���� ����
    float dragCoefficient = 0.3f;   // �׷� ���

    float KPH;

    private void Start()
    {
        TryGetComponent(out rigid);
        initFrontTireSideFric = wheel[0].sidewaysFriction;
        carWeigth = rigid.mass * 9.8f;
        tireContactArea = carWeigth / tirePressure;
    }

    private void Update()
    {
        KPH = rigid.velocity.magnitude * 3.6f;
    }

    private void FixedUpdate()
    {
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        // �̵�
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

        // ȸ��
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
        Debug.Log("���� : " + wheel[2].motorTorque);
        Debug.Log("rpm : " + wheel[2].rpm);
        Debug.Log("�극��ũ : " + wheel[2].brakeTorque);
        Debug.Log("�ӵ� : " + KPH);

        // force = 2000
        // ���⿪��
        //float force = 50 * rigid.velocity.magnitude;
        float force = (0+(0.5f* airDensity * carFrontalArea * dragCoefficient * rigid.velocity.sqrMagnitude)) / tireContactArea;
        rigid.AddForce(-transform.up * force);
        temp = initFrontTireSideFric;
        temp.extremumSlip = initFrontTireSideFric.extremumSlip * (1 + force / carWeigth);
        temp.asymptoteSlip = initFrontTireSideFric.asymptoteSlip * (1 + force / carWeigth);
        wheel[0].sidewaysFriction = temp;
        wheel[1].sidewaysFriction = temp;
    }

    private void Move()
    {
        
    }
}
