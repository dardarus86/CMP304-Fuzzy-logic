using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FLS;
using FLS.Rules;
using FLS.MembershipFunctions;

public class FuzzyBox : MonoBehaviour {

	IFuzzyEngine engine;
	LinguisticVariable distance;
	LinguisticVariable direction;
	LinguisticVariable speed;
	Rigidbody rbody;
	bool selected = false;

	void Start()
	{
		rbody = GetComponent<Rigidbody>();
		// Here we need to setup the Fuzzy Inference System
		distance = new LinguisticVariable("distance");
		var veryFarRightDis = distance.MembershipFunctions.AddTrapezoid("veryfarright", -100, -99, -69, -63);
		var farRightDis = distance.MembershipFunctions.AddTrapezoid("farright", -69, -63, -39, -33);
		var rightDis = distance.MembershipFunctions.AddTrapezoid("right", -39, -33, -9, -1);
		var noneDis = distance.MembershipFunctions.AddTrapezoid("none", -5, -0.5, 0.5, 5);
		var leftDis = distance.MembershipFunctions.AddTrapezoid("left", 1, 9, 33, 39);
		var farLeftDis = distance.MembershipFunctions.AddTrapezoid("farleft", 33, 39, 63, 69);
		var veryFarLeftDis = distance.MembershipFunctions.AddTrapezoid("veryfarleft", 63, 69, 99, 100);

		direction = new LinguisticVariable("distance");
		var veryFarRightDir = direction.MembershipFunctions.AddTrapezoid("veryfarright", -100, -99, -69, -63);
		var farRightDir = direction.MembershipFunctions.AddTrapezoid("farright", -69, -63, -39, -33);
		var rightDir = direction.MembershipFunctions.AddTrapezoid("right", -39, -33, -9, -1);
		var noneDir = direction.MembershipFunctions.AddTrapezoid("none", -5, -0.5, 0.5, 5);
		var leftDir = direction.MembershipFunctions.AddTrapezoid("left", 1, 9, 33, 39);
		var farLeftDir = direction.MembershipFunctions.AddTrapezoid("farleft", 33, 39, 63, 69);
		var VeryFarLeftDir = direction.MembershipFunctions.AddTrapezoid("veryfarleft", 63, 69, 99, 100);

		speed = new LinguisticVariable("speed");
		var veryFast = speed.MembershipFunctions.AddTrapezoid("Veryfast", -30, -28, -22, -20);
		var Fast = speed.MembershipFunctions.AddTrapezoid("fast", -22, -20, -12, -10);
		var slow = speed.MembershipFunctions.AddTrapezoid("slow", -12, -10, 2, 0);

		engine = new FuzzyEngineFactory().Default();

		var rule1 = Rule.If(distance.Is(veryFarRightDis)).Then(direction.Is(VeryFarLeftDir));
		var rule2 = Rule.If(distance.Is(farRightDis)).Then(direction.Is(farLeftDir));
		var rule3 = Rule.If(distance.Is(rightDis)).Then(direction.Is(leftDir));
		var rule4 = Rule.If(distance.Is(noneDis)).Then(direction.Is(noneDir));
		var rule5 = Rule.If(distance.Is(leftDis)).Then(direction.Is(rightDir));
		var rule6 = Rule.If(distance.Is(farLeftDis)).Then(direction.Is(farRightDir));
		var rule7 = Rule.If(distance.Is(veryFarLeftDis)).Then(direction.Is(veryFarRightDir));

		var speedrule1 = Rule.If(distance.Is(veryFarRightDis)).Then(speed.Is(veryFast));
		var speedrule2 = Rule.If(distance.Is(veryFarLeftDis)).Then(speed.Is(veryFast));
		var speedrule3 = Rule.If(distance.Is(farRightDis)).Then(speed.Is(Fast));
		var speedrule4 = Rule.If(distance.Is(farLeftDis)).Then(speed.Is(Fast));
		var speedrule5 = Rule.If(distance.Is(rightDis)).Then(speed.Is(slow));
		var speedrule6 = Rule.If(distance.Is(leftDis)).Then(speed.Is(slow));

		engine.Rules.Add(rule1, rule2, rule3, rule4, rule5, rule6, rule7, speedrule1, speedrule2, speedrule3, speedrule4, speedrule5, speedrule6);


	}

	void FixedUpdate()
	{
		if(!selected && this.transform.position.y < 0.6f)
		{
			// Convert position of box to value between 0 and 100
			double result = engine.Defuzzify(new { distance = (double)this.transform.position.x, speed = (double)rbody.velocity.x});
			
			//double speed2 = engine.Defuzzify(new { speed = (double)rbody.velocity.x, distance = (double)distance} );

			rbody.AddForce(new Vector3((float)result, 0f, 0f));
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0)) {
			var hit = new RaycastHit();
			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out hit)){
				if (hit.transform.name == "FuzzyBox" )Debug.Log( "You have clicked the FuzzyBox");
				selected = true;
			}
		}

		if(Input.GetMouseButton(0) && selected)
		{
			float distanceToScreen = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
			Vector3 curPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceToScreen));
			transform.position = new Vector3(curPosition.x, Mathf.Max(0.5f, curPosition.y), transform.position.z);
		}

		if(Input.GetMouseButtonUp(0))
		{
			selected = false;
		}
	}
}
