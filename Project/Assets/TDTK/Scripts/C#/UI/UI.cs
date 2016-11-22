using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (UIPerk))]

[AddComponentMenu("TDTK/UI/DefaultUI")]
public class UI : MonoBehaviour {

	public enum _BuildMenuType{Pie, Fixed, Box}
	public _BuildMenuType buildMenuType;
	
	public enum _BuildMode{PointNBuild, DragNDrop}
	public _BuildMode buildMode;
	
	public bool showBuildSample=true;
	
	public bool showWaveCount=true;
	public bool previewWaveInfo=true;
	
	public int fastForwardSpeed=3;
	
	public bool enableTargetPrioritySwitch=true;
	public bool enableTargetDirectionSwitch=true;
	
	public bool alwaysEnableNextButton=true;
	
	public string nextLevel="";
	public string mainMenu="";
	
	private bool enableSpawnButton=true;
	
	private bool buildMenu=false;
	
	private UnitTower currentSelectedTower;
	
	private int[] currentBuildList=new int[0];
	
	//indicate if the player have won or lost the game
	private bool winLostFlag=false;
	
	private bool paused=false;
	
	private static UI ui;
	void Awake(){
		ui=this;
	}
	
	public static bool IsPaused(){
		return ui.paused;
	}
	
	
	// Use this for initialization
	void Start () {		
		
		//init the rect to be used for drawing ui box
		//~ topPanelRect=new Rect(-3, -3, Screen.width+6, 28);
		topPanelRect=new Rect(Screen.width-130, 7, 125, 41);
		
		bottomPanelRect=new Rect(-3, Screen.height-25, Screen.width+6, 28);
		
		//init ui rect for DragNDrop mode, this will be use throughout the entire game
		UpdateBuildListRect(null);
		
		if(AbilityManager.GetAllAbilityList()!=null){
			UpdateAbilityListRect(null);
		}
		
		//this two lines are now obsolete
		//initiate sample menu, so player can preview the tower in pointNBuild buildphase
		//if(buildMode==_BuildMode.PointNBuild && showBuildSample) BuildManager.InitiateSampleTower();
		
		//~ StartCoroutine(MessageCountDown());
	}
	
	void UpdateBuildListRect(Perk perk){
		if(buildMode==_BuildMode.DragNDrop){
			UnitTower[] fullTowerList=BuildManager.GetTowerList();
			int width=buildPanelPos.itemWidth;
			int height=buildPanelPos.itemHeight;
			
			//~ int x=0;
			//~ int y=Screen.height-height-6-(int)bottomPanelRect.height;
			
			if(buildPanelPos.y>Screen.height-height-35) buildPanelPos.y=Screen.height-height-35;
			float x=buildPanelPos.x;
			float y=buildPanelPos.y;
			int menuLength=(fullTowerList.Length)*(width+3);
			
			//~ if(x>Screen.height-35) x=Screen.height-height-35;
			
			//~ buildListRect=new Rect(x, y, menuLength+3, height+6);
			
			if(buildPanelPos.orientation==_Orientation.Right) buildListRect=new Rect(x, y, menuLength+3, height+6);
			else if(buildPanelPos.orientation==_Orientation.Left) buildListRect=new Rect(x-menuLength+3+width, y, menuLength+3, height+6);
			else if(buildPanelPos.orientation==_Orientation.Up) buildListRect=new Rect(x, y-menuLength+3+height, width+6, menuLength+3);
			else if(buildPanelPos.orientation==_Orientation.Down) buildListRect=new Rect(x, y, width+6, menuLength+3);
		}
	}
	
	void UpdateAbilityListRect(Perk perk){
		//~ string label="";
		//~ if(perk!=null) label=perk.name; 
		
		int width=abilityPanelPos.itemWidth;
		int height=abilityPanelPos.itemHeight;
		
		float x=abilityPanelPos.x-3;
		float y=abilityPanelPos.y-3;
		
		int menuLength=(AbilityManager.GetActiveAbilityCount())*(width+3);
		
		if(abilityPanelPos.orientation==_Orientation.Right) abilityListRect=new Rect(x, y, menuLength+3, height+6);
		else if(abilityPanelPos.orientation==_Orientation.Left) abilityListRect=new Rect(x-menuLength+3+width, y, menuLength+3, height+6);
		else if(abilityPanelPos.orientation==_Orientation.Up) abilityListRect=new Rect(x, y-menuLength+3+height, width+6, menuLength+3);
		else if(abilityPanelPos.orientation==_Orientation.Down) abilityListRect=new Rect(x, y, width+6, menuLength+3);
	}
	
	void OnEnable(){
		GameControl.onGameOverE += OnGameOver;
		UnitTower.onDestroyE += OnTowerDestroy;
		
		SpawnManager.onClearForSpawningE += OnClearForSpawning;
		SpawnManager.onWaveStartSpawnE += OnNewWave;
		
		UnitTower.onDragNDropE += OnTowerDragNDropEnd;
		
		UIPerk.onPerkWindowE += OnUIPerkWindow;
		PerkManager.onPerkUnlockedE += OnPerkUnlocked;
		
		PerkManager.onRegenLifeE += OnRegenLife;
		PerkManager.onRegenResourceE += OnRegenResource;
		PerkManager.onLifeBonusWaveClearedE += OnBonusLifeWaveCleared;
		
		PerkManager.onNewTowerE += UpdateBuildListRect;
		PerkManager.onNewAbilityE += UpdateAbilityListRect;
	}
	
	void OnDisable(){
		GameControl.onGameOverE -= OnGameOver;
		UnitTower.onDestroyE -= OnTowerDestroy;
		
		SpawnManager.onClearForSpawningE -= OnClearForSpawning;
		SpawnManager.onWaveStartSpawnE -= OnNewWave;
		
		UnitTower.onDragNDropE -= OnTowerDragNDropEnd;
		
		UIPerk.onPerkWindowE -= OnUIPerkWindow;
		PerkManager.onPerkUnlockedE -= OnPerkUnlocked;
		
		PerkManager.onRegenLifeE -= OnRegenLife;
		PerkManager.onRegenResourceE -= OnRegenResource;
		PerkManager.onLifeBonusWaveClearedE -= OnBonusLifeWaveCleared;
		
		PerkManager.onNewTowerE -= UpdateBuildListRect;
		PerkManager.onNewAbilityE -= UpdateAbilityListRect;
	}
	
	void OnPerkUnlocked(string name){
		DisplayMessage(name+" has been unlocked");
	}
	
	public bool showPerkMessage=true;
	void OnRegenLife(int val){
		if(showPerkMessage) DisplayMessage("Generated "+val+" life!");
	}
	
	void OnRegenResource(int[] val){
		if(showPerkMessage) StartCoroutine(_OnRegenResource(val));
	}
	IEnumerator _OnRegenResource(int[] val){
		Resource[] resourceList=GameControl.GetResourceList();
		string rsc="";
		for(int i=0; i<val.Length; i++){
			if(val[i]>0){
				rsc="Generated "+val[i]+" "+resourceList[i].name+"!";
				DisplayMessage(rsc);
			}
			yield return new WaitForSeconds(0.5f);
		}
	}
	
	void OnBonusLifeWaveCleared(int val){
		if(showPerkMessage) DisplayMessage("Generated "+val+" life!");
	}
	
	void OnNewWave(int wave){
		if(wave>1) DisplayMessage("Incoming wave "+(wave-1).ToString()+"!!");
	}
	
	void OnTowerDragNDropEnd(string msg){
		if(msg!="") DisplayMessage(msg);
	}
	
	
	private bool perkWindow=false;
	void OnUIPerkWindow(bool flag){
		if(flag){
			perkWindow=true;
			buildMenu=false;
			if(currentSelectedTower!=null){
				GameControl.ClearSelection();
				currentSelectedTower=null;
			}
		}
		else{
			perkWindow=false;
		}
	}
	
	//called when game is over, flag passed indicate user win or lost
	void OnGameOver(bool flag){
		winLostFlag=flag;
	}
	
	//called if a tower is unbuilt or destroyed, check to see if that tower is selected, if so, clear the selection
	void OnTowerDestroy(UnitTower tower){
		if(currentSelectedTower==tower) currentSelectedTower=null;
	}
	
	//caleed when SpawnManager clearFor Spawing event is detected, enable spawnButton
	void OnClearForSpawning(bool flag){
		enableSpawnButton=flag;
	}
	
	
	
