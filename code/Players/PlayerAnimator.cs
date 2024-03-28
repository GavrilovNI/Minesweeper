using Sandbox;
using Sandbox.Citizen;

namespace Minesweeper.Players;

public class PlayerAnimator : Component
{
    [Property, RequireComponent] protected CitizenAnimationHelper AnimationHelper { get; set; } = null!;
    [Property] protected CharacterController CharacterController { get; set; } = null!;
    [Property] protected PlayerController PlayerController { get; set; } = null!;
    [Property] protected CameraController CameraController { get; set; } = null!;
    [Property] protected GameObject Body { get; set; } = null!;

    protected GameObject PlayerObject => PlayerController.GameObject;

    [Property] protected float MaxLookRotationAngle { get; set; } = 130f;

    [Property] protected float MinVelocityToRotateBody { get; set; } = 10f;
    [Property] protected float BodyRotationSpeed { get; set; } = 5f;


    protected override void OnUpdate()
    {
        HandleJumping();
        Animate();
        RotateBody();
    }

    protected virtual void HandleJumping()
    {
        if(PlayerController.IsJumped)
            AnimationHelper.TriggerJump();
    }

    protected virtual Vector3 GetLookDirection()
    {
        var lookRotation = Rotation.LookAt(CameraController.EyeAngles.Forward, PlayerObject.Transform.Rotation.Up);
        var angle = Body.Transform.Rotation.Forward.Angle(lookRotation.Forward);
        if(angle > MaxLookRotationAngle)
            lookRotation = Rotation.Slerp(Body.Transform.Rotation, lookRotation, MaxLookRotationAngle / angle);
        return lookRotation.Forward;

    }

    protected virtual void Animate()
    {
        var lookDirection = GetLookDirection();

        AnimationHelper.WithVelocity(CharacterController.Velocity);
        AnimationHelper.WithWishVelocity(PlayerController.WishVelocity);
        AnimationHelper.IsGrounded = CharacterController.IsOnGround;
        AnimationHelper.WithLook(lookDirection);
        AnimationHelper.MoveStyle = PlayerController.IsRunning ? CitizenAnimationHelper.MoveStyles.Run : CitizenAnimationHelper.MoveStyles.Walk;
    }

    protected virtual void RotateBody()
    {
        var targetAngle = new Angles(0, CameraController.EyeAngles.yaw, 0).ToRotation();
        var velocity = CharacterController.Velocity.WithZ(0);

        if(velocity.Length > MinVelocityToRotateBody)
        {
            targetAngle = Rotation.LookAt(velocity, Vector3.Up);
            Body.Transform.Rotation = Rotation.Lerp(Body.Transform.Rotation, targetAngle, Time.Delta * BodyRotationSpeed);
        }

        var rotateDifference = Body.Transform.Rotation.Distance(targetAngle);
        AnimationHelper.FootShuffle = rotateDifference;
    }
}
