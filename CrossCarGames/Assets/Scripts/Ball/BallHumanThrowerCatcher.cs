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
using UnityEngine;
using SpamalotExtensions;

namespace BallGame
{
    // FIXME: have reference to human so that we can access hand and vehicle interfaces
    public class BallHumanThrowerCatcher : BallThrowerCatcher
    {
        // TODO: make into a property
        public PlayerDB playerDB;
        private GameObject myCar;
        private GameObject myHand;

        private List<Vector3> vellog = new List<Vector3>();
        private List<float> avgmags = new List<float>();

        private enum ThrowState { Holding, Throwing, Thrown, Caught }
        private ThrowState throwState = ThrowState.Thrown;

        void Start()
        {
            playerDB = GameObject.Find("PlayerDB").GetComponent<PlayerDB>();
            myCar = this.GetPrefabInterface<VehicleInterface>().vehicle;
            myHand = myCar.GetPrefabInterface<HandInterface>().hand;
        }

        private void HoldBall()
        {
            // Called by invoke
            if (throwState != ThrowState.Caught) {
                // Since we happen after a delay, our state may have been lost if the ball was passed.
                return;
            }
            throwState = ThrowState.Holding;
            vellog.Clear();
            avgmags.Clear();
        }

        public void MakeCaught(GameObject newBall)
        {
            //GetComponent<BallHumanCollidable>().DisableCollisions(newBall);
            newBall.GetComponent<Ball>().MakeLanded();
            throwState = ThrowState.Caught;
            // Need a a delay here or else the same swing used to catch does the subsequent throw.
            Invoke(nameof(HoldBall), 0.5f);
        }

        // FIXME: this entire function is hacked into place as an experiment; should it work, code cleanup will be necessary
        //protected void UpdateBAK()
        protected override void Update()
        {
            base.Update();

            var state = playerDB.MyPlayer;

            if (state == null) {
                return;
            }

            myHand.GetComponent<HandAppearance>().MeshState =
                (state.SmoothedRightHandState == Human.HandState.Closed) ? HandAppearance.HandMeshState.Closed : HandAppearance.HandMeshState.Open;

            if (throwState == ThrowState.Thrown) {
                return;
            }

            HeldBall.GetComponent<DeadSimplePhysics>().position = myHand.transform.position;
            HeldBall.GetComponent<DeadSimplePhysics>().velocity = Vector3.zero;

            if (throwState == ThrowState.Caught) {
                return;
            }

            vellog.Add(state.RightHandVelocity);
            if (vellog.Count > 5) {
                vellog.RemoveAt(0);
            }

            var avgv = new Vector3(0, 0, 0);
            for (int i = 0; i < vellog.Count; i++) {
                avgv += vellog[i];
            }
            avgv /= vellog.Count;


            var avga = new Vector3(0, 0, 0);
            for (int i = 0; i < vellog.Count - 1; i++) {
                avga += vellog[i + 1] - vellog[i];
            }
            avga /= vellog.Count - 1;


            if (avga.magnitude > 0.15 && state.SmoothedRightHandState == Human.HandState.Open) {
                throwState = ThrowState.Throwing;
            }

            if (throwState == ThrowState.Throwing && avga.magnitude < 0.001) {
                throwState = ThrowState.Holding;
            }

            if (throwState == ThrowState.Throwing && avga.magnitude > 0.1 && Vector3.Project(avga, avgv).normalized == -avgv.normalized) {
                Vector3 vell = avgv;

                HeldBall.GetComponent<Ball>().MakeLanded();
                HeldBall.GetComponent<DeadSimplePhysics>().velocity = GameObject.Find("[CameraRig]").transform.rotation * new Vector3(vell.x * 16, vell.y * 4, vell.z * 16) + myCar.GetComponentInChildren<CarAI>().Velocity;
                //Release();
                Throw();
                throwState = ThrowState.Thrown;
                vellog.Clear();
                avgmags.Clear();
            }
        }

        public override void Release()
        {
            base.Release();
            throwState = ThrowState.Thrown;
            vellog.Clear();
            avgmags.Clear();
            //ClearBall();
        }

        public void CollidedWithBall(GameObject ball)
        {
            var state = playerDB.MyPlayer;

            // Ignore collision if hand "closed"
            if (IsHoldingBall || state == null || state.SmoothedRightHandState == Human.HandState.Closed || ball.GetComponent<Ball>().LastHeld == this) {
                return;
            }
            
            MakeCaught(ball);
            Catch(ball);
        }

        public void HitByBall(GameObject ball)
        {
            Hit(ball);
        }

    }
}
