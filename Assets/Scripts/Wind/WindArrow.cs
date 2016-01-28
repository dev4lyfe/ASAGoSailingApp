﻿using UnityEngine;
using System.Collections;

/*
	This class handles the functionality of the flying wind arrows 
*/

public class WindArrow : MonoBehaviour {
//	float spawnTimer;
	Transform player;
	float deathDistance = 80f;
	// Use this for initialization
//	float spawnFrequency = 40f;
	// Use this for initialization
	void Start () {
		player = GameObject.FindGameObjectWithTag ("Player").transform;
//		spawnTimer = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
		transform.Translate(transform.forward*-.6f);
		if (Vector3.Distance(transform.position, player.transform.position) > deathDistance) {
			Destroy(gameObject);
		}
	}
}
