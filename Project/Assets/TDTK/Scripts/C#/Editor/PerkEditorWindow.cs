using UnityEngine;
using UnityEditor;

using System;
using System.Xml;
using System.IO;

using System.Collections;
using System.Collections.Generic;


public class PerkEditorWindow : EditorWindow {
	
	public delegate void UpdateHandler(); 
	public static event UpdateHandler onPerksUpdateE;

	static private PerkEditorWindow window;
	
	private static List<Resource> rscList=new List<Resource>();
	
	private static List<UnitTower> towerList=new List<UnitTower>();
	private static string[] towerNameList=new string[0];
	private static int[] towerIDMapList=new int[0];
	
	private static List<Ability> abilityList=new List<Ability>();
	private static string[] abilityNameList=new string[0];
	private static int[] abilityIDMapList=new int[0];
	
	
	private static List<Perk> allPerkList=new List<Perk>();
	
	private static List<int> perkIDList=new List<int>();
	//~ private static List<int> towerIDList=new List<int>();
	
    // Add menu named "PerkEditor" to the Window menu
    //[MenuItem ("TDTK/PerkEditor")]
    public static void Init () {
        // Get existing open window or if none, make a new one:
        window = (PerkEditorWindow)EditorWindow.GetWindow(typeof (PerkEditorWindow));
		window.minSize=new Vector2(615, 700);
		window.maxSize=new Vector2(615, 701);
		
		InitLabel();
		
		allPerkList=new List<Perk>();
		
		rscList=ResourceEditorWindow.Load();
		LoadTower();
		LoadAbility();
		
		
		Load();
		//~ RefreshIDList();
		
		int i=0;
		foreach(Perk perk in allPerkList){
			//~ perk.SetType(perk.type);
			//~ perk.value=new float[perk.valueCount];
			perk.ID=i;
			i+=1;
		}
		
		NewSelection(0);
    }
	
	static void LoadTower(){
		GameObject obj=Resources.Load("PrefabListTower", typeof(GameObject)) as GameObject;
		if(obj!=null){
			TowerListPrefab prefab=obj.GetComponent<TowerListPrefab>();
			if(prefab!=null){
				towerList=prefab.towerList;
				
				towerNameList=new string[towerList.Count];
				towerIDMapList=new int[towerList.Count];
				for(int i=0; i<towerList.Count; i++){
					towerNameList[i]=towerList[i].unitName;
					towerIDMapList[i]=towerList[i].prefabID;
				}
			}
		}
	}
	
	static void LoadAbility(){
		GameObject obj=Resources.Load("PrefabListAbility", typeof(GameObject)) as GameObject;
		if(obj==null) obj=AbilityEditorWindow.CreatePrefab();
		AbilityListPrefab prefab=obj.GetComponent<AbilityListPrefab>();
		if(prefab==null) prefab=obj.AddComponent<AbilityListPrefab>();
		abilityList=prefab.abilityList;
		
		abilityNameList=new string[abilityList.Count];
		abilityIDMapList=new int[abilityList.Count];
		for(int i=0; i<abilityList.Count; i++){
			abilityNameList[i]=abilityList[i].name;
			abilityIDMapList[i]=abilityList[i].ID;
		}
		
		InitAbilityGroup();
	}
	
	static void SwapPerkInList(int ID1, int ID2){
		Perk temp=allPerkList[ID1].Clone();
		allPerkList[ID1]=allPerkList[ID2].Clone();
		allPerkList[ID2]=temp.Clone();
		InitPerkUIDependency();
		
		foreach(Perk perk in allPerkList){
			for(int i=0; i<perk.prereq.Count; i++){
				if(perk.prereq[i]==ID1) perk.prereq[i]=ID2;
				else if(perk.prereq[i]==ID2) perk.prereq[i]=ID1;
			}
		}
	}
	
	void UpdateResourceList(){
		rscList=ResourceEditorWindow.Load();
		Repaint();
	}
	
	void OnEnable(){
		ResourceEditorWindow.onResourceUpdateE+=UpdateResourceList;
		TowerManager.onTowerUpdateE+=LoadTower;
		AbilityEditorWindow.onAbilityUpdateE+=LoadAbility;
	}
	
	void OnDisable(){
		ResourceEditorWindow.onResourceUpdateE-=UpdateResourceList;
		TowerManager.onTowerUpdateE-=LoadTower;
		AbilityEditorWindow.onAbilityUpdateE-=LoadAbility;
	}
	
