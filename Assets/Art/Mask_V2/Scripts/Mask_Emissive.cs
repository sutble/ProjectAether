using UnityEngine;
using System.Collections;

public class Mask_Emissive : MonoBehaviour {


	public float wL = 2.0f;

	Renderer emissiveRender = null;

	Material emissiveMat = null;

	//private float t = 5.0f;
	private float p = 0.0f;
	private float pInc = 0.0f;
	private float baseColor = 0.1f;

	void Start (){

		emissiveRender = GetComponent<Renderer> ();
		emissiveMat = emissiveRender.material;
		CycleChange ();
	}


	// Update is called once per frame
	void Update () {

		EmissionCharge ();
	}


	void CycleChange (){
		pInc = (2 * Mathf.PI / Random.Range(wL, wL + 2.0f)) * Time.deltaTime;
	}

	void EmissionCharge (){

		Color nextColor = new Color(0.0f * baseColor, 0.2f * baseColor, 1.5f * baseColor);
		//purple #s are 0.76f, 0.0f, 1.56f
		//blue #s are 0.0f, 0.2f, 1.5f

		p += pInc;
		baseColor = Mathf.Abs(Mathf.Sin (p));

		emissiveMat.SetColor ("_EmissionColor", nextColor);
		DynamicGI.UpdateMaterials (emissiveRender);
		DynamicGI.UpdateEnvironment ();

		if (baseColor <= 0.01f)
			CycleChange ();

	}
}
