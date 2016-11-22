using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;


public class TowerEditor : EditorWindow {
    
	//int levelCap=1;
	
	[SerializeField] static string[] nameList=new string[0];
	
	[SerializeField] static UnitTower[] towerList=new UnitTower[0];
	
	private static List<Resource> rscList=new List<Resource>();
	
	
	static UnitTower tower;
	
	static int index=0;
	//~ int towerType=0;
	//~ int towerTargetMode=0;
	//~ int towerTargetPriority=0;
	//~ int towerTargetArea=0;
	//~ int turretAnimateMode=0;
	//~ int turretRotationMode=0;
	//~ int dotType=1;
	
	static string[] towerTypeLabel=new string[7];
	static string[] towerTargetModeLabel=new string[3];
	static string[] towerTargetPriorityLabel=new string[5];
	static string[] towerTargetAreaLabel=new string[3];
	static string[] turretAnimateModeLabel=new string[3];
	static string[] turretRotationModeLabel=new string[2];
	static string[] turretLOSModeLabel=new string[2];
	static string[] dotTypeLabel=new string[5];
	
	private static string[] turretAnimateModeTooltip=new string[3];
	private static string[] turretRotationModeTooltip=new string[2];
	private static string[] turretLOSModeTooltip=new string[2];
	
	GUIContent cont;
	GUIContent[] contL;
    
	private static bool[] indicatorFlags=new bool[1];
	
	private bool showAnimationList=false;
	private string showAnimationText="Show animation configuration";
	
	private bool showSoundList=false;
	private string showSoundText="Show sfx list";
	
	static private TowerEditor window;
	
    // Add menu named "TowerEditor" to the Window menu
    //[MenuItem ("TDTK/TowerEditor")]
    public static void Init () {
        // Get existing open window or if none, make a new one:
        window = (TowerEditor)EditorWindow.GetWindow(typeof (TowerEditor));
		window.minSize=new Vector2(700, 650);
		//~ window.maxSize=new Vector2(750, 651);
		
		indicatorFlags[0]=true;
		
		rscList=ResourceEditorWindow.Load();
		
		GetTower();
		
		towerTypeLabel[0]="Turret Tower";
		towerTypeLabel[1]="AOE Tower";
		towerTypeLabel[2]="Directional AOE Tower";
		towerTypeLabel[3]="Support Tower";
		towerTypeLabel[4]="Resource Tower";
		towerTypeLabel[5]="Mine";
		towerTypeLabel[6]="Block";
		
		towerTargetModeLabel[0]="Hybrid";
		towerTargetModeLabel[1]="Air";
		towerTargetModeLabel[2]="Ground";
		
		towerTargetPriorityLabel[0]="Nearest";
		towerTargetPriorityLabel[1]="Weakest";
		towerTargetPriorityLabel[2]="Toughest";
		towerTargetPriorityLabel[3]="SpawnOrderFirst";
		towerTargetPriorityLabel[4]="Random";
		
		towerTargetAreaLabel[0]="AllAround";
		towerTargetAreaLabel[1]="DirectionalCone";
		towerTargetAreaLabel[2]="StraightLine";
		
		turretAnimateModeLabel[0]="Full";
		turretAnimateModeLabel[1]="Y-Axis Only";
		turretAnimateModeLabel[2]="None";
		turretAnimateModeTooltip[0]="Rotate the turret to aim for target in both x and y axis";
		turretAnimateModeTooltip[1]="Rotate the turret to aim for target in y axis only";
		turretAnimateModeTooltip[2]="Dont Rotate turret at all";
		
		turretRotationModeLabel[0]="FullTurret";
		turretRotationModeLabel[1]="SeparatedBarrel";
		turretRotationModeTooltip[0]="Rotate turret along with the barrel in both x and y-axis";
		turretRotationModeTooltip[1]="Rotate the turret on y-axis and the barrel in x-axis";
		
		turretLOSModeLabel[0]="AimOnly";
		turretLOSModeLabel[1]="Realistic";
		turretLOSModeTooltip[0]="The turret will fire as long as the barrel is align to the target";
		turretLOSModeTooltip[1]="The turret will need to be aligned to the target and there has to be no obstalce in the way before the turret will fire";
		
		//~ turretAnimateModeLabel[0]="Full";
		//~ turretAnimateModeLabel[1]="Y-Axis Only";
		//~ turretAnimateModeLabel[2]="None";
		
		//~ turretRotationModeLabel[0]="FullTurret";
		//~ turretRotationModeLabel[1]="SeparatedBarrel";
		
		//~ turretLOSModeLabel[0]="AimOnly";
		//~ turretLOSModeLabel[1]="Realistic";
		
		dotTypeLabel[0]="FlatRate";
		dotTypeLabel[1]="Linear";
		dotTypeLabel[2]="Exponential";
		dotTypeLabel[3]="InverseLinear";
		dotTypeLabel[4]="InverseExponential";
		
    }
    
	
	static BuildManager buildManager;
	//~ static ResourceManager rscManager;
	
