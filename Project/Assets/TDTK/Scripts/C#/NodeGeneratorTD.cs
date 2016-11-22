using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private field assigned but not used.

public class NodeGeneratorTD : MonoBehaviour {
	[HideInInspector] public Rect area;
	[HideInInspector] public float gridSize=1;
	private float maxSlope=0;
	public bool connectDiagonalNeighbour=false;
	
	[HideInInspector] public float agentHeight=2;

	static public float _gridSize=1;
	
	static NodeTD[] nodeGraph;

	static NodeGeneratorTD nodeGenerator;
	
	//~ public Transform plane;
	
	static private Transform thisT;
	
	//~ static Transform refT;

	void Awake(){
		nodeGenerator=this;
		
		thisT=transform;
	}
	
	void Start(){
		CycleNode();
		//~ GenerateNode(plane);
	}
	
	static public void Init(){
		GameObject obj=new GameObject();
		nodeGenerator=obj.AddComponent<NodeGeneratorTD>();
		
		thisT=obj.transform;
		
		obj.name="NodeGenerator";
	}
	
	static public void CheckInit(){
		if(nodeGenerator==null) Init();
	}
	
	static public bool CheckConnection(Transform p1, Transform p2){
		//~ float gridSize=BuildManager.GetGridSize();
		
		if(Mathf.Abs(p1.position.y-p2.position.y)>_gridSize/2){
			Debug.Log("height is not compatible");
			return false;
		}
		
		//check x-axis
		float distX=p1.position.x-p2.position.x;
		float coverageX=p1.localScale.x*10/2+p2.localScale.x*10/2;
		
		if(distX<=coverageX) return true;
		
		//check z-axis
		float distZ=p1.position.z-p2.position.z;
		float coverageZ=p1.localScale.z*10/2+p2.localScale.z*10/2;
		
		if(distZ<=coverageZ) return true;
		
		return false;
	}

	
	static public NodeTD[] GenerateNode(PlatformTD platform, float heightOffset){
		
		//check if node generator
		CheckInit();
		
		//Debug.Log("generating nav node for "+platform.thisT);
		
		float timeStart=Time.realtimeSinceStartup;
		
		Transform platformT=platform.thisT;
		
		float gridSize=BuildManager.GetGridSize();
		
		float scaleX=platform.thisT.localScale.x;
		float scaleZ=platform.thisT.localScale.z;
		
		int countX=(int)(10*scaleX/gridSize);
		int countZ=(int)(10*scaleZ/gridSize);
		//Debug.Log(countX+"   "+countZ);
		
		
		float x=-scaleX*10/2/scaleX;
		float z=-scaleZ*10/2/scaleZ;
		
		
		Vector3 point=platformT.TransformPoint(new Vector3(x, 0, z));
		
		thisT.position=point;
		thisT.rotation=platformT.rotation;
		
		thisT.position=thisT.TransformPoint(new Vector3(gridSize/2, heightOffset, gridSize/2));
		
		NodeTD[] nodeGraph=new NodeTD[countZ*countX];
		
		int counter=0;
		for(int i=0; i<countZ; i++){
			for(int j=0; j<countX; j++){
				nodeGraph[counter]=new NodeTD(thisT.position, counter);
				counter+=1;
				
				thisT.position=thisT.TransformPoint(new Vector3(gridSize, 0, 0));
			}
			thisT.position=thisT.TransformPoint(new Vector3(-(countX)*gridSize, 0, gridSize));
		}
		
		thisT.position=Vector3.zero;
		thisT.rotation=Quaternion.identity;
		
		float timeUsed=Time.realtimeSinceStartup-timeStart;
		//Debug.Log("generate "+counter+" nodes, used "+timeUsed+"seconds");
		
		counter=0;
		foreach(NodeTD cNode in nodeGraph){
			if(cNode.walkable){
				//check if there's anything within the point
				LayerMask mask=1<<LayerManager.LayerPlatform();
				mask|=1<<LayerManager.LayerTower();
				if(LayerManager.LayerTerrain()>=0)
					mask|=1<<LayerManager.LayerTerrain();
				Collider[] cols=Physics.OverlapSphere(cNode.pos, gridSize*0.45f, ~mask);
				if(cols.Length>0){
					cNode.walkable=false;
					counter+=1;
				}
			}
		}
		//if(counter>0) Debug.Log(counter+" node is unwalkable");
		
		float neighbourDistance=0;
		float neighbourRange;
		if(nodeGenerator.connectDiagonalNeighbour) neighbourRange=gridSize*1.5f;
		else neighbourRange=gridSize*1.1f;
		
		timeStart=Time.realtimeSinceStartup;
		
		counter=0;
		//assign the neighouring  node for each node in the grid
		foreach(NodeTD currentNode in nodeGraph){
			//only if that node is walkable
			if(currentNode.walkable){
			
				//create an empty array
				List<NodeTD> neighbourNodeList=new List<NodeTD>();
				List<float> neighbourCostList=new List<float>();
				
				NodeTD[] neighbour=new NodeTD[8];
				int id=currentNode.ID;
				
				if(id>countX-1 && id<countX*countZ-countX){
					//print("middle rows");
					if(id!=countX) neighbour[0]=nodeGraph[id-countX-1];
					neighbour[1]=nodeGraph[id-countX];
					neighbour[2]=nodeGraph[id-countX+1];
					neighbour[3]=nodeGraph[id-1];
					neighbour[4]=nodeGraph[id+1];
					neighbour[5]=nodeGraph[id+countX-1];
					neighbour[6]=nodeGraph[id+countX];
					if(id!=countX*countZ-countX-1)neighbour[7]=nodeGraph[id+countX+1];
				}
				else if(id<=countX-1){
					//print("first row");
					if(id!=0) neighbour[0]=nodeGraph[id-1];
					if(nodeGraph.Length>id+1) neighbour[1]=nodeGraph[id+1];
					if(countZ>0){
						if(nodeGraph.Length>id+countX-1)	neighbour[2]=nodeGraph[id+countX-1];
						if(nodeGraph.Length>id+countX)	neighbour[3]=nodeGraph[id+countX];
						if(nodeGraph.Length>id+countX+1)	neighbour[4]=nodeGraph[id+countX+1];
					}
				}
				else if(id>=countX*countZ-countX){
					//print("last row");
					neighbour[0]=nodeGraph[id-1];
					if(id!=countX*countZ-1) neighbour[1]=nodeGraph[id+1];
					neighbour[2]=nodeGraph[id-countX-1];
					neighbour[3]=nodeGraph[id-countX];
					neighbour[4]=nodeGraph[id-countX+1];
				}
				


				//scan through all the node in the grid
				foreach(NodeTD node in neighbour){
					//if this the node is not currentNode
					if(node!=null && node.walkable){
						//if this node is within neighbour node range
						neighbourDistance=GetHorizontalDistance(currentNode.pos, node.pos);
						if(neighbourDistance<neighbourRange){
							//if nothing's in the way between these two
							LayerMask mask=1<<LayerManager.LayerPlatform();
							mask|=1<<LayerManager.LayerTower();
							if(!Physics.Linecast(currentNode.pos, node.pos, ~mask)){
								//if the slop is not too steep
								//if(Mathf.Abs(GetSlope(currentNode.pos, node.pos))<=maxSlope){
									//add to list
									//if(!node.walkable) Debug.Log("error");
									neighbourNodeList.Add(node);
									neighbourCostList.Add(neighbourDistance);
								//}//else print("too steep");
							}//else print("something's in the way");
						}//else print("out of range "+neighbourDistance);
					}//else print("unwalkable");
				}

				//set the list as the node neighbours array
				currentNode.SetNeighbour(neighbourNodeList, neighbourCostList);
				
				//if(neighbourNodeList.Count==0)
					//Debug.Log("no heighbour. node number "+counter+"  "+neighbourNodeList.Count);
			}
			
			//Debug.Log(currentN.pos+"  "+currentNode.neighbourNode.length);
			counter+=1;
		}
		
		//timeUsed=Time.realtimeSinceStartup-timeStart;
		//Debug.Log("connect neighbour for "+counter+" nodes, used "+timeUsed+"seconds");

		return nodeGraph;
	}

