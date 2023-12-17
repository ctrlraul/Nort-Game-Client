using Godot;
using System;

namespace Nort.Entities.Components;

public partial class FlightComponent : EntityComponent
{
    private const float Damp = 0.95f;

    public Vector2 Direction { get; set; } = Vector2.Zero;
    public Vector2 Velocity { get; private set; } = Vector2.Zero;

    private float torque = 1f;
    private float acceleration = 1f;

    public override void _PhysicsProcess(double delta)
    {
        delta *= Engine.PhysicsTicksPerSecond; // Normalize delta

        if (Direction != Vector2.Zero)
        {
            RotateTowardsAngle(Direction.Angle() + Mathf.Pi * 0.5f, torque * 0.08f);

            Velocity += Direction.Normalized() * acceleration * torque * (float)delta;
            Direction = Vector2.Zero;
        }

        Craft.Position += Velocity * (float)delta;
        Velocity *= Damp;
    }
    
    private static float DeltaAngle(float from, float to)
    {
        float difference = to - from % Mathf.Tau;
        return 2 * difference % Mathf.Tau - difference;
    }

    private void RotateTowardsAngle(float angle, float amount)
    {
        float distance = Mathf.Abs(DeltaAngle(Craft.Rotation, angle));
        Craft.Rotation = (
            amount > distance
            ? angle
            : Mathf.LerpAngle(Craft.Rotation, angle, amount / distance)
        );
    }
}
