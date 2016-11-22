using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;



public class SpawnEditor : EditorWindow {
	
	private enum _EState{Default, Configure}
	private static _EState state=_EState.Default;
	
	private static List<Resource> rscList=new List<Resource>();
	
	static int waveLength=0;
	static int[] subWaveLength=new int[0];
	
	//static int spawnType=0;
	static string[] spawnLimitLabel=new string[2];
	static string[] spawnLimitTooltip=new string[2];
	static string[] spawnTypeLabel=new string[5];
	static string[] spawnTypeTooltip=new string[5];
	
	static bool[] waveFoldList=new bool[0];
	static bool[] creepFoldList=new bool[0];
	static bool showInfiniteRscModifier=true;
	
	static private SpawnEditor window;

	// Add menu named "TowerEditor" to the Window menu
    //[MenuItem ("TDTK/SpawnEditor")]
    public static void Init(){
        // Get existing open window or if none, make a new one:
        window = (SpawnEditor)EditorWindow.GetWindow(typeof (SpawnEditor));
		window.minSize=new Vector2(670, 620);
		
		int enumLength = Enum.GetValues(typeof(_SpawnLimit)).Length;
		spawnLimitLabel=new string[enumLength];
		spawnLimitLabel[0]="Finite";
		spawnLimitLabel[1]="Infinite";
		
		spawnLimitTooltip=new string[enumLength];
		spawnLimitTooltip[0]="Finite number of waves";
		spawnLimitTooltip[1]="Infinite number of waves (for survival or endless mode)";
		
		enumLength = Enum.GetValues(typeof(_SpawnMode)).Length;
		spawnTypeLabel=new string[enumLength];
		spawnTypeLabel[0]="Continuous";
		spawnTypeLabel[1]="WaveCleared";
		spawnTypeLabel[2]="RoundBased";
		spawnTypeLabel[3]="SkippableContinuous";
		spawnTypeLabel[4]="SkippableWaveCleared";
		
		spawnTypeTooltip=new string[enumLength];
		spawnTypeTooltip[0]="A new wave is spawn upon every wave duration countdown";
		spawnTypeTooltip[1]="A new wave is spawned when the current wave is cleared";
		spawnTypeTooltip[2]="Similar to Continuous but user can initiate the new next wave before the timer runs out";
		spawnTypeTooltip[3]="Similar to WaveCleared but user can initiate the next wave before clearing current wave";
		spawnTypeTooltip[4]="Each wave is treated like a round. a new wave can only take place when the previous wave is cleared. Each round require initiation from user";
		
		LoadCreep();
		GetSpawnManager();
		
		rscList=ResourceEditorWindow.Load();
	}
	
	void UpdateResourceList(){
		rscList=ResourceEditorWindow.Load();
		Repaint();
	}
	
	void OnEnable(){
		CreepManager.onCreepUpdateE+=LoadCreep;
		ResourceEditorWindow.onResourceUpdateE+=UpdateResourceList;
	}
	
	void OnDisable(){
		CreepManager.onCreepUpdateE-=LoadCreep;
		ResourceEditorWindow.onResourceUpdateE-=UpdateResourceList;
	}
    
    static SpawnManager spawnManager;
	//~ static ResourceManager rscManager;
    
	static void GetSpawnManager(){
		spawnManager=(SpawnManager)FindObjectOfType(typeof(SpawnManager));

		if(spawnManager!=null){
			
			waveLength=spawnManager.waves.Length;
			//UpdateWaveLength();
			
			waveFoldList=new bool[waveLength];
			for(int i=0; i<waveFoldList.Length; i++){
				waveFoldList[i]=true;
			}
			
			subWaveLength=new int[waveLength];
			
			for(int i=0; i<waveLength; i++){
				if(spawnManager.waves[i]==null){
					spawnManager.waves[i]=new Wave();
					spawnManager.waves[i].subWaves[0]=new SubWave();
					//~ if(creepList.Count>0){
						//~ spawnManager.waves[i].subWaves[0].unit=creepList[0].gameObject;
					//~ }
				}
				else{
					for(int j=0; j<spawnManager.waves[i].subWaves.Length; j++){
						if(spawnManager.waves[i].subWaves[j]==null){
							spawnManager.waves[i].subWaves[j]=new SubWave();
							//~ if(creepList.Count>0){
								//~ spawnManager.waves[i].subWaves[j].unit=creepList[0].gameObject;
							//~ }
						}
					}
				}
				
				Wave wave=spawnManager.waves[i];
				
				//Debug.Log(wave.subWaves);
				subWaveLength[i]=wave.subWaves.Length;
				
				foreach(SubWave subWave in wave.subWaves){
					subWave.SetUnitComponent();
				}
			}
			
			//spawnManager.InitCreepParameter(creepList);
		}
		
		//~ rscManager=(ResourceManager)FindObjectOfType(typeof(ResourceManager));
		
	}
	
