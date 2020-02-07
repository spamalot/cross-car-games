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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Human : MonoBehaviour {

    public GameObject leftHandTracker;
    public GameObject rightHandTracker;
    public List<AbstractTracker> trackers = new List<AbstractTracker>();

    public enum HandState { Open, Closed, Unknown };

    public enum ProviderFeature {
        RightHandState, Ducking, RightHandPosition, RightHandVelocity,
        Pointing, GlobalRightHandPosition, GlobalRightHandRotation,
        RightTouching, RightTouchCoords, RightHandVibrate,
    };

    public static readonly Dictionary<ProviderFeature, Type> Providers =
        new Dictionary<ProviderFeature, Type> {
            { ProviderFeature.RightHandState, typeof(AbstractRightHandTracker) },
            { ProviderFeature.Ducking, typeof(AbstractDuckingTracker)},
            { ProviderFeature.RightHandPosition, typeof(AbstractRightHandTracker) },
            { ProviderFeature.RightHandVelocity, typeof(AbstractRightHandTracker) }, // FIXME: is this even used?
            { ProviderFeature.Pointing, typeof(AbstractRightHandTracker) },
            { ProviderFeature.GlobalRightHandPosition, typeof(AbstractRightHandTracker) },
            { ProviderFeature.GlobalRightHandRotation, typeof(AbstractRightHandTracker) },
            { ProviderFeature.RightHandVibrate, typeof(AbstractRightHandTracker) },
            { ProviderFeature.RightTouching, typeof(AbstractRightTouchTracker) },
            { ProviderFeature.RightTouchCoords, typeof(AbstractRightTouchTracker) },
        };

    public bool IsFullyTracked() {
        return trackers.Count > 0 && trackers.All(tracker => tracker.IsTracked(this));
    }

    public AbstractTracker Provider(Type type) {
        return trackers.Find(tracker => tracker.GetType().IsSubclassOf(type));
    }

    #region DISCUSTING_CACHE

    public delegate T CacheFunc<T>();

    private class CacheInfo<T> {
        public int lastRecalc;
        public T cachedVal;
    }

    private Dictionary<object, object> _cache = new Dictionary<object, object>();

    public T Cached<T>(CacheFunc<T> callback) {
        if(!_cache.ContainsKey(callback)) {
            //Debug.Log("don't have key " + callback);
            _cache[callback] = new CacheInfo<T> { lastRecalc = Time.frameCount, cachedVal = callback() };
        }
        if (((CacheInfo<T>)_cache[callback]).lastRecalc != Time.frameCount) {
            //Debug.Log("Recache val " + callback);
            _cache[callback] = new CacheInfo<T> { lastRecalc = Time.frameCount, cachedVal = callback() };
        }
        //Debug.Log("returning cached val " + callback + " " + ((Foo<T>)foo[callback]).cachedVal);
        return ((CacheInfo<T>) _cache[callback]).cachedVal;
    }

    #endregion

    public Vector3 GlobalRightHandPosition {
        get {
            return Cached(() => {
                var provider = (AbstractRightHandTracker)Provider(Providers[ProviderFeature.GlobalRightHandPosition]);
                return provider.GetHumanGlobalRightHandPosition(this);
            });
        }
    }

    public Quaternion GlobalRightHandRotation {
        get {
            return Cached(() => {
                var provider = (AbstractRightHandTracker)Provider(Providers[ProviderFeature.GlobalRightHandRotation]);
                return provider.GetHumanGlobalRightHandRotation(this);
            });
        }
    }

    public Vector3 RightHandVelocity {
        get {
            return Cached(() => {
                var provider = (AbstractRightHandTracker)Provider(Providers[ProviderFeature.RightHandVelocity]);
                return provider.GetHumanRightHandVelocity(this);
            });
        }
    }

    public HandState SmoothedRightHandState {
        get {
            return Cached(() => {
                var provider = (AbstractRightHandTracker)Provider(Providers[ProviderFeature.RightHandState]);
                return provider.GetHumanRightHandState(this);
            });
        }
    }

    private bool _wasDucking = false;
    public bool IsDucking {
        get {
            return Cached(() => {
                var provider = (AbstractDuckingTracker)Provider(Providers[ProviderFeature.Ducking]);
                bool? isDucking = provider.GetHumanDucking(this);
                _wasDucking = isDucking ?? _wasDucking;
                return _wasDucking;
            });
        }
    }

    private bool _wasPointing = false;
    public bool IsPointing {
        get {
            return Cached(() => {
                var provider = (AbstractRightHandTracker)Provider(Providers[ProviderFeature.Pointing]);
                bool? isPointing = provider.GetHumanPointing(this);
                _wasPointing = isPointing ?? _wasPointing;
                return _wasPointing;
            });
        }
    }

    public bool RightTouching {
        get {
            return Cached(() => {
                var provider = (AbstractRightTouchTracker)Provider(Providers[ProviderFeature.RightTouching]);
                return provider.GetRightTouching(this);
            });
        }
    }

    public Vector2 RightTouchCoords {
        get {
            return Cached(() => {
                var provider = (AbstractRightTouchTracker)Provider(Providers[ProviderFeature.RightTouchCoords]);
                return provider.GetRightTouchCoords(this);
            });
        }
    }

    public void VibrateRightHand() {
        var provider = (AbstractRightHandTracker)Provider(Providers[ProviderFeature.RightHandVibrate]);
        provider.VibrateRightHand(this);
    }

}