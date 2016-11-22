using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("TDTK/InGameObject/Path")]
public class PathTD : MonoBehaviour {

	
	[HideInInspector] public Transform[] waypoints=new Transform[2];
	public float heightOffsetOnPlatform=0.1f;
	
	public float dynamicWP=1;
	
	public bool generatePathObject=true;
	public bool showGizmo=true;
	
	private Transform thisT;
	//private GameObject thisObj;
	
	private List<PathSection> path=new List<PathSection>();
	
	
	private List<Vector3> wp=new List<Vector3>();
	public void test(){
		wp=new List<Vector3>();
		
		foreach(Transform p in waypoints){
			if(p==null) return;
				
			wp.Add(p.position);
		}
		
		//~ List<int> cornerIDList=new List<int>();
		for(int ID=1; ID<wp.Count; ID++){
			//~ Vector3 p=wp[ID];
			//~ if(p.gameObject.layer!=LayerManager.LayerPlatform()){
				ID+=SmoothCorner(ID);
			//~ }
		}
	}
	
	[HideInInspector] public float stepSize=1;
	int SmoothCorner(int ID){
		if(ID<=0 || ID>=wp.Count-1) return 0;
			
		int IDp=ID-1;
		int IDn=ID+1;
		
		int insertCount=0;
		
		//~ float distp=Vector3.Distance(wp[ID], wp[IDp]);
		//if(distp
		
		Vector3 dirF=(wp[IDn]-wp[ID]).normalized;
		for(int i=1; i<=2; i++){
			Vector3 newP=wp[ID]+dirF*stepSize*i;
			wp.Insert(ID+i, newP);
			insertCount+=1;
		}
		
		Vector3 dirB=(wp[IDp]-wp[ID]).normalized;
		for(int i=1; i<=2; i++){
			Vector3 newP=wp[ID]+dirB*stepSize;
			wp.Insert(ID, newP);
			insertCount+=1;
		}
		
		ID+=2;
		
		
		wp[ID]=(wp[ID-2]+wp[ID-1]+wp[ID]+wp[ID+1]+wp[ID+2])/5;
		wp[ID-1]=(wp[ID-2]+wp[ID-1]+wp[ID]+wp[ID+1])/4;
		wp[ID+1]=(wp[ID-1]+wp[ID]+wp[ID+1]+wp[ID+2])/4;
		wp[ID-2]=(wp[ID-2]+wp[ID-1]+wp[ID])/3;
		wp[ID+2]=(wp[ID]+wp[ID+1]+wp[ID+2])/3;
		
		
		//~ wp[ID-2]=(wp[ID-2]+wp[ID-1]+wp[ID])/3;
		//~ wp[ID-1]=(wp[ID-2]+wp[ID-1]+wp[ID]+wp[ID+1])/4;
		//~ wp[ID]=(wp[ID-2]+wp[ID-1]+wp[ID]+wp[ID+1]+wp[ID+2])/5;
		//~ wp[ID+1]=(wp[ID-1]+wp[ID]+wp[ID+1]+wp[ID+2])/4;
		//~ wp[ID+2]=(wp[ID]+wp[ID+1]+wp[ID+2])/3;
		
		//~ List<Vector3> wpTemp=new List<Vector3>();
		//~ for(int i=ID-2; i<=ID+2; i++) wpTemp.Add(wp[i]);
		//~ wp[ID-2]=(wpTemp[0]+wpTemp[1]+wpTemp[2])/3;
		//~ wp[ID+2]=(wpTemp[2]+wpTemp[3]+wpTemp[4])/3;
		//~ wp[ID-1]=(wpTemp[1]+wpTemp[2]+wpTemp[3]+wpTemp[4])/4;
		//~ wp[ID+1]=(wpTemp[0]+wpTemp[1]+wpTemp[2]+wpTemp[3])/4;
		//~ wp[ID]=(wpTemp[0]+wpTemp[1]+wpTemp[2]+wpTemp[3]+wpTemp[4])/5;
		
		//~ Debug.Log(insertCount);
		//~ for(int j=ID-2; j<=ID+2; j++){
			//~ Debug.Log(ID+" +  "+j);
			//~ int i=2;
			//~ if(j==ID-2)		wp[j]=(wpTemp[i]+wpTemp[i+1]+wpTemp[i+2])/3;
			//~ else if(j==ID-1) wp[j]=(wpTemp[i-1]+wpTemp[i]+wpTemp[i+1]+wpTemp[i+2])/4;
			//~ else if(j==ID) 	wp[j]=(wpTemp[i-2]+wpTemp[i-1]+wpTemp[i]+wpTemp[i+1]+wpTemp[i+2])/5;
			//~ else if(j==ID+1) wp[j]=(wpTemp[i-2]+wpTemp[i-1]+wpTemp[i]+wpTemp[i+1])/4;
			//~ else if(j==ID+2) wp[j]=(wpTemp[i-2]+wpTemp[i-1]+wpTemp[i])/3;
		//~ }
		//~ for(int i=ID-2; i<ID+2; i++){
			//~ Debug.Log(ID+" +  "+i);
			//~ if(i==ID-2)	wp[i]=(wp[i]+wp[i+1]+wp[i+2])/3;
			//~ else if(i==ID-1) wp[i]=(wp[i-1]+wp[i]+wp[i+1]+wp[i+2])/4;
			//~ else if(i==ID) wp[i]=(wp[i-2]+wp[i-1]+wp[i]+wp[i+1]+wp[i+2])/5;
			//~ else if(i==ID+1) wp[i]=(wp[i-2]+wp[i-1]+wp[i]+wp[i+1])/4;
			//~ else if(i==ID+2) wp[i]=(wp[i-2]+wp[i-1]+wp[i])/3;
		//~ }
		
		
		return insertCount;
	}
	
