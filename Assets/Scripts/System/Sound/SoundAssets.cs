using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundAssets : MonoBehaviour
{
	private static SoundAssets _i;

	public static SoundAssets i {
        get {
			if (_i == null) _i = Instantiate(Resources.Load<SoundAssets>("SoundAssets"));
			return _i;
        }
    }

	[Tooltip("A list of all sound effects that can be used within the game.")]
	public List<Sound> soundEffects;


}
