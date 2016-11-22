using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("TDTK/InGameObject/BuildPlatform")]
public class PathOnPlatform{
	public PathTD path;
	
	public PathSection thisWP;
	public PathSection prevNeighbouringWP;
	public PathSection nextNeighbouringWP;
	
	public NodeTD startN;
	public NodeTD endN;
	
	public List<Vector3> currentPath=new List<Vector3>();
	public int pathID=0;
	
	//public Node nextBuildNode;
	public List<Vector3> altPath=new List<Vector3>();
	
	public PathOnPlatform(PathTD p, PathSection pSec, PathSection prev, PathSection next){
		path=p;
		thisWP=pSec;
		prevNeighbouringWP=prev;
		nextNeighbouringWP=next;
	}
	
	public void InitNode(NodeTD[] nodeGraph){
		Vector3 prevPoint;
		if(prevNeighbouringWP.platform!=null){
			Debug.Log("dong");
			//place holder, platform to platform connection not supported yet
			prevPoint=prevNeighbouringWP.platform.thisT.position;
		}
		else prevPoint=prevNeighbouringWP.pos;
		startN=PathFinderTD.GetNearestNode(prevPoint, nodeGraph);
		
		Vector3 nextPoint;
		if(nextNeighbouringWP.platform!=null){
			Debug.Log("ding");
			//place holder, platform to platform connection not supported yet
			nextPoint=nextNeighbouringWP.platform.thisT.position;
		}
		else nextPoint=nextNeighbouringWP.pos;
		endN=PathFinderTD.GetNearestNode(nextPoint, nodeGraph);
		
		//Debug.DrawLine(endN.pos, endN.pos+new Vector3(0, 2, 0), Color.red, 5);
		//Debug.DrawLine(startN.pos, startN.pos+new Vector3(0, 2, 0), Color.blue, 5);
		//Debug.DrawLine(nextNeighbouringWP.pos, nextNeighbouringWP.pos+new Vector3(0, 2, 0), Color.green, 5);
	}
	
	public void SetPath(List<Vector3> wp, int id){
		currentPath=wp;
		pathID=id;
		thisWP.SetSectionPath(wp, pathID);
	}
	
	public void SmoothPath(){
		currentPath=PathFinderTD.SmoothPath(currentPath);
	}
}

public class PlatformTD : MonoBehaviour {
	
	//buildable tower type on this platform
	public _TowerType[] buildableType=new _TowerType[1];
	public List<TowerAvailability> towerAvaiList=new List<TowerAvailability>();
	//public GameObject[] buildablePrefab=new GameObject[1];
	
	public int[] specialBuildableID;
	
	//indicate if creep can walk pass this platform, true if this platform is part of a path
	private bool walkable;
	
	//the graph-node covering this platform
	private NodeTD[] nodeGraph;
	private bool graphGenerated=false;
	public bool IsNodeGenerated(){
		return graphGenerated;
	}
	
	//the current active waypoint to go through this platform from prevNeighbouringWP to nextNeighbouringWP
	//private List<Vector3> currentPath=new List<Vector3>();
	//private int pathID=0;	//a unique randomly generated number assigned whenever a new path is found
									//so the unit on this platform can know that a new path has been updated
	
	private List<PathOnPlatform> pathObjects=new List<PathOnPlatform>();
	
	//private PathSection prevNeighbouringWP;
	//private PathSection nextNeighbouringWP;
	
	//private Node startN;
	//private Node endN;
	
	//the node that will be blocked and the alt path for the current selected build pos, cache it so there's no need to do another search
	private NodeTD nextBuildNode;
	//List<Vector3> altPath=new List<Vector3>();
	
	[HideInInspector] public GameObject thisObj;
	[HideInInspector] public Transform thisT;
	
