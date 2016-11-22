using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public class PerkWindow{
	
}

[System.Serializable]
public class PerkListPanel{
	public GameObject perkItemTemplate;
	[HideInInspector] public PerkItem[] itemLists;
	public Transform selectionIndicator; 
	public UIDraggablePanel draggablePanel;
}

[System.Serializable]
public class PerkInfoPanel{
	public UILabel name;
	public UILabel desp;
	public UILabel misc;
	
	public GameObject resourceItem;
	[HideInInspector] public ResourceItem[] rscList;
	
	public GameObject unlockButton;
}


public class UIPerknGUI : MonoBehaviour {

	public delegate void PerkWindowHandler(bool flag); 
	public static event PerkWindowHandler onPerkWindowE;
	
	
	public Transform perkMenu;
	
	private bool isMenuOn=false;
	
	public CameraControl camControl;
	
	private int currentPerk=0;
	
	public bool showPerkPoint=true;
	public UILabel labelPerkPoint;
	public bool useIconOnly;
	public int maxItemPerRow=9;
	public PerkGroup[] perkGroups;
	
	public int spaceX=50;
	public int spaceY=50;
	
	public int groupSpacing=30;
	
	public PerkListPanel perkList;
	public PerkInfoPanel perkInfo;
	
	
	public static UIPerknGUI perkNGUI;
	
	public static bool IsMenuOn(){
		return perkNGUI.isMenuOn;
	}
	
	
	// Use this for initialization
	void Start () {
		perkNGUI=this;
		
		if(perkMenu) perkMenu.localPosition=new Vector3(5000, 0, 0);
		
		StartCoroutine(DelayStart());
		
		if(camControl==null) camControl=FindObjectOfType(typeof(CameraControl)) as CameraControl;
	}
	
	void OnEnable(){
		UInGUI.onPauseE += OnUIPause;
		
		//~ PerkManager.onUndatePerkPointE += OnUpdatePerkPoint;
	}
	
	void OnDisable(){
		UInGUI.onPauseE -= OnUIPause;
		
		//~ PerkManager.onUndatePerkPointE -= OnUpdatePerkPoint;
	}
	
	private bool pause=false;
	void OnUIPause(bool flag){
		if(isMenuOn){
			if(flag) perkMenu.localPosition=new Vector3(5000, 0, 0);
			else perkMenu.localPosition=Vector3.zero;
		}
		pause=flag;
	}
	
	IEnumerator DelayStart(){
		yield return null;
		
		if(FindObjectOfType(typeof(PerkManager))==null || PerkManager.GetAllPerks().Count==0){
			this.enabled=false;
			yield break;
		}
		
		InitResourceItem();
		
		if(useIconOnly) InitPerkItemIconOnly();
		else InitPerkItem();
		
		if(showPerkPoint) OnUpdatePerkPoint();
		else labelPerkPoint.text="";
	}
	
	void OnPerkMenu(){
		if(pause) return;
		
		if(!isMenuOn){
			perkMenu.localPosition=new Vector3(0, -30, 50);
		}
		else{
			perkMenu.localPosition=new Vector3(5000, 0, 0);
		}
		isMenuOn=!isMenuOn;
		
		if(onPerkWindowE!=null) onPerkWindowE(isMenuOn);
		
		if(camControl!=null){
			if(isMenuOn) camControl.enabled=false;
			else camControl.enabled=true;
		}
	}
	
	
	
