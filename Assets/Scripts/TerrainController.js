//TODO: actually do line intersection to find the point in the top of the terrain to make and then delete the ones inside
//      right now you are going through the entire bottom of circle and making it, even though you want to stop if the terrain is beneath the bottom of the circle in parts
#pragma strict

@script ExecuteInEditMode()
import System.IO;

var Sphere: GameObject;
var pointAndIndex: List.<PointAndIndex> = new List.<PointAndIndex>();

class PointAndIndex
{
	var point: Vector2;
	var index: int;
	public function PointAndIndex(tempPoint: Vector2, tempIndex: int)
	{
		point = tempPoint;
		index = tempIndex;
	}
}
//each BreakList has a start break and an end break in the breakPointsList
class BreakList
{	
	public function BreakList()
	{
		var breakPointsList: List.<PointAndIndex> = new List.<PointAndIndex>();
	}
}

class BreakObject
{	
	public function BreakObject(tempStart: PointAndIndex, tempEnd: PointAndIndex)
	{
		var start: PointAndIndex = tempStart;
		var end: PointAndIndex = tempEnd;
	}
}

class BreakPoints
{
	public function BreakPoints()
	{
		var leftPoints: List.<Vector2> = new List.<Vector2>();
		var rightPoints: List.<Vector2> = new List.<Vector2>();
		//first circleIntersectPoints corresponds to first magicPoints
		var circleIntersectPoints: List.<Vector2> = new List.<Vector2>();
		var magicPoints: List.<Vector2> = new List.<Vector2>();
	}
}

class TerrainController extends MonoBehaviour
{
	private var THICKNESS: float = .1;
	private var segmentWidth: float;
	public var numSegments: int;
	public var width: float;
	public var height: float;
	
	public var warpTime: int;
	public var warpScale: float;
	public var points: List.<Vector2> = new List.<Vector2>();
	public var segments: GameObject[];
	
	var vectorFieldObject: GameObject;
	
	var doneGenerating: boolean = false;
	
	public function Start()
	{
	 	segmentWidth = width/numSegments;
	 	var success: boolean = false;
	 	var count: int = 0;
	 	while(success == false && count < 10)
	 	{
       		if(count > 0) Debug.Log("Terrain generation failed, trying again");
			_generate();
			success = buildTerrainMesh();
			count++;
		}
	}
	
	public function Sgn (tempDY: float)
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
	
