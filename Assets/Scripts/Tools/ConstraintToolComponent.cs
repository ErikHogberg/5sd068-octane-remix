using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEditor;

public class ConstraintToolComponent : MonoBehaviour {

	[Tooltip("Min and max allowed distance of all constrained transforms")]
	public Vector2 DistanceConstraint;
	// IDEA: float value for deadzone compared to init distance instead of min-max vector
	// what would init value be based on?
	// how to assign init outside of play?

	public bool ConstrainAngle;
	public float AngleConstraint;



	public List<Transform> ConstrainedTransforms;

	public void EnforceConstraints() {
		EnforceConstraints(new List<Transform>());
	}

	private void EnforceConstraints(List< Transform> called) {

		Undo.RecordObjects(ConstrainedTransforms.ToArray(), "constraint tool move");
		foreach (var item in ConstrainedTransforms) {

			if (called.Contains(item))
				continue;

			called.Add(transform);

			// TODO: move constrained transforms
			if (Vector3.Distance(transform.position, item.position) < DistanceConstraint.x) 
				item.position = transform.position + Vector3.Normalize(item.position - transform.position)*DistanceConstraint.x;
			if (Vector3.Distance(transform.position, item.position) > DistanceConstraint.y) 
				item.position = transform.position + Vector3.Normalize(item.position - transform.position)*DistanceConstraint.y;
			

			var constraintComponent = item.GetComponent<ConstraintToolComponent>();
			if (constraintComponent) {
				constraintComponent.EnforceConstraints(called); // NOTE: reference to same list for entire call chain
			}
		}
	}

}
