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

public class DecorationLeveller : MonoBehaviour {

    public event Action<GameObject> OnNewDecoration;

    [Serializable]
    public struct DecorationLevel {
        [Range(0, 100)]
        public int minScore;
        public GameObject decoration;
    }

    public List<DecorationLevel> lockedDecorationLevels = new List<DecorationLevel>();

    private Scoreable scoreable;
    private CarDecorator decorator;
    private int _currentScore = -1;

    void Start () {
        scoreable = GetComponent<Scoreable>();
        decorator = GetComponent<CarDecorator>();
    }
    
    void Update () {
        GameObject newDecal = null;
        var newScore = scoreable.Score;
        foreach (var level in lockedDecorationLevels.ToList()) {
            if (newScore >= level.minScore && _currentScore < level.minScore) {
                decorator.unlockedDecorations.Add(level.decoration);
                // prevent down levelling and up levelling from doubling up decorations
                lockedDecorationLevels.Remove(level);
                newDecal = level.decoration;
            }
        }
        if (newDecal != null) {
            OnNewDecoration?.Invoke(newDecal);
        }
        _currentScore = newScore;
    }
}
