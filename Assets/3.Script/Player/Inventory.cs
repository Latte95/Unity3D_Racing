using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    // 몇 번째 아이템이 사용 될 지 모르고 정렬된 UI를 표시해야하기 때문에 List로 관리함.
    public List<IItem> items = new List<IItem>();
    private const int ITEM_MAX_NUM = 3;

    // UI에서 구독할 아이템 추가 제거 이벤트
    /// <summary>
    /// 이름으로 아이콘 불러오기 위해 string 전달
    /// </summary>
    public event Action<string> OnItemAdded;
    /// <summary>
    /// 슬롯 초기화를 위해 int로 인덱스 전달
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
