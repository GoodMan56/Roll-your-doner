using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum _TowerType{TurretTower, AOETower, DirectionalAOETower, SupportTower, ResourceTower, Mine, Block};
public enum _TargetMode{Hybrid, Air, Ground};
public enum _TargetingArea{AllAround, DirectionalCone, StraightLine};
public enum _TargetPriority{Nearest, Weakest, Toughest, First, Random};
public enum _TurretAni{Full, YAxis, None}
public enum _RotationMode{FullTurret, SeparatedBarrel}
public enum _LOSMode{AimOnly, Realistic}
	
[AddComponentMenu("TDTK/InGameObject/Tower")]
public class UnitTower : Unit {
	
	public Animation turretBuildAnimationBody;
	public AnimationClip turretBuildAnimation;
	public Animation baseBuildAnimationBody;
	public AnimationClip baseBuildAnimation;
	
	public Animation turretFireAnimationBody;
	public AnimationClip turretFireAnimation;
	public Animation baseFireAnimationBody;
	public AnimationClip baseFireAnimation;
	
	public delegate void DragNDropHandler(string result);
	public static event DragNDropHandler onDragNDropE;
	
	public delegate void BuildCompleteHandler(UnitTower tower);
	public static event BuildCompleteHandler onBuildCompleteE;
	
	public delegate void DestroyHandler(UnitTower tower);
	public static event DestroyHandler onDestroyE;
	
	public delegate void ResourceGainHandler(GainResourcePos grp);
	public static event ResourceGainHandler onGainResourceE;
	
	public int uniquePerkID=-1;
	
	//public int prefabID=0;
	public _TowerType type=_TowerType.TurretTower;

	public _TargetMode targetMode=_TargetMode.Hybrid;
	public _TargetPriority targetPriority=_TargetPriority.Nearest;
	public _TargetingArea targetingArea=_TargetingArea.AllAround;
	private int level=0;
	public int levelCap=0;
	public string description="This blocks of text here should give a brief description of this tower.";
	public TowerStat baseStat=new TowerStat();
	public TowerStat[] upgradeStat=new TowerStat[1];
	
	private int[] towerValue=new int[1];
	
	//~ public float GetDamage(){ return damage*(attBuffMod)+attBuffVal; }
	public float GetDamage(){ return damage*(attBuffMod+PerkManager.allTowerAttackBuffModifier)+(attBuffVal+PerkManager.allTowerAttackBuffValue); }
	public float GetRange(){ return effRange*(rangeBuffMod)+rangeBuffVal; }
	public float GetMinRange(){ return minRange; }
	public float GetCooldown(){ return cooldown; }
	public float GetClipSize(){ return clipSize; }
	public float GetReloadDuration(){ return reloadDuration; }
	public float GetCurrentClip(){ return currentClip; }
	public float GetLastReloadTime(){ return lastReloadTime; }
	public float GetAoeRadius(){ return aoeRadius; }
	//~ public float GetStunDuration(){ return stunDuration; }
	public Stun GetStun(){ return stun; }
	public Critical GetCritical(){ return crit; }
	public Dot GetDot(){ return dot; }
	public Slow GetSlow(){ return slow; }
	public BuffStat GetBuff(){ return buff; }
	public int[] GetIncomes(){ return incomes; }
	public bool GetMineOneOff() { return mineOneOff; }
	
	private GameObject shootObject;
	
	private float damage=2;
	private float range=8;
	private float effRange=8;
	private float minRange=0;
	private float cooldown=1;
	private int clipSize=5;
	private float reloadDuration=4;
	private int currentClip=1;
	private float lastReloadTime;
	private float aoeRadius=1;
	//~ private float stunDuration=1;
	private Stun stun;
	private Critical crit;
	private Dot dot;
	private Slow slow;
	[HideInInspector] public bool mineOneOff=true;
	public float aoeConeAngle=10;
	
	private BuffStat buff;
	private int[] incomes=new int[1];
	
	private Transform turretObject;
	private Transform baseObject;
	private Transform barrelObject;
	
	public _TurretAni animateTurret;
	public _LOSMode losMode=_LOSMode.AimOnly;
	private float turretMaxAngle=0;
	private float turretMaxRange=0;
	private bool targetInLOS=true; 	//in flag indicating if turret is facing the target
													// used to determine if a shot can be fire
	
	
	//to identity each tower built in a game
	private int towerID=-1;
	public void SetTowerID(int ID){ towerID=ID; }
	public int GetTowerID(){ return towerID; }
	
	private int footprint=0;
	public int GetFootprint(){ return footprint; }
	
	//for stat editor purpose;
	//[SerializeField] private int towerID=-1; 
	//public void SetTowerID(int id){ towerID=id; }
	//public int GetTowerID(){ return towerID; }

	private Unit target;
	private LayerMask maskTarget;
	private float currentTargetDist=0;
	public LayerMask GetTargetMask(){
		return maskTarget;
	}
	public void AssignTarget(Unit tgt){
		if(Vector3.Distance(tgt.thisT.position, thisT.position)<effRange){
			target=tgt;
		}
	}
	public void CheckForTarget(Vector3 pos){
		Ray ray = Camera.main.ScreenPointToRay(pos);
		RaycastHit hit;
		//LayerMask mask=currentSelectedTower.GetTargetMask();
		if(Physics.Raycast(ray, out hit, Mathf.Infinity, maskTarget)){
			Unit unit=hit.collider.gameObject.GetComponent<Unit>();
			if(unit!=null){
				AssignTarget(unit);
			}
		}
	}
	
	private bool built=false;
	private float currentBuildDuration=0;
	private float remainingBuildDuration=0;

	private int experience=0;

	public GameObject buildingEffect;
	public GameObject buildingDoneEffect;
	
	public AudioClip shootSound;
	//public AudioClip reloadSound;
	public AudioClip buildingSound;
	public AudioClip builtSound;
	public AudioClip soldSound;
	
	[HideInInspector] public Transform[] shootPoint;
	
	
	
	public override void Awake(){
		base.Awake();
		
		if(thisObj.collider==null){
			SphereCollider col=thisObj.AddComponent<SphereCollider>();
			col.center=new Vector3(0, 0.5f, 0);
			
			//scale the collider radius to match the gridsize
			float scale=thisT.localScale.x;
			if(scale>=1)	col.radius=0.5f*BuildManager.GetGridSize()/scale;
			if(scale<1)	col.radius=0.5f*BuildManager.GetGridSize();
		}
		
		SetSubClassInt(this);
		
		
		//assign base stat
		InitStat();
		
		
		InitPerkParameters();
	}
	
	// Use this for initialization
	public override void Start () {
		base.Start();
		
		thisObj.layer=LayerManager.LayerTower();
		
		if(Time.timeSinceLevelLoad<0.25f){
			baseStat.buildDuration=0;
			InitTower(BuildManager.PrePlaceTower(this), false);
		}
		
		//~ InitPerkParameters();
	}
	
	void OnEnable(){
		PerkManager.onBuffAllTowerHPE += OnPerkBuffTowerHP;
		
		PerkManager.onUpgradeCostReducE += OnPerkUpgradeCostReduc;
		PerkManager.onBuildCostReducE += OnPerkBuildCostReduc;
		PerkManager.onBuffTowerHPE += OnPerkBuffTowerHP;
		PerkManager.onBuffTowerDefE += OnPerkBuffTowerDef;
		PerkManager.onBuffTowerAttE += OnPerkBuffTowerAtt;
		PerkManager.onBuffTowerRangeE += OnPerkBuffTowerRange;
		PerkManager.onBuffTowerCritE += OnPerkBuffTowerCrit;
		PerkManager.onBuffTowerCritChanceE += OnPerkBuffTowerCritChance;
		PerkManager.onBuffTowerCritDamageE += OnPerkBuffTowerCritDamage;
		PerkManager.onStunE += OnPerkStun;
		PerkManager.onSlowE += OnPerkSlow;
	}
	
	void OnDisable(){
		PerkManager.onBuffAllTowerHPE -= OnPerkBuffTowerHP;
		
		PerkManager.onUpgradeCostReducE -= OnPerkUpgradeCostReduc;
		PerkManager.onBuildCostReducE -= OnPerkBuildCostReduc;
		PerkManager.onBuffAllTowerHPE -= OnPerkBuffTowerHP;
		PerkManager.onBuffTowerDefE -= OnPerkBuffTowerDef;
		PerkManager.onBuffTowerAttE -= OnPerkBuffTowerAtt;
		PerkManager.onBuffTowerRangeE -= OnPerkBuffTowerRange;
		PerkManager.onBuffTowerCritE -= OnPerkBuffTowerCrit;
		PerkManager.onBuffTowerCritChanceE -= OnPerkBuffTowerCritChance;
		PerkManager.onBuffTowerCritDamageE -= OnPerkBuffTowerCritDamage;
		PerkManager.onStunE -= OnPerkStun;
		PerkManager.onSlowE -= OnPerkSlow;
	}
	
	
	public void InitPerkParameters(){
		//Debug.Log("init perk parameter");
		attBuffMod+=PerkManager.GetTowerBuffAttMod(prefabID);
		attBuffVal+=PerkManager.GetTowerBuffAttVal(prefabID);
		
		if(crit!=null){
			crit.AddPerkChance(PerkManager.GetTowerCritChance(prefabID));
			crit.AddPerkDmgMod(PerkManager.GetTowerCritDamageMod(prefabID));
			crit.AddPerkDmgVal(PerkManager.GetTowerCritDamageVal(prefabID));
		}
		
		rangeBuffMod+=PerkManager.GetTowerRangeExMod(prefabID);
		rangeBuffVal+=PerkManager.GetTowerRangeExVal(prefabID);
		
		int rscCount=GameControl.GetResourceCount();
		buCostReducMod=new float[rscCount];
		buCostReducVal=new int[rscCount];
		
		upCostReducMod=new float[rscCount];
		upCostReducVal=new int[rscCount];
		
		float[] tbcmList=PerkManager.GetTowerBuildCostReducMod(prefabID);
		for(int i=0; i<tbcmList.Length; i++){
			buCostReducMod[i]+=tbcmList[i];
		}
		float[] tbcvList=PerkManager.GetTowerBuildCostReducVal(prefabID);
		for(int i=0; i<tbcvList.Length; i++){
			buCostReducVal[i]+=(int)tbcvList[i];
		}
		
		float[] tucmList=PerkManager.GetTowerUpgradeCostReducMod(prefabID);
		for(int i=0; i<tucmList.Length; i++){
			upCostReducMod[i]+=tucmList[i];
		}
		float[] tucvList=PerkManager.GetTowerUpgradeCostReducVal(prefabID);
		for(int i=0; i<tucvList.Length; i++){
			upCostReducVal[i]+=(int)tucvList[i];
		}
		
		float[] perkStun=PerkManager.GetTowerStun(prefabID);
		if(perkStun.Length==2){
			perkStunChance=perkStun[0];
			perkStunDuration=perkStun[1];
			
			stun.chance+=perkStun[0];
			stun.duration+=perkStun[1];
		}
		
		float[] perkSlow=PerkManager.GetTowerSlow(prefabID);
		if(perkSlow.Length==2){
			perkSlowDuration=perkSlow[0];
			perkSlowFactor=perkSlow[1];
			
			slow.duration+=perkSlow[0];
			slow.slowFactor+=perkSlow[1];
		}
	}
	
	
	private float[] buCostReducMod=new float[0];
	private int[] buCostReducVal=new int[0];
	