	//call to enable/disable pause
	void TogglePause(){
		paused=!paused;
		if(paused){
			Time.timeScale=0;
			
			//close all the button, so user can interact with game when it's paused
			if(currentSelectedTower!=null){
				GameControl.ClearSelection();
				currentSelectedTower=null;
			}
			if(buildMenu){
				buildMenu=false;
				BuildManager.ClearBuildPoint();
			}
		}
		else Time.timeScale=1;
	}
	
	//public Transform effect;
	//private Transform effect1;
	
	// Update is called once per frame
	void Update () {
		//~ Debug.Log(Time.deltaTime);
		
		if(Input.GetKeyDown(KeyCode.N)){
			if(nextLevel!="") Application.LoadLevel(nextLevel);
		}
		
		//~ if(Input.GetKeyDown(KeyCode.Space)){
			//~ //DisplayMessage(Random.Range(0, 999999999)+" "+Random.Range(0, 999999999)+" "+Random.Range(0, 999999999));
			//~ Vector3 pos=new Vector3(0, 0, 0);
			//~ if(effect1==null){
				//~ effect1=(Transform)Instantiate(effect, pos, Quaternion.identity);
			//~ }
			//~ else{
				//~ Utility.SetActive(effect1.gameObject, true);
			//~ }
		//~ }
		
		#if !UNITY_IPHONE && !UNITY_ANDROID
			if(buildMode==_BuildMode.PointNBuild && !IsCursorOnUI(Input.mousePosition))
				BuildManager.SetIndicator(Input.mousePosition);
		#endif
		
		if(selectedAbilityID>=0){
			AbilityManager.ShowIndicator(Input.mousePosition);
			
			if(Input.GetMouseButtonDown(0) && !IsCursorOnUI(Input.mousePosition) && !paused && !UIPerk.IsWindowOn()){
				if(selectedAbility!=null){
					selectedAbilityID=-1;
					AbilityManager.UnselectAbility();
				}
			}
			else if(Input.GetMouseButtonUp(1)  && !IsCursorOnUI(Input.mousePosition) && !paused && !UIPerk.IsWindowOn()){
				if(selectedAbility!=null){
					if(AbilityManager.CheckTriggerPoint(Input.mousePosition)){
						int status=AbilityManager.ActivateAbility(selectedAbilityID);
						if(status==-1) DisplayMessage("Invalid Target");
					}
					else{
						DisplayMessage("Invalid position");
					}
					
					selectedAbilityID=-1;
					AbilityManager.UnselectAbility();
				}
			}
		}
		else{	
			if(Input.GetMouseButtonDown(0) && !IsCursorOnUI(Input.mousePosition) && !paused && !UIPerk.IsWindowOn()){
				//check if user click on towers
				UnitTower tower=GameControl.Select(Input.mousePosition);
				
				//if user click on tower, select the tower
				if(tower!=null){
					currentSelectedTower=tower;
					
					//if build mode is active, disable buildmode
					if(buildMenu){
						buildMenu=false;
						BuildManager.ClearBuildPoint();
						ClearBuildListRect();
					}
				}
				//no tower is selected
				else{
					//if a tower is selected previously, clear the selection
					if(currentSelectedTower!=null){
						GameControl.ClearSelection();
						currentSelectedTower=null;
						towerUIRect=new Rect(0, 0, 0, 0);
						//UIRect.RemoveRect(towerUIRect);
					}
					
					//if we are in PointNBuild Mode
					if(buildMode==_BuildMode.PointNBuild){
					
						//check for build point, if true initiate build menu
						if(BuildManager.CheckBuildPoint(Input.mousePosition)==_TileStatus.Available){
							UpdateBuildList();
							InitBuildListRect();
							buildMenu=true;
						}
						//if there are no valid build point but we are in build mode, disable it
						else{
							if(buildMenu){
								buildMenu=false;
								BuildManager.ClearBuildPoint();
								ClearBuildListRect();
							}
						}
						
					}
				}
				
			}
			//if right click, 
			else if(Input.GetMouseButtonUp(1)){
				//clear the menu
				if(buildMenu){
					buildMenu=false;
					BuildManager.ClearBuildPoint();
					ClearBuildListRect();
				}
				
				//if there are tower currently being selected
				if(currentSelectedTower!=null){
					CheckForTarget();
				}
			}
		}
		
		
		
		//if escape key is pressed, toggle pause
		if(Input.GetKeyUp(KeyCode.Escape)){
			TogglePause();
		}
	}
	
	
	public PanelPosInfo abilityPanelPos;
	public bool showEnergy=true;
	
	private int selectedAbilityID=-1;
	private Ability selectedAbility;
	void AbilityRoutine(){
		GUI.depth=10;
		
		if(AbilityManager.abilityManager==null) return;
		
		List<Ability> activeAbilityList=AbilityManager.GetActiveAbilityList();
		
		if(showEnergy){
			GUIStyle styleL=new GUIStyle();
			styleL.fontStyle=FontStyle.Bold;
			
			string energy=AbilityManager.GetCurrentEnergy().ToString("f0");
			string energyMax=AbilityManager.GetMaximumEnergy().ToString("f0");
			
			styleL.fontSize=16;
			GUI.Label(new Rect(abilityPanelPos.x, abilityPanelPos.y-20, 500, 30), "Energy: "+energy+"/"+energyMax, styleL);
			styleL.normal.textColor=new Color(1, 1f, .5f, 1f);
			GUI.Label(new Rect(abilityPanelPos.x-2, abilityPanelPos.y-20-2, 500, 30), "Energy: "+energy+"/"+energyMax, styleL);
		}
		
		for(int i=0; i<3; i++) GUI.Box(abilityListRect, "");
		
		float width=abilityPanelPos.itemWidth;
		float height=abilityPanelPos.itemHeight;
		float x=abilityPanelPos.x;
		float y=abilityPanelPos.y;
		
		GUIStyle guiStyle=new GUIStyle();
		guiStyle.normal.textColor=Color.red;
		guiStyle.fontStyle=FontStyle.Bold;
		//~ guiStyle.fontSize=16;
		//~ guiStyle.alignment=TextAnchor.UpperRight;
		
		//~ guiStyle.normal.textColor=new Color(1, 1, 0, 1f);
		
		int j=0;
		foreach(Ability ability in activeAbilityList){
			//~ if(ability.enableInlvl){
				if(ability.maxUseCount>0){
					guiStyle.alignment=TextAnchor.UpperRight;
					int countLeft=ability.maxUseCount-ability.usedCount;
					GUI.Label(new Rect(x, y, width, height), countLeft.ToString(), guiStyle);
				}
				
				if(ability.cooldown>0){
					guiStyle.alignment=TextAnchor.LowerRight;
					GUI.Box(new Rect(x, y, width, height), ability.iconUnavailable);
					GUI.Label(new Rect(x, y, width, height), ability.cooldown.ToString("f1")+"s", guiStyle);
				}
				else{
					if(GUI.Button(new Rect(x, y, width, height), new GUIContent(ability.icon, ability.ID.ToString()))){
					//~ if(GUI.Button(new Rect(x, y, 50, 50), ability.icon)){
						
						int status=AbilityManager.SelectAbility(ability.ID);
						//~ if(status==0){
						//~ int status=ability.IsReady();
						
						if(status==1) DisplayMessage("Ability is on cooldown");
						else if(status==2) DisplayMessage("Insufficient energy");
						else if(status==3) DisplayMessage("Insufficient resource");
						else{
							selectedAbilityID=j;
							selectedAbility=ability;
							//~ AbilityManager.SelectAbility();
							//~ AbilityManager.ActivateIndicator();
						}
					}
				}
				
				if(abilityPanelPos.orientation==_Orientation.Right) x+=width+3;
				else if(abilityPanelPos.orientation==_Orientation.Left) x-=width+3;
				else if(abilityPanelPos.orientation==_Orientation.Up) y-=height+3;
				else if(abilityPanelPos.orientation==_Orientation.Down) y+=height+3;
			//~ }
			j+=1;
		}
		
		//~ if(selectedAbility!=null) GUI.Label(new Rect(50, Screen.height/2+60, 50, 50), "selected: "+selectedAbility.name);
		
		if(GUI.tooltip!=""){
			ShowAbilityToolTip(int.Parse(GUI.tooltip));
			GUI.tooltip="";
		}
	}
	
	
	//check if user has click on creep, if yes and if current tower is eligible to attack it, set the assign it as tower target
	void CheckForTarget(){
		currentSelectedTower.CheckForTarget(Input.mousePosition);
	}
	
	
	private Rect topPanelRect=new Rect(-3, -3, Screen.width+6, 28);
	private Rect bottomPanelRect;
	private Rect buildListRect;
	private Rect abilityListRect;
	private Rect towerUIRect;
	private Rect[] scatteredRectList=new Rect[0];	//for piemenu
	
