using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class TowerAvailability{
	public int ID=0;
	public bool enabledInLvl=true;
	
	public TowerAvailability(int id){
		ID=id;
	}
	
	public TowerAvailability Clone(){
		TowerAvailability avai=new TowerAvailability(ID);
		avai.enabledInLvl=enabledInLvl;
		return avai;
	}
}



[AddComponentMenu("TDTK/Managers/BuildManager")]
public class BuildManager : MonoBehaviour {

	[HideInInspector] public List<TowerAvailability> towerAvaiList=new List<TowerAvailability>();
	//[HideInInspector] 
	public UnitTower[] towers;
	//[HideInInspector] 
	private UnitTower[] availableTowers;
	
	static private float _gridSize=0;
	public float gridSize=1.5f;
	public Transform[] platforms;
	[HideInInspector] public PlatformTD[] buildPlatforms;
	
	
	
	public bool AutoAdjustTextureToGrid=true;
	
	public enum _TileIndicatorMode{All, ValidOnly, None}
	public _TileIndicatorMode tileIndicatorMode;
	//~ public bool enableTileIndicator=true;
	
	public bool retainPrefabShaderForSamples=false;
	
	//~ //public int terrainColliderLayer=-1;
	
	static public BuildManager buildManager;
	
	static private BuildableInfo currentBuildInfo;
	
	static private int towerCount=0;
	
	public bool autoSearchForPlatform=false;
	
	public static int PrePlaceTower(UnitTower tower){
		
		LayerMask maskPlatform=1<<LayerManager.LayerPlatform();
		
		Ray ray = Camera.main.ScreenPointToRay(Camera.main.WorldToScreenPoint(tower.thisT.position));
		RaycastHit hit;
		if(Physics.Raycast(ray, out hit, Mathf.Infinity, maskPlatform)){
			
			for(int i=0; i<buildManager.buildPlatforms.Length; i++){
				Transform basePlane=buildManager.platforms[i];
				
				if(hit.transform==basePlane){
					buildManager.buildPlatforms[i].Prebuild(tower.thisT.position, tower);
				}
			}
		}
		
		return towerCount+=1;
	}
	public static int GetTowerCount(){
		return towerCount;
	}
	
	public static bool AddTower(int towerID){
		if(buildManager!=null){
			foreach(UnitTower tower in buildManager.availableTowers){
				if(tower.prefabID==towerID){
					//~ Debug.Log("tower existed");
					return false;
				}
			}
			
			UnitTower newTower=null;
			GameObject prefabObj=Resources.Load("PrefabListTower", typeof(GameObject)) as GameObject;
			if(prefabObj!=null){
				TowerListPrefab prefab=prefabObj.GetComponent<TowerListPrefab>();
				
				if(prefab!=null){
					for(int i=0; i<prefab.towerList.Count; i++){
						if(prefab.towerList[i].prefabID==towerID){
							newTower=prefab.towerList[i];
						}
					}
				}
			}
			
			if(newTower!=null){
				UnitTower[] towerList=new UnitTower[buildManager.availableTowers.Length+1];
				for(int i=0; i<towerList.Length-1; i++){
					//~ towerList[i]=buildManager.towers[i];
					towerList[i]=buildManager.availableTowers[i];
				}
				
				towerList[towerList.Length-1]=newTower;
				
				//~ buildManager.towers=towerList;
				buildManager.availableTowers=towerList;
				
				InitSampleTower(towerList.Length-1);
				
				//~ foreach(PlatformTD platform in platforms){
					//~ platform.AddTower
				//~ }
				
				return true;
			}
			
		}
		return false;
	}
	
	void Awake(){
		
		buildManager=this;
		
		//~ foreach(UnitTower tower in towers){
			//~ tower.thisObj=tower.gameObject;
		//~ }
		
		towerCount=0;
		
		//gridSize=Mathf.Clamp(gridSize, 0.5f, 3.0f);
		gridSize=Mathf.Max(0.25f, gridSize);
		_gridSize=gridSize;
		
		InitTower();
		InitPlatform();
		
	}
	
