using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private field assigned but not used.

public delegate void SetPathCallbackTD(List<Vector3> wp);

public class PathFinderTD : MonoBehaviour {

	static private List<SearchQueue> queue=new List<SearchQueue>();
	
	static bool searching=false;
	
	static PathFinderTD pathFinder;
	
	public enum _PathSmoothing{None, LOS, Mean}
	public _PathSmoothing pathSmoothing=_PathSmoothing.Mean;
	
	//public bool pathSmoothing=false;
	public int ScanNodeLimitPerFrame=350;
	
	
	static public bool IsPathSmoothingOn(){
		if(pathFinder.pathSmoothing!=_PathSmoothing.None) return true;
		return false;
	}
	
	void Awake(){
		pathFinder=this;
	}
	
	void Start(){
		//~ new CallBackDelegate(platform.TestCallBack());
	}
	
	static public void Init(){
		GameObject obj=new GameObject();
		pathFinder=obj.AddComponent<PathFinderTD>();
		
		obj.name="PathFinder";
	}
	
	static public void CheckInit(){
		if(pathFinder==null) Init();
	}
	
	//~ static public Queue SearchPath(){
		//~ return new Queue();
	//~ }
	
	static public void CallMeBack(SetPathCallbackTD callBackFunc){
		//~ FooCallbackType myDelegate = new FooCallbackType(
			//~ this.TestCallBack );
		//~ callBackFunc("i Hear you!!");
		
		//~ Debug.Log("queue");
		
		//~ queue.Add(new Queue(callBackFunc));
		//~ Debug.Log(queue.Count);
	}
	
	void Update(){
		//~ Debug.Log("update");
		if(queue.Count>0 && !searching){
			
			//~ Debug.Log("queueX");
			
			//~ List<Vector3> list=new List<Vector3>();
			//~ list.Add(new Vector3(99, 0, 99));
			
			//~ queue[0].callBackFunc(list);
			
			//~
			
			Search(queue[0].startNode, queue[0].endNode, queue[0].blockNode, queue[0].graph, queue[0].callBackFunc);
			queue.RemoveAt(0);
		}
	}
	
	//void OnGUI(){
		//GUI.Label(new Rect(200, 200, 200, 200), "queue count: "+queue.Count);
		//GUI.Label(new Rect(200, 220, 200, 200), "searching: "+searching);
	//}
	
	static public NodeTD GetNearestNode(Vector3 point, NodeTD[] graph){
		return GetNearestNode(point, graph, 1);
	}
	static public NodeTD GetNearestNode(Vector3 point, NodeTD[] graph, int searchMode){
		float dist=Mathf.Infinity;
		float currentNearest=Mathf.Infinity;
		NodeTD nearestNode=null;
		foreach(NodeTD node in graph){
			if(searchMode==0){
				dist=Vector3.Distance(point, node.pos);
				if(dist<currentNearest){
					currentNearest=dist;
					nearestNode=node;
				}
			}
			else if(searchMode==1){
				if(node.walkable){
					dist=Vector3.Distance(point, node.pos);
					if(dist<currentNearest){
						currentNearest=dist;
						nearestNode=node;
					}
				}
			}
			else if(searchMode==2){
				if(!node.walkable){
					dist=Vector3.Distance(point, node.pos);
					if(dist<currentNearest){
						currentNearest=dist;
						nearestNode=node;
					}
				}
			}
		}
		return nearestNode;
	}
	
	//~ static public void GetPathX(Vector3 startP, Vector3 endP, Node[] graph, SetPathCallback callBackFunc){
		//~ Node startN=GetNearestNode(startP, graph);
		//~ Node endN=GetNearestNode(endP, graph);
		