	void OnClosePerkMenu(){
		perkMenu.localPosition=new Vector3(5000, 0, 0);
		isMenuOn=false;
		if(camControl!=null) camControl.enabled=true;
	}
	
	
	void OnUpdatePerkPoint(){
		if(showPerkPoint) labelPerkPoint.text="PerkPoint: "+PerkManager.perkPoint;
	}
	
	
	void OnPerkItem(GameObject obj){
		for(int i=0; i<perkList.itemLists.Length; i++){
			if(perkList.itemLists[i]!=null && perkList.itemLists[i].rootObj==obj){
				if(i!=currentPerk){
					Perk prePerk=PerkManager.GetAllPerks()[currentPerk];
					if(prePerk.unlocked){
						SetItemToUnlocked(perkList.itemLists[currentPerk], prePerk);
					}
					else if(prePerk.IsAvailable()==0){
						SetItemToAvailable(perkList.itemLists[currentPerk], prePerk);
					}
					else if(prePerk.IsAvailable()>0){
						SetItemToUnavailable(perkList.itemLists[currentPerk], prePerk);
					}
					
					currentPerk=i;
					
					prePerk=PerkManager.GetAllPerks()[currentPerk];
					PerkItem item=perkList.itemLists[currentPerk];
					if(prePerk.unlocked){
						item.spriteBut.spriteName="Highlight";
						item.spriteBut.color=new Color(.65f, 1f, .7f, 1);
						if(item.spriteIcon!=null) item.spriteIcon.color=Color.white;
						if(item.label!=null) item.label.color=Color.green;
						Utility.SetActive(perkInfo.unlockButton, false);
					}
					else if(prePerk.IsAvailable()==0){
						item.spriteBut.spriteName="Dark";
						item.spriteBut.color=new Color(0, .9f, .9f, 1);
						if(item.spriteIcon!=null) item.spriteIcon.color=Color.white;
						if(item.label!=null) item.label.color=Color.white;
						Utility.SetActive(perkInfo.unlockButton, true);
					}
					else if(prePerk.IsAvailable()>0){
						item.spriteBut.spriteName="Highlight";
						item.spriteBut.color=new Color(0, .4f, .4f, 1);
						if(item.spriteIcon!=null) item.spriteIcon.color=Color.gray;
						if(item.label!=null) item.label.color=Color.red;
						Utility.SetActive(perkInfo.unlockButton, false);
					}
					
					
					perkList.selectionIndicator.localPosition=item.rootT.transform.localPosition;
					//~ Debug.Log(selectIndicator.localPosition+"  "+perkItemLists[currentPerk].rootT.transform.position);
					
					UpdatePerkInfo();
					
					break;
				}
			}
		}
	}
	
	void OnPerkUnlock(){
		Perk perk=PerkManager.GetAllPerks()[currentPerk];
		if(!perk.unlocked){
			if(PerkManager.Unlock(currentPerk)){
				PerkItem item=perkList.itemLists[currentPerk];
				
				item.spriteBut.spriteName="Highlight";
				item.spriteBut.color=new Color(0, .9f, .9f, 1);
				item.spriteIcon.color=Color.white;
				item.label.color=Color.green;
				
				if(perk.unlocked){
					Utility.SetActive(perkInfo.unlockButton, false);
				
					if(perk.iconUnlocked!=null && item.spriteIcon!=null){
						if(item.spriteIcon.atlas.GetSprite(perk.iconUnlocked.name)!=null){
							item.spriteIcon.spriteName=perk.iconUnlocked.name;
						}
					}
				}
				
				UpdatePerkInfo();
				CheckDependency();
				
				OnUpdatePerkPoint();
				
				//~ if(onPerkUnlockedE!=null) onPerkUnlockedE(perk.name);
			}
		}
		
		
	}
	
	void CheckDependency(){
		List<Perk> list=PerkManager.GetAllPerks();
		for(int i=0; i<list.Count; i++){
			if(perkList.itemLists[i]!=null && perkList.itemLists[i].itemState==_ItemState.Unavailable){
				if(list[i].IsAvailable()==0 && !list[i].unlocked){
					SetItemToAvailable(perkList.itemLists[i], list[i]);
				}
			}
		}
	}
	
	void SetItemToAvailable(PerkItem item, Perk perk){
		item.spriteBut.spriteName="Button";
		item.spriteBut.color=Color.white;
		//~ if(item.spriteIcon!=null) item.spriteIcon.color=Color.white;
		if(item.label!=null) item.label.color=Color.white;
		item.itemState=_ItemState.Available;
		
		if(perk.icon!=null && item.spriteIcon!=null){
			if(item.spriteIcon.atlas.GetSprite(perk.icon.name)!=null){
				item.spriteIcon.spriteName=perk.icon.name;
			}
		}
	}
	
