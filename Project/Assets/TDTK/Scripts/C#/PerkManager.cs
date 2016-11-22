using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;
using System.Xml;
using System.IO;

//for specific tower perk, storing towerID and their corresponding value
public class TowerValue{
	public int ID=-1;
	//~ public float[] value=new float[0];
	//~ public float[] rsc=new float[0];
	
	public float[] mod=new float[1];
	public float[] val=new float[1];
	
	public float[] rscMod=new float[1];
	public float[] rscVal=new float[1];
	
	public TowerValue(int id){
		ID=id;
	}
	public TowerValue(int id, int num){
		ID=id;
		rscMod=new float[num];
		rscVal=new float[num];
	}
}

public class PerkManager : MonoBehaviour {

	public delegate void PerkUnlockedHandler(string text); 
	public static event PerkUnlockedHandler onPerkUnlockedE;
	
	
	public delegate void NewTowerHandler(Perk perk);
	public static event NewTowerHandler onNewTowerE;
	
	public delegate void BuffAllTowerHPHandler(Perk perk);
	public static event BuffAllTowerHPHandler onBuffAllTowerHPE;
	
	
	//~ public delegate void BonusResourceHandler(Perk perk);
	//~ public static event BonusResourceHandler onBonusResourceE; 
	
	//~ public delegate void BonusLifeHandler(Perk perk);
	//~ public static event BonusLifeHandler onBonusLifeE;  
	
	
	public delegate void RegenLifeHandler(int value);
	public static event RegenLifeHandler onRegenLifeE;  
	
	public delegate void RegenResourceHandler(int[] value);
	public static event RegenResourceHandler onRegenResourceE;  
	
	public delegate void LifeBonusWaveClearedHandler(int value);
	public static event LifeBonusWaveClearedHandler onLifeBonusWaveClearedE;  
	
	
	//on specific tower
	public delegate void BuffTowerHPHandler(Perk perk);
	public static event BuffTowerHPHandler onBuffTowerHPE;
	
	public delegate void BuffTowerAttHandler(Perk perk);
	public static event BuffTowerAttHandler onBuffTowerAttE;
	
	public delegate void BuffTowerDefHandler(Perk perk);
	public static event BuffTowerDefHandler onBuffTowerDefE;
	
	public delegate void TowerBuffRangeHandler(Perk perk);
	public static event TowerBuffRangeHandler onBuffTowerRangeE;
	
	public delegate void TowerCriticalHandler(Perk perk);
	public static event TowerCriticalHandler onBuffTowerCritE;
	
	public delegate void TowerCriticalChanceHandler(Perk perk);
	public static event TowerCriticalChanceHandler onBuffTowerCritChanceE;
	
	public delegate void TowerCriticalDamageHandler(Perk perk);
	public static event TowerCriticalDamageHandler onBuffTowerCritDamageE;
	
	public delegate void UpgradeCostReductionHandler(Perk perk);
	public static event UpgradeCostReductionHandler onUpgradeCostReducE;
	
	public delegate void BuildCostReductionHandler(Perk perk);
	public static event BuildCostReductionHandler onBuildCostReducE;
	
	public delegate void TowerStunHandler(Perk perk);
	public static event TowerStunHandler onStunE;
	
	public delegate void TowerSlowHandler(Perk perk);
	public static event TowerSlowHandler onSlowE;
	
	
	
	public delegate void NewAbilityHandler(Perk perk);
	public static event NewAbilityHandler onNewAbilityE;
	
	
	
	//*******************************************************
	//all towers modifier
	public static int allTowerLevelBonus=0;
	
	public static float allTowerHPBuffModifier=0;
	public static float allTowerHPBuffValue=0;
	
	public static float allTowerAttackBuffModifier=0;
	public static float allTowerAttackBuffValue=0;
	
	public static float allTowerDefBuffModifier=0;
	public static float allTowerDefBuffValue=0;
	
	public static float allTowerCriticalChance=0;
	public static float allTowerCriticalDmgValue=0;
	public static float allTowerCriticalDmgModifier=0;
	
	public static float[] allCostReductionModifier=new float[0];
	public static int[] allCostReductionValue=new int[0];
	
	public static float[] buildCostReductionModifier=new float[0];
	public static int[] buildCostReductionValue=new int[0];
	
	public static float[] upgradeCostReductionModifier=new float[0];
	public static int[]  upgradeCostReductionValue=new int[0];
	
	
	//*******************************************************
	//life modifier
	public static float lifeBonusWaveClearChance=0;
	public static float lifeBonusWaveClearMin=0;
	public static float lifeBonusWaveClearMax=0;
	
	
	//*******************************************************
	//resource modifier
	public static float rscGainChance=0;
	public static float[] rscGainModifier=new float[0];
	public static int[] rscGainValue=new int[0];
	
	public static float rscGainCreepChance=0;
	public static float[] rscGainCreepModifier=new float[0];
	public static int[] rscGainCreepValue=new int[0];
	
	public static float rscGainWaveChance=0;
	public static float[] rscGainWaveModifier=new float[0];
	public static int[] rscGainWaveValue=new int[0];
	
	public static float rscGainTowerChance=0;
	public static float[] rscGainTowerModifier=new float[0];
	public static int[] rscGainTowerValue=new int[0];
	
	
	//*******************************************************
	//specific tower modifier list
	public static List<TowerValue> towerLevelBonusList=new List<TowerValue>();
	public static List<TowerValue> towerBuffHPList=new List<TowerValue>();
	public static List<TowerValue> towerBuffAttackList=new List<TowerValue>();
	public static List<TowerValue> towerBuffDefenceList=new List<TowerValue>();
	public static List<TowerValue> towerBuffRangeList=new List<TowerValue>();
	public static List<TowerValue> towerCriticalChanceList=new List<TowerValue>();
	public static List<TowerValue> towerCriticalDamageList=new List<TowerValue>();
	public static List<TowerValue> towerBuildCostReducList=new List<TowerValue>();
	public static List<TowerValue> towerUpgradeCostReducList=new List<TowerValue>();
	public static List<TowerValue> towerStunList=new List<TowerValue>();
	public static List<TowerValue> towerSlowList=new List<TowerValue>();
	
	/*
	public static List<TowerValue> towerLevelBonusList=new List<TowerValue>();
	public static List<TowerValue> towerCriticalChanceList=new List<TowerValue>();
	
	public static List<TowerValue> buildCostReducValueList=new List<TowerValue>();
	public static List<TowerValue> upgradeCostReducValueList=new List<TowerValue>();
	public static List<TowerValue> buffTowerHPValueList=new List<TowerValue>();
	public static List<TowerValue> buffTowerAttackValueList=new List<TowerValue>();
	public static List<TowerValue> buffTowerDefValueList=new List<TowerValue>();
	public static List<TowerValue> towerCriticalDamageValueList=new List<TowerValue>();
	public static List<TowerValue> towerRangeValList=new List<TowerValue>();
	
	public static List<TowerValue> buildCostReducModList=new List<TowerValue>();
	public static List<TowerValue> upgradeCostReducModList=new List<TowerValue>();
	public static List<TowerValue> buffTowerHPModList=new List<TowerValue>();
	public static List<TowerValue> buffTowerAttackModList=new List<TowerValue>();
	public static List<TowerValue> buffTowerDefModList=new List<TowerValue>();
	public static List<TowerValue> towerCriticalDamageModList=new List<TowerValue>();
	public static List<TowerValue> towerRangeModList=new List<TowerValue>();
	*/
	
	
	public List<Perk> allPerkList=new List<Perk>();
	
	public static int perkPoint=0;
	
	public static PerkManager perkManager;
	
	
	void Awake(){
		InitResourceModifier();
	}
	
	// Use this for initialization
	void Start() {
		perkManager=this;
		
		perkPoint=0;
		
		InitTowerModifierList();
		
		VerifyPerk();
		
		for(int ID=0; ID<allPerkList.Count; ID++){
			if(allPerkList[ID].enableInlvl && allPerkList[ID].unlocked){
				//Debug.Log("unlocked  "+allPerkList[ID].name);
				UnlockPerk(allPerkList[ID]);
			}
		}
	}
	
	
	void OnEnable(){
		SpawnManager.onWaveClearedE += OnWaveCleared;
	}
	
