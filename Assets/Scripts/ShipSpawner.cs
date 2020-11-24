using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class ShipSpawner : MonoBehaviour
{
	[SerializeField]
	private Mesh shipMesh;
	[SerializeField]
	private Material shipMaterial;


	private void Start()
	{
		EntityManager manager = World.Active.EntityManager;
		Entity shipEntity = CreateShipEntity(manager);
		SetShipEntitySettings(shipEntity, manager);
	}

	private Entity CreateShipEntity(EntityManager manager)
	{
		EntityArchetype shipEntityArchetype = manager.CreateArchetype(
			typeof(Translation),
			typeof(Scale),
			typeof(RenderMesh),
			typeof(LocalToWorld),
			typeof(ShipData),
			typeof(Rotation)
		);

		return manager.CreateEntity(shipEntityArchetype);
	}

	private void SetShipEntitySettings(Entity shipEntity, EntityManager manager)
	{
		Translation translation = new Translation()
		{
			Value = new float3(0, 0, 0)
		};
		manager.SetComponentData(shipEntity, translation);

		manager.SetSharedComponentData(shipEntity, new RenderMesh
		{
			mesh = shipMesh,
			material = shipMaterial
		});

		Scale scale = new Scale()
		{
			Value = 0.25f
		};
		manager.SetComponentData(shipEntity, scale);

		Rotation rotation = new Rotation()
		{
			Value = quaternion.Euler(new float3(0, 0, 0))
		};
		manager.SetComponentData(shipEntity, rotation);

		ShipData shipData = new ShipData()
		{
			SpeedMultiplier = 0.75f,
			Speed = 0,
			Rotation = 0,
			RotationMultiplier = 3
		};
		manager.SetComponentData(shipEntity, shipData);
	}
}
