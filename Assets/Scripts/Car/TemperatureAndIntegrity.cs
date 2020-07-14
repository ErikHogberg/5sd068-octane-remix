using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

public class TemperatureAndIntegrity : MonoBehaviour, IObserver<bool> {
	[Header("Temperature")]
	[Tooltip("How much fire obstacles affects the car's temperature level.")]
	public float fireTempEffect = 10.0f;
	[Tooltip("How much frost obstacles affects the car's temperature level.")]
	public float frostTempEffect = 10.0f;
	[Tooltip("How much laser obstacles affect the car's temperature level.")]
	public float laserTempEffect = 10.0f;
	[Tooltip("How much using the boost affects the car's temperature level.")]
	public float boostTempEffect = 15.0f;
	[Space]
	[Tooltip("How many seconds between each tick of DOT damage and cooling effects.")]
	public float tickRate = 1.0f;
	[Tooltip("How quickly the car cools down by itself over time. Higher number = quicker cooling.")]
	public float coolingRate = 1.0f;
	[Tooltip("How hot does the car need to get before the heat affects its boost capability?")]
	public float boostTempThreshold = 30.0f;
	[Tooltip("At what temperature has the heat's detrimental influence on boost reached its maximum?")]
	public float boostTempMax = 70.0f;
	[Tooltip("How hot does the car need to get before the heat starts damaging it?")]
	public float integrityTempThreshold = 70.0f;
	[Tooltip("How much integrity damage the car receives every tick while being too hot.")]
	public float integrityTempDOT = 3.0f;

	[Header("Car Integrity")]
	[Tooltip("The car's maximum integrity value. Should be different for different cars.")]
	public float maxIntegrity = 100.0f;
	[Tooltip("How many seconds between each possible damage function call.")]
	public float damageRate = 0.5f;
	[Space]
	[Tooltip("How much fire obstacles affects the car's integrity level.")]
	public float fireIntegEffect = 10.0f;
	[Tooltip("How much frost obstacles affects the car's integrity level.")]
	public float frostIntegEffect = 10.0f;
	[Tooltip("How much laser obstacles affect the car's integrity level.")]
	public float laserIntegEffect = 10.0f;
	[Tooltip("How much saw obstacles affect the car's integrity level.")]
	public float sawIntegEffect = 10.0f;
	[Space]
	[Tooltip("How much rock obstacles affect the car's integrity level.")]
	public float rockIntegEffect = 10.0f;
	[Tooltip("How high velocity is required for taking any rock collision damage, and at how high velocity max damage is taken.")]
	[Min(0)]
	public Vector2 rockVelocityMinMax = Vector2.zero;
	[Tooltip("How much % of rock collision damage is taken at velocities between min and max velocity thresholds.")]
	public AnimationCurve rockVelocityDamageCurve = AnimationCurve.Linear(0, 0, 1, 1);

	[Header("Colors")]
	public Color ExplodeColor = Color.red;
	public Color InstakillColor = Color.red;

	[Header("Smoke")]
	public VisualEffect Smoke;
	private ExposedProperty smokeValue = "SmokeValue";
	// public Vector2 SmokeMinMaxIntegrity = new Vector2(0.1f, 0.9f);
	[Range(0, 1)]
	public float SmokeMinIntegrity = .1f;
	[Range(0, 1)]
	public float SmokeMaxIntegrity = .9f;
	public AnimationCurve SmokeCurve = AnimationCurve.Linear(0, 0, 1, 1);

	// [Header("UI Scripts")]
	// [Tooltip("This car's associated temperature UI bar")]
	// public TemperatureUIScript temperatureUI;
	// [Tooltip("This car's associated integrity UI bar")]
	// public IntegrityUIScript integrityUI;

	private SteeringScript carControls;

	// TODO: speed penalty when reaching low temperature

