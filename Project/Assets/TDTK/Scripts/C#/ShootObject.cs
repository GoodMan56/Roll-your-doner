using UnityEngine;
using System.Collections;

public enum _ShootObjectType{Projectile, Missile, Beam, Effect, FPS}

[AddComponentMenu("TDTK/InGameObject/ShootObject")]
public class ShootObject : MonoBehaviour {

	public _ShootObjectType type;
	
	
	public Unit target;
	private Vector3 targetPos;
	private UnitTower srcTower;
	private UnitCreepAttack srcCreep;
	private Transform shootPoint;
	
	private bool hit=true;
	
	private GameObject thisObj;
	private Transform thisT;
	
	//for instant shootObject
	public LineRenderer lineRenderer;
	public float beamLength=Mathf.Infinity;
	public float duration=0;
	public bool continousDamage=false;
	
	private TrailRenderer trail;
	private float trailDuration;
	
	public AudioClip shootAudio;
	public AudioClip hitAudio;
	
	public GameObject shootEffect;
	public GameObject hitEffect;
	
	//pType: particleType (legacy or shuriken)
	public int pType=1;
	
	[SerializeField] private ParticleSystem pSystem;
	private float startLifeTime;
	private bool looping;

	[SerializeField] private ParticleEmitter pEmitter;
	private float minE=0;
	private float maxE=0;
	
	private float particleDuration;
	
	public bool oneShot=false;
	
	public void SetupParticleEmitter(){
		pType=2;
		pEmitter=thisObj.GetComponent<ParticleEmitter>();
		Debug.Log("get ParticleEmitter   "+pEmitter);
	}
	public ParticleEmitter GetParticleEmitter(){
		return pEmitter;
	}
	
	public void SetupParticleSystem(){
		pType=1;
		pSystem=thisObj.GetComponent<ParticleSystem>();
		Debug.Log("get ParticleSystem   "+pSystem);
	}
	public ParticleSystem GetParticleSystem(){
		return pSystem;
	}
	
	void Awake(){
		thisObj=gameObject;
		thisT=transform;
		
		trail=thisObj.GetComponent<TrailRenderer>();
		if(trail!=null) trailDuration=trail.time;
		
		if(type==_ShootObjectType.Effect){
			
			if(pType==1){
				pSystem=thisObj.GetComponent<ParticleSystem>();
				particleDuration=pSystem.duration+pSystem.startLifetime;
				looping=pSystem.loop;
			}
			
			if(pType==2){
				pEmitter=thisObj.GetComponent<ParticleEmitter>();
				minE=pEmitter.minEmission;
				maxE=pEmitter.maxEmission;
				
				pEmitter.minEmission=0;
				pEmitter.maxEmission=0;
				
				particleDuration=pEmitter.maxEnergy;
			}
			
		}
		
		if(shootEffect!=null) ObjectPoolManager.New(shootEffect, 1);
		if(hitEffect!=null) ObjectPoolManager.New(hitEffect, 1);
	}

	// Use this for initialization
	void Start () {
	
	}
	
	private bool FPSMode=false;
	private float damage;
	private int dmgType;
	public void Shoot(float dmg, int dType){
		damage=dmg;
		dmgType=dType;
		
		StartCoroutine(FPSRoutine());
		StartCoroutine(FPSTimedSelfDestruct());
	}
	
	IEnumerator FPSTimedSelfDestruct(){
		yield return new WaitForSeconds(2.0f);
		if(!hit) Hit();
	}
	