	private static List<UnitCreep> creepList=new List<UnitCreep>();
	private static string[] creepNameList=new string[0];
	private static int[] creepIDMapList=new int[0];
	static void LoadCreep(){
		GameObject obj=Resources.Load("PrefabListCreep", typeof(GameObject)) as GameObject;
		if(obj!=null){
			CreepListPrefab prefab=obj.GetComponent<CreepListPrefab>();
			
			if(prefab!=null){
				for(int i=0; i<prefab.creepList.Count; i++){
					if(prefab.creepList[i]==null){
						prefab.creepList.RemoveAt(i);
						i-=1;
					}
				}
				
				creepList=new List<UnitCreep>();
				creepNameList=new string[prefab.creepList.Count];
				creepIDMapList=new int[prefab.creepList.Count];
				
				for(int i=0; i<prefab.creepList.Count; i++){
					if(prefab.creepList[i]!=null){
						creepList.Add(prefab.creepList[i]);
						creepNameList[i]=creepList[i].unitName;
						creepIDMapList[i]=creepList[i].prefabID;
					}
				}
				
				if(creepFoldList.Length==0){
					creepFoldList=new bool[creepList.Count];
					for(int i=0; i<creepFoldList.Length; i++) creepFoldList[i]=true;
				}
				else{
					bool[] tempList=new bool[creepList.Count];
					for(int i=0; i<creepList.Count; i++){
						if(i<creepFoldList.Length) tempList[i]=creepFoldList[i];
						else tempList[i]=true;
					}
					creepFoldList=tempList;
				}
			}
		}
	}
	
	//map creepID to element number the towerIDMapList
	int MapIndexToCreepIDList(int ID){
		for(int i=0; i<creepIDMapList.Length; i++){// in towerIDMapList){
			if(creepIDMapList[i]==ID){
				return i;
			}
		}
		return -1;
	}
	
	
	private Vector2 scrollPos;
	private int removeID=-1;
    
	private GUIContent cont;
	private GUIContent[] contList;
	
    void OnGUI(){
		if(window==null) Init();
		
		GUI.changed = false;
    	
    	float startX=3;
		float startY=3;
		
		float height=18;
		float spaceY=height+startX;
    	
    	if(spawnManager==null){
    		//~ if(GUI.Button(new Rect(startX, startY, 100, height), "Update")) 
			GUI.Label(new Rect(startX, startY, 500, height), "Current Scene doesn't contain a SpawnManager");
				GetSpawnManager();
    	}
    	else{
	    	//~ if(GUI.Button(new Rect(Mathf.Max(startX+500, window.position.width-120), startY, 100, height), "Update")) GetSpawnManager();
	    	
			//~ int spawnLimit=(int)spawnManager.spawnLimit;
	    	//~ spawnLimit = EditorGUI.Popup(new Rect(startX, startY, 300, 15), "Spawn Limit:", spawnLimit, spawnLimitLabel);
			//~ spawnManager.spawnLimit=(_SpawnLimit)spawnLimit;
			
			GUI.SetNextControlName ("AutoWaveGen");
			//~ spawnManager.autoGenWave=EditorGUI.Toggle(new Rect(startX, startY, 300, 15), "Auto Generate Wave: ", spawnManager.autoGenWave);
			cont=new GUIContent("Auto Generate Wave:", "check to have waves generated in run time");
			spawnManager.autoGenWave=EditorGUI.Toggle(new Rect(startX, startY, 300, 15), cont, spawnManager.autoGenWave);
			if(GUI.changed) GUI.FocusControl ("AutoWaveGen");
			
			int spawnType=(int)spawnManager.spawnMode;
			cont=new GUIContent("Spawn Mode:", "select the spawn mode in this level");
			contList=new GUIContent[spawnTypeLabel.Length];
			for(int i=0; i<contList.Length; i++) contList[i]=new GUIContent(spawnTypeLabel[i], spawnTypeTooltip[i]);
			//~ spawnType = EditorGUI.Popup(new Rect(startX, startY+=spaceY, 300, 15), cont, spawnType, spawnTypeLabel);
			spawnType = EditorGUI.Popup(new Rect(startX, startY+=spaceY, 300, 15), cont, spawnType, contList);
			spawnManager.spawnMode=(_SpawnMode)spawnType;
			
			
			if(!spawnManager.autoGenWave){
				
				if(state==_EState.Default){
					WaveConfigurator(startX, startY, spaceY, height);
					
					GUI.SetNextControlName ("ConfigureAuto");
					cont=new GUIContent("Configure", "configure parameter for auto-wave generation");
					if(GUI.Button(new Rect(window.position.width-100, 10, 90, 20), cont)){
						state=_EState.Configure;
						GUI.FocusControl ("ConfigureAuto");
					}
					
					GUI.SetNextControlName ("AutoFillWave");
					cont=new GUIContent("Generate", "procedurally generate all waves");
					if(GUI.Button(new Rect(window.position.width-200, 10, 90, 20), cont)){
						GUI.FocusControl ("AutoFillWave");
						for(int i=0; i<spawnManager.waves.Length; i++){
							spawnManager.waves[i]=spawnManager.GenerateWave(i, rscList.Count);
						}
					}
				}
				else if(state==_EState.Configure){
					AutoWaveParaConfigurator(startX, startY, spaceY);
					cont=new GUIContent("Done", "back to waves configuration");
					if(GUI.Button(new Rect(window.position.width-100, 10, 90, 20), cont)){
						state=_EState.Default;
					}
				}
			
			}
			else if(spawnManager.autoGenWave){
				
				int spawnLimit=(int)spawnManager.spawnLimit;
				GUI.SetNextControlName ("SpawnLimit");
				cont=new GUIContent("Spawn Limit:", "Switch between finite and infinite wave count");
				contList=new GUIContent[spawnLimitLabel.Length];
				for(int i=0; i<contList.Length; i++) contList[i]=new GUIContent(spawnLimitLabel[i], spawnLimitTooltip[i]);
				spawnLimit = EditorGUI.Popup(new Rect(startX, startY+=spaceY, 300, 15), cont, spawnLimit, contList);
				if(GUI.changed) GUI.FocusControl ("SpawnLimit");
				spawnManager.spawnLimit=(_SpawnLimit)spawnLimit;
				
				if(spawnManager.spawnLimit==_SpawnLimit.Infinite){
					AutoWaveParaConfigurator(startX, startY, spaceY);
				}
				else if(spawnManager.spawnLimit==_SpawnLimit.Finite){
					int waveLength=spawnManager.waves.Length;
					cont=new GUIContent("Total Wave Count:", "Total number of waves in the level");
					waveLength=EditorGUI.IntField(new Rect(startX, startY+=spaceY-4, 200, 15), cont, waveLength);
					if(waveLength>=0){
						if(waveLength!=spawnManager.waves.Length) MatchListLengthToWaveLength();
					}
					AutoWaveParaConfigurator(startX, startY, spaceY);
				}
				
				/*
				EditorGUI.LabelField(new Rect(startX, startY+spaceY, 300, 15), "Wave Size: "+waveLength);
				if(GUI.Button(new Rect(startX+150, startY+spaceY-1, 40, 15), "-1")){
					if(waveLength>1){
						waveLength-=1;
						MatchListLengthToWaveLength();
					}
				}
				if(GUI.Button(new Rect(startX+200, startY+spaceY-1, 40, 15), "+1")){
					waveLength+=1;
					MatchListLengthToWaveLength();
				}
				*/
			}
    	}
		
		if(GUI.changed) EditorUtility.SetDirty(spawnManager);
		
	}
	
