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
using SpamalotExtensions;

public class BallEnemyController : PrefabController {

    public override List<Type> Dependencies { get; } = new List<Type> { typeof(VehicleInterface) };

    public override void Install() {
        var vehicle = PrefabObject.GetPrefabInterface<VehicleInterface>().vehicle;
        var throwerCatcher = PrefabObject.AddComponent<BallGame.EnemyThrowerCatcher>();
        throwerCatcher.hand = vehicle.GetPrefabInterface<HandInterface>().hand;
        throwerCatcher.bodyCollider = vehicle.GetPrefabInterface<BodyCollidableInterface>().bodyCollider.gameObject;
    }

}
