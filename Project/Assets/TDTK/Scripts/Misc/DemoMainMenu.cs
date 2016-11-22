using UnityEngine;
using System.Collections;

public class DemoMainMenu : MonoBehaviour {

	private string[] tooltip;
	
	// Use this for initialization
	void Start () {
		tooltip=new string[8];
		tooltip[0]="Basic level with a linear path and a tier-based tower unlock progression system\nUses default UI and 'Point&Build' building scheme with pie-menu";
		
		
		tooltip[1]="Level with a linear looping path, with simple tech-tree for towers. Abilities are enabled\nUses NGUI and 'Point&Build' building scheme";
		
		
		tooltip[2]="Level with multiple linear path. Featuring a simple tier-based tower unlock progression system\nUses NGUI and 'Point&Build' building scheme with pie-menu";
		
		//level 4 - customed shape platform
		tooltip[3]="A mazing level with multiple paths, using a single custom shaped platform as the play area\nFeaturing simple ability system & independent tech progression for each towers\nUses NGUI and 'Drag&Drop' building scheme";
		
		//level 5 - 3 platforms with varied height
		tooltip[4]="A mazing level with multiple paths traversing multiple platforms sections. Featuring a shop with variety of perks and tower upgrade\nUses default UI and 'Drag&Drop' building scheme";
		
		//level 6 - full terrain
		tooltip[5]="A level showcasing terrain integration all possible tower types. There's no perk or ability in this level\nUses NGUI and 'Drag&Drop' building scheme";
		
		//level 7 - enless mode double looping path. focus on ability
		tooltip[6]="An endless level with randomized procedural generated waves.\nA showcase focuses on both perk and ability system, as well as the synergy between them. Uses default UI";
		
		tooltip[7]="An endless level with randomized procedural generated waves.\nA showcase focuses on both perk and ability system, as well as the synergy between them. Uses NGUI";
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnGUI(){
		//~ if(GUI.Button(new Rect(Screen.width/2-50, Screen.height/2-80, 100, 30), "Demo 1")){
			//~ Application.LoadLevel("ExampleScene1");
		//~ }
		//~ if(GUI.Button(new Rect(Screen.width/2-50, Screen.height/2-80+45, 100, 30), "Demo 2")){
			//~ Application.LoadLevel("ExampleScene2");
		//~ }
		//~ if(GUI.Button(new Rect(Screen.width/2-50, Screen.height/2-80+90, 100, 30), "Demo 3")){
			//~ Application.LoadLevel("ExampleScene3");
		//~ }
		//~ if(GUI.Button(new Rect(Screen.width/2-50, Screen.height/2-80+135, 100, 30), "Demo 4")){
			//~ Application.LoadLevel("ExampleScene4");
		//~ }
		//~ if(GUI.Button(new Rect(Screen.width/2-50, Screen.height/2-80+180, 100, 30), "Demo 1 nGUI")){
			//~ Application.LoadLevel("ExampleScene1(nGUI)");
		//~ }
		//~ if(GUI.Button(new Rect(Screen.width/2-50, Screen.height/2-80+225, 100, 30), "Demo 2 nGUI")){
			//~ Application.LoadLevel("ExampleScene2(nGUI)");
		//~ }
		
		float startY=Screen.height/2-200;
		float spaceY=45;
		
		GUIContent content=new GUIContent("Starter", "1");
		if(GUI.Button(new Rect(Screen.width/2-60, startY+=spaceY, 120, 35), content)){
			Application.LoadLevel("ExampleLinearSinglePath");
		}
		content=new GUIContent("Loop", "2");
		if(GUI.Button(new Rect(Screen.width/2-60, startY+=spaceY, 120, 35), content)){
			Application.LoadLevel("ExampleLinearSinglePathLoop");
		}
		content=new GUIContent("Parallel", "3");
		if(GUI.Button(new Rect(Screen.width/2-60, startY+=spaceY, 120, 30), content)){
			Application.LoadLevel("ExampleLinearMultiPath");
		}
		content=new GUIContent("Blockade", "4");
		if(GUI.Button(new Rect(Screen.width/2-60, startY+=spaceY, 120, 35), content)){
			Application.LoadLevel("ExampleMazeMultiPath");
		}
		content=new GUIContent("Islands Hopping", "5");
		if(GUI.Button(new Rect(Screen.width/2-60, startY+=spaceY, 120, 35), content)){
			Application.LoadLevel("ExampleMazeMultiPath&Platform");
		}
		content=new GUIContent("Terrain", "6");
		if(GUI.Button(new Rect(Screen.width/2-60, startY+=spaceY, 120, 35), content)){
			Application.LoadLevel("ExampleMazeTerrain");
		}
		content=new GUIContent("Survival(UI)", "7");
		if(GUI.Button(new Rect(Screen.width/2-60, startY+=spaceY, 120, 35), content)){
			Application.LoadLevel("ExampleLinearEndlessUI");
		}
		content=new GUIContent("Survival(NGUI)", "8");
		if(GUI.Button(new Rect(Screen.width/2-60, startY+=spaceY, 120, 35), content)){
			Application.LoadLevel("ExampleLinearEndlessNGUI");
		}
		
		
		if(GUI.tooltip!=""){
			int ID=int.Parse(GUI.tooltip)-1;
			
			GUIStyle style=new GUIStyle();
			style.alignment=TextAnchor.UpperCenter;
			style.fontStyle=FontStyle.Bold;
			style.normal.textColor=Color.white;
			
			GUI.Label(new Rect(0, Screen.height*0.75f+70, Screen.width, 200), tooltip[ID], style);
		}
	}
}
