using UnityEngine;
using System.Collections;

public class Cell : MonoBehaviour {

	public string playerName = "";

	public Color color = new Color (1, 1, 1);

	public bool active = true;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		gameObject.GetComponent<Renderer> ().material.color = color;
	}
}
