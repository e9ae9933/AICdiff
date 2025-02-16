using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Better;
using UnityEngine;

namespace XX
{
	public class LabelPointContainer<T> where T : DRect
	{
		public LabelPointContainer()
		{
			this.OLp = new BDic<string, T>();
			this.Atitle = new string[4];
		}

		public Vector2 getPos(string key, float _shx = 0f, float _shy = 0f)
		{
			Vector2 vector = new Vector2(-1000f, -1000f);
			if (REG.match(key, LabelPointContainer<T>.RegShift))
			{
				_shx += X.Nm(REG.R1, 0f, false);
				_shy += X.Nm(REG.R2, 0f, false);
			}
			string text = key;
			string text2 = "C";
			if (REG.match(text, LabelPointContainer<T>.RegFindLabelPoint))
			{
				text = REG.R1;
				text2 = REG.R2;
			}
			bool flag = text2.IndexOf("F") >= 0;
			DRect drect = this.Get(text, true, false);
			if (drect == null)
			{
				vector = new Vector2(_shx, _shy);
			}
			else
			{
				if (text2 != "")
				{
					vector = this.getPosSideCheck(text2, drect);
				}
				if (vector.x == -1000f)
				{
					vector.x = drect.cx;
				}
				if (vector.y == -1000f)
				{
					vector.y = drect.cy;
				}
				vector.x += _shx;
				vector.y += _shy;
				if (flag)
				{
					vector = drect.getFix(vector);
				}
			}
			return vector;
		}

		public virtual Vector2 getPosSideCheck(string pt_pos, DRect Pt)
		{
			Vector2 vector = new Vector2(-1000f, -1000f);
			if (pt_pos.IndexOf("r") != -1)
			{
				vector = Pt.getRandom();
			}
			else if (REG.match(pt_pos, LabelPointContainer<T>.RegLRTB))
			{
				vector = Pt.getSide((pt_pos.IndexOf("L") != -1) ? 0 : ((pt_pos.IndexOf("R") != -1) ? 2 : 1), (pt_pos.IndexOf("T") != -1) ? 0 : ((pt_pos.IndexOf("B") != -1) ? 2 : 1));
			}
			return vector;
		}

		public virtual T Get(string key, bool no_make = true, bool no_error = false)
		{
			return X.Get<string, T>(this.OLp, key);
		}

		public T Get(int i)
		{
			if (!X.BTW(0f, (float)i, (float)this.count))
			{
				return default(T);
			}
			return this.OLp[this.Atitle[i]];
		}

		public T[] makeArray()
		{
			T[] array = new T[this.count];
			for (int i = 0; i < this.count; i++)
			{
				array[i] = this.OLp[this.Atitle[i]];
			}
			return array;
		}

		public void makeArray(List<T> A)
		{
			foreach (KeyValuePair<string, T> keyValuePair in this.OLp)
			{
				A.Add(keyValuePair.Value);
			}
		}

		public int pushTo(ref T[] Ret, int first_i = 0)
		{
			if (Ret == null)
			{
				Ret = new T[this.count];
			}
			else if (Ret.Length <= first_i + this.count)
			{
				Array.Resize<T>(ref Ret, first_i + this.count);
			}
			for (int i = 0; i < this.count; i++)
			{
				Ret[first_i++] = this.OLp[this.Atitle[i]];
			}
			return first_i;
		}

		public void setFromArray(T[] A)
		{
			this.Clear();
			this.count = X.countNotEmpty<T>(A);
			if (this.Atitle.Length < this.count)
			{
				Array.Resize<string>(ref this.Atitle, this.count);
			}
			for (int i = 0; i < this.count; i++)
			{
				this.Atitle[i] = A[i].key;
				this.OLp[A[i].key] = A[i];
			}
			this.reindex();
		}

		public void setFromArray(List<T> A)
		{
			this.Clear();
			this.count = A.Count;
			if (this.Atitle.Length < this.count)
			{
				Array.Resize<string>(ref this.Atitle, this.count);
			}
			for (int i = 0; i < this.count; i++)
			{
				this.Atitle[i] = A[i].key;
				this.OLp[A[i].key] = A[i];
			}
			this.reindex();
		}