	void SetItemToUnavailable(PerkItem item, Perk perk){
		item.spriteBut.spriteName="Highlight";
		item.spriteBut.color=Color.gray;
		//~ if(item.spriteIcon!=null) item.spriteIcon.color=Color.gray;
		if(item.label!=null) item.label.color=Color.red;
		item.itemState=_ItemState.Unavailable;
		
		if(perk.iconUnavailable!=null && item.spriteIcon!=null){
			if(item.spriteIcon.atlas.GetSprite(perk.iconUnavailable.name)!=null){
				item.spriteIcon.spriteName=perk.iconUnavailable.name;
			}
		}
	}
	
	void SetItemToUnlocked(PerkItem item, Perk perk){
		item.spriteBut.spriteName="Highlight";
		item.spriteBut.color=new Color(.65f, 1f, .7f, 1);
		//~ if(item.spriteIcon!=null) item.spriteIcon.color=Color.white;
		if(item.label!=null) item.label.color=Color.green;
		item.itemState=_ItemState.Unlocked;
		
		 if(perk.iconUnlocked!=null && item.spriteIcon!=null){
			if(item.spriteIcon.atlas.GetSprite(perk.iconUnlocked.name)!=null){
				item.spriteIcon.spriteName=perk.iconUnlocked.name;
			}
		}
	}
	

	
	void UpdatePerkInfo(){
		Perk perk=PerkManager.GetAllPerks()[currentPerk];
		int perkAvai=perk.IsAvailable();
		
		string status="";
		perkInfo.name.color=new Color(1f, 230f/255f, 0, 1);
		if(perkAvai>0){
			status=" (Unavailable)";
			perkInfo.name.color=Color.red;
		}
		if(perk.unlocked){
			status=" (Unlocked)";
			perkInfo.name.color=Color.green;
		}
		
		perkInfo.name.text=perk.name+status;
		perkInfo.desp.text=perk.desp;
		
		perkInfo.misc.text="";
		
		if(perk.unlocked){
			HideResourceItem();
		}
		else if(perkAvai==0){
			int[] costs=perk.costs;
			int activeCount=0;
			for(int i=0; i<costs.Length; i++){
				if(costs[i]>0){
					Vector3 pos=perkInfo.rscList[0].rootT.localPosition+new Vector3(activeCount*55, 0, 0);
					perkInfo.rscList[i].rootT.localPosition=pos;
					perkInfo.rscList[i].label.text=costs[i].ToString();
					activeCount+=1;
					
					Utility.SetActive(perkInfo.rscList[i].rootObj, true);
				}
				else{
					Utility.SetActive(perkInfo.rscList[i].rootObj, false);
				}
			}
			
			if(activeCount>0) perkInfo.misc.text="Require:";
		}
		//pre-req perk not unlocked
		else if(perkAvai==1){
			string text="Require: ";
			
			List<int> list=perk.prereq;
			int i=0;
			foreach(int ID in list){
				if(i>0) text+=", ";
				text+=PerkManager.GetAllPerks()[ID].name;
				i+=1;
			}
			
			perkInfo.misc.text=text;
			HideResourceItem();
		}
		//min wave req not reach
		else if(perkAvai==2){
			perkInfo.misc.text="Available at wave "+perk.waveMin;
			HideResourceItem();
		}
		//min perk point not reach
		else if(perkAvai==3){
			perkInfo.misc.text="Require "+perk.perkMin+" perk points";
			HideResourceItem();
		}
	}
	
	void HideResourceItem(){
		for(int i=0; i<perkInfo.rscList.Length; i++){
			Utility.SetActive(perkInfo.rscList[i].rootObj, false);
		}
	}
	