	void Awake(){
		thisObj=gameObject;
		thisT=transform;
		
		thisObj.layer=LayerManager.LayerPlatform();
		
		if(specialBuildableID!=null && specialBuildableID.Length>0){
			for(int i=0; i<specialBuildableID.Length; i++){
				specialBuildableID[i]=Mathf.Max(0, specialBuildableID[i]);
			}
		}
	}
	
	public void InitTowerList(List<TowerAvailability> list){
		if(towerAvaiList.Count<=0){
			foreach(TowerAvailability avai in list){
				towerAvaiList.Add(avai.Clone());
				towerAvaiList[towerAvaiList.Count-1].enabledInLvl=true;
			}
		}
		else{
			foreach(TowerAvailability avaiS in list){
				bool match=false;
				foreach(TowerAvailability avaiT in towerAvaiList){
					if(avaiS.ID==avaiT.ID){
						match=true;
					}
				}
				if(!match){
					towerAvaiList.Add(avaiS.Clone());
					towerAvaiList[towerAvaiList.Count-1].enabledInLvl=true;
				}
			}
		}
		
		//~ if(towerAvaiList.Count>0){
			//~ List<TowerAvailability> tempList=new List<TowerAvailability>();
			//~ for(int i=0; i<list.Count; i++){
				//~ if(list[i].enabledInLvl){
					//~ tempList.Add(new TowerAvailability(list[i].ID));
					//~ foreach(TowerAvailability avai in towerAvaiList){
						//~ if(avai.ID==list[i].ID){
							//~ tempList[i].enabledInLvl=avai.enabledInLvl;
							//~ break;
						//~ }
					//~ }
				//~ }
				//~ else{
					//~ tempList.Add(new TowerAvailability(list[i].ID));
					//~ tempList[i].enabledInLvl=true;
				//~ }
			//~ }
			//~ towerAvaiList=tempList;
		//~ }
		//~ else{
			//~ foreach(TowerAvailability avai in list){
				//~ towerAvaiList.Add(avai.Clone());
				//~ towerAvaiList[towerAvaiList.Count-1].enabledInLvl=true;
			//~ }
		//~ }
	}
	
	void Start(){
		
	}
	
	void Update(){
		if(Input.GetKeyDown(KeyCode.Space)){
			//~ GenerateNode(.5f);
			
			//~ foreach(PathOnPlatform pathObj in pathObjects){
						//~ queue.Add(pathObj.thisWP);
						
						//~ PathFinderTD.GetPath(pathObj.startN, pathObj.endN, nodeGraph, this.SetPath);
					//~ }
		}
	}
	
	public void GenerateNode(float heightOffset){
		nodeGraph=NodeGeneratorTD.GenerateNode(this, heightOffset);
		graphGenerated=true;
	}
	
	//a queue for path finding call back when there are multiple path acessing the same platform
	private List<PathSection> queue=new List<PathSection>();
	
	public void SearchForNewPath(PathSection PS){
		queue.Add(PS);
		
		/*Vector3 prevPoint;
		if(prevNeighbouringWP.platform!=null){
			//place holder, platform to platform connection not supported yet
			prevPoint=prevNeighbouringWP.pos;
		}
		else prevPoint=prevNeighbouringWP.pos;
		startN=PathFinder.GetNearestNode(prevPoint, nodeGraph);
		
		Vector3 nextPoint;
		if(nextNeighbouringWP.platform!=null){
			//place holder, platform to platform connection not supported yet
			nextPoint=nextNeighbouringWP.pos;
		}
		else nextPoint=nextNeighbouringWP.pos;
		endN=PathFinder.GetNearestNode(nextPoint, nodeGraph);*/
		
		
		foreach(PathOnPlatform path in pathObjects){
			//Debug.Log("init path ................."+path.path);
			if(path.thisWP==PS) {
				path.InitNode(nodeGraph);
				
				PathFinderTD.GetPath(path.startN, path.endN, nodeGraph, this.SetPath);
			}
		}
		
		//PathFinder.GetPath(startN, endN, nodeGraph, this.SetPath);
	}
	
