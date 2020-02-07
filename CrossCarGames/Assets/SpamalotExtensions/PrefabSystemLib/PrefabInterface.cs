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
using UnityEngine;

namespace SpamalotExtensions {

    [RequireComponent(typeof(AdvancedPrefabNester))]
    public abstract class PrefabInterface : MonoBehaviour {

        class NotWhatYouMeantException : Exception { }

        /// <summary>
        /// Accessing this is almost certainly a mistake -- probably mean to
        /// access transform of an object *referenced by* this interface.
        /// </summary>
        public new Transform transform {
            get {
                throw new NotWhatYouMeantException();
            }
        }

        /// <summary>
        /// Accessing this is almost certainly a mistake -- probably mean to
        /// call this on an object *referenced by* this interface.
        /// </summary>
        public T GetPrefabInterface<T>() {
            throw new NotWhatYouMeantException();
        }

        /// <summary>
        /// Same as GetPrefabInterface, but explicitly states intent.
        /// </summary>
        public T GetSiblingPrefabInterface<T>() where T : PrefabInterface {
            return ((Component)this).GetPrefabInterface<T>();
        }


        /// <summary>
        /// Accessing this is likely a mistake -- probably mean to
        /// access a different GameObject *referenced by* this interface.
        /// </summary>
        public new GameObject gameObject {
            get {
                throw new NotWhatYouMeantException();
            }
        }


    }

    /*[RequireComponent(typeof(AdvancedPrefabNester))]
    public class ProxyInterface<T> : PrefabInterface where T : TypeTag {

        public T proxy;

    }*/

}