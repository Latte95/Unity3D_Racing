using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerState
{
    protected int direction;
    protected Quaternion rotate = Quaternion.Euler(0, 20, 0);
    public abstract void HandleDirection();
    public virtual void Move(PlayerControl player)
    {

    }
}

public class NormalMoveState : PlayerState
{
    public override void HandleDirection()
    {
        throw new System.NotImplementedException();
    }

    public override void Move(PlayerControl player)
    {
        throw new System.NotImplementedException();
    }
}

public class CantMoveState : PlayerState
{
    public override void HandleDirection()
    {
        throw new System.NotImplementedException();
    }

    public override void Move(PlayerControl player)
    {
        throw new System.NotImplementedException();
    }
}

public class ReverseMoveState : PlayerState
{
    public override void HandleDirection()
    {
        throw new System.NotImplementedException();
    }

    public override void Move(PlayerControl player)
    {
        throw new System.NotImplementedException();
    }
}
