using UnityEngine;
using System.Collections;
using System.Collections.Generic;





[System.Serializable]
public class UnitAttribute{
	public float fullHPDefault=10;
	[HideInInspector] public float fullHP=10;
	[HideInInspector] public float HP=10;
	
	public float fullShieldDefault=0;
	[HideInInspector] public float fullShield=0;
	[HideInInspector] public float shield=10;
	
	public float shieldRechargeRate=0.5f;
	public float shieldStagger=3;
	
	[HideInInspector] public float lastHitTime=0;
	
	public Transform overlayHP;
	public Transform overlayShield;
	public Transform overlayBase;
	public bool alwaysShowOverlay=false;
	
	private Transform cam;
	private bool overlayIsVisible=false;
	
	private Vector3 overlayScaleH;
	private Vector3 overlayScaleS;
	
	private Vector3 overlayPosH;
	private Vector3 overlayPosS;
	
	private Renderer overlayRendererH;
	private Renderer overlayRendererS;
	
	private bool neverShowBase=false;
	
	private Transform rootTransform;
	
	public void Init(Transform transform, int subClass){
		fullHPDefault=Mathf.Max(0, fullHPDefault);
		fullShieldDefault=Mathf.Max(0, fullShieldDefault);
		
		float globalModifierH=1, globalModifierS=1;
		if(subClass==1){
			globalModifierH=GlobalStatsModifier.CreepHP;
			globalModifierS=GlobalStatsModifier.CreepShield;
		}
		else if(subClass==2){
			globalModifierH=GlobalStatsModifier.TowerHP;
			globalModifierS=GlobalStatsModifier.TowerShield;
		}
		
		fullHP=fullHPDefault * globalModifierH;
		HP=fullHP;
		fullShield=fullShieldDefault * globalModifierS;
		shield=fullShield;
		
		rootTransform=transform;
		cam=Camera.main.transform;
		
		if(overlayBase==null){
			if(overlayHP){
				overlayBase=GameObject.CreatePrimitive(PrimitiveType.Plane).transform;
				overlayBase.position=overlayHP.position;
				overlayBase.rotation=overlayHP.rotation;
			}
			else if(overlayShield){
				overlayBase=GameObject.CreatePrimitive(PrimitiveType.Plane).transform;
				overlayBase.position=overlayShield.position;
				overlayBase.rotation=overlayShield.rotation;
			}
			else return;
			
			UnitUtility.DestroyColliderRecursively(overlayBase);
			overlayBase.renderer.enabled=false;
			overlayBase.parent=transform; 
			
			neverShowBase=true;
		}
		
		offsetB=overlayBase.localPosition;
		
		if(overlayHP) scaleModifierH=UnitUtility.GetWorldScale(overlayHP).x*5;
		if(overlayShield) scaleModifierS=UnitUtility.GetWorldScale(overlayShield).x*5;
		
		if(overlayHP!=null) {
			overlayHP.gameObject.layer=LayerManager.LayerOverlay();
			overlayRendererH=overlayHP.renderer;
			overlayHP.parent=overlayBase;
			overlayScaleH=overlayHP.localScale;
			offsetH=overlayHP.localPosition;
			if(alwaysShowOverlay) overlayRendererH.enabled=true;
			else overlayRendererH.enabled=false;
		}
		if(overlayShield!=null) {
			overlayShield.gameObject.layer=LayerManager.LayerOverlay();
			overlayRendererS=overlayShield.renderer;
			overlayShield.parent=overlayBase;
			overlayScaleS=overlayShield.localScale;
			offsetS=overlayShield.localPosition;
			if(alwaysShowOverlay) overlayRendererS.enabled=true;
			else overlayRendererS.enabled=false;
		}
		
		if(alwaysShowOverlay){
			overlayIsVisible=true;
		}
		
	}
	
	public void Reset(Unit unit, int subClass){
		float globalModifierH=1, globalModifierS=1;
		if(subClass==1){
			globalModifierH=GlobalStatsModifier.CreepHP;
			globalModifierS=GlobalStatsModifier.CreepShield;
		}
		else if(subClass==2){
			globalModifierH=GlobalStatsModifier.TowerHP;
			globalModifierS=GlobalStatsModifier.TowerShield;
		}
		
		if(unit.GetTowerPrefabID()>=0){
			float modifier=1+PerkManager.allTowerHPBuffModifier+PerkManager.GetTowerBuffHPMod(unit.GetTowerPrefabID());
			float value=PerkManager.allTowerHPBuffValue+PerkManager.GetTowerBuffHPVal(unit.GetTowerPrefabID());
			fullHP=(fullHPDefault*modifier+value)*globalModifierH;
		}
		else{
			fullHP=fullHPDefault*globalModifierH;
		}
		HP=fullHP;
		
		if(unit.GetTowerPrefabID()>=0){
			fullShield=fullShieldDefault*globalModifierS;
		}
		else{
			fullShield=fullShieldDefault*globalModifierS;
		}
		shield=fullShield;
		
		UpdateOverlay();
	}
	