	void OnDisable(){
		SpawnManager.onWaveClearedE -= OnWaveCleared;
	}
	
	
	public static void InitResourceModifier(){
		int num=GameControl.GetResourceCount();
		
		allCostReductionModifier=new float[num];
		allCostReductionValue=new int[num];
		
		buildCostReductionModifier=new float[num];
		buildCostReductionValue=new int[num];
		
		upgradeCostReductionModifier=new float[num];
		upgradeCostReductionValue=new int[num];
		
		rscGainModifier=new float[num];
		rscGainValue=new int[num];
		
		rscGainCreepModifier=new float[num];
		rscGainCreepValue=new int[num];
		
		rscGainWaveModifier=new float[num];
		rscGainWaveValue=new int[num];
		
		rscGainTowerModifier=new float[num];
		rscGainTowerValue=new int[num];
		
	}
	
	
	void InitTowerModifierList(){
		GameObject obj=Resources.Load("PrefabListTower", typeof(GameObject)) as GameObject;
		TowerListPrefab prefab=obj.GetComponent<TowerListPrefab>();
		List<UnitTower> towerList=prefab.towerList;
		
		int rscCount=GameControl.GetResourceCount();
		
		foreach(UnitTower tower in towerList){
			/*
			towerLevelBonusList.Add(new TowerValue(tower.prefabID));
			towerCriticalChanceList.Add(new TowerValue(tower.prefabID));
			
			buildCostReducValueList.Add(new TowerValue(tower.prefabID, rscCount));
			upgradeCostReducValueList.Add(new TowerValue(tower.prefabID, rscCount));
			buffTowerHPValueList.Add(new TowerValue(tower.prefabID));
			buffTowerAttackValueList.Add(new TowerValue(tower.prefabID));
			buffTowerDefValueList.Add(new TowerValue(tower.prefabID));
			towerCriticalDamageValueList.Add(new TowerValue(tower.prefabID));
			towerRangeValList.Add(new TowerValue(tower.prefabID));
			
			buildCostReducModList.Add(new TowerValue(tower.prefabID, rscCount));
			upgradeCostReducModList.Add(new TowerValue(tower.prefabID, rscCount));
			buffTowerHPModList.Add(new TowerValue(tower.prefabID));
			buffTowerAttackModList.Add(new TowerValue(tower.prefabID));
			buffTowerDefModList.Add(new TowerValue(tower.prefabID));
			towerCriticalDamageModList.Add(new TowerValue(tower.prefabID));
			towerRangeModList.Add(new TowerValue(tower.prefabID));
			*/
			
			towerLevelBonusList.Add(new TowerValue(tower.prefabID));
			towerBuffHPList.Add(new TowerValue(tower.prefabID));
			towerBuffAttackList.Add(new TowerValue(tower.prefabID));
			towerBuffDefenceList.Add(new TowerValue(tower.prefabID));
			towerBuffRangeList.Add(new TowerValue(tower.prefabID));
			towerCriticalChanceList.Add(new TowerValue(tower.prefabID));
			towerCriticalDamageList.Add(new TowerValue(tower.prefabID));
			towerBuildCostReducList.Add(new TowerValue(tower.prefabID, rscCount));
			towerUpgradeCostReducList.Add(new TowerValue(tower.prefabID, rscCount));
			towerStunList.Add(new TowerValue(tower.prefabID));
			towerSlowList.Add(new TowerValue(tower.prefabID));
		}
	}
	
	void VerifyPerk(){
		List<Perk> tempList=Load();
		
		for(int i=0; i<tempList.Count; i++){
			Perk perk=tempList[i];
			
			foreach(Perk p in allPerkList){
				if(perk.iconUnlocked==null) perk.iconUnlocked=perk.icon;
				if(perk.iconUnavailable==null) perk.iconUnlocked=perk.icon;
				
				if(perk.ID==p.ID){
					perk.enableInlvl=p.enableInlvl;
					perk.unlocked=p.unlocked;
					//~ break;
				}
			}
		}
		
		allPerkList=tempList;
		
		int totalRscCount=GameControl.GetResourceCount();
		foreach(Perk perk in allPerkList){
			if(perk.costs.Length!=totalRscCount){
				perk.UpdateCostListLength(totalRscCount);
			}
		}
	}
	
	
	public static List<Perk> GetAllPerks(){
		if(perkManager!=null) return perkManager.allPerkList;
		return null;
	}
	
	public static int GetFirstAvailabelID(){
		if(perkManager==null) return -1;
		
		for(int i=0; i<perkManager.allPerkList.Count; i++){
			if(perkManager.allPerkList[i].enableInlvl) return i;
		}
		
		return -1;
	}
	
	public static int GetTotalPerkCount(){
		return perkManager.allPerkList.Count;
	}
	
	public static Perk GetPerk(int ID){
		return perkManager.allPerkList[ID];
	}
	
	
	//******************************************************************************************************
	//get specific tower modifier
	public static int GetTowerLevelBonus(int prefabID){
		foreach(TowerValue val in towerLevelBonusList){
			if(val.ID==prefabID) return (int)val.val[0];
		}
		return 0;
	}
	
	
	//specific tower HP buff
	public static float GetTowerBuffHPVal(int prefabID){
		foreach(TowerValue towerVal in towerBuffHPList){
			if(towerVal.ID==prefabID) return towerVal.val[0];
		}
		return 0;
	}
	public static float GetTowerBuffHPMod(int prefabID){
		foreach(TowerValue towerVal in towerBuffHPList){
			if(towerVal.ID==prefabID) return towerVal.mod[0];
		}
		return 0;
	}
	
	
	//specific tower attack buff
	public static float GetTowerBuffAttVal(int prefabID){
		foreach(TowerValue towerVal in towerBuffAttackList){
			if(towerVal.ID==prefabID) return towerVal.val[0];
		}
		return 0;
	}
	public static float GetTowerBuffAttMod(int prefabID){
		foreach(TowerValue towerVal in towerBuffAttackList){
			if(towerVal.ID==prefabID) return towerVal.mod[0];
		}
		return 0;
	}
	
	
	//specific tower defense buff
	public static float GetTowerBuffDefVal(int prefabID){
		foreach(TowerValue towerVal in towerBuffDefenceList){
			if(towerVal.ID==prefabID) return towerVal.val[0];
		}
		return 0;
	}
	public static float GetTowerBuffDefMod(int prefabID){
		foreach(TowerValue towerVal in towerBuffDefenceList){
			if(towerVal.ID==prefabID) return towerVal.mod[0];
		}
		return 0;
	}
	
	
	//specific tower range buff
	public static float GetTowerRangeExVal(int prefabID){
		foreach(TowerValue towerVal in towerBuffRangeList){
			if(towerVal.ID==prefabID) return towerVal.val[0];
		}
		return 0;
	}
	public static float GetTowerRangeExMod(int prefabID){
		foreach(TowerValue towerVal in towerBuffRangeList){
			if(towerVal.ID==prefabID) return towerVal.mod[0];
		}
		return 0;
	}
	
	
	//specific tower critical
	public static float GetTowerCritChance(int prefabID){
		foreach(TowerValue towerVal in towerCriticalChanceList){
			if(towerVal.ID==prefabID) return towerVal.val[0];
		}
		return 0;
	}
	public static float GetTowerCritDamageVal(int prefabID){
		foreach(TowerValue towerVal in towerCriticalDamageList){
			if(towerVal.ID==prefabID) return towerVal.val[0];
		}
		return 0;
	}
	public static float GetTowerCritDamageMod(int prefabID){
		foreach(TowerValue towerVal in towerCriticalDamageList){
			if(towerVal.ID==prefabID) return towerVal.mod[0];
		}
		return 0;
	}
	
	
	//specific tower build cost
	public static float[] GetTowerBuildCostReducVal(int prefabID){
		foreach(TowerValue towerVal in towerBuildCostReducList){
			if(towerVal.ID==prefabID) return towerVal.rscVal;
		}
		return new float[0];
	}
	public static float[] GetTowerBuildCostReducMod(int prefabID){
		foreach(TowerValue towerVal in towerBuildCostReducList){
			if(towerVal.ID==prefabID) return towerVal.rscMod;
		}
		return new float[0];
	}
	
