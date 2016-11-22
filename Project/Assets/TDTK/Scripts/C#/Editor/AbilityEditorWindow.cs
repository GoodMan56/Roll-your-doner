using UnityEngine;
using UnityEditor;

using System;
using System.Xml;
using System.IO;

using System.Collections;
using System.Collections.Generic;


public class AbilityEditorWindow : EditorWindow {
	
	public delegate void UpdateHandler(); 
	public static event UpdateHandler onAbilityUpdateE;
	
	static private AbilityEditorWindow window;

	private static List<Ability> allAbilityList=new List<Ability>();
	
	private static List<int> abilityIDList=new List<int>();
	
	private static List<Resource> rscList=new List<Resource>();
	
	
    public static void Init () {
        // Get existing open window or if none, make a new one:
        window = (AbilityEditorWindow)EditorWindow.GetWindow(typeof (AbilityEditorWindow));
		window.minSize=new Vector2(615, 750);
		window.maxSize=new Vector2(615, 751);
		
		InitLabel();
		
		allAbilityList=new List<Ability>();
		
		rscList=ResourceEditorWindow.Load();
		
		Load();
		
		
		//int i=0;
		//foreach(Ability ability in allAbilityList){
			//ability.ID=i;
			//i+=1;
			//Debug.Log(ability.ID);
		//}
		
		
		selectedAbilID=0;
    }
	
	public static AbilityListPrefab prefab;
	public static List<Ability> Load(){
		GameObject obj=Resources.Load("PrefabListAbility", typeof(GameObject)) as GameObject;
		if(obj==null) obj=CreatePrefab();
		
		prefab=obj.GetComponent<AbilityListPrefab>();
		if(prefab==null) prefab=obj.AddComponent<AbilityListPrefab>();
		
		allAbilityList=prefab.abilityList;
		
		//~ for(int i=0; i<allAbilityList.Count; i++){
			//~ allAbilityList[i]=allAbilityList[i].Clone();
		//~ }
		
		foreach(Ability ability in allAbilityList){
			abilityIDList.Add(ability.ID);
		}
		
		return allAbilityList;
	}
	
	public static GameObject CreatePrefab(){
		GameObject obj=new GameObject();
		obj.AddComponent<AbilityListPrefab>();
		GameObject prefab=PrefabUtility.CreatePrefab("Assets/TDTK/Resources/PrefabListAbility.prefab", obj, ReplacePrefabOptions.ConnectToPrefab);
		DestroyImmediate(obj);
		AssetDatabase.Refresh ();
		return prefab;
	}
	
	
	static void SwapAbilityInList(int ID1, int ID2){
		Ability temp=allAbilityList[ID1].Clone();
		allAbilityList[ID1]=allAbilityList[ID2].Clone();
		allAbilityList[ID2]=temp.Clone();
	}
	
	void UpdateResourceList(){
		rscList=ResourceEditorWindow.Load();
		Repaint();
	}
	
	
	int delete=-1;
	static int swapID=-1;
	static int selectedAbilID=0;
	private Vector2 scrollPos;
	
	int GenerateNewID(){
		int newID=0;
		while(abilityIDList.Contains(newID)) newID+=1;
		return newID;
	}
	
