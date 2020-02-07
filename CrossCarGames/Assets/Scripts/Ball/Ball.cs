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

using System;
using UnityEngine;

namespace BallGame
{
    public class Ball : MonoBehaviour
    {

        [SpamalotExtensions.Layer]
        public int defaultLayer;
        [SpamalotExtensions.Layer]
        public int heldLayer;

        public event Action OnDropped;

        public enum BallState { Normal, Dead, Held };

        public BallThrowerCatcher LastHeld { get; set; }
        public BallState State { get; set; } = BallState.Normal;

        private DeadSimplePhysics phys;
        private MeshRenderer meshRenderer;

        void Start()
        {
            phys = GetComponent<DeadSimplePhysics>();
            meshRenderer = GetComponent<MeshRenderer>();
        }

        void Update()
        {
            if (transform.position.y < -6) {
                transform.position = Vector3.zero;
                OnDropped.Invoke();
            }

            // Hide the ball if it's hit a target.
            meshRenderer.enabled = (State != BallState.Dead);

            // Prevents enemies' hands from rapidly vibrating. Without, Unity
            // gets stuck in loop of fixing the ball to your hand position but
            // re-colliding with enemy hand and rebounding.
            gameObject.layer = (State == BallState.Held) ? heldLayer : defaultLayer;
        }

        public void MakeLanded()
        {
            phys.velocity = Vector3.zero;
        }

    }
}