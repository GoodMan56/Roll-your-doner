using UnityEngine;
using System.Collections;

public class FPSControl : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public GameObject shootObject;
	public Transform[] shootPoint;
	
	public float cooldown=0.5f;
	public float damage=1;
	
	private float lastShootT;
	
	public void Shoot(){
		if(Time.time-lastShootT<cooldown) return;
		
		lastShootT=Time.time;
		
		foreach(Transform sp in shootPoint){
			GameObject shootObj=(GameObject)Instantiate(shootObject, sp.position, sp.rotation);
			ShootObject soCom=shootObj.GetComponent<ShootObject>();
			if(soCom!=null) soCom.Shoot(damage, 0);
		}
	}
	
	public void SetTower(UnitTower tower){
		shootPoint=tower.shootPoint;
		damage=tower.GetDamage();
		cooldown=tower.GetCooldown();
	}
}