	//specific tower upgrade cost
	public static float[] GetTowerUpgradeCostReducVal(int prefabID){
		foreach(TowerValue towerVal in towerUpgradeCostReducList){
			if(towerVal.ID==prefabID) return towerVal.rscVal;
		}
		return new float[0];
	}
	public static float[] GetTowerUpgradeCostReducMod(int prefabID){
		foreach(TowerValue towerVal in towerUpgradeCostReducList){
			if(towerVal.ID==prefabID) return towerVal.rscMod;
		}
		return new float[0];
	}
	
	
	//specific tower special ability
	public static float[] GetTowerStun(int prefabID){
		foreach(TowerValue towerVal in towerStunList){
			if(towerVal.ID==prefabID) return towerVal.val;
		}
		return new float[0];
	}
	public static float[] GetTowerSlow(int prefabID){
		foreach(TowerValue towerVal in towerSlowList){
			if(towerVal.ID==prefabID) return towerVal.val;
		}
		return new float[0];
	}
	
	
	
	//******************************************************************************************************
	//perk routine
	IEnumerator LifeRegen(Perk perk){
		while(true){
			yield return new WaitForSeconds(perk.value[2]);
			int value=(int)UnityEngine.Random.Range(perk.value[0], perk.value[1]);
			if(value>0){
				GameControl.OnGainLife(value);
				if(onRegenLifeE!=null) onRegenLifeE(value);
			}
		}
	}
	
	IEnumerator ResourceGeneration(Perk perk){
		while(true){
			yield return new WaitForSeconds(UnityEngine.Random.Range(perk.value[0], perk.value[1]));
			
			int[] rscGain=new int[perk.rsc.Length];
			for(int i=0; i<rscGain.Length; i++){
				rscGain[i]=(int)Mathf.Round(perk.rsc[i]);
			}
			GameControl.GainResource(rscGain);
			if(onRegenResourceE!=null) onRegenResourceE(rscGain);
		}
	}
	
	void OnWaveCleared(int waveID){
		if(lifeBonusWaveClearChance>0){
			if(UnityEngine.Random.Range(0f, 1f)<lifeBonusWaveClearChance){
				int val=(int)UnityEngine.Random.Range(lifeBonusWaveClearMin, lifeBonusWaveClearMax+1);
				if(val>0){
					GameControl.OnGainLife(val);
					if(onLifeBonusWaveClearedE!=null) onLifeBonusWaveClearedE(val);
				}
			}
		}
	}
	
	
	
	//******************************************************************************************************
	//unlock perk
	public static bool Unlock(int ID){
		Perk perk=perkManager.allPerkList[ID];
		if(perk.IsAvailable()==0 && perk.HaveSufficientResource()){
			//~ GameControl.SpendResource(perk.costs);
			UnlockPerk(perk);
			perk.Unlock();
			if(onPerkUnlockedE!=null) onPerkUnlockedE(perk.name);
			return true;
		}
		
		return false;
	}
	
