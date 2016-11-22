using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(PlatformTD))]
public class PlatformEditor : Editor {

	static private PlatformTD platform;
	string[] typeLabel;
	
	static bool showTowerList=true;
	static UnitTower[] towerList=new UnitTower[0];
	
	void Awake(){
		platform = (PlatformTD)target;
		
		GetTower();
		
		EditorUtility.SetDirty(platform);
		
		//~ typeLabel=new string[7];
		//~ typeLabel[0]="TurretTower";
		//~ typeLabel[1]="AOETower";
		//~ typeLabel[2]="DirectionalAOETower";
		//~ typeLabel[3]="SupportTower";
		//~ typeLabel[4]="ResourceTower";
		//~ typeLabel[5]="Mine";
		//~ typeLabel[6]="Block";
	}
	
	
	void UpdateBuildableTypeLength(int num){
		_TowerType[] tempList=new _TowerType[num];
		for(int i=0; i<tempList.Length; i++){
			if(i>=platform.buildableType.Length){
				tempList[i]=_TowerType.TurretTower;
			}
			else{
				tempList[i]=platform.buildableType[i];
			}
		}
		platform.buildableType=tempList;
	}
	
	static void GetTower(){
		towerList=new UnitTower[0];
		//~ nameList=new string[0];
		
		GameObject obj=Resources.Load("PrefabListTower", typeof(GameObject)) as GameObject;
		if(obj!=null){
			TowerListPrefab prefab=obj.GetComponent<TowerListPrefab>();
			
			if(prefab!=null){
				towerList=new UnitTower[prefab.towerList.Count];
				//~ platform.towers=new UnitTower[prefab.towerList.Count];
				//~ nameList=new string[prefab.towerList.Count];
				
				for(int i=0; i<prefab.towerList.Count; i++){
					towerList[i]=prefab.towerList[i];
					//~ platform.towers[i]=prefab.towerList[i];
				}
			}
		}
		
		if(!EditorApplication.isPlaying){
			List<TowerAvailability> tempAvaiList=new List<TowerAvailability>();
			foreach(UnitTower tower in towerList){
				bool match=false;
				foreach(TowerAvailability avai in platform.towerAvaiList){
					if(avai.ID==tower.prefabID){
						match=true;
						tempAvaiList.Add(avai);
						break;
					}
				}
				if(!match) tempAvaiList.Add(new TowerAvailability(tower.prefabID));
			}
			platform.towerAvaiList=tempAvaiList;
		}
		//~ Debug.Log(towerList.Length+"  "+platform.towerAvaiList.Count);
	}
	
	public override void OnInspectorGUI(){
		
		Undo.SetSnapshotTarget(platform, "BuildPlatform");
		
		GUI.changed = false;
		
		//~ EditorGUILayout.Space();
		
		//~ DrawDefaultInspector();
		
		//~ GUILayout.Label("Buildable Type");
		//~ EditorGUILayout.BeginHorizontal();
		//~ GUILayout.Space(20);
		//~ int num=platform.buildableType.Length;
		//~ num=EditorGUILayout.IntField ("Size", num);
		//~ if(num!=platform.buildableType.Length) UpdateBuildableTypeLength(num);
		//~ EditorGUILayout.EndHorizontal ();
		
		//~ for(int i=0; i<platform.buildableType.Length; i++){
			//~ EditorGUILayout.BeginHorizontal();
			//~ GUILayout.Space(20);
			//~ int index=(int)platform.buildableType[i];
			//~ index=EditorGUILayout.Popup("Element"+i, index, typeLabel);
			//~ platform.buildableType[i]=(_TowerType)index;
			//~ EditorGUILayout.EndHorizontal ();
		//~ }
		
		platform.GizmoShowNodes=GUILayout.Toggle(platform.GizmoShowNodes, "Gizmo Show Nodes");
		platform.GizmoShowPath=GUILayout.Toggle(platform.GizmoShowPath, "Gizmo Show Path");
		
		EditorGUILayout.Space();
		
		showTowerList=GUILayout.Toggle(showTowerList, "Show Tower List");
		
		
		
		if(showTowerList){
			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button("EnableAll")){
				for(int i=0; i<platform.towerAvaiList.Count; i++){
					platform.towerAvaiList[i].enabledInLvl=true;
				}
			}
			if(GUILayout.Button("DisableAll")){
				for(int i=0; i<platform.towerAvaiList.Count; i++){
					platform.towerAvaiList[i].enabledInLvl=false;
				}
			}
			EditorGUILayout.EndHorizontal ();
			
			for(int i=0; i<platform.towerAvaiList.Count; i++){
				UnitTower tower=towerList[i];
				
				GUILayout.BeginHorizontal();
						
					GUILayout.Box(tower.icon, GUILayout.Width(30),  GUILayout.Height(30));
					
					GUILayout.BeginVertical();
						GUILayout.Label(tower.name);
						platform.towerAvaiList[i].enabledInLvl=EditorGUILayout.Toggle("enabled:", platform.towerAvaiList[i].enabledInLvl);
					GUILayout.EndVertical();
				
				GUILayout.EndHorizontal();
			
			}
		}
		
		EditorGUILayout.Space();
		
		if(GUI.changed){
			EditorUtility.SetDirty(platform);
			Undo.CreateSnapshot();
            Undo.RegisterSnapshot();
        }
        Undo.ClearSnapshotTarget();
	}
	
}
