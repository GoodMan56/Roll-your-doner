	using UnityEngine;
	using System.Collections;
	
	public class GeneralRoll : MonoBehaviour 
	{
		
		public float minSwipeDistY;
		
		public float minSwipeDistX;
		
		private bool BottomCol = false;
		
		private Vector2 startPos;

		private GameObject doner;
	

	/*void OnCollisionEnter (Collision col) //работает для сталкивающися тел
	{
		if(col.gameObject.tag == "Bottom")
		{
			BottomCol = true;
		}
	}*/


	void Start() {
		doner = GameObject.Find ("Doner");
		//		coll = GetComponent<Collider>();
			
		}


		void Update()
		{

		//#if UNITY_ANDROID
		if (Input.touchCount > 0) 
				
			{
				
				Touch touch = Input.touches[0];
				
				
				
				switch (touch.phase) 
					
				{
					
				case TouchPhase.Began:

					Ray ray = Camera.main.ScreenPointToRay (touch.position);

					RaycastHit hit = new RaycastHit();;

					if (collider.Raycast (ray, out hit, 100F)) {

						if (hit.collider.tag == "Bottom")

							BottomCol = true;

					}
					
					startPos = touch.position;
					
					break;
					
					
					
				case TouchPhase.Ended:
					
					float swipeDistVertical = (new Vector3(0, touch.position.y, 0) - new Vector3(0, startPos.y, 0)).magnitude;
					
					if (swipeDistVertical > minSwipeDistY) 
						
					{
						
						float swipeValue = Mathf.Sign(touch.position.y - startPos.y);
						
					if (swipeValue > 0 && BottomCol == true){//up swipe
							
							doner.animation.Play ("Take 001");
							BottomCol = false;
					}

						else if (swipeValue < 0)//down swipe
								
							doner.animation.Play ("Take 0010");
					
					}
					
					float swipeDistHorizontal = (new Vector3(touch.position.x,0, 0) - new Vector3(startPos.x, 0, 0)).magnitude;
					
					if (swipeDistHorizontal > minSwipeDistX) 
						
					{
						
						float swipeValue = Mathf.Sign(touch.position.x - startPos.x);
						
						if (swipeValue > 0)//right swipe
							
							doner.animation.Play ("Take 0011");
					
						/*else if (swipeValue < 0)//left swipe
								
							doner.animation.Play ("Take 001");*/
					
					}
					break;
				}
			}
		}
	}