	//if this platform is part of a path, this function set the previous and next transform
	//public void SetNeighbouringWP(PathSection prev, PathSection next){
	//	prevNeighbouringWP=prev;
	//	nextNeighbouringWP=next;
	//}
	
	
	
	
	
	//check if building on particular point of the platform will block all possible route
	//use brute force check to return a flag instantly. try find a more perfomance friendly solution
	public bool CheckForBlock(Vector3 pos, int footprint){
		float gridSize=BuildManager.GetGridSize();
		bool blocked=false;
		
		nextBuildNode=PathFinderTD.GetNearestNode(pos, nodeGraph);
		//Debug.DrawLine(nextBuildNode.pos, nextBuildNode.pos+new Vector3(0, 2, 0), Color.red, 0.5f);
		
		foreach(PathOnPlatform pathObj in pathObjects){
			//Debug.Log("check path for "+pathObj.path);
			
			//check if the start of end has been blocked
			if(Vector3.Distance(pos, pathObj.startN.pos)<gridSize/2) return true;
			if(Vector3.Distance(pos, pathObj.endN.pos)<gridSize/2) return true;
			
			//check if the node is in currentPath, if not, then the node is buildable
			//this is only applicable if pathSmoothing is off
			if(!PathFinderTD.IsPathSmoothingOn()){
				bool inCurrentPath=false;
				foreach(Vector3 pathPoint in pathObj.currentPath){
					float dist=Vector3.Distance(pos, pathPoint);
					if(dist<gridSize/2){
						inCurrentPath=true;
						break;
					}
				}
				
				if(inCurrentPath) {
					//the current node is in current path, check to see if there's other alternative path if this one's blocked
					//while getting another path, cache it so it can be used later without redo the search
					//use force instant search so the path is return immediately
					pathObj.altPath=PathFinderTD.ForceSearch(pathObj.startN, pathObj.endN, nextBuildNode, nodeGraph, footprint);
					if(pathObj.altPath.Count==0) blocked=true;
				}
				else{
					pathObj.altPath=pathObj.currentPath;
				}
			}
			else{
				//the current node is in current path, check to see if there's other alternative path if this one's blocked
				//while getting another path, cache it so it can be used later without redo the search
				//use force instant search so the path is return immediately
				pathObj.altPath=PathFinderTD.ForceSearch(pathObj.startN, pathObj.endN, nextBuildNode, nodeGraph, footprint);
				
				if(pathObj.altPath.Count==0) blocked=true;
				
				if(blocked) break;
			}
		}
		
		return blocked;
		
		/*
		float gridSize=BuildManager.GetGridSize();
		
		//check if the start of end has been blocked
		if(Vector3.Distance(pos, startN.pos)<gridSize/2) return true;
		if(Vector3.Distance(pos, endN.pos)<gridSize/2) return true;
		
		//check if the node is in currentPath, if not, then the node is buildable
		//this is only applicable if pathSmoothing is off
		if(!PathFinder.IsPathSmoothingOn()){
			bool InCurrentPath=false;
			
			foreach(Vector3 pathPoint in currentPath){
				float dist=Vector3.Distance(pos, pathPoint);
				if(dist<gridSize/2){
					InCurrentPath=true;
					break;
				}
			}
			
			if(!InCurrentPath) {
			//Debug.Log("not in current path ");
				return false;
			}
		}
		
		
		Debug.Log("calling pathfinder, check for block");
		
		//the current node is in current path, check to see if there's other alternative path if this one's blocked
		//while getting another path, cache it so it can be used later without redo the search
		nextBuildNode=PathFinder.GetNearestNode(pos, nodeGraph);
		//use force instant search so the path is return immediately
		altPath=PathFinder.ForceSearch(startN, endN, nextBuildNode, nodeGraph);

		if(altPath.Count>0) return false;
		*/
		
		//return true;
	}
	
	
	public void SetPathObject(PathTD p, PathSection pSec, PathSection prev, PathSection next){
		PathOnPlatform path=new PathOnPlatform(p, pSec, prev, next);
		
		pathObjects.Add(path);
	}
	
	
	public void SetPath(List<Vector3> wp){
		//Debug.Log("GotPath. Path length = "+wp.Count);
		//currentPath=wp;
		
		//~ Debug.Log("queue "+queue.Count+"  "+queue[0].GetPathID());
		
		//assign a new ID to the path
		int pathID=queue[0].GetPathID();
		int rand=pathID;
		while(rand==pathID){
			rand=Random.Range(-999999, 999999);
		}
		pathID=rand;
		
		//~ Debug.Log("new ID "+pathID);
		
		foreach(PathOnPlatform pathObj in pathObjects){
			if(pathObj.thisWP==queue[0]) {
				pathObj.SetPath(wp, pathID);
				break;
			}
		}
		
					//~ foreach(PathOnPlatform pathObj in pathObjects){
						//~ int rand=pathObj.pathID;
						//~ while(rand==pathObj.pathID){
							//~ rand=Random.Range(-999999, 999999);
						//~ }
						//~ pathObj.SetPath(pathObj.altPath, rand);
						//~ //forceSearch doesnt smooth path so path smoothing is call
						//~ //smoothPath will only valid path smoothing is enable in PathFinder
						//~ pathObj.SmoothPath();
					//~ }
		
		queue.RemoveAt(0);
	}
	
