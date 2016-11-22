using UnityEngine;
using System.Collections;

public class Instruction : MonoBehaviour {

	private string camInst="";
	
	private bool show=false;
	
	// Use this for initialization
	void Start () {
		camInst="";
		camInst+="- w,a,s,d key to pan the camera\n";
		camInst+="- right click and drag to rotate the view angle\n";
		camInst+="- mouse wheel to zoom\n";
		camInst+="- touch input supported\n";
		
		camInst+="\nFor PointNBuild mode, left click on any of the platform to bring up the build menu\n";
		
		camInst+="\nFor abilities, after select one from the button,\nright-click to trigger, left-click to cancel\n";
		//camInst+="\n- mouse wheel to zoom\n";
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnGUI(){
		for(int i=0; i<2; i++) GUI.Box(new Rect(Screen.width-315, 7, 90, 41), "");
		if(GUI.Button(new Rect(Screen.width-310, 10, 80, 35), "Instruction")){
			show=!show;
		}
		
		if(show) GUI.Label(new Rect(Screen.width-310, 50, 300, 500), camInst);
	}
}