	function Update()
	{	//figure out how to tell if circle intersection is tangent cause it'll f everything up, you want to ignore the single
		//circle intersection
		// indexes selected are the ones you want to destroy
		if (Input.GetMouseButtonDown(0))
		{
			var explosionRadius: float = 6.0;
			var mousePosition: Vector3 = Camera.mainCamera.ScreenToWorldPoint(Input.mousePosition);
			//mousePosition.z = 0.0;
			
			var fullMagicList: List.< List.<PointAndIndex> > = new List.< List.<PointAndIndex> >();
			var fullBreakList: List.<BreakList> = new List.<BreakList>();
			var newBreakList: List.<BreakObject> = new List.<BreakObject>();
			var counter: int = 0;
			var firstPassCircleIntersects: List.<PointAndIndex> = new List.<PointAndIndex>();
			var leftExplosionIntersects: List.<PointAndIndex> = new List.<PointAndIndex>();
			var rightExplosionIntersects: List.<PointAndIndex> = new List.<PointAndIndex>();			
			
			//first pass through Points, determines all Left, Right, and Circle Intersections
			for (var i: int = 0; i <= points.Count - 2; i++)
			{
				var leftExplosionRadiusX: float = mousePosition.x - explosionRadius;
				var rightExplosionRadiusX: float = mousePosition.x + explosionRadius;
				
					//<= and >= might need to be switched, so you get correct indexes
					//left circle upwards Intersections
				if (((points[i].x <= leftExplosionRadiusX && points[i + 1].x > leftExplosionRadiusX) || (points[i].x >= leftExplosionRadiusX && points[i + 1].x < leftExplosionRadiusX)) && points[i].y > mousePosition.y - explosionRadius)
				{
					Debug.Log("leftSide");
					var intersectLeftX: float = leftExplosionRadiusX;
					var intersectLeftY: float = ((leftExplosionRadiusX - points[i].x) / (points[i + 1].x - points[i].x)) * (points[i + 1].y - points[i].y) + points[i].y;
					// if even Count
					if (leftExplosionIntersects.Count % 2.0 == 0.0)
					{
						leftExplosionIntersects.Add(new PointAndIndex(Vector2(intersectLeftX, intersectLeftY), i + 1));
					}
					else //if odd Count
					{
						leftExplosionIntersects.Add(new PointAndIndex(Vector2(intersectLeftX, intersectLeftY), i));
					}
				}
					//right circle upwards Intersections
				if (((points[i].x <= rightExplosionRadiusX && points[i + 1].x > rightExplosionRadiusX) || (points[i].x >= rightExplosionRadiusX && points[i + 1].x < rightExplosionRadiusX)) && points[i].y > mousePosition.y - explosionRadius)
				{
					Debug.Log("rightSide");
					var intersectRightX: float = rightExplosionRadiusX;
					var intersectRightY: float = ((rightExplosionRadiusX - points[i].x) / (points[i + 1].x - points[i].x)) * (points[i + 1].y - points[i].y) + points[i].y;
					// if even Count
					if (rightExplosionIntersects.Count % 2.0 == 0.0)
					{
						rightExplosionIntersects.Add(new PointAndIndex(Vector2(intersectRightX, intersectRightY), i + 1));
					}
					else //if odd Count
					{
						rightExplosionIntersects.Add(new PointAndIndex(Vector2(intersectRightX, intersectRightY), i));
					}
				}
				
//				var x0: float = -10.0;
//				var y0: float = 2.0;
//				var x1: float = 10.0;
//				var y1: float = 2.0;
//				var r: float = 2.0;
				
//				var x0: float = mousePosition.x - points[i].x;
//				var y0: float = mousePosition.y - points[i].y;
//				var x1: float = mousePosition.x - points[i+1].x;
//				var y1: float = mousePosition.y - points[i+1].y;
//				var r: float = explosionRadius;
				var negativeSqrt: boolean;
				var firstIsOnLineSegment: boolean;
				var secondIsOnLineSegment: boolean;
				
//				if ((Mathf.Pow(-2.0*x0*y0*y1 + Mathf.Pow(2.0*x0*y1, 2) + Mathf.Pow(2.0*x1*y0, 2) - 2.0*x1*y0*y1, 2) - 4*(Mathf.Pow(-x0, 2) + 2.0*x0*x1 - Mathf.Pow(x1, 2) - Mathf.Pow(y0, 2) + 2.0*y0*y1 - Mathf.Pow(y1, 2)*(Mathf.Pow(r, 2)*Mathf.Pow(x0, 2) - 2.0*Mathf.Pow(r, 2)*x0*x1 + Mathf.Pow(r, 2)*Mathf.Pow(x1, 2) - Mathf.Pow(x0, 2)*Mathf.Pow(y1, 2) + 2.0*x0*x1*y0*y1 - Mathf.Pow(x1, 2)*Mathf.Pow(y0,2)) ) ) < 0)
//				{
//				//zero intersections
//					Debug.Log("negSqrt");
//					negativeSqrt = true;
//				}
				if (!negativeSqrt)
				{
					var x0: float = mousePosition.x - points[i].x;
					var y0: float = mousePosition.y - points[i].y;
					var x1: float = mousePosition.x - points[i+1].x;
					var y1: float = mousePosition.y - points[i+1].y;
					var r: float = explosionRadius;
					
					var dX:float = x1 - x0;
					var dY:float = y1 - y0;
					var dR:float = Mathf.Sqrt (Mathf.Pow (dX, 2) + Mathf.Pow (dY, 2));
					var d:float = x0*y1 - x1*y0;
					var incidence:float = r*r * dR*dR - d*d;
					var resultingX1:float = 0;
					var resultingX2:float = 0;
					var resultingY1:float = 0;
					var resultingY2:float = 0;
					
					if (incidence > 0)
					{
						resultingX1 = (d * dY + Sgn(dY) * dX * Mathf.Sqrt (r*r * dR*dR - d*d)) / (dR*dR);
						resultingY1 = (d * dY + Mathf.Abs (dY) * Mathf.Sqrt (r*r * dR*dR - d*d)) / (dR*dR);
						resultingX2 = (d * dY + Sgn(dY) * dX * Mathf.Sqrt (r*r * dR*dR - d*d)) / (dR*dR);
						resultingY2 = (d * dY - Mathf.Abs (dY) * Mathf.Sqrt (r*r * dR*dR - d*d)) / (dR*dR);
					}
					
					if (resultingX1 > x0 && resultingX1 < x1 && resultingY1 > y0 && resultingY1 < y1)
					{
						Debug.Log("firstSeg");
						firstIsOnLineSegment = true; //since the resulting points dont fall within the lineSegment
					}
					if (resultingX2 > x0 && resultingX2 < x1 && resultingY2 > y0 && resultingY2 < y1)
					{
						Debug.Log("secondSeg");
						secondIsOnLineSegment = true; //since the resulting points dont fall within the lineSegment
					}
	
					
					// if two Intersections
					if (!negativeSqrt && firstIsOnLineSegment && secondIsOnLineSegment)
					{
						Debug.Log("boobs1");
						// if Count is even
						if (firstPassCircleIntersects.Count % 2.0 == 0.0)
						{
							firstPassCircleIntersects.Add(new PointAndIndex(Vector2(resultingX1, resultingY1), i + 1));
						}
						//if Count is odd
						else
						{
							firstPassCircleIntersects.Add(new PointAndIndex(Vector2(resultingX2, resultingY2), i));
						}
														
						Debug.Log("ox0 " + x0);
						Debug.Log("oy0 " + y0);
						Debug.Log("ox1 " + x1);
						Debug.Log("oy1 " + y1);
						Debug.Log("r " + r);
						Debug.Log("rx1 " + resultingX1);
						Debug.Log("ry1 " + resultingY1);
						Debug.Log("rx2 " + resultingX2);
						Debug.Log("ry2 " + resultingY2);
					}
					// if one Intersection
					if (!negativeSqrt && firstIsOnLineSegment && !secondIsOnLineSegment)
					{
						Debug.Log("boobs2");
						if (firstPassCircleIntersects.Count % 2.0 == 0.0)
						{
							firstPassCircleIntersects.Add(new PointAndIndex(Vector2(resultingX1, resultingY1), i + 1));
						}
						else
						{
							firstPassCircleIntersects.Add(new PointAndIndex(Vector2(resultingX1, resultingY1), i));
						}
														
						Debug.Log("ox0 " + x0);
						Debug.Log("oy0 " + y0);
						Debug.Log("ox1 " + x1);
						Debug.Log("oy1 " + y1);
						Debug.Log("r " + r);
						Debug.Log("rx1 " + resultingX1);
						Debug.Log("ry1 " + resultingY1);
						Debug.Log("rx2 " + resultingX2);
						Debug.Log("ry2 " + resultingY2);
					}
					if (!negativeSqrt && !firstIsOnLineSegment && secondIsOnLineSegment)
					{
						Debug.Log("boobs3");
						if (firstPassCircleIntersects.Count % 2.0 == 0.0)
						{
							firstPassCircleIntersects.Add(new PointAndIndex(Vector2(resultingX2, resultingY2), i + 1));
						}
						else
						{
							firstPassCircleIntersects.Add(new PointAndIndex(Vector2(resultingX2, resultingY2), i));
						}
														
						Debug.Log("ox0 " + x0);
						Debug.Log("oy0 " + y0);
						Debug.Log("ox1 " + x1);
						Debug.Log("oy1 " + y1);
						Debug.Log("r " + r);
						Debug.Log("rx1 " + resultingX1);
						Debug.Log("ry1 " + resultingY1);
						Debug.Log("rx2 " + resultingX2);
						Debug.Log("ry2 " + resultingY2);
					}
				}
			}
				//if leftExplosionIntersects are odd number above, set lowest Point
			if (leftExplosionIntersects.Count % 2.0 != 0.0)
			{
				var lowestLeftPoint: PointAndIndex = leftExplosionIntersects[0];
				for (var j: int = 1; j < leftExplosionIntersects.Count; j++)
				{
					if (leftExplosionIntersects[j].point.y < lowestLeftPoint.point.y)
					{
						lowestLeftPoint = leftExplosionIntersects[j];
					}
				}
			}
				//if rightExplosionIntersects are odd number above, set lowest Point
			if (rightExplosionIntersects.Count % 2.0 != 0.0)
			{
				var lowestRightPoint: PointAndIndex = rightExplosionIntersects[0];
				for (var k: int = 1; k < rightExplosionIntersects.Count; k++)
				{
					if (rightExplosionIntersects[k].point.y < lowestRightPoint.point.y)
					{
						lowestRightPoint = rightExplosionIntersects[k];
					}
				}
			}
			//if leftIntersect is first, make it first break
			//if leftIntersect is not first or null, make first circleIntersect first breakpoint
			fullBreakList.Add(new BreakList());
			var breakPoint1: PointAndIndex;
			var breakPoint2: PointAndIndex;
			Debug.Log("firstPass" + firstPassCircleIntersects.Count);
			if (lowestLeftPoint != null && lowestLeftPoint.index < firstPassCircleIntersects[0].index)
			{
				breakPoint1 = lowestLeftPoint;
			}
			if (lowestLeftPoint == null || lowestLeftPoint.index > firstPassCircleIntersects[0].index)
			{
				breakPoint1 = firstPassCircleIntersects[0];
			}
			//if rightIntersect is last, make it last breakpoint
			//if rightIntersect is not last or null, make last circleIntersect last breakpoint
			if (lowestRightPoint !=null && lowestRightPoint.index > firstPassCircleIntersects[firstPassCircleIntersects.Count - 1].index)
			{
				breakPoint2 = lowestRightPoint;
			}
			if (lowestRightPoint == null || lowestRightPoint.index < firstPassCircleIntersects[firstPassCircleIntersects.Count - 1].index)
			{
				breakPoint2 = firstPassCircleIntersects[firstPassCircleIntersects.Count - 1];
			}
			
			newBreakList.Add(new BreakObject(breakPoint1, breakPoint2));
			
			
			// make a magicList for each circleIntersection fullMagicList index corresponds to firstPassCircleIntersects index	
			for (var l: int = 0; l < firstPassCircleIntersects.Count; l++)
			{
				fullMagicList.Add(new List.<PointAndIndex>());
			}			
			//second pass through Points, finding "magic" points
			//might want to only use magic points if its the top half of the circle thats intersected, the way it is now is if any
			//circle intersection has a magic point
			for (var m: int = 0; m <= points.Count - 2; m++)//i
			{
				for (var n: int = 0; n < firstPassCircleIntersects.Count; n++)//k
				{	//<= and >= might need to be switched around so you get correct indexes
					if (points[m].x <= firstPassCircleIntersects[n].point.x && points[m + 1].x > firstPassCircleIntersects[n].point.x || points[m].x >= firstPassCircleIntersects[n].point.x && points[m + 1].x < firstPassCircleIntersects[n].point.x)
					{
						var intersectX: float = firstPassCircleIntersects[n].point.x;
						var intersectY: float = ((firstPassCircleIntersects[n].point.x - points[m].x) / (points[m + 1].x - points[m].x)) * (points[m + 1].y - points[m].y) + points[m].y;
						if (intersectY > firstPassCircleIntersects[n].point.y)
						{
							//if even Count
							if (fullMagicList[n].Count % 2.0 == 0.0)
							{
							fullMagicList[n].Add(PointAndIndex(Vector2(intersectX, intersectY), m + 1));
							}
							//if odd Count
							else
							{
							fullMagicList[n].Add(PointAndIndex(Vector2(intersectX, intersectY), m));
							}
						}
					}
				}
			}
			
			//go through fullMagicList to see if there are any odd Counts and if so, determine the lowest Y value to use as
			//the magic value, then break every magic number to the number it was determined from
			for (var o: int = 0; o < fullMagicList.Count; o++)
			{	//if odd Count
				if (fullMagicList[o].Count % 2.0 != 0.0)//j
				{
					var lowestCircleIntersect: PointAndIndex = fullMagicList[o][0];
					for (var p: int = 1; p < fullMagicList[o].Count; p++)//k
					{
						if (fullMagicList[o][p].point.y < lowestCircleIntersect.point.y)
						{
							lowestCircleIntersect = fullMagicList[o][p];
						}
					}
					for (var q: int = 0; q < firstPassCircleIntersects.Count; q++)
					{
						if (firstPassCircleIntersects[q] == lowestCircleIntersect.point.x)
						{
							var magicFoundFromThisPoint: PointAndIndex = firstPassCircleIntersects[q];
						}
					}
					newBreakList.Add(new BreakObject(lowestCircleIntersect, magicFoundFromThisPoint));
				}
			}
			
			//third pass through points, finding which exits fall within counterclockwise from first enter circle to last exit circl

			var firstEnter: Vector2 = Vector2(firstPassCircleIntersects[0].point.x - mousePosition.x, firstPassCircleIntersects[0].point.y - mousePosition.y);
			var lastExitX: float = firstPassCircleIntersects[firstPassCircleIntersects.Count - 1].point.x - mousePosition.x;
			var lastExitY: float = firstPassCircleIntersects[firstPassCircleIntersects.Count - 1].point.y - mousePosition.y;
			var fromVector2: Vector2 = firstEnter;
			var toVector2: Vector2 = new Vector2(lastExitX, lastExitY);
			
			var firstEnterToLastExitAngle: float = Vector2.Angle(fromVector2, toVector2);
			var cross: Vector3 = Vector3.Cross(fromVector2, toVector2);
			
			if (cross.z > 0)
			{
			    firstEnterToLastExitAngle = 360 - firstEnterToLastExitAngle;
			}
			Debug.Log(firstEnterToLastExitAngle);
			for (var s: int = 1; s < firstPassCircleIntersects.Count; s = s + 2)
			{
				var currentExitVector2: Vector2 = (firstPassCircleIntersects[s].point - mousePosition);
				var firstEnterToCurrentExitAngle: float = Vector2.Angle(firstEnter, currentExitVector2);
				var cross1: Vector3 = Vector3.Cross(firstEnter, currentExitVector2);
				if (cross1.z > 0)
				{
			    	firstEnterToCurrentExitAngle = 360 - firstEnterToCurrentExitAngle;
			    	if (firstEnterToCurrentExitAngle < firstEnterToLastExitAngle)
					{
						newBreakList.Add(new BreakObject(firstPassCircleIntersects[s - 1], firstPassCircleIntersects[s]));
					}
				}
			}
		}
			
	
	
		
//		if (Input.GetMouseButtonDown(0))
//		{
//			var leftExplosionIndexesTerrain: List.<int> = new List.<int>();
//			var rightExplosionIndexesTerrain: List.<int> = new List.<int>();
//			var leftExplosionPointsTerrain: List.<Vector2> = new List.<Vector2>();
//			var rightExplosionPointsTerrain: List.<Vector2> = new List.<Vector2>();
//			var insideExplosionRadius: List.<int> = new List.<int>();
//			
//			Debug.Log("insideCount" + insideExplosionRadius.Count);
//			//Debug.Log("0element" + insideExplosionRadius[0]);
//			
//			var mousePosition: Vector3 = Camera.mainCamera.ScreenToWorldPoint(Input.mousePosition);
//			Debug.Log("mY" + mousePosition.y);
//			Debug.Log("mX" + mousePosition.x);
//			Debug.Log("count" + points.Count);
//			
//			var explosionRadius: float = 6.0;
//
//			var leftExplosionRadiusX: float = mousePosition.x - explosionRadius;
//			var rightExplosionRadiusX: float = mousePosition.x + explosionRadius;
//			
//			var leftPoint: Vector2;
//			var rightPoint: Vector2;
//			var leftCirclePoint: Vector2;
//			var rightCirclePoint: Vector2;
//
//			var firstCircle: int = insideExplosionRadius[0];
//			var lastCircle: int = insideExplosionRadius[0];
//			
//			for (var i: int = 0; i <= points.Count - 2; i++)
//			{				
//				if (((points[i].x < leftExplosionRadiusX && points[i + 1].x > leftExplosionRadiusX) || (points[i].x > leftExplosionRadiusX && points[i + 1].x < leftExplosionRadiusX)) && points[i].y > mousePosition.y)
//				{
//					Debug.Log("1boobs");
//					Debug.Log(i + 1);
//					leftPoint = points[i];
//					leftExplosionIndexesTerrain.Add(i + 1); //+1 so it lands inside the explosion radius
//					//leftExplosionIndexesTerrain[leftExplosionIndexesTerrain.Count - 1].x = i; //x becomes i position in array
//				}
//				if (((points[i].x < rightExplosionRadiusX && points[i + 1].x > rightExplosionRadiusX) || (points[i].x > rightExplosionRadiusX && points[i + 1].x < rightExplosionRadiusX)) && points[i].y > mousePosition.y)
//
//				{
//					Debug.Log("2boobs");
//					Debug.Log(i);
//
//					rightPoint = points[i + 1];
//					rightExplosionIndexesTerrain.Add(i); //+0 so it lands inside the explosion radius
//					//rightExplosionIndexesTerrain[rightExplosionIndexesTerrain.Count - 1].x = i; //x becomes i position in array
//				}
//				if (Mathf.Sqrt(Mathf.Pow(points[i].x - mousePosition.x, 2) + Mathf.Pow(points[i].y - mousePosition.y, 2)) <= explosionRadius)
//				{
//					Debug.Log("3boobs");
//					Debug.Log(i);
//					insideExplosionRadius.Add(i);
//				}
//				var x0: float = mousePosition.x - points[i].x;
//				var y0: float = mousePosition.y - points[i].y;
//				var x1: float = mousePosition.x - points[i+1].x;
//				var y1: float = mousePosition.y - points[i+1].y;
//				var r: float = explosionRadius;
//				var negativeSqrt: boolean;
//				
//				if ((Mathf.Pow(-2.0*x0*y0*y1 + Mathf.Pow(2.0*x0*y1, 2) + Mathf.Pow(2.0*x1*y0, 2) - 2.0*x1*y0*y1, 2) - 4*(Mathf.Pow(-x0, 2) + 2.0*x0*x1 - Mathf.Pow(x1, 2) - Mathf.Pow(y0, 2) + 2.0*y0*y1 - Mathf.Pow(y1, 2)*(Mathf.Pow(r, 2)*Mathf.Pow(x0, 2) - 2.0*Mathf.Pow(r, 2)*x0*x1 + Mathf.Pow(r, 2)*Mathf.Pow(x1, 2) - Mathf.Pow(x0, 2)*Mathf.Pow(y1, 2) + 2.0*x0*x1*y0*y1 - Mathf.Pow(x1, 2)*Mathf.Pow(y0,2)) ) ) < 0)
//				{
//					negativeSqrt = true;
//				}
//				var resultingX1: float = (-Mathf.Sqrt( Mathf.Pow(-2.0*x0*y0*y1 + Mathf.Pow(2.0*x0*y1, 2) + Mathf.Pow(2.0*x1*y0, 2) - 2.0*x1*y0*y1, 2) - 4*(Mathf.Pow(-x0, 2) + 2.0*x0*x1 - Mathf.Pow(x1, 2) - Mathf.Pow(y0, 2) + 2.0*y0*y1 - Mathf.Pow(y1, 2)*(Mathf.Pow(r, 2)*Mathf.Pow(x0, 2) - 2.0*Mathf.Pow(r, 2)*x0*x1 + Mathf.Pow(r, 2)*Mathf.Pow(x1, 2) - Mathf.Pow(x0, 2)*Mathf.Pow(y1, 2) + 2.0*x0*x1*y0*y1 - Mathf.Pow(x1, 2)*Mathf.Pow(y0,2)) ) ) + 2.0*x0*y0*y1 - 2.0*x0*Mathf.Pow(y1, 2) - 2.0*x1*Mathf.Pow(y0, 2) + 2.0*x1*y0*y1) / (2.0*(Mathf.Pow(-x0, 2) + 2.0*x0*x1 - Mathf.Pow(x1, 2) - Mathf.Pow(y0, 2) + 2.0*y0*y1 - Mathf.Pow(y1, 2)));
//				var resultingX2: float = (Mathf.Sqrt( Mathf.Pow(-2.0*x0*y0*y1 + Mathf.Pow(2.0*x0*y1, 2) + Mathf.Pow(2.0*x1*y0, 2) - 2.0*x1*y0*y1, 2) - 4*(Mathf.Pow(-x0, 2) + 2.0*x0*x1 - Mathf.Pow(x1, 2) - Mathf.Pow(y0, 2) + 2.0*y0*y1 - Mathf.Pow(y1, 2)*(Mathf.Pow(r, 2)*Mathf.Pow(x0, 2) - 2.0*Mathf.Pow(r, 2)*x0*x1 + Mathf.Pow(r, 2)*Mathf.Pow(x1, 2) - Mathf.Pow(x0, 2)*Mathf.Pow(y1, 2) + 2.0*x0*x1*y0*y1 - Mathf.Pow(x1, 2)*Mathf.Pow(y0,2)) ) ) + 2.0*x0*y0*y1 - 2.0*x0*Mathf.Pow(y1, 2) - 2.0*x1*Mathf.Pow(y0, 2) + 2.0*x1*y0*y1) / (2.0*(Mathf.Pow(-x0, 2) + 2.0*x0*x1 - Mathf.Pow(x1, 2) - Mathf.Pow(y0, 2) + 2.0*y0*y1 - Mathf.Pow(y1, 2)));
//				
//				var firstIsOnLineSegment: boolean;
//				var secondIsOnLineSegment: boolean;
//				var leftInBetweenSegmentIndex: int;
//				var rightInBetweenSegmentIndex: int;
//				
//				var resultingY1: float = y0 + ((y1 - y0) / (x1 - x0)) * (resultingX1 - x0);
//				var resultingY2: float = y0 + ((y1 - y0) / (x1 - x0)) * (resultingX2 - x0);
//				
//				if (resultingX1 > x0 && resultingX1 < x1 && resultingY1 > y0 && resultingY1 < y1)
//				{
//					firstIsOnLineSegment = true; //since the resulting points dont fall within the lineSegment
//				}
//				if (resultingX2 > x0 && resultingX2 < x1 && resultingY2 > y0 && resultingY2 < y1)
//				{
//					secondIsOnLineSegment = true; //since the resulting points dont fall within the lineSegment
//				}
//				if (!negativeSqrt && firstIsOnLineSegment)
//				{
//					leftInBetweenSegmentIndex = i + 1;
//				}
//				if (!negativeSqrt && secondIsOnLineSegment)
//				{
//					rightInBetweenSegmentIndex = i;
//				}
//			}
//
//			
//			
//			
//			
//			var leftYoYo: Vector3 = Vector3(0.0,0.0,0.0);
//			var rightYoYo: Vector3 = Vector3(0.0,0.0,0.0);
//			leftYoYo.z = -7;
//			rightYoYo.z = -7;
//			for (var t: int = 0; t < leftExplosionIndexesTerrain.Count; t++)
//			{
//				Debug.Log("leftSplodeIndex" + leftExplosionIndexesTerrain[t]);
//				leftYoYo = points[leftExplosionIndexesTerrain[t]];
//				var PooChainCloneHead33 = Instantiate(Sphere, leftYoYo, Quaternion.LookRotation(Vector3(Mathf.Cos(MonkeyController.angle - Mathf.PI/2),Mathf.Sin(MonkeyController.angle - Mathf.PI/2),0.0),Vector3.right));
//				PooChainCloneHead33.renderer.material.color = Color.Lerp (Color.red, Color.green, 1);
//			}
//			for (var u: int = 0; u < rightExplosionIndexesTerrain.Count; u++)
//			{
//				Debug.Log("rightSplodeIndex" + rightExplosionIndexesTerrain[u]);
//				rightYoYo = points[rightExplosionIndexesTerrain[u]];
//				var PooChainCloneHead22 = Instantiate(Sphere, rightYoYo, Quaternion.LookRotation(Vector3(Mathf.Cos(MonkeyController.angle - Mathf.PI/2),Mathf.Sin(MonkeyController.angle - Mathf.PI/2),0.0),Vector3.right));
//				PooChainCloneHead22.renderer.material.color = Color.Lerp (Color.green, Color.red, 1);
//			}
//			
//			
//			
//			
//			
//			if (leftExplosionIndexesTerrain.Count % 2.0 == 0)
//			{
//				Debug.Log("leftEvenCount");
//				leftExplosionIndexesTerrain = new List.<int>();
//			}
//			if (rightExplosionIndexesTerrain.Count % 2.0 == 0)
//			{
//				Debug.Log("rightEvenCount");
//				rightExplosionIndexesTerrain = new List.<int>();
//			}
//			
//			var lowestLeftPoint: int = 666666;
//			var lowestRightPoint: int = 0;
//			var lowestLowestLeftPoint: int = 666666;
//			var lowestLowestRightPoint: int = 0;
//			
//			for (var j: int = 0; j <= leftExplosionIndexesTerrain.Count - 1; j++)
//			{
//				if (leftExplosionIndexesTerrain[j] < lowestLeftPoint)
//				{
//					lowestLeftPoint = leftExplosionIndexesTerrain[j];
//				}
//			}
//			for (var k: int = 0; k <= rightExplosionIndexesTerrain.Count - 1; k++)
//			{
//				if (rightExplosionIndexesTerrain[k] > lowestRightPoint)
//				{
//					lowestRightPoint = rightExplosionIndexesTerrain[k];
//				}
//			}
//			for (var l: int = 0; l <= insideExplosionRadius.Count - 1; l++)
//			{ 
////				if (insideExplosionRadius == null)
////				{
////					break;
////				}
//				//Debug.Log("IER" + insideExplosionRadius[l]);
//				if (insideExplosionRadius[l] < lowestLeftPoint)
//				{
//					lowestLeftPoint = insideExplosionRadius[l];
//				}
//				if (insideExplosionRadius[l] > lowestRightPoint)
//				{
//					lowestRightPoint = insideExplosionRadius[l];
//				}
//			}
//			
//			var beginAngle: float = Mathf.PI;
//			var endAngle: float = 2 * Mathf.PI;
//			
//			if (leftInBetweenSegmentIndex < lowestLeftPoint && firstIsOnLineSegment)
//			{
//				lowestLeftPoint = leftInBetweenSegmentIndex;
//				beginAngle = Mathf.Atan(resultingY1 / resultingX1);				
//			}
//			if (rightInBetweenSegmentIndex > lowestRightPoint && secondIsOnLineSegment)
//			{
//				lowestRightPoint = rightInBetweenSegmentIndex;
//				endAngle = Mathf.Atan(resultingY2 / resultingX2);
//			}
//			if (endAngle > 2 * Mathf.PI)
//			{
//				endAngle = 2 * Mathf.PI;
//			}
//			if (beginAngle < Mathf.PI)
//			{
//				beginAngle = Mathf.PI;
//			}						
//			
//			leftYoYo = points[lowestLeftPoint];
//			rightYoYo = points[lowestRightPoint];
//			leftYoYo.z = -7;
//			rightYoYo.z = -7;
//			//places markers at destroyed indexes, or one off of them or something
//			var PooChainCloneHead = Instantiate(Sphere, leftYoYo, Quaternion.LookRotation(Vector3(Mathf.Cos(MonkeyController.angle - Mathf.PI/2),Mathf.Sin(MonkeyController.angle - Mathf.PI/2),0.0),Vector3.right));
//			var PooChainCloneHead1 = Instantiate(Sphere, rightYoYo, Quaternion.LookRotation(Vector3(Mathf.Cos(MonkeyController.angle - Mathf.PI/2),Mathf.Sin(MonkeyController.angle - Mathf.PI/2),0.0),Vector3.right));
//			PooChainCloneHead.renderer.material.color = Color.Lerp (Color.red, Color.green, 1);
//			PooChainCloneHead1.renderer.material.color = Color.Lerp (Color.green, Color.red, 1);
//			
//			//maybe needs to not be both cause if one side is off it might not mean other side is off too
//			if (lowestLeftPoint == 666666)
//			{
//			Debug.Log("return1");
//				return;
//			}
//			if (lowestRightPoint == 0)
//			{
//			Debug.Log("return2");
//				return;
//			}
//			Debug.Log("Removed" + lowestLeftPoint + "to" + lowestRightPoint);
//			points.RemoveRange(lowestLeftPoint, lowestRightPoint - lowestLeftPoint + 1);			
//			
//			//mousePosition.y = mousePosition.y + 10.0;
//			//points.Insert(lowestLeftPoint, mousePosition);
//			
//			var numberOfSegmentsInCircleBottom: int = explosionRadius * 2 - 1;
//			var increaseAngleBy: float = (endAngle - beginAngle) / numberOfSegmentsInCircleBottom;
//			
//			var counter: int = 0;
//			
//			for (var m: int = lowestLeftPoint; m <= lowestLeftPoint + 3 + numberOfSegmentsInCircleBottom; m++)
//			{
//				if(m == lowestLeftPoint)
//				{
//					leftPoint.x = mousePosition.x - explosionRadius;
//					points.Insert(m, leftPoint);
//				}
//				if(m == lowestLeftPoint + 1)
//				{
//					leftPoint.y = mousePosition.y;
//					points.Insert(m, leftPoint);
//				}
//								
//				if (m > lowestLeftPoint + 1 && m < lowestLeftPoint + 2 + numberOfSegmentsInCircleBottom && beginAngle + increaseAngleBy * counter < endAngle)
//				{
//					counter++;
//					var yCircleBottom: float = mousePosition.y + explosionRadius * Mathf.Sin(beginAngle + increaseAngleBy * counter);
//					var xCircleBottom: float = mousePosition.x + explosionRadius * Mathf.Cos(beginAngle + increaseAngleBy * counter);
//					var circleBottomPoint: Vector2 = Vector2(xCircleBottom, yCircleBottom);
//					points.Insert(m, circleBottomPoint);
//				}
//				if(m == lowestLeftPoint + 3 + numberOfSegmentsInCircleBottom)
//				{
//					rightPoint.y = mousePosition.y;
//					points.Insert(m - 1, rightPoint);
//				}
//				if(m == lowestLeftPoint + 2 + numberOfSegmentsInCircleBottom)
//				{
//
//					rightPoint.x = mousePosition.x + explosionRadius;
//					points.Insert(m, rightPoint);
//				}
//			}
//			if(!buildTerrainMesh()){Debug.Log("terrainFailed");}
//
//		}

	}
	
