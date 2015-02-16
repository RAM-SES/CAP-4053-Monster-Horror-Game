﻿using UnityEngine;
using System.Collections;

public class cone_front_trigger : MonoBehaviour {

	public int isInfront = 0;

	void OnTriggerEnter2D(Collider2D other){
		//Debug.Log ("Object Entered the trigger");
		isInfront = 1;
	}
	void OnTriggerExit2D(Collider2D other) {
		//Debug.Log ("Object Exited the trigger");
		isInfront = 0;
	}
}
