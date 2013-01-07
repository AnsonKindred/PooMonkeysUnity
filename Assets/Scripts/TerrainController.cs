//TODO: why did i have to make the constructor for my P&I and BREAKOBJECT public
//points includes bottom left of screen and bottom right of terrain as points so you dont want to use the first and last index of points when determining if the linesegment intersects with explosion circle
//me thinks general rule set for break and addition is to:
//find outer most breaks and only use those(i dont think you can get it only one part within another, the whole thing is always inside, not just part?)
//go through break list where you start at highest indexes working backwards so as not to disrupt indexes and BREAK THAT SHIT UP
//after the single break, add points accordingly(backwards), it will be start index?
//GENERAL IDEA: if the point at end index is not on circle edge, make a point at it and then go straight down until you are on the circles edge and add points until you reach and end of that break around the circle until at end index Xvalue, then place a point at the start index and it should all be dandy dancy cotton go fuck yourself

//1-6-13 check out 4 segs, i think everything is actually working, when terrainBuild is invalid you are going to want to draw a line from startbreak to end break, then check if it intersects with any lineSegments from Points[], if it does you draw a line from the start to the intersect and break from intersect to end
using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;


[ExecuteInEditMode]



public class TerrainController : MonoBehaviour 
{
	float THICKNESS = .1f;
	float segmentWidth;
	public int numSegments;
	public float width;
	public float height;
	float explosionRadius = 13.0f;
	
	float EPSILON = .0001f;
	
	public GameObject PooChainClone;
	
	public int warpTime;
	public float warpScale;
	List<Vector2> points = new List<Vector2>();
	GameObject[] segments;
	
	public GameObject vectorFieldObject;
	
	bool doneGenerating = false;
	
	
	// Use this for initialization
	void Start () 
	{
		segmentWidth = width/numSegments;
	 	bool success = false;
	 	int count = 0;
	 	while(success == false && count < 10)
	 	{
       		if(count > 0) Debug.Log("Terrain generation failed, trying again");
			_generate();
			success = buildTerrainMesh();
			count++;
		}
	}
	
	float Sgn (float tempDY)
	{
		if (tempDY < 0)
		{
			return -1.0f;
		}
		else
		{
			return 1.0f;
		}
	}
	