		//~ //GetPath(startNode, endNode, null, graph, callBackFunc, false, true);
		//~ if(!searching){
			//~ //commence search
			//~ Search(startN, endN, null, graph, callBackFunc, true);
		//~ }
		//~ else{
			//~ //if a serach is in progress, put current request into a list
			//~ SearchQueue q=new SearchQueue(startN, endN, null, graph, callBackFunc);
			//~ if(urgent) queue.Insert(0, q);
			//~ else queue.Add(q);
		//~ }
	//~ }
	
	static public void GetPath(Vector3 startP, Vector3 endP, NodeTD[] graph, SetPathCallbackTD callBackFunc){
		NodeTD startNode=GetNearestNode(startP, graph);
		NodeTD endNode=GetNearestNode(endP, graph);
		
		GetPath(startNode, endNode, null, graph, callBackFunc, false);
	}
	
	static public void GetPath(Vector3 startP, Vector3 endP, Vector3 blockP, NodeTD[] graph, SetPathCallbackTD callBackFunc){
		NodeTD startNode=GetNearestNode(startP, graph);
		NodeTD endNode=GetNearestNode(endP, graph);
		NodeTD blockNode=GetNearestNode(blockP, graph);
		
		GetPath(startNode, endNode, blockNode, graph, callBackFunc, false);
	}
	
	static public void GetPath(NodeTD startN, NodeTD endN, NodeTD[] graph, SetPathCallbackTD callBackFunc){
		GetPath(startN, endN, null, graph, callBackFunc, false);
	}
	
	static public void GetPath(NodeTD startN, NodeTD endN, NodeTD[] graph, SetPathCallbackTD callBackFunc, bool urgent){
		GetPath(startN, endN, null, graph, callBackFunc, urgent);
	}
	
	static public void GetPath(NodeTD startN, NodeTD endN, NodeTD blockN, NodeTD[] graph, SetPathCallbackTD callBackFunc, bool urgent){
		CheckInit();
		
		if(!searching){
			//commence search
			Search(startN, endN, blockN, graph, callBackFunc);
		}
		else{
			//if a serach is in progress, put current request into a list
			SearchQueue q=new SearchQueue(startN, endN, blockN, graph, callBackFunc);
			if(urgent) queue.Insert(0, q);
			else queue.Add(q);
		}
	}
	
	static private void Search(NodeTD startN, NodeTD endN, NodeTD blockN, NodeTD[] graph, SetPathCallbackTD callBackFunc){
		pathFinder.StartCoroutine(pathFinder._Search(startN, endN, blockN, graph, callBackFunc));
	}
	
