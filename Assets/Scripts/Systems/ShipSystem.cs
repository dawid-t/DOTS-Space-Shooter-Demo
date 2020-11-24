using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class ShipSystem : ComponentSystem
{
	private static int score = 0;

	private float lastProjectileSpawnedTime = 0;


	public static int Score { get => score; set => score = value; }


	protected override void OnUpdate()
	{
		Camera mainCamera = Camera.main;

		Entities.ForEach((Entity entity, ref Translation translation, ref Rotation rotation, ref ShipData shipData) =>
		{
			// Update the ship's position and rotation:
			float shipSpeedChanger;
			float shipRotationChanger;
			if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
			{
				shipSpeedChanger = 1;
			}
			else
			{
				shipSpeedChanger = 0;
			}

			if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
			{
				shipRotationChanger = 1;
			}
			else if(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
			{
				shipRotationChanger = -1;
			}
			else
			{
				shipRotationChanger = 0;
			}
			
			rotation.Value = math.mul(rotation.Value, quaternion.RotateZ(shipRotationChanger * shipData.RotationMultiplier * Time.deltaTime));
			
			float3 forwardVector = math.mul(rotation.Value, new float3(0, -1, 0));
			translation.Value += forwardVector * shipSpeedChanger * shipData.SpeedMultiplier * Time.deltaTime;

			// Update the camera position (follow the ship):
			mainCamera.transform.position = new Vector3(translation.Value.x, translation.Value.y, mainCamera.transform.position.z);

			// Spawn projectile after every "shipData.AttackSpeed" time:
			if(Time.time - lastProjectileSpawnedTime > shipData.AttackSpeed)
			{
				float3 projectileSpawnPosition = translation.Value + forwardVector * 0.25f;
				SpawnProjectile(projectileSpawnPosition, rotation);
				lastProjectileSpawnedTime = Time.time;
			}

			// Destroy if collided with asteroid (use more quadrant cells for better collision precision):
			int hashMapKey = QuadrantSystem.GetQuadrantHashMapKey(translation.Value);
			QuadrantSystem.QuadrantEntityData quadrantData;

			bool[] isCollision = new bool[9];
			isCollision[0] = AsteroidsSystem.IsCollisionWithAsteroid(hashMapKey, entity, entity.Index, translation, QuadrantSystem.QuadrantAsteroidsMultiHashMap, out quadrantData);

			isCollision[1] = AsteroidsSystem.IsCollisionWithAsteroid(hashMapKey+1, entity, entity.Index, translation, QuadrantSystem.QuadrantAsteroidsMultiHashMap, out quadrantData);
			isCollision[2] = AsteroidsSystem.IsCollisionWithAsteroid(hashMapKey-1, entity, entity.Index, translation, QuadrantSystem.QuadrantAsteroidsMultiHashMap, out quadrantData);

			isCollision[3] = AsteroidsSystem.IsCollisionWithAsteroid(hashMapKey+QuadrantSystem.QuadrantYMultiplier, entity, entity.Index, translation, QuadrantSystem.QuadrantAsteroidsMultiHashMap, out quadrantData);
			isCollision[4] = AsteroidsSystem.IsCollisionWithAsteroid(hashMapKey-QuadrantSystem.QuadrantYMultiplier, entity, entity.Index, translation, QuadrantSystem.QuadrantAsteroidsMultiHashMap, out quadrantData);

			isCollision[5] = AsteroidsSystem.IsCollisionWithAsteroid(hashMapKey+1+QuadrantSystem.QuadrantYMultiplier, entity, entity.Index, translation, QuadrantSystem.QuadrantAsteroidsMultiHashMap, out quadrantData);
			isCollision[6] = AsteroidsSystem.IsCollisionWithAsteroid(hashMapKey-1+QuadrantSystem.QuadrantYMultiplier, entity, entity.Index, translation, QuadrantSystem.QuadrantAsteroidsMultiHashMap, out quadrantData);

			isCollision[7] = AsteroidsSystem.IsCollisionWithAsteroid(hashMapKey+1-QuadrantSystem.QuadrantYMultiplier, entity, entity.Index, translation, QuadrantSystem.QuadrantAsteroidsMultiHashMap, out quadrantData);
			isCollision[8] = AsteroidsSystem.IsCollisionWithAsteroid(hashMapKey-1-QuadrantSystem.QuadrantYMultiplier, entity, entity.Index, translation, QuadrantSystem.QuadrantAsteroidsMultiHashMap, out quadrantData);

			if(isCollision[0] || isCollision[1] || isCollision[2] || isCollision[3] || isCollision[4] || isCollision[5] || isCollision[6] || isCollision[7] || isCollision[8])
			{
				if(entity != Entity.Null)
				{
					PostUpdateCommands.DestroyEntity(entity);
				}

				// End game:
				UI.ShowRestartScenePanel();
			}
		});
	}

	private void SpawnProjectile(float3 projectileSpawnPosition, Rotation shipRotation)
	{
		EntityManager manager = World.Active.EntityManager;
		Entity shipEntity = CreateProjectileEntity(manager);
		SetProjectileEntitySettings(shipEntity, projectileSpawnPosition, shipRotation, manager);
	}

	private Entity CreateProjectileEntity(EntityManager manager)
	{
		EntityArchetype prjectileEntityArchetype = manager.CreateArchetype(
			typeof(Translation),
			typeof(NonUniformScale),
			typeof(Rotation),
			typeof(RenderMesh),
			typeof(LocalToWorld),
			typeof(ProjectileData)
		);

		return manager.CreateEntity(prjectileEntityArchetype);
	}

	private void SetProjectileEntitySettings(Entity projectileEntity, float3 projectileSpawnPosition, Rotation shipRotation, EntityManager manager)
	{
		Translation translation = new Translation()
		{
			Value = projectileSpawnPosition
		};
		manager.SetComponentData(projectileEntity, translation);

		manager.SetSharedComponentData(projectileEntity, new RenderMesh
		{
			mesh = ShipSpawner.Instance.ProjectileMesh,
			material = ShipSpawner.Instance.ProjectileMaterial
		});

		NonUniformScale scale = new NonUniformScale()
		{
			Value = new float3(0.04f, 0.15f, 1)
		};
		manager.SetComponentData(projectileEntity, scale);

		Rotation rotation = new Rotation()
		{
			Value = shipRotation.Value
		};
		manager.SetComponentData(projectileEntity, rotation);

		ProjectileData projectileData = new ProjectileData()
		{
			SpeedMultiplier = 3,
			LifeTime = 0,
			MaxLifeTime = 3
		};
		manager.SetComponentData(projectileEntity, projectileData);
	}
}
