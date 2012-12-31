//TODO: why did i have to make the constructor for my P&I and BREAKOBJECT public, why the fuck does it have to be addRange to add a list to a list,
//points includes bottom left of screen and bottom right of terrain as points so you dont want to use the first and last index of points when determining if the linesegment intersects with explosion circle
//you have a problem with secondSeg or firstSeg being called twice instead it should be once each when on a corner terrain point

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
			float explosionRadius = 13.0f;
			Vector3 mousePositionTemp = Camera.mainCamera.ScreenToWorldPoint(Input.mousePosition);
			Vector2 mousePosition;
			mousePosition.x = mousePositionTemp.x;
			mousePosition.y = mousePositionTemp.y;
			List<PointAndIndex> firstPassCircleIntersects = new List<PointAndIndex>();
			List<PointAndIndex> leftExplosionIntersects = new List<PointAndIndex>();
			List<PointAndIndex> rightExplosionIntersects = new List<PointAndIndex>();
			List<PointAndIndex> fullMagicList = new List<PointAndIndex>();
			
			List<BreakObject> newBreakList = new List<BreakObject>();
			
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
						leftExplosionIntersects.Add(new PointAndIndex(new Vector2(intersectLeftX, intersectLeftY), i));
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
						rightExplosionIntersects.Add(new PointAndIndex(new Vector2(intersectRightX, intersectRightY), i + 1));
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
				
