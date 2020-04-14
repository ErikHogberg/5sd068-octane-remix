using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts {
	public static class CustomUtilities {
		public static float EvaluateMirrored(this AnimationCurve curve, float input) {
			float evaluation = curve.Evaluate(Mathf.Abs(input));

			if (input < 0) {
				return -evaluation;
			} else {
				return evaluation;
			}
		}

		public static void StartEffect(TrailRenderer trail) {
			trail.emitting = true;
		}

		public static void StartEffect(ParticleSystem particles) {
			if (!particles.isEmitting)
				particles.Play();
		}

		public static void StopEffect(TrailRenderer trail) {
			trail.emitting = false;
		}

		public static void StopEffect(ParticleSystem particles) {
			if (particles.isEmitting)
				particles.Stop();
		}

	}
}
