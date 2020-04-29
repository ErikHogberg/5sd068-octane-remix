using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class UpdateMeshColliderScript : MonoBehaviour {
	SkinnedMeshRenderer meshRenderer;
	MeshCollider meshCollider;

	private void Awake() {
		meshRenderer = GetComponent<SkinnedMeshRenderer>();
		meshCollider = GetComponent<MeshCollider>();
		UpdateMeshCollider();
	}

	public void UpdateMeshCollider() {
		// Source: https://answers.unity.com/questions/39490/collider-on-skinned-mesh.html
		Mesh colliderMesh = new Mesh();
		meshRenderer.BakeMesh(colliderMesh);
		// meshCollider.sharedMesh.Clear(keepVertexLayout: true); // NOTE: crashes unity if keepVertexLayout is true, seemingly permanently deletes the mesh if false
		// var oldMesh = meshCollider.sharedMesh;
		// TODO: destroy or reuse old mesh
		meshCollider.sharedMesh = null;
		// Destroy(oldMesh);
		meshCollider.sharedMesh = colliderMesh;
	}
	
}
