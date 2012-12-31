using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Triangulator : MonoBehaviour {
	
	public static int[] Triangulate(Vector2[] points)
	{
		return Triangulate(new List<Vector2>(points));
	}
	
	public static int[] Triangulate(List<Vector2> m_points)
	{
	    List<int> indices = new List<int>();
	    
	    int n = m_points.Count;
	    if (n < 3)
	    {
	        return indices.ToArray();
	    }
	
	    int[] V = new int[n];
	    bool flipped = Area(m_points) < 0;
	    if(!flipped) 
	    {
	        for (int v = 0; v < n; v++)
	            V[v] = v;
	    }
	    else
	    {
	        for (int v = 0; v < n; v++)
	            V[v] = (n - 1) - v;
	    }
	
	    int nv = n;
	    int count = 2 * nv;
	    var m = 0;
	    for (int v = nv - 1; nv > 2; ) 
	    {
	        if ((count--) <= 0)
	        {
	        	// Bad polygon, or close enough that it can't be triangulated
	        	return null;
	        }
	
	        int u = v;
	        if (nv <= u)
	            u = 0;
	        v = u + 1;
	        if (nv <= v)
	            v = 0;
	        int w = v + 1;
	        if (nv <= w)
	            w = 0;
	
	        if (Snip(m_points, u, v, w, nv, V)) 
	        {
	            int a;
	            int b;
	            int c;
	            int s;
	            int t;
	            a = V[u];
	            b = V[v];
	            c = V[w];
	    		
	            Vector2 A = m_points[a];
	    		Vector2 B = m_points[b];
	    		Vector2 C = m_points[c];
	    		
	            indices.Add(a);
	            indices.Add(b);
	            indices.Add(c);
	            m++;
	            s = v;
	            for (t = v + 1; t < nv; t++)
	            {
	                V[s] = V[t];
	                s++;
	            }
	            nv--;
	            count = 2 * nv;
	        }
	    }
	
	    indices.Reverse();
	    return indices.ToArray();
	}
	
	public static float Area(List<Vector2> m_points)
	{
	    int n = m_points.Count;
	    float A = 0.0f;
	    int q= 0;
	    for (int p = n - 1; q < n; p = q++) {
	        Vector2 pval = m_points[p];
	        Vector2 qval = m_points[q];
	        float piece = pval.x * qval.y - qval.x * pval.y;
	        A += piece;
	    }
	    return (A * 0.5f);
	}
	
	public static bool Snip(List<Vector2> m_points, int u, int v, int w, int n, int[] V) 
	{
	    int p;
	    Vector2 A = m_points[V[u]];
	    Vector2 B = m_points[V[v]];
	    Vector2 C = m_points[V[w]];
	    if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
	    {
	        return false;
	    }
	    for (p = 0; p < n; p++) {
	        if ((p == u) || (p == v) || (p == w))
	            continue;
	        Vector2 P = m_points[V[p]];
	        if (InsideTriangle(m_points, A, B, C, P))
	        {
	            return false;
	        }
	    }
	    return true;
	}
	
	public static bool InsideTriangle(List<Vector2> m_points, Vector2 A, Vector2 B, Vector2 C, Vector2 P) 
	{
	    float ax;
	    float ay;
	    float bx;
	    float by;
	    float cx;
	    float cy;
	    float apx;
	    float apy;
	    float bpx;
	    float bpy;
	    float cpx;
	    float cpy;
	    float cCROSSap;
	    float bCROSScp;
	    float aCROSSbp;
	
	    ax = C.x - B.x; ay = C.y - B.y;
	    bx = A.x - C.x; by = A.y - C.y;
	    cx = B.x - A.x; cy = B.y - A.y;
	    apx = P.x - A.x; apy = P.y - A.y;
	    bpx = P.x - B.x; bpy = P.y - B.y;
	    cpx = P.x - C.x; cpy = P.y - C.y;
	
	    aCROSSbp = ax * bpy - ay * bpx;
	    cCROSSap = cx * apy - cy * apx;
	    bCROSScp = bx * cpy - by * cpx;
	
	    return ((aCROSSbp >= 0.0) && (bCROSScp >= 0.0) && (cCROSSap >= 0.0));
	}
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
