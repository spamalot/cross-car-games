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

public class PointingDebug : MonoBehaviour {

    void Update() {
        // Depict Pointing Direction

        var state = GameObject.Find("PlayerDB").GetComponent<PlayerDB>().MyPlayer;

        if (state == null) {
            return;
        }

        var x = state.GlobalRightHandRotation;
        //gameObject.GetComponent<Renderer>().enabled = true;// state.IsPointing;
        transform.position = GameObject.Find("MyHand").transform.position + x * new Vector3(0, 0, 1);
        transform.rotation = x;
        ((Behaviour) GetComponent("Halo")).enabled = state.IsPointing;
        transform.localScale = state.IsDucking ? new Vector3(2, 2, 2) : new Vector3(1, 1, 1);
    }
}
