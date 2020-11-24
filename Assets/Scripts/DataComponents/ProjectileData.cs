

using Unity.Entities;

public struct ProjectileData : IComponentData
{
	public float SpeedMultiplier;
	public float LifeTime;
	public float MaxLifeTime;
}