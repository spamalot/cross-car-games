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

using SpamalotExtensions;
using UnityEngine;

namespace BallGame
{
    public class BallHumanCollidable : MonoBehaviour
    {

        private enum CollisionLocation { LeftHand, RightHand, Elsewhere };

        void Start() {
            var myCar = this.GetPrefabInterface<VehicleInterface>().vehicle;
            // Since hand stops ball, need to intercept the ball bounce
            myCar.GetPrefabInterface<HandInterface>().hand.GetComponent<DeadSimplePhysics>().OnCollided += (c1, c2, n) => HandleCollision(CollisionLocation.RightHand, c1, c2);
            myCar.GetPrefabInterface<BodyCollidableInterface>().bodyCollider.GetComponent<DeadSimplePhysics>().OnCollided += (c1, c2, n) => HandleCollision(CollisionLocation.Elsewhere, c1, c2);
        }

        private void HandleCollision(CollisionLocation loc, Collider me, Collider other) {
            var ball = other.GetComponent<Ball>();
            var throwerCatcher = GetComponent<BallHumanThrowerCatcher>();

            if (ball == null || ball.State == Ball.BallState.Held) {
                return;
            }

            if (loc == CollisionLocation.RightHand && throwerCatcher.IsThrownTo) {
                throwerCatcher.CollidedWithBall(other.gameObject);
            } else {
                throwerCatcher.HitByBall(other.gameObject);
            }
        }

    }
}