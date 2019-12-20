using System;
using UnityEngine;
using UnityEngine.Events;

public class EventManager : Singleton<EventManager> {

	public FoodEvent OnFoodEaten;

	void Start () {
		if (OnFoodEaten == null)
			OnFoodEaten = new FoodEvent();
	}
	
}
