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
        if (currentState != State.RUNNING)
        {
            StartCoroutine(RunRide());
        }
    }

    private IEnumerator RunRide()
    {
        currentState = State.RUNNING;
       
        yield return new WaitForSeconds(30);

        currentState = State.STOPPED;
    }

    public override bool shouldLetGuestsOut()
    {
        return currentState == State.STOPPED;
    }
}