	void OnGUI () {
		if(window==null) Init();
		
		//if(GUI.Button(new Rect(window.position.width-110, 10, 100, 30), "Save")){
			//SaveToXML();
		//}
		
		GUI.color = Color.white;
		
		if(GUI.Button(new Rect(5, 10, 100, 30), "New Ability")){
			Ability abil=new Ability();
			abil.ID=GenerateNewID();
			abilityIDList.Add(abil.ID);
			abil.name="Ability "+allAbilityList.Count;
			allAbilityList.Add(abil);
			
			selectedAbilID=allAbilityList.Count-1;
			delete=-1;
		}
		if(selectedAbilID>=0 && selectedAbilID<allAbilityList.Count){
			if(GUI.Button(new Rect(115, 10, 100, 30), "Clone Ability")){
				Ability abil=allAbilityList[selectedAbilID].Clone();
				abil.ID=GenerateNewID();
				abilityIDList.Add(abil.ID);
				allAbilityList.Add(abil);
				
				selectedAbilID=allAbilityList.Count-1;
				delete=-1;
			}
		}
		
		if(prefab.resource==null) prefab.resource=new Resource();
		prefab.resource.icon=(Texture)EditorGUI.ObjectField(new Rect(window.position.width-160, 5, 40, 40), prefab.resource.icon, typeof(Texture), false);
		prefab.resource.name=EditorGUI.TextField(new Rect(window.position.width-115, 15, 100, 20), prefab.resource.name);
		
		
		GUI.Box(new Rect(5, 50, window.position.width-10, 185), "");
		scrollPos = GUI.BeginScrollView(new Rect(5, 55, window.position.width-12, 175), scrollPos, new Rect(5, 50, window.position.width-40, 10+((allAbilityList.Count-1)/3)*35));
		
		int row=0;
		int column=0;
		for(int i=0; i<allAbilityList.Count; i++){
			
			GUIStyle style=GUI.skin.label;
			style.alignment=TextAnchor.MiddleCenter;
			if(swapID==i) GUI.color=new Color(.9f, .9f, .0f, 1);
			else GUI.color=new Color(.8f, .8f, .8f, 1);
			GUI.Box(new Rect(10+column*210, 50+row*30, 25, 25), "");
			if(GUI.Button(new Rect(10+column*210, 50+row*30, 25, 25), "")){
				delete=-1;
				if(swapID==i) swapID=-1;
				else if(swapID==-1) swapID=i;
				else{
					SwapAbilityInList(swapID, i);
					swapID=-1;
				}
			}
			GUI.Label(new Rect(8+column*210, 50+row*30, 25, 25), i.ToString(), style);
			//GUI.Label(new Rect(8+column*210, 50+row*30, 25, 25), allAbilityList[i].ID.ToString(), style);
			GUI.color=Color.white;
			style.alignment=TextAnchor.MiddleLeft;
			
			if(selectedAbilID==i) GUI.color = Color.green;
			//if(allAbilityList[selectedAbilID].prereq.Contains(i)) GUI.color=new Color(0, 1f, 1f, 1f);
			style=GUI.skin.button;
			style.fontStyle=FontStyle.Bold;
			
			GUI.SetNextControlName ("AbilityButton");
			if(GUI.Button(new Rect(10+27+column*210, 50+row*30, 100, 25), allAbilityList[i].name, style)){
				GUI.FocusControl ("AbilityButton");
				selectedAbilID=i;
				delete=-1;
			}
			GUI.color = Color.white;
			style.fontStyle=FontStyle.Normal;
			
			if(delete!=i){
				if(GUI.Button(new Rect(10+27+102+column*210, 50+row*30, 25, 25), "X")){
					delete=i;
				}
			}
			else{
				GUI.color = Color.red;
				if(GUI.Button(new Rect(10+27+102+column*210, 50+row*30, 55, 25), "Delete")){
					//CheckDeletePrereq(i);
					abilityIDList.Remove(allAbilityList[i].ID);
					allAbilityList.RemoveAt(i);
					//InitAbilityUIDependency();
					if(i<=selectedAbilID) selectedAbilID-=1;
					delete=-1;
				}
				GUI.color = Color.white;
			}
			
			column+=1;
			if(column==3){
				column=0;
				row+=1;
			}
		}
		
		GUI.EndScrollView();
		
		if(selectedAbilID>-1 && selectedAbilID<allAbilityList.Count){
			AbilityConfigurator();
		}
		
		if (GUI.changed){
			prefab.abilityList=allAbilityList;
			EditorUtility.SetDirty(prefab);
			if(onAbilityUpdateE!=null) onAbilityUpdateE();
		}
		
		//Debug.Log("change!  "+Time.time);
		//~ Debug.Log(window.position);
		
		 //~ flags = EditorGUI.MaskField (new Rect (0, 0, 300, 20), "Player Flags", flags, abilityTypeLabel);
		//~ Debug.Log(flags);
	}
	
	
	GUIContent cont;
	
