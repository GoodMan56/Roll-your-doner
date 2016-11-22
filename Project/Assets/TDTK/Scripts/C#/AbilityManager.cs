using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AbilityManager : MonoBehaviour {

	public delegate void UpdateEnergyHandler(float val); 
	public static event UpdateEnergyHandler onUpdateEnergyE;
	
	public delegate void AbilityActivateHandler(); 
	public static event AbilityActivateHandler onAbilityActivateE;
	
	public delegate void AbilityReadyHandler(); 
	public static event AbilityReadyHandler onAbilityReadyE;
	
	
	[HideInInspector] 
	public List<Ability> allAbilityList=new List<Ability>();
	[HideInInspector] 
	public List<Ability> activeAbilityList=new List<Ability>();

	private float energyDefault;
	public float energyMax=100;
	public float energy=100;
	public float energyRate=2;
	
	public static float energyCostModifier=1;
	public static float energyRateModifier=1;
	public static float energyCapModifier=1;
	
	public static float energyCostValue=0;
	public static float energyRateValue=0;
	public static float energyCapValue=0;
	
	public static float energyGainWaveClearedChance=0;
	public static float energyGainCreepKilledChance=0;
	public static float energyGainWaveClearedMod=0;
	public static float energyGainWaveClearedVal=0;
	public static float energyGainCreepKilledMod=0;
	public static float energyGainCreepKilledVal=0;
	
	public static AbilityManager abilityManager;
	
	public static Resource energyInfo;

	
	public List<Ability> Load(){
		GameObject obj=(GameObject)Instantiate(Resources.Load("PrefabListAbility", typeof(GameObject)));
		obj.transform.parent=transform;
		
		AbilityListPrefab prefab=obj.GetComponent<AbilityListPrefab>();
		List<Ability> list=prefab.abilityList;
		
		for(int i=0; i<list.Count; i++){//from prefab
			Ability ability=list[i];
			bool match=false;
			foreach(Ability a in allAbilityList){//default
				if(ability.ID==a.ID){
					ability.enableInlvl=a.enableInlvl;
					if(ability.enableInlvl){
						activeAbilityCount+=1;
						activeAbilityList.Add(ability);
					}
					match=true;
				}
			}
			if(!match){
				activeAbilityCount+=1;
				activeAbilityList.Add(ability);
			}
		}
		
		allAbilityList=list;
		
		foreach(Ability ability in allAbilityList){
			ability.cooldown=0;
		}
		
		energyInfo=prefab.resource;
		
		return allAbilityList;
	}
	

	void Awake(){
		abilityManager=this;
		energyDefault=energyMax;
		
		activeAbilityCount=0;
		Load();
	}
	
	// Use this for initialization
	void Start() {
		if(indicator==null){
			indicator=(Transform)Instantiate(Resources.Load("AbilityIndicator", typeof(Transform)));
			if(indicator==null){
				Debug.Log("warning, no default indicator has been assigned for AbilityManager");
				indicator.parent=transform;
			}
		}
		
		int overlayLayer=LayerManager.LayerOverlay();
		foreach(Ability ability in allAbilityList){
			if(ability.indicator!=null){
				ability.indicator=(Transform)Instantiate(ability.indicator);
				UnitUtility.DestroyColliderRecursively(ability.indicator);
				Utility.SetActive(ability.indicator.gameObject, false);
				Utility.SetLayerRecursively(ability.indicator, overlayLayer);
				//Debug.Log("visualEffect:  "+ability.ID);
				//~ ability.indicator.gameObject.name=ability.name;
				
				if(ability.autoScaleIndicator){
					float scale=2*ability.aoeRange/10;
					ability.indicator.localScale=new Vector3(scale, 1, scale);
				}
				
				ability.indicator.gameObject.layer=overlayLayer;
				ability.indicator.parent=transform;
			}
			
			if(ability.visualEffect!=null){
				ObjectPoolManager.New(ability.visualEffect, 1);
				
			}
		}
		
		Utility.SetActive(indicator.gameObject, false);
		indicator.gameObject.layer=LayerManager.LayerOverlay();
	}
	
	void OnEnable(){
		SpawnManager.onWaveClearedE += OnWaveCleared;
		Unit.onDeadE += OnUnitDeath;
	}
	
	void OnDisable(){
		SpawnManager.onWaveClearedE -= OnWaveCleared;
		Unit.onDeadE -= OnUnitDeath;
	}
	
	void OnWaveCleared(int waveID){
		if(Random.Range(0f, 1)<energyGainWaveClearedChance){
			energy=Mathf.Clamp(energy+energyMax*energyGainWaveClearedMod+energyGainWaveClearedVal, 0, energyMax);
		}
	}
	
	void OnUnitDeath(int waveID){
		if(Random.Range(0f, 1f)<energyGainCreepKilledChance){
			energy=Mathf.Clamp(energy+energyMax*energyGainCreepKilledMod+energyGainCreepKilledVal, 0, energyMax);
		}
	}
	
	public static void UpdateEnergyCap(){
		abilityManager.energyMax=abilityManager.energyDefault*energyCapModifier+energyCapValue;
	}
	
	public static bool AddAbility(int ID){
		foreach(Ability ability in abilityManager.allAbilityList){
			if(ability.ID==ID){
				ability.enableInlvl=true;
				activeAbilityCount+=1;
				abilityManager.activeAbilityList.Add(ability);
				return true;
			}
		}
		return false;
	}
	
	public static bool UpgradeAbility(int oldID, int newID){
		Ability old=null;
		Ability ne=null;
		foreach(Ability ability in abilityManager.allAbilityList){
			if(ability.ID==newID){
				ability.enableInlvl=true;
				ne=ability;
			}
			else if(ability.ID==oldID){
				ability.enableInlvl=false;
				old=ability;
			}
		}
		if(old!=null && ne!=null){
			for(int i=0; i<abilityManager.activeAbilityList.Count; i++){
				if(abilityManager.activeAbilityList[i]==old){
					abilityManager.activeAbilityList[i]=ne;
				}
			}
			return true;
		}
		return false;
	}
	
	public static void ReduceAbilityCost(Perk perk){
		for(int i=0; i<perk.abilityGroup.Count; i++){
			foreach(Ability ability in abilityManager.allAbilityList){
				if(ability.ID==perk.abilityGroup[i]){
					if(perk.modTypeVal==_ModifierType.percentage){
						ability.energy=Mathf.Max(0, ability.energy*perk.value[0]);
					}
					else if(perk.modTypeVal==_ModifierType.value){
						ability.energy=Mathf.Max(0, ability.energy-perk.value[0]);
					}
				}
			}
		}
	}
	
	public static void ReduceAbilityCD(Perk perk){
		for(int i=0; i<perk.abilityGroup.Count; i++){
			foreach(Ability ability in abilityManager.allAbilityList){
				if(ability.ID==perk.abilityGroup[i]){
					if(perk.modTypeVal==_ModifierType.percentage){
						ability.cdDuration=Mathf.Max(0, ability.cdDuration*perk.value[0]);
					}
					else if(perk.modTypeVal==_ModifierType.value){
						ability.cdDuration=Mathf.Max(0, ability.cdDuration-perk.value[0]);
					}
				}
			}
		}
	}
	
	
	
	public static int activeAbilityCount=0;
	public static int GetActiveAbilityCount(){
		return activeAbilityCount;
	}
	
	public static List<Ability> GetActiveAbilityList(){
		return abilityManager.activeAbilityList;
	}
	
	// Update is called once per frame
	void Update () {
		if(energy<energyMax){
			energy+=Time.deltaTime*(energyRate*energyRateModifier+energyRateValue);
			energy=Mathf.Clamp(energy, 0, energyMax);
			if(onUpdateEnergyE!=null) onUpdateEnergyE(energy);
		}
		
		//Debug.Log(activeAbilityList[0].cooldown);
		
		foreach(Ability ab in activeAbilityList){
			ab.cooldown=Mathf.Max(0, ab.cooldown-=Time.deltaTime);
		}
	}
	
	void IncreaseEnergyCap(float val, _ModifierType mod){
		if(mod==_ModifierType.value){
			energyMax+=val;
		}
		else if(mod==_ModifierType.percentage){
			energyMax+=energyDefault*val;
		}
	}
	
	private static Vector3 currentTriggerPos=new Vector3(-9999, -9999, -9999);
	private static bool checkedTrigger=false;
	public static bool CheckTriggerPoint(Vector3 pointer){
		LayerMask mask=1<<LayerManager.LayerPlatform() | 1<<LayerManager.LayerTerrain();
		
		Ray ray = Camera.main.ScreenPointToRay(pointer);
		RaycastHit hit;
		if(Physics.Raycast(ray, out hit, Mathf.Infinity, mask)){
			checkedTrigger=true;
			currentTriggerPos=hit.point;
			return true;
		}
		
		currentTriggerPos=new Vector3(-9999, -9999, -9999);
		return false;
	}
	public static Vector3 GetTriggerPos(){
		return currentTriggerPos;
	}
	
	public Transform indicator;
	private Transform currentIndicator;
	public static void ShowIndicator(Vector3 pointer){
		//~ _AbilityTargetType targetType=abilityManager.allAbilityList[selectedAbilityID].targetType;
		//~ if(targetType==_AbilityTargetType.AOEHostile){
			if(abilityManager.currentIndicator==null) return;
			
			CheckTriggerPoint(pointer);
			Transform ind=abilityManager.currentIndicator;
			
			ind.position=currentTriggerPos;
		
		//~ }
		//~ else if(targetType==_AbilityTargetType.AOEFriendly){
			//~ if(abilityManager.currentIndicator==null) return;
			
			//~ CheckTriggerPoint(pointer);
			//~ Transform ind=abilityManager.currentIndicator;
			
			//~ ind.position=currentTriggerPos;
		//~ }
		//~ else if(targetType==_AbilityTargetType.SingleHostile){
			
		//~ }
		//~ else if(targetType==_AbilityTargetType.SingleFriendly){
			
		//~ }
	}
	
	//~ void OnGUI(){
		
	//~ }
	
	public static void ActivateIndicator(){
		//~ if(abilityManager!=null) abilityManager.indicator.renderer.enabled=true;
		
		if(abilityManager.activeAbilityList[selectedAbilityID].indicator==null){
			abilityManager.currentIndicator=abilityManager.indicator;
			
			if(abilityManager.activeAbilityList[selectedAbilityID].autoScaleIndicator){
				float scale=2*abilityManager.activeAbilityList[selectedAbilityID].aoeRange/10;
				abilityManager.currentIndicator.localScale=new Vector3(scale, 1, scale);
			}
		}
		else{
			abilityManager.currentIndicator=abilityManager.activeAbilityList[selectedAbilityID].indicator;
		}
		
		Utility.SetActive(abilityManager.currentIndicator.gameObject, true);
	}
	public static void DeactivateIndicator(){
		//~ if(abilityManager!=null) abilityManager.indicator.renderer.enabled=false;
		Utility.SetActive(abilityManager.currentIndicator.gameObject, false);
		abilityManager.currentIndicator=null;
	}
	
	public static int selectedAbilityID=-1;
	public static int SelectAbility(int ID){
		if(ID<0) return -1;
		
		for(int i=0; i<abilityManager.activeAbilityList.Count; i++){
			Ability ability=abilityManager.activeAbilityList[i];
			if(ability.ID==ID){
				ID=i;
				int status=ability.IsReady(abilityManager.energy);
				if(status!=0) return status;
				break;
			}
		}
		
		if(ID<0 || ID>=abilityManager.activeAbilityList.Count) return -1;
		//Debug.Log(ID);
		//~ int status=abilityManager.activeAbilityList[ID].IsReady(abilityManager.energy);
		//~ if(status!=0) return status;
		
		selectedAbilityID=ID;
		ActivateIndicator();
		return 0;
	}
	public static void UnselectAbility(){
		selectedAbilityID=-1;
		DeactivateIndicator();
	}
	
	public static int ActivateAbility(int ID){ 
		if(ID<0 && ID>=abilityManager.allAbilityList.Count) return -1;
		//~ return abilityManager.ActivateAbility(abilityManager.allAbilityList[ID], currentTriggerPos); 
		return abilityManager.ActivateAbility(ID, currentTriggerPos); 
	}
	//~ public static int ActivateAbility(int ID, Vector3 pos){ 
		//~ if(ID<0 && ID>=abilityManager.allAbilityList.Count) return -1;
		//~ //return abilityManager.ActivateAbility(abilityManager.allAbilityList[ID], pos); 
		//~ return abilityManager.ActivateAbility(ID, pos); 
	//~ }
	public static int ActivateAbility(Vector3 pos){
		if(selectedAbilityID<0 && selectedAbilityID>=abilityManager.allAbilityList.Count) return -1;
		//~ return abilityManager.ActivateAbility(abilityManager.allAbilityList[selectedAbilityID], pos); 
		return abilityManager.ActivateAbility(selectedAbilityID, pos); 
	}
	//~ public int ActivateAbility(Ability ability, Vector3 pos){
	public int ActivateAbility(int ID, Vector3 pos){
		
		Ability ability=abilityManager.activeAbilityList[ID];
		
		if(checkedTrigger){
			checkedTrigger=false;
		}
		else{
			Debug.Log("Check for a trigger position before you activate ability");
			return -1;
		}
		
		int status=ability.IsReady(abilityManager.energy);
		if(status!=0) return status;
		
		if(ability.energy>0){
			energy-=ability.energy*energyCostModifier-energyCostValue;
			if(onUpdateEnergyE!=null) onUpdateEnergyE(energy);
		}
		GameControl.SpendResource(ability.costs);
		
		//~ if(ability.costs!=null) GameControl.SpendResource(ability.costs);
		ability.cooldown=ability.cdDuration;
		//Debug.Log("AbilityManager: "+ability.name+"  "+ability.cooldown+"   "+ability.cdDuration);
		
		//~ Debug.Log(ability.effects.Count);
		
		ability.usedCount+=1;
		
		if(ability.visualEffect!=null) ObjectPoolManager.Spawn(ability.visualEffect, pos, Quaternion.identity);
		
		foreach(AbilityEffect effect in ability.effects){
			//Debug.Log(effect.type+"  "+effect.value[0]+"  "+effect.value[1]);
			if(effect.type==_AbilityEffects.AOEDamage){
				StartCoroutine(AOEDamage(pos, ability.aoeRange, effect.value[0], 0, ability.launchDelay));
			}
			if(effect.type==_AbilityEffects.AOESlow){
				Slow slow=new Slow();
				slow.duration=effect.value[0];
				slow.slowFactor=effect.value[1];
				StartCoroutine(AOESlow(pos, ability.aoeRange, slow, ability.launchDelay));
			}
			if(effect.type==_AbilityEffects.AOEStun){
				StartCoroutine(AOEStun(pos, ability.aoeRange, effect.value[0], ability.launchDelay));
			}
			if(effect.type==_AbilityEffects.AOEDotUnit){
				Dot dot=new Dot();
				dot.duration=effect.value[0];
				dot.interval=effect.value[1];
				dot.modifier1=effect.value[2];
				StartCoroutine(AOEDotUnit(pos, ability.aoeRange, dot, ability.launchDelay));
			}
			if(effect.type==_AbilityEffects.AOEDotArea){
				Dot dot=new Dot();
				dot.duration=effect.value[0];
				dot.interval=effect.value[1];
				dot.modifier1=effect.value[2];
				StartCoroutine(AOEDotArea(pos, ability.aoeRange, dot, ability.launchDelay));
			}
			if(effect.type==_AbilityEffects.AOEArmorReduction){
				//~ StartCoroutine(AOEArmorReduction(ability.launchDelay));
			}
		
			//~ if(effect.type==_AbilityEffects.SingleDamage){
				//~ UnitCreep creep=GetCreep(pos);
				//~ if(creep==null) return -1;
				//~ creep.ApplyDamage(effect.value[0], 0);
			//~ }
			//~ if(effect.type==_AbilityEffects.SingleSlow){
				//~ UnitCreep creep=GetCreep(pos);
				//~ if(creep==null) return -1;
				
				//~ Slow slow=new Slow();
				//~ slow.duration=effect.value[0];
				//~ slow.slowFactor=effect.value[1];
				//~ creep.ApplySlow(slow);
			//~ }
			//~ if(effect.type==_AbilityEffects.SingleStun){
				//~ UnitCreep creep=GetCreep(pos);
				//~ if(creep==null) return -1;
				//~ creep.ApplyStun(effect.value[0]);
			//~ }
			//~ if(effect.type==_AbilityEffects.SingleDot){
				//~ UnitCreep creep=GetCreep(pos);
				//~ if(creep==null) return -1;
				
				//~ Dot dot=new Dot();
				//~ dot.duration=effect.value[1];
				//~ dot.duration=effect.value[2];
				//~ dot.modifier1=effect.value[3];
				//~ creep.ApplyDot(dot, 0);
			//~ }
		
			if(effect.type==_AbilityEffects.AOEBoost){
				BuffStat buff=new BuffStat();
				buff.damageBuff=effect.value[1];
				buff.cooldownBuff=effect.value[2];
				buff.rangeBuff=effect.value[3];
				buff.regenHP=effect.value[4];
				
				StartCoroutine(AOEBuff(pos, ability.aoeRange, effect.value[0], buff, effect.modType, ability.launchDelay));
			}
			if(effect.type==_AbilityEffects.AOERepair){
				StartCoroutine(AOERepair(pos, ability.aoeRange, effect.value[0], ability.launchDelay));
			}
			else if(effect.type==_AbilityEffects.AOEShield){
				StartCoroutine(AOEShield(pos, ability.aoeRange, effect.value[0], ability.launchDelay));
			}
		}
		
		selectedAbilityID=-1;
		
		return 0;
	}
	
	UnitCreep GetCreep(Vector3 pointer){
		LayerMask mask=1<<LayerManager.LayerCreep() | 1<<LayerManager.LayerCreepF();
		
		Ray ray = Camera.main.ScreenPointToRay(pointer);
		RaycastHit hit;
		if(!Physics.Raycast(ray, out hit, Mathf.Infinity, mask)){
			return null;
		}
		
		UnitCreep creep=hit.transform.gameObject.GetComponent<UnitCreep>();
		
		return creep;
	}
	
	
	
	IEnumerator AOEDamage(Vector3 pos, float range, float damage, int dmgType, float delay){
		yield return new WaitForSeconds(delay);
		LayerMask mask=1<<LayerManager.LayerCreep() | 1<<LayerManager.LayerCreepF();
		Collider[] cols=Physics.OverlapSphere(pos, range, mask);
		foreach(Collider col in cols){
			Unit unit=col.gameObject.GetComponent<Unit>();
			unit.ApplyDamage(damage, dmgType);
		}
	}
	
	IEnumerator AOESlow(Vector3 pos, float range, Slow slow, float delay){
		yield return new WaitForSeconds(delay);
		LayerMask mask=1<<LayerManager.LayerCreep() | 1<<LayerManager.LayerCreepF();
		Collider[] cols=Physics.OverlapSphere(pos, range, mask);
		foreach(Collider col in cols){
			Unit unit=col.gameObject.GetComponent<Unit>();
			unit.ApplySlow(slow);
		}
	}
	
	IEnumerator AOEStun(Vector3 pos, float range, float duration, float delay){
		yield return new WaitForSeconds(delay);
		LayerMask maskTower=1<<LayerManager.LayerCreep() | 1<<LayerManager.LayerCreepF();
		Collider[] cols=Physics.OverlapSphere(pos, range, maskTower);
		foreach(Collider col in cols){
			Unit unit=col.gameObject.GetComponent<Unit>();
			unit.ApplyStun(duration);
		}
	}
	
	IEnumerator AOEDotUnit(Vector3 pos, float range, Dot dot, float delay){
		yield return new WaitForSeconds(delay);
		LayerMask maskTower=1<<LayerManager.LayerCreep() | 1<<LayerManager.LayerCreepF();
		Collider[] cols=Physics.OverlapSphere(pos, range, maskTower);
		foreach(Collider col in cols){
			Unit unit=col.gameObject.GetComponent<Unit>();
			unit.ApplyDot(dot, 0);
		}
	}
	
	IEnumerator AOEDotArea(Vector3 pos, float range, Dot dot, float delay){
		yield return new WaitForSeconds(delay);
		LayerMask maskTower=1<<LayerManager.LayerCreep() | 1<<LayerManager.LayerCreepF();
		float timeStart=Time.time;
		while(Time.time-timeStart<dot.duration){
			Collider[] cols=Physics.OverlapSphere(pos, range, maskTower);
			foreach(Collider col in cols){
				Unit unit=col.gameObject.GetComponent<Unit>();
				unit.ApplyDamage(dot.modifier1, 0);
			}
			yield return new WaitForSeconds(dot.interval);
		}
	}
	
	IEnumerator AOEArmorReduction(float duration){
		yield return new WaitForSeconds(duration);
	}
	
	void SingleDamage(UnitCreep unit, float dmg){
		unit.ApplyDamage(dmg, 0);
	}
	
	void SingleSlow(UnitCreep unit, Slow slow){
		unit.ApplySlow(slow);
	}
	
	void SingleStun(UnitCreep unit, float duration){
		unit.ApplyStun(duration);
	}
	
	void SingleDot(UnitCreep unit, Dot dot){
		unit.ApplyDot(dot, 0);
	}
	
	IEnumerator AOEBuff(Vector3 pos, float range, float duration, BuffStat buff, _ModifierType modType, float delay){
		yield return new WaitForSeconds(delay);
		
		LayerMask maskTower=1<<LayerManager.LayerTower();
			
		Collider[] cols=Physics.OverlapSphere(pos, range, maskTower);
		
		foreach(Collider col in cols){
			UnitTower unitTower=col.gameObject.GetComponent<UnitTower>();
			if(unitTower!=null){
				if(modType==_ModifierType.percentage) unitTower.AbilityBuffMod(buff, duration);
				else if(modType==_ModifierType.value)	unitTower.AbilityBuffVal(buff, duration);
			}
		}
			//~ List<UnitTower> tempBuffList = new List<UnitTower>(buffList.Length);
			//~ tempBuffList.AddRange(buffList);
	}
	
	IEnumerator AOERepair(Vector3 pos, float range, float val, float delay){
		yield return new WaitForSeconds(delay);
		LayerMask maskTower=1<<LayerManager.LayerTower();
		Collider[] cols=Physics.OverlapSphere(pos, range, maskTower);
		foreach(Collider col in cols){
			Unit unit=col.gameObject.GetComponent<Unit>();
			unit.RestoreHP(val);
		}
	}
	
	IEnumerator AOEShield(Vector3 pos, float range, float val, float delay){
		yield return new WaitForSeconds(delay);
		LayerMask maskTower=1<<LayerManager.LayerTower();
		Collider[] cols=Physics.OverlapSphere(pos, range, maskTower);
		foreach(Collider col in cols){
			Unit unit=col.gameObject.GetComponent<Unit>();
			unit.ShieldUp(val);
			Debug.DrawLine(unit.thisT.position, unit.thisT.position+new Vector3(0, 3, 0), Color.red, 3);
		}
	}
	
	public static float GetMaximumEnergy(){
		return abilityManager != null ? abilityManager.energyMax : 0;
	}
	
	public static float GetCurrentEnergy(){
		return abilityManager != null ? abilityManager.energy : 0;
	}
	
	public static Texture GetEnergyIcon(){
		return energyInfo.icon;
	}
	
	public static string GetEnergyName(){
		return energyInfo.name;
	}
	
	public static List<Ability> GetAllAbilityList() {
  		return abilityManager != null ? abilityManager.allAbilityList : null;
	}
	
	public static Ability GetMatchingAbility(int ID){
		foreach(Ability ability in abilityManager.allAbilityList){
			if(ability.ID==ID) return ability;
		}
		return null;
	}
}


