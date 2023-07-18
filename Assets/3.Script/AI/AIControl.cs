using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIControl : CharacterControl
{
    public override void HandleItem(Item item)
    {
        item.behavior.UseItem(this);
    }

    public override void LapIncrease()
    {
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Path"))
        {
            // 지나간 경로 저장
            int pathIndex = int.Parse(other.gameObject.name.Replace("Path", "")) - 1;
            GameManager.Instance.SetPathCheck(this.gameObject, pathIndex);
        }
    }
}
