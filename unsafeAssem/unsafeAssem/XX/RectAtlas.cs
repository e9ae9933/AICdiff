using System;
using System.Collections.Generic;
using UnityEngine;

namespace XX
{
	public class RectAtlas
	{
		public RectAtlas(int w, int h)
		{
			this.current_id = 1U;
			this.ANode = new List<RectAtlas.AtlasNode>(64);
			if (w > 0 && h > 0)
			{
				this.Clear(w, h);
			}
		}

		public virtual void Clear(int w, int h)
		{
			RectAtlas.AtlasNode atlasNode = new RectAtlas.AtlasNode(0, 0, w, h)
			{
				id = 1U
			};
			this.width = w;
			this.height = h;
			this.current_id = 1U;
			this.ANode.Clear();
			this.ANode.Add(atlasNode);
			this.use_w = (this.use_h = 0);
		}

		private void assignNode(ref RectAtlas.AtlasNode Node)
		{
			if (this.ANode.Count >= this.ANode.Capacity)
			{
				this.ANode.Capacity = this.ANode.Count * 2;
			}
			uint num = this.current_id + 1U;
			this.current_id = num;
			Node.id = num;
			this.ANode.Add(Node);
		}

		private void removeNode(RectAtlas.AtlasNode Node)
		{
			RectAtlas.AtlasNode atlasNode = default(RectAtlas.AtlasNode);
			this.removeNode(Node, ref atlasNode);
		}

		private void removeNode(RectAtlas.AtlasNode Node, ref RectAtlas.AtlasNode ReplaceNode)
		{
			int i = this.ANode.Count - 1;
			while (i >= 0)
			{
				RectAtlas.AtlasNode atlasNode = this.ANode[i];
				if (atlasNode.id == Node.id)
				{
					if (ReplaceNode.valid)
					{
						ReplaceNode.id = atlasNode.id;
						this.ANode[i] = ReplaceNode;
						return;
					}
					this.ANode.RemoveAt(i);
					if (Node.id == this.current_id)
					{
						this.current_id -= 1U;
					}
					return;
				}
				else
				{
					i--;
				}
			}
		}

		public virtual RectInt createRect(int imgw, int imgh, out int cost)
		{
			RectAtlas.AtlasNode atlasNode = default(RectAtlas.AtlasNode);
			cost = 0;
			for (;;)
			{
				int count = this.ANode.Count;
				for (int i = 0; i < count; i++)
				{
					atlasNode = this.ANode[i].Insert(this, ref cost, imgw, imgh);
					if (atlasNode.valid)
					{
						break;
					}
				}
				if (atlasNode.valid)
				{
					break;
				}
				int num = this.width;
				int num2 = this.height;
				if (this.width > this.height)
				{
					this.height *= 2;
					this.wholeExtendAfter();
					int count2 = this.ANode.Count;
					this.ANode.Add(new RectAtlas.AtlasNode(0, num2, num, this.height - num2));
				}
				else
				{
					this.width *= 2;
					this.wholeExtendAfter();
					int count3 = this.ANode.Count;
					this.ANode.Add(new RectAtlas.AtlasNode(num, 0, this.width - num, num2));
				}
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

		public RectAtlas.FnExtend fnExtend;

		private List<RectAtlas.AtlasNode> ANode;

		private uint current_id = 1U;

		public delegate void FnExtend(int width, int height);

		private struct AtlasNode
		{
			public bool used
			{
				get
				{
					return this.id == uint.MaxValue;
				}
				set
				{
					if (value)
					{
						this.id = uint.MaxValue;
					}
				}
			}

			public AtlasNode(int _x, int _y, int _w, int _h)
			{
				this.x = _x;
				this.y = _y;
				this.w = _w;
				this.h = _h;
				this.id = 0U;
			}

			public AtlasNode(int _x, int _y, int _w, int _h, RectAtlas.AtlasNode _Branch0, RectAtlas.AtlasNode _Branch1)
			{
				this.x = _x;
				this.y = _y;
				this.w = _w;
				this.h = _h;
				this.id = 0U;
			}

			public void Clear(int _w, int _h)
			{
				this.w = _w;
				this.h = _h;
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

			public bool valid
			{
				get
				{
					return this.w > 0 && this.h > 0;
				}
			}

			public override string ToString()
			{
				return string.Concat(new string[]
				{
					this.x.ToString(),
					", ",
					this.y.ToString(),
					", ",
					this.r.ToString(),
					", ",
					this.b.ToString()
				});
			}

			public bool isContaining(int imgw, int imgh)
			{
				return imgw <= this.w && imgh <= this.h;
			}

			public bool isFit(int imgw, int imgh)
			{
				return imgw == this.w && imgh == this.h;
			}

			public RectAtlas.AtlasNode Insert(RectAtlas Con, ref int cost, int imgw, int imgh)
			{
				if (this.used || !this.isContaining(imgw, imgh))
				{
					return default(RectAtlas.AtlasNode);
				}
				cost++;
				if (this.isFit(imgw, imgh))
				{
					Con.removeNode(this);
					this.used = true;
					return this;
				}
				int num = this.w - imgw;
				int num2 = this.h - imgh;
				RectAtlas.AtlasNode atlasNode;
				RectAtlas.AtlasNode atlasNode2;
				if (num > num2)
				{
					atlasNode = new RectAtlas.AtlasNode(this.x, this.y, imgw, this.h);
					atlasNode2 = new RectAtlas.AtlasNode(this.x + imgw, this.y, this.w - imgw, this.h);
				}
				else
				{
					atlasNode = new RectAtlas.AtlasNode(this.x, this.y, this.w, imgh);
					atlasNode2 = new RectAtlas.AtlasNode(this.x, this.y + imgh, this.w, this.h - imgh);
				}
				Con.removeNode(this, ref atlasNode);
				Con.assignNode(ref atlasNode2);
				return atlasNode.Insert(Con, ref cost, imgw, imgh);
			}

			public readonly int x;

			public readonly int y;

			public int w;

			public int h;

			public uint id;
		}
	}
}