	//check for all UI screen space, see if user cursor is within any of them, return true if yes
	//this is to prevent user from being able to interact with in game object even when clicking on UI panel and buttons
	public bool IsCursorOnUI(Vector3 point){
		Rect tempRect=new Rect(0, 0, 0, 0);
		
		tempRect=topPanelRect;
		tempRect.y=Screen.height-tempRect.y-tempRect.height;
		if(tempRect.Contains(point)) return true;
		
		tempRect=bottomPanelRect;
		tempRect.y=Screen.height-tempRect.y-tempRect.height;
		if(tempRect.Contains(point)) return true;
		
		tempRect=abilityListRect;
		tempRect.y=Screen.height-tempRect.y-tempRect.height;
		if(tempRect.Contains(point)) return true;
		
		tempRect=buildListRect;
		tempRect.y=Screen.height-tempRect.y-tempRect.height;
		if(tempRect.Contains(point)) return true;
		
		tempRect=towerUIRect;
		tempRect.y=Screen.height-tempRect.y-tempRect.height;
		if(tempRect.Contains(point)) return true;
		
		for(int i=0; i<scatteredRectList.Length; i++){
			tempRect=scatteredRectList[i];
			tempRect.y=Screen.height-tempRect.y-tempRect.height;
			if(tempRect.Contains(point)) return true;
		}
		
		return false;
	}
	
	//clear all ui space occupied by build menu
	void ClearBuildListRect(){
		if(buildMode==_BuildMode.PointNBuild){
			if(buildMenuType==_BuildMenuType.Pie) scatteredRectList=new Rect[0];
			else buildListRect=new Rect(0, 0, 0, 0);
		}
	}
	
	//initiate ui space that will be occupied by build menu
	void InitBuildListRect(){
		if(buildMode==_BuildMode.PointNBuild){
			if(buildMenuType==_BuildMenuType.Fixed){
				int width=50;
				int height=50;
						
				int x=0;
				int y=Screen.height-height-6-(int)bottomPanelRect.height;
						
				int menuLength=(currentBuildList.Length+1)*(width+3);
				
				//calculate the buildlist rect
				buildListRect=new Rect(x, y, menuLength+3, height+6);
			}
			else if(buildMenuType==_BuildMenuType.Box){
				//since this is a floating menu, the actual location cannot be pre-calculated
				//instead it will be calculated in every frame in OnGUI()
			}
			else if(buildMenuType==_BuildMenuType.Pie){
				//since this is a floating menu, the actual location cannot be pre-calculated
				//instead it will be calculated in every frame in OnGUI()
			}
		}
		else if(buildMode==_BuildMode.DragNDrop){
			//DragNDrop mode will always have build menu enable, so no need to calculate it
		}
	}
	
	
	//calculate the position occupied by each individual button based on number of button, position on screen and button size
	public Vector2[] GetPieMenuPos(int num, Vector3 pos, int size){
		if(num==1){
			Vector2[] poss=new Vector2[1];
			poss[0]=pos+new Vector3(0, 20, 0);
			return poss;
		}
		
		//first off calculate the radius require to accomodate all buttons
		float radius=(num*size*2.1f)/(2*Mathf.PI);
		
		//create the rect array and the angle spacing required for the number of button
		Vector2[] piePos=new Vector2[num];
		float angle=200/(Mathf.Max(1, num-1));
		
		int factor=num%2;
		float startAngleOffset=82.5f-(factor)*2.5f;
		
		//loop through and calculate the button's position
		for(int i=0; i<num; i++){
			float x = pos.x+radius*Mathf.Sin((startAngleOffset)*Mathf.Deg2Rad+i*angle*Mathf.PI/180);
			float y = pos.y+radius*-Mathf.Cos((startAngleOffset)*Mathf.Deg2Rad+i*angle*Mathf.PI/180);
			
			piePos[i]=new Vector2(x, y);
		}
		
		
		return piePos;
	}
	
	public Vector2 CheckShiftPiePos(Vector2[] satelitePos, int sizeX, int sizeY){
		float maxDevX=0;
		float maxDevY=0;
		
		foreach(Vector2 pos in satelitePos){
			if(pos.x<sizeX*0.5f){
				maxDevX=(sizeX*0.5f-pos.x);
			}
			else if(pos.x>Screen.width-sizeX*0.5f){
				float dif=Screen.width-pos.x-sizeX*0.5f;
				if(dif<maxDevX) maxDevX=dif;
			}
			
			//~ //strictly considering satelitePos only
			//~ if(pos.y<0){
				//~ maxDevY=-pos.y;
			//~ }
			//~ else if(pos.y>Screen.height-sizeY){
				//~ float dif=Screen.height-pos.y-sizeY;
				//~ if(dif<maxDevY) maxDevY=dif;
			//~ }
			
			//default
			if(pos.y<0+sizeY+35){
				maxDevY=-pos.y+sizeY+35;
			}
			else if(pos.y>Screen.height-sizeY){
				float dif=Screen.height-pos.y-sizeY;
				if(dif<maxDevY) maxDevY=dif;
			}
		}
		
		return new Vector2(maxDevX, maxDevY);
	}
	
	
	
