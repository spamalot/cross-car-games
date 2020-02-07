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

public class DecorationEnemy : MonoBehaviour {

    private const float chanceRate = 0.004f;
    private const float chanceRateGood = 1f;
    private const float chanceChangeOutfit = 0.002f;
    private const float chanceChangeSlot = 0.6f;

    private CarDecorator decorator;

    private NearbyPlayerHelper nearbyPlayerHelper;

    void Start() {
        // FIXME: not the greatest, but it works
        nearbyPlayerHelper = GameObject.Find("Helpers").GetComponent<NearbyPlayerHelper>();

        decorator = GetComponent<CarDecorator>();
        GetComponent<Scoreable>().Score += Random.Range(0, 4); // for realism
        ChangeOutfit();
    }
    
    private void ChangeOutfit() {
        foreach (var slot in decorator.Slots) {
            if (Random.value < chanceChangeSlot) {
                var available = decorator.AvailableDecorations[slot];
                if (available.Count == 0) {
                    // Don't even have the empty decoration yet
                    break;
                }
                decorator.SetDecoration(slot, available[Random.Range(0, available.Count)]);
            }
        }
    }

    private void RateSomeone() {
        var other = nearbyPlayerHelper.FindInRangePlayer(gameObject);
        // FIXME: not the greatest, but it works
        other.GetPrefabInterface<VehicleInterface>().vehicle.GetPrefabInterface<BodyCollidableInterface>().bodyCollider.GetComponent<RaycastRating>().Rating =
            Random.value < chanceRateGood ? RaycastRating.RatingType.Happy : RaycastRating.RatingType.Sad;
    }

    void Update() {
        if (Random.value < chanceChangeOutfit) {
            ChangeOutfit();
        }
        if (Random.value < chanceRate) {
            RateSomeone();
        }
    }
}
