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

    private void RenewSlot()
    {
        slotIcon[0].sprite = null;
        slotIcon[0].color = new Color(1, 1, 1, 0);

        int length = slotIcon.Length;
        for (int i = 1; i < length; i++)
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
            inventory = FindObjectOfType<PlayerControl>().inven;
            yield return null;
        }

        inventory.OnItemAdded += RenewSlot;
        inventory.OnItemRemoved += RenewSlot;
    }
}
