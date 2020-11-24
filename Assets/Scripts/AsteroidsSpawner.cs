using System.Collections;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class AsteroidsSpawner : MonoBehaviour
{
	private static int asteroidsToSpawn = 0;
	private static int maxAsteroidsNumber = 0;

	[SerializeField]
	private int asteroidsNumberOnX = 160, asteroidsNumberOnY = 160;
	[SerializeField] [Tooltip("The seed is for generate the same random velocity of asteroids in every game.")]
	private int asteroidsVelocitySeed = 100;
	[SerializeField]
	private float asteroidsMaxSpeed = 1;
	[SerializeField]
	private Mesh asteroidMesh;
	[SerializeField]
	private Material asteroidMaterial;


	public static int AsteroidsToSpawn { get => asteroidsToSpawn; set => asteroidsToSpawn = value; }
	public static int MaxAsteroidsNumber => maxAsteroidsNumber;


	private void Start()
	{
		SetMaxAsteroidsNumber();
		SpawnFirstAsteroids();
		StartCoroutine(SpawnDestroyedAsteroids());
	}

	private void OnValidate()
	{
		asteroidsNumberOnX = Mathf.Abs(asteroidsNumberOnX);
		asteroidsNumberOnY = Mathf.Abs(asteroidsNumberOnY);
	}

	public void SetMaxAsteroidsNumber()
	{
		maxAsteroidsNumber = asteroidsNumberOnX * asteroidsNumberOnY;
	}

	private void SpawnFirstAsteroids()
	{
		EntityManager manager = World.Active.EntityManager;
		int entityArrayLength = asteroidsNumberOnX * asteroidsNumberOnY;
		NativeArray<Entity> entityArray = CreateAsteroidsEntities(manager, entityArrayLength);

		float xSpawnPoint = -(Mathf.Abs(asteroidsNumberOnX)/2 - 0.5f);
		float ySpawnPoint = -(Mathf.Abs(asteroidsNumberOnY)/2 - 0.5f);
		float yFirstSpawnPoint = ySpawnPoint;

		UnityEngine.Random.State originalRandomState = UnityEngine.Random.state;
		for(int i = 0; i < asteroidsNumberOnX; i++)
		{
			for(int j = 0; j < asteroidsNumberOnY; j++)
			{
				Entity asteroidEntity = entityArray[i*asteroidsNumberOnX + j];
				float3 spawnPoint = new float3(xSpawnPoint, ySpawnPoint, 0);
				
				asteroidsVelocitySeed++;
				UnityEngine.Random.InitState(asteroidsVelocitySeed); // On the level beginning set the same velocities to asteroids.
				SetAsteroidsEntitiesSettings(asteroidEntity, spawnPoint, manager);

				ySpawnPoint += 1;
			}
			xSpawnPoint += 1;
			ySpawnPoint = yFirstSpawnPoint;
		}
		UnityEngine.Random.state = originalRandomState; // Reset seed randomness.

		entityArray.Dispose();
	}

	private IEnumerator SpawnDestroyedAsteroids()
	{
		EntityManager manager = World.Active.EntityManager;
		Camera mainCamera = Camera.main;

		while(true)
		{
			yield return new WaitForSeconds(1);

			float height = 2*mainCamera.orthographicSize;
			float width = height*mainCamera.aspect;
			Vector3 cameraPosition = mainCamera.transform.position;

			if(asteroidsToSpawn > 0)
			{
				NativeArray<Entity> entityArray = CreateAsteroidsEntities(manager, asteroidsToSpawn);
				for(int i = 0; i < asteroidsToSpawn; i++)
				{
					Entity asteroidEntity = entityArray[i];

					int randXSide = (UnityEngine.Random.Range(0, 2) == 0) ? 1 : -1;
					int randYSide = (UnityEngine.Random.Range(0, 2) == 0) ? 1 : -1;

					float xSpawnPoint = cameraPosition.x + randXSide*UnityEngine.Random.Range(width, asteroidsNumberOnX*2);
					float ySpawnPoint = cameraPosition.y + randYSide*UnityEngine.Random.Range(height, asteroidsNumberOnY*2);
					float3 spawnPoint = new float3(xSpawnPoint, ySpawnPoint, 0);

					SetAsteroidsEntitiesSettings(asteroidEntity, spawnPoint, manager);
				}
				
				asteroidsToSpawn = 0;
			}
		}
	}

	private NativeArray<Entity> CreateAsteroidsEntities(EntityManager manager, int entityArrayLength)
	{
		NativeArray<Entity> entityArray = new NativeArray<Entity>(entityArrayLength, Allocator.Temp);
		EntityArchetype asteroidEntityArchetype = manager.CreateArchetype(
			typeof(Translation),
			typeof(Scale),
			typeof(RenderMesh),
			typeof(LocalToWorld),
			typeof(AsteroidVelocityData)
		);
		manager.CreateEntity(asteroidEntityArchetype, entityArray);
		return entityArray;
	}

	private void SetAsteroidsEntitiesSettings(Entity asteroidEntity, float3 spawnPoint, EntityManager manager)
	{
		#region Set asteroid random velocity:
		float xForce = UnityEngine.Random.Range(-asteroidsMaxSpeed, asteroidsMaxSpeed);
		float yForce = UnityEngine.Random.Range(-asteroidsMaxSpeed, asteroidsMaxSpeed);
		xForce = (xForce == 0) ? 1 : xForce;
		yForce = (yForce == 0) ? 1 : yForce;
		#endregion Set asteroid random velocity.

		#region Set asteroid settings:
		Translation translation = new Translation()
		{
			Value = spawnPoint
		};
		manager.SetComponentData(asteroidEntity, translation);

		manager.SetSharedComponentData(asteroidEntity, new RenderMesh
		{
			mesh = asteroidMesh,
			material = asteroidMaterial
		});

		Scale scale = new Scale()
		{
			Value = 0.5f
		};
		manager.SetComponentData(asteroidEntity, scale);

		AsteroidVelocityData asteroidVelocityData = new AsteroidVelocityData()
		{
			Velocity = new float2(xForce, yForce)
		};
		manager.SetComponentData(asteroidEntity, asteroidVelocityData);
		#endregion Set asteroid settings.
	}
}
