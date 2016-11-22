using UnityEngine;
using System.Collections;

public class TextureScroll : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public int materialIndex = 0;
    public Vector2 uvAnimationRate = new Vector2( 1.0f, 0.0f );
    public string textureName = "_MainTex";

    Vector2 uvOffset = Vector2.zero;

    void LateUpdate() 
    {
        uvOffset += ( uvAnimationRate * Time.deltaTime );
        if( renderer.enabled )
        {
            renderer.materials[ materialIndex ].SetTextureOffset( textureName, uvOffset );
		}
    }
	
	void OnEnable(){
		StartCoroutine(ShieldEffect());
	}
	
	IEnumerator ShieldEffect(){
		StartCoroutine(FadeIn());
		yield return new WaitForSeconds(6.5f);
		StartCoroutine(FadeOut());
	}
	
	IEnumerator FadeIn(){
		float duration=0;
		while(duration<1){
			transform.localScale=Vector3.Lerp(Vector3.zero, new Vector3(6, 6, 6), duration);
			renderer.materials[ materialIndex ].SetColor("_TintColor", new Color(0, 1, .9f, duration/2));
			duration+=Time.deltaTime*2;
			yield return null;
		}
	}
	
	IEnumerator FadeOut(){
		float duration=0;
		while(duration<1){
			transform.localScale=Vector3.Lerp(new Vector3(6, 6, 6), Vector3.zero, duration);
			renderer.materials[ materialIndex ].SetColor("_TintColor", new Color(0, 1, .9f, 0.5f-duration/2));
			duration+=Time.deltaTime;
			yield return null;
		}
	}
}
