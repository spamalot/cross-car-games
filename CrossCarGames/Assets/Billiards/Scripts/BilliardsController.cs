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
using System.Linq;
using UnityEngine;
using SpamalotExtensions;

public class BilliardsController : MonoBehaviour {

    public NearbyPlayerHelper nearbyPlayerHelper;

    public GameObject scoreTextPrefab;
    public GameObject myPlayer;
    public GameObject billiardPrefab;
    public GameObject osdCursorPrefab;

    public GameObject shieldControllerPrefab;

    public AudioClip triggerSound;
    public AudioClip bounceSound;
    public AudioClip targetHitSound;
    public AudioClip newRoundSound;
    public AudioClip bonusSound;

    public PlayerDB playerDB;
    public GameObject trackerObj;
    public GameObject cursor;

    public GameObject screen;
    public OSDText osdText;

    private bool firstRun = true;

    private int currentBallCount = 0;
    private int lastBallCount = 0;

    private List<GameObject> assistPlayers = new List<GameObject>();

    private List<BilliardBouncer> forwardPlayers;

    void Start() {
        
        foreach (var p in playerDB.PlayerNames) {
            if (!p.Key.StartsWith("Enemy")) {
                continue;
            }
            var pCar = p.Value.GetPrefabInterface<VehicleInterface>().vehicle;
            pCar.GetComponent<AdvancedPrefabNester>().AddController(shieldControllerPrefab);
            pCar.GetPrefabInterface<ShieldInterface>().applicableScorable = p.Value.GetComponent<Scoreable>();
            var score = p.Value.AddComponent<ScoreDisplay>();
            score.prefab = scoreTextPrefab;
            p.Value.AddComponent<BilliardStash>();
            var bouncer = p.Value.AddComponent<BilliardBouncer>();
            var enemyBeh = p.Value.AddComponent<BilliardsEnemy>();
            enemyBeh.billiardPrefab = billiardPrefab;
            enemyBeh.billiardsController = this;
            enemyBeh.bouncer = bouncer;
        }

        var myCar = myPlayer.GetPrefabInterface<VehicleInterface>().vehicle;
        var t1 = trackerObj.AddComponent<ViveRightHandTracker>();
        var t2 = trackerObj.AddComponent<ViveRightTouchTracker>();
        myPlayer.GetComponent<Human>().trackers.Add(t1);
        myPlayer.GetComponent<Human>().trackers.Add(t2);
        myPlayer.AddComponent<BilliardStash>();
        var myBb = myPlayer.AddComponent<BilliardBouncer>();

        myBb.OnLaunchModeChanged += OnMyLaunchModeChange;

        myCar.GetComponent<AdvancedPrefabNester>().AddController(shieldControllerPrefab);
        myCar.GetPrefabInterface<ShieldInterface>().applicableScorable = myPlayer.GetComponent<Scoreable>();
        var myShield = myCar.GetPrefabInterface<ShieldInterface>().shield.gameObject;

        // FIXME: this is not the place for this!
        myShield.GetComponentInChildren<Renderer>().enabled = false;

        myShield.name = "MyShield";
        var hp = myShield.AddComponent<HandPositioner>();
        hp.human = myPlayer.GetComponent<Human>();

        //FIXME: move OSD stuff to controller
        foreach (var side in (BilliardsOSDCursor.Position[]) System.Enum.GetValues(typeof(BilliardsOSDCursor.Position))) {
            var x = Instantiate(osdCursorPrefab).GetComponent<BilliardsOSDCursor>();
            x.human = myPlayer.GetComponent<Human>();
            x.arrowPosition = side;
        }
        //end OSD stuff

        playerDB.PlayerNames["MyPlayer"] = myPlayer;

        forwardPlayers = playerDB.PlayerNames
            .Where(p => p.Value.GetPrefabInterface<VehicleInterface>().vehicle.GetComponentInChildren<CarAI>().VehicleDirection == CarAI.Direction.Forward)
            .Select(p => p.Value.GetComponent<BilliardBouncer>())
            .ToList();

        foreach (var bb in forwardPlayers) {
            bb.OnTriggered += HandleTriggered;
        }

    }

    private void OnMyLaunchModeChange(BilliardBouncer.Mode mode) {
        if (mode == BilliardBouncer.Mode.Bouncer) {
            osdText.Show(osdText.LookingPosition, OSDText.TextType.Sprite, "NewRoundBounce", hold: true);
            screen.GetComponent<PhoneScreenController>().ChangeScreen("bounce");
        } else if (mode == BilliardBouncer.Mode.Launcher) {
            osdText.Show(osdText.LookingPosition, OSDText.TextType.Sprite, "NewRoundLaunch", hold: true);
            screen.GetComponent<PhoneScreenController>().ChangeScreen("launch");
        }
    }

