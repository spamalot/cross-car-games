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

public class ViveTouchGestureRecognizer : MonoBehaviour {

    public Human human;

    public enum GestureType { None, SwipeRight, SwipeLeft, SwipeUp, SwipeDown };
    public event Action<GestureType, Vector2> OnGestured;
    
    private bool ingesture = false;
    private Vector2 pressCoords;
    private Vector2 releaseCoords;

    private const float MIN_DIST = 0.5f;

    private GestureType GetGestureType() {
        var d = Vector2.Distance(pressCoords, releaseCoords);
        if (d < MIN_DIST) {
            return GestureType.None;
        }

        var a = Vector2.SignedAngle(new Vector2(1, 0), releaseCoords - pressCoords);
        if (Mathf.Abs(a) < 45) {
            return GestureType.SwipeRight;
        }
        if (Mathf.Abs(a - 90) < 45) {
            return GestureType.SwipeUp;
        }
        if (Mathf.Abs(a + 90) < 45) {
            return GestureType.SwipeDown;
        }
        if (Mathf.Abs(Math.Abs(a) - 180) < 45) {
            return GestureType.SwipeLeft;
        }

        return GestureType.None;
    }

    void Update() {
        if (human.RightTouching) {
            if (ingesture) {
                releaseCoords = human.RightTouchCoords;
            } else {
                ingesture = true;
                pressCoords = human.RightTouchCoords;
            }
        } else {
            if (ingesture) {
                ingesture = false;
                var t = GetGestureType();
                if (t != GestureType.None && OnGestured != null) {
                    OnGestured.Invoke(t, pressCoords);
                }
            }
        }
            
    }
}