	public static void UnlockPerk(Perk perk){
		perkPoint+=1;
		
		
		//***************************************************************************
		//all tower section
		if(perk.type==_PerkType.TowerUnlockNew){
			if(BuildManager.AddTower(perk.towerID)){
				if(onNewTowerE!=null) onNewTowerE(perk);
			}
		}
		else if(perk.type==_PerkType.TowerBuildLevelBonus){
			allTowerLevelBonus+=(int)Mathf.Round(perk.value[0]);
		}
		
		
		//all tower buff
		else if(perk.type==_PerkType.TowerBuffHP){
			if(perk.modTypeVal==_ModifierType.percentage){
				allTowerHPBuffModifier+=perk.value[0];
			}
			else if(perk.modTypeVal==_ModifierType.value){
				allTowerHPBuffValue+=(int)Mathf.Round(perk.value[0]);
			}
			if(onBuffAllTowerHPE!=null) onBuffAllTowerHPE(perk);
		}
		else if(perk.type==_PerkType.TowerBuffAttack){
			if(perk.modTypeVal==_ModifierType.percentage){
				allTowerAttackBuffModifier+=perk.value[0];
			}
			else if(perk.modTypeVal==_ModifierType.value){
				allTowerAttackBuffValue+=(int)Mathf.Round(perk.value[0]);
			}
		}
		else if(perk.type==_PerkType.TowerBuffDef){
			if(perk.modTypeVal==_ModifierType.percentage){
				allTowerDefBuffModifier=Mathf.Max(0, allTowerDefBuffModifier+=perk.value[0]);
			}
			else if(perk.modTypeVal==_ModifierType.value){
				allTowerDefBuffValue+=Mathf.Round(perk.value[0]);
			}
		}
		
		
		//all tower critical
		else if(perk.type==_PerkType.TowerCritical){
			allTowerCriticalChance+=perk.value[0];
			allTowerCriticalDmgModifier+=perk.value[1];
		}
		else if(perk.type==_PerkType.TowerCriticalChance){
			allTowerCriticalChance+=perk.value[0];
		}
		else if(perk.type==_PerkType.TowerCriticalDamage){
			if(perk.modTypeVal==_ModifierType.percentage){
				allTowerCriticalDmgModifier+=perk.value[0];
			}
			else if(perk.modTypeVal==_ModifierType.value){
				allTowerCriticalDmgValue+=Mathf.Round(perk.value[0]);
			}
		}
		
		
		//all tower cost reduction
		if(perk.type==_PerkType.TowerAllCostReduction){
			if(perk.modTypeRsc==_ModifierType.percentage){
				for(int i=0; i<perk.rsc.Length; i++){
					allCostReductionModifier[i]+=perk.rsc[i];
				}
			}
			else if(perk.modTypeRsc==_ModifierType.value){
				for(int i=0; i<perk.rsc.Length; i++){
					allCostReductionValue[i]+=(int)Mathf.Round(perk.rsc[i]);
					Debug.Log(allCostReductionValue[i]+"  "+perk.rsc[i]);
				}
				
			}
			
		}
		else if(perk.type==_PerkType.TowerBuildCostReduction){
			if(perk.modTypeRsc==_ModifierType.percentage){
				for(int i=0; i<perk.rsc.Length; i++){
					buildCostReductionModifier[i]+=perk.rsc[i];
				}
			}
			else if(perk.modTypeRsc==_ModifierType.value){
				for(int i=0; i<perk.rsc.Length; i++){
					buildCostReductionValue[i]+=(int)Mathf.Round(perk.rsc[i]);
				}
			}
		}
		else if(perk.type==_PerkType.TowerUpgradeCostReduction){
			if(perk.modTypeRsc==_ModifierType.percentage){
				for(int i=0; i<perk.rsc.Length; i++){
					upgradeCostReductionModifier[i]+=perk.rsc[i];
				}
			}
			else if(perk.modTypeRsc==_ModifierType.value){
				for(int i=0; i<perk.rsc.Length; i++){
					upgradeCostReductionValue[i]+=(int)Mathf.Round(perk.rsc[i]);
				}
			}
			
		}
		
		
		//***************************************************************************
		//life section
		else if(perk.type==_PerkType.LifeIncreaseCap){
			if(perk.modTypeVal==_ModifierType.percentage){
				GameControl.OnIncreaseLifeCap((int)(Mathf.Round(GameControl.GetPlayerLifeCap()*perk.value[0])));
			}
			else if(perk.modTypeVal==_ModifierType.value){
				GameControl.OnIncreaseLifeCap((int)Mathf.Round(perk.value[0]));
			}
		}
		else if(perk.type==_PerkType.LifeBonus){
			if(perk.modTypeVal==_ModifierType.percentage){
				GameControl.OnGainLife((int)(Mathf.Round(GameControl.GetPlayerLife()*perk.value[0])));
			}
			else if(perk.modTypeVal==_ModifierType.value){
				GameControl.OnGainLife((int)Mathf.Round(perk.value[0]));
			}
		}
		else if(perk.type==_PerkType.LifeBonusWaveCleared){
			lifeBonusWaveClearChance+=perk.value[0];
			lifeBonusWaveClearMin+=perk.value[0];
			lifeBonusWaveClearMax+=perk.value[0];
			lifeBonusWaveClearMin=Mathf.Min(lifeBonusWaveClearMin, lifeBonusWaveClearMax);
		}
		else if(perk.type==_PerkType.LifeRegen){
			perkManager.StartCoroutine(perkManager.LifeRegen(perk));
		}
		
	
		
		
		
		//***************************************************************************
		//resource section
		else if(perk.type==_PerkType.ResourceBonus){
			int[] rscGain=new int[perk.rsc.Length];
			for(int i=0; i<rscGain.Length; i++){
				rscGain[i]=(int)Mathf.Round(perk.rsc[i]);
			}
			GameControl.GainResource(rscGain);
		}
		else if(perk.type==_PerkType.ResourceGeneration){
			perkManager.StartCoroutine(perkManager.ResourceGeneration(perk));
		}
		else if(perk.type==_PerkType.ResourceGain){
			if(perk.modTypeRsc==_ModifierType.percentage){
				CopyListValueFloat(perk.rsc, rscGainModifier);
			}
			else if(perk.modTypeRsc==_ModifierType.value){
				CopyListValueInt(perk.rsc, rscGainValue);
			}
		}
		else if(perk.type==_PerkType.ResourceGainCreepKilled){
			if(perk.modTypeRsc==_ModifierType.percentage){
				CopyListValueFloat(perk.rsc, rscGainCreepModifier);
			}
			else if(perk.modTypeRsc==_ModifierType.value){
				CopyListValueInt(perk.rsc, rscGainCreepValue);
			}
		}
		else if(perk.type==_PerkType.ResourceGainWaveCleared){
			if(perk.modTypeRsc==_ModifierType.percentage){
				CopyListValueFloat(perk.rsc, rscGainWaveModifier);
			}
			else if(perk.modTypeRsc==_ModifierType.value){
				CopyListValueInt(perk.rsc, rscGainWaveValue);
			}
		}
		else if(perk.type==_PerkType.ResourceGainResourceTower){
			if(perk.modTypeRsc==_ModifierType.percentage){
				CopyListValueFloat(perk.rsc, rscGainTowerModifier);
			}
			else if(perk.modTypeRsc==_ModifierType.value){
				CopyListValueInt(perk.rsc, rscGainTowerValue);
			}
		}
		
		
		//tower specific
		else if(perk.type==_PerkType.SpecificTowerLevelBonus){
			for(int i=0; i<towerLevelBonusList.Count; i++){
				if(towerLevelBonusList[i].ID==perk.towerID){
					UpdateTowerValue(towerLevelBonusList[i], perk.value, _ModifierType.value);
					break;
				}
			}
		}
		
		
	
		
		else if(perk.type==_PerkType.SpecificTowerBuffHP){
			for(int i=0; i<towerBuffHPList.Count; i++){
				if(towerBuffHPList[i].ID==perk.towerID){
					UpdateTowerValue(towerBuffHPList[i], perk.value, perk.modTypeVal);
					break;
				}
			}
			if(onBuffAllTowerHPE!=null) onBuffTowerHPE(perk);
		}
		else if(perk.type==_PerkType.SpecificTowerBuffAttack){
			for(int i=0; i<towerBuffAttackList.Count; i++){
				if(towerBuffAttackList[i].ID==perk.towerID){
					UpdateTowerValue(towerBuffAttackList[i], perk.value, perk.modTypeVal);
					break;
				}
			}
			if(onBuffTowerAttE!=null) onBuffTowerAttE(perk);
		}
		else if(perk.type==_PerkType.SpecificTowerBuffDef){
			for(int i=0; i<towerBuffDefenceList.Count; i++){
				if(towerBuffDefenceList[i].ID==perk.towerID){
					UpdateTowerValue(towerBuffDefenceList[i], perk.value, perk.modTypeVal);
					break;
				}
			}
			if(onBuffTowerDefE!=null) onBuffTowerDefE(perk);
		}
		else if(perk.type==_PerkType.SpecificTowerBuffRange){
			for(int i=0; i<towerBuffRangeList.Count; i++){
				if(towerBuffRangeList[i].ID==perk.towerID){
					UpdateTowerValue(towerBuffRangeList[i], perk.value, perk.modTypeVal);
					break;
				}
			}
			if(onBuffTowerRangeE!=null) onBuffTowerRangeE(perk);
		}
		
		
		//specific tower critical
		else if(perk.type==_PerkType.SpecificTowerCritical){
			for(int i=0; i<towerCriticalChanceList.Count; i++){
				if(towerCriticalChanceList[i].ID==perk.towerID){
					UpdateTowerValue(towerCriticalChanceList[i], perk.value, _ModifierType.percentage);
					break;
				}
			}
			for(int i=0; i<towerCriticalDamageList.Count; i++){
				if(towerCriticalDamageList[i].ID==perk.towerID){
					UpdateTowerValue(towerCriticalDamageList[i], perk.value, perk.modTypeVal);
					break;
				}
			}
			if(onBuffTowerCritE!=null) onBuffTowerCritE(perk);
		}
		else if(perk.type==_PerkType.SpecificTowerCriticalChance){
			for(int i=0; i<towerCriticalChanceList.Count; i++){
				if(towerCriticalChanceList[i].ID==perk.towerID){
					UpdateTowerValue(towerCriticalChanceList[i], perk.value, _ModifierType.percentage);
					break;
				}
			}
			if(onBuffTowerCritChanceE!=null) onBuffTowerCritChanceE(perk);
		}
		else if(perk.type==_PerkType.SpecificTowerCriticalDamage){
			for(int i=0; i<towerCriticalDamageList.Count; i++){
				if(towerCriticalDamageList[i].ID==perk.towerID){
					UpdateTowerValue(towerCriticalDamageList[i], perk.value, perk.modTypeVal);
					break;
				}
			}
			if(onBuffTowerCritDamageE!=null) onBuffTowerCritDamageE(perk);
		}
		
		
		
		//specific tower cost 
		else if(perk.type==_PerkType.SpecificTowerAllCostReduction){
			for(int i=0; i<towerBuildCostReducList.Count; i++){
				if(towerBuildCostReducList[i].ID==perk.towerID){
					UpdateTowerValueResource(towerBuildCostReducList[i], perk.rsc, perk.modTypeRsc);
					Debug.Log(towerBuildCostReducList[i].val[0]);
					break;
				}
			}
			for(int i=0; i<towerUpgradeCostReducList.Count; i++){
				if(towerUpgradeCostReducList[i].ID==perk.towerID){
					UpdateTowerValueResource(towerUpgradeCostReducList[i], perk.rsc, perk.modTypeRsc);
					break;
				}
			}
		}
		else if(perk.type==_PerkType.SpecificTowerBuildCostReduction){
			for(int i=0; i<towerBuildCostReducList.Count; i++){
				if(towerBuildCostReducList[i].ID==perk.towerID){
					UpdateTowerValueResource(towerBuildCostReducList[i], perk.rsc, perk.modTypeRsc);
					break;
				}
			}
			if(onBuildCostReducE!=null) onBuildCostReducE(perk);
		}
		else if(perk.type==_PerkType.SpecificTowerUpgradeCostReduction){
			for(int i=0; i<towerUpgradeCostReducList.Count; i++){
				if(towerUpgradeCostReducList[i].ID==perk.towerID){
					UpdateTowerValueResource(towerUpgradeCostReducList[i], perk.rsc, perk.modTypeRsc);
					break;
				}
			}
			if(onUpgradeCostReducE!=null) onUpgradeCostReducE(perk);
		}
		
		
		
		
		//specific tower special
		else if(perk.type==_PerkType.SpecificTowerStun){
			for(int i=0; i<towerStunList.Count; i++){
				if(towerStunList[i].ID==perk.towerID){
					UpdateTowerValue(towerStunList[i], perk.value, _ModifierType.value);
					break;
				}
			}
			if(onStunE!=null) onStunE(perk);
		}
		else if(perk.type==_PerkType.SpecificTowerSlow){
			for(int i=0; i<towerSlowList.Count; i++){
				if(towerSlowList[i].ID==perk.towerID){
					UpdateTowerValue(towerSlowList[i], perk.value, _ModifierType.value);
					break;
				}
			}
			if(onSlowE!=null) onSlowE(perk);
		}
		
		
		else if(perk.type==_PerkType.EnergyRegenRate){
			if(perk.modTypeRsc==_ModifierType.percentage){
				AbilityManager.energyRateModifier+=perk.value[0];
			}
			else if(perk.modTypeRsc==_ModifierType.value){
				AbilityManager.energyRateValue+=perk.value[0];
			}
		}
		else if(perk.type==_PerkType.EnergyIncreaseCap){
			if(perk.modTypeRsc==_ModifierType.percentage){
				AbilityManager.energyCapModifier+=perk.value[0];
			}
			else if(perk.modTypeRsc==_ModifierType.value){
				AbilityManager.energyCapValue+=perk.value[0];
			}
			AbilityManager.UpdateEnergyCap();
		}
		else if(perk.type==_PerkType.EnergyCostReduction){
			if(perk.modTypeRsc==_ModifierType.percentage){
				AbilityManager.energyCostModifier+=perk.value[0];
			}
			else if(perk.modTypeRsc==_ModifierType.value){
				AbilityManager.energyCostValue+=perk.value[0];
			}
		}
		else if(perk.type==_PerkType.EnergyGainCreepKilled){
			AbilityManager.energyGainCreepKilledChance+=perk.value[1];
			if(perk.modTypeRsc==_ModifierType.percentage){
				AbilityManager.energyGainWaveClearedMod+=perk.value[0];
			}
			else if(perk.modTypeRsc==_ModifierType.value){
				AbilityManager.energyGainWaveClearedVal+=perk.value[0];
			}
		}
		else if(perk.type==_PerkType.EnergyGainWaveCleared){
			AbilityManager.energyGainWaveClearedChance+=perk.value[1];
			if(perk.modTypeRsc==_ModifierType.percentage){
				AbilityManager.energyGainCreepKilledMod+=perk.value[0];
			}
			else if(perk.modTypeRsc==_ModifierType.value){
				AbilityManager.energyGainCreepKilledVal+=perk.value[0];
			}
		}
		
		if(perk.type==_PerkType.AbilityUnlockNew){
			if(perk.enableAbilityS && perk.abilitySID>=0){
				if(AbilityManager.UpgradeAbility(perk.abilitySID, perk.abilityID)){
					if(onNewAbilityE!=null) onNewAbilityE(perk);
				}
			}
			else{
				if(AbilityManager.AddAbility(perk.abilityID)){
					if(onNewAbilityE!=null) onNewAbilityE(perk);
				}
			}
		}
		else if(perk.type==_PerkType.AbilityCost){
			AbilityManager.ReduceAbilityCost(perk);
		}
		else if(perk.type==_PerkType.AbililyCooldown){
			AbilityManager.ReduceAbilityCD(perk);
		}
		
		
		//if(!perk.repeatable) perk.unlocked=true;
	}
	
	
	/*
	void Update(){
		if(Input.GetMouseButtonDown(0)){
			TowerValue textObj=new TowerValue(5);
			float[] addition=new float[2];
			addition[0]=UnityEngine.Random.Range(0, 10);
			addition[1]=UnityEngine.Random.Range(0, 10);
			Debug.Log("test data: "+addition[0]+" "+addition[1]);
			
			UpdateValue1(textObj, addition, _ModifierType.percentage);
			
			Debug.Log("result: "+textObj.val.Length+"  "+textObj.mod.Length);
			foreach(float val in textObj.val) Debug.Log(val);
			foreach(float val in textObj.mod) Debug.Log(val);
		}
	}
	*/
	
