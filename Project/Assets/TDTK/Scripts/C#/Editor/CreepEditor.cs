using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;


public class CreepEditor : EditorWindow {
    
	
	[SerializeField] static string[] nameList=new string[0];
	
	[SerializeField] static UnitCreep[] creepList=new UnitCreep[0];
	
	private static List<Resource> rscList=new List<Resource>();

	
	static private CreepEditor window;
	
	private static bool showAnimation=false;
	private static bool showAudio=false;
	
	private int index=0;
	
    // Add menu named "TowerEditor" to the Window menu
    //[MenuItem ("TDTK/TowerEditor")]
    public static void Init () {
        // Get existing open window or if none, make a new one:
        window = (CreepEditor)EditorWindow.GetWindow(typeof (CreepEditor));
		window.minSize=new Vector2(720, 650);
		
		rscList=ResourceEditorWindow.Load();
		
		GetCreep();
		
		InitCreepAttackLabel();
	}
	
	
	static void GetCreep(){
		creepList=new UnitCreep[0];
		nameList=new string[0];
		
		GameObject obj=Resources.Load("PrefabListCreep", typeof(GameObject)) as GameObject;
		if(obj!=null){
			CreepListPrefab prefab=obj.GetComponent<CreepListPrefab>();
			
			if(prefab!=null){
				for(int i=0; i<prefab.creepList.Count; i++){
					if(prefab.creepList[i]==null){
						prefab.creepList.RemoveAt(i);
						i-=1;
					}
				}
				
				creepList=new UnitCreep[prefab.creepList.Count];
				nameList=new string[prefab.creepList.Count];
				
				for(int i=0; i<prefab.creepList.Count; i++){
					if(prefab.creepList[i]!=null){
						creepList[i]=prefab.creepList[i];
						nameList[i]=prefab.creepList[i].unitName;
					}
				}
			}
		}
		
		//~ rscManager=(ResourceManager)FindObjectOfType(typeof(ResourceManager));
	}
	
	void UpdateResourceList(){
		rscList=ResourceEditorWindow.Load();
		Repaint();
	}
	
	void OnEnable(){
		ResourceEditorWindow.onResourceUpdateE+=UpdateResourceList;
		CreepManager.onCreepUpdateE+=GetCreep;
	}
	
	void OnDisable(){
		ResourceEditorWindow.onResourceUpdateE-=UpdateResourceList;
		CreepManager.onCreepUpdateE-=GetCreep;
	}
	
	
	float startX, startY, height, spaceY;//, lW;
	//~ int rscCount=1;
	float contentHeight;
	
	GUIContent cont;
	GUIContent[] contL;
	
	private Vector2 scrollPos;
	
