using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum _SpawnLimit{Finite, Infinite}
public enum _SpawnMode{Continuous, WaveCleared, RoundBased, SkippableContinuous, SkippableWaveCleared}

[System.Serializable]
public class UnitParameter{
	public GameObject unit;
	public bool enabled=true;
	public int minWave=0;
	
	//public float minSpeed=2;
	public float minDelay=1;
	public float maxDelay=1;
	public float minInterval=1;
	public float maxInterval=1;
	
	public float minSpd=1.5f;
	public float startSpd=1;
	public float spdIncrement=0.05f;
	//public float spdModifier=0;
	public float spdDeviation=2;
	
	public float minHP=10;
	public float startHP=10;
	public float HPIncrement=1;
	//public float HPModifier=0;
	public float HPDeviation=3;
	
	public float minShd=-1;
	public float startShd=8;
	public float shdIncrement=1;
	//public float HPModifier=0;
	public float shdDeviation=3;
	
	public float startInterval=100;
	public float intIncrement=50;
	public float intModifier=0;
	public float intDeviation=0;
	
	public UnitParameter Clone(){
		UnitParameter up=new UnitParameter();
		up.unit=unit;
		up.enabled=enabled;
		up.minWave=minWave;
		
		up.minDelay=minDelay;
		up.maxDelay=maxDelay;
		up.minInterval=minInterval;
		up.maxInterval=maxInterval;
		
		up.minSpd=minSpd;
		up.startSpd=startSpd;
		up.spdIncrement=spdIncrement;
		up.spdDeviation=spdDeviation;
		
		up.minHP=minHP;
		up.startHP=startHP;
		up.HPIncrement=HPIncrement;
		up.HPDeviation=HPDeviation;
		
		up.minShd=minShd;
		up.startShd=startShd;
		up.shdIncrement=shdIncrement;
		up.shdDeviation=shdDeviation;
		
		up.startInterval=startInterval;
		up.intIncrement=intIncrement;
		up.intModifier=intModifier;
		up.intDeviation=intDeviation;
		
		return up;
	}
}

[AddComponentMenu("TDTK/Managers/SpawnManager")]
public class SpawnManager : MonoBehaviour {

	public delegate void WaveStartSpawnHandler(int waveID);
	public static event WaveStartSpawnHandler onWaveStartSpawnE;

	public delegate void WaveSpawnedHandler(int waveID);
	public static event WaveSpawnedHandler onWaveSpawnedE;
	
	public delegate void WaveClearedHandler(int waveID);
	public static event WaveClearedHandler onWaveClearedE;
	
	public delegate void WaveClearedForSpawningHandler(bool flag);
	public static event WaveClearedForSpawningHandler onClearForSpawningE;
	
	public bool autoGenWave=false;
	public _SpawnLimit spawnLimit=_SpawnLimit.Finite;
	public _SpawnMode spawnMode=_SpawnMode.Continuous;
	
	public bool pathLooing=false;
	
	public PathTD defaultPath;
	private List<Vector3> waypoints;
	
	public Wave[] waves=new Wave[1];
	
	private bool isClearForSpawning=true;
	private int currentWave=0;
	
	private float timeLastSpawn;
	private float waitDuration;
	
	static private SpawnManager spawnManager;
	
	private int totalSpawnCount=0;
	
	public bool showDebugMessage=false;
	
	public static Wave GetCurrentWave(){
		if(spawnManager.spawnLimit==_SpawnLimit.Infinite) return null;
		if(spawnManager.currentWave-1>=spawnManager.waves.Length || spawnManager.currentWave-1<0) return null;
		return spawnManager.waves[spawnManager.currentWave-1];
	}
	
	public static Wave GetNextWave(){
		if(spawnManager.spawnLimit==_SpawnLimit.Infinite) return null;
		if(spawnManager.currentWave>=spawnManager.waves.Length) return null;
		return spawnManager.waves[spawnManager.currentWave];
	}
	
	public static Wave[] GetAllWaves(){
		return spawnManager.waves;
	}
	
	void Awake(){
		spawnManager=this;
	}
	