	public void InitTower(){
		GameObject obj=Resources.Load("PrefabListTower", typeof(GameObject)) as GameObject;
		if(obj!=null){
			TowerListPrefab prefab=obj.GetComponent<TowerListPrefab>();
			
			if(prefab!=null){
				for(int i=0; i<prefab.towerList.Count; i++){
					if(prefab.towerList[i]==null){
						prefab.towerList.RemoveAt(i);
						i-=1;
					}
				}
				
				List<UnitTower> tempList=new List<UnitTower>();
				towers=new UnitTower[prefab.towerList.Count];
				
				for(int i=0; i<prefab.towerList.Count; i++){
					towers[i]=prefab.towerList[i];
				}
				
				if(towerAvaiList.Count>0){
					for(int i=0; i<prefab.towerList.Count; i++){
						UnitTower tower=prefab.towerList[i];
						tower.thisObj=tower.gameObject;
						
						bool match=false;
						foreach(TowerAvailability avai in towerAvaiList){
							if(avai.ID==tower.prefabID){
								match=true;
								if(avai.enabledInLvl && !tempList.Contains(tower)) tempList.Add(tower);
								break;
							}
						}
						if(!match){
							towerAvaiList.Add(new TowerAvailability(tower.prefabID));
							tempList.Add(tower);
						}
					}
					
					availableTowers=new UnitTower[tempList.Count];
					for(int i=0; i<availableTowers.Length; i++){
						availableTowers[i]=tempList[i];
					}
				}
				else{
					availableTowers=new UnitTower[prefab.towerList.Count];
					for(int i=0; i<prefab.towerList.Count; i++){
						availableTowers[i]=prefab.towerList[i];
						towerAvaiList.Add(new TowerAvailability(prefab.towerList[i].prefabID));
					}
				}
			}
		}
		
	}
	
	void UpdateTowerCost(UnitTower tower, int length){
		int[] tempCostList=tower.baseStat.costs;
		
		tower.baseStat.costs=new int[length];
		
		for(int i=0; i<length; i++){
			if(i>=tempCostList.Length){
				tower.baseStat.costs[i]=0;
			}
			else{
				tower.baseStat.costs[i]=tempCostList[i];
			}
		}
		
		for(int j=0; j<tower.upgradeStat.Length; j++){
			int[] tempCostList2=tower.upgradeStat[j].costs;
			
			tower.upgradeStat[j].costs=new int[length];
			
			for(int i=0; i<length; i++){
				if(i>=tempCostList.Length){
					tower.upgradeStat[j].costs[i]=0;
				}
				else{
					tower.upgradeStat[j].costs[i]=tempCostList2[i];
				}
			}
		}
	}
	


	// Use this for initialization
	void InitPlatform() {

		if(autoSearchForPlatform){
			LayerMask mask=1<<LayerManager.LayerPlatform();
			Collider[] platformCols=Physics.OverlapSphere(Vector3.zero, Mathf.Infinity, mask);
			platforms=new Transform[platformCols.Length];
			for(int j=0; j<platformCols.Length; j++){
				platforms[j]=platformCols[j].transform;
			}
		}
		
		buildPlatforms=new PlatformTD[platforms.Length];
		
		int i=0;
		foreach(Transform basePlane in platforms){
			//clear the platform of any unneeded collider
			ClearPlatformColliderRecursively(basePlane);
						
			//if the platform object havent got a platform componet on it, assign it
			PlatformTD platform=basePlane.gameObject.GetComponent<PlatformTD>();
			
			if(platform==null){
				platform=basePlane.gameObject.AddComponent<PlatformTD>();
				//~ platform.buildableType=new _TowerType[7];
				
				//~ //by default, all tower type is builidable
				//~ platform.buildableType[0]=_TowerType.TurretTower;
				//~ platform.buildableType[1]=_TowerType.AOETower;
				//~ platform.buildableType[2]=_TowerType.DirectionalAOETower;
				//~ platform.buildableType[3]=_TowerType.SupportTower;
				//~ platform.buildableType[4]=_TowerType.ResourceTower;
				//~ platform.buildableType[5]=_TowerType.Mine;
				//~ platform.buildableType[6]=_TowerType.Block;
			}
			
			buildPlatforms[i]=platform;
			buildPlatforms[i].InitTowerList(towerAvaiList);
			
			//make sure the plane is perfectly horizontal, rotation around the y-axis is presreved
			basePlane.eulerAngles=new Vector3(0, basePlane.rotation.eulerAngles.y, 0);
			
			//adjusting the scale
			float scaleX=Mathf.Floor(UnitUtility.GetWorldScale(basePlane).x*10/gridSize)*gridSize*0.1f;
			float scaleZ=Mathf.Floor(UnitUtility.GetWorldScale(basePlane).z*10/gridSize)*gridSize*0.1f;
			
			if(scaleX==0) scaleX=gridSize*0.1f;
			if(scaleZ==0) scaleZ=gridSize*0.1f;
			
			basePlane.localScale=new Vector3(scaleX, 1, scaleZ);
			
			//adjusting the texture
			if(AutoAdjustTextureToGrid){
				Material mat=basePlane.renderer.material;
				
				float x=(UnitUtility.GetWorldScale(basePlane).x*10f)/gridSize;
				float z=(UnitUtility.GetWorldScale(basePlane).z*10f)/gridSize;
				
				mat.mainTextureOffset=new Vector2(0.5f, 0.5f);
				mat.mainTextureScale=new Vector2(x, z);
			}
			
			
			//get the platform component, if any
			//Platform p=basePlane.gameObject.GetComponent<Platform>();
			//buildPlatforms[i]=new BuildPlatform(basePlane, p);
			i++;
		}

	}
	
