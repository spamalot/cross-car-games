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

public class RiseFromPosition : MonoBehaviour {

    // FIXME: rename to risefromposition

    public float yOffset = -1;

    private int _counter;
    private Vector3? _from;

    public Vector3? From {
        get { return _from; }
        set {
            // When set, start the animation
            _from = value;
            _counter = 0;
        }
    }

    void Start() {
        GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0f);
    }

    void Update () {
        if (From == null) {
            return;
        }

        // Rise up and fade out
        transform.position = (From ?? Vector3.zero) + new Vector3(0, yOffset + _counter / 30f, 0);
        GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1 - _counter / 100f);

        if (_counter++ == 100) {
            From = null;
        }
    }
}
