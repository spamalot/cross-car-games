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

public class EnemySpawner : MonoBehaviour {

    public GameObject enemyPlayerPrefab;
    public GameObject NPCCarPrefab;
    public GameObject carAIControllerPrefab;
    public Arranger arranger;

    // FIXME: don't make static
    public static readonly string[] enemyNames = { "Enemy1", "Enemy2", "Enemy3", "Enemy4", "Enemy5", "Enemy6", "Enemy7" , "Enemy8", "Enemy9", "Enemy10", "Enemy11" };

    void Start() {

        foreach (var name in enemyNames) {
            var npcCar = Instantiate(NPCCarPrefab, transform);
            npcCar.name = $"{name}Car";
            npcCar.GetComponent<AdvancedPrefabNester>().AddController(carAIControllerPrefab);
            arranger.AddArrangeable(npcCar.GetComponent<Arrangeable>());
            npcCar.GetPrefabInterface<BodyCollidableInterface>().bodyCollider.GetComponent<Collider>().enabled = false;
        }

        PlayerDB playerDB = GameObject.Find("PlayerDB").GetComponent<PlayerDB>();

        // FIXME: make instance level
        string[] actualEnemyNames = { "Enemy2", "Enemy5", "Enemy8", "Enemy9", "Enemy11" };

        foreach (var name in actualEnemyNames) {
            var npcCar = GameObject.Find($"{name}Car");
            npcCar.GetPrefabInterface<BodyCollidableInterface>().bodyCollider.GetComponent<Collider>().enabled = true;

            var enemyPlayer = Instantiate(enemyPlayerPrefab, transform);
            enemyPlayer.name = name;
            var vehicleInterface = enemyPlayer.AddComponent<VehicleInterface>();
            vehicleInterface.vehicle = npcCar;

            /*
            var v2 = enemyPlayer.AddComponent<GenericMonoBehaviour>();
            v2.genericType = typeof(ProxyInterface<>).FullName.Split('`')[0];
            v2.genericArguments = new string[] { typeof(VehicleTypeTag).FullName };
            v2.ClearCache();
            Debug.Log("test "+enemyPlayer.GetComponent<ProxyInterface<VehicleTypeTag>>());//.proxy = npcCar.GetComponent<VehicleTypeTag>();
            */

            // FIXME: nominally the above should be a controller, so we don't have to do this:
            enemyPlayer.GetComponent<AdvancedPrefabNester>().AddQueuedControllers();

            playerDB.PlayerNames[name] = enemyPlayer;
        }

       /* var t2 = GameObject.Find("MyPlayer").GetComponent<GenericMonoBehaviour>()._dynamicType;
        Debug.Log("test" + t2.IsSubclassOf(typeof(ProxyInterface<TypeTag>)));
        Debug.Log("test" + GameObject.Find("MyPlayer").GetComponent(GameObject.Find("MyPlayer").GetComponent<GenericMonoBehaviour>()._dynamicType));
        */
    }

}
