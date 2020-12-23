using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//*
[CustomEditor(typeof(SteeringScript))]
[CanEditMultipleObjects]
public class SteeringScriptEditor : Editor {

	// SerializedProperty cellSize;

	bool showSpeedProfiles = false;
	List<bool> profileFoldStates = new List<bool>();

	bool showInAirControl = false;
	bool showBoost = false;
	bool showDrifting = false;
	bool showRumble = false;
	bool showScore = false;
	bool showMisc = false;
	bool showRequired = false;
	bool showInputBindings = false;
	bool showCameras = false;

	void OnEnable() {
		// cellSize = serializedObject.FindProperty("CellSize");
	}


	// private void OnSceneGUI() {
	// 	SteeringScript segmentBendingScript = target as SteeringScript;

	// 	Handles.color = Color.white;

	// }

	public override void OnInspectorGUI() {
		// Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
		serializedObject.Update();

		// oppositeCorner = serializedObject.FindProperty("OppositeCorner");
		// cellPrefab = serializedObject.FindProperty("CellPrefab");

		SteeringScript steeringScript = (SteeringScript)target;

		showSpeedProfiles = EditorGUILayout.Foldout(showSpeedProfiles, "Speed profiles");

		EditorGUI.BeginChangeCheck();

		if (showSpeedProfiles) {

			if (steeringScript.SpeedProfiles.Count > profileFoldStates.Count) {
				int diff = steeringScript.SpeedProfiles.Count - profileFoldStates.Count;
				for (int i = 0; i < diff; i++) {
					profileFoldStates.Add(false);
				}
			} else if (steeringScript.SpeedProfiles.Count < profileFoldStates.Count) {
				int diff = profileFoldStates.Count - steeringScript.SpeedProfiles.Count;
				profileFoldStates.RemoveRange(steeringScript.SpeedProfiles.Count - 1, diff);
			}

			// foreach (var item in steeringScript.SpeedProfiles) {
			for (int i = 0; i < steeringScript.SpeedProfiles.Count; i++) {
				SteeringScript.SpeedProfile item = steeringScript.SpeedProfiles[i];
				bool isCurrentProfile = steeringScript.CurrentProfileIndex == i;

				EditorGUILayout.BeginHorizontal();
				profileFoldStates[i] = EditorGUILayout.Foldout(profileFoldStates[i], item.ProfileName);
				bool selectToggle = EditorGUILayout.Toggle(isCurrentProfile);
				item.ProfileName = EditorGUILayout.TextField(item.ProfileName);
				if (selectToggle && !isCurrentProfile) {
					steeringScript.CurrentProfileIndex = i;
				}
				EditorGUILayout.EndHorizontal();

				if (profileFoldStates[i]) {
					EditorGUI.indentLevel += 1;

					EditorGUILayout.LabelField("Steering");

					item.SteeringMax = EditorGUILayout.Slider("Steering Max", item.SteeringMax, 0, 90);
					item.SteeringCurve = EditorGUILayout.CurveField("Steering Curve", item.SteeringCurve);
					item.EnableNarrowing = EditorGUILayout.Toggle("Enable Narrowing", item.EnableNarrowing);
					if (item.EnableNarrowing) {
						item.MaxNarrowingSpeed = EditorGUILayout.FloatField("Max Narrowing Speed", item.MaxNarrowingSpeed);
						item.MaxNarrowingAmount = EditorGUILayout.Slider("Max Narrowing Amount", item.MaxNarrowingAmount, 0, 1);
						item.SteeringNarrowingCurve = EditorGUILayout.CurveField("Narrowing Curve", item.SteeringNarrowingCurve);
					}

					EditorGUILayout.Space();
					item.SteeringRotationHelp = EditorGUILayout.FloatField("Rotation Help", item.SteeringRotationHelp);
					item.SteeringStrafeHelp = EditorGUILayout.FloatField("Strafe Help", item.SteeringStrafeHelp);
					item.SteeringStrafeMode = (ForceMode)EditorGUILayout.EnumPopup("Strafe Mode", item.SteeringStrafeMode);

					EditorGUILayout.Space();
					EditorGUILayout.LabelField("Gas");
					item.GasSpeed = EditorGUILayout.FloatField("Gas Speed", item.GasSpeed);
					item.GasPedalCurve = EditorGUILayout.CurveField("Gas Pedal Curve", item.GasPedalCurve);

					EditorGUILayout.Space();
					EditorGUILayout.LabelField("Brakes");
					item.BrakeForce = EditorGUILayout.FloatField("Brake Force", item.BrakeForce);
					item.BrakePedalCurve = EditorGUILayout.CurveField("Brake Pedal Curve", item.BrakePedalCurve);
					item.DampenRigidBody = EditorGUILayout.Toggle("Dampen Rigidbody", item.DampenRigidBody);
					if (item.DampenRigidBody) {
						item.BrakeDampeningAmount = EditorGUILayout.FloatField("Brake Dampening Amount", item.BrakeDampeningAmount);
					}

					EditorGUILayout.Space();
					if (item.EnableDownwardForce) {
						EditorGUILayout.LabelField("Downward Force");
					}
					item.EnableDownwardForce = EditorGUILayout.Toggle("Enable DownwardForce", item.EnableDownwardForce);
					if (item.EnableDownwardForce) {
						item.DownwardForce = EditorGUILayout.FloatField("Downward Force", item.DownwardForce);
						item.MinDownwardsForceSpeed = EditorGUILayout.FloatField("Min Speed", item.MinDownwardsForceSpeed);
						item.MaxDownwardsForceSpeed = EditorGUILayout.FloatField("Max Speed", item.MaxDownwardsForceSpeed);
						item.DownwardsForceSpeedCurve = EditorGUILayout.CurveField("Force Curve", item.DownwardsForceSpeedCurve);
						item.DownwardForceMode = (ForceMode)EditorGUILayout.EnumPopup("Force Mode", item.DownwardForceMode);
						item.UseRelativeDownwardForce = EditorGUILayout.Toggle("Use Relative Force", item.UseRelativeDownwardForce);
					}

					EditorGUILayout.Space();
					EditorGUILayout.LabelField("Boost");
					item.BoostSpeed = EditorGUILayout.FloatField("Boost Speed", item.BoostSpeed);

					EditorGUILayout.Space();
					if (item.CapVelocity) {
						EditorGUILayout.LabelField("Velocity Cap");
					}
					item.CapVelocity = EditorGUILayout.Toggle("Cap Velocity", item.CapVelocity);
					if (item.CapVelocity) {
						item.DisableCapInAir = EditorGUILayout.Toggle("Disable Cap in-air", item.DisableCapInAir);
						item.VelocityCap = EditorGUILayout.FloatField("Velocity Cap", item.VelocityCap);
						item.BoostVelocityCap = EditorGUILayout.FloatField("Boost Cap", item.BoostVelocityCap);
						item.VelocityCapCorrectionSpeed = EditorGUILayout.FloatField("Cap Correction Speed", item.VelocityCapCorrectionSpeed);
						item.AbsoluteVelocityCap = EditorGUILayout.FloatField("Absolute Cap", item.AbsoluteVelocityCap);
					}

					EditorGUILayout.Space();
					EditorGUILayout.LabelField("Wheel Friction");
					item.FrontWheelForwardStiffness = EditorGUILayout.FloatField("Frown Wheel Forward Stiffness", item.FrontWheelForwardStiffness);
					item.FrontWheelSidewaysStiffness = EditorGUILayout.FloatField("Frown Wheel Forward Stiffness", item.FrontWheelSidewaysStiffness);
					item.RearWheelForwardStiffness = EditorGUILayout.FloatField("Frown Wheel Forward Stiffness", item.RearWheelForwardStiffness);
					item.RearWheelSidewaysStiffness = EditorGUILayout.FloatField("Frown Wheel Forward Stiffness", item.RearWheelSidewaysStiffness);

					EditorGUI.indentLevel -= 1;

				}
			}

			if (GUILayout.Button("Add profile")) {
				steeringScript.SpeedProfiles.Add(new SteeringScript.SpeedProfile());
			}

			EditorGUILayout.Space();
			steeringScript.ProfileChangeColor = EditorGUILayout.ColorField("Profile Change Color", steeringScript.ProfileChangeColor);
			EditorGUILayout.Space();
		}

		// EditorGUILayout.LabelField("In-air Controls");
		showInAirControl = EditorGUILayout.Foldout(showInAirControl, "In-air Controls");
		if (showInAirControl) {

			steeringScript.LeftStickRotationWhenInAir = EditorGUILayout.Toggle("Left stick rotation when in-air", steeringScript.LeftStickRotationWhenInAir);
			steeringScript.ZeroAngularVelocityOnLanding = EditorGUILayout.Toggle("Reset angular velocity on landing", steeringScript.ZeroAngularVelocityOnLanding);
			steeringScript.ZeroAngularVelocityOnAir = EditorGUILayout.Toggle("Reset angular velocity on jump", steeringScript.ZeroAngularVelocityOnAir);
			steeringScript.YawSpeed = EditorGUILayout.FloatField("Yaw Speed", steeringScript.YawSpeed);
			steeringScript.YawInputCurve = EditorGUILayout.CurveField("", steeringScript.YawInputCurve);
			EditorGUILayout.Space();
		}

		// EditorGUILayout.LabelField("Boost");
		showBoost = EditorGUILayout.Foldout(showBoost, "Boost");
		if (showBoost) {
			steeringScript.BoostConsumptionRate = EditorGUILayout.Slider("Boost Consumption Rate", steeringScript.BoostConsumptionRate, 0, 1);
			steeringScript.BoostFillRate = EditorGUILayout.Slider("Boost Fill Rate", steeringScript.BoostFillRate, 0, 1);
			steeringScript.MinBoostLevel = EditorGUILayout.Slider("Min Boost Level", steeringScript.MinBoostLevel, 0, 1);
			steeringScript.BoostAffectedBySteering = EditorGUILayout.Toggle("Boost Affected By Steering", steeringScript.BoostAffectedBySteering);
			steeringScript.BoostMaxSteering = EditorGUILayout.Slider("Boost Max Steering", steeringScript.BoostMaxSteering, 0, 90);
			steeringScript.BoostWindupSkill = (SteeringScript.BoostSkill)EditorGUILayout.EnumPopup("Boost Windup Skill", steeringScript.BoostWindupSkill);
			if (steeringScript.BoostWindupSkill != SteeringScript.BoostSkill.None) {
				steeringScript.BoostWindup = EditorGUILayout.FloatField("Boost Windup", steeringScript.BoostWindup);
			}
			if (steeringScript.BoostWindupSkill == SteeringScript.BoostSkill.SloMo) {
				steeringScript.BoostSloMoTimescale = EditorGUILayout.Slider("Boost SloMo Timescale", steeringScript.BoostSloMoTimescale, 0, 1);
			}
			EditorGUILayout.Space();
		}

		// EditorGUILayout.LabelField("Drifting");
		showDrifting = EditorGUILayout.Foldout(showDrifting, "Drifting");
		if (showDrifting) {
			steeringScript.DriftStartAngle = EditorGUILayout.Slider("Drift Start Angle", steeringScript.DriftStartAngle, 0, 180);
			steeringScript.DriftStopAngle = EditorGUILayout.Slider("Drift Stop Angle", steeringScript.DriftStopAngle, 0, 180);
			steeringScript.DriftStartVelocity = EditorGUILayout.FloatField("Drift Start Velocity", steeringScript.DriftStartVelocity);
			steeringScript.DriftStopVelocity = EditorGUILayout.FloatField("Drift Stop Velocity", steeringScript.DriftStopVelocity);
			steeringScript.DriftCorrectionSpeed = EditorGUILayout.Slider("Drift Correction Speed", steeringScript.DriftCorrectionSpeed, 0, 180);
			steeringScript.DriftSpeedReductionWhenCorrecting = EditorGUILayout.FloatField("Speed Recuction When Correcting", steeringScript.DriftSpeedReductionWhenCorrecting);
			EditorGUILayout.Space();
		}

		// if (SteeringScript.EnableRumble) {
		// 	EditorGUILayout.LabelField("Rumble");
		// }
		SteeringScript.EnableRumble = EditorGUILayout.Toggle("Enable Rumble", SteeringScript.EnableRumble);
		if (SteeringScript.EnableRumble) {
			showRumble = EditorGUILayout.Foldout(showRumble, "Rumble");
			if (showRumble) {
				steeringScript.EngineRumbleHiHzMaxVelocity = EditorGUILayout.FloatField("Engine Rumble Hi-Hz Max Velocity", steeringScript.EngineRumbleHiHzMaxVelocity);
				steeringScript.EngineRumbleHiHzCurve = EditorGUILayout.CurveField("Engine Rumble Hi-Hz Curve", steeringScript.EngineRumbleHiHzCurve);
				steeringScript.EngineRumbleLoHzMaxVelocity = EditorGUILayout.FloatField("Engine Rumble Lo-Hz Max Velocity", steeringScript.EngineRumbleLoHzMaxVelocity);
				steeringScript.EngineRumbleLoHzCurve = EditorGUILayout.CurveField("Engine Rumble Lo-Hz Curve", steeringScript.EngineRumbleLoHzCurve);
				EditorGUILayout.Space();
				steeringScript.BoostRumbleHiLoHzRatio = EditorGUILayout.Slider("Boost Rumble Hi/Lo-Hz Ratio", steeringScript.BoostRumbleHiLoHzRatio, 0, 1);
				steeringScript.BoostRumbleAmount = EditorGUILayout.Slider("Boost Rumble Amount", steeringScript.BoostRumbleAmount, 0, 1);
				steeringScript.DriftRumbleHiLoHzRatio = EditorGUILayout.Slider("Drift Rumble Hi/Lo-Hz Ratio", steeringScript.DriftRumbleHiLoHzRatio, 0, 1);
				steeringScript.DriftRumbleAmount = EditorGUILayout.Slider("Drift Rumble Amount", steeringScript.DriftRumbleAmount, 0, 1);
				EditorGUILayout.Space();
			}
		}

		// EditorGUILayout.LabelField("Score");
		showScore = EditorGUILayout.Foldout(showScore, "Score");
		if (showScore) {
			steeringScript.DriftScorePerSec = EditorGUILayout.FloatField("Drift Score/Sec", steeringScript.DriftScorePerSec);
			steeringScript.DriftTimeThreshold = EditorGUILayout.FloatField("Drift Time Threshold", steeringScript.DriftTimeThreshold);
			EditorGUILayout.Space();
			steeringScript.BoostScorePerSec = EditorGUILayout.FloatField("Boost Score/Sec", steeringScript.BoostScorePerSec);
			steeringScript.BoostTimeThreshold = EditorGUILayout.FloatField("Boost Time Threshold", steeringScript.BoostTimeThreshold);
			EditorGUILayout.Space();
			steeringScript.AirTimeScorePerSec = EditorGUILayout.FloatField("Air-time Score/Sec", steeringScript.AirTimeScorePerSec);
			steeringScript.AirTimeTimeThreshold = EditorGUILayout.FloatField("Air-time Time Threshold", steeringScript.AirTimeTimeThreshold);
			EditorGUILayout.Space();
			steeringScript.DestructionScore = EditorGUILayout.LongField("Destruction Score", steeringScript.DestructionScore);
			EditorGUILayout.Space();
		}

		showMisc = EditorGUILayout.Foldout(showMisc, "Misc.");
		if (showMisc) {
			steeringScript.EnableCheatMitigation = EditorGUILayout.Toggle("Enable Cheat Mitigation", steeringScript.EnableCheatMitigation);
			steeringScript.UIUpdateInterval = EditorGUILayout.IntField("UI Update Interval", steeringScript.UIUpdateInterval);
			steeringScript.OverrideGravity = EditorGUILayout.Toggle("Override Gravity", steeringScript.OverrideGravity);
			if (steeringScript.OverrideGravity) {
				EditorGUI.indentLevel += 1;
				steeringScript.GravityOverride = EditorGUILayout.FloatField("Gravity Override", steeringScript.GravityOverride);
				EditorGUI.indentLevel -= 1;
			}
			steeringScript.InAirStabilization = EditorGUILayout.Toggle("In-air Stabilization", steeringScript.InAirStabilization);
			if (steeringScript.InAirStabilization) {
				EditorGUI.indentLevel += 1;
				steeringScript.InAirStabilizationAmount = EditorGUILayout.FloatField("Amount", steeringScript.InAirStabilizationAmount);
				EditorGUI.indentLevel -= 1;
			}
			steeringScript.AllowYawOnGround = EditorGUILayout.Toggle("Allow Yaw Control On Ground", steeringScript.AllowYawOnGround);
			EditorGUILayout.Space();
		}

		showRequired = EditorGUILayout.Foldout(showRequired, "Required Objects");
		if (showRequired) {
			// TODO: Required objects
		}

		showInputBindings = EditorGUILayout.Foldout(showInputBindings, "Key Bindings");
		if (showInputBindings) {
			// TODO: Key bindings
		}

		if (showCameras) {
			// TODO: cameras
			// IDEA: draw one extra empty camera field, one more than current amount of cameras. if field is filled, then another empty field will be added below
			// IDEA: auto remove empty fields other than last
		}

		if (EditorGUI.EndChangeCheck()) {
			// FIXME: folding counts as script change
			Undo.RecordObject(steeringScript, "Steering Script Change");
			EditorUtility.SetDirty(steeringScript);
		}

		// Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
		serializedObject.ApplyModifiedProperties();
	}

}
// */
