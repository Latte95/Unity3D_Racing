using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterControl : MonoBehaviour
{
    [Header("������")]
    public GameObject banana;
    public GameObject greenShell;
    public GameObject redShell;
    public GameObject blueShell;

    public Kart kart;
    public WheelCollider LFTire;
    public WheelCollider RFTire;
    public WheelCollider LRTire;
    public WheelCollider RRTire;

    [Header("������Ʈ")]
    [HideInInspector]
    public Rigidbody rigid;
    [SerializeField][Tooltip("ĳ���� �� �ִϸ�����")]
    protected Animator charAnim;

    //[HideInInspector]
    public float boostTime = 0f;
    // ���� �ӵ� (km/h)
    protected float KPH;
    protected WaitUntil boost_wait;

    public abstract void HandleItem(Item item);

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
        if (boostTime > 0)
        {
            if (kart.boostSpeed > KPH)
            {
                Vector3 boostSpeed = new Vector3(kart.boostForce * rigid.velocity.x, rigid.velocity.y, kart.boostForce * rigid.velocity.z);
                rigid.velocity = boostSpeed;
            }
            boostTime -= Time.deltaTime;
            foreach (ParticleSystem p in kart.boostPar)
            {
                if (!p.isPlaying)
                {
                    p.Play();
                }
            }
        }
        else
        {
            foreach (ParticleSystem p in kart.boostPar)
            {
                if (p.isPlaying)
                {
                    p.Stop();
                }
            }
        }
    }
        
    /// <summary>
    /// �� �ݶ��̴� ĳ��
    /// </summary>
    protected void Init()
    {
        LFTire = kart.axleInfos[0].leftWheel;
        LFTire.transform.SetParent(kart.transform);
        RFTire = kart.axleInfos[0].rightWheel;
        RFTire.transform.SetParent(kart.transform);
        LRTire = kart.axleInfos[1].leftWheel;
        LRTire.transform.SetParent(kart.transform);
        RRTire = kart.axleInfos[1].rightWheel;
        RRTire.transform.SetParent(kart.transform);
    }

    protected void CalculateKPH()
    {
        KPH = rigid.velocity.magnitude * 3.6f;
        if (KPH < 0.9f)
        {
            KPH = 0f;
        }
    }

    /// <summary>
    /// kart ������Ʈ�� �����Ǵ� ���� ��ٸ� �� kart �Ҵ�
    /// </summary>
    /// <returns></returns>
    protected IEnumerator SetKart_co()
    {
        Vector3 center = Vector3.zero;
        int tireNum = 0;
        yield return new WaitUntil(() => transform.GetChild(0).TryGetComponent(out kart));

        Init();
        foreach (GameObject go in kart.wheels_Col_Obj)
        {
            center += go.transform.localPosition;
            tireNum++;
        }
        center /= tireNum;
        rigid.centerOfMass = center + 0.1f * Vector3.down - 0.1f * Vector3.forward;
    }
}