    void OnGUI () {
		if(window==null) Init();
		
		Rect visibleRect=new Rect(0, 0, window.position.width, window.position.height);
		Rect contentRect=new Rect(0, 0, window.position.width-15, contentHeight);
		scrollPos = GUI.BeginScrollView(visibleRect, scrollPos, contentRect);
		//~ scrollPos = GUI.BeginScrollView(new Rect(0, 0, window.position.width, window.position.height), scrollPos, new Rect(0, 0, Mathf.Max(window.position.width, 610+(levelCap-3)*180), 1410));
		
		GUI.changed = false;
		
		startX=3;
		startY=3;
		height=18;
		//~ spaceY=height+startX;
		spaceY=height;
			
		if(creepList.Length>0) {
			GUI.SetNextControlName ("CreepSelect");
			index = EditorGUI.Popup(new Rect(startX, startY, 300, height), "Creep:", index, nameList);
			if(GUI.changed) GUI.FocusControl ("CreepSelect");
			
			
			UnitCreep creep=creepList[index];
			
			
			//creep.unitName = EditorGUI.TextField(new Rect(startX, startY+=30, 300, height-3), "CreepName:", creep.unitName);
			
			EditorGUI.LabelField(new Rect(340, startY, 70, height), "Icon: ");
			creep.icon=(Texture)EditorGUI.ObjectField(new Rect(380, startY, 70, 70), creep.icon, typeof(Texture), false);
			
			
			HPAttributeEditor(index);
			
			startY+=8;
			creep.moveSpeed = EditorGUI.FloatField(new Rect(startX, startY+=spaceY, 300, height-3), "moveSpeed:", creep.moveSpeed);
			creep.armorType = EditorGUI.IntField(new Rect(startX, startY+=spaceY, 300, height-3), "ArmorType:", creep.armorType);
			creep.flying = EditorGUI.Toggle(new Rect(startX, startY+=spaceY, 300, height-3), "Flying:", creep.flying);
			if(creep.flying){
				creep.flightHeightOffset = EditorGUI.FloatField(new Rect(startX, startY+=20, 300, height-3), "Flight Height Offset:", creep.flightHeightOffset);
			}
			
			creep.lifeCost = EditorGUI.IntField(new Rect(startX, startY+=spaceY, 300, height-3), "LifeCost:", creep.lifeCost);
			creep.immuneToCrit = EditorGUI.Toggle(new Rect(startX, startY+=spaceY, 300, height-3), "ImmuneToCritical:", creep.immuneToCrit);
			creep.immuneToSlow = EditorGUI.Toggle(new Rect(startX, startY+=spaceY, 300, height-3), "ImmuneToSlow:", creep.immuneToSlow);
			
			
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY+5, 300, height-3), "Value:");
			if(rscList.Count!=creep.value.Length) creep.value=UpdateValueLength(creep.value, rscList.Count);
			for(int i=0; i<creep.value.Length; i++){
				creep.value[i] = EditorGUI.IntField(new Rect(startX, startY+=spaceY, 300, height-3), " - "+rscList[i].name+":", creep.value[i]);
			}
			startY+=5;
			
			
			
