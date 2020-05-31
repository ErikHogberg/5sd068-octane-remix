using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using TMPro;

[RequireComponent(typeof(AudioSource))]
public class CarSoundHandler : MonoBehaviour
{
	[Tooltip("The name/id of the sound from SoundAssets that the engine of this car should make.")]
	public string soundName;
	[Range(-3.0f, 3.0f)]
	public float minPitch;
	[Range(-3.0f, 3.0f)]
	public float maxPitch;
	[Tooltip("Once the car starts falling, how many seconds does it take before the engine sound pitch starts to slowly drop?")]
	public float fallTimerDuration = 0.7f;

	private AudioSource output;
	private Sound sound;

	private float velocity = 0f;
	private bool grounded = false;

	private float pitchChangeSpeed = 5f;
	private float pitchDifference;
	private float effectivePitch = 1f;

	private float fallTimer = 0;
	private bool fallTiming = false;
	private bool fallPitchDrop = false;

	void Awake()
	{
		output = GetComponent<AudioSource>();
		foreach (Sound item in SoundAssets.Instance.soundEffects) {
			if (item.name == soundName) {
				sound = item;
				break;
			}
		}
		output.clip = sound.audioClip;
		output.outputAudioMixerGroup = sound.audioMixer;
		output.loop = true;
		output.volume = sound.volume;

		pitchDifference = maxPitch - minPitch;
	}

	void OnEnable() { output.Play(); }
	void OnDisable() { output.Stop(); }

	public void RecieveVelocityData(float p_velocity) { if (grounded) velocity = p_velocity; }
	public void RecieveGroundedData(bool p_grounded) {
		bool prevGrounded = grounded;
		grounded = p_grounded; 

		if (prevGrounded == true && grounded == false) {
			if (fallTiming == false)
				StartFallTimer();
        }
		else if (prevGrounded == false && grounded == true) {
			if (fallTiming) {
				fallTimer = 0f;
				fallTiming = false;
			}
        }
	}

	private void Update()
	{
		if (velocity >= 0) {
			if (fallTiming) {
				fallTimer -= Time.deltaTime;
				if (fallTimer <= 0f) {
					fallTimer = 0f;
					fallTiming = false;
					fallPitchDrop = true;
				}
            } else if (fallPitchDrop) {
				if (!grounded) {
					velocity = Mathf.Clamp(velocity - (0.3f * Time.deltaTime), 0f, 1f);
					effectivePitch = minPitch + (pitchDifference * velocity);
					output.pitch = effectivePitch;
				}
				else fallPitchDrop = false;
            }
			else {
				effectivePitch = Mathf.Clamp(minPitch + (pitchDifference * velocity), minPitch, maxPitch);
				output.pitch = effectivePitch;
			}
		}
	}

	private void StartFallTimer() {
		fallTimer = fallTimerDuration;
		fallTiming = true;
    }

}