	void SaveToXML(){
		
		Debug.Log("writing...");
		
		GameObject obj=Resources.Load("IconList", typeof(GameObject)) as GameObject;
		IconList iconList=obj.GetComponent<IconList>();
		iconList.perkIconList=new List<Texture>();
		
		XmlDocument xmlDoc=new XmlDocument();
		xmlDoc.LoadXml("<something></something>");
		
		for(int j=0; j<allPerkList.Count; j++){
			Perk perk=allPerkList[j];
			
			XmlNode docRoot = xmlDoc.DocumentElement;
			XmlElement perkEle = xmlDoc.CreateElement("Perk"+j.ToString());
		
			XmlAttribute Attr = xmlDoc.CreateAttribute("ID");
			Attr.Value = perk.ID.ToString();
			perkEle.Attributes.Append(Attr);
			
			XmlAttribute Attr1 = xmlDoc.CreateAttribute("name");
			XmlAttribute Attr2 = xmlDoc.CreateAttribute("type");
			XmlAttribute Attr3 = xmlDoc.CreateAttribute("waveMin");
			XmlAttribute Attr4 = xmlDoc.CreateAttribute("perkMin");
			XmlAttribute Attr5 = xmlDoc.CreateAttribute("modTypeVal");
			XmlAttribute Attr6 = xmlDoc.CreateAttribute("modTypeRsc");
			XmlAttribute Attr8 = xmlDoc.CreateAttribute("repeat");
			
			Attr1.Value = perk.name.ToString(); 
			Attr2.Value = ((int)perk.type).ToString();
			Attr3.Value = perk.waveMin.ToString();
			Attr4.Value = perk.perkMin.ToString();
			Attr5.Value = ((int)perk.modTypeVal).ToString();
			Attr6.Value = ((int)perk.modTypeRsc).ToString();
			Attr8.Value = (perk.repeatable ? 1 : 0).ToString();
			
			perkEle.Attributes.Append(Attr1);
			perkEle.Attributes.Append(Attr2);
			perkEle.Attributes.Append(Attr3);
			perkEle.Attributes.Append(Attr4);
			perkEle.Attributes.Append(Attr5);
			perkEle.Attributes.Append(Attr6);
			perkEle.Attributes.Append(Attr8);
			
			if(perk.valueCount>0){
				XmlAttribute valCount = xmlDoc.CreateAttribute("valueCount");
				valCount.Value = perk.value.Length.ToString();
				perkEle.Attributes.Append(valCount);
				
				XmlAttribute[] attrVal = new XmlAttribute[perk.value.Length];
				for(int i=0; i<perk.value.Length; i++){
					attrVal[i]=xmlDoc.CreateAttribute("val"+i.ToString());
					attrVal[i].Value=perk.value[i].ToString();
					perkEle.Attributes.Append(attrVal[i]);
				}
			}
			
			if(perk.icon!=null){
				int ID=0;
				if(!iconList.perkIconList.Contains(perk.icon)){
					iconList.perkIconList.Add(perk.icon);
					ID=iconList.perkIconList.Count-1;
				}
				else ID=iconList.perkIconList.IndexOf(perk.icon);
				
				XmlAttribute AttrIcon = xmlDoc.CreateAttribute("icon");
				AttrIcon.Value = (ID).ToString();
				perkEle.Attributes.Append(AttrIcon);
			}
			
			if(perk.iconUnavailable!=null){
				int ID=0;
				if(!iconList.perkIconList.Contains(perk.iconUnavailable)){
					iconList.perkIconList.Add(perk.iconUnavailable);
					ID=iconList.perkIconList.Count-1;
				}
				else ID=iconList.perkIconList.IndexOf(perk.iconUnavailable);
				
				XmlAttribute AttrIcon = xmlDoc.CreateAttribute("iconUA");
				AttrIcon.Value = (ID).ToString();
				perkEle.Attributes.Append(AttrIcon);
			}
			
			if(perk.iconUnlocked!=null){
				//~ string sourcePath=AssetDatabase.GetAssetPath(perk.iconUnlocked);
				//~ string targetPath=Application.dataPath  + "/TDTK/Resources/PerkIcons/";
				//~ if(Resources.Load("PerkIcons/"+perk.iconUnlocked.name, typeof(Texture))==null)
				//~ AssetDatabase.CopyAsset(sourcePath, targetPath);
				
				//~ XmlAttribute AttrIcon = xmlDoc.CreateAttribute("iconUL");
				//~ AttrIcon.Value = (perk.iconUnlocked.name).ToString();
				//~ perkEle.Attributes.Append(AttrIcon);
				
				int ID=0;
				if(!iconList.perkIconList.Contains(perk.iconUnlocked)){
					iconList.perkIconList.Add(perk.iconUnlocked);
					ID=iconList.perkIconList.Count-1;
				}
				else ID=iconList.perkIconList.IndexOf(perk.iconUnlocked);
				
				XmlAttribute AttrIcon = xmlDoc.CreateAttribute("iconUL");
				AttrIcon.Value = (ID).ToString();
				perkEle.Attributes.Append(AttrIcon);
			}
			
			if(perk.enableTower){
				XmlAttribute AttrTID = xmlDoc.CreateAttribute("towerID");
				AttrTID.Value = perk.towerID.ToString();
				perkEle.Attributes.Append(AttrTID);
			}
			
			if(perk.enableAbility){
				XmlAttribute AttrAID = xmlDoc.CreateAttribute("abilityID");
				AttrAID.Value = perk.abilityID.ToString();
				perkEle.Attributes.Append(AttrAID);
			}
			if(perk.enableAbilityS){
				XmlAttribute AttrASIDF = xmlDoc.CreateAttribute("enableASID");
				AttrASIDF.Value = (perk.enableAbilityS ? 1 : 0).ToString();
				perkEle.Attributes.Append(AttrASIDF);
				
				XmlAttribute AttrASID = xmlDoc.CreateAttribute("abilitySID");
				AttrASID.Value = perk.abilitySID.ToString();
				perkEle.Attributes.Append(AttrASID);
			}
			if(perk.enableAbilityGroup){
				XmlAttribute attrAbilCount = xmlDoc.CreateAttribute("abilityGCount");
				attrAbilCount.Value = perk.abilityGroup.Count.ToString();
				perkEle.Attributes.Append(attrAbilCount);
				
				XmlAttribute[] attrAbil = new XmlAttribute[perk.abilityGroup.Count];
				for(int i=0; i<perk.abilityGroup.Count; i++){
					attrAbil[i]=xmlDoc.CreateAttribute("abil"+i.ToString());
					attrAbil[i].Value=perk.abilityGroup[i].ToString();
					perkEle.Attributes.Append(attrAbil[i]);
				}
			}
			
			if(perk.enableRsc){
				XmlAttribute attrRscCount = xmlDoc.CreateAttribute("rscCount");
				attrRscCount.Value = perk.rsc.Length.ToString();
				perkEle.Attributes.Append(attrRscCount);
				
				XmlAttribute[] attrRsc = new XmlAttribute[perk.rsc.Length];
				for(int i=0; i<perk.rsc.Length; i++){
					attrRsc[i]=xmlDoc.CreateAttribute("rsc"+i.ToString());
					attrRsc[i].Value=perk.rsc[i].ToString();
					perkEle.Attributes.Append(attrRsc[i]);
				}
			}
			
			
			XmlAttribute attrCostCount = xmlDoc.CreateAttribute("costCount");
			attrCostCount.Value = perk.costs.Length.ToString();
			perkEle.Attributes.Append(attrCostCount);
			XmlAttribute[] attrCost = new XmlAttribute[perk.costs.Length];
			for(int i=0; i<perk.costs.Length; i++){
				attrCost[i]=xmlDoc.CreateAttribute("cost"+i.ToString());
				attrCost[i].Value=perk.costs[i].ToString();
				perkEle.Attributes.Append(attrCost[i]);
			}
			
			XmlAttribute attrPrereqCount = xmlDoc.CreateAttribute("prereqCount");
			attrPrereqCount.Value = perk.prereq.Count.ToString();
			perkEle.Attributes.Append(attrPrereqCount);
			XmlAttribute[] attrPrereq = new XmlAttribute[perk.prereq.Count];
			for(int i=0; i<perk.prereq.Count; i++){
				attrPrereq[i]=xmlDoc.CreateAttribute("prereq"+i.ToString());
				attrPrereq[i].Value=perk.prereq[i].ToString();
				perkEle.Attributes.Append(attrPrereq[i]);
			}
			
			
			XmlAttribute attrDesp = xmlDoc.CreateAttribute("desp");
			attrDesp.Value = perk.desp.ToString();
			perkEle.Attributes.Append(attrDesp);
			
			docRoot.AppendChild(perkEle);
		}
		
		//~ if(Application.platform == RuntimePlatform.OSXEditor){
			xmlDoc.Save(Application.dataPath  + "/TDTK/Resources/Perk.txt");
		//~ }
		//~ else if (Application.platform == RuntimePlatform.WindowsEditor){
			//~ xmlDoc.Save(Application.dataPath  + "\\TDTK\\Resources\\Perk.txt");
		//~ }
		
		EditorUtility.SetDirty(iconList);
		AssetDatabase.Refresh();
		
		if(onPerksUpdateE!=null) onPerksUpdateE();
		
		Debug.Log("done");
	}
	
