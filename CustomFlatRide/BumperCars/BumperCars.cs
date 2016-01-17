using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BumperCars : FlatRide
{
    public new enum State
    {
        STOPPED,
        RUNNING
    }
    
    [Serialized] public State currentState;

    private float _time;

    void Start()
    {
        currentState = State.STOPPED;

        Transform cars = transform.Find("Cars");

        foreach (Transform car in cars)
        {
            car.gameObject.AddComponent<BumperCar>();
        }

        base.Start();
    }

    public override void onStartRide()
    {
        base.onStartRide();

        currentState = State.RUNNING;
    }

    public override void tick(StationController stationController)
    {
        if (currentState == State.RUNNING)
        {
            _time += Time.deltaTime;

            if (_time > 25)
            {
                currentState = State.STOPPED;
                _time = 0;
            }
        }
    }
    
    public override bool shouldLetGuestsOut()
    {
        return currentState == State.STOPPED;
    }

    public override bool shouldLetGuestsIn(StationController stationController)
    {
        return currentState == State.STOPPED;
    }
}
