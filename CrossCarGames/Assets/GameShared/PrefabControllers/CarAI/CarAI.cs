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

using UnityEngine;

public class CarAI : MonoBehaviour {

    public enum Direction { Forward, Backward };

    public CarAIInitializer initializer;
    public Transform car;
    public Transform submesh;
    public Collider leftCollider;
    public Collider rightCollider;
    public LayerMask involvedLayers;

    public Vector3 Velocity {
        get {
            return Spd2incr(mySpeed) * forward /  Time.deltaTime; //car.rotation * new Vector3(Spd2incr(mySpeed) / Time.deltaTime, 0, 0);
        }
    }

    public LaneSide TakeoverSafeLane {
        get {
            if (laneChangeState != LaneChangeState.None) {
                return LaneSide.None;
            }
            var available = new System.Collections.Generic.List<LaneSide>();
            foreach (var side in new LaneSide [] { LaneSide.Left, LaneSide.Right }) {
                laneToMove = side;
                if (CanChangeLanes()) {
                    available.Add(laneToMove);
                }
            }
            if (available.Count == 0) {
                return LaneSide.None;
            }
            return available[Random.Range(0, available.Count)];
        }
    }

    public Direction VehicleDirection { get; set; }

    private const float SPEED_CHANGE_PROBABILITY = 0.004f;
    public const float DEFAULT_LANE_CHANGE_PROBABILITY = 0.002f;
    public float laneChangeProbability = DEFAULT_LANE_CHANGE_PROBABILITY;
    
    public enum LaneSide { None, Right, Left };
    private enum LaneChangeState { None, Begin, Active, End };

    private LaneSide laneToMove;
    private LaneChangeState laneChangeState = LaneChangeState.None;
    private float t;
    private int mySpeed;
    private Vector3 forward;

    private Vector3 AdjacentPosition {
        get {
            return car.position + car.rotation * new Vector3(0, 0, GetLaneOffset(laneToMove));
        }
    }

    private float GetLaneOffset(LaneSide side) {
        switch (side) {
            case LaneSide.Right: return -5;
            case LaneSide.Left: return 5;
            default: throw new System.ArgumentException();
        }
    }

    private Collider GetCollider(LaneSide side) {
        switch (side) {
            case LaneSide.Right: return rightCollider;
            case LaneSide.Left: return leftCollider;
            default: throw new System.ArgumentException();
        }
    }

    float Spd2incr(int spd) {
        // "kph" to z offset in world
        return spd / 400f;
    }

    void Start () {
        Respeed();
    }
    
    private void Respeed() {
        if (Random.value < SPEED_CHANGE_PROBABILITY) {
            mySpeed = (int)((Random.value - 0.5f) * initializer.Config.nominalSpd * 1.8f) + initializer.Config.nominalSpd;
        } else {
            mySpeed = initializer.Config.nominalSpd;
        }
    }

    private bool LaneChangeStateNone() {
        // If small probability, switch to lane change mode
        if (Random.value < laneChangeProbability) {
            laneToMove = (Random.value < 0.5) ? LaneSide.Right : LaneSide.Left;
            if (CanChangeLanes()) {
                laneChangeState = LaneChangeState.Begin;
            }
        }
        return true;
    }

    private bool CanChangeLanes() {
        Collider[] y2 = Physics.OverlapBox(AdjacentPosition, new Vector3(5f, 2.5f, 5f) / 2, Quaternion.identity, involvedLayers);
        return y2.Length == 0;
    }

    private bool LaneChangeStateBegin() {
        // If planning to change lanes, check that space is free, and then block off that space for us
        t = 0;
        GetCollider(laneToMove).enabled = true;
        laneChangeState = LaneChangeState.Active;
        return true;
    }

    private bool LaneChangeStateActive() {
        t += initializer.Config.laneChangeStep;
        submesh.localPosition = Vector3.Lerp(Vector3.zero, new Vector3(0, 0, GetLaneOffset(laneToMove)), t);
        submesh.localRotation = Quaternion.Euler(new Vector3(0, -Mathf.Sign(GetLaneOffset(laneToMove)) * initializer.Config.STEER_ANGLE_DEGREES * Mathf.Sin(t * Mathf.PI), 0));
        if (t >= 1) {
            laneChangeState = LaneChangeState.End;
        }
        return true;
    }

    private bool LaneChangeStateEnd() {
        submesh.localRotation = Quaternion.identity;
        submesh.localPosition = Vector3.zero;
        car.position = AdjacentPosition;
        GetCollider(laneToMove).enabled = false;
        laneChangeState = LaneChangeState.None;
        return true;
    }

    void Update () {
        forward = car.rotation * Vector3.right;
        //Debug.DrawRay(submesh.position + new Vector3(0, 1, 0), forward * initializer.Config.closeDistance);

        Ray ray = new Ray(submesh.position + new Vector3(0, 1, 0), forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 300, involvedLayers)) {
            float dist = hit.distance;            
            if (dist < initializer.Config.closeDistance) {
                // Stop moving if we get too close
                var factor = mySpeed > 100 ? 1.5f : 1.05f;
                mySpeed = (int) (mySpeed / factor);
            } else {
                // Speed up if we're lagging behind
                mySpeed = (int) Mathf.Min(initializer.Config.nominalSpd * 3f, mySpeed + (int)hit.distance / 10);
            }
        } else {
            // We're leading the pack; fix our speed
           Respeed();
        }

        

        bool move = false;
        switch(laneChangeState) {
            case LaneChangeState.None: move = LaneChangeStateNone(); break;
            case LaneChangeState.Begin: move = LaneChangeStateBegin(); break;
            case LaneChangeState.Active: move = LaneChangeStateActive(); break;
            case LaneChangeState.End: move = LaneChangeStateEnd(); break;
        }

        // Set the driving speed
        if (move) {
            car.position += Spd2incr(mySpeed) * forward;
        }

        // Warping code; shouldn't be here, but whatever
        var myCarTransform = GameObject.Find("MyCar").transform;
        if (VehicleDirection == Direction.Backward && car.position.z < myCarTransform.position.z - 40) {
            car.position = new Vector3(car.position.x, car.position.y, myCarTransform.position.z + 150) ;
            Respeed();
        }
    }
}
