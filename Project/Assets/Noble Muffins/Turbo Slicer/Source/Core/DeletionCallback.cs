using UnityEngine;

public delegate void DeletionOccurred(Mesh m);

public class DeletionCallback : MonoBehaviour
{
	public Mesh mesh;
	public DeletionOccurred deletionListener;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		
	}
	
	void OnDestroy()
	{
		if(deletionListener != null)
			deletionListener(mesh);
		mesh = null;
	}
}
