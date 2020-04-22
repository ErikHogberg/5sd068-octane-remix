using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;

public class CarSurfaceGripHandlerScript : MonoBehaviour {


	[Serializable]
	public struct EnvironmentFrictionSettings {
		[Tooltip("The tag of the road surface collider for these settings")]
		public string EnvironmentTag; // object tags that enable these effects

		// IDEA: separate settings for front and rear wheels? unnecessary?

		public WheelFrictionCurve ForwardFriction;
		public WheelFrictionCurve SidewaysFriction;

	}

	[Header("Empty tag = default")]

	public List<EnvironmentFrictionSettings> SurfaceSettings;

	public List<WheelCollider> Wheels;

	private WheelFrictionCurve defaultForwardFriction;
	private WheelFrictionCurve defaultSidewaysFriction;

	private void Start() {
		defaultForwardFriction = Wheels[0].forwardFriction;
		defaultSidewaysFriction = Wheels[0].sidewaysFriction;
	}

	private void OnTriggerEnter(Collider other) {
		WheelFrictionCurve forwardsFriction = defaultForwardFriction;
		WheelFrictionCurve sidewaysFriction = defaultSidewaysFriction;

		foreach (var setting in SurfaceSettings) {
			if (setting.EnvironmentTag == "") {
				forwardsFriction = setting.ForwardFriction;
				sidewaysFriction = setting.SidewaysFriction;
				continue;
			}

			if (setting.EnvironmentTag == other.tag) {
				forwardsFriction = setting.ForwardFriction;
				sidewaysFriction = setting.SidewaysFriction;
				break;
			}
		}

		foreach (WheelCollider wheel in Wheels) {
			wheel.forwardFriction = forwardsFriction;
			wheel.sidewaysFriction = sidewaysFriction;
		}
	}


}
