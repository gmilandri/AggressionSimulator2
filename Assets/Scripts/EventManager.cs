using System;
using UnityEngine;
using UnityEngine.Events;

public class EventManager : Singleton<EventManager> {

	public FoodEvent OnFoodEaten;
	public FoodPopEvent OnPopEaten;

	void Start () {
		if (OnFoodEaten == null)
			OnFoodEaten = new FoodEvent();
		if (OnPopEaten == null)
			OnPopEaten = new FoodPopEvent();
	}
	
}
