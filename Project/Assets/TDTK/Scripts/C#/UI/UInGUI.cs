using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class WaveInfo{
	public Transform rootT;
	public Transform box;
	public UILabel label;
	public GameObject item;
	[HideInInspector] public WaveInfoItem[] items;
}



[System.Serializable]
public class WaveInfoItem{
	public Transform rootT;
	public UISprite icon;
	public UILabel labelCount;
}

[System.Serializable]
public class Tooltip{
	public GameObject rootObj;
	public UILabel towerName;
	public UILabel towerInfo;
	public GameObject resourceItem;
	[HideInInspector] public ResourceItem[] rscList;
	[HideInInspector] public Vector3 posOffsetFromButton;
}

[System.Serializable]
public class BuildButtonTooltip{
	public GameObject rootObj;
	public UILabel name;
	public UILabel desp;
	public GameObject resourceItem;
	[HideInInspector] public ResourceItem[] rscList;
	[HideInInspector] public Vector3 posOffsetFromButton;
}

[System.Serializable]
public class AbilityButtonTooltip{
	public GameObject rootObj;
	public UILabel name;
	public UILabel desp;
	public GameObject resourceItem;
	[HideInInspector] public ResourceItem rscItem;
	[HideInInspector] public ResourceItem[] rscItems;
	[HideInInspector] public Vector3 rscItemDefaultPos;
	[HideInInspector] public Vector3 posOffsetFromButton;
}

[System.Serializable]
public class GeneralHUD{
	public UILabel life;
	public UILabel wave;
	public UILabel timer;
	public GameObject resourceItem;
	public GameObject rscAbilityItem;
	[HideInInspector] public ResourceItem[] rscList;
	[HideInInspector] public ResourceItem rscAbilItem;
	public GameObject message;
}

[System.Serializable]
public class GeneralButton{
	public GameObject fastforward;
	public GameObject spawn;
	public GameObject spawnHighlight;
	
	[HideInInspector] public NGUIButton spawnBut;
	[HideInInspector] public NGUIButton ffBut;
}

[System.Serializable]
public class InGameMenu{
	public GameObject background;
	public GameObject main;
	public GameObject option;
	public UILabel title;
	public GameObject resume;
	[HideInInspector] public NGUIButton resumeBut;
	
	//~ public GameObject message;
}

[System.Serializable]
public class BuildPanel{
	public bool alwaysShowBuildButton=false;
	public bool autoGenerateBuildButton=true;
	
	public _Orientation buttonOrientation;
	public float buildButtonSpacing=60;
	
	public GameObject buildButtonTemplate;
	//public List<GameObject> buildButtons=new List<GameObject>();
	//public List<Transform> buildButtonsT=new List<Transform>();
	public GameObject[] buildButtons;
	public Transform[] buildButtonsT;
}

[System.Serializable]
public class AbilityPanel{
	public bool alwaysShowAbilityButton=false;
	public bool autoGenerateAbilityButton=true;
	
	public _Orientation buttonOrientation;
	public float abilityButtonSpacing=60;
	
	public GameObject abilityButtonTemplate;
	public GameObject[] abilityButtons;
	public Transform[] abilityButtonsT;
	public UILabel[] abilityCDs;
	public UISprite[] abilityIcons;
}

[System.Serializable]
public class TowerPanel{
	public GameObject rootObj;
	public UILabel name;
	public UILabel info;
	public GameObject upgradeUI;
	
	public GameObject resourceItemS;
	public GameObject resourceItemU;
	
	[HideInInspector] public ResourceItem[] rscListS;
	[HideInInspector] public ResourceItem[] rscListU;
}

[System.Serializable]
public class TowerConfiguration{
	public GameObject targetingPriority;
	public UIPopupList targetingPriorityList;
	public BoxCollider targetingPriorityBox;
	public GameObject targetingDirection;
	public UISlider targetingDirectionSlider;
}


[System.Serializable]
public class WaveInfoLayout{
	public int rowLimit=3;
	public float itemWidth=65;
	public float itemHeight=30;
	public float offsetX=15;
	public float offsetY=35;
	
	public _Orientation itemOrientation=_Orientation.Right;
	
	public bool tiePanel=true;
	public _Orientation panelOrientation=_Orientation.Right;
	
	[HideInInspector] public Vector3 waveInfoPosition;
	[HideInInspector] public Vector3 waveInfoPositionAlt;
}


[RequireComponent (typeof (UIPerknGUI))]
public class UInGUI : MonoBehaviour {

	public delegate void UIPausedHandler(bool flag); 
	public static event UIPausedHandler onPauseE;
	
	
	
	private UnitTower currentSelectedTower;
	
	//camera for nGUI layer, used to detect if any mouse/touch is landed on any of the GUI element
	public Camera nGUICam;
	//layer name for nGUI, must match the layer assigned in nGUI setup
	public string nGUILayer="NGUI";
	
	//name of the next scene and main menu
	public string nextScene;
	public string mainMenu;
	
	public float FastForwardTime=3;
	
	//build mode and build button related variable
	public enum _BuildMode{PointNBuild, DragNDrop}
	public bool usePieMenu=true;
	public bool showBuildSample=true;
	public bool enableWavePreview=true;
	
	public _BuildMode buildMode;
	private bool buildMenu=false;
	
	//build panel with the build button
	public BuildPanel buildPanel;
	public AbilityPanel abilityPanel;
	
	//tooltip for build button
	//~ public Tooltip tooltip;
	public BuildButtonTooltip buildTooltip;
	public AbilityButtonTooltip abilityTooltip;
	
	//label for general info
	public GeneralHUD hud;
	
	//spawn and fastforward button
	public GeneralButton generalButton;
	
	//pause button and pause/game-end menu
	private bool pause=false;
	public InGameMenu menu;
	
	//selected tower info panel
	public TowerPanel towerPanel;
	
	//selected tower targeting setting
	public bool enableTargetPrioritySwitch=true;
	public bool enableTargetDirectionSwitch=true;
	public TowerConfiguration towerConfig;
	
	//for showing current and next wave spawn info
	public WaveInfo currentWaveInfo;
	public WaveInfo nextWaveInfo;
	public WaveInfoLayout waveInfoLayout;
	
	public static UInGUI uiNGUI;
	
	
	void Awake(){
		uiNGUI=this;
		
		//if this ui is not used, deactivate all the nGUI gameObject
		//~ if(!this.enabled){
			//~ UIRoot[] uiRoots=(UIRoot[])FindObjectsOfType(typeof(UIRoot));
			//~ foreach(UIRoot uiRoot in uiRoots){
				//~ #if UNITY_4_0
					//~ uiRoot.gameObject.SetActive(false);
				//~ #else
					//~ uiRoot.gameObject.SetActiveRecursively(false);
				//~ #endif
			//~ }
		//~ }
		
		waveInfoLayout.waveInfoPosition=currentWaveInfo.rootT.localPosition;
		waveInfoLayout.waveInfoPositionAlt=nextWaveInfo.rootT.localPosition;
		
	}
	
	// Use this for initialization
	void Start () {
		InitResourceItem();
		
		//initiate all the display
		OnResource();
		OnLife();
		OnNewWave(0);
		OnTowerSelect();
		
		menu.resumeBut=new NGUIButton(menu.resume);
		generalButton.spawnBut=new NGUIButton(generalButton.spawn);
		generalButton.ffBut=new NGUIButton(generalButton.fastforward);
		
		//~ generalButton.spawnBut.scale.enabled=false;
		
		//if a continuous mode is used, show spawn timer
		_SpawnMode mode=SpawnManager.GetSpawnMode();
		if(mode==_SpawnMode.Continuous || mode==_SpawnMode.SkippableContinuous){
			StartCoroutine(SpawnTimerRoutine());
		}
		else{
			if(hud.timer!=null) hud.timer.text="";
		}
		
		//disable all the menu
		if(menu.background) Utility.SetActive(menu.background, false);
		if(menu.main) Utility.SetActive(menu.main, false);
		if(menu.option) Utility.SetActive(menu.option, false);
		
		//disable tower tooltip
		OnClearTowerTooltip();
		
		
		//generate build button is needed
		if(buildPanel.autoGenerateBuildButton) InitBuildButton();
		//get the position offset of the tooltip from the first button, so this offset can be cached and applied when other button are mouse overed
		if(buildPanel.buildButtons[0]!=null){
			buildTooltip.posOffsetFromButton=buildTooltip.rootObj.transform.position-buildPanel.buildButtons[0].transform.position;
		}
		
		if(AbilityManager.abilityManager!=null){
			InitAbilityButton();
			
			hud.rscAbilItem=CloneResourceItem(hud.rscAbilityItem, AbilityManager.energyInfo);
			
			//hud.rscAbilItem.rootT.parent=hud.rootObj.transform;
			OnUpdateEnergyLabel(AbilityManager.GetCurrentEnergy());
			
			//abilityTooltip.rscItem=CloneResourceItem(abilityTooltip.resourceItem, AbilityManager.energyInfo);
			
			Resource[] resourceList=GameControl.GetResourceList();
			abilityTooltip.rscItemDefaultPos=abilityTooltip.resourceItem.transform.localPosition;
			abilityTooltip.rscItems=new ResourceItem[resourceList.Length+1];
			abilityTooltip.rscItems[0]=CloneResourceItem(abilityTooltip.resourceItem, AbilityManager.energyInfo);
			for(int i=0; i<resourceList.Length; i++){
				abilityTooltip.rscItems[i+1]=CloneResourceItem(abilityTooltip.resourceItem, resourceList[i]);
				abilityTooltip.rscItems[i+1].rootT.localPosition+=new Vector3((i+1)*80, 0, 0);
			}
			
			//abilityTooltip.rscItem.rootT.parent=abilityTooltip.rootObj.transform;
			abilityTooltip.posOffsetFromButton=abilityTooltip.rootObj.transform.position-abilityPanel.abilityButtons[0].transform.position;
			
			Destroy(abilityTooltip.resourceItem);
			Destroy(hud.rscAbilityItem);
		}
		else{
			if(hud.rscAbilityItem!=null) Destroy(hud.rscAbilityItem);
			Utility.SetActive(abilityPanel.abilityButtonTemplate, false);
			Utility.SetActive(abilityTooltip.rootObj, false);
		}
		OnClearAbilityTooltip();
		
		//initiate sample menu, so player can preview the tower in pointNBuild buildphase
		if(buildMode==_BuildMode.PointNBuild && showBuildSample) BuildManager.InitiateSampleTowers();
		
		InitWaveInfo();
	}
	