	public function buildTerrainMesh()
	{
		// Use the triangulator to get indices for creating triangles
        var indices: int[] = Triangulator.Triangulate(points);
        
        if(indices == null) return false;
        
        var vertices: Vector3[] = new Vector3[points.Count];
        var normals: Vector3[] = new Vector3[points.Count];
		var textureCoords: Vector2[] = new Vector2[points.Count];
		
        for (var i = 0; i < vertices.Length; i++) 
        {
            vertices[i] = new Vector3(points[i].x, points[i].y, 0);
            normals[i] = Vector3.forward;
			textureCoords[i] = new Vector2(0, 0);
        }
	
		var mf: MeshFilter = GetComponent(MeshFilter);
		
		var mesh = new Mesh();
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
		
		for (var p: int = 0; p < points.Count-3; p++)
		{
			i = p*6;
			indices[i] = p;
			indices[i+1] = p+points.Count-2;
			indices[i+2] = p+1;
			
			indices[i+3] = p+1;
			indices[i+4] = p+points.Count-2;
			indices[i+5] = p+points.Count-1;
		}
		
        for (p = 0; p < points.Count-2; p++)
        {
            vertices[p] = new Vector3(points[p+1].x, points[p+1].y, 0);
            vertices[p+points.Count-2] = new Vector3(points[p+1].x, points[p+1].y, 10);
            normals[p] = Vector3.up;
            normals[p+points.Count-2] = Vector3.up;
        }
	
		var mc: MeshCollider = GetComponent(MeshCollider);
		
		mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.triangles = indices;
		mesh.normals = normals;
	    
	    mc.mesh = mesh;
	    mesh.Optimize();
	    mesh.RecalculateBounds();
	    
	    return true;
	}

