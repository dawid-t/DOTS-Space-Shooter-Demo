using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class QuadrantSystem : ComponentSystem
{
	private const int quadrantYMultiplier = 10000;
	private const float quadrantCellSize = 0.5f;
	private static NativeMultiHashMap<int, QuadrantEntityData> quadrantMultiHashMap;

	
	public static NativeMultiHashMap<int, QuadrantEntityData> QuadrantMultiHashMap => quadrantMultiHashMap;


	public struct QuadrantEntityData
	{
		public Entity entity;
		public float3 position;
	}

	[BurstCompile]
	private struct SetQuadrantDataHashMapJob : IJobForEachWithEntity<Translation>
	{
		public NativeMultiHashMap<int, QuadrantEntityData>.Concurrent quadrantMultiHashMap;

		public void Execute(Entity entity, int index, ref Translation translation)
		{
			int hashMapKey = GetQuadrantHashMapKey(translation.Value);
			quadrantMultiHashMap.Add(hashMapKey, new QuadrantEntityData
			{
				entity = entity,
				position = translation.Value
			});
		}
	}


	protected override void OnCreate()
	{
		quadrantMultiHashMap = new NativeMultiHashMap<int, QuadrantEntityData>(0, Allocator.Persistent);
		base.OnCreate();
	}

	protected override void OnUpdate()
	{
		EntityQuery entityQuery = GetEntityQuery(typeof(Translation));

		quadrantMultiHashMap.Clear();
		if(entityQuery.CalculateEntityCount() > quadrantMultiHashMap.Capacity)
		{
			quadrantMultiHashMap.Capacity = entityQuery.CalculateEntityCount();
		}

		SetQuadrantDataHashMapJob setQuadrantDataHashMapJob = new SetQuadrantDataHashMapJob
		{
			quadrantMultiHashMap = quadrantMultiHashMap.ToConcurrent(),
		};
		JobHandle jobHandle = JobForEachExtensions.Schedule(setQuadrantDataHashMapJob, entityQuery);
		jobHandle.Complete();
	}

	protected override void OnDestroy()
	{
		quadrantMultiHashMap.Dispose();
		base.OnDestroy();
	}

	public static int GetQuadrantHashMapKey(float3 position)
	{
		return (int)(math.floor(position.x / quadrantCellSize) + (quadrantYMultiplier * math.floor(position.y / quadrantYMultiplier)));
	}
}
