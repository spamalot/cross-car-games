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
using UnityEngine;

public class GameInitializer : MonoBehaviour
{

    public enum Game { None, HotPotato, Bang, Killerball, Billiards, Decoration, TakeoverPractice };

    // Set by Unity
    [Header("Play Configuration")]
    public Game game;
    private bool _ARWas3D = false;
    public bool ARIs3D = false;
    private CarAIInitializer.Mode _oldTrafficMode = CarAIInitializer.Mode.Traffic;
    public CarAIInitializer.Mode trafficMode = CarAIInitializer.Mode.Traffic;

    [Header("Convenience Config")]
    public Human human;
    public CarAIInitializer carAIInitializer;
    public TerrainLeapFrog terrainLeapFrog;

    [Header("Cameras")]
    public LayerMask dontRender;

    [Header("AR")]
    public LayerMask renderInAR;
    public Camera hmdEye;
    public Camera[] hudCameras;

    private Dictionary<Game, GameObject> _games;

    void Awake()
    {
        _games = new Dictionary<Game, GameObject>() {
            { Game.HotPotato, GameObject.Find("Game_HotPotato") },
            { Game.Bang, GameObject.Find("Game_Bang") },
            { Game.Killerball, GameObject.Find("Game_Killerball") },
            { Game.Billiards, GameObject.Find("Game_Billiards") },
            { Game.Decoration, GameObject.Find("Game_Decoration") },
            { Game.TakeoverPractice, GameObject.Find("Game_TakeoverPractice") },
        };

        // Destroy unnecessary games.
        foreach (var entry in _games) {
            if (entry.Key != game) {
                Destroy(entry.Value);
            }
        }

        foreach (var obj in _games.Values) {
            obj.SetActive(false);
        }

        _games[game].SetActive(true);

        ConfigCameras();
    }

    public void ConfigCameras() {
        if (ARIs3D) {
            hmdEye.cullingMask = -1;
            foreach (var cam in hudCameras) {
                cam.cullingMask = 0;
            }
        } else {
            hmdEye.cullingMask = ~renderInAR;
            foreach (var cam in hudCameras) {
                cam.cullingMask = renderInAR;
            }
        }
        hmdEye.cullingMask &= ~dontRender;
    }

    void Update() {
        if (_ARWas3D != ARIs3D) {
            ConfigCameras();
        }
        _ARWas3D = ARIs3D;

        if (_oldTrafficMode != trafficMode) {
            carAIInitializer.mode = trafficMode;
            terrainLeapFrog.showSkyscrapers = (trafficMode != CarAIInitializer.Mode.Highway);
        }
        _oldTrafficMode = trafficMode;
    }
}
