using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(PerkManager))]
public class PerkEditor : Editor {

	static private PerkManager pm;
	
	void Awake(){
		pm = (PerkManager)target;
		
		if(!EditorApplication.isPlaying) VerifyingList();
		
		EditorUtility.SetDirty(pm);
	}
	
	void OnEnable(){
		PerkEditorWindow.onPerksUpdateE+=VerifyingList;
	}
	
	void OnDisable(){
		PerkEditorWindow.onPerksUpdateE-=VerifyingList;
	}
	
	public void VerifyingList(){
		Undo.SetSnapshotTarget(pm, "PerkManager");
		
		List<Perk> list=PerkEditorWindow.Load();
		
		int n=0;
		foreach(Perk perk in pm.allPerkList){
			if(perk.enableInlvl) n+=1;
		}
		int m=0;
		foreach(Perk perk in list){
			if(perk.enableInlvl) m+=1;
		}
		
		for(int i=0; i<list.Count; i++){
			Perk perk=list[i];
			foreach(Perk p in pm.allPerkList){
				if(perk.ID==p.ID){
					//~ if(perk.name=="CanonTower") Debug.Log(perk.name+"   "+p.unlocked);
					//~ Debug.Log(p.ID+"  "+p.name+"    "+p.enableInlvl);
					perk.enableInlvl=p.enableInlvl;
					perk.unlocked=p.unlocked;
				}
			}
		}
		
		pm.allPerkList=list;
		EditorUtility.SetDirty(pm);
		
		Undo.CreateSnapshot();
		Undo.RegisterSnapshot();
		Undo.ClearSnapshotTarget();
	}
	
	private Vector2 scrollPosition;
	
	public override void OnInspectorGUI(){
		
		Undo.SetSnapshotTarget(pm, "PerkManager");
		
		GUI.changed = false;
		
		EditorGUILayout.Space();
		
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Disable All")){
			foreach(Perk perk in pm.allPerkList) perk.enableInlvl=false;
		}
		if(GUILayout.Button("Enable All")){
			foreach(Perk perk in pm.allPerkList) perk.enableInlvl=true;
		}
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Locked All")){
			foreach(Perk perk in pm.allPerkList) perk.unlocked=false;
		}
		if(GUILayout.Button("Unlocked All")){
			foreach(Perk perk in pm.allPerkList) perk.unlocked=true;
		}
		GUILayout.EndHorizontal();
		
		EditorGUILayout.Space();
		
		scrollPosition = GUILayout.BeginScrollView (scrollPosition);
		
		for(int i=0; i<pm.allPerkList.Count; i++){
			Perk perk=pm.allPerkList[i];
			
			GUILayout.BeginHorizontal();
					
				GUIContent cont=new GUIContent(perk.icon, perk.desp);
				GUILayout.Box(cont, GUILayout.Width(30),  GUILayout.Height(30));
				//~ GUILayout.Box(perk.icon, GUILayout.Width(30),  GUILayout.Height(30));
				//~ GUILayout.Label(perk.icon, GUILayout.Width(50),  GUILayout.Height(50));
		
				GUILayout.BeginVertical();
					//~ GUILayout.Space (5);
					GUILayout.Label(perk.name, GUILayout.ExpandWidth(false));
					//~ perk.enableInlvl=EditorGUILayout.Toggle("enabled:", perk.enableInlvl, GUILayout.MaxWidth(50), GUILayout.ExpandWidth(false));
					//~ if(perk.enableInlvl){
						//~ perk.unlocked=EditorGUILayout.Toggle("unlocked:", perk.unlocked, GUILayout.MaxWidth(50), GUILayout.ExpandWidth(false));
					//~ }
					GUILayout.BeginHorizontal();
					
					//~ if(perk.enableInlvl){
						GUILayout.Label("Enabled/Unlocked:", GUILayout.ExpandWidth(false));
						perk.enableInlvl=EditorGUILayout.Toggle(perk.enableInlvl, GUILayout.MaxWidth(50), GUILayout.ExpandWidth(false));
						perk.unlocked=EditorGUILayout.Toggle(perk.unlocked, GUILayout.MaxWidth(50), GUILayout.ExpandWidth(false));
					//~ }
					//~ else{
						//~ perk.enableInlvl=EditorGUILayout.Toggle("enabled:", perk.enableInlvl);
					//~ }
					GUILayout.EndHorizontal();
				GUILayout.EndVertical();
			
			GUILayout.EndHorizontal();
		
			//~ EditorGUILayout.Space();
		}
		
		GUILayout.EndScrollView ();
		
	
		if(GUI.changed){
			EditorUtility.SetDirty(pm);
			Undo.CreateSnapshot();
            Undo.RegisterSnapshot();
        }
        Undo.ClearSnapshotTarget();
	}
	
	
	
	
	
}
