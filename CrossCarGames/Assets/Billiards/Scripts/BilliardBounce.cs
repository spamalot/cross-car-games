// Copyright (C) 2017-2019 Matthew Lakier
// 
// This file is part of CrossCarGames.
// 
// CrossCarGames is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// CrossCarGames is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with CrossCarGames.  If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SpamalotExtensions;

/// <summary>
/// junk class for all billiard ball behavior -- eventually separate out into different classes
/// </summary>
[RequireComponent(typeof(DeadSimplePhysics))]
public class BilliardBounce : MonoBehaviour {

    public enum StateType { None, Aiming, Airborne, Dead }

    public StateType State { get; set; } = StateType.Aiming;
    private StateType _lastState = StateType.None;

    // note: type might change
    public GameObject SourcePlayer { get; set; }
    public GameObject TargetPlayer { get; set; }
    public int BounceCount { get; private set; } = 1;
    public Vector3 AnticipatedLaunchDirection { get; set; }

    public LayerMask billiardLayerMask;
    [Layer]
    public int floorLayer;
    public ParticleSystem destroyParticleSystem;
    public ParticleSystem trailParticleSystem;
    public LineRenderer aimingLine;
    public LineRenderer targetLine;
    public LineRenderer phoneTargetLine;

    private DeadSimplePhysics _phys;
    private PlayerDB playerDB;

    private List<RelativeTransform> _pastLinePositions = new List<RelativeTransform>();
    private List<RelativeTransform> _futureLinePositions = new List<RelativeTransform>();
    private MagnitudeRay? _nextBounce = null;
    private HashSet<Collider> _usedCars = new HashSet<Collider>();

    private bool _isSlowMo = false;
    private int _deadCounter;
    private Vector3 coordFrame;

    private Vector3 PerceivedVelocity { get {
            return (State == StateType.Aiming ? AnticipatedLaunchDirection : _phys.velocity) - coordFrame;
        } }

    private struct RelativeTransform {
        public Transform Transform { get; private set; }
        public Vector3 Offset { get; private set; }
        public RelativeTransform(Transform transform, Vector3 offset) {
            Transform = transform;
            Offset = offset;
        }
        public Vector3 GetPosition() {
            return Transform.position + Offset;
        }
        public static RelativeTransform FromGlobal(Transform transform, Vector3 position) {
            return new RelativeTransform(transform, position - transform.position);
        }
    }

    private struct MagnitudeRay {
        public Vector3 Origin { get; private set; }
        public Vector3 Direction { get; private set; }
        public MagnitudeRay(Vector3 origin, Vector3 direction) {
            Origin = origin;
            Direction = direction;
        }
        public Ray ToRay() {
            return new Ray(Origin, Direction.normalized);
        }
    }

    [System.Obsolete("", true)]
    public void GoSlowMo() {
        if (_isSlowMo) {
            return;
        }
        _isSlowMo = true;
        //lifeSpanFrames *= 2;
        _phys.velocity /= 3;
    }

    [System.Obsolete("", true)]
    public void CancelSlowMo() {
        if (!_isSlowMo) {
            return;
        }
        _isSlowMo = false;
        _phys.velocity *= 3;
    }

    void Start() {
        _phys = GetComponent<DeadSimplePhysics>();
        playerDB = GameObject.Find("PlayerDB").GetComponent<PlayerDB>();
        _phys.OnCollided += OnCollided;
        GetComponent<Renderer>().enabled = false;
    }

    private void StateAiming() {
        if (_lastState != State) {
            _usedCars.Add(SourcePlayer.GetPrefabInterface<VehicleInterface>().vehicle.GetPrefabInterface<BodyCollidableInterface>().bodyCollider.GetComponent<Collider>());
        }
        // Need to do repeatedly to always snap to controller position while launching.
        _pastLinePositions.Clear();
        _pastLinePositions.Add(RelativeTransform.FromGlobal(SourcePlayer.GetPrefabInterface<TransformableInterface>().transformable, transform.position));
        coordFrame = SourcePlayer.GetPrefabInterface<VehicleInterface>().vehicle.GetComponentInChildren<CarAI>().Velocity;
    }