	void ClearPlatformColliderRecursively(Transform t){
		foreach(Transform child in t){
			ClearPlatformColliderRecursively(child);
			Collider col=child.gameObject.GetComponent<Collider>();
			if(col!=null && !col.enabled){
				Destroy(col);
			}
		}
	}
	
	private static GameObject indicator;
	private static GameObject indicator2;
	
	void Start(){
		//~ buildManager=this;
		
		//~ towerCount=0;
		
		//~ gridSize=Mathf.Clamp(gridSize, 0.5f, 3.0f);
		//~ _gridSize=gridSize;
		
		
		if(towers!=null){
			int totalRscCount=GameControl.GetResourceCount();
			foreach(UnitTower tower in towers){
				if(tower.baseStat.costs.Length!=totalRscCount){
					UpdateTowerCost(tower, totalRscCount);
				}
			}
		}
		
		//~ if(buildManager.enableTileIndicator){
		if(tileIndicatorMode!=_TileIndicatorMode.None){
			indicator=GameObject.CreatePrimitive(PrimitiveType.Cube);
			indicator.name="indicator";
			#if UNITY_4_0
				indicator.SetActive(false);
			#else
				indicator.active=false;
			#endif
			indicator.transform.localScale=new Vector3(gridSize, 0.025f, gridSize);
			indicator.transform.renderer.material=(Material)Resources.Load("IndicatorSquare");
			indicator.transform.parent=transform;
			
			indicator2=GameObject.CreatePrimitive(PrimitiveType.Cube);
			indicator2.name="indicator2";
			#if UNITY_4_0
				indicator2.SetActive(false);
			#else
				indicator2.active=false;
			#endif
			indicator2.transform.localScale=new Vector3(gridSize, 0.025f, gridSize);
			indicator2.transform.renderer.material=(Material)Resources.Load("IndicatorSquare");
			indicator2.transform.parent=transform;
			
			Destroy(indicator.collider);
			Destroy(indicator2.collider);
		}
		
		BuildManager.InitiateSampleTowers();
	}
	
	
	void OnEnable(){
		//~ PerkManager.onBuffAllTowerHPE += OnPerkBuffTowerHP;
		
		//~ PerkManager.onUpgradeCostReducE += OnPerkUpgradeCostReduc;
		//~ PerkManager.onBuildCostReducE += OnPerkBuildCostReduc;
		//~ PerkManager.onBuffTowerHPE += OnPerkBuffTowerHP;
		//~ PerkManager.onBuffTowerDefE += OnPerkBuffTowerDef;
		//~ PerkManager.onBuffTowerAttE += OnPerkBuffTowerAtt;
		PerkManager.onBuffTowerRangeE += OnPerkBuffSampleTowerRange;
		//~ PerkManager.onBuffTowerCritChanceE += OnPerkBuffTowerCritChance;
		//~ PerkManager.onBuffTowerCritDamageE += OnPerkBuffTowerCritDamage;
	}
	
	void OnDisable(){
		//~ PerkManager.onBuffAllTowerHPE -= OnPerkBuffTowerHP;
		
		//~ PerkManager.onUpgradeCostReducE -= OnPerkUpgradeCostReduc;
		//~ PerkManager.onBuildCostReducE -= OnPerkBuildCostReduc;
		//~ PerkManager.onBuffAllTowerHPE -= OnPerkBuffTowerHP;
		//~ PerkManager.onBuffTowerDefE -= OnPerkBuffTowerDef;
		//~ PerkManager.onBuffTowerAttE -= OnPerkBuffTowerAtt;
		PerkManager.onBuffTowerRangeE -= OnPerkBuffSampleTowerRange;
		//~ PerkManager.onBuffTowerCritChanceE -= OnPerkBuffTowerCritChance;
		//~ PerkManager.onBuffTowerCritDamageE -= OnPerkBuffTowerCritDamage;
	}
	
	
	void OnPerkBuffSampleTowerRange(Perk perk){
		foreach(UnitTower tower in sampleTower){
			if(tower.prefabID==perk.towerID){
				tower.OnPerkBuffTowerRange(perk);
				break;
			}
		}
	}
	
	
	// Update is called once per frame
	void Update () {
		//~ if(Input.GetMouseButtonDown(1)){
			//~ LayerMask maskPlatform=1<<LayerManager.LayerPlatform();
			//~ Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			//~ RaycastHit hit;
			//~ if(Physics.Raycast(ray, out hit, Mathf.Infinity, maskPlatform)){
				
				//~ for(int i=0; i<buildManager.buildPlatforms.Length; i++){
					
					//~ Transform basePlane=buildManager.buildPlatforms[i].thisT;
					//~ if(hit.transform==basePlane){
						
						//~ //calculating the build center point base on the input position
						//~ Vector3 pos=GetTilePos(basePlane, hit.point);
						
						//~ Node node=buildManager.buildPlatforms[i].GetNearestNode(pos);
						
						//~ DebugDraw.Cross(node.pos, Color.red, 3);
						//~ foreach(Node neighbour in node.neighbourNode){
							//~ DebugDraw.Cross(neighbour.pos, Color.blue, 3);
						//~ }
					//~ }
				//~ }
			//~ }
		//~ }
	}
	
	
	static public void ClearBuildPoint(){
		//Debug.Log("ClearBuildPoint");
		currentBuildInfo=null;
		ClearIndicator();
	}
	
