using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
    [SerializeField]
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
            inventory.OnItemAdded -= RenewSlot;
            inventory.OnItemRemoved -= RenewSlot;
        }
    }

    private void RenewSlot(string name)
    {
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

    private void RenewSlot(int index)
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

        inventory.OnItemAdded += RenewSlot;
        inventory.OnItemRemoved += RenewSlot;
    }

    public void OnClick(Image slot)
    {
        for (int i = 0; i < Define.ITEM_MAX_NUM; i++)
        {
            if (slotIcon[i].Equals(slot))
            {
                GameManager.Instance.characters[0].character.GetComponent<PlayerControl>().UseItem(i);
                break;
            }
        }
    }
}
