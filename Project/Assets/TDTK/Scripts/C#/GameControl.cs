using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum _GameState{Idle, Started, Ended}
public enum _GameMode{Classic, PointDefend}

[RequireComponent (typeof (ResourceManager))]
[RequireComponent (typeof (LayerManager))]
[RequireComponent (typeof (DamageTable))]

[AddComponentMenu("TDTK/Managers/GameControl")]
public class GameControl : MonoBehaviour {
	
	public delegate void GameOverHandler(bool win); //true if win
	public static event GameOverHandler onGameOverE;
	
	public delegate void ResourceHandler(); 
	public static event ResourceHandler onResourceE;
	
	public delegate void LifeHandler(); 
	public static event LifeHandler onLifeE;

	static public _GameState gameState=_GameState.Idle;
	
	public int playerLifeCap=0;
	public int playerLife=10;
	
	public float sellTowerRefundRatio=0.5f;

	//~ [HideInInspector] public LayerManager layerManager;
	public SpawnManager spawnManager;
	private int totalWaveCount;
	private int currentWave=0;
	
	public Transform rangeIndicatorH;
	public Transform rangeIndicatorConeH;
	public Transform rangeIndicatorF;
	public bool showIndicator=true;
	
	
	static public GameControl gameControl;
	
	public float buildingBarWidthModifier=1f;
	public float buildingBarHeightModifier=1f;
	public Vector3 buildingBarPosOffset=new Vector3(0, -0.5f, 0);
	

	public bool autoCreateOverlayCamera=true;
	
	
	private List<Unit> lifeUnit=new List<Unit>();
	private int lifeUnitCountFull=0;
	
	IEnumerator TrackLifeUnit(){
		while(true){
			for(int i=0; i<lifeUnit.Count; i++){
				if(lifeUnit[i].GetCurrentHP()<=0){
					lifeUnit.RemoveAt(i);
					i-=1;
				}
			}
			yield return new WaitForSeconds(0.1f);
		}
	}
	
	public static List<Unit> GetLifeUnit(){
		return gameControl.lifeUnit;
	}

	void Awake(){
		ObjectPoolManager.Init();
		gameObject.GetComponent<LayerManager>().Awake();
		
		GameMessage gameMessage=(GameMessage)FindObjectOfType(typeof(GameMessage));
		if(gameMessage==null){
			GameMessage.Init();
		}
		
		AudioManager audioManager=(AudioManager)FindObjectOfType(typeof(AudioManager));
		if(audioManager==null){
			AudioManager.Init();
		}
		
		//LayerManager.Init();

		gameControl=this;
		
		gameState=_GameState.Idle;
		
		if(rangeIndicatorH){
			rangeIndicatorH=(Transform)Instantiate(rangeIndicatorH);
			rangeIndicatorH.parent=transform;
		}
		if(rangeIndicatorConeH){
			rangeIndicatorConeH=(Transform)Instantiate(rangeIndicatorConeH);
			rangeIndicatorConeH.parent=transform;
		}
		if(rangeIndicatorF){
			rangeIndicatorF=(Transform)Instantiate(rangeIndicatorF);
			rangeIndicatorF.parent=transform;
		}
		ClearIndicator();
		
		//~ OverlayManager.Init();
		OverlayManager.SetModifier(buildingBarWidthModifier, buildingBarHeightModifier);
		OverlayManager.SetOffset(buildingBarPosOffset);
		
		
		gameObject.GetComponent<ResourceManager>().Init();
		if(FindObjectOfType(typeof(PerkManager))==null) PerkManager.InitResourceModifier();
	}


