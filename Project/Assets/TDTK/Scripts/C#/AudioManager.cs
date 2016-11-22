using UnityEngine;
using System.Collections;

[AddComponentMenu("TDTK/Optional/AudioManager")]
public class AudioManager : MonoBehaviour {

	static private AudioObject[] audioObject;
	
	static public AudioManager audioManager;
	
	static private Transform cam;
	
	public float minFallOffRange=10;
	
	public AudioClip[] musicList;
	public bool playMusic=true;
	public bool shuffle=false;
	private int currentTrackID=0;
	private AudioSource musicSource;
	
	public AudioClip waveClearedSound;
	public AudioClip newWaveSound;
	public AudioClip gameWonSound;
	public AudioClip gameLostSound;
	
	public AudioClip towerBuildingSound;
	public AudioClip towerBuiltSound;
	public AudioClip towerSoldSound;
	
	public AudioClip perkUnlockSound;
	public AudioClip abilityActivateSound;
	public AudioClip abilityReadySound;
	public AudioClip energyFullSound;
	
	public AudioClip actionFailedSound;
	
	private GameObject thisObj;
	
	
	static public void PlayPerkUnlockSound(string label){
		if(audioManager==null) return;
		if(audioManager.perkUnlockSound!=null) PlaySound(audioManager.perkUnlockSound);
	}
	
	static public void PlayAbilityActivateSound(){
		if(audioManager==null) return;
		if(audioManager.abilityActivateSound!=null) PlaySound(audioManager.abilityActivateSound);
	}
	
	static public void PlayAbilityReadySound(){
		if(audioManager==null) return;
		if(audioManager.abilityReadySound!=null) PlaySound(audioManager.abilityReadySound);
	}
	
	static public void PlayEnergyFullSound(){
		if(audioManager==null) return;
		if(audioManager.energyFullSound!=null) PlaySound(audioManager.energyFullSound);
	}
	
	static public void PlayWaveClearedSound(){
		if(audioManager==null) return;
		if(audioManager.waveClearedSound!=null) PlaySound(audioManager.waveClearedSound);
	}
	
	static public void PlayNewWaveSound(){
		if(audioManager==null) return;
		if(audioManager.newWaveSound!=null) PlaySound(audioManager.newWaveSound);
	}
	
	static public void PlayGameWonSound(){
		if(audioManager==null) return;
		if(audioManager.gameWonSound!=null) PlaySound(audioManager.gameWonSound);
	}
	
	static public void PlayGameLostSound(){
		if(audioManager==null) return;
		if(audioManager.gameLostSound!=null) PlaySound(audioManager.gameLostSound);
	}
	
	static public void PlayActionFailedSound(){
		if(audioManager==null) return;
		if(audioManager.actionFailedSound!=null) PlaySound(audioManager.actionFailedSound);
	}
	
	
	void Awake(){
		//Init();
		thisObj=gameObject;
		
		cam=Camera.main.transform;
		
		if(playMusic && musicList!=null && musicList.Length>0){
			GameObject musicObj=new GameObject();
			musicObj.name="MusicSource";
			musicObj.transform.position=cam.position;
			musicObj.transform.parent=cam;
			musicSource=musicObj.AddComponent<AudioSource>();
			musicSource.loop=false;
			musicSource.playOnAwake=false;
			
			musicSource.ignoreListenerVolume=true;
			
			StartCoroutine(MusicRoutine());
		}
		
		audioObject=new AudioObject[20];
		for(int i=0; i<audioObject.Length; i++){
			GameObject obj=new GameObject();
			obj.name="AudioSource";
			
			AudioSource src=obj.AddComponent<AudioSource>();
			src.playOnAwake=false;
			src.loop=false;
			src.minDistance=minFallOffRange;
			
			Transform t=obj.transform;
			t.parent=thisObj.transform;
			
			audioObject[i]=new AudioObject(src, t);
		}
		
		AudioListener.volume=0.8f;
		
		if(audioManager==null) audioManager=this;
	}
	
	static public void Init(){
		if(audioManager==null){
			GameObject objParent=new GameObject();
			objParent.name="AudioManager";
			audioManager=objParent.AddComponent<AudioManager>();
		}		
	}
	