	public static List<Perk> Load(){
		XmlDocument xmlDoc = new XmlDocument();
		
		GameObject obj=Resources.Load("IconList", typeof(GameObject)) as GameObject;
		IconList iconList=obj.GetComponent<IconList>();
		
		TextAsset perkTextAsset=Resources.Load("Perk", typeof(TextAsset)) as TextAsset;
		if(perkTextAsset!=null){
			xmlDoc.Load(new MemoryStream(perkTextAsset.bytes));
			XmlNode rootNode = xmlDoc.FirstChild;
			if (rootNode.Name == "something"){
				int perkCount=rootNode.ChildNodes.Count;
				allPerkList=new List<Perk>();
				for(int n=0; n<perkCount; n++){
					allPerkList.Add(new Perk());
					Perk perk=allPerkList[n];
					
					for(int m=0; m<rootNode.ChildNodes[n].Attributes.Count; m++){
						XmlAttribute attr=rootNode.ChildNodes[n].Attributes[m];
						if(attr.Name=="ID"){
							perk.ID=int.Parse(attr.Value);
						}
						else if(attr.Name=="icon"){
							int ID=int.Parse(attr.Value);
							if(iconList.perkIconList.Count>ID) perk.icon=iconList.perkIconList[ID];
						}
						else if(attr.Name=="iconUL"){
							int ID=int.Parse(attr.Value);
							if(iconList.perkIconList.Count>ID) perk.iconUnlocked=iconList.perkIconList[ID];
						}
						else if(attr.Name=="iconUA"){
							int ID=int.Parse(attr.Value);
							if(iconList.perkIconList.Count>ID) perk.iconUnavailable=iconList.perkIconList[ID];
							//~ string icon=attr.Value;
							//~ perk.iconUnavailable=Resources.Load("PerkIcons/"+icon, typeof(Texture)) as Texture;
						}
						else if(attr.Name=="name"){
							perk.name=attr.Value;
						}
						else if(attr.Name=="type"){
							perk.SetType((_PerkType)int.Parse(attr.Value));
						}
						else if(attr.Name=="waveMin"){
							perk.waveMin=int.Parse(attr.Value);
						}
						else if(attr.Name=="perkMin"){
							perk.perkMin=int.Parse(attr.Value);
						}
						else if(attr.Name=="modTypeVal"){
							perk.modTypeVal=(_ModifierType)int.Parse(attr.Value);
						}
						else if(attr.Name=="modTypeRsc"){
							perk.modTypeRsc=(_ModifierType)int.Parse(attr.Value);
						}
						else if(attr.Name=="valueCount"){
							int count=int.Parse(attr.Value);
							perk.valueCount=count;
							perk.value=new float[count];
							for(int i=m; i<m+count; i++){
								perk.value[i-m]=float.Parse(rootNode.ChildNodes[n].Attributes[i+1].Value.ToString());
							}
						}
						else if(attr.Name=="desp"){
							perk.desp=attr.Value;
						}
						else if(attr.Name=="towerID"){
							perk.towerID=int.Parse(attr.Value);
						}
						
						else if(attr.Name=="abilityID"){
							perk.abilityID=int.Parse(attr.Value);
						}
						else if(attr.Name=="enableASID"){
							int val=int.Parse(attr.Value);
							if(val==1) perk.enableAbilityS=true;
							else perk.enableAbilityS=false;
						}
						else if(attr.Name=="abilitySID"){
							perk.abilitySID=int.Parse(attr.Value);
						}
						else if(attr.Name=="abilityGCount"){
							int count=int.Parse(attr.Value);
							perk.abilityGroup=new List<int>();
							for(int i=m; i<m+count; i++){
								perk.abilityGroup.Add(int.Parse(rootNode.ChildNodes[n].Attributes[i+1].Value));
							}
						}
						
						else if(attr.Name=="repeat"){
							int val=int.Parse(attr.Value);
							if(val==1) perk.repeatable=true;
							else perk.repeatable=false;
						}
						else if(attr.Name=="rscCount"){
							int count=int.Parse(attr.Value);
							perk.rsc=new float[count];
							for(int i=m; i<m+count; i++){
								perk.rsc[i-m]=float.Parse(rootNode.ChildNodes[n].Attributes[i+1].Value);
							}
						}
						else if(attr.Name=="costCount"){
							int count=int.Parse(attr.Value);
							perk.costs=new int[count];
							for(int i=m; i<m+count; i++){
								perk.costs[i-m]=int.Parse(rootNode.ChildNodes[n].Attributes[i+1].Value);
							}
						}
						else if(attr.Name=="prereqCount"){
							int count=int.Parse(attr.Value);
							for(int i=m; i<m+count; i++){
								perk.prereq.Add(int.Parse(rootNode.ChildNodes[n].Attributes[i+1].Value));
							}
						}
					}
					
				}
			}
		}
		
		for(int i=0; i<allPerkList.Count; i++){
			//allPerkList[i].ID=i;
			perkIDList.Add(allPerkList[i].ID);
		}
		
		return allPerkList;
	}
	
	
	int delete=-1;
	static int swapID=-1;
	static int selectedPerkID=0;
	private Vector2 scrollPos;
	
