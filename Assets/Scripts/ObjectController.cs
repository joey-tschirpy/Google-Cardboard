using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectController : MonoBehaviour {

    [SerializeField] GameObject leftOrb;
    [SerializeField] GameObject rightOrb;

    Vector3 originalScale;

	// Use this for initialization
	void Start () {
        leftOrb.SetActive(false);
        rightOrb.SetActive(false);

        originalScale = transform.localScale;
    }

    public void onHoverTextButton()
    {
        transform.localScale = transform.localScale * 1.1f;
        GetComponent<TextMesh>().color = new Color(0.9f, 0.9f, 0.9f);

        leftOrb.SetActive(true);
        rightOrb.SetActive(true);
    }

    public void onExitTextButton()
    {
        if (transform.GetComponent<BoxCollider>().enabled)
        {
            transform.localScale = originalScale;
            GetComponent<TextMesh>().color = new Color(0.8f, 0.0f, 0.0f);
        }

        leftOrb.SetActive(false);
        rightOrb.SetActive(false);
    }
}
