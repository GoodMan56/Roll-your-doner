using UnityEngine;
using System.Collections;

public class AirStrike : MonoBehaviour {

	public Transform[] missile;
	public Vector3[] defaultPos;
	
	public float radius=3;
	public float speed=50;
	
	private int hit=0;
	
	private Transform thisT;
	
	public Transform hitEffect;
	
	void Awake(){
		defaultPos=new Vector3[missile.Length];
		for(int i=0; i<defaultPos.Length; i++){
			defaultPos[i]=missile[i].localPosition;
		}
		
		thisT=transform;
		
		ObjectPoolManager.New(hitEffect, missile.Length);
	}
	
	// Use this for initialization
	void Start () {
	
	}
	
	void OnEnable(){
		//~ for(int i=0; i<defaultPos.Length; i++){
			//~ missile[i].localPosition=defaultPos[i];
		//~ }
		
		StartCoroutine(LaunchMissile());
		
		hit=0;
	}
	
	void OnDisable(){
		
	}
	
	IEnumerator LaunchMissile(){
		for(int i=0; i<defaultPos.Length; i++){
			StartCoroutine(MissileRoutine(i));
			yield return new WaitForSeconds(Random.Range(0.0f, .3f));
		}
	}
	
	IEnumerator MissileRoutine(int i){
		float heightOffset=1;
		Vector3 targetPos=thisT.position+new Vector3(Random.Range(-radius, radius), heightOffset, Random.Range(-radius, radius));
		missile[i].rotation=Quaternion.LookRotation(targetPos-missile[i].position);
		while(true){
			float dist=Vector3.Distance(missile[i].position, targetPos);
			missile[i].Translate(Vector3.forward*Mathf.Min(dist, speed*Time.deltaTime));
			if(dist<0.2f || missile[i].localPosition.y<heightOffset){
				Hit(i);
				yield break;
			}
			yield return null;
		}
	}
	
	void Hit(int i){
		//~ missile[i].localPosition=defaultPos[i];
		hit+=1;
		ObjectPoolManager.Spawn(hitEffect, missile[i].position, Quaternion.identity);
		
		if(hit==missile.Length){
			StartCoroutine(DeactivateSelf());
		}
	}
	
	IEnumerator DeactivateSelf(){
		yield return new WaitForSeconds(.5f);
		Utility.SetActive(gameObject, false);
		
		for(int i=0; i<defaultPos.Length; i++){
			missile[i].localPosition=defaultPos[i];
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
