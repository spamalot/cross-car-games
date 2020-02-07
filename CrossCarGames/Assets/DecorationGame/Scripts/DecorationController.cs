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

using UnityEngine;
using SpamalotExtensions;

public class DecorationController : MonoBehaviour {

    public GameObject decorationMountPrefab;

    public GameObject raycastRatingControllerPrefab;
    public GameObject scoreDisplayControllerPrefab;
    public GameObject decorationLevellerControllerPrefab;

    public PhoneScreenController phoneScreenController;

    public AudioClip levelSound;
    public AudioClip newDecorationSound;
    public AudioClip zeroKarmaSound;


    public OSDText osdText;
    public PlayerDB playerDB;
    public GameObject trackerObj;
    private GameObject myPlayer;

    void Start() {

        foreach (var p in playerDB.PlayerNames) {
            if (!p.Key.StartsWith("Enemy")) {
                continue;
            }
            var pCar = p.Value.GetPrefabInterface<VehicleInterface>().vehicle;

            p.Value.GetComponent<AdvancedPrefabNester>().AddController(scoreDisplayControllerPrefab);

            var decorator2 = p.Value.AddComponent<CarDecorator>();

            var ti2 = pCar.GetPrefabInterface<TransformableInterface>().transformable;
            // FIXME: improve
            var foo2 = Instantiate(decorationMountPrefab, ti2);
            decorator2.parentObj = foo2.transform;

            p.Value.AddComponent<DecorationEnemy>();
            p.Value.GetComponent<AdvancedPrefabNester>().AddController(decorationLevellerControllerPrefab);
            //Instantiate(selectHighlightPrefab, p.Value.GetPrefabInterface<TransformableInterface>().transformable);
            
            pCar.GetComponent<AdvancedPrefabNester>().AddController(raycastRatingControllerPrefab);
        }

        myPlayer = GameObject.Find("MyPlayer");
        var myCar = myPlayer.GetPrefabInterface<VehicleInterface>().vehicle;
        var t1 = trackerObj.AddComponent<ViveRightHandTracker>();
        var t2 = trackerObj.AddComponent<ViveRightTouchTracker>();
        myPlayer.GetComponent<Human>().trackers.Add(t1);
        myPlayer.GetComponent<Human>().trackers.Add(t2);
        var karmaCounter = myPlayer.AddComponent<DecorationKarmaCounter>();
        karmaCounter.OnKarmaZero += HandlePlayerKarmaZero;
        var decorator = myPlayer.AddComponent<CarDecorator>();
        
        var ti = myCar.GetPrefabInterface<TransformableInterface>().transformable;
        // FIXME: improve
        var foo = Instantiate(decorationMountPrefab, ti);
        decorator.parentObj = foo.transform;
        
        myPlayer.GetComponent<AdvancedPrefabNester>().AddController(decorationLevellerControllerPrefab);
        myCar.GetComponent<AdvancedPrefabNester>().AddController(raycastRatingControllerPrefab);
        myPlayer.GetComponent<DecorationLeveller>().OnNewDecoration += (go) => HandleNewDecoration(myPlayer, go);

        playerDB.PlayerNames["MyPlayer"] = myPlayer;

        foreach (var p in playerDB.PlayerNames) {
            // FIXME: not the greatest, but it works
            p.Value.GetPrefabInterface<VehicleInterface>().vehicle.GetPrefabInterface<BodyCollidableInterface>().bodyCollider.GetComponent<RaycastRating>().OnRated += r => HandleRate(p.Value, r);
        }
    }

    private void HandleRate(GameObject player, RaycastRating.RatingType rating) {
        player.GetComponent<Scoreable>().Score = Mathf.Max(0, player.GetComponent<Scoreable>().Score
            + ((rating == RaycastRating.RatingType.Happy) ? 1 : -1));
        if (rating == RaycastRating.RatingType.Happy) {
            PlaySound(player, levelSound);
            if (player == myPlayer) {
               osdText.Show(osdText.LookingPosition, OSDText.TextType.Sprite, "LevelUp");
            }
        }
    }

    public void HandleNewDecoration(GameObject player, GameObject decal) {

        StartCoroutine(_ApplyLastDecal(decal.GetComponent<CarDecoration>().slot));

        if (GameObject.Find("InstructionText").GetComponent<UnityEngine.UI.Text>().enabled) {
            return;
        }

        phoneScreenController.ChangeScreen("newdecal");
        GameObject.Find("NewDecalSpriteRenderer").GetComponent<SpriteRenderer>().sprite = decal.GetComponent<CarDecoration>().UIIcon;

        
        PlaySound(player, newDecorationSound);
        myPlayer.GetComponent<Human>().VibrateRightHand();

    }
    private System.Collections.IEnumerator _ApplyLastDecal(CarDecorator.DecorationSlot slot) {
        // Need to wait one frame because the CarDecorator has to categorize the decal
        // into the right slot, which takes one frame.
        yield return null;
        phoneScreenController.GetComponent<DecorationScreen>().ApplyLastDecoration(slot);
    }

    private void HandlePlayerKarmaZero() {
        myPlayer.GetPrefabInterface<VehicleInterface>().vehicle.GetPrefabInterface<BodyCollidableInterface>().bodyCollider.GetComponent<RaycastRating>().Rating = RaycastRating.RatingType.Sad;
        osdText.Show(osdText.LookingPosition, OSDText.TextType.Sprite, "KarmaZero");
        PlaySound(myPlayer, zeroKarmaSound);
    }

    public static void PlaySound(GameObject source, AudioClip sound, float vol = 1f) {
        source.GetComponentInChildren<AudioSource>().PlayOneShot(sound, vol);
    }

}