	public void Prebuild(Vector3 point, UnitTower tower){
		if(nodeGraph==null || nodeGraph.Length==0) return;
		
		NodeTD node=PathFinderTD.GetNearestNode(point, nodeGraph, 0);
		node.walkable=false;
		
		List<NodeTD> nodeList=PathFinderTD.GetNodeInFootprint(node, tower.GetFootprint());
		foreach(NodeTD n in nodeList){
			n.walkable=false;
		}
		
		tower.SetPlatform(this, node);
		
		foreach(PathOnPlatform pathObj in pathObjects){
			queue.Add(pathObj.thisWP);
			
			PathFinderTD.GetPath(pathObj.startN, pathObj.endN, nodeGraph, this.SetPath);
		}
	}
	
	
	public void Build(Vector3 point, UnitTower tower){
		//pathfinding related code, only call if this platform is walkable;
		if(walkable){
			if(tower.type!=_TowerType.Mine){
				//if build on the node last check for block, use the cached node and path
				if(nextBuildNode!=null && Vector3.Distance(nextBuildNode.pos, point)<BuildManager.GetGridSize()/2){
					//Debug.Log("use cached path");
					nextBuildNode.walkable=false;
					
					List<NodeTD> nodeList=PathFinderTD.GetNodeInFootprint(nextBuildNode, tower.GetFootprint());
					foreach(NodeTD node in nodeList){
						node.walkable=false;
					}
					
					tower.SetPlatform(this, nextBuildNode);
					foreach(PathOnPlatform pathObj in pathObjects){
						int rand=pathObj.pathID;
						while(rand==pathObj.pathID){
							rand=Random.Range(-999999, 999999);
						}
						pathObj.SetPath(pathObj.altPath, rand);
						//forceSearch doesnt smooth path so path smoothing is call
						//smoothPath will only valid path smoothing is enable in PathFinder
						pathObj.SmoothPath();
					}
				}
				//if build on node that is unchecked, find the block node and initiate a new path search
				else{
					//Debug.Log("unexpected build point, query for new path");
					NodeTD node=PathFinderTD.GetNearestNode(point, nodeGraph);
					node.walkable=false;
					//~ Debug.Log(node.ID);
					
					List<NodeTD> nodeList=PathFinderTD.GetNodeInFootprint(node, tower.GetFootprint());
					foreach(NodeTD n in nodeList){
						n.walkable=false;
					}
					
					tower.SetPlatform(this, node);
					foreach(PathOnPlatform pathObj in pathObjects){
						queue.Add(pathObj.thisWP);
						
						PathFinderTD.GetPath(pathObj.startN, pathObj.endN, nodeGraph, this.SetPath);
					}
				}
			}
		}
	}
	
