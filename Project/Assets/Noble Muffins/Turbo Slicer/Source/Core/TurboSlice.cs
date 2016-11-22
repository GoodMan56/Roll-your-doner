//Normals are no longer defined via #define. Please refer to the Sliceable component's configuration.

using UnityEngine;
using System.Collections.Generic;

/*
  Toby Grierson's turboslice kit
  
  The public methods are:

    GameObject[] splitByLine(GameObject target, Camera camera, Vector3 _start, Vector3 _end)
    GameObject[] splitByTriangle(GameObject target, Vector3[] triangleInWorldSpace)
    GameObject[] splitByPlane(GameObject target, Vector4 localPlane)

  We give it a target object (which includes a single mesh renderer with a single material), the camera and
  the start and end positions of the touch on-screen.
  
  Or, in the second, a plane in world space defined by three vertices.
  
  Finally, in the third, a vector4 (equation of plane) describing the plane in LOCAL space (this is hardcore
  mode, so this math task can be up to you).

  You may (in the Unity editor) add mesh-bearing assets that will be cached on-load for faster processing.
  
  There is a public property; bool meshCaching. Do NOT turn this on unless the diced gameobjects are
  doomed to deletion like in a slicing game. There IS a safety switch that will warn you if it detects inappropriate
  use and turns off the feature, releasing the memory.

  It is not good at quickly identifying when objects will not be affected by the split. This is the responsibility
  of the client; see the demonstration code for a simple example of doing it with colliders.
*/

public class _TurboSliceModuleHack : MonoBehaviour
{
	public const bool supportsSkinned = false;
	
	public GameObject[] splitByPlaneRD(GameObject go, Vector4 plane, bool destroyOriginal)
	{
		return new GameObject[0];
	}
}

public partial class TurboSlice : _TurboSliceModuleHack
{
	private static TurboSlice _instance;
	public static TurboSlice instance {
		get {
			if(_instance == null)
			{
				Debug.LogWarning("No TurboSlice component in scene. Creating one now with default configuration. Add one manually if you wish to configure infill support. Please refer to the guide for more details.");
				GameObject go = new GameObject();
				go.AddComponent<TurboSlice>();
				_instance = go.GetComponent<TurboSlice>();
			}
			return _instance;
		}
	}
	
	public bool meshCaching = false;
	
	//Any object put in here will have its mesh data fetched on boot rather than on first usage in play-time.
	public GameObject[] preload;
	
	[System.Serializable]
	public class InfillConfiguration {
		public Material material;
		public Rect regionForInfill;
	}
	
	public InfillConfiguration[] infills = new InfillConfiguration[0];

	//A const for internal use
	const float epsilon = 0f;

	// Suppose we have source model A and we slice it. We create a mesh cache X (cloned from it) which is going to contain
	//the geometrical for the resulting slices and ALL subsequent slices. A number too large will result in unneccessarily
	//large memory allocations while a figure too small will result in frequent time-consuming reallocations as the vector
	//must be expanded to accomodate the growing geometry. Note that when a lineage is finally, entirely off screen, this
	//memory will be dereferenced such that the VM can release it at its discretion.
	// This number (9/2) was arrived at experimentally; at this number, reallocations are rare ( < 3 times a minute ).
	const float factorOfSafetyGeometry = 9f / 2f;
	
	// This is a little different as indices are -not- retained. This is how much we need to allocate for each resultant mesh,
	//compared to the original. I have set it assume that resultant meshes may be up to 90% the complexity of originals because
	//a highly uneven slice (a common occurrence) will result in this.
	const float factorOfSafetyIndices = 0.9f;
	
	//This here is a mesh cache. A mesh cache contains data for both a collection of slice results (multiples might refer to
	//a single cache) or for a preloaded mesh. When preloaded mesh A is split, it will yield meshs B and C and its mesh
	//cache X will be duplicated into mesh cache Y that meshes B, C and all further derivatives will refer to. When B, C and
	//all other derivatives have fallen away from the screen, their respective mesh cache will be zapped.
	class MeshCache
	{
		public bool wasPreloaded = false;
		
		private float _creationTime = Time.time;
		public float creationTime {
			get {
				return wasPreloaded ? float.MaxValue : _creationTime;
			}
		}
		
		public TurboList<Vector3> vertices;
		public TurboList<Vector3> normals;
		public TurboList<Vector2> UVs;
		
		public int[][] indices;
	
		public Material[] mats;
		
