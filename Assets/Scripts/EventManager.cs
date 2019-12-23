using System;
using UnityEngine;
using UnityEngine.Events;

public class EventManager : Singleton<EventManager> {

	public FoodPopEvent OnPopEaten;

	void Start () {
		if (OnPopEaten == null)
			OnPopEaten = new FoodPopEvent();
	}
	
}
