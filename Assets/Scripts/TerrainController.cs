//TODO: why did i have to make the constructor for my P&I and BREAKOBJECT public

//when terrainBuild is invalid you are going to want to draw a line from startbreak to end break, then check if it intersects with any lineSegments from Points[], if it does you draw a line from the start to the intersect and break from intersect to end

//possibly points are getting draw at the same location so the terrain fails, fix it, use EPSILON to add or subtract x or some shit based on direction

//worst comes to worst you cango through all thep oints and if two are the same that are next to eachother make one off a bit so you can build the terrain

//you tried to fix thebottom of screen problem(odd intersect results), but later you are just going to not have it affect it and move the terrain down like yeah
using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;


[ExecuteInEditMode]


public class TerrainController : MonoBehaviour
{
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
	
	PointAndIndex[] firstPassCircleIntersects = new PointAndIndex[100];
	PointAndIndex[,] fullMagicList = new PointAndIndex[100,100];
	BreakObject[] newBreakList = new BreakObject[100];
	PointAndIndex[] leftExplosionIntersects = new PointAndIndex[100];
	PointAndIndex[] rightExplosionIntersects = new PointAndIndex[100];
	int firstPassCircleIntersectsCount;
	int[] fullMagicListCount = new int[100];
	int newBreakListCount;//need to somehow pass this into delete method while not resetting at end of mmouseinput?
	int leftExplosionIntersectsCount;
	int rightExplosionIntersectsCount;
	BreakObject[,] explosionList = new BreakObject[100,100];
	Vector2[] explosionCenter = new Vector2[100];
	int explosionListAndCenterCount;
	
	public GameObject vectorFieldObject;
	
	bool doneGenerating = false;
	
	//List<List<BreakObject>> explosionList = new List<List<BreakObject>>();
	//List<Vector2> explosionCenter = new List<Vector2>();
	
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
	
