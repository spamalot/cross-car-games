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
using UnityEngine.UI;
using SpamalotExtensions;

public class BilliardsTouchLauncher : PhoneScreen {

    public Human human;
    public BilliardsController billiardsController;
    public GameObject ballIcon;
    public LineRenderer line1;
    public LineRenderer line2;
    public Sprite background1;
    public Sprite background2;
    public GameObject billiardPrefab;
    public Material myBilliardMaterial;
    public LineRenderer countDownLine;
    public Text countDownText;
    public MeshRenderer topViewRenderer;

    private BilliardBouncer bouncer;
    private readonly Vector2 touchDownCoords = new Vector2(0, 1);
    private Vector3 touchUpCoords;

    private GameObject b;

    private bool wait;
    private bool subscribed = false;
    private bool onScreen2;


    private void PrepAndDoLaunch() {
        var beh = b.GetComponent<BilliardBounce>();
        if (onScreen2) {
            if (bouncer.IsLaunching) {
                beh.State = BilliardBounce.StateType.Airborne;
            }
        } else {
            beh.State = BilliardBounce.StateType.Dead;
        }
        bouncer.IsLaunching = false;
        controller.ChangeScreen("wait");
    }

    private void Screen1Update() {
        if (wait && human.RightTouching) {
            return;
        }

        wait = false;

        if (human.RightTouching) {
            OnStartLaunch();
        }

    }

    private void Screen2Update() {
        bool _canLaunch = true; // human.GetComponent<BilliardStash>().BilliardCount > 0;

        ballIcon.GetComponent<SpriteRenderer>().enabled = _canLaunch;

        if (!_canLaunch) {
            controller.ChangeScreen("choose");
            return;
        }

        if (human.RightTouching) {
            touchUpCoords = human.RightTouchCoords;
            line1.SetPosition(0, new Vector3(-1, 1, 0));
            line1.SetPosition(1, new Vector3(touchUpCoords.x, touchUpCoords.y, 0));
            line2.SetPosition(0, new Vector3(1, 1, 0));
            line2.SetPosition(1, new Vector3(touchUpCoords.x, touchUpCoords.y, 0));
            ballIcon.GetComponent<RectTransform>().anchoredPosition = touchUpCoords;

            var beh = b.GetComponent<BilliardBounce>();
            // Launch as if slingshot, rather than based on touch down coordinates
            beh.AnticipatedLaunchDirection =
                (transform.TransformPoint(new Vector3(touchDownCoords.x, touchDownCoords.y, 0))
                - transform.TransformPoint(new Vector3(touchUpCoords.x, touchUpCoords.y, 0))) * 80 * bouncer.Power
                 + human.GetPrefabInterface<VehicleInterface>().vehicle.GetComponentInChildren<CarAI>().Velocity;

            bouncer.IsLaunching = true;

        } else {
            PrepAndDoLaunch();
        }
    }

    // late because need updated vive position so we aren't skippy
    void Update () {
        var phys = b.GetComponent<DeadSimplePhysics>();
        phys.position = transform.position;
        if (!onScreen2) {
            Screen1Update();
        } else {
            Screen2Update();
        }
    }

    private void OnTimeUp() {
        PrepAndDoLaunch();
    }

    void OnEnable() {
        wait = human.IsFullyTracked() ? human.RightTouching : false;
        onScreen2 = false;

        bouncer = human.GetComponent<BilliardBouncer>();
        
        countDownLine.enabled = true;
        countDownText.enabled = true;

        topViewRenderer.enabled = true;

        controller.SetBackground(background1);
        // Compute and store target
        if (human.IsFullyTracked()) {
            bouncer.CurrentLaunchTargetPlayer = billiardsController.GenerateTarget(human.gameObject);
            b = Instantiate(billiardPrefab, GameObject.Find("Balls").transform);
            b.GetComponent<MeshRenderer>().material = myBilliardMaterial;
            var beh = b.GetComponent<BilliardBounce>();
            beh.SourcePlayer = human.gameObject;
            beh.TargetPlayer = bouncer.CurrentLaunchTargetPlayer;

        }
        if (!subscribed) {
            human.GetComponent<BilliardBouncer>().OnLaunchTimeUp += OnTimeUp;
            subscribed = true;
        }

    }

    void OnStartLaunch() {
        onScreen2 = true;
        line1.enabled = true;
        line2.enabled = true;
        topViewRenderer.enabled = false;
    }

    void OnDisable() {
        ballIcon.GetComponent<SpriteRenderer>().enabled = false;
        line1.enabled = false;
        line2.enabled = false;
        countDownLine.enabled = false;
        countDownText.enabled = false;
        topViewRenderer.enabled = false;

        if (human.IsFullyTracked()) {
            bouncer.CurrentLaunchTargetPlayer = null;
        }
    }

}