	//~ static private int abilityType=0;
	void AbilityConfigurator(){
		int startY=245;
		int startX=5;
		
		GUIStyle style=new GUIStyle();
		style.wordWrap=true;
		
		Ability ability=allAbilityList[selectedAbilID];
		
		startX=440;
		
		EditorGUI.LabelField(new Rect(startX, startY, 200, 20), "Default Icon: ");
		ability.icon=(Texture)EditorGUI.ObjectField(new Rect(startX, startY+17, 60, 60), ability.icon, typeof(Texture), false);
		startX+=80;
		
		EditorGUI.LabelField(new Rect(startX, startY, 200, 34), "Unavailable: ");
		ability.iconUnavailable=(Texture)EditorGUI.ObjectField(new Rect(startX+5, startY+17, 60, 60), ability.iconUnavailable, typeof(Texture), false);
		
		
//		startX+=100;
//		EditorGUI.LabelField(new Rect(startX, startY, 200, 34), "Description: ");
//		startY+=17;
//		EditorGUI.LabelField(new Rect(startX, startY, window.position.width-startX-20, 120), abilityDesp[(int)ability.type], style);
		
		startX=5;
		startY=245;
		
		EditorGUI.LabelField(new Rect(startX, startY, 200, 20), "Name: ");
		ability.name=EditorGUI.TextField(new Rect(startX+50, startY-1, 130, 17), ability.name);
		
		cont=new GUIContent("Max Use Count: ", "the maximum amount of instance the ability can be use in one level");
		EditorGUI.LabelField(new Rect(startX, startY+=20, 200, 20), cont);
		ability.maxUseCount=EditorGUI.IntField(new Rect(startX+130, startY-1, 50, 17), ability.maxUseCount);
		
		EditorGUI.LabelField(new Rect(startX, startY+=20, 200, 20), "cooldown:");
		ability.cdDuration=EditorGUI.FloatField(new Rect(startX+130, startY-1, 50, 17), ability.cdDuration);
		
		cont=new GUIContent("launch Delay: ", "time delay before the effects take place after the ability is activated. this is to allow the effect to sync up with visual effect");
		EditorGUI.LabelField(new Rect(startX, startY+=20, 200, 20), cont);
		ability.launchDelay=EditorGUI.FloatField(new Rect(startX+130, startY-1, 50, 17), ability.launchDelay);
		
		cont=new GUIContent("Indicator: ", "the indicator to use to select the activation position when the tower has been selected");
		EditorGUI.LabelField(new Rect(startX, startY+=20, 200, 20), cont);
		ability.indicator=(Transform)EditorGUI.ObjectField(new Rect(startX+70, startY-1, 110, 17), ability.indicator, typeof(Transform), false);
		
		cont=new GUIContent("AutoScaleIndicatorToRange: ", "check to automatically scale the indicator to match the range of effect");
		EditorGUI.LabelField(new Rect(startX, startY+=20, 200, 20), cont);
		ability.autoScaleIndicator=EditorGUI.Toggle(new Rect(startX+167, startY-1, 50, 17), ability.autoScaleIndicator);
		
		cont=new GUIContent("VisualEffect: ", "the prefab intend as visual effect to spawn everytime the ability is activated");
		EditorGUI.LabelField(new Rect(startX, startY+=20, 200, 20), cont);
		ability.visualEffect=(GameObject)EditorGUI.ObjectField(new Rect(startX+70, startY-1, 110, 17), ability.visualEffect, typeof(GameObject), false);
		
		
		
		startY=245;
		startX+=230;
		
		EditorGUI.LabelField(new Rect(startX, startY, 200, 20), "Energy Cost: ");
		ability.energy=EditorGUI.FloatField(new Rect(startX+100, startY-1, 40, 17), ability.energy);
		
		if(rscList.Count!=ability.costs.Length) ability.UpdateCostListLength(rscList.Count);
		EditorGUI.LabelField(new Rect(startX, startY+=25, 200, 20), "Resource Cost: ");
		for(int i=0; i<rscList.Count; i++){
			EditorGUI.LabelField(new Rect(startX, startY+=17, 200, 17), " - "+rscList[i].name+": ");
			ability.costs[i] = EditorGUI.IntField(new Rect(startX+100, startY-1, 40, 17), ability.costs[i]);
		}
		
		
//		startY+=20;
//		abilityType=(int)ability.type;
//		EditorGUI.LabelField(new Rect(startX, startY, 200, 20), "Type: ");
//		abilityType=EditorGUI.Popup(new Rect(startX+50, startY, 150, 20), abilityType, abilityTypeLabel);
//		if(abilityType!=(int)ability.type) ability.SetType((_AbilityEffects)abilityType);
		
		
		startY+=20;
		
		
//		EditorGUI.LabelField(new Rect(startX, startY+=20, 200, 20), "Min Abilitys Required: ");
//		ability.abilityMin=EditorGUI.IntField(new Rect(startX+150, startY-1, 50, 17), ability.abilityMin);
//		
//		EditorGUI.LabelField(new Rect(startX, startY+=20, 200, 20), "Max Use Count: ");
//		ability.repeatable=EditorGUI.Toggle(new Rect(startX+150, startY-1, 50, 17), ability.repeatable);
//		
		startY+=10;
		
	
		
//		if(ability.enableObj){
//			int towerID=MapAbilityToTowerIDList(ability.towerID);
//			
//			EditorGUI.LabelField(new Rect(startX, startY+=20, 100, 20), "Tower: ");
//			towerID=EditorGUI.Popup(new Rect(startX+50, startY, 140, 17), towerID, towerNameList);
//			
//			ability.towerID=MapTowerIDListToAbility(towerID);
//		}
		
//		if(ability.valueCount>0){
//			if(ability.enableModTypeVal){
//				int modType=(int)ability.modTypeVal;
//				EditorGUI.LabelField(new Rect(startX, startY+=20, 200, 20), "Modifier Type: ");
//				modType=EditorGUI.Popup(new Rect(startX+110, startY, 90, 20), modType, modTypeLabel);
//				ability.modTypeVal=(_ModifierType)modType;
//			}
//			
//			for(int i=0; i<ability.valueCount; i++){
//				EditorGUI.LabelField(new Rect(startX, startY+=20, 200, 20), "Value"+(i+1)+": ");
//				ability.value[i]=EditorGUI.FloatField(new Rect(startX+150, startY-1, 50, 17), ability.value[i]);
//			}
//		}
		
//		if(ability.enableRsc){
//			if(ability.enableModTypeRsc){
//				int modType=(int)ability.modTypeRsc;
//				EditorGUI.LabelField(new Rect(startX, startY+=20, 200, 20), "Modifier Type: ");
//				modType=EditorGUI.Popup(new Rect(startX+110, startY, 80, 20), modType, modTypeLabel);
//				ability.modTypeRsc=(_ModifierType)modType;
//			}
//			
//			if(rscList.Count!=ability.rsc.Length) ability.UpdateRscListLength(rscList.Count);
//			EditorGUI.LabelField(new Rect(startX, startY+=50, 200, 20), "Resource: ");
//			for(int i=0; i<rscList.Count; i++){
//				EditorGUI.LabelField(new Rect(startX, startY+=17, 200, 17), " - "+rscList[i].name+": ");
//				ability.rsc[i] = EditorGUI.FloatField(new Rect(startX+100, startY-1, 40, 17), ability.rsc[i]);
//			}
//		}
		
		
		//~ abilityType=(int)ability.type;
		//~ EditorGUI.LabelField(new Rect(startX, startY, 200, 20), "AbilityType: ");
		//~ abilityType=EditorGUI.Popup(new Rect(startX+70, startY, 130, 20), abilityType, abilityTypeLabel);
		//~ ability.SetType((_AbilityEffects)abilityType);
		//ability.type=(_AbilityEffects)abilityType;
		
		
		
		
		
		
		startX=10;
		startY=400;
		
//		if(GUI.Button(new Rect(startX, startY, 50, 20), "-")){
//			
//		}
		if(ability.effects.Count<3){
			cont=new GUIContent("Add Effects", "Add effect to the selected ability. Each ability can have up to 3");
			if(GUI.Button(new Rect(startX, startY, 120, 20), cont)){
				if(ability.effects.Count<3){
					ability.effects.Add(new AbilityEffect());
				}
			}
		}
		
		//~ ability.aoeRange=
		//~ EditorGUI.FloatField(new Rect(startX+150, startY, 120, 20), "Ability Range:", ability.aoeRange);
		EditorGUI.LabelField(new Rect(startX+150, startY, 200, 17), "Ability Range (Radius): ");
		ability.aoeRange=EditorGUI.FloatField(new Rect(startX+290, startY, 40, 17), ability.aoeRange);
		//~ ability.cdDuration=EditorGUI.FloatField(new Rect(startX+130, startY-1, 50, 17), ability.cdDuration);
		
		startY=430;
		
		for(int i=0; i<Mathf.Min(ability.effects.Count, 3); i++){
			if(effectSwapID==i) GUI.color=new Color(0f, 1f, .5f, 1); 
			if(GUI.Button(new Rect(startX, startY, 55, 15), "Swap")){
				if(effectSwapID==-1) effectSwapID=i;
				else if(effectSwapID==i) effectSwapID=-1;
				else{
					SwapEffect(ability, effectSwapID, i);
					effectSwapID=-1;
				}
			}
			GUI.color=Color.white;
			if(GUI.Button(new Rect(startX+60, startY, 75, 15), "Remove")){
				ability.RemoveEffect(i);
				return;
			}
			
			startY+=20;
			
			int abilType=(int)ability.effects[i].type;
			EditorGUI.LabelField(new Rect(startX, startY, 200, 20), "Effect "+(i+1).ToString()+": ");
			abilType=EditorGUI.Popup(new Rect(startX+55, startY, 115, 20), abilType, abilityTypeLabel);
			if(abilType!=(int)ability.effects[i].type) ability.effects[i].SetType((_AbilityEffects)abilType);
			
			startY+=5;
			
			if(ability.effects[i].valueCount>0){
				if(ability.effects[i].enableModTypeVal){
					int modType=(int)ability.effects[i].modType;
					EditorGUI.LabelField(new Rect(startX, startY+=20, 170, 20), "ModifierType: ");
					modType=EditorGUI.Popup(new Rect(startX+90, startY, 80, 20), modType, modTypeLabel);
					ability.effects[i].modType=(_ModifierType)modType;
				}
				
				for(int j=0; j<ability.effects[i].valueCount; j++){
					EditorGUI.LabelField(new Rect(startX, startY+=20, 180, 20), "Value"+(j+1)+": ");
					ability.effects[i].value[j]=EditorGUI.FloatField(new Rect(startX+120, startY-1, 50, 17), ability.effects[i].value[j]);
				}
			}
		
			startY+=10;	
			//EditorGUI.LabelField(new Rect(startX, startY, 200, 34), "Description: ");
			startY+=17;
			EditorGUI.LabelField(new Rect(startX+5, startY, 170, 300), abilityDesp[(int)ability.effects[i].type], style);
			
			startX+=200;
			startY=430;
		}
		
		

		startX=5;
		startY=(int)window.position.height-75;
		EditorGUI.LabelField(new Rect(startX, startY, 300, 25), "Ability Description (for runtime UI): ");
		ability.desp=EditorGUI.TextArea(new Rect(startX, startY+17, window.position.width-10, 50), ability.desp);
	}
	