	// Update is called once per frame
	void Update()
	{	//figure out how to tell if circle intersection is tangent cause it'll f everything up, you want to ignore the single
		//circle intersection
		// indexes selected are the ones you want to destroy
		
		
		if (Input.GetMouseButtonDown(0))
		{			
			Vector3 mousePositionTemp = Camera.mainCamera.ScreenToWorldPoint(Input.mousePosition);
			Vector2 mousePosition;
			mousePosition.x = mousePositionTemp.x;
			mousePosition.y = mousePositionTemp.y;
			List<PointAndIndex> firstPassCircleIntersects = new List<PointAndIndex>();
			List<List<PointAndIndex>> fullMagicList = new List<List<PointAndIndex>>();
			List<BreakObject> newBreakList = new List<BreakObject>();
			List<PointAndIndex> leftExplosionIntersects = new List<PointAndIndex>();
			List<PointAndIndex> rightExplosionIntersects = new List<PointAndIndex>();			
			
			//first pass through Points, determines all Left, Right, and Circle Intersections
			for (int i = 0; i < points.Count - 2; i++)
			{
				float leftExplosionRadiusX = mousePosition.x - explosionRadius;
				float rightExplosionRadiusX = mousePosition.x + explosionRadius;
				float percentageAcrossRightX = (rightExplosionRadiusX - points[i].x) / (points[i + 1].x - points[i].x);
				float percentageAcrossLeftX = (leftExplosionRadiusX - points[i].x) / (points[i + 1].x - points[i].x);
				
				//<= and >= might need to be switched, so you get correct indexes
				//left circle upwards Intersections
				if (((points[i].x <= leftExplosionRadiusX && points[i + 1].x > leftExplosionRadiusX) || (points[i].x > leftExplosionRadiusX && points[i + 1].x <= leftExplosionRadiusX)) && (points[i + 1].y - points[i].y) * percentageAcrossLeftX + points[i].y > mousePosition.y)
				{
					Debug.Log("leftSide");
					float intersectLeftX = leftExplosionRadiusX;
					float intersectLeftY = ((leftExplosionRadiusX - points[i].x) / (points[i + 1].x - points[i].x)) * (points[i + 1].y - points[i].y) + points[i].y;
					// if even Count
					if (leftExplosionIntersects.Count % 2.0 == 0.0)
					{
						leftExplosionIntersects.Add(new PointAndIndex(new Vector2(intersectLeftX, intersectLeftY), i + 1));
					}
					else //if odd Count
					{
						leftExplosionIntersects.Add(new PointAndIndex(new Vector2(intersectLeftX, intersectLeftY), i + 1));
					}
				}
				//right circle upwards Intersections
				if (((points[i].x <= rightExplosionRadiusX && points[i + 1].x > rightExplosionRadiusX) || (points[i].x > rightExplosionRadiusX && points[i + 1].x <= rightExplosionRadiusX)) && (points[i + 1].y - points[i].y) * percentageAcrossRightX + points[i].y > mousePosition.y)
				{
					Debug.Log("rightSide");
					float intersectRightX = rightExplosionRadiusX;
					float intersectRightY = ((rightExplosionRadiusX - points[i].x) / (points[i + 1].x - points[i].x)) * (points[i + 1].y - points[i].y) + points[i].y;
					// if even Count
					if (rightExplosionIntersects.Count % 2.0 == 0.0)
					{
						rightExplosionIntersects.Add(new PointAndIndex(new Vector2(intersectRightX, intersectRightY), i));
					}
					else //if odd Count
					{
						rightExplosionIntersects.Add(new PointAndIndex(new Vector2(intersectRightX, intersectRightY), i));
					}
				}
				
				
				//line segment Circle intersections
				float x0 = points[i].x - mousePosition.x;
				float y0 = points[i].y - mousePosition.y;
				float x1 = points[i+1].x - mousePosition.x;
				float y1 = points[i+1].y - mousePosition.y;
				float r = explosionRadius;
				
				
				bool firstIsOnLineSegment = false;
				bool secondIsOnLineSegment = false;
				
				float dX = x1 - x0;
				float dY = y1 - y0;
				float dR = Mathf.Sqrt (Mathf.Pow (dX, 2) + Mathf.Pow (dY, 2));
				float d = x0*y1 - x1*y0;
				float incidence = r*r * dR*dR - d*d;
				float resultingX1 = 0;
				float resultingX2 = 0;
				float resultingY1 = 0;
				float resultingY2 = 0;
				
				if (incidence > 0)
				{
					//why do the opposite signs go together?
					//Debug.Log ("incidence > 0");
					resultingX1 = (d * dY + Sgn(dY) * dX * Mathf.Sqrt (r*r * dR*dR - d*d)) / (dR*dR);
					resultingY1 = -(d * dX - Mathf.Abs (dY) * Mathf.Sqrt (r*r * dR*dR - d*d)) / (dR*dR);
					resultingX2 = (d * dY - Sgn(dY) * dX * Mathf.Sqrt (r*r * dR*dR - d*d)) / (dR*dR);
					resultingY2 = -(d * dX + Mathf.Abs (dY) * Mathf.Sqrt (r*r * dR*dR - d*d)) / (dR*dR);
					
					float distance01 = Mathf.Sqrt((resultingX1 - x0) * (resultingX1 - x0) + (resultingY1 - y0) * (resultingY1 - y0));
					float distance02 = Mathf.Sqrt((resultingX2 - x0) * (resultingX2 - x0) + (resultingY2 - y0) * (resultingY2 - y0));
					//float distance11 = Mathf.Sqrt((resultingX1 - x1) * (resultingX1 - x1) + (resultingY1 - y1) * (resultingY1 - y1));
					//float distance12 = Mathf.Sqrt((resultingX2 - x1) * (resultingX2 - x1) + (resultingY2 - y1) * (resultingY2 - y1));
					
					bool D1greaterthanD2 = false;
					if (distance01 > distance02)
					{
						D1greaterthanD2 = true;
						float tempResultX = resultingX1;
						float tempResultY = resultingY1;
						resultingX1 = resultingX2;
						resultingY1 = resultingY2;
						resultingX2 = tempResultX;
						resultingY2 = tempResultY;
					}
//					Debug.Log("ox0 " + x0);
//					Debug.Log("oy0 " + y0);
//					Debug.Log("ox1 " + x1);
//					Debug.Log("oy1 " + y1);
////					//Debug.Log("r " + r);
//					Debug.Log("rx1 " + resultingX1);
//					Debug.Log("ry1 " + resultingY1);
//					Debug.Log("rx2 " + resultingX2);
//					Debug.Log("ry2 " + resultingY2);
				
					//maybe when land goes in left direction this needs to be reworked?
					//might need to be just > and < not =
					if ((resultingX1 >= x0 && resultingX1 <= x1 && resultingY1 >= y0 && resultingY1 <= y1) || (resultingX1 <= x0 && resultingX1 >= x1 && resultingY1 <= y0 && resultingY1 >= y1) || (resultingX1 <= x0 && resultingX1 >= x1 && resultingY1 >= y0 && resultingY1 <= y1) || (resultingX1 >= x0 && resultingX1 <= x1 && resultingY1 <= y0 && resultingY1 >= y1))
					{
						Debug.Log("firstResult");
						firstIsOnLineSegment = true; //since the resulting points dont fall within the lineSegment
											Debug.Log("ox0 " + x0);
					Debug.Log("oy0 " + y0);
					Debug.Log("ox1 " + x1);
					Debug.Log("oy1 " + y1);
//					//Debug.Log("r " + r);
					Debug.Log("rx1 " + resultingX1);
					Debug.Log("ry1 " + resultingY1);
					Debug.Log("rx2 " + resultingX2);
					Debug.Log("ry2 " + resultingY2);
						if (D1greaterthanD2)
						{
							Debug.Log ("distance01>distance02");
						}
					}
					if ((resultingX2 >= x0 && resultingX2 <= x1 && resultingY2 >= y0 && resultingY2 <= y1) || (resultingX2 <= x0 && resultingX2 >= x1 && resultingY2 <= y0 && resultingY2 >= y1) || (resultingX2 <= x0 && resultingX2 >= x1 && resultingY2 >= y0 && resultingY2 <= y1) || (resultingX2 >= x0 && resultingX2 <= x1 && resultingY2 <= y0 && resultingY2 >= y1))
					{
						Debug.Log("secondResult");
						secondIsOnLineSegment = true; //since the resulting points dont fall within the lineSegment
											Debug.Log("ox0 " + x0);
					Debug.Log("oy0 " + y0);
					Debug.Log("ox1 " + x1);
					Debug.Log("oy1 " + y1);
//					//Debug.Log("r " + r);
					Debug.Log("rx1 " + resultingX1);
					Debug.Log("ry1 " + resultingY1);
					Debug.Log("rx2 " + resultingX2);
					Debug.Log("ry2 " + resultingY2);
							if (D1greaterthanD2)
						{
							Debug.Log ("distance01>distance02");
						}
					}
				}
				//if two Intersections
				//if both land on same linesegment then later on it gets fixed to delete point 666
				if (firstIsOnLineSegment && secondIsOnLineSegment)
				{
					Debug.Log("boobs1");
					// if Count is even
					if (firstPassCircleIntersects.Count % 2.0 == 0.0)
					{
						firstPassCircleIntersects.Add(new PointAndIndex(new Vector2(resultingX1 + mousePosition.x, resultingY1 + mousePosition.y), i + 1));
						firstPassCircleIntersects.Add(new PointAndIndex(new Vector2(resultingX2 + mousePosition.x, resultingY2 + mousePosition.y), i + 1));
					}
					//if Count is odd
					else
					{
						Debug.Log ("jew");
						firstPassCircleIntersects.Add(new PointAndIndex(new Vector2(resultingX1 + mousePosition.x, resultingY1 + mousePosition.y), i));
						firstPassCircleIntersects.Add(new PointAndIndex(new Vector2(resultingX2 + mousePosition.x, resultingY2 + mousePosition.y), i));
					}
				}
				// if only first Intersection
				if (firstIsOnLineSegment && !secondIsOnLineSegment)
				{
					Debug.Log("boobs2");
					if (firstPassCircleIntersects.Count % 2.0 == 0.0)
					{
						firstPassCircleIntersects.Add(new PointAndIndex(new Vector2(resultingX1 + mousePosition.x, resultingY1 + mousePosition.y), i + 1));
					}
					else
					{
						firstPassCircleIntersects.Add(new PointAndIndex(new Vector2(resultingX1 + mousePosition.x, resultingY1 + mousePosition.y), i));
					}
				}
				//if only second intersection
				if (!firstIsOnLineSegment && secondIsOnLineSegment)
				{
					Debug.Log("boobs3");
					if (firstPassCircleIntersects.Count % 2.0 == 0.0)
					{
						firstPassCircleIntersects.Add(new PointAndIndex(new Vector2(resultingX2 + mousePosition.x, resultingY2 + mousePosition.y), i + 1));
					}
					else
					{
						firstPassCircleIntersects.Add(new PointAndIndex(new Vector2(resultingX2 + mousePosition.x, resultingY2 + mousePosition.y), i));
					}
				}
			}
			
			for (int i = 0; i < firstPassCircleIntersects.Count; i++)
			{
				Debug.Log ("firstpassi" + i + " " + firstPassCircleIntersects[i].point);
			}
			
			
			//had to do 666 thing because below this it cant use unassigned even though it would never go into that loop unless it has already gone through this one
			PointAndIndex lowestLeftPoint = new PointAndIndex(new Vector2(666,666), 666);
			PointAndIndex lowestRightPoint = new PointAndIndex(new Vector2(666,666), 666);
			//if leftExplosionIntersects are odd number above, set lowest Point
			if (leftExplosionIntersects.Count % 2.0 != 0.0)//leftexplosionintersects.count > 0
			{
				lowestLeftPoint = leftExplosionIntersects[0];
				for (int j = 1; j < leftExplosionIntersects.Count; j++)
				{
					if (leftExplosionIntersects[j].point.y < lowestLeftPoint.point.y)
					{
						lowestLeftPoint = leftExplosionIntersects[j];
					}
				}
				Debug.Log ("left" + lowestLeftPoint.index);
			}
			//if rightExplosionIntersects are odd number above, set lowest Point
			if (rightExplosionIntersects.Count % 2.0 != 0.0)
			{
				lowestRightPoint = rightExplosionIntersects[0];
				for (int k = 1; k < rightExplosionIntersects.Count; k++)
				{
					if (rightExplosionIntersects[k].point.y < lowestRightPoint.point.y)
					{
						lowestRightPoint = rightExplosionIntersects[k];
					}
				}
				Debug.Log ("right" + lowestRightPoint.index);
			}
			
			
			//if circle is completely in between two indexes on points
			if (lowestRightPoint.index != 666 && lowestLeftPoint.index !=666 && lowestRightPoint.index < lowestLeftPoint.index)
			{
				//lowestRightPoint.index = 666; //want it to be itself foraccess later durring breaks
				//lowestLeftPoint.index = 666;
				//adds these to break list, will have to deal with indexes being 666 when breaking, basically will be
				//no break and will instead make points at points specified and then circle blah blah
				newBreakList.Add (new BreakObject(lowestLeftPoint, lowestRightPoint));
				deletePoints (newBreakList);
				return;
			}
			
			//if no circleIntersects, then it's an easy break and quit
			if (firstPassCircleIntersects.Count == 0)
			{
				if (leftExplosionIntersects.Count % 2.0 == 0.0 && rightExplosionIntersects.Count % 2.0 == 0.0)
				{
					return;
				}
				else
				{
					newBreakList.Add (new BreakObject(lowestLeftPoint, lowestRightPoint));
					deletePoints (newBreakList);
					return;
				}
			}

			
			
			GameObject PooChain = (GameObject)Instantiate(PooChainClone, new Vector3(lowestLeftPoint.point.x, lowestLeftPoint.point.y, -6.0f),Quaternion.identity);
			GameObject PooChain44 = (GameObject)Instantiate(PooChainClone, new Vector3(lowestRightPoint.point.x, lowestRightPoint.point.y, -6.0f),Quaternion.identity);
			//PooChain.rigidbody.isKinematic = true;
			//PooChain44.rigidbody.isKinematic = true;
			PooChain.renderer.material.color = Color.Lerp (Color.blue, Color.blue, 1.0f);
			PooChain44.renderer.material.color = Color.Lerp (Color.blue, Color.blue, 1.0f);
			PooChain.transform.localScale -= new Vector3(1.0f,1.0f,1.0f);
			PooChain44.transform.localScale -= new Vector3(1.0f,1.0f,1.0f);
			
			
			// make a magicList for each circleIntersection fullMagicList
			for (int l = 0; l < firstPassCircleIntersects.Count; l++)
			{
				GameObject PooChain11 = (GameObject)Instantiate(PooChainClone, new Vector3(firstPassCircleIntersects[l].point.x, firstPassCircleIntersects[l].point.y, -9.0f),Quaternion.identity);
				PooChain11.renderer.material.color = Color.Lerp (Color.green, Color.red, 1.0f);
				PooChain11.transform.localScale -= new Vector3(1.0f,1.0f,1.0f);
				fullMagicList.Add(new List<PointAndIndex>());
			}
			
			//second pass through Points, finding "magic" points
//			//might want to only use magic points if its the top half of the circle thats intersected, the way it is now is if any
//			//circle intersection has a magic point ...not too sure...
			for (int m = 0; m < points.Count - 1; m++)//the -1 is there for a reason dumbass
			{
				for (int n = 0; n < firstPassCircleIntersects.Count; n++)
				{	//<= and >= might need to be switched around so you get correct indexes
					if ((points[m].x <= firstPassCircleIntersects[n].point.x && points[m + 1].x > firstPassCircleIntersects[n].point.x) || (points[m].x >= firstPassCircleIntersects[n].point.x && points[m + 1].x < firstPassCircleIntersects[n].point.x))
					{
						float intersectX = firstPassCircleIntersects[n].point.x;
						float intersectY = ((firstPassCircleIntersects[n].point.x - points[m].x) / (points[m + 1].x - points[m].x)) * (points[m + 1].y - points[m].y) + points[m].y;
						if (intersectY > firstPassCircleIntersects[n].point.y + .1f) //.01f needs to be epsilon maybe
						{
							//if even Count
							if (fullMagicList[n].Count % 2.0 == 0.0)
							{
							fullMagicList[n].Add(new PointAndIndex(new Vector2(intersectX, intersectY), m + 1));
							}
							//if odd Count
							else
							{
							fullMagicList[n].Add(new PointAndIndex(new Vector2(intersectX, intersectY), m));
							}
							GameObject PooChain11 = (GameObject)Instantiate(PooChainClone, new Vector3(intersectX, intersectY, -6.0f),Quaternion.identity);
							PooChain11.renderer.material.color = Color.Lerp (Color.green, Color.black, 1.0f);
						}
					}
				}
			}
			
			//go through fullMagicList to see if there are any odd Counts and if so, determine the lowest Y value to use as
			//the magic value, then break every magic number to the number it was determined from
			
			//have to assign it or it wont work, ask zeb bout better waytadodis
			PointAndIndex magicFoundFromThisPoint = new PointAndIndex(new Vector2(0.0f, 0.0f), 1);
			for (int o = 0; o < fullMagicList.Count; o++)
			{	//if odd Count
				if (fullMagicList[o].Count % 2.0 != 0.0)
				{
					PointAndIndex lowestCircleIntersect = fullMagicList[o][0];
					//Debug.Log ("set lowest magic");
					for (int i = 0; i < fullMagicList[o].Count; i++)
					{
						if (fullMagicList[o][i].point.y < lowestCircleIntersect.point.y)
						{
							lowestCircleIntersect = fullMagicList[o][i];
						}
					}
					for (int q = 0; q < firstPassCircleIntersects.Count; q++)
					{
						//might need to make this an espsilon thing because there maybe will be rounding errors
						if (firstPassCircleIntersects[q].point.x > lowestCircleIntersect.point.x - EPSILON && firstPassCircleIntersects[q].point.x < lowestCircleIntersect.point.x + EPSILON)
						{
							//Debug.Log ("magic found from " + magicFoundFromThisPoint.point.x);
							//Debug.Log ("magicPoint " + lowestCircleIntersect.point.x);
							magicFoundFromThisPoint = firstPassCircleIntersects[q];
							if (lowestCircleIntersect.index > magicFoundFromThisPoint.index)
							{
							newBreakList.Add(new BreakObject(magicFoundFromThisPoint, lowestCircleIntersect));
							}
							else
							{
							newBreakList.Add(new BreakObject(lowestCircleIntersect, magicFoundFromThisPoint));
							}
						}
					}	
										
				}

			}
			//right and left intersections only start a break if they are the first break, they can end any break though by overriding circle intersections, magic points and points derived from are always breaks
			//third pass through points, finding which exits fall within counterclockwise from first enter circle to last exit circle
			//breaking all that exit counterclockwise
			//impossible for lowestLeft to end any breaks?
			Debug.Log ("firstpassCount" + firstPassCircleIntersects.Count);
			//Vector2 firstEnter = new Vector2(points[firstPassCircleIntersects[0].index].x - points[firstPassCircleIntersects[0].index - 1].x, points[firstPassCircleIntersects[0].index].y - points[firstPassCircleIntersects[0].index - 1].y);
			Vector2 firstEnter = new Vector2(firstPassCircleIntersects[0].point.x - mousePosition.x, firstPassCircleIntersects[0].point.y - mousePosition.y);
			//float lastExitX = points[firstPassCircleIntersects[firstPassCircleIntersects.Count - 1].index].x - points[firstPassCircleIntersects[firstPassCircleIntersects.Count - 1].index + 1].x;
			//float lastExitY = points[firstPassCircleIntersects[firstPassCircleIntersects.Count - 1].index].y - points[firstPassCircleIntersects[firstPassCircleIntersects.Count - 1].index + 1].y;
			float lastExitX = firstPassCircleIntersects[firstPassCircleIntersects.Count - 1].point.x - mousePosition.x;
			float lastExitY = firstPassCircleIntersects[firstPassCircleIntersects.Count - 1].point.y - mousePosition.y;
			Vector2 fromVector2 = firstEnter;
			Vector2 toVector2 = new Vector2(lastExitX, lastExitY);
			
			Debug.Log ("firstCircle" + firstEnter);
			Debug.Log ("lastCircle" + toVector2);
			
			float firstEnterToLastExitAngle = Vector2.Angle(fromVector2, toVector2);
			Vector3 cross = Vector3.Cross(fromVector2, toVector2);
			
			if (cross.z < 0)
			{
			    firstEnterToLastExitAngle = 360 - firstEnterToLastExitAngle;
			}
			Debug.Log("firstEnterToLastExitAngle " + firstEnterToLastExitAngle);
			
			//have to assign it or it wont work, ask zeb bout better waytadodis
			PointAndIndex startBreak = new PointAndIndex(new Vector2(0.0f, 0.0f), 1);
			bool lowestRightPointUsed = false;
			//PointAndIndex currentEnterPoint = firstPassCircleIntersects[0];
			for (int s = 0; s < firstPassCircleIntersects.Count; s = s + 1)
			{

				Vector2 currentExitVector2 = (firstPassCircleIntersects[s].point - mousePosition);
				float firstEnterToCurrentExitAngle = Vector2.Angle(firstEnter, currentExitVector2);
				Vector3 cross1 = Vector3.Cross(firstEnter, currentExitVector2);
				//pretty sure this is <0 but might need to be >0
				if (cross1.z < 0)
				{
					firstEnterToCurrentExitAngle = 360 - firstEnterToCurrentExitAngle;
				}
				Debug.Log ("firstEnterCurrentExitAngle" + firstEnterToCurrentExitAngle);
				//if final pass through loop and lowestRight has not been used
				if (s == firstPassCircleIntersects.Count - 1 && !lowestRightPointUsed && lowestRightPoint.index != 666)
				{
					Debug.Log ("s1 " + s);
					newBreakList.Add(new BreakObject(startBreak, lowestRightPoint));
					continue;
				}
				//if final pass through loop and lowestRight has been used or does not exist
				if (s == firstPassCircleIntersects.Count - 1 && (lowestRightPointUsed || lowestRightPoint.index == 666))
				{
					Debug.Log ("s2 " + s + " index" + lowestRightPoint.index);
					newBreakList.Add(new BreakObject(startBreak, firstPassCircleIntersects[s]));
					continue;
				}
				//if first pass through loop, sets start break
				if (s == 0)
				{
					if (leftExplosionIntersects.Count % 2.0 != 0.0 && lowestLeftPoint.index <= firstPassCircleIntersects[0].index)//firstpasscircleintersects.count > 0
					{
						
						startBreak = lowestLeftPoint;
						continue;
					}	
					else
					{
						startBreak = firstPassCircleIntersects[0];
						continue;
					}
				}
				//if accesing an exitCircle and is CounterClockwise, might need to be <= firstEntertolastexitangle
				if (s % 2 != 0.0 && firstEnterToCurrentExitAngle < firstEnterToLastExitAngle)
				{
					if (!lowestRightPointUsed && lowestRightPoint.index != 666 && firstPassCircleIntersects[s + 1].index > lowestRightPoint.index)
					{
						if (lowestRightPoint.point.x == 0)
						{
							Debug.Log ("s3 " + s);
						}
						newBreakList.Add(new BreakObject(startBreak, lowestRightPoint));
						startBreak = firstPassCircleIntersects[s + 1];
						lowestRightPointUsed = true;
						s++;
						continue;
					}
					else
					{
						if (firstPassCircleIntersects[s].point.x == 0)
						{
							Debug.Log ("s4 " + s);
						}
						newBreakList.Add(new BreakObject(startBreak, firstPassCircleIntersects[s]));
						startBreak = firstPassCircleIntersects[s + 1];
						s++;
						continue;
					}
				}
				//if accessing an exitCircle and is !CounterClockwise
				if (s % 2 != 0.0 && firstEnterToCurrentExitAngle > firstEnterToLastExitAngle)
				{
					s++;
					continue;
				}
				//if accessing an enterCircle and is not first pass through loop
				//i dont think this ever happens, it would be an exit circle since start circle would
				//have been added earlier and then s++ to skip to exit
				if (s % 2 == 0.0 && s != 0)
				{
					Debug.Log ("jew");
					if (!lowestRightPointUsed)
					{
						//if (lowestRightPoint.index < 
					}
				}
			}
			deletePoints (newBreakList);
		}
	}
	//when there are no bottom circle points you are trying to draw them anyway, i.e top of circle and shit, justbreak magic points, so it'd 
	//just be add at magic point and derived from and then end
	
