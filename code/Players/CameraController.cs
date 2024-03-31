using Sandbox;
using System;

namespace Minesweeper.Players;

public class CameraController : Component
{
    [Property] protected GameObject CameraTransform { get; set; } = null!;

    private float _distanceToPivot = 200;

    [Property, Sync] protected float DistanceToPivot
    {
        get => _distanceToPivot;
        set
        {
            _distanceToPivot = value;
            if(CameraTransform.IsValid())
                CameraTransform.Transform.LocalPosition = CameraTransform.Transform.LocalPosition.WithX(-value);
        }
    }

    [Property] protected float MinDistanceToPivot { get; set; } = 100;
    [Property] protected float MaxDistanceToPivot { get; set; } = 400;
    [Property] protected float DistanceToPivotChangingSensitivity { get; set; } = 25;

    [Property] protected GameObject Player { get; set; } = null!;
    [Property] protected float BackTraceRadius { get; set; } = 8f;
    [Property] protected TagSet BackTraceTags { get; set; } = null!;
    [Property] protected TagSet BackTraceIgnoreTags { get; set; } = null!;

    [Sync] public Angles EyeAngles { get; set; }

    protected override void OnUpdate()
    {
        if(IsProxy)
            return;

        var distanceToPivotOffset = -Input.MouseWheel.y * DistanceToPivotChangingSensitivity;
        var newDistanceToPivot = DistanceToPivot + distanceToPivotOffset;
        newDistanceToPivot = Math.Clamp(newDistanceToPivot, MinDistanceToPivot, MaxDistanceToPivot);
        DistanceToPivot = newDistanceToPivot;

        if(Input.Down("RotateCamera"))
        {
            var angles = EyeAngles;
            angles += Input.AnalogLook;
            angles.roll = 0;
            angles.pitch = angles.pitch.Clamp(-90, 90);
            EyeAngles = angles;

            Transform.Rotation = EyeAngles.ToRotation();
        }

        var backTraceResult = Scene.Trace.Ray(Transform.Position, CameraTransform.Transform.Position)
            .IgnoreGameObjectHierarchy(Player)
            .Radius(BackTraceRadius)
            .WithAnyTags(BackTraceTags)
            .WithoutTags(BackTraceIgnoreTags)
            .Run();
        CameraTransform.Transform.Position = backTraceResult.EndPosition;
    }
}