	static void GetTower(){
		towerList=new UnitTower[0];
		nameList=new string[0];
		
		//~ buildManager=(BuildManager)FindObjectOfType(typeof(BuildManager));
		
		//~ if(buildManager!=null){
			//~ towerList=buildManager.towers;
			
			//~ nameList=new string[towerList.Length];
			//~ for(int i=0; i<towerList.Length; i++){
				//~ nameList[i]=towerList[i].name;
			//~ }
		//~ }
		//~ else{
			//~ towerList=new UnitTower[0];
			//~ nameList=new string[0];
		//~ }
		
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
				
				towerList=new UnitTower[prefab.towerList.Count];
				nameList=new string[prefab.towerList.Count];
				
				for(int i=0; i<prefab.towerList.Count; i++){
					if(prefab.towerList[i]!=null){
						towerList[i]=prefab.towerList[i];
						nameList[i]=prefab.towerList[i].unitName;
					}
				}
			}
		}
		
		index=Mathf.Clamp(index, 0, towerList.Length-1);
		tower=towerList[index];
		UpdateIndicatorFlags(tower);
		
		//~ rscManager=(ResourceManager)FindObjectOfType(typeof(ResourceManager));
	}
	
	void UpdateResourceList(){
		rscList=ResourceEditorWindow.Load();
		Repaint();
	}
	
	void OnEnable(){
		ResourceEditorWindow.onResourceUpdateE+=UpdateResourceList;
		TowerManager.onTowerUpdateE+=GetTower;
	}
	
	void OnDisable(){
		ResourceEditorWindow.onResourceUpdateE-=UpdateResourceList;
		TowerManager.onTowerUpdateE-=GetTower;
	}
	
	
	float startX, startY, height, spaceY, lW;
	
	float statBoxStartY;
	float statBoxHeight;
	float contentHeight;
	
	int rscCount=1;
	
	private Vector2 scrollPos;
	
    void OnGUI () {
		if(window==null) Init();
		
		Rect visibleRect=new Rect(0, 0, window.position.width, window.position.height);
		Rect contentRect=new Rect(0, 0, Mathf.Max(window.position.width-15, 610+(tower.levelCap-3)*180), contentHeight);
		//~ scrollPos = GUI.BeginScrollView(visibleRect, scrollPos, new Rect(0, 0, Mathf.Max(window.position.width, 610+(levelCap-3)*180), 1410));
		scrollPos = GUI.BeginScrollView(visibleRect, scrollPos, contentRect);
		
		GUI.changed = false;
		
		
		{
			startX=3;
			startY=3;
			height=18;
			spaceY=height+startX;
			
			lW=100;	//label width, the offset from label to the editable field
			
			if(towerList.Length>0) {
				GUI.SetNextControlName ("TowerSelect");
				index = EditorGUI.Popup(new Rect(startX, startY, 300, height), "Tower:", index, nameList);
				if(GUI.changed){
					tower=towerList[index];
					GUI.FocusControl ("TowerSelect");
					UpdateIndicatorFlags(tower);
				}
				
				
				EditorGUI.LabelField(new Rect(320+startX, startY, 200, height), "LevelCap: ");
				if(GUI.Button(new Rect(385+startX, startY, 50, height), "-1")){
					if(tower.levelCap>1){
						tower.levelCap-=1;
						UpdateIndicatorFlags(tower);
						tower.UpdateTowerUpgradeStat(tower.levelCap-1);
					}
				}
				if(GUI.Button(new Rect(440+startX, startY, 50, height), "+1")){
					if(tower.type!=_TowerType.Block && tower.type!=_TowerType.Mine){
						tower.levelCap+=1;
						UpdateIndicatorFlags(tower);
						tower.UpdateTowerUpgradeStat(tower.levelCap-1);
					}
				}
				
				
				
				
				tower.unitName = EditorGUI.TextField(new Rect(startX, startY+=30, 300, height-3), "TowerName:", tower.unitName);
				
				EditorGUI.LabelField(new Rect(startX+320, startY, 70, height), "Icon: ");
				tower.icon=(Texture)EditorGUI.ObjectField(new Rect(startX+360, startY, 70, 70), tower.icon, typeof(Texture), false);
				
				int towerType=(int)tower.type;
				towerType = EditorGUI.Popup(new Rect(startX, startY+=20, 300, 15), "TowerType:", towerType, towerTypeLabel);
				tower.type=(_TowerType)towerType;
				
				
				if(tower.type==_TowerType.TurretTower || tower.type==_TowerType.DirectionalAOETower || tower.type==_TowerType.AOETower){
					int towerTargetMode=(int)tower.targetMode;
					towerTargetMode = EditorGUI.Popup(new Rect(startX, startY+=20, 300, 15), "TargetingMode:", towerTargetMode, towerTargetModeLabel);
					tower.targetMode=(_TargetMode)towerTargetMode;
				}
				else startY+=20;
				
				{
					tower.armorType=EditorGUI.IntField(new Rect(startX, startY+=20, 300, height-3), "ArmorType:", tower.armorType);
					if(tower.type!=_TowerType.SupportTower && tower.type!=_TowerType.ResourceTower && tower.type!=_TowerType.Block){
						tower.damageType=EditorGUI.IntField(new Rect(startX, startY+=20, 300, height-3), "DamageType:", tower.damageType);
					}
					else startY+=20;
				}
				
				
				if(tower.type==_TowerType.TurretTower || tower.type==_TowerType.DirectionalAOETower){
					int towerTargetArea=(int)tower.targetingArea;
					towerTargetArea = EditorGUI.Popup(new Rect(startX, startY+=20, 300, 15), "TargetingArea:", towerTargetArea, towerTargetAreaLabel);
					tower.targetingArea=(_TargetingArea)towerTargetArea;
					
					if(tower.targetingArea!=_TargetingArea.AllAround){
						tower.matchTowerDir2TargetDir=EditorGUI.Toggle(new Rect(startX+205, startY+20-1, 300, height-3), tower.matchTowerDir2TargetDir);
						EditorGUI.LabelField(new Rect(startX+220, startY+20-1, 300, height-3), "FaceDirection");
						tower.targetingDirection = EditorGUI.FloatField(new Rect(startX, startY+=20, 195, height-3), "TargetingDirection:", tower.targetingDirection);
					}
					else startY+=20;
				}
				else startY+=40;
				
				
				if(tower.targetingArea==_TargetingArea.DirectionalCone){
					tower.targetingFOV = EditorGUI.FloatField(new Rect(startX, startY+=20, 300, height-3), "TargetingFOV:", tower.targetingFOV);
				}
				else startY+=20;
				
				if(tower.type==_TowerType.TurretTower || tower.type==_TowerType.DirectionalAOETower){
					if(tower.targetingArea!=_TargetingArea.StraightLine){
						int towerTargetPriority=(int)tower.targetPriority;
						towerTargetPriority = EditorGUI.Popup(new Rect(startX, startY+=20, 300, 15), "TargetingPriority:", towerTargetPriority, towerTargetPriorityLabel);
						tower.targetPriority=(_TargetPriority)towerTargetPriority;
					}
				}
				else startY+=20;
				
				if(tower.type==_TowerType.TurretTower || tower.type==_TowerType.DirectionalAOETower){
					int turretAnimateMode=(int)tower.animateTurret;
					cont=new GUIContent("TurretAnimateMode:", "How turret rotation behaves in reaction to target");
					contL=new GUIContent[turretAnimateModeLabel.Length];
					for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(turretAnimateModeLabel[i], turretAnimateModeTooltip[i]);
					turretAnimateMode = EditorGUI.Popup(new Rect(startX, startY+=20, 300, 15), cont, turretAnimateMode, contL);
					//~ turretAnimateMode = EditorGUI.Popup(new Rect(startX, startY+=20, 300, 15), "TurretAnimateMode:", turretAnimateMode, turretAnimateModeLabel);
					tower.animateTurret=(_TurretAni)turretAnimateMode;
					
					if(turretAnimateMode==0){
						int turretRotationMode=(int)tower.turretRotationModel;
						cont=new GUIContent("TurretRotationMode:", "The rotation mode of turret with respect to barrel");
						contL=new GUIContent[turretRotationModeLabel.Length];
						for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(turretRotationModeLabel[i], turretRotationModeTooltip[i]);
						turretRotationMode = EditorGUI.Popup(new Rect(startX, startY+=20, 300, 15), cont, turretRotationMode, contL);
						//~ turretRotationMode = EditorGUI.Popup(new Rect(startX, startY+=20, 300, 15), "TurretRotationMode:", turretRotationMode, turretRotationModeLabel);
						tower.turretRotationModel=(_RotationMode)turretRotationMode;
					}
					else startY+=20;
					
					int turretLOSMode=(int)tower.losMode;
					cont=new GUIContent("TurretLOSMode:", "The line of sight mechanic used with the tower");
					contL=new GUIContent[turretLOSModeLabel.Length];
					for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(turretLOSModeLabel[i], turretLOSModeTooltip[i]);
					turretLOSMode = EditorGUI.Popup(new Rect(startX, startY+=20, 300, 15), cont, turretLOSMode, contL);
					//~ turretLOSMode = EditorGUI.Popup(new Rect(startX, startY+=20, 300, 15), "TurretLOSMode:", turretLOSMode, turretLOSModeLabel);
					tower.losMode=(_LOSMode)turretLOSMode;
				}
				else startY+=20;
				
				if(tower.type==_TowerType.TurretTower || tower.type==_TowerType.DirectionalAOETower){
					tower.turretRotateSpeed = EditorGUI.FloatField(new Rect(320, 233, 300, height-3), "TurretRotateSpeed:", tower.turretRotateSpeed);
					tower.aimTolerance = EditorGUI.FloatField(new Rect(320, 253, 300, height-3), "AimTolerance:", tower.aimTolerance);
				}
				
				if(tower.type==_TowerType.DirectionalAOETower){
					tower.aoeConeAngle = EditorGUI.FloatField(new Rect(startX, startY+=20, 300, height-3), "AOE Cone Angle:", tower.aoeConeAngle);
				}
				
				
				if(tower.type==_TowerType.Mine){
					tower.mineOneOff=EditorGUI.Toggle(new Rect(startX, startY, 300, 15), "DestroyUponTriggered:", tower.mineOneOff);
				}
				
				{	
					startY+=5;
					tower.buildingEffect=(GameObject)EditorGUI.ObjectField(new Rect(startX, startY+=20, 300, 15), "BuildingEffect: ", tower.buildingEffect, typeof(GameObject), false);
					tower.buildingDoneEffect=(GameObject)EditorGUI.ObjectField(new Rect(startX, startY+=20, 300, 15), "BuildingDoneEffect: ", tower.buildingDoneEffect, typeof(GameObject), false);
					tower.destroyEffect=(GameObject)EditorGUI.ObjectField(new Rect(startX, startY+=20, 300, 15), "DestroyEffect: ", tower.destroyEffect, typeof(GameObject), false);
					startY+=5;
					
					
					showAnimationList=EditorGUI.Foldout(new Rect(startX, startY+=spaceY, 300, 15), showAnimationList, showAnimationText);
					if(showAnimationList){
						showAnimationText="Hide build animation list";
						tower.turretBuildAnimationBody=(Animation)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), " - TurretAnimationComponent: ", tower.turretBuildAnimationBody, typeof(Animation), false);
						tower.turretBuildAnimation=(AnimationClip)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), " - TurretAnimation: ", tower.turretBuildAnimation, typeof(AnimationClip), false);
						tower.baseBuildAnimationBody=(Animation)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), " - BaseAnimationComponent: ", tower.baseBuildAnimationBody, typeof(Animation), false);
						tower.baseBuildAnimation=(AnimationClip)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), " - BaseAnimation: ", tower.baseBuildAnimation, typeof(AnimationClip), false);
					}
					else{
						showAnimationText="Show build animation list";
					}
					
					
					
					showSoundList=EditorGUI.Foldout(new Rect(startX, startY+=spaceY, 300, 15), showSoundList, showSoundText);
					if(showSoundList){
						tower.shootSound=(AudioClip)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), " - ShootSound: ", tower.shootSound, typeof(AudioClip), false);
						tower.buildingSound=(AudioClip)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), " - BuildingSound: ", tower.buildingSound, typeof(AudioClip), false);
						tower.builtSound=(AudioClip)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), " - BuiltSound: ", tower.builtSound, typeof(AudioClip), false);
						tower.soldSound=(AudioClip)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), " - SoldSound: ", tower.soldSound, typeof(AudioClip), false);
						
						showSoundText="Hide sfx list";
					}
					else{
						showSoundText="Show sfx list";
					}
					
					
					EditorGUI.LabelField(new Rect(startX, startY+=25, 150, height), "Tower Description: ");
					tower.description=EditorGUI.TextArea(new Rect(startX, startY+=17, 485, 50), tower.description);
					startY+=25;
				}
				
				
				//**********************************************************************************************************************************************************
				//position in which the stat editor for tower levels start
				startY+=20;
				//~ float tabYPos=startY;
				//~ float statBoxHeight=610+(rscCount*20);
				
				rscCount=rscList.Count;
				
				indicatorFlags[0] = EditorGUI.Toggle(new Rect(startX, startY+spaceY-10, 20, height), indicatorFlags[0]);
				
				statBoxStartY=startY;
				startY+=10;
				
				if(indicatorFlags[0]){
					
					GUI.Box(new Rect(startX, startY+spaceY-1, 175, statBoxHeight), "");
					startX+=3;
					
					EditorGUI.LabelField(new Rect(50+startX, startY+=spaceY, 200, height), "Level 1: ");
					
					
					if(rscCount!=tower.baseStat.costs.Length){
						UpdateBaseStatCost(index, rscCount);
					}
					
					if(tower.baseStat.costs.Length==1){
						EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "Cost("+rscList[0].name+"): ");
						tower.baseStat.costs[0] = EditorGUI.IntField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.costs[0]);
					}
					else{
						EditorGUI.LabelField(new Rect(startX, startY+=spaceY-5, 200, height), "Cost: ");
						for(int i=0; i<tower.baseStat.costs.Length; i++){
							EditorGUI.LabelField(new Rect(startX, startY+spaceY-3, 200, height), " - "+rscList[i].name+": ");
							
							tower.baseStat.costs[i] = EditorGUI.IntField(new Rect(startX+lW, startY+=spaceY-3, 50, height-2), tower.baseStat.costs[i]);
						}
						startY+=8;
					}
					
					EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "BuildDuration: ");
					tower.baseStat.buildDuration = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.buildDuration);
					
					startY+=3;
					
					TypeDependentBaseStat(towerType);
					
					spaceY+=2;	startY+=8;
					
					if(!(tower.type==_TowerType.Mine || tower.type==_TowerType.Block)){
						EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "ShootObj: ");
						tower.baseStat.shootObject=(Transform)EditorGUI.ObjectField(new Rect(startX+lW-30, startY+=spaceY, 100, height-2), tower.baseStat.shootObject, typeof(Transform), false);
						
						EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "TurretObj: ");
						tower.baseStat.turretObject=(Transform)EditorGUI.ObjectField(new Rect(startX+lW-30, startY+=spaceY, 100, height-2), tower.baseStat.turretObject, typeof(Transform), false);
						
						EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "BaseObj: ");
						tower.baseStat.baseObject=(Transform)EditorGUI.ObjectField(new Rect(startX+lW-30, startY+=spaceY, 100, height-2), tower.baseStat.baseObject, typeof(Transform), false);
					
						//if(tower.type==_TowerType.TurretTower || tower.type==_TowerType.DirectionalAOETower){
						//	if(tower.turretRotationModel==_RotationMode.SeparatedBarrel){
						//		EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "BarrelObj: ");
						//		tower.baseStat.barrelObject=(Transform)EditorGUI.ObjectField(new Rect(startX+lW-30, startY+=spaceY, 100, height-2), tower.baseStat.barrelObject, typeof(Transform), false);
						//	}
						//}
					}
					
					statBoxHeight=startY-statBoxStartY;
					
					//~ startY=870;
					startY+=10;
					startX-=3;
					
						
					if(!(tower.type==_TowerType.Mine || tower.type==_TowerType.Block)){
						spaceY-=3;
						//~ startY=tabYPos+statBoxHeight+15;
						startY=statBoxStartY+statBoxHeight+15;
						//GUI.Box(new Rect(startX, startY+20, 175, 160), "");
						GUI.Box(new Rect(startX, startY+20, 175, 133), "");
						
						startX+=3;
						
						EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 200, height), "- Turret Fire Animation: ");
						EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "Clip: ");
						tower.baseStat.turretFireAnimation=(AnimationClip)EditorGUI.ObjectField(new Rect(startX+lW-25, startY+=spaceY, 95, height-2), tower.baseStat.turretFireAnimation, typeof(AnimationClip), false);
						EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "Component: ");
						tower.baseStat.turretFireAnimationBody=(Animation)EditorGUI.ObjectField(new Rect(startX+lW-25, startY+=spaceY, 95, height-2), tower.baseStat.turretFireAnimationBody, typeof(Animation), false);
						startY+=10;
						
						EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 200, height), "- Base Fire Animation: ");
						EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "Clip: ");
						tower.baseStat.baseFireAnimation=(AnimationClip)EditorGUI.ObjectField(new Rect(startX+lW-25, startY+=spaceY, 95, height-2), tower.baseStat.baseFireAnimation, typeof(AnimationClip), false);
						EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "Component: ");
						tower.baseStat.baseFireAnimationBody=(Animation)EditorGUI.ObjectField(new Rect(startX+lW-25, startY+=spaceY, 95, height-2), tower.baseStat.baseFireAnimationBody, typeof(Animation), false);
						startY+=10;
						
						//EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "AniSpeed: ");
						//tower.baseStat.fireAnimationSpeed=EditorGUI.FloatField(new Rect(startX+lW-25, startY+=spaceY, 75, height-2), tower.baseStat.fireAnimationSpeed);
					}	
					contentHeight=startY+30;
					startX+=200;	startY=statBoxStartY;	spaceY=21;
					
				}
				else{
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 200, height), "1");
					startX+=35;	//startY=tabYPos;	
				}
					
					
					
					
				//***************************************************************************************************************************************************
				//upgrade stat
				for(int i=0; i<tower.upgradeStat.Length; i++){
					
					if(tower!=null && tower.upgradeStat[i]!=null){
						
						if(i+1>indicatorFlags.Length) return;
						indicatorFlags[i+1] = EditorGUI.Toggle(new Rect(startX, startY+spaceY-10, 20, height), indicatorFlags[i+1]);
						startY+=10;
						
						if(indicatorFlags[i+1]){
							GUI.Box(new Rect(startX, startY+spaceY-1, 175, statBoxHeight), "");
							startX+=3;
							
							EditorGUI.LabelField(new Rect(50+startX, startY+=spaceY, 200, height), "Level "+(i+2).ToString()+": ");
							
							if(rscCount!=tower.upgradeStat[i].costs.Length){
								UpdateUpgradeStatCost(index, rscCount);
							}
							
							if(tower.upgradeStat[i].costs.Length==1){
								EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "Cost("+rscList[0].name+"): ");
								tower.upgradeStat[i].costs[0] = EditorGUI.IntField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[i].costs[0]);
							}
							else{
								EditorGUI.LabelField(new Rect(startX, startY+=spaceY-5, 200, height), "Cost: ");
								for(int j=0; j<tower.upgradeStat[i].costs.Length; j++){
									EditorGUI.LabelField(new Rect(startX, startY+spaceY-3, 200, height), " - "+rscList[j].name+": ");
									
									tower.upgradeStat[i].costs[j] = EditorGUI.IntField(new Rect(startX+lW, startY+=spaceY-3, 50, height-2), tower.upgradeStat[i].costs[j]);
								}
								startY+=8;
							}
							
							
							EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "BuildDuration: ");
							tower.upgradeStat[i].buildDuration = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[i].buildDuration);
							startY+=3;
							
							TypeDependentUpgradeStat(towerType, i);
							
							spaceY+=2;	startY+=8;
							
							if(!(tower.type==_TowerType.Mine || tower.type==_TowerType.Block)){
								EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "ShootObj: ");
								tower.upgradeStat[i].shootObject=(Transform)EditorGUI.ObjectField(new Rect(startX+lW-30, startY+=spaceY, 100, height-2), tower.upgradeStat[i].shootObject, typeof(Transform), false);
								
								EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "TurretObj: ");
								tower.upgradeStat[i].turretObject=(Transform)EditorGUI.ObjectField(new Rect(startX+lW-30, startY+=spaceY, 100, height-2), tower.upgradeStat[i].turretObject, typeof(Transform), false);
								
								EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "BaseObj: ");
								tower.upgradeStat[i].baseObject=(Transform)EditorGUI.ObjectField(new Rect(startX+lW-30, startY+=spaceY, 100, height-2), tower.upgradeStat[i].baseObject, typeof(Transform), false);
								
								//~ if(tower.type==_TowerType.TurretTower || tower.type==_TowerType.DirectionalAOETower){
									//~ if(tower.turretRotationModel==_RotationMode.SeparatedBarrel){
										//~ EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "BarrelObj: ");
										//~ tower.upgradeStat[i].barrelObject=(Transform)EditorGUI.ObjectField(new Rect(startX+lW-30, startY+=spaceY, 100, height-2), tower.upgradeStat[i].barrelObject, typeof(Transform), false);
									//~ }
								//~ }
							}
							
							
							if(!(tower.type==_TowerType.Mine || tower.type==_TowerType.Block)){
								spaceY-=3;
								startY=statBoxStartY+statBoxHeight+15;
								startX-=3;
								
								//GUI.Box(new Rect(startX, startY+20, 175, 160), "");
								GUI.Box(new Rect(startX, startY+20, 175, 133), "");
								
								startX+=3;
								
								EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 200, height), "- Turret Fire Animation: ");
								EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "Clip: ");
								tower.upgradeStat[i].turretFireAnimation=(AnimationClip)EditorGUI.ObjectField(new Rect(startX+lW-25, startY+=spaceY, 95, height-2), tower.upgradeStat[i].turretFireAnimation, typeof(AnimationClip), false);
								startY+=10;
								
								EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 200, height), "Base Fire Animation: ");
								EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "Clip: ");
								tower.upgradeStat[i].baseFireAnimation=(AnimationClip)EditorGUI.ObjectField(new Rect(startX+lW-25, startY+=spaceY, 95, height-2), tower.upgradeStat[i].baseFireAnimation, typeof(AnimationClip), false);
								startY+=10;
								
								//EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "AniSpeed: ");
								//tower.upgradeStat[i].fireAnimationSpeed=EditorGUI.FloatField(new Rect(startX+lW-25, startY+=spaceY, 75, height-2), tower.upgradeStat[i].fireAnimationSpeed);
							}
							
							//contentHeight=startY;
							startX+=190;	startY=statBoxStartY;	spaceY=21;
							
						}
						else{
							EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 200, height), (i+2).ToString());
							startX+=25;	//startY=tabYPos;
						}
					}
				
				}
				
				HPAttributeEditor(index);
				
				//contentHeight=startY;
			}
			else{
				if(GUI.Button(new Rect(startX, startY, 140, height), "Find Build Manager")) GetTower();
			}
			
			
		}
	
		
		
		GUI.EndScrollView();
		
		if(GUI.changed) EditorUtility.SetDirty(tower);
		
		
    }
	
	
	
	
	
	//*****************************************************************************************************************************************************************
	void HPAttributeEditor(int index){
		float startY=133;
		float startX=320;
		
		float space=105;
		
		EditorGUI.LabelField(new Rect(startX, startY, 200, height), "Tower HP: ");
		tower.HPAttribute.fullHP = EditorGUI.FloatField(new Rect(startX+space, startY, 50, height-2), tower.HPAttribute.fullHP);
		
		EditorGUI.LabelField(new Rect(startX, startY+20, 200, height), "Tower Shield: ");
		tower.HPAttribute.fullShield = EditorGUI.FloatField(new Rect(startX+space, startY+=20, 50, height-2), tower.HPAttribute.fullShield);
		
		if(tower.HPAttribute.fullShield>0){
			EditorGUI.LabelField(new Rect(startX, startY+20, 200, height), "Shield Recharge: ");
			tower.HPAttribute.shieldRechargeRate = EditorGUI.FloatField(new Rect(startX+space, startY+=20, 50, height-2), tower.HPAttribute.shieldRechargeRate);
			
			EditorGUI.LabelField(new Rect(startX, startY+20, 200, height), "Shield Stagger: ");
			tower.HPAttribute.shieldStagger = EditorGUI.FloatField(new Rect(startX+space, startY+=20, 50, height-2), tower.HPAttribute.shieldStagger);
		}
		
		startY=133;
		startX=320+105+50+10;
		space=100;
		
		EditorGUI.LabelField(new Rect(startX, startY, 200, height), "HP Overlay: ");
		tower.HPAttribute.overlayHP=(Transform)EditorGUI.ObjectField(new Rect(startX+space, startY, 100, height-2), tower.HPAttribute.overlayHP, typeof(Transform), false);
		
		EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 200, height), "Shield Overlay: ");
		tower.HPAttribute.overlayShield=(Transform)EditorGUI.ObjectField(new Rect(startX+space, startY, 100, height-2), tower.HPAttribute.overlayShield, typeof(Transform), false);
		
		EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 200, height), "Base Overlay: ");
		tower.HPAttribute.overlayBase=(Transform)EditorGUI.ObjectField(new Rect(startX+space, startY, 100, height-2), tower.HPAttribute.overlayBase, typeof(Transform), false);
		
		EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 200, height), "Always Show Overlay: ");
		tower.HPAttribute.alwaysShowOverlay= EditorGUI.Toggle(new Rect(startX+space+40, startY, 100, height-2), tower.HPAttribute.alwaysShowOverlay);
	}
	
	
	
	
	//*****************************************************************************************************************************************************************
	void TypeDependentBaseStat(int towerType){
		//turretTower
		if(towerType==0){
			
			EditorGUI.LabelField(new Rect(startX, startY+spaceY+5, 200, height), "Damage: ");
			tower.baseStat.damage = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY+5, 50, height-2), tower.baseStat.damage);
			
			EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "Cooldown: ");
			tower.baseStat.cooldown = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.cooldown);
			
			EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "ReloadDuration: ");
			tower.baseStat.reloadDuration = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.reloadDuration);
			
			EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "ClipSize: ");
			tower.baseStat.clipSize = EditorGUI.IntField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.clipSize);
			
			EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "Range: ");
			tower.baseStat.range = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.range);
			
			EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "AoeRadius: ");
			tower.baseStat.aoeRadius = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.aoeRadius);
			
			//~ EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "StunDuration: ");
			//~ tower.baseStat.stunDuration = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.stunDuration);
			

			
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY+3, 200, height), "Critical Damage: ");
			spaceY-=3;
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Chance: ");
			tower.baseStat.crit.chance = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.crit.chance);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- DmgModifier: ");
			tower.baseStat.crit.dmgModifier = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.crit.dmgModifier);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- DmgBonus: ");
			tower.baseStat.crit.dmgBonus = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.crit.dmgBonus);
			
			spaceY+=3;
			
			
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY+3, 200, height), "Stun Effect: ");
			spaceY-=3;
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Chance: ");
			tower.baseStat.stun.chance = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.stun.chance);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Duration: ");
			tower.baseStat.stun.duration = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.stun.duration);
			
			spaceY+=3;
			
			
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY+3, 200, height), "Slow Effect: ");
			spaceY-=3;
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Duration: ");
			tower.baseStat.slow.duration = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.slow.duration);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- SlowFactor: ");
			tower.baseStat.slow.slowFactor = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.slow.slowFactor);
			
			spaceY+=3;
			
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY+3, 200, height), "DamageOverTime: ");
			spaceY-=3;
			
			
			int dotType=(int)tower.baseStat.dot.dotType;
			EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "DotType: ");
			dotType = EditorGUI.Popup(new Rect(startX+lW-30, startY+=spaceY, 80, height-2), dotType, dotTypeLabel);
			tower.baseStat.dot.dotType=(_DotType)dotType;
			
			EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "Stackable: ");
			tower.baseStat.dot.stackable = EditorGUI.Toggle(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.dot.stackable);
			
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY+3, 200, height), "- Damage: ");
			tower.baseStat.dot.modifier1 = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY+3, 50, height-2), tower.baseStat.dot.modifier1);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Duration: ");
			tower.baseStat.dot.duration = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.dot.duration);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Interval: ");
			tower.baseStat.dot.interval = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.dot.interval);
			
			if(dotType!=0){
				EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Modifier: ");
				tower.baseStat.dot.modifier2 = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.dot.modifier2);
			
				if(dotType==2 || dotType==4){
					EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- power: ");
					tower.baseStat.dot.power = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.dot.power);
				}
			}
			
			//float ttDmg=tower.baseStat.dot.damage*tower.baseStat.dot.duration/tower.baseStat.dot.interval;
			float ttDmg=tower.baseStat.dot.GetTotalDamage();
			EditorGUI.LabelField(new Rect(startX+10, startY+=spaceY+3, 160, height), "TotalDamage:  "+ttDmg.ToString("f1"));
			
			spaceY+=3;
			//startY+=5;
			
		}
		//AOETower
		else if(towerType==1){
			EditorGUI.LabelField(new Rect(startX, startY+spaceY+5, 200, height), "Damage: ");
			tower.baseStat.damage = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY+5, 50, height-2), tower.baseStat.damage);
			
			EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "Cooldown: ");
			tower.baseStat.cooldown = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.cooldown);
			
			EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "Range: ");
			tower.baseStat.range = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.range);
			
			//~ EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "StunDuration: ");
			//~ tower.baseStat.stunDuration = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.stunDuration);
			
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY+3, 200, height), "Stun Effect: ");
			spaceY-=3;
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Chance: ");
			tower.baseStat.stun.chance = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.stun.chance);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Duration: ");
			tower.baseStat.stun.duration = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.stun.duration);
			
			spaceY+=3;
			
			
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY+3, 200, height), "Slow Effect: ");
			spaceY-=3;
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Duration: ");
			tower.baseStat.slow.duration = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.slow.duration);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- SlowFactor: ");
			tower.baseStat.slow.slowFactor = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.slow.slowFactor);
			
			spaceY+=3;
			
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY+3, 200, height), "DamageOverTime: ");
			spaceY-=3;
			
			int dotType=(int)tower.baseStat.dot.dotType;
			EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "DotType: ");
			dotType = EditorGUI.Popup(new Rect(startX+lW-30, startY+=spaceY, 80, height-2), dotType, dotTypeLabel);
			tower.baseStat.dot.dotType=(_DotType)dotType;
			
			EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "Stackable: ");
			tower.baseStat.dot.stackable = EditorGUI.Toggle(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.dot.stackable);
			
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY+3, 200, height), "- Damage: ");
			tower.baseStat.dot.modifier1 = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY+3, 50, height-2), tower.baseStat.dot.modifier1);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Duration: ");
			tower.baseStat.dot.duration = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.dot.duration);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Interval: ");
			tower.baseStat.dot.interval = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.dot.interval);
			
			if(dotType!=0){
				EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Modifier: ");
				tower.baseStat.dot.modifier2 = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.dot.modifier2);
			
				if(dotType==2 || dotType==4){
					EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- power: ");
					tower.baseStat.dot.power = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.dot.power);
				}
			}
			
			//float ttDmg=tower.baseStat.dot.damage*tower.baseStat.dot.duration/tower.baseStat.dot.interval;
			float ttDmg=tower.baseStat.dot.GetTotalDamage();
			EditorGUI.LabelField(new Rect(startX+10, startY+=spaceY+3, 160, height), "TotalDamage:  "+ttDmg.ToString("f1"));
			
			spaceY+=3;
		}
		//directionalAOETower
		else if(towerType==2){
			EditorGUI.LabelField(new Rect(startX, startY+spaceY+5, 200, height), "Damage: ");
			tower.baseStat.damage = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY+5, 50, height-2), tower.baseStat.damage);
			
			EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "Cooldown: ");
			tower.baseStat.cooldown = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.cooldown);
			
			EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "ReloadDuration: ");
			tower.baseStat.reloadDuration = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.reloadDuration);
			
			EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "ClipSize: ");
			tower.baseStat.clipSize = EditorGUI.IntField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.clipSize);
			
			EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "Range: ");
			tower.baseStat.range = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.range);
			
			//~ EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "StunDuration: ");
			//~ tower.baseStat.stunDuration = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.stunDuration);
			
			
			
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY+3, 200, height), "Critical Damage: ");
			spaceY-=3;
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Chance: ");
			tower.baseStat.crit.chance = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.crit.chance);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- DmgMultiplier: ");
			tower.baseStat.crit.dmgModifier = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.crit.dmgModifier);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- DmgBonus: ");
			tower.baseStat.crit.dmgBonus = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.crit.dmgBonus);
			
			spaceY+=3;
			
			
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY+3, 200, height), "Stun Effect: ");
			spaceY-=3;
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Chance: ");
			tower.baseStat.stun.chance = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.stun.chance);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Duration: ");
			tower.baseStat.stun.duration = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.stun.duration);
			
			spaceY+=3;
			
			
			
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY+3, 200, height), "Slow Effect: ");
			spaceY-=3;
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Duration: ");
			tower.baseStat.slow.duration = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.slow.duration);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- SlowFactor: ");
			tower.baseStat.slow.slowFactor = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.slow.slowFactor);
			
			spaceY+=3;
			
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY+3, 200, height), "DamageOverTime: ");
			spaceY-=3;
			
			int dotType=(int)tower.baseStat.dot.dotType;
			EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "DotType: ");
			dotType = EditorGUI.Popup(new Rect(startX+lW-30, startY+=spaceY, 80, height-2), dotType, dotTypeLabel);
			tower.baseStat.dot.dotType=(_DotType)dotType;
			
			EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "Stackable: ");
			tower.baseStat.dot.stackable = EditorGUI.Toggle(new Rect(startX+lW, startY+=spaceY+3, 50, height-2), tower.baseStat.dot.stackable);
			
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY+3, 200, height), "- Damage: ");
			tower.baseStat.dot.modifier1 = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY+3, 50, height-2), tower.baseStat.dot.modifier1);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Duration: ");
			tower.baseStat.dot.duration = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.dot.duration);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Interval: ");
			tower.baseStat.dot.interval = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.dot.interval);
			
			if(dotType!=0){
				EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Modifier: ");
				tower.baseStat.dot.modifier2 = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.dot.modifier2);
			
				if(dotType==2 || dotType==4){
					EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- power: ");
					tower.baseStat.dot.power = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.dot.power);
				}
			}
			
			//float ttDmg=tower.baseStat.dot.damage*tower.baseStat.dot.duration/tower.baseStat.dot.interval;
			float ttDmg=tower.baseStat.dot.GetTotalDamage();
			EditorGUI.LabelField(new Rect(startX+10, startY+=spaceY+3, 160, height), "TotalDamage:  "+ttDmg.ToString("f1"));
						
			spaceY+=3;
			//startY+=5;
			
		}
		//SupportTower
		else if(towerType==3){
			
			spaceY+=5;
			EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "Effective Range: ");
			tower.baseStat.range = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.range);
			
			spaceY+=5;
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY+3, 200, height), "Buff Effect: ");
			spaceY-=5;
			startY-=3;
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Damage: ");
			tower.baseStat.buff.damageBuff = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.buff.damageBuff);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Cooldown: ");
			tower.baseStat.buff.cooldownBuff = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.buff.cooldownBuff);
			tower.baseStat.buff.cooldownBuff = Mathf.Clamp(tower.baseStat.buff.cooldownBuff, -0.8f, 0.8f);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Range: ");
			tower.baseStat.buff.rangeBuff = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.buff.rangeBuff);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- RegenHP: ");
			tower.baseStat.buff.regenHP = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY+3, 50, height-2), tower.baseStat.buff.regenHP);
			
			
			EditorGUI.LabelField(new Rect(startX, startY+spaceY+10, 200, height), "Cooldown: ");
			tower.baseStat.cooldown = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY+10, 50, height-2), tower.baseStat.cooldown);
			
			spaceY-=5;
		}
		//ResourceTower
		else if(towerType==4){
			
			//EditorGUI.LabelField(new Rect(startX, startY+spaceY+10, 200, height), "IncomeValue: ");
			//tower.baseStat.incomeValue = EditorGUI.IntField(new Rect(startX+lW, startY+=spaceY+10, 50, height-2), tower.baseStat.incomeValue);
			
			EditorGUI.LabelField(new Rect(startX, startY+spaceY+10, 200, height), "Cooldown: ");
			tower.baseStat.cooldown = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY+10, 50, height-2), tower.baseStat.cooldown);
			
			startY+=10;
			
			if(rscCount!=tower.baseStat.incomes.Length){
				UpdateBaseStatIncomes(index, rscCount);
			}
			
			//~ if(tower.baseStat.incomes.Length==1){
				//~ //EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "resources:");
				//~ EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), rscList[0].name+":");
				//~ tower.baseStat.incomes[0] = EditorGUI.IntField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.incomes[0]);
			//~ }
			//~ else{
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY-5, 200, height), "Resources Per CD:");
				for(int i=0; i<tower.baseStat.incomes.Length; i++){
					//~ string rscName="";
					//~ if(rscManager!=null) rscName=rscManager.resources[i].name;
					//~ EditorGUI.LabelField(new Rect(startX, startY+spaceY-3, 200, height), " - "+rscName+": ");
					EditorGUI.LabelField(new Rect(startX, startY+spaceY-3, 200, height), " - "+rscList[i].name+": ");
					
					tower.baseStat.incomes[i] = EditorGUI.IntField(new Rect(startX+lW, startY+=spaceY-3, 50, height-2), tower.baseStat.incomes[i]);
				}
				startY+=8;
			//~ }
			
			
		}
		//mine
		else if(towerType==5){
			
			EditorGUI.LabelField(new Rect(startX, startY+spaceY+5, 200, height), "Damage: ");
			tower.baseStat.damage = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY+5, 50, height-2), tower.baseStat.damage);
			
			if(!tower.mineOneOff){
				EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "Cooldown: ");
				tower.baseStat.cooldown = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.cooldown);
			}
				
			EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "EffectiveRange: ");
			tower.baseStat.range = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.range);
			
			//~ EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "StunDuration: ");
			//~ tower.baseStat.stunDuration = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.stunDuration);
			
			
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY+3, 200, height), "Critical Damage: ");
			spaceY-=3;
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Chance: ");
			tower.baseStat.crit.chance = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.crit.chance);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- DmgModifier: ");
			tower.baseStat.crit.dmgModifier = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.crit.dmgModifier);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- DmgBonus: ");
			tower.baseStat.crit.dmgBonus = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.crit.dmgBonus);
			
			spaceY+=3;
			
			
			
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY+3, 200, height), "Stun Effect: ");
			spaceY-=3;
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Chance: ");
			tower.baseStat.stun.chance = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.stun.chance);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Duration: ");
			tower.baseStat.stun.duration = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.stun.duration);
			
			spaceY+=3;
			
			
			
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY+3, 200, height), "Slow Effect: ");
			spaceY-=3;
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Duration: ");
			tower.baseStat.slow.duration = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.slow.duration);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- SlowFactor: ");
			tower.baseStat.slow.slowFactor = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.slow.slowFactor);
			
			spaceY+=3;
			
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY+3, 200, height), "DamageOverTime: ");
			spaceY-=3;
			
			int dotType=(int)tower.baseStat.dot.dotType;
			EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "DotType: ");
			dotType = EditorGUI.Popup(new Rect(startX+lW-30, startY+=spaceY, 80, height-2), dotType, dotTypeLabel);
			tower.baseStat.dot.dotType=(_DotType)dotType;
			
			EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "Stackable: ");
			tower.baseStat.dot.stackable = EditorGUI.Toggle(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.dot.stackable);
			
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY+3, 200, height), "- Damage: ");
			tower.baseStat.dot.modifier1 = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY+3, 50, height-2), tower.baseStat.dot.modifier1);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Duration: ");
			tower.baseStat.dot.duration = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.dot.duration);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Interval: ");
			tower.baseStat.dot.interval = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.dot.interval);
			
			if(dotType!=0){
				EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Modifier: ");
				tower.baseStat.dot.modifier2 = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.dot.modifier2);
			
				if(dotType==2 || dotType==4){
					EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- power: ");
					tower.baseStat.dot.power = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.dot.power);
				}
			}
			
			//float ttDmg=tower.baseStat.dot.damage*tower.baseStat.dot.duration/tower.baseStat.dot.interval;
			float ttDmg=tower.baseStat.dot.GetTotalDamage();
			EditorGUI.LabelField(new Rect(startX+10, startY+=spaceY+3, 160, height), "TotalDamage:  "+ttDmg.ToString("f1"));
			
			spaceY+=15;
			
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 200, height), "TriggerEffect: ");
			tower.mineEffect=(GameObject)EditorGUI.ObjectField(new Rect(startX+5, startY+15, 160, height-2), tower.mineEffect, typeof(GameObject), false);
			
		}
		
	}
	
	
	
	
	
	
	
	
	
	//*****************************************************************************************************************************************************************
	void TypeDependentUpgradeStat(int towerType, int lvl){
		if(towerType==0){
			
			EditorGUI.LabelField(new Rect(startX, startY+spaceY+5, 200, height), "Damage: ");
			tower.upgradeStat[lvl].damage = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY+5, 50, height-2), tower.upgradeStat[lvl].damage);
			
			EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "Cooldown: ");
			tower.upgradeStat[lvl].cooldown = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].cooldown);
			
			EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "ReloadDuration: ");
			tower.upgradeStat[lvl].reloadDuration = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].reloadDuration);
			
			EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "ClipSize: ");
			tower.upgradeStat[lvl].clipSize = EditorGUI.IntField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].clipSize);
			
			EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "Range: ");
			tower.upgradeStat[lvl].range = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].range);
			
			EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "AoeRadius: ");
			tower.upgradeStat[lvl].aoeRadius = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].aoeRadius);
			
			//~ EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "StunDuration: ");
			//~ tower.upgradeStat[lvl].stunDuration = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].stunDuration);
			
			
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY+3, 200, height), "Critical Damage: ");
			spaceY-=3;
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Chance: ");
			tower.upgradeStat[lvl].crit.chance = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].crit.chance);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- DmgModifier: ");
			tower.upgradeStat[lvl].crit.dmgModifier = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].crit.dmgModifier);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- DmgBonus: ");
			tower.upgradeStat[lvl].crit.dmgBonus = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].crit.dmgBonus);
			
			spaceY+=3;
			
			
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY+3, 200, height), "Stun Effect: ");
			spaceY-=3;
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Chance: ");
			tower.upgradeStat[lvl].stun.chance = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].stun.chance);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Duration: ");
			tower.upgradeStat[lvl].stun.duration = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].stun.duration);
			
			spaceY+=3;
			
			
			
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY+3, 200, height), "Slow Effect: ");
			spaceY-=3;
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Duration: ");
			tower.upgradeStat[lvl].slow.duration = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].slow.duration);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- SlowFactor: ");
			tower.upgradeStat[lvl].slow.slowFactor = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].slow.slowFactor);
			
			spaceY+=3;
			
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY+3, 200, height), "DamageOverTime: ");
			spaceY-=3;
			
			EditorGUI.LabelField(new Rect(startX+10, startY+=spaceY, 200, height), "Type: "+tower.baseStat.dot.dotType);
			EditorGUI.LabelField(new Rect(startX+10, startY+=spaceY, 200, height), "Stackable: "+tower.baseStat.dot.stackable);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY+3, 200, height), "- Damage: ");
			tower.upgradeStat[lvl].dot.modifier1 = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY+3, 50, height-2), tower.upgradeStat[lvl].dot.modifier1);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Duration: ");
			tower.upgradeStat[lvl].dot.duration = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].dot.duration);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Interval: ");
			tower.upgradeStat[lvl].dot.interval = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].dot.interval);
			
			int dotType=(int)tower.baseStat.dot.dotType;
			if(dotType!=0){
				EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Modifier: ");
				tower.baseStat.dot.modifier2 = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.dot.modifier2);
			
				if(dotType==2 || dotType==4){
					EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- power: ");
					tower.baseStat.dot.power = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.dot.power);
				}
			}
			
			//float ttDmg=tower.baseStat.dot.damage*tower.baseStat.dot.duration/tower.baseStat.dot.interval;
			float ttDmg=tower.upgradeStat[lvl].dot.GetTotalDamage();
			EditorGUI.LabelField(new Rect(startX+10, startY+=spaceY+3, 160, height), "TotalDamage:  "+ttDmg.ToString("f1"));
			
			spaceY+=3;
			//startY+=5;
			
		}
		//AOETower
		else if(towerType==1){
			
			EditorGUI.LabelField(new Rect(startX, startY+spaceY+5, 200, height), "Damage: ");
			tower.upgradeStat[lvl].damage = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY+5, 50, height-2), tower.upgradeStat[lvl].damage);
			
			EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "Cooldown: ");
			tower.upgradeStat[lvl].cooldown = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].cooldown);
			
			EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "Range: ");
			tower.upgradeStat[lvl].range = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].range);
			
			//~ EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "StunDuration: ");
			//~ tower.upgradeStat[lvl].stunDuration = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].stunDuration);
			
			
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY+3, 200, height), "Stun Effect: ");
			spaceY-=3;
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Chance: ");
			tower.upgradeStat[lvl].stun.chance = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].stun.chance);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Duration: ");
			tower.upgradeStat[lvl].stun.duration = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].stun.duration);
			
			spaceY+=3;
			
			
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY+3, 200, height), "Slow Effect: ");
			spaceY-=3;
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Duration: ");
			tower.upgradeStat[lvl].slow.duration = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].slow.duration);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- SlowFactor: ");
			tower.upgradeStat[lvl].slow.slowFactor = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].slow.slowFactor);
			
			spaceY+=3;
			
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY+3, 200, height), "DamageOverTime: ");
			spaceY-=3;
			
			EditorGUI.LabelField(new Rect(startX+10, startY+=spaceY, 200, height), "Type: "+tower.baseStat.dot.dotType);
			EditorGUI.LabelField(new Rect(startX+10, startY+=spaceY, 200, height), "Stackable: "+tower.baseStat.dot.stackable);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY+3, 200, height), "- Damage: ");
			tower.upgradeStat[lvl].dot.modifier1 = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY+3, 50, height-2), tower.upgradeStat[lvl].dot.modifier1);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Duration: ");
			tower.upgradeStat[lvl].dot.duration = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].dot.duration);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Interval: ");
			tower.upgradeStat[lvl].dot.interval = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].dot.interval);
			
			int dotType=(int)tower.baseStat.dot.dotType;
			if(dotType!=0){
				EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Modifier: ");
				tower.baseStat.dot.modifier2 = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.dot.modifier2);
			
				if(dotType==2 || dotType==4){
					EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- power: ");
					tower.baseStat.dot.power = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.dot.power);
				}
			}
			
			//float ttDmg=tower.baseStat.dot.damage*tower.baseStat.dot.duration/tower.baseStat.dot.interval;
			float ttDmg=tower.upgradeStat[lvl].dot.GetTotalDamage();
			EditorGUI.LabelField(new Rect(startX+10, startY+=spaceY+3, 160, height), "TotalDamage:  "+ttDmg.ToString("f1"));
			
			spaceY+=3;
		}
		//directionalAOE
		else if(towerType==2){
			
			EditorGUI.LabelField(new Rect(startX, startY+spaceY+5, 200, height), "Damage: ");
			tower.upgradeStat[lvl].damage = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY+5, 50, height-2), tower.upgradeStat[lvl].damage);
			
			EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "Cooldown: ");
			tower.upgradeStat[lvl].cooldown = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].cooldown);
			
			EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "ReloadDuration: ");
			tower.upgradeStat[lvl].reloadDuration = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].reloadDuration);
			
			EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "ClipSize: ");
			tower.upgradeStat[lvl].clipSize = EditorGUI.IntField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].clipSize);
			
			EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "Range: ");
			tower.upgradeStat[lvl].range = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].range);
			
			//~ EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "StunDuration: ");
			//~ tower.upgradeStat[lvl].stunDuration = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].stunDuration);
			
			
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY+3, 200, height), "Critical Damage: ");
			spaceY-=3;
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Chance: ");
			tower.upgradeStat[lvl].crit.chance = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].crit.chance);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- DmgModifier: ");
			tower.upgradeStat[lvl].crit.dmgModifier = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].crit.dmgModifier);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- DmgBonus: ");
			tower.upgradeStat[lvl].crit.dmgBonus = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].crit.dmgBonus);
			
			spaceY+=3;
			
			
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY+3, 200, height), "Stun Effect: ");
			spaceY-=3;
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Chance: ");
			tower.upgradeStat[lvl].stun.chance = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].stun.chance);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Duration: ");
			tower.upgradeStat[lvl].stun.duration = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].stun.duration);
			
			spaceY+=3;
			
			
			
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY+3, 200, height), "Slow Effect: ");
			spaceY-=3;
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Duration: ");
			tower.upgradeStat[lvl].slow.duration = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].slow.duration);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- SlowFactor: ");
			tower.upgradeStat[lvl].slow.slowFactor = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].slow.slowFactor);
			
			spaceY+=3;
			
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY+3, 200, height), "DamageOverTime: ");
			spaceY-=3;
			
			EditorGUI.LabelField(new Rect(startX+10, startY+=spaceY, 200, height), "Type: "+tower.baseStat.dot.dotType);
			EditorGUI.LabelField(new Rect(startX+10, startY+=spaceY, 200, height), "Stackable: "+tower.baseStat.dot.stackable);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY+3, 200, height), "- Damage: ");
			tower.upgradeStat[lvl].dot.modifier1 = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY+3, 50, height-2), tower.upgradeStat[lvl].dot.modifier1);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Duration: ");
			tower.upgradeStat[lvl].dot.duration = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].dot.duration);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Interval: ");
			tower.upgradeStat[lvl].dot.interval = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].dot.interval);
			
			int dotType=(int)tower.baseStat.dot.dotType;
			if(dotType!=0){
				EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Modifier: ");
				tower.baseStat.dot.modifier2 = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.dot.modifier2);
			
				if(dotType==2 || dotType==4){
					EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- power: ");
					tower.baseStat.dot.power = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.baseStat.dot.power);
				}
			}
			
			//float ttDmg=tower.baseStat.dot.damage*tower.baseStat.dot.duration/tower.baseStat.dot.interval;
			float ttDmg=tower.upgradeStat[lvl].dot.GetTotalDamage();
			EditorGUI.LabelField(new Rect(startX+10, startY+=spaceY+3, 160, height), "TotalDamage:  "+ttDmg.ToString("f1"));
			
			spaceY+=3;
			//startY+=5;
			
		}
		else if(towerType==3){
			
			spaceY+=5;
			EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "Effective Range: ");
			tower.upgradeStat[lvl].range = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].range);
			
			spaceY+=5;
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY+3, 200, height), "Buff Effect: ");
			spaceY-=5;
			startY-=3;
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Damage: ");
			tower.upgradeStat[lvl].buff.damageBuff = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].buff.damageBuff);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Cooldown: ");
			tower.upgradeStat[lvl].buff.cooldownBuff = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].buff.cooldownBuff);
			tower.upgradeStat[lvl].buff.cooldownBuff = Mathf.Clamp(tower.upgradeStat[lvl].buff.cooldownBuff, -0.8f, 0.8f);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- Range: ");
			tower.upgradeStat[lvl].buff.rangeBuff = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].buff.rangeBuff);
			
			EditorGUI.LabelField(new Rect(startX+10, startY+spaceY, 200, height), "- RegenHP: ");
			tower.upgradeStat[lvl].buff.regenHP = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY+3, 50, height-2), tower.upgradeStat[lvl].buff.regenHP);
			
			
			EditorGUI.LabelField(new Rect(startX, startY+spaceY+10, 200, height), "Cooldown: ");
			tower.upgradeStat[lvl].cooldown = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY+10, 50, height-2), tower.upgradeStat[lvl].cooldown);
		
			spaceY-=5;
		}
		else if(towerType==4){
			
			//EditorGUI.LabelField(new Rect(startX, startY+spaceY+10, 200, height), "Income Value: ");
			//tower.upgradeStat[lvl].incomeValue = EditorGUI.IntField(new Rect(startX+lW, startY+=spaceY+10, 50, height-2), tower.upgradeStat[lvl].incomeValue);
			
			EditorGUI.LabelField(new Rect(startX, startY+spaceY+10, 200, height), "Cooldown: ");
			tower.upgradeStat[lvl].cooldown = EditorGUI.FloatField(new Rect(startX+lW, startY+=spaceY+10, 50, height-2), tower.upgradeStat[lvl].cooldown);
		
			startY+=10;
			
			if(rscCount!=tower.upgradeStat[lvl].incomes.Length){
				UpdateUpgradeStatIncomes(index, rscCount);
			}
			
			//~ if(tower.upgradeStat[lvl].incomes.Length==1){
				//~ //EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), "resources:");
				//~ EditorGUI.LabelField(new Rect(startX, startY+spaceY, 200, height), rscList[0].name+":");
				//~ tower.upgradeStat[lvl].incomes[0] = EditorGUI.IntField(new Rect(startX+lW, startY+=spaceY, 50, height-2), tower.upgradeStat[lvl].incomes[0]);
			//~ }
			//~ else{
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY-5, 200, height), "Resources Per CD:");
				for(int i=0; i<tower.upgradeStat[lvl].incomes.Length; i++){
					//~ string rscName="";
					//~ if(rscManager!=null) rscName=rscManager.resources[i].name;
					//~ EditorGUI.LabelField(new Rect(startX, startY+spaceY-3, 200, height), " - "+rscName+": ");
					EditorGUI.LabelField(new Rect(startX, startY+spaceY-3, 200, height), " - "+rscList[i].name+": ");
					
					tower.upgradeStat[lvl].incomes[i] = EditorGUI.IntField(new Rect(startX+lW, startY+=spaceY-3, 50, height-2), tower.upgradeStat[lvl].incomes[i]);
				}
				startY+=8;
			//~ }
		
		}
		
	}
	
	static void UpdateIndicatorFlags(UnitTower tower){
		if(tower.levelCap==0) tower.levelCap=1+tower.upgradeStat.Length;
		
		if(indicatorFlags.Length!=tower.levelCap){
			indicatorFlags=new bool[tower.levelCap];
			for(int i=0; i<indicatorFlags.Length; i++) indicatorFlags[i]=true;
		}
	}
	
	
	void UpdateBaseStatIncomes(int id, int length){
		int[] tempIncList=tower.baseStat.incomes;
		
		tower.baseStat.incomes=new int[length];
		
		for(int i=0; i<length; i++){
			if(i>=tempIncList.Length){
				tower.baseStat.incomes[i]=0;
			}
			else{
				tower.baseStat.incomes[i]=tempIncList[i];
			}
		}
	}
	
	void UpdateUpgradeStatIncomes(int id, int length){
		for(int j=0; j<tower.upgradeStat.Length; j++){
			int[] tempIncList=tower.upgradeStat[j].incomes;
			
			tower.upgradeStat[j].incomes=new int[length];
			
			for(int i=0; i<length; i++){
				if(i>=tempIncList.Length){
					tower.upgradeStat[j].incomes[i]=0;
				}
				else{
					tower.upgradeStat[j].incomes[i]=tempIncList[i];
				}
			}
		}
	}
	
	void UpdateBaseStatCost(int id, int length){
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
	}
	
	void UpdateUpgradeStatCost(int id, int length){
		for(int j=0; j<tower.upgradeStat.Length; j++){
			int[] tempCostList=tower.upgradeStat[j].costs;
			
			tower.upgradeStat[j].costs=new int[length];
			
			for(int i=0; i<length; i++){
				if(i>=tempCostList.Length){
					tower.upgradeStat[j].costs[i]=0;
				}
				else{
					tower.upgradeStat[j].costs[i]=tempCostList[i];
				}
			}
		}
	}
	

}





