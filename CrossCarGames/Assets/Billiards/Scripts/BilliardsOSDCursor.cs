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

public class BilliardsOSDCursor : MonoBehaviour {

    public Human human;

    public enum Position { Left, Right, BackLeft, BackRight };
    public Position arrowPosition;

    private int off = 0;


    public static float DistanceToLine(Ray ray, Vector3 point) {
        return Vector3.Cross(ray.direction, point - ray.origin).magnitude;
    }

    void Update() {

        var bouncer = human.GetComponent<BilliardBouncer>();

        var active = bouncer.IsLaunching;

        GetComponent<SpriteRenderer>().enabled = active;

        if (!active) {
            off = 0;
            return;
        }

        var osdi = human.GetPrefabInterface<VehicleInterface>().vehicle.GetPrefabInterface<MyOSDInterface>();

        var targp = bouncer.CurrentLaunchTargetPlayer.GetPrefabInterface<TransformableInterface>().transformable.position;

        var fd = Vector3.Distance(targp, osdi.front.canvas.transform.position);
        var ld = Vector3.Distance(targp, osdi.left.canvas.transform.position);
        var rd = Vector3.Distance(targp, osdi.right.canvas.transform.position);
        var bd = Vector3.Distance(targp, osdi.back.canvas.transform.position);

        MyOSDInterface.OSD iff = osdi.left;
        if (fd < ld && fd < rd && fd < bd) {
            switch (arrowPosition) {
                case Position.Left: iff = osdi.left;  break;
                case Position.Right: iff = osdi.right; break;
                case Position.BackLeft: case Position.BackRight: iff = osdi.back; break;
            }
        } else if (ld < fd && ld < rd && ld < bd) {
            switch (arrowPosition) {
                case Position.Left: iff = osdi.back; break;
                case Position.Right: iff = osdi.front; break;
                case Position.BackLeft: case Position.BackRight: iff = osdi.right; break;
            }
        } else if (rd < ld && rd < fd && rd < bd) {
            switch (arrowPosition) {
                case Position.Left: iff = osdi.front; break;
                case Position.Right: iff = osdi.back; break;
                case Position.BackLeft: case Position.BackRight: iff = osdi.left; break;
            }
        } else if (bd < ld && bd < rd && bd < fd) {
            switch (arrowPosition) {
                case Position.Left: iff = osdi.right; break;
                case Position.Right: iff = osdi.left; break;
                case Position.BackLeft: case Position.BackRight: iff = osdi.front; break;
            }
        } else {
            Debug.Log("problem");
            return;
        }

        bool isLeft = (arrowPosition == Position.Left || arrowPosition == Position.BackLeft);
        bool isDouble = (arrowPosition == Position.BackLeft || arrowPosition == Position.BackRight);

        transform.SetParent(iff.canvas.transform, worldPositionStays: false);

        GetComponent<SpriteRenderer>().flipY = !isLeft;
        GetComponent<RectTransform>().anchoredPosition = new Vector2(512+off, 256);

        off += (isDouble ? 8 : 16) * (isLeft ? 1 : -1);
        if (Mathf.Abs(off) > 256) {
            off = isDouble ? off/8 : -off;
        }

    }

}
