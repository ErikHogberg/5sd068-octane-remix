using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class ConstraintToolComponent : MonoBehaviour {

	[Tooltip("Min and max allowed distance of all constrained transforms")]
	public Vector2 DistanceConstraint; 
	// IDEA: float value for deadzone compared to init distance instead of min-max vector

	public bool ConstrainAngle;
	public float AngleConstraint;



	public List<Transform> ConstrainedTransforms;

	public void EnforceConstraints() {
		foreach (var item in ConstrainedTransforms) {
			// TODO: move constrained transforms

			var constraintComponent = item.GetComponent<ConstraintToolComponent>();
			if (constraintComponent) {
				constraintComponent.EnforceConstraints();
			}
		}
	}

}
