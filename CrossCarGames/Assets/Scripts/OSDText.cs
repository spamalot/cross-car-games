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
using UnityEngine.UI;
using System.Collections.Generic;

public class OSDText : MonoBehaviour {

    [System.Serializable]
    public struct SpriteKey {
        public string name;
        public Sprite sprite;
    }

    public Human human;
    public List<SpriteKey> sprites = new List<SpriteKey>();

    public enum DeprecatedTextType { Instructions, SelfHit, OtherHit };
    public enum TextType { Sprite, Text };

    /// <summary>
    /// In seconds
    /// </summary>
    private float showCounter = 0;
    private bool _active = true;
    private bool _hold = false;

    /// <summary>
    /// TextType.Text should be used for showing instructions
    /// </summary>
    /// <param name="position">Point of action, screen will be based on this</param>
    /// <param name="textType"></param>
    /// <param name="spriteName"></param>
    /// <param name="hold">Prevent being replaced</param>
    public void Show(Vector3 position, TextType textType, string spriteName = null, bool hold = false) {
        if (_hold) {
            return;
        }

        var osdi = human.GetPrefabInterface<VehicleInterface>().vehicle.GetPrefabInterface<MyOSDInterface>();
        var targp = position;
        var fd = Vector3.Distance(targp, osdi.front.canvas.transform.position);
        var ld = Vector3.Distance(targp, osdi.left.canvas.transform.position);
        var rd = Vector3.Distance(targp, osdi.right.canvas.transform.position);
        var bd = Vector3.Distance(targp, osdi.back.canvas.transform.position);

        if (fd < ld && fd < rd && fd < bd) {
            transform.SetParent(osdi.front.canvas.transform, worldPositionStays: false);
        } else if (ld < fd && ld < rd && ld < bd) {
            transform.SetParent(osdi.left.canvas.transform, worldPositionStays: false);
        } else if (rd < ld && rd < fd && rd < bd) {
            transform.SetParent(osdi.right.canvas.transform, worldPositionStays: false);
        } else if (bd < ld && bd < rd && bd < fd) {
            transform.SetParent(osdi.back.canvas.transform, worldPositionStays: false);
        } else {
            Debug.Log("problem");
        }

        if (textType == TextType.Sprite) {
            GetComponentInChildren<Text>().enabled = false;
            var sr = GetComponentInChildren<SpriteRenderer>();
            sr.enabled = true;
            sr.sprite = sprites.Find(x => x.name == spriteName).sprite;
            showCounter = 1.2f;
        } else {
            GetComponentInChildren<SpriteRenderer>().enabled = false;
            GetComponentInChildren<Text>().enabled = true;
            showCounter = 10f; // Longer because probably for instructions
        }
        _active = true;
        if (hold) {
            _hold = true;
        }
    }

    void Update() {

        if (!_active) {
            return;
        }

        if (showCounter <= 0) {
            _active = false;
            _hold = false;
            GetComponentInChildren<SpriteRenderer>().enabled = false;
            GetComponentInChildren<Text>().enabled = false;
        }

        showCounter -= Time.deltaTime;
    }

    public Vector3 LookingPosition {
        get {
            var cp = GameObject.Find("Camera (eye)").transform;
            return cp.position + cp.TransformDirection(Vector3.forward * 10);
        }
    }

    public Vector3 ForwardPosition {
        get {
            return human.GetPrefabInterface<TransformableInterface>().transformable.position + new Vector3(0, 0, 5);
        }
    }

}