    private void StartNewRound() {
        assistPlayers.Clear();
        var launcher = forwardPlayers[Random.Range(0, forwardPlayers.Count)];
        //launcher = myPlayer.GetComponent<BilliardBouncer>();
        //Debug.Log("the launcher is " + launcher);
        foreach (var bb in forwardPlayers) {
            bb.CurrentMode = bb == launcher ?
                BilliardBouncer.Mode.Launcher : BilliardBouncer.Mode.Bouncer;
        }
        // FIXME: todo: notify new round with sprite for my player
        PlaySound(myPlayer, newRoundSound);
    }

    public GameObject GenerateTarget(GameObject sourcePlayer) {
        return nearbyPlayerHelper.FindInRangePlayer(sourcePlayer, sameSide: true);
    }

    public void HandleTriggered(GameObject player, GameObject ball) {
        // if it's the target player, give a bonus
        // TODO move to controller
        var bbounce = ball.GetComponent<BilliardBounce>();
        if (player == bbounce.TargetPlayer) {
            ball.GetPrefabInterface<BilliardParticleSystemInterface>().glowRing.Play();
            PlaySound(player.gameObject, targetHitSound);
            if (bbounce.SourcePlayer == myPlayer) {
                PlaySound(bbounce.SourcePlayer, bonusSound);
            }
            var bounceScore = 1000 * (1 << (bbounce.BounceCount - 1));
            bbounce.SourcePlayer.GetComponent<Scoreable>().Score += bounceScore;
            foreach (var assistPlayer in assistPlayers) {
                assistPlayer.GetComponent<Scoreable>().Score += bounceScore / 2 + 500;
                if (bbounce.SourcePlayer == myPlayer) {
                    PlaySound(assistPlayer, bonusSound);
                }
            }
            if (player.GetComponent<BilliardBouncer>().IsBouncing) {
                player.GetComponent<Scoreable>().Score += bounceScore /2 + 500;
                BounceEffects(player, ball);
            }
            bbounce.State = BilliardBounce.StateType.Dead;
        } else {
            if (player.GetComponent<BilliardBouncer>().IsBouncing) {
                BounceEffects(player, ball);
                assistPlayers.Add(player);
            } else {
                PlaySound(player.gameObject, triggerSound);
            }
        }
    }

    private void BounceEffects(GameObject player, GameObject ball) {
        ball.transform.Find("BouncedEffect").GetComponent<ParticleSystem>().Play();
        if (player == myPlayer) {
            myPlayer.GetComponent<Human>().VibrateRightHand();
        }
        PlaySound(player.gameObject, bounceSound);
    }

    void Update() {

        // i.e. if the user just dismissed the instructions
        if (firstRun && screen.GetComponent<WaitScreen>().enabled) {
            firstRun = false;
            StartNewRound();
        }


        currentBallCount = GameObject.Find("Balls").transform.childCount;

        if (currentBallCount == 0 && lastBallCount == 1) {
            StartNewRound();
        }

        lastBallCount = currentBallCount;

        if (myPlayer.GetComponent<BilliardBouncer>().CurrentLaunchTargetPlayer != null) {
            cursor.GetComponent<SpriteRenderer>().enabled = true;
            cursor.GetComponent<CursorHover>().BasePos = myPlayer.GetComponent<BilliardBouncer>().CurrentLaunchTargetPlayer.GetPrefabInterface<TransformableInterface>().transformable.position.y + 6;
            cursor.transform.position = TransformableInterface.OffsetForNotification(myPlayer.GetComponent<BilliardBouncer>().CurrentLaunchTargetPlayer.GetPrefabInterface<TransformableInterface>().transformable.position) + new Vector3(0, 6,0);
        } else {
            cursor.GetComponent<SpriteRenderer>().enabled = false;
        }

        /*if (Random.value < 0.01) {
            foreach (var p in playerDB.PlayerNames) {
                p.Value.GetComponent<BilliardStash>().BilliardCount++;
            }
        }*/
    }

    public static void PlaySound(GameObject source, AudioClip sound, float vol = 1f) {
        source.GetComponentInChildren<AudioSource>().PlayOneShot(sound, vol);
    }

}
