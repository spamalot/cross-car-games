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

public class NewDecalScreen : PhoneScreen {

    public Sprite background;
    public Human human;
    public string nextScreenName;

    public SpriteRenderer decorationSpriteRenderer;

    private bool wait = true;

    //private CarDecorator decorator;

    void Update() {
        if (wait && human.RightTouching) {
            return;
        }
        wait = false;
        if (human.RightTouching) {
            controller.ChangeScreen(nextScreenName);
        }
    }

    void OnEnable() {
        controller.SetBackground(background);
        decorationSpriteRenderer.enabled = true;
        wait = true;
    }

    void OnDisable() {
        decorationSpriteRenderer.enabled = false;
    }
}