public enum _AbilityEffects{
	AOEDamage,
	AOESlow,
	AOEStun,
	AOEDotUnit,
	AOEDotArea,
	AOEArmorReduction,
	
	//~ SingleDamage,
	//~ SingleSlow,
	//~ SingleStun,
	//~ SingleDot,
	
	AOEBoost,
	AOERepair,
	AOEShield
}

public enum _AbilityTargetType{
	AOEHostile,
	AOEFriendly,
	SingleHostile,
	SingleFriendly,
}

[System.Serializable]
public class Ability{
	public int ID;
	public string name;
	public string desp;
	public List<AbilityEffect> effects=new List<AbilityEffect>();
	
	public Texture icon;
	public Texture iconUnavailable;
	public Transform indicator;
	public bool autoScaleIndicator=true;
	public GameObject visualEffect;
	
	public int maxUseCount=0;
	public int usedCount=0;
	public float energy=25;
	public int[] costs=new int[0];
	
	public float cooldown;
	public float cdDuration;
	public float aoeRange;
	public float launchDelay;
	
	//~ public _AbilityTargetType targetType;
	
	//~ public _ModifierType modTypeVal;
	//~ public float[] value=new float[1];
	
	public bool enableInlvl=true;
	
	
	
	
	
	//~ public int valueCount=1;
//	public bool enableObj=false;
//	public bool enableRsc=false;
	public bool enableModTypeVal=true;
//	public bool enableModTypeRsc=false;
	
	
	public Ability Clone(){
		Ability abil=new Ability();
		abil.ID=ID;
		abil.name=name;
		abil.desp=desp;
		//~ abil.effects=effects;
		
		abil.effects=new List<AbilityEffect>();
		foreach(AbilityEffect effect in effects){
			abil.effects.Add(effect.Clone());
		}
		
		abil.icon=icon;
		abil.iconUnavailable=iconUnavailable;
		abil.indicator=indicator;
		abil.visualEffect=visualEffect;
		
		abil.maxUseCount=maxUseCount;
		abil.energy=energy;
		abil.costs=costs;
		
		abil.cooldown=cooldown;
		abil.cdDuration=cdDuration;
		abil.aoeRange=aoeRange;
		abil.launchDelay=launchDelay;
		
		abil.enableInlvl=enableInlvl;
		abil.enableModTypeVal=enableModTypeVal;
		
		return abil;
	}
	
