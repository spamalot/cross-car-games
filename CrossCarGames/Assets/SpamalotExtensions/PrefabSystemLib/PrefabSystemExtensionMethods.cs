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

namespace SpamalotExtensions {

    public static class PrefabSystemExtensionMethods {

        /// <summary>
        /// Try to grab the given interface from the given object.
        /// </summary>
        /// <remarks>
        /// Equivalent to obj.GetComponent with type checking.
        /// </remarks>
        /// <typeparam name="T">Type of prefab</typeparam>
        /// <param name="obj">GameObject</param>
        /// <returns>Interface</returns>
        public static T GetPrefabInterface<T>(this Component component) where T : PrefabInterface {
            return component.gameObject.GetPrefabInterface<T>();
        }

        public static T GetPrefabInterface<T>(this GameObject obj) where T : PrefabInterface {

            if (obj.GetComponent<AdvancedPrefabNester>() == null) {
                throw new PrefabController.HasNoPrefabNesterException();
            }

            return obj.GetComponent<T>();
        }
    }

}