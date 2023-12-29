using Godot;
using Nort.Skills;

namespace Nort.Entities;

public partial class CoreBulletProjectile : Entity
{
	protected override float Damp { get; } = 1;

	public Area2D hitBox;
    
	private float speed = 80;
	private float damage = 5;
	private CoreBulletSkill source;
	
	
	public override void _Ready()
	{
		hitBox = GetNode<Area2D>("HitBox");
		AudioManager.Instance.PlayCoreBulletFired(GlobalPosition);
	}


	public void SetSource(CoreBulletSkill skillNode)
	{
		source = skillNode;
		
		Position = source.GlobalPosition;
		Velocity = Position.DirectionTo(source.Target.GlobalPosition) * speed;

		hitBox.CollisionMask = Assets.Instance.GetFactionCollisionMask(source.Part.Faction);
		hitBox.Monitoring = true;
		
		LookAt(source.Target.GlobalPosition);
	}


	private void OnHitBoxAreaEntered(Area2D area)
	{
		CraftPart partHit = (CraftPart)area;
		partHit.Craft.TakeHit(partHit, source, damage);
		QueueFree();
	}
	
	private void OnLifespanTimerTimeout()
	{
		QueueFree();
	}
}
