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

namespace SpamalotExtensions {

    /// <summary>
    /// Abstract type for a Controller script, which should be a single script
    /// on an empty GameObject, stored in a Prefab.
    /// </summary>
    public abstract class PrefabController : MonoBehaviour {

        /// <summary>
        /// The parent object in which we're nested as a child.
        /// </summary>
        public GameObject PrefabObject { protected get; set; }

        public class HasNoPrefabNesterException : Exception { }

        /// <summary>
        /// Instantiate and configure controller-specific behaviour.
        /// </summary>
        public abstract void Install();

        /// <summary>
        /// Interfaces that the controller depends on the PrefabObject
        /// containing.
        /// </summary>
        public abstract List<Type> Dependencies { get; }

    }

}