	//draw GUI
	void OnGUI(){
		GUI.depth=10;
		
		GUI.skin=skin;
		
		if(previewWaveInfo){
			Wave currentWave=SpawnManager.GetCurrentWave();
			Wave nextWave=SpawnManager.GetNextWave();
			
			int waveInfoStartY=42;
			
			GUIStyle style=new GUIStyle();
			style.fontStyle=FontStyle.Bold;
			style.fontSize=14;
			style.normal.textColor=new Color(1, 1, 0.15f, 1f);
			
			if(currentWave!=null){
				int height=25+currentWave.subWaves.Length*25;
				for(int i=0; i<4; i++) GUI.Box(new Rect(10, waveInfoStartY, 100, height), "");
				
				GUI.Label(new Rect(15, waveInfoStartY+3, 200, 25), "Wave "+SpawnManager.GetCurrentWaveID()+":", style);
				
				int subStartXW=15;
				int subStartYW=waveInfoStartY+20;
				
				int n=0; 
				foreach(SubWave subWave in currentWave.subWaves){
					Unit unit=subWave.GetUnitComponent();
					
					GUI.Label(new Rect(subStartXW, subStartYW+n*25, 28, 28f), unit.icon);
					GUI.Label(new Rect(subStartXW+30, subStartYW+5+n*25, 40, 25), subWave.count.ToString());
					
					n+=1;
				}
				
				waveInfoStartY+=height+10;
			}
			
			if(nextWave!=null){
				for(int i=0; i<4; i++) GUI.Box(new Rect(10, waveInfoStartY, 100, 25+nextWave.subWaves.Length*25), "");
				if(SpawnManager.GetCurrentWaveID()==SpawnManager.GetTotalWaveCount()-1){
					GUI.Label(new Rect(15, waveInfoStartY+3, 200, 25), "Final Wave:", style);
				}
				else if(SpawnManager.GetCurrentWaveID()==0){
					GUI.Label(new Rect(15, waveInfoStartY+3, 200, 25), "First Wave:", style);
				}
				else{
					GUI.Label(new Rect(15, waveInfoStartY+3, 200, 25), "Next Wave:", style);
				}
				
				waveInfoStartY+=20;
				
				if(nextWave!=null){
					int subStartXW=15;
					int subStartYW=waveInfoStartY;
					
					int n=0; 
					foreach(SubWave subWave in nextWave.subWaves){
						Unit unit=subWave.GetUnitComponent();
						
						if(unit!=null && subWave.count>0){
							GUI.Label(new Rect(subStartXW, subStartYW+n*25, 28, 28f), unit.icon);
							GUI.Label(new Rect(subStartXW+30, subStartYW+5+n*25, 40, 25), subWave.count.ToString());
							
							n+=1;
						}
					}
				}
			}
		}
		
		
		//general infobox
		//draw top panel
		//~ GUI.BeginGroup (topPanelRect);
		
			//~ for(int i=0; i<2; i++) GUI.Box(new Rect(0, 0, topPanelRect.width, topPanelRect.height), "");
			for(int i=0; i<2; i++) GUI.Box(topPanelRect, "");
		
			//~ int buttonX=8;
		
			//if not pause, draw the spawnButton and fastForwardButton
			if(!paused){
				//if SpawnManager is ready to spawn
				if(enableSpawnButton){
					for(int i=0; i<2; i++) GUI.Box(new Rect(Screen.width-215, 7, 80, 41), "");
					if(GUI.Button(new Rect(Screen.width-210, 10, 70, 35), "Spawn")){
						//if the game is not ended
						if(GameControl.gameState!=_GameState.Ended){
							//if spawn is successful, disable the spawnButton
							if(SpawnManager.Spawn())
								enableSpawnButton=false;
						}
					}
					//~ buttonX+=65;
				}
				
				//display the fastforward button based on current time scale
				if(Time.timeScale==1){
					if(GUI.Button(new Rect(Screen.width-125, 10, 70, 35), "Timex"+fastForwardSpeed.ToString())){
						Time.timeScale=fastForwardSpeed;
					}
				}
				else{
					if(GUI.Button(new Rect(Screen.width-125, 10, 70, 35), "Timex1")){
						Time.timeScale=1;
					}
				}
			}
			//shift the cursor to where the next element will be drawn
			//~ else buttonX+=65;
			//~ buttonX+=130;
			
			
			GUIStyle styleL=new GUIStyle();
			styleL.fontStyle=FontStyle.Bold;
			
			
			
			//draw life and wave infomation
			string lifeLabel="Life: "+GameControl.GetPlayerLife().ToString();
			if(GameControl.GetPlayerLifeCap()>0) lifeLabel+="/"+GameControl.GetPlayerLifeCap().ToString();
			
			styleL.fontSize=20;
			GUI.Label(new Rect(12+2, 10+2, 100, 30), lifeLabel, styleL);
			//styleL.normal.textColor=new Color(.1f, 1f, .35f, 1f);
			styleL.normal.textColor=new Color(1, 1f, .5f, 1f);
			GUI.Label(new Rect(12, 10, 100, 30), lifeLabel, styleL);
			
			if(showWaveCount){
				string totalWaveCountLabel="";
				int totalWaveCount=SpawnManager.GetTotalWaveCount();
				if(totalWaveCount>0) totalWaveCountLabel=" / "+totalWaveCount;
				
				styleL.fontSize=16;
				styleL.normal.textColor=Color.black;
				GUI.Label(new Rect(120+2, 14+2, 100, 30), "Wave: "+SpawnManager.GetCurrentWaveID()+totalWaveCountLabel, styleL);
				styleL.normal.textColor=new Color(1, 1f, .5f, 1f);
				GUI.Label(new Rect(120, 14, 100, 30), "Wave: "+SpawnManager.GetCurrentWaveID()+totalWaveCountLabel, styleL);
			}
			
			_SpawnMode mode=SpawnManager.GetSpawnMode();
			if(mode==_SpawnMode.Continuous || mode==_SpawnMode.SkippableContinuous){
				float time=SpawnManager.GetTimeNextSpawn();
				if(time>0){
					int offset=0;
					if(showWaveCount) offset=120;
					
					string text="Next Wave: "+time.ToString("f1")+"s";
					
					styleL.fontSize=16;
					styleL.normal.textColor=Color.black;
					GUI.Label(new Rect(offset+120+2, 14+2, 150, 30), text, styleL);
					styleL.normal.textColor=new Color(1, 1f, .5f, 1f);
					GUI.Label(new Rect(offset+120, 14, 150, 30), text, styleL);
				}
			}
			
			
			//pause button
			if(GUI.Button(new Rect(Screen.width-45, 10, 35, 35), "II")) {
				TogglePause();
			}
		//~ GUI.EndGroup ();
			
		//draw bottom panel
		GUI.BeginGroup (bottomPanelRect);
			for(int i=0; i<2; i++) GUI.Box(new Rect(0, 0, bottomPanelRect.width, bottomPanelRect.height), "");
		
			//get all the resource information
			Resource[] resourceList=GameControl.GetResourceList();
			float subStartX=10; float subStartY=0; float widthMin=0f; float widthMax=0f;
			
			GUI.Label(new Rect(subStartX, subStartY+2, 70f, 25f), "Resource:");
			subStartX+=70;
			
			//display all the resource
			for(int i=0; i<resourceList.Length; i++){
				
				//if an icon has been assigned to that particular resource type
				if(resourceList[i].icon!=null){
					GUI.Label(new Rect(subStartX, subStartY, 20f, 25f), resourceList[i].icon);
					subStartX+=20;
					GUI.Label(new Rect(subStartX, subStartY+2, 40, 25), resourceList[i].value.ToString());
					subStartX+=40;
				}
				//if not icon, just show the text
				else {
					GUIContent labelRsc=new GUIContent(resourceList[i].value.ToString()+resourceList[i].name);
					GUI.skin.GetStyle("Label").CalcMinMaxWidth(labelRsc, out widthMin, out widthMax);
					GUI.Label(new Rect(subStartX, subStartY+2, widthMax, 25), labelRsc);
					subStartX+=widthMax+20;
				}
				
			}
		GUI.EndGroup ();
		
		
		//if game is still not over
		if(GameControl.gameState!=_GameState.Ended){
			
			//clear tooltip, each button when hovered will assign tooltip with corresponding ID
			//the tooltip will be checked later, if there's anything, we show the corresponding tooltip
			GUI.tooltip="";
			
			//build menu
			if(buildMode==_BuildMode.PointNBuild){
				//if PointNBuild mode, only show menu when there are active buildpoint (build menu on)
				if(buildMenu){
					if(buildMenuType==_BuildMenuType.Fixed) BuildMenuFix();
					else if(buildMenuType==_BuildMenuType.Box) BuildMenuBox();
					else if(buildMenuType==_BuildMenuType.Pie) BuildMenuPie();
				}
				//if there's no build menu, clear the buildList rect
				else buildListRect=new Rect(0, 0, 0, 0);
			}
			else if(buildMode==_BuildMode.DragNDrop){
				//if DragNDrop mode, show menu all time
				BuildMenuAllTowersFix();
			}
			
			//if the cursor is hover on top of tower button, the tooltip will bear the ID of the Buildable Tower
			if(GUI.tooltip!=""){
				int ID=int.Parse(GUI.tooltip);
				//show the build tooltip
				ShowToolTip(ID);
				//if no preview has been showing, preview the sample tower on the grid, only needed in PointNBuild mode
				if(buildMode==_BuildMode.PointNBuild && showBuildSample) BuildManager.ShowSampleTower(ID); 
				
				GUI.tooltip="";
			}
			else{
				//if a sample tower is shown on the grid, only needed in PointNBuild mode
				if(buildMode==_BuildMode.PointNBuild && showBuildSample) BuildManager.ClearSampleTower();
			}
			
			//selected tower information UI
			if(currentSelectedTower!=null){
				SelectedTowerUI();
			}
			//else towerUIRect=new Rect(0, 0, 0, 0);
			
			//if paused, draw the pause menu
			if(paused){
				float startX=Screen.width/2-100;
				float startY=Screen.height*0.35f;
				
				for(int i=0; i<4; i++) GUI.Box(new Rect(startX, startY, 200, 180), "Game Paused");
				
				startX+=50;
				
				if(GUI.Button(new Rect(startX, startY+=30, 100, 30), "Resume Game")){
					TogglePause();
				}
				if(GUI.Button(new Rect(startX, startY+=35, 100, 30), "Restart Level")){
					Application.LoadLevel(Application.loadedLevelName);
				}
				if(GUI.Button(new Rect(startX, startY+=35, 100, 30), "Next Level")){
					if(nextLevel!="") Application.LoadLevel(nextLevel);
				}
				if(GUI.Button(new Rect(startX, startY+=35, 100, 30), "Main Menu")){
					if(mainMenu!="") Application.LoadLevel(mainMenu);
				}
			}
			
		}
		//gameOver, draw the game over screen
		else{
			
			float startX=Screen.width/2-100;
			float startY=Screen.height*0.35f;
			
			string levelCompleteString="Level Complete";
			if(!winLostFlag) levelCompleteString="Level Lost";
		
			for(int i=0; i<4; i++) GUI.Box(new Rect(startX, startY, 200, 150), levelCompleteString);
			
			startX+=50;
			
			if(GUI.Button(new Rect(startX, startY+=30, 100, 30), "Restart Level")){
				Application.LoadLevel(Application.loadedLevelName);
			}
			if(alwaysEnableNextButton || winLostFlag){
				if(GUI.Button(new Rect(startX, startY+=35, 100, 30), "Next Level")){
					if(nextLevel!="") Application.LoadLevel(nextLevel);
				}
			}
			if(GUI.Button(new Rect(startX, startY+=35, 100, 30), "Main Menu")){
				if(mainMenu!="") Application.LoadLevel(mainMenu);
			}
		
		}
		
		
		//if game message is not empty, show the game message.
		//shift the text alignment to LowerRight first then back to UpperLeft after the message
		//~ if(gameMessage!=""){
			//~ GUI.skin.label.alignment=TextAnchor.LowerRight;
			//~ GUI.Label(new Rect(0, 0, Screen.width-5, Screen.height+12), gameMessage);
			//~ GUI.skin.label.alignment=TextAnchor.UpperLeft;
		//~ }
		
		
		MessageRoutine();
		
		AbilityRoutine();
	}
	