	private function _generate() 
	{
		points.Clear();
		points.Capacity = numSegments+1+2; // segments + 2 bottom points to create a polygon
		
		for(var i:int = 0; i < points.Capacity; i++)
		{
			points.Add(new Vector2(0, 0));
		}
		
		points[0] = new Vector2(0, 0);
		points[1] = new Vector2(0, Random.value*height);
		
		points[numSegments+1] = new Vector2(numSegments*segmentWidth, Random.value*height);
		points[numSegments+2] = new Vector2(numSegments*segmentWidth, 0);
		
		_midPointDivision(1, numSegments+1, 0);
		
		var vectorField: VectorField = vectorFieldObject.GetComponent(MonoBehaviour);
		
		for(var t: int = 0; t < warpTime; t++)
		{
			for(i=2; i < points.Count-2; i++)
			{
				var xForce: float = vectorField.getXForce(points[i].x, points[i].y);
				var yForce: float = vectorField.getYForce(points[i].x, points[i].y);
				
				var nextX: float = Mathf.Clamp(points[i].x + xForce*warpScale, 0, width);
				var nextY: float = Mathf.Clamp(points[i].y + yForce*warpScale, 1, height);
				
				points[i] = new Vector2(nextX, nextY);
				
				if(i != 2)
				{
					var distance: float = Vector2.Distance(points[i], points[i-1]);
					if(distance > segmentWidth*2)
					{
						points.Insert(i, points[i-1] + (points[i]-points[i-1])*.5);
						i--;
					}
				}
			}
		}
	}
	
	private function _midPointDivision(start: int, end: int, depth: int) 
	{
		if (end <= start+1) 
		{
			return;
		}
		
		var midPoint: int = (end + start) / 2;
		var midY: float = (points[start].y + points[end].y) / 2;
		var depthFactor: float = .5 / (depth*depth);
		
		var y: float = midY + (Random.value * 2 * depthFactor - depthFactor)*height;
		
		// Make sure y is between 0 and height
		y = Mathf.Max(Mathf.Min(height, y), 1); 
		
		points[midPoint] = new Vector2((midPoint-1)*segmentWidth, y);
		
		_midPointDivision(start, midPoint, depth + 1);
		_midPointDivision(midPoint, end, depth + 1);
	}

}