	public NodeTD GetNearestNode(Vector3 point){
		return PathFinderTD.GetNearestNode(point, nodeGraph);
	}
	
	public void UnBuild(NodeTD node){
		UnBuild(node, 0);
	}
	public void UnBuild(NodeTD node, int footprint){
		footprint=0;
		
		//Debug.Log("unbuild  "+node.ID);
		
		node.walkable=true;
		
		List<NodeTD> nodeList=PathFinderTD.GetNodeInFootprint(node, footprint);
		foreach(NodeTD n in nodeList){
			n.walkable=true;
		}
		
		foreach(PathOnPlatform pathObj in pathObjects){
			
			//~ Debug.Log("update path");
			
			queue.Add(pathObj.thisWP);
			PathFinderTD.GetPath(pathObj.startN, pathObj.endN, nodeGraph, this.SetPath);
		}
	}
	
	//set to true if creep can move pass this platform
	public void SetWalkable(bool flag){
		walkable=flag;
	}
	
	public bool IsWalkable(){
		return walkable;
	}
	
	
	public NodeTD[] GetNodeGraph(){
		return nodeGraph;
	}
	
	public bool GizmoShowNodes=true;
	public bool GizmoShowPath=true;
	
	void OnDrawGizmos(){
		if(GizmoShowNodes){
			if(nodeGraph!=null && nodeGraph.Length>0){
				foreach(NodeTD node in nodeGraph){
					//bool flag=false;
					//foreach(PathOnPlatform pathObj in pathObjects){
					//	if(pathObj.startN==node) flag=true;
					//	else if(pathObj.endN==node) flag=true;
					//}
					
					//if(flag) Gizmos.color=Color.blue;
					//else Gizmos.color=Color.white;
					
					if(!node.walkable){
						Gizmos.color=Color.red;
						Gizmos.DrawSphere(node.pos, BuildManager.GetGridSize()*.3f);
					}
					else{
						Gizmos.color=Color.white;
						Gizmos.DrawSphere(node.pos, BuildManager.GetGridSize()*.25f);
					}
					
					//Gizmos.DrawSphere(node.pos, 0.25f);
				}
			}
		}
		
		if(GizmoShowPath){
			foreach(PathOnPlatform pathObj in pathObjects){
				if(pathObj.currentPath.Count>0){
					Gizmos.color =new Color(1.0f, 0f, 0f, 1.0f);
					foreach(Vector3 p in pathObj.currentPath){
						Gizmos.color-=new Color(1.0f/pathObj.currentPath.Count, 0, 0, 0);
						Gizmos.color+=new Color(0, 1.0f/pathObj.currentPath.Count, 0, 0);
						Gizmos.DrawSphere (p, BuildManager.GetGridSize()*.4f);
					}
				}
				//else Debug.Log(pathObj.path+"'s path length=0");
				
				if(pathObj.altPath.Count>0){
					for(int i=1; i<pathObj.altPath.Count; i++){
						Gizmos.color=Color.red;
						Gizmos.DrawLine(pathObj.altPath[i], pathObj.altPath[i-1]);
					}
				}
			}
		}
		
		
		//~ foreach(PathOnPlatform pathObj in pathObjects){
			//~ Gizmos.DrawSphere (pathObj.startN.pos, 0.5f);
			//~ //Debug.Log(pathObj.startN.pos);
			//~ Gizmos.DrawSphere (pathObj.endN.pos, 0.5f);
			//~ //Debug.Log(pathObj.endN.pos);
		//~ }
	}
	
	
}
