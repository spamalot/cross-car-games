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

public class RatingScreen : PhoneScreen {

    public Sprite background;
    public ViveTouchGestureRecognizer gestureRecognizer;
    public LineRenderer pointingLine;
    public Human human;
    public Transform selectHighlight;

    private bool wait;
    private RaycastRating last;
    private bool isMode2;


    private void Mode1() {
        float l;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformVector(Vector3.up), out hit, 100f, LayerMask.GetMask("BodyCollider"))) {
            var foo = hit.collider.GetComponent<BodyCollidableBackpointer>().pointer.GetSiblingPrefabInterface<TransformableInterface>();
            if (hit.collider.GetComponent<RaycastRating>() != null) { // i.e. is a player
                selectHighlight.transform.position = foo.transformable.position + new Vector3(0, 1, 0);
                l = Vector3.Distance(foo.transformable.position, transform.position);
                last = hit.collider.GetComponent<RaycastRating>();
            } else {
                l = 50;
                last = null;
            }
        } else {
            l = 50;
            last = null;
        }
        pointingLine.SetPosition(0, transform.position);
        pointingLine.SetPosition(1, transform.position + transform.TransformDirection(Vector3.up) * l);

        if (wait && human.RightTouching) {
            return;
        }

        wait = false;

        if (human.RightTouching & last != null) {
            wait = true;
            isMode2 = true;
            //controller.SetBackground(background2);
        }
    }


    private void Mode2() {
        //if (wait && human.RightTouching) {
        //    return;
        //}

        //wait = false;

        //if (human.RightTouching) {
        //    if (human.RightTouchCoords.x > 0.2) {
        //        last.Rating = RaycastRating.RatingType.Happy;
        //    }
        //    if (human.RightTouchCoords.x < -0.2) {
        //        last.Rating = RaycastRating.RatingType.Sad;
        //    }
        last.Rating = RaycastRating.RatingType.Happy;
        var osdText = GameObject.Find("OSDText").GetComponent<OSDText>();
        osdText.Show(osdText.LookingPosition, OSDText.TextType.Sprite, "RatedOther");
        human.GetComponent<DecorationKarmaCounter>().ResetKarma();
        controller.ChangeScreen("rating");
        //}
    }

    void Update() {
        if (!isMode2) {
            Mode1();
        } else {
            Mode2();
        }
    }

    private void Gestured(ViveTouchGestureRecognizer.GestureType gestureType, Vector2 pos) {

        if (isMode2) {
            return;
        }

        if (gestureType == ViveTouchGestureRecognizer.GestureType.SwipeRight) {
            controller.ChangeScreen("decoration");
        }
    }

    void OnEnable() {
        controller.SetBackground(background);
        wait = human.IsFullyTracked() ? human.RightTouching : false;
        gestureRecognizer.OnGestured += Gestured;
        pointingLine.enabled = true;
        isMode2 = false;
    }

    void OnDisable() {
        gestureRecognizer.OnGestured -= Gestured;
        pointingLine.enabled = false;
    }
}