	// Use this for initialization
	void Start () {
		AudioManager.PlayNewWaveSound ();
		
		if(lifeUnit.Count>0){
			//StartCoroutine(TrackLifeUnit());
			for(int i=0; i<lifeUnit.Count; i++){
				//~ unitLife+=lifeUnit[i].GetCurrentHP();
				//lifeUnit[i].SetLifeUnit();
				lifeUnit[i].tieToPlayerLife=true;
				if(lifeUnit[i]!=null) lifeUnitCountFull+=1;
			}
		}
		
		totalWaveCount=SpawnManager.GetTotalWaveCount();
		
		Camera mainCam=Camera.main;
		LayerMask layerUI=1<<LayerManager.LayerMiscUIOverlay();
		mainCam.cullingMask=mainCam.cullingMask&~layerUI;
		
		//Create OverlayCamera
		if(autoCreateOverlayCamera){
			
			Transform mainCamT=mainCam.transform;
			
			GameObject overlayCamObj=new GameObject();
			overlayCamObj.name="OverlayCamera";
			
			LayerMask layer=1<<LayerManager.LayerOverlay();
			
			mainCam.cullingMask=mainCam.cullingMask&~layer;
			
			Camera overlayCam=overlayCamObj.AddComponent<Camera>();
			
			overlayCam.orthographic=mainCam.orthographic;
			overlayCam.orthographicSize=mainCam.orthographicSize;

			overlayCam.clearFlags=CameraClearFlags.Depth;
			overlayCam.depth=mainCam.depth + 1;
			overlayCam.cullingMask=layer;
			overlayCam.fieldOfView=mainCam.fieldOfView;
			
			overlayCamObj.transform.parent=mainCamT;
			overlayCamObj.transform.rotation=mainCamT.rotation;
			overlayCamObj.transform.localPosition=Vector3.zero;
		}
		
		Time.timeScale=1;
	}
	
	void OnEnable(){
		SpawnManager.onWaveStartSpawnE += OnWaveStartSpawned;
		SpawnManager.onWaveClearedE += OnWaveCleared;
		
		Unit.onLifeUnitAttackedE += OnDeductUnitLife;
		
		UnitCreep.onScoreDeductLifeE += OnDeductLife;
		UnitTower.onDestroyE += OnTowerDestroy;
		UnitTower.onDragNDropE += onTowerDragNDropEnd;
	}
	
	void OnDisable(){
		SpawnManager.onWaveStartSpawnE -= OnWaveStartSpawned;
		SpawnManager.onWaveClearedE -= OnWaveCleared;
		
		Unit.onLifeUnitAttackedE += OnDeductUnitLife;
		
		UnitCreep.onScoreDeductLifeE -= OnDeductLife;
		UnitTower.onDestroyE -= OnTowerDestroy;
		UnitTower.onDragNDropE -= onTowerDragNDropEnd;
	}
	
	public static void OnIncreaseLifeCap(int val){
		if(gameControl.playerLifeCap>0) gameControl.playerLifeCap+=val;
		gameControl.playerLife+=val;
		if(onLifeE!=null) onLifeE();
	}
	
	public static void OnGainLife(int val){
		if(gameControl.playerLifeCap>0){
			gameControl.playerLife=Mathf.Min(gameControl.playerLife+=val, gameControl.playerLifeCap);
		}
		else gameControl.playerLife+=val;
		if(onLifeE!=null) onLifeE();
	}
	
	void OnDeductUnitLife(Unit unit){
		for(int i=0; i<lifeUnit.Count; i++){
			if(unit.GetCurrentHP()<=0 && unit==lifeUnit[i]){
				lifeUnit.RemoveAt(i);
				break;
			}
		}
		
		if(onLifeE!=null) onLifeE();
		
		if(lifeUnit.Count==0){
			//game over, player lost
			gameState=_GameState.Ended;
			if(onGameOverE!=null) onGameOverE(false);
		}
	}
	
	void OnDeductLife(int lifeCost){
		playerLife-=lifeCost;
		if(playerLife<=0) playerLife=0;
		
		if(onLifeE!=null) onLifeE();
		
		if(playerLife==0){
			//game over, player lost
			gameState=_GameState.Ended;
			if(onGameOverE!=null) onGameOverE(false);
		}
	}
	
