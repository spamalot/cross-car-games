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

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SpamalotExtensions {

    /// <summary>
    /// Separate from PrefabInterface, because then this allows us to have multiple
    /// advanced prefab nesters for a given interface
    /// FIXME: this above statement is bs -- we can add as many controllers as we need with just
    /// this one component
    /// </summary>
    public class AdvancedPrefabNester : MonoBehaviour {

        public List<GameObject> queuedControllers; // initial set to populate; also can populate via AddController

        public class DependencyNotAnInterfaceException : System.Exception { }

        /// <summary>
        /// Adds a controller, assuming all its dependencies are satisfied.
        /// Automatically adds queued controllers waiting for this one as
        /// a dependency.
        /// </summary>
        /// <param name="controllerPrefab"></param>
        public void AddController(GameObject controllerPrefab) {
            var prefabDeps = controllerPrefab.GetComponent<PrefabController>().Dependencies;

            // Ensure dependencies are in fact PrefabInterfaces
            if (!prefabDeps.All(type => type.IsSubclassOf(typeof(PrefabInterface)))) {
                throw new DependencyNotAnInterfaceException();
            }

            // check dependencies satisfied
            // (don't use GetPrefabInterface because we've already done
            //  type-checking)
            if (RecursiveDependencies(prefabDeps, this)) {//prefabDeps.All(x => (GetComponent(x) != null))) {
                var controllerObj = Instantiate(controllerPrefab, transform);
                var controller = controllerObj.GetComponent<PrefabController>();
                controller.PrefabObject = gameObject;
                controller.Install();
                Destroy(controllerObj);
                // Maybe we were a dependency of the queued controllers:
                AddQueuedControllers();
            } else {
                queuedControllers.Add(controllerPrefab);
            }
        }

        private bool RecursiveDependencies(List<System.Type> deps, Component level) {
            bool res = true;
            foreach (var dep in deps) {
                if (dep.IsGenericType && dep.GetGenericTypeDefinition() == typeof(NestedDependency<,>)) {
                    //Debug.Log("bingo");
                    var newLevel = dep.GetGenericArguments()[0];
                    var newDep = dep.GetGenericArguments()[1];
                    //Debug.Log($"A {newLevel} B {newDep}");
                    res &= RecursiveDependencies(new List<System.Type> { newDep }, level.GetComponent(newLevel));
                //   
                } else {
                    //Debug.Log($"A {level} B {dep} C {level.GetComponent(dep)}");
                    res &= level.GetComponent(dep) != null;
                }
            }

            return res;
            //throw new System.NotImplementedException();
        }

        public void AddQueuedControllers() {
            var tryCount = queuedControllers.Count;
            for (var i = 0; i < tryCount; ++i) {
                var item = queuedControllers[0];
                queuedControllers.RemoveAt(0);
                AddController(item);
            }
        }

        void Awake() {
            var initialControllers = queuedControllers;
            queuedControllers = new List<GameObject>();

            foreach (var c in initialControllers) {
                AddController(c);
            }
        }

    }

}