	public int IsReady(){
		return IsReady(AbilityManager.GetCurrentEnergy());
	}
	public int IsReady(float currentEnergy){
		if(cooldown>0) return 1; 
		if(currentEnergy<=energy) return 2;
		if(costs!=null && !GameControl.HaveSufficientResource(costs)) return 3;
		return 0;
	}
	
	public void UpdateCostListLength(int length){
		int[] tempCosts=costs;
		costs=new int[length];
		
		for(int i=0; i<length; i++){
			if(i>=tempCosts.Length){
				costs[i]=0;
			}
			else{
				costs[i]=tempCosts[i];
			}
		}
	}
	
	public void RemoveEffect(int i){
		effects.RemoveAt(i);
	}
	
//	public void SetType(_AbilityEffects abilType){
//		
//	}
}

[System.Serializable]
public class AbilityEffect{
	public _AbilityEffects type;
	public float[] value=new float[2];
	
	public int valueCount=1;
	public bool enableModTypeVal=true;
	public _ModifierType modType;
	
	public AbilityEffect Clone(){
		AbilityEffect effect=new AbilityEffect();
		
		effect.type=type;
		
		effect.value=new float[value.Length];
		for(int i=0; i<value.Length; i++){
			effect.value[i]=value[i];
		}
		
		effect.valueCount=valueCount;
		effect.enableModTypeVal=enableModTypeVal;
		effect.modType=modType;
		
		return effect;
	}
	
