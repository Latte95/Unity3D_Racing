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
    /// 이동 방향 결정 (평소 : 1 반환, 리버스 : -1 반환)
    /// </summary>
    /// <returns></returns>
    public abstract float CurveDirection();
    /// <summary>
    /// 타이어의 마찰을 통해 이동 가능 여부 설정
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

        // 모든 타이어 마찰을 0으로 하여 바퀴는 돌아가지만 이동 불가능하게 만듦
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
