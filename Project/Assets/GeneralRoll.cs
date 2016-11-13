	using UnityEngine;
	using System.Collections;
	
	public class GeneralRoll : MonoBehaviour 
	{
		
		public float minSwipeDistY;
		
		public float minSwipeDistX;

		/*public Collider Top; //не видит коллаидеры родительских обьектов донера

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


	void Start() {
		doner = GameObject.Find ("Doner");
		coll = GetComponent<Collider>();
			
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

					RaycastHit hit = new RaycastHit();

					if (coll.Raycast (ray, out hit, 100F)) {

						
						if (hit.collider.name == "Bot")

							BottomCol = true;
							

						
						else if (hit.collider.name == "Top")
							
							TopCol = true;
						


						else if (hit.collider.name == "Left")
								
							LeftCol = true;
							


						else if (hit.collider.name == "Right")
								
							RightCol = true;
						
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
							
							/*trash = GameObject.FindWithTag("Bot");
							
							Destroy (trash);*/
					}

						else if (swipeValue < 0 && TopCol == true)//down swipe
								
							doner.animation.Play ("Take 0010");

							TopCol = false;

							/*trash = GameObject.FindWithTag("Top");
							
							Destroy (trash);*/
					
					}
					
					float swipeDistHorizontal = (new Vector3(touch.position.x,0, 0) - new Vector3(startPos.x, 0, 0)).magnitude;
					
					if (swipeDistHorizontal > minSwipeDistX) 
						
					{
						
						float swipeValue = Mathf.Sign(touch.position.x - startPos.x);
						
						if (swipeValue > 0 && LeftCol == true){//right swipe
							
							doner.animation.Play ("Take 0011");

							RightCol = false;
						
							/*trash = GameObject.FindWithTag("Left");

							Destroy (trash);*/
						 	
					}

						else if (swipeValue < 0 && RightCol == true)//left swipe
					
							doner.animation.Play ("Take 0012");

							LeftCol = false;

							/*trash = GameObject.FindWithTag("Right");
							
							Destroy (trash);*/
					
					}
					break;
				}
			}
		}
	}