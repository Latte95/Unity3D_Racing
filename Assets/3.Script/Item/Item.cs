using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EItem
{
    Banana,
    GreenShell,
    //RedShell,
    //BlueShell,
    //Star,
    GoldMushroom,
    //Reverse,
    Count
}

[System.Serializable]
public class Item
{
    public IItemBehavior behavior;
    public EItem item;

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
    public void UseItem(CharacterControl user)
    {
        GameObject item = ItemManager.Instance.MakeItem(EItem.Banana);
        Vector3 position = user.transform.position + user.transform.up - 3 * user.transform.forward;
        Vector3 eulerRotation = new Vector3(-user.transform.rotation.eulerAngles.x, user.transform.rotation.eulerAngles.y + 180, -user.transform.rotation.eulerAngles.z);
        Quaternion rotation = Quaternion.Euler(eulerRotation);

        item.transform.SetPositionAndRotation(position, rotation);
        item.SetActive(true);
    }
}
public class GoldMushroomBehavior : IItemBehavior
{
    public void UseItem(CharacterControl user)
    {
        user.boostTime = 4;
    }
}
public class GreenShellBehavior : IItemBehavior
{
    public void UseItem(CharacterControl user)
    {
        GameObject item = ItemManager.Instance.MakeItem(EItem.GreenShell);
        Vector3 position = user.transform.position + user.transform.up + 6 * user.transform.forward;
        Quaternion rotation = user.transform.rotation;
        item.transform.SetPositionAndRotation(position, rotation);
        item.SetActive(true);
    }
}

//public class RedShellBehavior : IItemBehavior
//{
//    public void UseItem(CharacterControl user)
//    {
//        GameObject item = ItemManager.Instance.MakeItem(EItem.RedShell);
//        item.SetActive(true);
//        Vector3 position = user.transform.position + user.transform.up + 3 * user.transform.forward;
//        Quaternion rotation = user.transform.rotation;
//        item.transform.SetPositionAndRotation(position, rotation);
//    }
//}
//public class BlueShellBehavior : IItemBehavior
//{
//    public void UseItem(CharacterControl user)
//    {
//        GameObject item = ItemManager.Instance.MakeItem(EItem.BlueShell);
//        item.SetActive(true);
//        Vector3 position = user.transform.position + user.transform.up - 3 * user.transform.forward;
//        Quaternion rotation = user.transform.rotation * item.transform.rotation;
//        item.transform.SetPositionAndRotation(position, rotation);
//    }
//}
public class StarBehavior : IItemBehavior
{
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