	void InitPerkItemIconOnly(){
		GameObject obj=perkList.perkItemTemplate;
		
		//~ Transform rootT=obj.transform;
		UILabel label=obj.GetComponentInChildren<UILabel>();
		Destroy(label.gameObject);
		
		UISprite[] sprites=obj.GetComponentsInChildren<UISprite>();
		
		Transform iconT=null;
		Transform bgT=null;
		
		foreach(UISprite sprite in sprites){
			if(sprite.gameObject.name=="Icon"){
				iconT=sprite.transform;
				sprite.transform.localPosition=Vector3.zero;
			}
			else if(sprite.gameObject.name=="BG"){
				bgT=sprite.transform;
				sprite.transform.localPosition=Vector3.zero;
			}
		}
		
		if(iconT!=null && bgT!=null)
		bgT.localScale=new Vector3(iconT.localScale.x+10, iconT.localScale.x+10, iconT.localScale.x+10);
		
		obj.GetComponent<BoxCollider>().size=new Vector3(bgT.localScale.x+10, bgT.localScale.y+10, 1);
		obj.transform.localPosition+=new Vector3(-50, 0, 0);
		
		perkList.selectionIndicator.localPosition=obj.transform.localPosition;
		TweenScale tweenS=perkList.selectionIndicator.gameObject.GetComponent<TweenScale>();
		tweenS.from=new Vector3(bgT.localScale.x+2, bgT.localScale.y+2, 1);
		tweenS.to=new Vector3(bgT.localScale.x+8, bgT.localScale.y+8, 1);
		
		
		List<Perk> list=PerkManager.GetAllPerks();
		perkList.itemLists=new PerkItem[list.Count];
		
		int column=0;
		int row=0;
		
		int groupCount=0;
		int currentGroupCount=0;
		int groupOffset=0;
		
		for(int i=0; i<list.Count; i++){
			Perk perk=list[i];
			if(perk.enableInlvl){
				PerkItem item=ClonePerkItem(perkList.perkItemTemplate, i);
				item.label.text=perk.name;
				if(perk.icon!=null){
					if(item.spriteIcon.atlas.GetSprite(perk.icon.name)!=null){
						item.spriteIcon.spriteName=perk.icon.name;
					}
				}
				
				
				if(perk.unlocked) SetItemToUnlocked(item, perk);
				else if(perk.IsAvailable()!=0) SetItemToUnavailable(item, perk);
				else SetItemToAvailable(item, perk);
				
				item.rootT.localPosition+=new Vector3(column*(50+spaceX), -row*(50+spaceY)-groupOffset, 0);
				perkList.itemLists[i]=item;
				
				if(perkGroups!=null && perkGroups.Length>0 && perkGroups.Length>groupCount){
					currentGroupCount+=1;
					if(currentGroupCount>=perkGroups[groupCount].itemCount){
						GameObject sourceObj=perkList.perkItemTemplate.GetComponentInChildren<UILabel>().gameObject;
						GameObject labelObj=Instantiate(sourceObj) as GameObject;
						labelObj.name="GroupLabel "+(groupCount+1);
						labelObj.transform.parent=perkList.perkItemTemplate.transform.parent;
						labelObj.transform.localScale=new Vector3(20, 20, 1);
						labelObj.transform.localPosition=perkList.perkItemTemplate.transform.localPosition;
						labelObj.transform.localPosition+=new Vector3(-20, -row*(50+spaceY)-groupOffset-28, 0);
						
						UILabel groupLabel=labelObj.GetComponent<UILabel>();
						groupLabel.text=perkGroups[groupCount].label;
						groupLabel.pivot=UIWidget.Pivot.TopLeft;
						groupLabel.lineWidth=500;
						groupLabel.enabled=true;
						
						groupCount+=1;
						currentGroupCount=0;
						column=maxItemPerRow-1;
						groupOffset+=groupSpacing;
					}
				}
				
				column+=1;
				if(column==maxItemPerRow){
					column=0;
					row+=1;
				}
			}
		}
		
		perkList.itemLists[0].spriteBut.spriteName="Dark";
		perkList.itemLists[0].spriteBut.color=new Color(0, .9f, .9f, 1);
		
		//~ panel.MoveAbsolute(new Vector3(0, -5, 0));
		perkList.draggablePanel.ResetPosition();
		StartCoroutine(Reset());
		Utility.SetActive(perkList.perkItemTemplate, false);
		
		UpdatePerkInfo();
	}
	
