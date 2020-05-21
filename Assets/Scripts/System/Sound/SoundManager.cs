using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound {
	[Tooltip("The name of this sound within the game system. It's used as an ID, so it needs to be unique.")]
	public string name = "Blank";
	[Tooltip("When played, should this sound effect be looped?")]
	public bool loop = false;
	[Tooltip("The sound to be played.")]
	public AudioClip audioClip;
	[Tooltip("What mixer this sound should use.")]
	public AudioMixerGroup audioMixer;
}

public class SoundManager : MonoBehaviour {
	//Dictionary to keep track of looping sound sources that have their own gameobjects
	private static Dictionary<string, GameObject> loopingSounds;

	//Dictionary to keep track of looping sound sources without unique gameobjects
	private static Dictionary<string, AudioSource> loopingSoundsSources;

	//For efficiency's sake, a dedicated game object for playing sounds without a specified location
	private static GameObject defaultSource;

	void Awake() {
		loopingSounds = new Dictionary<string, GameObject>();
		loopingSoundsSources = new Dictionary<string, AudioSource>();

		defaultSource = new GameObject("SoundManagerDefaultSource");
		defaultSource.transform.parent = gameObject.transform;
	}

	//Plays a sound without a specified origin location
	public static void PlaySound(string name) {
		Sound sound = new Sound();
		bool found = false;

		//Check if requested sound exists in the "library" on the SoundAssets prefab (in Resources folder)
		foreach (Sound item in SoundAssets.Instance.soundEffects) {
			if (item.name == name) {
				sound = item;
				found = true;
				break;
			}
		}

		if (!found) {
			Debug.Log("SoundManager/PlaySound: Could not find requested sound among SoundAssets.");
			return;
		}

		if (sound.loop) {
			if (loopingSoundsSources.ContainsKey(sound.name)) {
				Debug.Log("SoundManager/PlaySoundLooping: Loop dictionary already contains an instance for " + sound.name);
				return;
			}
		}

		AudioSource audioSource = defaultSource.AddComponent<AudioSource>();
		audioSource.clip = sound.audioClip;
		audioSource.outputAudioMixerGroup = sound.audioMixer;
		audioSource.loop = sound.loop;
		audioSource.Play();

		if (sound.loop) loopingSoundsSources.Add(sound.name, audioSource);
		else Object.Destroy(audioSource, audioSource.clip.length);
	}

	//Plays a sound at a specified position
	public static void PlaySound(string name, Vector3 pos) {
		Sound sound = new Sound();
		bool found = false;
		//Check if requested sound exists in the "library" on the SoundAssets prefab (in Resources folder)
		foreach (Sound item in SoundAssets.Instance.soundEffects) {
			if (item.name == name) {
				sound = item;
				found = true;
				break;
			}
		}
		if (!found) {
			Debug.Log("SoundManager/PlaySound: Could not find requested sound among SoundAssets.");
			return;
		}
		if (sound.loop) {
			if (loopingSounds.ContainsKey(sound.name)) {
				Debug.Log("SoundManager/PlaySoundLooping: Loop dictionary already contains an instance for " + sound.name);
			}
		}

		GameObject soundSource = new GameObject("SoundSource");
		AudioSource audioSource = soundSource.AddComponent<AudioSource>();
		soundSource.transform.position = pos;

		audioSource.clip = sound.audioClip;
		audioSource.outputAudioMixerGroup = sound.audioMixer;
		audioSource.loop = sound.loop;
		audioSource.spatialBlend = 1.0f;
		audioSource.Play();

		if (sound.loop) loopingSounds.Add(sound.name, soundSource);
		else Object.Destroy(soundSource, audioSource.clip.length);

	}

	public static void StopLooping(string name, bool printDebug = true) {
		if (loopingSounds.ContainsKey(name)) {
			GameObject dest = loopingSounds[name];
			loopingSounds.Remove(name);
			Object.Destroy(dest);
		} else if (loopingSoundsSources.ContainsKey(name)) {
			AudioSource dest = loopingSoundsSources[name];
			loopingSoundsSources.Remove(name);
			Object.Destroy(dest);
		} else if (printDebug) {
			Debug.Log("SoundManager/StopLooping: Loop dictionaries do not contain an instance for " + name);
		}
	}

	public static void StopSound(string name) {
		StopLooping(name, printDebug: false);
		// TODO: stop non-looping sounds
	}

	// TODO: pause and unpause all sounds

}
