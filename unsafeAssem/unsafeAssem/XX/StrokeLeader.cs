using System;
using UnityEngine;

namespace XX
{
	public struct StrokeLeader
	{
		public void MoveTo(float _x, float _y, float _t, Color32 _C, int _first_ver_i)
		{
			this.x = _x;
			this.first_x = _x;
			this.y = _y;
			this.first_y = _y;
			this.t = _t;
			this.ag01 = 0f;
			this.first = true;
			this.C = _C;
			this.first_ver_i = _first_ver_i;
		}

		public void Set(float _x, float _y, float _t, float _ag01, Color32 _C)
		{
			this.x = _x;
			this.y = _y;
			this.t = _t;
			this.ag01 = _ag01;
			this.C = _C;
		}

		public float x;

		public float y;

		public float t;

		public float ag01;

		public bool first;

		public int first_ver_i;

		public float first_x;

		public float first_y;

		public Color32 C;
	}
}
