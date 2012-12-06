public class VectorField extends MonoBehaviour
{
	private var MAX_SEED_SPOTS: int = 20;
	
	public var width: float;
	public var height: float;
	
	private var seedSpots: List.<Vector3> = new List.<Vector3>();
	
	function Awake()
	{
		Debug.Log("Init vector field");
		var numSeedSpots: int = Random.value*(MAX_SEED_SPOTS+1);
		for(i = 0; i < numSeedSpots; i++)
		{
			seedSpots.Add(new Vector3(Random.value*width, Random.value*height, Random.value*height*.15));
		}
	}

	function getXForce(x: float, y: float)
	{
		var force: float = 0;
		var closeSpots: int = 0;
		for(var i = 0; i < seedSpots.Count; i++)
		{
			var d: float = Mathf.Sqrt(Mathf.Pow(seedSpots[i].y - y, 2) + Mathf.Pow(seedSpots[i].x - x, 2))/seedSpots[i].z;
			if(d <= Mathf.PI/2)
			{
				if(i%2 == 0)
				{
					force += Mathf.Sin((seedSpots[i].y - y)/seedSpots[i].z);
				}
				else
				{
					force += Mathf.Sin((y - seedSpots[i].y)/seedSpots[i].z);
				}
				closeSpots++;
			}
		}
		
		if(closeSpots == 0)
		{
			return 0;
		}
		
		force /= closeSpots;
		return force;
		
		//return Mathf.Sin((y + Time.fixedTime)*Time.fixedTime*.05);
	}
	
	function getYForce(x: float, y: float)
	{
		var force: float = 0;
		var closeSpots: int = 0;
		for(var i = 0; i < seedSpots.Count; i++)
		{
			var d: float = Mathf.Sqrt(Mathf.Pow(seedSpots[i].y - y, 2) + Mathf.Pow(seedSpots[i].x - x, 2))/seedSpots[i].z;
			if(d <= Mathf.PI/2)
			{
				if(i%2 == 0)
				{
					force += Mathf.Cos((seedSpots[i].x - x)/seedSpots[i].z + Mathf.PI/2.0);
				}
				else
				{
					force += Mathf.Cos((x-seedSpots[i].x)/seedSpots[i].z + Mathf.PI/2.0);
				}
				closeSpots++;
			}
		}
		
		if(closeSpots == 0)
		{
			return 0;
		}
		
		force /= closeSpots;
		return force;
		
		//return Mathf.Sin((x + Time.fixedTime)*Time.fixedTime*.05);
	}
}
