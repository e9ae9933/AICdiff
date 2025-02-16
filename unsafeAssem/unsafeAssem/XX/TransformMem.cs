using System;
using UnityEngine;

namespace XX
{
	public class TransformMem
	{
		public TransformMem(Transform Src = null)
		{
			this.Set(Src);
		}

		public TransformMem(string s)
		{
			this.Parse(s, null);
		}

		public TransformMem Set(Transform Src = null)
		{
			if (Src != null)
			{
				this.localScale = Src.localScale;
				this.localEulerAngles = Src.localEulerAngles;
				this.localPosition = Src.localPosition;
			}
			return this;
		}

		public Transform Apply(Transform Dest)
		{
			if (Dest != null)
			{
				Dest.localScale = this.localScale;
				Dest.localEulerAngles = this.localEulerAngles;
				Dest.localPosition = this.localPosition;
			}
			return Dest;
		}

		public TransformMem Parse(string str, Transform Dest = null)
		{
			string[] array = str.Split(new char[] { ',' });
			if (array.Length == 9)
			{
				this.localScale = this.str2vec(array[0], array[1], array[2], 1f);
				this.localPosition = this.str2vec(array[3], array[4], array[5], 0f);
				this.localEulerAngles = this.str2vec(array[6], array[7], array[8], 0f);
			}
			this.Apply(Dest);
			return this;
		}

		public override string ToString()
		{
			return string.Concat(new string[]
			{
				this.vec2str(this.localScale),
				",",
				this.vec2str(this.localPosition),
				",",
				this.vec2str(this.localEulerAngles)
			});
		}

		private string vec2str(Vector3 v)
		{
			return string.Concat(new string[]
			{
				v.x.ToString(),
				",",
				v.y.ToString(),
				",",
				v.z.ToString()
			});
		}

		private Vector3 str2vec(string x, string y, string z, float def = 0f)
		{
			return new Vector3(X.Nm(x, def, false), X.Nm(y, def, false), X.Nm(z, def, false));
		}

		public Vector3 localScale;

		public Vector3 localPosition;

		public Vector3 localEulerAngles;
	}
}
