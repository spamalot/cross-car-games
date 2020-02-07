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

using System.Linq;
using UnityEngine;
using SpamalotExtensions;

public class NearbyPlayerHelper : MonoBehaviour {

    public PlayerDB playerDB;

    [Layer]
    public int bodyColliderLayer;

    /// <summary>
    /// Try to find a player that isn't blocked by another player
    /// </summary>
    /// <param name="sourcePlayer">Player to look outwards from</param>
    /// <returns>First player found</returns>
    public GameObject FindInRangePlayer(GameObject sourcePlayer, bool sameSide=false) {
        var otherPlayerNames = playerDB.PlayerNames.Keys.Where(i => i != sourcePlayer.name).ToList();
        GameObject playerObj = null;
        var good = false;
        var timeout = 10; // FIXME: won't scale with more players
        
        while (!good && timeout-- > 0) {
            playerObj = playerDB.PlayerNames[otherPlayerNames[Random.Range(0, otherPlayerNames.Count())]];
           
            if (sameSide && playerObj.GetPrefabInterface<VehicleInterface>().vehicle.GetComponentInChildren<CarAI>().VehicleDirection != sourcePlayer.GetPrefabInterface<VehicleInterface>().vehicle.GetComponentInChildren<CarAI>().VehicleDirection) {
                continue;
            }
            var sourcePlayerPos = sourcePlayer.GetPrefabInterface<TransformableInterface>().transformable.position + new Vector3(0, 2, 0);
            var destPlayerPos = playerObj.GetPrefabInterface<TransformableInterface>().transformable.position;
            RaycastHit hit;
            if (Physics.Raycast(new Ray(sourcePlayerPos, destPlayerPos - sourcePlayerPos), out hit, 60f, 1 << bodyColliderLayer)) {
                good = hit.collider == playerObj.GetPrefabInterface<VehicleInterface>().vehicle.GetPrefabInterface<BodyCollidableInterface>().bodyCollider.GetComponent<Collider>();
            }
        }

        return playerObj;
    }
}
