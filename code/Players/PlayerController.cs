using Minesweeper.Mth;
using Sandbox;
using Sandbox.Citizen;
using System;
using System.Linq;

namespace Minesweeper.Players;

public class PlayerController : Component
{
    [Property] protected CharacterController CharacterController { get; set; } = null!;
    [Property] protected GameObject CameraTransform { get; set; } = null!;
    [Property] protected float WalkSpeed { get; set; } = 160f;
    [Property] protected float RunSpeed { get; set; } = 270f;
    [Property] protected float JumpVelocity { get; set; } = 320f;
    [Property] protected float GroundFriction { get; set; } = 4f;
    [Property] protected float AirFriction { get; set; } = 0.1f;
    [Property] protected float StopSpeed { get; set; } = 140f;
    [Property] protected float AirWishVelocityClamp { get; set; } = 50f;

    public virtual Vector3 Gravity => Scene.PhysicsWorld.Gravity;
    public Vector3 GravityNormal
    {
        get
        {
            var gravity = Gravity;
            var result = gravity.Normal;
            if(result.AlmostEqual(0))
                return Vector3.Down;
            return result;
        }
    }

    [Sync] public bool IsRunning { get; protected set; }
    [Sync] public bool IsJumped { get; protected set; }

    [Sync] public Vector3 WishVelocity { get; protected set; }

    protected override void OnAwake()
    {
        CharacterController ??= Components.Get<CharacterController>();
    }

    protected override void OnUpdate()
    {
        if(IsProxy)
            return;

        UpdateInputs();
        WishVelocity = CalculateWishVelocity();
    }

    protected override void OnFixedUpdate()
    {
        if(IsProxy)
            return;

        Move();
    }

    protected virtual void Move()
    {
        var halfGravityVelocity = Gravity * (Time.Delta * 0.5f);
        var gravityNormal = GravityNormal;

        if(CharacterController.IsOnGround && Input.Pressed("Jump"))
            Jump();

        if(CharacterController.IsOnGround)
        {
            CharacterController.Velocity = CharacterController.Velocity.ProjectOnPlane(gravityNormal);
            CharacterController.Accelerate(WishVelocity);
            CharacterController.ApplyFriction(GroundFriction, StopSpeed);
        }
        else
        {
            CharacterController.Velocity += halfGravityVelocity;
            CharacterController.Accelerate(WishVelocity.ClampLength(AirWishVelocityClamp));
            CharacterController.ApplyFriction(AirFriction, StopSpeed);
        }

        CharacterController.Move();

        if(CharacterController.IsOnGround)
            CharacterController.Velocity = CharacterController.Velocity.ProjectOnPlane(gravityNormal);
        else
            CharacterController.Velocity += halfGravityVelocity;
    }

    protected virtual void Jump()
    {
        CharacterController.Punch(-GravityNormal * JumpVelocity);
    }

    protected virtual float GetWishSpeed(Vector3 input) => IsRunning ? RunSpeed : WalkSpeed;

    protected virtual void UpdateInputs()
    {
        IsRunning = Input.Down("Run");
        IsJumped = Input.Pressed("Jump");
    }

    protected virtual Vector3 CalculateWishVelocity()
    {
        var input = Input.AnalogMove.WithZ(0);
        Vector3 result = input * CameraTransform.Transform.Rotation;
        result = result.ProjectOnPlane(GravityNormal).Normal;
        result *= GetWishSpeed(input);
        return result;
    }
}