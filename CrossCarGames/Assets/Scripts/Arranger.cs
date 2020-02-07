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
using System.Text.RegularExpressions;
using UnityEngine;
using SpamalotExtensions;

public class Arranger : MonoBehaviour {

    private const float STEP_SIZE = 20f;
    private const float LANE_SIZE = 5f;

    public TextAsset file;
    public Arrangeable[] initialArrangeables; // initial set to populate; also can populate via AddArrangeable

    [Layer]
    public int forwardLayer;
    [Layer]
    public int backwardLayer;
    [Layer]
    public int allTrafficLayer;

    private struct _Position {
        // TODO: support car orientation
        public float pos;
        public int lane;
        public CarAI.Direction direction;
    }

    private Queue<_Position> _availablePositions = new Queue<_Position>();
    private int _idCounter = 0;
    // Only used for rearranging
    private List<Arrangeable> _arranged = new List<Arrangeable>();

    public class AlreadyArrangedException : System.Exception {}
    public class NoMorePositionsException : System.Exception {}

    public void AddArrangeable(Arrangeable arrangeable) {
        if (arrangeable.Id != null) {
            throw new AlreadyArrangedException();
        }

        if (_availablePositions.Count == 0) {
            throw new NoMorePositionsException();
        }

        _Position p = _availablePositions.Dequeue();
        arrangeable.transform.position = new Vector3((p.lane - 0.5f) * LANE_SIZE, arrangeable.transform.position.y, p.pos * STEP_SIZE);
        arrangeable.transform.rotation = p.direction == CarAI.Direction.Forward ? Quaternion.Euler(0, -90, 0) : Quaternion.Euler(0, 90, 0);
        arrangeable.GetComponentInChildren<CarAI>().gameObject.layer = p.direction == CarAI.Direction.Forward ? forwardLayer : backwardLayer;
        arrangeable.GetComponentInChildren<CarAI>().involvedLayers =
            (p.direction == CarAI.Direction.Forward ? (1 << forwardLayer) : (1 << backwardLayer)) | (1 << allTrafficLayer);
        arrangeable.GetComponentInChildren<CarAI>().VehicleDirection = p.direction;
        arrangeable.Id = _idCounter++;

        _arranged.Add(arrangeable);
    }

    private void Awake() {
        // load script data from text asset
        ReadInScriptedPositions();
    }

    void Start () {
        foreach (var a in initialArrangeables) {
            AddArrangeable(a);
        }
    }

    private void ReadInScriptedPositions() {
        // each line is position followed by lane
        var lines = Regex.Split(file.text.Trim(), @"\r\n|\n|\r");
 
        for (var i = 0; i < lines.Length; i++) {
            var split = Regex.Split(lines[i].Trim(), @"\s+");
            _availablePositions.Enqueue(new _Position { pos = float.Parse(split[0]), lane = int.Parse(split[1]),  direction = (CarAI.Direction) System.Enum.Parse(typeof(CarAI.Direction), split[2]) });
        }
    }

    /// <summary>
    /// Can be used to reset car positions in case there is a car overlapping
    /// bug.
    /// </summary>
    private void Rearrange() {
        ReadInScriptedPositions();
        var cloneArranged = new List<Arrangeable>(_arranged);
        _arranged.Clear();
        foreach (var a in cloneArranged) {
            a.Id = null;
            AddArrangeable(a);
        }
    }

    void Update() {
        if (Input.GetKeyDown("r")) {
            Debug.Log("Rearranging!");
            Rearrange();
        }
    }

}