	public _OrientationD abilityTooltipOrientation=_OrientationD.TopRight;
	void ShowAbilityToolTip(int ID){
		
		Ability ability=AbilityManager.GetMatchingAbility(ID);
		
		//create a new GUIStyle to highlight the tower name with different font size and color
		GUIStyle tempGUIStyle=new GUIStyle();
		
		tempGUIStyle.fontStyle=FontStyle.Bold;
		tempGUIStyle.fontSize=16;
		
		//create the GUIContent that shows the tower's name
		GUIContent guiContentTitle=new GUIContent(ability.name); 
		
		//calculate the height required
		float heightT=tempGUIStyle.CalcHeight(guiContentTitle, 150);
		
		//switch to normal guiStyle and calculate the height needed to display cost
		tempGUIStyle.fontStyle=FontStyle.Normal;
		int[] cost=ability.costs;
		float heightC=0;	
		for(int i=0; i<cost.Length; i++){
			if(cost[i]>0){
				heightC+=25;
			}
		}
		
		//create a guiContent showing the tower description
		GUIContent guiContent=new GUIContent(""+ability.desp); 
		//calculate the height require to show the tower description
		float heightD= GUI.skin.GetStyle("Label").CalcHeight(guiContent, 150);
		
		//sum up all the height, so the tooltip box size can be known
		float height=heightT+heightC+heightD+30;
		
		//set the tooltip draw position
		int y=32;
		int x=5;
		//if this is a fixed or drag and drop mode, tooltip always appear at bottom left corner instaed of top left corner
		//~ if(buildMenuType==_BuildMenuType.Fixed || buildMode==_BuildMode.DragNDrop){
			//~ y=(int)(Screen.height-height-buildListRect.height-bottomPanelRect.height-2);
			//~ x=(int)(Mathf.Floor(Input.mousePosition.x/50))*50;
		//~ }
		//~ else{
			//~ y=(int)(Screen.height-height-buildListRect.height-bottomPanelRect.height-2);
			//~ x=(int)Input.mousePosition.x-160;
			//~ y=(int)(Screen.height-Input.mousePosition.y-height);
		//~ }
		
		if(abilityTooltipOrientation==_OrientationD.TopLeft){
			x=(int)Input.mousePosition.x-160;
			y=(int)(Screen.height-Input.mousePosition.y-height);
		}
		else if(abilityTooltipOrientation==_OrientationD.TopRight){
			x=(int)Input.mousePosition.x;
			y=(int)(Screen.height-Input.mousePosition.y-height);
		}
		else if(abilityTooltipOrientation==_OrientationD.BottomLeft){
			x=(int)Input.mousePosition.x-160;
			y=(int)(Screen.height-Input.mousePosition.y);
		}
		else if(abilityTooltipOrientation==_OrientationD.BottomRight){
			x=(int)Input.mousePosition.x;
			y=(int)(Screen.height-Input.mousePosition.y);
		}

		//define the rect then draw the box
		Rect rect=new Rect(x, y, 160, height);
		for(int i=0; i<4; i++) GUI.Box(rect, "");
		
		//display all the guiContent assigned ealier
		GUI.BeginGroup(rect);
			//show tower name, format it to different text style, size and color
			tempGUIStyle.fontStyle=FontStyle.Bold;
			tempGUIStyle.fontSize=16;
			tempGUIStyle.normal.textColor=new Color(1, 1, 0, 1f);
			GUI.Label(new Rect(5, 5, 150, heightT), guiContentTitle, tempGUIStyle);
		
			GUI.Label(new Rect(5, 5+heightT+2, 150, 25), "Energy - "+ability.energy.ToString());
			heightT+=20;
		
			//show tower's cost
			//get the resourceList from GameControl so we have all the information
			Resource[] rscList=GameControl.GetResourceList();
			//loop throught all the resource type in the cost list
			for(int i=0; i<cost.Length; i++){
				//only show it this the cost required something from this type of resource
				if(cost[i]>0){
					//check if the resource type has a icon or not
					if(rscList[i].icon!=null){
						//show the cost with the resource type icon, draw the icon first then the cost value
						GUI.Label(new Rect(5, 5+heightT+i*20, 25, 25), rscList[i].icon);
						GUI.Label(new Rect(5+25, 5+heightT+i*20+2, 150, 25), "- "+cost[i].ToString());
					}
					else{
						//show the cost with the resource type name
						GUI.Label(new Rect(5, 5+heightT+i*20, 150, 25), " - "+cost[i].ToString()+rscList[i].name);
					}
				}
			}
			
			//show desciption
			GUI.Label(new Rect(5, 5+heightC+heightT, 150, heightD), guiContent);
		GUI.EndGroup();
	}
	
	
	
	//show tooptip when a build button is hovered
	void ShowToolTip(int ID){
		
		//get the tower component first
		UnitTower[] towerList=BuildManager.GetTowerList();
		UnitTower tower=towerList[ID];
		
		//create a new GUIStyle to highlight the tower name with different font size and color
		GUIStyle tempGUIStyle=new GUIStyle();
		
		tempGUIStyle.fontStyle=FontStyle.Bold;
		tempGUIStyle.fontSize=16;
		
		//create the GUIContent that shows the tower's name
		string towerName=tower.unitName;
		GUIContent guiContentTitle=new GUIContent(towerName); 
		
		//calculate the height required
		float heightT=tempGUIStyle.CalcHeight(guiContentTitle, 150);
		
		//switch to normal guiStyle and calculate the height needed to display cost
		tempGUIStyle.fontStyle=FontStyle.Normal;
		int[] cost=tower.GetCost();
		float heightC=0;	
		for(int i=0; i<cost.Length; i++){
			if(cost[i]>0){
				heightC+=25;
			}
		}
		
		//create a guiContent showing the tower description
		string towerDesp=tower.GetDescription();
		GUIContent guiContent=new GUIContent(""+towerDesp); 
		//calculate the height require to show the tower description
		float heightD= GUI.skin.GetStyle("Label").CalcHeight(guiContent, 150);
		
		//sum up all the height, so the tooltip box size can be known
		float height=heightT+heightC+heightD+10;
		
		//set the tooltip draw position
		int y=32;
		int x=5;
		//if this is a fixed or drag and drop mode, tooltip always appear at bottom left corner instaed of top left corner
		//~ if(buildMenuType==_BuildMenuType.Fixed || buildMode==_BuildMode.DragNDrop){
			//~ y=(int)(Screen.height-height-buildListRect.height-bottomPanelRect.height-2);
			//~ x=(int)(Mathf.Floor(Input.mousePosition.x/50))*50;
		//~ }
		//~ else{
			//~ y=(int)(Screen.height-height-buildListRect.height-bottomPanelRect.height-2);
			//~ x=(int)Input.mousePosition.x-160;
			//~ y=(int)(Screen.height-Input.mousePosition.y-height);
		//~ }
		
		if(buildTooltipOrientation==_OrientationD.TopLeft){
			x=(int)Input.mousePosition.x-160;
			y=(int)(Screen.height-Input.mousePosition.y-height);
		}
		else if(buildTooltipOrientation==_OrientationD.TopRight){
			x=(int)Input.mousePosition.x;
			y=(int)(Screen.height-Input.mousePosition.y-height);
		}
		else if(buildTooltipOrientation==_OrientationD.BottomLeft){
			x=(int)Input.mousePosition.x-160;
			y=(int)(Screen.height-Input.mousePosition.y);
		}
		else if(buildTooltipOrientation==_OrientationD.BottomRight){
			x=(int)Input.mousePosition.x;
			y=(int)(Screen.height-Input.mousePosition.y);
		}

		//define the rect then draw the box
		Rect rect=new Rect(x, y, 160, height);
		for(int i=0; i<4; i++) GUI.Box(rect, "");
		
		//display all the guiContent assigned ealier
		GUI.BeginGroup(rect);
			//show tower name, format it to different text style, size and color
			tempGUIStyle.fontStyle=FontStyle.Bold;
			tempGUIStyle.fontSize=16;
			tempGUIStyle.normal.textColor=new Color(1, 1, 0, 1f);
			GUI.Label(new Rect(5, 5, 150, heightT), guiContentTitle, tempGUIStyle);
		
			//show tower's cost
			//get the resourceList from GameControl so we have all the information
			Resource[] rscList=GameControl.GetResourceList();
			//loop throught all the resource type in the cost list
			for(int i=0; i<cost.Length; i++){
				//only show it this the cost required something from this type of resource
				if(cost[i]>0){
					//check if the resource type has a icon or not
					if(rscList[i].icon!=null){
						//show the cost with the resource type icon, draw the icon first then the cost value
						GUI.Label(new Rect(5, 5+heightT+i*20, 25, 25), rscList[i].icon);
						GUI.Label(new Rect(5+25, 5+heightT+i*20+2, 150, 25), "- "+cost[i].ToString());
					}
					else{
						//show the cost with the resource type name
						GUI.Label(new Rect(5, 5+heightT+i*20, 150, 25), " - "+cost[i].ToString()+rscList[i].name);
					}
				}
			}
			
			//show desciption
			GUI.Label(new Rect(5, 5+heightC+heightT, 150, heightD), guiContent);
		GUI.EndGroup();
	}
	
	
	public PanelPosInfo buildPanelPos;
	public _OrientationD buildTooltipOrientation;
	
