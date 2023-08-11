using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBox : MonoBehaviour
{
    private readonly float respawnTime = 2.0f;

    private BoxCollider col;
    private Animator anim;

    // ĳ��
    private WaitForSeconds respawn_wait;
    private readonly int EnlargeHash = Animator.StringToHash("Enlarge");

    public BananaBehavior bananaBehavior = new BananaBehavior();
    public GoldMushroomBehavior goldMushroomBehavior = new GoldMushroomBehavior();
    public GreenShellBehavior greenShellBehavior = new GreenShellBehavior();

    private void Awake()
    {
        TryGetComponent(out col);
        TryGetComponent(out anim);
        respawn_wait = new WaitForSeconds(respawnTime);
    }

    private void OnEnable()
    {
        // ȸ�� �ִϸ��̼�
        anim.SetBool(EnlargeHash, true);
        col.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        CharacterControl character;
        if (other.TryGetComponent(out character))
        {
            Deactivate();

            IItem newItem = CreateRandomItem();
            character.HandleItem(newItem);
        }
    }
    private IItem CreateRandomItem()
    {
        int random = Random.Range(0, (int)EItem.Count);

        return GetBehaviorForItem((EItem)random);
    }
    private IItem GetBehaviorForItem(EItem item)
    {
        switch (item)
        {
            case EItem.Banana:
                return bananaBehavior;
            case EItem.GoldMushroom:
                return goldMushroomBehavior;
            case EItem.GreenShell:
                return greenShellBehavior;
            default:
                throw new System.NotImplementedException($"{item} �����ۿ� ���� ���ǰ� �������� �ʽ��ϴ�.");
        }
    }
    /// <summary>
    /// �������� ���� �浹�� �̹����� ���� ������Ʈ�� Ȱ��ȭ ���·� ����
    /// </summary>
    private void Deactivate()
    {
        col.enabled = false;
        // �ڽ� ������Ʈ���� �Ž��� ������ �ֱ� ������ ��Ȱ��ȭ
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
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
    }
}
