using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour 
{

	private AudioSource AS;

	public AudioClip Audio_UIConfirm;
	public AudioClip Audio_UIScroller;

	public AudioClip Audio_SFXRockHit;
	public AudioClip Audio_SFXPickUp;
	public AudioClip Audio_SFXFailure;
	public AudioClip Audio_SFXSuccess;
	public AudioClip Audio_SFXExplosion;
	public AudioClip Audio_SFXDiamondHit;
	public AudioClip Audio_SFXDig;
	public AudioClip Audio_SFXWinLevel;



	private int minPitch = 3;
	private int maxPitch = 7;

	//Cache volume here
	public float Volume;

	//

	void Start()
	{
		AS = gameObject.AddComponent<AudioSource> ();
	}

	//

	void Update()
	{
		AS.volume = Volume;
	}

	//

	private void setClip(AudioClip c) { AS.clip = c; }
	private void stopAudio() { AS.Stop (); }
	private void resetPitch() { AS.pitch = 1f; }
	private void play() { AS.Play (); }

	#region PLAY SOUND FUNCTIONS

	public void play_SFX_Explosion()
	{
		stopAudio ();
		resetPitch ();
		setClip(Audio_SFXExplosion);
		play();
	}

	public void play_SFX_WinLevel()
	{
		stopAudio ();
		resetPitch ();
		setClip(Audio_SFXWinLevel);
		play();
	}

	//

	public void play_UI_ConfirmAudio()
	{
		setClip(Audio_UIConfirm);
		play();
	}

	//

	public void play_UI_Scroller()
	{
		float v = AS.volume;
		setClip(Audio_UIScroller);
		//Play at quarter volume
		AS.PlayOneShot(Audio_UIScroller, v*0.25f);

	}
	//

	public void play_SFX_Dig (bool isDirt)
	{
		if (!AS.isPlaying) //Only play if no other audio is playing
		{
			setClip(Audio_SFXDig);	//	DIGGING			WALKING
			AS.pitch = (isDirt == true) ? 2 : 3;
			play();
		}
	}

	public void play_SFX_Failure()
	{
		stopAudio ();
		resetPitch ();
		setClip(Audio_SFXFailure);
		play();
	}

	public void play_SFX_Success ()
	{
		stopAudio ();
		resetPitch ();
		setClip(Audio_SFXSuccess);
		play();
	}

	public void play_SFX_DiamondHit ()
	{
		resetPitch ();
		if (!AS.isPlaying || (AS.isPlaying && AS.clip == Audio_SFXDig) ) //Override Dig
		{
			stopAudio ();
			setClip(Audio_SFXDiamondHit);
			play();
		}
	}

	public void play_SFX_RockHit () //RAND
	{
		resetPitch ();
		if (!AS.isPlaying || (AS.isPlaying && AS.clip == Audio_SFXDig) )
		{
			stopAudio ();
			setClip(Audio_SFXRockHit);
			AS.pitch = Random.Range (minPitch-1, maxPitch-1);
			play();
		}
	}

	public void play_SFX_PickUp () //RAND
	{
		resetPitch ();
		if (!AS.isPlaying || (AS.isPlaying && AS.clip == Audio_SFXDig) )
		{
			setClip(Audio_SFXPickUp);
			AS.pitch = Random.Range (minPitch, maxPitch);
			play();
		}
	}

	#endregion
}