	IEnumerator _Search(NodeTD startN, NodeTD endN, NodeTD blockN, NodeTD[] graph, SetPathCallbackTD callBackFunc){
		
		//~ foreach(NodeTD nn in graph){
			//~ DebugDrawTD.Cross(nn.pos, nn.walkable!=true ? Color.red : Color.white, 6);
			//~ DebugDrawTD.Square(nn.pos, nn.neighbourNode==null ? Color.red : Color.white, 6);
		//~ }
		
		//block node is for checking if a path exist if that particular node is block
		if(blockN!=null){
			//var switched:boolean=true;
			//if(block.walkable) block.walkable=false;
			//else switched=false;
			
			blockN.walkable=false;
		}
			

		//time tracker for performance check
		float timeStart=Time.realtimeSinceStartup;

		//mark that a serac has started, any further query will be queued
		searching=true;
		bool pathFound=true;
		
		int searchCounter=0;	//used to count the total amount of node that has been searched
		int loopCounter=0;		//used to count how many node has been search in the loop, if it exceed a value, bring it to the next frame
		//float LoopTime=Time.realtimeSinceStartup;

		//closelist, used to store all the node that are on the path
		List<NodeTD> closeList=new List<NodeTD>();
		//openlist, all the possible node that yet to be on the path, the number can only be as much as the number of node in the garph
		NodeTD[] openList=new NodeTD[graph.Length];
		
		//an array use to record the element number in the open list which is empty after the node is removed to be use as currentNode,
		//so we can use builtin array with fixed length for openlist, also we can loop for the minimal amount of node in every search
		List<int> openListRemoved=new List<int>();
		//current count of elements that are occupied in openlist, openlist[n>openListCounter] are null
		int openListCounter=0;

		//set start as currentNode
		NodeTD currentNode=startN;
		
		//use to compare node in the openlist has the lowest score, alwyas set to Infinity when not in used
		float currentLowestF=Mathf.Infinity;
		int id=0;	//use element num of the node with lowest score in the openlist during the comparison process
		int i=0;		//universal int value used for various looping operation

		//loop start
		while(true){
		
			//if we have reach the destination
			if(currentNode==endN) break;
			
			//for gizmo debug purpose
			//currentNodeBeingProcess=currentNode;

			//move currentNode to closeList;
			closeList.Add(currentNode);
			currentNode.listState=_ListStateTD.Close;
			
			//loop through the neighbour of current loop, calculate  score and stuff
			currentNode.ProcessNeighbour(endN);
			
			//put all neighbour in openlist
			foreach(NodeTD neighbour in currentNode.neighbourNode){
				if(neighbour.listState==_ListStateTD.Unassigned && neighbour.walkable) {
					//set the node state to open
					neighbour.listState=_ListStateTD.Open;
					//if there's an open space in openlist, fill the space
					if(openListRemoved.Count>0){
						openList[openListRemoved[0]]=neighbour;
						//remove the number from openListRemoved since this element has now been occupied
						openListRemoved.RemoveAt(0);
					}
					//else just stack on it and increase the occupication counter
					else{
						openList[openListCounter]=neighbour;
						openListCounter+=1;
					}
				}
			}
			
			//clear the current node, before getting a new one, so we know if there isnt any suitable next node
			currentNode=null;
			
			//get the next point from openlist, set it as current point
			//just loop through the openlist until we reach the maximum occupication
			//while that, get the node with the lowest score
			currentLowestF=Mathf.Infinity;
			id=0;
			for(i=0; i<openListCounter; i++){
				if(openList[i]!=null){
					if(openList[i].scoreF<currentLowestF){
						currentLowestF=openList[i].scoreF;
						currentNode=openList[i];
						id=i;
					}
				}
			}
			
			//if there's no node left in openlist, path doesnt exist
			if(currentNode==null) {
				pathFound=false;
				break;
			}
			
			//remove the new currentNode from openlist
			openList[id]=null;
			//put the id into openListRemoved so we know there's an empty element that can be filled in the next loop
			openListRemoved.Add(id);
			
			

			//increase the counter
			searchCounter+=1;
			loopCounter+=1;
			
			
			//if exceed the search limit per frame, bring the search to the next frame
			if(loopCounter>ScanNodeLimitPerFrame){
				loopCounter=0;	//reset the loopCounter for the next frame
				yield return null;
			}
		}

		//see how long it has taken to complete the search
		//float timeUsed=Time.realtimeSinceStartup-timeStart;
		//if(pathFound)	Debug.Log("Path found. Searched "+searchCounter+" nodes, used "+timeUsed+"seconds");
		//else	Debug.Log("no path. Searched "+searchCounter+" nodes, used "+timeUsed+"seconds");


		//trace back the path through closeList
		List<Vector3> p=new List<Vector3>();
			
		if(pathFound){
			//track back the node's parent to form a path
			while(currentNode!=null){
				p.Add(currentNode.pos);
				currentNode=currentNode.parent;
			}
			
			//since the path is now tracked from endN ot startN, invert the list
			p=InvertArray(p);
			//~ if(pathSmoothing) {
				//~ //Debug.Log("smoothing path");
				//~ p=LOSPathSmoothingBackward(p);
				//~ p=LOSPathSmoothingForward(p);
			//~ }
			
			if(pathSmoothing==_PathSmoothing.Mean){
				p=MeanSmoothPath(p);
			}
			else if(pathSmoothing==_PathSmoothing.LOS){
				p=LOSPathSmoothingBackward(p);
				p=LOSPathSmoothingForward(p);
			}
			
			//else Debug.Log("skip smoothing path");
		}
		//~ else Debug.Log("no path found");
		
		//if this is just to check if a path is blocked, now we can clear the assumed blocked node
		if(blockN!=null) blockN.walkable=true; 
		
		callBackFunc(p);

		//reset the all the nodegraph's node state?
		//Reset();
		
		//clear searching so indicate the search has end and a new serach can be called
		searching=false;
		
		ResetGraph(graph);
	
	}
	
	
	static public List<NodeTD> GetNodeInFootprint(NodeTD origin, int footprint){
		if(footprint<=0) return new List<NodeTD>();
		
		bool connectDNeighbour=NodeGeneratorTD.ConnectDiagonalNeighbour();
		
		List<NodeTD> currentList=new List<NodeTD>();
		List<NodeTD> openList=new List<NodeTD>();
		List<NodeTD> closeList=new List<NodeTD>();
		
		if(connectDNeighbour){
			closeList.Add(origin);
			foreach(NodeTD node in origin.neighbourNode){
				currentList.Add(node);
			}
			
			for(int i=0; i<footprint; i++){
				openList=currentList;
				currentList=new List<NodeTD>();
				foreach(NodeTD node in openList){
					foreach(NodeTD neighbour in node.neighbourNode){
						if(!openList.Contains(neighbour) && !closeList.Contains(neighbour)){
							currentList.Add(neighbour);
						}
					}
					closeList.Add(node);
				}
			}
		}
		else{
			closeList.Add(origin);
			foreach(NodeTD node in origin.neighbourNode){
				currentList.Add(node);
			}
			
			//~ float range=1.5f*(footprint)*BuildManager.GetGridSize();
			float range=1*(footprint)*BuildManager.GetGridSize()+BuildManager.GetGridSize()*0.25f;
			
			for(int i=0; i<footprint*2; i++){
				openList=currentList;
				currentList=new List<NodeTD>();
				foreach(NodeTD node in openList){
					foreach(NodeTD neighbour in node.neighbourNode){
						//~ if(Vector3.Distance(origin.pos, neighbour.pos)<range){
						if(Mathf.Abs(origin.pos.x-neighbour.pos.x)<=range && Mathf.Abs(origin.pos.z-neighbour.pos.z)<=range){
							if(!openList.Contains(neighbour) && !closeList.Contains(neighbour)){
								currentList.Add(neighbour);
							}
						}
					}
					closeList.Add(node);
				}
			}
		}
		 
		return closeList;
	}
	
