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
using System.Collections.Generic;
using UnityEngine;
using SpamalotExtensions;


/// <summary>
/// Spawn an empty GameObject as a child of the vehicle Transformable, and set
/// our Transformable to be the Transform of this child.
/// </summary>
/// <remarks>
/// Eventually, can use the SeatInterface to customize the player position
/// within the vehicle;
/// </remarks>
public class ContainedPlayerController : PrefabController {

    public override List<Type> Dependencies { get; } = new List<Type> { typeof(VehicleInterface), typeof(SeatInterface) }; //, typeof(NestedDependency<VehicleInterface, TransformableInterface>) };

    public override void Install() {
        var vehicleTransformable = PrefabObject.GetPrefabInterface<VehicleInterface>().vehicle.GetPrefabInterface<TransformableInterface>().transformable;
        var obj = new GameObject();
        obj.name = "ContainedPlayerTransformable";
        obj.transform.SetParent(vehicleTransformable, worldPositionStays: false);
        var transformableInterface = PrefabObject.AddComponent<TransformableInterface>();
        transformableInterface.transformable = obj.transform;
    }

}