	void GenerateNode(){

		//float gridSize=GameControl.gameControlCom.TDS.gridSize;
		agentHeight=Mathf.Max(agentHeight, gridSize);

		area.x=Mathf.Floor(area.x/gridSize)*gridSize;
		area.y=Mathf.Floor(area.y/gridSize)*gridSize;
		
		area.width=Mathf.Floor(area.width/gridSize)*gridSize;
		area.height=Mathf.Floor(area.height/gridSize)*gridSize;

		nodeGraph=new NodeTD[(int)((area.width-area.x)/gridSize*(area.height-area.y)/gridSize)];

		float timeStart=Time.realtimeSinceStartup;

		int counter=0;
		float heightOffset=agentHeight/2;
		for(float j=area.y; j<area.height; j+=gridSize){
			for(float i=area.x; i<area.width; i+=gridSize){
				RaycastHit hit1;
				if(Physics.Raycast(new Vector3(i, 500, j), Vector3.down, out hit1)) {
					nodeGraph[counter]=new NodeTD(new Vector3(i, hit1.point.y+heightOffset, j), counter);
				}
				else{
					nodeGraph[counter]=new NodeTD(new Vector3(i, 0, j), counter);
					nodeGraph[counter].walkable=false;
				}
				counter+=1;
			}
		}
		//Debug.Log(counter);
		
		float timeUsed=Time.realtimeSinceStartup-timeStart;
		//Debug.Log("generate "+counter+" nodes, used "+timeUsed+"seconds");
		
		counter=0;
		RaycastHit hit2;
		foreach(NodeTD cNode in nodeGraph){
			if(cNode.walkable){
				if(Physics.SphereCast(cNode.pos+new Vector3(0, heightOffset+heightOffset*0.1f, 0), gridSize*0.45f, Vector3.down, out hit2, heightOffset)){
					cNode.walkable=false;
					counter+=1;
				}
			}
		}
		//if(counter>0) Debug.Log(counter+" node is unwalkable");

		float neighbourDistance=0;
		float neighbourRange;
		if(connectDiagonalNeighbour)	neighbourRange=gridSize*1.5f;
		else neighbourRange=gridSize*1.1f;
		
		timeStart=Time.realtimeSinceStartup;
		
		int rowLength=(int)Mathf.Floor((area.width-area.x)/gridSize);
		int columnLength=(int)Mathf.Floor((area.height-area.y)/gridSize);

		counter=0;
		//assign the neighouring  node for each node in the grid
		foreach(NodeTD currentNode in nodeGraph){
			//only if that node is walkable
			if(currentNode.walkable){
			
				//create an empty array
				List<NodeTD> neighbourNodeList=new List<NodeTD>();
				List<float> neighbourCostList=new List<float>();
				
				NodeTD[] neighbour=new NodeTD[8];
				int id=currentNode.ID;
				
				if(id>rowLength-1 && id<rowLength*columnLength-rowLength){
					//print("middle rows");
					if(id!=rowLength) neighbour[0]=nodeGraph[id-rowLength-1];
					neighbour[1]=nodeGraph[id-rowLength];
					neighbour[2]=nodeGraph[id-rowLength+1];
					neighbour[3]=nodeGraph[id-1];
					neighbour[4]=nodeGraph[id+1];
					neighbour[5]=nodeGraph[id+rowLength-1];
					neighbour[6]=nodeGraph[id+rowLength];
					if(id!=rowLength*columnLength-rowLength-1)neighbour[7]=nodeGraph[id+rowLength+1];
				}
				else if(id<=rowLength-1){
					//print("first row");
					if(id!=0) neighbour[0]=nodeGraph[id-1];
					neighbour[1]=nodeGraph[id+1];
					neighbour[2]=nodeGraph[id+rowLength-1];
					neighbour[3]=nodeGraph[id+rowLength];
					neighbour[4]=nodeGraph[id+rowLength+1];
				}
				else if(id>=rowLength*columnLength-rowLength){
					//print("last row");
					neighbour[0]=nodeGraph[id-1];
					if(id!=rowLength*columnLength-1) neighbour[1]=nodeGraph[id+1];
					neighbour[2]=nodeGraph[id-rowLength-1];
					neighbour[3]=nodeGraph[id-rowLength];
					neighbour[4]=nodeGraph[id-rowLength+1];
				}
				


				//scan through all the node in the grid
				foreach(NodeTD node in neighbour){
					//if this the node is not currentNode
					if(node!=null && node.walkable){
						//if this node is within neighbour node range
						neighbourDistance=GetHorizontalDistance(currentNode.pos, node.pos);
						if(neighbourDistance<neighbourRange){
							//if nothing's in the way between these two
							if(!Physics.Linecast(currentNode.pos, node.pos)){
								//if the slop is not too steep
								if(Mathf.Abs(GetSlope(currentNode.pos, node.pos))<=maxSlope){
									//add to list
									//if(!node.walkable) Debug.Log("error");
									neighbourNodeList.Add(node);
									neighbourCostList.Add(neighbourDistance);
								}//else print("too steep");
							}//else print("something's in the way");
						}//else print("out of range "+neighbourDistance);
					}
				}

				//set the list as the node neighbours array
				currentNode.SetNeighbour(neighbourNodeList, neighbourCostList);
				
				//if(neighbourNodeList.Count==0)
					//Debug.Log("no heighbour. node number "+counter+"  "+neighbourNodeList.Count);
			}
			
			//Debug.Log(currentN.pos+"  "+currentNode.neighbourNode.length);
			counter+=1;
		}
		
		//timeUsed=Time.realtimeSinceStartup-timeStart;
		//Debug.Log("connect neighbour for "+counter+" nodes, used "+timeUsed+"seconds");
		
	}