	/*
	void InitWaveInfoZZ(){
		int countMax=0;
		Wave[] waves=SpawnManager.GetAllWaves();
		foreach(Wave wave in waves){
			if(wave.subWaves.Length>countMax) countMax=wave.subWaves.Length;
		}
		
		currentWaveInfo.items=new WaveInfoItem[countMax];
		for(int i=0; i<currentWaveInfo.items.Length; i++) currentWaveInfo.items[i]=new WaveInfoItem();
		
		currentWaveInfo.items[0].rootT=currentWaveInfo.item.transform;
		currentWaveInfo.items[0].icon=currentWaveInfo.item.GetComponentInChildren<UISprite>();
		currentWaveInfo.items[0].labelCount=currentWaveInfo.item.GetComponentInChildren<UILabel>();
		
		for(int i=1; i<countMax; i++){
			currentWaveInfo.items[i].rootT=(Transform)Instantiate(currentWaveInfo.item.transform);
			currentWaveInfo.items[i].rootT.parent=currentWaveInfo.item.transform.parent;
			currentWaveInfo.items[i].rootT.localPosition=currentWaveInfo.item.transform.localPosition;
			currentWaveInfo.items[i].rootT.localScale=new Vector3(1, 1, 1);
			
			currentWaveInfo.items[i].icon=currentWaveInfo.items[i].rootT.gameObject.GetComponentInChildren<UISprite>();
			currentWaveInfo.items[i].labelCount=currentWaveInfo.items[i].rootT.gameObject.GetComponentInChildren<UILabel>();
			
			if(i%2==1) currentWaveInfo.items[i].rootT.localPosition+=new Vector3(65, -(int)(i)/2*30, 0);
			else currentWaveInfo.items[i].rootT.localPosition+=new Vector3(0, -(int)(i)/2*30, 0);
		}
		
		nextWaveInfo.items=new WaveInfoItem[countMax];
		for(int i=0; i<nextWaveInfo.items.Length; i++) nextWaveInfo.items[i]=new WaveInfoItem();
		
		nextWaveInfo.items[0].rootT=nextWaveInfo.item.transform;
		nextWaveInfo.items[0].icon=nextWaveInfo.item.GetComponentInChildren<UISprite>();
		nextWaveInfo.items[0].labelCount=nextWaveInfo.item.GetComponentInChildren<UILabel>();
		
		for(int i=1; i<countMax; i++){
			nextWaveInfo.items[i].rootT=(Transform)Instantiate(currentWaveInfo.item.transform);
			nextWaveInfo.items[i].rootT.parent=nextWaveInfo.item.transform.parent;
			nextWaveInfo.items[i].rootT.localPosition=nextWaveInfo.item.transform.localPosition;
			nextWaveInfo.items[i].rootT.localScale=new Vector3(1, 1, 1);
			
			nextWaveInfo.items[i].icon=nextWaveInfo.items[i].rootT.gameObject.GetComponentInChildren<UISprite>();
			nextWaveInfo.items[i].labelCount=nextWaveInfo.items[i].rootT.gameObject.GetComponentInChildren<UILabel>();
			
			if(i%2==1) nextWaveInfo.items[i].rootT.localPosition+=new Vector3(65, -(int)(i)/2*30, 0);
			else nextWaveInfo.items[i].rootT.localPosition+=new Vector3(0, -(int)(i)/2*30, 0);
		}
		
		UpdateWaveInfo();
	}
	*/
	