	//~ public List<Node> GetNeighbouringNode(Node origin, Node node, int iteration){
		//~ float gridSize=Node.GetGridSize();
		//~ List<Node> nodeList=new List<Node>();
		//~ foreach(Node node in node.neighbourNode){
			//~ if(Vector3.Distance(node.pos, origin.pos)<gridSize*1.5f){
				//~ nodeList.Add(node);
			//~ }
		//~ }
	//~ }
	
	//make cause system to slow down, use with care
	static public List<Vector3> ForceSearch(NodeTD startN, NodeTD endN, NodeTD blockN, NodeTD[] graph, int footprint){
		if(blockN!=null){
			blockN.walkable=false;
			//Debug.Log("block reference node");
			
			List<NodeTD> tempList=GetNodeInFootprint(blockN, footprint);
			//Debug.Log(footprint+"  "+tempList.Count);
			foreach(NodeTD node in tempList){
				node.walkable=false;
			}
		}
		
		bool pathFound=true;
		
		int searchCounter=0;	//used to count the total amount of node that has been searched
		
		List<NodeTD> closeList=new List<NodeTD>();
		NodeTD[] openList=new NodeTD[graph.Length];
		
		List<int> openListRemoved=new List<int>();
		int openListCounter=0;

		NodeTD currentNode=startN;
		
		float currentLowestF=Mathf.Infinity;
		int id=0;	//use element num of the node with lowest score in the openlist during the comparison process
		int i=0;		//universal int value used for various looping operation
		
		while(true){
		
			if(currentNode==endN) break;
			
			closeList.Add(currentNode);
			currentNode.listState=_ListStateTD.Close;
			
			currentNode.ProcessNeighbour(endN);
			
			foreach(NodeTD neighbour in currentNode.neighbourNode){
				if(neighbour.listState==_ListStateTD.Unassigned && neighbour.walkable) {
					neighbour.listState=_ListStateTD.Open;
					if(openListRemoved.Count>0){
						openList[openListRemoved[0]]=neighbour;
						openListRemoved.RemoveAt(0);
					}
					else{
						openList[openListCounter]=neighbour;
						openListCounter+=1;
					}
				}
			}
			
			currentNode=null;
			
			currentLowestF=Mathf.Infinity;
			id=0;
			for(i=0; i<openListCounter; i++){
				if(openList[i]!=null){
					if(openList[i].scoreF<currentLowestF){
						currentLowestF=openList[i].scoreF;
						currentNode=openList[i];
						id=i;
					}
				}
			}
			
			if(currentNode==null) {
				pathFound=false;
				break;
			}
			
			openList[id]=null;
			openListRemoved.Add(id);

			searchCounter+=1;
			
		}

		//~ float timeUsed=Time.realtimeSinceStartup-timeStart;
		//~ if(pathFound)	Debug.Log("Path found. Searched "+searchCounter+" nodes, used "+timeUsed+"seconds");
		//~ else	Debug.Log("no path. Searched "+searchCounter+" nodes, used "+timeUsed+"seconds");


		List<Vector3> p=new List<Vector3>();
			
		if(pathFound){
			while(currentNode!=null){
				p.Add(currentNode.pos);
				currentNode=currentNode.parent;
			}
			
			p=InvertArray(p);
			//if(pathFinder.pathSmoothing) {
			//	p=pathFinder.LOSPathSmoothingBackward(p);
			//	p=pathFinder.LOSPathSmoothingForward(p);
			//}
		}
		
		if(blockN!=null) {
			blockN.walkable=true; 
			//Debug.Log("unblock reference node");
			
			List<NodeTD> tempList=GetNodeInFootprint(blockN, footprint);
			foreach(NodeTD node in tempList){
				node.walkable=true;
			}
		}
		
		ResetGraph(graph);
		
		return p;

	}
	
