using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBox : MonoBehaviour
{
    private readonly float respawnTime = 2.0f;

    private BoxCollider col;
    private Animator anim;

    private WaitForSeconds respawn_wait;
    private readonly int EnlargeHash = Animator.StringToHash("Enlarge");

    public BananaBehavior bananaBehavior = new BananaBehavior();
    public GoldMushroomBehavior goldMushroomBehavior = new GoldMushroomBehavior();
    public GreenShellBehavior greenShellBehavior = new GreenShellBehavior();
    public RedShellBehavior redShellBehavior= new RedShellBehavior();
    public BlueShellBehavior blueShellBehavior = new BlueShellBehavior();
    public StarBehavior starBehavior = new StarBehavior();
    public ReverseBehavior reverseBehavior = new ReverseBehavior();

    private void Awake()
    {
        TryGetComponent(out col);
        TryGetComponent(out anim);
        respawn_wait = new WaitForSeconds(respawnTime);
    }

    private void OnEnable()
    {
        anim.SetBool(EnlargeHash, true);
        col.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        CharacterControl character = other.GetComponent<CharacterControl>();
        if (character != null)
        {
            Deactivate();

            //int random = Random.Range(0, (int)EItem.Count);
            //test
            int random = (int)EItem.Banana;
            IItemBehavior behavior = GetBehaviorForItem((EItem)random);
            Item newItem = new Item((EItem)random, behavior);
            character.HandleItem(newItem);
        }
    }
    private IItemBehavior GetBehaviorForItem(EItem item)
    {
        switch (item)
        {
            case EItem.Banana:
                return bananaBehavior;
            case EItem.GoldMushroom:
                return goldMushroomBehavior;
            case EItem.GreenShell:
                return greenShellBehavior;
            case EItem.RedShell:
                return redShellBehavior;
            case EItem.BlueShell:
                return blueShellBehavior;
            case EItem.Star:
                return starBehavior;
            case EItem.Reverse:
                return reverseBehavior;
            default:
                throw new System.NotImplementedException($"{item} 아이템에 대한 정의가 존재하지 않습니다.");
        }
    }
    private void Deactivate()
    {
        col.enabled = false;
        anim.SetBool(EnlargeHash, false);
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
        StartCoroutine(Respawn_co());
    }

    private IEnumerator Respawn_co()
    {
        yield return respawn_wait;
        col.enabled = true;
        anim.SetBool(EnlargeHash, true);
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
    }
}