	void Start () {
		int rscCount=GameControl.GetResourceCount();
		
		if(defaultPath==null) defaultPath=(PathTD)FindObjectOfType(typeof(PathTD));
		
		if(autoGenWave){
			InitAutoGenCreepPara();
			if(spawnLimit==_SpawnLimit.Finite){
				for(int i=0; i<waves.Length; i++){
					waves[i]=GenerateWave(i);
				}
			}
		}
		else{
			//prespawn the unit
			foreach(Wave wave in waves){
				foreach(SubWave subWave in wave.subWaves){
					if(subWave.unit!=null){
						UnitCreep unit=subWave.unit.GetComponent<UnitCreep>();
						//make sure the creep value length match the resource count
						if(unit!=null) unit.InitValue(rscCount);
						ObjectPoolManager.New(subWave.unit, subWave.count);
					}
				}
			}
		}

		//LoadCreep();
	}
	
	private int infinitePrespawnCount=20;
	public void InitAutoGenCreepPara(){
		rscGain=MatchRscLength(rscGain);
		rscInc=MatchRscLength(rscInc);
		rscDev=MatchRscLength(rscDev);
		
		LoadCreep();
		
		if(spawnLimit==_SpawnLimit.Infinite){
			foreach(UnitParameter up in creepParameter){
				if(up.enabled){
					//~ ObjectPoolManager.New(up.unit, 30);
					ObjectPoolManager.New(up.unit, infinitePrespawnCount);
				}
			}
		}
		
		if(allPath.Count==0){
			allPath.Add(defaultPath);
		}
	}
	
	public float[] MatchRscLength(float[] list){
		int length=GameControl.GetResourceCount();
		
		if(list.Length==0){
			return new float[length];
		}
		
		float[] temp=new float[length];
		for(int i=0; i<temp.Length; i++){
			if(i<list.Length){
				temp[i]=list[i];
			}
			else{
				temp[i]=0;
			}
		}
		
		return temp;
	}
	
	
	public float difficultyModifier=1;
	
	public float startHPSum=100;
	public float HPIncrement=50;
	public float HPModifier=0;
	
	public float moveSpeed=1;
	public float spdIncrement=0.05f;
	public float spdModifier=0;
	
	public float[] rscGain=new float[1];
	public float[] rscInc=new float[1];
	public float[] rscDev=new float[1];
	
	//~ public float spawnTLimit=10;
	//~ public float spawnTLimitInc=0.5f;
	//~ public float spawnTLimitMod=0;
	
	public int unitCount=10;
	public float countIncrement=3;
	public float countDevMod=.2f;
	
	public List<PathTD> allPath=new List<PathTD>();
	public float subWaveCount=1;
	public int maxSubWaveCount=4;
	public float subWaveCountInc=0.75f;
	
	public UnitParameter[] creepParameter=new UnitParameter[0];
	
	public void InitCreepParameter(List<UnitCreep> list){
		//~ Debug.Log("init creep    "+creepParameter.Length);
		
		UnitParameter[] temp=new UnitParameter[list.Count];
		
		for(int i=0; i<list.Count; i++){
			bool match=false;
			//~ Debug.Log(list[i].gameObject);
			//~ for(int j=0; j<creepParameter.Length; i++){
			foreach(UnitParameter UP in creepParameter){
				//~ UnitParameter UP=creepParameter[j];
				if(UP.unit=list[i].gameObject){
					temp[i]=UP.Clone();
					match=true;
					break;
				}
			}
			
			if(!match){
				temp[i]=new UnitParameter();
				temp[i].unit=list[i].gameObject;
			}
			
		}
		
		creepParameter=temp;
		//~ creepParameter=new UnitParameter[temp.Length];
		//~ for(int i=0; i<temp.Length; i++){
			//~ creepParameter[i]=temp[i];
		//~ }
		
		//~ for(int i=0; i<temp.Length; i++){
			//~ Debug.Log(i+"   obj:"+temp[i].unit+"   "+list[i].gameObject);
		//~ }
	}
	
	public void InitCreepParameter2(List<UnitCreep> list){
		UnitParameter[] temp=new UnitParameter[list.Count];
		
		for(int i=0; i<temp.Length; i++){
			if(i>=creepParameter.Length){
				temp[i]=new UnitParameter();
				temp[i].unit=list[i].gameObject;
			}
			else{
				temp[i]=creepParameter[i];
			}
		}
		
		creepParameter=temp;
	}
	
