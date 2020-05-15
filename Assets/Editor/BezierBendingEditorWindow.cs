using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

class BezierBendingEditorWindow : EditorWindow {

	Transform startObject;
	Transform endObject;

	private float startBezierMagnitude = 100;
	private float endBezierMagnitude = 100;

	[MenuItem("Window/Bezier bone bender")]
	public static void ShowWindow() {
		EditorWindow.GetWindow(typeof(BezierBendingEditorWindow));
	}

	void OnGUI() {

		startObject = (Transform)EditorGUILayout.ObjectField("Start:",startObject, typeof(Transform), true);
		endObject = (Transform)EditorGUILayout.ObjectField("End:",endObject, typeof(Transform), true);

		GUILayout.Space(8);

		startBezierMagnitude = EditorGUILayout.FloatField("Start magnitude: ", startBezierMagnitude);
		endBezierMagnitude = EditorGUILayout.FloatField("End magnitude: ", endBezierMagnitude);

		GUILayout.Space(16);

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();

		if (GUILayout.Button("Bend!", GUILayout.Width(128), GUILayout.Height(32)))
			BendBones();
		
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

	}

	void BendBones() {

		if (!Selection.activeTransform) {
			Debug.LogWarning("No object selected");
			return;
		}

		if (!startObject) {
			Debug.LogWarning("Start object not assigned");
			return;
		}

		if (!endObject) {
			Debug.LogWarning("End object not assigned");
			return;
		}

		BoneCollectionScript bones = Selection.activeTransform.GetComponent<BoneCollectionScript>();

		if (!bones) {
			Debug.LogWarning("No bone collection script in selected object");
			return;
		}


		float endpointsDistance = Vector3.Distance(
			startObject.position,
			endObject.position
		);

		Vector3 centerMidpoint = Vector3.Lerp(startObject.position, endObject.position, .5f);

		Undo.RecordObjects(bones.Bones.Select(b => b.BoneTransform).ToArray(), "move bones");

		Undo.RecordObject(bones.transform, "move segment root");
		bones.transform.position = centerMidpoint;

		int boneCount = bones.Bones.Length;

		List<Vector3> points;
		{
			Vector3 start = startObject.position;
			Vector3 startDir = start - startObject.forward * startBezierMagnitude;
			Vector3 end = endObject.position;
			Vector3 endDir = end - endObject.forward * endBezierMagnitude;
			points = Bezier.CubicBezierRender(start, startDir, endDir, end, boneCount);
		}

		if (points.Count != boneCount) {
			Debug.LogError("bone and bezier point count dont match");
			return;
		}

		for (int i = 0; i < points.Count; i++) {
			bones.Bones[i].BoneTransform.position = points[i];
		}

		for (int i = 0; i < boneCount; i++) {

			Transform nextBone;
			if (i == boneCount - 1)
				nextBone = endObject.transform;
			else
				nextBone = bones.Bones[i + 1].BoneTransform;

			var bone = bones.Bones[i];

			bone.BoneTransform.LookAt(nextBone, Vector3.Lerp(startObject.up, endObject.up, (float)i / boneCount));

			bone.BoneTransform.rotation *= Quaternion.FromToRotation(Vector3.forward, bone.Forward);
			bone.BoneTransform.rotation *= Quaternion.FromToRotation(Vector3.up, bone.Up);

		}


	}
}
