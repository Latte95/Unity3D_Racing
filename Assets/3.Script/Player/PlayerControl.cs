using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControl : MonoBehaviour
{
    [Header("Player")]
    public Kart kart;
    private float targetSpeed = 0;

    [Header("State")]
    private PlayerState currentStarategy = new NormalMoveState();
    public bool isBoost { get; private set; }

    [Header("Components")]
    private Animator anim;
    private Rigidbody rigid;

<<<<<<< Updated upstream
    public PlayerInput input;
=======
    private WheelFrictionCurve tempFric;

    [HideInInspector]
    public PlayerInputs input;
>>>>>>> Stashed changes

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
        rigid.centerOfMass = 0.3f * Vector3.down;
    }

    private void FixedUpdate()
    {
        Move();
<<<<<<< Updated upstream
        //Drift();
        WheelPos();
        AddDownForce();
=======
        Curve();
        DownForce();
        AirBorne();
        if (input.move.y <= 0 && boostTime > 0)
        {
            boostTime = 0;
        }
        base.FixedUpdate();
    }
    public void UseItem()
    {
        // PC
        if (input.useItem)
        {
            input.useItem = false;
            if (inven.items.Count > 0)
            {
                inven.items[0].behavior.UseItem(this);
                inven.RemoveItem();
            }
        }
    }
    public void UseItem(int index)
    {
        // �����
        if (inven.items.Count > index)
        {
            if (inven.items[index] != null)
            {
                inven.items[index].behavior.UseItem(this);
                inven.RemoveItem(index);
            }
        }
>>>>>>> Stashed changes
    }

    #region �̵�
    public float radius = 6;
    private void Move()
    {
        // �ӵ� ���
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
                for (int i = 0; i < 2; i++)
                {
                    axleInfo.leftWheel[i].steerAngle = Mathf.Rad2Deg * Mathf.Atan(kart.wheelBase / (radius + halfVehicleWidth * input.move.x)) * steering;
                    axleInfo.rightWheel[i].steerAngle = Mathf.Rad2Deg * Mathf.Atan(kart.wheelBase / (radius - halfVehicleWidth * input.move.x)) * steering;
                }
            }
            // ���� ����
            if (axleInfo.motor)
            {
                axleInfo.leftWheel[0].motorTorque = targetSpeed;
                axleInfo.leftWheel[1].motorTorque = targetSpeed;
                axleInfo.rightWheel[0].motorTorque = targetSpeed;
                axleInfo.rightWheel[1].motorTorque = targetSpeed;
            }
            Debug.Log(axleInfo.leftWheel[1].rpm);
            Debug.Log(axleInfo.rightWheel[1].rpm);
        }
        Debug.Log($"{rigid.velocity.magnitude * 3.6f} km/h" );
    }
    /// <summary>
    /// ���� �̹����� �� �ݶ��̴� ��ġ�� ����ȭ ��Ű�� �޼ҵ�
    /// </summary>
    private void WheelPos()
    {
        Vector3 wheelPosition;
        Quaternion wheelRotation;

        for (int i = 0; i < 4; i++)
        {
            Vector3 temp1, temp2;
            kart.wheels_Col[i * 2].GetWorldPose(out temp1, out wheelRotation);
            kart.wheels_Col[i * 2 + 1].GetWorldPose(out temp2, out wheelRotation);
            wheelPosition = (temp1 + temp2) * 0.5f;
            kart.wheels_Mesh[i].transform.position = wheelPosition;
            kart.wheels_Mesh[i].transform.rotation = wheelRotation;
        }
    }
    private void Drift()
    {
        for (int i = 0; i < 2; i++)
        {
            if (input.drift)
            {
                kart.axleInfos[1].leftWheel[i].forwardFriction = kart.driftFriction_f;
                kart.axleInfos[1].leftWheel[i].sidewaysFriction = kart.driftFriction_s;
                kart.axleInfos[1].rightWheel[i].forwardFriction = kart.driftFriction_f;
                kart.axleInfos[1].rightWheel[i].sidewaysFriction = kart.driftFriction_s;
            }
            else
            {
                kart.axleInfos[1].leftWheel[i].forwardFriction = kart.nomalFriction_f;
                kart.axleInfos[1].leftWheel[i].sidewaysFriction = kart.nomalFriction_s;
                kart.axleInfos[1].rightWheel[i].forwardFriction = kart.nomalFriction_f;
                kart.axleInfos[1].rightWheel[i].sidewaysFriction = kart.nomalFriction_s;
            }
        }
    }
    #endregion �̵�

    #region ���� ����ȭ
    public float downForce = 100.0f;

    private void AddDownForce()
    {
        rigid.AddForce(-transform.up * downForce * rigid.velocity.magnitude);
    }
<<<<<<< Updated upstream
    #endregion
}
=======
    #endregion ���

    public void SetState(PlayerState state)
    {
        currentState = state;
        currentState.SetFriction(this);
    }

    #region �ִϸ��̼�
    /// <summary>
    /// �÷��̾� ĳ���� �ִϸ��̼� ����
    /// </summary>
    float curveBlend;
    private void SetAnimation()
    {
        // Ŀ��
        if (input.move.y >= 0)
        {
            charAnim.SetFloat(CurveHash, curveBlend);
        }
        // ���� �� ����� x
        else
        {
            charAnim.SetBool(TurnRightHash, false);
            charAnim.SetBool(TurnLeftHash, false);
        }

        // ����
        // �̵� ����� �ٶ󺸴� ������ ������ 90�� �̻�(����)�̸� ����, ����(����)�� ���
        float dotProduct = Vector3.Dot(rigid.velocity.normalized, transform.forward.normalized);
        // �Ĺ� �ٶ󺸴� �ִϸ��̼��� mario�ۿ� ��� ����
    }
    #endregion �ִϸ��̼�

    #region ���� �� �ʱ� ������
    public int myIndex;
    public string myName;
    [PunRPC]
    public void SetMyIndex(int index, string name)
    {
        myIndex = index;
        myName = name;
    }
    private new void Init()
    {
        gameManager = GameManager.Instance;

        GameObject kartPrefab = Resources.Load<GameObject>("Kart/" + gameManager.kartName[myIndex]);
        if (kartPrefab != null)
        {
            // īƮ ����
            GameObject kartInstance = Instantiate(kartPrefab, transform);
            kartInstance.name = "Kart";
            kartInstance.transform.SetSiblingIndex(0);
            kartInstance.TryGetComponent(out kart);

            // ĳ���� ����
            GameObject characterPrefab = Resources.Load<GameObject>("Character/" + gameManager.charName[myIndex]);
            if (characterPrefab != null)
            {
                GameObject characterInstance = Instantiate(characterPrefab, kartInstance.transform);
                characterInstance.name = gameManager.charName[0];
                characterInstance.transform.SetSiblingIndex(1);
                characterInstance.TryGetComponent(out charAnim);
                gameObject.GetComponent<Minimap>().icon = characterInstance.transform.Find("MyIcon").GetComponent<Image>();
            }
        }
        base.Init();

        // ������ �����ϱ� �������� ���ο� �������� �̿��� �������� ������
        rigid.constraints = ~(RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX);
    }
    #endregion �ʱ� ����

    public override void HandleItem(Item item)
    {
        inven.AddItem(item);
    }

    public override void LapIncrease()
    {
        currentLapCount++;
        if (currentLapCount <= gameManager.totalLap)
        {
            currentLap_txt.text = currentLapCount.ToString();
        }
        else
        {
            currentLap_txt.text = gameManager.totalLap.ToString();
        }
        timeManager.SetBestTime();
    }
}
>>>>>>> Stashed changes
