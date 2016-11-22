using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIPerk : MonoBehaviour {

	public delegate void PerkWindowHandler(bool flag); 
	public static event PerkWindowHandler onPerkWindowE;
	
	//~ private bool perkManager=false;;
	
	private bool showWindow=false;
	private int selectedID=0;
	
	public static UIPerk uiPerk;
	public CameraControl camControl;
	
	public bool showPerkPoint=true;
	
	public int maxItemPerRow=3;
	public bool iconOnly=false;
	
	public PerkGroup[] perkGroups;
	
	public int spaceX=10;
	public int spaceY=10;
	
	public int groupSpacing=15;
	
	private int scrollAreaLength=300;
	
	public static bool IsWindowOn(){
		return uiPerk.showWindow;
	}
	
	// Use this for initialization
	void Start () {
		uiPerk=this;
		
		StartCoroutine(DelayStart());
	}
	
	IEnumerator DelayStart(){
		yield return null;
		
		if(FindObjectOfType(typeof(PerkManager))==null || PerkManager.GetAllPerks().Count==0){
			this.enabled=false;
			yield break;
		}
		
		if(camControl==null) camControl=FindObjectOfType(typeof(CameraControl)) as CameraControl;
		
		
		int count=0;
		List<Perk> list=PerkManager.GetAllPerks();
		int perkRemaining=0;
		
		foreach(Perk perk in list){
			if(perk.enableInlvl) perkRemaining+=1;
		}
		
		if(perkGroups!=null && perkGroups.Length>0){
			while(count<perkGroups.Length){
				perkRemaining-=Mathf.Min(perkGroups[count].itemCount, maxItemPerRow);
				count+=1;
			}
		}
		int estimateLength=10+(50+spaceY)*(1+(((perkRemaining-1)/maxItemPerRow)+count))+perkGroups.Length*groupSpacing;
		scrollAreaLength=Mathf.Max(310, estimateLength);
		
		selectedID=PerkManager.GetFirstAvailabelID();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void OnGUI(){
		
		
		//~ if(PerkManager.GetAllPerks()==null) return;
		
		//~ GUI.skin=skin;
		//~ Perk perk=PerkManager.GetAllPerks()[0];
		//~ GUI.Box(new Rect(5, 200, 50, 50), "");
		//~ GUI.Label(new Rect(5, 200, 50, 50), perk.icon);
		//~ GUI.skin=null;
		
		//~ Perk perk=PerkManager.GetAllPerks()[0];
		//~ //GUI.Box(new Rect(5, 200, 160, 50), "");
		//~ GUI.Button(new Rect(5, 200, 160, 50), "");
		//~ GUI.Label(new Rect(10, 200, 50, 50), perk.icon);
		//~ GUIStyle style=new GUIStyle();
		//~ //style.fontStyle=FontStyle.Bold;
		//~ //style.fontSize=16;
		//~ style.normal.textColor=new Color(1, 1, 1, 1f);
		//~ style.alignment=TextAnchor.MiddleCenter;
		//~ style.wordWrap=true;
		//~ GUI.Label(new Rect(60, 200, 100, 50), perk.name, style);
		
		for(int i=0; i<2; i++) GUI.Box(new Rect(Screen.width-83, Screen.height-88, 76, 56), "");
		if(GUI.Button(new Rect(Screen.width-80, Screen.height-85, 70, 50), "Perk\nMenu")){
			showWindow=!showWindow;
			
			if(camControl!=null){
				if(showWindow) camControl.enabled=false;
				else camControl.enabled=true;
			}
			
			if(onPerkWindowE!=null) onPerkWindowE(showWindow);
		}
	
		if(showWindow && !UI.IsPaused()) PerkWindow();
	}
	
	//~ public Texture2D tex;
	private Vector2 scrollPosition;
	void PerkWindow(){
		GUI.depth = 0;
		
		int winWidth=575;
		int winHeight=470;
		
		int startX=Screen.width/2-winWidth/2;
		int startY=Screen.height/2-winHeight/2;
		//~ int width=120;
		//~ int height=50;
		
		int row=0;
		int column=0;
		
		GUI.BeginGroup(new Rect(startX, startY, winWidth+50, winHeight+50));
		for(int i=0; i<2; i++) GUI.Box(new Rect(0, 0, winWidth, winHeight), "");
		
		GUIStyle styleT=new GUIStyle();
		styleT.padding=new RectOffset (0, 0, 0, 0);
		styleT.normal.textColor=new Color(0, 1, 1, 1f);
		styleT.alignment=TextAnchor.MiddleCenter;
		styleT.fontStyle=FontStyle.Bold;
		styleT.fontSize=18;
		GUI.Label(new Rect(winWidth/2-70, 10, 140, 25), "Perk Menu", styleT);
		
		if(showPerkPoint){
			styleT.alignment=TextAnchor.MiddleRight;
			styleT.fontStyle=FontStyle.Normal;
			styleT.fontSize=14;
			GUI.Label(new Rect(winWidth-130, 20, 120, 25), "Perk point: "+PerkManager.perkPoint, styleT);
		}
		
		//~ for(int i=0; i<2; i++) GUI.Box(new Rect(winWidth/2-53, winHeight+5, 106, 31), "");
		//~ if(GUI.Button(new Rect(winWidth+10, 10, 25, 25), "X")){
			//~ showWindow=false;
		//~ }
		//~ if(GUI.Button(new Rect(winWidth/2-50, winHeight+8, 100, 25), "Close")){
		if(GUI.Button(new Rect(10, 10, 40, 25), "X ")){
			showWindow=false;
			if(onPerkWindowE!=null) onPerkWindowE(showWindow);
			if(camControl!=null) camControl.enabled=true;
		}
		
		
		for(int i=0; i<4; i++) GUI.Box(new Rect(10, 50, winWidth-20, 310), "");
		scrollPosition = GUI.BeginScrollView (new Rect (10,50,winWidth-20,310), scrollPosition, new Rect(0, 0, winWidth-50, scrollAreaLength));
		
		//~ GUI.Box(new Rect(5+column*60, 5+row*60, 60, 60), "");
		
		
		GUIStyle style=new GUIStyle();
		style.padding=new RectOffset (0, 0, 0, 0);
		style.normal.textColor=new Color(1, 1, 1, 1f);
		style.alignment=TextAnchor.MiddleCenter;
		
		int groupCount=0;
		int currentGroupCount=0;
		int tierOffset=0;
		for(int ID=0; ID<PerkManager.GetAllPerks().Count; ID++){
			Perk perk=PerkManager.GetAllPerks()[ID];
			int avai=perk.IsAvailable();
			
			if(perk.enableInlvl){
				
				if(perk.unlocked) style.normal.textColor=new Color(0, 1, 0, 1f);
				else if(avai==0) style.normal.textColor=new Color(1, 1, 1, 1f);
				else style.normal.textColor=new Color(1, 0, 0, 1f);
				
				if(selectedID==ID){
					style.fontStyle=FontStyle.Bold;
					style.fontSize=14;
					if(avai==0) style.normal.textColor=new Color(0, 1, 1, 1f);
				}
				
				bool flag=false;
				
				if(iconOnly){
					if(!perk.unlocked){
						flag=GUI.Button(new Rect(10+column*(50+spaceX), 10+tierOffset+row*(50+spaceY), 50, 50), "");
					}
					else{
						GUI.Box(new Rect(10+column*(50+spaceX), 10+tierOffset+row*(50+spaceY), 50, 50), "");
						flag=GUI.Button(new Rect(10+column*60, 10+tierOffset+row*(50+spaceY), 50, 50), "", style);
					}
					
					if(perk.icon==null) GUI.Box(new Rect(14+column*(50+spaceX), 14+tierOffset+row*(50+spaceY), 42, 42), "");
					else{
						Texture icon=null;
						if(perk.unlocked) icon=perk.iconUnlocked;
						else if(avai==0) icon=perk.icon;
						else icon=perk.iconUnavailable;
							
						GUI.Label(new Rect(14+column*(50+spaceX), 14+tierOffset+row*(50+spaceY), 42, 42), icon, style);
					}
				}
				else{
					if(!perk.unlocked){
						flag=GUI.Button(new Rect(10+column*(160+spaceX), 10+tierOffset+row*(50+spaceY), 160, 50), "");
					}
					else{
						GUI.Box(new Rect(10+column*(160+spaceX), 10+tierOffset+row*(50+spaceY), 160, 50), "");
						flag=GUI.Button(new Rect(10+column*(160+spaceX), 10+tierOffset+row*(50+spaceY), 160, 50), "", style);
					}
					
					if(perk.icon==null) GUI.Box(new Rect(15+column*(160+spaceX), 14+tierOffset+row*(50+spaceY), 42, 42), "");
					else{
						Texture icon=null;
						if(perk.unlocked) icon=perk.iconUnlocked;
						else if(avai==0) icon=perk.icon;
						else icon=perk.iconUnavailable;
							
						GUI.Label(new Rect(15+column*(160+spaceX), 14+tierOffset+row*(50+spaceY), 42, 42), icon, style);
					}
					
					GUI.Label(new Rect(60+column*(160+spaceX), 10+tierOffset+row*(50+spaceY), 100, 50), perk.name, style);
				}	
				
				if(flag){
					selectedID=ID;
				}
				
				style.fontSize=12;
				if(selectedID==ID){
					style.fontStyle=FontStyle.Normal;
				}
				if(avai==0) style.normal.textColor=new Color(1, 1, 1, 1f);
				else style.normal.textColor=new Color(1, 1, 1, 1f);
				
				
				if(perkGroups!=null && perkGroups.Length>0 && perkGroups.Length>groupCount){
					currentGroupCount+=1;
					if(currentGroupCount>=perkGroups[groupCount].itemCount){
						GUIStyle style1=new GUIStyle();
						style1.normal.textColor=new Color(1, 1, 0, 1f);
						style1.fontStyle=FontStyle.Bold;
						GUI.Label(new Rect(10, 60+tierOffset+row*(50+spaceY), 500, 20), perkGroups[groupCount].label, style1);
						
						groupCount+=1;
						currentGroupCount=0;
						column=maxItemPerRow-1;
						tierOffset+=groupSpacing;
					}
				}
				
				
				column+=1;
				if(column==maxItemPerRow){
					column=0;
					row+=1;
				}
			}
		}
		
		
		GUI.EndScrollView();
		
		
		int panelWidth=winWidth-20;
		int panelHeight=90;
		
		GUI.BeginGroup(new Rect(10, winHeight-100, panelWidth, panelHeight));
		for(int i=0; i<4; i++) GUI.Box(new Rect(0, 0, panelWidth, panelHeight), "");
		
		Perk selectedPerk=PerkManager.GetAllPerks()[selectedID];
		int perkAvai=selectedPerk.IsAvailable();
		
		
		
		style=new GUIStyle();
		style.normal.textColor=new Color(1, 1, 0, 1f);
		style.fontStyle=FontStyle.Bold;
		style.fontSize=14;
		string status="";
		if(perkAvai>0){
			status=" (Unavailable)";
			style.normal.textColor=new Color(1, 0, 0, 1f);
		}
		if(selectedPerk.unlocked){
			status=" (Unlocked)";
			style.normal.textColor=new Color(0, 1, 0, 1f);
		}
		GUI.Label(new Rect(5, 5, 500, 30), selectedPerk.name+status, style);
		GUI.Label(new Rect(5, 25, 500, 30), selectedPerk.desp);
		
		style=new GUIStyle();
		style.normal.textColor=new Color(1, 0, 0, 1f);
		float subStartX=5; float subStartY=panelHeight-25; float widthMin=0f; float widthMax=0f;
		
		if(perkAvai==0){
			if(selectedPerk.RequireResource()){
				Resource[] resourceList=GameControl.GetResourceList();
				
				GUI.Label(new Rect(subStartX, subStartY+2, 70f, 25f), "Cost:");
				subStartX+=50;
				
				//display all the resource
				for(int i=0; i<Mathf.Min(selectedPerk.costs.Length, resourceList.Length); i++){
					if(selectedPerk.costs[i]>0){
						//if an icon has been assigned to that particular resource type
						if(resourceList[i].icon!=null){
							GUI.Label(new Rect(subStartX, subStartY, 20f, 25f), resourceList[i].icon);
							subStartX+=20;
							GUI.Label(new Rect(subStartX, subStartY+2, 40, 25), selectedPerk.costs[i].ToString());
							subStartX+=40;
						}
						//if not icon, just show the text
						else {
							GUIContent labelRsc=new GUIContent(selectedPerk.costs[i].ToString()+resourceList[i].name);
							GUI.skin.GetStyle("Label").CalcMinMaxWidth(labelRsc, out widthMin, out widthMax);
							GUI.Label(new Rect(subStartX, subStartY+2, widthMax, 25), labelRsc);
							subStartX+=widthMax+20;
						}
					}
				}
			}
		}
		//pre-req perk not unlocked
		else if(perkAvai==1){
			GUI.Label(new Rect(subStartX, subStartY+2, 70f, 25f), "Require: ", style);
			subStartX+=60;
			List<int> list=selectedPerk.prereq;
			foreach(int ID in list){
				
				GUIContent label=new GUIContent(PerkManager.GetAllPerks()[ID].name.ToString());
				GUI.skin.GetStyle("Label").CalcMinMaxWidth(label, out widthMin, out widthMax);
				GUI.Label(new Rect(subStartX, subStartY+2, widthMax+5, 25), label, style);
				subStartX+=widthMax+10;
			}
		}
		//min wave req not reach
		else if(perkAvai==2){
			GUI.Label(new Rect(subStartX, subStartY+2, panelWidth, 25f), "Available at wave "+selectedPerk.waveMin, style);
		}
		//min perk point not reach
		else if(perkAvai==3){
			GUI.Label(new Rect(subStartX, subStartY+2, panelWidth, 25f), "Require "+selectedPerk.perkMin+" perk points", style);
		}
		
		style=new GUIStyle();
		style.alignment=TextAnchor.MiddleCenter;
		style.fontStyle=FontStyle.Bold;
		
		
		if(perkAvai==0){
			if(!selectedPerk.unlocked){
				if(GUI.Button(new Rect(panelWidth-105, panelHeight-35, 100, 30), "Unlock")){
					PerkManager.Unlock(selectedID);
				}
			}
			//~ else{
				//~ GUI.Box(new Rect(panelWidth-105, panelHeight-30, 100, 25), "");
				//~ style.normal.textColor=new Color(0, 1, 0, 1f);
				//~ GUI.Label(new Rect(panelWidth-105, panelHeight-30, 100, 25), "Unlocked", style);
			//~ }
		}
		//~ else{
			//~ GUI.Box(new Rect(panelWidth-105, panelHeight-30, 100, 25), "");
			//~ style.normal.textColor=new Color(1, 0, 0, 1f);
			//~ GUI.Label(new Rect(panelWidth-105, panelHeight-30, 100, 25), "Unavailable", style);
		//~ }
		
		GUI.EndGroup();
		
		GUI.EndGroup();
		//~ GUI.Box(new Rect(10, 200, 150,150), PerkManager.GetAllPerks()[0].icon);
	}
}


[System.Serializable]
public class PerkGroup{
	public string label;
	public int itemCount;
}