	int GenerateNewID(){
		int newID=0;
		while(perkIDList.Contains(newID)) newID+=1;
		return newID;
	}
	
	void OnGUI () {
		if(window==null) Init();
		
		if(GUI.Button(new Rect(window.position.width-110, 10, 100, 30), "Save")){
			SaveToXML();
		}
		
		if(GUI.Button(new Rect(5, 10, 100, 30), "New Perk")){
			Perk perk=new Perk();
			perk.ID=GenerateNewID();
			perkIDList.Add(perk.ID);
			perk.name="Perk "+allPerkList.Count;
			allPerkList.Add(perk);
			
			NewSelection(allPerkList.Count-1);
			delete=-1;
		}
		if(selectedPerkID>=0 && selectedPerkID<allPerkList.Count){
			if(GUI.Button(new Rect(115, 10, 100, 30), "Clone Perk")){
				Perk perk=allPerkList[selectedPerkID].Clone();
				perk.ID=GenerateNewID();
				perkIDList.Add(perk.ID);
				allPerkList.Add(perk);
				
				NewSelection(allPerkList.Count-1);
				delete=-1;
			}
		}
		
		
		GUI.Box(new Rect(5, 50, window.position.width-10, 185), "");
		scrollPos = GUI.BeginScrollView(new Rect(5, 55, window.position.width-12, 175), scrollPos, new Rect(5, 50, window.position.width-40, 10+((allPerkList.Count-1)/3)*35));
		
		int row=0;
		int column=0;
		for(int i=0; i<allPerkList.Count; i++){
			
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
					SwapPerkInList(swapID, i);
					swapID=-1;
				}
			}
			GUI.Label(new Rect(8+column*210, 50+row*30, 25, 25), i.ToString(), style);
			GUI.color=Color.white;
			style.alignment=TextAnchor.MiddleLeft;
			
			if(selectedPerkID==i) GUI.color = Color.green;
			if(allPerkList[selectedPerkID].prereq.Contains(i)) GUI.color=new Color(0, 1f, 1f, 1f);
			style=GUI.skin.button;
			style.fontStyle=FontStyle.Bold;
			
			GUI.SetNextControlName ("PerkButton");
			if(GUI.Button(new Rect(10+27+column*210, 50+row*30, 100, 25), allPerkList[i].name, style)){
				GUI.FocusControl ("PerkButton");
				NewSelection(i);
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
					CheckDeletePrereq(i);
					perkIDList.Remove(allPerkList[i].ID);
					allPerkList.RemoveAt(i);
					InitPerkUIDependency();
					if(i<=selectedPerkID) selectedPerkID-=1;
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
		
		if(selectedPerkID>-1 && selectedPerkID<allPerkList.Count){
			PerkConfigurator();
		}
		
		//~ if (GUI.changed) Debug.Log("change!  "+Time.time);
		//~ Debug.Log(window.position);
		
		 //~ flags = EditorGUI.MaskField (new Rect (0, 0, 300, 20), "Player Flags", flags, perkTypeLabel);
		//~ Debug.Log(flags);
	}
	
