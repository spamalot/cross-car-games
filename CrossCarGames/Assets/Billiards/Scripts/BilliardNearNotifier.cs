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

public class BilliardNearNotifier : MonoBehaviour {

    public Human human;
    public Transform myCar;
    //public LineRenderer bounceTargetLine;

    private GameObject currentBallTarget;

    void Update() {
        transform.position = myCar.position;

        if (!human.GetComponent<BilliardBouncer>().IsBouncing) {
            currentBallTarget = null;
        }

        //bounceTargetLine.enabled = currentBallTarget != null;
        if (currentBallTarget != null) {
           // bounceTargetLine.SetPosition(0, human.GetPrefabInterface<VehicleInterface>().vehicle.GetPrefabInterface<ShieldInterface>().shield.transform.position);
            // bounceTargetLine.SetPosition(1, currentBallTarget.GetPrefabInterface<TransformableInterface>().transformable.position + new Vector3(0, 2, 0));
        }

    }

    void OnTriggerEnter(Collider other) {
        // ignore non-billiards
        if (other.GetComponent<BilliardBounce>() == null) {
            return;
        }

        var foo = Mathf.Abs(Vector3.SignedAngle(other.GetComponent<DeadSimplePhysics>().velocity, other.transform.position - transform.position, Vector3.up));
        if (foo > 100 && human.GetComponent<BilliardBouncer>().IsBouncing) {
            //other.GetComponent<BilliardBounce>().GoSlowMo();
            other.transform.Find("DangerRing").GetComponent<ParticleSystem>().Play();
            currentBallTarget = other.GetComponent<BilliardBounce>().TargetPlayer;
        }

    }
}