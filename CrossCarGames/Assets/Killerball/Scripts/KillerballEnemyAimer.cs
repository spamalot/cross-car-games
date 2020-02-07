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

public class KillerballEnemyAimer : AbstractEnemyAimer {

    public BallGame.EnemyThrowerCatcher throwerCatcher;

    public void SafeToThrow() {
        var playerDB = GameObject.Find("PlayerDB").GetComponent<PlayerDB>();

        Vector3 targetPos;
        var ourPos = throwerCatcher.GetPrefabInterface<VehicleInterface>().vehicle.GetPrefabInterface<HandInterface>().hand.transform.position;

        var randomPlayer = GameObject.Find("Helpers").GetComponent<NearbyPlayerHelper>().FindInRangePlayer(gameObject);
        
        // To keep the enemies fair, basically have them only throw at other players (or in a random direction sometimes)

        // FIXME: improve
        if (randomPlayer.name != "MyPlayer") {
            targetPos = randomPlayer.GetPrefabInterface<TransformableInterface>().transformable.position;
        } else {
            targetPos = ourPos + Quaternion.Euler(0, Random.Range(0, 360), 0) * Vector3.forward * Random.Range(2f, 10f);
        }
        throwerCatcher.ThrowAtTarget(
            targetPos + new Vector3(throwerCatcher.SignedRand * 0.1f, 0, throwerCatcher.SignedRand * 0.1f),
            ourPos,
            Mathf.PI / 8 + throwerCatcher.SignedRand * Mathf.PI / 16);
        throwerCatcher.Throw();
    }

}
