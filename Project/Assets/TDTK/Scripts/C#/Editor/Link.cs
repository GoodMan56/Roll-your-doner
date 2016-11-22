using UnityEngine;
using UnityEditor;

using System.Collections;

public class Link : EditorWindow {
	
	[MenuItem ("TDTK/New Scene (Linear Path)", false, -100)]
    static void New1 () { NewScene(false); }
	
	[MenuItem ("TDTK/New Scene (Mazing)", false, -100)]
    static void New2 () { NewScene(true); }
	
    static void NewScene (bool flag) {
		EditorApplication.NewScene();
		
		GameObject rootObj=new GameObject();
		rootObj.name="TDTK";
		Transform rootT=rootObj.transform;
		
    	GameObject obj=new GameObject(); obj.transform.parent=rootT;
		obj.name="GameControl";
		GameControl gameControl=obj.AddComponent<GameControl>();
		
		ResourceManager rscManager=obj.GetComponent<ResourceManager>();
		rscManager.InitResources();
		foreach(Resource rsc in rscManager.resources){
			rsc.value=500;
		}
		
		obj=new GameObject(); obj.transform.parent=rootT;
		obj.name="SpawnManager";
		SpawnManager spawnManager=obj.AddComponent<SpawnManager>();
		
		gameControl.spawnManager=spawnManager;
		gameControl.rangeIndicatorH=Resources.Load("RangeIndicatorH", typeof(Transform)) as Transform;
		gameControl.rangeIndicatorF=Resources.Load("RangeIndicatorF", typeof(Transform)) as Transform;
		gameControl.rangeIndicatorConeH=Resources.Load("RangeIndicatorConeH", typeof(Transform)) as Transform;
		
		obj=new GameObject(); obj.transform.parent=rootT;
		obj.name="BuildManager";
		BuildManager buildManager=obj.AddComponent<BuildManager>();
		buildManager.InitTower();
		
		Camera.main.transform.position=new Vector3(0, 12, -12);
		Camera.main.transform.rotation=Quaternion.Euler(45, 0, 0);
		
		obj=new GameObject(); obj.transform.parent=rootT;
		obj.name="Path1";
		PathTD path=obj.AddComponent<PathTD>();
		if(flag) path.waypoints=new Transform[3];
		else path.waypoints=new Transform[2];
		
		obj=new GameObject();
		obj.name="waypoint1";
		obj.transform.position=new Vector3(-12, 0, 0);
		obj.transform.parent=path.transform;
		path.waypoints[0]=obj.transform;
		
		obj=new GameObject();
		obj.name="waypoint2";
		obj.transform.position=new Vector3(12, 0, 0);
		obj.transform.parent=path.transform;
		if(flag) path.waypoints[2]=obj.transform;
		else path.waypoints[1]=obj.transform;
		
		obj=GameObject.CreatePrimitive(PrimitiveType.Plane); obj.transform.parent=rootT;
		obj.name="BuildPlatform";
		
		if(flag){
			obj.transform.localScale=new Vector3(1.8f, 1, 1.15f);
			obj.transform.position=Vector3.zero;
			obj.AddComponent<PlatformTD>();
		}
		else{
			obj.transform.localScale=new Vector3(1.8f, 1, 0.3f);
			obj.transform.position=new Vector3(0, 0, -3);
		}
		obj.transform.renderer.material=Resources.Load("grid") as Material;
		buildManager.platforms=new Transform[1];
		buildManager.platforms[0]=obj.transform;
		if(flag) path.waypoints[1]=obj.transform;
		
		if(flag){
			obj=new GameObject(); obj.transform.parent=rootT;
			obj.name="PathFinder";
			obj.AddComponent<NodeGeneratorTD>();
			obj.AddComponent<PathFinderTD>();
		}
		
		spawnManager.defaultPath=path;
		
		//~ obj=new GameObject(); obj.transform.parent=rootT;
		//~ obj.name="UI";
		//~ obj.AddComponent<UI>();
    }
	
	
	[MenuItem ("TDTK/Add AudioManager", false, -10)]
    static void AudioManager (){ 
		if(FindObjectOfType(typeof(AudioManager))!=null) return;
		
		GameObject obj=new GameObject(); 
		obj.name="AudioManager";
		obj.AddComponent<AudioManager>();
		
		GameObject rootObj=GameObject.Find ("TDTK");
		if(rootObj!=null) obj.transform.parent=rootObj.transform;
	}
	
