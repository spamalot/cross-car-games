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
using PlayerState = Bang.BangPlayer.PlayerState;

namespace Bang
{
    public class BangController : MonoBehaviour
    {
        public GameObject scoreTextPrefab;
        public GameObject tooSlowTextPrefab;
        public GameObject myPlayer;
        public GameObject bangEnemyControllerPrefab;
        public GameObject bangHumanControllerPrefab;
        public GameObject handPrefab;
        public PlayerDB playerDB;
        public Sprite spIt;
        public Sprite spDuck;
        public Sprite spDuckw;
        public Sprite spPt;
        public Sprite spSl;

        public AudioClip bangSound;
        public AudioClip duckSound;
        public AudioClip wrongActionSound;
        public AudioClip duckingHit;
        public AudioClip notifySound;

        private enum GameState { WaitingIt, WaitingDuckAndPoint, WaitingToBecomeIt };
        private GameState _state;
        private GameObject _it;
        private GameObject _ducking;
        private List<GameObject> _pointing;
        private GameObject _toBecomeIt;

        private const int DUCK_ALLOWED_FRAMES = 240; // (= 4 seconds)

        void Start()
        {

            foreach (var p in playerDB.PlayerNames) {
                if (!p.Key.StartsWith("Enemy")) {
                    continue;
                }
                var hand = Instantiate(handPrefab, p.Value.GetPrefabInterface<TransformableInterface>().transformable);
                var handInterface = p.Value.AddComponent<HandInterface>();
                handInterface.hand = hand;
                p.Value.GetComponent<AdvancedPrefabNester>().AddController(bangEnemyControllerPrefab);
                var score = p.Value.AddComponent<ScoreDisplay>();
                score.prefab = scoreTextPrefab;
                GameObject tooSlowText = Instantiate(tooSlowTextPrefab, p.Value.transform);
                tooSlowText.GetComponent<SpriteFaceCamera>().camera = GameObject.Find("Camera (eye)").GetComponent<Camera>();
            }

            myPlayer = GameObject.Find("MyPlayer");
            var trackerObj = GameObject.Find("Trackers");
            var t1 = trackerObj.AddComponent<ViveRightHandTracker>();
            var t2 = trackerObj.AddComponent<ViveDuckingTracker>();
            myPlayer.GetComponent<Human>().trackers.Add(t1);
            myPlayer.GetComponent<Human>().trackers.Add(t2);
            var myHand = Instantiate(handPrefab);
            myHand.name = "MyHand";
            myHand.AddComponent<HandPositioner>();
            var myHandInterface = myPlayer.AddComponent<HandInterface>();
            myHandInterface.hand = myHand;
            myPlayer.GetComponent<AdvancedPrefabNester>().AddController(bangHumanControllerPrefab);
            GameObject tooSlowText2 = Instantiate(tooSlowTextPrefab, myPlayer.transform);
            tooSlowText2.GetComponent<SpriteFaceCamera>().camera = GameObject.Find("Camera (eye)").GetComponent<Camera>();
            playerDB.PlayerNames["MyPlayer"] = myPlayer;

            //foreach (var p in playerDB.PlayerNames) {
            //    var stateDisplay = p.Value.AddComponent<BangStateDisplay>();
            //    stateDisplay.prefab = dbgico;
            //}

            foreach (var p in playerDB.PlayerNames) {
                p.Value.GetComponent<BangPlayer>().OnDuck += HandleDuck;
                p.Value.GetComponent<BangPlayer>().OnPoint += HandlePoint;
            }

            // FIXME: Choose "it" randomly?, or always just start with player?
            myPlayer.GetComponent<BangPlayer>().State = PlayerState.It;
            _it = myPlayer;
            _state = GameState.WaitingIt;

        }

        private void HandleDuck(BangPlayer player)
        {
            if (player.State == PlayerState.ToDuck) {
                PlaySound(player.gameObject, duckSound);
            } else {
                PlaySound(player.gameObject, wrongActionSound);
            }
        }

        private void HandlePoint(BangPlayer player)
        {
            if (player.State == PlayerState.It || player.State == PlayerState.ToPoint) {
                PlaySound(player.gameObject, bangSound);
            } else {
                PlaySound(player.gameObject, wrongActionSound);
            }
        }

        public static void PlaySound(GameObject source, AudioClip sound, float vol = 1f)
        {
            source.GetComponentInChildren<AudioSource>().PlayOneShot(sound, vol);
        }

