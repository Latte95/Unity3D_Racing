using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterControl : MonoBehaviour
{
    [Header("아이템")]
    public GameObject banana;
    public GameObject greenShell;
    public GameObject redShell;
    public GameObject blueShell;

    public Kart kart;
    protected WheelCollider LFTire;
    protected WheelCollider RFTire;
    protected WheelCollider LRTire;
    protected WheelCollider RRTire;

    [Header("컴포넌트")]
    protected Rigidbody rigid;
    [SerializeField][Tooltip("캐릭터 모델 애니메이터")]
    protected Animator anim;

    [HideInInspector]
    public float boostTime = 0f;
    // 현재 속도 (km/h)
    protected float KPH;
    protected WaitUntil boost_wait;

    protected void Awake()
    {
        TryGetComponent(out rigid);

        boost_wait = new WaitUntil(() => boostTime > 0);
        StartCoroutine(SetKart_co());
    }
    protected void Update()
    {
        CalculateKPH();
    }
    protected void FixedUpdate()
    {
        if(boostTime>0 && kart.boostSpeed > KPH)
        {
            Vector3 boostSpeed = new Vector3(kart.boostForce * rigid.velocity.x, rigid.velocity.y, kart.boostForce * rigid.velocity.z);
            rigid.velocity = boostSpeed;
        }
    }
    protected void CalculateKPH()
    {
        KPH = rigid.velocity.magnitude * 3.6f;
        if (KPH < 0.9f)
        {
            KPH = 0f;
        }
    }

    public abstract void HandleItem(Item item);
    public IEnumerator Boost_co()
    {
        while (true)
        {
            yield return boost_wait;
            boostTime -= Time.deltaTime;
            if (boostTime < 0)
            {
                boostTime = 0;
            }

            Quaternion rotation = Quaternion.Euler(0, LFTire.steerAngle, 0);
            Vector3 direction = rotation * -LFTire.transform.up;
            rigid.AddForce(kart.boostForce * direction, ForceMode.Impulse);
        }
    }
    /// <summary>
    /// 휠 콜라이더 캐싱
    /// </summary>
    protected void Init()
    {
        LFTire = kart.axleInfos[0].leftWheel;
        RFTire = kart.axleInfos[0].rightWheel;
        LRTire = kart.axleInfos[1].leftWheel;
        RRTire = kart.axleInfos[1].rightWheel;
    }

    /// <summary>
    /// kart 오브젝트가 생성되는 것을 기다린 뒤 kart 할당
    /// </summary>
    /// <returns></returns>
    protected IEnumerator SetKart_co()
    {
        Vector3 center = Vector3.zero;
        int tireNum = 0;
        yield return new WaitUntil(() => transform.GetChild(0).TryGetComponent(out kart));

        Init();
        //StartCoroutine(Boost_co());
        foreach (GameObject go in kart.wheels_Col_Obj)
        {
            center += go.transform.localPosition;
            tireNum++;
        }
        center /= tireNum;
        rigid.centerOfMass = center + 0.1f * Vector3.down - 0.1f * Vector3.forward;
    }
}
