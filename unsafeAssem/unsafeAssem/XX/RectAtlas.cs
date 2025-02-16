using System;
using UnityEngine;

namespace XX
{
	public class RectAtlas
	{
		public RectAtlas(int w, int h)
		{
			this.FirstWrapper = new RectAtlas.AtlasNode(0, 0, w, h);
			if (w > 0 && h > 0)
			{
				this.Clear(w, h);
			}
		}

		public virtual void Clear(int w, int h)
		{
			this.FirstWrapper = new RectAtlas.AtlasNode(0, 0, w, h);
			this.width = w;
			this.height = h;
			this.use_w = (this.use_h = 0);
		}

		public virtual RectInt createRect(int imgw, int imgh, out int cost)
		{
			cost = 0;
			RectAtlas.AtlasNode atlasNode;
			while ((atlasNode = this.FirstWrapper.Insert(ref cost, imgw, imgh)) == null)
			{
				int num = this.width;
				int num2 = this.height;
				RectAtlas.AtlasNode atlasNode2;
				if (this.width > this.height)
				{
					this.height *= 2;
					this.wholeExtendAfter();
					this.FirstWrapper.clipRect(true, num, num2, this.use_w, this.use_h);
					atlasNode2 = new RectAtlas.AtlasNode(0, this.use_h, num, this.height - this.use_h);
				}
				else
				{
					this.width *= 2;
					this.wholeExtendAfter();
					this.FirstWrapper.clipRect(false, num, num2, this.use_w, this.use_h);
					atlasNode2 = new RectAtlas.AtlasNode(this.use_w, 0, this.width - this.use_w, this.height);
				}
				this.FirstWrapper = new RectAtlas.AtlasNode(0, 0, this.width, this.height, this.FirstWrapper, atlasNode2);
			}
			this.use_w = X.Mx(atlasNode.r, this.use_w);
			this.use_h = X.Mx(atlasNode.b, this.use_h);
			cost = 4;
			return new RectInt(atlasNode.x, atlasNode.y, atlasNode.w, atlasNode.h);
		}

		protected virtual void wholeExtendAfter()
		{
			if (this.fnExtend != null)
			{
				this.fnExtend(this.width, this.height);
			}
		}

		public int width;

		public int height;

		public int use_w;

		public int use_h;

		private RectAtlas.AtlasNode FirstWrapper;

		public RectAtlas.FnExtend fnExtend;

		public delegate void FnExtend(int width, int height);

		private class AtlasNode
		{
			public bool has_branch
			{
				get
				{
					return this.Branch0 != null;
				}
			}

			public bool h_split
			{
				get
				{
					return this.Branch0 != null && this.Branch0.w < this.w;
				}
			}

			public bool v_split
			{
				get
				{
					return this.Branch0 != null && this.Branch0.h < this.h;
				}
			}

			public AtlasNode(int _x, int _y, int _w, int _h)
			{
				this.used = false;
				this.x = _x;
				this.y = _y;
				this.w = _w;
				this.h = _h;
			}

			public AtlasNode(int _x, int _y, int _w, int _h, RectAtlas.AtlasNode _Branch0, RectAtlas.AtlasNode _Branch1)
			{
				this.used = false;
				this.x = _x;
				this.y = _y;
				this.w = _w;
				this.h = _h;
				this.Branch0 = _Branch0;
				this.Branch1 = _Branch1;
			}

			public void Clear(int _w, int _h)
			{
				this.w = _w;
				this.h = _h;
				this.used = false;
				this.Branch0 = null;
				this.Branch1 = null;
			}

			public int r
			{
				get
				{
					return this.x + this.w;
				}
			}

			public int b
			{
				get
				{
					return this.y + this.h;
				}
			}

			public override string ToString()
			{
				string text = string.Concat(new string[]
				{
					this.x.ToString(),
					", ",
					this.y.ToString(),
					", ",
					this.r.ToString(),
					", ",
					this.b.ToString()
				});
				if (this.Branch0 != null)
				{
					if (this.h_split)
					{
						text = text + "# |" + this.Branch0.r.ToString();
					}
					else
					{
						text = text + "# -" + this.Branch0.b.ToString();
					}
				}
				return text;
			}

			public bool isContaining(int imgw, int imgh)
			{
				return imgw <= this.w && imgh <= this.h;
			}

			public bool isFit(int imgw, int imgh)
			{
				return imgw == this.w && imgh == this.h;
			}

			public RectAtlas.AtlasNode Insert(ref int cost, int imgw, int imgh)
			{
				if (this.used || !this.isContaining(imgw, imgh))
				{
					return null;
				}
				cost++;
				RectAtlas.AtlasNode atlasNode2;
				if (this.Branch0 != null)
				{
					RectAtlas.AtlasNode atlasNode = this.Branch0.Insert(ref cost, imgw, imgh);
					atlasNode2 = ((atlasNode != null) ? atlasNode : this.Branch1.Insert(ref cost, imgw, imgh));
				}
				else
				{
					if (this.isFit(imgw, imgh))
					{
						this.used = true;
						return this;
					}
					int num = this.w - imgw;
					int num2 = this.h - imgh;
					if (num > num2)
					{
						this.Branch0 = new RectAtlas.AtlasNode(this.x, this.y, imgw, this.h);
						this.Branch1 = new RectAtlas.AtlasNode(this.x + imgw, this.y, this.w - imgw, this.h);
					}
					else
					{
						this.Branch0 = new RectAtlas.AtlasNode(this.x, this.y, this.w, imgh);
						this.Branch1 = new RectAtlas.AtlasNode(this.x, this.y + imgh, this.w, this.h - imgh);
					}
					atlasNode2 = this.Branch0.Insert(ref cost, imgw, imgh);
				}
				if (this.Branch0.used && this.Branch1.used)
				{
					this.used = true;
				}
				return atlasNode2;
			}

			public RectAtlas.AtlasNode clipRect(bool extend_vertical, int pre_w, int pre_h, int clip_w, int clip_h)
			{
				if (this.used)
				{
					return null;
				}
				if (extend_vertical)
				{
					if (this.b >= pre_h)
					{
						this.h = clip_h - this.y;
						if (this.h <= 0)
						{
							this.used = true;
						}
					}
				}
				else if (this.r >= pre_h)
				{
					this.w = clip_w - this.x;
					if (this.w <= 0)
					{
						this.used = true;
					}
				}
				if (this.Branch0 != null)
				{
					this.Branch0.clipRect(extend_vertical, pre_w, pre_h, clip_w, clip_h);
					this.Branch1.clipRect(extend_vertical, pre_w, pre_h, clip_w, clip_h);
				}
				return null;
			}

			public bool used;

			public readonly int x;

			public readonly int y;

			public int w;

			public int h;

			public RectAtlas.AtlasNode Branch0;

			public RectAtlas.AtlasNode Branch1;
		}
	}
}
