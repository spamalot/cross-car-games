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

public class BilliardsTouchBouncer : PhoneScreen {

    public Human human;
    public LineRenderer line1;
    public LineRenderer line2;
    public Sprite background;
    public MeshRenderer topViewRenderer;

    private Vector3 touchUpCoords;

    private GameObject _vehicle;
    private BilliardBouncer bouncer;
    private ShieldBackpointer shield;

    private bool _wasTouching = false;

    private void CalcBounce() {
        shield.Normal = shield.transform.TransformVector(Vector3.up);
        bouncer.IsBouncing = true;
    }

    void Update() {
        if (human.RightTouching) {
            if (!_wasTouching) {
                shield.GetComponentInChildren<Renderer>().enabled = true;
                topViewRenderer.enabled = false;
                human.GetComponent<Scoreable>().Score -= 500;
            }
            CalcBounce();
        } else { 
            shield.GetComponentInChildren<Renderer>().enabled = false;
            topViewRenderer.enabled = true;
            shield.Normal = null;
            bouncer.IsBouncing = false;
        }
        _wasTouching = human.RightTouching;
    }

    void OnEnable() {
        bouncer = human.GetComponent<BilliardBouncer>();
        _vehicle = human.GetPrefabInterface<VehicleInterface>().vehicle;

        shield = _vehicle.GetPrefabInterface<ShieldInterface>().shield;
        
        controller.SetBackground(background);

        topViewRenderer.enabled = true;
    }

    void OnDisable() {
        shield.GetComponentInChildren<Renderer>().enabled = false;
        topViewRenderer.enabled = false;
    }

}