	static float GetHorizontalDistance(Vector3 p1, Vector3 p2){
		p1.y=0;
		p2.y=0;
		
		return Vector3.Distance(p1, p2);
	}


	float GetSlope(Vector3 p1, Vector3 p2){
		var h1=p1.y;
		var h2=p2.y;
		
		float distH=GetHorizontalDistance(p1, p2);
		float slope=(h1-h2)/distH;
		
		return Mathf.Atan(slope)*Mathf.Rad2Deg;
	}

	NodeTD GetNearestWalkableNode(Vector3 point){
		float dist=Mathf.Infinity;
		float currentNearest=Mathf.Infinity;
		NodeTD nearestNode=null;
		//float rangeTH=gridSize*1.5;
		foreach(NodeTD node in nodeGraph){
			if(node.walkable){
				dist=Vector3.Distance(point, node.pos);
				if(dist<currentNearest){
					currentNearest=dist;
					nearestNode=node;
					//if(currentNearest<rangeTH) break;
				}
			}
		}
		
		return nearestNode;
	}

	NodeTD GetNearestUnwalkableNode(Vector3 point){
		float dist=Mathf.Infinity;
		float currentNearest=Mathf.Infinity;
		NodeTD nearestNode=null;
		foreach(NodeTD node in nodeGraph){
			if(!node.walkable){
				dist=Vector3.Distance(point, node.pos);
				if(dist<currentNearest){
					currentNearest=dist;
					nearestNode=node;
				}
			}
		}
		return nearestNode;
	}

