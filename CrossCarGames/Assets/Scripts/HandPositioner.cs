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
using SpamalotExtensions;

public class HandPositioner : MonoBehaviour {

    // FIXME: rename to HumanHandPositioner or something else b/c for Shield too

    public event System.Action OnRotatedUp;

    public Human human;

    private Vector3 _AvatarHandPosition {
        get {

            var rot = human.GlobalRightHandRotation;

            var rotStuff = rot * new Vector3(0, 0, 1) * 2.5f;
            rotStuff.y = 0;
            var rotStuff2 = (human.GlobalRightHandPosition - GameObject.Find("Camera (eye)").transform.position + new Vector3(0, 1, 0)) * 2.5f;

            var center = human.GetPrefabInterface<TransformableInterface>().transformable.position;
            var rotTot = center + rotStuff2;
            // FIXME: move hand to inside car; use local coords
            rotTot.x = Mathf.Clamp(rotTot.x, center.x - 0.1f, center.x + 0.1f);
            rotTot.y = Mathf.Clamp(rotTot.y, center.y + 0.1f, center.y + 3);
            rotTot.z = Mathf.Clamp(rotTot.z, center.z - 2, center.z + 1);

            return rotTot + rotStuff;
            
        }
    }

    private int count1 = 0;

    private void Update() {
        // Angle from vertical upward
        var ang = Vector3.Angle(human.GlobalRightHandRotation * Vector3.up, Vector3.down);

        if (ang < 20) {
            ++count1;
        } else {
            count1 = 0;
        }

        if (count1 > 30) {
            OnRotatedUp?.Invoke();
            count1 = 0;
        }

    }

    void LateUpdate() {
        if (!human.IsFullyTracked()) {
            return;
        }
        
        transform.position = _AvatarHandPosition;
        GetComponent<Rigidbody>().MoveRotation(human.GlobalRightHandRotation * Quaternion.Euler(90,0,0));
    }
}
