using UnityEngine;

namespace BumperCars.CustomFlatRide.BumperCars
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
        public float Time;

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

            if (!GetComponent<BuildableObject>().isPreview || !GetComponent<BuildableObject>().dontSerialize)
            {
                audio.clip = Tune;
                audio.playOnAwake = true;
                audio.loop = true;
                audio.spatialBlend = 0.9f;
                audio.rolloffMode = AudioRolloffMode.Linear;
                audio.maxDistance = 40;
                audio.volume = 0.07f;
                audio.Play();
            }
            else
            {
                audio.playOnAwake = false;
                audio.volume = 0;
            }

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
                Time += UnityEngine.Time.deltaTime;

                if (Time > 60)
                {
                    CurrentState = State.Stopped;
                    Time = 0;
                }
            }
        }
    
        public override bool shouldLetGuestsOut()
        {
            return CurrentState == State.Stopped;
        }
    }
}
