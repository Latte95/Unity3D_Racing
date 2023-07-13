using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterControl : MonoBehaviour
{
    [Header("æ∆¿Ã≈€")]
    public GameObject banana;
    public GameObject greenShell;
    public GameObject redShell;
    public GameObject blueShell;

    public float boostTime = 0f;
    protected WaitUntil boost_wait;

    public abstract void HandleItem(Item item);
    public abstract IEnumerator Boost_co();
}
