
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public enum _AttackCreepType{Attack, Support}
public enum _TargetingA{AllAround, FrontalCone, Obstacle};
public enum _AttackMode{RunNGun, StopNAttack};
public enum _AttackMethod{Range, Melee};
public enum _CDTracking{Easy, Precise};

[RequireComponent (typeof (UnitCreep))]

[AddComponentMenu("TDTK/InGameObject/CreepAttack")]
public class UnitCreepAttack : MonoBehaviour {

	public delegate void BuffHandler(UnitCreepAttack unitCreepAttack);
	public static event BuffHandler buffE;
	
	void Enable(){
		UnitCreepAttack.buffE += Buff;
	}
	
	void Disable(){
		UnitCreepAttack.buffE -= Buff;
	}
	
	void Buff(UnitCreepAttack src){
		if(Vector3.Distance(src.unit.thisT.position, unit.thisT.position)<src.range){
			
		}
	}
	
	
	
	[HideInInspector] public UnitCreep unit;
	
	public _AttackCreepType type=_AttackCreepType.Attack;
	
	public float meleeAttackRange=2f;
	enum _MeleeState{OutOfRange, Attacking}
	private _MeleeState meleeState=_MeleeState.OutOfRange;
	
	
	public _TargetingA targetArea;
	//public _TargetMode targetMode;
	public float frontalConeAngle=35f;
	public Transform turretObject;
	public Transform barrelObject;
	private bool targetInLOS=false;
	
	
	public _AttackMode attackMode;
	
	private Unit target;
	private float currentTargetDist;
	
	
	public _AttackMethod attackMethod;
	
	
	public _CDTracking cdTracking;
	
	public GameObject shootObject;
	public Transform[] shootPoint=new Transform[0];
	public float range=5;
	public float cooldown=2;
	public float damage=1;
	public float stun=0;
	public BuffStat buff;
	
	private int clipSize=-1;
	private float reloadDuration=4;
	private int currentClip=1;
	
	public AudioClip attackSound;
	public AnimationClip animationIdle;
	public AnimationClip animationAttack;
	public float aniAttackTimeOffset=0.1f;
	
	protected bool dash=false;
	protected float dashFactor=1.5f;
	
	
	void Awake(){
		unit=gameObject.GetComponent<UnitCreep>();
		if(unit==null) return;
		
		if(animationAttack!=null) unit.SetAttackAnimation(animationAttack);
		if(animationIdle!=null) unit.SetIdleAnimation(animationIdle);
		
		if(shootObject!=null) ObjectPoolManager.New(shootObject, 2);
		
		UpdateShootPointNBarrelObj();
	}

