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

using System;
using UnityEngine;
using SpamalotExtensions;

public class BilliardBouncer : MonoBehaviour {

    public enum Mode { None, Launcher, Bouncer };

    public const float LAUNCH_TIME = 20f;

    // me, ball
    public event Action<GameObject, GameObject> OnTriggered;
    public event Action OnLaunchTimeUp;
    public event Action<Mode> OnLaunchModeChanged;

    private Mode _currentMode;
    public Mode CurrentMode {
        get { return _currentMode; }
        set {
            _currentMode = value;
            if (_currentMode == Mode.Launcher) {
                LaunchTimeLeft = LAUNCH_TIME;
            }
            OnLaunchModeChanged?.Invoke(_currentMode);
        }
    }
    public float LaunchTimeLeft { get; private set; }
    public bool IsBouncing { get; set; } = false;
    public bool IsLaunching { get; set; } = false;
    public GameObject CurrentLaunchTargetPlayer { get; set; }
    public float Power { get; private set; } = 1f;

    public void BoostPower() {
        Power *= 2;
    }

    void Start() {
        var car = this.GetPrefabInterface<VehicleInterface>().vehicle;
        var collideObjs = car.GetComponentsInChildren<DeadSimplePhysics>(includeInactive: true);
        foreach (var obj in collideObjs) {
            obj.OnCollided += CollisionEntered;
        }
    }

    void Update() {
        if (Power > 1) {
            Power -= 0.004f;
        }
        if (CurrentMode == Mode.Launcher) {
            LaunchTimeLeft -= Time.deltaTime;
            if (LaunchTimeLeft < 0) {
                OnLaunchTimeUp?.Invoke();
                CurrentMode = Mode.None;
            }
        }
    }

    private void CollisionEntered(Collider me, Collider other, Vector3 normal) {
        if (me.GetComponent<BodyCollidableBackpointer>() != null) {
            OnTriggered?.Invoke(gameObject, other.gameObject);
        }
    }
}