		public int Length
		{
			get
			{
				return this.count;
			}
		}

		public virtual void reindex()
		{
		}

		public virtual LabelPointContainer<T> Clear()
		{
			this.OLp.Clear();
			X.ALLN<string>(this.Atitle);
			this.count = 0;
			return this;
		}

		public T Add(T P)
		{
			if (P != null)
			{
				this.OLp[P.key] = P;
				if (X.isinC<string>(this.Atitle, P.key) == -1)
				{
					X.pushToEmptyS<string>(ref this.Atitle, P.key, ref this.count, 20);
				}
			}
			return P;
		}

		public T Insert(T P, int index)
		{
			if (P != null)
			{
				this.OLp[P.key] = P;
				if (X.isinC<string>(this.Atitle, P.key) == -1)
				{
					X.unshiftToEmptyR(ref this.Atitle, P.key, index);
				}
			}
			return P;
		}

		public LabelPointContainer<T> CopyFrom(LabelPointContainer<T> Src)
		{
			int length = Src.Length;
			for (int i = 0; i < length; i++)
			{
				this.Add(Src.Get(i));
			}
			return this;
		}

		public virtual T Rem(string key)
		{
			T t = X.Get<string, T>(this.OLp, key);
			if (t != null)
			{
				this.OLp.Remove(key);
				int num = X.isinC<string>(this.Atitle, key);
				if (num != -1)
				{
					X.shiftEmpty<string>(this.Atitle, 1, num, -1);
					this.count--;
				}
			}
			return t;
		}

		public string fineIndividualName(string newname, T MyMtr = default(T))
		{
			if (MyMtr == null)
			{
				return X.fineIndividualName<T>(this.OLp, newname, default(T));
			}
			string text;
			using (BList<T> blist = ListBuffer<T>.Pop(this.count))
			{
				this.makeArray(blist);
				text = X.fineIndividualName<T>(blist, newname, MyMtr);
				if (MyMtr != null)
				{
					MyMtr.key = text;
				}
				this.setFromArray(blist);
			}
			return text;
		}

		public T findStanding(float x, float y, float _extend = 0f, LabelPointListener<T> LPLis = null, int max_len = -1)
		{
			float num = -1f;
			if (LPLis != null)
			{
				LPLis.findStart();
			}
			T t = default(T);
			if (max_len < 0)
			{
				max_len = this.Length;
			}
			foreach (KeyValuePair<string, T> keyValuePair in this.OLp)
			{
				T value = keyValuePair.Value;
				if (value.active && value.isin(x, y, _extend))
				{
					if (LPLis != null)
					{
						LPLis.addStanding(value);
					}
					float lengthInContainer = value.getLengthInContainer(x, y, 0f);
					if (num < 0f || num > lengthInContainer)
					{
						num = lengthInContainer;
						t = value;
					}
				}
			}
			if (LPLis != null)
			{
				LPLis.findEnd(t);
			}
			return t;
		}

		public LabelPointListener<T> beginAll(LabelPointListener<T> LpLis)
		{
			LpLis.beginIterator(this.OLp, this.Atitle);
			return LpLis;
		}

		public T Find(Func<T, bool> Fn)
		{
			for (int i = 0; i < this.count; i++)
			{
				T t = this.OLp[this.Atitle[i]];
				if (Fn(t))
				{
					return t;
				}
			}
			return default(T);
		}

		protected BDic<string, T> OLp;

		private string[] Atitle;

		private int count;

		private static readonly Regex RegShift = new Regex(" *([\\+\\-][\\d\\.]+) *\\,? *([\\+\\-]?[\\d\\.]+)");

		private static readonly Regex RegFindLabelPoint = new Regex(" *(\\w+)(?: *\\. *(\\w+))?");

		private static readonly Regex RegLRTB = new Regex("[LRTB]");
	}
}
