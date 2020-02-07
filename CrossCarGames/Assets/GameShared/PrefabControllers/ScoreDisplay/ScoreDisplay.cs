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
using SpamalotExtensions;

[RequireComponent(typeof(Scoreable))]
public class ScoreDisplay : MonoBehaviour
{
    public GameObject prefab;
    public bool isMyPlayer = false;

    private GameObject _scoreText;
    private Scoreable _scoreable;
    private Transform _transformable;
    private GameObject _eye;

    private int _lastScore = 0;
    private int goodTimer = 0;
    private int badTimer = 0;

    public string visibleName;
    private static List<string> availableEnemyNames = new List<string> { "Lightning", "Goliath", "Freddy", "Lord", "Hero", "Phantom", "Rex", "Mobius" };

    void Start() {
        _eye = GameObject.Find("Camera (eye)");
        _transformable = this.GetPrefabInterface<TransformableInterface>().transformable;
        _scoreable = GetComponent<Scoreable>();
        _scoreText = Instantiate(prefab, _transformable);
        _scoreText.GetComponent<SpriteFaceCamera>().camera = _eye.GetComponent<Camera>();
        if (isMyPlayer) {
            visibleName = name;
        } else {
            var idx = Random.Range(0, availableEnemyNames.Count);
            visibleName = availableEnemyNames[idx];
            availableEnemyNames.RemoveAt(idx);
        }
    }

    void Update() {
        var score = _scoreable.Score;
        _scoreText.GetComponent<TextMesh>().text = "<size=32>" + visibleName + "</size>\n" + score.ToString();

        if (isMyPlayer) {

            if (score - _lastScore > 0) {
                goodTimer += 30;
            } else if (score - _lastScore < 0) {
                badTimer += 30;
            }

            if (badTimer > 0) {
                _scoreText.GetComponent<TextMesh>().color = Color.red;
            } else if (goodTimer > 0) {
                _scoreText.GetComponent<TextMesh>().color = Color.green;
            } else {
                _scoreText.GetComponent<TextMesh>().color = Color.white;
            }

            goodTimer = Mathf.Max(0, goodTimer - 1);
            badTimer = Mathf.Max(0, badTimer - 1);

            _lastScore = score;
        }

        if (isMyPlayer) {
            _scoreText.transform.position = _eye.transform.position + _eye.transform.rotation * Vector3.forward * 2.5f;
        } else {
            var basePos = _transformable.position + new Vector3(0, 2.5f, 0);
            _scoreText.transform.position =
                basePos + (_eye.transform.position - basePos).normalized * 2.5f;
        }

        
            //_transformable.position
            //+ new Vector3(0, 2f, 0)
            //+ TransformableInterface.OffsetForNotification(_transformable.position)
            //- (_eye.transform.forward * (cameraOffset + 3f);
        // Last part with the camera eye to make text appear in front of the cursor in general
        //_scoreText.transform.position =
        //   _transformable.position
        //   + new Vector3(0, height, 0)
        //+ TransformableInterface.OffsetForNotification(_transformable.position)
        //;- (_eye.transform.rotation * new Vector3(0, 0, 1)).normalized * 0.9f;

        // Disabled to reduce visual clutter
        //_scoreText.transform.localScale = new Vector3(-1, 1, 1) * 0.16f;
        var a = Mathf.Pow(Vector3.Distance(_eye.transform.position, _scoreText.transform.position) / 40, 0.7f);
        if (!isMyPlayer) {
            a /= 2;
        }
        //var a = Mathf.Min(0.3f, Mathf.Max(0.3f, Vector3.Distance(_eye.transform.position, _scoreText.transform.position)) / 30);
        _scoreText.transform.localScale = new Vector3(-a, a, a);
    }
}
