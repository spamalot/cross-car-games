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

/// <summary>
/// Simple physics which only detects collisions and leaves the rest to the
/// user. No angular physics implemented.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class DeadSimplePhysics : MonoBehaviour {

    // me, other, normal
    public event Action<Collider, Collider, Vector3> OnCollided;

    public bool useGravity = false;
    public float g = 9.81f;

    [HideInInspector]
    public Vector3 velocity = Vector3.zero;

    public Vector3 position {
        set {
            // Not using rb because it completely breaks positioning
            // for some reason.
            //_rb.MovePosition(value);
            transform.position = value;
        }
    }

    private Rigidbody _rb;
    private Collider _myCollider;

    void Awake() {
        _rb = GetComponent<Rigidbody>();
        _myCollider = GetComponent<Collider>();
        if (!_rb.isKinematic) {
            throw new Exception($"'{name}'s Rigidbody must be kinematic");
        }
    }
    
    void Update() {
        if (float.IsNaN(velocity.x) || float.IsNaN(velocity.y) || float.IsNaN(velocity.z)) {
           throw new Exception("You're moving at hyperspeed....! " + velocity);
        }

        //_rb.MovePosition(_rb.position + velocity * Time.deltaTime);
        transform.position += velocity * Time.deltaTime;
        if (useGravity) {
            velocity += new Vector3(0, -g * Time.deltaTime, 0);
        }
    }

    private void OnTriggerEnter(Collider other) {

        RaycastHit hit;
        Vector3 normal;
        // Most accurate normal: find normal
        if (velocity.magnitude == 0 || !other.Raycast(new Ray(transform.position, velocity.normalized), out hit, float.PositiveInfinity)) {
            // we weren't moving towards the collider; try a less accurate normal
            var dir = other.ClosestPoint(transform.position) - transform.position;
            if (dir.magnitude == 0 || !other.Raycast(new Ray(transform.position, dir.normalized), out hit, float.PositiveInfinity)) {
                // Last-ditch effort
                normal = (transform.position - other.transform.position).normalized;
            } else {
                normal = hit.normal;
            }
        } else {
            normal = hit.normal;
        }

        OnCollided?.Invoke(_myCollider, other, normal);
    }

}
