using System;
using System.Collections.Generic;
using Better;
using UnityEngine;

namespace XX
{
	public class MImage : IDisposable
	{
		public MImage(Texture _Tx_)
		{
			this.OOMtr = new BDic<int, BDic<Shader, Material>>(1);
			this.Tx_ = _Tx_;
		}

		public Texture Tx
		{
			get
			{
				return this.Tx_;
			}
			set
			{
				if (this.Tx_ == value)
				{
					return;
				}
				this.Tx_ = value;
				foreach (KeyValuePair<int, BDic<Shader, Material>> keyValuePair in this.OOMtr)
				{
					foreach (KeyValuePair<Shader, Material> keyValuePair2 in keyValuePair.Value)
					{
						keyValuePair2.Value.mainTexture = this.Tx_;
					}
				}
			}
		}

		public void Dispose()
		{
			this.Tx_ = null;
			this.DisposeMaterial();
		}

		public void DisposeMaterial()
		{
			foreach (KeyValuePair<int, BDic<Shader, Material>> keyValuePair in this.OOMtr)
			{
				foreach (KeyValuePair<Shader, Material> keyValuePair2 in keyValuePair.Value)
				{
					keyValuePair2.Value.mainTexture = null;
					IN.DestroyOne(keyValuePair2.Value);
				}
			}
			this.OOMtr.Clear();
		}

		public Material getMtr(int stencil_ref)
		{
			return this.getMtr(BLEND.NORMAL, stencil_ref);
		}

		public Material getMtr(BLEND blnd = BLEND.NORMAL, int stencil_ref = -1)
		{
			if (stencil_ref >= 0)
			{
				BLEND blend;
				if (blnd != BLEND.NORMAL)
				{
					if (blnd != BLEND.NORMALBORDER8)
					{
						blend = blnd;
					}
					else
					{
						blend = BLEND.NORMALBORDER8ST;
					}
				}
				else
				{
					blend = BLEND.NORMALST;
				}
				blnd = blend;
			}
			else
			{
				BLEND blend;
				if (blnd != BLEND.NORMALST)
				{
					if (blnd != BLEND.NORMALBORDER8ST)
					{
						blend = blnd;
					}
					else
					{
						blend = BLEND.NORMALBORDER8;
					}
				}
				else
				{
					blend = BLEND.NORMAL;
				}
				blnd = blend;
			}
			return this.getMtr(MTRX.blend2ShaderImg(blnd), stencil_ref);
		}

		public Material getMtr(Shader Shd, int stencil_ref = -1)
		{
			bool flag;
			return this.getMtr(out flag, Shd, stencil_ref);
		}

		public Material getMtr(out bool created, Shader Shd, int stencil_ref = -1)
		{
			created = false;
			BDic<Shader, Material> bdic;
			if (!this.OOMtr.TryGetValue(stencil_ref, out bdic))
			{
				bdic = (this.OOMtr[stencil_ref] = new BDic<Shader, Material>(1));
			}
			Material material = X.Get<Shader, Material>(bdic, Shd);
			if (material == null)
			{
				material = (bdic[Shd] = MTRX.newMtr(Shd));
				created = true;
			}
			material.mainTexture = this.Tx_;
			MTRX.fixMaterialStencilRef(material, Shd, stencil_ref);
			return material;
		}

		public int width
		{
			get
			{
				return this.Tx_.width;
			}
		}

		public int height
		{
			get
			{
				return this.Tx_.height;
			}
		}

		private Texture Tx_;

		public BDic<int, BDic<Shader, Material>> OOMtr;
	}
}
