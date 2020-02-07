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
using System.IO;
using UnityEngine;
using SpamalotExtensions;

namespace BallGame
{
    namespace HotPotato
    {
        public class HotPotatoController : MonoBehaviour
        {
            public GameObject scoreTextPrefab;
            public GameObject ballHumanPrefab;
            public GameObject potato;
            private GameObject myPlayer;
            public GameObject ballEnemyControllerPrefab;
            public GameObject ballHumanControllerPrefab;
            public GameObject hotPotatoEnemyAimerControllerPrefab;
            public GameObject handPrefab;

            public AudioClip respawnSound;
            public AudioClip catchSound;
            public AudioClip throwSound;
            public AudioClip notifySound;
            public AudioClip bonusSound;

            private PlayerDB playerDB;

            private List<BallThrowerCatcher> order = new List<BallThrowerCatcher>();
            private BallThrowerCatcher thrower;
            private BallThrowerCatcher throwTo;
            private bool respawned = false;

            private Dictionary<string, int> scores;

            void Start()
            {
                playerDB = GameObject.Find("PlayerDB").GetComponent<PlayerDB>();

                foreach (var p in playerDB.PlayerNames) {
                    if (!p.Key.StartsWith("Enemy")) {
                        continue;
                    }
                            
                    var hand = Instantiate(handPrefab, p.Value.GetPrefabInterface<TransformableInterface>().transformable);
                    var handInterface = p.Value.AddComponent<HandInterface>();
                    handInterface.hand = hand;
                    p.Value.GetComponent<AdvancedPrefabNester>().AddController(ballEnemyControllerPrefab);
                    p.Value.GetComponent<AdvancedPrefabNester>().AddController(hotPotatoEnemyAimerControllerPrefab);
                    var score = p.Value.AddComponent<ScoreDisplay>();
                    score.prefab = scoreTextPrefab;
                   
                }

                myPlayer = GameObject.Find("MyPlayer");
                var trackerObj = GameObject.Find("Trackers");
                var t1 = trackerObj.AddComponent<ViveRightHandTracker>();
                myPlayer.GetComponent<Human>().trackers.Add(t1);
                var myHand = Instantiate(handPrefab);
                myHand.name = "MyHand";
                myHand.AddComponent<HandPositioner>();
                var myHandInterface = myPlayer.AddComponent<HandInterface>();
                myHandInterface.hand = myHand;
                myPlayer.GetComponent<AdvancedPrefabNester>().AddController(ballHumanControllerPrefab);
                playerDB.PlayerNames["MyPlayer"] = myPlayer;

                RegenerateThrowOrder();
                HandleCaught(myPlayer.GetComponent<BallThrowerCatcher>(), null);
                respawned = true;

                //GameObject.Find("Enemy1").GetComponent<HotPotatoThrowerCatcher>().IsThrownTo = true;
                //GameObject.Find("Cursor").transform.position = GameObject.Find("Enemy1").GetComponent<Locatable>().EffectivePosition + new Vector3(0, GameObject.Find("Enemy1").transform.lossyScale.y * 2f, 0);
                potato.GetComponent<Ball>().OnDropped += respawn;

                // FIXME: for loop this
                //GameObject.Find("Enemy1").GetComponent<HotPotatoThrowerCatcher>().OnCaught += HandleCaught;
                GameObject.Find("Enemy2").GetComponent<BallThrowerCatcher>().OnCaught += HandleCaught;
                GameObject.Find("Enemy2").GetComponent<BallThrowerCatcher>().OnThrown += HandleThrown;
                //GameObject.Find("Enemy3").GetComponent<BallThrowerCatcher>().OnCaught += HandleCaught;
                //GameObject.Find("Enemy4").GetComponent<BallThrowerCatcher>().OnCaught += HandleCaught;
                GameObject.Find("Enemy5").GetComponent<BallThrowerCatcher>().OnCaught += HandleCaught;
                GameObject.Find("Enemy5").GetComponent<BallThrowerCatcher>().OnThrown += HandleThrown;
                //GameObject.Find("Enemy6").GetComponent<BallThrowerCatcher>().OnCaught += HandleCaught;
                GameObject.Find("Enemy7").GetComponent<BallThrowerCatcher>().OnCaught += HandleCaught;
                GameObject.Find("Enemy7").GetComponent<BallThrowerCatcher>().OnThrown += HandleThrown;
                GameObject.Find("Enemy8").GetComponent<BallThrowerCatcher>().OnCaught += HandleCaught;
                GameObject.Find("Enemy8").GetComponent<BallThrowerCatcher>().OnThrown += HandleThrown;
                //GameObject.Find("Enemy9").GetComponent<HotPotatoThrowerCatcher>().OnCaught += HandleCaught;
                //GameObject.Find("Enemy10").GetComponent<BallThrowerCatcher>().OnCaught += HandleCaught;
                GameObject.Find("Enemy11").GetComponent<BallThrowerCatcher>().OnCaught += HandleCaught;
                GameObject.Find("Enemy11").GetComponent<BallThrowerCatcher>().OnThrown += HandleThrown;
                myPlayer.GetComponent<BallThrowerCatcher>().OnCaught += HandleCaught;
                myPlayer.GetComponent<BallThrowerCatcher>().OnThrown += HandleThrown;

                GameObject.Find("MusicMaker").GetComponent<SoundPlayer>().OnFinish += HandleMusicFinish;

                GameObject.Find("MissedText").GetComponent<SpriteFaceCamera>().camera = GameObject.Find("Camera (eye)").GetComponent<Camera>();

                // FIXME: do the same for all enemy vehicles
                //GameObject.Find("MyCar").GetComponentInChildren<CollisionDetector>().SetCollisionsEnabled(GameObject.Find("Potato").GetComponent<Collider>(), false);
            }

