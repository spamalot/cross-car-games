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

/// <summary>
/// Assumes left controller mounted on back of player
/// </summary>
public class ViveDuckingTracker : AbstractDuckingTracker {
    
    public override bool IsTracked(Human human) {
        return human.leftHandTracker != null
            && human.leftHandTracker.activeInHierarchy
            && SteamVR_Controller.Input(
                (int)human.leftHandTracker.GetComponent<SteamVR_TrackedObject>().index).hasTracking;
    }

    public override bool? GetHumanDucking(Human human) {
        var controllerBack = SteamVR_Controller.Input(
            (int)human.leftHandTracker.GetComponent<SteamVR_TrackedObject>().index);

        var val = controllerBack.transform.rot.eulerAngles.x;
        if (val > 310) {
            return true;
        }
        if (val < 290) {
            return false;
        }
        return null;
    }

}