	static public List<Vector3> SmoothPath(List<Vector3> p){
		//~ if(pathFinder.pathSmoothing) {
			//~ p=pathFinder.LOSPathSmoothingBackward(p);
			//~ p=pathFinder.LOSPathSmoothingForward(p);
		//~ }
		
		if(pathFinder.pathSmoothing==_PathSmoothing.Mean){
			p=MeanSmoothPath(p);
		}
		else if(pathFinder.pathSmoothing==_PathSmoothing.LOS){
			p=pathFinder.LOSPathSmoothingBackward(p);
			p=pathFinder.LOSPathSmoothingForward(p);
		}
		
		return p;
	}
	

	
	static private List<Vector3> InvertArray(List<Vector3> p){
		List<Vector3> pInverted=new List<Vector3>();
		for(int i=0; i<p.Count; i++){
			pInverted.Add(p[p.Count-(i+1)]);
		}
		return pInverted;
	}
	
	static public List<Vector3> MeanSmoothPath(List<Vector3> p){
		//~ p=pathFinder.LOSPathSmoothingBackward(p);
		//~ p=pathFinder.LOSPathSmoothingForward(p);
		
		if(p.Count<=2) return p;
		
		for(int i=0; i<p.Count; i++){
			if(i==0){
				//~ if(p.Count>=2) p[i]=(p[i]+p[i+1])/2;
				//~ else break;
			}
			else if(i==p.Count-1){
				//~ if(p.Count>=3) p[i]=(p[i-1]+p[i])/2;
				//~ else break;
			}
			else p[i]=(p[i-1]+p[i]+p[i+1])/3;
		}
		
		return p;
	}
	
	
	
