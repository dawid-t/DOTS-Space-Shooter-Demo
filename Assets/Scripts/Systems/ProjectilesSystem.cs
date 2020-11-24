using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class ProjectilesSystem : ComponentSystem
{
	protected override void OnUpdate()
	{
		Entities.ForEach((Entity entity, ref Translation translation, ref Rotation rotation, ref ProjectileData projectileData) =>
		{
			// Update position:
			float3 forwardVector = math.mul(rotation.Value, new float3(0, -1, 0));
			translation.Value += forwardVector * projectileData.SpeedMultiplier * Time.deltaTime;

			// Update life time:
			projectileData.LifeTime += Time.deltaTime;
			if(projectileData.LifeTime >= projectileData.MaxLifeTime)
			{
				PostUpdateCommands.DestroyEntity(entity);
				return;
			}

			// Destroy if collided with asteroid and update the score:
			QuadrantSystem.QuadrantEntityData quadrantData;
			if(AsteroidsSystem.IsCollisionWithAsteroid(entity, entity.Index, translation, QuadrantSystem.QuadrantAsteroidsMultiHashMap, out quadrantData))
			{
				if(entity != Entity.Null)
				{
					PostUpdateCommands.DestroyEntity(entity);
				}
				if(quadrantData.entity != Entity.Null)
				{
					PostUpdateCommands.DestroyEntity(quadrantData.entity);
				}

				ShipSystem.Score++;
				UI.UpdateScore(ShipSystem.Score);
			}
		});
	}
}