	int effectSwapID=-1;
	void SwapEffect(Ability abil, int source, int target){
		AbilityEffect effect=abil.effects[source];
		abil.effects[source]=abil.effects[target];
		abil.effects[target]=effect;
	}
	
	static string[] abilityTypeLabel=new string[0];
	static string[] abilityDesp=new string[0];
	static string[] modTypeLabel=new string[2];
	
	public static void InitLabel(){
		modTypeLabel[0]="numerical";
		modTypeLabel[1]="percentage";
		
		int enumLength = Enum.GetValues(typeof(_AbilityEffects)).Length;
		abilityTypeLabel=new string[enumLength];
		abilityDesp=new string[enumLength];
		
		int n=-1;
		n+=1;	abilityTypeLabel[n]="AOEDamage";
		n+=1;	abilityTypeLabel[n]="AOESlow";
		n+=1;	abilityTypeLabel[n]="AOEStun";
		n+=1;	abilityTypeLabel[n]="AOEDotUnit";
		n+=1;	abilityTypeLabel[n]="AOEDotArea";
		n+=1;	abilityTypeLabel[n]="AOEArmorReduction";
		
		//~ n+=1;	abilityTypeLabel[n]="SingleDamage";
		//~ n+=1;	abilityTypeLabel[n]="SingleSlow";
		//~ n+=1;	abilityTypeLabel[n]="SingleStun";
		//~ n+=1;	abilityTypeLabel[n]="SingleDot";
		
		n+=1;	abilityTypeLabel[n]="AOEBoost";
		n+=1;	abilityTypeLabel[n]="AOERepair";
		n+=1;	abilityTypeLabel[n]="AOEShield";
//		n+=1;	abilityTypeLabel[n]="AOEBoostSpeed";
//		n+=1;	abilityTypeLabel[n]="AOEBoostRange";
//		n+=1;	abilityTypeLabel[n]="AOEBoostDamage";
		
		int m=-1;
		m+=1;	abilityDesp[m]="Cuase AOE damage in a designated area.\nvalue 1 = damage";
		//~ m+=1;	abilityDesp[m]="/////////////////////////////////////////////////////////////////////////////////";
		m+=1;	abilityDesp[m]="AOE slow creep in a designated area.\nvalue 1 = slow factor\nvalue 2 = slow duration";
		m+=1;	abilityDesp[m]="AOE stun creep in a designated area.\nvalue 1 = stun duration";
		m+=1;	abilityDesp[m]="Cuase AOE damage over time on unit in a designated area.\nvalue 1 = duration\nvalue 2 = interval\nvalue 3 = damage";
		m+=1;	abilityDesp[m]="Cuase AOE damage over time in a designated spot.\nvalue 1 = duration\nvalue 2 = interval\nvalue 3 = damage";
		m+=1;	abilityDesp[m]="All affected creep take more damage over a specified duration.\nvalue 1 = additional damage\nvalue 2 = duration";
		
		//~ m+=1;	abilityDesp[m]="Damage a designated creep.";
		//~ m+=1;	abilityDesp[m]="Slow a designated creep.";
		//~ m+=1;	abilityDesp[m]="Stun a designated creep.";
		//~ m+=1;	abilityDesp[m]="Apply damage over time to a designated creep.";
		
		m+=1;	abilityDesp[m]="Boost certain stats of towers within a designated Area. \nvalue 1 = duration\nvalue 2 = damage buff\nvalue 3 = cooldown buff\nvalue 4 = range buff\nvalue 5 = HP regeneration";
		m+=1;	abilityDesp[m]="Instantly repair any damaged towers within the designated area.\nvalue 1 = heal amount";
		m+=1;	abilityDesp[m]="All towers within the area is immune to any damage for the specified duration.\nvalue 1 = shield duration";
//		m+=1;	abilityDesp[m]="Boost speed of towers within a designated Area.";
//		m+=1;	abilityDesp[m]="Boost range of towers within a designated Area.";
//		m+=1;	abilityDesp[m]="Boost damage of towers within a designated Area.";
		
		
//		for(int i=0; i<enumLength; i++){
//			abilityTypeLabel[i]="type label"+i;
//		}
//		for(int i=0; i<enumLength; i++){
//			abilityDesp[i]="type desp "+ i;
//		}
	}
	
	
//public enum _AbilityEffects{
//	AOEDamage,
//	AOESlow,
//	AOEStun,
//	AOEDot,
//	
//	SingleDamage,
//	SingleSlow,
//	SingleStun,
//	SingleDot,
//	
//	AOEBoostAll,
//	AOEBoostSpeed,
//	AOEBoostRange,
//	AOEBoostDamage,
//}
	
}
