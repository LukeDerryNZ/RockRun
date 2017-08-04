using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuPanelScroller : MonoBehaviour {

    public RawImage rawImage;
    private float i;
	public float xAmp = 0.1f;
	public float yAmp = 0.2f;
	public float ox = 0.2f;
	public float oy = 0.4f;

	// Use this for initialization
	void Start () {
        
        rawImage = gameObject.transform.GetComponent<RawImage>();
		i += Random.Range( 0.0f, 100.0f );

	}
	

	// Update is called once per frame
	void Update () {

        i += Time.deltaTime;
        float x = xAmp * Mathf.Cos( ox * i);
		float y = yAmp * Mathf.Sin( oy * i );// Mathf.Abs(yAmp * Mathf.Sin( oy * i ) );
        Rect r = new Rect( x, y, 1.0f, 1.0f );
        rawImage.uvRect = r;

	}
}
