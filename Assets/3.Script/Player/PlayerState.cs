using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class PlayerState
{
    protected float targetSpeed = 0;
    protected int direction;
    protected Quaternion rotate = Quaternion.Euler(0, 20, 0);
    public abstract void HandleDirection();
    public virtual void Move(PlayerControl player)
    {
        // ¼Óµµ °è»ê
        //if (Mathf.Abs(targetSpeed) < player.MaxSpeed)
        //{
        //    targetSpeed += player.Speed * Time.deltaTime * player.input.move.y;
        //}
        //if (player.input.move.Equals(Vector2.zero))
        //{
        //    targetSpeed = 0;
        //}
        //else if (player.isBoost)
        //{
        //    targetSpeed *= boostSpeed;
        //}


        //float steering = steerRotate * input.move.x;
        //float halfVehicleWidth = vehicleWidth * 0.5f;

        //foreach (AxleInfo axleInfo in axleInfos)
        //{
        //    // ¹ÙÄû È¸Àü
        //    if (axleInfo.steering)
        //    {
        //        for (int i = 0; i < 2; i++)
        //        {
        //            axleInfo.leftWheel[i].steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (radius + halfVehicleWidth * input.move.x)) * steering;
        //            axleInfo.rightWheel[i].steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (radius - halfVehicleWidth * input.move.x)) * steering;
        //        }
        //    }
        //    // ¹ÙÄû ±¼¸²
        //    if (axleInfo.motor)
        //    {
        //        axleInfo.leftWheel[0].motorTorque = targetSpeed;
        //        axleInfo.leftWheel[1].motorTorque = targetSpeed;
        //        axleInfo.rightWheel[0].motorTorque = targetSpeed;
        //        axleInfo.rightWheel[1].motorTorque = targetSpeed;
        //    }
        //}
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
