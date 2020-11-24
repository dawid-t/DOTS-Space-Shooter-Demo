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
		[ReadOnly] public NativeMultiHashMap<int, QuadrantSystem.QuadrantEntityData> quadrantMultiHashMap;


		public void Execute(Entity entity, int index, ref Translation t, ref AsteroidVelocityData vel)
		{
			// Update position:
			t.Value.x += vel.Velocity.x * deltaTime;
			t.Value.y += vel.Velocity.y * deltaTime;

			// Destroy if collided with something (with other asteroid, projectile or player):
			int hashMapKey = QuadrantSystem.GetQuadrantHashMapKey(t.Value);
			QuadrantSystem.QuadrantEntityData quadrantData;
			NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
			if(quadrantMultiHashMap.TryGetFirstValue(hashMapKey, out quadrantData, out nativeMultiHashMapIterator))
			{
				do
				{
					if(!entity.Equals(quadrantData.entity) && math.distance(t.Value, quadrantData.position) < 0.25f)
					{
						if(entity != Entity.Null)
						{
							entityCommandBuffer.DestroyEntity(index, entity);
						}
						if(quadrantData.entity != Entity.Null)
						{
							entityCommandBuffer.DestroyEntity(quadrantData.entity.Index, quadrantData.entity);
						}
						break;
					}
				} while(quadrantMultiHashMap.TryGetNextValue(out quadrantData, ref nativeMultiHashMapIterator));
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
			quadrantMultiHashMap = QuadrantSystem.QuadrantMultiHashMap
		};
		JobHandle jobHandle = asteroidsJob.Schedule(this, inputDeps);
		endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);

		EntityQuery entityQuery = GetEntityQuery(typeof(AsteroidVelocityData));
		AsteroidsSpawner.AsteroidsToSpawn = AsteroidsSpawner.MaxAsteroidsNumber - entityQuery.CalculateEntityCount();

		return jobHandle;
	}
}
