using UnityEngine;
using System.Collections;

#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private field assigned but not used.

[AddComponentMenu("TDTK/Optional/CameraControl")]
public class CameraControl : MonoBehaviour {

	//public enum _Platform{Hybird, Mouse&Keyboard, Touch}
	//public _Platform platform;

	public float panSpeed=5;
	public float zoomSpeed=5;
	
	private float initialMousePosX;
	private float initialMousePosY;
	
	private float initialRotX;
	private float initialRotY;
	
	public bool enableMouseRotate=true;
	public bool enableMousePanning=false;
	public bool enableKeyPanning=true;
	
	public int mousePanningZoneWidth=10;
	
	//for iOS touch input panning
	public bool iOSEnablePan=true;
	private Vector3 lastTouchPos=new Vector3(9999, 9999, 9999);
	private Vector3 moveDir=Vector3.zero;
	
	public bool iOSEnableZoom=true;
	private float touchZoomSpeed;
	
	public bool iOSEnableRotate=false;
	public float rotationSpeed=1;
	
	public float minPosX=-10;
	public float maxPosX=10;
	
	public float minPosZ=-10;
	public float maxPosZ=10;
	
	public float minRadius=8;
	public float maxRadius=30;
	
	public float minRotateAngle=10;
	public float maxRotateAngle=89;

	//calculated deltaTime based on timeScale so camera movement speed always remain constant
	private float deltaT;
	
	private Transform cam;
	private Transform thisT;
	
	public bool positionalZoom=true;
	public float transitionSpeed=3;
	//~ public int zoomLayer=15;
	public float colliderHeight=-1;
	private Vector3 zoomScreenPos;
	private Vector3 zoomPosAdjust=Vector3.zero;
	private LayerMask zoomMask;
	

	void Awake(){
		thisT=transform;
		
		cam=Camera.main.transform;
	}
	
	// Use this for initialization
	void Start () {
		minRotateAngle=Mathf.Max(10, minRotateAngle);
		maxRotateAngle=Mathf.Min(89, maxRotateAngle);
		
		minRadius=Mathf.Max(1, minRadius);
		
		if(positionalZoom){
			GameObject obj=new GameObject();
			BoxCollider col=obj.AddComponent<BoxCollider>();
			//~ col.size=new Vector3(Mathf.Infinity, 0, Mathf.Infinity);
			col.size=new Vector3(9999999, 0, 9999999);
			obj.layer=LayerManager.LayerTerrain();
			obj.name="TerrainCollider";
			//obj.transform.parent=thisT;
			obj.transform.position=new Vector3(0, colliderHeight, 0);

			zoomMask=1<<LayerManager.LayerTerrain();
		}
	}
	
