using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class ShipSpawner : MonoBehaviour
{
	private static ShipSpawner instance;

	[SerializeField]
	private Mesh shipMesh, projectileMesh;
	[SerializeField]
	private Material shipMaterial, projectileMaterial;


	public static ShipSpawner Instance => instance;

	public Mesh ProjectileMesh => projectileMesh;
	public Material ProjectileMaterial => projectileMaterial;


	private void Start()
	{
		instance = this;
		EntityManager manager = World.Active.EntityManager;
		Entity shipEntity = CreateShipEntity(manager);
		SetShipEntitySettings(shipEntity, manager);
	}

	private Entity CreateShipEntity(EntityManager manager)
	{
		EntityArchetype shipEntityArchetype = manager.CreateArchetype(
			typeof(Translation),
			typeof(Scale),
			typeof(Rotation),
			typeof(RenderMesh),
			typeof(LocalToWorld),
			typeof(ShipData)
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
			RotationMultiplier = 3,
			AttackSpeed = 0.5f
		};
		manager.SetComponentData(shipEntity, shipData);
	}
}