	[MenuItem ("TDTK/Add CameraControl", false, -10)]
    static void CameraControl (){ 
		if(FindObjectOfType(typeof(CameraControl))!=null) return;
		
		GameObject obj=new GameObject(); 
		obj.name="Camera";
		obj.AddComponent<CameraControl>();
		
		Transform cam=Camera.main.transform;
		if(cam==null){
			GameObject camObj=new GameObject();
			camObj.name="CameraObj";
			camObj.tag="MainCamera";
			camObj.AddComponent<Camera>();
			cam=camObj.transform;
		}
		
		cam.parent=obj.transform;
		obj.transform.rotation=Quaternion.Euler(45, 0, 0);
		cam.localPosition=new Vector3(0, 0, -20);
		cam.localRotation=Quaternion.identity;
		
		GameObject rootObj=GameObject.Find ("TDTK");
		if(rootObj!=null) obj.transform.parent=rootObj.transform;
	}
	
	
	[MenuItem ("TDTK/Add AbilityManager", false, -10)]
    static void AbilityManager (){ 
		if(FindObjectOfType(typeof(AbilityManager))!=null) return;
		
		GameObject obj=new GameObject(); 
		obj.name="AbilityManager";
		obj.AddComponent<AbilityManager>();
		
		GameObject rootObj=GameObject.Find ("TDTK");
		if(rootObj!=null) obj.transform.parent=rootObj.transform;
	}
	
	[MenuItem ("TDTK/Add PerkManager", false, -10)]
    static void PerkManager (){ 
		if(FindObjectOfType(typeof(PerkManager))!=null) return;
		
		GameObject obj=new GameObject(); 
		obj.name="PerkManager";
		obj.AddComponent<PerkManager>();
		
		GameObject rootObj=GameObject.Find ("TDTK");
		if(rootObj!=null) obj.transform.parent=rootObj.transform;
	}
	
	
	[MenuItem ("TDTK/Add OverlayManager", false, -10)]
    static void OverlayManager (){ 
		if(FindObjectOfType(typeof(OverlayManager))!=null) return;
		
		GameObject obj=new GameObject(); 
		obj.name="OverlayManager";
		obj.AddComponent<OverlayManager>();
		
		GameObject rootObj=GameObject.Find ("TDTK");
		if(rootObj!=null) obj.transform.parent=rootObj.transform;
	}
	
	
	[MenuItem ("TDTK/Add UI Unity", false, -10)]
    static void UI (){ 
		if(FindObjectOfType(typeof(UI))!=null) return;
		
		GameObject obj=new GameObject(); 
		obj.name="UI";
		obj.AddComponent<UI>();
		
		GameObject rootObj=GameObject.Find ("TDTK");
		if(rootObj!=null) obj.transform.parent=rootObj.transform;
	}
	
	[MenuItem ("TDTK/Add UI NGUI", false, -10)]
    static void NGUI (){ 
		if(FindObjectOfType(typeof(UInGUI))!=null) return;
		
		GameObject obj=Instantiate(Resources.Load("UI NGUI", typeof(GameObject))) as GameObject;
		obj.name="UI NGUI";
		
		GameObject rootObj=GameObject.Find ("TDTK");
		if(rootObj!=null) obj.transform.parent=rootObj.transform;
	}
	
	
	
	[MenuItem ("TDTK/CreepManager", false, 10)]
    static void OpenCreepManager () {
    	CreepManager.Init();
    }
	
	[MenuItem ("TDTK/CreepEditor", false, 10)]
    static void OpenCreepEditor () {
    	CreepEditor.Init();
    }
	
	[MenuItem ("TDTK/TowerManager", false, 10)]
    static void OpenTowerManager () {
    	TowerManager.Init();
    }
	
    [MenuItem ("TDTK/TowerEditor", false, 10)]
    static void OpenTowerEditor() {
    	TowerEditor.Init();
    }
    
    [MenuItem ("TDTK/SpawnEditor", false, 10)]
    static void OpenSpawnEditor () {
    	SpawnEditor.Init();
    }
	
	[MenuItem ("TDTK/AbilityEditor", false, 10)]
    public static void OpenAbilityEditor () {
    	AbilityEditorWindow.Init();
    }
	
	[MenuItem ("TDTK/PerkEditor", false, 10)]
    public static void OpenPerkEditor () {
    	PerkEditorWindow.Init();
    }
	
	[MenuItem ("TDTK/ResourceEditor", false, 10)]
    public static void OpenResourceEditor () {
    	ResourceEditorWindow.Init();
    }
    
    [MenuItem ("TDTK/DamageArmorTable", false, 10)]
    public static void OpenDamageTable () {
    	DamageArmorTableEditor.Init();
    }
    	
    [MenuItem ("TDTK/Support Forum", false, 100)]
    static void OpenForumLink () {
    	Application.OpenURL("http://goo.gl/ZX4OA");
    }
}


