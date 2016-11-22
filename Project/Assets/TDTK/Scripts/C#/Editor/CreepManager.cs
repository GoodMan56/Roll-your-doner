using UnityEngine;
using UnityEditor;

using System;
using System.Xml;
using System.IO;

using System.Collections;
using System.Collections.Generic;

public class CreepManager : EditorWindow {

	public delegate void UpdateHandler(); 
	public static event UpdateHandler onCreepUpdateE;
	
	static private CreepManager window;
	
	private static CreepListPrefab prefab;
	private static List<UnitCreep> creepList=new List<UnitCreep>();
	private static List<int> creepIDList=new List<int>();
	//~ private static List<Tower> towerList=new List<Resource>();
	
    // Add menu named "PerkEditor" to the Window menu
    //[MenuItem ("TDTK/PerkEditor")]
    public static void Init () {
        // Get existing open window or if none, make a new one:
        window = (CreepManager)EditorWindow.GetWindow(typeof (CreepManager));
		window.minSize=new Vector2(375, 449);
		window.maxSize=new Vector2(375, 800);
		
		GameObject obj=Resources.Load("PrefabListCreep", typeof(GameObject)) as GameObject;
		
		if(obj==null) obj=CreatePrefab();
		
		prefab=obj.GetComponent<CreepListPrefab>();
		if(prefab==null) prefab=obj.AddComponent<CreepListPrefab>();
		
		creepList=prefab.creepList;
		
		for(int i=0; i<creepList.Count; i++){
			//creepList[i].prefabID=i;
			if(creepList[i]!=null) creepIDList.Add(creepList[i].prefabID);
			else{
				creepList.RemoveAt(i);
				i-=1;
			}
		}
		
		
		
		//~ resourceList.Add(new Resource());
		
		//~ Load();
		
		//AssetDatabase.GetAssetPath 
		
		//~ for(int i=0; i<towerList.Count; i++) EditorUtility.SetDirty(towerList[i]);
    }
	
	private static GameObject CreatePrefab(){
		GameObject obj=new GameObject();
		obj.AddComponent<CreepListPrefab>();
		GameObject prefab=PrefabUtility.CreatePrefab("Assets/TDTK/Resources/PrefabListCreep.prefab", obj, ReplacePrefabOptions.ConnectToPrefab);
		DestroyImmediate(obj);
		AssetDatabase.Refresh ();
		return prefab;
	}
	
	private void OnEnable()
    {
        //~ EditorApplication.modifierKeysChanged += this.Repaint;
    }

    private void OnDisable()
    {
        //~ EditorApplication.modifierKeysChanged -= this.Repaint;
    }
	
	int delete=-1;
	private Vector2 scrollPos;
	
