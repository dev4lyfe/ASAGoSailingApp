﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages the Apparent Wind module.
/// </summary>
public class ApparentWindModuleManager : MonoBehaviour {
	public static ApparentWindModuleManager s_instance;
	public enum GameState { Intro, Playing, Complete };

	[System.NonSerialized]
	public bool hasClickedRun;
	[System.NonSerialized]
	public string currAnimState;

	public GameState gameState = GameState.Intro;
	public Vector3 directionOfWind = new Vector3 (1f,0,1f);
	/// <summary>
	/// The mast position used when computing apparent wind arrows.
	/// </summary>
	public Transform mastRendererPosition;
	/// <summary>
	/// The position where the boat velocity arrow will start from.
	/// </summary>
	public Transform boatVelocityRendererOrigin;
	/// <summary>
	/// The position where the wind speed arrow will start from.
	/// </summary>
	public Transform windLineRendererOrigin;
	public Camera mainCamera;
	public Transform lowWindCameraPos;
	public Transform highWindCameraPos;
	public GameObject[] instructionPanels;

	private ApparentWindBoatControl apparentWindBoatControl;
	private int currentInstructionPanel = 0;
	private float boatVelocityRendererOffsetScalar = 2.2f;
	private float lowWindSpeedRendererOffset = 8f;
	private float highWindSpeedRendererOffset = 14f;
	private bool highWindSpeed = false;
	public bool cameraIsLerping = false;

	void Awake() {
		if (s_instance == null) {
			s_instance = this;
		}
		else {
			Destroy(gameObject);
			Debug.LogWarning( "Deleting "+ gameObject.name +" because it is a duplicate ApparentWindModuleManager." );
		}
	}

	void Start() {
		if( gameState == GameState.Intro )
			instructionPanels[0].SetActive( true );

		if( mainCamera == null )
			mainCamera = Camera.main;

		if( lowWindCameraPos == null )
			lowWindCameraPos = GameObject.Find( "LowWindCameraPos" ).transform;
		if( highWindCameraPos == null )
			highWindCameraPos = GameObject.Find( "HighWindCameraPos" ).transform;

		apparentWindBoatControl = ApparentWindBoatControl.s_instance;
	}

	void Update() {
		switch( gameState ) {
		case GameState.Intro:
			if( (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended ) || Input.GetMouseButtonDown( 0 ) ) {
				instructionPanels[currentInstructionPanel].SetActive( false );

				if( currentInstructionPanel == instructionPanels.Length-1 ) {
					ChangeState( GameState.Playing );
					return;
				}

				currentInstructionPanel++;
				instructionPanels[currentInstructionPanel].SetActive( true );
			}
			break;
		case GameState.Playing:
			break;
		}
	}

	void LateUpdate() {
		// Update boat velocity renderer's position
		boatVelocityRendererOrigin.position = mastRendererPosition.position + apparentWindBoatControl.myRigidbody.velocity*boatVelocityRendererOffsetScalar;

		// Update wind line renderer's position
		Vector3 newOffset = windLineRendererOrigin.position;
		if( !highWindSpeed ) {
			newOffset = boatVelocityRendererOrigin.position + ( Vector3.forward * lowWindSpeedRendererOffset );
		} else {
			newOffset = boatVelocityRendererOrigin.position + ( Vector3.forward * highWindSpeedRendererOffset );
		}
		windLineRendererOrigin.position = newOffset;

		// Update line renderers
		boatVelocityRendererOrigin.GetComponent<ConnectLineRenderer>().UpdatePosition();
		windLineRendererOrigin.GetComponent<ConnectLineRenderer>().UpdatePosition();
		mastRendererPosition.GetComponent<ConnectLineRenderer>().UpdatePosition();
	}

	public void ChangeState( GameState newState ) {
		switch( newState ) {
		case GameState.Intro:
			break;
		case GameState.Playing:
			foreach( GameObject panel in instructionPanels )
				panel.SetActive( false );
			break;
		case GameState.Complete:
			// Do completion animation
			// After tell GM to change level
			break;
		}
		gameState = newState;
	}

	/// <summary>
	/// Action taken when the GUI "Done" button is pressed.
	/// </summary>
	public void DoneButton() {
		ConfirmationPopUp.s_instance.InitializeConfirmationPanel( "move on to the next level?", (bool confirmed) => {
			if( confirmed == true ) {
				Debug.Log( "Accepted to go to next level." );
				ChangeState( GameState.Complete );
			} else {
				Debug.Log( "Declined to go to next level." );
			}
		});
	}

	/// <summary>
	/// Lerps the camera.
	/// </summary>
	/// <param name="lerpToHighWindPos">If set to <c>true</c> lerps to high wind position.</param>
	private void LerpCamera( bool lerpToHighWindPos ) {
		StartCoroutine( LerpCameraToPos( lerpToHighWindPos, 0.35f ) );
	}

	/// <summary>
	/// Lerps the camera to position.
	/// </summary>
	/// <returns>The camera to position.</returns>
	/// <param name="lerpToHighWindPos">If set to <c>true</c> lerps to high wind position from low wind position.</param>
	/// <param name="duration">Duration of lerp in seconds.</param>
	private IEnumerator LerpCameraToPos( bool lerpToHighWindPos, float duration ) {
		Vector3 startPos, endPos;
		if( lerpToHighWindPos ) {
			startPos = lowWindCameraPos.position;
			endPos = highWindCameraPos.position;
		} else {
			startPos = highWindCameraPos.position;
			endPos = lowWindCameraPos.position;
		}

		float lerpDuration = duration;
		float lerpTimer = 0f;

		while( lerpTimer < lerpDuration ) {
			if( (lerpTimer/lerpDuration) >= 0.99f ) {
				mainCamera.transform.position = endPos;
				break;
			}

			mainCamera.transform.position = Vector3.Slerp( startPos, endPos, lerpTimer/lerpDuration );
			yield return null;
			lerpTimer += Time.deltaTime;
		}

		cameraIsLerping = false;
	}

	/// <summary>
	/// Actions taken when the Wind Speed Toggle Button is pressed in the scene.
	/// </summary>
	public void WindSpeedToggleButton() {
		if( !cameraIsLerping ) {
			cameraIsLerping = true;
			// If we are currently in high wind speed, lerp to low wind camera position
			if( highWindSpeed )
				LerpCamera( false );
			else
				LerpCamera( true );

			highWindSpeed = !highWindSpeed;
		}
	}

	/// <summary>
	/// Action taken when the GUI pause button is pressed.
	/// </summary>
	public void PauseButton() {
		//TODO Tell GameManager to show pause menu.
	}
}
