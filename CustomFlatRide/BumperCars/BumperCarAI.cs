using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BumperCars.CustomFlatRide.BumperCars
{
    public class BumperCarAi : MonoBehaviour
    {
        private Rigidbody _rigidbody;

        private Vector3 _wanderTarget = Vector3.zero;

        public float WanderRadius = 2;

        private readonly float _wanderDistance = 8;

        private readonly float _wanderJitter = 0.1f;

        private readonly float _movingSpeed = 10;

        private float _oldHeading;

        public FlatRideScript.BumperCars BumperCars { get; set; }

        private List<BumperCar> _bumperCars;

        private bool _seeking;

        public BumperCarAi Target;

        public BumperCarAi FleeTarget;

        private enum Status
        {
            Pursuiing,
            FLEEING,
            SEEKING
        }

        private Status _status = Status.SEEKING;

        void Awake()
        {
            CapsuleCollider collider = gameObject.AddComponent<CapsuleCollider>();

            collider.center = new Vector3(0.001384966f, 0.1229123f, 0.03784022f);
            collider.radius = 0.25f;
            collider.height = 0.01481656f;

            _rigidbody = gameObject.AddComponent<Rigidbody>();
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
            _rigidbody.mass = 3;
        }

        void Start()
        {
            _bumperCars = BumperCars.GetComponentsInChildren<BumperCar>().Except(new[] { gameObject.GetComponent<BumperCar>() }).ToList();
        }

        void FixedUpdate()
        {
            switch (_status)
            {
                case Status.SEEKING:
                    Seek();
                    break;
                case Status.Pursuiing:
                    Pursuit();
                    break;
                case Status.FLEEING:
                    Flee();
                    break;
            }

            float wantedheading = Mathf.Atan2(_rigidbody.velocity.x, _rigidbody.velocity.z);

            float heading = Mathf.Lerp(_oldHeading, wantedheading, 4f * Time.deltaTime);

            _oldHeading = heading;

            transform.rotation = Quaternion.Euler(0, heading * Mathf.Rad2Deg, 0);
        }

        void OnCollisionStay(Collision collision)
        {
            BumperCarAi bumperCarAi = collision.gameObject.GetComponent<BumperCarAi>();
            if (bumperCarAi != null)
            {
                if (Target == bumperCarAi) // op zoek naar hem
                {
                    _status = Status.SEEKING;
                }
                else
                {
                    FleeTarget = bumperCarAi;
                    _status = Status.FLEEING;
                }
            }
            else if (collision.gameObject.name.StartsWith("Bound"))
            {
                _status = Status.SEEKING;
            }
            else if (collision.gameObject.name.StartsWith("Mouse") || collision.gameObject.name.StartsWith("Land"))
            {
                Physics.IgnoreCollision(GetComponent<Collider>(), collision.collider);
            }
        }

        private void Pursuit()
        {
            if (Target != null)
            {
                Vector3 direction = Vector3.zero;

                direction += WallAvoidance();
                direction += Pursuit(Target.transform.position, Target.GetComponent<Rigidbody>().velocity);

                _rigidbody.AddForce(direction.normalized * _movingSpeed);
            }
        }

        private void Flee()
        {
            if (!_seeking)
            {
                StartCoroutine(Fleeing());
            }

            Vector3 direction = Vector3.zero;

            if (FleeTarget != null)
            {
                direction += Separation();
                direction += Flee(FleeTarget.transform.position);
                //direction += Cohesion();
                direction += WallAvoidance();
            }

            _rigidbody.AddForce(direction.normalized * _movingSpeed);
        }

        private IEnumerator Fleeing()
        {
            yield return new WaitForSeconds(Random.value * 0.3f + 0.2f);

            _status = Status.SEEKING;
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
            //direction += Separation();
            direction += Cohesion();
            direction += WallAvoidance();

            _rigidbody.AddForce(direction.normalized * _movingSpeed);
        }

        private IEnumerator Seeking()
        {
            yield return new WaitForSeconds(Random.value * 0.3f + 0.2f);

            Target = FindTarget(this);
            _status = Status.Pursuiing;
            _seeking = false;
        }

        private BumperCarAi FindTarget(BumperCarAi excluded)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, 10);

            List<BumperCarAi> cars = new List<BumperCarAi>();

            foreach (Collider collider in colliders)
            {
                BumperCarAi bumperCarAi = collider.GetComponent<BumperCarAi>();
                if (bumperCarAi != null)
                {
                    if (bumperCarAi.gameObject == gameObject || bumperCarAi.gameObject == excluded.gameObject)
                        continue;

                    if (transform.TransformPoint(bumperCarAi.gameObject.transform.position).z < 0)
                    {
                        cars.Add(bumperCarAi);
                    }
                    else
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            cars.Add(bumperCarAi);
                        }
                    }
                }
            }

            return cars[(int)(Random.value * cars.Count)];
        }

        public Vector3 Wander()
        {
            _wanderTarget += new Vector3((Random.value * 2f - 1) * _wanderJitter, 0, (Random.value * 2f - 1) * _wanderJitter);

            _wanderTarget.Normalize();

            _wanderTarget *= WanderRadius;

            Vector3 targetLocal = _wanderTarget + Vector3.forward * _wanderDistance;

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
                    steeringForce += toAgent.normalized / toAgent.magnitude;
                }
            }

            return steeringForce.normalized;
        }

        public Vector3 Cohesion()
        {
            Vector3 centerOfMass = Vector3.zero;
            Vector3 steeringForce = Vector3.zero;

            int neighborCount = 0;

            foreach (BumperCar bumperCar in _bumperCars)
            {
                centerOfMass += bumperCar.transform.position;

                ++neighborCount;
            }

            if (neighborCount > 0)
            {
                centerOfMass /= neighborCount;
            
                steeringForce = Seek(centerOfMass);
            }
        
            return steeringForce.normalized;
        }

        Vector3 WallAvoidance()
        {
            float distToThisIp = 0.0f;
            float distToClosestIp = float.MaxValue;
            
            Vector3 steeringForce = Vector3.zero;
            Vector3 closestPoint = Vector3.zero;

            Vector3[] feelers = { Vector3.left, Vector3.forward, Vector3.right };

            foreach (Vector3 feeler in feelers)
            {
                GameObject closestWall = null;

                Vector3 worldFeeler = transform.TransformDirection(feeler);

                Ray ray = new Ray(transform.position, worldFeeler);

                RaycastHit[] hits = Physics.RaycastAll(ray, 5);

                if (hits.Any())
                {
                    foreach (RaycastHit hit in hits)
                    {
                        if (hit.collider.gameObject.name.StartsWith("Bound")) // een muur
                        {
                            if (distToThisIp < distToClosestIp)
                            {
                                distToClosestIp = distToThisIp;

                                closestWall = hit.collider.gameObject;

                                closestPoint = hit.point;
                            }

                            break;
                        }
                    }
                }
            
                if (closestWall != null)
                {
                    Vector3 overShoot = worldFeeler - closestPoint;

                    Vector3 normal = new Vector3(
                        Mathf.Abs(closestWall.transform.position.x) > Mathf.Abs(closestWall.transform.position.z) ? -closestWall.transform.position.x : 0,
                        0,
                        Mathf.Abs(closestWall.transform.position.z) > Mathf.Abs(closestWall.transform.position.x) ? -closestWall.transform.position.z : 0);
                
                    steeringForce = normal * overShoot.magnitude;
                }
            }

            return steeringForce.normalized;
        }


        public Vector3 Flee(Vector3 toEvade)
        {
            Vector3 desiredVelocity = (transform.position - toEvade).normalized * _movingSpeed;

            return desiredVelocity - _rigidbody.velocity;
        }

        public Vector3 Pursuit(Vector3 evaderPos, Vector3 evaderVelocity)
        {
            float relativeHeading = Vector3.Dot(_rigidbody.velocity.normalized, evaderVelocity.normalized);

            if ((Vector3.Dot(evaderVelocity.normalized, _rigidbody.velocity.normalized) > 0) && (relativeHeading < -0.95))
            {
                return Seek(evaderPos);
            }

            float lookAheadTime = evaderVelocity.magnitude / (_movingSpeed + evaderVelocity.magnitude);
        
            return Seek(evaderPos + evaderVelocity * lookAheadTime);
        }

        public Vector3 Seek(Vector3 targetPos)
        {
            Vector3 desiredVelocity = (targetPos - transform.position).normalized * _movingSpeed;

            return (desiredVelocity - _rigidbody.velocity).normalized;
        }
    }
}
