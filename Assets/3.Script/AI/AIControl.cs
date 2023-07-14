using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIControl : CharacterControl
{
    public override void HandleItem(Item item)
    {
        item.behavior.UseItem(this);
    }
}
