//TODO: why did i have to make the constructor for my P&I and BREAKOBJECT public

//possibly points are getting draw at the same location so the terrain fails, fix it, use EPSILON to add or subtract x or some shit based on direction(i dont think this happens an e mo)

//worst comes to worst you cango through all thep oints and if two are the same that are next to eachother make one off a bit so you can build the terrain

//circle intersects can equal 1 if it is on the right or left of the terrain, but below enough so that its still
//only one intersection, will probably fix the same way that I will fix thebottom of the terrain being a problem

//can still getoddnumber circle intersects

//reintroduced left cant be anything but first
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
	public List<Vector2> points = new List<Vector2>();
	List<Vector2> previousPoints = new List<Vector2>();
	GameObject[] segments;

	PointAndIndex[] firstPassCircleIntersects = new PointAndIndex[100];
	PointAndIndex[,] fullMagicList = new PointAndIndex[100,100];
	BreakObject[] newBreakList = new BreakObject[100];
	PointAndIndex[] leftExplosionIntersects = new PointAndIndex[100];
	PointAndIndex[] rightExplosionIntersects = new PointAndIndex[100];
	int firstPassCircleIntersectsCount;
	int[] fullMagicListCountArray = new int[100];
	int fullMagicListCount;
	int newBreakListCount;//need to somehow pass this into delete method while not resetting at end of mmouseinput?
	int leftExplosionIntersectsCount;
	int rightExplosionIntersectsCount;

	//List<List<PointAndIndex>> fullMagicList = new List<List<PointAndIndex>>();

	public GameObject vectorFieldObject;

	bool doneGenerating = false;


	// Use this for initialization
	void Start ()
	{
		
		for (int i = 0; i < 50; i++)
		{
			firstPassCircleIntersects[i] = new PointAndIndex(new Vector2(999,999), 999);
			//newBreakList[i] = new BreakObject(new PointAndIndex(new Vector2(999,999), 999),new PointAndIndex(new Vector2(999,999), 999));
			leftExplosionIntersects[i] = new PointAndIndex(new Vector2(999,999), 999);
			rightExplosionIntersects[i] = new PointAndIndex(new Vector2(999,999), 999);
		}
		
		
		segmentWidth = width/numSegments;
	 	bool success = false;
	 	int count = 0;
	 	while(success == false && count < 10)
	 	{
       		if(count > 0) Debug.Log("Terrain generation failed, trying again");
			if(count > 8) Debug.Log ("PROBLEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEMMMMMMMMMMMMM");
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

	}

	void OnPlayerConnected()
	{
		Debug.Log("yofhjd");
		for (int i = 0; i < points.Count; i++)
		{
			networkView.RPC("SendTerrain", RPCMode.Others, points[i].x, i, points[i].y, points.Count);
		}
	}
	
	[RPC]
	void SendTerrain(float pointX, int placement, float pointY, int pointsCount)
	{
		Vector2 pizoint = new Vector2 (pointX , pointY);
		Debug.Log("yoyoy");
		points[placement] = pizoint;
		if (placement == pointsCount - 1 && pointsCount < points.Count)
		{
			points.RemoveRange(pointsCount - 1, (points.Count - 1) - (pointsCount - 1));
		}
		if (placement >= points.Count)
		{
			points.Add (pizoint);
		}
	}
	
	// Update is called once per frame
	void Update()
	{
		// indexes selected are the ones you want to destroy		
		if (Input.GetMouseButtonDown(0))
		{			
			Vector3 mousePositionTemp = Camera.mainCamera.ScreenToWorldPoint(Input.mousePosition);
			//optimize for if sircle is no where near terrain possibly, just return
			if (mousePositionTemp.y < 1.0f)
			{
				return;
			}
			Vector2 mousePosition;
			mousePosition.x = mousePositionTemp.x;
			mousePosition.y = mousePositionTemp.y;
			//used to determine if the line segment and next line segment touch the circle but stay on the same side
			bool previousHadOneEqualTo = false;
			bool previousStartInsideRadius = false;

			//first pass through Points, determines all Left, Right, and Circle Intersections
			for (int i = 0; i < points.Count - 1; i++)
			{
				//used to determine if the line segment and next line segment touch the circle but stay on the same side
				bool thisStartsInsideRadius = false;
				bool thisEndsInsideRadius = false;
				bool thisHadOneEqualTo = false;

				float leftExplosionRadiusX = mousePosition.x - explosionRadius;
				float rightExplosionRadiusX = mousePosition.x + explosionRadius;
				float percentageAcrossRightX = (rightExplosionRadiusX - points[i].x) / (points[i + 1].x - points[i].x);
				float percentageAcrossLeftX = (leftExplosionRadiusX - points[i].x) / (points[i + 1].x - points[i].x);

				//left circle upwards Intersections
				//uses end points if they are equal to
				if (((points[i].x < leftExplosionRadiusX && points[i + 1].x >= leftExplosionRadiusX) || (points[i].x > leftExplosionRadiusX && points[i + 1].x <= leftExplosionRadiusX)) && (points[i + 1].y - points[i].y) * percentageAcrossLeftX + points[i].y > mousePosition.y)
				{
					float intersectLeftX = leftExplosionRadiusX;
					float intersectLeftY = ((leftExplosionRadiusX - points[i].x) / (points[i + 1].x - points[i].x)) * (points[i + 1].y - points[i].y) + points[i].y;
					// = new PointAndIndex(new Vector2(intersectLeftX, intersectLeftY), i + 1);
					leftExplosionIntersects[leftExplosionIntersectsCount].point.x = intersectLeftX;
					leftExplosionIntersects[leftExplosionIntersectsCount].point.y = intersectLeftY;
					leftExplosionIntersects[leftExplosionIntersectsCount].index = i + 1;
					leftExplosionIntersectsCount++;
				}
				//right circle upwards Intersections
				//uses start points if they are equal to
				if (((points[i].x <= rightExplosionRadiusX && points[i + 1].x > rightExplosionRadiusX) || (points[i].x >= rightExplosionRadiusX && points[i + 1].x < rightExplosionRadiusX)) && (points[i + 1].y - points[i].y) * percentageAcrossRightX + points[i].y > mousePosition.y)
				{
					float intersectRightX = rightExplosionRadiusX;
					float intersectRightY = ((rightExplosionRadiusX - points[i].x) / (points[i + 1].x - points[i].x)) * (points[i + 1].y - points[i].y) + points[i].y;
					// = new PointAndIndex(new Vector2(intersectRightX, intersectRightY), i);
					rightExplosionIntersects[rightExplosionIntersectsCount].point.x = intersectRightX;
					rightExplosionIntersects[rightExplosionIntersectsCount].point.y = intersectRightY;
					rightExplosionIntersects[rightExplosionIntersectsCount].index = i;
					rightExplosionIntersectsCount++;	
				}


				//line segment Circle intersections
				float x0 = points[i].x - mousePosition.x;
				float y0 = points[i].y - mousePosition.y;
				float x1 = points[i+1].x - mousePosition.x;
				float y1 = points[i+1].y - mousePosition.y;
				float r = explosionRadius;

				if (Mathf.Sqrt (x0*x0 + y0*y0) < explosionRadius)
				{
					thisStartsInsideRadius = true;
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
				float dR = Mathf.Sqrt (dX*dX + dY*dY);
				float d = x0*y1 - x1*y0;
				float rSquared = r*r;
				float dRSquared = dR*dR;
				float dSquared = d*d;
				float incidence = r*r * dR*dR - d*d;
				float sqrtMath = Mathf.Sqrt (rSquared * dRSquared - dSquared);
				float resultingX1 = 0;
				float resultingX2 = 0;
				float resultingY1 = 0;
				float resultingY2 = 0;

				if (incidence > 0)
				{
					resultingX1 = (d * dY + Sgn(dY) * dX * sqrtMath) / (dRSquared);
					resultingY1 = -(d * dX - Mathf.Abs (dY) * sqrtMath) / (dRSquared);
					resultingX2 = (d * dY - Sgn(dY) * dX * sqrtMath) / (dRSquared);
					resultingY2 = -(d * dX + Mathf.Abs (dY) * sqrtMath) / (dRSquared);

					float distance01 = Mathf.Sqrt((resultingX1 - x0) * (resultingX1 - x0) + (resultingY1 - y0) * (resultingY1 - y0));
					float distance02 = Mathf.Sqrt((resultingX2 - x0) * (resultingX2 - x0) + (resultingY2 - y0) * (resultingY2 - y0));

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

					//if x is = both then just check if its in between the ys, same for the other way 'round(because vertical lines wont be greater than start x it will be equal to, but it checks > start and<=end)
					if (resultingX1 <= x0 + EPSILON && resultingX1 >= x0 - EPSILON && resultingX1 <= x1 + EPSILON && resultingX1 >= x1 - EPSILON)
					{
						if (resultingY1 > y0 && resultingY1 <= y1 || resultingY1 >= y1 && resultingY1 < y0)
						{
							firstIsOnLineSegment = true;
						}
					}
					if (resultingX2 <= x0 + EPSILON && resultingX2 >= x0 - EPSILON && resultingX2 <= x1 + EPSILON && resultingX2 >= x1 - EPSILON)
					{
						if (resultingY2 > y0 && resultingY2 <= y1 || resultingY2 >= y1 && resultingY2 < y0)
						{
							secondIsOnLineSegment = true;
						}
					}
					if (resultingY1 <= y0 + EPSILON && resultingY1 >= y0 - EPSILON && resultingY1 <= y1 + EPSILON && resultingY1 >= y1 - EPSILON)
					{
						if (resultingX1 > x0 && resultingX1 <= x1 || resultingX1 >= x1 && resultingX1 < x0)
						{
							firstIsOnLineSegment = true;
						}
					}
					if (resultingY2 <= y0 + EPSILON && resultingY2 >= y0 - EPSILON && resultingY2 <= y1 + EPSILON && resultingY1 >= y1 - EPSILON)
					{
						if (resultingX2 > x0 && resultingX2 <= x1 || resultingX2 >= x1 && resultingX2 < x0)
						{
							secondIsOnLineSegment = true;
						}
					}

					if (!firstIsOnLineSegment && secondIsOnLineSegment || firstIsOnLineSegment && !secondIsOnLineSegment)
					{
						thisHadOneEqualTo = true;
					}

					//ignore the starts of the linesegments and only use the ends if equal to
					if ((resultingX1 > x0 && resultingX1 <= x1 && resultingY1 > y0 && resultingY1 <= y1) || (resultingX1 < x0 && resultingX1 >= x1 && resultingY1 < y0 && resultingY1 >= y1) || (resultingX1 < x0 && resultingX1 >= x1 && resultingY1 > y0 && resultingY1 <= y1) || (resultingX1 > x0 && resultingX1 <= x1 && resultingY1 < y0 && resultingY1 >= y1))
					{
						firstIsOnLineSegment = true;
					}
					if ((resultingX2 > x0 && resultingX2 <= x1 && resultingY2 > y0 && resultingY2 <= y1) || (resultingX2 < x0 && resultingX2 >= x1 && resultingY2 < y0 && resultingY2 >= y1) || (resultingX2 < x0 && resultingX2 >= x1 && resultingY2 > y0 && resultingY2 <= y1) || (resultingX2 > x0 && resultingX2 <= x1 && resultingY2 < y0 && resultingY2 >= y1))
					{
						secondIsOnLineSegment = true;
					}
				}
				//if two Intersections
				if (firstIsOnLineSegment && secondIsOnLineSegment)
				{
					thisHadOneEqualTo = false;
					// if Count is even
					if (firstPassCircleIntersectsCount % 2.0 == 0.0)
					{
						// = new PointAndIndex(new Vector2(resultingX1 + mousePosition.x, resultingY1 + mousePosition.y), i + 1);
						firstPassCircleIntersects[firstPassCircleIntersectsCount].point.x = resultingX1 + mousePosition.x;
						firstPassCircleIntersects[firstPassCircleIntersectsCount].point.y = resultingY1 + mousePosition.y;
						firstPassCircleIntersects[firstPassCircleIntersectsCount].index = i + 1;
						firstPassCircleIntersectsCount++;
						fullMagicListCount++;
						if (resultingX2 == x1 && resultingY2 == y1)
						{
							// = new PointAndIndex(new Vector2(resultingX2 + mousePosition.x, resultingY2 + mousePosition.y), i + 1);
							firstPassCircleIntersects[firstPassCircleIntersectsCount].point.x = resultingX2 + mousePosition.x;
							firstPassCircleIntersects[firstPassCircleIntersectsCount].point.y = resultingY2 + mousePosition.y;
							firstPassCircleIntersects[firstPassCircleIntersectsCount].index = i + 1;
							firstPassCircleIntersectsCount++;
							fullMagicListCount++;
						}
						else
						{
							// = new PointAndIndex(new Vector2(resultingX2 + mousePosition.x, resultingY2 + mousePosition.y), i);
							firstPassCircleIntersects[firstPassCircleIntersectsCount].point.x = resultingX2 + mousePosition.x;
							firstPassCircleIntersects[firstPassCircleIntersectsCount].point.y = resultingY2 + mousePosition.y;
							firstPassCircleIntersects[firstPassCircleIntersectsCount].index = i;
							firstPassCircleIntersectsCount++;
							fullMagicListCount++;
						}
					}
					//if Count is odd
					else
					{
						// = new PointAndIndex(new Vector2(resultingX1 + mousePosition.x, resultingY1 + mousePosition.y), i + 1);//i
						firstPassCircleIntersects[firstPassCircleIntersectsCount].point.x = resultingX1 + mousePosition.x;
						firstPassCircleIntersects[firstPassCircleIntersectsCount].point.y = resultingY1 + mousePosition.y;
						firstPassCircleIntersects[firstPassCircleIntersectsCount].index = i + 1;						
						firstPassCircleIntersectsCount++;
						fullMagicListCount++;
						// = new PointAndIndex(new Vector2(resultingX2 + mousePosition.x, resultingY2 + mousePosition.y), i);//i+1
						firstPassCircleIntersects[firstPassCircleIntersectsCount].point.x = resultingX2 + mousePosition.x;
						firstPassCircleIntersects[firstPassCircleIntersectsCount].point.y = resultingY2 + mousePosition.y;
						firstPassCircleIntersects[firstPassCircleIntersectsCount].index = i;
						firstPassCircleIntersectsCount++;
						fullMagicListCount++;
					}
				}
				// if only first Intersection
				if (firstIsOnLineSegment && !secondIsOnLineSegment)
				{
					if (firstPassCircleIntersectsCount % 2.0 == 0.0)
					{
						// = new PointAndIndex(new Vector2(resultingX1 + mousePosition.x, resultingY1 + mousePosition.y), i + 1);
						firstPassCircleIntersects[firstPassCircleIntersectsCount].point.x = resultingX1 + mousePosition.x;
						firstPassCircleIntersects[firstPassCircleIntersectsCount].point.y = resultingY1 + mousePosition.y;
						firstPassCircleIntersects[firstPassCircleIntersectsCount].index = i + 1;
						firstPassCircleIntersectsCount++;
						fullMagicListCount++;
					}
					else
					{
						if (resultingX1 == x1 && resultingY1 == y1)
						{
							// = new PointAndIndex(new Vector2(resultingX1 + mousePosition.x, resultingY1 + mousePosition.y), i + 1);
							firstPassCircleIntersects[firstPassCircleIntersectsCount].point.x = resultingX1 + mousePosition.x;
							firstPassCircleIntersects[firstPassCircleIntersectsCount].point.y = resultingY1 + mousePosition.y;
							firstPassCircleIntersects[firstPassCircleIntersectsCount].index = i + 1;
							firstPassCircleIntersectsCount++;
							fullMagicListCount++;
						}
						else
						{
							// = new PointAndIndex(new Vector2(resultingX1 + mousePosition.x, resultingY1 + mousePosition.y), i);
							firstPassCircleIntersects[firstPassCircleIntersectsCount].point.x = resultingX1 + mousePosition.x;
							firstPassCircleIntersects[firstPassCircleIntersectsCount].point.y = resultingY1 + mousePosition.y;
							firstPassCircleIntersects[firstPassCircleIntersectsCount].index = i;
							firstPassCircleIntersectsCount++;
							fullMagicListCount++;
						}
					}
				}
				//if only second intersection
				if (!firstIsOnLineSegment && secondIsOnLineSegment)
				{
					if (firstPassCircleIntersectsCount % 2.0 == 0.0)
					{
						// = new PointAndIndex(new Vector2(resultingX2 + mousePosition.x, resultingY2 + mousePosition.y), i + 1);
						firstPassCircleIntersects[firstPassCircleIntersectsCount].point.x = resultingX2 + mousePosition.x;
						firstPassCircleIntersects[firstPassCircleIntersectsCount].point.y = resultingY2 + mousePosition.y;
						firstPassCircleIntersects[firstPassCircleIntersectsCount].index = i + 1;
						firstPassCircleIntersectsCount++;
						fullMagicListCount++;
					}
					else
					{
						if (resultingX2 == x1 && resultingY2 == y1)
						{
							// = new PointAndIndex(new Vector2(resultingX2 + mousePosition.x, resultingY2 + mousePosition.y), i + 1);
							firstPassCircleIntersects[firstPassCircleIntersectsCount].point.x = resultingX2 + mousePosition.x;
							firstPassCircleIntersects[firstPassCircleIntersectsCount].point.y = resultingY2 + mousePosition.y;
							firstPassCircleIntersects[firstPassCircleIntersectsCount].index = i + 1;
							firstPassCircleIntersectsCount++;
							fullMagicListCount++;
						}
						else
						{
							// = new PointAndIndex(new Vector2(resultingX2 + mousePosition.x, resultingY2 + mousePosition.y), i);
							firstPassCircleIntersects[firstPassCircleIntersectsCount].point.x = resultingX2 + mousePosition.x;
							firstPassCircleIntersects[firstPassCircleIntersectsCount].point.y = resultingY2 + mousePosition.y;
							firstPassCircleIntersects[firstPassCircleIntersectsCount].index = i;
							firstPassCircleIntersectsCount++;
							fullMagicListCount++;
						}
					}
				}
				//in order to ignore the intersect if it doesnt fully cross butt-just lands on an endpoint and then goes back the way it came
				if (previousHadOneEqualTo && previousStartInsideRadius && ((!firstIsOnLineSegment && secondIsOnLineSegment || firstIsOnLineSegment && !secondIsOnLineSegment) || ((!firstIsOnLineSegment && !secondIsOnLineSegment) && thisEndsInsideRadius)))
				{
					firstPassCircleIntersectsCount--;
				}
				if (previousHadOneEqualTo && !previousStartInsideRadius && (!firstIsOnLineSegment && !secondIsOnLineSegment) && !thisEndsInsideRadius)
				{
					firstPassCircleIntersectsCount--;
				}
				previousStartInsideRadius = thisStartsInsideRadius;
				previousHadOneEqualTo = thisHadOneEqualTo;
			}

			//had to do 666 thing because below this it cant use unassigned even though it would never go into that loop unless it has already gone through this one
			PointAndIndex lowestLeftPoint = new PointAndIndex(new Vector2(666,666), 666);
			PointAndIndex lowestRightPoint = new PointAndIndex(new Vector2(666,666), 666);
			//if leftExplosionIntersects are odd number above, set lowest Point
			if (leftExplosionIntersectsCount % 2.0 != 0.0)
			{
				lowestLeftPoint = leftExplosionIntersects[0];
				for (int j = 1; j < leftExplosionIntersectsCount; j++)
				{
					if (leftExplosionIntersects[j].point.y < lowestLeftPoint.point.y)
					{
						lowestLeftPoint = leftExplosionIntersects[j];
					}
				}
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
			}			

			//if circleIntersects gets odd number of intersects you are going to want to ignore the shot
			//it's sad, i know, but it's basically perfect except when you click the same spot a bunch of times so fuck you
			//may want to eventually gowith left andrightbreak if !=666, miunno
			if (firstPassCircleIntersectsCount % 2 !=0.0)
			{
				Debug.Log ("ODD # of circleIntersectssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssss");
				//previousPoints.Clear ();
				firstPassCircleIntersectsCount = 0;
				//fullMagicList.Clear ();
				newBreakListCount = 0;
				leftExplosionIntersectsCount = 0;
				rightExplosionIntersectsCount = 0;
				for (int i = 0; i < fullMagicListCount; i++)
				{
					fullMagicListCountArray[i] = 0;
				}
				fullMagicListCount = 0;
				return;
			}			


			//if circle is completely in between two indexes on points
			if (lowestRightPoint.index != 666 && lowestLeftPoint.index !=666 && lowestRightPoint.index < lowestLeftPoint.index)
			{
				newBreakList[newBreakListCount] = new BreakObject(lowestLeftPoint, lowestRightPoint);
				newBreakListCount++;
				//Debug.Log (newBreakList[newBreakListCount - 1].start.index + "to" + newBreakList[newBreakListCount - 1].end.index);
				deletePoints (newBreakList, mousePosition, newBreakListCount);
				//fullMagicList.Clear ();
				firstPassCircleIntersectsCount = 0;
				newBreakListCount = 0;
				leftExplosionIntersectsCount = 0;
				rightExplosionIntersectsCount = 0;				
				for (int i = 0; i < fullMagicListCount; i++)
				{
					fullMagicListCountArray[i] = 0;
				}
				fullMagicListCount = 0;
				return;
			}

			//if no circleIntersects then it's an easy break and quit
			if (firstPassCircleIntersectsCount == 0)
			{
				if (leftExplosionIntersectsCount % 2.0 == 0.0 && rightExplosionIntersectsCount % 2.0 == 0.0)
				{
					//fullMagicList.Clear ();
					firstPassCircleIntersectsCount = 0;			
					newBreakListCount = 0;
					leftExplosionIntersectsCount = 0;
					rightExplosionIntersectsCount = 0;
					for (int i = 0; i < fullMagicListCount; i++)
					{
						fullMagicListCountArray[i] = 0;
					}
					fullMagicListCount = 0;
					return;
				}
				else
				{
					newBreakList[newBreakListCount] = new BreakObject(lowestLeftPoint, lowestRightPoint);
					newBreakListCount++;
					//Debug.Log (newBreakList[newBreakListCount - 1].start.index + "to" + newBreakList[newBreakListCount - 1].end.index);
					deletePoints (newBreakList, mousePosition, newBreakListCount);
					//fullMagicList.Clear ();
					firstPassCircleIntersectsCount = 0;			
					newBreakListCount = 0;
					leftExplosionIntersectsCount = 0;
					rightExplosionIntersectsCount = 0;
					for (int i = 0; i < fullMagicListCount; i++)
					{
						fullMagicListCountArray[i] = 0;
					}
					fullMagicListCount = 0;
					return;
				}
			}
//			Debug.Log (firstPassCircleIntersectsCount + "COUNT");
//			//for debug purposes: turns on circle intersect debug information
//			for (int l = 0; l < firstPassCircleIntersectsCount; l++)
//			{
//				//Debug.Log("circleI x" + firstPassCircleIntersects[l].point.x + " y" + firstPassCircleIntersects[l].point.y + " index" + firstPassCircleIntersects[l]);
//				GameObject PooChain11 = (GameObject)Instantiate(PooChainClone, new Vector3(firstPassCircleIntersects[l].point.x, firstPassCircleIntersects[l].point.y, -9.0f),Quaternion.identity);
//				PooChain11.renderer.material.color = Color.Lerp (Color.green, Color.red, 1.0f);
//				PooChain11.transform.localScale += new Vector3(1.0f,1.0f,1.0f);
//			}

			//second pass through Points, finding "magic" points
//			//might want to only use magic points if its the top half of the circle thats intersected, the way it is now is if any
//			//circle intersection has a magic point, might be able to cut performance but it works fine
			for (int m = 0; m < points.Count - 1; m++)
			{
				for (int n = 0; n < firstPassCircleIntersectsCount; n++)//could be fullMagicList.Count
				{
					//fixes it so it wont try to put a magic point at bottom left or bottom right index
					if (firstPassCircleIntersects[n].index == 0 || firstPassCircleIntersects[n].index == points.Count - 1)
					{
						continue;
					}
					//ignoring starts of line segments
					if ((points[m].x <= firstPassCircleIntersects[n].point.x && points[m + 1].x > firstPassCircleIntersects[n].point.x) || (points[m].x > firstPassCircleIntersects[n].point.x && points[m + 1].x <= firstPassCircleIntersects[n].point.x))
					{
						float intersectX = firstPassCircleIntersects[n].point.x;
						float intersectY = ((firstPassCircleIntersects[n].point.x - points[m].x) / (points[m + 1].x - points[m].x)) * (points[m + 1].y - points[m].y) + points[m].y;
						if (intersectY > firstPassCircleIntersects[n].point.y + .1f) //.01f needs to be epsilon maybe
						{
							fullMagicList[n,fullMagicListCountArray[n]] = (new PointAndIndex(new Vector2(intersectX, intersectY), m + 1));
							fullMagicListCountArray[n]++;
						}
					}
				}
			}

			//go through fullMagicList to see if there are any odd Counts and if so, determine the lowest Y value to use as
			//the magic value, then break every magic number to the number it was determined from

			//have to assign it or it wont work, ask zeb bout better waytadodis
			PointAndIndex magicFoundFromThisPoint = new PointAndIndex(new Vector2(0.0f, 0.0f), 1);

			for (int o = 0; o < firstPassCircleIntersectsCount; o++)//could use fullMagic.Count
			{	//if odd Count
				if (fullMagicListCountArray[o] % 2.0 != 0.0)
				{
					PointAndIndex lowestCircleIntersect = fullMagicList[o,0];

					for (int i = 0; i < fullMagicListCountArray[o]; i++)
					{
						if (fullMagicList[o,i].point.y < lowestCircleIntersect.point.y)
						{
							lowestCircleIntersect = fullMagicList[o,i];
						}
					}

					magicFoundFromThisPoint = firstPassCircleIntersects[o];

					if (lowestCircleIntersect.index > magicFoundFromThisPoint.index)
					{
						newBreakList[newBreakListCount] = new BreakObject(magicFoundFromThisPoint, lowestCircleIntersect);
						newBreakListCount++;
						//Debug.Log (newBreakList[newBreakListCount - 1].start.index + "to" + newBreakList[newBreakListCount - 1].end.index);
					}
					else
					{
						newBreakList[newBreakListCount] = new BreakObject(lowestCircleIntersect, magicFoundFromThisPoint);
						newBreakListCount++;
						//Debug.Log (newBreakList[newBreakListCount - 1].start.index + "to" + newBreakList[newBreakListCount - 1].end.index);
					}				
				}
			}
			//for enter and exit angle determination
			Vector2 firstEnter = new Vector2(firstPassCircleIntersects[0].point.x - mousePosition.x, firstPassCircleIntersects[0].point.y - mousePosition.y);
			float lastExitX = firstPassCircleIntersects[firstPassCircleIntersectsCount - 1].point.x - mousePosition.x;
			float lastExitY = firstPassCircleIntersects[firstPassCircleIntersectsCount - 1].point.y - mousePosition.y;
			Vector2 fromVector2 = firstEnter;
			Vector2 toVector2 = new Vector2(lastExitX, lastExitY);			
			float firstEnterToLastExitAngle = Vector2.Angle(fromVector2, toVector2);
			Vector3 cross = Vector3.Cross(fromVector2, toVector2);

			//cross.z>0 would be for clockwise angle?
			if (cross.z < 0)
			{
			    firstEnterToLastExitAngle = 360 - firstEnterToLastExitAngle;
			}

			//have to assign it or it wont work, ask zeb bout better waytadodis
			PointAndIndex startBreak = new PointAndIndex(new Vector2(0.0f, 0.0f), 1);

			bool lowestRightPointUsed = false;
			bool lowestLeftPointUsed = false;
			bool startBreakIsLeftPoint = false;

//			//it appears that i could combine this loop with the fullMagicList one above
			for (int s = 0; s < firstPassCircleIntersectsCount; s = s + 1)
			{
				Vector2 currentExitVector2 = (firstPassCircleIntersects[s].point - mousePosition);
				float firstEnterToCurrentExitAngle = Vector2.Angle(firstEnter, currentExitVector2);
				Vector3 cross1 = Vector3.Cross(firstEnter, currentExitVector2);
				if (cross1.z < 0)
				{
					firstEnterToCurrentExitAngle = 360 - firstEnterToCurrentExitAngle;
				}
				//if final pass through loop and lowestRight has not been used and does exist
				if (s == firstPassCircleIntersectsCount - 1 && !lowestRightPointUsed && lowestRightPoint.index != 666)
				{
					newBreakList[newBreakListCount] = new BreakObject(startBreak, lowestRightPoint);
					newBreakListCount++;
					Debug.Log (newBreakList[newBreakListCount - 1].start.index + "to" + newBreakList[newBreakListCount - 1].end.index);
					continue;
				}
				//if final pass through loop and lowestRight has been used or does not exist
				if (s == firstPassCircleIntersectsCount - 1 && (lowestRightPointUsed || lowestRightPoint.index == 666))
				{
					newBreakList[newBreakListCount] = new BreakObject(startBreak, firstPassCircleIntersects[s]);
					newBreakListCount++;
					Debug.Log (newBreakList[newBreakListCount - 1].start.index + "to" + newBreakList[newBreakListCount - 1].end.index);
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
						continue;
					}
					else
					{
						startBreak = firstPassCircleIntersects[0];
						continue;
					}
				}
				//if accesing an exitCircle and is CounterClockwise(but not last exit circle because it would go into other if statement above)
				//if there is a problem with weird terrain after an explosion, this is the cause, probably just want to completely rethink this whole section, also recently changed MagicPoitnsto Array instead of arrayLIst, but i went back in time and saw thtat it was having the problem before that but i figured i would just mention it
				if (s % 2 != 0.0 && firstEnterToCurrentExitAngle < firstEnterToLastExitAngle)
				{
					if (!lowestRightPointUsed && lowestRightPoint.index != 666 && firstPassCircleIntersects[s + 1].index > lowestRightPoint.index && firstPassCircleIntersects[s + 1].index < firstPassCircleIntersects[s + 2].index)
					{
						newBreakList[newBreakListCount] = new BreakObject(startBreak, lowestRightPoint);
						newBreakListCount++;
						lowestRightPointUsed = true;
						Debug.Log (newBreakList[newBreakListCount - 1].start.index + "to" + newBreakList[newBreakListCount - 1].end.index);
						if (!lowestLeftPointUsed && firstPassCircleIntersects[s + 1].index >= lowestLeftPoint.index)
						{
							Debug.Log ("should never happen?================lowestLeft = startBreak but not first break");//maybe?
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
						if ((startBreakIsLeftPoint == true) || (!lowestRightPointUsed && lowestRightPoint.index != 666))
						{
							if (points[firstPassCircleIntersects[s].index + 1].y < mousePosition.y)
							{
								newBreakList[newBreakListCount] = new BreakObject(startBreak, firstPassCircleIntersects[s]);
								newBreakListCount++;
								Debug.Log (newBreakList[newBreakListCount - 1].start.index + "to" + newBreakList[newBreakListCount - 1].end.index);
								startBreak = firstPassCircleIntersects[s + 1];
								startBreakIsLeftPoint = false;
								s++;
								continue;
							}
							else 
							{
								s++;
								continue;
							}
						}
						newBreakList[newBreakListCount] = new BreakObject(startBreak, firstPassCircleIntersects[s]);
						newBreakListCount++;
						if (!lowestLeftPointUsed && lowestLeftPoint.index != 666 && firstPassCircleIntersects[s + 1].index >= lowestLeftPoint.index)
						{
							Debug.Log ("used my kewl tricks");
							startBreak = lowestLeftPoint;
							lowestLeftPointUsed = true;
						}
						else
						{
							startBreak = firstPassCircleIntersects[s + 1];
						}
						Debug.Log (newBreakList[newBreakListCount - 1].start.index + "to" + newBreakList[newBreakListCount - 1].end.index);
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
					Debug.Log ("jew===========================------------===============================");
				}
			}
			
			for (int i = 0; i < points.Count; i++)
			{	
				previousPoints.Add (points[i]);
			}
			
			deletePoints (newBreakList, mousePosition, newBreakListCount);
			previousPoints.Clear ();
			firstPassCircleIntersectsCount = 0;
			//fullMagicList.Clear ();
			newBreakListCount = 0;
			leftExplosionIntersectsCount = 0;
			rightExplosionIntersectsCount = 0;
			for (int i = 0; i < fullMagicListCount; i++)
			{
				fullMagicListCountArray[i] = 0;
			}
			fullMagicListCount = 0;
		}
	}


	//currently if the same start and end are in multple breaks, it will keep them all, but it will just delete and make same points again
	//will want to get rid of this for speeder maybe later tuts
	void deletePoints(BreakObject[] newBreakList1, Vector2 mousePosition, int newBreakList1Count)
	{
		BreakObject[] finalBreakList = new BreakObject[100];
		int finalBreakListCount = 0;
		//determine which indexes fall within others, because when break it adds points at start and end
		for (int i = 0; i < newBreakList1Count; i++)
		{
			for (int j = 0; j < newBreakList1Count; j++)
			{
				if (j == i || newBreakList1[j] == null || newBreakList1[i] == null)
				{
					continue;
				}
				if (newBreakList1[i].start.index >= newBreakList1[j].start.index && newBreakList1[i].end.index <= newBreakList1[j].end.index)
				{
					if (newBreakList1[i].start.index == newBreakList1[j].start.index && newBreakList1[i].end.index == newBreakList1[j].end.index)
					{
						newBreakList1[i] = null;
						i--;
						break;
					}
					newBreakList1[i] = null;
					i--;
					break;
				}
			}
		}		
		for (int i = 0; i < newBreakList1Count; i++)
		{
			if (newBreakList1[i] == null)
			{
				continue;	
			}
			else
			{
				finalBreakList[finalBreakListCount] = newBreakList1[i];
				finalBreakListCount++;
			}
		}
		//sort because want them to be able to be deleted from max to min index
		quickSort (finalBreakList, 0, finalBreakListCount - 1);

		for (int i = finalBreakListCount - 1; i >= 0; i--)
		{
			if (finalBreakList[i] == null)
			{
				continue;	
			}

			//want to do no breaking, just want to add points accordingly since the points both fall
			//within the same line segment
			if 	(finalBreakList[i].start.index > finalBreakList[i].end.index)
			{
				//if y value is supposed to be below 1, make it 1 
				if (finalBreakList[i].end.point.y > 1.0f)
				{
					points.Insert(finalBreakList[i].start.index, new Vector2(finalBreakList[i].end.point.x, finalBreakList[i].end.point.y));
				}
				else
				{
					points.Insert(finalBreakList[i].start.index, new Vector2(finalBreakList[i].end.point.x, 1.0f));//Random.Range(0.0f, 3.0f)));
				}

				//1-17 this wasnt in when i went to clean up, but im pretty sure i need it since it needs to make a circle somehow, i was thinking itdidnt before
				for (float j = finalBreakList[i].end.point.x - .1f; j >= finalBreakList[i].start.point.x; j--)
				{
					if (j - mousePosition.x < explosionRadius && j - mousePosition.x > -explosionRadius)
					{
						float y = Mathf.Sqrt (Mathf.Pow(explosionRadius, 2) - Mathf.Pow(j - mousePosition.x, 2));
						points.Insert(finalBreakList[i].start.index, new Vector2(j, mousePosition.y - y));
					}
				}

				if (finalBreakList[i].start.point.y > 1.0f)
				{
					points.Insert(finalBreakList[i].start.index, new Vector2(finalBreakList[i].start.point.x, finalBreakList[i].start.point.y));
				}
				else
				{
					points.Insert(finalBreakList[i].start.index, new Vector2(finalBreakList[i].start.point.x, 1.0f));//Random.Range(0.0f, 3.0f)));
				}
				//buildTerrainMesh();
				continue;
			}

			//Debug.Log ("remove " + finalBreakList[i].end.index + "to " + finalBreakList[i].start.index);

			for (int k = finalBreakList[i].end.index; k >= finalBreakList[i].start.index; k--)
			{
				points.RemoveAt (k);
			}			

			if (finalBreakList[i].end.point.y > 1.0f)
			{
				points.Insert(finalBreakList[i].start.index, new Vector2(finalBreakList[i].end.point.x, finalBreakList[i].end.point.y));
			}
			else
			{
				points.Insert(finalBreakList[i].start.index, new Vector2(finalBreakList[i].end.point.x, 1.0f));//Random.Range(0.0f, 3.0f)));
			}

			//.1 used to be 1, changed it because steep slopes when click beneath terrain, then click a bit up, weird shit happens for circle intersections and left and right, just try it and see nucca
			for (float j = finalBreakList[i].end.point.x - .1f; j >= finalBreakList[i].start.point.x; j--)
			{
				float y = Mathf.Sqrt (Mathf.Pow(explosionRadius, 2) - Mathf.Pow(j - mousePosition.x, 2));
				if (mousePosition.y - y > 1.0f)
				{
					points.Insert(finalBreakList[i].start.index, new Vector2(j, mousePosition.y - y));
				}
				else
				{
					points.Insert(finalBreakList[i].start.index, new Vector2(j, 1.0f));//Random.Range(0.0f, 3.0f)));
				}
			}
			if (finalBreakList[i].start.point.y > 1.0f)
			{
				points.Insert(finalBreakList[i].start.index, new Vector2(finalBreakList[i].start.point.x, finalBreakList[i].start.point.y));
			}
			else
			{
				points.Insert(finalBreakList[i].start.index, new Vector2(finalBreakList[i].start.point.x, 1.0f));//Random.Range(0.0f, 3.0f)));
			}
		}

		//if terrain is fail, check to see if you can draw a straight line from start to end, if not, delete points between intersection and one of those,then hook it up  this is probably done wrong
		//right now this might only work when curve is in correct direction(commented out part)
	 	bool success = false;
	 	success = buildTerrainMesh();
		if (success == false)
		{
			Debug.Log ("================================================BUILDTERRAINFAILED");
//			for (int i = 0; i < points.Count - 1; i++)
//			{
//				Vector2 intersectionPoint = segmentIntersection (points[i].x, points[i].y, points[i + 1].x, points[i + 1].y, finalBreakList[0].start.point.x, finalBreakList[0].start.point.y, finalBreakList[finalBreakListCount - 1].end.point.x, finalBreakList[finalBreakListCount - 1].end.point.y);
//				if (intersectionPoint != new Vector2(666,666))
//				{
//					Debug.Log ("iPoint " + intersectionPoint);
//					points.RemoveRange (finalBreakList[finalBreakListCount - 1].end.index, i);
//					points.Insert(i, intersectionPoint);
//				}
//			}
//			if (buildTerrainMesh())
//			{
//				Debug.Log ("magicPoint2 success");
//			}
//			else
//			{
			points.Clear ();
			for (int i = 0; i < previousPoints.Count; i++)
			{				
				points.Add (previousPoints[i]);
			}

			bool success1 = false;
			success1 = buildTerrainMesh();
			if (success1 == false)
			{
				Debug.Log ("still fail");
			}
			else 
			{
				Debug.Log ("PASS");
			}
//			}
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