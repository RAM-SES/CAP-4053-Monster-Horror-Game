﻿using UnityEngine;
using System.Collections;

public class pieSensor : MonoBehaviour {

	public Transform fStart_line1, fEnd_line1;
	public Transform fStart_line2, fEnd_line2;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		Debug.DrawLine (fStart_line1.position, fEnd_line1.position, Color.white);
		Debug.DrawLine (fStart_line2.position, fEnd_line2.position, Color.white);

		Collider2D[] colliders = Physics2D.OverlapCircleAll (transform.position, 1.75f);

		foreach (Collider2D col in colliders) {

			Debug.Log("I see: " + col.name + " At an agle: " + Vector2.Angle(transform.position,col.transform.position));
		}
	}
}