	void Awake(){
		thisT=transform;
		//thisObj=gameObject;
	}
	
	void Update(){
		
	}
	
	void Start(){
		
		//run through the list, generate a list of PathSection
		//if the element is a platform, generate the node and set it to walkable
		for(int i=0; i<waypoints.Length; i++){
			Transform wp=waypoints[i];
			
			//check if this is a platform, BuildManager would have add the component and have them layered
			if(waypoints[i]!=null){
				if(wp.gameObject.layer==LayerManager.LayerPlatform()){
					//Debug.Log("platform");
					PlatformTD platform=wp.gameObject.GetComponent<PlatformTD>();
					path.Add(new PathSection(platform));
				}
				else path.Add(new PathSection(wp.position));
			}
		}
		
		//scan through the path, setup the platform pathSection if there's any
		//first initiate all the basic parameter
		for(int i=0; i<path.Count; i++){
			PathSection pSec=path[i];
			if(path[i].platform!=null){
				//get previous and next pathSection
				PathSection sec1=path[Mathf.Max(0, i-1)];
				PathSection sec2=path[Mathf.Min(path.Count-1, i+1)];
				
				//path[i].platform.SetNeighbouringWP(sec1, sec2);
				pSec.platform.SetPathObject(this, pSec, sec1, sec2);
				
				pSec.platform.SetWalkable(true);
				if(!pSec.platform.IsNodeGenerated()) 
					pSec.platform.GenerateNode(heightOffsetOnPlatform);
				
				pSec.platform.SearchForNewPath(pSec);
			}
		}
		
		//now the basic have been setup,  setup the path
		//~ for(int i=0; i<path.Count; i++){
			//~ if(path[i].platform!=null){
				//~ path.platform.GetPath();
			//~ }
		//~ }
		
		if(generatePathObject){
			StartCoroutine(CreateLinePath());
		}
	}