	// Update is called once per frame
	void Update () {
		
		if(Time.timeScale==1) deltaT=Time.deltaTime;
		else if(Time.timeScale>1) deltaT=Time.deltaTime/Time.timeScale;
		else deltaT=0.015f;

		
		#if UNITY_IPHONE || UNITY_ANDROID
	
		if(iOSEnablePan){
			Quaternion camDir=Quaternion.Euler(0, transform.eulerAngles.y, 0);
			if(Input.touchCount==1){
				Touch touch=Input.touches[0];
				if (touch.phase == TouchPhase.Moved){
					Vector3 deltaPos = touch.position;
					
					if(lastTouchPos!=new Vector3(9999, 9999, 9999)){
						deltaPos=deltaPos-lastTouchPos;
						moveDir=new Vector3(deltaPos.x, 0, deltaPos.y).normalized*-1;
					}
	
					lastTouchPos=touch.position;
				}
			}
			else lastTouchPos=new Vector3(9999, 9999, 9999);
			
			Vector3 dir=thisT.InverseTransformDirection(camDir*moveDir)*1.5f;
			thisT.Translate (dir * panSpeed * deltaT);
			
			moveDir=moveDir*(1-deltaT*5);
		}
		
		if(iOSEnableZoom){
			if(Input.touchCount==2){
				Touch touch1 = Input.touches[0];
				Touch touch2 = Input.touches[1];
				
				zoomScreenPos=(touch1.position+touch2.position)/2;
				
				if(touch1.phase==TouchPhase.Moved && touch1.phase==TouchPhase.Moved){
					//float dot=Vector2.Dot(touch1.deltaPosition, touch1.deltaPosition);
					Vector3 dirDelta=(touch1.position-touch1.deltaPosition)-(touch2.position-touch2.deltaPosition);
					Vector3 dir=touch1.position-touch2.position;
					float dot=Vector3.Dot(dirDelta.normalized, dir.normalized);
					
					if(Mathf.Abs(dot)>0.7f){	
						touchZoomSpeed=dir.magnitude-dirDelta.magnitude;
					}	
				}
				
			}
			
			//cam.Translate(Vector3.forward * touchZoomSpeed * zoomSpeed * Time.deltaTime * 0.1f);
			
			if(touchZoomSpeed<0){
				if(Vector3.Distance(cam.position, thisT.position)<maxRadius){
					cam.Translate(Vector3.forward*Time.deltaTime*zoomSpeed*touchZoomSpeed);
				}
			}
			else if(touchZoomSpeed>0){
				if(Vector3.Distance(cam.position, thisT.position)>minRadius){
					PositionalZooming(zoomScreenPos);
					cam.Translate(Vector3.forward*Time.deltaTime*zoomSpeed*touchZoomSpeed);
				}
			}
			
			//~ if(cam.transform.localPosition.z>0){
				//~ cam.localPosition=new Vector3(cam.localPosition.x, cam.localPosition.y, 0);
			//~ }
				
			touchZoomSpeed=touchZoomSpeed*(1-Time.deltaTime*5);
		}
		
		if(iOSEnableRotate){
			if(Input.touchCount==2){
				Touch touch1 = Input.touches[0];
				Touch touch2 = Input.touches[1];
				
				Vector2 delta1=touch1.deltaPosition.normalized;
				Vector2 delta2=touch2.deltaPosition.normalized;
				Vector2 delta=(delta1+delta2)/2;
				
				float rotX=thisT.rotation.eulerAngles.x-delta.y*rotationSpeed;
				float rotY=thisT.rotation.eulerAngles.y+delta.x*rotationSpeed;
				rotX=Mathf.Clamp(rotX, minRotateAngle, maxRotateAngle);
				
				Quaternion rot=Quaternion.Euler(delta.y, delta.x, 0);
				//Debug.Log(rotX+"   "+rotY);
				thisT.rotation=Quaternion.Euler(rotX, rotY, 0);
				//thisT.rotation*=rot;
			}
		}
		
		
		
		#endif
		
		#if UNITY_EDITOR || (!UNITY_IPHONE && !UNITY_ANDROID)
		
		//mouse and keyboard
		if(enableMouseRotate){
			if(Input.GetMouseButtonDown(1)){
				initialMousePosX=Input.mousePosition.x;
				initialMousePosY=Input.mousePosition.y;
				initialRotX=thisT.eulerAngles.y;
				initialRotY=thisT.eulerAngles.x;
			}

			if(Input.GetMouseButton(1)){
				float deltaX=Input.mousePosition.x-initialMousePosX;
				float deltaRotX=(.1f*(initialRotX/Screen.width));
				float rotX=deltaX+deltaRotX;
				
				float deltaY=initialMousePosY-Input.mousePosition.y;
				float deltaRotY=-(.1f*(initialRotY/Screen.height));
				float rotY=deltaY+deltaRotY;
				float y=rotY+initialRotY;
				
				//limit the rotation
				if(y>maxRotateAngle){
					initialRotY-=(rotY+initialRotY)-maxRotateAngle;
					y=maxRotateAngle;
				}
				else if(y<minRotateAngle){
					initialRotY+=minRotateAngle-(rotY+initialRotY);
					y=minRotateAngle;
				}
				
				thisT.rotation=Quaternion.Euler(y, rotX+initialRotX, 0);
			}
		}	
			
		Quaternion direction=Quaternion.Euler(0, thisT.eulerAngles.y, 0);
		
		if(enableKeyPanning){
			if(Input.GetButton("Horizontal")) {
				Vector3 dir=transform.InverseTransformDirection(direction*Vector3.right);
				thisT.Translate (dir * panSpeed * deltaT * Input.GetAxisRaw("Horizontal"));
			}

			if(Input.GetButton("Vertical")) {
				Vector3 dir=transform.InverseTransformDirection(direction*Vector3.forward);
				thisT.Translate (dir * panSpeed * deltaT * Input.GetAxisRaw("Vertical"));
			}
		}
		
		//cam.Translate(Vector3.forward*zoomSpeed*Input.GetAxis("Mouse ScrollWheel"));
		
		if(Input.GetAxis("Mouse ScrollWheel")<0){
			//~ if(Vector3.Distance(cam.position, thisT.position)<maxRadius){
				cam.Translate(Vector3.forward*zoomSpeed*Input.GetAxis("Mouse ScrollWheel"));
				//PositionalZooming(Input.mousePosition);
			//~ }
		}
		else if(Input.GetAxis("Mouse ScrollWheel")>0){
			//~ if(Vector3.Distance(cam.position, thisT.position)>minRadius){
				cam.Translate(Vector3.forward*zoomSpeed*Input.GetAxis("Mouse ScrollWheel"));
				PositionalZooming(Input.mousePosition);
			//~ }
		}
		
		if(enableMousePanning){
			Vector3 mousePos=Input.mousePosition;
			Vector3 dirHor=transform.InverseTransformDirection(direction*Vector3.right);
			if(mousePos.x<=0){
				thisT.Translate(dirHor * panSpeed * deltaT * -3);
			}
			else if(mousePos.x<=mousePanningZoneWidth){
				thisT.Translate(dirHor * panSpeed * deltaT * -1);
			}
			else if(mousePos.x>=Screen.width){
				thisT.Translate(dirHor * panSpeed * deltaT * 3);
			}
			else if(mousePos.x>Screen.width-mousePanningZoneWidth){
				thisT.Translate(dirHor * panSpeed * deltaT * 1);
			}
			
			Vector3 dirVer=transform.InverseTransformDirection(direction*Vector3.forward);
			if(mousePos.y<=0){
				thisT.Translate(dirVer * panSpeed * deltaT * -3);
			}
			else if(mousePos.y<=mousePanningZoneWidth){
				thisT.Translate(dirVer * panSpeed * deltaT * -1);
			}
			else if(mousePos.y>=Screen.height){
				thisT.Translate(dirVer * panSpeed * deltaT * 3);
			}
			else if(mousePos.y>Screen.height-mousePanningZoneWidth){
				thisT.Translate(dirVer * panSpeed * deltaT * 1);
			}
		}
		
		//thisT.Translate(cam.forward*zoomSpeed*Input.GetAxis("Mouse ScrollWheel"), Space.World);
		
		#endif
		
		float camZ=Mathf.Clamp(cam.localPosition.z, -maxRadius, -minRadius);
		cam.localPosition=new Vector3(cam.localPosition.x, cam.localPosition.y, camZ);
		
		float x=Mathf.Clamp(thisT.position.x, minPosX, maxPosX);
		float z=Mathf.Clamp(thisT.position.z, minPosZ, maxPosZ);
		//float y=Mathf.Clamp(thisT.position.y, verticalLimitBottom, verticalLimitTop);
		
		thisT.position=new Vector3(x, thisT.position.y, z);
		
	}
	