	private float[] upCostReducMod=new float[0];
	private int[] upCostReducVal=new int[0];
	
	private float attBuffMod=1;
	private float attBuffVal=0;
	
	private float rangeBuffMod=1;
	private float rangeBuffVal=0;
	
	private float perkStunChance=0;
	private float perkStunDuration=0;
	
	private float perkSlowDuration=0;
	private float perkSlowFactor=0;
	
	
	//general stat update from perk
	public void OnPerkBuffTowerHP(Perk perk){
		bool flag1=perk.type==_PerkType.TowerBuffHP;
		bool flag2=(perk.type==_PerkType.SpecificTowerBuffHP && perk.towerID==prefabID);
		if(flag1 || flag2){
			if(perk.modTypeVal==_ModifierType.percentage){
				float dif=HPAttribute.fullHPDefault*(perk.value[0]);
				HPAttribute.fullHP+=dif;
				HPAttribute.HP+=dif;
			}
			else if(perk.modTypeVal==_ModifierType.value){
				HPAttribute.fullHP+=perk.value[0];
				HPAttribute.HP+=perk.value[0];
			}
		}
	}
	public void OnPerkBuffTowerAtt(Perk perk){
		if(perk.towerID==prefabID){
			if(perk.modTypeVal==_ModifierType.percentage) attBuffMod+=perk.value[0];
			else if(perk.modTypeVal==_ModifierType.value) attBuffVal+=perk.value[0];
		}
	}
	public void OnPerkBuffTowerDef(Perk perk){
		if(perk.towerID==prefabID){
			if(perk.modTypeVal==_ModifierType.percentage) dmgReducMod+=perk.value[0];
			else if(perk.modTypeVal==_ModifierType.value) dmgReducVal+=perk.value[0];
		}
	}
	public void OnPerkBuffTowerRange(Perk perk){
		if(perk.towerID==prefabID){
			if(perk.modTypeVal==_ModifierType.percentage) rangeBuffMod+=perk.value[0];
			else if(perk.modTypeVal==_ModifierType.value) rangeBuffVal+=perk.value[0];
		}
	}
	
	//critical stat update from perk
	public void OnPerkBuffTowerCrit(Perk perk){
		if(perk.towerID==prefabID){
			crit.AddPerkChance(perk.value[0]);
		}
	}
	public void OnPerkBuffTowerCritChance(Perk perk){
		if(perk.towerID==prefabID){
			crit.AddPerkChance(perk.value[0]);
		}
	}
	public void OnPerkBuffTowerCritDamage(Perk perk){
		if(perk.towerID==prefabID){
			if(perk.modTypeVal==_ModifierType.percentage) crit.AddPerkDmgMod(perk.value[0]);
			else if(perk.modTypeVal==_ModifierType.value) crit.AddPerkDmgVal(perk.value[0]);
		}
	}
	
	//cost reduc stat update from perk
	public void OnPerkBuildCostReduc(Perk perk){
		if(perk.towerID==prefabID){
			if(perk.modTypeVal==_ModifierType.percentage){
				for(int i=0; i<buCostReducMod.Length; i++) buCostReducMod[i]+=perk.rsc[i];
			}
			else if(perk.modTypeVal==_ModifierType.value){
				for(int i=0; i<buCostReducVal.Length; i++) buCostReducVal[i]+=(int)perk.rsc[i];
			}
		}
	}
	public void OnPerkUpgradeCostReduc(Perk perk){
		if(perk.towerID==prefabID){
			if(perk.modTypeVal==_ModifierType.percentage){
				for(int i=0; i<upCostReducMod.Length; i++) upCostReducMod[i]+=perk.rsc[i];
			}
			else if(perk.modTypeVal==_ModifierType.value){
				for(int i=0; i<upCostReducVal.Length; i++) upCostReducVal[i]+=(int)perk.rsc[i];
			}
		}
	}
	
	//special ability from perk
	public void OnPerkStun(Perk perk){
		if(perk.towerID==prefabID){
			perkStunChance+=perk.value[0];
			perkStunDuration+=perk.value[1];
			
			stun.chance+=perk.value[0];
			stun.duration+=perk.value[1];
		}
	}
	public void OnPerkSlow(Perk perk){
		if(perk.towerID==prefabID){
			perkSlowDuration+=perk.value[0];
			perkSlowFactor+=perk.value[1];
			
			slow.duration+=perk.value[0];
			slow.slowFactor+=perk.value[1];
		}
	}
	
	
	//called when this tower is confirmed built, this will make the tower operational
	//called immediately upon built if the buildmode is point and build
	public void InitTower(int ID){
		InitTower(ID, true);
	}
	public void InitTower(int ID, bool gameStarted){
		towerID=ID;
		//Debug.Log("init tower... towerID: "+ID);
		
		if(targetMode==_TargetMode.Hybrid){
			LayerMask mask1=1<<LayerManager.LayerCreep();
			LayerMask mask2=1<<LayerManager.LayerCreepF();
			
			maskTarget=mask1 | mask2;
		}
		else if(targetMode==_TargetMode.Air){
			maskTarget=1<<LayerManager.LayerCreepF();
		}
		else if(targetMode==_TargetMode.Ground){
			maskTarget=1<<LayerManager.LayerCreep();
		}
		
		if(shootObject!=null){
			ObjectPoolManager.New(shootObject, 2);
		}
		
		if(buildingEffect!=null) ObjectPoolManager.New(buildingEffect, 2, false);
		if(buildingDoneEffect!=null) ObjectPoolManager.New(buildingDoneEffect, 2, false);
		
		foreach(TowerStat stat in upgradeStat){
			if(stat.shootObject!=null){
				ObjectPoolManager.New(stat.shootObject, 2);
			}
		}
		
		if(type==_TowerType.TurretTower){
			//calculate turret offset if this tower uses a projectile with elevated shoot angle
			ShootObject shootObj=shootObject.GetComponent<ShootObject>();
			if(shootObj.type==_ShootObjectType.Projectile){
				turretMaxAngle=shootObj.maxShootAngle;
				turretMaxRange=shootObj.maxShootRange;
			}
			
			if(targetingArea==_TargetingArea.AllAround) StartCoroutine(ScanForTargetAllAround());
			else if(targetingArea==_TargetingArea.DirectionalCone) StartCoroutine(ScanForTargetDirectional());
			else if(targetingArea==_TargetingArea.StraightLine) StartCoroutine(ScanForTargetStraightLine());
			StartCoroutine(TurretRoutine());
		}
		else if(type==_TowerType.DirectionalAOETower){
			if(targetingArea==_TargetingArea.AllAround) StartCoroutine(ScanForTargetAllAround());
			else if(targetingArea==_TargetingArea.DirectionalCone) StartCoroutine(ScanForTargetDirectional());
			else if(targetingArea==_TargetingArea.StraightLine) StartCoroutine(ScanForTargetStraightLine());
			StartCoroutine(DirectionalAOERoutine());
		}
		else if(type==_TowerType.AOETower){
			StartCoroutine(AOERoutine());
		}
		else if(type==_TowerType.SupportTower){
			StartCoroutine(SupportRoutine());
		}
		else if(type==_TowerType.ResourceTower){
			StartCoroutine(ResourceRoutine());
		}
		else if(type==_TowerType.Mine){
			StartCoroutine(MineRoutine());
		}
		
		
		level=1+PerkManager.allTowerLevelBonus+PerkManager.GetTowerLevelBonus(prefabID);
		//Debug.Log(level+"   "+lvlModifier+"   "+PerkManager.GetLevelBonus(prefabID));
		if(level==1) StartCoroutine(Building(baseStat.buildDuration, false, gameStarted));
		else{
			level-=1;
			StartCoroutine(Building(baseStat.buildDuration, true, gameStarted));
		}
		
		//if turret is not animating, then enable turret shoot under all circumstance
		//else turret can only shoot when facing target
		if(animateTurret==_TurretAni.None) targetInLOS=true;
		
		if(turretBuildAnimationBody!=null && turretBuildAnimation!=null){
			turretBuildAnimationBody.AddClip(turretBuildAnimation, turretBuildAnimation.name);
			turretBuildAnimationBody.Play(turretBuildAnimation.name);
		}
		if(baseBuildAnimationBody!=null && baseBuildAnimation!=null){
			baseBuildAnimationBody.AddClip(baseBuildAnimation, baseBuildAnimation.name);
			baseBuildAnimationBody.Play(baseBuildAnimation.name);
		}
	}
	

	//private MatShaderList matShaderList;
	//called immediately upon creation if the buildmode is DragNDrop
	public IEnumerator DragNDropRoutine(bool colorIndicator){
		
		HPAttribute.ClearParent();
		
		//set to additive and red color by default
		//~ UnitUtility.SetMat2AdditiveRecursively(thisT);
		//matShaderList=UnitUtility.SetMat2AdditiveRecursively(thisT);
		if(colorIndicator) UnitUtility.SetAdditiveMatColorRecursively(thisT, Color.red);
		
		//delay a frame, make sure awake is executed
		yield return null;
		
		//disable the collider so it wont get in the way
		//thisObj.collider.enabled=false;
		
		//show range indicator
		GameControl.DragNDropIndicator(this);
		
		//sure to check if the current visual state, red/green
		bool buildEnable=false;
		//a reference position use to check if the mouse has move on to a new build position
		Vector3 lastPos=Vector3.zero;
		
		while(true){
			bool flag=BuildManager.CheckBuildPoint(Input.mousePosition, footprint, prefabID);
			BuildableInfo currentBuildInfo=BuildManager.GetBuildInfo();
			
			//change the visual of the tower appropriately according to buildablity of the position
			
			if(currentBuildInfo!=null){
				//only execute if only the currentBuidPos has been updated
				//so the object remain "green" when the cursor move to somewhere without collider
				if(currentBuildInfo.position!=lastPos){
					lastPos=currentBuildInfo.position;
					
					//change color/state if need be
					if(flag && !buildEnable){
						//Debug.Log("update true");
						buildEnable=true;
						if(colorIndicator){
							UnitUtility.SetAdditiveMatColorRecursively(thisT, Color.green);
						}
					}
					else if(!flag && buildEnable){
						//Debug.Log("update false");
						buildEnable=false;
						if(colorIndicator) UnitUtility.SetAdditiveMatColorRecursively(thisT, Color.red);
					}
				}
			}
			
			//for dragNdrop without grid snapping when move between platforms
			//if(!flag){
			//	currentBuildInfo=null;
			//	BuildManager.ClearBuildPoint();
			//}
			
			//position the tower
			//this is just in case when the tower first spawn and the mouse is not pointing at a platform
			if(currentBuildInfo==null){
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if(Physics.Raycast(ray, out hit, Mathf.Infinity)) thisT.position=hit.point;
				//this there is no collier, randomly place it 30unit from camera
				else thisT.position=ray.GetPoint(30);
			}
			else{
				thisT.position=currentBuildInfo.position;
				//Debug.Log(currentBuildInfo.platform+" !!!     "+currentBuildInfo.platform.thisT+" !!!     ");
				thisT.rotation=currentBuildInfo.platform.thisT.rotation;
			}
			
			
			//left-click, build
			if(Input.GetMouseButtonDown(0)){
				//if current mouse point position is valid, build the tower
				if(flag) DragNDropBuilt();
				else{
					//GameMessage.DisplayMessage("Invalid Build Position");
					DragNDropCancel("Invalid BuildPoint");
				}
				break;
			}
			
			//right-click, cancel
			if(Input.GetMouseButtonDown(1)){
				DragNDropCancel();
			}
			
			yield return null;
		}
	}
	
