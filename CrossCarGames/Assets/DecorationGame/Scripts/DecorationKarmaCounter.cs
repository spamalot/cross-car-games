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
using UnityEngine;

public class DecorationKarmaCounter : MonoBehaviour {

    public event Action OnKarmaZero;

    public float Karma { get; private set; } = 1;

    public void ResetKarma() {
        Karma = 1;
    }

    void Update () {
        Karma -= 0.0005f;

        if (Karma < 0) {
            OnKarmaZero?.Invoke();
            ResetKarma();
        }
    }
}