	 //~ int flags  = 0;
    
	static private int perkType=0;
	static int newPrereq=2000;
	static private List<int> prereq=new List<int>();
	
	static int newAbility=2000;
	static private List<int> abilityGroup=new List<int>();
	
	void PerkConfigurator(){
		int startY=245;
		int startX=5;
		
		GUIStyle style=new GUIStyle();
		style.wordWrap=true;
		
		Perk perk=allPerkList[selectedPerkID];
		
		EditorGUI.LabelField(new Rect(startX, startY, 200, 20), "Default Icon: ");
		perk.icon=(Texture)EditorGUI.ObjectField(new Rect(startX+10, startY+17, 60, 60), perk.icon, typeof(Texture), false);
		startX+=100;
		
		EditorGUI.LabelField(new Rect(startX, startY, 200, 34), "Unavailable: ");
		perk.iconUnavailable=(Texture)EditorGUI.ObjectField(new Rect(startX+10, startY+17, 60, 60), perk.iconUnavailable, typeof(Texture), false);
		startX+=80;
		
		EditorGUI.LabelField(new Rect(startX, startY, 200, 34), "Unlocked: ");
		perk.iconUnlocked=(Texture)EditorGUI.ObjectField(new Rect(startX, startY+17, 60, 60), perk.iconUnlocked, typeof(Texture), false);
		
		
		startX+=100;
		EditorGUI.LabelField(new Rect(startX, startY, 200, 34), "Description: ");
		startY+=17;
		//~ GUI.Box(new Rect(startX, startY, window.position.width-startX-10, 120), "");
		//~ EditorGUI.LabelField(new Rect(startX+5, startY+5, window.position.width-startX-15, 115), perkDesp[(int)perk.type], style);
		EditorGUI.LabelField(new Rect(startX, startY, window.position.width-startX-20, 120), perkDesp[(int)perk.type], style);
		//~ GUI.Label(new Rect(startX, startY, window.position.width-startX-10, 90), perkDesp[(int)perk.type]);
		
		startX=5;
		startY=340;
		
		EditorGUI.LabelField(new Rect(startX, startY, 200, 20), "Name: ");
		perk.name=EditorGUI.TextField(new Rect(startX+50, startY-1, 150, 17), perk.name);
		
		startY+=20;
		perkType=(int)perk.type;
		EditorGUI.LabelField(new Rect(startX, startY, 200, 20), "Type: ");
		perkType=EditorGUI.Popup(new Rect(startX+50, startY, 150, 20), perkType, perkTypeLabel);
		if(perkType!=(int)perk.type) perk.SetType((_PerkType)perkType);
		
		//~ EditorGUI.LabelField(new Rect(startX, startY+=20, window.position.width-10, 50), perkDesp[(int)perk.type], style);
		
		
		startY+=15;
		
		EditorGUI.LabelField(new Rect(startX, startY+=20, 200, 20), "Min Wave Required: ");
		perk.waveMin=EditorGUI.IntField(new Rect(startX+150, startY-1, 50, 17), perk.waveMin);
		
		EditorGUI.LabelField(new Rect(startX, startY+=20, 200, 20), "Min Perks Required: ");
		perk.perkMin=EditorGUI.IntField(new Rect(startX+150, startY-1, 50, 17), perk.perkMin);
		
		EditorGUI.LabelField(new Rect(startX, startY+=20, 200, 20), "Repeatable: ");
		perk.repeatable=EditorGUI.Toggle(new Rect(startX+150, startY-1, 50, 17), perk.repeatable);
		
		startY+=10;
		

		
		if(perk.enableTower){
			int towerID=MapPerkToTowerIDList(perk.towerID);
			
			EditorGUI.LabelField(new Rect(startX, startY+=20, 100, 20), "Tower: ");
			towerID=EditorGUI.Popup(new Rect(startX+50, startY, 140, 17), towerID, towerNameList);
			
			perk.towerID=MapTowerIDListToPerk(towerID);
		}
		
		if(perk.enableAbility){
			int abilityID=MapPerkToTowerIDList(perk.abilityID);
			
			EditorGUI.LabelField(new Rect(startX, startY+=20, 100, 20), "Ability: ");
			abilityID=EditorGUI.Popup(new Rect(startX+50, startY, 150, 17), abilityID, abilityNameList);
			
			perk.abilityID=MapTowerIDListToPerk(abilityID);
			
			EditorGUI.LabelField(new Rect(startX, startY+=20, 200, 20), "Replace existing: ");
			perk.enableAbilityS=EditorGUI.Toggle(new Rect(startX+150, startY-1, 50, 17), perk.enableAbilityS);
			
			if(perk.enableAbilityS){
				int abilitySID=MapPerkToTowerIDList(perk.abilitySID);
				
				EditorGUI.LabelField(new Rect(startX, startY+=20, 100, 20), "Ability: ");
				abilitySID=EditorGUI.Popup(new Rect(startX+50, startY, 150, 17), abilitySID, abilityNameList);
				
				if(abilitySID!=perk.abilityID) perk.abilitySID=MapTowerIDListToPerk(abilitySID);
			}
		}
		
		
		if(perk.valueCount>0){
			if(perk.enableModTypeVal){
				int modType=(int)perk.modTypeVal;
				EditorGUI.LabelField(new Rect(startX, startY+=20, 200, 20), "Modifier Type: ");
				modType=EditorGUI.Popup(new Rect(startX+110, startY, 90, 20), modType, modTypeLabel);
				perk.modTypeVal=(_ModifierType)modType;
			}
			
			for(int i=0; i<perk.valueCount; i++){
				//~ if((int)perk.modType==0){
					//~ EditorGUI.LabelField(new Rect(startX, startY+=20, 200, 20), "Value: ");
				//~ }
				//~ else{
					//~ EditorGUI.LabelField(new Rect(startX, startY+=20, 200, 20), "Percent(0-1): ");
				//~ }
				EditorGUI.LabelField(new Rect(startX, startY+=20, 200, 20), "Value"+(i+1)+": ");
				perk.value[i]=EditorGUI.FloatField(new Rect(startX+150, startY-1, 50, 17), perk.value[i]);
				//~ if(perk.enableValueCap) perk.value=Mathf.Min(1f, perk.value[i]);
			}
			
			//~ if(perk.enableValueAlt){
				//~ if((int)perk.modType==0){
					//~ EditorGUI.LabelField(new Rect(startX, startY+=20, 200, 20), "ValueAlt: ");
				//~ }
				//~ else{
					//~ EditorGUI.LabelField(new Rect(startX, startY+=20, 200, 20), "PercentAlt(0-1): ");
				//~ }
				//~ perk.valueAlt=EditorGUI.FloatField(new Rect(startX+150, startY-1, 40, 17), perk.valueAlt);
			//~ }
		}
		
		if(perk.enableRsc){
			if(perk.enableModTypeRsc){
				int modType=(int)perk.modTypeRsc;
				EditorGUI.LabelField(new Rect(startX, startY+=20, 200, 20), "Modifier Type: ");
				modType=EditorGUI.Popup(new Rect(startX+110, startY, 80, 20), modType, modTypeLabel);
				perk.modTypeRsc=(_ModifierType)modType;
			}
			
			if(rscList.Count!=perk.rsc.Length) perk.UpdateRscListLength(rscList.Count);
			//~ if((int)perk.modTypeRsc==0){
				//~ EditorGUI.LabelField(new Rect(startX, startY+=20, 200, 20), "Gain (Value): ");
			//~ }
			//~ else{
				//~ EditorGUI.LabelField(new Rect(startX, startY+=20, 200, 20), "Gain (Percentage 0-1): ");
			//~ }
			EditorGUI.LabelField(new Rect(startX, startY+=50, 200, 20), "Resource: ");
			for(int i=0; i<rscList.Count; i++){
				EditorGUI.LabelField(new Rect(startX, startY+=17, 200, 17), " - "+rscList[i].name+": ");
				perk.rsc[i] = EditorGUI.FloatField(new Rect(startX+100, startY-1, 40, 17), perk.rsc[i]);
			}
		}
		
		
		if(perk.enableAbilityGroup){
			abilityGroup=perk.abilityGroup;
			EditorGUI.LabelField(new Rect(startX, startY+=25, 200, 20), "Ability(s): ");
			int y=startY;
			//existing prereq
			for(int i=0; i<abilityGroup.Count; i++){
				int existAbility=abilityGroup[i];
				existAbility=EditorGUI.Popup(new Rect(startX, startY+=17, 120, 20), existAbility, abilityListLabel);
				if(existAbility==abilityListLabel.Length-1){
					perk.abilityGroup.RemoveAt(i);
				}
				if((i+1)%5==0){ startX+=150; startY=y; }
			}
			//assignNewOne
			if(abilityGroup.Count<abilityListLabel.Length-1){
				newAbility=EditorGUI.Popup(new Rect(startX, startY+=17, 120, 20), newAbility, abilityListLabel);
			}
			if(newAbility<abilityListLabel.Length-1){
				if(!perk.abilityGroup.Contains(newAbility)) perk.abilityGroup.Add(newAbility);
				newAbility=abilityListLabel.Length-1;
			}
		}
		
		
		startY=390;
		startX=250;
		
		if(rscList.Count!=perk.costs.Length) perk.UpdateCostListLength(rscList.Count);
		EditorGUI.LabelField(new Rect(startX, startY, 200, 20), "Cost: ");
		for(int i=0; i<rscList.Count; i++){
			EditorGUI.LabelField(new Rect(startX, startY+=17, 200, 17), " - "+rscList[i].name+": ");
			perk.costs[i] = EditorGUI.IntField(new Rect(startX+100, startY-1, 40, 17), perk.costs[i]);
		}
		
		
		
		
		
		startY=390;
		startX+=200;
		
		
		prereq=perk.prereq;
		EditorGUI.LabelField(new Rect(startX, startY, 200, 20), "Pre-req Perk(s): ");
		//existing prereq
		for(int i=0; i<prereq.Count; i++){
			int existPrereq=prereq[i];
			existPrereq=EditorGUI.Popup(new Rect(startX, startY+=17, 150, 20), existPrereq, perkListLabel);
			if(existPrereq!=selectedPerkID) prereq[i]=existPrereq;
			if(existPrereq==allPerkList.Count) perk.prereq.RemoveAt(i);
		}
		//assignNewOne
		newPrereq=EditorGUI.Popup(new Rect(startX, startY+=17, 150, 20), newPrereq, perkListLabel);
		if(newPrereq<allPerkList.Count){
			if(!perk.prereq.Contains(newPrereq) && selectedPerkID!=newPrereq) perk.prereq.Add(newPrereq);
			newPrereq=allPerkList.Count;
		}
		
		
		
		
		
		startX=5;
		startY=(int)window.position.height-75;
		EditorGUI.LabelField(new Rect(startX, startY, 300, 25), "Perk Description (for runtime UI): ");
		perk.desp=EditorGUI.TextArea(new Rect(startX, startY+17, window.position.width-10, 50), perk.desp);
	}
	
