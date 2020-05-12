using System;
using System.Collections.Generic;
using UnityEngine;

public class BoneCollectionScript : MonoBehaviour {
	
	[Serializable]
	public struct BoneData{
		public Transform BoneTransform;
		public Vector3 Forward;
		public Vector3 Up;
	}

	public BoneData[] Bones;
}