	void DragNDropCancel(string msg="building cancelled"){
		GameControl.ClearIndicator();
		BuildManager.ClearBuildPoint();
		HPAttribute.RestoreParent();
		//~ Destroy(thisObj);
		
		if(onDragNDropE!=null) onDragNDropE(msg);
		
		//~ #if UNITY_4_0
			//~ thisObj.SetActive(false);
		//~ #else
			//~ thisObj.SetActiveRecursively(false);
		//~ #endif
		Utility.SetActive(thisObj, false);
	}
	
	//called when DragNDrop build is commenced
	void DragNDropBuilt(){
		/*
		//first check for resources
		//~ int[] cost=GetCost();
		//~ if(GameControl.HaveSufficientResource(cost)){
			//~ GameControl.SpendResource(cost);
			
			//~ //revert material to default stat and re-enable the collider
			//UnitUtility.SetMat2DiffuseRecursively(thisT);
			//~ UnitUtility.ResetMatRecursively(thisT, matShaderList);
			//~ thisObj.collider.enabled=true;
			
			//~ HPAttribute.RestoreParent();
			
			//~ //inform BuildManager, this is to setup platform block, pathfinding and so on
			//~ BuildManager.DragNDropBuilt(this);
			
			//~ //put tower into operation
			//~ InitTower(towerID);
		//~ }
		//~ else{
			//~ GameMessage.DisplayMessage("Insufficient Resource");
			//~ DragNDropCancel();
		//~ }
		*/
		
		string result="";
		result=BuildManager.DragNDropBuilt(this);
		
		if(onDragNDropE!=null) onDragNDropE(result);
		
		//~ #if UNITY_4_0
			//~ thisObj.SetActive(false);
		//~ #else
			//~ thisObj.SetActiveRecursively(false);
		//~ #endif
		Utility.SetActive(thisObj, false);
	}
	
	
	public float GetCurrentBuildDuration(){
		//BuildManager.ClearBuildPoint();
		return currentBuildDuration;
	}
	
	public float GetRemainingBuildDuration(){
		return remainingBuildDuration;
	}
	
	
	public float targetingDirection;
	public float targetingFOV=30;
	public bool matchTowerDir2TargetDir=false;
	protected Vector3 scanDirection;
	protected Quaternion scanQuaternion;
	
	public bool SetTargetingArea(int tgtArea){
		if(type==_TowerType.TurretTower || type==_TowerType.DirectionalAOETower){
			if(tgtArea==0) return SetTargetingArea(_TargetingArea.AllAround);
			else if(tgtArea==1) return SetTargetingArea(_TargetingArea.DirectionalCone);
			else if(tgtArea==2) return SetTargetingArea(_TargetingArea.StraightLine);
			else return false;
		}
		else return false;
	}
	
	public bool SetTargetingArea(_TargetingArea tgtArea){
		if(type==_TowerType.TurretTower || type==_TowerType.DirectionalAOETower){
			if(targetingArea!=tgtArea){
				targetingArea=tgtArea;
				if(targetingArea==_TargetingArea.AllAround) StartCoroutine(ScanForTargetAllAround());
				else if(targetingArea==_TargetingArea.DirectionalCone) StartCoroutine(ScanForTargetDirectional());
				else if(targetingArea==_TargetingArea.StraightLine) StartCoroutine(ScanForTargetStraightLine());
				return true;
			}
			else return false;
		}
		else return false;
	}
	
	public void SetTargetingDirection(float angle){
		targetingDirection=angle;
		scanDirection.x=Mathf.Cos(targetingDirection*Mathf.Deg2Rad);
		scanDirection.z=Mathf.Sin(targetingDirection*Mathf.Deg2Rad);
		scanQuaternion=Quaternion.LookRotation((thisT.position+scanDirection)-thisT.position);
		
		if(matchTowerDir2TargetDir){
			thisT.rotation=Quaternion.Euler(0, (360-angle)-90, 0);
		}
		
		GameControl.ShowIndicator(this);
	}
	
	public bool SetTargetPriority(int priority){
		if(priority==0) return SetTargetPriority(_TargetPriority.Nearest);
		else if(priority==1) return SetTargetPriority(_TargetPriority.Weakest);
		else if(priority==2) return SetTargetPriority(_TargetPriority.Toughest);
		else if(priority==3) return SetTargetPriority(_TargetPriority.Random);
		else return false;
	}
	
	public bool SetTargetPriority(_TargetPriority priority){
		if(targetingArea==_TargetingArea.StraightLine) return false;
		targetPriority=priority;
		return true;
	}
	
