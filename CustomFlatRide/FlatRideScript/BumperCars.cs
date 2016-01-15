﻿using BumperCars.CustomFlatRide.BumperCars;
using UnityEngine;

namespace BumperCars.CustomFlatRide.FlatRideScript
{
    public class BumperCars : FlatRide
    {
        public new enum State
        {
            Stopped,
            Running
        }
    
        [Serialized]
        public State CurrentState;

        [Serialized]
        private float _time;

        public AudioClip Tune;

        void Start()
        {
            guestsCanRaiseArms = false;

            CurrentState = State.Stopped;

            Transform cars = transform.Find("Cars");

            foreach (Transform car in cars)
            {
                car.gameObject.AddComponent<BumperCar>();
            }

            AudioSource audio = gameObject.AddComponent<AudioSource>();

            audio.clip = Tune;
            audio.playOnAwake = true;
            audio.loop = true;
            audio.spatialBlend = 1;
            audio.rolloffMode = AudioRolloffMode.Linear;
            audio.maxDistance = 75;
            audio.volume = 0.1f;

            base.Start();
        }

        public override void onStartRide()
        {
            base.onStartRide();

            CurrentState = State.Running;
        }

        public override void tick(StationController stationController)
        {
            if (CurrentState == State.Running)
            {
                _time += Time.deltaTime;

                if (_time > 25)
                {
                    CurrentState = State.Stopped;
                    _time = 0;
                }
            }
        }
    
        public override bool shouldLetGuestsOut()
        {
            return CurrentState == State.Stopped;
        }
    }
}
