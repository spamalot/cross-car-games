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

public class PlayerDB : MonoBehaviour
{

    // NOTE: Should tracking of playing *cars* be necessary, store them here as
    // an additional list, with a new "ContainedPlayersInterface" containing
    // a List<GameObject> of its contained players.

    [SerializeField]
    private Human _myPlayer;

    public Dictionary<string, GameObject> PlayerNames { get; set; } = new Dictionary<string, GameObject>();
    public Human MyPlayer { get; private set; }

    void Update() {
        MyPlayer = _myPlayer.IsFullyTracked() ? _myPlayer : null;
    }

}
