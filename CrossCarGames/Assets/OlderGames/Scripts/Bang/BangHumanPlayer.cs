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

namespace Bang
{
    public class BangHumanPlayer : BangPlayer
    {
        // TODO make into property
        public PlayerDB playerDB;

        void Start() {
            playerDB = GameObject.Find("PlayerDB").GetComponent<PlayerDB>();
        }

        void Update()
        {
            var state = playerDB.MyPlayer;

            if (state == null) {
                return;
            }

            if (!state.IsPointing) {
                PointingDirection = Vector3.zero;
            } else {
                PointingDirection = state.GlobalRightHandRotation * new Vector3(0, 0, 1);
            }

            Ducking = state.IsDucking;

            if (State == PlayerState.It || State == PlayerState.ItWaiting || State == PlayerState.PointWaiting) {
                GameObject.Find("MyHand").GetComponent<HandAppearance>().MeshState =
                    (PointingDirection == Vector3.zero) ? HandAppearance.HandMeshState.Open : HandAppearance.HandMeshState.Pointing;
            } else { 
                GameObject.Find("MyHand").GetComponent<HandAppearance>().MeshState =
                    HandAppearance.HandMeshState.Open;
            }
        }
    }
}
