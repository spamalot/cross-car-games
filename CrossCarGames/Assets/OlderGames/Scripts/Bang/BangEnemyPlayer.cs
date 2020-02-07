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

namespace Bang
{
    public class BangEnemyPlayer : BangPlayer
    {
        public GameObject Ducker { get; set; }
        public bool DuckerDucking { get; set; }
        public bool ItReady { get; set; }

        //private PlayerState _lastState;
        private Vector3 _towardsPointingDirection;

        void Start()
        {
            PointingDirection = Vector3.zero;
           // _lastState = PlayerState.None;
        }

        void Update()
        {
            switch (State) {
                case PlayerState.It:
                    if (!ItReady) {
                        // Tell controller that we know we're It.
                        _towardsPointingDirection = Vector3.zero;
                    }
                    if (Random.value < 0.01) {
                        // Choose a random nearby player and point to it.
                        var playerDB = GameObject.Find("PlayerDB").GetComponent<PlayerDB>();
                        var pointTo = FindClosePlayers(playerDB, gameObject, 1, new List<GameObject> {})[0];
                        _towardsPointingDirection = (pointTo.transform.position - transform.position).normalized;
                    }
                    break;
                case PlayerState.ItWaiting:
                    _towardsPointingDirection = Vector3.zero;
                    break;
                case PlayerState.ToDuck:
                    if (Random.value < 0.003) {
                        Ducking = true;
                    }
                    break;
                case PlayerState.ToPoint:
                    //if ((!DuckerDucking && Random.value < 0.003) || (DuckerDucking && Random.value < 0.001)) {
                    if (Random.value < 0.004) { 
                        _towardsPointingDirection = (Ducker.transform.position - transform.position).normalized;
                    }
                    break;
                default:
                    _towardsPointingDirection = Vector3.zero;
                    Ducking = false;
                    break;
            }

            // Make hand appearance reasonable.
            if (Ducking) {
                // FIXME:  improve
                transform.Find("Transformable/Hand(Clone)").GetComponent<HandAppearance>().MeshState = HandAppearance.HandMeshState.Closed;
            } else {
                if (_towardsPointingDirection != Vector3.zero) {
                    transform.Find("Transformable/Hand(Clone)").GetComponent<HandAppearance>().MeshState = HandAppearance.HandMeshState.Pointing;
                } else {
                    transform.Find("Transformable/Hand(Clone)").GetComponent<HandAppearance>().MeshState = HandAppearance.HandMeshState.Open;
                }
            }

            var rotateTo = (_towardsPointingDirection == Vector3.zero) ?
                Quaternion.identity :
                (Quaternion.LookRotation(_towardsPointingDirection) * Quaternion.Euler(90, 0, 0));

            transform.Find("Transformable/Hand(Clone)").transform.rotation = Quaternion.RotateTowards(
                transform.Find("Transformable/Hand(Clone)").transform.rotation, rotateTo, 2.1f);

            if (Quaternion.Dot(transform.Find("Transformable/Hand(Clone)").transform.rotation, rotateTo) > 0.99) {
                PointingDirection = _towardsPointingDirection;
            }

            //_lastState = State;

        }

    }
}