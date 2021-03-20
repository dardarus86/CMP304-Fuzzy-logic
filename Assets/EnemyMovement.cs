using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FLS;
using FLS.Rules;
using FLS.MembershipFunctions;
using UnityEngine.UI;

public class EnemyMovement : MonoBehaviour
{

    public Text dotRightText;
    public Text dotForwardText;
    public Text rotationText;

    [SerializeField] float motorFoamMultiplier;
    [SerializeField] float motorFoamBase;
    [SerializeField] float frontFoamMultiplier;

    [SerializeField] Vector4 veryHardTurnNegativeVar;

    [SerializeField] float thrust;
    [SerializeField] float turningSpeed;

    IFuzzyEngine turningEngine;
    IFuzzyEngine speedEngine;

    LinguisticVariable turningDirection;
    LinguisticVariable turningNeeded;
    LinguisticVariable speed;
    LinguisticVariable distanceToPlayer;
    LinguisticVariable frontOrBack;

    double thrustMultiplier;

    float Volume;
    const float pH2O = 1000;

    Rigidbody rb;
    BoxCollider box;
    Water water;
    GameObject player;
    // Start is called before the first frame update
    void Start()
    {
      //  transform.position = new Vector3(569.0f, 6.04f, 254.0f);
        rb = GetComponent<Rigidbody>();
        box = GetComponent<BoxCollider>();
        player = GameObject.FindGameObjectWithTag("Player");
        water = GameObject.Find("Water").GetComponent<Water>();

        turningEngine = new FuzzyEngineFactory().Default();
        speedEngine = new FuzzyEngineFactory().Default();

        turningNeeded = new LinguisticVariable("turningNeeded");
        var veryFarPositive = turningNeeded.MembershipFunctions.AddTrapezoid("VeryFarPositive", 0, 0.2, 0.5, 0.65);
        var farPositive = turningNeeded.MembershipFunctions.AddTrapezoid("FarPositive", 0.6, 0.7, 0.8, 0.9);
        var positive = turningNeeded.MembershipFunctions.AddTrapezoid("Positive", 0.8, .90, 0.95, 1);
        var straight = turningNeeded.MembershipFunctions.AddTrapezoid("Straight", -0.12, -0.05, 0.05, 0.12);
        var negative = turningNeeded.MembershipFunctions.AddTrapezoid("Right", -1, -0.95, -0.9, -0.8);
        var farNegative = turningNeeded.MembershipFunctions.AddTrapezoid("FarRight", -0.9, -0.8, -.7, -.6);
        var veryFarNegative = turningNeeded.MembershipFunctions.AddTrapezoid("VeryFarPositive", -0.65, -0.5, -0.2, 0);


        frontOrBack = new LinguisticVariable("FrontOrBack");
        var front = frontOrBack.MembershipFunctions.AddTrapezoid("Front", 0, 0, 1, 1);
        var behind = frontOrBack.MembershipFunctions.AddTrapezoid("Behind", -1, -1, 0, 0);

        turningDirection = new LinguisticVariable("TurningDirection");
        //direction taken from the objects forward vector, left being anti clockwise, right being clockwise not from camera perspective
        var veryHardTurnNegative = turningDirection.MembershipFunctions.AddTrapezoid("VeryHardTurnNegative", -2, -1.5, -1, -0.5);
        var hardTurnNegative = turningDirection.MembershipFunctions.AddTrapezoid("HardTurnNegative", -1.5, -1, -0.5, -0.25);
        var turnDirNegative = turningDirection.MembershipFunctions.AddTrapezoid("TurnDirNegative", -0.5, -0.25, -0.2, -0.15);
        var noTurn = turningDirection.MembershipFunctions.AddTrapezoid("NoTurn", -0.2, -0.15, 0.15, 0.2);
        var turnDirPositive = turningDirection.MembershipFunctions.AddTrapezoid("TurnDirPositive", 0.15, 0.2, 0.25, 0.5);
        var hardTurnPositive = turningDirection.MembershipFunctions.AddTrapezoid("HardTurnPositive", 0.25, 0.5, 1, 1.5);
        var veryHardTurnPositive = turningDirection.MembershipFunctions.AddTrapezoid("VeryHardTurnPositive", 0.5, 1, 1.5, 2);

        //var veryHardTurnNegative = turningDirection.MembershipFunctions.AddTrapezoid("VeryHardTurnNegative", -5, -4, -2, -1);
        //var hardTurnNegative = turningDirection.MembershipFunctions.AddTrapezoid("HardTurnNegative", -2, -1, -0.5, -0.2);
        //var turnDirNegative = turningDirection.MembershipFunctions.AddTrapezoid("TurnDirNegative", -0.40, -0.20, -0.10, -0.05);
        //var noTurn = turningDirection.MembershipFunctions.AddTrapezoid("NoTurn", -0.07, -0.03, 0.03, 0.07);
        //var turnDirPositive = turningDirection.MembershipFunctions.AddTrapezoid("TurnDirPositive", 0.05, 0.10, 0.20, 0.40);
        //var hardTurnPositive = turningDirection.MembershipFunctions.AddTrapezoid("HardTurnPositive", 0.2, 0.5, 1, 2);
        //var veryHardTurnPositive = turningDirection.MembershipFunctions.AddTrapezoid("VeryHardTurnPositive", 1, 2, 4, 5);


        distanceToPlayer = new LinguisticVariable("DistanceToPlayer");
        var veryFarAway = distanceToPlayer.MembershipFunctions.AddTrapezoid("VeryFarAway", 400, 900, 1100, 2000);
        var farAway = distanceToPlayer.MembershipFunctions.AddTrapezoid("FarAway", 150, 200, 300, 450);
        var optimal = distanceToPlayer.MembershipFunctions.AddTrapezoid("Optimal", 70, 90, 150, 170);
        var tooClose = distanceToPlayer.MembershipFunctions.AddTrapezoid("TooClose", 0, 0, 70, 70);

        speed = new LinguisticVariable("Speed");
        var fast = speed.MembershipFunctions.AddTrapezoid("Fast",       0.40, 0.60, 0.7, 0.80);
        var slow = speed.MembershipFunctions.AddTrapezoid("Slow",       0.20, 0.30, 0.40, 0.60);
        var crawl = speed.MembershipFunctions.AddTrapezoid("Crawl",     0.10, 0.15, 0.20, 0.30);
        var reverse = speed.MembershipFunctions.AddTrapezoid("Reverse",-0.10,-0.08,-0.06,-0.40);

        // if ship in front && if dot = 0 to .50 then very hard turn negative
        // if ship in front && if dot = .40 to .80 then  hard turn negative
        // if ship in front && if dot = .70 to 1 then  hard turn negative

        // if ship in front && if dot = 0 to -.50 then very hard turn positive
        // if ship in front && if dot = -.40 to -.80 then  hard turn positive
        // if ship in front && if dot = -.70 to -1 then  hard turn positive

        // if ship is behind && if dot = 0 to .50 then very hard turn negative
        // if ship in behind && if dot = .40 to .80 then  hard turn negative
        // if ship in behind && if dot = .70 to 1 then  hard turn negative

        // if ship in behind && if dot = 0 to -.50 then very hard turn positive
        // if ship in behind && if dot = -.40 to -.80 then  hard turn positive
        // if ship in behind && if dot = -.70 to -1 then  hard turn positive


        var turningRule1 = Rule.If(turningNeeded.Is(veryFarPositive).And(frontOrBack.Is(front))).Then(turningDirection.Is(turnDirNegative));
        var turningRule2 = Rule.If(turningNeeded.Is(farPositive).And(frontOrBack.Is(front))).Then(turningDirection.Is(hardTurnNegative));
        var turningRule3 = Rule.If(turningNeeded.Is(positive).And(frontOrBack.Is(front))).Then(turningDirection.Is(veryHardTurnNegative));
        var turningRule4 = Rule.If(turningNeeded.Is(straight).And(frontOrBack.Is(front))).Then(turningDirection.Is(noTurn));
        var turningRule5 = Rule.If(turningNeeded.Is(negative).And(frontOrBack.Is(front))).Then(turningDirection.Is(veryHardTurnPositive));
        var turningRule6 = Rule.If(turningNeeded.Is(farNegative).And(frontOrBack.Is(front))).Then(turningDirection.Is(hardTurnPositive));
        var turningRule7 = Rule.If(turningNeeded.Is(veryFarNegative).And(frontOrBack.Is(front))).Then(turningDirection.Is(turnDirPositive));

        var turningRule8 = Rule.If(turningNeeded.Is(veryFarPositive).And(frontOrBack.Is(behind))).Then(turningDirection.Is(veryHardTurnNegative));
        var turningRule9 = Rule.If(turningNeeded.Is(farPositive).And(frontOrBack.Is(behind))).Then(turningDirection.Is(hardTurnNegative));
        var turningRule10 = Rule.If(turningNeeded.Is(positive).And(frontOrBack.Is(behind))).Then(turningDirection.Is(turnDirNegative));
        var turningRule11 = Rule.If(turningNeeded.Is(straight).And(frontOrBack.Is(behind))).Then(turningDirection.Is(noTurn));
        var turningRule12 = Rule.If(turningNeeded.Is(negative).And(frontOrBack.Is(behind))).Then(turningDirection.Is(turnDirPositive));
        var turningRule13 = Rule.If(turningNeeded.Is(farNegative).And(frontOrBack.Is(behind))).Then(turningDirection.Is(hardTurnPositive));
        var turningRule14 = Rule.If(turningNeeded.Is(veryFarNegative).And(frontOrBack.Is(behind))).Then(turningDirection.Is(veryHardTurnPositive));





        var speedrule1 = Rule.If(distanceToPlayer.Is(veryFarAway)).Then(speed.Is(fast));
        var speedrule2 = Rule.If(distanceToPlayer.Is(farAway)).Then(speed.Is(slow));
        var speedrule3 = Rule.If(distanceToPlayer.Is(optimal)).Then(speed.Is(crawl));
        var speedrule4 = Rule.If(distanceToPlayer.Is(tooClose)).Then(speed.Is(reverse));

        //If(frontOrback.Is(Front)).If(turningNeeded.Is(HardTurnLeft).Then(turningDirection.Is(hardLeftTurnPositive));

        turningEngine.Rules.Add(turningRule1, turningRule2, turningRule3, turningRule4, turningRule5, turningRule6, turningRule7,
                                turningRule8, turningRule9, turningRule10, turningRule11, turningRule12, turningRule13, turningRule14);
        speedEngine.Rules.Add(speedrule1, speedrule2, speedrule3, speedrule4);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 tarDirection = (transform.position - player.transform.position).normalized;
        double distToPlayer = Vector3.Distance(transform.position, player.transform.position);
        double shipDotRight = Vector3.Dot(transform.right, tarDirection);
        double shipDotForward = Vector3.Dot(transform.up, tarDirection); //original model was rotated so up is forward

      


        double angle = turningEngine.Defuzzify(new { frontOrBack = shipDotRight, turningNeeded = shipDotForward });
        // double angle = turningEngine.Defuzzify(new { otherShipsPosition = shipDotRight });
        thrustMultiplier = speedEngine.Defuzzify(new { distanceToPlayer = distToPlayer});

        dotRightText.text = "DotRight: " + shipDotRight.ToString("F");
        dotForwardText.text = "DotForward: " + shipDotForward.ToString("F");
        rotationText.text = "Rotation: " + angle.ToString();
        // Debug.Log(shipDotRight);
        Debug.DrawRay(transform.position, transform.right * 50, Color.red);
        Debug.DrawRay(transform.position, -transform.up * 50, Color.blue);
        Debug.DrawRay(transform.position, tarDirection * 100, Color.green);
        Debug.Log(" Angle" + angle);
        Debug.Log("Right:" +  shipDotRight);
        Debug.Log("Forward:" + shipDotForward);

        if (distToPlayer >= 200)
        {
            transform.LookAt(new Vector3(player.transform.position.x - 90, player.transform.position.y, player.transform.position.z));
            transform.localEulerAngles = (new Vector3(-90, transform.localEulerAngles.y, transform.localEulerAngles.z));
        }
        else
        {

            //  transform.localEulerAngles = (new Vector3(-90, 0, transform.localEulerAngles.z + (float)angle));
            if (shipDotForward > 0)
            {


                //transform.localEulerAngles = (new Vector3(-90, 0, transform.localEulerAngles.y + (float)angle));
                //ship is in front of AI
                if (shipDotRight > 0 && shipDotRight < 0.92)
                {
                    transform.localEulerAngles = (new Vector3(-90, 0, transform.localEulerAngles.y - (float)angle/2));
                }
                else if (shipDotRight < 0 && shipDotRight > -0.92)
                {
                    transform.localEulerAngles = (new Vector3(-90, 0, transform.localEulerAngles.y + (float)angle / 2));
                }
            }
            else
            {
                // transform.localEulerAngles = (new Vector3(-90, 0, transform.localEulerAngles.y + (float)angle));
                ////ship is behind the AI
                if ((shipDotRight < 0 && shipDotRight > -0.92))

                {
                    transform.localEulerAngles = (new Vector3(-90, 0, transform.localEulerAngles.y + (float)angle / 2));
                }
                else if ((shipDotRight > 0 && shipDotRight < 0.92))
                {
                    transform.localEulerAngles = (new Vector3(-90, 0, transform.localEulerAngles.y - (float)angle / 2));
                }
            }
            Debug.Log("EularAngle" + transform.localEulerAngles.y);
            if (shipDotRight < -0.92 && distToPlayer > 50 && distToPlayer < 160)
            {
                Debug.Log(" FIRE RIGHT SIDE!!");
            }
            else if (shipDotRight > 0.92 && distToPlayer > 50 && distToPlayer < 160)
            {
                Debug.Log(" FIRE LEFT SIDE!!");
            }
        }
      //  }
        

    }

    private void FixedUpdate()
    {
        rb.AddRelativeForce(Vector3.down * thrust * Time.fixedDeltaTime * (float)thrustMultiplier);
        Volume = box.size.x * box.size.z * (water.WaterLevel(transform.position) - transform.position.y);

        //dot product forward to see if the boat is to the left or to the right,then dot product to the right to see if its in front or behind the AI

        if (Volume > 0)
        {

            rb.AddForce((Vector3.up * pH2O * Physics.gravity.magnitude * Volume));
        }
    }
}

