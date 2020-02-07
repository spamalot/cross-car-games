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
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundPlayer : MonoBehaviour {

    public bool IsPlaying { get; private set; }
    public event Action OnFinish;

    private AudioSource _src;
    
	void Start () {
        _src = GetComponent<AudioSource>();
        IsPlaying = false;

        Invoke("PlayMusic", 0.1f);
    }

    void PlayMusic()
    {
        IsPlaying = true;
        GetComponent<AudioSource>().Play();
        Debug.Log("Playing music...");
    }

    void Update() {
        if (IsPlaying && !_src.isPlaying) {
            IsPlaying = false;
            if (OnFinish != null) {
                OnFinish.Invoke();
            }
            Debug.Log("Music is over!");
        }
	}
}