	public AbilityEffect(){
		SetType(type);
	}
	
	public void SetType(_AbilityEffects abilType){
		type=abilType;
		//Debug.Log(type);
		
		if(type==_AbilityEffects.AOEDamage){
			valueCount=1;
			enableModTypeVal=true;
		}
		else if(type==_AbilityEffects.AOESlow){
			valueCount=2;
			enableModTypeVal=true;
		}
		else if(type==_AbilityEffects.AOEStun){
			valueCount=1;
			enableModTypeVal=true;
		}
		else if(type==_AbilityEffects.AOEDotUnit){
			valueCount=3;
			enableModTypeVal=true;
		}
		else if(type==_AbilityEffects.AOEDotArea){
			valueCount=3;
			enableModTypeVal=true;
		}
		else if(type==_AbilityEffects.AOEArmorReduction){
			valueCount=2;
			enableModTypeVal=true;
		}
	
		//~ else if(type==_AbilityEffects.SingleDamage){
			//~ valueCount=1;
			//~ enableModTypeVal=true;
		//~ }
		//~ else if(type==_AbilityEffects.SingleSlow){
			//~ valueCount=2;
			//~ enableModTypeVal=true;
		//~ }
		//~ else if(type==_AbilityEffects.SingleStun){
			//~ valueCount=1;
			//~ enableModTypeVal=true;
		//~ }
		//~ else if(type==_AbilityEffects.SingleDot){
			//~ valueCount=3;
			//~ enableModTypeVal=true;
		//~ }
		
		else if(type==_AbilityEffects.AOEBoost){
			valueCount=5;
			enableModTypeVal=false;
		}
		else if(type==_AbilityEffects.AOERepair){
			valueCount=1;
			enableModTypeVal=false;
		}
		else if(type==_AbilityEffects.AOEShield){
			valueCount=1;
			enableModTypeVal=false;
		}
		
//		else if(type==_AbilityEffects.AOEBoostAll){
//			valueCount=2;
//			enableModTypeVal=true;
//		}
//		else if(type==_AbilityEffects.AOEBoostSpeed){
//			valueCount=2;
//			enableModTypeVal=true;
//		}
//		else if(type==_AbilityEffects.AOEBoostRange){
//			valueCount=2;
//			enableModTypeVal=true;
//		}
//		else if(type==_AbilityEffects.AOEBoostDamage){
//			valueCount=2;
//			enableModTypeVal=true;
//		}

		MatchValueToCount();
	}
	
	void MatchValueToCount(){
		float[] temp=new float[valueCount];
		
		for(int i=0; i<temp.Length; i++){
			if(i<value.Length) temp[i]=value[i];
			else temp[i]=0;
		}
		
		value=temp;
	}
}