	IEnumerator Reset(){
		yield return null;
		perkList.draggablePanel.repositionClipping=true;
	}
	
	void InitPerkItem(){
		//Debug.Log("Init Perk Item");
		
		List<Perk> list=PerkManager.GetAllPerks();
		//~ List<Perk> list=new List<perk>();
		
		//~ int activePerkCount=0;
		//~ foreach(Perk perk in fullList){
			//~ if(perk.enableInlvl) list.AddPerk//activePerkCount+=1;
		//~ }
		//~ perkList.itemLists=new PerkItem[activePerkCount];
		perkList.itemLists=new PerkItem[list.Count];
		
		int column=0;
		int row=0;
		
		int groupCount=0;
		int currentGroupCount=0;
		int groupOffset=0;
		
		for(int i=0; i<list.Count; i++){
			Perk perk=list[i];
			if(perk.enableInlvl){
				PerkItem item=ClonePerkItem(perkList.perkItemTemplate, i);
				item.label.text=perk.name;
				if(perk.icon!=null){
					if(item.spriteIcon.atlas.GetSprite(perk.icon.name)!=null){
						item.spriteIcon.spriteName=perk.icon.name;
					}
				}
				
				if(perk.unlocked) SetItemToUnlocked(item, perk);
				else if(perk.IsAvailable()!=0) SetItemToUnavailable(item, perk);
				else SetItemToAvailable(item, perk);
				
				//~ item.rootT.localPosition+=new Vector3(column*190, -row*60, 0);
				item.rootT.localPosition+=new Vector3(column*(190), -row*60-groupOffset, 0);
				perkList.itemLists[i]=item;
				
				if(perkGroups!=null && perkGroups.Length>0 && perkGroups.Length>groupCount){
					currentGroupCount+=1;
					if(currentGroupCount>=perkGroups[groupCount].itemCount){
						GameObject sourceObj=perkList.perkItemTemplate.GetComponentInChildren<UILabel>().gameObject;
						GameObject labelObj=Instantiate(sourceObj) as GameObject;
						labelObj.name="GroupLabel "+(groupCount+1);
						labelObj.transform.parent=perkList.perkItemTemplate.transform.parent;
						labelObj.transform.localScale=new Vector3(20, 20, 1);
						labelObj.transform.localPosition=perkList.perkItemTemplate.transform.localPosition;
						labelObj.transform.localPosition+=new Vector3(-80, -row*(60)-groupOffset-28, 0);
						
						UILabel groupLabel=labelObj.GetComponent<UILabel>();
						groupLabel.text=perkGroups[groupCount].label;
						groupLabel.pivot=UIWidget.Pivot.TopLeft;
						groupLabel.lineWidth=500;
						groupLabel.enabled=true;
						
						groupCount+=1;
						currentGroupCount=0;
						column=maxItemPerRow-1;
						groupOffset+=groupSpacing;
					}
				}
						
				column+=1;
				if(column==maxItemPerRow){
					column=0;
					row+=1;
				}
			}
		}
		
		currentPerk=PerkManager.GetFirstAvailabelID();
		perkList.itemLists[currentPerk].spriteBut.spriteName="Dark";
		perkList.itemLists[currentPerk].spriteBut.color=new Color(0, .9f, .9f, 1);
		if(list[currentPerk].IsAvailable()>0 || list[currentPerk].unlocked) Utility.SetActive(perkInfo.unlockButton, false);
		
		//~ panel.MoveAbsolute(new Vector3(0, -5, 0));
		perkList.draggablePanel.ResetPosition();
		Utility.SetActive(perkList.perkItemTemplate, false);
		
		UpdatePerkInfo();
	}
	
