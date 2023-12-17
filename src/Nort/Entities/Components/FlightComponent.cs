using Godot;

namespace Nort.Entities.Components;

public partial class FlightComponent : EntityComponent
{
    public Vector2 Direction { get; set; } = Vector2.Zero;

    private float torque = 1f;
    private float acceleration = 1f;

    public override void _PhysicsProcess(double delta)
    {
        delta *= Engine.PhysicsTicksPerSecond; // Normalize delta

        if (Direction != Vector2.Zero)
        {
            RotateTowardsAngle(Direction.Angle() + Mathf.Pi * 0.5f, torque * 0.08f);

            Craft.Velocity += Direction.Normalized() * acceleration * torque * (float)delta;
            Direction = Vector2.Zero;
        }
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
