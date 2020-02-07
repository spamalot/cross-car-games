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
using Valve.VR;

public class ViveRightTouchTracker : AbstractRightTouchTracker {

    public override bool IsTracked(Human human) {
        return human.rightHandTracker != null
            && human.rightHandTracker.activeInHierarchy
            && SteamVR_Controller.Input(
                (int)human.rightHandTracker.GetComponent<SteamVR_TrackedObject>().index).hasTracking;
    }

    public override bool GetRightTouching(Human human) {
        var input = SteamVR_Controller.Input((int)human.rightHandTracker.GetComponent<SteamVR_TrackedObject>().index);
        //If finger is on touchpad
        return input.GetTouch(SteamVR_Controller.ButtonMask.Touchpad);
    }

    public override Vector2 GetRightTouchCoords(Human human) {
        var input = SteamVR_Controller.Input((int)human.rightHandTracker.GetComponent<SteamVR_TrackedObject>().index);
        return input.GetAxis(EVRButtonId.k_EButton_SteamVR_Touchpad);
    }

}


