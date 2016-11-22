using UnityEngine;
using System.Collections;

public class Utility : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public static bool IsActive(GameObject obj){
		#if UNITY_4_0
			if(obj.activeInHierarchy) return true;
		#else
			if(obj.active) return true;
		#endif
		
		return false;
	}
	
	public static void SetActive(GameObject obj, bool flag){
		#if UNITY_4_0
			if(IsActive(obj)!=flag) obj.SetActive(flag);
		#else
			if(IsActive(obj)!=flag) obj.SetActiveRecursively(flag);
		#endif
	}
	
	static public void SetLayerRecursively(Transform root, int layer){
		foreach(Transform child in root) {
			child.gameObject.layer=layer;
			SetLayerRecursively(child, layer);
		}
	}
}
