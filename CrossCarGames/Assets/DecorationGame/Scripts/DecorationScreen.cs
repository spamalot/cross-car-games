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

public class DecorationScreen : PhoneScreen {

    public Sprite background;
    public ViveTouchGestureRecognizer gestureRecognizer;
    public Human human;

    public GameObject noDecorationPrefab;

    public Sprite topIcon;
    public Sprite frontIcon;
    public Sprite backIcon;
    public Sprite bottomIcon;
    public Sprite sidesIcon;

    public SpriteRenderer positionSpriteRenderer;
    public SpriteRenderer decorationSpriteRenderer;
    public GameObject mirrorObject;

    private CarDecorator decorator;

    private Dictionary<CarDecorator.DecorationSlot, int> indices = new Dictionary<CarDecorator.DecorationSlot, int>();

    private int positionIndex = 0;

    private const int SLOT_COUNT = 5;


    void Start() {
        decorator = human.GetComponent<CarDecorator>();
        foreach (var slot in decorator.Slots) {
            indices[slot] = 0;
        }
        ApplyCurrentPosition();
    }

    /// <summary>
    /// For when a new decoration is unlocked.
    /// </summary>
    public void ApplyLastDecoration(CarDecorator.DecorationSlot slot) {
        var decorations = decorator.AvailableDecorations[slot];
        indices[slot] = decorations.Count - 1;
        ApplyCurrentDecoration(slot, 0);
    }

    private void ApplyCurrentDecoration(CarDecorator.DecorationSlot slot, int incr) {
        var decorations = decorator.AvailableDecorations[slot];
        indices[slot] = (indices[slot] + incr + decorations.Count) % decorations.Count;
        human.GetComponent<CarDecorator>().SetDecoration(slot, decorations[indices[slot]]);

        // Only show sprite on screen if this is the current slot
        CarDecorator.DecorationSlot slot2 = CarDecorator.DecorationSlot.None;
        switch (positionIndex) {
            case 0: slot2 = CarDecorator.DecorationSlot.Top; break;
            case 1: slot2 = CarDecorator.DecorationSlot.Front; break;
            case 2: slot2 = CarDecorator.DecorationSlot.Back; break;
            case 3: slot2 = CarDecorator.DecorationSlot.Bottom; break;
            case 4: slot2 = CarDecorator.DecorationSlot.Sides; break;
        }

        if (slot == slot2) {
            decorationSpriteRenderer.sprite = decorations[indices[slot]].GetComponent<CarDecoration>().UIIcon;
        }
    }

    private void ApplyCurrentPosition() {
        switch (positionIndex) {
            case 0: positionSpriteRenderer.sprite = topIcon; ApplyCurrentDecoration(CarDecorator.DecorationSlot.Top, 0);  break;
            case 1: positionSpriteRenderer.sprite = frontIcon; ApplyCurrentDecoration(CarDecorator.DecorationSlot.Front, 0); break;
            case 2: positionSpriteRenderer.sprite = backIcon; ApplyCurrentDecoration(CarDecorator.DecorationSlot.Back, 0); break;
            case 3: positionSpriteRenderer.sprite = bottomIcon; ApplyCurrentDecoration(CarDecorator.DecorationSlot.Bottom, 0); break;
            case 4: positionSpriteRenderer.sprite = sidesIcon; ApplyCurrentDecoration(CarDecorator.DecorationSlot.Sides, 0); break;
        }
    }

    private void Gestured(ViveTouchGestureRecognizer.GestureType gestureType, Vector2 pos) {
        if (gestureType == ViveTouchGestureRecognizer.GestureType.SwipeLeft) {
            controller.ChangeScreen("rating");
            return;
        }

        if (pos.x > 0) {
            CarDecorator.DecorationSlot slot = CarDecorator.DecorationSlot.None;
            switch (positionIndex) {
                case 0: slot = CarDecorator.DecorationSlot.Top; break;
                case 1: slot = CarDecorator.DecorationSlot.Front; break;
                case 2: slot = CarDecorator.DecorationSlot.Back; break;
                case 3: slot = CarDecorator.DecorationSlot.Bottom; break;
                case 4: slot = CarDecorator.DecorationSlot.Sides; break;
            }
            
            if (gestureType == ViveTouchGestureRecognizer.GestureType.SwipeDown) {
                ApplyCurrentDecoration(slot, 1);
            }
            if (gestureType == ViveTouchGestureRecognizer.GestureType.SwipeUp) {
                ApplyCurrentDecoration(slot, -1);
            }
        } else {
            if (gestureType == ViveTouchGestureRecognizer.GestureType.SwipeDown) {
                positionIndex = (positionIndex + 1) % SLOT_COUNT;
            }
            if (gestureType == ViveTouchGestureRecognizer.GestureType.SwipeUp) {
                positionIndex = (positionIndex - 1 + SLOT_COUNT) % SLOT_COUNT;
            }
            ApplyCurrentPosition();
        }
    }

    void OnEnable() {
        controller.SetBackground(background);
        positionSpriteRenderer.enabled = true;
        decorationSpriteRenderer.enabled = true;
        mirrorObject.SetActive(true);
        gestureRecognizer.OnGestured += Gestured;
    }

    void OnDisable() {
        gestureRecognizer.OnGestured -= Gestured;
        positionSpriteRenderer.enabled = false;
        decorationSpriteRenderer.enabled = false;
        mirrorObject.SetActive(false);
    }
}