//				float x0 = -1.0f;
//				float y0 = 0.0f;
//				float x1 = 1.0f;
//				float y1 = 0.0f;
//				float r = 2.0f;
				
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
					Debug.Log ("incidence > 0");
					resultingX1 = (d * dY + Sgn(dY) * dX * Mathf.Sqrt (r*r * dR*dR - d*d)) / (dR*dR);
					resultingY1 = -(d * dX - Mathf.Abs (dY) * Mathf.Sqrt (r*r * dR*dR - d*d)) / (dR*dR);
					resultingX2 = (d * dY - Sgn(dY) * dX * Mathf.Sqrt (r*r * dR*dR - d*d)) / (dR*dR);
					resultingY2 = -(d * dX + Mathf.Abs (dY) * Mathf.Sqrt (r*r * dR*dR - d*d)) / (dR*dR);
					

					Debug.Log("ox0 " + x0);
					Debug.Log("oy0 " + y0);
					Debug.Log("ox1 " + x1);
					Debug.Log("oy1 " + y1);
					//Debug.Log("r " + r);
					Debug.Log("rx1 " + resultingX1);
					Debug.Log("ry1 " + resultingY1);
					Debug.Log("rx2 " + resultingX2);
					Debug.Log("ry2 " + resultingY2);
				}
				//maybe when land goes in left direction this needs to be reworked?
				if ((resultingX1 > x0 && resultingX1 < x1 && resultingY1 > y0 && resultingY1 < y1) || (resultingX1 < x0 && resultingX1 > x1 && resultingY1 < y0 && resultingY1 > y1) || (resultingX1 < x0 && resultingX1 > x1 && resultingY1 > y0 && resultingY1 < y1) || (resultingX1 > x0 && resultingX1 < x1 && resultingY1 < y0 && resultingY1 > y1))
				{
					Debug.Log("firstSeg");
					firstIsOnLineSegment = true; //since the resulting points dont fall within the lineSegment
				}
				if ((resultingX2 > x0 && resultingX2 < x1 && resultingY2 > y0 && resultingY2 < y1) || (resultingX2 < x0 && resultingX2 > x1 && resultingY2 < y0 && resultingY2 > y1) || (resultingX2 < x0 && resultingX2 > x1 && resultingY2 > y0 && resultingY2 < y1) || (resultingX2 > x0 && resultingX2 < x1 && resultingY2 < y0 && resultingY2 > y1))
				{
					Debug.Log("secondSeg");
					secondIsOnLineSegment = true; //since the resulting points dont fall within the lineSegment
				}
				
				// if two Intersections
				//eventually need to fix this so that it deletes no indexes since both land on same linesegment
				//right now it deletes one index for both
				if (firstIsOnLineSegment && secondIsOnLineSegment)
				{
					Debug.Log("boobs1");
					// if Count is even
					if (firstPassCircleIntersects.Count % 2.0 == 0.0)
					{
						firstPassCircleIntersects.Add(new PointAndIndex(new Vector2(resultingX1, resultingY1), i + 1));
						firstPassCircleIntersects.Add(new PointAndIndex(new Vector2(resultingX2, resultingY2), i + 1));
					}
					//if Count is odd
					else
					{
						firstPassCircleIntersects.Add(new PointAndIndex(new Vector2(resultingX1, resultingY1), i));
						firstPassCircleIntersects.Add(new PointAndIndex(new Vector2(resultingX2, resultingY2), i));
					}
				}
				// if only first Intersection
				if (firstIsOnLineSegment && !secondIsOnLineSegment)
				{
					Debug.Log("boobs2");
					if (firstPassCircleIntersects.Count % 2.0 == 0.0)
					{
						firstPassCircleIntersects.Add(new PointAndIndex(new Vector2(resultingX1, resultingY1), i + 1));
					}
					else
					{
						firstPassCircleIntersects.Add(new PointAndIndex(new Vector2(resultingX1, resultingY1), i));
					}
				}
				//if only second intersection
				if (!firstIsOnLineSegment && secondIsOnLineSegment)
				{
					Debug.Log("boobs3");
					if (firstPassCircleIntersects.Count % 2.0 == 0.0)
					{
						firstPassCircleIntersects.Add(new PointAndIndex(new Vector2(resultingX2, resultingY2), i + 1));
					}
					else
					{
						firstPassCircleIntersects.Add(new PointAndIndex(new Vector2(resultingX2, resultingY2), i));
					}
				}				
			}
			
			
			PointAndIndex lowestLeftPoint = leftExplosionIntersects[0];
			PointAndIndex lowestRightPoint = rightExplosionIntersects[0];
			//if leftExplosionIntersects are odd number above, set lowest Point
			if (leftExplosionIntersects.Count > 0 && leftExplosionIntersects.Count % 2.0 != 0.0)
			{
				
				for (int j = 1; j < leftExplosionIntersects.Count; j++)
				{
					if (leftExplosionIntersects[j].point.y < lowestLeftPoint.point.y)
					{
						lowestLeftPoint = leftExplosionIntersects[j];
					}
				}
			}
			//if rightExplosionIntersects are odd number above, set lowest Point
			if (rightExplosionIntersects.Count > 0 && rightExplosionIntersects.Count % 2.0 != 0.0)
			{
				
				for (int k = 1; k < rightExplosionIntersects.Count; k++)
				{
					if (rightExplosionIntersects[k].point.y < lowestRightPoint.point.y)
					{
						lowestRightPoint = rightExplosionIntersects[k];
					}
				}
			}
			
			//drunk programming started, please review
			
			// make a magicList for each circleIntersection fullMagicList
			for (int l = 0; l < firstPassCircleIntersects.Count; l++)
			{				
				//why the fuck does this have to be AddRange??
				fullMagicList.AddRange(new List<PointAndIndex>());
			}
			
			//second pass through Points, finding "magic" points
