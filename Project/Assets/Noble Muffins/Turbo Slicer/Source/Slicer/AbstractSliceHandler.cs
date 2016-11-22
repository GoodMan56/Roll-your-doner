using UnityEngine;
using System.Collections.Generic;

public abstract class AbstractSliceHandler : MonoBehaviour
{
	public abstract void handleSlice( GameObject[] results );
	
	public virtual bool cloneAlternate ( Dictionary<string,bool> hierarchyPresence )
	{
		return false;
	}
}