	static public void ClearIndicator(){
		if(indicator!=null){
			Utility.SetActive(indicator, false);
		}
	}
	
	
	static public Vector3 GetTilePos(Transform basePlane, Vector3 hitPos){
		//check if the row count is odd or even number
		float remainderX=UnitUtility.GetWorldScale(basePlane).x*10/_gridSize%2;
		float remainderZ=UnitUtility.GetWorldScale(basePlane).z*10/_gridSize%2;
		
		//get the rotation offset of the plane
		Quaternion rot=Quaternion.LookRotation(hitPos-basePlane.position);
		
		//get the x and z distance from the centre of the plane in the baseplane orientation
		//from this point on all x and z will be in reference to the basePlane orientation
		float dist=Vector3.Distance(hitPos, basePlane.position);
		float distX=Mathf.Sin((rot.eulerAngles.y-basePlane.rotation.eulerAngles.y)*Mathf.Deg2Rad)*dist;
		float distZ=Mathf.Cos((rot.eulerAngles.y-basePlane.rotation.eulerAngles.y)*Mathf.Deg2Rad)*dist;
		
		//get the sign (1/-1) of the x and y direction
		float signX=distX!=0 ? distX/Mathf.Abs(distX) : 1;
		float signZ=distZ!=0 ? distZ/Mathf.Abs(distZ) : 1;
		
		//calculate the tile number selected in z and z direction
		float numX=Mathf.Round((distX+(remainderX-1)*(signX*_gridSize/2))/_gridSize);
		float numZ=Mathf.Round((distZ+(remainderZ-1)*(signZ*_gridSize/2))/_gridSize);
		
		//calculate offset in x-axis, 
		float offsetX=-(remainderX-1)*signX*_gridSize/2;
		float offsetZ=-(remainderZ-1)*signZ*_gridSize/2;
		
		//get the pos and apply the offset
		Vector3 p=basePlane.TransformDirection(new Vector3(numX, 0, numZ)*_gridSize);
		p+=basePlane.TransformDirection(new Vector3(offsetX, 0, offsetZ));
		
		//set the position;
		Vector3 pos=p+basePlane.position;
		
		return pos;
	}
	
	//called to set indicator to a particular node, set the color as well
	//not iOS performance friendly
	static public void SetIndicator(Vector3 pointer){
		
		//~ if(!buildManager.enableTileIndicator) return;
		if(buildManager.tileIndicatorMode==_TileIndicatorMode.None) return ;
		
		//layerMask for platform only
		LayerMask maskPlatform=1<<LayerManager.LayerPlatform();
		//layerMask for detect all collider within buildPoint
		LayerMask maskAll=1<<LayerManager.LayerPlatform();
		int terrainLayer=LayerManager.LayerTerrain();
		if(terrainLayer>=0) maskAll|=1<<terrainLayer;
		
		Ray ray = Camera.main.ScreenPointToRay(pointer);
		RaycastHit hit;
		if(Physics.Raycast(ray, out hit, Mathf.Infinity, maskPlatform)){
			
			for(int i=0; i<buildManager.buildPlatforms.Length; i++){
				
				Transform basePlane=buildManager.buildPlatforms[i].thisT;
				if(hit.transform==basePlane){
					
					//calculating the build center point base on the input position
					Vector3 pos=GetTilePos(basePlane, hit.point);
					
					//Debug.Log(new Vector3(remainderX, 0, remainderZ)+"  "+new Vector3(signX, 0, signZ)+"  "+p+"  "+basePlane.position);
					indicator2.transform.position=pos;
					indicator2.transform.rotation=basePlane.rotation;
					
					Collider[] cols=Physics.OverlapSphere(pos, _gridSize/2*0.9f, ~maskAll);
					if(cols.Length>0){
						if(buildManager.tileIndicatorMode==_TileIndicatorMode.All){
							Utility.SetActive(indicator2, true);
							indicator2.renderer.material.SetColor("_TintColor", Color.red);
						}
						else Utility.SetActive(indicator2, false);
					}
					else{
						Utility.SetActive(indicator2, true);
						indicator2.renderer.material.SetColor("_TintColor", Color.green);
					}
				}
			}
		}
		else{
			Utility.SetActive(indicator2, false);
		}
	}
	
	
	//~ static public bool CheckBuildPoint(Vector3 pointer){
		//~ return CheckBuildPoint(pointer, 0);
	//~ }
	//~ static public bool CheckBuildPoint(Vector3 pointer, int footprint){
		//~ footprint=-1;
		//~ //if(currentBuildInfo!=null) return false;
		
