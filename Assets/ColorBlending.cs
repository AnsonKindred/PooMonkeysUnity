using UnityEngine;
using System.Collections;

public class ColorBlending : MonoBehaviour {

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}
	
	public Color RGBtoYUV(Color RGB)
	{
	    float red = RGB.r;
		float green = RGB.g;
		float blue = RGB.b;
	
	    // normalizes red, green, blue values
	    float r = red/255.0f;
	    float g = green/255.0f;
	    float b = blue/255.0f;
		
		
	    float Y = 0.299f*r + 0.587f*g + 0.114f*b;
	    float U = -0.14713f*r -0.28886f*g + 0.436f*b;
	    float V = 0.615f*r -0.51499f*g -0.10001f*b;
		Color YUV = new Color(Y,U,V);
		
	    return YUV;
	}
	
	public Color YUVtoRGB(Color YUV)
	{
	    float y = YUV.r;
		float u = YUV.g;
		float v = YUV.b;
	
	    float Red = (y + 1.139837398373983740f*v)*255f;
	    float Green = (
	        y - 0.3946517043589703515f*u - 0.5805986066674976801f*v)*255f;
	    float Blue = (y + 2.032110091743119266f*u)*255f;
	
		Color RGB = new Color(Red,Green,Blue);
		
	    return RGB;
	}
}