	public void GainHP(float val){
		HP=Mathf.Min(fullHP, HP+=val);
		UpdateOverlay();
	}
	
	public void ApplyDamage(float dmg){
		lastHitTime=Time.time;
		
		if(shield>0){
			if(dmg>shield){
				dmg-=shield;
				shield=0;
			}
			else{
				shield-=dmg;
			}
		}
		
		if(shield==0) HP-=dmg;
		
		HP=Mathf.Clamp(HP, 0, fullHP);
		UpdateOverlay();
	}
	
	public IEnumerator ShieldRoutine(){
		if(fullShield<=0) yield break;
		
		while(true){
			
			//staggered, stop recharging
			if(Time.time-lastHitTime<shieldStagger){
				yield return null;
			}
			//recharging
			else{
				if(shield<fullShield){
					shield=Mathf.Min(fullShield, shield+Time.deltaTime*shieldRechargeRate);
					UpdateOverlay();
					yield return null;
				}
				else{
					yield return null;
				}
			}
		}
	}
	
	private Vector3 offsetS;
	private Vector3 offsetH;
	private Vector3 offsetB;
	
	private float scaleModifierH=1;
	private float scaleModifierS=1;
	
	public IEnumerator Update(){
				
		if(!overlayHP && !overlayShield) yield break;
		
		Quaternion offset=Quaternion.Euler(-90, 0, 0);
		
		//~ float scaleModifierH=1;
		//~ float scaleModifierS=1;
		
		
		while(true){
			if(overlayIsVisible){
				if(overlayHP || overlayBase){
					Quaternion rot=cam.rotation*offset;
					
					overlayBase.rotation=rot;
					Vector3 dirRight=overlayBase.TransformDirection(-Vector3.right);
					
					if(overlayHP){
						overlayHP.localPosition=offsetH;
						float dist=((fullHP-HP)/fullHP)*scaleModifierH;
						overlayHP.Translate(dirRight*dist, Space.World);
						overlayHP.localRotation=Quaternion.Euler(0, 0, 0);
					}
					
					if(overlayShield && fullShieldDefault>0){
						overlayShield.localPosition=offsetS;
						float dist=((fullShield-shield)/fullShield)*scaleModifierS;
						overlayShield.Translate(dirRight*dist, Space.World);
						overlayShield.localRotation=Quaternion.Euler(0, 0, 0);
					}
				}
			}
			
			yield return null;
		}
	}
	
	
	public void UpdateOverlay(){
		
		if(!overlayHP && !overlayShield) return;
		
		if(!alwaysShowOverlay){
			if(fullShieldDefault>0 && overlayShield){
				if(shield>=fullShield){
					if(!overlayHP || HP>=fullHP)
						overlayShield.renderer.enabled=false;
				}
				else if(shield<=0) overlayShield.renderer.enabled=false;
				else{
					overlayShield.renderer.enabled=true;
				}
			}
			
			if(overlayHP){
				if(!overlayShield){
					if(HP>=fullHP) overlayRendererH.enabled=false;
					else if(HP<=0) overlayRendererH.enabled=false;
					else{
						overlayRendererH.enabled=true;
					}
				}
				else{
					if(fullShield>0 && shield<fullShield){
						overlayHP.renderer.enabled=true;
					}
					else{
						if(HP>=fullHP) overlayRendererH.enabled=false;
						else if(HP<=0) overlayRendererH.enabled=false;
						else{
							overlayRendererH.enabled=true;
						}
					}
				}
			}
			
			if(overlayBase && !neverShowBase){
				if((overlayHP && overlayRendererH.enabled) || (overlayShield && overlayShield.renderer.enabled)){
					overlayBase.renderer.enabled=true;
				}
				else{
					if(HP>=fullHP) overlayBase.renderer.enabled=false;
				}
			}
			
			if((overlayHP && overlayRendererH.enabled) || (overlayShield && overlayShield.renderer.enabled)){
				overlayIsVisible=true;
			}
			else{
				overlayIsVisible=false;
			}
		}
		
		if(overlayHP && overlayRendererH.enabled){
			Vector3 scale=new Vector3(HP/fullHP*overlayScaleH.x, overlayScaleH.y, overlayScaleH.z);
			overlayHP.localScale=scale;
		}
		if(overlayShield && overlayRendererS.enabled){
			Vector3 scale=new Vector3(shield/fullShield*overlayScaleS.x, overlayScaleS.y, overlayScaleS.z);
			overlayShield.localScale=scale;
		}
		
	}
	
