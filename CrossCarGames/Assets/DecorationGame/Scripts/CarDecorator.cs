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
using System.Collections.ObjectModel;
using UnityEngine;

public class CarDecorator : MonoBehaviour {

    /// <summary>
    /// Transform of decoration mount to which decorations are added.
    /// </summary>
    public Transform parentObj;

    [System.Flags]
    public enum DecorationSlot { None = 0, Top = 1, Front = 2, Sides = 4, Back = 16, Bottom = 32, Any = (Top|Front|Sides|Back|Bottom) };

    public List<GameObject> unlockedDecorations = new List<GameObject>();

    private readonly DecorationSlot[] _slots = new DecorationSlot[] {
        DecorationSlot.Back, DecorationSlot.Front, DecorationSlot.Top, DecorationSlot.Bottom, DecorationSlot.Sides };
    /// <summary>
    /// Individual slot flags that can be iterated.
    /// </summary>
    public DecorationSlot[] Slots {
        get {
            return _slots;
        }
    }

    private Dictionary<DecorationSlot, List<GameObject>> _availableDecals = new Dictionary<DecorationSlot, List<GameObject>>();
    public ReadOnlyDictionary<DecorationSlot, List<GameObject>> AvailableDecorations {
        get {
            return new ReadOnlyDictionary<DecorationSlot, List<GameObject>>(_availableDecals);
        }
    }

    /// <summary>
    /// currently equipped decorations
    /// </summary>
    private Dictionary<DecorationSlot, GameObject> _decals = new Dictionary<DecorationSlot, GameObject>();

    public void SetDecoration(DecorationSlot slot, GameObject decoration) {
        if (slot == DecorationSlot.Any || slot == DecorationSlot.None) {
            throw new System.ArgumentException();
        }
        if ((decoration.GetComponent<CarDecoration>().slot & slot) == 0) {
            throw new System.ArgumentException();
        }
        var go = _decals.ContainsKey(slot) ? _decals[slot] : null;
        if (go != null) {
            Destroy(go);
        }
        var inst = Instantiate(decoration, parentObj);
        _decals[slot] = inst;
    }

    private void SortUnlockedDecorations() {
        foreach (var decal in unlockedDecorations) {
            foreach (var slot in _slots) {
                if ((decal.GetComponent<CarDecoration>().slot & slot) != 0) {
                    _availableDecals[slot].Add(decal);
                }
            }
        }
        unlockedDecorations.Clear();
    }

    void Awake() {
        foreach (var slot in _slots) {
            _availableDecals[slot] = new List<GameObject>();
        }
        SortUnlockedDecorations();
    }

    void Update() {
        if (unlockedDecorations.Count > 0) {
            SortUnlockedDecorations();
        }
    }


}
