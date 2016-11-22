using UnityEngine;
using UnityEditor;

using System;
using System.Xml;
using System.IO;

using System.Collections;
using System.Collections.Generic;

public class TowerManager : EditorWindow {

	public delegate void UpdateHandler(); 
	public static event UpdateHandler onTowerUpdateE;
	
	static private TowerManager window;
	
	private static TowerListPrefab prefab;
	private static List<UnitTower> towerList=new List<UnitTower>();
	private static List<int> towerIDList=new List<int>();
	//~ private static List<Tower> towerList=new List<Resource>();
	
    // Add menu named "PerkEditor" to the Window menu
    //[MenuItem ("TDTK/PerkEditor")]
    public static void Init () {
        // Get existing open window or if none, make a new one:
        window = (TowerManager)EditorWindow.GetWindow(typeof (TowerManager));
		window.minSize=new Vector2(375, 449);
		window.maxSize=new Vector2(375, 800);
		
		GameObject obj=Resources.Load("PrefabListTower", typeof(GameObject)) as GameObject;
		
		if(obj==null) obj=CreatePrefab();
		
		prefab=obj.GetComponent<TowerListPrefab>();
		if(prefab==null) prefab=obj.AddComponent<TowerListPrefab>();
		
		towerList=prefab.towerList;
		
		for(int i=0; i<towerList.Count; i++){
			//~ towerList[i].prefabID=i;
			if(towerList[i]!=null) towerIDList.Add(towerList[i].prefabID);
			else{
				towerList.RemoveAt(i);
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
		obj.AddComponent<TowerListPrefab>();
		GameObject prefab=PrefabUtility.CreatePrefab("Assets/TDTK/Resources/PrefabListTower.prefab", obj, ReplacePrefabOptions.ConnectToPrefab);
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
	//int currentRscID=0;
	private Vector2 scrollPos;
	//~ string warning="";
	
	//~ void Test(){
		//~ string sourcePath=AssetDatabase.GetAssetPath(resourceList[1].icon);
		//~ string targetPath=Application.dataPath  + "/TDTK/Resources/";
		//~ if(AssetDatabase.LoadAssetAtPath(targetPath+resourceList[0].icon.name, typeof(Texture2D))==null){
			//~ AssetDatabase.CopyAsset(sourcePath, targetPath);
		//~ }
		//~ AssetDatabase.Refresh();
	//~ }
	
	//~ void ClearWarning(){
		//~ warning="";
	//~ }
	//~ void UpdateWarning(){
		//~ warning="Please note that you need to save and refresh Resource\nManager in each scene for the new change to take effect.";
	//~ }
	
	
	void OnGUI () {
		if(window==null) Init();
		
		Undo.SetSnapshotTarget(this, "ResourceEditor");
		
		//~ if(towerList.Count%2==0) GUI.color=new Color(.8f, .8f, .8f, 1);
		//~ else GUI.color=Color.white;
		//~ GUI.Box(new Rect(5, 75+towerList.Count*49, window.position.width-10, 50), "");
		//~ GUI.color=Color.white;
		EditorGUI.LabelField(new Rect(5, 10, 150, 17), "Add new tower:");
		newTower=(UnitTower)EditorGUI.ObjectField(new Rect(100, 10, 160, 17), newTower, typeof(UnitTower), false);
		if(newTower!=null){
			if(!towerList.Contains(newTower)){
				//~ if(!towerIDList.Contains(newTower.prefabID)) towerIDList.Add(newTower.prefabID);
				//~ else{
					int rand=0;
					while(towerIDList.Contains(rand)) rand+=1;
					towerIDList.Add(rand);
					newTower.prefabID=rand;
				//~ }
				towerList.Add(newTower);
				if(onTowerUpdateE!=null) onTowerUpdateE();
			}
			newTower=null;
		}

		
		//~ if(GUI.Button(new Rect(window.position.width-85, 10, 80, 30), "Save")){
			//~ warning="Refresh ResourceManager in each scene for the new \nchange to take effect!";
			//~ SaveToXML();
		//~ }
		
		//~ if(GUI.Button(new Rect(10, 10, 100, 30), "New Resource")){
			//~ UpdateWarning();
			//~ towerList.Add(new UnitTower());
		//~ }
		
		if(towerList.Count>0){
			GUI.Box(new Rect(5, 40, 50, 20), "ID");
			GUI.Box(new Rect(5+50-1, 40, 60+1, 20), "Icon");
			GUI.Box(new Rect(5+110-1, 40, 160+2, 20), "Name - Tower Type");
			GUI.Box(new Rect(5+270, 40, window.position.width-300, 20), "");
		}
		
		scrollPos = GUI.BeginScrollView(new Rect(5, 60, window.position.width-12, window.position.height-50), scrollPos, new Rect(5, 55, window.position.width-30, 15+((towerList.Count))*50));
		
		int row=0;
		for(int i=0; i<towerList.Count; i++){
			if(i%2==0) GUI.color=new Color(.8f, .8f, .8f, 1);
			else GUI.color=Color.white;
			GUI.Box(new Rect(5, 60+i*49, window.position.width-30, 50), "");
			GUI.color=Color.white;
			
			//~ GUI.Label(new Rect(22, 15+60+i*49, 50, 20), towerList[i].prefabID.ToString());
			if(currentSwapID==i) GUI.color=new Color(.9f, .9f, .0f, 1);
			if(GUI.Button(new Rect(19, 12+60+i*49, 30, 30), towerList[i].prefabID.ToString())){
				if(currentSwapID==i) currentSwapID=-1;
				else if(currentSwapID==-1) currentSwapID=i;
				else SwapTower(i);
			}
			if(currentSwapID==i) GUI.color=Color.white;
			
			if(towerList[i]!=null){
				towerList[i].icon=(Texture)EditorGUI.ObjectField(new Rect(12+50, 3+60+i*49, 44, 44), towerList[i].icon, typeof(Texture), false);
				
				towerList[i].unitName=EditorGUI.TextField(new Rect(5+120, 6+60+i*49, 150, 17), towerList[i].unitName);
				
				EditorGUI.LabelField(new Rect(5+120, 10+60+i*49+18, 150, 17), " - "+towerList[i].type.ToString());
				//~ towerList[i]=(UnitTower)EditorGUI.ObjectField(new Rect(5+120, 10+75+i*49+18, 150, 17), towerList[i], typeof(UnitTower), false);
			}
			else{
				//~ towerList[i]=(UnitTower)EditorGUI.ObjectField(new Rect(5+120, 15+75+i*49, 150, 17), towerList[i], typeof(UnitTower), false);
				//~ towerIDList.Remove(towerList[i].prefabID);
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
					towerIDList.Remove(towerList[i].prefabID);
					towerList.RemoveAt(i);
					delete=-1;
					if(onTowerUpdateE!=null) onTowerUpdateE();
				}
				GUI.color = Color.white;
			}
			
			row+=1;
		}
		
		
		GUI.EndScrollView();
		//~ GUI.Label(new Rect(5, window.position.height-55, window.position.width-10, 55), warning);
		
		if(GUI.changed){
			EditorUtility.SetDirty(prefab);
			for(int i=0; i<towerList.Count; i++) EditorUtility.SetDirty(towerList[i]);
		}
		
		
		if (GUI.changed){
			//~ EditorUtility.SetDirty(this);
			Undo.CreateSnapshot();
			Undo.RegisterSnapshot();
		}
		Undo.ClearSnapshotTarget();
		
	}
	
	private int currentSwapID=-1;
	void SwapTower(int ID){
		UnitTower tower=towerList[currentSwapID];
		towerList[currentSwapID]=towerList[ID];
		towerList[ID]=tower;
		
		currentSwapID=-1;
	}
	
	private UnitTower newTower=null;
}

//~ public Tower
