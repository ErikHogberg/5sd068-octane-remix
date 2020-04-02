using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts {
	public static class Utilities {
		public static float EvaluateMirrored(this AnimationCurve curve, float input) {
			float evaluation = curve.Evaluate(Mathf.Abs(input));

			if (input < 0) {
				return -evaluation;
			} else {
				return evaluation;
			}
		}

	}
}