	IEnumerator FPSRoutine(){
		hit=false;
		//~ LayerMask mask=1<<LayerManager.LayerCreep() | 1<<LayerManager.LayerCreepF();
		while(!hit){
			transform.Translate(Vector3.forward * Time.deltaTime * speed);
			yield return null;
		}
	}

	
	void OnTriggerEnter(Collider col){
		if(FPSMode){
			int targetLayer=col.gameObject.layer;
			if(targetLayer==LayerManager.LayerCreep() || targetLayer==LayerManager.LayerCreepF()){
				Unit unit=col.gameObject.GetComponent<Unit>();
				if(unit!=null){
					unit.ApplyDamage(damage, dmgType);
				}
				
				Hit();
			}
			else if(targetLayer==LayerManager.LayerTerrain() || targetLayer==LayerManager.LayerPlatform()){
				Hit();
			}
		}
		else{
			if(col.gameObject.layer==LayerManager.LayerTerrain()){
				hit=true;
			
				if(hitAudio!=null) AudioManager.PlaySound(hitAudio, thisT.position);
				if(hitEffect!=null) ObjectPoolManager.Spawn(hitEffect, thisT.position, Quaternion.identity);
				
				StartCoroutine(Unspawn());
			}
		}
	}
	
	
	public void Shoot(Vector3 tgtPos, UnitCreepAttack src){
		Shoot(tgtPos, src, null);
	}
	public void Shoot(Vector3 tgtPos, UnitCreepAttack src, Transform sp){
		targetPos=tgtPos;
		srcCreep=src;
		shootPoint=sp;
		
		hit=false;
		
		_Shoot();
	}
	
	
	public void Shoot(Unit tgt, UnitCreepAttack src){
		Shoot(tgt, src, null);
	}
	
	public void Shoot(Unit tgt, UnitCreepAttack src, Transform sp){
		target=tgt;
		srcCreep=src;
		shootPoint=sp;
		
		hit=false;
		
		_Shoot();
	}
	
	
	public void Shoot(Unit tgt, UnitTower src){
		Shoot(tgt, src, null);
	}
	
	public void Shoot(Unit tgt, UnitTower src, Transform sp){
		target=tgt;
		srcTower=src;
		shootPoint=sp;
		
		hit=false;
		
		_Shoot();
	}
	
	
	
	public void Shoot(Unit tgt){
		target=tgt;
		hit=false;
		
		_Shoot();
	}
	
	public void _Shoot(){
		
		
		if(type==_ShootObjectType.Projectile) StartCoroutine(ProjectileRoutine());
		else if(type==_ShootObjectType.Missile) StartCoroutine(MissileRoutine());
		else if(type==_ShootObjectType.Beam) StartCoroutine(BeamRoutine());
		//else if(type==_ShootObjectType.Effect) StartCoroutine(EffectRoutine());
	}
	

	
	//for rojectile
	public float speed=10;
	public float maxShootRange=10;
	public float maxShootAngle=20;
	