	public List<int> PathAssignment(int count){
		int pathCount=allPath.Count;
		List<int> asgList=new List<int>();
		
		if(count<=pathCount){
			for(int i=0; i<count; i++){			
				int genID=Random.Range(0, pathCount);
				while(asgList.Contains(genID)) genID=Random.Range(0, pathCount);
				asgList.Add(genID);
			}
		}
		else{
			int ID=0;
			for(int i=0; i<count; i++){
				asgList.Add(ID);
				ID+=1;	if(ID>=pathCount) ID=0;
			}
		}
		
		
		
		return asgList;
	}
	
	public List<UnitCreep> creepList=new List<UnitCreep>();
	Wave GenerateWave(int waveNum){
		return GenerateWave(waveNum, GameControl.GetResourceCount());
	}
	public Wave GenerateWave(int waveNum, int rscCount){
		//~ if(creepList.Count==0){
			//~ return null;
		//~ }
		
		Wave thisWave=new Wave();
		thisWave.resourceGain=new int[rscCount];
		for(int i=0; i<thisWave.resourceGain.Length; i++){
			thisWave.resourceGain[i]=(int)(rscGain[i]+waveNum*rscInc[i]+(Random.Range(-rscDev[i], rscDev[i])));
		}
		
		int countThisWave=(int)(unitCount+((waveNum-1)*countIncrement));
		float randMod=1f+Random.Range(-countDevMod, countDevMod);
		countThisWave=(int)((float)countThisWave*(randMod));
		int currentCount=0;
		
		float HPQuota=startHPSum+(int)(HPIncrement*waveNum);
		//~ float spdQuota=moveSpeed+(int)(spdIncrement*currentWave);
		
		float maxSBC=subWaveCount+(waveNum)*subWaveCountInc;	//Debug.Log(Random.Range(10, 1)+"   "+max);
		maxSBC=Mathf.Min(maxSBC, maxSubWaveCount);
		//~ int subWaveCount=Random.Range(1, (int)max); 	//Debug.Log(subWaveCount+"   "+max);
		thisWave.subWaves=new SubWave[Random.Range(1, (int)maxSBC)];
		//~ thisWave.subWaves=new SubWave[allPath.Count];
		
		List<int> pathAsgList=PathAssignment(thisWave.subWaves.Length);
		
		List<UnitParameter> list=GenerateCreepListForWave(waveNum);
		
		for(int i=0; i<thisWave.subWaves.Length; i++){
			thisWave.subWaves[i]=new SubWave();
			//~ thisWave.subWaves[i].path=allPath[Random.Range(0, allPath.Count)];
			thisWave.subWaves[i].path=allPath[pathAsgList[i]];
			
			int ID=Random.Range(0, list.Count);
			UnitParameter up=list[ID];
			thisWave.subWaves[i].unit=list[ID].unit;
			thisWave.subWaves[i].SetUnitComponent();
			
			
			float unitHP=up.startHP+up.HPIncrement*waveNum+Random.Range(-up.HPDeviation, up.HPDeviation);
			thisWave.subWaves[i].overrideHP=Mathf.Max(up.minHP, unitHP);
			
			float unitSpd=up.startSpd+up.spdIncrement*waveNum+Random.Range(-up.spdDeviation, up.spdDeviation);
			thisWave.subWaves[i].overrideMoveSpd=Mathf.Max(up.minSpd, unitSpd);
			
			float unitShd=0;
			if(up.minShd>0){
				unitShd=up.startShd+up.shdIncrement*waveNum+Random.Range(-up.shdDeviation, up.shdDeviation);
				thisWave.subWaves[i].overrideShield=Mathf.Max(up.minShd, unitShd);
			}
			
			thisWave.subWaves[i].interval=Random.Range(list[ID].minInterval, list[ID].maxInterval);
			
			if(i==0) thisWave.subWaves[i].delay=Random.Range(list[ID].minDelay, list[ID].maxDelay);
			else thisWave.subWaves[i].delay=0;
			
			countThisWave+=thisWave.subWaves[i].count;
		}
		
		
		int swID=0;
		while(currentCount<countThisWave){
			int num=(int)Random.Range(countThisWave*0.2f, countThisWave*0.3f);
			num=Mathf.Min(num, countThisWave-currentCount);
			thisWave.subWaves[swID].count+=num;
			currentCount+=num;
			swID+=1;	if(swID==thisWave.subWaves.Length) swID=0;
		}
		
		
		float currentHPSum=0;
		for(int i=0; i<thisWave.subWaves.Length; i++){
			currentHPSum+=thisWave.subWaves[i].count*thisWave.subWaves[i].GetUnitComponent().GetFullHP();
		}
		if(currentHPSum<HPQuota){
			float delta=HPQuota-currentHPSum;
			float[] partition=new float[thisWave.subWaves.Length];
			float sum=0; 
			for(int i=0; i<partition.Length-1; i++){
				float rand=Random.Range(0f, 1.0f-sum);
				sum+=rand;
				partition[i]=rand;
			}
			for(int i=0; i<partition.Length-1; i++){
				partition[i]=partition[i]*delta/thisWave.subWaves[i].count;
				//Debug.Log("hp adjustment: "+partition[i]);
				float HPValue=thisWave.subWaves[i].GetUnitComponent().GetFullHP()+partition[i];
				thisWave.subWaves[i].overrideHP=Mathf.Max(thisWave.subWaves[i].overrideHP, HPValue);
			}
		}
		
		thisWave.waveInterval=thisWave.CalculateSpawnDuration()+waveCD+Random.Range(-waveCDDev, waveCDDev);
		//Debug.Log(currentCount+"  "+countThisWave+"  "+unitCount);
		
		return thisWave;
	}
	
