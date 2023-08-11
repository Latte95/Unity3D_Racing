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
    /// 아이템 이름
    /// </summary>
    EItem Name { get; }

    /// <summary>
    /// 아이템 사용 로직
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
        // 캐릭터 뒤에 아이템 스폰
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
        // 캐릭터 부스터 타임 설정
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
        // 캐릭터 앞에 아이템 스폰
        GameObject item = ItemManager.Instance.MakeItem(EItem.GreenShell);
        Vector3 position = user.transform.position + user.transform.up + 6 * user.transform.forward;
        Quaternion rotation = user.transform.rotation;
        item.transform.SetPositionAndRotation(position, rotation);
        item.SetActive(true);
    }
}