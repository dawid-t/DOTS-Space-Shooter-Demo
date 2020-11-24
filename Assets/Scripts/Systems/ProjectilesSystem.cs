using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class ProjectilesSystem : JobComponentSystem
{
	private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;


	[BurstCompile]
	private struct ProjectilesJob : IJobForEachWithEntity<Translation, Rotation, ProjectileData>
	{
		public float deltaTime;
		public EntityCommandBuffer.Concurrent entityCommandBuffer;


		public void Execute(Entity entity, int index, ref Translation translation, ref Rotation rotation, ref ProjectileData projectileData)
		{
			// Update position:
			float3 forwardVector = math.mul(rotation.Value, new float3(0, -1, 0));
			translation.Value += forwardVector * projectileData.SpeedMultiplier * deltaTime;

			// Update life time:
			projectileData.LifeTime += deltaTime;
			if(projectileData.LifeTime >= projectileData.MaxLifeTime)
			{
				entityCommandBuffer.DestroyEntity(index, entity);
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
		ProjectilesJob asteroidsJob = new ProjectilesJob
		{
			deltaTime = Time.deltaTime,
			entityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()
		};
		JobHandle jobHandle = asteroidsJob.Schedule(this, inputDeps);
		endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);

		return jobHandle;
	}
}