	//for drag and drop, show all available tower
	void BuildMenuAllTowersFix(){
		GUI.depth=10;
		
		UnitTower[] towerList=BuildManager.GetTowerList();
		
		int width=buildPanelPos.itemWidth;
		int height=buildPanelPos.itemHeight;
		
		//~ int x=0;
		//~ int y=Screen.height-height-6-(int)bottomPanelRect.height;
		
		float x=buildPanelPos.x;
		float y=buildPanelPos.y;
		
		for(int i=0; i<3; i++) GUI.Box(buildListRect, "");
		
		x+=3;	y+=3;
		
		for(int i=0; i<towerList.Length; i++){
			UnitTower tower=towerList[i];
				
			GUIContent guiContent=new GUIContent(tower.icon, i.ToString()); 
			if(GUI.Button(new Rect(x, y, width, height), guiContent)){
				if(!perkWindow){
					string result=BuildManager.BuildTowerDragNDrop(tower);
					if(result!=""){
						DisplayMessage(result);
					}
				}
			}
			
			if(buildPanelPos.orientation==_Orientation.Right) x+=width+3;
			else if(buildPanelPos.orientation==_Orientation.Left) x-=width+3;
			else if(buildPanelPos.orientation==_Orientation.Up) y-=height+3;
			else if(buildPanelPos.orientation==_Orientation.Down) y+=height+3;
		}
	}
	
	
	void BuildMenuPie(){
		BuildableInfo currentBuildInfo=BuildManager.GetBuildInfo();
				
		//calculate the position in which the build list ui will be appear at
		Vector3 screenPos = Camera.main.WorldToScreenPoint(currentBuildInfo.position);
		
		int width=50;
		int height=50;
		
		Vector2[] piePos=GetPieMenuPos(currentBuildList.Length, screenPos, (int)1.414f*(width+height)/2);
		Vector2 offset=CheckShiftPiePos(piePos, width, height);
		
		if(offset.magnitude>0){
			for(int i=0; i<piePos.Length; i++){
				piePos[i].x+=offset.x;
				piePos[i].y+=offset.y;
			}
			screenPos.x+=offset.x;
			screenPos.y+=offset.y;
		}
		
		Rect[] buildButtonRectList=new Rect[currentBuildList.Length+1];

		//show up the build buttons, scrolling through currentBuildList initiated whevenever the menu is first brought up
		UnitTower[] towerList=BuildManager.GetTowerList();
			
		for(int num=0; num<currentBuildList.Length; num++){
			int ID=currentBuildList[num];
			
			if(ID>=0){
				UnitTower tower=towerList[ID];
				Vector2 point=piePos[num];
				
				buildButtonRectList[num]=new Rect(point.x-width/2, Screen.height-point.y-height, width, height);
				GUI.Box(buildButtonRectList[num], "");
				
				GUIContent guiContent=new GUIContent(tower.icon, ID.ToString()); 
				if(GUI.Button(buildButtonRectList[num], guiContent)){
					//if building was successful, break the loop can close the panel
					if(BuildButtonPressed(tower)){
						ClearBuildListRect();
						return;
					}
				}
				
			}
		}
		
		
			
		//clear buildmode button
		buildButtonRectList[currentBuildList.Length]=new Rect(screenPos.x-width/2, Screen.height-screenPos.y+height*0.5f, width, height);
		
		if(GUI.Button(buildButtonRectList[currentBuildList.Length], "X")){
			buildMenu=false;
			BuildManager.ClearBuildPoint();
			ClearBuildListRect();
		}
		
		scatteredRectList=buildButtonRectList;
	}
	
	
	void BuildMenuBox(){
		BuildableInfo currentBuildInfo=BuildManager.GetBuildInfo();
				
		//calculate the position in which the build list ui will be appear at
		Vector3 screenPos = Camera.main.WorldToScreenPoint(currentBuildInfo.position);
		
		int width=50;
		int height=50;
				
		int x=(int)screenPos.x-width;
		x=Mathf.Clamp(x, 0, Screen.width-width*2);
				
		int menuLength=((int)Mathf.Floor((currentBuildList.Length+2)/2))*(height+3);
		int y=Screen.height-(int)screenPos.y;	//invert the height
		y-=menuLength/2-3;
		y=Mathf.Clamp(y, 29, Screen.height-menuLength-(int)bottomPanelRect.height);
		
		//calculate the buildlist rect
		buildListRect=new Rect(x-3, y-3, width*2+6+3, menuLength+4);
		for(int i=0; i<3; i++) GUI.Box(buildListRect, "");
		
		//show up the build buttons, scrolling through currentBuildList initiated whevenever the menu is first brought up
		UnitTower[] towerList=BuildManager.GetTowerList();
			
		for(int num=0; num<currentBuildList.Length; num++){
			int ID=currentBuildList[num];
			
			if(ID>=0){
				UnitTower tower=towerList[ID];
				
				GUIContent guiContent=new GUIContent(tower.icon, ID.ToString()); 
				if(GUI.Button(new Rect(x, y, width, height), guiContent)){
					//if building was successful, break the loop can close the panel
					if(BuildButtonPressed(tower)){
						ClearBuildListRect();
						return;
					}
				}
				
				if(num%2==1){
					x-=width+3;
					y+=height+3;
				}
				else x+=width+3;
			}
		}
			
		//clear buildmode button
		if(GUI.Button(new Rect(x, y, width, height), "X")){
			buildMenu=false;
			BuildManager.ClearBuildPoint();
			ClearBuildListRect();
		}
	}
	
	
	void BuildMenuFix(){
		GUI.depth=10;
		
		int width=buildPanelPos.itemWidth;
		int height=buildPanelPos.itemHeight;
				
		//~ int x=0;
		//~ int y=Screen.height-height-6-(int)bottomPanelRect.height;
		
		float x=buildPanelPos.x;
		float y=buildPanelPos.y;
		
		for(int i=0; i<3; i++) GUI.Box(buildListRect, "");
		
		//show up the build buttons, scrolling through currentBuildList initiated whevenever the menu is first brought up
		UnitTower[] towerList=BuildManager.GetTowerList();
		
		x+=3;	y+=3;
			
		for(int num=0; num<currentBuildList.Length; num++){
			int ID=currentBuildList[num];
			
			if(ID>=0){
				UnitTower tower=towerList[ID];
				
				GUIContent guiContent=new GUIContent(tower.icon, ID.ToString()); 
				if(GUI.Button(new Rect(x, y, width, height), guiContent)){
					//if building was successful, break the loop can close the panel
					if(BuildButtonPressed(tower)){
						ClearBuildListRect();
						return;
					}
				}
				
				if(buildPanelPos.orientation==_Orientation.Right) x+=width+3;
				else if(buildPanelPos.orientation==_Orientation.Left) x-=width+3;
				else if(buildPanelPos.orientation==_Orientation.Up) y-=height+3;
				else if(buildPanelPos.orientation==_Orientation.Down) y+=height+3;
			}
		}
			
		//clear buildmode button
		if(GUI.Button(new Rect(x, y, width, height), "X")){
			buildMenu=false;
			BuildManager.ClearBuildPoint();
			ClearBuildListRect();
		}
	}
	