	static void CheckDeletePrereq(int ID){
		for(int i=0; i<allPerkList.Count; i++){
			Perk perk=allPerkList[i];
			if(perk.prereq.Contains(ID)) perk.prereq.Remove(ID);
			for(int n=0; n<perk.prereq.Count; n++){
				if(perk.prereq[n]>ID) perk.prereq[n]-=1;
			}
		}
	}
	
	static void NewSelection(int ID){
		selectedPerkID=ID;
		InitPerkUIDependency();
	}
	
	static string[] perkListLabel=new string[0];
	protected static void InitPerkUIDependency(){
		perkListLabel=new string[allPerkList.Count+1];
		for(int i=0; i<allPerkList.Count; i++){
			if(i!=selectedPerkID){
				perkListLabel[i]=("("+i.ToString()+")"+allPerkList[i].name);
			}
			else perkListLabel[i]="Self";
		}
		perkListLabel[allPerkList.Count]="None";
		
		newPrereq=allPerkList.Count;
	}
	
	
	static string[] abilityListLabel=new string[0];
	protected static void InitAbilityGroup(){
		abilityListLabel=new string[abilityList.Count+1];
		for(int i=0; i<abilityList.Count; i++){
			abilityListLabel[i]=("("+i.ToString()+")"+abilityList[i].name);
		}
		abilityListLabel[abilityList.Count]="None";
		
		newAbility=abilityList.Count;
	}
	
