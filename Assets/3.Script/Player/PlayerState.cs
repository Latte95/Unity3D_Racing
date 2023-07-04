using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class PlayerState
{
    protected float targetSpeed = 0;
    protected int direction;
    protected Quaternion rotate = Quaternion.Euler(0, 20, 0);

    public abstract float Curve();
    /// <summary>
    /// Ÿ�̾��� ������ ���� �̵� ���� ���� ����
    /// </summary>
    /// <param name="player"></param>
    public virtual void SetFriction(PlayerControl player)
    {
        Kart kart = player.kart;

        kart.axleInfos[0].leftWheel.forwardFriction = kart.initForwardTireForwardFric;
        kart.axleInfos[0].leftWheel.sidewaysFriction = kart.initForwardTireSideFric;
        kart.axleInfos[0].rightWheel.forwardFriction = kart.initForwardTireForwardFric;
        kart.axleInfos[0].rightWheel.sidewaysFriction = kart.initForwardTireSideFric;
        kart.axleInfos[1].leftWheel.forwardFriction = kart.initRearTireForwardFric;
        kart.axleInfos[1].leftWheel.sidewaysFriction = kart.initRearTireSideFric;
        kart.axleInfos[1].rightWheel.forwardFriction = kart.initRearTireForwardFric;
        kart.axleInfos[1].rightWheel.sidewaysFriction = kart.initRearTireSideFric;
    }
}

public class NormalState : PlayerState
{
    public override float Curve()
    {
        return 1;
    }
}

public class CantMoveState : PlayerState
{
    WheelFrictionCurve friction = new WheelFrictionCurve();

    public override float Curve()
    {
        return 1;
    }

    public override void SetFriction(PlayerControl player)
    {
        Kart kart = player.kart;
        friction.stiffness = 0;

        foreach (AxleInfo a in kart.axleInfos)
        {
            a.leftWheel.forwardFriction = friction;
            a.leftWheel.sidewaysFriction = friction;
            a.rightWheel.forwardFriction = friction;
            a.rightWheel.sidewaysFriction = friction;
        }
    }
}

public class ReverseState : PlayerState
{
    public override float Curve()
    {
        return -1;
    }
}
