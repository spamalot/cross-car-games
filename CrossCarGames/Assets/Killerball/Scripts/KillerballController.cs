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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SpamalotExtensions;

namespace BallGame
{
    namespace Killerball
    {
        public class KillerballController : MonoBehaviour
        {
            public GameObject scoreTextPrefab;
            public GameObject ballHumanPrefab;
            public GameObject caughtTextPrefab;
            public GameObject osdCursorPrefab;
            

            public GameObject ballEnemyControllerPrefab;
            public GameObject ballHumanControllerPrefab;
            public GameObject killerballEnemyAimerControllerPrefab;
            public GameObject handControllerPrefab;
            public GameObject scoreDisplayControllerPrefab;

            public Material defaultBallMaterial;
            public Material myBallMaterial;

            public AudioClip respawnSound;
            public AudioClip catchSound;
            public AudioClip throwSound;
            public AudioClip notifySound;
            public AudioClip bonusHitSound;
            public AudioClip bonusCatchSound;
            public AudioClip hitSound;

            public OSDText osdText;
            public PlayerDB playerDB;
            public Human human;
            private GameObject myCar;
            public GameObject trackerObj;

            public event Action<Vector3> OnHumanSelfHit;
            public event Action<Vector3> OnHumanOtherHit;

            private bool _inIntermission = false;

            void Start()
            {
                foreach (var p in playerDB.PlayerNames) {
                    if (!p.Key.StartsWith("Enemy")) {
                        continue;
                    }
                    var pCar = p.Value.GetPrefabInterface<VehicleInterface>().vehicle;
                    p.Value.GetComponent<AdvancedPrefabNester>().AddController(scoreDisplayControllerPrefab);
                    pCar.GetComponent<AdvancedPrefabNester>().AddController(handControllerPrefab);
                    p.Value.GetComponent<AdvancedPrefabNester>().AddController(ballEnemyControllerPrefab);
                    p.Value.GetComponent<AdvancedPrefabNester>().AddController(killerballEnemyAimerControllerPrefab);
                    GameObject caughtText = Instantiate(caughtTextPrefab, p.Value.transform);
                    caughtText.GetComponent<SpriteFaceCamera>().camera = GameObject.Find("Camera (eye)").GetComponent<Camera>();
                }

                myCar = human.GetPrefabInterface<VehicleInterface>().vehicle;
                var t1 = trackerObj.AddComponent<ViveRightHandTracker>();
                human.trackers.Add(t1);
                // FIXME: trash the MyCar finding and access through the player's car inteface
                myCar.GetComponent<AdvancedPrefabNester>().AddController(handControllerPrefab);
                var myHand = myCar.GetPrefabInterface<HandInterface>().hand;
                myHand.name = "MyHand";
                var hp = myHand.AddComponent<HandPositioner>();
                hp.human = human;
                hp.OnRotatedUp += RespawnBallHuman;
                human.GetComponent<AdvancedPrefabNester>().AddController(ballHumanControllerPrefab);
                foreach (var textPrefab in new GameObject[] { caughtTextPrefab }) {
                    GameObject text2 = Instantiate(textPrefab, human.transform);
                    text2.GetComponent<SpriteFaceCamera>().camera = GameObject.Find("Camera (eye)").GetComponent<Camera>();
                }


                // FIXME: move OSD stuff to controller
                var osdi = myCar.GetPrefabInterface<MyOSDInterface>();
                foreach (var ball in GameObject.Find("Balls").GetComponentsInChildren<Ball>()) {
                    foreach (var osd in new MyOSDInterface.OSD [] { osdi.front, osdi.left, osdi.right, osdi.back}) {
                        var x = Instantiate(osdCursorPrefab, osd.canvas.transform).GetComponent<KillerballSpawnOSDCursor>();
                        x.hudCamera = osd.camera;
                        x.ball = ball;
                        x.human = human;
                    }
                }
                // FIXME: Move to 2nd OSD controlller
                osdText.human = human;
                OnHumanSelfHit += y => osdText.Show(y, OSDText.TextType.Sprite, "SelfHit");
                OnHumanOtherHit += y => osdText.Show(y, OSDText.TextType.Sprite, "OtherHit");
                
                // show on looked-at screen
                osdText.Show(osdText.ForwardPosition, OSDText.TextType.Text, hold: true);
                // end OSD stuff

                playerDB.PlayerNames["MyPlayer"] = human.gameObject;

                foreach (var ball in GameObject.Find("Balls").GetComponentsInChildren<Ball>()) {
                    ball.OnDropped += () => Respawn(ball);
                }

                foreach (var p in playerDB.PlayerNames) {
                    p.Value.GetComponent<BallThrowerCatcher>().OnCaught += HandleCaught;
                    p.Value.GetComponent<BallThrowerCatcher>().OnThrown += HandleThrown;
                    p.Value.GetComponent<BallThrowerCatcher>().OnHit += HandleHit;
                    p.Value.GetComponent<BallThrowerCatcher>().IsThrownTo = true;
                }

                StartCoroutine(IntermissionTimer());
            }