		//~ BuildableInfo buildableInfo=new BuildableInfo();
		
		//~ //layerMask for platform only
		//~ LayerMask maskPlatform=1<<LayerManager.LayerPlatform();
		//~ //layerMask for detect all collider within buildPoint
		//~ LayerMask maskAll=1<<LayerManager.LayerPlatform();
		//~ int terrainLayer=LayerManager.LayerTerrain();
		//~ if(terrainLayer>=0) maskAll|=1<<terrainLayer;
		
		//~ Ray ray = Camera.main.ScreenPointToRay(pointer);
		//~ RaycastHit hit;
		//~ if(Physics.Raycast(ray, out hit, Mathf.Infinity, maskPlatform)){
			
			//~ for(int i=0; i<buildManager.buildPlatforms.Length; i++){
				//~ Transform basePlane=buildManager.platforms[i];
				
				//~ if(hit.transform==basePlane){
					//~ //calculating the build center point base on the input position
					//~ Vector3 pos=GetTilePos(basePlane, hit.point);
					
					//~ //check if the position is blocked, by any other obstabcle other than the baseplane itself
					//~ Collider[] cols=Physics.OverlapSphere(pos, _gridSize/2*0.9f+footprint*_gridSize, ~maskAll);
					//~ if(cols.Length>0){
						//~ //Debug.Log("something's in the way "+cols[0]);
						//~ return false;
					//~ }
					//~ else{
						//~ //confirm that we can build here
						//~ buildableInfo.buildable=true;
						//~ buildableInfo.position=pos;
						
						//~ buildableInfo.platform=buildManager.buildPlatforms[i];
						//~ //Debug.Log(buildableInfo.platform+" !!!  "+buildManager.buildPlatforms[i]);
					//~ }
					
					//~ //check if the platform is walkable, if so, check if building on the point wont block all possible path
					//~ if(buildManager.buildPlatforms[i].IsWalkable()){
						//~ //return true is the platform is not block
						//~ if(buildManager.buildPlatforms[i].CheckForBlock(pos, footprint)){
							//~ //Debug.Log("all path is blocked "+Time.time);
							//~ return false;
						//~ }
					//~ }

					//~ //buildableType has obsolete
					//~ //buildableInfo.buildableType=buildManager.buildPlatforms[i].buildableType;
					//~ //buildableInfo.specialBuildableID=buildManager.buildPlatforms[i].specialBuildableID;
					//~ buildableInfo.towerAvaiList=buildManager.buildPlatforms[i].towerAvaiList;
					
					//~ break;
				//~ }
				
			//~ }

		//~ }
		//~ else return false;
		
		//~ currentBuildInfo=buildableInfo;
		
		//~ if(buildManager.tileIndicatorMode!=_TileIndicatorMode.None){
			//~ Utility.SetActive(indicator, true);
			//~ indicator.transform.position=currentBuildInfo.position;
			//~ if(currentBuildInfo.platform!=null)
			//~ indicator.transform.rotation=currentBuildInfo.platform.thisT.rotation;
		//~ }
			