	void OnGUI () {
		if(window==null) Init();
		
		Undo.SetSnapshotTarget(this, "ResourceEditor");
		
		//~ if(towerList.Count%2==0) GUI.color=new Color(.8f, .8f, .8f, 1);
		//~ else GUI.color=Color.white;
		//~ GUI.Box(new Rect(5, 75+towerList.Count*49, window.position.width-10, 50), "");
		//~ GUI.color=Color.white;
		EditorGUI.LabelField(new Rect(5, 10, 150, 17), "Add new creep:");
		newCreep=(UnitCreep)EditorGUI.ObjectField(new Rect(100, 10, 160, 17), newCreep, typeof(UnitCreep), false);
		if(newCreep!=null){
			if(!creepList.Contains(newCreep)){
				//~ if(!towerIDList.Contains(newTower.specialID)) towerIDList.Add(newTower.specialID);
				//~ else{
					int rand=0;
					while(creepIDList.Contains(rand)) rand+=1;
					creepIDList.Add(rand);
					newCreep.prefabID=rand;
				//~ }
				creepList.Add(newCreep);
				if(onCreepUpdateE!=null) onCreepUpdateE();
			}
			newCreep=null;
		}

		
		//~ if(GUI.Button(new Rect(window.position.width-85, 10, 80, 30), "Save")){
			//~ warning="Refresh ResourceManager in each scene for the new \nchange to take effect!";
			//~ SaveToXML();
		//~ }
		
		//~ if(GUI.Button(new Rect(10, 10, 100, 30), "New Resource")){
			//~ UpdateWarning();
			//~ towerList.Add(new UnitTower());
		//~ }
		
		if(creepList.Count>0){
			GUI.Box(new Rect(5, 40, 50, 20), "ID");
			GUI.Box(new Rect(5+50-1, 40, 60+1, 20), "Icon");
			GUI.Box(new Rect(5+110-1, 40, 160+2, 20), "Name");
			GUI.Box(new Rect(5+270, 40, window.position.width-300, 20), "");
		}
		
		scrollPos = GUI.BeginScrollView(new Rect(5, 60, window.position.width-12, window.position.height-50), scrollPos, new Rect(5, 55, window.position.width-30, 15+((creepList.Count))*50));
		
		int row=0;
		for(int i=0; i<creepList.Count; i++){
			if(i%2==0) GUI.color=new Color(.8f, .8f, .8f, 1);
			else GUI.color=Color.white;
			GUI.Box(new Rect(5, 60+i*49, window.position.width-30, 50), "");
			GUI.color=Color.white;
			
			//~ GUI.Label(new Rect(22, 15+60+i*49, 50, 20), creepList[i].prefabID.ToString());
			if(currentSwapID==i) GUI.color=new Color(.9f, .9f, .0f, 1);
			if(GUI.Button(new Rect(19, 12+60+i*49, 30, 30), creepList[i].prefabID.ToString())){
				if(currentSwapID==i) currentSwapID=-1;
				else if(currentSwapID==-1) currentSwapID=i;
				else SwapCreep(i);
			}
			if(currentSwapID==i) GUI.color=Color.white;
			
			if(creepList[i]!=null){
				creepList[i].icon=(Texture)EditorGUI.ObjectField(new Rect(12+50, 3+60+i*49, 44, 44), creepList[i].icon, typeof(Texture), false);
				
				creepList[i].unitName=EditorGUI.TextField(new Rect(5+120, 6+60+i*49, 150, 17), creepList[i].unitName);
				
				//~ EditorGUI.LabelField(new Rect(5+120, 10+60+i*49+18, 150, 17), " - "+creepList[i].type.ToString());
				//~ towerList[i]=(UnitTower)EditorGUI.ObjectField(new Rect(5+120, 10+75+i*49+18, 150, 17), towerList[i], typeof(UnitTower), false);
			}
			else{
				//~ towerList[i]=(UnitTower)EditorGUI.ObjectField(new Rect(5+120, 15+75+i*49, 150, 17), towerList[i], typeof(UnitTower), false);
				//~ towerIDList.Remove(towerList[i].specialID);
				//~ towerList.RemoveAt(i);
				//~ delete=-1;
				//~ if(onTowerUpdateE!=null) onTowerUpdateE();
			}
			
			if(delete!=i){
				if(GUI.Button(new Rect(window.position.width-55, 12+60+i*49, 25, 25), "X")){
					delete=i;
				}
			}
			else{
				GUI.color = Color.red;
				if(GUI.Button(new Rect(window.position.width-90, 12+60+i*49, 60, 25), "Remove")){
					if(currentSwapID==i) currentSwapID=-1;
					creepIDList.Remove(creepList[i].prefabID);
					creepList.RemoveAt(i);
					delete=-1;
					if(onCreepUpdateE!=null) onCreepUpdateE();
				}
				GUI.color = Color.white;
			}
			
			row+=1;
		}
		
		
		GUI.EndScrollView();
		//~ GUI.Label(new Rect(5, window.position.height-55, window.position.width-10, 55), warning);
		
		if(GUI.changed){
			EditorUtility.SetDirty(prefab);
			for(int i=0; i<creepList.Count; i++) EditorUtility.SetDirty(creepList[i]);
		}
		
		
		if (GUI.changed){
			//~ EditorUtility.SetDirty(this);
			Undo.CreateSnapshot();
			Undo.RegisterSnapshot();
		}
		Undo.ClearSnapshotTarget();
		
	}
	
	
	private int currentSwapID=-1;
	void SwapCreep(int ID){
		UnitCreep creep=creepList[currentSwapID];
		creepList[currentSwapID]=creepList[ID];
		creepList[ID]=creep;
		
		currentSwapID=-1;
	}
	
	
	private UnitCreep newCreep=null;
}

//~ public Tower
