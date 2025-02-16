using System;
using UnityEngine;

namespace XX
{
	public sealed class RectAtlasTexture : RectAtlas
	{
		public RectAtlasTexture(int w, int h, string _name_header = "", bool _use_mipmap = false, int _depth = 0, RenderTextureFormat _format = RenderTextureFormat.ARGB32)
			: base(0, 0)
		{
			this.name_header = _name_header;
			this.use_mipmap = _use_mipmap;
			this.depth = _depth;
			this.format = _format;
			if (w > 0 && h > 0)
			{
				this.Clear(w, h);
			}
		}

		public override void Clear(int w, int h)
		{
			base.Clear(w, h);
			BLIT.Alloc(ref this.Tx_, this.width, this.height, this.use_mipmap, this.format, this.depth);
		}

		public RectInt createRect(int imgw, int imgh, out int cost, out RenderTexture FirstTx, bool auto_dispose = true)
		{
			FirstTx = (RectAtlasTexture.FirstTxOnCreateRect = this.Tx);
			RectInt rectInt = base.createRect(imgw, imgh, out cost);
			if (FirstTx != this.Tx)
			{
				if (this.copy_previous_image)
				{
					BLIT.Clear(this.Tx, 0U, true);
					BLIT.PasteTo(this.Tx, FirstTx, (float)FirstTx.width * 0.5f, (float)FirstTx.height * 0.5f, 1f, 0f, 0f, 1f, 1f);
					RenderTexture.active = null;
				}
				if (auto_dispose)
				{
					BLIT.nDispose(FirstTx);
				}
			}
			return rectInt;
		}

		public RenderTexture Tx
		{
			get
			{
				return this.Tx_;
			}
		}

		public void destruct()
		{
			BLIT.nDispose(this.Tx);
		}

		protected override void wholeExtendAfter()
		{
			this.Tx_ = this.CreateTexture(this.width, this.height, (this.Tx == RectAtlasTexture.FirstTxOnCreateRect) ? null : this.Tx, string.Concat(new string[]
			{
				this.name_header,
				"(",
				this.Tx.width.ToString(),
				",",
				this.Tx.height.ToString()
			}));
			this.Tx.filterMode = FilterMode.Point;
			base.wholeExtendAfter();
		}

		private RenderTexture CreateTexture(int w, int h, RenderTexture FirstTx = null, string _name = null)
		{
			if (TX.noe(_name))
			{
				_name = this.name_header + w.ToString() + "x" + h.ToString();
			}
			RenderTexture renderTexture = new RenderTexture(w, h, this.depth, this.format);
			renderTexture.filterMode = FilterMode.Point;
			renderTexture.name = _name;
			BLIT.Clear(renderTexture, 0U, true);
			RenderTexture.active = null;
			if (FirstTx != null)
			{
				BLIT.nDispose(FirstTx);
			}
			return renderTexture;
		}

		private RenderTexture Tx_;

		public string name_header = "";

		public bool use_mipmap;

		public int depth;

		private RenderTextureFormat format;

		private static RenderTexture FirstTxOnCreateRect;

		public bool copy_previous_image = true;
	}
}
