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
using SpamalotExtensions;

namespace BallGame
{
    public abstract class BallThrowerCatcher : MonoBehaviour
    {
        protected GameObject HeldBall { get; private set; }

        public BallThrowerCatcher ThrowTo { get; set; }
        public bool IsThrownTo { get; set; }
        public event Action<BallThrowerCatcher, GameObject> OnCaught;
        public event Action<BallThrowerCatcher, GameObject> OnThrown;
        public event Action<BallThrowerCatcher, GameObject> OnHit;
        public event Action OnSafeToThrow;
        public bool IsThrown { get; set; } = true;
        public float MinBallDistance { get; set; }

        public bool IsHoldingBall {
            get { return HeldBall != null; }
        }

        public void Catch(GameObject ball)
        {
            HeldBall = ball;
            IsThrown = false;
            OnCaught?.Invoke(this, ball);
        }

        public void Throw()
        {
            IsThrown = true;
            OnThrown?.Invoke(this, HeldBall);
            HeldBall = null;
        }

        public void Hit(GameObject ball)
        {
            OnHit?.Invoke(this, ball);
        }

        public void SafeToThrow()
        {
            OnSafeToThrow?.Invoke();
        }

        public virtual void Release()
        {
            IsThrown = true;
            HeldBall = null;
        }

        protected virtual void Update()
        {
            if (!IsThrownTo) {
                return;
            }
            foreach (var flyingBall in GameObject.Find("Balls").GetComponentsInChildren<Ball>()) {
                MinBallDistance = Mathf.Min(MinBallDistance, Vector3.Distance(this.GetPrefabInterface<VehicleInterface>().vehicle.GetPrefabInterface<HandInterface>().hand.transform.position, flyingBall.transform.position));
            }
        }

    }
}