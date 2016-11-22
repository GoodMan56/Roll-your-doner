using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameMessage : MonoBehaviour {

	//~ public GUIText displayText;
	//~ [HideInInspector] public GUIText guiText2;
	
	public static GameMessage gameMessage;
	
	//~ private bool displayed=false;
	//~ private float timeDisplay;
	
	//~ private bool displayed2=false;
	//~ private float timeDisplay2;
	
	private bool init=false;
	//~ private GameObject messageObj;
	
	private bool uiFlag=false;
	private bool nGUIFlag=false;
	//private bool uiiOSFlag=true;
	
	void Awake () {
		if(gameMessage==null){
			gameMessage=this;
			//~ messageObj=gameObject;
			gameMessage.init=true;
			Init();
		}
		
		UI ui=(UI)FindObjectOfType(typeof(UI));
		if(ui!=null && ui.enabled){
			uiFlag=true;
		}
		
		UInGUI uiNGUI=(UInGUI)FindObjectOfType(typeof(UInGUI));
		if(uiNGUI!=null && uiNGUI.enabled){
			nGUIFlag=true;
		}
		
	}
	
	private List<string> msgList=new List<string>();
	public static void DisplayMessage(string msg){
		if(!gameMessage.init){
			GameObject obj=new GameObject();
			obj.name="GameMessage";
			gameMessage=obj.AddComponent<GameMessage>();
			//~ gameMessage.messageObj=obj;
		
			Init();
		}
		
		if(gameMessage.uiFlag) UI.DisplayMessage(msg);
		else if(gameMessage.nGUIFlag) UInGUI.DisplayMessage(msg);
		else{
			gameMessage.msgList.Add(msg);
			gameMessage.StartCoroutine(gameMessage.ClearMessage());
		}
	}
	
	IEnumerator ClearMessage(){
		yield return new WaitForSeconds(2f);
		msgList.RemoveAt(0);
	}
	
	void OnGUI(){
		//~ GUIStyle style=new GUIStyle();
		//~ style.alignment=TextAnchor.LowerCenter;
		//~ style.fontSize=14;
		//~ style.fontStyle=FontStyle.Bold;
		//~ style.normal.textColor=Color.white;
		
		//~ string message="";
		//~ foreach(string msg in msgList){
			//~ message+=msg+"\n";
		//~ }
		
		//~ GUI.Label(new Rect(0, 0, Screen.width, Screen.height/2), message, style);
		
		
		GUIStyle style=new GUIStyle();
		style.alignment=TextAnchor.LowerCenter;
		style.fontSize=14;
		style.fontStyle=FontStyle.Bold;
		style.normal.textColor=Color.black;
		
		string message="";
		foreach(string msg in msgList){
			message+=msg+"\n";
		}
		
		GUI.depth = 99;
		GUI.Label(new Rect(0, 0, Screen.width, Screen.height/2), message, style);
		
		style.normal.textColor=Color.white;
		GUI.Label(new Rect(-2f, -2f, Screen.width, Screen.height/2), message, style);
	}
	
	
	
	static public void Init(){
		
		if(gameMessage==null){
			GameObject obj=new GameObject();
			obj.name="GameMessage";
			gameMessage=obj.AddComponent<GameMessage>();
			
			//~ gameMessage.messageObj=obj;
		}
		
		gameMessage.init=true;
		
		/*
		if(gameMessage.displayText==null){
			//~ GameObject obj=new GameObject();
			//~ obj.name="guiText1";
			
			//~ Transform t=obj.transform;
			//~ t.parent=gameMessage.messageObj.transform;
			//~ t.position=new Vector3(1-(10f/Screen.width), 0, 1);
			
			//~ gameMessage.displayText=obj.AddComponent<GUIText>();
			
			//~ gameMessage.displayText.alignment = TextAlignment.Right;
			//~ gameMessage.displayText.anchor = TextAnchor.LowerRight;
		}
		
		if(gameMessage.guiText2==null){
//			GameObject obj=new GameObject();
//			obj.name="guiText2";
//			
//			Transform t=obj.transform;
//			t.parent=gameMessage.messageObj.transform;
//			t.position=new Vector3(1-(10f/Screen.width), 0, 1);
//			
//			gameMessage.guiText2=obj.AddComponent<GUIText>();
//			
//			gameMessage.guiText2.alignment = TextAlignment.Right;
//			gameMessage.guiText2.anchor = TextAnchor.LowerRight;
		}
		*/
	}
	
	
	/*
	static public void DisplayMessage1(string str){
		if(!gameMessage.init){
			GameObject obj=new GameObject();
			obj.name="GameMessage";
			gameMessage=obj.AddComponent<GameMessage>();
			//~ gameMessage.messageObj=obj;
		
			Init();
		}
		
		if(gameMessage.uiFlag) UI.DisplayMessage(str);
		else gameMessage.DisplayMsg(str);
	}
	
	
	void DisplayMsg(string str){
		timeDisplay=Time.time;
		displayText.text=displayText.text+str+"\n";
		if(!displayed){
			displayed=true;
			StartCoroutine(DisplayRoutine());
		}
	}
	
	IEnumerator DisplayRoutine(){
		while(Time.time-timeDisplay<3){
			yield return null;
		}
		displayed=false;
		displayText.text="";
	}
	
	static public void DisplayMessage2(string str){
		//if(gameMessage==null) Init();
		gameMessage.DisplayMsg2(str);
	}
	
	void DisplayMsg2(string str){
		timeDisplay2=Time.time;
		guiText2.text=str;
		if(!displayed2){
			displayed2=true;
			StartCoroutine(DisplayRoutine2());
		}
	}
	
	IEnumerator DisplayRoutine2(){
		while(Time.time-timeDisplay2<2){
			yield return null;
		}
		displayed2=false;
		guiText2.text="";
	}
	*/
	
}