            void Update()
            {
                //if (incScoreCounter == 0) {
                    foreach (var p in playerDB.PlayerNames) {
                        p.Value.GetComponent<Scoreable>().Score += 5;
                    }
               // }
               // incScoreCounter = (incScoreCounter + 1) % 3;
            }

            public static void PlaySound(GameObject source, AudioClip sound, float vol = 1f)
            {
                source.GetComponentInChildren<AudioSource>().PlayOneShot(sound, vol);
            }

            private void HandleCaught(BallThrowerCatcher throwerCatcher, GameObject ballObject)
            {
                Ball ball = ballObject.GetComponent<Ball>();
                var thrower = ball.LastHeld;
                if (thrower != null) {
                    PlaySound(throwerCatcher.gameObject, bonusCatchSound);
                    throwerCatcher.transform.Find("CaughtText(Clone)").GetComponent<RiseFromPosition>().From = TransformableInterface.OffsetForNotification(throwerCatcher.GetPrefabInterface<VehicleInterface>().vehicle.GetPrefabInterface<HandInterface>().hand.transform.position);
                    thrower.GetComponent<Scoreable>().Score -= 500;
                    //thrower.GetComponent<BallThrowerCatcher>().Release();
                }

                if (human.GetComponent<BallThrowerCatcher>() == throwerCatcher) {
                    human.VibrateRightHand();
                    ball.GetComponent<Renderer>().material = myBallMaterial;
                } else {
                    ball.GetComponent<Renderer>().material = defaultBallMaterial;
                }

                ball.State = Ball.BallState.Held;
                ball.LastHeld = throwerCatcher;
                PlaySound(throwerCatcher.gameObject, catchSound);
                throwerCatcher.SafeToThrow();
            }

            private void HandleThrown(BallThrowerCatcher throwerCatcher, GameObject ballObject)
            {
                ballObject.GetComponent<Ball>().State = Ball.BallState.Normal;
                PlaySound(throwerCatcher.gameObject, throwSound);
            }

            private void HandleHit(BallThrowerCatcher throwerCatcher, GameObject ballObject)
            {
                Ball ball = ballObject.GetComponent<Ball>();
                if (ball.State == Ball.BallState.Dead
                        || ball.LastHeld == throwerCatcher
                        || ball.LastHeld == null) {
                    return;
                }
                ball.transform.Find("ExplosionParticleSystem(Clone)").GetComponent<ParticleSystem>().Play();
                PlaySound(throwerCatcher.gameObject, hitSound);
                PlaySound(ball.LastHeld.gameObject, bonusHitSound);

                /*var foo = throwerCatcher.transform.Find("SelfHitText(Clone)");
                if (foo != null) {
                    foo.GetComponent<RiseFromPosition>().From = TransformableInterface.OffsetForNotification(throwerCatcher.GetPrefabInterface<VehicleInterface>().vehicle.GetPrefabInterface<HandInterface>().hand.transform.position);
                }

                var bar = ball.LastHeld.transform.Find("OtherHitText(Clone)");
                if (bar != null) {
                    bar.GetComponent<RiseFromPosition>().From = TransformableInterface.OffsetForNotification(ball.LastHeld.GetPrefabInterface<VehicleInterface>().vehicle.GetPrefabInterface<HandInterface>().hand.transform.position);
                }*/

                if (ball.LastHeld == human.GetComponent<BallThrowerCatcher>()) {
                    OnHumanOtherHit.Invoke(ball.transform.position);
                }

                if (throwerCatcher == human.GetComponent<BallThrowerCatcher>()) {
                    OnHumanSelfHit.Invoke(ball.transform.position);
                }
                    
                throwerCatcher.GetComponent<Scoreable>().Score -= 1000;

                if (ball.LastHeld.GetPrefabInterface<VehicleInterface>().vehicle.GetComponentInChildren<CarAI>().VehicleDirection != throwerCatcher.GetPrefabInterface<VehicleInterface>().vehicle.GetComponentInChildren<CarAI>().VehicleDirection) {
                    ball.LastHeld.GetComponent<Scoreable>().Score += 1000;
                }

                ball.State = Ball.BallState.Dead;
                // NOTE: Don't have to explicitly respawn ball -- it will
                // eventually fall out of bounds and respawn itself.
            }