	//Arbitrary bottom of the temperature UI bar. Only used for display purposes
	private float zeroTemp = 95.0f;
	//Around the standard celsius temperature for running car engines. Only used for display purposes
	private float standardTemp = 100.0f;
	//Around where engine celsius temperature would be dangerously high. Only used for display purposes
	private float highTemp = 180.0f;
	//Temperature values used for calculations
	private float currTemp = 0.0f;
	private float goalTemp = 0.0f;

	private bool tooHotBoost = false;
	private bool tooHotInteg = false;

	//The car's current integrity value, used in calculations
	private float currIntegrity;
	//The timer for DOT and cooling ticks
	private float tickTimer = 0.0f;
	//The timer for damage function calls
	private float damageTimer = 0.0f;

	TemperatureUIScript temperatureUI;
	IntegrityUIScript integrityUI;


	private void Start() {
		carControls = GetComponent<SteeringScript>();
		carControls.BoostStartObservers.Add(this);

		currIntegrity = maxIntegrity;

		temperatureUI = TemperatureUIScript.MainInstance;
		if (temperatureUI == null)
			Debug.Log("TemperatureAndIntegrity: " + gameObject.name + " is missing a reference to its TemperatureUIScript");

		integrityUI = IntegrityUIScript.MainInstance;
		if (integrityUI == null)
			Debug.Log("TemperatureAndIntegrity: " + gameObject.name + " is missing a reference to its IntegrityUIScript");

		UpdateSmoke();

	}

	public void BoostHeat() {
		goalTemp += boostTempEffect * Time.deltaTime;
		ValueCheck();
	}

	public void FireHit() {
		if (damageTimer <= 0.0f) {
			goalTemp += fireTempEffect;
			currIntegrity -= fireIntegEffect;
			Hit();
		}
	}
	public void FireHit(float dt) {
		// if (damageTimer <= 0.0f) {
		goalTemp += fireTempEffect * dt;
		currIntegrity -= fireIntegEffect * dt;
		Hit();
		// }
	}

	public void FrostHit(bool safe = false) {
		if (damageTimer <= 0.0f) {
			goalTemp -= frostTempEffect; // NOTE: decreases heat
			if (!safe)
				currIntegrity -= frostIntegEffect;
			Hit();
		}
	}
	public void FrostHit(float dt, bool safe = false) {
		// if (damageTimer <= 0.0f) {
		goalTemp -= frostTempEffect * dt; // NOTE: decreases heat
		if (!safe)
			currIntegrity -= frostIntegEffect * dt;
		Hit();
		// }
	}


	public void LaserHit() {
		if (damageTimer <= 0.0f) {
			goalTemp += laserTempEffect;
			currIntegrity -= laserIntegEffect;
			Hit();
		}
	}
	public void LaserHit(float dt) {
		// if (damageTimer <= 0.0f) {
		goalTemp += laserTempEffect * dt;
		currIntegrity -= laserIntegEffect * dt;
		Hit();
		// }
	}

	public void SawHit() {
		if (damageTimer <= 0.0f) {
			currIntegrity -= sawIntegEffect;
			Hit();
		}
	}

	public void RockHit(float sqrImpactVelocity) {
		float sqrVelocity = sqrImpactVelocity;//carControls.Velocity.sqrMagnitude;
		float sqrMin = rockVelocityMinMax.x * rockVelocityMinMax.x;
		if (damageTimer <= 0.0f && !carControls.IsInvulnerable && sqrVelocity > sqrMin) {
			float sqrMax = rockVelocityMinMax.y * rockVelocityMinMax.y;

			float percentage = 1f;
			if (sqrVelocity < sqrMax && rockVelocityMinMax.x != rockVelocityMinMax.y)
				percentage = Mathf.Clamp01((sqrVelocity - sqrMin) / (sqrMax - sqrMin));

			currIntegrity -= rockIntegEffect * rockVelocityDamageCurve.Evaluate(percentage);
			Hit();
		}
	}

