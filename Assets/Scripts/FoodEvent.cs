﻿using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class FoodEvent : UnityEvent<Vector2>
{

}

public class FoodPopEvent : UnityEvent<int>
{

}