	IEnumerator ProjectileRoutine() {
		if(target!=null) targetPos=target.GetTargetT().position;
		
		//make sure the shootObject is facing the target and adjust the projectile angle
		thisT.LookAt(targetPos);
		float angle=Mathf.Min(1, Vector3.Distance(thisT.position, targetPos)/maxShootRange)*maxShootAngle;
		//clamp the angle magnitude to be less than 45 or less the dist ratio will be off
		thisT.rotation=thisT.rotation*Quaternion.Euler(Mathf.Clamp(-angle, -42, 42), 0, 0);
		
		Vector3 startPos=thisT.position;
		float iniRotX=thisT.rotation.eulerAngles.x;
		
		if(shootEffect!=null) ObjectPoolManager.Spawn(shootEffect, thisT.position, thisT.rotation);
		
		//while the shootObject havent hit the target
		while(!hit){
			
			//if the target is still active, update the target position
			//if not, the position registered from last loop will be used as the target position
			#if UNITY_4_0
				if(target!=null && target.gameObject.activeInHierarchy){
			#else
				if(target!=null && target.gameObject.active){
			#endif
				targetPos=target.GetTargetT().position;
			}
			
			//calculating distance to targetPos
			float currentDist=Vector3.Distance(thisT.position, targetPos);
			//if the target is close enough, trigger a hit
			if(currentDist<0.25 && !hit) Hit();
			
			//calculate ratio of distance covered to total distance
			float totalDist=Vector3.Distance(startPos, targetPos);
			float invR=1-currentDist/totalDist;
			
			//use the distance information to set the rotation, 
			//as the projectile approach target, it will aim straight at the target
			Quaternion wantedRotation=Quaternion.LookRotation(targetPos-thisT.position);
			float rotX=Mathf.LerpAngle(iniRotX, wantedRotation.eulerAngles.x, invR);
			
			//make y-rotation always face target
			thisT.rotation=Quaternion.Euler(rotX, wantedRotation.eulerAngles.y, wantedRotation.eulerAngles.z);
			
			//Debug.Log(Time.timeScale+"   "+Time.deltaTime);
			
			//move forward
			thisT.Translate(Vector3.forward*Mathf.Min(speed*Time.deltaTime, currentDist));
			
			yield return null;
		}
	}
	
	
	public bool missileInitialBoost=true;
	private float missileSpeedModifier=1f;
	IEnumerator MissileSpeedRoutine(){
		missileSpeedModifier=2.5f;
		duration=0;
		while(duration<1){
			missileSpeedModifier=Mathf.Lerp(3, 1, duration);
			duration+=Time.deltaTime*4f;
			yield return null;
		}
		missileSpeedModifier=1;
	}
	
	IEnumerator MissileRoutine() {
		
		if(missileInitialBoost) StartCoroutine(MissileSpeedRoutine());
		
		Vector3 targetPos=target.GetTargetT().position;
		
		float randX=Random.Range(-maxShootAngle, -5); //randX=30;
		float randY=Random.Range(-30f, 30f);
		float randZ=Random.Range(-10f, 10f);
		
		//make sure the shootObject is facing the target and adjust the projectile angle
		thisT.LookAt(target.thisT);
		float angle=Mathf.Min(1, Vector3.Distance(thisT.position, target.GetTargetT().position)/maxShootRange)*maxShootAngle;
		thisT.rotation=thisT.rotation*Quaternion.Euler(Mathf.Clamp(-angle+randX, -40, 40), randY, randZ);
		
		Vector3 startPos=thisT.position;
		Quaternion iniRot=thisT.rotation;
		
		if(shootEffect!=null) ObjectPoolManager.Spawn(shootEffect, thisT.position, thisT.rotation);
		
		//while the shootObject havent hit the target
		while(!hit){
			
			//if the target is still active, update the target position
			//if not, the position registered from last loop will be used as the target position
			#if UNITY_4_0
				if(target!=null && target.gameObject.activeInHierarchy){
			#else
				if(target!=null && target.gameObject.active){
			#endif
				targetPos=target.GetTargetT().position;
			}
			
			//calculating distance to targetPos
			float currentDist=Vector3.Distance(thisT.position, targetPos);
			//if the target is close enough, trigger a hit
			if(currentDist<0.25 && !hit) Hit();
			
			//calculate ratio of distance covered to total distance
			float totalDist=Vector3.Distance(startPos, targetPos);
			float invR=1-currentDist/totalDist;
			
			//use the distance information to set the rotation, 
			//as the projectile approach target, it will aim straight at the target
			Quaternion wantedRotation=Quaternion.LookRotation(targetPos-thisT.position);
			thisT.rotation=Quaternion.Lerp(iniRot, wantedRotation, invR);
			
			//move forward
			thisT.Translate(Vector3.forward*Mathf.Min(speed*Time.deltaTime*missileSpeedModifier, currentDist));
			
			yield return null;
		}
	}
	
	
	
	IEnumerator BeamRoutine(){
		
		float tempDuration=0.1f;
		if(srcTower!=null) tempDuration=Mathf.Min(srcTower.GetCooldown()*0.5f, duration);
		else if(srcCreep!=null) tempDuration=Mathf.Min(srcCreep.cooldown*0.5f, duration);
		if(tempDuration<=0) tempDuration=Time.deltaTime/2;
		
		if(continousDamage) StartCoroutine(BeamRoutineDamage(tempDuration));
		
		
		Transform sEffectT=null;
		
		if(shootEffect!=null) 
			sEffectT=ObjectPoolManager.Spawn(shootEffect, thisT.position, Quaternion.identity).transform;
		
			while(tempDuration>0){
				if(shootPoint!=null) thisT.position=shootPoint.position;
				
				if(sEffectT!=null) sEffectT.position=thisT.position;
				
				tempDuration-=Time.deltaTime;
				
				if(lineRenderer!=null){
					lineRenderer.SetPosition(0, thisT.position);
					
					float dist=Vector3.Distance(thisT.position, target.GetTargetT().position);
					if(beamLength>=dist){
						lineRenderer.SetPosition(1, target.GetTargetT().position);
					}
					else{
						Ray ray=new Ray(thisT.position, (target.GetTargetT().position-thisT.position));
						lineRenderer.SetPosition(1, ray.GetPoint(beamLength));
					}
				}
				else Debug.Log("null");
				
				if(target.IsDead()){
					StartCoroutine(Unspawn());
					break;
				}
				
				yield return null;
			}

		if(!continousDamage) Hit();
	}
	
	IEnumerator BeamRoutineDamage(float tempDuration){
		int count=(int)Mathf.Max(1, Mathf.Floor(tempDuration/0.1f));
		int countT=count;
		
		while(count>0){
			count-=1;
			
			if(count>0) HitContinous(countT);
			else HitContinousF(countT);
			
			yield return new WaitForSeconds(0.1f);
		}
	}
	
	
	IEnumerator ResetRotation(){
		yield return null;
		thisT.rotation=Quaternion.Euler(-90, 0, 0);
	}
	
	void OnEnable(){
		if(type==_ShootObjectType.Effect){
			if(pType==2 && pEmitter!=null){
				//Debug.Log("pEmitter");
				if(oneShot){
					pEmitter.Emit((int)Random.Range(minE, maxE));
					StartCoroutine(Disable());
				}
				else{
					pEmitter.minEmission=minE;
					pEmitter.maxEmission=maxE;
				}
			}
			else if(pType==1 && pSystem!=null){
				//Debug.Log("pSystem");
				
				//thisT.rotation=Quaternion.Euler(90, 0, 0);
				StartCoroutine(ResetRotation());
				
				if(!looping){
					StartCoroutine(Disable());
				}
				else{
					pSystem.loop=true;
				}
			}
			else Debug.Log(pType+"  error");
		}
	}
	
	public void OnDisable(){
		if(type==_ShootObjectType.Effect){
			if(pType==1 && pEmitter!=null){
				if(!oneShot){
					pEmitter.minEmission=0;
					pEmitter.maxEmission=0;
				}
			}
			else if(pType==2 && pSystem!=null){
				if(looping){
					pSystem.loop=false;
				}
			}
		}
	}
	
	
	public void HitContinous(int count){
		if(srcTower) srcTower.HitTarget(thisT.position, target, false, count);
		else if(srcCreep) srcCreep.HitTarget(thisT.position, target, false, count);
	}
	
	public void HitContinousF(int count){
		if(hitAudio!=null) AudioManager.PlaySound(hitAudio, thisT.position);
		if(hitEffect!=null) ObjectPoolManager.Spawn(hitEffect, target.thisT.position, Quaternion.identity);
		
		if(srcTower) srcTower.HitTarget(thisT.position, target, true, count);
		else if(srcCreep) srcCreep.HitTarget(thisT.position, target, true, count);
		
		StartCoroutine(Unspawn());
	}
	
	public void Hit(){
		hit=true;
		
		if(hitAudio!=null) AudioManager.PlaySound(hitAudio, thisT.position);
		if(hitEffect!=null) ObjectPoolManager.Spawn(hitEffect, target.GetTargetT().position, Quaternion.identity);
		
		if(srcTower){
			srcTower.HitTarget(thisT.position, target);
			srcTower=null;
		}
		else if(srcCreep){
			srcCreep.HitTarget(thisT.position, target);
			srcCreep=null;
		}
		
		StartCoroutine(Unspawn());
	}
	
	
	
	
	IEnumerator Unspawn(){
		//Debug.Log(hit);
		yield return new WaitForSeconds(trailDuration);
		ObjectPoolManager.Unspawn(thisObj);
	}
	
	public IEnumerator Disable(){
		//Debug.Log(hit);
		yield return new WaitForSeconds(particleDuration);
		ObjectPoolManager.Unspawn(thisObj);
	}
	
	
}
