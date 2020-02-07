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
using UnityEngine;

public class TakeoverInstrumenter : MonoBehaviour {

    public CarTakeover takeover;
    public OSDText osdText;
    public AudioClip alertSound;
    public GameObject myCar;
    public GameInitializer gameInitializer;

    public float startTime;
    public float startTimeJitter;

    private float _timeCounter = 0;
    private float _setStartTime;

    private bool _takeoverStarted = false;
    private bool _takeoverEnded = false;
    private bool _mistake = false;
    private float _avgSpeed = 999f;
    private Vector3 lastPos = Vector3.zero;
    private CarAI _myCarAi;
    private GameObject[] _poles;
    private CarAI.LaneSide safeLane;
    private float _time = 0;


    private const string LOGFILE = "takeoverlog.txt";

    // Use this for initialization
    void Start () {
        _setStartTime = startTime + startTimeJitter * (Random.value - 0.5f) * 2f;
        _myCarAi = myCar.GetComponentInChildren<CarAI>();
        _poles = GameObject.FindGameObjectsWithTag("TakeoverPole");
    }
    
    /// <summary>
    /// returns [just passed pole, side of next pole is right]
    /// </summary>
    /// <returns></returns>
    private bool[] NearestPole() { 
        GameObject c = null;
        var md = float.PositiveInfinity;
        foreach (var p in _poles) {
            var d = Vector3.Distance(p.transform.position, _myCarAi.transform.position);
            if (d < md) {
                md = d;
                c = p;
            }
        }

        var dz = _myCarAi.transform.position.z - c.transform.position.z;

        //Debug.Log($"test {c.transform.position}  {_myCarAi.transform.position} {dz}");

        

        return new bool[] {
            dz > 0.5f && dz < 1f,
            c.transform.position.x < _myCarAi.transform.position.x // flipped because next pole is flipped
        };

    }

    private void DoLog(float time, bool mistake, bool success) {
        var mistakeText = mistake ? "turned wrong way first" : "";
        var successText = success ? "success" : "fail";
        System.IO.File.AppendAllText(LOGFILE, $"[{System.DateTime.Now}] [{gameInitializer.game}] [{mistakeText}] [{successText}] [{time}]" + System.Environment.NewLine);
    }

    // Update is called once per frame
    void Update () {

        if (_takeoverEnded) {
            return;
        }

        if (_takeoverStarted) {
            // FIXME: store and average controller *speed* (float)

            var pos = GameObject.Find("[CameraRig]").transform.InverseTransformPoint(
                GameObject.Find("Controller (right)").transform.position);
            _avgSpeed = _avgSpeed * 0.6f + (pos - lastPos).magnitude * 0.4f;
            lastPos = pos;
            var ang = Vector3.Angle(GameObject.Find("Controller (right)").transform.rotation * Vector3.up, Vector3.down);

            var canFinishTakeover = (_avgSpeed < 1e-3 && ang < 10);

            var wheelZ = GameObject.Find("SteeringWheel").transform.rotation.eulerAngles.z;

            // Since only two lanes, we can safely make the assmption that if they turn
            // correctly, they made the right choice.

            var chosenLane = CarAI.LaneSide.None;

            // Swapped because controller is now upside-down

            if (wheelZ > 35 && wheelZ < 120) {
                chosenLane = CarAI.LaneSide.Right;
            }
            if (wheelZ > 250 && wheelZ < 320) {
                chosenLane = CarAI.LaneSide.Left;
            }

            Debug.Log($"{chosenLane} {safeLane} {_avgSpeed} {ang} {canFinishTakeover}");

            if (chosenLane != CarAI.LaneSide.None && chosenLane != safeLane) {
                _mistake = true;
            }

            if (chosenLane == safeLane && canFinishTakeover) {
                _myCarAi.laneChangeProbability = CarAI.DEFAULT_LANE_CHANGE_PROBABILITY;
                takeover.isCurrentTakeover = false;
                osdText.Show(osdText.LookingPosition, OSDText.TextType.Sprite, "TakeoverSuccess");
                DoLog(_time, _mistake, true);
                _takeoverEnded = true;
                return;
            }

            _time += Time.deltaTime;

            if (_time > 10) {
                _myCarAi.laneChangeProbability = CarAI.DEFAULT_LANE_CHANGE_PROBABILITY;
                takeover.isCurrentTakeover = false;
                osdText.Show(osdText.LookingPosition, OSDText.TextType.Sprite, "TakeoverFail");
                DoLog(_time, _mistake, false);
                _takeoverEnded = true;
                return;
            }


        } else {
            bool[] np = NearestPole();
            if (_timeCounter > _setStartTime && np[0]) {
                safeLane = np[1] ? CarAI.LaneSide.Right : CarAI.LaneSide.Left;
                _myCarAi.laneChangeProbability = 0;
                _takeoverStarted = true;
                StartCoroutine(StartTakeover());
            }
            _timeCounter += Time.deltaTime;
        }

    }

    private IEnumerator StartTakeover() {
        // FIXME: play shortnotify sound
        // FIXME: show takeover text on HUD
        
        // Since we don't know the current game, we can't know which OSDText
        // reference to use, so we just use whatever we find.

        osdText.Show(osdText.LookingPosition, OSDText.TextType.Sprite, "Takeover");
        PlaySound(GameObject.Find("MyPlayer"), alertSound);

        yield return new WaitForSeconds(2f);
        takeover.isCurrentTakeover = true;
    }

    public static void PlaySound(GameObject source, AudioClip sound, float vol = 1f) {
        source.GetComponentInChildren<AudioSource>().PlayOneShot(sound, vol);
    }

}
