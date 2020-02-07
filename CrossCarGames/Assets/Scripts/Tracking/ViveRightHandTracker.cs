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

public class ViveRightHandTracker : AbstractRightHandTracker {

    public override bool IsTracked(Human human) {
        //Debug.Log("test " + human.rightHandTracker.activeInHierarchy + " " + SteamVR_Controller.Input(
        //        (int)human.rightHandTracker.GetComponent<SteamVR_TrackedObject>().index).hasTracking);
        return human.rightHandTracker != null
            && human.rightHandTracker.activeInHierarchy
            && SteamVR_Controller.Input(
                (int)human.rightHandTracker.GetComponent<SteamVR_TrackedObject>().index).hasTracking;
    }

    public override Human.HandState GetHumanRightHandState(Human human) {
        // FIXME note: "smoothed"
        var controller = SteamVR_Controller.Input(
            (int)human.rightHandTracker.GetComponent<SteamVR_TrackedObject>().index);
        var gripped = controller.GetPress(SteamVR_Controller.ButtonMask.Grip);
        return gripped ? Human.HandState.Closed : Human.HandState.Open;
    }

    public override Vector3 GetHumanRightHandVelocity(Human human) {
        var controller = SteamVR_Controller.Input(
            (int)human.rightHandTracker.GetComponent<SteamVR_TrackedObject>().index);
        return controller.velocity;
    }

    public override bool? GetHumanPointing(Human human) {
        var controllerHand = SteamVR_Controller.Input(
            (int)human.rightHandTracker.GetComponent<SteamVR_TrackedObject>().index);
        var controllerBack = SteamVR_Controller.Input(
            (int)human.rightHandTracker.GetComponent<SteamVR_TrackedObject>().index);

        var val = Vector3.Distance(controllerHand.transform.pos, controllerBack.transform.pos);
        if (val > 0.7) {
            return true;
        }
        if (val < 0.6) {
            return false;
        }
        return null;
    }

    public override Vector3 GetHumanGlobalRightHandPosition(Human human) {
        return human.rightHandTracker.transform.position;
    }

    public override Quaternion GetHumanGlobalRightHandRotation(Human human) {
        return human.rightHandTracker.transform.rotation;
    }

    public override void VibrateRightHand(Human human) {
        SteamVR_Controller.Input(
            (int)human.rightHandTracker.GetComponent<SteamVR_TrackedObject>().index).TriggerHapticPulse(20000);
    }

}