		public MeshCache clone()
		{
			bool cloneNormals = normals != null;
			
			MeshCache mc = new MeshCache();
			
			int cap = Mathf.RoundToInt((float) vertices.Count * factorOfSafetyGeometry);
			
			////////////
			mc.vertices = new TurboList<Vector3>(cap);
			if(cloneNormals)
			{
				mc.normals = new TurboList<Vector3>(cap);
			}
			mc.UVs = new TurboList<Vector2>(cap);
			
			////////////
			mc.vertices.AddArray(vertices.array);
			if(cloneNormals)
			{
				mc.normals.AddArray(normals.array);
			}
			mc.UVs.AddArray(UVs.array);
			
			////////////
			mc.indices = new int[indices.Length][];
			for(int i = 0 ; i < indices.Length; i++)
			{
				mc.indices[i] = new int[indices[i].Length];
				System.Array.Copy(indices[i], mc.indices[i], indices[i].Length);
			}
			
			mc.mats = mats;
			
			return mc;
		}
	}
	
	private Dictionary<Mesh,MeshCache> meshCaches;
	
	public void releaseCacheByMesh(Mesh m)
	{
		if(meshCaches != null && meshCaches.ContainsKey(m))
		{
			meshCaches.Remove(m);
		}
	}
	
	private void perfectSubset(
		TurboList<int> _sourceIndices,
		TurboList<Vector3> _sourceVertices,
		TurboList<Vector3> _sourceNormals,
		TurboList<Vector2> _sourceUVs,
		out int[] targetIndices,
		TurboList<Vector3> targetVertices,
		TurboList<Vector3> targetNormals,
		TurboList<Vector2> targetUVs,
		ref int[] transferTable)
	{
		int[] sourceIndices = _sourceIndices.array;
		Vector3[] sourceVertices = _sourceVertices.array;
		Vector2[] sourceUVs = _sourceUVs.array;
		
		Vector3[] sourceNormals = null;
		if(_sourceNormals != null)
			sourceNormals = _sourceNormals.array;
		
		targetIndices = new int[_sourceIndices.Count];
		
		int targetIndex = targetVertices.Count;
		for(int i = 0; i < _sourceIndices.Count; i++)
		{
			int requestedVertex = sourceIndices[i];
			
			int j = transferTable[requestedVertex];
			
			if(j == -1)
			{
				j = targetIndex;
				transferTable[requestedVertex] = j;
				targetIndex++;
			}
			
			targetIndices[i] = j;
		}
		
		targetVertices.EnsureCapacity(targetIndex);
		if(targetNormals != null)
			targetNormals.EnsureCapacity(targetIndex);
		targetUVs.EnsureCapacity(targetIndex);
		
		targetVertices.Count = targetIndex;
		if(targetNormals != null)
			targetNormals.Count = targetIndex;
		targetUVs.Count = targetIndex;
		
		for(int i = 0; i < transferTable.Length; i++)
		{
			int j = transferTable[i];
			if(j != -1)
				targetVertices.array[j] = sourceVertices[i];
		}
		
		if(targetNormals != null)
		{
			for(int i = 0; i < transferTable.Length; i++)
			{
				int j = transferTable[i];
				if(j != -1)
					targetNormals.array[j] = sourceNormals[i];
			}
		}
		
		for(int i = 0; i < transferTable.Length; i++)
		{
			int j = transferTable[i];
			if(j != -1)
				targetUVs.array[j] = sourceUVs[i];
		}
	}
	
	// Use this for initialization
	void Start ()
	{
		if(_instance != null)
		{
			Debug.LogWarning("There may be multiple TurboSlice components in scene " + Application.loadedLevelName + ". Please review this!");
		}
		
		_instance = this;
		
		if(meshCaching)
		{
			meshCaches = new Dictionary<Mesh, MeshCache>();
			
			foreach(GameObject go in preload)
			{
				Sliceable s = ensureSliceable(go);
				
				MeshCache c = cacheFromGameObject(s, false);
				c.wasPreloaded = true;
				
				MeshFilter filter = getMeshFilter(s);
				
				if(filter == null)
				{
					Mesh m = filter.sharedMesh;
					
					meshCaches[m] = c;
				}
				else
				{
					Debug.LogWarning("Turbo Slicer cannot preload object '" + go.name + "'; cannot find mesh filter.");
				}
			}
		}
	}
	
	private MeshRenderer getMeshRenderer(Sliceable s)
	{
		GameObject holder = getMeshHolder(s);
		
		if(holder != null)
		{			
			return holder.GetComponent<MeshRenderer>();
		}
		else
		{
			return null;
		}
	}
	
