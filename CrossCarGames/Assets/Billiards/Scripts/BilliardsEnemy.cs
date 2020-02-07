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

using System.Collections;
using System.Linq;
using UnityEngine;
using SpamalotExtensions;

/// <summary>
/// acts both as a launcher and a bouncer
/// </summary>
public class BilliardsEnemy : MonoBehaviour {

    // maybe make non-const eventually
    private const float chanceShoot = 0.2f;
    private const float chanceRedirectTarget = 0.9f;

    public GameObject billiardPrefab;
    public BilliardsController billiardsController;

    public BilliardBouncer bouncer;

    private System.Action ballLockAction = () => { };
    private System.Action shieldLockAction = () => { };
    private System.Action launchAngleLockAction = () => { };
    
    void Start() {
        bouncer.OnLaunchModeChanged += OnLaunchModeChange;
    }

    private void OnLaunchModeChange(BilliardBouncer.Mode mode) {
        bouncer.IsBouncing = false;
        var shield = this.GetPrefabInterface<VehicleInterface>().vehicle.GetPrefabInterface<ShieldInterface>().shield;
        shield.Normal = null;
        shieldLockAction = () => { };

        if (mode == BilliardBouncer.Mode.Launcher) {
            StartCoroutine(PrepAndDoLaunch());
        } else if (mode == BilliardBouncer.Mode.Bouncer) {
            StartCoroutine(PrepAndDoBounce());
        }
    }

    private IEnumerator PrepAndDoLaunch() {
        // FIXME: might want to delay and fiddle with the launch angle a bit first
        //if (GetComponent<BilliardStash>().BilliardCount > 0) {
        var target = billiardsController.GenerateTarget(gameObject);

        var beh = PrepLaunch(target);

        yield return new WaitForSeconds(0.8f + Random.value * 1.2f);

        GameObject launchTo;
        while (Random.value > chanceShoot) { 
            launchTo = billiardsController.GenerateTarget(gameObject);
            LaunchFiddle(beh, launchTo);
            //yield return null;
            yield return new WaitForSeconds(0.8f+Random.value*1.2f);
        }

        beh.State = BilliardBounce.StateType.Airborne;
        ballLockAction = () => { };
        launchAngleLockAction = () => { };

        //}
    }

    private BilliardBounce PrepLaunch(GameObject target) {
        var b = Instantiate(billiardPrefab, GameObject.Find("Balls").transform);

        var beh = b.GetComponent<BilliardBounce>();
        beh.SourcePlayer = gameObject;
        beh.TargetPlayer = target;

        var phys = b.GetComponent<DeadSimplePhysics>();
        ballLockAction = () => {
            phys.position = this.GetPrefabInterface<TransformableInterface>().transformable.position + new Vector3(0, 1f, 0);
        };

        return beh;
    }

    private void LaunchFiddle(BilliardBounce beh, GameObject launchTo) {
        launchAngleLockAction = () => {
            beh.AnticipatedLaunchDirection = (launchTo.GetPrefabInterface<TransformableInterface>().transformable.position - this.GetPrefabInterface<TransformableInterface>().transformable.position) * 0.4f
                + this.GetPrefabInterface<VehicleInterface>().vehicle.GetComponentInChildren<CarAI>().Velocity;
        };
        //--GetComponent<BilliardStash>().BilliardCount;
    }

    private IEnumerator PrepAndDoBounce() {
        var waitCount = 0;
        while (waitCount++ < BilliardBouncer.LAUNCH_TIME / 2f) {

            //yield return null;
            yield return new WaitForSeconds(1f);
            var balls = GameObject.Find("Balls").GetComponentsInChildren<BilliardBounce>();

            if (balls.Length == 0) {
                continue; // no billiards to redirect;
            }

            if (Random.value > chanceRedirectTarget) {
                continue;
            }

            // Keeping this old code from when there were >1 balls in case we want it again
            shieldLockAction = () => {
                var redirectTo = balls.OrderBy(ball => (ball.transform.position - this.GetPrefabInterface<TransformableInterface>().transformable.position).magnitude).First().GetComponent<BilliardBounce>().TargetPlayer;
                Bounce(redirectTo);
            };
            break;
        }
    }

    private void Bounce(GameObject redirectTo) {
        bouncer.IsBouncing = true;
        var shield = this.GetPrefabInterface<VehicleInterface>().vehicle.GetPrefabInterface<ShieldInterface>().shield;
        shield.Normal = (redirectTo.GetPrefabInterface<TransformableInterface>().transformable.position - this.GetPrefabInterface<TransformableInterface>().transformable.position).normalized;
        var t = this.GetPrefabInterface<TransformableInterface>().transformable.position;
        var r = redirectTo.GetPrefabInterface<TransformableInterface>().transformable.position;
        shield.transform.position = t + (r - t).normalized * 3f + new Vector3(0,1,0);
        shield.transform.LookAt(r);
        shield.transform.rotation *= Quaternion.Euler(90, 0, 0);
    }
    
    void Update() {
        ballLockAction();
        shieldLockAction();
        launchAngleLockAction();
    }

}