	void FixedUpdate()
	{
		for (int i = 0; i < explosionListAndCenterCount; i++)
		{
			deletePoints (explosionList, explosionCenter[i], explosionListAndCenterCount);
		}
		
//		for (int i = 0; i < explosionList.Count; i++)
//		{
//			deletePoints (explosionList[i], explosionCenter[i]);
//		}
//		explosionList.Clear();// = new List<List<BreakObject>>();
//		explosionCenter.Clear();// = new List<Vector2>();
	}
	
	
	// Update is called once per frame
	void Update()
	{	//figure out how to tell if circle intersection is tangent cause it'll f everything up, you want to ignore the single
		//circle intersection
		// indexes selected are the ones you want to destroy
		
		if (Input.GetMouseButtonDown(0))
		{
			Vector3 mousePositionTemp = Camera.mainCamera.ScreenToWorldPoint(Input.mousePosition);
			//optimize for if sircle is no where near terrain, just return
			if (mousePositionTemp.y < 1.0f)
			{
				return;
			}
			Vector2 mousePosition;
			mousePosition.x = mousePositionTemp.x;
			mousePosition.y = mousePositionTemp.y;
			//List<PointAndIndex> firstPassCircleIntersects = new List<PointAndIndex>();
			//List<List<PointAndIndex>> fullMagicList = new List<List<PointAndIndex>>();
			//List<BreakObject> newBreakList = new List<BreakObject>();
			//List<PointAndIndex> leftExplosionIntersects = new List<PointAndIndex>();
			//List<PointAndIndex> rightExplosionIntersects = new List<PointAndIndex>();
			bool previousHadOneEqualTo = false;
			bool previousStartInsideRadius = false;
			
			//first pass through Points, determines all Left, Right, and Circle Intersections
			//maybe needs to be .count - 1
			for (int i = 0; i < points.Count - 1; i++)
			{
				bool thisStartInsideRadius = false;
				bool thisEndsInsideRadius = false;
				bool thisHadOneEqualTo = false;
				float leftExplosionRadiusX = mousePosition.x - explosionRadius;
				float rightExplosionRadiusX = mousePosition.x + explosionRadius;
				float percentageAcrossRightX = (rightExplosionRadiusX - points[i].x) / (points[i + 1].x - points[i].x);
				float percentageAcrossLeftX = (leftExplosionRadiusX - points[i].x) / (points[i + 1].x - points[i].x);
				
				//<= < > >= and such should all be correct for the indexes deleted
				//left circle upwards Intersections
				//fairly very certain i dont need the or statement part of this(first statement), which means i dont need to use odd number things throughout most of this code
				//possibly only need the other direction check in circleIntersects
				//uses == end points
				if (((points[i].x < leftExplosionRadiusX && points[i + 1].x >= leftExplosionRadiusX) || (points[i].x > leftExplosionRadiusX && points[i + 1].x <= leftExplosionRadiusX)) && (points[i + 1].y - points[i].y) * percentageAcrossLeftX + points[i].y > mousePosition.y)
				{
					Debug.Log("leftSide");
					float intersectLeftX = leftExplosionRadiusX;
					float intersectLeftY = ((leftExplosionRadiusX - points[i].x) / (points[i + 1].x - points[i].x)) * (points[i + 1].y - points[i].y) + points[i].y;
					// if even Count
					if (leftExplosionIntersectsCount % 2.0 == 0.0)
					{
						leftExplosionIntersects[leftExplosionIntersectsCount] = new PointAndIndex(new Vector2(intersectLeftX, intersectLeftY), i + 1);
						leftExplosionIntersectsCount++;
						//leftExplosionIntersects.Add(new PointAndIndex(new Vector2(intersectLeftX, intersectLeftY), i + 1));
					}
					else //if odd Count
					{
						leftExplosionIntersects[leftExplosionIntersectsCount] = new PointAndIndex(new Vector2(intersectLeftX, intersectLeftY), i + 1);
						leftExplosionIntersectsCount++;
						//leftExplosionIntersects.Add(new PointAndIndex(new Vector2(intersectLeftX, intersectLeftY), i + 1));
					}
				}
				//right circle upwards Intersections
				//uses == start points
				if (((points[i].x <= rightExplosionRadiusX && points[i + 1].x > rightExplosionRadiusX) || (points[i].x >= rightExplosionRadiusX && points[i + 1].x < rightExplosionRadiusX)) && (points[i + 1].y - points[i].y) * percentageAcrossRightX + points[i].y > mousePosition.y)
				{
					Debug.Log("rightSide");
					float intersectRightX = rightExplosionRadiusX;
					float intersectRightY = ((rightExplosionRadiusX - points[i].x) / (points[i + 1].x - points[i].x)) * (points[i + 1].y - points[i].y) + points[i].y;
					// if even Count
					if (rightExplosionIntersectsCount % 2.0 == 0.0)
					{
						rightExplosionIntersects[rightExplosionIntersectsCount] = new PointAndIndex(new Vector2(intersectRightX, intersectRightY), i);
						rightExplosionIntersectsCount++;
						//rightExplosionIntersects.Add(new PointAndIndex(new Vector2(intersectRightX, intersectRightY), i));
					}
					else //if odd Count
					{
						rightExplosionIntersects[rightExplosionIntersectsCount] = new PointAndIndex(new Vector2(intersectRightX, intersectRightY), i);
						rightExplosionIntersectsCount++;
						//rightExplosionIntersects.Add(new PointAndIndex(new Vector2(intersectRightX, intersectRightY), i));
					}
				}
				
				
				//line segment Circle intersections
				float x0 = points[i].x - mousePosition.x;
				float y0 = points[i].y - mousePosition.y;
				float x1 = points[i+1].x - mousePosition.x;
				float y1 = points[i+1].y - mousePosition.y;
				float r = explosionRadius;
				
				if (Mathf.Sqrt (x0*x0 + y0*y0) < explosionRadius)
				{
					thisStartInsideRadius = true;
				}
				
				if (Mathf.Sqrt (x1*x1 + y1*y1) < explosionRadius)
				{
					thisEndsInsideRadius = true;
				}
				
				float x2 = points[i+1].x - mousePosition.x;
				float y2 = points[i+1].y - mousePosition.y;
				bool nextFirstIsOnLineSegment = false;
				bool nextSecondIsOnLineSegment = false;
				
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
					if (r*r*dR*dR-d*d < 0.0f)
					{
						Debug.Log ("LESS THAN MOTHERFUCKING 00000000000000000000000000000000000000000000000000000000000");	
					}
					if (dR == 0.0f)
					{
						Debug.Log ("LESS THAN MOTHERFUCKING 00000000000000000000000000000000000000000000000000000000000");
					}
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
					
					//if x is = both then just check if its in between the ys, same for the other way 'round
					if (resultingX1 <= x0 + EPSILON && resultingX1 >= x0 - EPSILON && resultingX1 <= x1 + EPSILON && resultingX1 >= x1 - EPSILON)
					{
						if (resultingY1 > y0 && resultingY1 <= y1 || resultingY1 >= y1 && resultingY1 < y0)
						{
							Debug.Log ("went through kewl == to thing");
							firstIsOnLineSegment = true;
						}
					}
					if (resultingX2 <= x0 + EPSILON && resultingX2 >= x0 - EPSILON && resultingX2 <= x1 + EPSILON && resultingX2 >= x1 - EPSILON)
					{
						if (resultingY2 > y0 && resultingY2 <= y1 || resultingY2 >= y1 && resultingY2 < y0)
						{
							Debug.Log ("went through kewl == to thing");
							secondIsOnLineSegment = true;
						}
					}
					if (resultingY1 <= y0 + EPSILON && resultingY1 >= y0 - EPSILON && resultingY1 <= y1 + EPSILON && resultingY1 >= y1 - EPSILON)
					{
						if (resultingX1 > x0 && resultingX1 <= x1 || resultingX1 >= x1 && resultingX1 < x0)
						{
							Debug.Log ("went through kewl == to thing");
							firstIsOnLineSegment = true;
						}
					}
					if (resultingY2 <= y0 + EPSILON && resultingY2 >= y0 - EPSILON && resultingY2 <= y1 + EPSILON && resultingY1 >= y1 - EPSILON)
					{
						if (resultingX2 > x0 && resultingX2 <= x1 || resultingX2 >= x1 && resultingX2 < x0)
						{
							Debug.Log ("went through kewl == to thing");
							secondIsOnLineSegment = true;
						}
					}
					
					if (!firstIsOnLineSegment && secondIsOnLineSegment || firstIsOnLineSegment && !secondIsOnLineSegment)
					{
						thisHadOneEqualTo = true;
					}
					
					//if i ignore the starts of the linesegments and only use the ends if equal to, then index + 1
					if ((resultingX1 > x0 && resultingX1 <= x1 && resultingY1 > y0 && resultingY1 <= y1) || (resultingX1 < x0 && resultingX1 >= x1 && resultingY1 < y0 && resultingY1 >= y1) || (resultingX1 < x0 && resultingX1 >= x1 && resultingY1 > y0 && resultingY1 <= y1) || (resultingX1 > x0 && resultingX1 <= x1 && resultingY1 < y0 && resultingY1 >= y1))
					{
						Debug.Log("firstResult");
						firstIsOnLineSegment = true; //since the resulting points dont fall within the lineSegment
//						Debug.Log("ox0 " + x0);
//						Debug.Log("oy0 " + y0);
//						Debug.Log("ox1 " + x1);
//						Debug.Log("oy1 " + y1);
////						//Debug.Log("r " + r);
//						Debug.Log("rx1 " + resultingX1);
//						Debug.Log("ry1 " + resultingY1);
//						Debug.Log("rx2 " + resultingX2);
//						Debug.Log("ry2 " + resultingY2);
						if (D1greaterthanD2)
						{
							Debug.Log ("distance01>distance02");
						}
					}
					if ((resultingX2 > x0 && resultingX2 <= x1 && resultingY2 > y0 && resultingY2 <= y1) || (resultingX2 < x0 && resultingX2 >= x1 && resultingY2 < y0 && resultingY2 >= y1) || (resultingX2 < x0 && resultingX2 >= x1 && resultingY2 > y0 && resultingY2 <= y1) || (resultingX2 > x0 && resultingX2 <= x1 && resultingY2 < y0 && resultingY2 >= y1))
					{
						Debug.Log("secondResult");
						secondIsOnLineSegment = true; //since the resulting points dont fall within the lineSegment
//						Debug.Log("ox0 " + x0);
//						Debug.Log("oy0 " + y0);
//						Debug.Log("ox1 " + x1);
//						Debug.Log("oy1 " + y1);
//	//					//Debug.Log("r " + r);
//						Debug.Log("rx1 " + resultingX1);
//						Debug.Log("ry1 " + resultingY1);
//						Debug.Log("rx2 " + resultingX2);
//						Debug.Log("ry2 " + resultingY2);
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
					thisHadOneEqualTo = false;
					Debug.Log("boobs1");
					// if Count is even
					if (firstPassCircleIntersectsCount % 2.0 == 0.0)
					{
						//both might need to be i+1
						firstPassCircleIntersects[firstPassCircleIntersectsCount] = new PointAndIndex(new Vector2(resultingX1 + mousePosition.x, resultingY1 + mousePosition.y), i + 1);
						firstPassCircleIntersectsCount++;
						//firstPassCircleIntersects.Add(new PointAndIndex(new Vector2(resultingX1 + mousePosition.x, resultingY1 + mousePosition.y), i + 1));//1-12 to all of these same comments downward used to be
						if (resultingX2 == x1 && resultingY2 == y1)
						{
							firstPassCircleIntersects[firstPassCircleIntersectsCount] = new PointAndIndex(new Vector2(resultingX2 + mousePosition.x, resultingY2 + mousePosition.y), i + 1);
							firstPassCircleIntersectsCount++;
							//firstPassCircleIntersects.Add(new PointAndIndex(new Vector2(resultingX2 + mousePosition.x, resultingY2 + mousePosition.y), i + 1));//i
						}
						else
						{
							firstPassCircleIntersects[firstPassCircleIntersectsCount] = new PointAndIndex(new Vector2(resultingX2 + mousePosition.x, resultingY2 + mousePosition.y), i);//i
							firstPassCircleIntersectsCount++;
							//firstPassCircleIntersects.Add(new PointAndIndex(new Vector2(resultingX2 + mousePosition.x, resultingY2 + mousePosition.y), i));//i
						}
					}
					//if Count is odd
					else
					{
						Debug.Log ("should never happen?");
						firstPassCircleIntersects[firstPassCircleIntersectsCount] = new PointAndIndex(new Vector2(resultingX1 + mousePosition.x, resultingY1 + mousePosition.y), i + 1);
						firstPassCircleIntersectsCount++;
						firstPassCircleIntersects[firstPassCircleIntersectsCount] = new PointAndIndex(new Vector2(resultingX2 + mousePosition.x, resultingY2 + mousePosition.y), i);
						firstPassCircleIntersectsCount++;
						//firstPassCircleIntersects.Add(new PointAndIndex(new Vector2(resultingX1 + mousePosition.x, resultingY1 + mousePosition.y), i + 1));//i
						//firstPassCircleIntersects.Add(new PointAndIndex(new Vector2(resultingX2 + mousePosition.x, resultingY2 + mousePosition.y), i));
					}
				}
				// if only first Intersection
				if (firstIsOnLineSegment && !secondIsOnLineSegment)
				{
					Debug.Log("boobs2");
					if (firstPassCircleIntersectsCount % 2.0 == 0.0)
					{
						firstPassCircleIntersects[firstPassCircleIntersectsCount] = new PointAndIndex(new Vector2(resultingX1 + mousePosition.x, resultingY1 + mousePosition.y), i + 1);
						firstPassCircleIntersectsCount++;
						//firstPassCircleIntersects.Add(new PointAndIndex(new Vector2(resultingX1 + mousePosition.x, resultingY1 + mousePosition.y), i + 1));
					}
					else
					{
						if (resultingX1 == x1 && resultingY1 == y1)
						{
							firstPassCircleIntersects[firstPassCircleIntersectsCount] = new PointAndIndex(new Vector2(resultingX1 + mousePosition.x, resultingY1 + mousePosition.y), i + 1);
							firstPassCircleIntersectsCount++;
							//firstPassCircleIntersects.Add(new PointAndIndex(new Vector2(resultingX1 + mousePosition.x, resultingY1 + mousePosition.y), i + 1));//i
						}
						else
						{
							firstPassCircleIntersects[firstPassCircleIntersectsCount] = new PointAndIndex(new Vector2(resultingX1 + mousePosition.x, resultingY1 + mousePosition.y), i);
							firstPassCircleIntersectsCount++;
							//firstPassCircleIntersects.Add(new PointAndIndex(new Vector2(resultingX1 + mousePosition.x, resultingY1 + mousePosition.y), i));//i
						}
					}
				}
				//if only second intersection
				if (!firstIsOnLineSegment && secondIsOnLineSegment)
				{
					Debug.Log("boobs3333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333");
					if (firstPassCircleIntersectsCount % 2.0 == 0.0)
					{
						firstPassCircleIntersects[firstPassCircleIntersectsCount] = new PointAndIndex(new Vector2(resultingX2 + mousePosition.x, resultingY2 + mousePosition.y), i + 1);
						firstPassCircleIntersectsCount++;
						//firstPassCircleIntersects.Add(new PointAndIndex(new Vector2(resultingX2 + mousePosition.x, resultingY2 + mousePosition.y), i + 1));
					}
					else
					{
						if (resultingX2 == x1 && resultingY2 == y1)
						{
							firstPassCircleIntersects[firstPassCircleIntersectsCount] = new PointAndIndex(new Vector2(resultingX2 + mousePosition.x, resultingY2 + mousePosition.y), i + 1);
							firstPassCircleIntersectsCount++;
							//firstPassCircleIntersects.Add(new PointAndIndex(new Vector2(resultingX2 + mousePosition.x, resultingY2 + mousePosition.y), i + 1));//i
						}
						else
						{
							firstPassCircleIntersects[firstPassCircleIntersectsCount] = new PointAndIndex(new Vector2(resultingX2 + mousePosition.x, resultingY2 + mousePosition.y), i);
							firstPassCircleIntersectsCount++;
							//firstPassCircleIntersects.Add(new PointAndIndex(new Vector2(resultingX2 + mousePosition.x, resultingY2 + mousePosition.y), i));//i
						}
					}
				}
				//in order to ignore the intersect if it doesnt fully cross butjust lands on an endpoint and then goes back the way it came
				if (previousHadOneEqualTo && previousStartInsideRadius && ((!firstIsOnLineSegment && secondIsOnLineSegment || firstIsOnLineSegment && !secondIsOnLineSegment) || ((!firstIsOnLineSegment && !secondIsOnLineSegment) && thisEndsInsideRadius)))
				{
					firstPassCircleIntersectsCount--;
					//firstPassCircleIntersects.RemoveAt (firstPassCircleIntersects.Count - 1);
				}
				if (previousHadOneEqualTo && !previousStartInsideRadius && (!firstIsOnLineSegment && !secondIsOnLineSegment) && !thisEndsInsideRadius)
				{
					firstPassCircleIntersectsCount--;
					//firstPassCircleIntersects.RemoveAt (firstPassCircleIntersects.Count - 1);
				}
				previousStartInsideRadius = thisStartInsideRadius;
				previousHadOneEqualTo = thisHadOneEqualTo;
			}
			
			for (int i = 0; i < firstPassCircleIntersectsCount; i++)
			{
				Debug.Log ("firstpassi" + i + " " + firstPassCircleIntersects[i].point + " index " + firstPassCircleIntersects[i].index);
			}
			
			
			//had to do 666 thing because below this it cant use unassigned even though it would never go into that loop unless it has already gone through this one
			PointAndIndex lowestLeftPoint = new PointAndIndex(new Vector2(666,666), 666);
			PointAndIndex lowestRightPoint = new PointAndIndex(new Vector2(666,666), 666);
			//if leftExplosionIntersects are odd number above, set lowest Point
			if (leftExplosionIntersectsCount % 2.0 != 0.0)//leftexplosionintersects.count > 0
			{
				lowestLeftPoint = leftExplosionIntersects[0];
				for (int j = 1; j < leftExplosionIntersectsCount; j++)
				{
					if (leftExplosionIntersects[j].point.y < lowestLeftPoint.point.y)
					{
						lowestLeftPoint = leftExplosionIntersects[j];
					}
				}
				Debug.Log ("left" + lowestLeftPoint.index);
			}
			//if rightExplosionIntersects are odd number above, set lowest Point
			if (rightExplosionIntersectsCount % 2.0 != 0.0)
			{
				lowestRightPoint = rightExplosionIntersects[0];
				for (int k = 1; k < rightExplosionIntersectsCount; k++)
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
				newBreakList[newBreakListCount] = new BreakObject(lowestLeftPoint, lowestRightPoint);
				newBreakListCount++;
				//newBreakList.Add (new BreakObject(lowestLeftPoint, lowestRightPoint));
				Debug.Log (newBreakList[newBreakListCount - 1].start.index + "to" + newBreakList[newBreakListCount - 1].end.index);
				explosionList[explosionListAndCenterCount] = newBreakList;
				explosionCenter[explosionListAndCenterCount] = mousePosition;
				explosionListAndCenterCount++;
				//explosionList.Add (newBreakList);
				//explosionCenter.Add(mousePosition);
				//deletePoints (newBreakList, mousePosition);
				return;
			}
			
			//if no circleIntersects(or 1 when you click too low on left or right of screen), then it's an easy break and quit
			if (firstPassCircleIntersectsCount == 0)// || firstPassCircleIntersects.Count == 1)
			{
				Debug.Log ("CircleCount = 0");
				if (leftExplosionIntersectsCount % 2.0 == 0.0 && rightExplosionIntersectsCount % 2.0 == 0.0)
				{
					return;
				}
				else
				{
					newBreakList[newBreakListCount] = new BreakObject(lowestLeftPoint, lowestRightPoint);
					newBreakListCount++;
					//newBreakList.Add (new BreakObject(lowestLeftPoint, lowestRightPoint));
					Debug.Log (newBreakList[newBreakListCount - 1].start.index + "to" + newBreakList[newBreakListCount - 1].end.index);
					explosionList[explosionListAndCenterCount] = newBreakList;
					explosionCenter[explosionListAndCenterCount] = mousePosition;
					explosionListAndCenterCount++;
					//explosionList.Add (newBreakList);
					//explosionCenter.Add(mousePosition);
					//deletePoints (newBreakList, mousePosition);
					return;
				}
			}
			//circle intersects can equal 1 if it is on the right or left of the terrain, but below enough so that its still
			//only one intersection
			//will probably fix the same way that I will fix thebottom of the terrain being a problem
//			if (firstPassCircleIntersects.Count == 1)
//			{
//				if (leftExplosionIntersects.Count % 2.0 == 0.0 && rightExplosionIntersects.Count % 2.0 == 0.0)
//				{
//					Debug.Log ("should never happen?");
//					return;
//				}
//				
//			}

			
			
			//GameObject PooChain = (GameObject)Instantiate(PooChainClone, new Vector3(lowestLeftPoint.point.x, lowestLeftPoint.point.y, -6.0f),Quaternion.identity);
			//GameObject PooChain44 = (GameObject)Instantiate(PooChainClone, new Vector3(lowestRightPoint.point.x, lowestRightPoint.point.y, -6.0f),Quaternion.identity);
			//PooChain.rigidbody.isKinematic = true;
			//PooChain44.rigidbody.isKinematic = true;
			//PooChain.renderer.material.color = Color.Lerp (Color.blue, Color.blue, 1.0f);
			//PooChain44.renderer.material.color = Color.Lerp (Color.blue, Color.blue, 1.0f);
			//PooChain.transform.localScale -= new Vector3(1.0f,1.0f,1.0f);
			//PooChain44.transform.localScale -= new Vector3(1.0f,1.0f,1.0f);
			
//took out since am using arrays so it will just access firstPassCounter to know how many			
//			// make a magicList for each circleIntersection fullMagicList
//			for (int l = 0; l < firstPassCircleIntersects.Count; l++)
//			{
//				//GameObject PooChain11 = (GameObject)Instantiate(PooChainClone, new Vector3(firstPassCircleIntersects[l].point.x, firstPassCircleIntersects[l].point.y, -9.0f),Quaternion.identity);
//				//PooChain11.renderer.material.color = Color.Lerp (Color.green, Color.red, 1.0f);
//				//PooChain11.transform.localScale -= new Vector3(1.0f,1.0f,1.0f);
//				fullMagicList.Add(new List<PointAndIndex>());
//			}
			
			//second pass through Points, finding "magic" points
//			//might want to only use magic points if its the top half of the circle thats intersected, the way it is now is if any
//			//circle intersection has a magic point ...not too sure...
			for (int m = 0; m < points.Count - 1; m++)
			{
				for (int n = 0; n < firstPassCircleIntersectsCount; n++)//could just be fullMagicList.Count?
				{	//<= and >= should be pertaining to correct indexes
					//fixes it so it wont try to put a magic point at bottom left or bottom right index
					if (firstPassCircleIntersects[n].index == 0 || firstPassCircleIntersects[n].index == points.Count - 1)
					{
						continue;
					}
					if ((points[m].x <= firstPassCircleIntersects[n].point.x && points[m + 1].x > firstPassCircleIntersects[n].point.x) || (points[m].x > firstPassCircleIntersects[n].point.x && points[m + 1].x <= firstPassCircleIntersects[n].point.x))
					{
						float intersectX = firstPassCircleIntersects[n].point.x;
						float intersectY = ((firstPassCircleIntersects[n].point.x - points[m].x) / (points[m + 1].x - points[m].x)) * (points[m + 1].y - points[m].y) + points[m].y;
						if (intersectY > firstPassCircleIntersects[n].point.y + .1f) //.01f needs to be epsilon maybe
						{
							//if even Count
							if (fullMagicListCount[n] % 2.0 == 0.0)
							{
								fullMagicList[n] = new PointAndIndex(new Vector2(intersectX, intersectY), m + 1);
								fullMagicListCount[n]++;
								//fullMagicList[n].Add(new PointAndIndex(new Vector2(intersectX, intersectY), m + 1));
							}
							//if odd Count
							else
							{
								fullMagicList[n] = new PointAndIndex(new Vector2(intersectX, intersectY), m + 1);
								fullMagicListCount[n]++;
								//fullMagicList[n].Add(new PointAndIndex(new Vector2(intersectX, intersectY), m + 1));//1-12 used to be just m
							}
							//GameObject PooChain11 = (GameObject)Instantiate(PooChainClone, new Vector3(intersectX, intersectY, -6.0f),Quaternion.identity);
							//PooChain11.renderer.material.color = Color.Lerp (Color.green, Color.black, 1.0f);
						}
					}
				}
			}
			
			//go through fullMagicList to see if there are any odd Counts and if so, determine the lowest Y value to use as
			//the magic value, then break every magic number to the number it was determined from
			
			//have to assign it or it wont work, ask zeb bout better waytadodis
			PointAndIndex magicFoundFromThisPoint = new PointAndIndex(new Vector2(0.0f, 0.0f), 1);
			for (int o = 0; o < firstPassCircleIntersectsCount; o++)
			{	//if odd Count
				if (fullMagicListCount[o] % 2.0 != 0.0)
				{
					PointAndIndex lowestCircleIntersect = fullMagicList[o,0];
					//Debug.Log ("set lowest magic");
					for (int i = 0; i < fullMagicListCount[o]; i++)
					{
						if (fullMagicList[o,i].point.y < lowestCircleIntersect.point.y)
						{
							lowestCircleIntersect = fullMagicList[o,i];
						}
					}
//					for (int q = 0; q < firstPassCircleIntersects.Count; q++)
//					{
//						if (firstPassCircleIntersects[q].point.x > lowestCircleIntersect.point.x - EPSILON && firstPassCircleIntersects[q].point.x < lowestCircleIntersect.point.x + EPSILON)
//						{
//							//Debug.Log ("magic found from " + magicFoundFromThisPoint.point.x);
//							//Debug.Log ("magicPoint " + lowestCircleIntersect.point.x);
//							magicFoundFromThisPoint = firstPassCircleIntersects[q];
					magicFoundFromThisPoint = firstPassCircleIntersects[o];
					if (lowestCircleIntersect.index > magicFoundFromThisPoint.index)
					{
						newBreakList[newBreakListCount] = new BreakObject(magicFoundFromThisPoint, lowestCircleIntersect);
						newBreakListCount++;
						//newBreakList.Add(new BreakObject(magicFoundFromThisPoint, lowestCircleIntersect));
						Debug.Log (newBreakList[newBreakListCount - 1].start.index + "to" + newBreakList[newBreakListCount - 1].end.index);
					}
					else
					{
						newBreakList[newBreakListCount] = new BreakObject(lowestCircleIntersect, magicFoundFromThisPoint);
						newBreakListCount++;
						//newBreakList.Add(new BreakObject(lowestCircleIntersect, magicFoundFromThisPoint));
						Debug.Log (newBreakList[newBreakListCount - 1].start.index + "to" + newBreakList[newBreakListCount - 1].end.index);
					}
//						}
//					}	
										
				}

			}
			//(known to be wrong) right and left intersections only start a break if they are the first break, they can end any break though by overriding circle intersections, magic points and points derived from are always breaks
			//third pass through points, finding which exits fall within counterclockwise from first enter circle to last exit circle
			//breaking all that exit counterclockwise
			//impossible for lowestLeft to end any breaks?
			Debug.Log ("firstpassCount" + firstPassCircleIntersectsCount);
			//Vector2 firstEnter = new Vector2(points[firstPassCircleIntersects[0].index].x - points[firstPassCircleIntersects[0].index - 1].x, points[firstPassCircleIntersects[0].index].y - points[firstPassCircleIntersects[0].index - 1].y);
			Vector2 firstEnter = new Vector2(firstPassCircleIntersects[0].point.x - mousePosition.x, firstPassCircleIntersects[0].point.y - mousePosition.y);
			//float lastExitX = points[firstPassCircleIntersects[firstPassCircleIntersects.Count - 1].index].x - points[firstPassCircleIntersects[firstPassCircleIntersects.Count - 1].index + 1].x;
			//float lastExitY = points[firstPassCircleIntersects[firstPassCircleIntersects.Count - 1].index].y - points[firstPassCircleIntersects[firstPassCircleIntersects.Count - 1].index + 1].y;
			float lastExitX = firstPassCircleIntersects[firstPassCircleIntersectsCount - 1].point.x - mousePosition.x;
			float lastExitY = firstPassCircleIntersects[firstPassCircleIntersectsCount - 1].point.y - mousePosition.y;
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
			bool lowestLeftPointUsed = false;
			bool startBreakIsLeftPoint = false;
			//PointAndIndex currentEnterPoint = firstPassCircleIntersects[0];
			for (int s = 0; s < firstPassCircleIntersectsCount; s = s + 1)
			{
				Vector2 currentExitVector2 = (firstPassCircleIntersects[s].point - mousePosition);
				Debug.Log ("curentExitVector2 " + currentExitVector2);
				float firstEnterToCurrentExitAngle = Vector2.Angle(firstEnter, currentExitVector2);
				Vector3 cross1 = Vector3.Cross(firstEnter, currentExitVector2);
				//pretty sure this is <0 but might need to be >0
				if (cross1.z < 0)
				{
					firstEnterToCurrentExitAngle = 360 - firstEnterToCurrentExitAngle;
				}
				Debug.Log ("firstEnterCurrentExitAngle" + firstEnterToCurrentExitAngle);
				//if final pass through loop and lowestRight has not been used
				if (s == firstPassCircleIntersectsCount - 1 && !lowestRightPointUsed && lowestRightPoint.index != 666)
				{
					Debug.Log ("s1 " + s);
					newBreakList[newBreakListCount] = new BreakObject(startBreak, lowestRightPoint);
					newBreakListCount++;
					//newBreakList.Add(new BreakObject(startBreak, lowestRightPoint));
					Debug.Log (newBreakList[newBreakListCount - 1].start.index + "to" + newBreakList[newBreakListCount - 1].end.index);
					continue;
				}
				//if final pass through loop and lowestRight has been used or does not exist
				if (s == firstPassCircleIntersectsCount - 1 && (lowestRightPointUsed || lowestRightPoint.index == 666))
				{
					Debug.Log ("s2 " + s + " index" + lowestRightPoint.index);
					newBreakList[newBreakListCount] = new BreakObject(startBreak, firstPassCircleIntersects[s]);
					newBreakListCount++;
					//newBreakList.Add(new BreakObject(startBreak, firstPassCircleIntersects[s]));
					//Debug.Log (newBreakList[newBreakList.Count - 1].start.index + "to" + newBreakList[newBreakList.Count - 1].end.index);
					continue;
				}
				//if first pass through loop, sets start break
				if (s == 0)
				{
					if (leftExplosionIntersectsCount % 2.0 != 0.0 && lowestLeftPoint.index <= firstPassCircleIntersects[0].index)//firstpasscircleintersects.count > 0
					{						
						startBreak = lowestLeftPoint;
						lowestLeftPointUsed = true;
						startBreakIsLeftPoint = true;
						Debug.Log ("starBreakIsLeftPoint");
						continue;
					}	
					else
					{
						startBreak = firstPassCircleIntersects[0];
						continue;
					}
				}
				//if accesing an exitCircle and is CounterClockwise, might need to be <= firstEntertolastexitangle(not really since if its equal to lastExitAngle it will go into another if statement and never see this one
				if (s % 2 != 0.0 && firstEnterToCurrentExitAngle < firstEnterToLastExitAngle)
				{
					if (!lowestRightPointUsed && lowestRightPoint.index != 666 && firstPassCircleIntersects[s + 1].index > lowestRightPoint.index && firstPassCircleIntersects[s + 1].index < firstPassCircleIntersects[s + 2].index)
					{
						if (lowestRightPoint.point.x == 0)
						{
							Debug.Log ("s3 " + s);
						}
						newBreakList[newBreakListCount] = new BreakObject(startBreak, lowestRightPoint);
						newBreakListCount++;
						//newBreakList.Add(new BreakObject(startBreak, lowestRightPoint));
						lowestRightPointUsed = true;
						//Debug.Log (newBreakList[newBreakList.Count - 1].start.index + "to" + newBreakList[newBreakList.Count - 1].end.index);
						if (!lowestLeftPointUsed && firstPassCircleIntersects[s + 1].index >= lowestLeftPoint.index)
						{
							Debug.Log ("should never happen?====lowestLeft = startBreak but not first break");
							startBreak = lowestLeftPoint;
							lowestLeftPointUsed = true;
						}
						else
						{
							startBreak = firstPassCircleIntersects[s + 1];
						}
						s++;
						continue;
					}
					else
					{
						//might need to put OR statement in its own if statement it appears to work, for now, need more thinking
						if (startBreakIsLeftPoint == true || (!lowestRightPointUsed && lowestRightPoint.index != 666))
						{							
							if (points[firstPassCircleIntersects[s].index + 1].y < mousePosition.y)
							{
								newBreakList[newBreakListCount] = new BreakObject(startBreak, firstPassCircleIntersects[s]);
								newBreakListCount++;
								//newBreakList.Add(new BreakObject(startBreak, firstPassCircleIntersects[s]));
								//Debug.Log (newBreakList[newBreakList.Count - 1].start.index + "to" + newBreakList[newBreakList.Count - 1].end.index);
								startBreak = firstPassCircleIntersects[s + 1];
								startBreakIsLeftPoint = false;
								s++;
								continue;
							}
							else 
							{
								Debug.Log ("didn't break, not lower than bottom of circle");
								s++;
								continue;
							}
						}
						
						if (firstPassCircleIntersects[s].point.x == 0)
						{
							Debug.Log ("s4 " + s);
						}
						
						newBreakList[newBreakListCount] = new BreakObject(startBreak, firstPassCircleIntersects[s]);
						newBreakListCount++;
						//newBreakList.Add(new BreakObject(startBreak, firstPassCircleIntersects[s]));
						
						if (!lowestLeftPointUsed && firstPassCircleIntersects[s + 1].index >= lowestLeftPoint.index)
						{
							Debug.Log ("====lowestLeft = startBreak but not first break");
							startBreak = lowestLeftPoint;
							lowestLeftPointUsed = true;
						}
						else
						{
							startBreak = firstPassCircleIntersects[s + 1];
						}
						
						//Debug.Log (newBreakList[newBreakList.Count - 1].start.index + "to" + newBreakList[newBreakList.Count - 1].end.index);
						//startBreak = firstPassCircleIntersects[s + 1];
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
			explosionList[explosionListAndCenterCount] = newBreakList;
			explosionCenter[explosionListAndCenterCount] = mousePosition;
			explosionListAndCenterCount++;
			//explosionList.Add (newBreakList);
			//explosionCenter.Add(mousePosition);
			//deletePoints (newBreakList, mousePosition);
			
				

			firstPassCircleIntersectsCount = 0;
			
			//fullMagicListCount[] = 0;
			newBreakListCount = 0;
			leftExplosionIntersectsCount = 0;
			rightExplosionIntersectsCount = 0;
		}
	}

	
	//currently if the same start and end are in multple breaks, it will keep them all, but it will just delete and make same points again
	//will want to get rid of this for speeder maybe later tuts
	void deletePoints(BreakObject[] newBreakList, Vector2[] mousePosition, int newBreakListCount)
	{
		//determine which indexes fall within others, because when break it adds points at start and end
		//List<int> insideIndexes = new List<int>();
		for (int i = 0; i < newBreakListCount; i++)
		{
			Debug.Log ("before " + i);
			Debug.Log ("start " + newBreakList[i].start.index + " end " + newBreakList[i].end.index);
			//Debug.Log ("end" + newBreakList[i].end.point.x);
			for (int j = 0; j < newBreakListCount; j++)
			{
				if (j == i)
				{
					continue;
				}
				if (newBreakList[i].start.index >= newBreakList[j].start.index && newBreakList[i].end.index <= newBreakList[j].end.index)
				{
					if (newBreakList[i].start.index == newBreakList[j].start.index && newBreakList[i].end.index == newBreakList[j].end.index)
					{
						newBreakList[i] = null;
						//newBreakList.RemoveAt(i);
						i--;
						Debug.Log ("i got deleted for not being unique");
						break;
						//continue;
					}
					newBreakList[i] = null;
					Debug.Log ("insideIndex " + i);
					//newBreakList.RemoveAt (i);
					i--;
					break;
					//insideIndexes.Add (i);
				}
			}
		}
		//insideIndexes.Sort ();
		//HashSet<int> hset = new HashSet<int>(insideIndexes);
		//List<int> uniqueBreakIndex = hset.ToList ();
		//sort list in numerical order so as not to fuck with order due to list compression after removeAt
		//uniqueBreakIndex.Sort ();
//		for (int i = insideIndexes.Count - 1; i >= 0; i--)
//		{
//			newBreakList.RemoveAt (insideIndexes[i]);
//		}
		
		for (int i = 0; i < newBreakListCount; i++)
		{
			Debug.Log ("after " + i);
			Debug.Log ("start " + newBreakList[i].start.index + " end " + newBreakList[i].end.index);
			//Debug.Log ("end" + newBreakList[i].end.point.x);
		}
		
		quickSort (newBreakList, 0, newBreakListCount - 1);
		//newBreakList.Sort ((x,y) => x.start.index == y.start.index ? 0 : (x.start.index < y.start.index ? -1 : 1));
				
		for (int i = 0; i < newBreakListCount; i++)
		{	
			Debug.Log ("afterSort " + i);
			Debug.Log ("start " + newBreakList[i].start.index + " end " + newBreakList[i].end.index);
			//Debug.Log ("end" + newBreakList[i].end.point.x);
		}
		
		
		//might want to pass in mouse position so it isnt possible to be different
		//Vector3 mousePositionTemp = Camera.mainCamera.ScreenToWorldPoint(Input.mousePosition);
		//Vector2 mousePosition;
		//mousePosition.x = mousePositionTemp.x;
		//mousePosition.y = mousePositionTemp.y;
		
		for (int i = newBreakListCount - 1; i >= 0; i--)
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
			//might need to be >=
			if 	(newBreakList[i].start.index > newBreakList[i].end.index)
			{
				//if y value is supposed to be below terrain, make it random number from 1 to 3
				if (newBreakList[i].end.point.y > 1.0f)
				{
					points.Insert(newBreakList[i].start.index, new Vector2(newBreakList[i].end.point.x, newBreakList[i].end.point.y));
				}
				else
				{
					points.Insert(newBreakList[i].start.index, new Vector2(newBreakList[i].end.point.x, 1.0f));//Random.Range(0.0f, 3.0f)));
				}
				//1-12 got rid off since start is greater than end index
//				for (float j = newBreakList[i].end.point.x - 1; j >= newBreakList[i].start.point.x; j--)
//				{
//					//Debug.Log ("j-mouseX" + (j-mousePosition.x));
//					if (j - mousePosition.x < explosionRadius && j - mousePosition.x > -explosionRadius)
//					{
//						//Debug.Log ("J" + j);
//						float y = Mathf.Sqrt (Mathf.Pow(explosionRadius, 2) - Mathf.Pow(j - mousePosition.x, 2));
//						points.Insert(newBreakList[i].start.index, new Vector2(j, mousePosition.y - y));
//					}
//				}
				//float y2 = Mathf.Sqrt (Mathf.Pow(explosionRadius, 2) - Mathf.Pow(newBreakList[i].start.point.x - mousePosition.x, 2));
				//points.Insert(newBreakList[i].start.index, new Vector2(newBreakList[i].start.point.x, mousePosition.y));// - y2));
				if (newBreakList[i].start.point.y > 1.0f)
				{
					points.Insert(newBreakList[i].start.index, new Vector2(newBreakList[i].start.point.x, newBreakList[i].start.point.y));
				}
				else
				{
					points.Insert(newBreakList[i].start.index, new Vector2(newBreakList[i].start.point.x, 1.0f));//Random.Range(0.0f, 3.0f)));
				}
				//buildTerrainMesh();
				continue;
			}
			
			Debug.Log ("remove " + newBreakList[i].end.index + "to " + newBreakList[i].start.index);
			
			for (int k = newBreakList[i].end.index; k >= newBreakList[i].start.index; k--)
			{
				points.RemoveAt (k);
			}
			
			//add this if statement if yuo think you are putting a point too close to another and getting terrainBuild error
			//if (Mathf.Abs (newBreakList[i].end.point.x - points[newBreakList[i].start.index+1].x) > .001 && Mathf.Abs (newBreakList[i].end.point.y - points[newBreakList[i].start.index+1].y) > .001)
			//{
			if (newBreakList[i].end.point.y > 1.0f)
			{
				points.Insert(newBreakList[i].start.index, new Vector2(newBreakList[i].end.point.x, newBreakList[i].end.point.y));
			}
			else
			{
				points.Insert(newBreakList[i].start.index, new Vector2(newBreakList[i].end.point.x, 1.0f));//Random.Range(0.0f, 3.0f)));
			}
			//}
			
			//points.Insert(newBreakList[i].start.index, new Vector2(newBreakList[i].end.point.x, mousePosition.y));
			//.1 used to be 1, changed it because steep slopes when click beneath terrain, then click a bit up, weird shit happens for circle intersections and left and right, just try it and see nucca
			for (float j = newBreakList[i].end.point.x - .1f; j >= newBreakList[i].start.point.x; j--)
			{
//				if (j - mousePosition.x < explosionRadius && j - mousePosition.x > -explosionRadius)
//				{
					//Debug.Log ("J" + j);
				float y = Mathf.Sqrt (Mathf.Pow(explosionRadius, 2) - Mathf.Pow(j - mousePosition[i].x, 2));
				if (mousePosition[i].y - y > 1.0f)
				{
					points.Insert(newBreakList[i].start.index, new Vector2(j, mousePosition[i].y - y));
				}
				else
				{
					points.Insert(newBreakList[i].start.index, new Vector2(j, 1.0f));//Random.Range(0.0f, 3.0f)));
				}
//				}
			}
			//float y3 = Mathf.Sqrt (Mathf.Pow(explosionRadius, 2) - Mathf.Pow(newBreakList[i].start.point.x - mousePosition.x, 2));
			//points.Insert(newBreakList[i].start.index, new Vector2(newBreakList[i].start.point.x, mousePosition.y));// - y3));
			if (newBreakList[i].start.point.y > 1.0f)
			{
				points.Insert(newBreakList[i].start.index, new Vector2(newBreakList[i].start.point.x, newBreakList[i].start.point.y));
			}
			else
			{
				points.Insert(newBreakList[i].start.index, new Vector2(newBreakList[i].start.point.x, 1.0f));//Random.Range(0.0f, 3.0f)));
			}

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
		
		//if terrain is fail, check to see if you can draw a straight line from start to end, if not, delete points between intersection and one of those,then hook it up  this is probably done wrong
		//right now this might only work when curve is in correct direction
	 	bool success = false;
	 	success = buildTerrainMesh();
		if (success == false)
		{
			Debug.Log ("================================================BUILDTERRAINFAILED");
			for (int i = 0; i < points.Count - 1; i++)
			{
				Vector2 intersectionPoint = segmentIntersection (points[i].x, points[i].y, points[i + 1].x, points[i + 1].y, newBreakList[0].start.point.x, newBreakList[0].start.point.y, newBreakList[newBreakListCount - 1].end.point.x, newBreakList[newBreakListCount - 1].end.point.y);
				if (intersectionPoint != new Vector2(666,666))
				{
					points.RemoveRange (newBreakList[newBreakListCount - 1].end.index, i);
					points.Insert(i, intersectionPoint);
				}
			}
			if (buildTerrainMesh())
			{
				Debug.Log ("magicPoint2 success");
			}
			else
			{
				Debug.Log ("magicPoint2 failure");	
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
	void quickSort(BreakObject[] arr, int left, int right) 
	{
      int i = left, j = right;
      BreakObject tmp;
      int pivot = arr[(left + right) / 2].start.index;
 
      /* partition */
      while (i <= j) {
            while (arr[i].start.index < pivot)
                  i++;
            while (arr[j].start.index > pivot)
                  j--;
            if (i <= j) {
                  tmp = arr[i];
                  arr[i] = arr[j];
                  arr[j] = tmp;
                  i++;
                  j--;
            }
      }
 
      /* recursion */
      if (left < j)
            quickSort(arr, left, j);
      if (i < right)
            quickSort(arr, i, right);
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