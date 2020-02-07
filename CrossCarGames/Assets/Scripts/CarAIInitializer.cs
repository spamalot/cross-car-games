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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAIInitializer : MonoBehaviour {

    public enum Mode { Traffic, Highway, Stopped, WarpSpeed };

    public Mode mode = Mode.Traffic;

    public struct ConfigType {
        public int STEER_ANGLE_DEGREES;
        public int nominalSpd;
        public float closeDistance;
        public float laneChangeStep;
    }

    private Dictionary<Mode, ConfigType> _modes = new Dictionary<Mode, ConfigType> {
        { Mode.Traffic, new ConfigType { STEER_ANGLE_DEGREES = 10, nominalSpd = 40, closeDistance = 15, laneChangeStep = 0.008f } },
        { Mode.Highway, new ConfigType { STEER_ANGLE_DEGREES = 8, nominalSpd = 100, closeDistance = 25, laneChangeStep = 0.016f} },
        { Mode.Stopped, new ConfigType { STEER_ANGLE_DEGREES = 45, nominalSpd = 0, closeDistance = 15, laneChangeStep = 0.002f} },
        { Mode.WarpSpeed, new ConfigType { STEER_ANGLE_DEGREES = 4, nominalSpd = 2800, closeDistance = 40, laneChangeStep = 0.1f} },
    };

    public ConfigType Config {
        get {
            return _modes[mode];
        }
    }
    
}