		//~ return true;
	//~ }
	
	
	public static _TileStatus CheckBuildPoint(Vector3 pointer){
		return CheckBuildPoint(pointer, 0);
	}
	public static _TileStatus CheckBuildPoint(Vector3 pointer, int footprint){
		footprint=-1;
		//if(currentBuildInfo!=null) return false;
		
		_TileStatus status=_TileStatus.Available;
		BuildableInfo buildableInfo=new BuildableInfo();
		
		//layerMask for platform only
		LayerMask maskPlatform=1<<LayerManager.LayerPlatform();
		//layerMask for detect all collider within buildPoint
		LayerMask maskAll=1<<LayerManager.LayerPlatform();
		int terrainLayer=LayerManager.LayerTerrain();
		if(terrainLayer>=0) maskAll|=1<<terrainLayer;
		
		Ray ray = Camera.main.ScreenPointToRay(pointer);
		RaycastHit hit;
		if(Physics.Raycast(ray, out hit, Mathf.Infinity, maskPlatform)){
			
			for(int i=0; i<buildManager.buildPlatforms.Length; i++){
				Transform basePlane=buildManager.platforms[i];
				
				if(hit.transform==basePlane){
					//calculating the build center point base on the input position
					Vector3 pos=GetTilePos(basePlane, hit.point);
					
					//check if the position is blocked, by any other obstabcle other than the baseplane itself
					Collider[] cols=Physics.OverlapSphere(pos, _gridSize/2*0.9f+footprint*_gridSize, ~maskAll);
					if(cols.Length>0){
						//Debug.Log("something's in the way "+cols[0]);
						return _TileStatus.Unavailable;
					}
					else{
						//confirm that we can build here
						buildableInfo.buildable=true;
						buildableInfo.position=pos;
						
						buildableInfo.platform=buildManager.buildPlatforms[i];
						//Debug.Log(buildableInfo.platform+" !!!  "+buildManager.buildPlatforms[i]);
					}
					
					//check if the platform is walkable, if so, check if building on the point wont block all possible path
					if(buildManager.buildPlatforms[i].IsWalkable()){
						//return true is the platform is not block
						if(buildManager.buildPlatforms[i].CheckForBlock(pos, footprint)){
							//Debug.Log("all path is blocked "+Time.time);
							status=_TileStatus.Blocked;
						}
					}

					//~ buildableInfo.buildableType=buildManager.buildPlatforms[i].buildableType;
					//~ buildableInfo.specialBuildableID=buildManager.buildPlatforms[i].specialBuildableID;
					
					if(status==_TileStatus.Blocked){
						List<TowerAvailability> tempList=new List<TowerAvailability>();
						for(int n=0; n<buildManager.buildPlatforms[i].towerAvaiList.Count; n++){
							UnitTower tower=GetTower(buildManager.buildPlatforms[i].towerAvaiList[n].ID);
							if(tower.type==_TowerType.Mine){
								tempList.Add(buildManager.buildPlatforms[i].towerAvaiList[n]);
							}
						}
						buildableInfo.towerAvaiList=tempList;
					}
					else{
						buildableInfo.towerAvaiList=buildManager.buildPlatforms[i].towerAvaiList;
					}
					
					break;
				}
				
			}

		}
		else return _TileStatus.NoPlatform;
		
		currentBuildInfo=buildableInfo;
		
		if(buildManager.tileIndicatorMode!=_TileIndicatorMode.None){
			Utility.SetActive(indicator, true);
			indicator.transform.position=currentBuildInfo.position;
			if(currentBuildInfo.platform!=null)
			indicator.transform.rotation=currentBuildInfo.platform.thisT.rotation;
		}
			
		return status;
	}
	
	
	
	//similar to CheckBuildPoint but called by UnitTower in DragNDrop mode, check tower type before return
	public static bool CheckBuildPoint(Vector3 pointer, int footprint, int ID){
		//~ Debug.Log(footprint);
		_TileStatus status=CheckBuildPoint(pointer, footprint);
		if(status==_TileStatus.NoPlatform || status==_TileStatus.Unavailable){
			return false;
		}
		
		UnitTower tower=GetTower(ID);
		if(status==_TileStatus.Blocked && tower.type!=_TowerType.Mine) return false;
		
		if(ID>=0){
			foreach(TowerAvailability avai in currentBuildInfo.towerAvaiList){
				if(avai.ID==ID){
					if(avai.enabledInLvl){
						return true;
					}
					else{
						return false;
					}
					//break;
				}
			}
		}
		
		currentBuildInfo.buildable=false;
		return false;
	}
	
	public static UnitTower GetTower(int ID){
		foreach(UnitTower tower in buildManager.towers){
			if(tower.prefabID==ID){
				return tower;
			}
		}
		
		return null;
	}
	
