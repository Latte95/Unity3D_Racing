using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<Item> items = new List<Item>();
    private const int ITEM_MAX_NUM = 3;

    public event Action<string> OnItemAdded;
    public event Action<int> OnItemRemoved;

    public void AddItem(Item newItem)
    {
        if(items.Count < ITEM_MAX_NUM)
        {
            items.Add(newItem);
            OnItemAdded?.Invoke(newItem.item.ToString());
        }
    }

    public void RemoveItem(int index = 0)
    {
        if (items.Count > index)
        {
            items.RemoveAt(index);
            OnItemRemoved?.Invoke(index);
        }
    }
}
