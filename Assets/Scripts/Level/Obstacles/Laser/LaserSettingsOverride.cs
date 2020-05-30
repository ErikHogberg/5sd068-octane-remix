using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserSettingsOverride : MonoBehaviour {

	[Header("Settings")]
	public float Range = 100f;
	[Header("Which things (layers) the lasers can hit")]
	public LayerMask LaserLayerMask;
	[Tooltip("Show laser even when not hitting anything?")]
	public bool AlwaysShowLaser = true;

}