//			//might want to only use magic points if its the top half of the circle thats intersected, the way it is now is if any
//			//circle intersection has a magic point ...not too sure...
			for (int m = 0; m < points.Count - 1; m++)//i
			{
				for (int n = 0; n < firstPassCircleIntersects.Count; n++)
				{	//<= and >= might need to be switched around so you get correct indexes
					if (points[m].x <= firstPassCircleIntersects[n].point.x && points[m + 1].x > firstPassCircleIntersects[n].point.x || points[m].x >= firstPassCircleIntersects[n].point.x && points[m + 1].x < firstPassCircleIntersects[n].point.x)
					{
						float intersectX = firstPassCircleIntersects[n].point.x;
						float intersectY = ((firstPassCircleIntersects[n].point.x - points[m].x) / (points[m + 1].x - points[m].x)) * (points[m + 1].y - points[m].y) + points[m].y;
						if (intersectY > firstPassCircleIntersects[n].point.y)
						{
							//if even Count
							if (fullMagicList.Count % 2.0 == 0.0)
							{
							fullMagicList.Add(new PointAndIndex(new Vector2(intersectX, intersectY), m + 1));
							}
							//if odd Count
							else
							{
							fullMagicList.Add(new PointAndIndex(new Vector2(intersectX, intersectY), m));
							}
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
				if (fullMagicList.Count % 2.0 != 0.0)//j
				{
					PointAndIndex lowestCircleIntersect = fullMagicList[o];
					for (int p = 1; p < fullMagicList.Count; p++)//k
					{
						if (fullMagicList[o].point.y < lowestCircleIntersect.point.y)
						{
							lowestCircleIntersect = fullMagicList[o];
						}
					}
					for (int q = 0; q < firstPassCircleIntersects.Count; q++)
					{
					//might need to make this an espsilon thing because there maybe will be rounding errors
						if (firstPassCircleIntersects[q].point.x == lowestCircleIntersect.point.x && firstPassCircleIntersects[q].point.y == lowestCircleIntersect.point.y)
						{
							magicFoundFromThisPoint = firstPassCircleIntersects[q];
						}
					}
					newBreakList.Add(new BreakObject(lowestCircleIntersect, magicFoundFromThisPoint));
				}
			}
			//right and left intersections only start a break if they are the first break, they can end any break though by overriding circle intersections, magic points and points derived from are always breaks
			//third pass through points, finding which exits fall within counterclockwise from first enter circle to last exit circle
			//breaking all that exit counterclockwise
			//impossible for lowestLeft to end any breaks?
			Debug.Log (firstPassCircleIntersects.Count);
			Vector2 firstEnter = new Vector2(firstPassCircleIntersects[0].point.x - mousePosition.x, firstPassCircleIntersects[0].point.y - mousePosition.y);
			float lastExitX = firstPassCircleIntersects[firstPassCircleIntersects.Count - 1].point.x - mousePosition.x;
			float lastExitY = firstPassCircleIntersects[firstPassCircleIntersects.Count - 1].point.y - mousePosition.y;
			Vector2 fromVector2 = firstEnter;
			Vector2 toVector2 = new Vector2(lastExitX, lastExitY);
			
			float firstEnterToLastExitAngle = Vector2.Angle(fromVector2, toVector2);
			Vector3 cross = Vector3.Cross(fromVector2, toVector2);
			
			if (cross.z > 0)
			{
			    firstEnterToLastExitAngle = 360 - firstEnterToLastExitAngle;
			}
			Debug.Log("firstEnterToLastExitAngle" + firstEnterToLastExitAngle);
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
				//if final pass through loop and lowestRight has not been used
				if (s == firstPassCircleIntersects.Count - 1 && !lowestRightPointUsed)
				{
					newBreakList.Add(new BreakObject(startBreak, lowestRightPoint));
					continue;
				}
				//if final pass through loop and lowestRight has been used
				if (s == firstPassCircleIntersects.Count - 1 && lowestRightPointUsed)
				{
					newBreakList.Add(new BreakObject(startBreak, firstPassCircleIntersects[s]));
					continue;
				}
				//if first pass through loop, sets start break
				if (s == 0)
				{
					if (lowestLeftPoint.index < firstPassCircleIntersects[0].index)
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
				//if accesing an exitCircle and is CounterClockwise
				if (s % 2 != 0.0 && firstEnterToCurrentExitAngle < firstEnterToLastExitAngle)
				{
					if (!lowestRightPointUsed)
					{
						if (firstPassCircleIntersects[s + 1].index > lowestRightPoint.index)
						{
							newBreakList.Add(new BreakObject(startBreak, lowestRightPoint));
							startBreak = firstPassCircleIntersects[s + 1];
							lowestRightPointUsed = true;
							s++;
							continue;
						}
					}
					else
					{
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
				if (s % 2 == 0.0 && s != 0)
				{
					if (!lowestRightPointUsed)
					{
						//if (lowestRightPoint.index < 
					}
				}
			}
			//in order to get unique items in a list
//			var items = "A B A D A C".Split(' ');
//			var uniqueItems = new HashSet<string>(items);
//			foreach (string s in uniqueItems)
//    		Console.WriteLine(s);
			//convert HashSet to List
//			using System.Linq;
//   			HashSet<int> hset = new HashSet<int>();
//   			hset.Add(10);
//   			List<int> hList= hset.ToList();
			
			//get unique deletion indexes from newBreakList
			List<int> everyBreakIndex = new List<int>();
			for (int i = 0; i < newBreakList.Count; i++)
			{
				everyBreakIndex.Add (newBreakList[i].start.index);
				everyBreakIndex.Add (newBreakList[i].end.index);
			}
			HashSet<int> hset = new HashSet<int>(everyBreakIndex);
			List<int> uniqueBreakIndex = hset.ToList ();
			//sort list in numerical order so as not to fuck with order due to list compression after removeAt
			uniqueBreakIndex.Sort ();
			for (int i = 0; i < uniqueBreakIndex.Count; i++)
			{
				i = (uniqueBreakIndex.Count - 1) - i;
				Debug.Log("Removed" + i);
			    points.RemoveAt(i);
			}
			
			
			

//			for (int s = 1; s < firstPassCircleIntersects.Count; s = s + 2)
//			{
//				Vector2 currentExitVector2 = (firstPassCircleIntersects[s].point - mousePosition);
//				float firstEnterToCurrentExitAngle = Vector2.Angle(firstEnter, currentExitVector2);
//				Vector3 cross1 = Vector3.Cross(firstEnter, currentExitVector2);
//				if (cross1.z > 0)
//				{
//			    	firstEnterToCurrentExitAngle = 360 - firstEnterToCurrentExitAngle;
//			    	if (firstEnterToCurrentExitAngle < firstEnterToLastExitAngle)
//					{
//						newBreakList.Add(new BreakObject(firstPassCircleIntersects[s - 1], firstPassCircleIntersects[s]));
//					}
//				}
//			}
		}
		
//		if (Input.GetMouseButtonDown(0))
//		{
//			float explosionRadius = 6.0f;
//			Vector3 mousePosition = Camera.mainCamera.ScreenToWorldPoint(Input.mousePosition);
//			//mousePosition.z = 0.0;
//			
//			List<List<PointAndIndex>> fullMagicList = new List< List<PointAndIndex> >();
////			List<BreakList> fullBreakList = new List<BreakList>();
//			List<BreakObject> newBreakList = new List<BreakObject>();
//			int counter = 0;
//			List<PointAndIndex> firstPassCircleIntersects = new List<PointAndIndex>();
//			List<PointAndIndex> leftExplosionIntersects = new List<PointAndIndex>();
//			List<PointAndIndex> rightExplosionIntersects = new List<PointAndIndex>();			
//			
//			//first pass through Points, determines all Left, Right, and Circle Intersections
//			for (int i = 0; i <= points.Count - 2; i++)
//			{
//				float leftExplosionRadiusX = mousePosition.x - explosionRadius;
//				float rightExplosionRadiusX = mousePosition.x + explosionRadius;
//				
//					//<= and >= might need to be switched, so you get correct indexes
//					//left circle upwards Intersections
//				if (((points[i].x <= leftExplosionRadiusX && points[i + 1].x > leftExplosionRadiusX) || (points[i].x >= leftExplosionRadiusX && points[i + 1].x < leftExplosionRadiusX)) && points[i].y > mousePosition.y - explosionRadius)
//				{
//					Debug.Log("leftSide");
//					float intersectLeftX = leftExplosionRadiusX;
//					float intersectLeftY = ((leftExplosionRadiusX - points[i].x) / (points[i + 1].x - points[i].x)) * (points[i + 1].y - points[i].y) + points[i].y;
//					// if even Count
//					if (leftExplosionIntersects.Count % 2.0 == 0.0)
//					{
//						leftExplosionIntersects.Add(new PointAndIndex(Vector2(intersectLeftX, intersectLeftY), i + 1));
//					}
//					else //if odd Count
//					{
//						leftExplosionIntersects.Add(new PointAndIndex(Vector2(intersectLeftX, intersectLeftY), i));
//					}
//				}
//					//right circle upwards Intersections
//				if (((points[i].x <= rightExplosionRadiusX && points[i + 1].x > rightExplosionRadiusX) || (points[i].x >= rightExplosionRadiusX && points[i + 1].x < rightExplosionRadiusX)) && points[i].y > mousePosition.y - explosionRadius)
//				{
//					Debug.Log("rightSide");
//					float intersectRightX = rightExplosionRadiusX;
//					float intersectRightY = ((rightExplosionRadiusX - points[i].x) / (points[i + 1].x - points[i].x)) * (points[i + 1].y - points[i].y) + points[i].y;
//					// if even Count
//					if (rightExplosionIntersects.Count % 2.0 == 0.0)
//					{
//						rightExplosionIntersects.Add(new PointAndIndex(Vector2(intersectRightX, intersectRightY), i + 1));
//					}
//					else //if odd Count
//					{
//						rightExplosionIntersects.Add(new PointAndIndex(Vector2(intersectRightX, intersectRightY), i));
//					}
//				}
//				
////				float x0 = -10.0;
////				float y0 = 2.0;
////				float x1 = 10.0;
////				float y1 = 2.0;
////				float r = 2.0;
//				
////				var x0: float = mousePosition.x - points[i].x;
////				var y0: float = mousePosition.y - points[i].y;
////				var x1: float = mousePosition.x - points[i+1].x;
////				var y1: float = mousePosition.y - points[i+1].y;
////				var r: float = explosionRadius;
//				bool negativeSqrt;
//				bool firstIsOnLineSegment;
//				bool secondIsOnLineSegment;
//				
////				if ((Mathf.Pow(-2.0*x0*y0*y1 + Mathf.Pow(2.0*x0*y1, 2) + Mathf.Pow(2.0*x1*y0, 2) - 2.0*x1*y0*y1, 2) - 4*(Mathf.Pow(-x0, 2) + 2.0*x0*x1 - Mathf.Pow(x1, 2) - Mathf.Pow(y0, 2) + 2.0*y0*y1 - Mathf.Pow(y1, 2)*(Mathf.Pow(r, 2)*Mathf.Pow(x0, 2) - 2.0*Mathf.Pow(r, 2)*x0*x1 + Mathf.Pow(r, 2)*Mathf.Pow(x1, 2) - Mathf.Pow(x0, 2)*Mathf.Pow(y1, 2) + 2.0*x0*x1*y0*y1 - Mathf.Pow(x1, 2)*Mathf.Pow(y0,2)) ) ) < 0)
////				{
////				//zero intersections
////					Debug.Log("negSqrt");
////					negativeSqrt = true;
////				}
//				if (!negativeSqrt)
//				{
//					float x0 = mousePosition.x - points[i].x;
//					float y0 = mousePosition.y - points[i].y;
//					float x1 = mousePosition.x - points[i+1].x;
//					float y1 = mousePosition.y - points[i+1].y;
//					float r = explosionRadius;
//					
//					float dX = x1 - x0;
//					float dY = y1 - y0;
//					float dR = Mathf.Sqrt (Mathf.Pow (dX, 2) + Mathf.Pow (dY, 2));
//					float d = x0*y1 - x1*y0;
//					float incidence = r*r * dR*dR - d*d;
//					float resultingX1 = 0;
//					float resultingX2 = 0;
//					float resultingY1 = 0;
//					float resultingY2 = 0;
//					
//					if (incidence > 0)
//					{
//						resultingX1 = (d * dY + Sgn(dY) * dX * Mathf.Sqrt (r*r * dR*dR - d*d)) / (dR*dR);
//						resultingY1 = (d * dY + Mathf.Abs (dY) * Mathf.Sqrt (r*r * dR*dR - d*d)) / (dR*dR);
//						resultingX2 = (d * dY + Sgn(dY) * dX * Mathf.Sqrt (r*r * dR*dR - d*d)) / (dR*dR);
//						resultingY2 = (d * dY - Mathf.Abs (dY) * Mathf.Sqrt (r*r * dR*dR - d*d)) / (dR*dR);
//						
//				PointAndIndex lowestLeftPoint = leftExplosionIntersects[0];
//				for (int j = 1; j < leftExplosionIntersects.Count; j++)
//				{
//					if (leftExplosionIntersects[j].point.y < lowestLeftPoint.point.y)
//					{
//						lowestLeftPoint = leftExplosionIntersects[j];
//					}
//				}
//			}
//				//if rightExplosionIntersects are odd number above, set lowest Point
//			if (rightExplosionIntersects.Count % 2.0 != 0.0)
//			{
//				PointAndIndex lowestRightPoint = rightExplosionIntersects[0];
//				for (int k = 1; k < rightExplosionIntersects.Count; k++)
//				{
//					if (rightExplosionIntersects[k].point.y < lowestRightPoint.point.y)
//					{
//						lowestRightPoint = rightExplosionIntersects[k];
//					}
//				}
//			}
//			//if leftIntersect is first, make it first break
//			//if leftIntersect is not first or null, make first circleIntersect first breakpoint
//			fullBreakList.Add(new BreakList());
//			PointAndIndex breakPoint1;
//			PointAndIndex breakPoint2;
//			Debug.Log("firstPass" + firstPassCircleIntersects.Count);
//			if (lowestLeftPoint != null && lowestLeftPoint.index < firstPassCircleIntersects[0].index)
//			{
//				breakPoint1 = lowestLeftPoint;
//			}
//			if (lowestLeftPoint == null || lowestLeftPoint.index > firstPassCircleIntersects[0].index)
//			{
//				breakPoint1 = firstPassCircleIntersects[0];
//			}
//			//if rightIntersect is last, make it last breakpoint
//			//if rightIntersect is not last or null, make last circleIntersect last breakpoint
//			if (lowestRightPoint !=null && lowestRightPoint.index > firstPassCircleIntersects[firstPassCircleIntersects.Count - 1].index)
//			{
//				breakPoint2 = lowestRightPoint;
//			}
//			if (lowestRightPoint == null || lowestRightPoint.index < firstPassCircleIntersects[firstPassCircleIntersects.Count - 1].index)
//			{
//				breakPoint2 = firstPassCircleIntersects[firstPassCircleIntersects.Count - 1];
//			}
//			
//			newBreakList.Add(new BreakObject(breakPoint1, breakPoint2));
//			
//			
//			// make a magicList for each circleIntersection fullMagicList index corresponds to firstPassCircleIntersects index	
//			for (int l = 0; l < firstPassCircleIntersects.Count; l++)
//			{
//				fullMagicList.Add(new List<PointAndIndex>());
//			}			
//			//second pass through Points, finding "magic" points
//			//might want to only use magic points if its the top half of the circle thats intersected, the way it is now is if any
//			//circle intersection has a magic point
//			for (int m = 0; m <= points.Count - 2; m++)//i
//			{
//				for (int n = 0; n < firstPassCircleIntersects.Count; n++)//k
//				{	//<= and >= might need to be switched around so you get correct indexes
//					if (points[m].x <= firstPassCircleIntersects[n].point.x && points[m + 1].x > firstPassCircleIntersects[n].point.x || points[m].x >= firstPassCircleIntersects[n].point.x && points[m + 1].x < firstPassCircleIntersects[n].point.x)
//					{
//						float intersectX = firstPassCircleIntersects[n].point.x;
//						float intersectY = ((firstPassCircleIntersects[n].point.x - points[m].x) / (points[m + 1].x - points[m].x)) * (points[m + 1].y - points[m].y) + points[m].y;
//						if (intersectY > firstPassCircleIntersects[n].point.y)
//						{
//							//if even Count
//							if (fullMagicList[n].Count % 2.0 == 0.0)
//							{
//							fullMagicList[n].Add(PointAndIndex(Vector2(intersectX, intersectY), m + 1));
//							}
//							//if odd Count
//							else
//							{
//							fullMagicList[n].Add(PointAndIndex(Vector2(intersectX, intersectY), m));
//							}
//						}
//					}
//				}
//			}
//			
//			//go through fullMagicList to see if there are any odd Counts and if so, determine the lowest Y value to use as
//			//the magic value, then break every magic number to the number it was determined from
//			for (int o = 0; o < fullMagicList.Count; o++)
//			{	//if odd Count
//				if (fullMagicList[o].Count % 2.0 != 0.0)//j
//				{
//					PointAndIndex lowestCircleIntersect = fullMagicList[o][0];
//					for (int p = 1; p < fullMagicList[o].Count; p++)//k
//					{
//						if (fullMagicList[o][p].point.y < lowestCircleIntersect.point.y)
//						{
//							lowestCircleIntersect = fullMagicList[o][p];
//						}
//					}
//					for (int q = 0; q < firstPassCircleIntersects.Count; q++)
//					{
//						if (firstPassCircleIntersects[q] == lowestCircleIntersect.point.x)
//						{
//							PointAndIndex magicFoundFromThisPoint = firstPassCircleIntersects[q];
//						}
//					}
//					newBreakList.Add(new BreakObject(lowestCircleIntersect, magicFoundFromThisPoint));
//				}
//			}
//			
//			//third pass through points, finding which exits fall within counterclockwise from first enter circle to last exit circl
//
//			Vector2 firstEnter = Vector2(firstPassCircleIntersects[0].point.x - mousePosition.x, firstPassCircleIntersects[0].point.y - mousePosition.y);
//			float lastExitX = firstPassCircleIntersects[firstPassCircleIntersects.Count - 1].point.x - mousePosition.x;
//			float lastExitY = firstPassCircleIntersects[firstPassCircleIntersects.Count - 1].point.y - mousePosition.y;
//			Vector2 fromVector2 = firstEnter;
//			Vector2 toVector2 = new Vector2(lastExitX, lastExitY);
//			
//			float firstEnterToLastExitAngle = Vector2.Angle(fromVector2, toVector2);
//			Vector3 cross = Vector3.Cross(fromVector2, toVector2);
//			
//			if (cross.z > 0)
//			{
//			    firstEnterToLastExitAngle = 360 - firstEnterToLastExitAngle;
//			}
//			Debug.Log(firstEnterToLastExitAngle);
//			for (int s = 1; s < firstPassCircleIntersects.Count; s = s + 2)
//			{
//				Vector2 currentExitVector2 = (firstPassCircleIntersects[s].point - mousePosition);
//				float firstEnterToCurrentExitAngle = Vector2.Angle(firstEnter, currentExitVector2);
//				Vector3 cross1 = Vector3.Cross(firstEnter, currentExitVector2);
//				if (cross1.z > 0)
//				{
//			    	firstEnterToCurrentExitAngle = 360 - firstEnterToCurrentExitAngle;
//			    	if (firstEnterToCurrentExitAngle < firstEnterToLastExitAngle)
//					{
//						newBreakList.Add(new BreakObject(firstPassCircleIntersects[s - 1], firstPassCircleIntersects[s]));
//					}
//				}
//			}
//		}
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
		
		for(int t = 0; t < warpTime; t++)
		{
			for(int i = 2; i < points.Count-2; i++)
			{
				float xForce = vectorField.getXForce(points[i].x, points[i].y);
				float yForce = vectorField.getYForce(points[i].x, points[i].y);
				
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

class MATHHOLDA
{
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
	
	void METHODHOLDAH()
	{
		float x0 = -10.0f;
		float y0 = 0.0f;
		float x1 = 10.0f;
		float y1 = 0.0f;
		float r = 4.0f;
		
		float dX = x1 - x0;
		float dY = y1 - y0;
		float dR = Mathf.Sqrt (Mathf.Pow (dX, 2) + Mathf.Pow (dY, 2));
		float d = x0*y1 - x1*y0;
		float incidence = r*r * dR*dR - d*d;
		float resultingX1;
		float resultingX2;
		float resultingY1;
		float resultingY2;
		
		if (incidence > 0)
		{
			resultingX1 = (d * dY + Sgn(dY) * dX * Mathf.Sqrt (r*r * dR*dR - d*d)) / (dR*dR);
			resultingY1 = (d * dY + Mathf.Abs (dY) * Mathf.Sqrt (r*r * dR*dR - d*d)) / (dR*dR);
			resultingX2 = (d * dY + Sgn(dY) * dX * Mathf.Sqrt (r*r * dR*dR - d*d)) / (dR*dR);
			resultingY2 = (d * dY - Mathf.Abs (dY) * Mathf.Sqrt (r*r * dR*dR - d*d)) / (dR*dR);
			
					
			Debug.Log("ox0 " + x0);
			Debug.Log("oy0 " + y0);
			Debug.Log("ox1 " + x1);
			Debug.Log("oy1 " + y1);
			Debug.Log("r" + r);
			Debug.Log("rx1 " + resultingX1);
			Debug.Log("ry1 " + resultingY1);
			Debug.Log("rx2 " + resultingX2);
			Debug.Log("ry2 " + resultingY2);
		}	
	}
}