	//create line-renderer along the path as indicator
	IEnumerator CreateLinePath(){
		
		
		
		Vector3 offsetPos=new Vector3(0, 0, 0);
		
		for(int i=1; i<waypoints.Length; i++){
			//waypoint to waypoint
			if(path[i].platform==null && path[i-1].platform==null){
				GameObject obj=new GameObject();
				obj.name="path"+i.ToString();
				
				Transform objT=obj.transform;
				objT.parent=thisT;
				
				LineRenderer line=obj.AddComponent<LineRenderer>();
				line.material=(Material)Resources.Load("PathMaterial");
				line.SetWidth(0.3f, 0.3f);
				
				line.SetPosition(0, waypoints[i-1].position+offsetPos);
				line.SetPosition(1, waypoints[i].position+offsetPos);
			}
			//platform to waypoint
			else if(path[i].platform==null && path[i-1].platform!=null){
				GameObject obj=new GameObject();
				obj.name="path"+i.ToString();
				
				Transform objT=obj.transform;
				objT.parent=thisT;
				
				LineRenderer line=obj.AddComponent<LineRenderer>();
				line.material=(Material)Resources.Load("PathMaterial");
				line.SetWidth(0.3f, 0.3f);
				
				List<Vector3> path1=path[i-1].GetSectionPath();
				while(path1.Count==0) yield return null;
				
				line.SetPosition(0, path1[path1.Count-1]+offsetPos);
				line.SetPosition(1, waypoints[i].position+offsetPos);
			}
			//waypoint to platform
			else if(path[i].platform!=null && path[i-1].platform==null){
				GameObject obj=new GameObject();
				obj.name="path"+i.ToString();
				
				Transform objT=obj.transform;
				objT.parent=thisT;
				
				LineRenderer line=obj.AddComponent<LineRenderer>();
				line.material=(Material)Resources.Load("PathMaterial");
				line.SetWidth(0.3f, 0.3f);
				
				List<Vector3> path1=path[i].GetSectionPath();
				//~ while(path1.Count<i) yield return null;
				
				line.SetPosition(0, waypoints[i-1].position+offsetPos);
				line.SetPosition(1, path1[0]+offsetPos);
			}
			//platform to platform
			else if(path[i].platform!=null && path[i-1].platform!=null){
				GameObject obj=new GameObject();
				obj.name="path"+i.ToString();
				
				Transform objT=obj.transform;
				objT.parent=thisT;
				
				LineRenderer line=obj.AddComponent<LineRenderer>();
				line.material=(Material)Resources.Load("PathMaterial");
				line.SetWidth(0.3f, 0.3f);
				
				List<Vector3> path1=path[i-1].GetSectionPath();
				List<Vector3> path2=path[i].GetSectionPath();
				
				line.SetPosition(0, path1[path1.Count-1]+offsetPos);
				line.SetPosition(1, path2[0]+offsetPos);
			}
			
		}
		
		foreach(PathSection ps in path){
			if(ps.platform==null){
				GameObject obj=Instantiate((GameObject)Resources.Load("wpNode"), ps.pos+offsetPos, Quaternion.identity) as GameObject;
				obj.transform.parent=thisT;
			}
		}
		
	}
	
	public List<PathSection> GetPath(){
		return path;
	}
	
	
	[HideInInspector] public Color gizmoColor=Color.blue;
	void OnDrawGizmos(){
		if(showGizmo){
			Gizmos.color = gizmoColor;
			if(waypoints!=null && waypoints.Length>0){
				
				for(int i=1; i<waypoints.Length; i++){
					if(waypoints[i-1]!=null && waypoints[i]!=null)
						Gizmos.DrawLine(waypoints[i-1].position, waypoints[i].position);
				}
			}
			
			//~ Gizmos.color=Color.red;
			//~ if(wp!=null && wp.Count>0){
				//~ for(int i=1; i<wp.Count; i++){
					//~ if(wp[i-1]!=null && wp[i]!=null)
						//~ Gizmos.DrawLine(wp[i-1], wp[i-1]+new Vector3(0, 2, 0));
						//~ Gizmos.DrawLine(wp[i-1], (wp[i-1]+wp[i])/2);
						//~ Gizmos.DrawLine(wp[i-1], wp[i]);
				//~ }
			//~ }
		}
	}
	
}

public class PathSection{
	public PlatformTD platform;
	public Vector3 pos;
	
	private List<Vector3> sectionPath=new List<Vector3>();
	private int pathID=0;	//a unique randomly generated number assigned whenever a new path is found
									//so the unit on this platform can know that a new path has been updated
	
	public PathSection(Vector3 p){
		pos=p;
		sectionPath.Add(pos);
	}
	
	public PathSection(PlatformTD p){
		platform=p;
	}
	
	public void SetSectionPath(List<Vector3> L, int id){
		sectionPath=L;
		pathID=id;
	}
	
	public List<Vector3> GetSectionPath(){
		return sectionPath;
	}
	
	public int GetPathID(){
		return pathID;
	}
}