	static float[] UpdateTowerValue(TowerValue towerVal, float[] addition, _ModifierType type){
		float[] resultList=new float[addition.Length];
		float[] original=new float[0];
		
		if(type==_ModifierType.value) original=towerVal.val;
		else if(type==_ModifierType.percentage) original=towerVal.mod;
		
		if(addition.Length!=original.Length){
			for(int i=0; i<addition.Length; i++){
				if(i<original.Length) resultList[i]=original[i]+addition[i];
				else resultList[i]=addition[i];
			}
		}
		else{
			for(int i=0; i<addition.Length; i++){
				resultList[i]=original[i]+addition[i];
			}
		}
		
		if(type==_ModifierType.value) towerVal.val=resultList;
		else if(type==_ModifierType.percentage) towerVal.mod=resultList;
		
		return resultList;
	}
	
	static float[] UpdateTowerValueResource(TowerValue towerVal, float[] addition, _ModifierType type){
		float[] resultList=new float[addition.Length];
		float[] original=new float[0];
		
		if(type==_ModifierType.value) original=towerVal.rscVal;
		else if(type==_ModifierType.percentage) original=towerVal.rscMod;
		
		if(addition.Length!=original.Length){
			for(int i=0; i<addition.Length; i++){
				if(i<original.Length) resultList[i]=original[i]+addition[i];
				else resultList[i]=addition[i];
			}
		}
		else{
			for(int i=0; i<addition.Length; i++) resultList[i]=original[i]+addition[i];
		}
		
		if(type==_ModifierType.value) towerVal.rscVal=resultList;
		else if(type==_ModifierType.percentage) towerVal.rscMod=resultList;

		return resultList;
	}
		
	static float[] UpdateValueObsolete(float[] original, float[] addition){
		float[] resultList=new float[addition.Length];
		//~ if(addition.Length!=original.Length){
			//~ for(int i=0; i<addition.Length; i++){
				//~ if(i<original.Length) resultList[i]=original[i]+addition[i];
				//~ else resultList[i]=addition[i];
			//~ }
		//~ }
		//~ else{
			//~ for(int i=0; i<addition.Length; i++) resultList[i]=original[i]+addition[i];
		//~ }
		return resultList;
	}
	
	static int[] CopyListValueInt(float[] src, int[] tgt){
		for(int i=0; i<src.Length; i++){
			tgt[i]+=(int)Mathf.Round(src[i]);
		}
		return tgt;
	}
	
	static float[] CopyListValueFloat(float[] src, float[] tgt){
		for(int i=0; i<src.Length; i++){
			tgt[i]+=src[i];
		}
		return tgt;
	}
	
	
	

	public static float[] GetTotalBuildCostReductionModifier(int prefabID){
		int length=GameControl.GetResourceCount();
		float[] list=GetTowerBuildCostReducMod(prefabID);
		if(list.Length==0) list=new float[length];
		float[] returnList=new float[length];
		for(int i=0; i<returnList.Length; i++){
			returnList[i]=allCostReductionModifier[i]+buildCostReductionModifier[i]+list[i];
		}
		return returnList;
	}
	public static float[] GetTotalBuildCostReductionValue(int prefabID){
		int length=GameControl.GetResourceCount();
		float[] list=GetTowerBuildCostReducVal(prefabID);
		if(list.Length==0) list=new float[length];
		float[] returnList=new float[length];
		for(int i=0; i<returnList.Length; i++){
			returnList[i]=allCostReductionValue[i]+buildCostReductionValue[i]+list[i];
		}
		return returnList;
	}
	
