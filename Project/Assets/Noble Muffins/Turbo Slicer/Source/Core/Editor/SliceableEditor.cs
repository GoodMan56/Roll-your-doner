using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(Sliceable))]
public class SliceableEditor : Editor
{
	public override void OnInspectorGUI ()
	{
		Sliceable s = (Sliceable) target;
		
		Renderer[] renderers = s.GetComponentsInChildren<Renderer>(true);
		
		bool isAnimated = false;
		foreach(Renderer r in renderers)
			isAnimated |= r is SkinnedMeshRenderer;
		
		//TurboSlice.supportsSkinned is a const that is overridden by the presence of Ragdoll Slicer
#pragma warning disable 0162
		if(isAnimated && !TurboSlice.supportsSkinned)
		{
			EditorGUILayout.LabelField("Error!");
			EditorGUILayout.LabelField("Skinned meshes are not supported.");
			return;
		}
		
		s.refreshColliders = EditorGUILayout.Toggle("Refresh colliders", s.refreshColliders);
		
		s.alternatePrefab = EditorGUILayout.ObjectField("Alternate prefab", (Object) s.alternatePrefab, typeof(GameObject), false);

		if(s.alternatePrefab != null)
		{
			s.alwaysCloneFromAlternate = EditorGUILayout.Toggle("Always clone from alternate", s.alwaysCloneFromAlternate);
		}
		
		s.currentlySliceable = EditorGUILayout.Toggle("Currently Sliceable", s.currentlySliceable);
		
		s.category = EditorGUILayout.TextField("Category", s.category);
		
		s.channelNormals = EditorGUILayout.Toggle("Process Normals", s.channelNormals);
		s.channelTangents = EditorGUILayout.Toggle("Process Tangents", s.channelTangents);
		s.channelUV2 = EditorGUILayout.Toggle("Process UV2", s.channelUV2);
		
		Renderer renderer = null;
		
		if(renderers.Length == 0)
		{
			EditorGUILayout.LabelField("No mesh renderers found in this object!");
		}
		else if(renderers.Length > 1)
		{
			EditorGUILayout.LabelField("This object has multiple meshes. Specify the primary.");
			
			int selectedRenderer = 0;
			
			if(s.explicitlySelectedMeshHolder != null)
			{
				Renderer r = s.explicitlySelectedMeshHolder.GetComponent<Renderer>();
				if(r != null)
				{
					selectedRenderer = System.Array.IndexOf<Renderer>(renderers, r);
				}
			}
			
			string[] displayedOptions = new string[renderers.Length];
			for(int i = 0; i < displayedOptions.Length; i++)
			{
				displayedOptions[i] = renderers[i].name;
			}
			
			selectedRenderer = EditorGUILayout.Popup("Slice Mesh", selectedRenderer, displayedOptions);
			
			renderer = renderers[selectedRenderer];
			s.explicitlySelectedMeshHolder = renderer.gameObject;
		}
		else if(renderers.Length == 1)
		{
			renderer = renderers[0];
			
			s.explicitlySelectedMeshHolder = renderer.gameObject;
		}
		
		if(renderer != null)
		{			
			List<TurboSlice.InfillConfiguration> newInfillers = new List<TurboSlice.InfillConfiguration>();
			
			Material[] mats = renderer.sharedMaterials;
			
			if(mats.Length > 0)
			{
				EditorGUILayout.LabelField("For each material, define what region is used for infill.");
			}
			
			foreach(Material mat in mats)
			{
				//Is this material represented in our array?
				
				EditorGUILayout.Separator();
				
				if(s.infillers == null)
					s.infillers = new TurboSlice.InfillConfiguration[0];
				
				TurboSlice.InfillConfiguration infiller = null;
				foreach(TurboSlice.InfillConfiguration ifc in s.infillers)
				{
					if(ifc.material == mat)
					{
						infiller = ifc;
						break;
					}
				}
				
				EditorGUILayout.LabelField("Material: " + mat.name);
				
				bool hasIt = EditorGUILayout.Toggle("Infill this material", infiller != null);
				
				if(hasIt && infiller == null)
				{
					infiller = new TurboSlice.InfillConfiguration();
					
					infiller.material = mat;
				}
				else if(!hasIt)
				{
					infiller = null;
				}
				
				if(infiller != null)
				{
					newInfillers.Add(infiller);
					
					infiller.regionForInfill = EditorGUILayout.RectField(infiller.regionForInfill);
				}
			}
			
			s.infillers = newInfillers.ToArray();
		}
		
        if (GUI.changed)
		{
            EditorUtility.SetDirty (target);
		}
    }

}
