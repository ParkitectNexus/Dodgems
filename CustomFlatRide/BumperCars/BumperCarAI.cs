using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BumperCarAI : MonoBehaviour {
    
    private Rigidbody _rigidbody;

    private Vector3 _wanderTarget = Vector3.zero;

    public float wanderRadius = 2;

    public float wanderDistance = 8;

    public float wanderJitter = 0.1f;

    public float movingSpeed = 3;

    private float _oldHeading;

    public BumperCars BumperCars { get; set; }

    private List<BumperCar> _bumperCars;

    private bool _seeking = false;

    public BumperCarAI Target = null;

    public BumperCarAI FleeTarget = null;

    private enum STATUS
    {
        PURSUIING,
        FLEEING,
        SEEKING
    }

    private STATUS _status = STATUS.SEEKING;
    private bool _fleeing = false;

    void Awake()
    {
        CapsuleCollider collider = gameObject.AddComponent<CapsuleCollider>();

        collider.center = new Vector3(0.001384966f, 0.1229123f, 0.03784022f);
        collider.radius = 0.25f;
        collider.height = 0.01481656f;

        _rigidbody = gameObject.AddComponent<Rigidbody>();
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Start()
    {
        _bumperCars = BumperCars.GetComponentsInChildren<BumperCar>().Except(new [] { gameObject.GetComponent<BumperCar>() }).ToList();

        Physics.gravity = new Vector3(0, -9.81f, 0);
    }

    void FixedUpdate()
    {
        _rigidbody.mass = 9;
        switch (_status)
        {
            case STATUS.SEEKING:
                Seek();
                break;
            case STATUS.PURSUIING:
                Pursuit();
                break;
            case STATUS.FLEEING:
                Flee();
                break;
        }

        float wantedheading = Mathf.Atan2(_rigidbody.velocity.x, _rigidbody.velocity.z);

        float heading = Mathf.Lerp(_oldHeading, wantedheading, 4f * Time.deltaTime);

        _oldHeading = heading;

        transform.rotation = Quaternion.Euler(0, heading * Mathf.Rad2Deg, 0);
    }

    private void Pursuit()
    {
        if (Target != null)
        {
            Vector3 direction = Vector3.zero;

            direction += WallAvoidance();
            direction += Pursuit(Target.transform.position, Target.GetComponent<Rigidbody>().velocity);

            _rigidbody.AddForce(direction.normalized * movingSpeed);
        }
    }

    private void Flee()
    {
        if (!_seeking)
        {
            StartCoroutine(Fleeing());
            _fleeing = true;
        }
        
        Vector3 direction = Vector3.zero;

        if (FleeTarget != null)
        {
            direction += Separation();
            direction += Flee(FleeTarget.transform.position);
            //direction += Cohesion();
            direction += WallAvoidance();
        }

        _rigidbody.AddForce(direction.normalized * movingSpeed);
    }

    private IEnumerator Fleeing()
    {
        yield return new WaitForSeconds(Random.value * 0.3f + 0.2f);
        
        _status = STATUS.SEEKING;
        _fleeing = false;
    }

    private void Seek()
    {
        if (!_seeking)
        {
            _wanderTarget = transform.InverseTransformPoint(Vector3.forward);
            StartCoroutine(Seeking());
            _seeking = true;
        }

        Vector3 direction = Vector3.zero;

        direction += Wander();
        direction += Separation();
        //direction += Cohesion();
        direction += WallAvoidance();

        _rigidbody.AddForce(direction.normalized * movingSpeed);
    }

    private IEnumerator Seeking()
    {
        yield return new WaitForSeconds(Random.value * 0.3f + 0.2f);

        Target = FindTarget(this);
        _status = STATUS.PURSUIING;
        _seeking = false;
    }


    void OnCollisionStay(Collision collision)
    {
        BumperCarAI bumperCarAi = collision.gameObject.GetComponent<BumperCarAI>();
        if (bumperCarAi != null)
        {
            if (Target == bumperCarAi) // op zoek naar hem
            {
                _status = STATUS.SEEKING;
            }
            else
            {
                FleeTarget = bumperCarAi;
                _status = STATUS.FLEEING;
            }
        }
    }

    private BumperCarAI FindTarget(BumperCarAI excluded)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 10);

        List<BumperCarAI> cars = new List<BumperCarAI>();

        foreach (Collider collider in colliders)
        {
            BumperCarAI BumperCarAI = collider.GetComponent<BumperCarAI>();
            if (BumperCarAI != null)
            {
                if (BumperCarAI.gameObject == gameObject || BumperCarAI.gameObject == excluded.gameObject)
                    continue;

                if (transform.TransformPoint(BumperCarAI.gameObject.transform.position).z < 0)
                {
                    cars.Add(BumperCarAI);
                }
                else
                {
                    for (int i = 0; i < 3; i++)
                    {
                        cars.Add(BumperCarAI);
                    }
                }
            }
        }

        return cars[(int)(Random.value * cars.Count)];
    }
    
    public Vector3 Wander()
    {
        const float wanderRadius = 2;

        const float wanderDistance = 4;

        const float wanderJitter = 0.5f;

        _wanderTarget += new Vector3((Random.value * 2f - 1) * wanderJitter, 0, (Random.value * 2f - 1) * wanderJitter);

        _wanderTarget.Normalize();

        _wanderTarget *= wanderRadius;

        Vector3 targetLocal = _wanderTarget + Vector3.forward * wanderDistance;

        Vector3 targetWorld = transform.TransformPoint(targetLocal);

        return Seek(targetWorld);
    }

    public Vector3 Separation()
    {
        Vector3 steeringForce = Vector3.zero;

        foreach (BumperCar bumperCar in _bumperCars)
        {
            Vector3 toAgent = transform.position - bumperCar.transform.position;

            if (toAgent.magnitude != 0)
            {
                //scale the force inversely proportional to the agents distance  
                //from its neighbor.
                steeringForce += toAgent.normalized/toAgent.magnitude;
            }
        }

        return steeringForce.normalized;
    }
    
    public Vector3 Cohesion()
    {
        //first find the center of mass of all the agents
        Vector3 centerOfMass = Vector3.zero;
        Vector3 steeringForce = Vector3.zero;

        int NeighborCount = 0;

        foreach (BumperCar bumperCar in _bumperCars)
        {
            centerOfMass += bumperCar.transform.position;

            ++NeighborCount;
        }

        if (NeighborCount > 0)
        {
            //the center of mass is the average of the sum of positions
            centerOfMass /= NeighborCount;

            //now seek towards that position
            steeringForce = Seek(centerOfMass);
        }

        //the magnitude of cohesion is usually much larger than separation or
        //allignment so it usually helps to normalize it.
        return steeringForce.normalized;
    }

    Vector3 WallAvoidance()
    {
  //the feelers are contained in a std::vector, m_Feelers
  //CreateFeelers();

    float DistToThisIP = 0.0f;
        float DistToClosestIP = float.MaxValue;


        Vector3 SteeringForce = Vector3.zero;
        Vector3 point = Vector3.zero;
        Vector3 ClosestPoint = Vector3.zero;

        Vector3[] feelers = new[] {Vector3.left, Vector3.forward, Vector3.right};

        foreach (Vector3 feeler in feelers)
        {
            //this will hold an index into the vector of walls
            GameObject ClosestWall = null;

            Vector3 worldFeeler = transform.TransformDirection(feeler);

            RaycastHit hit;

            Ray ray = new Ray(transform.position, worldFeeler);

            if (Physics.Raycast(ray, out hit, 5))
            {
                if (hit.collider.gameObject.GetComponent<BumperCarAI>() != null) // een muur
                {
                    //is this the closest found so far? If so keep a record
                    if (DistToThisIP < DistToClosestIP)
                    {
                        DistToClosestIP = DistToThisIP;

                        ClosestWall = hit.collider.gameObject;

                        ClosestPoint = hit.point;
                    }
                }
            }

            //if an intersection point has been detected, calculate a force  
            //that will direct the agent away
            if (ClosestWall != null)
            {
                //calculate by what distance the projected position of the agent
                //will overshoot the wall
                Vector3 OverShoot = worldFeeler - ClosestPoint;

                Vector3 normal = new Vector3(
                    Mathf.Abs(ClosestWall.transform.position.x) > Mathf.Abs(ClosestWall.transform.position.z) ? -ClosestWall.transform.position.x : 0,
                    0,
                    Mathf.Abs(ClosestWall.transform.position.z) > Mathf.Abs(ClosestWall.transform.position.x) ? -ClosestWall.transform.position.z : 0);

                //create a force in the direction of the wall normal, with a 
                //magnitude of the overshoot
                SteeringForce = normal * OverShoot.magnitude;
            }
        }

        return SteeringForce.normalized;
    }


    public Vector3 Flee(Vector3 toEvade)
    {
        Vector3 desiredVelocity = (transform.position - toEvade).normalized * movingSpeed;

        return desiredVelocity - _rigidbody.velocity;
    }

    public Vector3 Pursuit(Vector3 evaderPos, Vector3 evaderVelocity)
    {
        //if the evader is ahead and facing the agent then we can just seek
        //for the evader's current position.
        Vector3 ToEvader = evaderPos - transform.position;

        double RelativeHeading = Vector3.Dot(_rigidbody.velocity.normalized, evaderVelocity.normalized);

        if ((Vector3.Dot(evaderVelocity.normalized, _rigidbody.velocity.normalized) > 0) && (RelativeHeading < -0.95))
        {
            return Seek(evaderPos);
        }

        //Not considered ahead so we predict where the evader will be.

        //the lookahead time is propotional to the distance between the evader
        //and the pursuer; and is inversely proportional to the sum of the
        //agent's velocities
        float LookAheadTime = evaderVelocity.magnitude / (movingSpeed + evaderVelocity.magnitude);

        //now seek to the predicted future position of the evader
        return Seek(evaderPos + evaderVelocity * LookAheadTime);
    }

    public Vector3 Seek(Vector3 targetPos)
    {
        Vector3 desiredVelocity = (targetPos - transform.position).normalized * movingSpeed;

        return (desiredVelocity - _rigidbody.velocity).normalized;
    }
}
