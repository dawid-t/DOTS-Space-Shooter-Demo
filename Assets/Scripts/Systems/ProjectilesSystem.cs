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

			// Destroy if collided with asteroid and update the score (use more quadrant cells for better collision precision):
			int hashMapKey = QuadrantSystem.GetQuadrantHashMapKey(translation.Value);
			QuadrantSystem.QuadrantEntityData[] quadrantData = new QuadrantSystem.QuadrantEntityData[9];

			bool[] isCollision = new bool[9];
			isCollision[0] = AsteroidsSystem.IsCollisionWithAsteroid(hashMapKey, entity, entity.Index, translation, QuadrantSystem.QuadrantAsteroidsMultiHashMap, out quadrantData[0]);

			isCollision[1] = AsteroidsSystem.IsCollisionWithAsteroid(hashMapKey+1, entity, entity.Index, translation, QuadrantSystem.QuadrantAsteroidsMultiHashMap, out quadrantData[1]);
			isCollision[2] = AsteroidsSystem.IsCollisionWithAsteroid(hashMapKey-1, entity, entity.Index, translation, QuadrantSystem.QuadrantAsteroidsMultiHashMap, out quadrantData[2]);

			isCollision[3] = AsteroidsSystem.IsCollisionWithAsteroid(hashMapKey+QuadrantSystem.QuadrantYMultiplier, entity, entity.Index, translation, QuadrantSystem.QuadrantAsteroidsMultiHashMap, out quadrantData[3]);
			isCollision[4] = AsteroidsSystem.IsCollisionWithAsteroid(hashMapKey-QuadrantSystem.QuadrantYMultiplier, entity, entity.Index, translation, QuadrantSystem.QuadrantAsteroidsMultiHashMap, out quadrantData[4]);

			isCollision[5] = AsteroidsSystem.IsCollisionWithAsteroid(hashMapKey+1+QuadrantSystem.QuadrantYMultiplier, entity, entity.Index, translation, QuadrantSystem.QuadrantAsteroidsMultiHashMap, out quadrantData[5]);
			isCollision[6] = AsteroidsSystem.IsCollisionWithAsteroid(hashMapKey-1+QuadrantSystem.QuadrantYMultiplier, entity, entity.Index, translation, QuadrantSystem.QuadrantAsteroidsMultiHashMap, out quadrantData[6]);

			isCollision[7] = AsteroidsSystem.IsCollisionWithAsteroid(hashMapKey+1-QuadrantSystem.QuadrantYMultiplier, entity, entity.Index, translation, QuadrantSystem.QuadrantAsteroidsMultiHashMap, out quadrantData[7]);
			isCollision[8] = AsteroidsSystem.IsCollisionWithAsteroid(hashMapKey-1-QuadrantSystem.QuadrantYMultiplier, entity, entity.Index, translation, QuadrantSystem.QuadrantAsteroidsMultiHashMap, out quadrantData[8]);

			for(int i = 0; i < isCollision.Length; i++)
			{
				if(isCollision[i])
				{
					if(entity != Entity.Null)
					{
						PostUpdateCommands.DestroyEntity(entity);
					}
					if(quadrantData[i].entity != Entity.Null)
					{
						PostUpdateCommands.DestroyEntity(quadrantData[i].entity);
					}

					ShipSystem.Score++;
					UI.UpdateScore(ShipSystem.Score);
					break;
				}
			}
		});
	}
}


