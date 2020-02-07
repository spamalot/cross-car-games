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

using System.Linq;
using UnityEngine;
using SpamalotExtensions;

public class BilliardsShakeBoost : MonoBehaviour {

    public Human human;
    public PlayerDB playerDB;
    private CarAI _myCarAI;
    private Transform _myCarTransformable;

    public AudioClip boostSound;

    private Vector3 lastVelocity;
    private float acc;
    private int recoverTimer = 0;

    void Start () {
        _myCarAI = human.GetPrefabInterface<VehicleInterface>().vehicle.GetComponentInChildren<CarAI>();
        _myCarTransformable = human.GetPrefabInterface<VehicleInterface>().vehicle.GetPrefabInterface<TransformableInterface>().transformable;
        lastVelocity = human.RightHandVelocity;
    }
    
    void Update () {
        acc = (human.RightHandVelocity - lastVelocity).magnitude / Time.deltaTime;
        lastVelocity = human.RightHandVelocity;

        var myDir = _myCarAI.VehicleDirection;
        var myz = _myCarTransformable.position.z;
        var isOpposing = playerDB.PlayerNames.Values
            .Any(x => x.GetPrefabInterface<VehicleInterface>().vehicle.GetComponentInChildren<CarAI>().VehicleDirection != myDir
                && Mathf.Abs(myz - x.GetPrefabInterface<VehicleInterface>().vehicle.GetPrefabInterface<TransformableInterface>().transformable.position.z) < 5);

        if (recoverTimer == 0 && isOpposing) {

            if (acc > 40) {
                human.GetComponent<BilliardBouncer>().BoostPower();
                var osdText = GameObject.Find("OSDText").GetComponent<OSDText>();
                osdText.Show(osdText.LookingPosition, OSDText.TextType.Sprite, "LaunchPower");
                human.GetComponentInChildren<AudioSource>().PlayOneShot(boostSound);
                recoverTimer = 90;
            } else {
                var osdText = GameObject.Find("OSDText").GetComponent<OSDText>();
                osdText.Show(osdText.LookingPosition, OSDText.TextType.Sprite, "ShakeForLaunchPower");
            }
            
        }

        if (recoverTimer > 0) {
            --recoverTimer;
        }

    }
}