	public IEnumerator MusicRoutine(){
		while(true){
			if(shuffle) musicSource.clip=musicList[Random.Range(0, musicList.Length)];
			else{
				musicSource.clip=musicList[currentTrackID];
				currentTrackID+=1;
				if(currentTrackID==musicList.Length) currentTrackID=0;
			}
			
			musicSource.Play();
			
			yield return new WaitForSeconds(musicSource.clip.length);
		}
	}
	
	// Use this for initialization
	void Start () {
		
	}
	
	void OnEnable(){
		PerkManager.onPerkUnlockedE += PlayPerkUnlockSound;
		
		AbilityManager.onUpdateEnergyE += OnUpdateEnergy;
		AbilityManager.onAbilityActivateE += PlayAbilityActivateSound;
		AbilityManager.onAbilityReadyE += PlayAbilityReadySound;
	}
	
	void OnDisable(){
		PerkManager.onPerkUnlockedE += PlayPerkUnlockSound;
		
		AbilityManager.onUpdateEnergyE -= OnUpdateEnergy;
		AbilityManager.onAbilityActivateE -= PlayAbilityActivateSound;
		AbilityManager.onAbilityReadyE -= PlayAbilityReadySound;
	}
	
	void OnUpdateEnergy(float val){
		if(val>=AbilityManager.GetMaximumEnergy()){
			PlayEnergyFullSound();
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	//check for the next free, unused audioObject
	static private int GetUnusedAudioObject(){
		for(int i=0; i<audioObject.Length; i++){
			if(!audioObject[i].inUse){
				return i;
			}
		}
		
		//if everything is used up, use item number zero
		return 0;
	}
	
	//this is a 3D sound that has to be played at a particular position following a particular event
	static public void PlaySound(AudioClip clip, Vector3 pos){
		if(audioManager==null) Init();
		
		int ID=GetUnusedAudioObject();
		
		audioObject[ID].inUse=true;
		
		audioObject[ID].thisT.position=pos;
		audioObject[ID].source.clip=clip;
		audioObject[ID].source.Play();
		
		float duration=audioObject[ID].source.clip.length;
		
		audioManager.StartCoroutine(audioManager.ClearAudioObject(ID, duration));
	}
	
	//this no position has been given, assume this is a 2D sound
	static public void PlaySound(AudioClip clip){
		if(audioManager==null) Init();
		
		int ID=GetUnusedAudioObject();
		
		audioObject[ID].inUse=true;
		
		audioObject[ID].source.clip=clip;
		audioObject[ID].source.Play();
		
		float duration=audioObject[ID].source.clip.length;
		
		audioManager.StartCoroutine(audioManager.ClearAudioObject(ID, duration));
	}
	
	//a sound routine for 2D sound, make sure they follow the listener, which is assumed to be the main camera
	static IEnumerator SoundRoutine2D(int ID, float duration){
		while(duration>0){
			audioObject[ID].thisT.position=cam.position;
			yield return null;
		}
		
		//finish playing, clear the audioObject
		audioManager.StartCoroutine(audioManager.ClearAudioObject(ID, 0));
	}
	
	//function call to clear flag of an audioObject, indicate it's is free to be used again
	private IEnumerator ClearAudioObject(int ID, float duration){
		yield return new WaitForSeconds(duration);
		
		audioObject[ID].inUse=false;
	}
	
	static public void PlayTowerBuilding(){
		if(audioManager==null) return;
		if(audioManager.towerBuildingSound!=null) PlaySound(audioManager.towerBuildingSound);
	}
	
	static public void PlayTowerBuilt(){
		if(audioManager==null) return;
		if(audioManager.towerBuiltSound!=null) PlaySound(audioManager.towerBuiltSound);
	}
	
	static public void PlayTowerSold(){
		if(audioManager==null) return;
		if(audioManager.towerSoldSound!=null) PlaySound(audioManager.towerSoldSound);
	}
	
	static public void SetSFXVolume(float val){
		AudioListener.volume=val;
	}
	
	static public void SetMusicVolume(float val){
		if(audioManager  && audioManager.musicSource){
			audioManager.musicSource.volume=val;
		}
	}
	
}


[System.Serializable]
public class AudioObject{
	public AudioSource source;
	public bool inUse=false;
	public Transform thisT;
	
	public AudioObject(AudioSource src, Transform t){
		source=src;
		thisT=t;
	}
}