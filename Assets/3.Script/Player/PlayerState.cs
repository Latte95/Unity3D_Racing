using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class PlayerState
{
    protected float targetSpeed = 0;
    protected int direction;
    protected Quaternion rotate = Quaternion.Euler(0, 20, 0);

    /// <summary>
    /// �̵� ���� ���� (��� : 1 ��ȯ, ������ : -1 ��ȯ)
    /// </summary>
    /// <returns></returns>
    public abstract float CurveDirection();
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
    public override float CurveDirection()
    {
        return 1;
    }
}

public class CantMoveState : PlayerState
{
    WheelFrictionCurve targetFriction = new WheelFrictionCurve();

    public override float CurveDirection()
    {
        return 1;
    }

    public override void SetFriction(PlayerControl player)
    {
        List<AxleInfo> axleInfos = player.kart.axleInfos;
        targetFriction.stiffness = 0;

        // ��� Ÿ�̾� ������ 0���� �Ͽ� ������ ���ư����� �̵� �Ұ����ϰ� ����
        foreach (AxleInfo axle in axleInfos)
        {
            axle.leftWheel.forwardFriction = targetFriction;
            axle.leftWheel.sidewaysFriction = targetFriction;
            axle.rightWheel.forwardFriction = targetFriction;
            axle.rightWheel.sidewaysFriction = targetFriction;
        }
    }
}

public class ReverseState : PlayerState
{
    public override float CurveDirection()
    {
        return -1;
    }
}
