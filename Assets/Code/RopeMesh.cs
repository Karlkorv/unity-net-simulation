using System.Collections.Generic;
using UnityEngine;

namespace Code
{
	public class RopeMesh
	{
		private int m_edges = 9;
		private float m_radius = 0.1f;

		private List<Vector3> m_vertices;
		private List<int> m_triangles;
		private List<Vector2> m_uvs;
		private bool m_rebuild;
		private float m_totalDist;
		private Vector3 m_look = new Vector3(23,42,12).normalized;

		/// <summary>
		/// Generates the mesh.
		/// </summary>
		/// <param name="ropeMesh">Rope's mesh.</param>
		/// <param name="points">Points of the mesh, if the list is changed rebuild must be true.</param>
		/// <param name="rebuild">If set to <c>true</c> the triangles are rebuilt as well.</param>
		public void GenerateMesh(Mesh ropeMesh, List<Vector3> points, bool rebuild)
		{
			m_look = Random.onUnitSphere;

			m_totalDist = 0;
			this.m_rebuild = rebuild;
			m_vertices = new List<Vector3> ();
			m_triangles = new List<int> ();
			m_uvs= new List<Vector2> ();
			for (int i=0; i<points.Count-1; i++)
			{	
				Vector3 p0 = (i==0) ? Vector3.up : points[i-1]; //Not used when i==0
				Vector3 p1 = points[i];
				Vector3 p2 = points[i+1];
				Vector3 dir = (i==0) ? (p2-p1) : ((p1-p0).normalized + (p2-p1).normalized);

				if(Vector3.Dot(m_look, dir) > 0.8f)
					m_look = Random.onUnitSphere;
				//if the rope direction is parallel with the right vector, strange things happen.
				Quaternion rot = Quaternion.LookRotation(dir, m_look);
				if (i==0)
				{
					CreateEdge(p1, rot, false, 0.0f); //Create the top.
				}
				else
				{
					CreateCircle(p1, rot, (p2-p1).magnitude);
					if (rebuild)
						CreateTriangles(m_vertices.Count-m_edges*2, m_vertices.Count-m_edges);
				}
			}
			Vector3 direction = points [points.Count - 1] - points [points.Count - 2];
			//Create the bottom
			CreateEdge(points[points.Count - 1], Quaternion.LookRotation(direction, m_look), true, direction.magnitude);

			if (rebuild)
			{
				ropeMesh.Clear ();
				ropeMesh.vertices = m_vertices.ToArray();
				ropeMesh.triangles = m_triangles.ToArray();
			}
			else
			{
				ropeMesh.vertices = m_vertices.ToArray();
			}

			ropeMesh.uv = m_uvs.ToArray();
			ropeMesh.RecalculateNormals ();
		}

		/// <summary>
		/// Creates the top and bottom.
		/// </summary>
		/// <param name="center">The origo of the circle</param>
		/// <param name="rot">The facing direction of the circle in 3D</param>
		/// <param name="end">If set to <c>true</c> it's the end (bottom) else the start (top).</param>
		/// <param name="dist">The distance from the start of the rope. </param>
		private void CreateEdge(Vector3 center, Quaternion rot, bool end, float dist)
		{
			int centerIndex = 0;
			if (!end)
			{
				centerIndex = m_vertices.Count;
				m_vertices.Add (center);
				m_uvs.Add(new Vector2(0.5f, 0.5f));
			}
			int circleIndex = m_vertices.Count;
			CreateCircle(center, rot, dist);
			if (end) //Top
			{
				centerIndex = m_vertices.Count;
				m_vertices.Add (center);
				m_uvs.Add(new Vector2(0.5f, 0.5f));
				if (m_rebuild)
				{
					CreateTriangles(circleIndex-m_edges, circleIndex);
					for (int i=0; i<m_edges; i++)
					{
						m_triangles.Add(centerIndex);
						m_triangles.Add(circleIndex + i);
						m_triangles.Add(circleIndex + (i+1)%m_edges);
					}
				}
			} 
			else //Bottom
			{
				if (m_rebuild)
				{
					//To make sure backside culling, these need to be in the opposite order.
					for (int i=0; i<m_edges; i++)
					{
						m_triangles.Add(circleIndex + (i+1)%m_edges);
						m_triangles.Add(circleIndex + i);
						m_triangles.Add(centerIndex);
					}
				}
			}


		}

		/// <summary>
		/// Creates triangles between two circles, into a cylinder. Doesn't add the top or bottom.
		/// </summary>
		/// <param name="start1">The first virtice of circle 1</param>
		/// <param name="start2">The first virtice of circle 2</param>
		private void CreateTriangles(int start1, int start2)
		{
			for (int i=0; i<m_edges; i++)
			{
				m_triangles.Add(start2 + (i+1)%m_edges);
				m_triangles.Add(start2 + i);
				m_triangles.Add(start1 + i);
			
				m_triangles.Add(start1 + (i+1)%m_edges);
				m_triangles.Add(start2 + (i+1)%m_edges);
				m_triangles.Add(start1 + i);
			}
		}

		/// <summary>
		/// Creates a circle of vertices with center as origo and facing the rot quaterion.
		/// </summary>
		/// <param name="center">The origo.</param>
		/// <param name="rot">The facing direction.</param>
		/// <param name="dist">Used to set the uv, the distance so far of the rope.</param>
		private void CreateCircle(Vector3 center, Quaternion rot, float dist)
		{
			m_totalDist += dist;
			for (int i=0; i<m_edges; i++)
			{
				Quaternion circleRot = Quaternion.Euler(new Vector3(0, 0, (360*i)/m_edges));
				Vector3 newPoint = center + rot* circleRot * (Vector3.up*m_radius);
				m_vertices.Add(newPoint);
				m_uvs.Add(new Vector2(1.0f*i/(m_edges-1), m_totalDist));

			}
		}

	}
}
