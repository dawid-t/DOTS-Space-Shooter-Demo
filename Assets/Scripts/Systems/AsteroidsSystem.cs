using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class AsteroidsSystem : JobComponentSystem
{
	private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;


	[BurstCompile]
	private struct AsteroidsJob : IJobForEachWithEntity<Translation, AsteroidVelocityData>
	{
		public float deltaTime;
		public EntityCommandBuffer.Concurrent entityCommandBuffer;
		[ReadOnly]
		public NativeMultiHashMap<int, QuadrantSystem.QuadrantEntityData> quadrantMultiHashMap;


		public void Execute(Entity entity, int index, ref Translation translation, ref AsteroidVelocityData vel)
		{
			// Update position:
			translation.Value.x += vel.Velocity.x * deltaTime;
			translation.Value.y += vel.Velocity.y * deltaTime;

			// Destroy if collided with other asteroid:
			int hashMapKey = QuadrantSystem.GetQuadrantHashMapKey(translation.Value);
			QuadrantSystem.QuadrantEntityData quadrantData;
			if(IsCollisionWithAsteroid(hashMapKey, entity, index, translation, quadrantMultiHashMap, out quadrantData))
			{
				if(entity != Entity.Null)
				{
					entityCommandBuffer.DestroyEntity(index, entity);
				}
				if(quadrantData.entity != Entity.Null)
				{
					entityCommandBuffer.DestroyEntity(quadrantData.entity.Index, quadrantData.entity);
				}
			}
		}
	}


	protected override void OnCreate()
	{
		endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		base.OnCreate();
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		AsteroidsJob asteroidsJob = new AsteroidsJob
		{
			deltaTime = Time.deltaTime,
			entityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
			quadrantMultiHashMap = QuadrantSystem.QuadrantAsteroidsMultiHashMap
		};
		JobHandle jobHandle = asteroidsJob.Schedule(this, inputDeps);
		endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);

		EntityQuery asteroidsEntityQuery = GetEntityQuery(typeof(AsteroidVelocityData));
		AsteroidsSpawner.AsteroidsToSpawn = AsteroidsSpawner.MaxAsteroidsNumber - asteroidsEntityQuery.CalculateEntityCount();

		return jobHandle;
	}

	public static bool IsCollisionWithAsteroid(int hashMapKey, Entity entity, int index, Translation translation, NativeMultiHashMap<int, QuadrantSystem.QuadrantEntityData> quadrantMultiHashMap, out QuadrantSystem.QuadrantEntityData quadrantData)
	{
		NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;

		if(quadrantMultiHashMap.TryGetFirstValue(hashMapKey, out quadrantData, out nativeMultiHashMapIterator))
		{
			do
			{
				if(!entity.Equals(quadrantData.entity) && math.distance(translation.Value, quadrantData.position) < 0.25f)
				{
					return true;
				}
			} while(quadrantMultiHashMap.TryGetNextValue(out quadrantData, ref nativeMultiHashMapIterator));
		}
		return false;
	}
}
