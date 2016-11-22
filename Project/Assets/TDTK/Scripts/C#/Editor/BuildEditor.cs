using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(BuildManager))]
public class BuildEditor : Editor {

	static private BuildManager bm;
	
	static UnitTower[] towerList=new UnitTower[0];
	//~ [SerializeField] static string[] nameList=new string[0];
	static bool showTowerList=true;
	
	//~ string[] nameList=new string[0];
	
	void Awake(){
		bm = (BuildManager)target;
		
		//~ VerifyingList();
		GetTower();
		//~ if(!EditorApplication.isPlaying ) GetTower();
		
		EditorUtility.SetDirty(bm);
	}
	
	static void GetTower(){
		towerList=new UnitTower[0];
		//~ nameList=new string[0];
		
		GameObject obj=Resources.Load("PrefabListTower", typeof(GameObject)) as GameObject;
		if(obj!=null){
			TowerListPrefab prefab=obj.GetComponent<TowerListPrefab>();
			
			if(prefab!=null){
				for(int i=0; i<prefab.towerList.Count; i++){
					if(prefab.towerList[i]==null){
						prefab.towerList.RemoveAt(i);
						i-=1;
					}
				}
				
				towerList=new UnitTower[prefab.towerList.Count];
				bm.towers=new UnitTower[prefab.towerList.Count];
				//~ nameList=new string[prefab.towerList.Count];
				
				for(int i=0; i<prefab.towerList.Count; i++){
					towerList[i]=prefab.towerList[i];
					bm.towers[i]=prefab.towerList[i];
				}
			}
		}
		
		List<TowerAvailability> tempAvaiList=new List<TowerAvailability>();
		foreach(UnitTower tower in towerList){
			bool match=false;
			foreach(TowerAvailability avai in bm.towerAvaiList){
				if(avai.ID==tower.prefabID){
					match=true;
					tempAvaiList.Add(avai);
					break;
				}
			}
			if(!match) tempAvaiList.Add(new TowerAvailability(tower.prefabID));
		}
		bm.towerAvaiList=tempAvaiList;
		
	}
	
	void OnEnable(){
		//~ PerkEditorX.onPerksUpdateE+=VerifyingList;
	}
	
	void OnDisable(){
		//~ PerkEditorX.onPerksUpdateE-=VerifyingList;
	}
	
	public void VerifyingList(){
		//~ List<Perk> list=PerkEditorX.Load();
		
		//~ for(int i=0; i<list.Count; i++){
			//~ Perk perk=list[i];
			//~ foreach(Perk p in bm.allPerkList){
				//~ if(perk.ID==p.ID){
					//~ perk.enableInlvl=p.enableInlvl;
					//~ perk.unlocked=p.unlocked;
				//~ }
			//~ }
		//~ }
		
		//~ bm.allPerkList=list;
	}
	
	private Vector2 scrollPosition;
	
	public override void OnInspectorGUI(){
		
		Undo.SetSnapshotTarget(bm, "BuildManager");
		
		GUI.changed = false;
		
		EditorGUILayout.Space();
		
		DrawDefaultInspector();
		EditorGUILayout.Space();
		
		showTowerList=GUILayout.Toggle(showTowerList, "Show Tower List");
		
		if(showTowerList){
			
			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button("EnableAll")){
				for(int i=0; i<bm.towerAvaiList.Count; i++){
					bm.towerAvaiList[i].enabledInLvl=true;
				}
			}
			if(GUILayout.Button("DisableAll")){
				for(int i=0; i<bm.towerAvaiList.Count; i++){
					bm.towerAvaiList[i].enabledInLvl=false;
				}
			}
			EditorGUILayout.EndHorizontal ();
			
			scrollPosition = GUILayout.BeginScrollView (scrollPosition);
			
			for(int i=0; i<bm.towerAvaiList.Count; i++){
				//~ Perk perk=bm.allPerkList[i];
				UnitTower tower=towerList[i];
				
				GUILayout.BeginHorizontal();
					
					GUIContent cont=new GUIContent(tower.icon, tower.description);
					GUILayout.Box(cont, GUILayout.Width(30),  GUILayout.Height(30));
					
					GUILayout.BeginVertical();
						//~ GUILayout.Space (5);
						GUILayout.Label(tower.name, GUILayout.ExpandWidth(false));
						bm.towerAvaiList[i].enabledInLvl=EditorGUILayout.Toggle("enabled:", bm.towerAvaiList[i].enabledInLvl);
						//~ if(perk.enableInlvl){
							//~ perk.unlocked=EditorGUILayout.Toggle("unlocked at start:", perk.unlocked);
						//~ }
					GUILayout.EndVertical();
				
				GUILayout.EndHorizontal();
			
				//~ EditorGUILayout.Space();
			}
			
			GUILayout.EndScrollView ();
		}
		
		EditorGUILayout.Space();
		
		if(GUI.changed){
			EditorUtility.SetDirty(bm);
			Undo.CreateSnapshot();
            Undo.RegisterSnapshot();
        }
        Undo.ClearSnapshotTarget();
	}
	
	
	
	
	
}