	public void ClearParent(){
		if(overlayBase) overlayBase.parent=null;
	}
	
	public void RestoreParent(){
		if(overlayBase){
			overlayBase.parent=rootTransform;
			overlayBase.localPosition=offsetB;
		}
	}
	
}




public class Unit : MonoBehaviour {

	public delegate void PlayerLifeAttackedHandler(Unit unit); 
	public static event PlayerLifeAttackedHandler onLifeUnitAttackedE;
	
	public delegate void DeadHandler(int waveID);
	public static event DeadHandler onDeadE;
	
	[HideInInspector] public int prefabID=-1;
	public string unitName="unit";
	public Texture icon;
	
	public UnitAttribute HPAttribute;
	
	protected bool dead=false;
	protected bool scored=false;
	
	[HideInInspector] public Transform thisT;
	[HideInInspector] public GameObject thisObj;
	
	//waypoint and movement related variable
	protected bool wpMode=false; //if set to true, use wp, else use path
	protected PathTD path;
	protected List<Vector3> wp=new List<Vector3>();
	protected float currentMoveSpd;
	protected float rotateSpd=5f;
	protected int wpCounter=0;
	
	[HideInInspector] public int damageType=0;
	public int armorType=0;
	
	public bool immuneToCrit=false;
	public bool immunity=false;
	public float immunityDuration=0;
	
	[HideInInspector] public bool tieToPlayerLife=false;
	
	//function and configuration to set if this intance has any been inerited by any child instance
	//these functions are call in Awake() of the inherited clss
	private enum _UnitSubClass{None, Creep, Tower};
	private _UnitSubClass subClass=_UnitSubClass.None;
	private UnitCreep unitC;
	private UnitTower unitT;
	//Call by inherited class UnitCreep, caching inherited UnitCreep instance to this instance
	public void SetSubClassInt(UnitCreep unit){ 
		unitC=unit; 
		subClass=_UnitSubClass.Creep;
	}
	//Call by inherited class UnitTower, caching inherited UnitTower instance to this instance
	public void SetSubClassInt(UnitTower unit){ 
		unitT=unit; 
		subClass=_UnitSubClass.Tower;
	}
	public int GetTowerPrefabID(){
		if(unitT!=null) return unitT.prefabID;
		else return -1;
	}

	public virtual void Awake(){
		thisT=transform;
		thisObj=gameObject;
		
		//if(subClass==_UnitSubClass.Creep) 
			HPAttribute.Init(thisT, (int)subClass);
		
		UnitUtility.DestroyColliderRecursively(thisT);
	}


	// Use this for initialization
	public virtual void Start () {
		Init();
	}
	
	
	public virtual void Init(){
		HPAttribute.Reset(this, (int)subClass);
		StartCoroutine(HPAttribute.Update());
		StartCoroutine(HPAttribute.ShieldRoutine());
		
		if(unitT!=null){
			dmgReducMod=PerkManager.GetTowerBuffAttMod(unitT.prefabID);
			dmgReducVal=PerkManager.GetTowerBuffAttVal(unitT.prefabID);
		}
		
		dead=false;
		scored=false;
		stunned=false;
		slowModifier=1;
		slowEffect=new List<Slow>();
	}
	
