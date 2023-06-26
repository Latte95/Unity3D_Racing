using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [Header("Character Input Values")]
    public Vector2 move;
    public bool drift;
    public bool useItem;
    public bool changeItem;

    [Header("Mouse Cursor Settings")]
    public bool cursorLocked = true;
    public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM
    public void OnMove(InputValue value)
    {
        move = value.Get<Vector2>();
    }

    public void OnDrift(InputValue value)
    {
        drift = value.isPressed; ;
    }

    public void OnUseItem()
    {
        
    }

    public void OnChangeItem()
    {
        
    }
#endif
}