    private void StateAirborne() {
        if (_lastState != State) {
            GetComponent<Renderer>().enabled = true;
            _phys.velocity = AnticipatedLaunchDirection;
            _deadCounter = 0;
            var main = trailParticleSystem.main;
            main.customSimulationSpace = SourcePlayer.GetPrefabInterface<VehicleInterface>().vehicle.GetPrefabInterface<TransformableInterface>().transformable;
            trailParticleSystem.Play();
            GetComponent<AudioSource>().Play();
        } else if (_nextBounce == null) {
            if (_deadCounter++ > 30) {
                State = StateType.Dead;
            }
        }
    }

    private void StateDead() {
        if (_lastState != State) {
            destroyParticleSystem.Play();
            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<Collider>().enabled = false;
            aimingLine.enabled = false;
            targetLine.enabled = false;
            // Give death particle effect time to animate
            Invoke("GoodBye", 4);
        }
    }

    void Update() {

        var startState = State;

        //Debug.Log("the vel is " + _phys.velocity);
        
        
        //aimingLine.positionCount = 2;
        //aimingLine.SetPositions(new Vector3[] { transform.position, transform.position + _phys.velocity });// ((State == StateType.Aiming || _lastState == StateType.Aiming) ? (AnticipatedLaunchDirection-_phys.velocity) : _phys.velocity) * 1f });


        //Debug.Log($"info {State} X {_nextBounce} X {AnticipatedLaunchDirection} X {_phys.velocity}");

        switch (State) {
            case StateType.Aiming: StateAiming(); break;
            case StateType.Airborne: StateAirborne(); break;
            case StateType.Dead: StateDead(); break;
            default: throw new System.Exception("Invalid state");
        }

        _lastState = startState;

        UpdateLinePositions();
        var posns = _pastLinePositions.Concat(_futureLinePositions).Select(x => x.GetPosition()).ToArray();
        if (posns.Length == 1) {
            aimingLine.positionCount = 2;
            aimingLine.SetPositions(new Vector3[] { posns[0], posns[0] + PerceivedVelocity });
        } else {
            aimingLine.positionCount = posns.Length;
            aimingLine.SetPositions(posns);
        }

    }

    private void UpdateLinePositions() {
        // Target line
        if (_pastLinePositions.Count > 0) {
            var targposs = new Vector3[] { _pastLinePositions[0].GetPosition(), TargetPlayer.GetPrefabInterface<TransformableInterface>().transformable.position + new Vector3(0, 1.5f, 0) };
            targetLine.SetPositions(targposs);
            phoneTargetLine.SetPositions(targposs);
        }

        // Aiming polyline

        var tempUsedCars = new HashSet<Collider>(_usedCars);
        _futureLinePositions.Clear();

        var inMagRay = new MagnitudeRay(transform.position, PerceivedVelocity);
        //var inMagRay = new MagnitudeRay(transform.position, _phys.velocity);
        MagnitudeRay outMagRay;
        Collider collider;

        var first = true;
        bool hit;
        bool isUsedCar;

        while (true) {
            hit = GetEasyBounceDirection(inMagRay, out outMagRay, out collider);
            if (hit) {
                isUsedCar = tempUsedCars.Contains(collider);
            } else {
                isUsedCar = false;
            }

            if (!hit || (hit && isUsedCar)) {
                if (first) {
                    _nextBounce = null;
                }
                break;
            }

            if (hit && !isUsedCar) {
                if (first) {
                    _nextBounce = outMagRay;
                }
                tempUsedCars.Add(collider);
                _futureLinePositions.Add(RelativeTransform.FromGlobal(collider.transform, outMagRay.Origin));
            }

            inMagRay = outMagRay;
            first = false;
        }

    }
 