	private MeshFilter getMeshFilter(Sliceable s)
	{
		GameObject holder = getMeshHolder(s);
		
		if(holder != null)
		{
			return holder.GetComponent<MeshFilter>();
		}
		else
		{
			return null;
		}
	}
	
	private GameObject getMeshHolder(Sliceable s)
	{
		if(s.explicitlySelectedMeshHolder != null)
		{
			return s.explicitlySelectedMeshHolder;
		}
		else
		{
			MeshFilter[] allFilters = s.GetComponentsInChildren<MeshFilter>(true);
			
			if(allFilters.Length > 0)
			{
				return allFilters[0].gameObject;
			}
			else
			{
				return null;
			}
		}
	}
	
	private Sliceable ensureSliceable(GameObject go)
	{
		Sliceable sliceable = go.GetComponent<Sliceable>();
		
		if(sliceable == null)
		{
			Debug.LogWarning("Turbo Slicer was given an object (" + go.name + ") with no Sliceable; improvising.");
			
			sliceable = go.AddComponent<Sliceable>();
			sliceable.currentlySliceable = true;
			sliceable.refreshColliders = true;
		}
		
		return sliceable;
	}
	
	private MeshCache cacheFromGameObject(Sliceable sliceable, bool includeRoomForGrowth)
	{
		bool doNormals = sliceable != null ? sliceable.channelNormals : false;
		
		MeshFilter filter = getMeshFilter(sliceable);
		Renderer renderer = getMeshRenderer(sliceable);
		
		Mesh m = filter.sharedMesh;
		
		int initialCapacity = includeRoomForGrowth ? Mathf.RoundToInt((float) m.vertexCount * factorOfSafetyGeometry) : m.vertexCount;
		
		MeshCache c = new MeshCache();
		
		c.vertices = new TurboList<Vector3>(initialCapacity);
		if(doNormals)
			c.normals = new TurboList<Vector3>(initialCapacity);
		c.UVs = new TurboList<Vector2>(initialCapacity);
		
		c.indices = new int[m.subMeshCount][];
		
		for(int i = 0; i < m.subMeshCount; i++)
		{
			c.indices[i] = m.GetTriangles(i);
		}
		
		c.vertices.AddArray(m.vertices);
		if(doNormals)
			c.normals.AddArray(m.normals);
		c.UVs.AddArray(m.uv);
		
		if(renderer != null)
		{
			if(renderer.sharedMaterials == null)
			{
				c.mats = new Material[1];
				c.mats[0] = renderer.sharedMaterial;
			}
			else
			{
				c.mats = renderer.sharedMaterials;
			}
		}
		else
		{
			Debug.LogError("Object '" + sliceable.name + "' has no renderer");
		}
		
		return c;
	}
	
	private Vector4 planeForTransform(Camera camera, GameObject target, Vector3 _start, Vector3 _end)
	{		
		Ray r;
		
		r = camera.ScreenPointToRay(_start);
		Vector3 start = r.GetPoint(10f);
		
		r = camera.ScreenPointToRay(_end);
		Vector3 end = r.GetPoint(10f);
		
		Vector3 _middle = (_start + _end) / 2f;
		r = camera.ScreenPointToRay(_middle);
		Vector3 middle = r.GetPoint(15f);
		
		List<Vector3> _plane = new List<Vector3>();
		
		_plane.Add(start);
		_plane.Add(middle);
		_plane.Add(end);
			
		Vector4 plane = TurboSlice.planeFromTriangle(target, _plane.ToArray());
		return plane;
	}
	
	//This code here is translated from John Ratcliff's; it converts a triplet of vertices into a four-vector describing a plane.
	//This is used by splitByLine and splitByTriangle to feed a four-vector to splitByPlane.
	private static Vector4 planeFromTriangle(GameObject target, Vector3[] t)
	{	
		Matrix4x4 matrix = target.transform.worldToLocalMatrix;
		
		t[0] = matrix.MultiplyPoint(t[0]);
		t[1] = matrix.MultiplyPoint(t[1]);
		t[2] = matrix.MultiplyPoint(t[2]);
		
		Vector4 p = Vector4.zero;
		
		p.x = t[0].y * (t[1].z - t[2].z) + t[1].y * (t[2].z - t[0].z) + t[2].y * (t[0].z - t[1].z);
		p.y = t[0].z * (t[1].x - t[2].x) + t[1].z * (t[2].x - t[0].x) + t[2].z * (t[0].x - t[1].x);
		p.z = t[0].x * (t[1].y - t[2].y) + t[1].x * (t[2].y - t[0].y) + t[2].x * (t[0].y - t[1].y);
		p.w = -( t[0].x * (t[1].y * t[2].z - t[2].y * t[1].z) + t[1].x * (t[2].y * t[0].z - t[0].y * t[2].z) + t[2].x * (t[0].y * t[1].z - t[1].y * t[0].z) );
		
		return p;
	}
	