        void Update()
        {
            AdvanceStateMachine();
            PenalizeCheaters();
            //DisplayState();
            NotifyEnemiesState();

            ////////////////

            GameObject.Find("Cursor").GetComponent<CursorHover>().BasePos = _it.GetPrefabInterface<TransformableInterface>().transformable.position.y + 6;
            GameObject.Find("Cursor").transform.position = Vector3.MoveTowards(GameObject.Find("Cursor").transform.position, TransformableInterface.OffsetForNotification(_it.GetPrefabInterface<TransformableInterface>().transformable.position) + new Vector3(0, 6, 0), 1f);

            if (_pointing != null) {
                GameObject.Find("ParticleSpew").GetComponent<ParticleAttractor>().attractor1 = _pointing[0];
                GameObject.Find("ParticleSpew").GetComponent<ParticleAttractor>().attractor2 = _pointing[1];
            }

            // FIXME: messy logic
            if (_state == GameState.WaitingIt && _it.GetComponent<BangPlayer>().PointingDirection != Vector3.zero ) {
                if (!GameObject.Find("ParticleBeam").GetComponent<ParticleSystem>().isPlaying) {
                    GameObject.Find("ParticleBeam").GetComponent<ParticleSystem>().Play();
                }
                GameObject.Find("ParticleBeam").transform.position = _it.transform.position;
                GameObject.Find("ParticleBeam").transform.rotation = Quaternion.LookRotation(_it.GetComponent<BangPlayer>().PointingDirection);
            } else if (_state != GameState.WaitingIt && _state != GameState.WaitingToBecomeIt) {
                if (!GameObject.Find("ParticleBeam").GetComponent<ParticleSystem>().isPlaying) {
                    GameObject.Find("ParticleBeam").GetComponent<ParticleSystem>().Play();
                }
                GameObject.Find("ParticleBeam").transform.position = _it.transform.position;
                GameObject.Find("ParticleBeam").transform.rotation = Quaternion.LookRotation(_ducking.transform.position - _it.transform.position);
            } else {
                // Don't render particle beam.
                GameObject.Find("ParticleBeam").GetComponent<ParticleSystem>().Stop();
                GameObject.Find("ParticleBeam").GetComponent<ParticleSystem>().Clear();
            }
            

            if (_state != GameState.WaitingDuckAndPoint) {
                GameObject.Find("ParticleSpew").GetComponent<ParticleSystem>().Stop();
                GameObject.Find("ParticleSpew").GetComponent<ParticleSystem>().Clear();
            } else {
                if (!GameObject.Find("ParticleSpew").GetComponent<ParticleSystem>().isPlaying) {
                    //GameObject.Find("ParticleSpew").transform.parent = _ducking.transform;//.GetComponent<PathFollower>().Velocity;
                    GameObject.Find("ParticleSpew").GetComponent<ParticleSystem>().Play();
                }
                GameObject.Find("ParticleSpew").transform.position = _ducking.transform.position;// + new Vector3(0,8,0);
            }

        }

        private void DisplayState() {
            foreach (var p in playerDB.PlayerNames) {
                // FIXME: improve (transform.Find stuff)
                var pp = p.Value.GetComponent<BangPlayer>();
                //if (pp.State == PlayerState.ToDuck || pp.State == PlayerState.DuckLate) {
                //    p.Value.transform.Find("BangDebugIcon(Clone)").GetComponentInChildren<SpriteRenderer>().sprite = spDuck;
                //} else 
                if (pp.State == PlayerState.DuckWaiting) {
                    p.Value.transform.Find("BangDebugIcon(Clone)").GetComponentInChildren<SpriteRenderer>().sprite = spDuckw;
                //} else if (pp.State == PlayerState.ToPoint) {
                //    p.Value.transform.Find("BangDebugIcon(Clone)").GetComponentInChildren<SpriteRenderer>().sprite = spPt;
                } else if (pp.State == PlayerState.It || pp.State == PlayerState.ItWaiting) {
                    p.Value.transform.Find("BangDebugIcon(Clone)").GetComponentInChildren<SpriteRenderer>().sprite = spIt;
                } else {
                    p.Value.transform.Find("BangDebugIcon(Clone)").GetComponentInChildren<SpriteRenderer>().sprite = spSl;
                }
            }
        }

