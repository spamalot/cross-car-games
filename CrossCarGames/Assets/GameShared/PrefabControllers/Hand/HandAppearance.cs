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

public class HandAppearance : MonoBehaviour
{
    public Mesh handOpenMesh;
    public Mesh handClosedMesh;
    public Mesh handPointingMesh;

    public enum HandMeshState { Open, Closed, Pointing, Hidden };

    public HandMeshState MeshState {
        set {
            var child = GetComponentInChildren<MeshFilter>();
            switch (value) {
                case HandMeshState.Open: child.mesh = handOpenMesh; break;
                case HandMeshState.Closed: child.mesh = handClosedMesh; break;
                case HandMeshState.Pointing: child.mesh = handPointingMesh; break;
                case HandMeshState.Hidden: child.mesh = null; break;
            }
        }
    }

}
