using System;
using System.Collections.Generic;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2ChipLight : M2Light
	{
		public M2ChipLight(Map2d _Mp, M2Mover _FollowMv)
			: base(_Mp, _FollowMv)
		{
		}

		public M2ChipLight(M2ChipLight _Lig)
			: base(null, null)
		{
			if (_Lig == null)
			{
				return;
			}
			this.Cp = _Lig.Cp;
			this.readString(_Lig.writeString().Split(new char[] { '|' }));
		}

		public M2ChipLight initPuts(M2Puts _Cp)
		{
			this.Cp = _Cp;
			Vector2 vector = new Vector2(this.basex, this.basey);
			if (this.Cp.flip)
			{
				vector.x = (float)this.Cp.Img.iwidth - vector.x;
			}
			vector.x -= (float)(this.Cp.Img.iwidth / 2);
			vector.y = -(vector.y - (float)(this.Cp.Img.iheight / 2));
			vector = X.ROTV2e(vector, this.Cp.draw_rotR);
			this.mapx = vector.x / this.Cp.CLEN;
			this.mapy = -vector.y / this.Cp.CLEN;
			return this;
		}

		public string writeString()
		{
			return string.Concat(new string[]
			{
				this.Col.ToString(),
				"|",
				this.fill_radius.ToString(),
				"|",
				this.radius.ToString(),
				"|",
				this.basex.ToString(),
				"|",
				this.basey.ToString(),
				"|",
				this.vib_x.ToString(),
				"|",
				this.vib_y.ToString(),
				"|",
				this.timescale_x.ToString(),
				"|",
				this.timescale_y.ToString(),
				"|"
			});
		}

		public void readString(string[] Ad)
		{
			try
			{
				this.Col = new C32(Ad[0]);
				this.fill_radius = X.Nm(Ad[1], 0f, false);
				this.radius = X.Nm(Ad[2], 0f, false);
				this.basex = X.Nm(Ad[3], 0f, false);
				this.basey = X.Nm(Ad[4], 0f, false);
				this.vib_x = X.Nm(Ad[5], 0f, false);
				this.vib_y = X.Nm(Ad[6], 0f, false);
				this.timescale_x = X.Nm(Ad[7], 1f, false);
				this.timescale_y = X.Nm(Ad[8], 1f, false);
			}
			catch
			{
			}
		}

		public override void drawLight(MeshDrawer Md, M2SubMap Sm, float alpha, float fcnt)
		{
			base.drawLight(Md, Sm, alpha, fcnt);
			this.af += fcnt;
		}

		protected override void calcDrawMeshPos(out float x, out float y)
		{
			x = this.Mp.map2meshx((float)(this.Cp.drawx + this.Cp.iwidth / 2) * this.Mp.rCLEN + this.mapx);
			y = this.Mp.map2meshy((float)(this.Cp.drawy + this.Cp.iheight / 2) * this.Mp.rCLEN + this.mapy);
		}

		public override void drawLightAt(MeshDrawer Md, float x, float y, float scale, float alpha = 1f)
		{
			if (this.MxTransformMeshPos.m22 != 1f)
			{
				Vector3 vector = new Vector3(x, y, 0f);
				vector = this.MxTransformMeshPos.MultiplyPoint3x4(vector);
				x = vector.x;
				y = vector.y;
			}
			float num = ((this.Cp != null) ? this.Cp.light_alpha : 1f);
			if (num == 0f)
			{
				return;
			}
			if (this.vib_x != 0f && this.timescale_x != 0f)
			{
				if (this.vib_x_duration < 0f)
				{
					this.vib_x_duration = X.NIXP(300f, 400f);
				}
				if (this.af == 0f)
				{
					this.af = (float)X.xors(1024);
				}
				x += this.vib_x * X.COSI(this.af, this.vib_x_duration * this.timescale_x) * scale;
			}
			if (this.vib_y != 0f && this.timescale_y != 0f)
			{
				if (this.vib_y_duration < 0f)
				{
					this.vib_y_duration = X.NIXP(300f, 400f);
				}
				if (this.af == 0f)
				{
					this.af = (float)X.xors(1024);
				}
				y += this.vib_y * X.COSI(this.af + 180f, this.vib_y_duration * this.timescale_y) * scale;
			}
			base.drawLightAt(Md, x, y, scale, alpha * num);
		}

		public void setMatrix(Matrix4x4 Mx)
		{
			this.MxTransformMeshPos = Mx;
			this.MxTransformMeshPos.m22 = 0f;
		}

		public static List<M2ChipLight> getFromMeta(M2ChipImage Cp, Map2d Mp = null)
		{
			if (Mp == null)
			{
				Mp = M2DBase.Instance.curMap;
			}
			if (Cp.Meta == null)
			{
				return null;
			}
			string[] array = Cp.Meta.Get("__light");
			if (array == null || array.Length == 0)
			{
				return null;
			}
			List<M2ChipLight> list = new List<M2ChipLight>(array.Length);
			int num = array.Length;
			for (int i = 0; i < num; i++)
			{
				string[] array2 = array[i].Split(new char[] { '|' });
				M2ChipLight m2ChipLight = new M2ChipLight(Mp, null);
				m2ChipLight.readString(array2);
				list.Add(m2ChipLight);
			}
			return list;
		}

		public override string ToString()
		{
			if (this._tostring == null)
			{
				this._tostring = "ChipLight @ " + this.Cp.ToString();
			}
			return this._tostring;
		}

		public M2Puts Cp;

		public float basex;

		public float basey;

		public float vib_x;

		public float vib_y;

		public float af;

		public float vib_x_duration = -1f;

		public float vib_y_duration = -1f;

		public float timescale_x = 1f;

		public float timescale_y = 1f;

		private Matrix4x4 MxTransformMeshPos = Matrix4x4.identity;
	}
}