	// Use this for initialization
	void Start () {
		if(type==_AttackCreepType.Attack && shootObject!=null){
			ShootObject shootObj=shootObject.GetComponent<ShootObject>();
			if(shootObj!=null && shootObj.type==_ShootObjectType.Projectile){
				turretMaxAngle=shootObj.maxShootAngle;
				turretMaxRange=shootObj.maxShootRange;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		if(type==_AttackCreepType.Attack){
			if(target!=null){
				if(attackMethod==_AttackMethod.Melee){
					if(attackMode==_AttackMode.StopNAttack){
						LayerMask mask=1<<LayerManager.LayerTower();
						if(!Physics.Raycast(unit.thisT.position+new Vector3(0, 1, 0), unit.thisT.forward, meleeAttackRange, mask)){
							MoveToPoint(target.GetTargetT().position);
						}
						else{
							if(meleeState!=_MeleeState.Attacking){
								unit.StopAnimation();
								meleeState=_MeleeState.Attacking;
								targetInLOS=true;
							}
						}
					}
					else{
						LayerMask mask=1<<LayerManager.LayerTower();
						
						if(Physics.Raycast(unit.thisT.position+new Vector3(0, 1, 0), unit.thisT.forward, meleeAttackRange, mask)){
							if(meleeState!=_MeleeState.Attacking){
								//unit.StopAnimation();
								meleeState=_MeleeState.Attacking;
								targetInLOS=true;
							}
						}
						else{
							if(meleeState==_MeleeState.Attacking){
								meleeState=_MeleeState.OutOfRange;
								targetInLOS=false;
							}
						}
					}
				}
				else if(attackMethod==_AttackMethod.Range){
					if(turretObject!=null){
						TurretRoutine();
						
						Quaternion wantedRot=Quaternion.LookRotation(target.GetTargetT().position-unit.thisT.position);
						
						//~ turretObject.rotation=Quaternion.Slerp(turretObject.rotation, wantedRot, Time.deltaTime*10);
						
						//~ if(Quaternion.Angle(wantedRot, turretObject.rotation)<10){
							//~ targetInLOS=true;
						//~ }
						//~ else targetInLOS=false;
						
						if(attackMode==_AttackMode.StopNAttack){
							Vector3 point=target.GetTargetT().position;
							point.y=unit.thisT.position.y;
							wantedRot=Quaternion.LookRotation(point-unit.thisT.position);
							unit.thisT.rotation=Quaternion.Slerp(unit.thisT.rotation, wantedRot, 10*Time.deltaTime);
						}
					}
					else{
						if(attackMode==_AttackMode.StopNAttack){
							Vector3 point=target.GetTargetT().position;
							point.y=unit.thisT.position.y;
							
							Quaternion wantedRot=Quaternion.LookRotation(point-unit.thisT.position);
							unit.thisT.rotation=Quaternion.Slerp(unit.thisT.rotation, wantedRot, 10*Time.deltaTime);
							//Debug.Log("range, stop and attack   "+Quaternion.Angle(unit.thisT.rotation, wantedRot));
							if(Quaternion.Angle(unit.thisT.rotation, wantedRot)<10){
								if(!targetInLOS) targetInLOS=true;
							}
							else if(targetInLOS) targetInLOS=false;
						}
						else{
							if(!targetInLOS) targetInLOS=true;
						}
					}
				}
				
			}
			else{
				if(targetInLOS) targetInLOS=true;
				
				if(attackMethod==_AttackMethod.Melee){
					
				}
				else if(attackMethod==_AttackMethod.Range){
					if(turretObject!=null){
						//~ Quaternion wantedRot=Quaternion.Euler(0, 0, 0);
						
						//~ turretObject.localRotation=Quaternion.Slerp(turretObject.localRotation, wantedRot, Time.deltaTime*10);
						
						ResetTurretRoutine();
					}
				}
			}
		}
		else if(type==_AttackCreepType.Support){
			if(attackMode==_AttackMode.StopNAttack){
				
			}
			else if(attackMode==_AttackMode.RunNGun){
				
			}
		}
	}
	

	public _TurretAni animateTurret;
	public _LOSMode losMode=_LOSMode.AimOnly;
	private float turretMaxAngle=0;
	private float turretMaxRange=0;
	public float turretDefaultElevatedAngle=0;
	
	public _RotationMode turretRotationModel=_RotationMode.FullTurret;
	public float aimTolerance=15f;
	public float turretRotateSpeed=15f;
	
	void TurretRoutine(){
		if(animateTurret!=_TurretAni.None && !unit.IsStunned()){
				
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
							
							wantedRot=Quaternion.LookRotation(targetPos-barrelObject.position);
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
	
	void ResetTurretRoutine(){
		if(animateTurret!=_TurretAni.None && !unit.IsStunned()){
				
				if(turretObject!=null){
					Quaternion wantedRot=Quaternion.Euler(0, unit.thisT.rotation.eulerAngles.y, 0);
					turretObject.rotation=Quaternion.Slerp(turretObject.rotation, wantedRot, turretRotateSpeed*Time.deltaTime);
				}
				
				if(animateTurret==_TurretAni.Full && turretRotationModel==_RotationMode.SeparatedBarrel){
					if(barrelObject!=null){
						Quaternion wantedRot=Quaternion.Euler(turretDefaultElevatedAngle, barrelObject.rotation.eulerAngles.y, 0);
						barrelObject.rotation=Quaternion.Slerp(barrelObject.rotation, wantedRot, turretRotateSpeed*Time.deltaTime);
					}
				}
			
				//~ if(animateTurret==_TurretAni.YAxis){
					//~ Quaternion wantedRot=Quaternion.Euler(0, unit.thisT.rotation.eulerAngles.y, 0);
					//~ turretObject.rotation=Quaternion.Slerp(turretObject.rotation, wantedRot, turretRotateSpeed*Time.deltaTime);
				//~ }
				//~ else if(animateTurret==_TurretAni.Full){
					//~ if(turretRotationModel==_RotationMode.FullTurret){
						//~ Quaternion wantedRot=Quaternion.Euler(0, unit.thisT.rotation.eulerAngles.y, 0);
						//~ turretObject.rotation=Quaternion.Slerp(turretObject.rotation, wantedRot, turretRotateSpeed*Time.deltaTime);
					//~ }
					//~ else if(turretRotationModel==_RotationMode.SeparatedBarrel){
						//~ Quaternion wantedRot=Quaternion.Euler(0, unit.thisT.rotation.eulerAngles.y, 0);
						//~ turretObject.rotation=Quaternion.Slerp(turretObject.rotation, wantedRot, turretRotateSpeed*Time.deltaTime);
						
						//~ if(barrelObject!=null){
							//~ wantedRot=Quaternion.Euler(turretDefaultElevatedAngle, barrelObject.rotation.eulerAngles.y, 0);
							//~ barrelObject.rotation=Quaternion.Slerp(barrelObject.rotation, wantedRot, turretRotateSpeed*Time.deltaTime);
						//~ }
					//~ }
				//~ }

		}
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
		shootPoint[0]=unit.thisT;
		return;
	}
	
	
	
	
	public bool MoveToPoint(Vector3 point){
		point.y=unit.thisT.position.y;
		
		float dist=Vector3.Distance(point, unit.thisT.position);
		
		if(dist<0.15f) {
			//if the unit have reached the point specified
			return true;
		}
		
		Quaternion wantedRot=Quaternion.LookRotation(point-unit.thisT.position);
		unit.thisT.rotation=Quaternion.Slerp(unit.thisT.rotation, wantedRot, 10*Time.deltaTime);
		
		Vector3 dir=(point-unit.thisT.position).normalized;
		unit.thisT.Translate(dir*Mathf.Min(dist, dashFactor*unit.GetDefaultMoveSpeed() * Time.deltaTime *unit.GetSlowModifier()), Space.World);
		
		return false;
	}
	
	void OnEnable(){
		if(unit.thisT!=null){
			StartCoroutine(ScanForTarget());
			if(type==_AttackCreepType.Attack){
				StartCoroutine(AttackRoutine());
			}
			else{
				StartCoroutine(SupportRoutine());
			}
			
			if(!dash) dashFactor=1;
		}
	}
	
	
	
	IEnumerator Reload(){
		yield return new WaitForSeconds(reloadDuration);
		currentClip=clipSize;
	}
	
	public bool attackStrategicPointOnly=false;
	
	IEnumerator ScanForTarget(){
		LayerMask maskTarget=1<<LayerManager.LayerTower();
		while(true){
			if(target==null){
				if(unit.HasStoppedMoving()){
					unit.ResumeAnimation();
					unit.ResumeMoving();
				}
				
				if(attackStrategicPointOnly){
					List<Unit> list=GameControl.GetLifeUnit();
					
					foreach(Unit targetUnit in list){
						if(targetUnit!=null && targetUnit.HPAttribute.HP>0){
							if(Vector3.Distance(unit.thisT.position, targetUnit.GetTargetT().position)<=range){
								target=targetUnit;
										
								if(attackMode==_AttackMode.StopNAttack){
									if(attackMethod!=_AttackMethod.Melee) unit.StopAnimation();
									unit.StopMoving();
								}
							}
						}
					}
				}
				else{
					
					if(targetArea==_TargetingA.AllAround){
						
							Collider[] cols=Physics.OverlapSphere(unit.thisT.position, range, maskTarget);
							//if(cols!=null && cols.Length>0) Debug.Log(cols[0]);
						
							if(cols.Length>0){
								Collider currentCollider=cols[Random.Range(0, cols.Length)];
								Unit targetTemp=currentCollider.gameObject.GetComponent<Unit>();
								if(targetTemp!=null && targetTemp.HPAttribute.HP>0){
									target=targetTemp;
									
									if(attackMode==_AttackMode.StopNAttack){
										if(attackMethod!=_AttackMethod.Melee) unit.StopAnimation();
										unit.StopMoving();
										//if(dash){
										//	if(Vector3.Distance(unit.thisT.position, target.thisT.position)>8){
										//		//unit.PlayDash();
										//	}
										//}
									}
								}
							}
						
					}
					else if(targetArea==_TargetingA.FrontalCone){
						
							Collider[] cols=Physics.OverlapSphere(unit.thisT.position, range, maskTarget);
							//if(cols!=null && cols.Length>0) Debug.Log(cols[0]);
						
							if(cols.Length>0){
								Collider currentCollider=cols[0];
								foreach(Collider col in cols){
									Quaternion targetRot=Quaternion.LookRotation(col.transform.position-unit.thisT.position);
									if(Quaternion.Angle(targetRot, unit.thisT.rotation)<frontalConeAngle){
										Unit targetTemp=currentCollider.gameObject.GetComponent<Unit>();
										if(targetTemp!=null && targetTemp.HPAttribute.HP>0){
											target=targetTemp;
											if(attackMode==_AttackMode.StopNAttack){
												if(attackMethod!=_AttackMethod.Melee) unit.StopAnimation();
												unit.StopMoving();
												//if(dash){
												//	if(Vector3.Distance(unit.thisT.position, target.thisT.position)>8){
												//		//unit.PlayDash();
												//	}
												//}
											}
											break;
										}
									}
								}
							}
						
					}
					else if(targetArea==_TargetingA.Obstacle){
						
							RaycastHit hit;
							if(Physics.Raycast(unit.thisT.position, unit.thisT.forward, out hit, range, maskTarget)){
								Unit targetTemp=hit.collider.gameObject.GetComponent<Unit>();
								if(targetTemp!=null && targetTemp.HPAttribute.HP>0){
									target=targetTemp;
									if(attackMode==_AttackMode.StopNAttack){
										if(attackMethod!=_AttackMethod.Melee) unit.StopAnimation();
										unit.StopMoving();
										//if(dash){
										//	if(Vector3.Distance(unit.thisT.position, target.thisT.position)>8){
										//		//unit.PlayDash();
										//	}
										//}
									}
								}
							}
						
					}
				}
				
			}
			else{
				
				//if target is out of range or dead or inactive, clear target
				currentTargetDist=Vector3.Distance(unit.thisT.position, target.GetTargetT().position);
				#if UNITY_4_0
					if(currentTargetDist>range*1.25f || target.HPAttribute.HP<=0 || !target.thisObj.activeInHierarchy){
				#else
					if(currentTargetDist>range*1.25f || target.HPAttribute.HP<=0 || !target.thisObj.active){
				#endif
					target=null;
					if(attackMode==_AttackMode.StopNAttack){
						unit.ResumeAnimation();
						unit.ResumeMoving();
						//Debug.Log("target cleared");
					}
					if(meleeState==_MeleeState.Attacking){
						meleeState=_MeleeState.OutOfRange;
					}
				}
			}
			yield return null;
		}
	}
	
	private List<Collider> colList=new List<Collider>();
	IEnumerator SupportRoutine(){
		buff.buffID=unit.GetUnitID();
		LayerMask maskCreep=1<<LayerManager.LayerCreep() | 1<<LayerManager.LayerCreepF();
		while(true){
			if(!unit.IsStunned() && !unit.IsDead()){
				Collider[] cols=Physics.OverlapSphere(unit.thisT.position, range, maskCreep);
				
				List<Collider> tempList=new List<Collider>();
				foreach(Collider col in cols){
					tempList.Add(col);
				}
				
				for(int i=0; i<colList.Count; i++){
					if(!tempList.Contains(colList[i])){
						colList[i].gameObject.GetComponent<UnitCreep>().UnBuff(buff.buffID);
						colList.RemoveAt(i);
						i--;
					}
				}
				
				for(int i=0; i<tempList.Count; i++){
					if(!colList.Contains(tempList[i])){
						tempList[i].gameObject.GetComponent<UnitCreep>().Buff(buff);
					}
				}
				
				yield return new WaitForSeconds(0.2f);
			}
			else yield return null;
		}
	}
	
	//called by a support creep to buff this creep
	//~ private List<BuffStat> activeBuffList=new List<BuffStat>();
	//~ public void Buff(BuffStat newBuff){
		//~ if(activeBuffList.Contains(newBuff)) return;
		
		//~ activeBuffList.Add(newBuff);
		
		//~ damage=damage*(1+newBuff.damageBuff);
		//~ range=range*(1+newBuff.rangeBuff);
		//~ cooldown=cooldown*(1-newBuff.cooldownBuff);
		//~ if(newBuff.regenHP>0) StartCoroutine(RegenHPRoutine(newBuff));
	//~ }

	
	//~ //remove buff effect
	//~ public void UnBuff(int buffID){
		//~ BuffStat tempBuff;
		//~ for(int i=0; i<activeBuffList.Count; i++){
			//~ tempBuff=activeBuffList[i];
			//~ if(tempBuff.buffID==buffID){
				//~ damage=damage/(1+tempBuff.damageBuff);
				//~ range=range/(1+tempBuff.rangeBuff);
				//~ cooldown=cooldown/(1-tempBuff.cooldownBuff);
				
				//~ activeBuffList.RemoveAt(i);
				
				//~ break;
			//~ }
		//~ }
	//~ }
	
	//~ IEnumerator RegenHPRoutine(BuffStat buff){
		//~ while(activeBuffList.Contains(buff)){
			//~ //HPAttribute.HP=Mathf.Min(HPAttribute.fullHP, HP+);
			//~ unit.GainHP(Time.deltaTime*buff.regenHP);
			//~ yield return null;
		//~ }
	//~ }
	

	IEnumerator AttackRoutine(){
		if(shootPoint==null || shootPoint.Length==0){
			shootPoint=new Transform[1];
			shootPoint[0]=unit.thisT;
		}
		while(true){
			
			float delay=0;
			
			if(target!=null && !unit.IsStunned() && !unit.IsDead() && targetInLOS && currentClip>0){
				
				Vector3 targetPoint=target.GetTargetT().position;
				
				if(unit.PlayAttack()){
					delay=aniAttackTimeOffset;
					yield return new WaitForSeconds(delay);
				}
				
				if(target!=null){
					if(attackMethod==_AttackMethod.Range){
						if(shootObject!=null){
							foreach(Transform sPoint in shootPoint){
								GameObject obj=ObjectPoolManager.Spawn(shootObject, sPoint.position, sPoint.rotation);
								ShootObject shootObj=obj.GetComponent<ShootObject>();
								shootObj.Shoot(target, this, sPoint);
							}
						}
					}
					else if(attackMethod==_AttackMethod.Melee){
						ApplyEffect(target);
					}
				}
				else{
					if(attackMethod==_AttackMethod.Range){
						if(shootObject!=null){
							foreach(Transform sPoint in shootPoint){
								GameObject obj=ObjectPoolManager.Spawn(shootObject, sPoint.position, sPoint.rotation);
								ShootObject shootObj=obj.GetComponent<ShootObject>();
								shootObj.Shoot(targetPoint, this, sPoint);
							}
						}
					}
				}
				
				if(attackSound!=null) AudioManager.PlaySound(attackSound, unit.thisT.position);
				
				yield return new WaitForSeconds(cooldown-delay);
			}
			else{
				//~ Debug.Log("target:"+target+"  stunned:"+unit.IsStunned()+"  targetInLOS:"+targetInLOS+"   clipSize:"+currentClip);
				
				if(cdTracking==_CDTracking.Precise) yield return null;
				else yield return new WaitForSeconds(Random.Range(0, cooldown));
			}
		
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
		#if UNITY_4_0
			if(tgt!=null && tgt.gameObject!=null && tgt.gameObject.activeInHierarchy){
		#else
			if(tgt!=null && tgt.gameObject!=null && tgt.gameObject.active){
		#endif
			ApplyEffect(tgt, effect, div);
		}
	}
	
	//check stat and apply valid effect to the target
	void ApplyEffect(Unit targetUnit, bool effect, int div){
		#if UNITY_4_0
			if(targetUnit.thisObj.activeInHierarchy){
		#else
			if(targetUnit.thisObj.active){
		#endif
			if(damage>0){
				if(div>0) targetUnit.ApplyDamage(damage/div, damageType);
				else targetUnit.ApplyDamage(damage, damageType);
			}
			if(stun>0) target.ApplyStun(stun);
		}
	}
	
	public int damageType;
	void ApplyEffect(Unit targetUnit){
		//Debug.Log(targetUnit.unitName+"  "+damage);
		if(damage>0) targetUnit.ApplyDamage(damage/shootPoint.Length, damageType);
		if(stun>0) target.ApplyStun(stun);
	}
	
	public Unit GetTarget(){
		return target;
	}
	
	void OnDrawGizmos(){
		if(target!=null){
			Gizmos.color=Color.red;
			Gizmos.DrawLine(unit.thisT.position, target.thisT.position);
		}
	}
}
