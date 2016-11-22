	using UnityEngine;
	using System.Collections;
	
	public class GeneralRoll : MonoBehaviour 
	{

		public Transform endPos;
		public Transform stPos;
		
		public float minSwipeDistY;
		
		public float minSwipeDistX;

		/*public Collider Top; //не видит коллаидеры обьектов донера

		public Collider Bot;
		
		public Collider Left;

		public Collider Right;*/
		
		private GameObject doner;

		private bool BottomCol = false;

		private bool TopCol = false;

		private bool RightCol = false;

		private bool LeftCol = false;
		
		private Vector2 startPos;

		private GameObject trash;		

	    private Collider coll;

	/*void OnCollisionEnter (Collision col) //работает для сталкивающися тел
	{
		if(col.gameObject.tag == "Bottom")
		{
			BottomCol = true;
		}
	}*/


	private int startID = 0;


	void Start() {
		doner = GameObject.Find ("Doner");
		coll = GetComponent<Collider>();
			
		}

	private float vel = 0.0F;
		void Update()
		{

		//#if UNITY_ANDROID
		if (Input.touchCount > 0) 
				
			{
				
				Touch touch = Input.touches[0];
				
				
				
				switch (touch.phase) 
					
				{
				case TouchPhase.Stationary:
				case TouchPhase.Moved:
			{
				float swipeDistVertical2 = (new Vector3(0, touch.position.y, 0) - new Vector3(0, startPos.y, 0)).magnitude; //отслеживание позиции вертикального свайпа
				
				float swipeValue2 = Mathf.Abs(touch.position.y - Camera.main.WorldToScreenPoint(stPos.position).y) / Mathf.Abs(Camera.main.WorldToScreenPoint(endPos.position).y - Camera.main.WorldToScreenPoint(stPos.position).y);//свайп по у

				Debug.Log(swipeValue2);
				Debug.Log(startID);

				if(startID == 1) {
					float oldTime = doner.animation["Take 001"].normalizedTime;
					doner.animation["Take 001"].normalizedTime = Mathf.SmoothDamp(oldTime, Mathf.Clamp01(swipeValue2), ref vel, 0.06f);	
				}
			}
				break;
					
				case TouchPhase.Began:
					startID = 0;
				vel = 0.0F;
					
					float swipeDistVertical = (new Vector3(0, touch.position.y, 0) - new Vector3(0, startPos.y, 0)).magnitude; //отслеживание позиции вертикального свайпа
				
					float swipeValue = Mathf.Sign(touch.position.y - startPos.y);//свайп по у

					Ray ray = Camera.main.ScreenPointToRay (touch.position);

					RaycastHit hit = new RaycastHit();

					if (coll.Raycast (ray, out hit, 100F)) {

					//if (swipeDistVertical > minSwipeDistY) //если свайп вертикальный
						
					{
							
							if (hit.collider.name == "Bot"/* && swipeValue >= 0*/){

								doner.animation.Play ("Take 001");								
							doner.animation["Take 001"].normalizedSpeed = 0;
							startID = 1;
							Debug.LogError("!!!!!");
								
								
							}	
							
							else if (hit.collider.name == "Top" && swipeValue < 0){
								
								doner.animation.Play ("Take 0010");
							startID = 2;
							
							}

							else if (hit.collider.name == "Left")
									
								LeftCol = true;
								


							else if (hit.collider.name == "Right")
									
								RightCol = true;
							
						}
					}

					startPos = touch.position;
					
					break;
					
					
					
				case TouchPhase.Ended:
					
					
				/*	float swipeDistHorizontal = (new Vector3(touch.position.x,0, 0) - new Vector3(startPos.x, 0, 0)).magnitude;
					
					if (swipeDistHorizontal > minSwipeDistX) 
						
					{
						
						float swipeValue = Mathf.Sign(touch.position.x - startPos.x);
						
						if (swipeValue > 0 && LeftCol == true){//right swipe
							
							doner.animation.Play ("Take 0011");

							RightCol = false;
						
							/*trash = GameObject.FindWithTag("Left");

							Destroy (trash);
						 	
					}

						else if (swipeValue < 0 && RightCol == true)//left swipe
					
							doner.animation.Play ("Take 0012");

							LeftCol = false;

							/*trash = GameObject.FindWithTag("Right");
							
							Destroy (trash);
					
					}*/
					break;
				}
			}
		}
	}