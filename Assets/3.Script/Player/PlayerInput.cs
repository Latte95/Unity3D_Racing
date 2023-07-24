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
    public bool resetPosition;

    [Header("Mouse Cursor Settings")]
    public bool cursorLocked = true;
    public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM
    public void OnMove(InputValue value)
    {
        move = value.Get<Vector2>();
        if (move.x > 0.45f)
        {
            move.x = 1;
        }
        else if (move.x < -0.45f)
        {
            move.x = -1;
        }
        else
        {
            move.x = 0;
        }

        if (move.y > 0)
        {
            move.y = 1;
        }
        else if (move.y < 0)
        {
            move.y = -1;
        }
    }

    public void OnDrift(InputValue value)
    {
        drift = value.isPressed;
    }

    public void OnUseItem(InputValue value)
    {
        useItem = value.isPressed;
    }

    public void OnChangeItem()
    {

    }

    public void OnResetPosition(InputValue value)
    {
        resetPosition = value.isPressed;
    }
#endif

    public void MoveInput(Vector2 newMoveDirection)
    {
        move = newMoveDirection;
    }
}
