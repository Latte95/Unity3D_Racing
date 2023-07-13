using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIControl : CharacterControl
{
    public Kart kart;

    [SerializeField]
    private Animator anim;
    [HideInInspector]
    public Rigidbody rigid;

    [SerializeField]
    private WheelCollider LFTire;
    [SerializeField]
    private WheelCollider RFTire;
    [SerializeField]
    private WheelCollider LRTire;
    [SerializeField]
    private WheelCollider RRTire;

    private void Awake()
    {
        TryGetComponent(out anim);
        TryGetComponent(out rigid);

        boost_wait = new WaitUntil(() => boostTime > 0);
        StartCoroutine(Boost_co());
    }

    public override void HandleItem(Item item)
    {
        item.behavior.UseItem(this);
    }

    public override IEnumerator Boost_co()
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
}
