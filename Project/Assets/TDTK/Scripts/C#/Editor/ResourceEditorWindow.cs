using UnityEngine;
using UnityEditor;

using System;
using System.Xml;
using System.IO;

using System.Collections;
using System.Collections.Generic;

public class ResourceEditorWindow : EditorWindow {

	public delegate void UpdateHandler(); 
	public static event UpdateHandler onResourceUpdateE;
	
	static private ResourceEditorWindow window;
	
	private static List<Resource> resourceList=new List<Resource>();
	
    // Add menu named "PerkEditor" to the Window menu
    //[MenuItem ("TDTK/PerkEditor")]
    public static void Init () {
        // Get existing open window or if none, make a new one:
        window = (ResourceEditorWindow)EditorWindow.GetWindow(typeof (ResourceEditorWindow));
		window.minSize=new Vector2(350, 449);
		window.maxSize=new Vector2(350, 450);
		
		resourceList.Add(new Resource());
		
		Load();
		
		//~ Debug.Log(AssetDatabase.GetAssetPath(rm.resources[0].icon));
		//http://docs.unity3d.com/Documentation/Manual/AssetDatabase.html
		
		
		//AssetDatabase.GetAssetPath 
    }
	
	void SaveToXML(){
		
		//Debug.Log("writing...");
		
		XmlDocument xmlDoc=new XmlDocument();
		xmlDoc.LoadXml("<something></something>");
		
		GameObject obj=Resources.Load("IconList", typeof(GameObject)) as GameObject;
		IconList iconList=obj.GetComponent<IconList>();
		iconList.rscIconList=new List<Texture>();
		
		for(int j=0; j<resourceList.Count; j++){
			XmlNode docRoot = xmlDoc.DocumentElement;
			XmlElement rscEle = xmlDoc.CreateElement("Perk"+j.ToString());
		
			XmlAttribute Attr1 = xmlDoc.CreateAttribute("name");
			Attr1.Value = resourceList[j].name.ToString(); 
			rscEle.Attributes.Append(Attr1);
			
			//~ XmlAttribute Attr2 = xmlDoc.CreateAttribute("iconPath");
			//~ Attr2.Value = AssetDatabase.GetAssetPath(resourceList[j].icon).ToString();
			//~ rscEle.Attributes.Append(Attr2);
			
			if(resourceList[j].icon!=null){
				//~ string sourcePath=AssetDatabase.GetAssetPath(resourceList[j].icon);
				//~ string targetPath=Application.dataPath  + "/TDTK/Resources/RscIcons/";
				//~ if(Resources.Load("RscIcons/"+resourceList[j].icon.name, typeof(Texture))==null){
					//~ if(Application.platform == RuntimePlatform.OSXEditor){
						//~ FileInfo fInfo=new FileInfo(sourcePath);
						//~ fInfo.CopyTo(targetPath+resourceList[j].icon.name+".png", true);
					//~ }
					//~ else if (Application.platform == RuntimePlatform.WindowsEditor){
						//~ AssetDatabase.CopyAsset(sourcePath, targetPath);
					//~ }
				//~ }
				
				//~ XmlAttribute AttrIcon = xmlDoc.CreateAttribute("icon");
				//~ AttrIcon.Value = (resourceList[j].icon.name).ToString();
				//~ rscEle.Attributes.Append(AttrIcon);
				
				int ID=0;
				if(!iconList.rscIconList.Contains(resourceList[j].icon)){
					iconList.rscIconList.Add(resourceList[j].icon);
					ID=iconList.rscIconList.Count-1;
				}
				else{
					ID=iconList.rscIconList.IndexOf(resourceList[j].icon);
				}
				
				XmlAttribute AttrIcon = xmlDoc.CreateAttribute("icon");
				AttrIcon.Value = (ID).ToString();
				rscEle.Attributes.Append(AttrIcon);
			}
			
			/*
			string sourcePath=AssetDatabase.GetAssetPath(resourceList[j].icon);
			string targetPath=Application.dataPath  + "/TDTK/Resources";
			if(Resources.Load(resourceList[j].icon.name, typeof(Texture))==null){
				AssetDatabase.CopyAsset(sourcePath, targetPath);
			}
			Attr2.Value = resourceList[j].icon.name.ToString();
			*/
			
			docRoot.AppendChild(rscEle);
		}
		
		//~ if(Application.platform == RuntimePlatform.OSXEditor){
			xmlDoc.Save(Application.dataPath  + "/TDTK/Resources/Resource.txt");
		//~ }
		//~ else if (Application.platform == RuntimePlatform.WindowsEditor){
			//~ xmlDoc.Save(Application.dataPath  + "\\TDTK\\Resources\\Resource.txt");
		//~ }
		
		EditorUtility.SetDirty(iconList);
		AssetDatabase.Refresh();
		
		//Debug.Log("done");
		
		if(onResourceUpdateE!=null) onResourceUpdateE();
	}
	