	//currently if the same start and end are in multple breaks, it will keep them all, but it will just delete and make same points again
	//will want to get rid of this for speeder maybe later tuts
	void deletePoints(List<BreakObject> newBreakList)
	{
		//determine which indexes fall within others, because when break it adds points at start and end
		List<int> insideIndexes = new List<int>();
		for (int i = 0; i < newBreakList.Count; i++)
		{
			Debug.Log ("before " + i);
			Debug.Log ("start " + newBreakList[i].start.index + " end " + newBreakList[i].end.index);
			//Debug.Log ("end" + newBreakList[i].end.point.x);
			for (int j = 0; j < newBreakList.Count; j++)
			{
				if (j == i)
				{
					continue;
				}
				if (newBreakList[i].start.index >= newBreakList[j].start.index && newBreakList[i].end.index <= newBreakList[j].end.index)
				{
					if (newBreakList[i].start.index == newBreakList[j].start.index && newBreakList[i].end.index == newBreakList[j].end.index)
					{
						newBreakList.RemoveAt(i);
						i--;
						Debug.Log ("i got deleted for not being unique");
						break;
						//continue;
					}
					Debug.Log ("insideIndex " + i);
					newBreakList.RemoveAt (i);
					i--;
					break;
					//insideIndexes.Add (i);
				}
			}
		}
		insideIndexes.Sort ();
		//HashSet<int> hset = new HashSet<int>(insideIndexes);
		//List<int> uniqueBreakIndex = hset.ToList ();
		//sort list in numerical order so as not to fuck with order due to list compression after removeAt
		//uniqueBreakIndex.Sort ();
		for (int i = insideIndexes.Count - 1; i >= 0; i--)
		{
			newBreakList.RemoveAt (insideIndexes[i]);
		}
		
		for (int i = 0; i < newBreakList.Count; i++)
		{
			Debug.Log ("after " + i);
			Debug.Log ("start " + newBreakList[i].start.index + " end " + newBreakList[i].end.index);
			//Debug.Log ("end" + newBreakList[i].end.point.x);
		}
		
		newBreakList.Sort ((x,y) => x.start.index == y.start.index ? 0 : (x.start.index < y.start.index ? -1 : 1));
				
		for (int i = 0; i < newBreakList.Count; i++)
		{	
			Debug.Log ("afterSort " + i);
			Debug.Log ("start " + newBreakList[i].start.index + " end " + newBreakList[i].end.index);
			//Debug.Log ("end" + newBreakList[i].end.point.x);
		}
		
		
		//might want to pass in mouse position so it isnt possible to be different
		Vector3 mousePositionTemp = Camera.mainCamera.ScreenToWorldPoint(Input.mousePosition);
		Vector2 mousePosition;
		mousePosition.x = mousePositionTemp.x;
		mousePosition.y = mousePositionTemp.y;
		
		for (int i = newBreakList.Count - 1; i >= 0; i--)
		{
			//List<int> everyBreakIndex = new List<int>();
			
			//drawing balls on screen at break points
//			GameObject PooChain11 = (GameObject)Instantiate(PooChainClone, new Vector3(newBreakList[i].start.point.x, newBreakList[i].start.point.y + 1.0f + i * 1.0f, -7.0f),Quaternion.identity);
//			GameObject PooChain12 = (GameObject)Instantiate(PooChainClone, new Vector3(newBreakList[i].end.point.x, newBreakList[i].end.point.y + 1.0f + i * 1.0f, -7.0f),Quaternion.identity);
//			float r = Random.value;
//			float g = Random.value;
//			float b = Random.value;
//			Color jewsus = new Color(r,g,b);
//			PooChain11.renderer.material.color = jewsus;
//			PooChain12.renderer.material.color = jewsus;
				
			//want to do no breaking, just want to add points accordingly(probably general rules) since the points both fall
			//within the same line segment			
			if 	(newBreakList[i].start.index > newBreakList[i].end.index)
			{
				points.Insert(newBreakList[i].start.index, new Vector2(newBreakList[i].end.point.x, newBreakList[i].end.point.y));
				for (float j = newBreakList[i].end.point.x; j >= newBreakList[i].start.point.x; j--)
				{
					//Debug.Log ("j-mouseX" + (j-mousePosition.x));
					if (j - mousePosition.x < explosionRadius && j - mousePosition.x > -explosionRadius)
					{
						//Debug.Log ("J" + j);
						float y = Mathf.Sqrt (Mathf.Pow(explosionRadius, 2) - Mathf.Pow(j - mousePosition.x, 2));
						points.Insert(newBreakList[i].start.index, new Vector2(j, mousePosition.y - y));
					}
				}
				float y2 = Mathf.Sqrt (Mathf.Pow(explosionRadius, 2) - Mathf.Pow(newBreakList[i].start.point.x - mousePosition.x, 2));
				//points.Insert(newBreakList[i].start.index, new Vector2(newBreakList[i].start.point.x, mousePosition.y));// - y2));
				points.Insert(newBreakList[i].start.index, new Vector2(newBreakList[i].start.point.x, newBreakList[i].start.point.y));
				//buildTerrainMesh();
				continue;
			}
			
			for (int k = newBreakList[i].end.index; k >= newBreakList[i].start.index; k--)
			{
				Debug.Log ("remove " + newBreakList[i].end.index + "to " + newBreakList[i].start.index);
				points.RemoveAt (k);
			}
			
			points.Insert(newBreakList[i].start.index, new Vector2(newBreakList[i].end.point.x, newBreakList[i].end.point.y));
			//points.Insert(newBreakList[i].start.index, new Vector2(newBreakList[i].end.point.x, mousePosition.y));
			for (float j = newBreakList[i].end.point.x - 1; j >= newBreakList[i].start.point.x; j--)
			{
//				if (j - mousePosition.x < explosionRadius && j - mousePosition.x > -explosionRadius)
//				{
					//Debug.Log ("J" + j);
					float y = Mathf.Sqrt (Mathf.Pow(explosionRadius, 2) - Mathf.Pow(j - mousePosition.x, 2));
					points.Insert(newBreakList[i].start.index, new Vector2(j, mousePosition.y - y));
//				}
			}
			//float y3 = Mathf.Sqrt (Mathf.Pow(explosionRadius, 2) - Mathf.Pow(newBreakList[i].start.point.x - mousePosition.x, 2));
			//points.Insert(newBreakList[i].start.index, new Vector2(newBreakList[i].start.point.x, mousePosition.y));// - y3));
			points.Insert(newBreakList[i].start.index, new Vector2(newBreakList[i].start.point.x, newBreakList[i].start.point.y));

//			for (int j = newBreakList[i].start.index; j <= newBreakList[i].end.index; j++)
//			{
//				everyBreakIndex.Add (j);
//			}
		}
//		HashSet<int> hset = new HashSet<int>(everyBreakIndex);
//		List<int> uniqueBreakIndex = hset.ToList ();
//		//sort list in numerical order so as not to fuck with order due to list compression after removeAt
//		uniqueBreakIndex.Sort ();
//		for (int i = uniqueBreakIndex.Count - 1; i >= 0; i--)
//		{
//			Debug.Log("Removed" + uniqueBreakIndex[i]);
//		    points.RemoveAt(uniqueBreakIndex[i]);
//		}
//	 	float r = Random.value;
//		float g = Random.value;
//		float b = Random.value;
//		Color jewsus = new Color(r,g,b);
//		for (int i = 0; i < points.Count; i++)
//		{
//			GameObject PooChain = (GameObject)Instantiate(PooChainClone, new Vector3(points[i].x, points[i].y, -6.0f),Quaternion.identity);
//			PooChain.transform.localScale -= new Vector3(1.0f,1.0f,1.0f);
//			PooChain.renderer.material.color = jewsus;
//		}
		
		//this is probably done wrong
		//right now this might only work when curve is in correct direction
	 	bool success = false;
	 	success = buildTerrainMesh();
		if (success == false)
		{
			Debug.Log ("buildTerrainFAILED");
			for (int i = 0; i < points.Count - 1; i++)
			{
				Vector2 intersectionPoint = segmentIntersection (points[i].x, points[i].y, points[i + 1].x, points[i + 1].y, newBreakList[0].start.point.x, newBreakList[0].start.point.y, newBreakList[newBreakList.Count - 1].end.point.x, newBreakList[newBreakList.Count - 1].end.point.y);
				if (intersectionPoint != new Vector2(666,666))
				{
					points.RemoveRange (newBreakList[newBreakList.Count - 1].end.index, i);
					points.Insert(i, intersectionPoint);
				}
			}
		}
		Debug.Log ("buildTerrainMesh()");
	}
	