	void BlockNode(Vector3 pos){
		NodeTD node=GetNearestWalkableNode(pos);
		pos.y=node.pos.y;
		if(Vector3.Distance(pos, node.pos)<NodeGeneratorTD._gridSize/2)
			node.walkable=false;
	}

	void UnblockNode(Vector3 pos){
		NodeTD node=GetNearestUnwalkableNode(pos);
		if(node!=null){
			pos.y=node.pos.y;
			if(Vector3.Distance(pos, node.pos)<NodeGeneratorTD._gridSize/2)
				node.walkable=true;
		}
	}
	
	public static bool ConnectDiagonalNeighbour(){
		return nodeGenerator.connectDiagonalNeighbour;
	}


	//all gizmo related code goes here
	[HideInInspector] public bool showGizmo=true;
	private NodeTD currentNode;

	IEnumerator CycleNode(){
		int counter=0;
		while(true){
			yield return new WaitForSeconds(0.15f);
			currentNode=nodeGraph[counter];
			counter+=1;
			if(counter==nodeGraph.Length) counter=0;
		}
	}

	void OnDrawGizmos(){
		if(showGizmo){
			if(nodeGraph!=null){
				Gizmos.color = Color.white;
				foreach(NodeTD node in nodeGraph){
					if(node!=null && node.walkable) Gizmos.DrawSphere (node.pos, 0.2f);
					else if(node!=null && !node.walkable) Gizmos.DrawSphere (node.pos, 0.5f);
				}
			}
			
			if(currentNode!=null){
				Gizmos.color = Color.red;
				foreach(NodeTD neighbour in currentNode.neighbourNode){
					Gizmos.DrawSphere (neighbour.pos, 0.2f);
				}
			}
		}
	}

}