	public void Instakill() {
		// if (!carControls.IsInvulnerable) {
		// currIntegrity = 0;
		UINotificationSystem.Notify("Your car got crushed!", InstakillColor, 2);
		carControls.Reset();
		Reset();
		// }
	}


	private void Update() {
		if (damageTimer > 0.0f)
			damageTimer -= Time.deltaTime;

		tickTimer += Time.deltaTime;
		if (tickTimer >= tickRate) {
			if (currTemp > 0.0f) goalTemp = Mathf.MoveTowards(goalTemp, 0f, coolingRate);
			if (currTemp < 0.0f) goalTemp = 0.0f;

			if (tooHotInteg) {
				currIntegrity -= integrityTempDOT;
				var integrityUI = IntegrityUIScript.MainInstance;
				if (integrityUI != null)
					integrityUI.SetIntegPercentage(currIntegrity / maxIntegrity);
				ValueCheck();
			}

			//A bit of randomization for a slightly more "realistic" temperature reading
			float randFlux = Random.Range(-0.2f, 0.2f);
			currTemp += randFlux;

			tickTimer = 0.0f;
		}

		//Do whatever effects we may want temperature to have on boost every frame 
		//Like maybe updating max boost amount
		if (tooHotBoost) { carControls.SetBoostLimit((currTemp - boostTempThreshold) / (boostTempMax - boostTempThreshold)); }

		//Lerping temperature fluctuations to make it look more natural
		currTemp = Mathf.Lerp(currTemp, goalTemp, 5.0f * Time.deltaTime);

		SetTempUI();
	}

	private void Hit() {
		SetIntegUI();
		damageTimer = damageRate;

		UpdateSmoke();

		ValueCheck();
	}

	private void UpdateSmoke() {
		if (Smoke) {
			float smokePercent = 1f - (currIntegrity / maxIntegrity);
			smokePercent = (smokePercent - SmokeMinIntegrity) / (SmokeMaxIntegrity - SmokeMinIntegrity);
			float smokeIntensity = SmokeCurve.Evaluate(Mathf.Clamp01(smokePercent));
			Smoke.SetFloat(smokeValue, smokeIntensity);
			Debug.Log("smoke updated to " + smokeIntensity + ", " + smokePercent);
		}
	}

	private void SetTempUI() {
		if (temperatureUI != null) {
			float displayTempPercent = ((standardTemp + currTemp) - zeroTemp) / (highTemp - zeroTemp);
			temperatureUI.SetTempPercentage(displayTempPercent, (standardTemp + currTemp));
		}
	}

	private void SetIntegUI() {
		if (integrityUI != null)
			integrityUI.SetIntegPercentage(currIntegrity / maxIntegrity);
	}

	private void ValueCheck() {
		if (currIntegrity <= 0.0f) {
			Debug.Log("Integrity reached 0!");
			// UINotificationSystem.Notify("Your car exploded!", ExplodeColor, 5);
			UINotificationSystem.Notify("Your car broke down! Restarting with penalty.", ExplodeColor, 5);
			// TODO: time and/or score penalty
			carControls.Reset();
			Reset();
		}

		if (currTemp >= boostTempThreshold) {
			tooHotBoost = true;
		} else {
			tooHotBoost = false;
			carControls.SetBoostLimit(0.0f);
		}

		if (currTemp >= integrityTempThreshold) {
			tooHotInteg = true;
			TemperatureWarningTextScript.Show();
		} else {
			tooHotInteg = false;
			TemperatureWarningTextScript.Hide();
		}
	}

	private void Reset() {
		currIntegrity = maxIntegrity;
		currTemp = 0.0f; goalTemp = 0.0f;
		SetTempUI(); SetIntegUI();
		UpdateSmoke();
		Smoke?.Reinit();
	}

	// on car boost start
	public void Notify(bool carIsInvulnerable) {
		BoostHeat();
	}

}
