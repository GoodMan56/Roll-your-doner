using UnityEngine;
using System.Collections.Generic;

public class Sliceable : MonoBehaviour
{
	public bool currentlySliceable = true;
	
	public string category = "";
	
	public bool refreshColliders = true;
	public TurboSlice.InfillConfiguration[] infillers = new TurboSlice.InfillConfiguration[0];
	
	public bool channelNormals = true;
	public bool channelTangents = false;
	public bool channelUV2 = false;
		
	public GameObject explicitlySelectedMeshHolder = null;
	
	public Object alternatePrefab = null;
	public bool alwaysCloneFromAlternate = false;
	
	public void handleSlice( GameObject[] results )
	{
		AbstractSliceHandler[] handlers = gameObject.GetComponents<AbstractSliceHandler>();
		
		foreach(AbstractSliceHandler handler in handlers)
		{
			handler.handleSlice(results);
		}
	}
	
	public bool cloneAlternate( Dictionary<string,bool> hierarchyPresence )
	{
		if(alternatePrefab == null)
		{
			return false;
		}
		else if(alwaysCloneFromAlternate)
		{
			return true;
		}
		else
		{
			AbstractSliceHandler[] handlers = gameObject.GetComponents<AbstractSliceHandler>();
			
			foreach(AbstractSliceHandler handler in handlers)
			{
				if(handler.cloneAlternate( hierarchyPresence ))
				{
					return true;
				}
			}
		
			return false;
		}
	}
}
