using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharSelectMaterial
{
	public CharacterSelected tag;
	public Material material;
}

public class CharSelectRotateStage : MonoBehaviour
{
	public float rotationSpeed = 10.0f;
	public MeshRenderer stageSurface;

	public CharSelectMaterial[] stageMaterials;

	public void ChangeMaterial(CharacterSelected p_name) {
		foreach (CharSelectMaterial mat in stageMaterials) {
			if (p_name == mat.tag) {
				stageSurface.material = mat.material;
			}
        }
    }

    void Update()
    {
		transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
	}
}
