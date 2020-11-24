using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class ShipSystem : ComponentSystem
{
	protected override void OnUpdate()
	{
		Camera mainCamera = Camera.main;

		Entities.ForEach((Entity entity, ref Translation translation, ref Rotation rotation, ref ShipData shipData) =>
		{
			if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
			{
				shipData.Speed = 1;
			}
			else
			{
				shipData.Speed = 0;
			}

			if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
			{
				shipData.Rotation = 1;
			}
			else if(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
			{
				shipData.Rotation = -1;
			}
			else
			{
				shipData.Rotation = 0;
			}
			
			rotation.Value = math.mul(rotation.Value, quaternion.RotateZ(shipData.Rotation * shipData.RotationMultiplier * Time.deltaTime));
			
			float3 forwardVector = math.mul(rotation.Value, new float3(0, -1, 0));
			translation.Value += forwardVector * shipData.Speed * shipData.SpeedMultiplier * Time.deltaTime;

			mainCamera.transform.position = new Vector3(translation.Value.x, translation.Value.y, mainCamera.transform.position.z);
		});
	}
}
