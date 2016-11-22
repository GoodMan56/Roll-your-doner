using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OverlayManager : MonoBehaviour {

	static private GUITexture[] overlays=new GUITexture[3];
	static private GUITexture[] overlaysB=new GUITexture[3];
	static private bool[] inUseFlags=new bool[3];
	
	static public OverlayManager overlayManager;
	
	static private Transform camT;
	
	static public float widthModifier=1f;
	static public float heightModifier=1f;
	static public Vector3 offset=Vector3.zero;
	
	
	
	public bool enableOverlayText=false;
	public bool scaleTextSizeToDistance=true;
	public float overlayTextSizeMod=25;
	//~ public bool outlinedText=true;
	
	
	
	public static void SetModifier(float w, float h){
		widthModifier=w;
		heightModifier=h;
	}
	
	public static void SetOffset(Vector3 os){
		offset=os;
	}
	
	void Awake(){
		SelfInit();
		overlayText=new List<OverlayText>();
	}
	
	// Use this for initialization
	void Start () {
	
	}
	
	void OnEnable(){
		UnitTower.onGainResourceE += OnGainResource;
		UnitCreep.onGainResourceE += OnGainResource;
	}
	
	void OnDisable(){
		UnitTower.onGainResourceE -= OnGainResource;
		UnitCreep.onGainResourceE -= OnGainResource;
	}
	
	
	static public void Init11(){
		GameObject obj=new GameObject();
		obj.name="OverlayManger";
		overlayManager=obj.AddComponent<OverlayManager>();
	}
		
	void SelfInit(){
		GameObject obj=gameObject;
		if(overlayManager==null) overlayManager=this;
		else return;
		
		Camera cam=Camera.main;
		if(cam!=null)	camT=Camera.main.transform;
		
		int poolSize=3;
		overlays=new GUITexture[poolSize];
		overlaysB=new GUITexture[poolSize];
		inUseFlags=new bool[poolSize];
		
		if(overlayManager!=null){
			for(int i=0; i<poolSize; i++){
				obj=new GameObject();
				obj.name="overlay";
				obj.transform.parent=overlayManager.transform;
				obj.transform.localScale=new Vector3(0, 0, 1);
				
				overlays[i]=obj.AddComponent<GUITexture>();
				overlays[i].texture=(Texture)Resources.Load("GreenBar");
				overlays[i].pixelInset=new Rect(0, 0, 0, 0);
				
				obj=new GameObject();
				obj.name="overlayB";
				obj.transform.parent=overlayManager.transform;
				obj.transform.localScale=new Vector3(0, 0, 1);
				
				overlaysB[i]=obj.AddComponent<GUITexture>();
				overlaysB[i].texture=(Texture)Resources.Load("GreyBar");
				overlaysB[i].pixelInset=new Rect(0, 0, 0, 0);
				
				inUseFlags[i]=false;
			}
		}
	}
	
	static public void Building(UnitTower unit){
		if(overlayManager==null) Init11();
		
		int num=GetUnuseFlag();
		inUseFlags[num]=true;
		
		overlayManager.StartCoroutine(overlayManager.Building(num, unit));
	}
	
	static public void Unbuilding(UnitTower unit){
		if(overlayManager==null) Init11();
		
		int num=GetUnuseFlag();
		inUseFlags[num]=true;
		
		overlayManager.StartCoroutine(overlayManager.Unbuilding(num, unit));
	}
	
	static private int GetUnuseFlag(){
		for(int i=0; i<inUseFlags.Length; i++){
			if(!inUseFlags[i])	return i;
		}
		
		return 0;
	}
	
	IEnumerator Building(int i, UnitTower unit){
		
		float gridSize=BuildManager.GetGridSize();
		float totalDuration=unit.GetCurrentBuildDuration();
		
		while(true){
			if(unit==null) break;
			
			Vector3 screenPos=Camera.main.WorldToScreenPoint(unit.thisT.position+offset);
			screenPos=new Vector3((screenPos.x), screenPos.y, 0); 
			
			float remainDuration=unit.GetRemainingBuildDuration();
			if(remainDuration<=0) break;
			
			float dist=Vector3.Distance(camT.position, unit.thisT.position);
			
			float ratio=remainDuration/totalDuration;
			float width=widthModifier*20*gridSize*20/dist;
			float height=heightModifier*30/dist;
			
			overlaysB[i].pixelInset=new Rect(screenPos.x-width/2, screenPos.y, width, height);
			overlays[i].pixelInset=new Rect(screenPos.x-width/2, screenPos.y, width*(1-ratio), height);
			
			yield return null;
		}
		
		overlaysB[i].pixelInset=new Rect(0, 0, 0, 0);
		overlays[i].pixelInset=new Rect(0, 0, 0, 0);
		inUseFlags[i]=false;
	}
	
	IEnumerator Unbuilding(int i, UnitTower unit){
		
		float gridSize=BuildManager.GetGridSize();
		float totalDuration=unit.GetCurrentBuildDuration();
		
		while(true){
			if(unit==null) break;
			
			Vector3 screenPos=Camera.main.WorldToScreenPoint(unit.thisT.position+offset);
			screenPos=new Vector3((screenPos.x), screenPos.y, 0); 
			
			float remainDuration=unit.GetRemainingBuildDuration();
			if(remainDuration>=totalDuration) break;
			
			float dist=Vector3.Distance(camT.position, unit.thisT.position);
			
			float ratio=remainDuration/totalDuration;
			float width=widthModifier*20*gridSize*20/dist;
			float height=heightModifier*30/dist;
			
			overlaysB[i].pixelInset=new Rect(screenPos.x-width/2, screenPos.y, width, height);
			overlays[i].pixelInset=new Rect(screenPos.x-width/2, screenPos.y, width*(1-ratio), height);
			
			yield return null;
		}
		
		overlaysB[i].pixelInset=new Rect(0, 0, 0, 0);
		overlays[i].pixelInset=new Rect(0, 0, 0, 0);
		inUseFlags[i]=false;
	}
	
	
	void OnGainResource(GainResourcePos grp){
		Color col=new Color(0, 1, 1, 1);
		foreach(int val in grp.value){
			if(val>0){
				DisplayOverlayText(val.ToString(), grp.pos, col, true);
			}
		}
	}
	
	//~ void OnDamage(){
		//~ Color col=new Color(1, 0, 0, 1);
		//~ DisplayOverlayText(val.ToString(), grp.pos, col);
	//~ }
	
	private static List<OverlayText> overlayText=new List<OverlayText>();
	
	public static void DisplayOverlayText(string text, Vector3 pos, Color col, bool setToBold){
		if(overlayManager.enableOverlayText){
			overlayText.Add(new OverlayText(text, col, pos, Time.time, setToBold));
			overlayManager.StartCoroutine(overlayManager.OverlayTextRoutine());
		}
	}
	
	
	
	IEnumerator OverlayTextRoutine(){
		yield return new WaitForSeconds(1.0f);
		overlayText.RemoveAt(0);
	}
	
	void OnGUI(){
		GUI.depth=100;
		
		GUIStyle style=new GUIStyle();
		style.padding=new RectOffset (0, 0, 0, 0);
		style.alignment=TextAnchor.MiddleCenter;
		style.fontStyle=FontStyle.Bold;
		
		
		foreach(OverlayText ot in overlayText){
			
			
			//~ float offset=;
			
			Vector3 pos=ot.pos+new Vector3(0, (Time.time-ot.startT), 0);
			
			float dist=Vector3.Distance(Camera.main.transform.position, pos);
			int size=10;
			if(scaleTextSizeToDistance) size=(int)(overlayTextSizeMod*12/dist);
			
			if(size>0){
				Vector3 screenPos = Camera.main.WorldToScreenPoint(pos);
				screenPos.y=Screen.height-screenPos.y;
				//~ screenPos.y-=Time.time-ot.startT;
				
				style.fontSize=size;
				
				//~ Vector3 screenPos = Camera.main.WorldToScreenPoint(ot.pos);
				//~ screenPos.y-=Time.time-ot.startT;
				
				Rect rect=new Rect(screenPos.x-Screen.width/2, screenPos.y-Screen.height/2, Screen.width, Screen.height);
				
				
				style.normal.textColor=ot.color;
				
				
				//~ Rect rect=new Rect(screenPos.x, screenPos.y, 100, 100);
				GUI.Label(rect, ot.text, style);
				
			}
		}
	}
	
	void Update(){
		//~ if(Input.GetKeyDown(KeyCode.Space)){
			//~ //DisplayMessage(Random.Range(0, 999999999)+" "+Random.Range(0, 999999999)+" "+Random.Range(0, 999999999));
			//~ Vector3 pos=new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5));
			//~ string text=Random.Range(-2000, 2000).ToString();
			//~ Color color=new Color(Random.Range(0.5f, 1.0f), Random.Range(0.5f, 1.0f), Random.Range(0.5f, 1.0f), 1);
			//~ DisplayOverlayText(text, pos, color);
			
			//~ Debug.DrawLine(pos, pos+new Vector3(0, 1.5f, 0), color, 1);
		//~ }
	}
	
	
}


//
public class OverlayText{
	public string text;
	public Color color;
	public Vector3 pos;
	public float startT;
	public bool setToBold=false;
	
	
	public OverlayText(string txt, Color col, Vector3 p, float t): this(txt, col, p, t, false){}
	public OverlayText(string txt, Color col, Vector3 p, float t, bool setToBold){
		text=txt;
		color=col;
		pos=p;
		startT=t;
	}
}



public class GainResourcePos{
	public Vector3 pos;
	public int[] value;
	
	public GainResourcePos(Vector3 p, int[] val){
		pos=p;
		value=val;
	}
}