	private bool BuildButtonPressed(UnitTower tower){
		string result=BuildManager.BuildTowerPointNBuild(tower);
		if(result==""){
			//built, clear the build menu flag
			buildMenu=false;
			ClearBuildListRect();
			return true;
		}	
		else{
			//~ //Debug.Log("build failed. invalide position");
			DisplayMessage(result);
			return false;
		}
	}
	
	
	//show to draw the selected tower info panel, which include the upgrade and sell button
	void SelectedTowerUI(){
		
		//~ float initialStartY=Screen.height-455-bottomPanelRect.height;
		float initialStartY=90;
		
		float startX=Screen.width-260;
		//~ float startY=Screen.height-455-bottomPanelRect.height;
		float startY=initialStartY;
		float widthBox=250;
		float heightBox=450;
		
		towerUIRect=new Rect(startX, startY, widthBox, heightBox);
		for(int i=0; i<3; i++) GUI.Box(towerUIRect, "");
		
		startX=Screen.width-260+20;
		//~ startY=Screen.height-455-bottomPanelRect.height+20;
		startY+=20;
		
		float width=250-40;
		float height=20;
		
		GUIStyle tempGUIStyle=new GUIStyle();
		
		tempGUIStyle.fontStyle=FontStyle.Bold;
		tempGUIStyle.fontSize=16;
		tempGUIStyle.normal.textColor=new Color(1, 1, 0, 1f);
		
		string towerName=currentSelectedTower.unitName;
		GUIContent guiContentTitle=new GUIContent(towerName); 
		
		GUI.Label(new Rect(startX, startY, width, height), guiContentTitle, tempGUIStyle);
		
		tempGUIStyle.fontStyle=FontStyle.Bold;
		tempGUIStyle.fontSize=13;
		tempGUIStyle.normal.textColor=new Color(1, 0, 0, 1f);
		
		GUI.Label(new Rect(startX, startY+=height, width, height), "Level: "+currentSelectedTower.GetLevel().ToString(), tempGUIStyle);
		
		startY+=10;
		
		
		string towerInfo="";
		
		_TowerType type=currentSelectedTower.type;
		
		//display relevent information based on tower type
		if(type==_TowerType.ResourceTower){
			
			//show resource gain value and cooldown only
			int[] incomes=currentSelectedTower.GetIncomes();
			string cd=currentSelectedTower.GetCooldown().ToString("f1");
			
			Resource[] resourceList=GameControl.GetResourceList();
			
			string rsc="";
			for(int i=0; i<incomes.Length; i++){
				rsc="Increase "+resourceList[i].name+" by "+incomes[i].ToString("0")+" for every "+cd+"sec\n";
			}
			
			towerInfo+=rsc;
			
		}
		else if(type==_TowerType.SupportTower){
			
			//show buff info only
			BuffStat buffInfo=currentSelectedTower.GetBuff();
			
			string buff="";
			if(buffInfo.damageBuff!=0) buff+="Buff damage by "+(buffInfo.damageBuff*100).ToString("f1")+"%\n";
			if(buffInfo.rangeBuff!=0) buff+="Buff range by "+(buffInfo.rangeBuff*100).ToString("f1")+"%\n";
			if(buffInfo.cooldownBuff!=0) buff+="Reduce CD by "+(buffInfo.cooldownBuff*100).ToString("f1")+"%\n";
			if(buffInfo.regenHP!=0) buff+="Renegerate HP by "+(buffInfo.regenHP).ToString("f1")+" per seconds\n";
			
			towerInfo+=buff;
			
		}
		else if(type==_TowerType.Mine){
			//show the basic info for mine
			if(currentSelectedTower.GetDamage()>0)
				towerInfo+="Damage: "+currentSelectedTower.GetDamage().ToString("f1")+"\n";
			if(type==_TowerType.TurretTower && currentSelectedTower.GetAoeRadius()>0)
				towerInfo+="AOE Radius: "+currentSelectedTower.GetRange().ToString("f1")+"\n";
			//~ if(currentSelectedTower.GetStunDuration()>0)
				//~ towerInfo+="Stun target for "+currentSelectedTower.GetStunDuration().ToString("f1")+"sec\n";
			Stun stun=currentSelectedTower.GetStun();
			if(stun.chance>0 && stun.duration>0){
				towerInfo+=(stun.chance*100).ToString("f0")+"% chance to stun target for "+stun.duration.ToString("f1")+"sec\n";
			}
			
			towerInfo+=AddCriticalLabel();
			
			//if the mine have damage over time value, display it
			Dot dot=currentSelectedTower.GetDot();
			float totalDot=dot.GetTotalDamage();
			if(totalDot>0){
				string dotInfo="Cause "+totalDot.ToString("f1")+" damage over the next "+dot.duration+" sec\n";
				
				towerInfo+=dotInfo;
			}
			
			//if the mine have slow value, display it
			Slow slow=currentSelectedTower.GetSlow();
			if(slow.duration>0){
				string slowInfo="Slow target by "+(slow.slowFactor*100).ToString("f1")+"% for "+slow.duration.ToString("f1")+"sec\n";
				towerInfo+=slowInfo;
			}
		}
		else if(type==_TowerType.TurretTower || type==_TowerType.AOETower || type==_TowerType.DirectionalAOETower){
			
			//show the basic info for turret and aoeTower
			if(currentSelectedTower.GetDamage()>0)
				towerInfo+="Damage: "+currentSelectedTower.GetDamage().ToString("f1")+"\n";
			if(currentSelectedTower.GetCooldown()>0)
				towerInfo+="Cooldown: "+currentSelectedTower.GetCooldown().ToString("f1")+"sec\n";
			if(type==_TowerType.TurretTower && currentSelectedTower.GetAoeRadius()>0)
				towerInfo+="AOE Radius: "+currentSelectedTower.GetAoeRadius().ToString("f1")+"\n";
			//~ if(currentSelectedTower.GetStunDuration()>0)
				//~ towerInfo+="Stun target for "+currentSelectedTower.GetStunDuration().ToString("f1")+"sec\n";
			Stun stun=currentSelectedTower.GetStun();
			if(stun.chance>0 && stun.duration>0){
				towerInfo+=(stun.chance*100).ToString("f0")+"% chance to stun target for "+stun.duration.ToString("f1")+"sec\n";
			}
			
			towerInfo+=AddCriticalLabel();
			
			//if the tower have damage over time value, display it
			Dot dot=currentSelectedTower.GetDot();
			float totalDot=dot.GetTotalDamage();
			if(totalDot>0){
				string dotInfo="Cause "+totalDot.ToString("f1")+" damage over the next "+dot.duration+" sec\n";
				
				towerInfo+=dotInfo;
			}
			
			//if the tower have slow value, display it
			Slow slow=currentSelectedTower.GetSlow();
			if(slow.duration>0){
				string slowInfo="Slow target by "+(slow.slowFactor*100).ToString("f1")+"% for "+slow.duration.ToString("f1")+"sec\n";
				towerInfo+=slowInfo;
			}
		}
		
		
		//show the tower's description
		towerInfo+="\n\n"+currentSelectedTower.description;
		
		GUIContent towerInfoContent=new GUIContent(towerInfo);
			
		//draw all the information on screen
		float contentHeight= GUI.skin.GetStyle("Label").CalcHeight(towerInfoContent, 200);
		GUI.Label(new Rect(startX, startY+=20, width, contentHeight), towerInfoContent);
	
		
		//reset the draw position
		//~ startY=Screen.height-180-bottomPanelRect.height;
		startY=initialStartY+275;
		
		if(enableTargetPrioritySwitch){
			if(currentSelectedTower.type==_TowerType.TurretTower || currentSelectedTower.type==_TowerType.DirectionalAOETower){
				if(currentSelectedTower.targetingArea!=_TargetingArea.StraightLine){
					GUI.Label(new Rect(startX, startY, 120, 30), "Targeting Priority:");
					if(GUI.Button(new Rect(startX+120, startY-3, 100, 30), currentSelectedTower.targetPriority.ToString())){
						if(currentSelectedTower.targetPriority==_TargetPriority.Nearest) currentSelectedTower.SetTargetPriority(1);
						else if(currentSelectedTower.targetPriority==_TargetPriority.Weakest) currentSelectedTower.SetTargetPriority(2);
						else if(currentSelectedTower.targetPriority==_TargetPriority.Toughest) currentSelectedTower.SetTargetPriority(3);
						else if(currentSelectedTower.targetPriority==_TargetPriority.Random) currentSelectedTower.SetTargetPriority(0);
					}
					startY+=30;
				}
			}
		}
		
		if(enableTargetDirectionSwitch){
			if(currentSelectedTower.type==_TowerType.TurretTower || currentSelectedTower.type==_TowerType.DirectionalAOETower){
				if(currentSelectedTower.targetingArea!=_TargetingArea.AllAround){
					//~ GUI.changed = false;
					GUI.Label(new Rect(startX, startY, 120, 30), "Targeting Direction:");
					float direction=currentSelectedTower.targetingDirection;
					direction=GUI.HorizontalSlider(new Rect(startX+120, startY+4, 100, 30), direction, 0, 359F);
					currentSelectedTower.SetTargetingDirection(direction);
					//~ if(GUI.changed) GameControl.ShowIndicator(currentSelectedTower);
				}
			}
		}

		//check if the tower can be upgrade
		bool upgradable=false;
		if(!currentSelectedTower.IsLevelCapped() && currentSelectedTower.IsBuilt()){
			upgradable=true;
		}
		
		//reset the draw position
		//~ startY=Screen.height-50-bottomPanelRect.height;
		startY=initialStartY+405;
		
		//if the tower is eligible to upgrade, draw the upgrade button
		if(upgradable){
			if(GUI.Button(new Rect(startX, startY, 100, 30), new GUIContent("Upgrade", "1"))){
				//upgrade the tower, if false is return, player have insufficient fund
				if(!currentSelectedTower.Upgrade()){
					DisplayMessage("Insufficient Resource");
					//~ Debug.Log("Insufficient Resource");
				}
			}
		}
		//sell button
		if(currentSelectedTower.IsBuilt()){
			if(GUI.Button(new Rect(startX+110, startY, 100, 30), new GUIContent("Sell", "2"))){
				currentSelectedTower.Sell();
			}
		}
		
		//if the cursor is hover on the upgrade button, show the cost
		if(GUI.tooltip=="1"){
			Resource[] rscList=GameControl.GetResourceList();
			int[] cost=currentSelectedTower.GetCost();
			
			int count=0;
			foreach(int val in cost){
				if(val>0) count+=1;
			}
			
			startY-=1+count*25;
			//~ for(int i=0; i<3; i++) GUI.Box(new Rect(startX, startY, count*25-3, 150), "");
			count=0;
			for(int i=0; i<cost.Length; i++){
				if(cost[i]>0){
					if(rscList[i].icon!=null){
						GUI.Label(new Rect(startX+10, startY+count*20, 25, 25), rscList[i].icon);
						GUI.Label(new Rect(startX+10+25, startY+count*20+3, 150, 25), "- "+cost[i].ToString());
					}
					else{
						GUI.Label(new Rect(startX+10, startY+count*20, 150, 25), " - "+cost[i].ToString()+rscList[i].name);
					}
					count+=1;
				}
			}
		}
		//if the cursor is hover on the sell button, show the resource gain
		else if(GUI.tooltip=="2"){
			Resource[] rscList=GameControl.GetResourceList();
			int[] sellValue=currentSelectedTower.GetTowerSellValue();
			
			int count=0;
			foreach(int val in sellValue){
				if(val>0) count+=1;
			}
			
			startY-=1+count*25;
			count=0;
			for(int i=0; i<sellValue.Length; i++){
				if(sellValue[i]>0){
					if(rscList[i].icon!=null){
						GUI.Label(new Rect(startX+120, startY+count*20, 25, 25), rscList[i].icon);
						GUI.Label(new Rect(startX+120+25, startY+count*20+3, 150, 25), "+ "+sellValue[i].ToString());
					}
					else{
						GUI.Label(new Rect(startX+120, startY+count*20, 150, 25), " + "+sellValue[i].ToString()+rscList[i].name);
					}
					count+=1;
				}
			}
		}
	}
	
	
	
