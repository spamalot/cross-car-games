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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bang
{
    public class BangPlayer : MonoBehaviour
    {

        public enum PlayerState {
            None, It, ItWaiting, ToDuck, DuckWaiting, DuckLate,
            ToPoint, PointWaiting };

        private Vector3 _pointingDirection = Vector3.zero;
        private bool _ducking;

        public PlayerState State { get; set; }

        public Vector3 PointingDirection {
            get { return _pointingDirection; }
            protected set {
                if (_pointingDirection == Vector3.zero && value != Vector3.zero) {
                    OnPoint.Invoke(this);
                }
                _pointingDirection = value;
            }
        }

        public bool Ducking {
            get { return _ducking; }
            protected set {
                if (!_ducking && value) {
                    OnDuck.Invoke(this);
                }
                _ducking = value;
            }
        }

        public event Action<BangPlayer> OnPoint;
        public event Action<BangPlayer> OnDuck;

        private const float CLOSE = 12f;

        /// <summary>
        /// Return `count` random "close" players to `toPlayer`. Close means
        /// within a certain radius if possible, but outside of the radius
        /// if not possible.
        /// </summary>
        /// <returns>List of `count` closest players.</returns>
        public static List<GameObject> FindClosePlayers(PlayerDB players, GameObject toPlayer, int count, List<GameObject> exclude)
        {
            exclude.Add(toPlayer);
            var toPlayerPos = toPlayer.transform.position;
            var ordered = players.PlayerNames.Values
                .Where(x => !exclude.Contains(x))
                .OrderBy(x => Vector3.Distance(toPlayerPos, x.transform.position))
                .Select(x =>
                    new {
                        Close = Vector3.Distance(toPlayerPos, x.transform.position) < CLOSE,
                        Player = x,
                    }
                );
            var closep = ordered.Where(x => x.Close).ToList();
            var farp = ordered.Where(x => !x.Close).ToList();

            var close = new List<GameObject>();

            // NOTE: will hang if fewer than `count - exclude.Count` players are playing.
            while (close.Count < count) {
                if (closep.Count > 0) {
                    var idx = (int) (UnityEngine.Random.value * 0.999f * closep.Count);
                    close.Add(closep[idx].Player);
                    closep.RemoveAt(idx);
                } else {
                    close.Add(farp[0].Player);
                    farp.RemoveAt(0);
                }
            }

            return close;
        }

    }
}