	static public List<Vector3> MeanSmoothPath5(List<Vector3> p){
		//~ p=pathFinder.LOSPathSmoothingBackward(p);
		//~ p=pathFinder.LOSPathSmoothingForward(p);
		
		for(int i=1; i<p.Count-1; i++){
			if(i==0){
				if(p.Count>=3) p[i]=(p[i]+p[i+1]+p[i+2])/3;
				else break;
			}
			else if(i==1){
				if(p.Count==3) p[i]=(p[i-1]+p[i]+p[i+1])/3;
				if(p.Count>=4) p[i]=(p[i-1]+p[i]+p[i+1]+p[i+2])/4;
				else break;
			}
			else if(i==p.Count-2){
				if(p.Count==3) p[i]=(p[i-1]+p[i]+p[i+1])/3;
				if(p.Count>=4) p[i]=(p[i-2]+p[i-1]+p[i]+p[i+1])/4;
				else break;
			}
			else if(i==p.Count-1){
				if(p.Count>=3) p[i]=(p[i-2]+p[i-1]+p[i])/3;
				else break;
			}
			else p[i]=(p[i-2]+p[i-1]+p[i]+p[i+1]+p[i+2])/5;
		}
		
		return p;
	}
	
	//pathSmoothing forward
	private List<Vector3> LOSPathSmoothingForward(List<Vector3> p){
		float gridSize=BuildManager.GetGridSize();
		int num=0;
		float allowance=gridSize*0.4f;
		while (num+2<p.Count){
			bool increase=false;
			Vector3 p1=p[num];
			Vector3 p2=p[num+2];
			RaycastHit hit;
			Vector3 dir=p2-p1;
			
			LayerMask mask=1<<LayerManager.LayerTerrain();
			if(!Physics.SphereCast(p1, allowance, dir, out hit, Vector3.Distance(p2, p1), ~mask)){
				if(p1.y==p2.y) p.RemoveAt(num+1);
				else increase=true;
			}
			else {
				increase=true;
			}
			
			if(increase) num+=1;

		}
		return p;
	}



	//pathSmoothing backward
	private List<Vector3> LOSPathSmoothingBackward(List<Vector3> p){
		float gridSize=BuildManager.GetGridSize();
		int num=p.Count-1;
		float allowance=gridSize*0.4f;
		while (num>1){
			bool decrease=false;
			Vector3 p1=p[num];
			Vector3 p2=p[num-2];
			RaycastHit hit;
			Vector3 dir=p2-p1;
			
			if(!Physics.SphereCast(p1, allowance, dir, out hit, Vector3.Distance(p2, p1))){
				if(p1.y==p2.y) p.RemoveAt(num-1);
				else decrease=true;
			}
			else {
				decrease=true;
			}
			
			num-=1;
			if(decrease) num-=1;

		}
		return p;
	}
	
	static public void ResetGraph(NodeTD[] nodeGraph){
		foreach(NodeTD node in nodeGraph){
			node.listState=_ListStateTD.Unassigned;
			node.parent=null;
		}
	}
	
}

//~ public class QueueElement{
	//~ public SetPathCallback callBackFunc;
	
	//~ public Queue(SetPathCallback callBack){
		//~ callBackFunc=callBack;
	//~ }
//~ }

class SearchQueue{
	public NodeTD startNode;
	public NodeTD endNode;
	public NodeTD blockNode;
	public NodeTD[] graph;
	public SetPathCallbackTD callBackFunc;
	
	public SearchQueue(NodeTD n1, NodeTD n2, NodeTD n3, NodeTD[] g, SetPathCallbackTD func){
		startNode=n1;
		endNode=n2;
		blockNode=n3;
		graph=g;
		callBackFunc=func;
	}
}
