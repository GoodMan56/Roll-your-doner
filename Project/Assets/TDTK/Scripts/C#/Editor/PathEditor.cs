using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(PathTD))]
public class PathEditor : Editor {

	static private PathTD path;
	
	bool showPath=true;
	
	void Awake(){
		path = (PathTD)target;
		
		if(path.waypoints==null) path.waypoints=new Transform[2];
		
		EditorUtility.SetDirty(path);
	}
	
	void UpdateWaypoints(int length){
		Transform[] list=new Transform[length];
		
		for(int i=0; i<list.Length; i++){
			if(i<path.waypoints.Length){
				list[i]=path.waypoints[i];
			}
			else{
				list[i]=path.waypoints[path.waypoints.Length-1];
			}
		}
		
		path.waypoints=list;
	}
	
	void InsertWaypoints(int ID){
		Transform[] list=new Transform[path.waypoints.Length+1];
		
		for(int i=0; i<list.Length; i++){
			if(i<ID){
				list[i]=path.waypoints[i];
			}
			else if(i>ID){
				list[i]=path.waypoints[i-1];
			}
		}
		
		path.waypoints=list;
	}
	
	void RemoveWaypoints(int ID){
		Transform[] list=new Transform[path.waypoints.Length-1];
		
		for(int i=0; i<list.Length; i++){
			if(i<ID){
				list[i]=path.waypoints[i];
			}
			else if(i>=ID){
				list[i]=path.waypoints[i+1];
			}
		}
		
		path.waypoints=list;
	}
	
	public override void OnInspectorGUI(){
		
		Undo.SetSnapshotTarget(path, "Path");
		
		GUI.changed = false;
		
		DrawDefaultInspector();
		
		EditorGUILayout.Space();
		
		//~ if(GUILayout.Button("Test")){
			//~ path.test();
		//~ }
		//~ EditorGUILayout.Space();
		
		showPath=GUILayout.Toggle(showPath, "show waypoint list:");
		
		if(showPath){
			GUILayout.BeginHorizontal();
			if(GUILayout.Button("Add")){
				UpdateWaypoints(path.waypoints.Length+1);
			}
			else if(GUILayout.Button("Remove")){
				UpdateWaypoints(path.waypoints.Length-1);
			}
			GUILayout.EndHorizontal();
			
			for(int i=0; i<path.waypoints.Length; i++){
				GUILayout.BeginHorizontal();
				GUILayout.Label(" - Point "+i.ToString(), GUILayout.MaxWidth(80));
				path.waypoints[i]=EditorGUILayout.ObjectField(path.waypoints[i], typeof(Transform), true) as Transform;
				if(GUILayout.Button("-", GUILayout.MaxWidth(25))){
					RemoveWaypoints(i);
				}
				if(GUILayout.Button("+", GUILayout.MaxWidth(25))){
					InsertWaypoints(i);
				}
				GUILayout.EndHorizontal();
			}
		}
		
		//~ int count=path.waypoints.Length;
		//~ count=GUILayout.IntField(count);
		//~ if(count!=path.waypoints.Length) 
		
		//~ for(int i=0; i<pm.allPerkList.Count; i++){
			//~ Perk perk=pm.allPerkList[i];
			
			//~ GUILayout.BeginHorizontal();
					
				//~ GUILayout.Box(perk.icon, GUILayout.Width(30),  GUILayout.Height(30));
				//~ //GUILayout.Label(perk.icon, GUILayout.Width(50),  GUILayout.Height(50));
		
				//~ GUILayout.BeginVertical();
					//~ //GUILayout.Space (5);
					//~ GUILayout.Label(perk.name);
					
					//~ GUILayout.BeginHorizontal();
					
					//~ if(perk.enableInlvl){
						//~ GUILayout.Label("Enabled/Unlocked:", GUILayout.ExpandWidth(false));
						//~ perk.enableInlvl=EditorGUILayout.Toggle(perk.enableInlvl, GUILayout.MaxWidth(50), GUILayout.ExpandWidth(false));
						//~ perk.unlocked=EditorGUILayout.Toggle(perk.unlocked, GUILayout.MaxWidth(50), GUILayout.ExpandWidth(false));
					//~ }
					//~ else{
						//~ perk.enableInlvl=EditorGUILayout.Toggle("enabled:", perk.enableInlvl);
					//~ }
					//~ GUILayout.EndHorizontal();
				//~ GUILayout.EndVertical();
			
			//~ GUILayout.EndHorizontal();
		
			
		//~ }
		
		EditorGUILayout.Space();
		
		
		if(GUI.changed){
			EditorUtility.SetDirty(path);
			Undo.CreateSnapshot();
            Undo.RegisterSnapshot();
        }
        Undo.ClearSnapshotTarget();
	}
	
	
	
	
	
}