	/*
	void OnEnable(){
		PerkManager.onBuffAllTowerHPE += OnBuffTowerHP;
		PerkManager.onBuffTowerDefE += OnBuffTowerDef;
	}
	
	void OnDisable(){
		PerkManager.onBuffAllTowerHPE -= OnBuffTowerHP;
		PerkManager.onBuffTowerDefE -= OnBuffTowerDef;
	}
	
	void OnBuffTowerDef(Perk perk){
		if(subClass==_UnitSubClass.Tower){
			if(perk.towerID==unitT.prefabID){
				if(perk.modType==_ModifierType.percentage) dmgReducMod+=perk.value;
				else if(perk.modType==_ModifierType.value) dmgReducVal+=perk.value;
			}
		}
	}
	
	void OnBuffTowerHP(Perk perk){
		if(subClass==_UnitSubClass.Tower){
			bool flag1=perk.type==_PerkType.BuffTowerHP;
			//~ bool flag2=(perk.type==_PerkType.SpecificBuffTowerHP && perk.towerID==GetTowerprefabID());
			bool flag2=(perk.type==_PerkType.SpecificBuffTowerHP && perk.towerID==unitT.prefabID);
			//~ bool flag=flag1 | flag2;
			if(flag1 || flag2){
				if(perk.modType==_ModifierType.percentage){
					float dif=HPAttribute.fullHPDefault*(perk.value);
					HPAttribute.fullHP+=dif;
					HPAttribute.HP+=dif;
				}
				else if(perk.modType==_ModifierType.value){
					HPAttribute.fullHP+=perk.value;
					HPAttribute.HP+=perk.value;
				}
			}
			//~ if(perk.type=_PerkType.BuffTowerHP){
				//~ if(modType==_ModifierType.percentage){
					//~ float dif=HPAttribute.fullHPDefault*(perk.value);
					//~ HPAttribute.fullHP+=dif;
					//~ HPAttribute.HP+=dif;
				//~ }
				//~ else if(modType==_ModifierType.value){
					//~ HPAttribute.fullHP+=perk.value;
					//~ HPAttribute.HP+=perk.value;
				//~ }
			//~ }
			//~ else if(perk.type=_PerkType.SpecificBuffTowerHP){
				//~ if(perk.towerID==GetTowerprefabID()){
					//~ if(modType==_ModifierType.percentage){
						//~ float dif=HPAttribute.fullHPDefault*(perk.value);
						//~ HPAttribute.fullHP+=dif;
						//~ HPAttribute.HP+=dif;
					//~ }
					//~ else if(modType==_ModifierType.value){
						//~ HPAttribute.fullHP+=perk.value;
						//~ HPAttribute.HP+=perk.value;
					//~ }
				//~ }
			//~ }
		}
	}
	*/
	
	//~ void OnGUI(){
		//~ Vector3 screenPos = Camera.main.WorldToScreenPoint(thisT.position);
		//~ GUI.Label(new Rect(screenPos.x-20, Screen.height-screenPos.y-25, 150, 25), currentMoveSpd.ToString()+"  "+stunned);
		//~ GUI.Label(new Rect(screenPos.x-20, Screen.height-screenPos.y, 150, 25), HPAttribute.fullHP.ToString("f0")+"  "+HPAttribute.HP.ToString("f0"));
	//~ }
	
	
	public void SetFullHP(float hp){
		HPAttribute.fullHPDefault=hp;
		HPAttribute.fullHP=hp;
		HPAttribute.HP=HPAttribute.fullHP;
	}
	
	public void SetFullShield(float shield){
		HPAttribute.fullShieldDefault=shield;
		HPAttribute.fullShield=shield;
		HPAttribute.shield=HPAttribute.fullShield;
	}
	
	public void GainHP(float val){
		HPAttribute.GainHP(val);
	}
	
	public float GetFullHP(){
		return HPAttribute.fullHP;
	}
	
	public float GetCurrentHP(){
		return HPAttribute.HP;
	}
	
	//a test function call to demonstrate overlay in action
	IEnumerator TestOverlay(){
		yield return new WaitForSeconds(0.75f);
		while(true){
			ApplyDamage(0.1f*HPAttribute.fullHP*0.1f, 0);
			yield return new WaitForSeconds(0.1f);
		}
	}
	