			creep.targetPointT=(Transform)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), "TargetPoint: ", creep.targetPointT, typeof(Transform), false);
			
			startY+=10;
			
			cont=new GUIContent("SpawnEffect:", "The visual effect to spawn when this creep is spawned");
			creep.spawnEffect=(GameObject)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), "SpawnEffect: ", creep.spawnEffect, typeof(GameObject), false);
			cont=new GUIContent("DeadEffect:", "The visual effect to spawn when this creep is destroyed");
			creep.deadEffect=(GameObject)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), "DeadEffect: ", creep.deadEffect, typeof(GameObject), false);
			cont=new GUIContent("ScoreEffect:", "The visual effect to spawn when this creep has reached its final waypoint");
			creep.scoreEffect=(GameObject)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), "ScoreEffect: ", creep.scoreEffect, typeof(GameObject), false);
			
			startY+=10;
			
			//~ int targetArea=(int)creepA.targetArea;
			//~ cont=new GUIContent("Target Area:", "select the targeting area of this creep");
			//~ contL=new GUIContent[tgtAreaLabel.Length];
			//~ for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(tgtAreaLabel[i], tgtAreaTooltip[i]);
			
			cont=new GUIContent("SpawnUnitUponDestroyed:", "The creep to spawn when this creep is destroyed,");
			//~ creep.spawnUponDestroyed=(GameObject)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), "SpawnUnitUponDestroyed: ", creep.spawnUponDestroyed, typeof(GameObject), false);
			creep.spawnUponDestroyed=(GameObject)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), cont, creep.spawnUponDestroyed, typeof(GameObject), false);
			if(creep.spawnUponDestroyed!=null){
				cont=new GUIContent(" - Count:", "The creep to spawn when this creep is destroyed,");
				//~ creep.spawnNumber = EditorGUI.IntField(new Rect(startX, startY+=spaceY, 300, height-3), " - Count:", creep.spawnNumber);
				creep.spawnNumber = EditorGUI.IntField(new Rect(startX, startY+=spaceY, 300, height-3), cont, creep.spawnNumber);
			}
			else startY+=spaceY;
			
			startY+=10;
			
			showAnimation=EditorGUI.Foldout(new Rect(startX, startY+=30, 300, height-3), showAnimation, "Animation:");
			//EditorGUI.LabelField(new Rect(startX, startY+=30, 300, height-3), "Animation:");	//startY-=3;
			
			if(showAnimation){
				cont=new GUIContent("AnimationBody:", "The root gameObject that hold the animation component for the animation");
				creep.animationBody=(GameObject)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), "AnimationBody: ", creep.animationBody, typeof(GameObject), false);
				cont=new GUIContent("MoveAnimationModifier:", "The speed modifier for the move animation, adjust to match move animation to move speed");
				creep.moveAnimationModifier = EditorGUI.FloatField(new Rect(startX, startY+=20, 300, height-3), "MoveAnimationModifier:", creep.moveAnimationModifier);
				
				startY+=5;
				
				int aniLength=0;
				aniLength=creep.animationMove.Length;
				aniLength = EditorGUI.IntField(new Rect(startX, startY+=spaceY, 300, height-3), " - Move Animation:", aniLength);
				if(aniLength!=creep.animationMove.Length) creep.animationMove=MatchAnimationArrayLength(creep.animationMove, aniLength);
				for(int i=0; i<aniLength; i++){
					creep.animationMove[i]=(AnimationClip)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), " 	- "+i+": ", creep.animationMove[i], typeof(AnimationClip), false);
				}
				
				aniLength=creep.animationSpawn.Length;
				aniLength = EditorGUI.IntField(new Rect(startX, startY+=spaceY, 300, height-3), " - Spawn Animation:", aniLength);
				if(aniLength!=creep.animationSpawn.Length) creep.animationSpawn=MatchAnimationArrayLength(creep.animationSpawn, aniLength);
				for(int i=0; i<aniLength; i++){
					creep.animationSpawn[i]=(AnimationClip)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), " 	- "+i+": ", creep.animationSpawn[i], typeof(AnimationClip), false);
				}
				
				aniLength=creep.animationHit.Length;
				aniLength = EditorGUI.IntField(new Rect(startX, startY+=spaceY, 300, height-3), " - Hit Animation:", aniLength);
				if(aniLength!=creep.animationHit.Length) creep.animationHit=MatchAnimationArrayLength(creep.animationHit, aniLength);
				for(int i=0; i<aniLength; i++){
					creep.animationHit[i]=(AnimationClip)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), " 	- "+i+": ", creep.animationHit[i], typeof(AnimationClip), false);
				}

				aniLength=creep.animationDead.Length;
				aniLength = EditorGUI.IntField(new Rect(startX, startY+=spaceY, 300, height-3), " - Dead Animation:", aniLength);
				if(aniLength!=creep.animationDead.Length) creep.animationDead=MatchAnimationArrayLength(creep.animationDead, aniLength);
				for(int i=0; i<aniLength; i++){
					creep.animationDead[i]=(AnimationClip)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), " 	- "+i+": ", creep.animationDead[i], typeof(AnimationClip), false);
				}

				aniLength=creep.animationScore.Length;
				aniLength = EditorGUI.IntField(new Rect(startX, startY+=spaceY, 300, height-3), " - Score Animation:", aniLength);
				if(aniLength!=creep.animationScore.Length) creep.animationScore=MatchAnimationArrayLength(creep.animationScore, aniLength);
				for(int i=0; i<aniLength; i++){
					creep.animationScore[i]=(AnimationClip)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), " 	- "+i+": ", creep.animationScore[i], typeof(AnimationClip), false);
				}
			}
			
			
			showAudio=EditorGUI.Foldout(new Rect(startX, startY+=30, 300, height-3), showAudio, "Audio:");
			//EditorGUI.LabelField(new Rect(startX, startY+=30, 300, height-3), "Audio:");	//startY-=3;
			if(showAudio){
				creep.audioSpawn=(AudioClip)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), " - AudioSpawn: ", creep.audioSpawn, typeof(AudioClip), false);
				creep.audioHit=(AudioClip)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), " - AudioHit: ", creep.audioHit, typeof(AudioClip), false);
				creep.audioDead=(AudioClip)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), " - AudioDead: ", creep.audioDead, typeof(AudioClip), false);
				creep.audioScore=(AudioClip)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), " - AudioScore: ", creep.audioScore, typeof(AudioClip), false);
			}
			float anchorY=startY+5;
			
			float y=CreepAttackComEditor(index)+5;
			
			contentHeight=Mathf.Max(y, anchorY);
			
			if(GUI.changed) EditorUtility.SetDirty(creep);
		}
		
		
		GUI.EndScrollView();
    }
	
	
	
	AnimationClip[] MatchAnimationArrayLength(AnimationClip[] list, int length){
		AnimationClip[] tempList=new AnimationClip[length];
		
		for(int i=0; i<length; i++){
			if(i<list.Length){
				tempList[i]=list[i];
			}
			else{
				tempList[i]=null;
			}
		}
		
		return tempList;
	}
	
	int[] UpdateValueLength(int[] list, int length){
		int[] tempList=new int[length];
		
		for(int i=0; i<length; i++){
			if(i<list.Length){
				tempList[i]=list[i];
			}
			else{
				tempList[i]=0;
			}
		}
		
		return tempList;
	}
	
	
	
	private static string[] creepTypeLabel=new string[2];
	private static string[] tgtAreaLabel=new string[3];
	private static string[] attModeLabel=new string[2];
	private static string[] attMethodLabel=new string[2];
	private static string[] cdTrackingLabel=new string[2];
	
	private static string[] creepTypeTooltip=new string[2];
	private static string[] tgtAreaTooltip=new string[3];
	private static string[] attModeTooltip=new string[2];
	private static string[] attMethodTooltip=new string[2];
	private static string[] cdTrackingTooltip=new string[2];
	
	static string[] turretAnimateModeLabel=new string[3];
	static string[] turretRotationModeLabel=new string[2];
	static string[] turretLOSModeLabel=new string[2];
	
	private static string[] turretAnimateModeTooltip=new string[3];
	private static string[] turretRotationModeTooltip=new string[2];
	private static string[] turretLOSModeTooltip=new string[2];
	
	
	public static void InitCreepAttackLabel(){
		creepTypeLabel[0]="Attack";
		creepTypeLabel[1]="Support";
		creepTypeTooltip[0]="offsensive creep -  can attack tower";
		creepTypeTooltip[1]="support creep -  can boost or heal other creep";
		
		tgtAreaLabel[0]="AllAround";
		tgtAreaLabel[1]="FrontalCone";
		tgtAreaLabel[2]="Obstacle";
		tgtAreaTooltip[0]="creep can target tower in every direction in range";
		tgtAreaTooltip[1]="creep can only target tower in a front cone";
		tgtAreaTooltip[2]="creep can only target tower that are straight infront and blocking the way";
		
		attModeLabel[0]="RunNGun";
		attModeLabel[1]="StopNAttack";
		attModeTooltip[0]="creep will not stop moving when acquire a target and attacking";
		attModeTooltip[1]="creep will stop moving when acquire a target and attacking. only after the target is destroyed the creep will resume moving";
		
		attMethodLabel[0]="Range";
		attMethodLabel[1]="Melee";
		attMethodTooltip[0]="creep uses range attack";
		attMethodTooltip[1]="creep uses melee attack";
		
		cdTrackingLabel[0]="Easy";
		cdTrackingLabel[1]="Precise";
		cdTrackingTooltip[0]="the attack cooldown is some what random but still based on the duration specified. often there will be some delay to next attack after cooldown";
		cdTrackingTooltip[1]="the attack cooldown tracking is precise. creep attack as soon as cooldown is done";
		
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
	}
	
	
	float CreepAttackComEditor(int index){
		float startY=250;
		float startX=340;
		
		
		UnitCreep creep=creepList[index];
		
		EditorGUI.LabelField(new Rect(startX, startY, 200, 20), "Attack/Support Creep:");
		
		UnitCreepAttack creepA=creep.gameObject.GetComponent<UnitCreepAttack>();
		if(creepA==null){
			cont=new GUIContent("Add Com", "Make this creep available for attack tower or support creep");
			if(GUI.Button(new Rect(startX+300-110, startY, 110, 20), "Add Com")){
				creepA=creep.gameObject.AddComponent<UnitCreepAttack>();
			}
			else return startY;
		}
		else{
			cont=new GUIContent("Remove Com", "Remove creep ability to attack tower or support creep");
			if(GUI.Button(new Rect(startX+300-110, startY, 110, 20), "Remove Com")){
				DestroyImmediate(creepA, true);
			}
		}
		
		startY+=10;
		
		int creepAType=(int)creepA.type;
		cont=new GUIContent("Type:", "select the type of this creep (offensive/support)");
		contL=new GUIContent[creepTypeLabel.Length];
		for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(creepTypeLabel[i], creepTypeTooltip[i]);
		creepAType = EditorGUI.Popup(new Rect(startX, startY+=spaceY, 300, 15), cont, creepAType, contL);
		creepA.type=(_AttackCreepType)creepAType;
		
		int creepAMode=(int)creepA.attackMode;
		cont=new GUIContent("Mode:", "select the attacking behaviour of this creep");
		contL=new GUIContent[attModeLabel.Length];
		for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(attModeLabel[i], attModeTooltip[i]);
		creepAMode = EditorGUI.Popup(new Rect(startX, startY+=spaceY, 300, 15), cont, creepAMode, contL);
		creepA.attackMode=(_AttackMode)creepAMode;
		
		if(creepA.type==_AttackCreepType.Attack){
			int targetArea=(int)creepA.targetArea;
			cont=new GUIContent("Target Area:", "select the targeting area of this creep");
			contL=new GUIContent[tgtAreaLabel.Length];
			for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(tgtAreaLabel[i], tgtAreaTooltip[i]);
			targetArea = EditorGUI.Popup(new Rect(startX, startY+=spaceY, 300, 15), cont, targetArea, contL);
			creepA.targetArea=(_TargetingA)targetArea;
			
			cont=new GUIContent("frontalConeAngle:", "angle of the fontal cone targeting area. larger value indicate larger area. (0-360)");
			if(targetArea==1) creepA.frontalConeAngle = EditorGUI.FloatField(new Rect(startX, startY+=spaceY, 300, height-3), "frontalConeAngle:", creepA.frontalConeAngle);
			
			
			int attackMethod=(int)creepA.attackMethod;
			cont=new GUIContent("Attack Method:", "select between range or melee attack");
			contL=new GUIContent[attMethodLabel.Length];
			for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(attMethodLabel[i], attMethodTooltip[i]);
			attackMethod = EditorGUI.Popup(new Rect(startX, startY+=spaceY, 300, 15), cont, attackMethod, contL);
			creepA.attackMethod=(_AttackMethod)attackMethod;
			
			int cdTracking=(int)creepA.cdTracking;
			cont=new GUIContent("CDTracking:", "select cooldown tracking mode");
			contL=new GUIContent[cdTrackingLabel.Length];
			for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(cdTrackingLabel[i], cdTrackingTooltip[i]);
			cdTracking = EditorGUI.Popup(new Rect(startX, startY+=spaceY, 300, 15), cont, cdTracking, contL);
			creepA.cdTracking=(_CDTracking)cdTracking;
			
			startY+=5;
			
			if(creepA.attackMethod==_AttackMethod.Range) 
				creepA.range = EditorGUI.FloatField(new Rect(startX, startY+=spaceY, 300, height-3), "AttackRange:", creepA.range);
			else if(creepA.attackMethod==_AttackMethod.Melee) 
				creepA.range = EditorGUI.FloatField(new Rect(startX, startY+=spaceY, 300, height-3), "TargetRange:", creepA.range);
			
			
			creepA.cooldown = EditorGUI.FloatField(new Rect(startX, startY+=spaceY, 300, height-3), "AttackCooldown:", creepA.cooldown);
			creepA.damage = EditorGUI.FloatField(new Rect(startX, startY+=spaceY, 300, height-3), "Damage:", creepA.damage);
			creepA.damageType = EditorGUI.IntField(new Rect(startX, startY+=spaceY, 300, height-3), "DamageType:", creepA.damageType);
			creepA.stun = EditorGUI.FloatField(new Rect(startX, startY+=spaceY, 300, height-3), "StunDuration:", creepA.stun);
			
			startY+=5;
			
			if(creepA.attackMethod==_AttackMethod.Range){
				creepA.turretObject=(Transform)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), "TurretObject:", creepA.turretObject, typeof(Transform), false);
				
				if(creepA.turretObject!=null){
					int turretAnimateMode=(int)creepA.animateTurret;
					cont=new GUIContent("TurretAnimateMode:", "How turret rotation behaves in reaction to target");
					contL=new GUIContent[turretAnimateModeLabel.Length];
					for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(turretAnimateModeLabel[i], turretAnimateModeTooltip[i]);
					turretAnimateMode = EditorGUI.Popup(new Rect(startX, startY+=20, 300, 15), cont, turretAnimateMode, contL);
					//~ turretAnimateMode = EditorGUI.Popup(new Rect(startX, startY+=20, 300, 15), "TurretAnimateMode:", turretAnimateMode, turretAnimateModeLabel);
					creepA.animateTurret=(_TurretAni)turretAnimateMode;
						
					if(turretAnimateMode==0){
						int turretRotationMode=(int)creepA.turretRotationModel;
						cont=new GUIContent("TurretRotationMode:", "The rotation mode of turret with respect to barrel");
						contL=new GUIContent[turretRotationModeLabel.Length];
						for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(turretRotationModeLabel[i], turretRotationModeTooltip[i]);
						turretRotationMode = EditorGUI.Popup(new Rect(startX, startY+=20, 300, 15), cont, turretRotationMode, contL);
						creepA.turretRotationModel=(_RotationMode)turretRotationMode;
					}
					else startY+=20;
					
					int turretLOSMode=(int)creepA.losMode;
					cont=new GUIContent("TurretLOSMode:", "The line of sight mechanic used with the tower");
					contL=new GUIContent[turretLOSModeLabel.Length];
					for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(turretLOSModeLabel[i], turretLOSModeTooltip[i]);
					turretLOSMode = EditorGUI.Popup(new Rect(startX, startY+=20, 300, 15), cont, turretLOSMode, contL);
					creepA.losMode=(_LOSMode)turretLOSMode;
				}
				
				int spLength=creepA.shootPoint.Length;
				cont=new GUIContent("ShootPointCount:", "number of shootpoint. each shootpoint is a empty transform indicate position where the shootObject should be fired from");
				spLength=EditorGUI.IntField(new Rect(startX, startY+=spaceY, 300, height-3), cont, spLength);	startY-=1;
				if(spLength!=creepA.shootPoint.Length) creepA.shootPoint=CloneTransformList(creepA.shootPoint, spLength);
				for(int i=0; i<creepA.shootPoint.Length; i++){
					creepA.shootPoint[i]=(Transform)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), " 	- "+i+": ", creepA.shootPoint[i], typeof(Transform), false);
				}
				
				cont=new GUIContent("ShootObject:", "the object to be fired by the creep range attack (must contain a ShootObject component)");
				creepA.shootObject=(GameObject)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), cont, creepA.shootObject, typeof(GameObject), false);
			}
			
			startY+=5;
			
			creepA.attackSound=(AudioClip)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), "AttackSound:", creepA.attackSound, typeof(AudioClip), false);
			creepA.animationIdle=(AnimationClip)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), "IdleAnimation:", creepA.animationIdle, typeof(AnimationClip), false);
			creepA.animationAttack=(AnimationClip)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), "AttackAnimation:", creepA.animationAttack, typeof(AnimationClip), false);
			if(creepA.animationAttack!=null) creepA.aniAttackTimeOffset = EditorGUI.FloatField(new Rect(startX, startY+=spaceY, 300, height-3), "AttackAnimationTimeOffset::", creepA.aniAttackTimeOffset);
		
		}
		else if(creepA.type==_AttackCreepType.Support){
			//~ if(creep.attackMode==_AttackMode.RunNGun) attackMode=0;
			
			creepA.range = EditorGUI.FloatField(new Rect(startX, startY+=spaceY, 300, height-3), "EffectiveRange:", creepA.range);
			
			startY+=10;
			
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 200, 20), "Buff:");
			creepA.buff.damageBuff = EditorGUI.FloatField(new Rect(startX, startY+=spaceY, 300, height-3), " - DamageBuff:", creepA.buff.damageBuff);
			creepA.buff.rangeBuff = EditorGUI.FloatField(new Rect(startX, startY+=spaceY, 300, height-3), " - RangeBuff:", creepA.buff.rangeBuff);
			creepA.buff.cooldownBuff = EditorGUI.FloatField(new Rect(startX, startY+=spaceY, 300, height-3), " - cooldownBuff:", creepA.buff.cooldownBuff);
			creepA.buff.regenHP = EditorGUI.FloatField(new Rect(startX, startY+=spaceY, 300, height-3), " - HPRegenRate:", creepA.buff.regenHP);
			
			startY+=10;
			
			//~ int spLength=creepA.sp.Length;
			//~ spLength=EditorGUI.IntField(new Rect(startX, startY+=spaceY, 300, height-3), "ShootPointCount:", spLength);	startY-=1;
			//~ if(spLength!=creepA.sp.Length) creepA.sp=CloneTransformList(creepA.sp, spLength);
			//~ for(int i=0; i<creepA.sp.Length; i++){
				//~ creepA.sp[i]=(Transform)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), " 	- "+i+": ", creepA.sp[i], typeof(Transform), false);
			//~ }
			//~ creepA.shootObject=(GameObject)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), "ShootObject:", creepA.shootObject, typeof(GameObject), false);
			//~ creepA.cooldown = EditorGUI.FloatField(new Rect(startX, startY+=spaceY, 300, height-3), "EffectCooldown:", creepA.cooldown);
			creepA.animationIdle=(AnimationClip)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), "IdleAnimation:", creepA.animationIdle, typeof(AnimationClip), false);
			
		}
		
		
		return startY;
	}
	
	
	void HPAttributeEditor(int index){
		float startY=133;
		float startX=340;
		
		float space=105;
		
		UnitCreep creep=creepList[index];
		
		EditorGUI.LabelField(new Rect(startX, startY, 200, height), "Creep HP: ");
		creep.HPAttribute.fullHPDefault = EditorGUI.FloatField(new Rect(startX+space, startY, 50, height-2), creep.HPAttribute.fullHPDefault);
		//~ creep.HPAttribute.HP=creep.HPAttribute.fullHP;
		
		EditorGUI.LabelField(new Rect(startX, startY+20, 200, height), "Creep Shield: ");
		creep.HPAttribute.fullShieldDefault = EditorGUI.FloatField(new Rect(startX+space, startY+=20, 50, height-2), creep.HPAttribute.fullShieldDefault);
		
		if(creep.HPAttribute.fullShieldDefault>0){
			EditorGUI.LabelField(new Rect(startX, startY+20, 200, height), "Shield Recharge: ");
			creep.HPAttribute.shieldRechargeRate = EditorGUI.FloatField(new Rect(startX+space, startY+=20, 50, height-2), creep.HPAttribute.shieldRechargeRate);
			
			EditorGUI.LabelField(new Rect(startX, startY+20, 200, height), "Shield Stagger: ");
			creep.HPAttribute.shieldStagger = EditorGUI.FloatField(new Rect(startX+space, startY+=20, 50, height-2), creep.HPAttribute.shieldStagger);
		}
		
		startY=133;
		startX=340+105+50+10;
		space=100;
		
		EditorGUI.LabelField(new Rect(startX, startY, 200, height), "HP Overlay: ");
		creep.HPAttribute.overlayHP=(Transform)EditorGUI.ObjectField(new Rect(startX+space, startY, 100, height-2), creep.HPAttribute.overlayHP, typeof(Transform), false);
		
		EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 200, height), "Shield Overlay: ");
		creep.HPAttribute.overlayShield=(Transform)EditorGUI.ObjectField(new Rect(startX+space, startY, 100, height-2), creep.HPAttribute.overlayShield, typeof(Transform), false);
		
		EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 200, height), "Base Overlay: ");
		creep.HPAttribute.overlayBase=(Transform)EditorGUI.ObjectField(new Rect(startX+space, startY, 100, height-2), creep.HPAttribute.overlayBase, typeof(Transform), false);
		
		EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 200, height), "Always Show Overlay: ");
		creep.HPAttribute.alwaysShowOverlay= EditorGUI.Toggle(new Rect(startX+space+40, startY, 100, height-2), creep.HPAttribute.alwaysShowOverlay);
	}
	
	
	Transform[] CloneTransformList(Transform[] oldList, int count){
		Transform[] newList=new Transform[count];
		
		for(int i=0; i<newList.Length; i++){
			if(i<oldList.Length) newList[i]=oldList[i];
			else newList[i]=null;
		}
		
		return newList;
	}

}