	List<UnitParameter> GenerateCreepListForWave(int waveNum){
		List<UnitParameter> list=new List<UnitParameter>();
		
		for(int i=0; i<creepParameter.Length; i++){
			if(creepParameter[i].enabled && creepParameter[i].minWave<=waveNum){
				list.Add(creepParameter[i]);
			}
		}
		
		return list;
	}
	
	void LoadCreep(){
		//Debug.Log("load");
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
				
				for(int i=0; i<prefab.creepList.Count; i++){
					if(prefab.creepList[i]!=null){
						creepList.Add(prefab.creepList[i]);
					}
				}
			}
		}
		
		//~ creepParameter=new UnitParameter[creepList.Count];
		//~ if(creepParameter.Length!=creepList.Count){
			//~ UnitParameter[] temp=new UnitParameter[creepList.Count];
			//~ for(int i=0; i<temp.Length; i++){
				//~ if(i<creepParameter.Length){
					//~ temp[i]=creepParameter[i];
				//~ }
				//~ else{
					//~ temp[i]=new UnitParameter();
				//~ }
			//~ }
			//~ creepParameter=temp;
		//~ }
	}
	
	void OnEnable(){
		Unit.onDeadE += OnCheckIsWaveCleared;
		UnitCreep.onScoreE += OnCheckIsWaveCleared;
	}
	
	void OnDisable(){
		Unit.onDeadE -= OnCheckIsWaveCleared;
		UnitCreep.onScoreE -= OnCheckIsWaveCleared;
	}
	
	//external call for spawing of any kind, return true if spawning is successful, else return false
	protected bool _SpawnFinite(){
		if(isClearForSpawning && GameControl.gameState!=_GameState.Ended){
			//if currentwave has exceed available wave length
			if(currentWave>=waves.Length){
				if(showDebugMessage) Debug.Log("All wave has been spawned");
				return false;
			}
			else{
				//set gamestate to started if this is the first wave
				if(currentWave==0){
					GameControl.gameState=_GameState.Started;
					if(showDebugMessage) Debug.Log("game started");
				}
				
				//initiate corresponding spawn routine
				if(spawnMode==_SpawnMode.Continuous || spawnMode==_SpawnMode.SkippableContinuous){
					if(currentWave==0) StartCoroutine(ContinousTimedSpawn());
					else ContinousTimedSpawnSkip();
				}
				if(spawnMode==_SpawnMode.WaveCleared || spawnMode==_SpawnMode.SkippableWaveCleared){
					if(currentWave==0) StartCoroutine(WaveClearedSpawn());
					else WaveClearedSpawnSkip();
				}
				if(spawnMode==_SpawnMode.RoundBased){
					SpawnWave();
				}
			}
		}
		else {
			//anything else
			Debug.Log("SpawnManager is not ready to spawn next wave");
			return false;
		}
		
		return true;
	}
	
	void Update(){
		//~ if(Input.GetKeyDown(KeyCode.Space)){
			//~ SpawnWave(GenerateWave());
		//~ }
	}
	
	
	protected bool _SpawnInfinite(){
		if(!isClearForSpawning) return false;
		
		if(currentWave==0){
			GameControl.gameState=_GameState.Started;
			if(showDebugMessage) Debug.Log("game started");
		}
		
		if(spawnMode==_SpawnMode.Continuous || spawnMode==_SpawnMode.SkippableContinuous){
			if(currentWave==0) StartCoroutine(ContinousTimedSpawnInfinite());
			else ContinousTimedSpawnInfiniteSkip();
		}
		if(spawnMode==_SpawnMode.WaveCleared || spawnMode==_SpawnMode.SkippableWaveCleared){
			if(currentWave==0) StartCoroutine(WaveClearedSpawnInfinite());
			else WaveClearedSpawnInfiniteSkip();
		}
		if(spawnMode==_SpawnMode.RoundBased){
			SpawnWaveInfinite();
		}
		
		return true;
	}
	
	private Wave currentWaveI;
	private Wave nextWaveI;
	void SpawnWaveInfinite(){
		Wave thisWave=GenerateWave(currentWave);
		spawnedWaves.Add(thisWave);
		SpawnWave(thisWave);
	}
	
	
	public List<Wave> spawnedWaves=new List<Wave>();
	
	//call when a unit is dead, check if a wave has been cleared
	void OnCheckIsWaveCleared(int waveID){
		if(waveID<0) return;
		
		bool flag=false;
		Wave thisWave=null;
		
		//~ if(spawnLimit==_SpawnLimit.Finite){
		if(!autoGenWave){
			thisWave=waves[waveID];
		}
		else{
			for(int i=0; i<spawnedWaves.Count; i++){
				if(spawnedWaves[i].waveID==waveID){
					thisWave=spawnedWaves[i];
				}
			}
		}
		
		if(thisWave!=null){
			//reduce the acvitve unit of the corrsponding wave
			thisWave.activeUnitCount-=1;
			//if all the unit in that wave is spawned and the acitve unit count is 0, then the wave is cleared
			if(thisWave.spawned && thisWave.activeUnitCount==0){
				thisWave.cleared=true;
				flag=true;
				
				if(PerkManager.rscGainChance>0 || PerkManager.rscGainWaveChance>0){
					float randG=Random.Range(0f, 1f);
					float randC=Random.Range(0f, 1f);
					int[] inc=new int[thisWave.resourceGain.Length];
					
					for(int i=0; i<thisWave.resourceGain.Length; i++){
						float modifier=1;
						float bonus=0;
						
						if(randG<=PerkManager.rscGainChance){
							modifier+=PerkManager.rscGainModifier[i];
							bonus+=PerkManager.rscGainValue[i];
						}
						if(randC<=PerkManager.rscGainTowerChance){
							modifier+=PerkManager.rscGainWaveModifier[i];
							bonus+=PerkManager.rscGainWaveValue[i];
						}
						
						inc[i]=(int)Mathf.Round((float)thisWave.resourceGain[i]*modifier+bonus);
					}
					GameControl.GainResource(inc);
				}
				else GameControl.GainResource(thisWave.resourceGain);
				
				if(spawnLimit==_SpawnLimit.Infinite) spawnedWaves.Remove(thisWave);
			}
		}

		if(flag){
			if(showDebugMessage) Debug.Log("Wave "+waveID+" has been cleared");
			
			if(onWaveClearedE!=null) onWaveClearedE(waveID);
			
			AudioManager.PlayWaveClearedSound();
			
			//trigger event that the spawnmanger is clear to spawn again
			if(spawnMode==_SpawnMode.RoundBased){
				if(spawnLimit==_SpawnLimit.Infinite || (spawnLimit==_SpawnLimit.Finite && currentWave<waves.Length)){
					isClearForSpawning=true;
					onClearForSpawningE(isClearForSpawning);
				}
			}
		}
		
	}
	
	
	//call to skip ContinousTimedSpawn waiting time and spawn instantly
	private void ContinousTimedSpawnSkip(){
		if(GameControl.gameState!=_GameState.Ended){
			timeLastSpawn=Time.time;
			waitDuration=waves[currentWave].waveInterval;
			SpawnWave();
		}
		else{
			if(showDebugMessage) Debug.Log("The game is over");
		}
	}
	
	//continous spawn mode, spawn wave according to predefined interval
	IEnumerator ContinousTimedSpawn(){
		waitDuration=waves[currentWave].waveInterval;
		timeLastSpawn=-waitDuration;
		while(currentWave<waves.Length && GameControl.gameState!=_GameState.Ended){
			if(Time.time-timeLastSpawn>=waitDuration){
				timeLastSpawn=Time.time;
				waitDuration=waves[currentWave].waveInterval;
				SpawnWave();
			}
			yield return null;
		}
	}
	
	public float waveCD=15;
	public float waveCDDev=7.5f;
	
	private void ContinousTimedSpawnInfiniteSkip(){
		if(GameControl.gameState!=_GameState.Ended){
			
			SpawnWaveInfinite();
			
			timeLastSpawn=Time.time;
			float wavDur=spawnedWaves[spawnedWaves.Count-1].CalculateSpawnDuration();
			waitDuration=Mathf.Max(wavDur+waveCD+Random.Range(-waveCDDev, waveCDDev));
			
		}
		else{
			if(showDebugMessage) Debug.Log("The game is over");
		}
	}
	
	//continous spawn mode, spawn wave according to predefined interval
	IEnumerator ContinousTimedSpawnInfinite(){
		timeLastSpawn=-Mathf.Infinity;
		while(GameControl.gameState!=_GameState.Ended){
			if(Time.time-timeLastSpawn>=waitDuration){
				
				SpawnWaveInfinite();
				
				timeLastSpawn=Time.time;
				float wavDur=spawnedWaves[spawnedWaves.Count-1].CalculateSpawnDuration();
				waitDuration=Mathf.Max(wavDur+waveCD+Random.Range(-waveCDDev, waveCDDev));
				
			}
			yield return null;
		}
	}
	
	//call to skip wave cleared spawn mode, will instantly spwan next wave
	private void WaveClearedSpawnSkip(){
		if(GameControl.gameState!=_GameState.Ended){
			SpawnWave();
		}
	}
	
	//wave cleared spawn mode, spawn new wave automatically when current wave is cleared
	IEnumerator WaveClearedSpawn(){
		SpawnWave();
		while(currentWave<waves.Length && GameControl.gameState!=_GameState.Ended){
			if(waves[currentWave-1].cleared){
				SpawnWave();
			}
			yield return null;
		}
	}
	
	//wave cleared spawn mode, spawn new wave automatically when current wave is cleared
	IEnumerator WaveClearedSpawnInfinite(){
		SpawnWaveInfinite();
		while(GameControl.gameState!=_GameState.Ended){
			if(spawnedWaves.Count==0){
				SpawnWaveInfinite();
			}
			yield return null;
		}
	}
	
	//call to skip wave cleared spawn mode, will instantly spwan next wave
	private void WaveClearedSpawnInfiniteSkip(){
		if(GameControl.gameState!=_GameState.Ended){
			SpawnWaveInfinite();
		}
	}
	
	//spawning call for individual wave
	//from here, multiple spawn routine for each creep type in the wave is called
	public void SpawnWave(){
		SpawnWave(waves[currentWave]);
	}
	public void SpawnWave(Wave wave){
		wave.waveID=currentWave;
		
		AudioManager.PlayNewWaveSound();
		
		//disable user initiated spawning until current wave has done spawning
		isClearForSpawning=false;
		if(onClearForSpawningE!=null) onClearForSpawningE(isClearForSpawning);
		//Debug.Log("wave: "+currentWave+"      subwave Length:"+wave.subWaves.Length);
		foreach(SubWave subWave in wave.subWaves){
			StartCoroutine(SpawnSubwave(subWave, wave, currentWave));
		}
		
		StartCoroutine(CheckSpawn(wave, currentWave));
		
		currentWave+=1;
		if(onWaveStartSpawnE!=null) onWaveStartSpawnE(currentWave+1);
	}

	//actual spawning routine, responsible for spawning one type of creep only
	IEnumerator SpawnSubwave(SubWave subWave, Wave parentWave, int waveID){
		yield return new WaitForSeconds(subWave.delay);
		int spawnCount=0;
		while(spawnCount<subWave.count){

			Vector3 pos;
			Quaternion rot;
			
			PathTD tempPath;
			if(subWave.path==null) tempPath=defaultPath;
			else tempPath=subWave.path;
			
			pos=tempPath.waypoints[0].position;
			rot=tempPath.waypoints[0].rotation;
			
			GameObject obj=ObjectPoolManager.Spawn(subWave.unit, pos, rot);
			//Unit unit=obj.GetComponent<Unit>();
			UnitCreep unit=obj.GetComponent<UnitCreep>();
			
			if(subWave.overrideHP>0) unit.SetFullHP(subWave.overrideHP);
			if(subWave.overrideShield>0) unit.SetFullShield(subWave.overrideShield);
			if(subWave.overrideMoveSpd>0) unit.SetMoveSpeed(subWave.overrideMoveSpd);
			if(subWave.overrideLifeCost>=0) unit.SetLifeCost(subWave.overrideLifeCost);
			bool overrideValue=false;
			if(subWave.overrideValue.Length>=0){
				foreach(float val in subWave.overrideValue){
					if(val>0) overrideValue=true;
				}
			}
			if(overrideValue) unit.SetValue(subWave.overrideValue);
			
			unit.Init(tempPath, totalSpawnCount, waveID);
			unit.pathLooping=pathLooing;
			
			totalSpawnCount+=1;
			
			parentWave.activeUnitCount+=1;
			
			spawnCount+=1;
			if(spawnCount==subWave.count) break;
			
			yield return new WaitForSeconds(subWave.interval);
		}
		
		subWave.spawned=true;
	}
	
	//check if all the spawning in one individual wave is done
	IEnumerator CheckSpawn(Wave wave, int waveID){
		while(true){
			bool allSpawned=true;
			foreach(SubWave subWave in wave.subWaves){
				if(!subWave.spawned) allSpawned=false;
			}
			if(allSpawned) break;
			yield return null;
		}
		
		if(showDebugMessage) Debug.Log("wave "+(currentWave-1)+" has done spawning");
		
		//set the wave spawn flag to true, so check can be run to see if this wave is cleared
		wave.spawned=true;
		
		if(autoGenWave || currentWave<waves.Length){
			//enabled flag so next wave can be skip if skiappable spawn mode is selected
			if(spawnMode==_SpawnMode.SkippableContinuous || spawnMode==_SpawnMode.SkippableWaveCleared)
				isClearForSpawning=true;
			
			//trigger event if there are listener
			//telling listener is current wave is done spawning
			if(onWaveSpawnedE!=null) onWaveSpawnedE(waveID);
			//telling listener if spawning is now available/unavailable depending on flag isClearForSpawning 
			if(onClearForSpawningE!=null) onClearForSpawningE(isClearForSpawning);
		}
	}
	
	IEnumerator CheckSpawn1(int waveID){
		while(true){
			bool allSpawned=true;
			foreach(SubWave subWave in waves[waveID].subWaves){
				if(!subWave.spawned) allSpawned=false;
			}
			if(allSpawned) break;
			yield return null;
		}
		
		if(showDebugMessage) Debug.Log("wave "+(currentWave-1)+" has done spawning");
		
		//set the wave spawn flag to true, so check can be run to see if this wave is cleared
		waves[waveID].spawned=true;
		
		if(currentWave<waves.Length){
			//enabled flag so next wave can be skip if skiappable spawn mode is selected
			if(spawnMode==_SpawnMode.SkippableContinuous || spawnMode==_SpawnMode.SkippableWaveCleared)
				isClearForSpawning=true;
			
			//trigger event if there are listener
			//telling listener is current wave is done spawning
			if(onWaveSpawnedE!=null) onWaveSpawnedE(waveID);
			//telling listener if spawning is now available/unavailable depending on flag isClearForSpawning 
			if(onClearForSpawningE!=null) onClearForSpawningE(isClearForSpawning);
		}
	}
	
	static public int NewUnitID(){
		spawnManager.totalSpawnCount+=1;
		return spawnManager.totalSpawnCount-1;
	}
	
	static public void AddActiveUnit(int waveID, int num){
		spawnManager.waves[waveID].activeUnitCount+=num;
	}
	
	public float _TimeNextSpawn(){
		return timeLastSpawn+waitDuration-Time.time;
	}
	
	static public bool IsClearForSpawning(){
		return spawnManager.isClearForSpawning;
	}
	
	static public bool Spawn(){
		if(spawnManager.autoGenWave){
			if(spawnManager.spawnLimit==_SpawnLimit.Finite) return spawnManager._SpawnFinite();
			else if(spawnManager.spawnLimit==_SpawnLimit.Infinite) return spawnManager._SpawnInfinite();
		}
		else{
			return spawnManager._SpawnFinite();
		}
		//spawnManager._Spawn();
		
		return false;
	}
	
	static public int GetCurrentWaveID(){
		return Mathf.Max(1, spawnManager.currentWave);
	}
	
	static public int GetTotalWaveCount(){
		if(spawnManager.spawnLimit!=_SpawnLimit.Infinite || !spawnManager.autoGenWave) return spawnManager.waves.Length;
		else return -1;
	}
	
	static public float GetTimeNextSpawn(){
		int totalSpawnCount=GetTotalWaveCount();
		
		if(totalSpawnCount>0 && spawnManager.currentWave>=totalSpawnCount) return -1;
		return spawnManager._TimeNextSpawn();
	}
	
	static public _SpawnMode GetSpawnMode(){
		return spawnManager.spawnMode;
	}
	
	//pass the wave number, 0 for the first wave, 1 for the second wave and so on
	public static int GetUnitCountInWave(int ID){
		if(ID>spawnManager.waves.Length) return 0;
		
		Wave wave=spawnManager.waves[ID];
		
		int count=0;
		foreach(SubWave subWave in wave.subWaves){
			count+=subWave.count;
			count+=GetUnitCountForSpawnUnitRecursively(subWave.unit)*subWave.count;
		}
		
		return count;
	}
	
	public static int GetUnitCountForSpawnUnitRecursively(GameObject creep){
		int count=0;
		UnitCreep creepCom=creep.GetComponent<UnitCreep>();
		if(creepCom.spawnUponDestroyed!=null){
			count+=creepCom.spawnNumber;
			count+=GetUnitCountForSpawnUnitRecursively(creepCom.spawnUponDestroyed)*creepCom.spawnNumber;
		}
		return count;
	}
	
}