	void InitWaveInfo(){
		
		int rowLimit=waveInfoLayout.rowLimit;
		float itemWidth=waveInfoLayout.itemWidth;
		float itemHeight=waveInfoLayout.itemHeight;
		
		_Orientation itemOrientation=waveInfoLayout.itemOrientation;
		
		int countMax=0;
		Wave[] waves=SpawnManager.GetAllWaves();
		foreach(Wave wave in waves){
			if(wave.subWaves.Length>countMax) countMax=wave.subWaves.Length;
		}
		
		currentWaveInfo.items=new WaveInfoItem[countMax];
		for(int i=0; i<currentWaveInfo.items.Length; i++) currentWaveInfo.items[i]=new WaveInfoItem();
		
		currentWaveInfo.items[0].rootT=currentWaveInfo.item.transform;
		currentWaveInfo.items[0].icon=currentWaveInfo.item.GetComponentInChildren<UISprite>();
		currentWaveInfo.items[0].labelCount=currentWaveInfo.item.GetComponentInChildren<UILabel>();
		
		for(int i=1; i<countMax; i++){
			currentWaveInfo.items[i].rootT=(Transform)Instantiate(currentWaveInfo.item.transform);
			currentWaveInfo.items[i].rootT.parent=currentWaveInfo.item.transform.parent;
			currentWaveInfo.items[i].rootT.localPosition=currentWaveInfo.item.transform.localPosition;
			currentWaveInfo.items[i].rootT.localScale=new Vector3(1, 1, 1);
			
			currentWaveInfo.items[i].icon=currentWaveInfo.items[i].rootT.gameObject.GetComponentInChildren<UISprite>();
			currentWaveInfo.items[i].labelCount=currentWaveInfo.items[i].rootT.gameObject.GetComponentInChildren<UILabel>();
			
			int rowCount=i%rowLimit;
			int colCount=i/rowLimit;
			
			if(itemOrientation==_Orientation.Right){
				currentWaveInfo.items[i].rootT.localPosition+=new Vector3(rowCount*itemWidth, -colCount*itemHeight, 0);
			}
			else if(itemOrientation==_Orientation.Left){
				currentWaveInfo.items[i].rootT.localPosition+=new Vector3(-rowCount*itemWidth, -colCount*itemHeight, 0);
			}
			else if(itemOrientation==_Orientation.Up){
				currentWaveInfo.items[i].rootT.localPosition+=new Vector3(colCount*itemWidth, rowCount*itemHeight, 0);
			}
			else if(itemOrientation==_Orientation.Down){
				currentWaveInfo.items[i].rootT.localPosition+=new Vector3(colCount*itemWidth, -rowCount*itemHeight, 0);
			}
			
			//~ if(i%rowLimit==1) currentWaveInfo.items[i].rootT.localPosition+=new Vector3(65, -(int)(i)/rowLimit*30, 0);
			//~ else currentWaveInfo.items[i].rootT.localPosition+=new Vector3(0, -(int)(i)/rowLimit*30, 0);
		}
		
		nextWaveInfo.items=new WaveInfoItem[countMax];
		for(int i=0; i<nextWaveInfo.items.Length; i++) nextWaveInfo.items[i]=new WaveInfoItem();
		
		nextWaveInfo.items[0].rootT=nextWaveInfo.item.transform;
		nextWaveInfo.items[0].icon=nextWaveInfo.item.GetComponentInChildren<UISprite>();
		nextWaveInfo.items[0].labelCount=nextWaveInfo.item.GetComponentInChildren<UILabel>();
		
		for(int i=1; i<countMax; i++){
			nextWaveInfo.items[i].rootT=(Transform)Instantiate(currentWaveInfo.item.transform);
			nextWaveInfo.items[i].rootT.parent=nextWaveInfo.item.transform.parent;
			nextWaveInfo.items[i].rootT.localPosition=nextWaveInfo.item.transform.localPosition;
			nextWaveInfo.items[i].rootT.localScale=new Vector3(1, 1, 1);
			
			nextWaveInfo.items[i].icon=nextWaveInfo.items[i].rootT.gameObject.GetComponentInChildren<UISprite>();
			nextWaveInfo.items[i].labelCount=nextWaveInfo.items[i].rootT.gameObject.GetComponentInChildren<UILabel>();
			
			int rowCount=i%rowLimit;
			int colCount=i/rowLimit;
			
			//nextWaveInfo.items[i].rootT.localPosition+=new Vector3(rowCount*65, -colCount*30, 0);
			if(itemOrientation==_Orientation.Right){
				nextWaveInfo.items[i].rootT.localPosition+=new Vector3(rowCount*itemWidth, -colCount*itemHeight, 0);
			}
			else if(itemOrientation==_Orientation.Left){
				nextWaveInfo.items[i].rootT.localPosition+=new Vector3(-rowCount*itemWidth, -colCount*itemHeight, 0);
			}
			else if(itemOrientation==_Orientation.Up){
				nextWaveInfo.items[i].rootT.localPosition+=new Vector3(colCount*itemWidth, rowCount*itemHeight, 0);
			}
			else if(itemOrientation==_Orientation.Down){
				nextWaveInfo.items[i].rootT.localPosition+=new Vector3(colCount*itemWidth, -rowCount*itemHeight, 0);
			}
			
			//~ if(i%rowLimit==1) nextWaveInfo.items[i].rootT.localPosition+=new Vector3(65, -(int)(i)/rowLimit*30, 0);
			//~ else nextWaveInfo.items[i].rootT.localPosition+=new Vector3(0, -(int)(i)/rowLimit*30, 0);
		}
		
		if(!enableWavePreview){
			nextWaveInfo.rootT.localPosition+=new Vector3(50000, 0, 0);
			currentWaveInfo.rootT.localPosition+=new Vector3(50000, 0, 0);
		}
		else UpdateWaveInfo();
	}
	
	
	void UpdateWaveInfo(){
		if(!enableWavePreview) return;
		
		int rowLimit=waveInfoLayout.rowLimit;
		float itemWidth=waveInfoLayout.itemWidth;
		float itemHeight=waveInfoLayout.itemHeight;
		float offsetX=waveInfoLayout.offsetX;
		float offsetY=waveInfoLayout.offsetY;
		
		_Orientation itemOrientation=waveInfoLayout.itemOrientation;
		_Orientation panelOrientation=waveInfoLayout.panelOrientation;
		bool tiePanel=waveInfoLayout.tiePanel;
		
		Vector3 waveInfoPosition=waveInfoLayout.waveInfoPosition;
		Vector3 waveInfoPositionAlt=waveInfoLayout.waveInfoPositionAlt;
		
		
		Wave currentWave=SpawnManager.GetCurrentWave();
		Wave nextWave=SpawnManager.GetNextWave();
		
		if(currentWave!=null){
			//~ int countCurrent=currentWave.subWaves.Length;
			//~ currentWaveInfo.box.localScale=new Vector3(150, 35+Mathf.Ceil(countCurrent/2f)*30, 1);
			
			float subWaveCount=currentWave.subWaves.Length;
			if(itemOrientation==_Orientation.Right){
				float x=Mathf.Clamp(subWaveCount, 1, rowLimit)*itemWidth+offsetX;
				float y=Mathf.Max(1, Mathf.Ceil(subWaveCount/rowLimit))*itemHeight+offsetY;
				x=GetPreviewBoxWidthLimit(x);
				currentWaveInfo.box.localScale=new Vector3(x, y, 1);
				currentWaveInfo.box.localPosition=new Vector3(0, 0, 0);
			}
			else if(itemOrientation==_Orientation.Left){
				float x=Mathf.Clamp(subWaveCount, 1, rowLimit)*itemWidth+offsetX;
				float y=(int)Mathf.Max(1, Mathf.Ceil(subWaveCount/rowLimit))*itemHeight+offsetY;
				x=GetPreviewBoxWidthLimit(x);
				currentWaveInfo.box.localScale=new Vector3(x, y, 1);
				currentWaveInfo.box.localPosition=new Vector3(-x+offsetX+itemWidth, 0, 0);
			}
			else if(itemOrientation==_Orientation.Up){
				float x=(int)Mathf.Max(1, Mathf.Ceil(subWaveCount/rowLimit))*itemWidth+offsetX;
				float y=Mathf.Clamp(subWaveCount, 1, rowLimit)*itemHeight+offsetY;
				x=GetPreviewBoxWidthLimit(x);
				currentWaveInfo.box.localScale=new Vector3(x, y, 1);
				currentWaveInfo.box.localPosition=new Vector3(0, y-offsetY-itemHeight, 0);
			}
			else if(itemOrientation==_Orientation.Down){
				float x=(int)Mathf.Max(1, Mathf.Ceil(subWaveCount/rowLimit))*itemWidth+offsetX;
				float y=Mathf.Clamp(subWaveCount, 1, rowLimit)*itemHeight+offsetY;
				x=GetPreviewBoxWidthLimit(x);
				currentWaveInfo.box.localScale=new Vector3(x, y, 1);
				currentWaveInfo.box.localPosition=new Vector3(0, 0, 0);
			}
			//~ Debug.Log("current wave count: "+subWaveCount+"    "+currentWaveInfo.items.Length);
			
			//~ currentWaveInfo.rootT.localPosition=new Vector3(-380, 90, 0);
			currentWaveInfo.rootT.localPosition=waveInfoPosition;
			
			
			for(int i=0; i<currentWaveInfo.items.Length; i++){
				WaveInfoItem item=currentWaveInfo.items[i];
				if(i<currentWave.subWaves.Length){
					Utility.SetActive(item.rootT.gameObject, true);
					
					item.labelCount.text=currentWave.subWaves[i].count.ToString();
					//~ Debug.Log(item.icon.spriteName=currentWave.subWaves[i].GetUnitComponent().gameObject+"   "+currentWave.subWaves[i].count);
					if(currentWave.subWaves[i].GetUnitComponent().icon!=null){
						if(item.icon.atlas.GetSprite(currentWave.subWaves[i].GetUnitComponent().icon.name)!=null){
							item.icon.spriteName=currentWave.subWaves[i].GetUnitComponent().icon.name;
						}
					}
				}
				else{
					Utility.SetActive(item.rootT.gameObject, false);
				}
			}
			
			//~ if(SpawnManager.GetCurrentWaveID()==0){
				//~ currentWaveInfo.label.text="First Wave:";
			//~ }
			//~ else{
				//~ currentWaveInfo.label.text="Wave "+SpawnManager.GetCurrentWaveID()+":";
			//~ }
			//~ if(SpawnManager.GetCurrentWaveID()==SpawnManager.GetTotalWaveCount()-1){
				//~ currentWaveInfo.label.text="Wave "+SpawnManager.GetCurrentWaveID()+":";
			//~ }
			//~ else if(SpawnManager.GetCurrentWaveID()==0){
				//~ currentWaveInfo.label.text="First Wave:";
			//~ }
			
			if(SpawnManager.GetCurrentWaveID()==SpawnManager.GetTotalWaveCount()){
				currentWaveInfo.label.text="Final Wave:";
			}
			else{
				currentWaveInfo.label.text="Wave "+SpawnManager.GetCurrentWaveID()+":";
				//~ if(GameControl.gameState==_GameState.Started) currentWaveInfo.label.text="Next Wave:";
				//~ if(GameControl.gameState==_GameState.Idle) currentWaveInfo.label.text="First Wave:";
			}
		}
		else{
			currentWaveInfo.rootT.localPosition=new Vector3(0, 50000, 0);
		}
		
		
		if(nextWave!=null){
			
			//~ if(currentWave==null) nextWaveInfo.rootT.localPosition=waveInfoPosition;
			//~ else{
				//~ nextWaveInfo.rootT.localPosition=waveInfoPosition+new Vector3(0, -(40+Mathf.Ceil(currentWave.subWaves.Length/2f)*30), 0);
			//~ }
			
			//~ int countNext=nextWave.subWaves.Length;
			//~ nextWaveInfo.box.localScale=new Vector3(150, 35+Mathf.Ceil(countNext/2f)*30, 1);
			
			float subWaveCount=nextWave.subWaves.Length;
			if(itemOrientation==_Orientation.Right){
				//~ Debug.Log("cc: "+(int)Mathf.Max(1, Mathf.Ceil(subWaveCount/rowLimit))+"   "+(subWaveCount/rowLimit));
				float x=Mathf.Clamp(subWaveCount, 1, rowLimit)*itemWidth+offsetX;
				float y=(int)Mathf.Max(1, Mathf.Ceil(subWaveCount/rowLimit))*itemHeight+offsetY;
				nextWaveInfo.box.localScale=new Vector3(Mathf.Max(115, x), y, 1);
				nextWaveInfo.box.localPosition=new Vector3(0, 0, 0);
			}
			else if(itemOrientation==_Orientation.Left){
				float x=Mathf.Clamp(subWaveCount, 1, rowLimit)*itemWidth+offsetX;
				float y=(int)Mathf.Max(1, Mathf.Ceil(subWaveCount/rowLimit))*itemHeight+offsetY;
				nextWaveInfo.box.localScale=new Vector3(Mathf.Max(115, x), y, 1);
				nextWaveInfo.box.localPosition=new Vector3(-x+offsetX+itemWidth, 0, 0);
			}
			else if(itemOrientation==_Orientation.Up){
				float x=(int)Mathf.Max(1, Mathf.Ceil(subWaveCount/rowLimit))*itemWidth+offsetX;
				float y=Mathf.Clamp(subWaveCount, 1, rowLimit)*itemHeight+offsetY;
				nextWaveInfo.box.localScale=new Vector3(Mathf.Max(115, x), y, 1);
				nextWaveInfo.box.localPosition=new Vector3(0, y-offsetY-itemHeight, 0);
			}
			else if(itemOrientation==_Orientation.Down){
				float x=(int)Mathf.Max(1, Mathf.Ceil(subWaveCount/rowLimit))*itemWidth+offsetX;
				float y=Mathf.Clamp(subWaveCount, 1, rowLimit)*itemHeight+offsetY;
				nextWaveInfo.box.localScale=new Vector3(Mathf.Max(115, x), y, 1);
				nextWaveInfo.box.localPosition=new Vector3(0, 0, 0);
			}
			//Debug.Log("next wave count: "+subWaveCount);
			
			if(tiePanel){
				if(currentWave!=null){
					if(panelOrientation==_Orientation.Right){
						nextWaveInfo.rootT.localPosition=currentWaveInfo.rootT.localPosition+new Vector3(currentWaveInfo.box.localScale.x, 0, 0);
					}
					else if(panelOrientation==_Orientation.Left){
						nextWaveInfo.rootT.localPosition=currentWaveInfo.rootT.localPosition-new Vector3(currentWaveInfo.box.localScale.x, 0, 0);
					}
					else if(panelOrientation==_Orientation.Up){
						nextWaveInfo.rootT.localPosition=currentWaveInfo.rootT.localPosition+new Vector3(0, currentWaveInfo.box.localScale.y, 0);
					}
					else if(panelOrientation==_Orientation.Down){
						nextWaveInfo.rootT.localPosition=currentWaveInfo.rootT.localPosition-new Vector3(0, currentWaveInfo.box.localScale.y, 0);
					}
				}
				else{
					nextWaveInfo.rootT.localPosition=waveInfoPosition;
				}
			}
			else{
				nextWaveInfo.rootT.localPosition=waveInfoPositionAlt;
			}
			
			
			
			for(int i=0; i<nextWaveInfo.items.Length; i++){
				WaveInfoItem item=nextWaveInfo.items[i];
				
				if(i<nextWave.subWaves.Length){
					int count=nextWave.subWaves[i].count;
					
					if(count>0){
						item.labelCount.text=count.ToString();
						if(nextWave.subWaves[i].GetUnitComponent()!=null){
							if(nextWave.subWaves[i].GetUnitComponent().icon!=null){
								if(item.icon.atlas.GetSprite(nextWave.subWaves[i].GetUnitComponent().icon.name)!=null){
									item.icon.spriteName=nextWave.subWaves[i].GetUnitComponent().icon.name;
								}
							}
						}
						Utility.SetActive(item.rootT.gameObject, true);
					}
					else Utility.SetActive(item.rootT.gameObject, false);
				}
				else{
					Utility.SetActive(item.rootT.gameObject, false);
				}
				
				if(SpawnManager.GetCurrentWaveID()==SpawnManager.GetTotalWaveCount()-1){
					nextWaveInfo.label.text="Final Wave:";
				}
				else{
					if(GameControl.gameState==_GameState.Started) nextWaveInfo.label.text="Next Wave:";
					if(GameControl.gameState==_GameState.Idle) nextWaveInfo.label.text="First Wave:";
				}
			}
		}
		else{
			nextWaveInfo.rootT.localPosition=new Vector3(0, 50000, 0);
		}
	}
	
	float GetPreviewBoxWidthLimit(float x){
		if(GameControl.gameState==_GameState.Started){
			if(SpawnManager.GetCurrentWaveID()<=SpawnManager.GetTotalWaveCount()) return x;
			else return Mathf.Max(115, x);
		}
		else{
			return Mathf.Max(115, x);
		}
	}
	
	void OnWaveCleared(){
		
	}
	
