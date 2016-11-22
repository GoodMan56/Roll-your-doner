using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;
using System.Xml;
using System.IO;


[System.Serializable]
public class Resource{
	[HideInInspector] public int ID=0;
	
	public string name="Resource";
	public Texture icon=null;
	
	public int value=0;
	
	public bool CheckMatch(Resource target){
		if(target!=null && name==target.name && icon==target.icon){
			return true;
		}
		return false;
	}
}


public class ResourceManager : MonoBehaviour {

	public Resource[] resources=new Resource[1];
	
	static ResourceManager resourceManager;
	
	public static float gainModifier=1;
	public static float buildModifier=1;
	public static float upgradeModifier=1;
	
	public void Init(){
		resourceManager=this;
		InitResources();
	}
	
	public void InitResources(){
		List<Resource> tempRscList=Load();
		
		if(tempRscList.Count!=resources.Length){
			for(int i=0; i<tempRscList.Count; i++){
				for(int j=0; j<resources.Length; j++){
					if(tempRscList[i].CheckMatch(resources[j])){
						tempRscList[i].value=resources[j].value;
						break;
					}
				}
			}
			resources=new Resource[tempRscList.Count];
			for(int i=0; i<resources.Length; i++){
				resources[i]=tempRscList[i];
			}
		}
		else{
			for(int i=0; i<tempRscList.Count; i++){
				if(!tempRscList[i].CheckMatch(resources[i])){
					resources[i].name=tempRscList[i].name;
					resources[i].icon=tempRscList[i].icon;
				}
			}
		}
	}
	
	// Use this for initialization
	void Start () {
		
	}
	
	static public List<Resource> Load(){
		XmlDocument xmlDoc = new XmlDocument();
		List<Resource> resourceList=new List<Resource>();
		
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
					
					/*
					resourceList.Add(new Resource());
					resourceList[n].name=rootNode.ChildNodes[n].Attributes[0].Value;
					
					string icon=rootNode.ChildNodes[n].Attributes[1].Value;
					resourceList[n].icon=Resources.Load("RscIcons/"+icon, typeof(Texture)) as Texture;
					//string icon=rootNode.ChildNodes[n].Attributes[1].Value;
					//resourceList[n].icon=Resources.Load(icon, typeof(Texture)) as Texture;
					*/
				}
			}
		}
		
		return resourceList;
	}
	
	//~ static public List<Resource> Load(){
		//~ XmlDocument xmlDoc = new XmlDocument();
		//~ List<Resource> resourceList=new List<Resource>();
		
		//~ TextAsset rscTextAsset=Resources.Load("Resource", typeof(TextAsset)) as TextAsset;
		//~ if(rscTextAsset!=null){
			//~ xmlDoc.Load(new MemoryStream(rscTextAsset.bytes));
			//~ XmlNode rootNode = xmlDoc.FirstChild;
			//~ if (rootNode.Name == "something"){
				//~ int rscCount=rootNode.ChildNodes.Count;
				//~ for(int n=0; n<rscCount; n++){
					//~ resourceList.Add(new Resource());
					//~ resourceList[n].name=rootNode.ChildNodes[n].Attributes[0].Value;
					
					//~ //string iconName=rootNode.ChildNodes[n].Attributes[1].Value;
					//~ //resourceList[n].icon=Resources.Load(iconName, typeof(Texture)) as Texture;
					//~ //resourceList[n].icon=AssetDatabase.LoadAssetAtPath(iconPath, typeof(Texture2D)) as Texture2D;
				//~ }
			//~ }
		//~ }
		
		//~ return resourceList;
	//~ }
	
	// Update is called once per frame
	void Update () {
	
	}
	
	
	public static void GainResource(int val){
		resourceManager._GainResource(0, val);
	}
	
	public static void GainResource(int id, int val){
		resourceManager._GainResource(id, val);
	}
	
	void _GainResource(int id, int val){
		if(resources.Length>id){
			resources[id].value=Mathf.Max(0, resources[id].value+=val);
		}
		else Debug.Log("resource type unconfigured");
	}
	
	public static void GainResource(int[] val){
		resourceManager._GainResource(val);
	}
	
	void _GainResource(int[] val){
		for(int i=0; i<val.Length; i++){
			if(i>=resources.Length){
				Debug.Log("resource gain contain unconfigured resource type");
				return;
			}
			else {
				resources[i].value+=val[i];
			}
		}
	}
	
	public static void SpendResource(int[] val){
		resourceManager._SpendResource(val);
	}
	
	void _SpendResource(int[] val){
		for(int i=0; i<val.Length; i++){
			if(i>=resources.Length){
				Debug.Log("costs contain unconfigured resource type");
				return;
			}
			else {
				resources[i].value-=val[i];
				if(resources[i].value<0) resources[i].value=0;
			}
		}
	}
	
	
	public static int GetResourceVal(){
		return resourceManager._GetResource(0);
	}
	
	public static int GetResourceVal(int id){
		return resourceManager._GetResource(id);
	}
	
	int _GetResource(int id){
		if(resources.Length>id){
			return resources[id].value;
		}
		
		return 0;
	}
	
	public static Resource[] GetResourceList(){
		return resourceManager.resources;
	}

	
	public static bool HaveSufficientResource(int[] cost){
		return resourceManager._HaveSufficientResource(cost);
	}
	
	bool _HaveSufficientResource(int[] cost){
		for(int i=0; i<cost.Length; i++){
			if(i>=resources.Length){
				Debug.Log("costs contain unconfigured resource type");
				return false;
			}
			else if(resources[i].value<cost[i]){
				Debug.Log("insufficient resource");
				return false;
			}
		}
		
		return true;
	}
	
	public static int GetResourceCount(){
		return resourceManager.resources.Length;
	}
	

	
}