	public float contentHeight=0;
	
	public float waveBoxHeight=0;
	public float subWaveBoxHeight=0;
	
	void WaveConfigurator(float startX, float startY, float spaceY, float height){
				startY+=3;
		
				cont=new GUIContent("Default Path:", "The primary path to be used. This path can be overridden by other alternate path for each individual spawn");
				spawnManager.defaultPath=(PathTD)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 15), cont, spawnManager.defaultPath, typeof(PathTD), true);
				cont=new GUIContent("Path Looing:", "Check to enable loop mode for all paths. Creep will loop back to the start of the path when it reach the end of it. It will only be taken out of the game when it's killed");
				spawnManager.pathLooing=EditorGUI.Toggle(new Rect(startX, startY+=spaceY, 300, 15), cont, spawnManager.pathLooing);
			
				EditorGUI.LabelField(new Rect(startX, startY+spaceY, 300, 15), "Wave Size: "+waveLength);
				if(GUI.Button(new Rect(startX+150, startY+spaceY-1, 40, 15), "-1")){
					if(waveLength>1){
						waveLength-=1;
						MatchListLengthToWaveLength();
					}
				}
				if(GUI.Button(new Rect(startX+200, startY+spaceY-1, 40, 15), "+1")){
					waveLength+=1;
					MatchListLengthToWaveLength();
				}
				
				startY+=spaceY*2+10;
		
				
				
				rscCount=rscList.Count;

				int maxSubWave=GetMaximumSubWaveCount();
				Rect visibleRect=new Rect(startX, startY, window.position.width-5, window.position.height-startY);
				Rect contentRect=new Rect(0, 0, maxSubWave*200, contentHeight);
				//~ Rect contentRect=new Rect(0, 0, maxSubWave*200, waveLength*350+waveLength*rscCount*20);
				
				//GUI.Box(visibleRect, "");
				scrollPos = GUI.BeginScrollView(visibleRect, scrollPos, contentRect);
				
				
				startY=0;
		
				for(int i=0; i<spawnManager.waves.Length; i++){
					waveFoldList[i] = EditorGUI.Foldout(new Rect(startX, startY, 60, 15), waveFoldList[i], "wave "+(i+1).ToString());
					
					if(spawnManager.waves[i]==null) spawnManager.waves[i]=new Wave();
					
					if(GUI.Button(new Rect(startX+60, startY+1, 65, 15), "Clone")){
						CloneWave(i);
						return;
					}
					
					if(removeID==i){
						GUI.color=Color.red;
						if(GUI.Button(new Rect(startX+140, startY+1, 65, 15), "Confirm")){
							RemoveWave(i);
							removeID=-1;
							return;
						}
						GUI.color=Color.white;
					}
					else{
						if(GUI.Button(new Rect(startX+140, startY+1, 65, 15), "Remove")){
							removeID=i;
						}
					}
					
					
					if(waveFoldList[i]){
						
						if(spawnManager.waves[i].subWaves==null){ 
							spawnManager.waves[i].subWaves=new SubWave[1];
							spawnManager.waves[i].subWaves[0]=new SubWave();
						}
						
						//~ GUI.Box(new Rect(startX, startY+spaceY-1, Mathf.Max(310, spawnManager.waves[i].subWaves.Length*199+3), 265+rscList.Count*spaceY+(rscCount*21)), "");
						float waveBoxStartY=startY;
						startY+=spaceY;
						GUI.Box(new Rect(startX, startY, Mathf.Max(310, spawnManager.waves[i].subWaves.Length*199+3), waveBoxHeight), "");
						
						startX+=3; startY+=3;
						
						EditorGUI.LabelField(new Rect(startX, startY, 300, 15), "Number of SubWave: "+subWaveLength[i]);
						if(GUI.Button(new Rect(startX+160, startY, 40, 15), "-1")){
							subWaveLength[i]=Mathf.Max(1, subWaveLength[i]-1);
							UpdateUnit(i, subWaveLength[i]);
						}
						if(GUI.Button(new Rect(startX+205, startY, 40, 15), "+1")){
							subWaveLength[i]=Mathf.Max(1, subWaveLength[i]+1);
							UpdateUnit(i, subWaveLength[i]);
						}

						float subWaveBoxStartY=startY;
						
						for(int j=0; j<spawnManager.waves[i].subWaves.Length; j++){
							SubWave subWave=spawnManager.waves[i].subWaves[j];
							if(subWave==null){
								spawnManager.waves[i].subWaves[j]=new SubWave();
								subWave=spawnManager.waves[i].subWaves[j];
							}
							
							//~ GUI.Box(new Rect(startX, startY+spaceY-1, 188, 205+rscList.Count*spaceY), "");
							GUI.Box(new Rect(startX, startY+spaceY-1, 188, subWaveBoxHeight), "");
							startX+=4; startY+=4;
							
							
							int index=-1;
							if(subWave.GetUnitComponent()!=null) index=MapIndexToCreepIDList(subWave.GetUnitComponent().prefabID);
							cont=new GUIContent("Unit Prefab:", "The creep prefab to be spawned");
							EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, 15), cont);
							index = EditorGUI.Popup(new Rect(startX+100, startY+=spaceY, 80, 15), index, creepNameList);
							if(index>=0){
								subWave.unit=creepList[index].gameObject;
								subWave.SetUnitComponent();
							}
							
							cont=new GUIContent("Number of Unit:", "number of unit to be spawned");
							EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, 15), cont);
							subWave.count=EditorGUI.IntField(new Rect(startX+100, startY+=spaceY, 80, 15), subWave.count);
							
							cont=new GUIContent("Spawn Interval:", "the time interval between each single creep spawned");
							EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, 15), cont);
							subWave.interval=EditorGUI.FloatField(new Rect(startX+100, startY+=spaceY, 80, 15), subWave.interval);
							
							cont=new GUIContent("Pre-Spawn Delay:", "time delay before the first creep start spawn");
							EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, 15), cont);
							subWave.delay=EditorGUI.FloatField(new Rect(startX+100, startY+=spaceY, 80, 15), subWave.delay);
							
							cont=new GUIContent("Alternate Path:", "The path to use, if it's different from the default path. Can be left blank");
							EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, 15), cont);
							subWave.path=(PathTD)EditorGUI.ObjectField(new Rect(startX+100, startY+=spaceY, 80, 15), subWave.path, typeof(PathTD), true);
							
							cont=new GUIContent("OverrideLifeCost:", "override the life-cost value on the prefab each the unit reach it's destination.\nif value is set to -1, the value wont be overriden.\nLife cost being the value of player life lost when a creep reach it's destination ");
							EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, 15), cont);
							subWave.overrideLifeCost=EditorGUI.IntField(new Rect(startX+100, startY+=spaceY, 80, 15), subWave.overrideLifeCost);
							//if(subWave.overrideLifeCost==0) subWave.overrideLifeCost=-1;
							
							cont=new GUIContent("OverrideHP:", "override the HP of the unit on prefab.\nif value is set to -1, the value wont be overriden.");
							EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, 15), cont);
							subWave.overrideHP=EditorGUI.FloatField(new Rect(startX+100, startY+=spaceY, 80, 15), subWave.overrideHP);
							//if(subWave.overrideHP==0) subWave.overrideHP=-1;
							
							cont=new GUIContent("OverrideShield:", "override the shield value of the unit on prefab.\nif value is set to -1, the value wont be overriden.");
							EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, 15), cont);
							subWave.overrideShield=EditorGUI.FloatField(new Rect(startX+100, startY+=spaceY, 80, 15), subWave.overrideShield);
							//if(subWave.overrideShield==0) subWave.overrideShield=-1;
							
							cont=new GUIContent("OverrideSpeed:", "override the speed value of the unit on prefab.\nif value is set to -1, the value wont be overriden.");
							EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, 15), cont);
							subWave.overrideMoveSpd=EditorGUI.FloatField(new Rect(startX+100, startY+=spaceY, 80, 15), subWave.overrideMoveSpd);
							//if(subWave.overrideMoveSpd==0) subWave.overrideMoveSpd=-1;
							
							if(subWave.overrideValue.Length!=rscList.Count) subWave.MatchValue(rscList.Count);
							cont=new GUIContent("OverrideValue:", "override the resource value of the unit on prefab.\nif value is set to -1, the value wont be overriden.\nresource value being the value of resource obtained by player when the creep is killed");
							EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 200, 15), cont);
							startY-=3;
							for(int n=0; n<subWave.overrideValue.Length; n++){
								EditorGUI.LabelField(new Rect(startX, startY+spaceY-3, 200, 15), " - "+rscList[n].name+":");
								subWave.overrideValue[n]=EditorGUI.IntField(new Rect(startX+100, startY+=spaceY-3, 80, 15), subWave.overrideValue[n]);
								//if(subWave.overrideValue[n]==0) subWave.overrideValue[n]=-1;
							}
							
							subWaveBoxHeight=startY-subWaveBoxStartY;
							
							startX+=195;
							if(j<spawnManager.waves[i].subWaves.Length-1) startY=subWaveBoxStartY;
							else startY+=5;
						}
						
						startX=6;
						
						cont=new GUIContent("Time before next wave:", "Time before next wave is spawned. (Only valid for continous spawn mode)\nThe figure beside is the total time required to spawn all creep in this wave.");
						EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, 15), cont);
						spawnManager.waves[i].waveInterval=EditorGUI.FloatField(new Rect(startX+150, startY+=spaceY, 80, 15), spawnManager.waves[i].waveInterval);
						EditorGUI.LabelField(new Rect(startX+250, startY, 200, 15), spawnManager.waves[i].CalculateSpawnDuration().ToString());
						
						if(rscCount!=spawnManager.waves[i].resourceGain.Length){
							UpdateResourceGain(i, rscCount);
						}
						
						cont=new GUIContent("Reousrce Upon Wave Clear:", "The value of resource gain by player upon surviving this wave");
						EditorGUI.LabelField(new Rect(startX, startY+=spaceY-5, 200, height), cont);
						for(int j=0; j<spawnManager.waves[i].resourceGain.Length; j++){
							EditorGUI.LabelField(new Rect(startX, startY+spaceY-3, 200, height), " - "+rscList[j].name+": ");
							
							spawnManager.waves[i].resourceGain[j] = EditorGUI.IntField(new Rect(startX+150, startY+=spaceY-3, 50, height-2), spawnManager.waves[i].resourceGain[j]);
						}
						
						waveBoxHeight=startY-waveBoxStartY;
						
						startY+=35;
					}
					else startY+=spaceY;
					
					startX=3;
				}
				
				contentHeight=startY;//-contentStartY;
				
				GUI.EndScrollView();
	}
	
	
	void AutoWaveParaConfigurator(float startX, float startY, float spaceY){
				//startY+=25;
		//~ Debug.Log(spaceY);
		spaceY=17;
		
				cont=new GUIContent("All Path (TotalCount):", "All the path available in this level");
				int allPathCount=spawnManager.allPath.Count;
				allPathCount = EditorGUI.IntField(new Rect(startX, startY+=spaceY+5, 300, 15), cont, allPathCount);
				if(allPathCount!=spawnManager.allPath.Count){
					//spawnManager.allPath=MatchPathListCount(spawnManager.allPath, aniLength);
					List<PathTD> tempList=new List<PathTD>();
					for(int i=0; i<allPathCount; i++){
						if(i<spawnManager.allPath.Count) tempList.Add(spawnManager.allPath[i]);
						else tempList.Add(null);
					}
					spawnManager.allPath=tempList;
				}
				for(int i=0; i<allPathCount; i++){
					spawnManager.allPath[i]=(PathTD)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY-2, 300, 15), "  - "+i+": ", spawnManager.allPath[i], typeof(PathTD), true);
				}
				
				cont=new GUIContent("Path Looing:", "Check to enable loop mode for all paths. Creep will loop back to the start of the path when it reach the end of it. It will only be taken out of the game when it's killed");
				spawnManager.pathLooing=EditorGUI.Toggle(new Rect(startX, startY+=spaceY, 300, 15), cont, spawnManager.pathLooing);
		
				startY+=10;
		
				if(spawnManager.spawnMode==_SpawnMode.Continuous || spawnManager.spawnMode==_SpawnMode.SkippableContinuous){
					cont=new GUIContent("Spawn Cooldown:", "duration to wait until next wave is started (only start counting after current wave has finish spawning)");
					spawnManager.waveCD=EditorGUI.FloatField(new Rect(startX, startY+=spaceY, 200, 15), cont, spawnManager.waveCD);
					cont=new GUIContent("Cooldown Deviation:", "maximum +/- devaition for the waiting duration");
					spawnManager.waveCDDev=EditorGUI.FloatField(new Rect(startX, startY+=spaceY, 200, 15), cont, spawnManager.waveCDDev);
				}
				
				startY+=10;
				
				cont=new GUIContent("Starting Unit Count:", "initial unit count");
				spawnManager.unitCount=EditorGUI.IntField(new Rect(startX, startY+=spaceY, 200, 15), cont, spawnManager.unitCount);
				cont=new GUIContent("Unit Count Increment:", "unit increment for each subsequent wave");
				spawnManager.countIncrement=EditorGUI.FloatField(new Rect(startX, startY+=spaceY, 200, 15), cont, spawnManager.countIncrement);
				cont=new GUIContent("Unit Count Deviation:", "maximum +/- deviation for the unit count in each wave in percentage (value 0-1)");
				spawnManager.countDevMod=EditorGUI.FloatField(new Rect(startX, startY+=spaceY, 200, 15), cont, spawnManager.countDevMod);
				
				startX+=230; startY-=3*spaceY;
				cont=new GUIContent("subWaveCount:", "initial subwave count for each wave");
				spawnManager.subWaveCount=EditorGUI.FloatField(new Rect(startX, startY+=spaceY, 200, 15), cont, spawnManager.subWaveCount);
				
				cont=new GUIContent("subWaveCountInc:", "subwave count for each subsequent wave.\ncan take value of less than 1 (takes more than 1 wave to increase subwave count)");
				spawnManager.subWaveCountInc=EditorGUI.FloatField(new Rect(startX, startY+=spaceY, 200, 15), cont, spawnManager.subWaveCountInc);
				
				cont=new GUIContent("maxSubWaveCount:", "maximum subwave allowed in each wave");
				spawnManager.maxSubWaveCount=EditorGUI.IntField(new Rect(startX, startY+=spaceY, 200, 15), cont, spawnManager.maxSubWaveCount);
				
				startX-=230; startY+=10;
				
				
				if(spawnManager.rscGain.Length!=rscList.Count) spawnManager.rscGain=MatchFloatArray(spawnManager.rscGain, rscList.Count);
				if(spawnManager.rscInc.Length!=rscList.Count) spawnManager.rscInc=MatchFloatArray(spawnManager.rscInc, rscList.Count);
				if(spawnManager.rscDev.Length!=rscList.Count) spawnManager.rscDev=MatchFloatArray(spawnManager.rscDev, rscList.Count);
				//~ EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 200, 15), "Resource Gain Modifier:");
				showInfiniteRscModifier=EditorGUI.Foldout(new Rect(startX, startY+=spaceY, 200, 15), showInfiniteRscModifier, "Resource Gain Modifier:");
				if(showInfiniteRscModifier){
					startY-=3;
					startX+=10;
					for(int n=0; n<rscList.Count; n++){
						EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 200, 15), "-"+rscList[n].name+":");
						
						cont=new GUIContent("-Start:", "initial value gain for each wave");
						EditorGUI.LabelField(new Rect(startX+10, startY+spaceY-3, 200, 15), cont);
						spawnManager.rscGain[n]=EditorGUI.FloatField(new Rect(startX+85, startY+=spaceY-2, 40, 15), spawnManager.rscGain[n]);
						
						cont=new GUIContent("-Increment:", "increment over each subsequent wave");
						EditorGUI.LabelField(new Rect(startX+10, startY+spaceY-3, 200, 15),cont);
						spawnManager.rscInc[n]=EditorGUI.FloatField(new Rect(startX+85, startY+=spaceY-2, 40, 15), spawnManager.rscInc[n]);
						
						cont=new GUIContent("-Deviation:", "maximum +/- deviation for the value gain in each wave in percentage (value 0-1)");
						EditorGUI.LabelField(new Rect(startX+10, startY+spaceY-3, 200, 15), cont);
						spawnManager.rscDev[n]=EditorGUI.FloatField(new Rect(startX+85, startY+=spaceY-2, 40, 15), spawnManager.rscDev[n]);
						
						startX+=150;
						startY-=3*spaceY+9;
					}
				
					startY+=4.5f*spaceY+10;
					startX=5;
				}
				else	startY+=spaceY+10;
				
				int c=0;
				foreach(bool flag in creepFoldList) if(flag) c+=1;
				Rect visibleRect=new Rect(startX, startY, window.position.width-10, window.position.height-startY-5);
				//~ Rect contentRect=new Rect(0, 0, window.position.width-25, spawnManager.creepParameter.Length*60+c*70);
				Rect contentRect=new Rect(0, 0, window.position.width-25, creepBoxHeight);
				GUI.Box(visibleRect, "");
				scrollPos = GUI.BeginScrollView(visibleRect, scrollPos, contentRect);
				startY=0;
				
				if(spawnManager.creepParameter.Length!=creepList.Count){
					spawnManager.InitCreepParameter(creepList);
				}
				
				spaceY=17;
				for(int i=0; i<spawnManager.creepParameter.Length; i++){
					UnitParameter up=spawnManager.creepParameter[i];
					
					startX=5;
					EditorGUI.LabelField(new Rect(startX+18, startY, 200, 20), creepList[i].unitName);
					EditorGUI.LabelField(new Rect(startX+18, startY+17, 40, 40), new GUIContent(creepList[i].icon));
					
					
					
					cont=new GUIContent("", "check to enable this prefab in this level");
					EditorGUI.LabelField(new Rect(startX, startY, 60, 40), cont);
					up.enabled=EditorGUI.Toggle(new Rect(startX, startY, 40, 15), up.enabled);
					
					startX+=60;
					
					if(up.enabled){
						cont=new GUIContent("Min Wave Req:", "minimum wave required before this creep will start being spawned");
						EditorGUI.LabelField(new Rect(startX, startY+spaceY+3, 200, 15), cont);
						up.minWave=EditorGUI.IntField(new Rect(startX+90, startY+=spaceY+3, 40, 15), up.minWave);
						
						creepFoldList[i]=EditorGUI.Foldout(new Rect(startX, startY+=spaceY, 200, 15), creepFoldList[i], "Show Parameters:");
						if(creepFoldList[i]){
						
							//startX+=150;	//startY-=2*spaceY;
							//startY+=5;
							startX=25;
							cont=new GUIContent("Min Delay:", "minimum spawn delay for subwave spawning this creep");
							EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, 15),cont);
							up.minDelay=EditorGUI.FloatField(new Rect(startX+90, startY+=spaceY, 40, 15), up.minDelay);
							
							cont=new GUIContent("Max Delay:", "maximum spawn delay for subwave spawning this creep");
							EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, 15), cont);
							up.maxDelay=EditorGUI.FloatField(new Rect(startX+90, startY+=spaceY, 40, 15), up.maxDelay);
							
							cont=new GUIContent("Min Interval:", "minimum spawn interval for subwave spawning this creep");
							EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, 15), cont);
							up.minInterval=EditorGUI.FloatField(new Rect(startX+90, startY+=spaceY, 40, 15), up.minInterval);
							
							cont=new GUIContent("Max Interval:", "maximum spawn interval for subwave spawning this creep");
							EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, 15), cont);
							up.maxInterval=EditorGUI.FloatField(new Rect(startX+90, startY+=spaceY, 40, 15), up.maxInterval);


							startX+=150;	startY-=4*spaceY;
							cont=new GUIContent("Min HP:", "minimum HP for this creep");
							EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, 15), cont);
							up.minHP=EditorGUI.FloatField(new Rect(startX+90, startY+=spaceY, 40, 15), up.minHP);
							
							cont=new GUIContent("default HP:", "default starting HP for this creep");
							EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, 15), cont);
							up.startHP=EditorGUI.FloatField(new Rect(startX+90, startY+=spaceY, 40, 15), up.startHP);
							
							cont=new GUIContent("HP Increment:", "HP increment over each subsequent wave for this creep");
							EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, 15), cont);
							up.HPIncrement=EditorGUI.FloatField(new Rect(startX+90, startY+=spaceY, 40, 15), up.HPIncrement);
							
							cont=new GUIContent("HP Deviation:", "maximum +/- HP deviation this creep");
							EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, 15), cont);
							up.HPDeviation=EditorGUI.FloatField(new Rect(startX+90, startY+=spaceY, 40, 15), up.HPDeviation);
							
							
							startX+=150;	startY-=4*spaceY;
							cont=new GUIContent("Min Speed:", "minimum Speed for this creep");
							EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, 15), cont);
							up.minSpd=EditorGUI.FloatField(new Rect(startX+110, startY+=spaceY, 40, 15), up.minSpd);
							
							cont=new GUIContent("default Speed:", "default starting Speed for this creep");
							EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, 15), cont);
							up.startSpd=EditorGUI.FloatField(new Rect(startX+110, startY+=spaceY, 40, 15), up.startSpd);
							
							cont=new GUIContent("Speed Increment:", "Speed increment over each subsequent wave for this creep");
							EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, 15), cont);
							up.spdIncrement=EditorGUI.FloatField(new Rect(startX+110, startY+=spaceY, 40, 15), up.spdIncrement);
							
							cont=new GUIContent("Speed Deviation:", "maximum +/- Speed deviation this creep");
							EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, 15), cont);
							up.spdDeviation=EditorGUI.FloatField(new Rect(startX+110, startY+=spaceY, 40, 15), up.spdDeviation);
							
							
							startX+=170;	startY-=4*spaceY;
							cont=new GUIContent("Min Shield:", "minimum Shield for this creep");
							EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, 15), cont);
							up.minShd=EditorGUI.FloatField(new Rect(startX+90, startY+=spaceY, 40, 15), up.minShd);
							
							if(up.minShd>0){
								cont=new GUIContent("default Shield:", "default starting Shield for this creep");
								EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, 15), cont);
								up.startShd=EditorGUI.FloatField(new Rect(startX+90, startY+=spaceY, 40, 15), up.startSpd);
								
								cont=new GUIContent("Shield Inc.:", "Shield increment over each subsequent wave for this creep");
								EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, 15), cont);
								up.shdIncrement=EditorGUI.FloatField(new Rect(startX+90, startY+=spaceY, 40, 15), up.shdIncrement);
								
								cont=new GUIContent("Shield Dev.:", "maximum +/- Shield deviation this creep");
								EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, 15), cont);
								up.shdDeviation=EditorGUI.FloatField(new Rect(startX+90, startY+=spaceY, 40, 15), up.shdDeviation);
								
								startX-=300+55;
								startY+=30;
							}
							else{
								startY+=30+3*spaceY;
							}

							
							
						}
						else{
							startY+=30;
						}
					}
					else{
						startX-=55;	startY+=2*spaceY+33;//52;
					}
				}
				
				creepBoxHeight=startY;
				
				GUI.EndScrollView();
	}
	
	float creepBoxHeight=0;
	
	
	private int rscCount=1;
	void UpdateResourceGain(int index, int length){
		int[] tempList=spawnManager.waves[index].resourceGain;
		
		spawnManager.waves[index].resourceGain=new int[length];
		for(int i=0; i<spawnManager.waves[index].resourceGain.Length; i++){
			if(i>=tempList.Length){
				spawnManager.waves[index].resourceGain[i]=0;
			}
			else{
				spawnManager.waves[index].resourceGain[i]=tempList[i];
			}
		}
	}
    
    void MatchListLengthToWaveLength(){
    	if(waveLength!=spawnManager.waves.Length){
			if(waveLength>spawnManager.waves.Length){
    			
    			Wave[] tempWaveList=new Wave[waveLength];
    			bool[] tempwaveFoldList=new bool[waveLength];
    			
    			for(int i=0; i<tempWaveList.Length; i++){
    				if(i<spawnManager.waves.Length){
    					tempWaveList[i]=spawnManager.waves[i];
    					tempwaveFoldList[i]=waveFoldList[i];
    				}
    				else{
    					tempWaveList[i]=CopyWaveInfo(spawnManager.waves[spawnManager.waves.Length-1]);
    					tempwaveFoldList[i]=false;
    				}
    			}
    			
    			spawnManager.waves=tempWaveList;
    			waveFoldList=tempwaveFoldList;
    			
			}
    		else{
    			
    			Wave[] tempWaveList=new Wave[waveLength];
    			bool[] tempwaveFoldList=new bool[waveLength];
    			
    			for(int i=0; i<tempWaveList.Length; i++){
    				tempWaveList[i]=spawnManager.waves[i];
    				tempwaveFoldList[i]=waveFoldList[i];
    			}
    			
    			spawnManager.waves=tempWaveList;
    			waveFoldList=tempwaveFoldList;
    			
    		}
			
		}
		
		subWaveLength=new int[waveLength];
		for(int i=0; i<waveLength; i++){
			subWaveLength[i]=spawnManager.waves[i].subWaves.Length;
		}
	}
	
	
	
	void RemoveWave(int waveID){
		Wave[] newWave=new Wave[spawnManager.waves.Length-1];
		bool[] tempwaveFoldList=new bool[spawnManager.waves.Length-1];
		for(int i=0; i<waveID; i++){
			tempwaveFoldList[i]=waveFoldList[i];
			newWave[i]=CopyWaveInfo(spawnManager.waves[i]);
		}
		for(int i=waveID; i<newWave.Length; i++){
			tempwaveFoldList[i]=waveFoldList[i+1];
			newWave[i]=CopyWaveInfo(spawnManager.waves[i+1]);
		}
		waveFoldList=tempwaveFoldList;
		spawnManager.waves=newWave;
		
		waveLength=spawnManager.waves.Length;
		
		subWaveLength=new int[waveLength];
		for(int i=0; i<waveLength; i++){
			subWaveLength[i]=spawnManager.waves[i].subWaves.Length;
		}
	}
	
	void CloneWave(int waveID){
		Wave[] newWave=new Wave[spawnManager.waves.Length+1];
		bool[] tempwaveFoldList=new bool[spawnManager.waves.Length+1];
		for(int i=0; i<waveID; i++){
			tempwaveFoldList[i]=waveFoldList[i];
			newWave[i]=CopyWaveInfo(spawnManager.waves[i]);
		}
		for(int i=waveID+1; i<newWave.Length; i++){
			tempwaveFoldList[i]=waveFoldList[i-1];
			newWave[i]=CopyWaveInfo(spawnManager.waves[i-1]);
		}
		tempwaveFoldList[waveID]=waveFoldList[waveID];
		newWave[waveID]=CopyWaveInfo(spawnManager.waves[waveID]);
		
		waveFoldList=tempwaveFoldList;
		spawnManager.waves=newWave;
		
		waveLength=spawnManager.waves.Length;
		
		subWaveLength=new int[waveLength];
		for(int i=0; i<waveLength; i++){
			subWaveLength[i]=spawnManager.waves[i].subWaves.Length;
		}
	}
    
    Wave CopyWaveInfo(Wave srcWave){
    	Wave tempWave=new Wave();
    	
    	tempWave.subWaves=new SubWave[srcWave.subWaves.Length];
    	for(int i=0; i<tempWave.subWaves.Length; i++){
    		tempWave.subWaves[i]=CopySubWaveInfo(srcWave.subWaves[i]);
    	}
    	
    	tempWave.waveInterval=srcWave.waveInterval;
    	//tempWave.resource=srcWave.resource;
    	tempWave.resourceGain=srcWave.resourceGain;
    	
    	return tempWave;
    }
    
    SubWave CopySubWaveInfo(SubWave srcSubWave){
    	SubWave tempSubWave=new SubWave();
    	
    	tempSubWave.unit=srcSubWave.unit;
    	tempSubWave.count=srcSubWave.count;
    	tempSubWave.interval=srcSubWave.interval;
    	tempSubWave.delay=srcSubWave.delay;
    	tempSubWave.path=srcSubWave.path;
		tempSubWave.overrideHP=srcSubWave.overrideHP;
		tempSubWave.overrideMoveSpd=srcSubWave.overrideMoveSpd;
    	
    	return tempSubWave;
    }
    
    void UpdateUnit(int ID, int length){
    	Wave wave=spawnManager.waves[ID];
    	
    	if(length!=wave.subWaves.Length){
    		if(length>wave.subWaves.Length){
    			SubWave[] tempSubWaveList=new SubWave[length];
    			
    			for(int i=0; i<tempSubWaveList.Length; i++){
    				
    				if(i<wave.subWaves.Length){
    					tempSubWaveList[i]=wave.subWaves[i];
    				}
    				else{
    					if(wave.subWaves.Length!=0)
    						tempSubWaveList[i]=CopySubWaveInfo(wave.subWaves[wave.subWaves.Length-1]);
    					else tempSubWaveList[i]=new SubWave();
    				}
    			}
    			
    			spawnManager.waves[ID].subWaves=tempSubWaveList;
    		}
    		else{
    			SubWave[] tempSubWaveList=new SubWave[length];
    			//bool[] tempwaveFoldList=new bool[waveLength];
    			
    			for(int i=0; i<tempSubWaveList.Length; i++){
    				tempSubWaveList[i]=wave.subWaves[i];
    				//tempwaveFoldList[i]=waveFoldList[i];
    			}
    			
    			spawnManager.waves[ID].subWaves=tempSubWaveList;
    			//waveFoldList=tempwaveFoldList;
    		}
    	}
		
		
    }
	
	int GetMaximumSubWaveCount(){
		int count=0;
		for(int i=0; i<subWaveLength.Length; i++){
			if(subWaveLength[i]>count){
				count=subWaveLength[i];
			}
		}
		return count;
	}
	
	//~ float CalculateWaveSpawnDuration(SubWave[] subWaveList){
		//~ float duration=0;
		//~ foreach(SubWave subWave in subWaveList){
			//~ float tempDuration=subWave.count*subWave.interval+subWave.delay;
			//~ if(tempDuration>duration){
				//~ duration=tempDuration;
			//~ }
		//~ }
		//~ return duration;
	//~ }
    
	//pass the wave list varaible of the SpawnManager
	float GetAllWaveDuration(Wave[] waves){
		float duration=0;
		
		//all the wave before the last one
		for(int i=0; i<waves.Length-1; i++){
			duration+=waves[i].waveInterval;
		}
		
		//the last wave
		float durationForLastWave=0;
		foreach(SubWave subWave in waves[waves.Length-1].subWaves){
			float tempDuration=subWave.count*subWave.interval+subWave.delay;
			if(tempDuration>duration){
				durationForLastWave=tempDuration;
			}
		}
		
		duration+=durationForLastWave;
		
		Debug.Log("all wave will be spawned in "+duration+"s");
		return duration;
	}
	
	float[] MatchFloatArray(float[] src, int count){
		float[] newList=new float[count];
		for(int i=0; i<count; i++){
			if(i<src.Length) newList[i]=src[i];
			else newList[i]=0;
		}
		return newList;
	}
}

