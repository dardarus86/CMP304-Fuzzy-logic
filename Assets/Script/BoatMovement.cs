using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatMovement : MonoBehaviour
{

    [SerializeField] float motorFoamMultiplier;
    [SerializeField] float motorFoamBase;
    [SerializeField] float frontFoamMultiplier;

    [SerializeField] float trust;
    [Range (0,5)]
    [SerializeField] float thrustMultiplier = 0.1f;
    [SerializeField] float turningSpeed;

    float Volume;
    const float pH2O = 1000;

    Rigidbody rb;
    BoxCollider box;
    Water water;

  //  ParticleSystem.EmissionModule motor, front;

    void Start()
    {
        transform.position = new Vector3(-577, 2, 2);

        rb = GetComponent<Rigidbody>();
        box = GetComponent<BoxCollider>();
        water = GameObject.Find("Water").GetComponent<Water>();

      //  motor = transform.GetChild(0).GetComponent<ParticleSystem>().emission;
      //  front = transform.GetChild(1).GetComponent<ParticleSystem>().emission;
    }

    void FixedUpdate()
    {
        //Debug.Log("FixedUpdate called");
        //Debug.Log(Input.GetAxis("Horizontal"));
        if (Input.GetAxis("Horizontal") < -.4f || Input.GetAxis("Horizontal") > .4f)
        {
            transform.rotation = Quaternion.Euler(-90, transform.rotation.eulerAngles.y + Input.GetAxis("Horizontal") * turningSpeed * Time.fixedDeltaTime, 0);
        }
        if (Input.GetAxis("Throttle") > 0.2f)
        {
           // Debug.Log(Vector3.down * trust * Time.fixedDeltaTime * Input.GetAxis("Throttle") * thrustMultiplier);
            rb.AddRelativeForce(Vector3.down * trust * Time.fixedDeltaTime * Input.GetAxis("Throttle") * thrustMultiplier);
        }

      //  motor.rate = motorFoamMultiplier * Input.GetAxis("Throttle") + motorFoamBase;
       // front.rate = frontFoamMultiplier * rb.velocity.magnitude;

        Volume = box.size.x * box.size.z * (water.WaterLevel(transform.position) - transform.position.y);
        
        if (Volume > 0)
        {
           
            rb.AddForce((Vector3.up * pH2O * Physics.gravity.magnitude * Volume));
        }
    }
}