            private void RespawnBallHuman() {
                if (human.GetComponent<BallThrowerCatcher>().IsHoldingBall) {
                    return;
                }
                doMe = true;
            }

            private bool doMe = false;

            private void Respawn(Ball ball)
            {

                if (_inIntermission) {
                    return;
                }

                ball.LastHeld = null;
                ball.State = Ball.BallState.Normal;

                GameObject thrower;


                var throwMe = !human.GetComponent<BallThrowerCatcher>().IsHoldingBall && doMe;

                /*if (throwMe) {
                    var myPos = human.GetPrefabInterface<VehicleInterface>().vehicle.GetPrefabInterface<HandInterface>().hand.transform.position;
                    foreach (var testBall in GameObject.Find("Balls").GetComponentsInChildren<Ball>()) {
                        var dist = Vector3.Distance(myPos, testBall.transform.position);
                        if (dist < 25) {
                            throwMe = false;
                            break;
                        }
                    }
                }*/

                if (throwMe) {
                    doMe = false;
                    thrower = human.gameObject;
                } else {
                    var potentialThrowers =
                        playerDB.PlayerNames.Values.Where(
                            x => !x.GetComponent<BallThrowerCatcher>().IsHoldingBall && x != human.gameObject).ToList();
                    thrower =
                       potentialThrowers[(int)(potentialThrowers.Count * UnityEngine.Random.value * 0.999f)];
                }

                PlaySound(thrower, respawnSound);
                if (thrower == human.gameObject) {
                    PlaySound(thrower, notifySound);
                    ball.GetComponent<Renderer>().material = myBallMaterial;
                } else {
                    ball.GetComponent<Renderer>().material = defaultBallMaterial;
                }

                ball.MakeLanded();
                ball.transform.position =
                    thrower.GetPrefabInterface<VehicleInterface>().vehicle.GetPrefabInterface<HandInterface>().hand.transform.position
                    + new Vector3(0, 0, 0);
                var ac = (thrower == human.gameObject) ? human.gameObject : thrower;
                ball.GetComponent<DeadSimplePhysics>().velocity = ac.GetPrefabInterface<VehicleInterface>().vehicle.GetComponentInChildren<CarAI>().Velocity;
            }


            private IEnumerator IntermissionTimer() {
                while (true) {
                    yield return new WaitForSeconds(60f);
                    yield return StartCoroutine(Intermission());
                }
            }

            private IEnumerator Intermission() {
                _inIntermission = true;
                foreach (var p in playerDB.PlayerNames) {
                    p.Value.GetComponent<BallThrowerCatcher>().Release();
                }
                osdText.Show(osdText.LookingPosition, OSDText.TextType.Sprite, "Intermission");
                yield return new WaitForSeconds(2f);

                osdText.GetComponentInChildren<UnityEngine.UI.Text>().text = AssembleLeaderboard();
                osdText.Show(osdText.ForwardPosition, OSDText.TextType.Text);
                yield return new WaitForSeconds(5f);

                osdText.Show(osdText.ForwardPosition, OSDText.TextType.Sprite, "Resume3");
                yield return new WaitForSeconds(1f);
                osdText.Show(osdText.ForwardPosition, OSDText.TextType.Sprite, "Resume2");
                yield return new WaitForSeconds(1f);
                osdText.Show(osdText.ForwardPosition, OSDText.TextType.Sprite, "Resume1");
                yield return new WaitForSeconds(1f);

                _inIntermission = false;
            }

            private string AssembleLeaderboard() {
                var scores = new Dictionary<string, int>();
                foreach (var p in playerDB.PlayerNames) {
                    if (p.Value.GetPrefabInterface<VehicleInterface>().vehicle.GetComponentInChildren<CarAI>().VehicleDirection != CarAI.Direction.Forward) {
                        continue;
                    }
                    scores[p.Value.GetComponent<ScoreDisplay>().visibleName] = p.Value.GetComponent<Scoreable>().Score;
                }

                var top = scores.OrderByDescending(p => p.Value).Take(5);
                return "<b>Leaderboard</b>\n" + string.Join("\n", top.Select(p => $"{p.Key.PadRight(10)} \t\t{p.Value}"));
            }
        }
    }
}