	private int selectedAbilityID=-1;
	private Ability selectedAbility;
	void OnAbilityTrigger(GameObject obj){
		int ID=0;
		for(int i=0; i<abilityPanel.abilityButtons.Length; i++){
			if(obj==abilityPanel.abilityButtons[i]){
				ID=i;
			}
		}
		
		int status=AbilityManager.SelectAbility(ID);

		if(status==1) DisplayMessage("Ability is on cooldown");
		else if(status==2) DisplayMessage("Insufficient energy");
		else if(status==3) DisplayMessage("Insufficient resource");
		else{
			selectedAbilityID=ID;
			selectedAbility=AbilityManager.GetActiveAbilityList()[ID];
		}
	}
	
	void InitAbilityButton(){
		int abilityCount=AbilityManager.GetActiveAbilityCount();
		
		abilityPanel.abilityButtons=new GameObject[abilityCount];
		abilityPanel.abilityButtonsT=new Transform[abilityCount];
		abilityPanel.abilityCDs=new UILabel[abilityCount];
		abilityPanel.abilityIcons=new UISprite[abilityCount];
		
		for(int i=0; i<abilityCount ; i++){
			CloneAbilityButton(i);
		}
		
		//disable the template button
		Utility.SetActive(abilityPanel.abilityButtonTemplate, false);
		
		//if PointNBuild mode is selected, hide the buttons
		//~ if(buildMode==_BuildMode.PointNBuild && !buildPanel.alwaysShowBuildButton) DisableBuildButtons();
	}
	
	void CloneAbilityButton(int ID){
		Ability ability=AbilityManager.GetActiveAbilityList()[ID];
		
		//clone a new button based on the template, set the position and parent to that of the template
		GameObject obj=(GameObject) Instantiate(abilityPanel.abilityButtonTemplate);
		obj.transform.parent=abilityPanel.abilityButtonTemplate.transform.parent;
		obj.transform.position=abilityPanel.abilityButtonTemplate.transform.position;
		
		//shift the position of the clone button
		if(abilityPanel.buttonOrientation==_Orientation.Right)
			obj.transform.localPosition+=new Vector3(ID*abilityPanel.abilityButtonSpacing, 0, 0);
		else if(buildPanel.buttonOrientation==_Orientation.Left)
			obj.transform.localPosition+=new Vector3(ID*-abilityPanel.abilityButtonSpacing, 0, 0);
		else if(buildPanel.buttonOrientation==_Orientation.Down)
			obj.transform.localPosition+=new Vector3(0, -ID*abilityPanel.abilityButtonSpacing, 0);
		else if(buildPanel.buttonOrientation==_Orientation.Up)
			obj.transform.localPosition+=new Vector3(0, ID*abilityPanel.abilityButtonSpacing, 0);
		
		//set the scale
		obj.transform.localScale=buildPanel.buildButtonTemplate.transform.localScale;
		
		//assign the cloned buttonto the button list,
		//this list will be used for comparison to decide which tower to be build when any of the build button is pressed
		abilityPanel.abilityButtons[ID]=obj;
		abilityPanel.abilityButtonsT[ID]=obj.transform;
		obj.name="abilityButton"+ID.ToString()+" "+ability.name;
		
		abilityPanel.abilityCDs[ID]=obj.GetComponentInChildren<UILabel>();
		abilityPanel.abilityCDs[ID].text="";
		
		UISprite[] list=obj.GetComponentsInChildren<UISprite>();
		foreach(UISprite icon in list){
			if(icon.gameObject.name=="AbilityIcon"){
				abilityPanel.abilityIcons[ID]=icon;
			}
		}
		
		if(ability.icon!=null) abilityPanel.abilityIcons[ID].spriteName=ability.icon.name;
		
		//assign the tower icon to the button, if there's one in the atlas
		//~ UISlicedSprite sprite=obj.GetComponentInChildren<UISlicedSprite>();
		//~ if(sprite!=null){
			//~ if(ability.icon!=null && sprite.atlas.GetSprite(ability.icon.name)!=null){
				//~ sprite.spriteName=ability.icon.name;
			//~ }
			//~ else{
				//~ sprite.spriteName="";
			//~ }
		//~ }
	}
	
	void OnExtendAbilityButton(Perk perk){
		
		if(perk.abilitySID<0){
			int abilityCount=abilityPanel.abilityButtons.Length+1;
			GameObject[] objList=new GameObject[abilityPanel.abilityButtons.Length+1];
			Transform[] tList=new Transform[abilityPanel.abilityButtons.Length+1];
			UILabel[] labelList=new UILabel[abilityPanel.abilityButtons.Length+1];
			UISprite[] iconList=new UISprite[abilityPanel.abilityButtons.Length+1];
			
			for(int i=0; i<abilityPanel.abilityButtons.Length; i++){
				objList[i]=abilityPanel.abilityButtons[i];
				tList[i]=abilityPanel.abilityButtonsT[i];
				labelList[i]=abilityPanel.abilityCDs[i];
				iconList[i]=abilityPanel.abilityIcons[i];
			}
			
			abilityPanel.abilityButtons=objList;
			abilityPanel.abilityButtonsT=tList;
			abilityPanel.abilityCDs=labelList;
			abilityPanel.abilityIcons=iconList;
			
			CloneAbilityButton(abilityCount-1);
		}
		else{
			List<Ability> activeAbilityList=AbilityManager.GetActiveAbilityList();
			
			int ID=0;
			foreach(Ability ability in activeAbilityList){
				if(ability.ID==perk.abilityID){
					break;
				}
				ID+=1;
			}
			
			UISlicedSprite sprite=abilityPanel.abilityButtons[ID].GetComponentInChildren<UISlicedSprite>();
			if(sprite!=null){
				if(activeAbilityList[ID].icon!=null && sprite.atlas.GetSprite(activeAbilityList[ID].icon.name)!=null){
					sprite.spriteName=activeAbilityList[ID].icon.name;
				}
				else{
					sprite.spriteName="";
				}
			}
		}
		
		//~ if(buildMode==_BuildMode.PointNBuild && !buildPanel.alwaysShowBuildButton) DisableBuildButtons();
	}
	
	IEnumerator AbilityCDLabel(int ID){
		//~ Debug.Log(ID);
		Ability ability=AbilityManager.GetActiveAbilityList()[ID];
		//~ Debug.Log(ability.cooldown+"  "+ability.cdDuration);
		yield return null;
		//~ Debug.Log(ability.cooldown);
		while(ability.cooldown>0){
			abilityPanel.abilityCDs[ID].text=ability.cooldown.ToString("f1")+"s";
			yield return null;
		}
		abilityPanel.abilityCDs[ID].text="";
		abilityPanel.abilityIcons[ID].spriteName=ability.icon.name;
	}
	
	
	
	//generate build button based on the template
	void InitBuildButton(){
		//get the full tower list from the BuildManager
		//~ UnitTower[] towerList=BuildManager.GetTowerList();
		int towerCount=BuildManager.GetTowerList().Length;
		
		buildPanel.buildButtons=new GameObject[towerCount];
		buildPanel.buildButtonsT=new Transform[towerCount];
		
		for(int i=0; i<towerCount ; i++){
			CloneBuildButton(i);
		}
		
		//disable the template button
		Utility.SetActive(buildPanel.buildButtonTemplate, false);
		
		//if PointNBuild mode is selected, hide the buttons
		if(buildMode==_BuildMode.PointNBuild && !buildPanel.alwaysShowBuildButton) DisableBuildButtons();
	}
	
	void CloneBuildButton(int ID){
		UnitTower tower=BuildManager.GetTowerList()[ID];
		
		//clone a new button based on the template, set the position and parent to that of the template
		GameObject obj=(GameObject) Instantiate(buildPanel.buildButtonTemplate);
		obj.transform.parent=buildPanel.buildButtonTemplate.transform.parent;
		obj.transform.position=buildPanel.buildButtonTemplate.transform.position;
		
		//shift the position of the clone button
		if(buildPanel.buttonOrientation==_Orientation.Right)
			obj.transform.localPosition+=new Vector3(ID*buildPanel.buildButtonSpacing, 0, 0);
		else if(buildPanel.buttonOrientation==_Orientation.Left)
			obj.transform.localPosition+=new Vector3(ID*-buildPanel.buildButtonSpacing, 0, 0);
		else if(buildPanel.buttonOrientation==_Orientation.Down)
			obj.transform.localPosition+=new Vector3(0, -ID*buildPanel.buildButtonSpacing, 0);
		else if(buildPanel.buttonOrientation==_Orientation.Up)
			obj.transform.localPosition+=new Vector3(0, ID*buildPanel.buildButtonSpacing, 0);
		
		//set the scale
		obj.transform.localScale=buildPanel.buildButtonTemplate.transform.localScale;
		
		//assign the cloned buttonto the button list,
		//this list will be used for comparison to decide which tower to be build when any of the build button is pressed
		buildPanel.buildButtons[ID]=obj;
		buildPanel.buildButtonsT[ID]=obj.transform;
		obj.name="buildButton"+ID.ToString()+" "+tower.unitName;
		
		//assign the tower icon to the button, if there's one in the atlas
		UISlicedSprite sprite=obj.GetComponentInChildren<UISlicedSprite>();
		if(sprite!=null){
			if(tower.icon!=null && sprite.atlas.GetSprite(tower.icon.name)!=null){
				sprite.spriteName=tower.icon.name;
			}
			else{
				sprite.spriteName="";
			}
		}
		
	}
	