	public static float[] GetAllBuildCostReductionModifier(){
		float[] list=new float[allCostReductionModifier.Length];
		for(int i=0; i<list.Length; i++) list[i]=allCostReductionModifier[i]+buildCostReductionModifier[i];
		return list;
		//~ return allCostReductionModifier+buildCostReductionModifier;
	}
	public static int[] GetAllBuildCostReductionValue(){
		int[] list=new int[allCostReductionValue.Length];
		for(int i=0; i<list.Length; i++) list[i]=allCostReductionValue[i]+buildCostReductionValue[i];
		return list;
		//~ return allCostReductionValue+buildCostReductionValue;
	}
	public static float[] GetAllUpgradeCostReductionModifier(){
		float[] list=new float[allCostReductionModifier.Length];
		for(int i=0; i<list.Length; i++) list[i]=allCostReductionModifier[i]+upgradeCostReductionModifier[i];
		return list;
		//~ return allCostReductionModifier+upgradeCostReductionModifier;
	}
	public static int[] GetAllUpgradeCostReductionValue(){
		int[] list=new int[allCostReductionValue.Length];
		for(int i=0; i<list.Length; i++) list[i]=allCostReductionValue[i]+upgradeCostReductionValue[i];
		return list;
		//~ return allCostReductionValue+upgradeCostReductionValue;
	}
	
	List<Perk> Load(){
		XmlDocument xmlDoc = new XmlDocument();
		List<Perk> newList=new List<Perk>();
		
		GameObject obj=Resources.Load("IconList", typeof(GameObject)) as GameObject;
		IconList iconList=obj.GetComponent<IconList>();
		
		TextAsset perkTextAsset=Resources.Load("Perk", typeof(TextAsset)) as TextAsset;
		if(perkTextAsset!=null){
			xmlDoc.Load(new MemoryStream(perkTextAsset.bytes));
			XmlNode rootNode = xmlDoc.FirstChild;
			if (rootNode.Name == "something"){
				int perkCount=rootNode.ChildNodes.Count;
				
				for(int n=0; n<perkCount; n++){
					newList.Add(new Perk());
					Perk perk=newList[n];
					
					for(int m=0; m<rootNode.ChildNodes[n].Attributes.Count; m++){
						XmlAttribute attr=rootNode.ChildNodes[n].Attributes[m];
						if(attr.Name=="ID"){
							perk.ID=int.Parse(attr.Value);
						}
						else if(attr.Name=="icon"){
							int ID=int.Parse(attr.Value);
							if(iconList.perkIconList.Count>ID) perk.icon=iconList.perkIconList[ID];
						}
						else if(attr.Name=="iconUL"){
							int ID=int.Parse(attr.Value);
							if(iconList.perkIconList.Count>ID) perk.iconUnlocked=iconList.perkIconList[ID];
						}
						else if(attr.Name=="iconUA"){
							int ID=int.Parse(attr.Value);
							if(iconList.perkIconList.Count>ID) perk.iconUnavailable=iconList.perkIconList[ID];
						}
						else if(attr.Name=="name"){
							perk.name=attr.Value;
						}
						else if(attr.Name=="type"){
							perk.type=(_PerkType)int.Parse(attr.Value); 
						}
						else if(attr.Name=="waveMin"){
							perk.waveMin=int.Parse(attr.Value);
						}
						else if(attr.Name=="perkMin"){
							perk.perkMin=int.Parse(attr.Value);
						}
						else if(attr.Name=="modTypeVal"){
							perk.modTypeVal=(_ModifierType)int.Parse(attr.Value);
						}
						else if(attr.Name=="modTypeRsc"){
							perk.modTypeRsc=(_ModifierType)int.Parse(attr.Value);
						}
						else if(attr.Name=="valueCount"){
							int count=int.Parse(attr.Value);
							perk.valueCount=count;
							perk.value=new float[count];
							for(int i=m; i<m+count; i++){
								perk.value[i-m]=float.Parse(rootNode.ChildNodes[n].Attributes[i+1].Value);
							}
						}
						else if(attr.Name=="desp"){
							perk.desp=attr.Value;
						}
						else if(attr.Name=="towerID"){
							perk.towerID=int.Parse(attr.Value);
						}
						else if(attr.Name=="abilityID"){
							perk.abilityID=int.Parse(attr.Value);
						}
						else if(attr.Name=="enableASID"){
							int val=int.Parse(attr.Value);
							if(val==1) perk.enableAbilityS=true;
							else perk.enableAbilityS=false;
						}
						else if(attr.Name=="abilitySID"){
							perk.abilitySID=int.Parse(attr.Value);
						}
						else if(attr.Name=="abilityGCount"){
							int count=int.Parse(attr.Value);
							perk.abilityGroup=new List<int>();
							for(int i=m; i<m+count; i++){
								perk.abilityGroup.Add(int.Parse(rootNode.ChildNodes[n].Attributes[i+1].Value));
							}
						}
						else if(attr.Name=="repeat"){
							int val=int.Parse(attr.Value);
							if(val==1) perk.repeatable=true;
							else perk.repeatable=false;
						}
						else if(attr.Name=="rscCount"){
							int count=int.Parse(attr.Value);
							perk.rsc=new float[count];
							for(int i=m; i<m+count; i++){
								perk.rsc[i-m]=float.Parse(rootNode.ChildNodes[n].Attributes[i+1].Value);
							}
						}
						else if(attr.Name=="costCount"){
							int count=int.Parse(attr.Value);
							perk.costs=new int[count];
							for(int i=m; i<m+count; i++){
								perk.costs[i-m]=int.Parse(rootNode.ChildNodes[n].Attributes[i+1].Value);
							}
						}
						else if(attr.Name=="prereqCount"){
							int count=int.Parse(attr.Value);
							for(int i=m; i<m+count; i++){
								perk.prereq.Add(int.Parse(rootNode.ChildNodes[n].Attributes[i+1].Value));
							}
						}
					}
					
				}
			}
		}
		
		return newList;
	}
}



public enum _ModifierType{value, percentage}

[System.Serializable]
public class Perk{
	public int ID;
	public string name="";
	public string desp="";
	public Texture icon;
	public Texture iconUnavailable;
	public Texture iconUnlocked;
	public _PerkType type;
	
	public int[] costs=new int[1];
	public int levelMin=0;
	public int waveMin=0;
	public int perkMin=0;
	public List<int> prereq=new List<int>();
	
	public _ModifierType modTypeVal;
	public _ModifierType modTypeRsc;
	public float[] value=new float[0];
	public float[] rsc=new float[0];
	
	public int towerID=-1;
	public int abilityID=-1;
	public int abilitySID=-1;
	public List<int> abilityGroup=new List<int>();
	
	public bool repeatable=false;
	public bool unlocked=false;
	public bool enableInlvl=true;
	
	
	
	public Perk Clone(){
		Perk p=new Perk();
		p.ID=ID;
		p.name=name;
		p.desp=desp;
		p.icon=icon;
		p.iconUnavailable=iconUnavailable;
		p.iconUnlocked=iconUnlocked;
		p.type=type;
		
		p.costs=CloneCost();
		
		p.levelMin=levelMin;
		p.waveMin=waveMin;
		p.perkMin=perkMin;
		p.prereq=prereq;
		
		p.modTypeVal=modTypeVal;
		p.modTypeRsc=modTypeRsc;
		p.value=CloneValue();
		p.rsc=CloneRsc();
		
		p.towerID=towerID;
		p.abilityID=abilityID;
		p.abilitySID=abilitySID;
		p.abilityGroup=abilityGroup;
		
		p.repeatable=repeatable;
		p.unlocked=unlocked;
		p.enableInlvl=enableInlvl;
		
		p.valueCount=valueCount;
		p.enableTower=enableTower;
		p.enableAbility=enableAbility;
		p.enableAbilityS=enableAbilityS;
		p.enableAbilityGroup=enableAbilityGroup;
		p.enableRsc=enableRsc;
		p.enableModTypeVal=enableModTypeVal;
		p.enableModTypeRsc=enableModTypeRsc;
		
		return p;
	}
	
	
	float[] CloneValue(){
		float[] newList=new float[value.Length];
		for(int i=0; i<value.Length; i++) newList[i]=value[i];
		return newList;
	}
	float[] CloneRsc(){
		float[] newList=new float[rsc.Length];
		for(int i=0; i<rsc.Length; i++) newList[i]=rsc[i];
		return newList;
	}
	int[] CloneCost(){
		int[] newList=new int[costs.Length];
		for(int i=0; i<costs.Length; i++) newList[i]=costs[i];
		return newList;
	}
	
	
	