public enum _ListStateTD{Unassigned, Open, Close};

public class NodeTD{
	public int ID;
	public Vector3 pos;
	public NodeTD[] neighbourNode;
	public float[] neighbourCost;
	public NodeTD parent;
	public bool walkable=true;
	public float scoreG;
	public float scoreH;
	public float scoreF;
	public _ListStateTD listState=_ListStateTD.Unassigned;
	public float tempScoreG=0;
	
	public NodeTD(){}
	
	public NodeTD(Vector3 position, int id){
		pos=position;
		ID=id;
	}
	
	public void SetNeighbour(List<NodeTD> arrNeighbour, List<float> arrCost){
		neighbourNode = arrNeighbour.ToArray();
		neighbourCost = arrCost.ToArray();
	}
	
	public void ProcessNeighbour(NodeTD node){
		ProcessNeighbour(node.pos);
	}
	
	//call during a serach to scan through all neighbour, check their score against the position passed
	public void ProcessNeighbour(Vector3 pos){
		for(int i=0; i<neighbourNode.Length; i++){
			//if the neightbour state is clean (never evaluated so far in the search)
			if(neighbourNode[i].listState==_ListStateTD.Unassigned){
				//check the score of G and H and update F, also assign the parent to currentNode
				neighbourNode[i].scoreG=scoreG+neighbourCost[i];
				neighbourNode[i].scoreH=Vector3.Distance(neighbourNode[i].pos, pos);
				neighbourNode[i].UpdateScoreF();
				neighbourNode[i].parent=this;
			}
			//if the neighbour state is open (it has been evaluated and added to the open list)
			else if(neighbourNode[i].listState==_ListStateTD.Open){
				//calculate if the path if using this neighbour node through current node would be shorter compare to previous assigned parent node
				tempScoreG=scoreG+neighbourCost[i];
				if(neighbourNode[i].scoreG>tempScoreG){
					//if so, update the corresponding score and and reassigned parent
					neighbourNode[i].parent=this;
					neighbourNode[i].scoreG=tempScoreG;
					neighbourNode[i].UpdateScoreF();
				}
			}
		}
	}
	
	void UpdateScoreF(){
		scoreF=scoreG+scoreH;
	}
	
}

/*
[System.Serializable]
public class PlaneConnection : MonoBehaviour{
	public Transform plane1;
	public Transform plane2;
	public bool isConnected=false;
	public bool isAligned=false;
	public Vector3[] overlapPoint;
}*/