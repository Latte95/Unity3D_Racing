using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EItem
{
    Banana,
    GreenShell,
    RedShell,
    BlueShell,
    Star,
    GoldMushroom,
    Reverse,
    Count
}

[System.Serializable]
public class Item
{
    public EItem item;
    public IItemBehavior behavior;

    public Item(EItem item, IItemBehavior behavior)
    {
        this.item = item;
        this.behavior = behavior;
    }
}

public interface IItemBehavior
{
    void UseItem(CharacterControl user);
}

public class BananaBehavior : IItemBehavior
{
    public GameObject bananaPrefab;

    public void UseItem(CharacterControl user)
    {
        user.banana.SetActive(true);

        Vector3 position = user.transform.position + user.transform.up - 3 * user.transform.forward;
        Quaternion rotation = user.transform.rotation * user.banana.transform.rotation;
        user.banana.transform.SetPositionAndRotation(position, rotation);
    }
}

public class GoldMushroomBehavior : IItemBehavior
{
    public void UseItem(CharacterControl user)
    {
        user.boostTime = 3;
    }
}


public class GreenShellBehavior : IItemBehavior
{
    //public GameObject ;

    public void UseItem(CharacterControl user)
    {

    }
}
public class RedShellBehavior : IItemBehavior
{
    //public GameObject ;

    public void UseItem(CharacterControl user)
    {

    }
}
public class BlueShellBehavior : IItemBehavior
{
    //public GameObject ;

    public void UseItem(CharacterControl user)
    {

    }
}
public class StarBehavior : IItemBehavior
{
    //public GameObject ;

    public void UseItem(CharacterControl user)
    {

    }
}
public class ReverseBehavior : IItemBehavior
{
    //public GameObject ;

    public void UseItem(CharacterControl user)
    {

    }
}

/*
    private IEnumerator Banana_co(CharacterControl character)
    {
        Debug.Log(1);
        //WheelFrictionCurve a = character.LFTire.forwardFriction;
        //WheelFrictionCurve b = character.RFTire.forwardFriction;
        //WheelFrictionCurve c = character.LRTire.forwardFriction;
        //WheelFrictionCurve d = character.RRTire.forwardFriction;
        //character.LFTire.forwardFriction = new WheelFrictionCurve();
        //character.RFTire.forwardFriction = new WheelFrictionCurve();
        //character.LRTire.forwardFriction = new WheelFrictionCurve();
        //character.RRTire.forwardFriction = new WheelFrictionCurve();

        yield return new WaitForSeconds(3);
        Debug.Log(2);
        //character.LFTire.forwardFriction = a;
        //character.RFTire.forwardFriction = b;
        //character.LRTire.forwardFriction = c;
        //character.RRTire.forwardFriction = d;
    }
*/