	static public List<Resource> Load(){
		XmlDocument xmlDoc = new XmlDocument();
		
		GameObject obj=Resources.Load("IconList", typeof(GameObject)) as GameObject;
		IconList iconList=obj.GetComponent<IconList>();
		
		TextAsset rscTextAsset=Resources.Load("Resource", typeof(TextAsset)) as TextAsset;
		if(rscTextAsset!=null){
			xmlDoc.Load(new MemoryStream(rscTextAsset.bytes));
			XmlNode rootNode = xmlDoc.FirstChild;
			if (rootNode.Name == "something"){
				int rscCount=rootNode.ChildNodes.Count;
				resourceList=new List<Resource>();
				for(int n=0; n<rscCount; n++){
					
					resourceList.Add(new Resource());
					
					for(int m=0; m<rootNode.ChildNodes[n].Attributes.Count; m++){
						XmlAttribute attr=rootNode.ChildNodes[n].Attributes[m];
						if(attr.Name=="name"){
							resourceList[n].name=attr.Value;
						}
						else if(attr.Name=="icon"){
							int ID=int.Parse(attr.Value);
							if(iconList.rscIconList.Count>ID){
								resourceList[n].icon=iconList.rscIconList[ID];
							}
						}
					}
					
					//~ resourceList.Add(new Resource());
					//~ resourceList[n].name=rootNode.ChildNodes[n].Attributes[0].Value;
					
					//~ string icon=rootNode.ChildNodes[n].Attributes[1].Value;
					//~ resourceList[n].icon=Resources.Load("RscIcons/"+icon, typeof(Texture)) as Texture;
					//string icon=rootNode.ChildNodes[n].Attributes[1].Value;
					//resourceList[n].icon=Resources.Load(icon, typeof(Texture)) as Texture;
				}
			}
		}
		
		return resourceList;
	}
	

	
	private void OnEnable()
    {
        EditorApplication.modifierKeysChanged += this.Repaint;
    }

    private void OnDisable()
    {
        EditorApplication.modifierKeysChanged -= this.Repaint;
    }
	
	int delete=-1;
	//int currentRscID=0;
	private Vector2 scrollPos;
	//~ string warning="";
	
	//~ void Test(){
		//~ string sourcePath=AssetDatabase.GetAssetPath(resourceList[1].icon);
		//~ string targetPath=Application.dataPath  + "/TDTK/Resources/";
		//~ if(AssetDatabase.LoadAssetAtPath(targetPath+resourceList[0].icon.name, typeof(Texture2D))==null){
			//~ AssetDatabase.CopyAsset(sourcePath, targetPath);
		//~ }
		//~ AssetDatabase.Refresh();
	//~ }
	
	//~ void ClearWarning(){
		//~ warning="";
	//~ }
	//~ void UpdateWarning(){
		//~ warning="Please note that you need to save and refresh Resource\nManager in each scene for the new change to take effect.";
	//~ }
	
	void OnGUI () {
		if(window==null) Init();
		
		Undo.SetSnapshotTarget(this, "ResourceEditor");
		
		//~ if(GUI.Button(new Rect(150, 10, 100, 30), "test")){
			//~ Test();
		//~ }
		
		if(GUI.Button(new Rect(window.position.width-110, 10, 100, 30), "Save")){
			//~ warning="Refresh ResourceManager in each scene for the new \nchange to take effect!";
			SaveToXML();
		}
		
		if(GUI.Button(new Rect(10, 10, 100, 30), "New Resource")){
			//UpdateWarning();
			resourceList.Add(new Resource());
		}
		
		if(resourceList.Count>0){
			GUI.Box(new Rect(5, 50, 50, 20), "ID");
			GUI.Box(new Rect(5+50-1, 50, 70+1, 20), "Texture");
			GUI.Box(new Rect(5+120-1, 50, 150+2, 20), "Name");
			GUI.Box(new Rect(5+270, 50, window.position.width-280, 20), "");
		}
		
		int row=0;
		for(int i=0; i<resourceList.Count; i++){
			if(i%2==0) GUI.color=new Color(.8f, .8f, .8f, 1);
			else GUI.color=Color.white;
			GUI.Box(new Rect(5, 75+i*49, window.position.width-10, 50), "");
			GUI.color=Color.white;
			
			GUI.Label(new Rect(22, 15+75+i*49, 50, 20), i.ToString());
			//if(currentSwapID==i) GUI.color=new Color(.9f, .9f, .0f, 1);
			//if(GUI.Button(new Rect(19, 12+60+i*49, 30, 30), i.ToString())){
			//	if(currentSwapID==i) currentSwapID=-1;
			//	else if(currentSwapID==-1) currentSwapID=i;
			//	else SwapResource(i);
			//}
			//if(currentSwapID==i) GUI.color=Color.white;
			
			resourceList[i].icon=(Texture)EditorGUI.ObjectField(new Rect(12+50, 3+75+i*49, 44, 44), resourceList[i].icon, typeof(Texture), false);
			
			resourceList[i].name=EditorGUI.TextField(new Rect(5+120, 15+75+i*49, 150, 20), resourceList[i].name);
			
			if(delete!=i){
				if(GUI.Button(new Rect(window.position.width-35, 12+75+i*49, 25, 25), "X")){
					delete=i;
				}
			}
			else{
				GUI.color = Color.red;
				if(GUI.Button(new Rect(window.position.width-65, 12+75+i*49, 55, 25), "Delete")){
					//if(currentSwapID==i) currentSwapID=-1;
					resourceList.RemoveAt(i);
					delete=-1;
				}
				GUI.color = Color.white;
			}
			
			row+=1;
		}
		
		//~ GUI.Label(new Rect(5, window.position.height-55, window.position.width-10, 55), warning);
		
		
		if (GUI.changed){
			//~ EditorUtility.SetDirty(this);
			Undo.CreateSnapshot();
			Undo.RegisterSnapshot();
		}
		Undo.ClearSnapshotTarget();
		
	}
	
	private int currentSwapID=-1;
	void SwapResource(int ID){
		Resource rsc=resourceList[currentSwapID];
		resourceList[currentSwapID]=resourceList[ID];
		resourceList[ID]=rsc;
		
		currentSwapID=-1;
	}
}
