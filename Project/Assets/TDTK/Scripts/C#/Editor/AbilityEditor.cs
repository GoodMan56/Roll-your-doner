using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;


[CustomEditor(typeof(AbilityManager))]
public class AbilityEditor : Editor {

	static private AbilityManager am;
	
	void Awake(){
		am = (AbilityManager)target;
		
		if(!EditorApplication.isPlaying) VerifyingList();
		
		EditorUtility.SetDirty(am);
	}
	
	
	public void VerifyingList(){
		Undo.SetSnapshotTarget(am, "AbilityManager");
		
		List<Ability> list=AbilityEditorWindow.Load();
		
		for(int i=0; i<list.Count; i++){
			list[i]=list[i].Clone();
		}
		
		//~ int n=0;
		//~ foreach(Ability ability in am.allAbilityList){
			//~ if(ability.enableInlvl) n+=1;
		//~ }
		//~ int m=0;
		//~ foreach(Ability ability in list){
			//~ if(ability.enableInlvl) m+=1;
		//~ }
		
		for(int i=0; i<list.Count; i++){
			Ability ability=list[i];
			foreach(Ability a in am.allAbilityList){
				if(ability.ID==a.ID){
					ability.enableInlvl=a.enableInlvl;
				}
			}
		}
		
		am.allAbilityList=list;
		EditorUtility.SetDirty(am);
		
		Undo.CreateSnapshot();
		Undo.RegisterSnapshot();
		Undo.ClearSnapshotTarget();
	}
	
	

	private Vector2 scrollPosition;
	
	// Update is called once per frame
	public override void OnInspectorGUI () {
		Undo.SetSnapshotTarget(am, "AbilityManager");
		
		GUI.changed = false;
		
		DrawDefaultInspector();
		
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		
		GUILayout.Label("Abilities List:");
		EditorGUILayout.Space();
		
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Disable All")){
			foreach(Ability ability in am.allAbilityList) ability.enableInlvl=false;
		}
		if(GUILayout.Button("Enable All")){
			foreach(Ability ability in am.allAbilityList) ability.enableInlvl=true;
		}
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
//		if(GUILayout.Button("Locked All")){
//			foreach(Perk perk in am.allAbilityList) perk.unlocked=false;
//		}
//		if(GUILayout.Button("Unlocked All")){
//			foreach(Perk perk in am.allAbilityList) perk.unlocked=true;
//		}
		GUILayout.EndHorizontal();
		
		EditorGUILayout.Space();
		
		
		
		scrollPosition = GUILayout.BeginScrollView (scrollPosition);
		
		for(int i=0; i<am.allAbilityList.Count; i++){
			Ability ability=am.allAbilityList[i];
			
			GUILayout.BeginHorizontal();
				
				GUIContent cont=new GUIContent(ability.icon, ability.desp);
				GUILayout.Box(cont, GUILayout.Width(30),  GUILayout.Height(30));
				
				GUILayout.BeginVertical();
					GUILayout.Label(ability.name+"  "+ability.ID, GUILayout.ExpandWidth(false));
					GUILayout.BeginHorizontal();
					
						GUILayout.Label("Enabled:", GUILayout.ExpandWidth(false));
						ability.enableInlvl=EditorGUILayout.Toggle(ability.enableInlvl, GUILayout.MaxWidth(50), GUILayout.ExpandWidth(false));
						//perk.unlocked=EditorGUILayout.Toggle(perk.unlocked, GUILayout.MaxWidth(50), GUILayout.ExpandWidth(false));
					
					GUILayout.EndHorizontal();
				GUILayout.EndVertical();
			
			GUILayout.EndHorizontal();
		
		}
		
		GUILayout.EndScrollView ();
		
	
		if(GUI.changed){
			EditorUtility.SetDirty(am);
			Undo.CreateSnapshot();
            Undo.RegisterSnapshot();
        }
        Undo.ClearSnapshotTarget();
	}
	
}