    /// <summary>
    /// Direction need not be normalized
    /// </summary>
    /// <param name="direction">to raycast</param>
    private bool GetEasyBounceDirection(MagnitudeRay inMagRay, out MagnitudeRay outMagRay, out Collider collider) {
        outMagRay = new MagnitudeRay();
        collider = null;

       // var sc = SourcePlayer.GetPrefabInterface<VehicleInterface>().vehicle.GetPrefabInterface<BodyCollidableInterface>().bodyCollider.gameObject;

        // FIXME: should just set layer to this *when launching*, independently of the ball, because it's a human thing
        // OR even better spawn the ball outside our collider
        //sc.layer = LayerMask.NameToLayer("Ignore Raycast");
        RaycastHit hit;
        if (Physics.Raycast(inMagRay.ToRay(), out hit, 50, billiardLayerMask)) {
           // sc.layer = LayerMask.NameToLayer("BodyCollider");

            // Account for ball radius (ensures more realistic bounce trajectories)
            var hp = hit.point - inMagRay.Direction.normalized * 0.5f;

            if (hit.collider.gameObject.layer == floorLayer) {
                var n = new Vector3(0, 1, 0);
                outMagRay = new MagnitudeRay(hp, Vector3.Reflect(inMagRay.Direction, n));
                collider = hit.collider;
                return true;
            }

            var bpp = hit.collider.GetComponent<BodyCollidableBackpointer>();
            if (bpp == null) {
                throw new System.Exception("Shouldn't get here");
            }

            var otherVehicle = bpp.pointer;

            // Note: hardcoded for the purposes of the game
           // if (otherVehicle.GetComponentInChildren<CarAI>().VehicleDirection == CarAI.Direction.Backward) {
           //     return false;
           // }

            var shieldNormal = otherVehicle.GetSiblingPrefabInterface<ShieldInterface>().shield.Normal;

            Vector3 reflVel;
            if (shieldNormal == null) {
                reflVel = Vector3.Reflect(inMagRay.Direction, hit.normal);
            } else {
                // Purposely don't reflect here to make bouncing easier
                reflVel = (Vector3) shieldNormal * inMagRay.Direction.magnitude;
            }
            var dirmag = RoundMagRayToVehicle(otherVehicle, new MagnitudeRay(hp, reflVel)) * inMagRay.Direction.magnitude;
            outMagRay = new MagnitudeRay(hp, dirmag);
            collider = hit.collider;
            return true;
        }
        //sc.layer = LayerMask.NameToLayer("BodyCollider");
        return false;
    }

    private Vector3 RoundMagRayToVehicle(BodyCollidableInterface otherVehicle, MagnitudeRay magRay) {
        // Perform "easy" (for player) bounce angle calculation
        GameObject closestAngleVehicle = null;
        float mang = 180;
        foreach (var car in playerDB.PlayerNames) {
            if (car.Value.GetPrefabInterface<VehicleInterface>().vehicle.GetPrefabInterface<BodyCollidableInterface>() == otherVehicle) {
                continue;
            }
            if (car.Value.GetPrefabInterface<VehicleInterface>().vehicle.GetComponentInChildren<CarAI>().VehicleDirection == CarAI.Direction.Backward) {
                continue;
            }

            var ang = Vector3.Angle(magRay.Direction,
                car.Value.GetPrefabInterface<VehicleInterface>().vehicle.GetPrefabInterface<TransformableInterface>().transformable.position + new Vector3(0, 1.5f, 0)
                - magRay.Origin);

            if (ang < mang) {
                mang = ang;
                closestAngleVehicle = car.Value;
            }
        }

        return (closestAngleVehicle.GetPrefabInterface<TransformableInterface>().transformable.position + new Vector3(0, 1.5f, 0)
            - magRay.Origin).normalized;

    }

    private void GoodBye() {
        Destroy(gameObject);
    }

    void OnCollided(Collider me, Collider other, Vector3 normal) {

        // This collided event is physics generated, not raycast generated,
        // so we have to explicitly check that we are bouncing off only the
        // colliders that we raytrace
        if ((billiardLayerMask & (1 << other.gameObject.layer)) == 0) {
            return;
        }

        if (State != StateType.Airborne) {
            return;
        }

        if (_nextBounce == null) {
            return;
        }

        _phys.velocity = ((MagnitudeRay)_nextBounce).Direction + coordFrame;
        _pastLinePositions.Add(RelativeTransform.FromGlobal(other.transform, ((MagnitudeRay)_nextBounce).Origin));
        _usedCars.Add(other);
        _nextBounce = null;
        ++BounceCount;

        //CancelSlowMo();

    }
}