	//called when a tower building is initated in DragNDrop, instantiate the tower and set it in DragNDrop mode
	public static string BuildTowerDragNDrop(UnitTower tower){
		
		if(tower.type==_TowerType.ResourceTower && GameControl.gameState==_GameState.Idle){
			//GameMessage.DisplayMessage("Cant Build Tower before spawn start");
			return "Cant Build Tower before spawn start"; 
		}
		
		if(GameControl.HaveSufficientResource(tower.GetCost())){
			//~ Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			//~ Vector3 pos=ray.GetPoint(10000);
			
			//~ GameObject towerObj=(GameObject)Instantiate(tower.thisObj, pos, Quaternion.identity);
			//~ UnitTower towerCom=towerObj.GetComponent<UnitTower>();
			
			//~ towerCom.StartCoroutine(towerCom.DragNDropRoutine(buildManager.dragNDropStatusFlicker));
			
			int ID=0;
			for(int i=0; i<buildManager.availableTowers.Length; i++){
				if(buildManager.availableTowers[i]==tower){
					ID=i;
					break;
				}
			}
			#if UNITY_4_0
				buildManager.sampleTower[ID].thisObj.SetActive(true);
			#else
				buildManager.sampleTower[ID].thisObj.SetActiveRecursively(true);
			#endif
			GameControl.ShowIndicator(buildManager.sampleTower[ID]);
			UnitTower towerCom=buildManager.sampleTower[ID];
			towerCom.StartCoroutine(towerCom.DragNDropRoutine(!buildManager.retainPrefabShaderForSamples));
			
			return "";
		}
		
		//GameMessage.DisplayMessage("Insufficient Resource");
		return "Insufficient Resource";
	}
	
	public static string DragNDropBuilt(UnitTower tower){
		
		//~ if(currentBuildInfo.platform!=null){
			//~ tower.SetTowerID(towerCount+=1);
			//~ if(tower.type!=_TowerType.Mine)
				//~ currentBuildInfo.platform.Build(currentBuildInfo.position, tower);
			//~ ClearBuildPoint();
			//~ return "";
		//~ }
		
			//~ int ID=0;
			//~ for(int i=0; i<buildManager.towers.Length; i++){
				//~ if(buildManager.sampleTower[i]==tower){
					//~ ID=i;
					//~ break;
				//~ }
			//~ }
			
			int ID=0;
			for(int i=0; i<buildManager.availableTowers.Length; i++){
				if(buildManager.availableTowers[i].prefabID==tower.prefabID){
					ID=i;
					break;
				}
			}
			
			BuildManager.ClearSampleTower();
			
			//~ return BuildTowerPointNBuild(buildManager.towers[ID]);
			return BuildTowerPointNBuild(buildManager.availableTowers[ID]);
		
		//~ return "Invalid Build Point";
	}
	
	//called by any external component to build tower, uses currentBuildInfo, return false if there isnt one
	public static string BuildTowerPointNBuild(UnitTower tower){
		if(currentBuildInfo==null) return "Select a Build Point First";
		
		return BuildTowerPointNBuild(tower, currentBuildInfo.position, currentBuildInfo.platform);
	}
	
	//called by any external component to build tower
	public static string BuildTowerPointNBuild(UnitTower tower, Vector3 pos, PlatformTD platform){
		
		//dont allow building of resource tower before game started
		if(tower.type==_TowerType.ResourceTower && GameControl.gameState==_GameState.Idle){
			//GameMessage.DisplayMessage("Cant Build Tower before spawn start");
			return "Cant Build Tower before spawn start"; 
		}
		
		
		//type defined buildable check, obsolete
		//bool matched=false;
		//foreach(_TowerType type in platform.buildableType){
		//	if(tower.type==type){
		//		matched=true;
		//		break;
		//	}
		//}
		//if(!matched) return "Invalid Tower Type"; 
		
		//check if there are sufficient resource
		int[] cost=tower.GetCost();
		if(GameControl.HaveSufficientResource(cost)){
			GameControl.SpendResource(cost);
			
			GameObject towerObj=(GameObject)Instantiate(tower.thisObj, pos, platform.thisT.rotation);
			UnitTower towerCom=towerObj.GetComponent<UnitTower>();
			towerCom.InitTower(towerCount+=1);
			
			//register the tower to the platform
			if(platform!=null) platform.Build(pos, towerCom);
			
			//~ if(tower.type!=_TowerType.Mine)
				//~ currentBuildInfo.platform.Build(currentBuildInfo.position, tower);
			
			//clear the build info and indicator for build manager
			ClearBuildPoint();
			
			return "";
		}
		
		//GameMessage.DisplayMessage("Insufficient Resource");
		return "Insufficient Resource";
	}
	
	
	private List<UnitTower> sampleTower=new List<UnitTower>();
	private int currentSampleID=-1;
	public static void InitiateSampleTowers(){
		//~ buildManager.sampleTower=new UnitTower[buildManager.towers.Length];
		buildManager.sampleTower=new List<UnitTower>();//[buildManager.towers.Length];
		for(int i=0; i<buildManager.availableTowers.Length; i++){
			InitSampleTower(i);
		}
	}
	
