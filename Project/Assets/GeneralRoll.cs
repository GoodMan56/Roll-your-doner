	using UnityEngine;
	using System.Collections;
	
	public class GeneralRoll : MonoBehaviour 
	{
		
		public float minSwipeDistY;
		
		public float minSwipeDistX;
		
		private Vector2 startPos;

		private GameObject doner;
		void Start() {
//		coll = GetComponent<Collider>();
		doner = GameObject.Find("Doner");
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
					
					startPos = touch.position;
					
					break;
					
					
					
				case TouchPhase.Ended:
					
					float swipeDistVertical = (new Vector3(0, touch.position.y, 0) - new Vector3(0, startPos.y, 0)).magnitude;
					
					if (swipeDistVertical > minSwipeDistY) 
						
					{
						
						float swipeValue = Mathf.Sign(touch.position.y - startPos.y);
						
						if (swipeValue > 0)//up swipe
							
							doner.animation.Play ("Take 001");
							
						else if (swipeValue < 0)//down swipe
								
							//doner.animation.Play ("Take 0010");
							doner.animation["Take001"].speed= -1;
							doner.animation.Play ("Take 001");
					
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