	IEnumerator ScanForTargetStraightLine(){
		
		//rotate tower to match scan direction
		if(matchTowerDir2TargetDir){
			thisT.rotation=Quaternion.Euler(0, targetingDirection-90, 0);
		}
		
		scanDirection=Vector3.zero;
		scanDirection.x=Mathf.Cos(targetingDirection*Mathf.Deg2Rad);
		scanDirection.z=Mathf.Sin(targetingDirection*Mathf.Deg2Rad);
		RaycastHit hit;		
		while(targetingArea==_TargetingArea.StraightLine){
			if(built && !stunned){
				//~ if(target==null){
					target=null;
					if(Physics.Raycast(thisT.position, scanDirection, out hit, effRange, maskTarget)){
						Unit currentTarget=hit.collider.gameObject.GetComponent<Unit>();
						if(currentTarget!=null && currentTarget.HPAttribute.HP>0) target=currentTarget;
					}
				//~ }
				//~ else{
					//if target is out of range or dead or inactive, clear target
					//~ currentTargetDist=Vector3.Distance(thisT.position, target.thisT.position);
					//~ if(currentTargetDist>range || target.HPAttribute.HP<=0 || !target.thisObj.active){
						//~ target=null;
					//~ }
				//~ }
					
				//Debug.DrawLine(thisT.position, thisT.position+scanDirection, Color.red, 0.1f);
			}
			yield return null;
		}
	}
	
	
	IEnumerator ScanForTargetDirectional(){
		//rotate tower to match scan direction
		if(matchTowerDir2TargetDir){
			thisT.rotation=Quaternion.Euler(0, targetingDirection-90, 0);
		}
		
		scanDirection.x=Mathf.Cos(targetingDirection*Mathf.Deg2Rad);
		scanDirection.z=Mathf.Sin(targetingDirection*Mathf.Deg2Rad);
		scanQuaternion=Quaternion.LookRotation((thisT.position+scanDirection)-thisT.position);
		
		while(targetingArea==_TargetingArea.DirectionalCone){
			if(built && !stunned){
				
				//Debug.DrawLine(thisT.position, thisT.position+scanDirection*5, Color.red, 0.1f);
				
				if(target==null){
					Collider[] cols=Physics.OverlapSphere(thisT.position, effRange, maskTarget);
					Transform[] targetList=FilterTarget(cols, scanQuaternion);
					if(targetList.Length>0){
						
						Unit currentTarget=null;
						Unit targetTemp=null;
						
						if(targetPriority==_TargetPriority.Nearest){
							float dist=Mathf.Infinity;
							Transform currentTargetT=null;
							foreach(Transform targetT in targetList){
								float currentDist=Vector3.Distance(thisT.position, targetT.position);
								if(currentDist<dist){
									currentTargetT=targetT;
									dist=currentDist;
								}
							}
							currentTarget=currentTargetT.gameObject.GetComponent<Unit>();
							if(currentTarget!=null && currentTarget.HPAttribute.HP>0) target=currentTarget;
						}
						else if(targetPriority==_TargetPriority.Weakest){
							float hp=Mathf.Infinity;
							foreach(Transform targetT in targetList){
								targetTemp=targetT.gameObject.GetComponent<Unit>();
								if(targetTemp.HPAttribute.HP<hp && targetTemp.HPAttribute.HP>0){
									hp=targetTemp.HPAttribute.HP;
									currentTarget=targetTemp;
								}
							}
							if(currentTarget!=null && currentTarget.HPAttribute.HP>0) target=currentTarget;
						}
						else if(targetPriority==_TargetPriority.Toughest){
							float hp=0;
							foreach(Transform targetT in targetList){
								targetTemp=targetT.gameObject.GetComponent<Unit>();
								if(targetTemp.HPAttribute.HP>hp){
									hp=targetTemp.HPAttribute.HP;
									currentTarget=targetTemp;
								}
							}
							if(currentTarget!=null && currentTarget.HPAttribute.HP>0) target=currentTarget;
						}
						//~ else if(targetPriority==_TargetPriority.Toughest){
							//~ float hp=0;
							//~ foreach(Transform targetT in targetList){
								//~ targetTemp=targetT.gameObject.GetComponent<Unit>();
								//~ if(targetTemp.HPAttribute.HP>hp){
									//~ hp=targetTemp.HPAttribute.HP;
									//~ currentTarget=targetTemp;
								//~ }
							//~ }
							//~ if(currentTarget!=null && currentTarget.HPAttribute.HP>0) target=currentTarget;
						//~ }
						// Added the below else if statement
						else if(targetPriority == _TargetPriority.First) {
							float lastDm = 0;
							foreach(Transform targetT in targetList){
								targetTemp=targetT.gameObject.GetComponent<Unit>();
								float distMoved=targetTemp.GetDistanceMovedCreep();
								if(distMoved > lastDm){
									currentTarget=targetTemp;
									lastDm = distMoved;
								}
							}
							if(currentTarget!=null && currentTarget.HPAttribute.HP>0) target=currentTarget; 
						}
						else{
							Transform targetT=targetList[Random.Range(0, cols.Length)];
							currentTarget=targetT.gameObject.GetComponent<Unit>();
							if(currentTarget!=null && currentTarget.HPAttribute.HP>0) target=currentTarget;
						}
						
					}
				}
				else{
					//if target is out of range or dead or inactive, clear target
					currentTargetDist=Vector3.Distance(thisT.position, target.GetTargetT().position);
					#if UNITY_4_0
						if(currentTargetDist>effRange || target.HPAttribute.HP<=0 || !target.thisObj.activeInHierarchy){
					#else
						if(currentTargetDist>effRange || target.HPAttribute.HP<=0 || !target.thisObj.active){
					#endif
					
						target=null;
					}
					else{
						//if target is out of fov, clear the target
						if(!CheckIfInFOV(target.thisT, scanQuaternion)) target=null;
					}
				}
			}
			yield return null;
		}
	}
	
	bool CheckIfInFOV(Transform t, Quaternion wantedRot){
		Quaternion currentRot=Quaternion.LookRotation(t.position-thisT.position);
		if(Quaternion.Angle(wantedRot, currentRot)<targetingFOV){
			return true;
		}
		return false;
	}
	
	Transform[] FilterTarget(Collider[] cols, Quaternion wantedRot){
		List<Transform> filtered=new List<Transform>();
		foreach(Collider col in cols){
			Transform unit=col.transform;
			Quaternion currentRot=Quaternion.LookRotation(unit.position-thisT.position);
			if(Quaternion.Angle(wantedRot, currentRot)<targetingFOV){
				filtered.Add(unit);
			}
		}
		return filtered.ToArray();
	}	
	
	IEnumerator ScanForTargetAllAround(){
		while(targetingArea==_TargetingArea.AllAround){
			if(built && !stunned){
				if(target==null){
					Collider[] cols=Physics.OverlapSphere(thisT.position, effRange, maskTarget);
					if(cols.Length>0){
						
						Unit currentTarget=null;
						Unit targetTemp=null;
						
						if(targetPriority==_TargetPriority.Nearest){
							float dist=Mathf.Infinity;
							Collider currentCollider=cols[0];
							foreach(Collider col in cols){
								float currentDist=Vector3.Distance(thisT.position, col.transform.position);
								if(currentDist<dist){
									currentCollider=col;
									dist=currentDist;
								}
							}
							currentTarget=currentCollider.gameObject.GetComponent<Unit>();
							if(currentTarget!=null && currentTarget.HPAttribute.HP>0) target=currentTarget;
						}
						else if(targetPriority==_TargetPriority.Weakest){
							float hp=Mathf.Infinity;
							foreach(Collider col in cols){
								targetTemp=col.gameObject.GetComponent<Unit>();
								if(targetTemp.HPAttribute.HP<hp && targetTemp.HPAttribute.HP>0){
									hp=targetTemp.HPAttribute.HP;
									currentTarget=targetTemp;
								}
							}
							if(currentTarget!=null && currentTarget.HPAttribute.HP>0) target=currentTarget;
						}
						else if(targetPriority==_TargetPriority.Toughest){
							float hp=0;
							foreach(Collider col in cols){
								targetTemp=col.gameObject.GetComponent<Unit>();
								if(targetTemp.HPAttribute.HP>hp){
									hp=targetTemp.HPAttribute.HP;
									currentTarget=targetTemp;
								}
							}
							if(currentTarget!=null && currentTarget.HPAttribute.HP>0) target=currentTarget;
						}
						else if(targetPriority == _TargetPriority.First) {
							float lastDm = 0;
							foreach(Collider col in cols){
								targetTemp=col.gameObject.GetComponent<Unit>();
								float distMoved=targetTemp.GetDistanceMovedCreep();
								if(distMoved > lastDm){
									currentTarget=targetTemp;
									lastDm = distMoved;
								}
							}
							if(currentTarget!=null && currentTarget.HPAttribute.HP>0) target=currentTarget; 
						}
						else{
							Collider currentCollider=cols[Random.Range(0, cols.Length)];
							currentTarget=currentCollider.gameObject.GetComponent<Unit>();
							if(currentTarget!=null && currentTarget.HPAttribute.HP>0) target=currentTarget;
						}
					}
				}
				else{
					//if target is out of range or dead or inactive, clear target
					currentTargetDist=Vector3.Distance(thisT.position, target.GetTargetT().position);
					#if UNITY_4_0
						if(currentTargetDist>effRange || target.HPAttribute.HP<=0 || !target.thisObj.activeInHierarchy){
					#else
						if(currentTargetDist>effRange || target.HPAttribute.HP<=0 || !target.thisObj.active){
					#endif
					
						target=null;
					}
				}
			}
			yield return null;
		}
	}
	
	//default scan for target, get nearest target from all-direction, Obsolete
	IEnumerator ScanForTarget(){
		while(true){
			if(built && !stunned){
				if(target==null){
					Collider[] cols=Physics.OverlapSphere(thisT.position, effRange, maskTarget);
					if(cols.Length>0){
						float dist=Mathf.Infinity;
						Collider currentCollider=cols[0];
						foreach(Collider col in cols){
							float currentDist=Vector3.Distance(thisT.position, col.transform.position);
							if(currentDist<dist){
								currentCollider=col;
								dist=currentDist;
							}
						}
						Unit targetTemp=currentCollider.gameObject.GetComponent<Unit>();
						if(targetTemp!=null && targetTemp.HPAttribute.HP>0) target=targetTemp;
						//target=cols[0].gameObject.GetComponent<Unit>();
					}
				}
				else{
					//if target is out of range or dead or inactive, clear target
					currentTargetDist=Vector3.Distance(thisT.position, target.GetTargetT().position);
					#if UNITY_4_0
						if(currentTargetDist>effRange || target.HPAttribute.HP<=0 || !target.thisObj.activeInHierarchy){
					#else
						if(currentTargetDist>effRange || target.HPAttribute.HP<=0 || !target.thisObj.active){
					#endif
					
						target=null;
					}
				}
			}
			yield return null;
		}
	}
	
	
	IEnumerator Reload(){
		lastReloadTime=Time.time;
		yield return new WaitForSeconds(reloadDuration);
		currentClip=clipSize;
	}
	
	
	
	IEnumerator TurretRoutine(){
		while(true){
			if(built && target!=null && Vector3.Distance(thisT.position, target.GetTargetT().position)<effRange && targetInLOS && !stunned && currentClip!=0){
				
				if(turretFireAnimationBody!=null && turretFireAnimation!=null) turretFireAnimationBody.Play(turretFireAnimation.name);
				if(baseFireAnimationBody!=null && baseFireAnimation!=null) baseFireAnimationBody.Play(baseFireAnimation.name);
				
				foreach(Transform sp in shootPoint){
					GameObject obj=ObjectPoolManager.Spawn(shootObject, sp.position, sp.rotation);
					ShootObject shootObj=obj.GetComponent<ShootObject>();
					
					if(shootSound!=null) AudioManager.PlaySound(shootSound, thisT.position);
					
					shootObj.Shoot(target, this, sp);
				}
				
				currentClip-=1;
				if(currentClip==0) StartCoroutine(Reload());
				
				yield return new WaitForSeconds(Mathf.Max(0.05f, cooldown));
			}
			else{
				yield return null;
			}
		}
	}	
	
	
	IEnumerator DirectionalAOERoutine(){
		while(true){
			if(built && target!=null && Vector3.Distance(thisT.position, target.GetTargetT().position)<effRange && targetInLOS && !stunned && currentClip!=0){
				
				Vector3 srcPos=thisT.position;
				if(turretObject!=null) srcPos=turretObject.position;
				Quaternion wantedRotation=Quaternion.LookRotation(target.GetTargetT().position-srcPos);
				
				Collider[] cols=Physics.OverlapSphere(thisT.position, effRange, maskTarget);
				foreach(Collider col in cols){
					Quaternion tgtRotation=Quaternion.LookRotation(col.transform.position-srcPos);
					if(Quaternion.Angle(wantedRotation, tgtRotation)<aoeConeAngle/2){
						Unit unit=col.gameObject.GetComponent<Unit>();
						
						ApplyEffect(unit);
						
						Debug.DrawLine(thisT.position, unit.thisT.position, Color.red, 0.25f);
					}
				}
				
				if(turretFireAnimationBody!=null && turretFireAnimation!=null) turretFireAnimationBody.Play(turretFireAnimation.name);
				if(baseFireAnimationBody!=null && baseFireAnimation!=null) baseFireAnimationBody.Play(baseFireAnimation.name);
				
				if(shootObject!=null){
					foreach(Transform sp in shootPoint){
						ObjectPoolManager.Spawn(shootObject, sp.position, sp.rotation);
						//ObjectPoolManager.Unspawn(obj);
					}
				}
				
				if(shootSound!=null) AudioManager.PlaySound(shootSound, thisT.position);
				
				currentClip-=1;
				if(currentClip==0) StartCoroutine(Reload());
				
				yield return new WaitForSeconds(Mathf.Max(0.05f, cooldown));
			}
			else{
				yield return null;
			}
		}
	}
	
	IEnumerator AOERoutine(){
		while(true){
			if(built && !stunned){
				Collider[] cols=Physics.OverlapSphere(thisT.position, effRange, maskTarget);
				foreach(Collider col in cols){
					Unit unit=col.gameObject.GetComponent<Unit>();
					
					ApplyEffect(unit);
					
					Debug.DrawLine(thisT.position, unit.thisT.position, Color.red, 0.25f);
				}
				
				if(turretFireAnimationBody!=null && turretFireAnimation!=null) turretFireAnimationBody.Play(turretFireAnimation.name);
				if(baseFireAnimationBody!=null && baseFireAnimation!=null) baseFireAnimationBody.Play(baseFireAnimation.name);
				
				if(shootObject!=null){
					foreach(Transform sp in shootPoint){
						ObjectPoolManager.Spawn(shootObject, sp.position, thisT.rotation);
						//ObjectPoolManager.Unspawn(obj);
					}
				}
				
				if(shootSound!=null) AudioManager.PlaySound(shootSound, thisT.position);
			
				yield return new WaitForSeconds(cooldown);
			}
			else yield return null;
		}
	}
	
	IEnumerator ResourceRoutine(){
		while(true){
			if(built && !stunned){
				//gain resource
				//only valid when game is progressing, so resource tower cant be exploited
				if(GameControl.gameState==_GameState.Started){
					if(PerkManager.rscGainChance>0 || PerkManager.rscGainTowerChance>0){
						float randG=Random.Range(0f, 1f);
						float randT=Random.Range(0f, 1f);
						int[] inc=new int[incomes.Length];
						
						for(int i=0; i<incomes.Length; i++){
							float modifier=1;
							float bonus=0;
							
							if(randG<=PerkManager.rscGainChance){
								modifier+=PerkManager.rscGainModifier[i];
								bonus+=PerkManager.rscGainValue[i];
							}
							if(randT<=PerkManager.rscGainTowerChance){
								modifier+=PerkManager.rscGainTowerModifier[i];
								bonus+=PerkManager.rscGainTowerValue[i];
							}
							
							inc[i]=(int)Mathf.Round((float)incomes[i]*modifier+bonus);
						}
						GameControl.GainResource(inc);
						
						if(onGainResourceE!=null){
							GainResourcePos grp=new GainResourcePos(thisT.position, inc);
							onGainResourceE(grp);
						}
					}
					else{
						GameControl.GainResource(incomes);
						if(onGainResourceE!=null){
							GainResourcePos grp=new GainResourcePos(thisT.position, incomes);
							onGainResourceE(grp);
						}
					}
				}
				
				//~ if(GameControl.gameState==_GameState.Started){
					//~ if(PerkManager.rscGainTowerChance>0 && Random.Range(0f, 1f)>=PerkManager.rscGainTowerChance){
						//~ int[] inc=new int[incomes.Length];
						//~ for(int i=0; i<incomes.Length; i++){
							//~ float modifier=1+PerkManager.rscGainTowerModifier[i]+PerkManager.rscGainModifier[i];
							//~ float bonus=PerkManager.rscGainTowerValue[i]+PerkManager.rscGainValue[i];
							//~ inc[i]=(int)Mathf.Round((float)incomes[i]*modifier+bonus);
						//~ }
						//~ GameControl.GainResource(inc);
					//~ }
					//~ else GameControl.GainResource(incomes);
				//~ }
				
				if(turretFireAnimationBody!=null && turretFireAnimation!=null) turretFireAnimationBody.Play(turretFireAnimation.name);
				if(baseFireAnimationBody!=null && baseFireAnimation!=null) baseFireAnimationBody.Play(baseFireAnimation.name);
				
				if(shootObject!=null){
					foreach(Transform sp in shootPoint){
						ObjectPoolManager.Spawn(shootObject, sp.position, thisT.rotation);
					}
				}
				
				yield return new WaitForSeconds(cooldown);
			}
			else yield return null;
		}
	}
	
	
	private UnitTower[] buffList=new UnitTower[0];
	IEnumerator SupportRoutine(){
		buff.buffID=towerID;
		//when tower is upgraded, UpgradeStat() will take care of the buffID
		
		if(shootObject!=null){
			StartCoroutine(SupportTowerShootRoutine());
		}
		
		LayerMask maskTower=1<<LayerManager.LayerTower();
		while(true){
			if(built && !stunned){
				Collider[] cols=Physics.OverlapSphere(thisT.position, effRange, maskTower);

				if(buffList.Length>cols.Length){
					List<UnitTower> tempBuffList = new List<UnitTower>(buffList.Length);
					tempBuffList.AddRange(buffList);
					
					for(int i=0; i<tempBuffList.Count; i++){
						if(tempBuffList[i]==null){
							tempBuffList.RemoveAt(i);
							i--;
						}
					}
					
					buffList=tempBuffList.ToArray();
				}
				else if(buffList.Length<cols.Length){
					buffList=new UnitTower[cols.Length];
				
					for(int i=0; i<buffList.Length; i++){
						buffList[i]=cols[i].gameObject.GetComponent<UnitTower>();
						
						buffList[i].Buff(buff);
					}
				}
				
				yield return new WaitForSeconds(0.2f);
			}
			else if(!built){
				UnBuffAll();
				while(!built) yield return null;
				ReBuffAll();
			}
			else yield return null;
		}
	}
	
	IEnumerator SupportTowerShootRoutine(){
		while(true){
			while(!built) yield return null;
			
			if(turretFireAnimationBody!=null && turretFireAnimation!=null) turretFireAnimationBody.Play(turretFireAnimation.name);
			if(baseFireAnimationBody!=null && baseFireAnimation!=null) baseFireAnimationBody.Play(baseFireAnimation.name);
			
			if(shootObject!=null){
				foreach(Transform sp in shootPoint){
					ObjectPoolManager.Spawn(shootObject, sp.position, thisT.rotation);
				}
			}
			
			yield return new WaitForSeconds(cooldown);
		}
	}
	
	//thanks to GIOWorks
	public GameObject mineEffect;
	IEnumerator MineRoutine(){
		
		float gridSize=BuildManager.GetGridSize();
		if(mineEffect!=null) ObjectPoolManager.New(mineEffect, 2);
		
		while(true){
			
			if(built && !stunned){
				Collider[] cols=Physics.OverlapSphere(thisT.position, gridSize/2, maskTarget);
				if(cols.Length>0){
					
					if(shootSound!=null) AudioManager.PlaySound(shootSound, thisT.position);
					if(mineEffect!=null) ObjectPoolManager.Spawn(mineEffect, thisT.position, Quaternion.identity);
					//~ if(mineEffect!=null) Instantiate(mineEffect, thisT.position, Quaternion.identity);
					
					Collider[] targets=Physics.OverlapSphere(thisT.position, effRange, maskTarget);
					
					foreach(Collider col in targets){
						Unit unit=col.gameObject.GetComponent<Unit>();
						
						ApplyEffect(unit);
					}
					
					if(mineOneOff){
						if(onDestroyE!=null) onDestroyE(this);
						#if UNITY_4_0
							thisObj.SetActive(false);
						#else
							if(thisT.childCount>0) thisObj.SetActiveRecursively(false);
							else thisObj.active=false;
						#endif
					}
					else{
						yield return new WaitForSeconds(cooldown);
					}
					
				}
			}
			
			yield return new WaitForSeconds(0.1f);
		}
	}
	
	//call by projectile when the target is hit
	public void HitTarget(Vector3 pos, Unit tgt){
		HitTarget(pos, tgt, true, 0);
	}
	
	//div is sample count for continous damage over the duration of the beam shootObj
	//normal projectile shootObject has a default div value of 0
	//effect flag indicate if side effect is to be applied, only false for continous damage before the final tick
	public void HitTarget(Vector3 pos, Unit tgt, bool effect, int div){
		if(aoeRadius<=0){
			#if UNITY_4_0
				if(tgt.gameObject!=null && tgt.gameObject.activeInHierarchy){
			#else
				if(tgt.gameObject!=null && tgt.gameObject.active){
			#endif
			
				ApplyEffect(tgt, effect, div);
				GainExperience();
			}
		}
		else{
			Collider[] cols=Physics.OverlapSphere(pos, aoeRadius, maskTarget);
			foreach(Collider col in cols){
				Unit subTarget=col.gameObject.GetComponent<Unit>();
				if(subTarget!=null){
					ApplyEffect(subTarget, effect, div);
				}
			}
			
			if(cols.Length>0) GainExperience();
		}
	}
	
	//to call a specific dealth animation on the creep
	public int creepDeathAni=-1;
	
	//check stat and apply valid effect to the target
	void ApplyEffect(Unit unit, bool effect, int div){
		#if UNITY_4_0
			if(unit.thisObj.activeInHierarchy){
		#else
			if(unit.thisObj.active){
		#endif
			if(designatedDeathAni>=0) unit.SetDeathAnimation(creepDeathAni);			
			if(damage>0){
				
				float critRand=Random.Range(0f, 1f);
				
				float bonus=attBuffVal+PerkManager.allTowerAttackBuffValue;
				float dmg=damage*(attBuffMod+PerkManager.allTowerAttackBuffModifier)+bonus;
				
				float critMod=1;	float critVal=0;
				if(critRand<=GetCriticalChance(unit)){
					critMod=GetCriticalModifier();
					critVal=GetCriticalBonus();
					//~ Debug.Log("critical!");
				}
				
				if(div>0){
					unit.ApplyDamage(dmg*critMod/div+critVal/div, damageType);
				}
				else{
					unit.ApplyDamage(dmg*critMod+critVal, damageType);
				}
				//if(!continousDamage) unit.ApplyDamage(damage);
				//else unit.ApplyDamage(damage*0.05f);
			}
			if(unit.HPAttribute.HP>0){
				if(effect){
					if(stun.IsApplicable()) unit.ApplyStun(stun.duration);
					//~ if(stunDuration>0) unit.ApplyStun(stunDuration);
					if(slow.duration*slow.slowFactor>0) unit.ApplySlow(slow);
					if(dot.GetTotalDamage()>0) unit.ApplyDot(dot, damageType);
				}
			}
		}
	}
	
	void ApplyEffect(Unit unit){
		if(designatedDeathAni>=0) unit.SetDeathAnimation(creepDeathAni);
		if(damage>0){
			float critRand=Random.Range(0f, 1f);
			float dmg=damage*(attBuffMod)+attBuffVal;
			
			float critMod=1;	float critVal=0;
			if(critRand<=GetCriticalChance(unit)){
				critMod=GetCriticalModifier();
				critVal=GetCriticalBonus();
				//Debug.Log("critical!");
			}
			
			unit.ApplyDamage(dmg*critMod+critVal, damageType);
		}
		if(unit.HPAttribute.HP>0){
			if(stun.IsApplicable()) unit.ApplyStun(stun.duration);
			//~ if(stunDuration>0) unit.ApplyStun(stunDuration);
			if(slow.duration*slow.slowFactor>0) unit.ApplySlow(slow);
			if(dot.GetTotalDamage()>0) unit.ApplyDot(dot, damageType);
		}
	}
	
	public float GetCriticalChance(Unit unit){
		if(unit.immuneToCrit) return 0;
		return GetCriticalChance();
	}
	public float GetCriticalChance(){
		return PerkManager.allTowerCriticalChance+crit.chance+crit.GetPerkChance();
	}
	
	public float GetCriticalModifier(){
		return crit.dmgModifier+PerkManager.allTowerCriticalDmgModifier+crit.GetPerkDmgMod();
	}
	public float GetCriticalBonus(){
		return crit.dmgBonus+PerkManager.allTowerCriticalDmgValue+crit.GetPerkDmgVal();
	}
	
	
	
	private bool gainExp=false;
	
	void GainExperience(){
		//gain experience code goes here
		if(!gainExp) return;
		
		if(level<levelCap){
			experience+=1;
			
			int[] expCap=GetCost();
			if(experience>=expCap[0]){
				experience=experience-expCap[0];
				Upgrade();
			}
		}
	}
	
	public int GetExperience(){
		return experience;
	}

	
	//public void Select(){
	//	rangeIndicator.renderer.enabled=true;
	//}
	
	//public void Unselect(){
	//	rangeIndicator.renderer.enabled=false;
	//}
	
	//call for support tower to rebuff all the towers on bufflist, when the support tower is upgraded or etc
	private void ReBuffAll(){
		foreach(UnitTower unit in buffList){
			unit.Buff(buff);
		}
	}
	
	private void UnBuffAll(){
		//Debug.Log("unbuff all");
		foreach(UnitTower unit in buffList){
			unit.UnBuff(towerID);
		}
	}
	
	
	//called by a support tower to buff this tower
	private List<BuffStat> activeBuffList=new List<BuffStat>();
	public void Buff(BuffStat newBuff){
		if(activeBuffList.Contains(newBuff) || type==_TowerType.SupportTower) return;
		
		activeBuffList.Add(newBuff);
		
		damage=damage*(1+newBuff.damageBuff);
		effRange=effRange*(1+newBuff.rangeBuff);
		cooldown=cooldown*(1-newBuff.cooldownBuff);
		if(newBuff.regenHP>0) StartCoroutine(RegenHPRoutine(newBuff));
		
		//if(type==_TowerType.TurretTower) Debug.Log(unitName+"  "+newBuff.buffID+" damage: "+damage+"  range:"+range+"  cooldown:"+cooldown);
	}
	
	//remove buff effect, called when a support tower is destroy/sold
	public void UnBuff(int buffID){
		BuffStat tempBuff;
		for(int i=0; i<activeBuffList.Count; i++){
			tempBuff=activeBuffList[i];
			if(tempBuff.buffID==buffID){
				damage=damage/(1+tempBuff.damageBuff);
				effRange=effRange/(1+tempBuff.rangeBuff);
				cooldown=cooldown/(1-tempBuff.cooldownBuff);
				
				activeBuffList.RemoveAt(i);
				
				break;
			}
		}
		
		//if(type==_TowerType.TurretTower) Debug.Log(buffID+" unbuffing "+unitName+" damage: "+damage+"  range:"+range+"  cooldown:"+cooldown);
	}
	
	IEnumerator RegenHPRoutine(BuffStat buff){
		while(activeBuffList.Contains(buff)){
			//HPAttribute.HP=Mathf.Min(HPAttribute.fullHP, HP+);
			GainHP(Time.deltaTime*buff.regenHP);
			yield return null;
		}
	}
	
	private List<BuffStat> abilityBuffMod=new List<BuffStat>();
	private List<BuffStat> abilityBuffVal=new List<BuffStat>();
	private BuffStat abilityBuff=new BuffStat();
	private float buffDuration;
	
	public void AbilityBuffVal(BuffStat buff, float duration){
		StartCoroutine(AbilityBuffRoutine(buff, duration, _ModifierType.value));
	}
	public void AbilityBuffMod(BuffStat buff, float duration){
		StartCoroutine(AbilityBuffRoutine(buff, duration, _ModifierType.percentage));
	}
	
	IEnumerator AbilityBuffRoutine(BuffStat buff, float duration, _ModifierType modType){
		if(modType==_ModifierType.percentage){
			abilityBuffMod.Add(buff);
			UpdateAbilityBuff();
			yield return new WaitForSeconds(duration);
			abilityBuffMod.Remove(buff);
			
		}
		else if(modType==_ModifierType.value){
			abilityBuffVal.Add(buff);
			UpdateAbilityBuff();
			yield return new WaitForSeconds(duration);
			abilityBuffVal.Remove(buff);
		}
		UpdateAbilityBuff();
	}
		
	public void UpdateAbilityBuff(){
		float dmg=0;
		foreach(BuffStat stat in abilityBuffMod){
			dmg+=damage*stat.damageBuff;
		}
		foreach(BuffStat stat in abilityBuffVal){
			dmg+=stat.damageBuff;
		}
		
		float cd=0;
		foreach(BuffStat stat in abilityBuffMod){
			cd+=cooldown*stat.cooldownBuff;
		}
		foreach(BuffStat stat in abilityBuffVal){
			cd+=stat.cooldownBuff;
		}
		
		float rng=0;
		foreach(BuffStat stat in abilityBuffMod){
			rng+=range*stat.rangeBuff;
		}
		foreach(BuffStat stat in abilityBuffVal){
			rng+=stat.rangeBuff;
		}
		
		float rgh=0;
		foreach(BuffStat stat in abilityBuffMod){
			rgh+=HPAttribute.fullHP*stat.regenHP;
		}
		foreach(BuffStat stat in abilityBuffVal){
			rgh+=stat.regenHP;
		}
		
		abilityBuff.damageBuff=dmg;
		abilityBuff.cooldownBuff=cd;
		abilityBuff.rangeBuff=rng;
		abilityBuff.regenHP=rgh;
		
		if(abilityBuffVal.Count>0 || abilityBuffMod.Count>0){
			if(!abilityBuffRoutine) StartCoroutine(AbilityBuffDurationCD());
		}
	}
	
	private bool abilityBuffRoutine=false;
	IEnumerator AbilityBuffDurationCD(){
		abilityBuffRoutine=true;
		while(buffDuration>0){
			buffDuration-=Time.deltaTime;
			GainHP(Time.deltaTime*abilityBuff.regenHP);
			yield return null;
		}
		abilityBuff.damageBuff=0;
		abilityBuff.rangeBuff=0;
		abilityBuff.cooldownBuff=0;
		abilityBuff.regenHP=0;
		abilityBuffRoutine=false;
	}
	
	
	public _RotationMode turretRotationModel=_RotationMode.FullTurret;
	public float turretDefaultElevatedAngle=0;
	public float aimTolerance=15f;
	public float turretRotateSpeed=15f;
	//public Transform barrel;
	public override void Update(){
		base.Update();
		
		if(animateTurret!=_TurretAni.None && turretObject!=null && target!=null && !stunned){
				
				Vector3 shootPos=turretObject.position;
			
				if(animateTurret==_TurretAni.YAxis){
					Vector3 targetPos=target.GetTargetT().position;
					targetPos.y=turretObject.position.y;
					
					Quaternion wantedRot=Quaternion.LookRotation(targetPos-turretObject.position);
					turretObject.rotation=Quaternion.Slerp(turretObject.rotation, wantedRot, turretRotateSpeed*Time.deltaTime);
					
					if(Quaternion.Angle(turretObject.rotation, wantedRot)<aimTolerance) targetInLOS=true;
					else targetInLOS=false;
				}
				else if(animateTurret==_TurretAni.Full){
					if(turretRotationModel==_RotationMode.FullTurret){
						Vector3 targetPos=target.GetTargetT().position;
						Quaternion wantedRot=Quaternion.LookRotation(targetPos-turretObject.position);
						
						//calculate elavation offset
						float distFactor=Mathf.Min(1, Vector3.Distance(turretObject.position, targetPos)/turretMaxRange);
						float offset=distFactor*turretMaxAngle;
						wantedRot*=Quaternion.Euler(-offset, 0, 0);
						
						turretObject.rotation=Quaternion.Slerp(turretObject.rotation, wantedRot, turretRotateSpeed*Time.deltaTime);
						
						if(Quaternion.Angle(turretObject.rotation, wantedRot)<aimTolerance) targetInLOS=true;
						else targetInLOS=false;
					}
					else if(turretRotationModel==_RotationMode.SeparatedBarrel){
						Vector3 targetPos=target.GetTargetT().position;
						Vector3 dummyPos=targetPos;
						dummyPos.y=turretObject.position.y;
						
						Quaternion wantedRot=Quaternion.LookRotation(dummyPos-turretObject.position);
						turretObject.rotation=Quaternion.Slerp(turretObject.rotation, wantedRot, turretRotateSpeed*Time.deltaTime);
						
						//calculate elavation offset
						if(barrelObject!=null){
							shootPos=barrelObject.position;
							
							//wantedRot=Quaternion.LookRotation(targetPos-barrelObject.position);
							wantedRot=Quaternion.Euler(wantedRot.eulerAngles.x, barrelObject.rotation.eulerAngles.y, 0);
							
							float distFactor=Mathf.Min(1, Vector3.Distance(turretObject.position, targetPos)/turretMaxRange);
							float offset=distFactor*turretMaxAngle;
							wantedRot*=Quaternion.Euler(-offset, 0, 0);
							
							barrelObject.rotation=Quaternion.Slerp(barrelObject.rotation, wantedRot, turretRotateSpeed*Time.deltaTime);
							
							if(Quaternion.Angle(barrelObject.rotation, wantedRot)<aimTolerance) targetInLOS=true;
							else targetInLOS=false;
						}
						else{
							if(Quaternion.Angle(turretObject.rotation, wantedRot)<aimTolerance) targetInLOS=true;
							else targetInLOS=false;
						}
					}
				}
				
				if(targetInLOS && losMode==_LOSMode.Realistic){
					Vector3 targetPos=target.GetTargetT().position;
					Vector3 dir = targetPos-shootPos;
					float dist=Vector3.Distance(targetPos, shootPos);
					RaycastHit hit;
					if(Physics.Raycast(shootPos, dir, out hit, dist)){
						if(hit.transform!=target.thisT){
							
							targetInLOS=false;
							//Debug.DrawLine(targetPos, shootPos, Color.white, .75f);
							//Debug.DrawLine(hit.point, shootPos, Color.red, .75f);
						}
					}
				}

		}
		
	}
	
	
	
	//initialise stat, call when tower is first built
	private void InitStat(){
		//level=1;
		cooldown=baseStat.cooldown;
		
		range=baseStat.range;
		effRange=range*(rangeBuffMod)+rangeBuffVal;
		
		if(type==_TowerType.TurretTower || type==_TowerType.DirectionalAOETower || type==_TowerType.AOETower || type==_TowerType.Mine){
			damage=baseStat.damage;
			
			clipSize=baseStat.clipSize;
			currentClip=clipSize;
			if(currentClip<=0) currentClip=-1;
			reloadDuration=baseStat.reloadDuration;
			aoeRadius=baseStat.aoeRadius;
			//~ stunDuration=baseStat.stunDuration;
			stun=baseStat.stun;
			crit=baseStat.crit;
			slow=baseStat.slow;
			dot=baseStat.dot;
			
			//modify stun and slow with perk stats
			stun.chance+=perkStunChance;
			stun.duration+=perkStunDuration;
			slow.duration+=perkSlowDuration;
			slow.slowFactor+=perkSlowFactor;
		}
		else if(type==_TowerType.SupportTower){
			buff=baseStat.buff;
		}
		else if(type==_TowerType.ResourceTower){
			incomes=baseStat.incomes;
		}
		
		
		if(baseStat.shootObject!=null){
			shootObject=baseStat.shootObject.gameObject;
		}
		else{
			if(type==_TowerType.TurretTower){
				GameObject tempObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				tempObj.AddComponent<ShootObject>();
				#if UNITY_4_0
					tempObj.SetActive(false);
				#else
					tempObj.active=false;
				#endif
				
				shootObject=tempObj;
			}
		}
		
		if(baseStat.turretObject!=null){
			turretObject=baseStat.turretObject;
		}
		
		//since this is initialization, update shootpoint regardless of if there's a turretObject
		UpdateShootPointNBarrelObj();
		
		if(baseStat.baseObject!=null){
			baseObject=baseStat.baseObject;
		}
		
		if(baseStat.turretBuildAnimationBody!=null) turretBuildAnimationBody=baseStat.turretBuildAnimationBody;
		if(baseStat.turretBuildAnimation!=null) turretBuildAnimation=baseStat.turretBuildAnimation;
		if(baseStat.baseBuildAnimationBody!=null) baseBuildAnimationBody=baseStat.baseBuildAnimationBody;
		if(baseStat.baseBuildAnimation!=null) baseBuildAnimation=baseStat.baseBuildAnimation;

		if(baseStat.turretFireAnimationBody!=null) turretFireAnimationBody=baseStat.turretFireAnimationBody;
		if(baseStat.turretFireAnimation!=null) turretFireAnimation=baseStat.turretFireAnimation;
		if(baseStat.baseFireAnimationBody!=null) baseFireAnimationBody=baseStat.baseFireAnimationBody;
		if(baseStat.baseFireAnimation!=null) baseFireAnimation=baseStat.baseFireAnimation;
		
		if(turretBuildAnimationBody!=null && turretBuildAnimation!=null) turretBuildAnimationBody.AddClip(turretBuildAnimation, turretBuildAnimation.name);
		if(baseBuildAnimationBody!=null && baseBuildAnimation!=null) baseBuildAnimationBody.AddClip(baseBuildAnimation, baseBuildAnimation.name);
		if(turretFireAnimationBody!=null && turretFireAnimation!=null){
			turretFireAnimationBody.AddClip(turretFireAnimation, turretFireAnimation.name);
			turretFireAnimationBody.animation[turretFireAnimation.name].speed=baseStat.fireAnimationSpeed;
		}
		if(baseFireAnimationBody!=null && baseFireAnimation!=null){
			baseFireAnimationBody.AddClip(baseFireAnimation, baseFireAnimation.name);
			baseFireAnimationBody.animation[baseFireAnimation.name].speed=baseStat.fireAnimationSpeed;
		}
		
		UpdateTowerValue();
	}
	
	public GameObject nextLevelTower;
	public bool UpgradeBuildNew(){
		if(nextLevelTower!=null){
			int[] cost=GetCost();
			if(GameControl.HaveSufficientResource(cost)){
				GameControl.SpendResource(cost);
			}
			else{
				GameMessage.DisplayMessage("Insufficient Resource");
				return false;
			}
			
			GameObject towerObj=(GameObject)Instantiate(nextLevelTower, thisT.position, Quaternion.identity);
			UnitTower towerCom=towerObj.GetComponent<UnitTower>();
			towerCom.InitTower(towerID);
			
			Destroy();
			
			return true;
		}
		else GameMessage.DisplayMessage("Tower is fully upgraded");
		
		return false;
	}
	
	
	//public function call to level up tower, 
	public bool Upgrade(){
		if(level<levelCap){
			int levelM=level-1;
			//Debug.Log(levelM);
			
			//check if there are sufficient resource
			int[] cost=GetCost();
			if(GameControl.HaveSufficientResource(cost)){
				GameControl.SpendResource(cost);
			}
			else{
				GameMessage.DisplayMessage("Insufficient Resource");
				return false;
			}
			
			//start building process, stat will be update by the end of this coroutine
			StartCoroutine(Building(upgradeStat[levelM].buildDuration, true));
			return true;
		}
		
		GameMessage.DisplayMessage("Tower is fully upgraded");
		return false;
	}
	
	//called when the tower is being build or upgrade
	private IEnumerator Building(float dur, bool isUpgrade){
		StartCoroutine(Building(dur, isUpgrade, true));
		yield return null;
	}
	private IEnumerator Building(float dur, bool isUpgrade, bool showEffect){
		built=false;
		//level=1;
		
		if(showEffect && buildingEffect!=null) ObjectPoolManager.Spawn(buildingEffect, thisT.position, thisT.rotation);
		
		if(buildingSound!=null) AudioManager.PlaySound(buildingSound, thisT.position);
		else AudioManager.PlayTowerBuilding();
		
		currentBuildDuration=dur;
		remainingBuildDuration=dur;
		
		OverlayManager.Building(this);
		
		while(remainingBuildDuration>0){
			remainingBuildDuration-=Time.deltaTime;
			
			yield return null;
		}
		
		if(showEffect && buildingDoneEffect!=null) ObjectPoolManager.Spawn(buildingDoneEffect, thisT.position, thisT.rotation);
		
		if(builtSound!=null) AudioManager.PlaySound(builtSound, thisT.position);
		else AudioManager.PlayTowerBuilt();
		
		built=true;
		
		//when the tower is first built, there's no need to update stat since the stat has been updated
		if(isUpgrade) UpgradeStat();
		
		if(onBuildCompleteE!=null) onBuildCompleteE(this);
	}
	
	private void UpgradeStat(){
		
		int levelM=level-1;
		
		if(type==_TowerType.TurretTower || type==_TowerType.DirectionalAOETower || type==_TowerType.AOETower){
			damage=upgradeStat[levelM].damage;
			cooldown=upgradeStat[levelM].cooldown;
			clipSize=upgradeStat[levelM].clipSize;
			currentClip=clipSize;
			if(currentClip<=0) currentClip=-1;
			reloadDuration=upgradeStat[levelM].reloadDuration;
			range=upgradeStat[levelM].range;
			effRange=range*rangeBuffMod+rangeBuffVal;
			minRange=upgradeStat[levelM].minRange;
			aoeRadius=upgradeStat[levelM].aoeRadius;
			stun=upgradeStat[levelM].stun;
			crit=upgradeStat[levelM].crit;
			slow=upgradeStat[levelM].slow;
			dot=upgradeStat[levelM].dot;
			
			//modify stun and slow with perk stats
			stun.chance+=perkStunChance;
			stun.duration+=perkStunDuration;
			slow.duration+=perkSlowDuration;
			slow.slowFactor+=perkSlowFactor;
		}
		else if(type==_TowerType.SupportTower){
			buff=upgradeStat[levelM].buff;
			buff.buffID=towerID;
		}
		else if(type==_TowerType.ResourceTower){
			incomes=upgradeStat[levelM].incomes;
			cooldown=upgradeStat[levelM].cooldown;
		}
		
		if(upgradeStat[levelM].shootObject!=null) 
			shootObject=upgradeStat[levelM].shootObject.gameObject;
		
		if(upgradeStat[levelM].turretObject!=null){
			
			#if UNITY_4_0
				turretObject.gameObject.SetActive(false);
			#else
				if(turretObject.childCount>0) turretObject.gameObject.SetActiveRecursively(false);
				else turretObject.gameObject.active=false;
			#endif
			
			Transform turretTemp=(Transform)Instantiate(upgradeStat[levelM].turretObject);
			turretTemp.position=turretObject.position;
			turretTemp.rotation=turretObject.rotation;
			turretTemp.parent=thisT;
			turretObject=turretTemp;
			
			UpdateShootPointNBarrelObj();
			
			//Animation tempAni=null;
			
			//search for turret build animation component
			//if there's a fire animation clip
			if(upgradeStat[levelM].turretFireAnimation!=null){
				turretFireAnimationBody=(Animation)turretObject.GetComponent(typeof(Animation));
				if(turretFireAnimationBody==null){
					foreach(Transform child in turretObject.transform){
						turretFireAnimationBody=(Animation)child.gameObject.GetComponent(typeof(Animation));
						if(turretFireAnimationBody!=null) break;
					}
				}
				
				//if there's an animation component, assign the animation clip
				if(turretFireAnimationBody!=null){
					turretFireAnimation=upgradeStat[levelM].turretFireAnimation;
					turretFireAnimationBody.AddClip(turretFireAnimation, turretFireAnimation.name);
					turretFireAnimationBody.animation[turretFireAnimation.name].speed=upgradeStat[levelM].fireAnimationSpeed;
				}
			}
		}
		//~ if(upgradeStat[levelM].barrelObject!=null){
			//~ barrelObject=upgradeStat[levelM].barrelObject;
		//~ }
		
		if(upgradeStat[levelM].baseObject!=null){
			
			#if UNITY_4_0
				baseObject.gameObject.SetActive(false);
			#else
				if(baseObject.childCount>0) baseObject.gameObject.SetActiveRecursively(false);
				else baseObject.gameObject.active=false;
			#endif
			
			Transform baseTemp=(Transform)Instantiate(upgradeStat[levelM].baseObject);
			baseTemp.position=baseObject.position;
			baseTemp.rotation=baseObject.rotation;
			baseTemp.parent=thisT;
			baseObject=baseTemp;
			
			//baseObject=upgradeStat[levelM].baseObject;
			
			//Animation tempAni=null;
			
			//search for base animation component
			//if there's a fire animation clip
			if(upgradeStat[levelM].baseFireAnimation!=null){
				baseFireAnimationBody=(Animation)baseObject.GetComponent(typeof(Animation));
				if(baseFireAnimationBody==null){
					foreach(Transform child in baseObject.transform){
						baseFireAnimationBody=(Animation)child.gameObject.GetComponent(typeof(Animation));
						if(baseFireAnimationBody!=null) break;
					}
				}
				
				//if there's an animation component, assign the animation clip
				if(baseFireAnimationBody!=null){
					baseFireAnimation=upgradeStat[levelM].baseFireAnimation;
					baseFireAnimationBody.AddClip(baseFireAnimation, baseFireAnimation.name);
					baseFireAnimationBody.animation[baseFireAnimation.name].speed=upgradeStat[levelM].fireAnimationSpeed;
				}
			}
		}
		
		//~ if(upgradeStat[levelM].turretBuildAnimationBody!=null) turretBuildAnimationBody=upgradeStat[levelM].turretBuildAnimationBody;
		//~ if(upgradeStat[levelM].turretBuildAnimation!=null) turretBuildAnimation=upgradeStat[levelM].turretBuildAnimation;
		//~ if(upgradeStat[levelM].baseBuildAnimationBody!=null) baseBuildAnimationBody=upgradeStat[levelM].baseBuildAnimationBody;
		//~ if(upgradeStat[levelM].baseBuildAnimation!=null) baseBuildAnimation=upgradeStat[levelM].baseBuildAnimation;

		//~ if(upgradeStat[levelM].turretFireAnimationBody!=null) turretFireAnimationBody=upgradeStat[levelM].turretFireAnimationBody;
		//~ if(upgradeStat[levelM].turretFireAnimation!=null) turretFireAnimation=upgradeStat[levelM].turretFireAnimation;
		//~ if(upgradeStat[levelM].baseFireAnimationBody!=null) baseFireAnimationBody=upgradeStat[levelM].baseFireAnimationBody;
		//~ if(upgradeStat[levelM].baseFireAnimation!=null) baseFireAnimation=upgradeStat[levelM].baseFireAnimation;
		
		//~ if(turretBuildAnimationBody!=null && turretBuildAnimation!=null) turretBuildAnimationBody.AddClip(turretBuildAnimation, turretBuildAnimation.name);
		//~ if(baseBuildAnimationBody!=null && baseBuildAnimation!=null) baseBuildAnimationBody.AddClip(baseBuildAnimation, baseBuildAnimation.name);
		//~ if(turretFireAnimationBody!=null && turretFireAnimation!=null) turretFireAnimationBody.AddClip(turretFireAnimation, turretFireAnimation.name);
		//~ if(baseFireAnimationBody!=null && baseFireAnimation!=null) baseFireAnimationBody.AddClip(baseFireAnimation, baseFireAnimation.name);
		
		
		level+=1;
			
		UpdateTowerValue();
		GameControl.TowerUpgradeComplete(this);
	}
	
	private void UpdateShootPointNBarrelObj(){
		//get shootpoint, assigned to TurretObject component on turretObject
		if(turretObject!=null){
			TurretObject turretObj=turretObject.gameObject.GetComponent<TurretObject>();
			if(turretObj!=null){
				barrelObject=turretObj.barrelPivotPoint;
				//make sure the shootpoint is not null
				if(turretObj.shootPoint!=null && turretObj.shootPoint.Length>0){
					shootPoint=turretObj.shootPoint;
					return;
				}
			}
			else{
				barrelObject=null;
				//no specify shootpoint, use turretObject itself
				shootPoint=new Transform[1];
				shootPoint[0]=turretObject;
				return;
			}
		}
		
		//this tower have no turretObject, use thisT as shootPoint
		shootPoint=new Transform[1];
		shootPoint[0]=thisT;
		return;
	}

	
	public bool IsLevelCapped(){
		if(level<levelCap) return false;
		else return true;
	}
	
	public int GetLevel(){
		return level;
	}
	
	
	public int[] GetCost(){
		if(level<=0){
			int[] costs=CloneCost(baseStat.costs);
			float[] mod=PerkManager.GetTotalBuildCostReductionModifier(prefabID);
			
			for(int i=0; i<mod.Length; i++) mod[i]=Mathf.Clamp(1f-mod[i], 0f, 1f);
			
			float[] val=PerkManager.GetTotalBuildCostReductionValue(prefabID);
			for(int i=0; i<costs.Length; i++){
				
				costs[i]=(int)Mathf.Round((float)costs[i]*mod[i]-val[i]);
			}
			return costs;
		}
		else{
			int[] costs=CloneCost(upgradeStat[Mathf.Min(level-1, upgradeStat.Length)].costs);
			float[] mod=PerkManager.GetAllUpgradeCostReductionModifier();
			
			for(int i=0; i<mod.Length; i++){
				mod[i]+=upCostReducMod[i];
				mod[i]=Mathf.Clamp(1f-mod[i], 0f, 1f);
			}
			
			int[] val=PerkManager.GetAllUpgradeCostReductionValue();
			for(int i=0; i<mod.Length; i++){
				val[i]+=upCostReducVal[i];
			}
			
			for(int i=0; i<costs.Length; i++){
				costs[i]=(int)Mathf.Round((float)costs[i]*mod[i]-val[i]);
			}
			return costs;
		}
	}
	
	int[] CloneCost(int[] cost){
		int[] clone=new int[cost.Length];
		for(int i=0; i<clone.Length; i++){ clone[i]=cost[i]; }
		return clone;
	}
	
	
	public TowerStat GetCurrentStat(){
		if(level==1) return baseStat;
		else return upgradeStat[level-2];
	}
	
	public TowerStat GetBaseStat(){
		return baseStat;
	}
	
	public string GetDescription(){
		return description;
	}
	
	private OccupiedPlatform occupiedPlatform;
	
	//set a parent platform and node to this tower when it's build on a walkable platform
	public void SetPlatform(PlatformTD platform, NodeTD node){
		occupiedPlatform=new OccupiedPlatform(platform, node);
	}
	
	public bool IsBuilt(){
		return built;
	}
	
	public void Sell(){
		StartCoroutine(Unbuilding());
	}
	
	private IEnumerator Unbuilding(){
		built=false;
		
		//stunned=true;
		//if(type==_TowerType.SupportTower) UnBuffAll();
		
		//currentBuildDuration=dur;
		//remainingBuildDuration=0;
		
		//Debug.Log(currentBuildDuration);
		
		OverlayManager.Unbuilding(this);
		
		while(remainingBuildDuration<currentBuildDuration){
			remainingBuildDuration+=Time.deltaTime;
			yield return null;
		}
		
		if(soldSound!=null) AudioManager.PlaySound(soldSound, thisT.position);
		else AudioManager.PlayTowerSold();
		
		//tells gamecontrol to refund the tower
		int[] sellValue=GetTowerSellValue();
		
		if(onGainResourceE!=null){
			GainResourcePos grp=new GainResourcePos(thisT.position, sellValue);
			onGainResourceE(grp);
		}
		
		GameControl.GainResource(sellValue);
		
		//GameControl.ClearSelection();
		
		Destroy();
	}
	
	//call when the tower is sold or destroy
	public void Destroy(){
		if(onDestroyE!=null) onDestroyE(this);
		
		//tells platform to reactivate node if platform is walkable
		if(occupiedPlatform!=null){
			//Debug.Log("clear attached platform");
			occupiedPlatform.platform.UnBuild(occupiedPlatform.node, footprint);
			occupiedPlatform=null;
		}
		//else Debug.Log("no platform attached");
		
		//thisObj.SetActiveRecursively(false);
		Destroy(thisObj);
	}
	
	public GameObject destroyEffect;
	
	public void Dead(){
		if(destroyEffect!=null) ObjectPoolManager.Spawn(destroyEffect, GetTargetT().position, Quaternion.identity);
		Destroy();
	}
	
	
	private void UpdateTowerValue(){
		//Debug.Log("update tower value, level "+level);
		towerValue=new int[baseStat.costs.Length];
		
		towerValue=baseStat.costs;
		for(int i=0; i<level-1; i++){
			for(int j=0; j<towerValue.Length; j++){
				towerValue[j]+=upgradeStat[i].costs[j];
			}
		}
	}
	
	
	public int[] GetTowerValue(){
		return towerValue;
	}
	
	public int[] GetTowerSellValue(){
		int[] sellValue=new int[towerValue.Length];
		
		for(int j=0; j<sellValue.Length; j++){
			sellValue[j]=towerValue[j];
		}
		
		GameControl.GetSellTowerRefundRatio();
		for(int i=0; i<towerValue.Length; i++){
			sellValue[i]=(int)Mathf.Floor(sellValue[i]*GameControl.GetSellTowerRefundRatio());
		}
		
		return sellValue;
	}
	
	//for editor, when updating upgrade stat
	public void UpdateTowerUpgradeStat(int size){
		TowerStat[] temp=upgradeStat;
		
		if(temp.Length==0){
			temp=new TowerStat[1];
			temp[0]=new TowerStat();
			temp[0].stun=new Stun();
			temp[0].crit=new Critical();
			temp[0].slow=new Slow();
			temp[0].dot=new Dot();
			temp[0].buff=new BuffStat();
		}
		
		upgradeStat=new TowerStat[size];
		
		for(int i=0; i<upgradeStat.Length; i++){
			if(i>=temp.Length) upgradeStat[i]=temp[temp.Length-1].Clone();
			else upgradeStat[i]=temp[i];
		}
	}
	
	void OnDrawGizmos(){
		if(target!=null){
			Gizmos.DrawLine(thisT.position, target.GetTargetT().position);
		}
		//~ else Debug.Log("no target");
	}
	
}




[System.Serializable]
public class TowerStat{
	public int cost=10;
	public int[] costs=new int[1];
	
	public float damage=5;
	public float cooldown=1;
	public int clipSize=5;
	public float reloadDuration=1;
	public float range=10;
	public float minRange=0;
	public float aoeRadius=0;
	//~ public float stunChance
	//~ public float stunDuration=0;
	public Stun stun;
	public Critical crit;
	public Slow slow;
	public Dot dot;
	//public bool mineOneOff;
	
	public BuffStat buff;
	public int[] incomes=new int[1];
	
	public float buildDuration=1;
	
	public Transform shootObject;
	
	public Transform barrelObject;
	public Transform turretObject;
	public Transform baseObject;
	
	//~ public GameObject BuildingEffect;
	//~ public GameObject BuildingDoneEffect;
	
	public Animation turretBuildAnimationBody;
	public AnimationClip turretBuildAnimation;
	public Animation baseBuildAnimationBody;
	public AnimationClip baseBuildAnimation;
	
	public Animation turretFireAnimationBody;
	public AnimationClip turretFireAnimation;
	public Animation baseFireAnimationBody;
	public AnimationClip baseFireAnimation;
	
	public float fireAnimationSpeed=1;
	
	
	public TowerStat Clone(){
		TowerStat clone=new TowerStat();
		clone.cost=cost;
		//clone.costs=costs;
		
		clone.costs=new int[costs.Length];
		for(int i=0; i<costs.Length; i++){
			clone.costs[i]=costs[i];
		}
		
		clone.damage=damage;
		clone.cooldown=cooldown;
		clone.clipSize=clipSize;
		clone.reloadDuration=reloadDuration;
		clone.range=range;
		clone.minRange=minRange;
		clone.aoeRadius=aoeRadius;
		//~ clone.stunDuration=stunDuration;
		clone.stun=stun.Clone();
		clone.crit=crit.Clone();
		clone.slow=slow.Clone();
		clone.dot=dot.Clone();
		clone.buff=buff.Clone();
		//clone.incomes=incomes;
		
		clone.incomes=new int[incomes.Length];
		for(int i=0; i<incomes.Length; i++){
			clone.incomes[i]=incomes[i];
		}
		
		clone.buildDuration=buildDuration;
		
		return clone;
	}
	
}

public enum _DotType{FlatRate, Linear, Exponential, InverseLinear, InverseExponential}
[System.Serializable]
public class Dot{
	public _DotType dotType=_DotType.FlatRate;
	
	//public float damage=1f;
	public float duration=3;
	public float interval=0.5f;
	
	public float modifier1=0.5f;
	public float modifier2=1;
	public float power=2;
	
	public bool stackable=false;
	
	public float GetTotalDamage(){
		int count=(int)Mathf.Round(duration/interval);
		float totalDamage=0;
		
		for(int i=1; i<=count; i++){
			totalDamage+=SampleDamage(i);
		}
		
		return totalDamage;
	}
	
	public float SampleDamage(int i){
		float dmg=0;
		
		if(dotType==_DotType.FlatRate){
			dmg=modifier1;
		}
		else if(dotType==_DotType.Linear){
			dmg=modifier1*(i*interval)+modifier2;
		}
		else if(dotType==_DotType.Exponential){
			dmg=modifier1*(float)System.Math.Pow(i*interval, power)+modifier2;
		}
		else if(dotType==_DotType.InverseLinear){
			int count=(int)Mathf.Round(duration/interval);
			dmg=modifier1*((count-i+1)*interval)+modifier2;
		}
		else if(dotType==_DotType.InverseExponential){
			int count=(int)Mathf.Round(duration/interval);
			dmg=modifier1*(float)System.Math.Pow((count-i-1)*interval, power)+modifier2;
		}
		
		return Mathf.Max(0, dmg);
	}
	
	public Dot Clone(){
		Dot clone=new Dot();
		
		clone.dotType=dotType;
		clone.duration=duration;
		clone.interval=interval;
		clone.modifier1=modifier1;
		clone.modifier1=modifier2;
		clone.power=power;
		
		return clone;
	}
}


[System.Serializable]
public class Critical{
	public float chance=0;
	public float dmgModifier=0;
	public float dmgBonus=0;
	
	[HideInInspector] private float perkChance=0;
	[HideInInspector] private float perkDmgMod=1;
	[HideInInspector] private float perkDmgVal=0;
	
	public void AddPerkChance(float val){ perkChance+=val; }
	public void AddPerkDmgMod(float val){ perkDmgMod+=val; }
	public void AddPerkDmgVal(float val){ perkDmgVal+=val; }
	
	public float GetPerkChance(){ return perkChance; }
	public float GetPerkDmgMod(){ return perkDmgMod; }
	public float GetPerkDmgVal(){ return perkDmgVal; }
	
	
	public Critical Clone(){
		Critical clone=new Critical();
		clone.chance=chance;
		clone.dmgModifier=dmgModifier;
		clone.dmgBonus=dmgBonus;
		
		perkChance=0;
		perkDmgMod=1;
		perkDmgVal=0;
		
		return clone;
	}
}

[System.Serializable]
public class Slow{
	public float duration=3;
	public float slowFactor=0.5f;
	private float timeEnd;
	
	public float GetTimeEnd(){
		return timeEnd;
	}
	public void SetTimeEnd(float val){
		timeEnd=val;
	}
	
	public Slow Clone(){
		Slow clone=new Slow();
		clone.slowFactor=slowFactor;
		clone.duration=duration;
		clone.timeEnd=timeEnd;
		
		return clone;
	}
}

[System.Serializable]
public class BuffStat{
	//buff doesnt stack, higher level override lowerlevel buff
	[HideInInspector] public int buffID=0;
	public float damageBuff=0.1f;
	public float cooldownBuff=0.1f;
	public float rangeBuff=0.1f;
	public float regenHP=1.0f;
	
	public BuffStat Clone(){
		BuffStat clone=new BuffStat();
		clone.buffID=buffID;
		clone.damageBuff=damageBuff;
		clone.cooldownBuff=cooldownBuff;
		clone.rangeBuff=rangeBuff;
		clone.regenHP=regenHP;
		
		return clone;
	}
}


[System.Serializable]
public class Stun{
	public float chance;
	public float duration;
	
	public bool IsApplicable(){
		if(duration>0){
			if(Random.Range(0f, 1f)<chance){
				return true;
			}
		}
		return false;
	}
	
	public Stun Clone(){
		Stun stun=new Stun();
		stun.chance=chance;
		stun.duration=duration;
		
		return stun;
	}
}


public class OccupiedPlatform{
	public PlatformTD platform;
	public NodeTD node;
	
	public OccupiedPlatform(PlatformTD p, NodeTD n){
		platform=p;
		node=n;
	}
}


