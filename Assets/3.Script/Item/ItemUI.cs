using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
    private Inventory inventory;
    [SerializeField]
    private Image[] slotIcon;

    private void OnEnable()
    {
        StartCoroutine(EventRegist_co());
    }
    private void OnDisable()
    {
        if (inventory != null)
        {
            inventory.OnItemAdded -= AddSlot;
            inventory.OnItemRemoved -= RemoveSlot;
        }
    }

    private void AddSlot(string name)
    {
        // 빈 슬롯 찾아서 해당 슬롯에 아이콘 할당
        foreach (Image img in slotIcon)
        {
            if (img.sprite == null)
            {
                img.sprite = Resources.Load<Sprite>("Item/" + name);
                img.color = Color.white;
                break;
            }
        }
    }

    private void RemoveSlot(int index)
    {
        slotIcon[index].sprite = null;
        slotIcon[index].color = new Color(1, 1, 1, 0);

        int length = slotIcon.Length;
        for (int i = index+1; i < length; i++)
        {
            slotIcon[i - 1].sprite = slotIcon[i].sprite;
            if (slotIcon[i].sprite != null)
            {
                slotIcon[i - 1].color = Color.white;
            }
            else
            {
                slotIcon[i - 1].color = Color.clear;
            }
            slotIcon[i].sprite = null;
            slotIcon[i].color = Color.clear;
        }
    }

    /// <summary>
    /// 인벤토리 할당 후 이벤트 등록
    /// </summary>
    /// <returns></returns>
    private IEnumerator EventRegist_co()
    {
        while (inventory == null)
        {
            if (GameObject.Find("Player"))
            {
                inventory = GameObject.Find("Player").GetComponent<PlayerControl>().inven;
            }
            yield return null;
        }

        inventory.OnItemAdded += AddSlot;
        inventory.OnItemRemoved += RemoveSlot;
    }

    /// <summary>
    /// 모바일 환경에서 UI 클릭시 클릭 된 아이템 사용
    /// </summary>
    /// <param name="slot"></param>
    public void OnClick(Image slot)
    {
        for (int i = 0; i < Define.ITEM_MAX_NUM; i++)
        {
            if (slotIcon[i].Equals(slot))
            {
                GameManager.Instance.characters[0].character_object.GetComponent<PlayerControl>().UseItem(i);
                break;
            }
        }
    }
}
