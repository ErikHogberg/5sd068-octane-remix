using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundAssets : MonoBehaviour {
	private static SoundAssets instance;

	public static SoundAssets Instance => instance ?? CreateSoundAssets();
	
	private static SoundAssets CreateSoundAssets() {
		instance = Instantiate(Resources.Load<SoundAssets>("SoundAssets"));
		DontDestroyOnLoad(instance);
		return instance;
	}
	
	[Tooltip("A list of all sound effects that can be used within the game.")]
	public List<Sound> soundEffects;


}
