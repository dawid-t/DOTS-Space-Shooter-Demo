using Unity.Entities;

public struct ShipData : IComponentData
{
	public float SpeedMultiplier;
	public float Speed;
	public float RotationMultiplier;
	public float Rotation;
}