[System.Serializable]
public class SubWave{
	public GameObject unit;
	public int count=1;
	public float interval=1;
	public float delay;
	public PathTD path;
	public float overrideHP=-1;
	public float overrideShield=-1;
	public float overrideMoveSpd=-1;
	public int overrideLifeCost=-1;
	public int[] overrideValue=new int[0];
	
	[HideInInspector] public bool spawned=false;
	
	[HideInInspector] public Unit unitComponent;
	public Unit GetUnitComponent(){
		if(unitComponent!=null) return unitComponent;
		else{
			if(unit!=null) unitComponent=unit.GetComponent<Unit>();
			return unitComponent;
		}
	}
	
	public void SetUnitComponent(){
		if(unit!=null){
			unitComponent=unit.GetComponent<Unit>();
		}
	}
	
	public void MatchValue(int length){
		if(overrideValue.Length==0){
			overrideValue=new int[length];
			return;
		}
		
		int[] value=new int[length];
		for(int i=0; i<value.Length; i++){
			if(i<overrideValue.Length){
				value[i]=overrideValue[i];
			}
			else{
				value[i]=-1;
			}
		}
		overrideValue=value;
	}
	
//	[HideInInspector] public UnitCreep[] unitList;
}

[System.Serializable]
public class Wave{
	[HideInInspector] public int waveID=-1;
	public SubWave[] subWaves=new SubWave[1];
	public float waveInterval;
	//public int resource;
	public int[] resourceGain=new int[1];
	
//	[HideInInspector] public List<UnitCreep> activeUnitList=new List<UnitCreep>();

	//[HideInInspector] 
	public int activeUnitCount=0;
	
	//[HideInInspector] 
	public bool spawned=false; //flag indicating weather all unit in the wave have been spawn
	//[HideInInspector] 
	public bool cleared=false; //flag indicating weather the wave has been cleared
	
	
	public float CalculateSpawnDuration(){
		float duration=0;
		foreach(SubWave subWave in subWaves){
			float tempDuration=subWave.count*subWave.interval+subWave.delay;
			if(tempDuration>duration){
				duration=tempDuration;
			}
		}
		return duration;
	}
}