	public int IsAvailable(){
		List<Perk> allPerkList=PerkManager.GetAllPerks();
		foreach(int id in prereq){
			if(!allPerkList[id].unlocked){
				return 1;
			}
		}
		
		if(waveMin>SpawnManager.GetCurrentWaveID()){
			return 2;
		}
		
		if(perkMin>PerkManager.perkPoint) return 3;
		
		return 0;
	}
	
	public bool HaveSufficientResource(){
		if(GameControl.HaveSufficientResource(costs)) return true;
		return false;
	}
	
	public void Unlock(){
		if(!unlocked){
			if(!repeatable) unlocked=true;
			GameControl.SpendResource(costs);
		}
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
	
	public void UpdateRscListLength(int length){
		float[] tempCosts=rsc;
		rsc=new float[length];
		
		for(int i=0; i<length; i++){
			if(i>=tempCosts.Length){
				rsc[i]=0;
			}
			else{
				rsc[i]=tempCosts[i];
			}
		}
	}
	
	public bool RequireResource(){
		foreach(int cost in costs){
			if(cost>0) return true;
		}
		return false;
	}
	
	
	void SetValueCount(int count){
		valueCount=count;
		if(value.Length!=count){
			float[] newList=new float[count];
			
			for(int i=0; i<count; i++){
				if(i<value.Length) newList[i]=value[i];
				else newList[i]=0;
			}
			
			value=newList;
		}
	}
	
	
	public int valueCount;
	public bool enableTower=false;
	public bool enableAbility=false;
	public bool enableAbilityS=false;
	public bool enableAbilityGroup=false;
	public bool enableRsc=false;
	public bool enableModTypeVal=false;
	public bool enableModTypeRsc=false;
	
	public void SetType(_PerkType t){
		type=t;
		
		if(type==_PerkType.TowerUnlockNew){
			SetValueCount(0);
			enableTower=true; enableAbility=false;
			enableAbilityS=false; enableAbilityGroup=false;
			enableRsc=false;
			enableModTypeVal=false;	enableModTypeRsc=false;
		}
		//all tower
		else if(type==_PerkType.TowerBuildLevelBonus){
			SetValueCount(1);
			enableTower=false; enableAbility=false;
			enableAbilityS=false; enableAbilityGroup=false;
			enableRsc=false;
			enableModTypeVal=false;	enableModTypeRsc=false;
		}
		
		//all tower buff
		else if(type==_PerkType.TowerBuffHP){
			SetValueCount(1);
			enableTower=false; enableAbility=false;
			enableAbilityS=false; enableAbilityGroup=false;
			enableRsc=false;
			enableModTypeVal=true;	enableModTypeRsc=false;
		}
		else if(type==_PerkType.TowerBuffAttack){
			SetValueCount(1);
			enableTower=false; enableAbility=false;
			enableAbilityS=false; enableAbilityGroup=false;
			enableRsc=false;
			enableModTypeVal=true;	enableModTypeRsc=false;
		}
		else if(type==_PerkType.TowerBuffDef){
			SetValueCount(1);
			enableTower=false; enableAbility=false;
			enableAbilityS=false; enableAbilityGroup=false;
			enableRsc=false;
			enableModTypeVal=true;	enableModTypeRsc=false;
		}
		
		//all tower critical
		else if(type==_PerkType.TowerCritical){
			SetValueCount(2);
			enableTower=false; enableAbility=false;
			enableAbilityS=false; enableAbilityGroup=false;
			enableRsc=false;
			enableModTypeVal=false;	enableModTypeRsc=false;
		}
		else if(type==_PerkType.TowerCriticalChance){
			SetValueCount(1);
			enableTower=false; enableAbility=false;
			enableAbilityS=false; enableAbilityGroup=false;
			enableRsc=false;
			enableModTypeVal=false;	enableModTypeRsc=false;
		}
		else if(type==_PerkType.TowerCriticalDamage){
			SetValueCount(1);
			enableTower=false; enableAbility=false;
			enableAbilityS=false; enableAbilityGroup=false;
			enableRsc=false;
			enableModTypeVal=true;	enableModTypeRsc=false;
		}
		
		//all tower cost
		else if(type==_PerkType.TowerAllCostReduction){
			SetValueCount(0);
			enableTower=false; enableAbility=false;
			enableAbilityS=false; enableAbilityGroup=false;
			enableRsc=true;
			enableModTypeVal=false;	enableModTypeRsc=true;
		}
		else if(type==_PerkType.TowerBuildCostReduction){
			SetValueCount(0);
			enableTower=false; enableAbility=false;
			enableAbilityS=false; enableAbilityGroup=false;
			enableRsc=true;
			enableModTypeVal=false;	enableModTypeRsc=true;
		}
		else if(type==_PerkType.TowerUpgradeCostReduction){
			SetValueCount(0);
			enableTower=false; enableAbility=false;
			enableAbilityS=false; enableAbilityGroup=false;
			enableRsc=true;
			enableModTypeVal=false;	enableModTypeRsc=true;
		}
		
		
		//life
		else if(type==_PerkType.LifeIncreaseCap){
			SetValueCount(1);
			enableTower=false; enableAbility=false;
			enableAbilityS=false; enableAbilityGroup=false;
			enableRsc=false;
			enableModTypeVal=true;	enableModTypeRsc=false;
		}
		else if(type==_PerkType.LifeBonus){
			SetValueCount(1);
			enableTower=false; enableAbility=false;
			enableAbilityS=false; enableAbilityGroup=false;
			enableRsc=false;
			enableModTypeVal=true;	enableModTypeRsc=false;
		}
		else if(type==_PerkType.LifeBonusWaveCleared){
			SetValueCount(3);
			enableTower=false; enableAbility=false;
			enableAbilityS=false; enableAbilityGroup=false;
			enableRsc=false;
			enableModTypeVal=false;	enableModTypeRsc=false;
		}
		else if(type==_PerkType.LifeRegen){
			SetValueCount(3);
			enableTower=false; enableAbility=false;
			enableAbilityS=false; enableAbilityGroup=false;
			enableRsc=false;
			enableModTypeVal=false;	enableModTypeRsc=false;
		}
		
		
		//resource
		else if(type==_PerkType.ResourceBonus){
			SetValueCount(0);
			enableTower=false; enableAbility=false;
			enableAbilityS=false; enableAbilityGroup=false;
			enableRsc=true;
			enableModTypeVal=false;	enableModTypeRsc=false;
		}
		else if(type==_PerkType.ResourceGeneration){
			SetValueCount(2);
			enableTower=false; enableAbility=false;
			enableAbilityS=false; enableAbilityGroup=false;
			enableRsc=true;
			enableModTypeVal=false;	enableModTypeRsc=false;
		}
		else if(type==_PerkType.ResourceGain){
			SetValueCount(1);
			enableTower=false; enableAbility=false;
			enableAbilityS=false; enableAbilityGroup=false;
			enableRsc=true;
			enableModTypeVal=false;	enableModTypeRsc=true;
		}
		else if(type==_PerkType.ResourceGainCreepKilled){
			SetValueCount(1);
			enableTower=false; enableAbility=false;
			enableAbilityS=false; enableAbilityGroup=false;
			enableRsc=true;
			enableModTypeVal=false;	enableModTypeRsc=true;
		}
		else if(type==_PerkType.ResourceGainWaveCleared){
			SetValueCount(1);
			enableTower=false; enableAbility=false;
			enableAbilityS=false; enableAbilityGroup=false;
			enableRsc=true;
			enableModTypeVal=false;	enableModTypeRsc=true;
		}
		else if(type==_PerkType.ResourceGainResourceTower){
			SetValueCount(1);
			enableTower=false; enableAbility=false;
			enableAbilityS=false; enableAbilityGroup=false;
			enableRsc=true;
			enableModTypeVal=false;	enableModTypeRsc=true;
		}
		
		
		//specific tower 
		else if(type==_PerkType.SpecificTowerLevelBonus){
			SetValueCount(1);
			enableTower=true; enableAbility=false;
			enableAbilityS=false; enableAbilityGroup=false;
			enableRsc=false;
			enableModTypeVal=false;	enableModTypeRsc=false;
		}
		//specific tower buff
		else if(type==_PerkType.SpecificTowerBuffHP){
			SetValueCount(1);
			enableTower=true; enableAbility=false;
			enableAbilityS=false; enableAbilityGroup=false;
			enableRsc=false;
			enableModTypeVal=true;	enableModTypeRsc=false;
		}
		else if(type==_PerkType.SpecificTowerBuffAttack){
			SetValueCount(1);
			enableTower=true; enableAbility=false;
			enableAbilityS=false; enableAbilityGroup=false;
			enableRsc=false;
			enableModTypeVal=true;	enableModTypeRsc=false;
		}
		else if(type==_PerkType.SpecificTowerBuffDef){
			SetValueCount(1);
			enableTower=true; enableAbility=false;
			enableAbilityS=false; enableAbilityGroup=false;
			enableRsc=false;
			enableModTypeVal=true;	enableModTypeRsc=false;
		}
		else if(type==_PerkType.SpecificTowerBuffRange){
			SetValueCount(1);
			enableTower=true; enableAbility=false;
			enableAbilityS=false; enableAbilityGroup=false;
			enableRsc=false;
			enableModTypeVal=true;	enableModTypeRsc=false;
		}
		//specific tower critical
		else if(type==_PerkType.SpecificTowerCritical){
			SetValueCount(2);
			enableTower=true; enableAbility=false;
			enableAbilityS=false; enableAbilityGroup=false;
			enableRsc=false;
			enableModTypeVal=false;	enableModTypeRsc=false;
		}
		else if(type==_PerkType.SpecificTowerCriticalChance){
			SetValueCount(1);
			enableTower=true; enableAbility=false;
			enableAbilityS=false; enableAbilityGroup=false;
			enableRsc=false;
			enableModTypeVal=false;	enableModTypeRsc=false;
		}
		else if(type==_PerkType.SpecificTowerCriticalDamage){
			SetValueCount(1);
			enableTower=true; enableAbility=false;
			enableAbilityS=false; enableAbilityGroup=false;
			enableRsc=false;
			enableModTypeVal=true;	enableModTypeRsc=false;
		}
		//specific tower special
		else if(type==_PerkType.SpecificTowerStun){
			SetValueCount(2);
			enableTower=true; enableAbility=false;
			enableAbilityS=false; enableAbilityGroup=false;
			enableRsc=false;
			enableModTypeVal=false;	enableModTypeRsc=false;
		}
		else if(type==_PerkType.SpecificTowerSlow){
			SetValueCount(2);
			enableTower=true; enableAbility=false;
			enableAbilityS=false; enableAbilityGroup=false;
			enableRsc=false;
			enableModTypeVal=false;	enableModTypeRsc=false;
		}
		//specific tower cost
		else if(type==_PerkType.SpecificTowerAllCostReduction){
			SetValueCount(0);
			enableTower=true; enableAbility=false;
			enableAbilityS=false; enableAbilityGroup=false;
			enableRsc=true;
			enableModTypeVal=false;	enableModTypeRsc=true;
		}
		else if(type==_PerkType.SpecificTowerBuildCostReduction){
			SetValueCount(0);
			enableTower=true; enableAbility=false;
			enableAbilityS=false; enableAbilityGroup=false;
			enableRsc=true;
			enableModTypeVal=false;	enableModTypeRsc=true;
		}
		else if(type==_PerkType.SpecificTowerUpgradeCostReduction){
			SetValueCount(0);
			enableTower=true; enableAbility=false;
			enableAbilityS=false; enableAbilityGroup=false;
			enableRsc=true;
			enableModTypeVal=false;	enableModTypeRsc=true;
		}
		
		//ernergy
		else if(type==_PerkType.EnergyRegenRate){
			SetValueCount(1);
			enableTower=false; enableAbility=false;
			enableAbilityS=false; enableAbilityGroup=false;
			enableRsc=false;
			enableModTypeVal=false;	enableModTypeRsc=false;
		}
		else if(type==_PerkType.EnergyIncreaseCap){
			SetValueCount(1);
			enableTower=false; enableAbility=false;
			enableAbilityS=false; enableAbilityGroup=false;
			enableRsc=false;
			enableModTypeVal=true;	enableModTypeRsc=false;
		}
		else if(type==_PerkType.EnergyCostReduction){
			SetValueCount(1);
			enableTower=false; enableAbility=false;
			enableAbilityS=false; enableAbilityGroup=false;
			enableRsc=false;
			enableModTypeVal=true;	enableModTypeRsc=false;
		}
		else if(type==_PerkType.EnergyGainCreepKilled){
			SetValueCount(2);
			enableTower=false; enableAbility=false;
			enableAbilityS=false; enableAbilityGroup=false;
			enableRsc=false;
			enableModTypeVal=true;	enableModTypeRsc=false;
		}
		else if(type==_PerkType.EnergyGainWaveCleared){
			SetValueCount(2);
			enableTower=false; enableAbility=false;
			enableAbilityS=false; enableAbilityGroup=false;
			enableRsc=false;
			enableModTypeVal=true;	enableModTypeRsc=false;
		}
		
		else if(type==_PerkType.AbilityUnlockNew){
			SetValueCount(0);
			enableTower=false;	enableAbility=true; 
			enableAbilityS=false; enableAbilityGroup=false;
			enableRsc=false;
			enableModTypeVal=false;	enableModTypeRsc=false;
		}
		else if(type==_PerkType.AbilityCost){
			SetValueCount(1);
			enableTower=false;	enableAbility=false; 
			enableAbilityS=false; enableAbilityGroup=true;
			enableRsc=false;
			enableModTypeVal=true;	enableModTypeRsc=false;
		}
		else if(type==_PerkType.AbililyCooldown){
			SetValueCount(1);
			enableTower=false;	enableAbility=false; 
			enableAbilityS=false; enableAbilityGroup=true;
			enableRsc=false;
			enableModTypeVal=true;	enableModTypeRsc=false;
		}
		
		//Debug.Log(type);
		
		if(!enableAbilityS) abilitySID=-1;
		if(!enableAbilityGroup) abilityGroup=new List<int>();
	}
}










public enum _PerkType{
	TowerUnlockNew, 
	TowerBuildLevelBonus, 
	TowerBuffHP, 
	TowerBuffAttack, 
	TowerBuffDef, 
	TowerCritical,
	TowerCriticalChance, 
	TowerCriticalDamage, 
	TowerAllCostReduction, 
	TowerBuildCostReduction, 
	TowerUpgradeCostReduction, //10
	LifeIncreaseCap, 
	LifeBonus, 
	LifeBonusWaveCleared, 
	LifeRegen,
	ResourceBonus,
	ResourceGeneration,
	ResourceGain,
	ResourceGainCreepKilled,
	ResourceGainWaveCleared,
	ResourceGainResourceTower,	//19
	SpecificTowerLevelBonus, 		
	SpecificTowerBuffHP,
	SpecificTowerBuffAttack,
	SpecificTowerBuffDef,
	SpecificTowerBuffRange,
	SpecificTowerCritical,
	SpecificTowerCriticalChance, 
	SpecificTowerCriticalDamage, //29
	SpecificTowerAllCostReduction,	
	SpecificTowerBuildCostReduction,	
	SpecificTowerUpgradeCostReduction,
	SpecificTowerStun, //29
	SpecificTowerSlow, //29
	//~ RandomResourceCreepKilled,
	//~ RandomResourceWaveCleared,
	//~ RandomLifeBonusWaveCleared,
	//~ RandomTowerLevelBonus,
	//~ RandomSpecificTowerLevelBonus,
	//~ RandomKillCreep,
	
	EnergyRegenRate,
	EnergyIncreaseCap,
	EnergyCostReduction,
	EnergyGainCreepKilled,
	EnergyGainWaveCleared,
	AbilityUnlockNew,
	//~ AbilityMod,
	AbilityCost,
	AbililyCooldown,
	
	
}