	// Update is called once per frame
	public virtual void Update () {
		
	}
	
	
	public virtual bool MoveToPoint(Vector3 point){
		return false;
		
		float dist=Vector3.Distance(point, thisT.position);
		
		//if the unit have reached the point specified
		if(dist<0.15f) return true;
		
		//rotate towards destination
		Quaternion wantedRot=Quaternion.LookRotation(point-thisT.position);
		thisT.rotation=Quaternion.Slerp(thisT.rotation, wantedRot, rotateSpd*Time.deltaTime);
		
		//move, with speed take distance into accrount so the unit wont over shoot
		Vector3 dir=(point-thisT.position).normalized;
		thisT.Translate(dir*Mathf.Min(dist, currentMoveSpd * Time.deltaTime * slowModifier), Space.World);
		
		return false;
	}

	
	public void ApplyDamage(float dmg, int dmgType){
		if(immunity) return;
		
		float globalModifier=1;
		if(subClass==_UnitSubClass.Tower){
			globalModifier=GlobalStatsModifier.CreepToTowerDmg;
		}
		else if(subClass==_UnitSubClass.Creep){
			globalModifier=GlobalStatsModifier.TowerToCreepDmg;
		}
		
		dmg=CalculateDamage(dmg, dmgType)*globalModifier;
		
		HPAttribute.ApplyDamage(dmg);
		
		//Debug.Log(dmg +"  "+HPAttribute.HP);
		
		if(tieToPlayerLife){
			if(onLifeUnitAttackedE!=null) onLifeUnitAttackedE(this);
		}
		
		if(subClass==_UnitSubClass.Tower){
			//Debug.Log(CalculateDamage(dmg, dmgType)+"   "+HPAttribute.HP);
		}
		
		if(subClass==_UnitSubClass.Creep && !dead) unitC.PlayHit();
		
		if(HPAttribute.HP<=0 && !dead){
			HPAttribute.HP=0;
			dead=true;
			
			if(subClass==_UnitSubClass.Creep){
				unitC.Dead();
				if(onDeadE!=null) onDeadE(unitC.waveID);
			}
			else if(subClass==_UnitSubClass.Tower){
				unitT.Dead();
			}
			else{
				ObjectPoolManager.Unspawn(thisObj);
			}
			
		}
		
		//UpdateOverlay();
		//HPAttribute.UpdateOverlayRenderer();
	}
	
	[HideInInspector] public float dmgReducMod=0;
	[HideInInspector] public float dmgReducVal=0;
	float CalculateDamage(float dmg, int dmgType){
		if(subClass==_UnitSubClass.Tower){
			float damage=dmg*DamageTable.GetModifier(armorType, dmgType);
			damage*=Mathf.Clamp(1-(PerkManager.allTowerDefBuffModifier+dmgReducMod), 0, 1);
			damage=Mathf.Max(0, damage-=(PerkManager.allTowerDefBuffValue+dmgReducVal));
			return damage;
		}
		else{
			float damage=dmg*DamageTable.GetModifier(armorType, dmgType);
			damage*=Mathf.Clamp(1+(dmgReducMod), 0, 1);
			damage=Mathf.Max(0, damage+=(dmgReducVal));
			return damage;
			//~ return dmg*DamageTable.GetModifier(armorType, dmgType);
		}
	}

	
	public bool stunned=false;
	private float stunnedDuration=0;
	public bool immuneToStun=false;
	public void ApplyStun(float stun){
		if(immuneToStun) return;
		if(stun>stunnedDuration) stunnedDuration=stun;
		if(!stunned){
			stunned=true;
			if(subClass==_UnitSubClass.Creep) unitC.Stunned();
			StartCoroutine(StunRoutine());
		}
	}
	
	IEnumerator StunRoutine(){
		while(stunnedDuration>0){
			stunnedDuration-=Time.deltaTime;
			yield return null;
		}
		stunned=false;
		
		if(subClass==_UnitSubClass.Creep) unitC.Unstunned();
	}
	
	private List<Slow> slowEffect=new List<Slow>();
	private bool slowRoutine=false;
	protected float slowModifier=1.0f;
	public bool immuneToSlow=false;
	
	public void ApplySlow(Slow slow){
		if(!immuneToSlow){
			slow.SetTimeEnd(Time.time+slow.duration);
			slowEffect.Add(slow);
			if(!slowRoutine) StartCoroutine(SlowRoutine());
			
			if(subClass==_UnitSubClass.Creep) unitC.UpdateMoveAnimationSpeed();
		}
	}
	
