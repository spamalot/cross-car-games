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

public class KillerballSpawnOSDCursor : MonoBehaviour {

    public BallGame.Ball ball;
    public Camera hudCamera;
    public Human human;

    void Update () {

        var active = (ball.State == BallGame.Ball.BallState.Normal
            && !human.GetComponent<BallGame.BallThrowerCatcher>().IsHoldingBall);

        GetComponent<SpriteRenderer>().enabled = active;

        if (!active) {
            return;
        }

        var bob = ball.transform.position;
        var thang = hudCamera.WorldToScreenPoint(bob);
        // FIXME: improve; assumes we know the height of the screen is 512!
        var thang2 = new Vector2(thang.x, 200);

        GetComponent<RectTransform>().anchoredPosition = thang2;
    }

}
