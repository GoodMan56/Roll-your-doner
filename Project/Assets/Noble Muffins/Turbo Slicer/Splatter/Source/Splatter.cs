using UnityEngine;
using System.Collections;

public class Splatter : AbstractSliceHandler
{
	public Object particlePrefab;
	
	public override void handleSlice( GameObject[] results )
	{
		Vector3 position = results[0].transform.position;
		
		GameObject.Instantiate(particlePrefab, position, Quaternion.identity);
	}
	
}
