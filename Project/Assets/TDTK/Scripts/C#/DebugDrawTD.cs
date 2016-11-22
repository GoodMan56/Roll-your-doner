using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DebugDrawTD : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public static void Square(Vector3 point, float duration){
		Square(point, 0.5f, Color.white, duration);
	}
	public static void Square(Vector3 point, Color color, float duration){
		Square(point, 0.5f, color, duration);
	}
	public static void Square(Vector3 point, float width, Color color, float duration){
		width*=0.5f;
		Debug.DrawLine(point+new Vector3(-width, 0, width), point+new Vector3(width, 0, width), color, duration);
		Debug.DrawLine(point+new Vector3(width, 0, -width), point+new Vector3(-width, 0, -width), color, duration);
		Debug.DrawLine(point+new Vector3(-width, 0, width), point+new Vector3(-width, 0, -width), color, duration);
		Debug.DrawLine(point+new Vector3(width, 0, -width), point+new Vector3(width, 0, width), color, duration);
	}
	
	public static void Cross(Vector3 point, float duration){
		Cross(point, 0.5f, Color.white, duration);
	}
	public static void Cross(Vector3 point, Color color, float duration){
		Cross(point, 0.5f, color, duration);
	}
	public static void Cross(Vector3 point, float width, Color color, float duration){
		width*=0.5f;
		Debug.DrawLine(point+new Vector3(width, 0, width), point+new Vector3(-width, 0, -width), color, duration);
		Debug.DrawLine(point+new Vector3(-width, 0, width), point+new Vector3(width, 0, -width), color, duration);
	}
	
	public static void Rect(Vector3 center, float width, float height, Color color, float duration){
		width=width/2;
		height=height/2;
		Debug.DrawLine(center+new Vector3(width, 0, -height), center+new Vector3(width, 0, height), color, duration);
		Debug.DrawLine(center+new Vector3(width, 0, -height), center+new Vector3(-width, 0, -height), color, duration);
		Debug.DrawLine(center+new Vector3(-width, 0, height), center+new Vector3(-width, 0, -height), color, duration);
		Debug.DrawLine(center+new Vector3(-width, 0, height), center+new Vector3(width, 0, height), color, duration);
	}
	
	public static void Rect(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, Color color, float duration){
		Debug.DrawLine(p1, p2, color, duration);
		Debug.DrawLine(p2, p3, color, duration);
		Debug.DrawLine(p3, p4, color, duration);
		Debug.DrawLine(p4, p1, color, duration);
	}
	

	
	public static void PathPointSquare(List<Vector3> p, Color col, float duration){
		for(int k=0; k<p.Count; k++){
			Square(p[k], col, duration);
		}
	}
	
	public static void PathPointSquare(List<Vector3> p, float duration){
		Debug.Log("draw");
		float r=0f;
		float g=1f;
		for(int k=0; k<p.Count; k++){
			r+=1f/p.Count;
			g-=1f/p.Count;
			Color col=new Color(r, g, 0, 1);
			Square(p[k], col, duration);
			//~ Square(p[k]+new Vector3(0, k*0.5f, 0), col, duration);
			//~ if(k>0) Debug.DrawLine(p[k-1]+new Vector3(0, (k-1)*0.5f, 0), p[k]+new Vector3(0, k*0.5f, 0), col, duration);
		}
	}
	
	public static void PathPointSquare(List<Vector3> p, float width, Color col, float duration){
		for(int k=0; k<p.Count; k++){
			Square(p[k], width, col, duration);
		}
	}
	
	public static void PathPointSquare(List<Vector3> p, float width, float duration){
		float r=0f;
		float g=1f;
		for(int k=0; k<p.Count; k++){
			r+=1f/p.Count;
			g-=1f/p.Count;
			Color col=new Color(r, g, 0, 1);
			Square(p[k], width, col, duration);
		}
	}
	
	public static void PathPointCross(List<Vector3> p, Color col, float duration){
		for(int k=0; k<p.Count; k++){
			Cross(p[k], col, duration);
		}
	}
	
	public static void PathPointCross(List<Vector3> p, float duration){
		float r=0f;
		float g=1f;
		for(int k=0; k<p.Count; k++){
			r+=1f/p.Count;
			g-=1f/p.Count;
			Color col=new Color(r, g, 0, 1);
			Cross(p[k], col, duration);
		}
	}
	
	public static void PathPointCross(List<Vector3> p, float width, Color col, float duration){
		for(int k=0; k<p.Count; k++){
			Cross(p[k], width, col, duration);
		}
	}
	
	public static void PathPointCross(List<Vector3> p, float width, float duration){
		float r=0f;
		float g=1f;
		for(int k=0; k<p.Count; k++){
			r+=1f/p.Count;
			g-=1f/p.Count;
			Color col=new Color(r, g, 0, 1);
			Cross(p[k], width, col, duration);
		}
	}
	
	/*
	public static void ANode(ANode aNode, float duration){
		ANode(aNode, true, Color.white, duration);
	}
	public static void ANode(ANode aNode, Color color, float duration){
		ANode(aNode, true, color, duration);
	}
	public static void ANode(ANode aNode, bool showNeighbour, Color color, float duration){
		//Node[] nodeGraph=NodeGeneratorC.GetNodeGraph();
		float gridSize=NodeGeneratorC.GetGridSize();
		
			DebugDraw.Rect(aNode.pos, aNode.width, aNode.length, color, 3f);
			//~ Color color=new Color(Random.Range(0.25f, 1.0f), Random.Range(0.25f, 1.0f), Random.Range(0.25f, 1.0f), 1);
			//~ for(int i=0; i<aNode.sortedNeighbourNodes.Count; i++){
				//~ Node[] list=aNode.sortedNeighbourNodes[i];
				
				//~ if(list.Length>=2){
					//~ for(int j=1; j<list.Length; j++){
						//~ Debug.DrawLine(list[j-1].pos, list[j].pos, color, duration);
					//~ }
				//~ }
			//~ }
		
		if(showNeighbour){
			for(int j=0; j<aNode.sortedNeighbourNodes.Count; j++){
				Color col=new Color(Random.Range(0.25f, 1.0f), Random.Range(0.25f, 1.0f), Random.Range(0.25f, 1.0f), 1);
				foreach(Node node in aNode.sortedNeighbourNodes[j]){
					if(node!=null) DebugDraw.Square(node.pos, gridSize, col, duration);
				}
			}
		}
	}
	*/
}