	string AddCriticalLabel(){
		string towerInfo="";
		
		float critChance=currentSelectedTower.GetCriticalChance();
		if(critChance>0){
			float critMod=currentSelectedTower.GetCriticalModifier();
			float critVal=currentSelectedTower.GetCriticalBonus();
			if(critMod>1 || critVal>0){
				towerInfo+="critical chance: "+((critChance)*100)+"%\n";
				towerInfo+="critical bonus: ";
				
				if(critMod>1 && critVal<=0) towerInfo+=((critMod)*100)+"%\n";
				else if(critMod<=1 && critVal>0) towerInfo+=critVal+"\n";
				else towerInfo+=((critMod+critVal)*100)+"%\n";
			}
		}
		
		return towerInfo;
	}
	
	
	//called whevenever the build list is called up
	//compute the number of tower that can be build in this build pointer
	//store the tower that can be build in an array of number that reference to the towerlist
	//this is so these dont need to be calculated in every frame in OnGUI()
	void UpdateBuildList(){
		
		//get the current buildinfo in buildmanager
		BuildableInfo currentBuildInfo=BuildManager.GetBuildInfo();
		
		//get the current tower list in buildmanager
		UnitTower[] towerList=BuildManager.GetTowerList();
		
		//construct a temporary interger array the length of the buildinfo
		int[] tempBuildList=new int[towerList.Length];
		//for(int i=0; i<currentBuildList.Length; i++) tempBuildList[i]=-1;
		
		//scan through the towerlist, if the tower matched the build type, 
		//put the tower ID in the towerlist into the interger array
		int count=0;	//a number to record how many towers that can be build
		for(int i=0; i<towerList.Length; i++){
			UnitTower tower=towerList[i];
			//Debug.Log(tower.unitName);
			//~ if(currentBuildInfo.specialBuildableID!=null && currentBuildInfo.specialBuildableID.Length>0){
				//~ foreach(int specialBuildableID in currentBuildInfo.specialBuildableID){
					//~ if(specialBuildableID==tower.prefabID){
						//~ count+=1;
						//~ break;
					//~ }
				//~ }
			//~ }
			//~ else{
				//~ if(tower.prefabID<0){
					//~ //check if this type of tower can be build on this platform
					//~ foreach(_TowerType type in currentBuildInfo.buildableType){
						//~ if(tower.type==type && tower.prefabID<0){
							//~ tempBuildList[count]=i;
							//~ count+=1;
							//~ break;
						//~ }
					//~ }
				//~ }
			//~ }
			
			foreach(TowerAvailability avai in currentBuildInfo.towerAvaiList){
				if(avai.ID==tower.prefabID){
					if(avai.enabledInLvl){
						tempBuildList[count]=i;
						count+=1;
					}
					break;
				}
			}
		}
		
		//for as long as the number that can be build, copy from the temp buildList to the real buildList
		currentBuildList=new int[count];
		for(int i=0; i<currentBuildList.Length; i++){
			currentBuildList[i]=tempBuildList[i];
		}
	}
	
	
	private List<string> msgList=new List<string>();
	public static void DisplayMessage(string msg){
		ui.msgList.Add(msg);
		ui.StartCoroutine(ui.ClearMessage());
	}
	
	IEnumerator ClearMessage(){
		yield return new WaitForSeconds(2f);
		msgList.RemoveAt(0);
	}
	
	void MessageRoutine(){
		GUIStyle style=new GUIStyle();
		style.alignment=TextAnchor.LowerCenter;
		style.fontSize=14;
		style.normal.textColor=Color.black;
		
		string message="";
		foreach(string msg in msgList){
			message+=msg+"\n";
		}
		
		style.fontStyle=FontStyle.Bold;
		
		GUI.depth = -99;
		GUI.Label(new Rect(0, 0, Screen.width, Screen.height/2), message, style);
		
		style.normal.textColor=Color.white;
		GUI.Label(new Rect(-2f, -2f, Screen.width, Screen.height/2), message, style);
	}
	
	
	void OnDrawGizmos(){
		if(buildMenu){
			//BuildableInfo currentBuildInfo=BuildManager.GetBuildInfo();
			//float gridSize=BuildManager.GetGridSize();
			//Gizmos.DrawCube(currentBuildInfo.position, new Vector3(gridSize, 0, gridSize));
		}
	}
	
	public GUISkin skin;
}

public enum _Orientation{Right, Left, Up, Down}
public enum _OrientationD{TopLeft, TopRight, BottomLeft, BottomRight}

[System.Serializable]
public class PanelPosInfo{
	public _Orientation orientation;
	public int x;
	public int y;
	public int itemWidth=50;
	public int itemHeight=50;
}
