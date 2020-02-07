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

public class ParticleAttractor : MonoBehaviour
{
    public GameObject attractor1;
    public GameObject attractor2;
    public ParticleSystem particleSystemObject;

    //public GameObject Owner { get; set; }

    private ParticleSystem.Particle[] _particles = new ParticleSystem.Particle[200];

    void Start()
    {
        particleSystemObject = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        if (attractor1 == null || attractor2 == null) {
            //Debug.Log("Doing nothing");
            return;
        }

        var oldState = Random.state;
        Random.InitState(42);

        //if (particleSystemObject.isPlaying) {
        int length = particleSystemObject.GetParticles(_particles);
        var attractor1Pos = attractor1.transform.position;
        var attractor2Pos = attractor2.transform.position;

        for (int i = 0; i < length; i++) {
            //if (i == 0) {
            //    Debug.Log(_particles[i].startLifetime.ToString() + " " + _particles[i].remainingLifetime.ToString());
            // }
            //_particles[i].startLifetime
            //if (Random.value < 0.5) {
            //    continue;
            //}
            
            var pos = ((i % 2 == 0) ? attractor1Pos : attractor2Pos) + new Vector3(Random.value * 2 - 1, Random.value * 2 -1, Random.value * 2 -1);
            //_particles[i].velocity = Vector3.zero;
            //if ((pos - _particles[i].position).magnitude < 0.5) {
            //    _particles[i].startSize = 0.001f;
            //}
            _particles[i].position = _particles[i].position + (pos - _particles[i].position).normalized * ((_particles[i].startLifetime - _particles[i].remainingLifetime) / _particles[i].startLifetime) + new Vector3(0, _particles[i].remainingLifetime / _particles[i].startLifetime / 10f, 0);
        }
        particleSystemObject.SetParticles(_particles, length);
        //}

        Random.state = oldState;

    }
}