	PerkItem ClonePerkItem(GameObject source, int ID){
		//~ GameObject obj=Instantiate(perkButtonTemplate);
		PerkItem item=new PerkItem();
		GameObject obj=(GameObject)Instantiate(source);
		obj.name+=ID.ToString();
		item.rootObj=obj;
		item.rootT=item.rootObj.transform;
		item.rootT.parent=source.transform.parent;
		item.rootT.localPosition=source.transform.localPosition;
		item.rootT.localScale=source.transform.localScale;
		item.label=item.rootObj.GetComponentInChildren<UILabel>();
		UISprite[] sprites=item.rootObj.GetComponentsInChildren<UISprite>();
		foreach(UISprite sprite in sprites){
			if(sprite.gameObject.name=="Icon"){
				item.spriteIcon=sprite;
			}
			else if(sprite.gameObject.name=="BG"){
				item.spriteBut=sprite;
			}
		}
		return item;
	}
	
	PerkItem ClonePerkItemIconOnly(GameObject source, int ID){
		//~ GameObject obj=Instantiate(perkButtonTemplate);
		PerkItem item=new PerkItem();
		GameObject obj=(GameObject)Instantiate(source);
		obj.name+=ID.ToString();
		item.rootObj=obj;
		item.rootT=item.rootObj.transform;
		item.rootT.parent=source.transform.parent;
		item.rootT.localPosition=source.transform.localPosition;
		item.rootT.localScale=source.transform.localScale;
		//~ item.label=item.rootObj.GetComponentInChildren<UILabel>();
		UISprite[] sprites=item.rootObj.GetComponentsInChildren<UISprite>();
		foreach(UISprite sprite in sprites){
			if(sprite.gameObject.name=="Icon"){
				item.spriteIcon=sprite;
			}
			else if(sprite.gameObject.name=="BG"){
				item.spriteBut=sprite;
			}
		}
		
		return item;
	}
	
	void InitResourceItem(){
		Resource[] resourceList=GameControl.GetResourceList();
		
		perkInfo.rscList=new ResourceItem[resourceList.Length];
		
		for(int i=0; i<resourceList.Length; i++){
			perkInfo.rscList[i]=CloneResourceItem(perkInfo.resourceItem, resourceList[i]);
			perkInfo.rscList[i].rootT.localPosition+=new Vector3(i*80, 0, 0);
		}
		
		Destroy(perkInfo.resourceItem);
	}
	
	ResourceItem CloneResourceItem(GameObject source, Resource rsc){
		ResourceItem item=new ResourceItem();
		GameObject obj=(GameObject)Instantiate(source);
		item.rootObj=obj;
		item.rootT=item.rootObj.transform;
		item.rootT.parent=source.transform.parent;
		item.rootT.localPosition=source.transform.localPosition;
		item.rootT.localScale=source.transform.localScale;
		item.label=obj.GetComponentInChildren<UILabel>();
		item.icon=obj.GetComponentInChildren<UISprite>();
		
		if(rsc.icon!=null){
			if(item.icon.atlas.GetSprite(rsc.icon.name)!=null){
				item.icon.spriteName=rsc.icon.name;
			}
			else item.icon.spriteName="Highlight";
		}
		
		return item;
	}
}

public enum _ItemState{Unavailable, Available, Unlocked};
[System.Serializable]
public class PerkItem{
	public GameObject rootObj;
	public Transform rootT;
	public UIButton button;
	public UILabel label;
	public UISprite spriteBut;
	public UISprite spriteIcon;
	//~ public UIButtonScale scale;
	
	
	[HideInInspector] public _ItemState itemState=_ItemState.Available;
	
}

[System.Serializable]
public class NGUIButton{
	public GameObject rootObj;
	public Transform rootT;
	public UIButton button;
	public UILabel label;
	public UISprite sprite;
	public UIButtonScale scale;
	
	public NGUIButton(GameObject obj){
		rootObj=obj;
		rootT=rootObj.transform;
		button=rootObj.GetComponent<UIButton>();
		scale=rootObj.GetComponent<UIButtonScale>();
		label=rootObj.GetComponentInChildren<UILabel>();
		sprite=rootObj.GetComponentInChildren<UISprite>();
	}
}