        private void AdvanceStateMachine()
        {
            if (_state == GameState.WaitingIt) {

                if (_it.GetComponent<BangPlayer>().PointingDirection == Vector3.zero) {
                    return;
                }

                // Determine who It is pointing to, tell them to duck
                _ducking = FindPlayerInDirection(_it);
                _ducking.GetComponent<BangPlayer>().State = PlayerState.ToDuck;

                // Find close players to player to duck, tell them to point
                _pointing = BangPlayer.FindClosePlayers(playerDB, _it, 2, new List<GameObject> { _ducking });
                foreach (var p in _pointing) {
                    p.GetComponent<BangPlayer>().State = PlayerState.ToPoint;
                }

                _state = GameState.WaitingDuckAndPoint;
                _it.GetComponent<BangPlayer>().State = PlayerState.ItWaiting;
                return;
            }

            if (_state == GameState.WaitingDuckAndPoint) {

                if (_ducking.GetComponent<BangPlayer>().Ducking
                        && _ducking.GetComponent<BangPlayer>().State == PlayerState.ToDuck) {
                    //Debug.Log("ducker is fast");
                    _ducking.GetComponent<Scoreable>().Score += 500;
                    _ducking.GetComponent<BangPlayer>().State = PlayerState.DuckWaiting;
                }

                foreach (var p in _pointing) {
                    // i.e. If the player is pointing in the general vicinity
                    // of the ducking player.
                    if (Vector3.Dot(p.GetComponent<BangPlayer>().PointingDirection,
                            _ducking.transform.position - p.transform.position) > 0.8) {

                        //Debug.Log(p.name + " pointed first");
                        p.GetComponent<Scoreable>().Score += 1000;

                        if (_ducking.GetComponent<BangPlayer>().State == PlayerState.ToDuck) {
                            //Debug.Log("ducker too slow!");
                            _ducking.GetComponentInChildren<RiseFromPosition>().From = TransformableInterface.OffsetForNotification(_ducking.GetPrefabInterface<HandInterface>().hand.transform.position);
                            PlaySound(_ducking, duckingHit);
                            _ducking.GetComponent<Scoreable>().Score -= 1000;
                            // If user still tries to duck, will make error sound
                            // instead of duck sound, and points will be deducted.
                            _ducking.GetComponent<BangPlayer>().State = PlayerState.DuckLate;
                        }

                        // Allow the enemies to stop pointing
                        foreach (var q in _pointing) {
                            q.GetComponent<BangPlayer>().State = PlayerState.PointWaiting;
                            if (q != p) {
                                q.GetComponentInChildren<RiseFromPosition>().From = TransformableInterface.OffsetForNotification(q.GetPrefabInterface<HandInterface>().hand.transform.position);
                            }
                        }

                        _toBecomeIt = p;
                        _state = GameState.WaitingToBecomeIt;
                        return;
                    }
                }

                return;
            }

            if (_state == GameState.WaitingToBecomeIt) {

                if (_toBecomeIt.GetComponent<BangPlayer>().PointingDirection == Vector3.zero) {

                    _ducking.GetComponent<BangPlayer>().State = PlayerState.None;
                    foreach (var q in _pointing) {
                        q.GetComponent<BangPlayer>().State = PlayerState.None;
                    }
                    _toBecomeIt.GetComponent<BangPlayer>().State = PlayerState.It;
                    _it.GetComponent<BangPlayer>().State = PlayerState.None;
                    _it = _toBecomeIt;

                    if (_it == myPlayer) {
                        PlaySound(_it, notifySound);
                    }

                    _state = GameState.WaitingIt;
                }
                return;
            }
        }

        private void PenalizeCheaters()
        {
            
            foreach (var p in playerDB.PlayerNames) {
                // Penalize those who are ducking when they shouldn't be.
                if ((p.Value.GetComponent<BangPlayer>().State != PlayerState.ToDuck
                        && p.Value.GetComponent<BangPlayer>().State != PlayerState.DuckWaiting)
                        && p.Value.GetComponent<BangPlayer>().Ducking) {
                    p.Value.GetComponent<Scoreable>().Score -= 2;
                }

                // Penalize those who are pointing when they shouldn't be.
                if ((p.Value.GetComponent<BangPlayer>().State != PlayerState.It
                        && p.Value.GetComponent<BangPlayer>().State != PlayerState.ItWaiting
                        && p.Value.GetComponent<BangPlayer>().State != PlayerState.ToPoint
                        && p.Value.GetComponent<BangPlayer>().State != PlayerState.PointWaiting)
                        && p.Value.GetComponent<BangPlayer>().PointingDirection != Vector3.zero) {
                    //Debug.Log(p.Key + " is pointing too early!");
                    p.Value.GetComponent<Scoreable>().Score -= 2;
                }
            }
        }

        private void NotifyEnemiesState()
        {
            foreach (var p in playerDB.PlayerNames) {
                if (!p.Key.StartsWith("Enemy")) {
                    continue;
                }
                var enemyPlayer = p.Value.GetComponent<BangEnemyPlayer>();
                enemyPlayer.Ducker = _ducking;
                if (_ducking != null) {
                    enemyPlayer.DuckerDucking = _ducking.GetComponent<BangPlayer>().Ducking;
                }
                enemyPlayer.ItReady = _state == GameState.WaitingIt;
            }
        }

        private GameObject FindPlayerInDirection(GameObject fromPlayer)
        {
            GameObject min = null;
            var maxDot = -2f; // Dot product no less than -1.
            foreach (var p in playerDB.PlayerNames) {
                if (p.Value == fromPlayer) {
                    continue;
                }
                var dot = Vector3.Dot(
                    fromPlayer.GetComponent<BangPlayer>().PointingDirection,
                    (p.Value.GetPrefabInterface<TransformableInterface>().transformable.position - fromPlayer.GetPrefabInterface<TransformableInterface>().transformable.position).normalized);
                if (dot > maxDot) {
                    maxDot = dot;
                    min = p.Value;
                }
            }
            //Debug.Log(min);
            return min;
        }



    }
}