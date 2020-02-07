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
using System.Linq;
using UnityEngine;

public class WheelRotator : MonoBehaviour {

    private Vector3 lastPos;
    private List<Vector3> velocities;

    void Start () {
        lastPos = transform.position;
        velocities = new List<Vector3>();
    }
    
    void Update () {
        velocities.Add((transform.position - lastPos) / Time.deltaTime);
        if(velocities.Count > 40) {
            velocities.RemoveAt(0);
        }

        var avgv = velocities.Aggregate((a, b) => a + b) / velocities.Count;
        var speed = avgv.magnitude;
        if (speed > 0) {
            transform.localRotation *= Quaternion.Euler(new Vector3(0, 0, speed));
        }

        lastPos = transform.position;
    }
}