	void OnExtendBuildButton(Perk perk){
		int towerCount=BuildManager.GetTowerList().Length;
		
		GameObject[] objList=new GameObject[towerCount];
		for(int i=0; i<buildPanel.buildButtons.Length; i++){
			objList[i]=buildPanel.buildButtons[i];
		}
		buildPanel.buildButtons=objList;
		
		Transform[] objTList=new Transform[towerCount];
		for(int i=0; i<buildPanel.buildButtonsT.Length; i++){
			objTList[i]=buildPanel.buildButtonsT[i];
		}
		buildPanel.buildButtonsT=objTList;
		
		CloneBuildButton(towerCount-1);
		
		if(buildMode==_BuildMode.PointNBuild && !buildPanel.alwaysShowBuildButton) DisableBuildButtons();
	}
	
	
	
	
	void OnEnable(){
		//subscribe to these events so we can change the UI accordingly when they happen
		GameControl.onGameOverE += OnGameOver;
		GameControl.onResourceE += OnResource;
		GameControl.onLifeE += OnLife;
		
		SpawnManager.onClearForSpawningE += OnClearForSpawning;
		SpawnManager.onWaveStartSpawnE += OnNewWave;
		//~ SpawnManager.onWaveClearedE += OnWaveCleared;
		
		UnitTower.onDestroyE += OnTowerDestroyed;
		UnitTower.onBuildCompleteE += OnTowerBuildComplete;
		UnitTower.onDragNDropE += OnTowerDragNDropEnd;
		
		UIPerknGUI.onPerkWindowE += OnPerkWindow;
		PerkManager.onPerkUnlockedE += OnPerkUnlocked;
		
		PerkManager.onRegenLifeE += OnRegenLife;
		PerkManager.onRegenResourceE += OnRegenResource;
		PerkManager.onLifeBonusWaveClearedE += OnBonusLifeWaveCleared;
		
		PerkManager.onNewTowerE += OnExtendBuildButton;
		PerkManager.onNewAbilityE += OnExtendAbilityButton;
		
		AbilityManager.onUpdateEnergyE += OnUpdateEnergyLabel;
	}
	
