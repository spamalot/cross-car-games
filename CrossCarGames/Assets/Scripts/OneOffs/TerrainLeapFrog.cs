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

public class TerrainLeapFrog : MonoBehaviour {

    private bool _wasShowingSkyscrapers = true;
    public bool showSkyscrapers = true;
    public Transform myCarTransform;

    private List<GameObject> _skyscrapers;

    void Start() {
        _skyscrapers = new List<GameObject>(GameObject.FindGameObjectsWithTag("Skyscraper"));
    }

    // Update is called once per frame
    void Update () {
        transform.position = new Vector3(transform.position.x, transform.position.y, ((int) myCarTransform.position.z / 160) * 160);

        if (showSkyscrapers != _wasShowingSkyscrapers) {
            _skyscrapers.ForEach(x => x.SetActive(showSkyscrapers));
            _wasShowingSkyscrapers = showSkyscrapers;
        }
    }
}
