﻿using System.Collections;
using UnityEngine;

namespace BigBen
{
    internal class AlertSoundPlayer : MonoBehaviour
    {
        public GameObject bigBenPlayer;
        public FXGroup source; //The source to be added to the object
        public AudioClip loadedClip;

        int repeat = 0;
        WaitForSecondsRealtime wait;
        void Start()
        {
            wait = new WaitForSecondsRealtime(0.1f);
        }


        public void PlaySound(int repeat = 0)
        {
            this.repeat = repeat;
            source.audio.clip = loadedClip;
            source.audio.loop = false;
            StartCoroutine(PlayIt(repeat + 1));
        }

        IEnumerator PlayIt(int r)
        {
            if (source == null)
                Debug.Log("BigBen.PlayIt, source is null");
            for (int i = 0; i < r; i++)
            {
                if (source.audio == null)
                    Debug.Log("BigBen.PlayIt, audio is null");
                source.audio.Play();
                while (source.audio.isPlaying)
                    yield return wait;
            }
        }
#if false
        public void SetVolume(float vol)
        {
            source.audio.volume = vol / 100;
        }
#endif
        public void StopSound()
        {
            source.audio.Stop();
        }
        public void PauseSound()
        {
            source.audio.Stop();
        }
        public void unPauseSound()
        {
            source.audio.UnPause();
        }
        public bool SoundPlaying() //Returns true if sound is playing, otherwise false
        {
            if (source != null && source.audio != null)
            {
                return source.audio.isPlaying;
            }
            else
            {
                return false;
            }
        }

        public void LoadNewSound(string soundPath)
        {
            loadedClip = GameDatabase.Instance.GetAudioClip(soundPath);
        }
        public void Initialize(string soundPath)
        {
            //Initializing stuff;
            bigBenPlayer = new GameObject("BigBenPlayer"); //Makes the GameObject
            source = new FXGroup("BigBenPlayer");
            source.audio = bigBenPlayer.AddComponent<AudioSource>();
            loadedClip = GameDatabase.Instance.GetAudioClip(soundPath);

            source.audio.volume = 0.5f;
            source.audio.spatialBlend = 0;
        }

        public float Volume {
            get
            {
                if (source != null && source.audio != null)
                    return source.audio.volume;
                else return 0;
            }
            set {
                if (source != null && source.audio != null) source.audio.volume = value; 
            }
        }
    }
}
