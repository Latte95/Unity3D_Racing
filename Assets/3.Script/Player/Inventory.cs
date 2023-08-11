using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    // �� ��° �������� ��� �� �� �𸣰� ���ĵ� UI�� ǥ���ؾ��ϱ� ������ List�� ������.
    public List<IItem> items = new List<IItem>();
    private const int ITEM_MAX_NUM = 3;

    // UI���� ������ ������ �߰� ���� �̺�Ʈ
    /// <summary>
    /// �̸����� ������ �ҷ����� ���� string ����
    /// </summary>
    public event Action<string> OnItemAdded;
    /// <summary>
    /// ���� �ʱ�ȭ�� ���� int�� �ε��� ����
    /// </summary>
    public event Action<int> OnItemRemoved;

    public void AddItem(IItem newItem)
    {
        if(items.Count < ITEM_MAX_NUM)
        {
            items.Add(newItem);
            OnItemAdded?.Invoke(newItem.Name.ToString());
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
