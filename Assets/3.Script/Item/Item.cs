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

public interface IItem
{
    /// <summary>
    /// ������ �̸�
    /// </summary>
    EItem Name { get; }

    /// <summary>
    /// ������ ��� ����
    /// </summary>
    /// <param name="user"></param>
    void UseItem(CharacterControl user);
}

public class BananaBehavior : IItem
{
    public EItem Name { get; private set; }

    public BananaBehavior()
    {
        Name = EItem.Banana;
    }

    public void UseItem(CharacterControl user)
    {
        // ĳ���� �ڿ� ������ ����
        GameObject item = ItemManager.Instance.MakeItem(EItem.Banana);
        Vector3 position = user.transform.position + user.transform.up - 3 * user.transform.forward;
        Vector3 eulerRotation = new Vector3(-user.transform.rotation.eulerAngles.x, user.transform.rotation.eulerAngles.y + 180, -user.transform.rotation.eulerAngles.z);
        Quaternion rotation = Quaternion.Euler(eulerRotation);

        item.transform.SetPositionAndRotation(position, rotation);
        item.SetActive(true);
    }
}
public class GoldMushroomBehavior : IItem
{
    public EItem Name { get; private set; }

    public GoldMushroomBehavior()
    {
        Name = EItem.GoldMushroom;
    }

    public void UseItem(CharacterControl user)
    {
        // ĳ���� �ν��� Ÿ�� ����
        user.boostTime = 4;
    }
}
public class GreenShellBehavior : IItem
{
    public EItem Name { get; private set; }

    public GreenShellBehavior()
    {
        Name = EItem.GreenShell;
    }

    public void UseItem(CharacterControl user)
    {
        // ĳ���� �տ� ������ ����
        GameObject item = ItemManager.Instance.MakeItem(EItem.GreenShell);
        Vector3 position = user.transform.position + user.transform.up + 6 * user.transform.forward;
        Quaternion rotation = user.transform.rotation;
        item.transform.SetPositionAndRotation(position, rotation);
        item.SetActive(true);
    }
}