	public enum PlaneTriResult {
		PTR_FRONT, PTR_BACK, PTR_SPLIT
	};
	
	private static float classifyPoint(ref Vector4 plane, ref Vector3 p)
	{
    	return p.x * plane.x + p.y * plane.y + p.z * plane.z + plane.w;
	}
	
	private static PlaneTriResult getSidePlane(ref Vector3 p, ref Vector4 plane)
	{
	  double d = distanceToPoint(ref p, ref plane);
	
	  if ( (d+epsilon) > 0 )
			return PlaneTriResult.PTR_FRONT; // it is 'in front' within the provided epsilon value.

	  return PlaneTriResult.PTR_BACK;
	}
	
	private static float distanceToPoint(ref Vector3 p, ref Vector4 plane)
	{
		float d = p.x * plane.x + p.y * plane.y + p.z * plane.z + plane.w;
		return d;
	}
	
	static float intersectCommon(ref Vector3 p1, ref Vector3 p2, ref Vector4 plane)
	{
		float dp1 = distanceToPoint(ref p1, ref plane);
		
		Vector3 dir = p2 - p1;
		
		float dot1 = dir.x * plane.x + dir.y * plane.y + dir.z * plane.z;
		float dot2 = dp1 - plane.w;
		
		float t = -(plane.w + dot2 ) / dot1;
		
		return t;
	}

	
	//These here are the only public interfaces.
	//You can see that the core interface is splitByPlane and the others merely wrap it; the math uses Vector4 plane descriptions
	//throughout, so the byLine and byTriangle are conveniences.
	
	public GameObject[] splitByLine(GameObject target, Camera camera, Vector3 _start, Vector3 _end)
	{
		return splitByLine(target, camera, _start, _end, true);
	}
	
	public GameObject[] splitByLine(GameObject target, Camera camera, Vector3 _start, Vector3 _end, bool destroyOriginal)
	{
		Vector4 v = planeForTransform(camera, target, _start, _end);
		return splitByPlane(target, v, destroyOriginal);
	}
	
	public GameObject[] splitByTriangle(GameObject target, Vector3[] triangleInWorldSpace, bool destroyOriginal)
	{
		Vector4 v = TurboSlice.planeFromTriangle(target, triangleInWorldSpace);
		return splitByPlane(target, v, destroyOriginal);
	}
	
	public GameObject[] shatter(GameObject go, int steps)
	{
		List<GameObject> l = new List<GameObject>(1);
		
		l.Add(go);
		
		List<GameObject> l2 = l;
		
		for(int i = 0; i < steps; i++)
		{
			l2 = new List<GameObject>(l.Count * 2);
			
			Vector4 shatterPlane = (Vector4) Random.insideUnitSphere;
			
			foreach(GameObject go2 in l)
			{
				l2.AddRange( splitByPlane(go2, shatterPlane, true) );
			}
			
			l = l2;
		}
		
		return l2.ToArray();
	}
	
	private const float meshCacheSoftTimeout = 5f, meshCacheHardTimeout = 15f;
	private bool meshCacheWarningDelivered = false;
	
	// Update is called once per frame
	void Update ()
	{
		if(meshCaches == null)
			return;
		
		float t = Time.time;
		
		foreach(MeshCache mc in meshCaches.Values)
		{
			float age = t - mc.creationTime;
			bool softTimeout = age > meshCacheSoftTimeout;
			bool hardTimeout = age > meshCacheHardTimeout;
			
			if(!meshCacheWarningDelivered && softTimeout)
			{
				Debug.LogWarning("A mesh cache lingered over " + meshCacheSoftTimeout + " seconds. Please review the TurboSlice guide's performance section.");
				meshCacheWarningDelivered = true;
			}
			else if(hardTimeout)
			{
				Debug.LogWarning("A mesh cache lingered over " + meshCacheHardTimeout + " seconds. Disabling caches!");
				
				meshCaching = false;
				meshCaches = null;
				return;
			}
		}
	}
}