	void OnTowerDestroy(UnitTower tower){
		#if UNITY_4_0
			if(selectedTower==tower || selectedTower==null || !selectedTower.thisObj.activeInHierarchy){
		#else
			if(selectedTower==tower || selectedTower==null || !selectedTower.thisObj.active){
		#endif
		//if(selectedTower==tower || selectedTower==null || !selectedTower.thisObj.active){
			ClearSelection();
		}
	}
	
	public static int GetFullLifeUnitCount(){
		return gameControl.lifeUnitCountFull;
	}
	
	public static int GetLifeUnitCount(){
		return gameControl.lifeUnit.Count;
	}
	
	public static float GetLifeUnitHPFull(){
		float life=0;
		foreach(Unit unit in gameControl.lifeUnit){
			life+=unit.GetFullHP();
		}
		return life;
	}
	
	public static float GetLifeUnitHP(){
		float life=0;
		foreach(Unit unit in gameControl.lifeUnit){
			life+=unit.GetCurrentHP();
		}
		return life;
	}
	
	public static int GetPlayerLife(){
		return gameControl.playerLife;
	}
	
	public static int GetPlayerLifeCap(){
		return gameControl.playerLifeCap;
	}
	
	// Update is called once per frame
	void Update () {
		//~ if(Input.GetKeyDown(KeyCode.Space)){
			//~ GameMessage.DisplayMessage(Random.Range(0, 999999999)+" "+Random.Range(0, 999999999)+" "+Random.Range(0, 999999999));
		//~ }
	}
	
	
	void OnWaveStartSpawned(int waveID){
		currentWave+=1;
		
		//if game is not yet started, start it now
		if(gameState==_GameState.Idle) gameState=_GameState.Started;
	}
	
	void WaveSpawned(int waveID){
		
	}
	
	private int waveClearedCount=0;
	void OnWaveCleared(int waveID){
		//Debug.Log("Wave "+waveID+" has been cleared");
		waveClearedCount+=1;
		if(waveClearedCount==totalWaveCount){
			//game over, player won
			gameState=_GameState.Ended;
			if(onGameOverE!=null) onGameOverE(true);
		}
	}
	
	
	
	static public UnitTower selectedTower;
	
	static public UnitTower Select(Vector3 pointer){
		//change this
		int layer=LayerManager.LayerTower();
		
		LayerMask mask=1<<layer;
		Ray ray = Camera.main.ScreenPointToRay(pointer);
		RaycastHit hit;
		if(!Physics.Raycast(ray, out hit, Mathf.Infinity, mask)){
			return null;
		}
		
		selectedTower=hit.transform.gameObject.GetComponent<UnitTower>();
		//selectedTower.Select();
		
		gameControl._ShowIndicator(selectedTower);
		
		return selectedTower;
	}
	
	public static void ShowIndicator(UnitTower tower){
		gameControl._ShowIndicator(tower);
	}
	
	public void _ShowIndicator(UnitTower tower){
		if(!showIndicator) return;
		
		if(tower.type==_TowerType.Block) return;
		
		//show range indicator on the tower
		//for support tower, show friendly range indicator
		if(tower.type==_TowerType.SupportTower){
			//Debug.Log(tower.type);
			float range=tower.GetRange();
			if(rangeIndicatorF!=null){
				rangeIndicatorF.position=tower.thisT.position;
				rangeIndicatorF.localScale=new Vector3(2*range/10, 1, 2*range/10);
				rangeIndicatorF.renderer.enabled=true;
			}
			if(rangeIndicatorH!=null) rangeIndicatorH.renderer.enabled=false;
			if(rangeIndicatorConeH!=null) rangeIndicatorConeH.renderer.enabled=false;
		}
		//for support tower, show hostile range indicator
		else if(tower.type!=_TowerType.ResourceTower){
			float range=tower.GetRange();
			if(tower.targetingArea==_TargetingArea.AllAround){
				if(rangeIndicatorH!=null){
					rangeIndicatorH.position=tower.thisT.position;
					rangeIndicatorH.localScale=new Vector3(2*range/10, 1, 2*range/10);
					rangeIndicatorH.renderer.enabled=true;
				}
				if(rangeIndicatorConeH!=null) rangeIndicatorConeH.renderer.enabled=false;
			}
			else{
				if(rangeIndicatorH!=null){
					rangeIndicatorConeH.position=tower.thisT.position;
					rangeIndicatorConeH.localScale=new Vector3(2*range/10, 1, 2*range/10);
					rangeIndicatorConeH.rotation=Quaternion.Euler(0, (360-tower.targetingDirection)-90, 0);
					rangeIndicatorConeH.renderer.enabled=true;
				}
				if(rangeIndicatorH!=null) rangeIndicatorH.renderer.enabled=false;
			}
			if(rangeIndicatorF!=null) rangeIndicatorF.renderer.enabled=false;
		}
	}
	
	private bool dragNDrop=false;
	public static void DragNDropIndicator(UnitTower tower){
		if(tower.type!=_TowerType.ResourceTower){
			gameControl._ShowIndicator(tower);
			gameControl.StartCoroutine(gameControl._DragNDropIndicator(tower));
		}
	}
	IEnumerator _DragNDropIndicator(UnitTower tower){
		if(tower.type==_TowerType.Block){
			ClearIndicator();
			yield break;
		}
		
		dragNDrop=true;
		
		while(dragNDrop){
			if(tower.type==_TowerType.SupportTower){
				if(rangeIndicatorF!=null) rangeIndicatorF.position=tower.thisT.position;
			}
			else{
				if(tower.targetingArea==_TargetingArea.AllAround){
					if(rangeIndicatorH!=null) rangeIndicatorH.position=tower.thisT.position;
				}
				else{
					if(rangeIndicatorConeH!=null) rangeIndicatorConeH.position=tower.thisT.position;
				}
			}
				
			yield return null;
		}
		ClearIndicator();
	}
	
	void onTowerDragNDropEnd(string msg){
		dragNDrop=false;
	}
	
	public static void ClearIndicator(){
		gameControl._ClearIndicator();
	}
	
	public void _ClearIndicator(){
		if(rangeIndicatorConeH!=null) rangeIndicatorConeH.renderer.enabled=false;
		if(rangeIndicatorH!=null) rangeIndicatorH.renderer.enabled=false;
		if(rangeIndicatorF!=null) rangeIndicatorF.renderer.enabled=false;
	}
	
	static public void ClearSelection(){
		selectedTower=null;
		gameControl._ClearIndicator();
	}
	
	//call when a tower complete upgrade, if tower is currently selected, update the range indicator
	static public void TowerUpgradeComplete(UnitTower tower){
		if(tower==selectedTower){
			gameControl._ShowIndicator(tower);
		}
	}
	
	
	static public int GetResourceCount(){
		return ResourceManager.GetResourceCount();
	}
	
	static public void GainResource(int val){
		ResourceManager.GainResource(0, val);
		if(onResourceE!=null) onResourceE();
	}
	
	static public void GainResource(int id, int val){
		ResourceManager.GainResource(id, val);
		if(onResourceE!=null) onResourceE();
	}
	
	static public void GainResource(int[] val){
		ResourceManager.GainResource(val);
		if(onResourceE!=null) onResourceE();
	}
	
	static public void SpendResource(int[] val){
		ResourceManager.SpendResource(val);
		if(onResourceE!=null) onResourceE();
	}
	
	static public int GetResourceVal(){
		return ResourceManager.GetResourceVal(0);
	}
	
	static public int GetResourceVal(int id){
		return ResourceManager.GetResourceVal(id);
	}
	
	static public int[] GetAllResourceVal(){
		Resource[] rscList=ResourceManager.GetResourceList();
		
		int[] valList=new int[rscList.Length];
		for(int i=0; i<valList.Length; i++){
			valList[i]=rscList[i].value;
		}
		
		return valList;
	}
	
	static public bool HaveSufficientResource(int[] cost){
		return ResourceManager.HaveSufficientResource(cost);
	}
	
	static public Resource[] GetResourceList(){
		return ResourceManager.GetResourceList();
	}
	
	static public float GetSellTowerRefundRatio(){
		return gameControl.sellTowerRefundRatio;
	}
	
	static public void GradualPause(){
		
	}
	
	static public void GradualResume(){
		
	}
}