	void OnDisable(){
		//on disable, unsubcribe
		GameControl.onGameOverE -= OnGameOver;
		GameControl.onResourceE -= OnResource;
		GameControl.onLifeE -= OnLife;
		
		SpawnManager.onClearForSpawningE -= OnClearForSpawning;
		SpawnManager.onWaveStartSpawnE -= OnNewWave;
		//~ SpawnManager.onWaveClearedE -= OnWaveCleared;
		
		UnitTower.onDestroyE -= OnTowerDestroyed;
		UnitTower.onBuildCompleteE -= OnTowerBuildComplete;
		UnitTower.onDragNDropE -= OnTowerDragNDropEnd;
		
		UIPerknGUI.onPerkWindowE -= OnPerkWindow;
		PerkManager.onPerkUnlockedE -= OnPerkUnlocked;
		
		PerkManager.onRegenLifeE -= OnRegenLife;
		PerkManager.onRegenResourceE -= OnRegenResource;
		PerkManager.onLifeBonusWaveClearedE -= OnBonusLifeWaveCleared;
		
		PerkManager.onNewTowerE -= OnExtendBuildButton;
		PerkManager.onNewAbilityE -= OnExtendAbilityButton;
		
		AbilityManager.onUpdateEnergyE -= OnUpdateEnergyLabel;
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
	
	void OnUpdateEnergyLabel(float val){
		if(hud.rscAbilItem!=null) hud.rscAbilItem.label.text=val.ToString("f0");
	}
	
	void OnPerkWindow(bool flag){
		if(flag){
			if(buildMenu){
				BuildManager.ClearBuildPoint();
				DisableBuildButtons();
				buildMenu=false;
			}
			if(currentSelectedTower!=null){
				GameControl.ClearSelection();
				currentSelectedTower=null;
				OnTowerSelect();	
			}
		}
	}
	
	void OnPerkUnlocked(string name){
		DisplayMessage(name+" has been unlocked");
	}
	
	void OnTowerDragNDropEnd(string msg){
		if(msg!="") DisplayMessage(msg);
	}
	
	// Update is called once per frame
	void Update () {
		//~ if(Input.GetKeyDown(KeyCode.Space)){
			//~ DisplayMessage("somehting, something...");
		//~ }
		if(Input.GetKeyDown(KeyCode.N)){
			OnNextScene();
		}
		
		
		#if !UNITY_IPHONE && !UNITY_ANDROID
			if(buildMode==_BuildMode.PointNBuild)
				BuildManager.SetIndicator(Input.mousePosition);
		#endif
		
		if(selectedAbilityID>=0){
			AbilityManager.ShowIndicator(Input.mousePosition);
			
			if(Input.GetMouseButtonDown(0) && !IsCursorOnUI(Input.mousePosition) && !pause && !UIPerknGUI.IsMenuOn()){
				if(selectedAbility!=null){
					selectedAbilityID=-1;
					AbilityManager.UnselectAbility();
				}
			}
			else if(Input.GetMouseButtonUp(1)  && !IsCursorOnUI(Input.mousePosition) && !pause && !UIPerknGUI.IsMenuOn()){
				if(selectedAbility!=null){
					if(AbilityManager.CheckTriggerPoint(Input.mousePosition)){
						int status=AbilityManager.ActivateAbility(selectedAbilityID);
						if(status==-1) DisplayMessage("Invalid Target");
						else{
							StartCoroutine(AbilityCDLabel(selectedAbilityID));
							if(selectedAbility.iconUnavailable!=null){
								abilityPanel.abilityIcons[selectedAbilityID].spriteName=selectedAbility.iconUnavailable.name;
							}
						}
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
			
			//when is pressed, make sure it's not on any of the ui element and the game is not paused
			if(Input.GetMouseButtonDown(0) && !IsCursorOnUI(Input.mousePosition) && !pause && !UIPerknGUI.IsMenuOn()){
				
				//call function to see if any tower is being selected upon the click
				UnitTower tower=GameControl.Select(Input.mousePosition);
				
				//if a tower has been selected on the click
				if(tower!=null){
					//set the selecte tower
					currentSelectedTower=tower;
					//for PointNBuild mode, clear buildpoint if buildmenu is currently activated
					if(buildMode==_BuildMode.PointNBuild && buildMenu){
						BuildManager.ClearBuildPoint();
						DisableBuildButtons();
						buildMenu=false;
					}
				}
				//if no tower has been selected
				else{
					// if a tower has been selected previous, clear the selection now
					if(currentSelectedTower!=null){
						GameControl.ClearSelection();
						currentSelectedTower=null;
					}
					
					if(buildMode==_BuildMode.PointNBuild){
						//if(buildMenu){
						//	BuildManager.ClearBuildPoint();
						//	DisableBuildButtons();
						//	buildMenu=false;
						//}
						//else{
							//check for build point, if true initiate build menu
							_TileStatus status=BuildManager.CheckBuildPoint(Input.mousePosition);
							
							if(status==_TileStatus.Available || status==_TileStatus.Blocked){
								if(!buildPanel.alwaysShowBuildButton){
									buildMenu=true;
									UpdateBuildList();
									if(usePieMenu) StartCoroutine(FloatingBuildButtonRoutine());
								}
							}
							//if there are no valid build point but we are in build mode, disable it
							else{
								if(buildMenu && !buildPanel.alwaysShowBuildButton){
									buildMenu=false;
									DisableBuildButtons();
								}
								BuildManager.ClearBuildPoint();
							}
						//}
					}
				}
				
				//call function to update the tower selection panel
				OnTowerSelect();
			}
			
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
		float angle=360/((Mathf.Max(1, num-1))+1);
		
		int factor=num%2;
		//float startAngleOffset=82.5f-(factor)*2.5f;
		float startAngleOffset=0-(factor)*0f;
		
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
				float dif=sizeX*0.5f-pos.x;
				if( dif>maxDevX) maxDevX=dif;
			}
			else if(pos.x>Screen.width-sizeX*0.5f){
				float dif=Screen.width-pos.x-sizeX*0.5f;
				if(dif<maxDevX) maxDevX=dif;
			}
			
			if(pos.y<sizeY/2){
				float dif=-(pos.y-sizeY/2);
				if(dif>maxDevY) maxDevY=dif;
			}
			else if(pos.y>Screen.height-sizeY/2){
				
				float dif=Screen.height-pos.y-sizeY/2;
				if(dif<maxDevY) maxDevY=dif;
			}
		}
		
		return new Vector2(maxDevX, maxDevY);
	}
	
	IEnumerator FloatingBuildButtonRoutine(){
		while(buildMenu){
			BuildableInfo currentBuildInfo=BuildManager.GetBuildInfo();
				
			//calculate the position in which the build list ui will be appear at
			Vector3 screenPos = Camera.main.WorldToScreenPoint(currentBuildInfo.position);
			
			
			int width=50;
			int height=50;
			Vector2[] piePos=GetPieMenuPos(currentBuildList.Length, screenPos, (int)1.414f*(width+height)/2);
			Vector2 offset=CheckShiftPiePos(piePos, width, height);
			
			//Debug.Log(piePos[0]+"    "+screenPos+"  "+offset);
			
			if(offset.magnitude>0){
				for(int i=0; i<piePos.Length; i++){
					piePos[i].x+=offset.x;
					piePos[i].y+=offset.y;
				}
				screenPos.x+=offset.x;
				screenPos.y+=offset.y;
			}
			
			
			//~ screenPos.x-=(Screen.width)/2;
			//~ screenPos.y-=(Screen.height)/2;
			
			
			for(int i=0; i<currentBuildList.Length; i++){
				int ID=currentBuildList[i];
				float x=piePos[i].x-Screen.width/2;
				float y=piePos[i].y-Screen.height/2;
				float z=buildPanel.buildButtonsT[ID].position.z;
				buildPanel.buildButtonsT[ID].localPosition=new Vector3(x, y, z);
			}
			
			yield return null;
		}
	}
	
	
	//function call to check if the cursor pos on screen is hitting any nGUI lelements
	bool IsCursorOnUI(Vector3 point){
		if( nGUICam != null ){
			// pos is the Vector3 representing the screen position of the input
			Ray inputRay = nGUICam.ScreenPointToRay( Input.mousePosition );    
			RaycastHit hit;

			LayerMask mask=1<<LayerManager.LayerMiscUIOverlay();
			if( Physics.Raycast( inputRay, out hit, Mathf.Infinity, mask ) ){
				// UI was hit
				return true;
			}
		}
		
		return false;
	}
	
	
	
	//A coroutine used to update spawn timer, only called when time-based spawn mode is usded
	IEnumerator SpawnTimerRoutine(){
		if(hud.timer==null) yield break;
		
		//while game is not started, dont do anything
		hud.timer.text="";
		while(GameControl.gameState==_GameState.Idle){
			yield return null;
		}
		
		//while the game is still going, display the time
		while(SpawnManager.GetCurrentWaveID()<SpawnManager.GetTotalWaveCount() || SpawnManager.GetTotalWaveCount()==-1){
			if(GameControl.gameState==_GameState.Ended) break;
			hud.timer.text="Next Wave: "+SpawnManager.GetTimeNextSpawn().ToString("f1")+"s";
			yield return null;
		}
		
		//once the last wave is spawned, clear the label
		hud.timer.text="";
	}
	
	
	//called when game is over, just change the label accordingly
	void OnGameOver(bool flag){
		#if UNITY_4_0
			if(menu.option.activeInHierarchy) return;
		#else
			if(menu.option.active) return;
		#endif
		
		//check if the label has been assigned
		if(menu.resumeBut!=null) menu.resumeBut.label.text="Continue";
		
		//check if the label has been assigned
		if(menu.title){
			//if player won, show victory
			if(flag){
				menu.title.text="Victory!";
			}
			//else show gameover
			else{
				menu.title.text="GameOver";
			}
		}
		
		if(menu.background) Utility.SetActive(menu.background, true);
		if(menu.main) Utility.SetActive(menu.main, true);
	}
	
	//called whenever resource is gain or used
	void OnResource(){
		//make sure the label for resource is assigned
		
		//get all resource value
		int[] rsc=GameControl.GetAllResourceVal();
		
		for(int i=0; i<rsc.Length; i++){
			hud.rscList[i].label.text=rsc[i].ToString();
		}
	}
	
	//called whenever player life value is change, update the label accordingly
	void OnLife(){
		if(hud.life){
			int cap=GameControl.GetPlayerLifeCap();
			if(cap<=0){
				hud.life.text="Life: "+GameControl.GetPlayerLife().ToString();
			}
			else{
				hud.life.text="Life: "+GameControl.GetPlayerLife().ToString()+"/"+cap.ToString();
			}
		}
	}
	
	//called when pause button is pressed, toggle between pause and unpause
	void OnPause(){
		if(GameControl.gameState==_GameState.Ended) return;
		
		//if the game is currently pause, unpause it
		if(pause){
			pause=false;
			
			//resume timeScale
			if(Time.timeScale==0) Time.timeScale=1;
			
			//deactivate all pause/option menu
			if(menu.background) Utility.SetActive(menu.background, false);
			if(menu.main) Utility.SetActive(menu.main, false);
			if(menu.option) Utility.SetActive(menu.option, false);
		}
		//if the game is currently running, pause it
		else{
			pause=true;
			
			//stop time
			if(Time.timeScale>0) Time.timeScale=0;
			
			if(menu.background) Utility.SetActive(menu.background, true);
			if(menu.main) Utility.SetActive(menu.main, true);
			//activate pause menu
			
			if(buildMenu){
				BuildManager.ClearBuildPoint();
				DisableBuildButtons();
				buildMenu=false;
			}
			if(currentSelectedTower!=null){
				GameControl.ClearSelection();
				currentSelectedTower=null;
				OnTowerSelect();	
			}

		}
		
		if(onPauseE!=null) onPauseE(pause);
	}
	
	//called when back button on option menu is pressed
	void OnOptionBack(){
		if(menu.main) Utility.SetActive(menu.main, true);
		if(menu.option) Utility.SetActive(menu.option, false);
	}
	
	//called when option button on pause menu is pressed
	void OnOption(){
		if(menu.main) Utility.SetActive(menu.main, false);
		if(menu.option) Utility.SetActive(menu.option, true);
	}
	
	
	//called when sfxvolume slider is adjust
	void OnSetSFXVolume(float val){
		AudioManager.SetSFXVolume(val);
	}
	
	//called when music volume slider is adjust
	void OnSetMusicVolume(float val){
		AudioManager.SetMusicVolume(val);
	}
	
	//called when resume button on pause menu is pressed
	void OnResume(){
		//if the game is over, load next scene
		if(GameControl.gameState==_GameState.Ended){
			OnNextScene();
		}
		//else resume the game by calling OnPause 
		else{
			OnPause();
		}
	}
	
	void OnNextScene(){
		if(nextScene!="") Application.LoadLevel(nextScene);
	}
	
	void OnRestart(){
		Application.LoadLevel(Application.loadedLevelName);
	}
	
	//called when menu button on pause menu is pressed
	void OnMenu(){
		if(mainMenu!="") Application.LoadLevel(mainMenu);
	}
	
	//called when fastforward button is pressed, toggle between normal speed and fastforward speed
	void OnFFButton(){
		//ignore if the game is current being paused
		if(pause) return;
		
		if(Time.timeScale==1){
			//set the timeScale
			Time.timeScale=FastForwardTime;
			//change the text on the button
			generalButton.ffBut.label.text="Timex"+FastForwardTime.ToString("f0");
		}
		else if(Time.timeScale>1){
			//set the timeScale
			Time.timeScale=1;
			//change the text on the button
			generalButton.ffBut.label.text="Timex1";
		}
	}
	
	//called when spawn button is pressed
	void OnSpawn(){
		if(pause) return;
		
		//if this is before game started and spawnHighLight is still on, disable it
		if(generalButton.spawnHighlight) Utility.SetActive(generalButton.spawnHighlight, false);
		
		//call the function to spawn the next wave
		SpawnManager.Spawn();
		//diasble the buttonScaleTween component on the button so we can override the scale tweening
		generalButton.spawnBut.scale.enabled=false;
		
		StartCoroutine(SpawnButtonScaleDown());
	}
	
	//called when spawnManager indicate that it's ready to be spawn the next wave
	void OnClearForSpawning(bool flag){
		_SpawnMode mode=SpawnManager.GetSpawnMode();
		if(mode!=_SpawnMode.SkippableWaveCleared && mode!=_SpawnMode.SkippableContinuous){
			return;
		}
		
		//enable the default scalingTween on the spawn button 
		generalButton.spawnBut.scale.enabled=true;
		//scale the button back to default size
		if(flag){
			TweenScale.Begin(generalButton.spawn, 0.2f, new Vector3(1, 1, 1));
		}
	}
	
	//scale the spawn button to invisible, wait for 1 frame so the command override the default button scaling animation
	IEnumerator SpawnButtonScaleDown(){
		yield return null;
		TweenScale.Begin(generalButton.spawn, 0.2f, new Vector3(0.01f, 0.01f, 0.01f));
	}
	
	//called when a new wave is spawned
	void OnNewWave(int wave){
		string totalWaveCountLabel="";
		int totalWaveCount=SpawnManager.GetTotalWaveCount();
		if(totalWaveCount>0) totalWaveCountLabel="/"+totalWaveCount;
		
		//update the wave count label
		hud.wave.text="Wave: "+SpawnManager.GetCurrentWaveID()+totalWaveCountLabel;
		UpdateWaveInfo();
		//show message on screen
		if(wave>1) DisplayMessage("Incoming wave "+(wave-1).ToString()+"!!");
	}
	
	
	//function call to enable all build button
	void EnableBuildButtons(){
		foreach(GameObject button in buildPanel.buildButtons){
			Utility.SetActive(button, true);
		}
	}
	
	//function call to disaable all build button
	void DisableBuildButtons(){
		foreach(GameObject button in buildPanel.buildButtons){
			Utility.SetActive(button, false);
		}
	}
	
	//called when a build button is pressed, the button object which is pressed is passed
	void OnTowerBuild(GameObject obj){
		if(UIPerknGUI.IsMenuOn()) return;
		if(pause) return;
		
		//check which button has been pressed exactly
		//we can know from comparing to the buildButton list
		//from the position of the button in the list, we can know which towers is associated with the button
		int towerID=-1;
		for(int i=0; i<buildPanel.buildButtons.Length; i++){
			if(obj==buildPanel.buildButtons[i]){
				towerID=i;
				break;
			}
		}
		
		if(towerID==-1){
			return;
		}
		//if there's a matching button
		else{
			
			//get the tower
			UnitTower[] towerList=BuildManager.GetTowerList();
			UnitTower tower=towerList[towerID];
			
			//for PointNBuild mode
			if(buildMode==_BuildMode.PointNBuild){
				//call the function to build a tower, an empty string will be returned if building is sucessful
				string result=BuildManager.BuildTowerPointNBuild(tower);
				//if a tower is built
				if(result==""){
					//hide the build menu and clear the tooltip
					if(buildMenu && !buildPanel.alwaysShowBuildButton){
						buildMenu=false;
						DisableBuildButtons();
						BuildManager.ClearBuildPoint();
						OnClearTowerTooltip();
					}
					else{
						if(buildMode==_BuildMode.PointNBuild && showBuildSample) BuildManager.ClearSampleTower();
					}
				}
				//else just display why the building operation failed
				else{
					DisplayMessage(result);
				}
			}
			//for drag and drop mode
			else if(buildMode==_BuildMode.DragNDrop){
				//call the function to build a tower, an empty string will be returned if building is sucessful
				string result=BuildManager.BuildTowerDragNDrop(tower);
				//display why the building operation failed
				if(result!=""){
					DisplayMessage(result);
				}
			}
		}
		
		//if there's a tower being selected, clear it
		if(currentSelectedTower!=null){
			currentSelectedTower=null;
			OnTowerSelect();
		}
	}
	
	//function call to show tower tooltip when the build button is mouse-overed.
	void OnTowerTooltip(GameObject obj){
		if(UIPerknGUI.IsMenuOn()) return;
		if(pause) return;
		
		//check which button has been pressed exactly
		//we can know from comparing to the buildButton list
		//from the position of the button in the list, we can know which towers is associated with the button
		int towerID=-1;
		for(int i=0; i<buildPanel.buildButtons.Length; i++){
			if(obj==buildPanel.buildButtons[i]){
				towerID=i;
				break;
			}
		}
		
		//if there's no matching button, deactivate the all tooltip object
		if(towerID==-1){
			Utility.SetActive(buildTooltip.rootObj, false);
		}
		//if there's a matching button
		else{
		
			//get the tower
			UnitTower[] towerList=BuildManager.GetTowerList();
			UnitTower tower=towerList[towerID];
			
			//show the name
			buildTooltip.name.text=tower.unitName;
			
			
			//show the description
			buildTooltip.desp.text=tower.GetDescription();
			
			//set the tooltip position, 
			//just offset the tooltip transform position from the button transform based on the tooltip to Button position offset
			buildTooltip.rootObj.transform.position=buildPanel.buildButtons[towerID].transform.position+buildTooltip.posOffsetFromButton;
			
			//activate the tooltip so it's visible
			Utility.SetActive(buildTooltip.rootObj, true);
			
			//show the cost
			int[] cost=tower.GetCost();
			int activeCount=0;
			int totalRscCount=GameControl.GetResourceList().Length;
			for(int i=0; i<Mathf.Max(cost.Length, totalRscCount); i++){
				if(i<cost.Length){
					if(cost[i]>0){
						Vector3 pos=buildTooltip.rscList[0].rootT.localPosition+new Vector3(activeCount*50, 0, 0);
						buildTooltip.rscList[i].rootT.localPosition=pos;
						buildTooltip.rscList[i].label.text=cost[i].ToString();
						activeCount+=1;
						
						Utility.SetActive(buildTooltip.rscList[i].rootObj, true);
					}
					else Utility.SetActive(buildTooltip.rscList[i].rootObj, false);
				}
				else Utility.SetActive(buildTooltip.rscList[i].rootObj, false);
			}
			
			//if this is point and build mode, call function to show the sample tower
			if(buildMode==_BuildMode.PointNBuild && showBuildSample) BuildManager.ShowSampleTower(towerID); 
		}
	}
	
	//function call to clear tooltip, called when the mouse is moved out of a button
	void OnClearTowerTooltip(){
		#if UNITY_4_0
			if(buildTooltip.rootObj.activeInHierarchy){
				buildTooltip.rootObj.SetActive(false);
				if(buildMode==_BuildMode.PointNBuild && showBuildSample) BuildManager.ClearSampleTower();
			}
		#else
			if(buildTooltip.rootObj.active){
				buildTooltip.rootObj.SetActiveRecursively(false);
				if(buildMode==_BuildMode.PointNBuild && showBuildSample) BuildManager.ClearSampleTower();
			}
		#endif
	}
	
	
	void OnAbilityTooltip(GameObject obj){
		if(UIPerknGUI.IsMenuOn()) return;
		if(pause) return;
		
		//check which button has been pressed exactly
		//we can know from comparing to the buildButton list
		//from the position of the button in the list, we can know which towers is associated with the button
		int abilityID=-1;
		for(int i=0; i<abilityPanel.abilityButtons.Length; i++){
			if(obj==abilityPanel.abilityButtons[i]){
				abilityID=i;
				break;
			}
		}
		
		//if there's no matching button, deactivate the all tooltip object
		if(abilityID==-1){
			Utility.SetActive(abilityTooltip.rootObj, false);
		}
		//if there's a matching button
		else{
		
			//get the ability
			List<Ability> abilityList=AbilityManager.GetActiveAbilityList();
			Ability ability=abilityList[abilityID];
			
			//show the name
			abilityTooltip.name.text=ability.name;
			
			//show the description
			abilityTooltip.desp.text=ability.desp;
			
			//set the tooltip position, 
			//just offset the tooltip transform position from the button transform based on the tooltip to Button position offset
			abilityTooltip.rootObj.transform.position=abilityPanel.abilityButtons[abilityID].transform.position+abilityTooltip.posOffsetFromButton;
			
			//activate the tooltip so it's visible
			Utility.SetActive(abilityTooltip.rootObj, true);
			
			//show the cost
			//abilityTooltip.rscItem.label.text=ability.energy.ToString("f0");
			
			Vector3 startPos=abilityTooltip.rscItemDefaultPos;
			if(ability.energy>0){
				abilityTooltip.rscItems[0].label.text=ability.energy.ToString("f0");
				abilityTooltip.rscItems[0].rootT.localPosition=startPos;
				startPos+=new Vector3(60, 0, 0);
			}
			else abilityTooltip.rscItems[0].rootT.localPosition=new Vector3(-99999, 0, 0);
			
			for(int i=0; i<ability.costs.Length; i++){
				if(ability.costs[i]>0){
					abilityTooltip.rscItems[i+1].label.text=ability.costs[i].ToString("f0");
					abilityTooltip.rscItems[i+1].rootT.localPosition=startPos;
					startPos+=new Vector3(60, 0, 0);
				}
				else abilityTooltip.rscItems[i+1].rootT.localPosition=new Vector3(-99999, 0, 0);
			}
		}
	}
	
	void OnClearAbilityTooltip(){
		Utility.SetActive(abilityTooltip.rootObj, false);
		//~ #if UNITY_4_0
			//~ if(buildTooltip.rootObj.activeInHierarchy){
				//~ buildTooltip.rootObj.SetActive(false);
				//~ if(buildMode==_BuildMode.PointNBuild && showBuildSample) BuildManager.ClearSampleTower();
			//~ }
		//~ #else
			//~ if(buildTooltip.rootObj.active){
				//~ buildTooltip.rootObj.SetActiveRecursively(false);
				//~ if(buildMode==_BuildMode.PointNBuild && showBuildSample) BuildManager.ClearSampleTower();
			//~ }
		//~ #endif
	}
	
	
	//called when the value of the slider ot towerTargetingDirection is chancged
	void OnTowerTargetingDirection(float val){
		if(currentSelectedTower==null) return;
		
		//make sure targetingPriority setting applies to the selected tower.
		if(currentSelectedTower.type==_TowerType.TurretTower || currentSelectedTower.type==_TowerType.DirectionalAOETower){
			if(currentSelectedTower.targetingArea!=_TargetingArea.AllAround){
				//calculate the direction and call the function to set it
				float direction=val*360;
				currentSelectedTower.SetTargetingDirection(direction);
			}
		}
	}
	
	//called when one of the option on targetPriority popup list is selected
	void OnTowerTargetingPriority(string type){
		if(currentSelectedTower==null) return;
		
		//make sure targetingPriority setting applies to the selected tower.
		if(currentSelectedTower.type==_TowerType.TurretTower || currentSelectedTower.type==_TowerType.DirectionalAOETower){
			if(currentSelectedTower.targetingArea!=_TargetingArea.StraightLine){
				//call the function to set the target priority based on the string passed
				if(type=="Nearest") currentSelectedTower.SetTargetPriority(0);
				else if(type=="Weakest") currentSelectedTower.SetTargetPriority(1);
				else if(type=="Toughest") currentSelectedTower.SetTargetPriority(2);
				else if(type=="Random") currentSelectedTower.SetTargetPriority(3);
			}
		}
	}
	
	//called when the towerTargetingPriority panel is being clicked
	//this is just to expend the collider of the GUI element so that when clicking outside the panel when the popup list expand,
	//the selected tower wont get unselected.
	void towerTargetingPriotityBoxClicked(){
		towerConfig.targetingPriorityBox.size=new Vector3(1, 4, 1);
		StartCoroutine(ResizeBox());
	}
	//coroutine called when the towerTargetingPriority panel is being clicked
	//detect any input that could close the popup list, then resize the box back to normal
	IEnumerator ResizeBox(){
		while(true){
			bool input=false;
			input|=Input.GetMouseButton(0);
			input|=Input.GetMouseButton(1);
			input|=Input.GetKeyUp(KeyCode.Return);
			if(input) break;
			yield return null;
		}
		towerConfig.targetingPriorityBox.size=new Vector3(1, 1, 1);
	}
	
	
	//called when a tower is selected, update the selected tower info panel
	void OnTowerSelect(){
		//if there's no tower being selected, deactivate all ui element
		if(currentSelectedTower==null){
			Utility.SetActive(towerPanel.rootObj, false);
		}
		else{
			//make sure all related ui element is active and visible
			Utility.SetActive(towerPanel.rootObj, true);
			
			//set up the level string, add as many I as the tower level
			string lvl=currentSelectedTower.GetLevel().ToString();
			//show the tower's name, follow by it's level
			towerPanel.name.text=currentSelectedTower.unitName+" (lvl "+lvl+")";
			
			//get the sell value, make sure there's enough label assigned to show all the value's element before display it
			int[] sellValue=currentSelectedTower.GetTowerSellValue();
			
			
			int totalRscCount=GameControl.GetResourceCount();
			int activeCount=0;
			for(int i=0; i<Mathf.Max(sellValue.Length, totalRscCount); i++){
				if(i<sellValue.Length){
					if(sellValue[i]>0){
						
						Vector3 pos=towerPanel.rscListS[0].rootT.localPosition+new Vector3(0, activeCount*17, 0);
						towerPanel.rscListS[i].rootT.localPosition=pos;
						towerPanel.rscListS[i].label.text=sellValue[i].ToString();
						activeCount+=1;
						
						Utility.SetActive(towerPanel.rscListS[i].rootObj, true);
					}
					else Utility.SetActive(towerPanel.rscListS[i].rootObj, false);
				}
				else Utility.SetActive(towerPanel.rscListS[i].rootObj, false);
			}
			
			
			//upgrade button and cost
			//check if the tower has been updgrade to max level, only then we proceed to show the related ui element
			if(!currentSelectedTower.IsLevelCapped()){
				//make sure all the upgrade related ui element is active and visible
				Utility.SetActive(towerPanel.upgradeUI, true);
				
				int[] cost=currentSelectedTower.GetCost();
				int aCount=0;
				for(int i=0; i<Mathf.Max(cost.Length, totalRscCount); i++){
					if(i<cost.Length){
						if(cost[i]>0){
							Vector3 pos=towerPanel.rscListU[0].rootT.localPosition+new Vector3(0, aCount*17, 0);
							towerPanel.rscListU[i].rootT.localPosition=pos;
							towerPanel.rscListU[i].label.text=cost[i].ToString();
							activeCount+=1;
							
							Utility.SetActive(towerPanel.rscListU[i].rootObj, true);
						}
						else Utility.SetActive(towerPanel.rscListU[i].rootObj, false);
					}
					else Utility.SetActive(towerPanel.rscListU[i].rootObj, false);
				}
			}
			//if tower level has been capped, deactive all upgrade realted ui element so it's not visible
			else Utility.SetActive(towerPanel.upgradeUI, false);
			
			
			//create an empty string, this will be added on with all the general stats information and description of the twoer
			string towerInfo="";
			
			//get the tower type, depend on the type, different information is displayed
			_TowerType type=currentSelectedTower.type;
			
			//for resource tower, show the resource info only
			if(type==_TowerType.ResourceTower){
				
				int[] incomes=currentSelectedTower.GetIncomes();
				string cd=currentSelectedTower.GetCooldown().ToString("f1");
				
				Resource[] resourceList=GameControl.GetResourceList();
				
				string rsc="";
				for(int i=0; i<incomes.Length; i++){
					if(incomes[i]>0){
						rsc="Increase "+resourceList[i].name+" by "+incomes[i].ToString("0")+" for every "+cd+"sec\n";
					}
				}
				
				towerInfo+=rsc;
				
			}
			//for support tower, show the buff info only
			else if(type==_TowerType.SupportTower){
				
				BuffStat buffInfo=currentSelectedTower.GetBuff();
				
				string buff="";
				//only show the buff if it carry value
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
			//for direct offensive tower, show the offensive stats
			else if(type==_TowerType.TurretTower || type==_TowerType.DirectionalAOETower || type==_TowerType.AOETower){
				
				if(currentSelectedTower.GetDamage()>0){
					towerInfo+="Damage: "+currentSelectedTower.GetDamage().ToString("f1")+"\n";
				}
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
				
				Dot dot=currentSelectedTower.GetDot();
				float totalDot=dot.GetTotalDamage();
				if(totalDot>0){
					string dotInfo="Cause "+totalDot.ToString("f1")+" damage over the next "+dot.duration+" sec\n";
					towerInfo+=dotInfo;
				}
				
				Slow slow=currentSelectedTower.GetSlow();
				if(slow.duration>0){
					string slowInfo="Slow target by "+(slow.slowFactor*100).ToString("f1")+"% for "+slow.duration.ToString("f1")+"sec\n";
					towerInfo+=slowInfo;
				}
			}
			//note that block type tower doesnt carry any stats, so it's ignored
			
			
			towerInfo+="\n\n"+currentSelectedTower.description;
				
			towerPanel.info.text=towerInfo;
			
		
			if(enableTargetDirectionSwitch){
				if(currentSelectedTower.type==_TowerType.TurretTower || currentSelectedTower.type==_TowerType.DirectionalAOETower){
					if(currentSelectedTower.targetingArea!=_TargetingArea.AllAround){
						towerConfig.targetingDirectionSlider.sliderValue=currentSelectedTower.targetingDirection/360f;
					}
					else Utility.SetActive(towerConfig.targetingDirection, false);
				}
				else Utility.SetActive(towerConfig.targetingDirection, false);
			}
			else Utility.SetActive(towerConfig.targetingDirection, false);
			
			
			if(enableTargetPrioritySwitch){
				if(currentSelectedTower.type==_TowerType.TurretTower || currentSelectedTower.type==_TowerType.DirectionalAOETower){
					if(currentSelectedTower.targetingArea!=_TargetingArea.StraightLine){
						if(currentSelectedTower.targetPriority==_TargetPriority.Nearest) towerConfig.targetingPriorityList.textLabel.text="Nearest";
						else if(currentSelectedTower.targetPriority==_TargetPriority.Weakest) towerConfig.targetingPriorityList.textLabel.text="Weakest";
						else if(currentSelectedTower.targetPriority==_TargetPriority.Toughest) towerConfig.targetingPriorityList.textLabel.text="Toughest";
						else if(currentSelectedTower.targetPriority==_TargetPriority.Random) towerConfig.targetingPriorityList.textLabel.text="Random";
					}
					else Utility.SetActive(towerConfig.targetingPriority, false);
				}
				else Utility.SetActive(towerConfig.targetingPriority, false);
			}
			else Utility.SetActive(towerConfig.targetingPriority, false);
			
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
				else towerInfo+=((critMod)*100)+"%+"+critVal+"\n";
			}
		}
		
		return towerInfo;
	}
	
	void OnTowerDestroyed(UnitTower tower){
		if(currentSelectedTower==tower){
			currentSelectedTower=null;
			OnTowerSelect();
		}
	}
	
	void OnTowerBuildComplete(UnitTower tower){
		if(currentSelectedTower==tower){
			OnTowerSelect();
		}
	}
	
	void OnTowerUpgrade(){
		//Debug.Log("upgrade");
		if(currentSelectedTower){
			if(!currentSelectedTower.Upgrade()) DisplayMessage("Insufficient Resource");
		}
	}
	
	void OnTowerSell(){
		if(currentSelectedTower) currentSelectedTower.Sell();
	}
	
	private List<GameObject> msgList=new List<GameObject>();
	
	public static void DisplayMessage(string msg){
		uiNGUI._DisplayMessage(msg);
	}
	void _DisplayMessage(string msg){
		if(hud.message==null) return;
		
		foreach(GameObject msgObj in msgList){
			TweenPosition.Begin(msgObj, .25f, msgObj.transform.localPosition+new Vector3(0, 20, 0));
		}
		
		GameObject obj=(GameObject)Instantiate(hud.message);
		obj.transform.parent=hud.message.transform.parent;
		obj.transform.localScale=hud.message.transform.localScale;
		obj.GetComponent<UILabel>().text=msg;
		
		if(msgList.Count>0) obj.transform.localPosition+=new Vector3(0, 0, -msgList.Count-1);
		
		msgList.Add(obj);
		StartCoroutine(DestroyMessage(obj));
	}
	
	IEnumerator DestroyMessage(GameObject obj){
		yield return new WaitForSeconds(1.5f);
		TweenScale.Begin(obj, 0.5f, new Vector3(0.01f, 0.01f, 0.01f));
		yield return new WaitForSeconds(0.75f);
		msgList.RemoveAt(0);
		Destroy(obj);
	}
	
	IEnumerator FadeMessage(GameObject obj){
		TweenPosition.Begin(obj, 2f, obj.transform.position+new Vector3(0, 150, 0));
		yield return new WaitForSeconds(0.5f);
		TweenScale.Begin(obj, 0.5f, new Vector3(0.01f, 0.01f, 0.01f));
		Destroy(obj, 2);
	}
	
	
	private int[] currentBuildList=new int[0];
	//called whevenever the a new build point is selected in PointNBuild mode
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
			
			foreach(TowerAvailability avai in currentBuildInfo.towerAvaiList){
				//~ Debug.Log(
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
		for(int i=0; i<currentBuildList.Length; i++) currentBuildList[i]=tempBuildList[i];
		
		RearrangeBuildButtons();
	}
	
	
	//arrange the build button layout based on which button will be active and which will not
	//button that build tower that is not supported by the platform supported will not be active
	void RearrangeBuildButtons(){
		//DisableBuildButtons();
		
		//loop through all the buttons
		for(int i=0; i<buildPanel.buildButtons.Length; i++){
			//a flag to indicate if this button should be active
			bool matched=false;
			
			//integer indicate how many tower has been scan through
			int j=0;
			//loop through all the buildable tower ID list
			foreach(int towerID in currentBuildList){
				//if there's a match, the tower can be build
				if(i==towerID){
					//get the corresponding button and set it to buildbuttontemplate position so we can shift it properly 
					Transform button=buildPanel.buildButtons[towerID].transform;
					button.position=buildPanel.buildButtonTemplate.transform.position;
					
					//set the button position accordingly
					if(buildPanel.buttonOrientation==_Orientation.Right)
						button.localPosition+=new Vector3(j*buildPanel.buildButtonSpacing, 0, 0);
					else if(buildPanel.buttonOrientation==_Orientation.Left)
						button.localPosition+=new Vector3(j*-buildPanel.buildButtonSpacing, 0, 0);
					else if(buildPanel.buttonOrientation==_Orientation.Down)
						button.localPosition+=new Vector3(0, -j*buildPanel.buildButtonSpacing, 0);
					else if(buildPanel.buttonOrientation==_Orientation.Up)
						button.localPosition+=new Vector3(0, j*buildPanel.buildButtonSpacing, 0);
					
					//set the tweening position on the TweenPosition on the component, if any, to current position
					//so the button position wont get shift when mouse over
					TweenPosition tweenCom=(TweenPosition)button.GetComponent(typeof(TweenPosition));
					if(tweenCom!=null){
						tweenCom.to=button.localPosition;
						tweenCom.from=button.localPosition;
					}
					
					//activate the button
					Utility.SetActive(buildPanel.buildButtons[towerID], true);
					
					//set match flag to true, else the button will be deactivated
					matched=true;
					
					//break the loop since we have found a match
					break;
				}
				
				j+=1;
			}
			
			//if no match is found from the buildable list, disable the button
			if(!matched){
				Utility.SetActive(buildPanel.buildButtons[i], false);
			}
		}
	}
	
	
	void InitResourceItem(){
		Resource[] resourceList=GameControl.GetResourceList();
		
		hud.rscList=new ResourceItem[resourceList.Length];
		buildTooltip.rscList=new ResourceItem[resourceList.Length];
		towerPanel.rscListS=new ResourceItem[resourceList.Length];
		towerPanel.rscListU=new ResourceItem[resourceList.Length];
		
		for(int i=0; i<resourceList.Length; i++){
			hud.rscList[i]=CloneResourceItem(hud.resourceItem, resourceList[i]);
			hud.rscList[i].rootT.localPosition+=new Vector3(i*80, 0, 0);
			
			buildTooltip.rscList[i]=CloneResourceItem(buildTooltip.resourceItem, resourceList[i]);
			buildTooltip.rscList[i].rootT.localPosition+=new Vector3(i*51, 0, 0);
			
			towerPanel.rscListS[i]=CloneResourceItem(towerPanel.resourceItemS, resourceList[i]);
			towerPanel.rscListS[i].rootT.localPosition+=new Vector3(0, i*-17, 0);
			
			towerPanel.rscListU[i]=CloneResourceItem(towerPanel.resourceItemU, resourceList[i]);
			towerPanel.rscListU[i].rootT.localPosition+=new Vector3(0, i*-17, 0);
		}
		
		Destroy(hud.resourceItem);
		Destroy(buildTooltip.resourceItem);
		Destroy(towerPanel.resourceItemS);
		Destroy(towerPanel.resourceItemU);
	}
	
	ResourceItem CloneResourceItem(GameObject source, Resource rsc){
		ResourceItem item=new ResourceItem();
		GameObject obj=(GameObject)Instantiate(source);
		item.rootObj=obj;
		item.rootT=item.rootObj.transform;
		item.rootT.parent=source.transform.parent;
		item.rootT.localPosition=source.transform.localPosition;
		item.rootT.localScale=source.transform.localScale;
		item.label=obj.GetComponentInChildren<UILabel>();
		item.icon=obj.GetComponentInChildren<UISprite>();
		
		if(rsc.icon!=null){
			if(item.icon.atlas.GetSprite(rsc.icon.name)!=null){
				item.icon.spriteName=rsc.icon.name;
			}
			else item.icon.spriteName="Highlight";
		}
		
		return item;
	}
	

}


[System.Serializable]
public class ResourceItem{
	public GameObject rootObj;
	public Transform rootT;
	public UILabel label;
	public UISprite icon;
}
