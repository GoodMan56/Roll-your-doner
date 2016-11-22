using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(ResourceManager))]
public class ResourceEditor : Editor {

	static private List<bool> foldList=new List<bool>();
	
	static private ResourceManager rm;
	
	static int num=1;
	
	static public List<Resource> list=new  List<Resource>();
	
	void Awake(){
		rm = (ResourceManager)target;
		num=rm.resources.Length;
		
		for(int i=foldList.Count; i<num; i++){
			foldList.Add(false);
		}
		
		//Debug.Log("Awake "+rm.resources.Length+"  "+foldList.Count);
		
		//Debug.Log(list.Count);
		
		VerifyingList();
		EditorUtility.SetDirty(rm);
	}
	
	void OnEnable(){
		ResourceEditorWindow.onResourceUpdateE+=VerifyingList;
	}
	
	void OnDisable(){
		ResourceEditorWindow.onResourceUpdateE-=VerifyingList;
	}
	
	public void VerifyingList(){
		list=ResourceEditorWindow.Load();
		Resource[] tempList=new Resource[list.Count];
		for(int i=0; i<tempList.Length; i++){
			tempList[i]=list[i];
			
			for(int j=0; j<rm.resources.Length; j++){
				if(tempList[i].CheckMatch(rm.resources[j])){
					tempList[i].value=rm.resources[j].value;
				}
			}
		}
		
		rm.resources=tempList;
	}
	
	public override void OnInspectorGUI(){
		Undo.SetSnapshotTarget(rm, "ResourceManager");
		
		GUI.changed = false;
		
		EditorGUILayout.Space();
		//num=EditorGUILayout.IntField("Total ResourceType: ", num);
		//if(num<=0) num=1;
		//EditorGUILayout.Space();
		
		//if(num!=rm.resources.Length) UpdateResourceSize(num);
		if(foldList.Count!=rm.resources.Length) UpdateFoldListSize();
		
		for(int i=0; i<rm.resources.Length; i++){
			if(rm.resources[i]!=null){
				foldList[i]=EditorGUILayout.Foldout(foldList[i], rm.resources[i].name+": "+rm.resources[i].value);
				
				if(foldList[i]){
					GUILayout.BeginHorizontal();
					GUILayout.Label(rm.resources[i].icon, GUILayout.Width(43),  GUILayout.Height(43));
					
						GUILayout.BeginVertical();
							GUILayout.Label(rm.resources[i].name);
							rm.resources[i].value=EditorGUILayout.IntField("Start Value: ", rm.resources[i].value);
						GUILayout.EndVertical();
					
					GUILayout.EndHorizontal();
				}
			}
		}
		
		
		
		if(GUI.changed){
			EditorUtility.SetDirty(rm);
			Undo.CreateSnapshot();
            Undo.RegisterSnapshot();
        }
        Undo.ClearSnapshotTarget();
	}
	
	void UpdateResourceSize(int n){
		Resource[] tempList=rm.resources;
		
		rm.resources=new Resource[n];
		
		for(int i=0; i<rm.resources.Length; i++){
			if(i>=tempList.Length){
				rm.resources[i]=new Resource();
				foldList.Add(false);
			}
			else{
				rm.resources[i]=tempList[i];
			}
		}
		
		UpdateFoldListSize();
	}
	
	void UpdateFoldListSize(){
		if(rm.resources.Length<foldList.Count){
			int diff=foldList.Count-rm.resources.Length;
			foldList.RemoveRange(rm.resources.Length, diff);
		}
		else{
			for(int i=foldList.Count; i<rm.resources.Length; i++){
				foldList.Add(false);
			}
		}
	}
	
	
}
