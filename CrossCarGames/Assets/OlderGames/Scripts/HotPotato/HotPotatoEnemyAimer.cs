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

public class HotPotatoEnemyAimer : AbstractEnemyAimer {

    public BallGame.EnemyThrowerCatcher throwerCatcher;

    public void SafeToThrow() {
        throwerCatcher.ThrowAtTarget(
            throwerCatcher.ThrowTo.GetPrefabInterface<HandInterface>().hand.transform.position,
            throwerCatcher.GetPrefabInterface<VehicleInterface>().vehicle.GetPrefabInterface<HandInterface>().hand.transform.position + new Vector3(throwerCatcher.SignedRand * 7f, 0, throwerCatcher.SignedRand * 7f),
            Mathf.PI / 6 + throwerCatcher.SignedRand * Mathf.PI / 8);
        throwerCatcher.Throw();
    }

}