	private IEnumerator SlowRoutine(){
		slowRoutine=true;
		while(slowEffect.Count>0){
			float targetVal=1.0f;
			for(int i=0; i<slowEffect.Count; i++){
				Slow slow=slowEffect[i];
				
				//check if the effect has expired
				if(Time.time>=slow.GetTimeEnd()){
					slowEffect.RemoveAt(i);
					i--;
				}
				
				//if the effect is not expire, check the slowFactor
				//record the val if the slowFactor is slower than the previous entry
				else if(1-slow.slowFactor<targetVal){
					targetVal=1-slow.slowFactor;
					targetVal=Mathf.Max(0, targetVal);
				}
			}
			
			slowModifier=Mathf.Lerp(slowModifier, targetVal, Time.deltaTime*10);
			if(subClass==_UnitSubClass.Creep) unitC.UpdateMoveAnimationSpeed();
			yield return null;
		}
		slowRoutine=false;
		
		while(slowEffect.Count==0){
			slowModifier=Mathf.Lerp(slowModifier, 1, Time.deltaTime*10);
			if(subClass==_UnitSubClass.Creep) unitC.UpdateMoveAnimationSpeed();
			yield return null;
		}
		
		if(subClass==_UnitSubClass.Creep) unitC.UpdateMoveAnimationSpeed();
	}
	
	
	
	public void ApplyDot(Dot dot, int dmgType){
		if(dot.stackable) StartCoroutine(DotRoutineStack(dot, dmgType));
		else if(!dot.stackable){
			dotTick=1;
			currentDotDmgType=dmgType;
			dotTickMax=(int)Mathf.Round(dot.duration/dot.interval);
			if(currentDot==null){
				currentDot=dot;
				StartCoroutine(DotRoutine());
			}
		}
	}
	
	private IEnumerator DotRoutineStack(Dot dot, int dmgType){
		int tick=1;
		int tickMax=(int)Mathf.Round(dot.duration/dot.interval);
		//Debug.Log("dot - stack");
		while(tick<=tickMax){
			ApplyDamage(dot.SampleDamage(dotTick), dmgType);
			//Debug.Log("dot tick "+dotTick+": "+dot.SampleDamage(dotTick));
			tick+=1;
			yield return new WaitForSeconds(dot.interval);
		}
	}
	
	private Dot currentDot;
	private int currentDotDmgType;
	private int dotTick;
	private int dotTickMax;
	private IEnumerator DotRoutine(){
		//Debug.Log("dot - not stack");
		while(dotTick<=dotTickMax){
			ApplyDamage(currentDot.SampleDamage(dotTick), currentDotDmgType);
			//Debug.Log("dot tick "+dotTick+": "+currentDot.SampleDamage(dotTick));
			dotTick+=1;
			yield return new WaitForSeconds(currentDot.interval);
		}
		currentDot=null;
	}
	
	
	public void RestoreHP(float val){
		StartCoroutine(RestoreHPRoutine(val));
	}
	IEnumerator RestoreHPRoutine(float val){
		float duration=0;
		while(duration<0.5){
			HPAttribute.GainHP(Time.deltaTime*val*2);
			duration+=Time.deltaTime;
			yield return null;
		}
	}
	
	
	public void ShieldUp(float val){
		immunity=true;
		if(immunityDuration<=0){
			immunityDuration	=val;
			StartCoroutine(ImmunityRoutine());
		}
		else immunityDuration	=val;
	}
	
	public IEnumerator ImmunityRoutine(){
		while(immunityDuration>0){
			immunityDuration-=Time.deltaTime;
			yield return null;
		}
		immunity=false;
	}
	
	
	protected bool stopMoving=false;
	public void StopMoving(){
		currentMoveSpd=0;
		stopMoving=true;
	}
	public void ResumeMoving(){
		stopMoving=false;
		if(unitC!=null) currentMoveSpd=unitC.moveSpeed;
		else if(unitT!=null) currentMoveSpd=unitC.moveSpeed;
	}
	public bool HasStoppedMoving(){
		return stopMoving;
	}
	
	public float GetSlowModifier(){
		return slowModifier;
	}
	
	public float GetDefaultMoveSpeed(){
		if(unitC!=null) return unitC.moveSpeed;
		
		return currentMoveSpd;
	}
	
	public int GetWPCounter(){
		return wpCounter;
	}
	
	public void SetWPCounter(int num){
		wpCounter=num;
	}
	
	public bool IsStunned(){
		return stunned;
	}
	
	public bool IsDead(){
		return dead;
	}
	
	public float GetDistanceMovedCreep(){
		return unitC != null ? unitC.GetDistanceMoved() : 0;
	}
	
	public Transform targetPointT;
	public Transform GetTargetT() {
  		return targetPointT != null ? targetPointT : thisT;
	} 
	
	[HideInInspector] public int designatedDeathAni=-1;
	public void SetDeathAnimation(int aniID){
		designatedDeathAni=aniID;
	}
}
