using UnityEngine;
using System.Collections;

public class SelfDeactivator : MonoBehaviour {

	public float duration=1;
	
	// Use this for initialization
	void Start () {
	
	}
	
	void OnEnable(){
		StartCoroutine(DeactivateRoutine());
	}
	
	IEnumerator DeactivateRoutine(){
		yield return new WaitForSeconds(duration);
		ObjectPoolManager.Unspawn(gameObject);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