	void PositionalZooming(Vector3 pointer){
		Ray ray = Camera.main.ScreenPointToRay(pointer);
		RaycastHit hit;
		if(Physics.Raycast(ray, out hit, Mathf.Infinity, zoomMask)){
			//Vector3 dir=hit.point-thisT.position;
			hit.point=new Vector3(hit.point.x, thisT.position.y, hit.point.z);
			#if UNITY_IPHONE || UNITY_ANDROID
			thisT.position=Vector3.Lerp(thisT.position, hit.point, Time.deltaTime*transitionSpeed*touchZoomSpeed);
			#else
			thisT.position=Vector3.Lerp(thisT.position, hit.point, Time.deltaTime*transitionSpeed);
			#endif
		}
	}
	
	public bool showGizmo=true;
	void OnDrawGizmos(){
		if(showGizmo){
			Vector3 p1=new Vector3(minPosX, transform.position.y, maxPosZ);
			Vector3 p2=new Vector3(maxPosX, transform.position.y, maxPosZ);
			Vector3 p3=new Vector3(maxPosX, transform.position.y, minPosZ);
			Vector3 p4=new Vector3(minPosX, transform.position.y, minPosZ);
			
			Gizmos.color=Color.green;
			Gizmos.DrawLine(p1, p2);
			Gizmos.DrawLine(p2, p3);
			Gizmos.DrawLine(p3, p4);
			Gizmos.DrawLine(p4, p1);
		}
	}
	
}
