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
using SpamalotExtensions;

namespace BallGame
{
    public class EnemyThrowerCatcher : BallThrowerCatcher
    {
        public GameObject hand;
        public GameObject bodyCollider;

        private DeadSimplePhysics[] collideObjs;
        private Transform _carTransformable;

        void Start()
        {
            var car = this.GetPrefabInterface<VehicleInterface>().vehicle;
            _carTransformable = car.GetComponent<TransformableInterface>().transformable;
            collideObjs = car.GetComponentsInChildren<DeadSimplePhysics>();
            foreach (var obj in collideObjs) {
                obj.OnCollided += CollisionEntered;
            }
        }

        private void CollisionEntered(Collider me, Collider other, Vector3 normal)
        {
            var ball = other.GetComponent<Ball>();
            if (ball == null || ball.State == Ball.BallState.Held) {
                return;
            }

            if (me.gameObject == bodyCollider) {
                Hit(other.gameObject);
                return;
            }

            if (IsHoldingBall || !IsThrownTo) {
                return;
            }

            Catch(other.gameObject);
            //ClearBall();
        }

        public float SignedRand {
            get { return (Random.value - 0.5f) * 2; }
        }

        public void ThrowAtTarget(Vector3 targetPos, Vector3 ourPos, float launchAngle)
        {
            var distxz = Vector3.Distance(
                new Vector3(targetPos.x, 0, targetPos.z),
                new Vector3(ourPos.x, 0, ourPos.z));
            var disty = ourPos.y - targetPos.y;

            // FIXME: use ThrowTo.GetComponent<PathFollower>().Velocity? (not easy)

            var mag = Mathf.Sqrt((-Physics.gravity.y * Mathf.Pow(distxz, 2))
                / (distxz * Mathf.Sin(2 * launchAngle) + 2 * disty * Mathf.Pow(Mathf.Cos(launchAngle), 2)));

            // FIXME: when is there no solution?
            if (float.IsNaN(mag)) {
                mag = 0;
            }
            
            //HeldBall.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            HeldBall.GetComponent<DeadSimplePhysics>().velocity =
                this.GetPrefabInterface<VehicleInterface>().vehicle.GetComponentInChildren<CarAI>().Velocity
                + (new Vector3(targetPos.x - ourPos.x, distxz * Mathf.Tan(launchAngle), targetPos.z - ourPos.z)).normalized * mag;
        }

        private Ball NearestBall()
        {
            Ball minBall = null;
            float minDist = float.PositiveInfinity;

            foreach (var ball in GameObject.Find("Balls").GetComponentsInChildren<Ball>()) {
                var dist = Vector3.Distance(_carTransformable.position, ball.transform.position);
                if (dist < minDist && ball.LastHeld != this && ball.State != Ball.BallState.Dead) {
                    minBall = ball;
                    minDist = dist;
                }
            }

            return minBall;
        }

        protected override void Update()
        {
            base.Update();
            if (IsThrownTo) {
                var b = NearestBall();
                if (b == null) {
                    // No balls worth pursuing.
                    return;
                }

                var to = b.transform.position;
                to.x = Mathf.Clamp(to.x, _carTransformable.position.x - 2, _carTransformable.position.x + 2);
                to.y = Mathf.Clamp(to.y, _carTransformable.position.y + 1, _carTransformable.position.y + 4);
                to.z = Mathf.Clamp(to.z, _carTransformable.position.z - 4, _carTransformable.position.z + 4);

                hand.transform.position =
                    Vector3.MoveTowards(hand.transform.position, to, 0.11f);
                hand.transform.rotation =
                    Quaternion.RotateTowards(hand.transform.rotation, Quaternion.LookRotation(b.transform.position - _carTransformable.position), 30f);
            } else {
                hand.transform.position =
                     Vector3.MoveTowards(hand.transform.position, _carTransformable.position, 0.5f);
            }
        }

    }
}