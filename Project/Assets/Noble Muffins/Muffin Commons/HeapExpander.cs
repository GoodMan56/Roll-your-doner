using UnityEngine;

public class HeapExpander : MonoBehaviour {
	
	public int kilobytes = 8192;
	
	// Use this for initialization
	void Start ()
	{
		//Try to claim eight megs.
		byte[][] junkAllocations = new byte[kilobytes][];
		for(int i = 0; i < junkAllocations.Length; i++)
		{
			junkAllocations[i] = new byte[1024];
		}
		junkAllocations = null;
		System.GC.Collect();
	
		GameObject.Destroy(this.gameObject);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