	//map towerID to element number the towerIDMapList
	int MapPerkToTowerIDList(int ID){
		for(int i=0; i<towerIDMapList.Length; i++){// in towerIDMapList){
			if(towerIDMapList[i]==ID){
				return i;
			}
		}
		return -1;
	}
	
	//get towerID based on element number of towerIDMapList
	int MapTowerIDListToPerk(int ID){
		if(ID<0 || ID>=towerIDMapList.Length) return -1;
		return towerIDMapList[ID];
	}
	
	
	static string[] perkTypeLabel=new string[0];
	static string[] perkDesp=new string[0];
	static string[] modTypeLabel=new string[2];
	
	protected static void InitLabel() {
		
		modTypeLabel[0]="numerical";
		modTypeLabel[1]="percentage";
		
		int enumLength = Enum.GetValues(typeof(_PerkType)).Length;
		perkTypeLabel=new string[enumLength];
		perkDesp=new string[enumLength];
		
		int n=-1;
		n+=1;	perkTypeLabel[n]="TowerUnlockNew";
		n+=1;	perkTypeLabel[n]="TowerBuildLevelBonus";
		n+=1;	perkTypeLabel[n]="TowerBuffHP";
		n+=1;	perkTypeLabel[n]="TowerBuffAttack";
		n+=1;	perkTypeLabel[n]="TowerBuffDef";
		n+=1;	perkTypeLabel[n]="TowerCritical";
		n+=1;	perkTypeLabel[n]="TowerCriticalChance";
		n+=1;	perkTypeLabel[n]="TowerCriticalDamage";
		n+=1;	perkTypeLabel[n]="AllCostReduction";
		n+=1;	perkTypeLabel[n]="BuildCostReduction";
		n+=1;	perkTypeLabel[n]="UpgradeCostReduction";
		n+=1;	perkTypeLabel[n]="LifeIncreaseCap";			//10
		n+=1;	perkTypeLabel[n]="LifeBonus";
		n+=1;	perkTypeLabel[n]="LifeBonusWaveCleared";
		n+=1;	perkTypeLabel[n]="LifeRegen";
		n+=1;	perkTypeLabel[n]="ResourceBonus";
		n+=1;	perkTypeLabel[n]="ResourceGeneration";
		n+=1;	perkTypeLabel[n]="ResourceGain";
		n+=1;	perkTypeLabel[n]="ResourceGainCreepKilled";
		n+=1;	perkTypeLabel[n]="ResourceGainWaveCleared";
		n+=1;	perkTypeLabel[n]="ResourceGainResourceTower";
		n+=1;	perkTypeLabel[n]="SpecifiedTowerLevelBonus";
		n+=1;	perkTypeLabel[n]="SpecifiedTowerBuffHP";			
		n+=1;	perkTypeLabel[n]="SpecifiedTowerBuffAttack";
		n+=1;	perkTypeLabel[n]="SpecifiedTowerBuffDef";
		n+=1;	perkTypeLabel[n]="SpecifiedTowerBuffRange";
		n+=1;	perkTypeLabel[n]="SpecifiedTowerCritical";
		n+=1;	perkTypeLabel[n]="SpecifiedTowerCriticalChance";
		n+=1;	perkTypeLabel[n]="SpecifiedTowerCriticalDamage";
		n+=1;	perkTypeLabel[n]="SpecifiedTowerAllCostReduction";
		n+=1;	perkTypeLabel[n]="SpecifiedTowerBuildCostReduction";
		n+=1;	perkTypeLabel[n]="SpecifiedTowerUpgradeCostReduction";	//20
		n+=1;	perkTypeLabel[n]="SpecifiedTowerStun";
		n+=1;	perkTypeLabel[n]="SpecifiedTowerSlow";
		n+=1;	perkTypeLabel[n]="EnergyRegenRate";
		n+=1;	perkTypeLabel[n]="EnergyIncreaseCap";
		n+=1;	perkTypeLabel[n]="EnergyCostReduction";
		n+=1;	perkTypeLabel[n]="EnergyGainCreepKilled";
		n+=1;	perkTypeLabel[n]="EnergyGainWaveCleared";
		n+=1;	perkTypeLabel[n]="AbilityUnlockNew";
		n+=1;	perkTypeLabel[n]="AbilityCost";
		n+=1;	perkTypeLabel[n]="AbililyCooldown";
		


		int m=-1;
		m+=1;	perkDesp[m]="Unlock a new tower.";
		m+=1;	perkDesp[m]="All new tower built with additional level specified by value.";
		m+=1;	perkDesp[m]="All towers' HP pool is Increased by value specified";
		m+=1;	perkDesp[m]="All towers does additional damage specified by value. ";
		m+=1;	perkDesp[m]="All towers reduce incoming damage by value specified.";
		m+=1;	perkDesp[m]="All towers gain additional chance to score critical hit and cause additional damage.\nvalue1-chance increment, value2-damage increment.\nBoth in percent (0-1).";
		m+=1;	perkDesp[m]="All towers gain additional chance (specified by value1) to score critical hit.";
		m+=1;	perkDesp[m]="All towers cause additional damage (specified by value1) when score a critical hit.";
		m+=1;	perkDesp[m]="All tower can be built and upgraded with cost reduction of value specified.";
		m+=1;	perkDesp[m]="All tower can be built with cost reduction of value specified.";
		m+=1;	perkDesp[m]="All tower can be upgraded with cost reduction of value specified.";
		m+=1;	perkDesp[m]="Increase player life by value specified.";		//10
		m+=1;	perkDesp[m]="Gain specified value (range from value1 to value2) amount of life instantly.";
		m+=1;	perkDesp[m]="Chances (value1) to gain specified value (range from value2 to value3) amount of life after surviving each wave.";
		m+=1;	perkDesp[m]="Life will regenerate by value specified (range from value1 to value2) over the time period (value3, in second) specified.";
		m+=1;	perkDesp[m]="Gain instant bonus resource specified.";
		m+=1;	perkDesp[m]="Generate set amount of resource over a period specified by values (range from value1 to value2) in second.";
		m+=1;	perkDesp[m]="Player gain additional resource specified from all source. value1 being the chance of the occurance.";
		m+=1;	perkDesp[m]="Player gain additional resource specified from killing creeps. value1 being the chance of the occurance.";
		m+=1;	perkDesp[m]="Player gain additional resource specified from surviving each wave bonus. value1 being the chance of the occurance.";
		m+=1;	perkDesp[m]="Player gain additional resource specified from resource tower. value1 being the chance of the occurance.";
		m+=1;	perkDesp[m]="Specified tower built with additional level specified by value.";
		m+=1;	perkDesp[m]="Increase Specified towers' HP Increased by value specified";			
		m+=1;	perkDesp[m]="Specified tower does additional damage specified by value.";
		m+=1;	perkDesp[m]="Specified tower reduce incoming damage by value specified.";
		m+=1;	perkDesp[m]="Specified tower range is increased by value specified.";
		m+=1;	perkDesp[m]="Specified tower gain additional chance to score critical hit and cause additional damage.\nvalue1-chance increment, value2-damage increment.\nBoth in percent (0-1)";
		m+=1;	perkDesp[m]="Specified tower gain additional chance (specified by value1) to score critical hit.";
		m+=1;	perkDesp[m]="Specified tower cause additional damage (specified by value1) when score a critical hit.";
		m+=1;	perkDesp[m]="Specified tower can be built and upgraded with cost reduction of value specified.";
		m+=1;	perkDesp[m]="Specified tower can be built with cost reduction of value specified.";
		m+=1;	perkDesp[m]="Specified tower can be upgraded with cost reduction of value specified.";//20
		m+=1;	perkDesp[m]="Specified tower add stun ability or extend stun duration.\nvalue1-chance of occurance, value2-stun duration.";
		m+=1;	perkDesp[m]="Specified tower add slow or modify slow ability.\nvalue1-duration of effect, value2-slow factor.";
		
		m+=1;	perkDesp[m]="Increase the rate at which the energy of AbilityManager is generated";
		m+=1;	perkDesp[m]="Increase the maximum energy capacity of AbilityManager";
		m+=1;	perkDesp[m]="Reduce the cost of general energy usage of Abilities";
		m+=1;	perkDesp[m]="Gain energy for AbilityManager whenever a creep is destroyed.\nvalue1-value gain, value2-change of occurance(0-1)";
		m+=1;	perkDesp[m]="Gain energy for AbilityManager whenever a wave is cleared.\nvalue1-value gain, value2-change of occurance(0-1)";
		m+=1;	perkDesp[m]="Enable new ability with option to remove an existing one.";
		m+=1;	perkDesp[m]="Reduce energy cost for a group of selected abilities";
		m+=1;	perkDesp[m]="Reduce cooldown for a group of selected abilities";
		
	}
	
	
	
}