	public static void InitSampleTower(int ID){
		GameObject towerObj=(GameObject)Instantiate(buildManager.availableTowers[ID].gameObject);
		buildManager.sampleTower.Add(towerObj.GetComponent<UnitTower>());
		buildManager.sampleTower[buildManager.sampleTower.Count-1].InitPerkParameters();
		
		towerObj.transform.parent=buildManager.transform;
		
		if(!buildManager.retainPrefabShaderForSamples) UnitUtility.SetMat2AdditiveRecursively(buildManager.sampleTower[ID].thisT);
		
		if(towerObj.collider!=null) Destroy(towerObj.collider);
		UnitUtility.DestroyColliderRecursively(towerObj.transform);
		//~ #if UNITY_4_0
			//~ towerObj.SetActive(false);
		//~ #else
			//~ towerObj.SetActiveRecursively(false);
		//~ #endif
		Utility.SetActive(towerObj, false);
	}
	
	static public void ShowSampleTower(int ID){
		buildManager._ShowSampleTower(ID);
	}
	public void _ShowSampleTower(int ID){
		if(currentSampleID==ID || currentBuildInfo==null) return;
		
		if(currentSampleID>=0){
			ClearSampleTower();
		}
		
		bool matched=false;
		//~ foreach(_TowerType type in currentBuildInfo.buildableType){
			//~ if(type==sampleTower[ID].type){
				//~ matched=true;
				//~ break;
			//~ }
		//~ }
		foreach(TowerAvailability avai in currentBuildInfo.towerAvaiList){
			if(avai.ID==sampleTower[ID].prefabID){
				if(avai.enabledInLvl) matched=true;
				break;
			}
		}
		
		if(!retainPrefabShaderForSamples){
			if(matched) UnitUtility.SetAdditiveMatColorRecursively(sampleTower[ID].thisT, Color.green);
			else UnitUtility.SetAdditiveMatColorRecursively(sampleTower[ID].thisT, Color.red);
		}
		
		currentSampleID=ID;
		sampleTower[ID].thisT.position=currentBuildInfo.position;
		//sampleTower[ID].thisObj.SetActiveRecursively(true);
		//~ #if UNITY_4_0
			//~ sampleTower[ID].thisObj.SetActive(true);
		//~ #else
			//~ sampleTower[ID].thisObj.SetActiveRecursively(true);
		//~ #endif
		Utility.SetActive(sampleTower[ID].thisObj, true);
		GameControl.ShowIndicator(sampleTower[ID]);
	}
	
	static public void ClearSampleTower(){
		buildManager._ClearSampleTower();
	}
	public void _ClearSampleTower(){
		if(currentSampleID<0) return;
		
		//sampleTower[currentSampleID].thisObj.SetActiveRecursively(false);
		//~ #if UNITY_4_0
			//~ sampleTower[currentSampleID].thisObj.SetActive(false);
		//~ #else
			//~ sampleTower[currentSampleID].thisObj.SetActiveRecursively(false);
		//~ #endif
		Utility.SetActive(sampleTower[currentSampleID].thisObj, false);
		GameControl.ClearIndicator();
		currentSampleID=-1;
	}
	
	
	static public BuildableInfo GetBuildInfo(){
		return currentBuildInfo;
	}
	
	static public UnitTower[] GetTowerList(){
		//~ return buildManager.towers;
		return buildManager.availableTowers;
	}
	
	static public UnitTower[] GetAvailableTowerList(){
		return buildManager.availableTowers;
	}
	
	static public float GetGridSize(){
		return _gridSize;
	}
	
	Vector3 poss;
	//public bool debugSelectPos=true;
	void OnDrawGizmos(){
		
		//if(debugSelectPos) Gizmos.DrawCube(SelectBuildPos(Input.mousePosition), new Vector3(gridSize, 0, gridSize));
		
	}
	
}



public enum _TileStatus{NoPlatform, Available, Unavailable, Blocked}

[System.Serializable]
public class BuildableInfo{
	public bool buildable=false;
	public Vector3 position=Vector3.zero;
	public PlatformTD platform;
	//~ public _TowerType[] buildableType=null;
	//public GameObject[] buildableTower=null;
	
	//~ public int[] specialBuildableID;
	
	public List<TowerAvailability> towerAvaiList=new List<TowerAvailability>();
	
	//cant build
	public void BuildSpotInto(){}
	
	//can build anything
	public void BuildSpotInto(Vector3 pos){
		position=pos;
	}
	
	//can build with restriction to certain tower type
	public void BuildSpotInto(Vector3 pos, List<TowerAvailability> list){
		position=pos;
		towerAvaiList=list;
	}
	
	//~ public void BuildSpotInto(Vector3 pos, _TowerType[] bT){
		//~ position=pos;
		//~ buildableType=bT;
	//~ }
}