	bool buildTerrainMesh()
	{
		// Use the triangulator to get indices for creating triangles
        int[] indices = Triangulator.Triangulate(points);
        
        if(indices == null) return false;
        
        Vector3[] vertices = new Vector3[points.Count];
        Vector3[] normals = new Vector3[points.Count];
		Vector2[] textureCoords = new Vector2[points.Count];
		
        for (int i = 0; i < vertices.Length; i++) 
        {
            vertices[i] = new Vector3(points[i].x, points[i].y, 0);
            normals[i] = Vector3.forward;
			textureCoords[i] = new Vector2(0, 0);
        }
	
		MeshFilter mf = GetComponent("MeshFilter") as MeshFilter;
		
		Mesh mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.triangles = indices;
		mesh.normals = normals;
		mesh.uv = textureCoords;
		mesh.Optimize();
	    mesh.RecalculateBounds();
	    
	    mf.mesh = mesh;
	    
        // Build the mesh for the collider
        indices = new int[(points.Count-3)*6];
        vertices = new Vector3[(points.Count-2)*2];
        normals = new Vector3[(points.Count-2)*2];
		
		for (int p = 0; p < points.Count-3; p++)
		{
			int i = p*6;
			indices[i] = p;
			indices[i+1] = p+points.Count-2;
			indices[i+2] = p+1;
			
			indices[i+3] = p+1;
			indices[i+4] = p+points.Count-2;
			indices[i+5] = p+points.Count-1;
		}
		
        for (int p = 0; p < points.Count-2; p++)
        {
            vertices[p] = new Vector3(points[p+1].x, points[p+1].y, 0);
            vertices[p+points.Count-2] = new Vector3(points[p+1].x, points[p+1].y, 10);
            normals[p] = Vector3.up;
            normals[p+points.Count-2] = Vector3.up;
        }
	
		MeshCollider mc = GetComponent("MeshCollider") as MeshCollider;
		
		mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.triangles = indices;
		mesh.normals = normals;
	    
	    mc.sharedMesh = mesh;
	    mesh.Optimize();
	    mesh.RecalculateBounds();
	    
	    return true;
	}
	void _generate()
	{
		points.Clear();
		points.Capacity = numSegments+1+2; // segments + 2 bottom points to create a polygon
		
		for(int i = 0; i < points.Capacity; i++)
		{
			points.Add(new Vector2(0, 0));
		}
		
		points[0] = new Vector2(0, 0);
		points[1] = new Vector2(0, Random.value*height);
		
		points[numSegments+1] = new Vector2(numSegments*segmentWidth, Random.value*height);
		points[numSegments+2] = new Vector2(numSegments*segmentWidth, 0);
		
		_midPointDivision(1, numSegments+1, 0);
		
		VectorField vectorField = vectorFieldObject.GetComponent("MonoBehaviour") as VectorField;
		vectorField.width = width;
		vectorField.height = height;
		
		for(int t = 0; t < warpTime; t++)
		{
			for(int i = 2; i < points.Count-2; i++)
			{
				float xForce = vectorField.getXForce(points[i].x, points[i].y);
				float yForce = vectorField.getYForce(points[i].x, points[i].y);
				if (xForce < 1.0f)
				{
					//Debug.Log ("<1 " + xForce + " " + yForce);	
				}

				float nextX = Mathf.Clamp(points[i].x + xForce*warpScale, 0, width);
				float nextY = Mathf.Clamp(points[i].y + yForce*warpScale, 1, height);
				
				points[i] = new Vector2(nextX, nextY);
				
				if(i != 2)
				{
					float distance = Vector2.Distance(points[i], points[i-1]);
					if(distance > segmentWidth*2)
					{
						points.Insert(i, points[i-1] + (points[i]-points[i-1])*.5f);
						i--;
					}
				}
			}
		}
	}
	void _midPointDivision(int start, int end, int depth) 
	{
		if (end <= start+1) 
		{
			return;
		}
		
		int midPoint = (end + start) / 2;
		float midY = (points[start].y + points[end].y) / 2.0f;
		float depthFactor = .5f / (depth*depth);
		
		float y = midY + (Random.value * 2 * depthFactor - depthFactor)*height;
		
		// Make sure y is between 0 and height
		y = Mathf.Max(Mathf.Min(height, y), 1); 
		
		points[midPoint] = new Vector2((midPoint-1)*segmentWidth, y);
		
		_midPointDivision(start, midPoint, depth + 1);
		_midPointDivision(midPoint, end, depth + 1);
	}
	
	Vector2 segmentIntersection(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4) 
	{
		float bx = x2 - x1;
		float bY = y2 - y1;
		float dx = x4 - x3;
		float dy = y4 - y3;
		float b_dot_d_perp = bx * dy - bY * dx;
		if(b_dot_d_perp == 0) 
		{
		  return new Vector2(666,666);
		}
		float cx = x3 - x1;
		float cy = y3 - y1;
		float t = (cx * dy - cy * dx) / b_dot_d_perp;
		if(t < 0 || t > 1) 
		{
		  return new Vector2(666,666);
		}
		float u = (cx * bY - cy * bx) / b_dot_d_perp;
		if(u < 0 || u > 1) 
		{ 
		  return new Vector2(666,666);
		}
		return new Vector2(x1 + t * bx, y1 + t * bY);
	}
}

public class PointAndIndex
{
	public Vector2 point;
	public int index;
	public PointAndIndex(Vector2 tempPoint, int tempIndex)
	{
		point = tempPoint;
		index = tempIndex;
	}
}

public class BreakObject
{	
	public PointAndIndex start;
	public PointAndIndex end;
	public BreakObject(PointAndIndex tempStart, PointAndIndex tempEnd)
	{
		start = tempStart;
		end = tempEnd;
	}
}