            private void HandleMusicFinish()
            {
                GameObject.Find("Canvas2").GetComponent<Canvas>().enabled = true;
                GameObject.Find("FinalScore").GetComponent<UnityEngine.UI.Text>().text = "Time's up!\nScore: " + myPlayer.GetComponent<Scoreable>().Score.ToString();
            }

            private void respawn()
            {
                if (!respawned) {

                    //if (thrower.name == "HotPotatoHuman") {
                    //    File.AppendAllText(@"ball_dists.log", "missed " + thrower.ThrowTo.MinBallDistance + "\n");
                    //}

                    PlaySound(thrower.gameObject, respawnSound);

                    if (thrower.ThrowTo.MinBallDistance < 2) {
                        GameObject.Find("MissedText").GetComponent<RiseFromPosition>().From = TransformableInterface.OffsetForNotification(throwTo.GetPrefabInterface<HandInterface>().hand.transform.position);
                        thrower.ThrowTo.GetComponent<Scoreable>().Score -= 500;
                    } else {
                        GameObject.Find("MissedText").GetComponent<RiseFromPosition>().From = TransformableInterface.OffsetForNotification(thrower.GetPrefabInterface<HandInterface>().hand.transform.position);
                        thrower.GetComponent<Scoreable>().Score -= 500;
                    }
                }

                if (thrower.gameObject == myPlayer) {
                    PlaySound(thrower.gameObject, notifySound);
                }


                respawned = true;
                throwTo = thrower;
                thrower.IsThrownTo = true;

                potato.GetComponent<Ball>().MakeLanded();
                potato.transform.position = thrower.GetPrefabInterface<HandInterface>().hand.transform.position + new Vector3(0, 3, 0);
                var ac = (thrower.gameObject == myPlayer) ? GameObject.Find("MyCar") : thrower.gameObject;
                potato.GetComponent<DeadSimplePhysics>().velocity = ac.GetComponentInChildren<CarAI>().Velocity;
            }

            private void HandleCaught(BallThrowerCatcher throwerCatcher, GameObject ball)
            {
                if (!respawned) {

                    if (throwerCatcher.gameObject == myPlayer && thrower != null) {
                        File.AppendAllText(@"ball_dists.log", "caught " + thrower.ThrowTo.MinBallDistance + "\n");
                    }

                    // Potentially recompute throw order based on current positioning. Won't ever appear to change who the current person is throwing to.
                    if (Random.value < 0.1 && throwTo != null) {
                        RegenerateThrowOrder();
                        throwTo.IsThrownTo = false;
                    }

                    if (thrower != null) {
                        // Force thrower to give up the ball in case they're still holding it.
                        thrower.Release();

                        PlaySound(thrower.gameObject, bonusSound);
                        thrower.GetComponent<Scoreable>().Score += 1000;
                    }
                }
                respawned = false;

                PlaySound(throwerCatcher.gameObject, catchSound);


                thrower = throwerCatcher;
                throwTo = order[(order.IndexOf(throwerCatcher) + 1) % order.Count];

                throwTo.MinBallDistance = float.PositiveInfinity;

                thrower.ThrowTo = throwTo;
                thrower.IsThrownTo = false;
                throwTo.IsThrownTo = true;
                thrower.SafeToThrow();
            }

            private void HandleThrown(BallThrowerCatcher throwerCatcher, GameObject ball)
            {
                PlaySound(throwerCatcher.gameObject, throwSound);
            }

            public static void PlaySound(GameObject source, AudioClip sound, float vol = 1f)
            {
                source.GetComponentInChildren<AudioSource>().PlayOneShot(sound, vol);
            }

            private void Update()
            {
                GameObject.Find("Cursor").GetComponent<CursorHover>().BasePos = throwTo.GetPrefabInterface<TransformableInterface>().transformable.position.y + 6;
                GameObject.Find("Cursor").transform.position = Vector3.MoveTowards(GameObject.Find("Cursor").transform.position, TransformableInterface.OffsetForNotification(throwTo.GetPrefabInterface<TransformableInterface>().transformable.position) + new Vector3(0, 6, 0), 1f);
                if (!thrower.IsThrown && thrower.ThrowTo == throwTo && Random.value < 0.2) {
                    thrower.GetComponent<Scoreable>().Score -= 1;
                }

                //GameObject.Find("DebugText").GetComponent<UnityEngine.UI.Text>().text = throwTo.name + " " + throwTo.MinPotatoDistance.ToString() + " "+ throwTo.GetComponent<Locatable>().EffectivePosition.ToString() + " " + GameObject.Find("Potato").transform.position.ToString();
            }

            private void RegenerateThrowOrder()
            {
                // Use Nearest-Neighbor algorithm for solving TSP problem.
                //
                // If this is no good, change to DP, Christofides', etc.

                order.Clear();

                var left = new List<string>(playerDB.PlayerNames.Keys);

                order.Add(GameObject.Find(left[0]).GetComponent<BallThrowerCatcher>());
                left.RemoveAt(0);

                // FIXME: start using dict values so we don't have to call GameObject.Find(...)
                while (left.Count > 0) {
                    var minDist = float.PositiveInfinity;
                    string min = null;
                    foreach (var eachLeft in left) {
                        var dist = (order[order.Count - 1].GetPrefabInterface<TransformableInterface>().transformable.position - GameObject.Find(eachLeft).GetPrefabInterface<TransformableInterface>().transformable.position).magnitude;
                        if (dist < minDist) {
                            minDist = dist;
                            min = eachLeft;
                        }
                    }
                    order.Add(GameObject.Find(min).GetComponent<BallThrowerCatcher>());
                    left.Remove(min);
                }

            }

        }
    }
}