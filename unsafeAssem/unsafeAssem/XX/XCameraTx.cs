using System;
using UnityEngine;

namespace XX
{
	public class XCameraTx : XCameraBase
	{
		public XCameraTx(string _key, Color32 BgCol, int w, int h, RenderTextureFormat Format = RenderTextureFormat.ARGB32)
			: base(_key)
		{
			if (w > 0 && h > 0)
			{
				this.Tx = new RenderTexture(w, h, 0, Format);
				this.need_dispose = true;
			}
		}

		public XCameraTx(string _key, Color32 BgCol, RenderTexture Rd, bool _need_dispose = false)
			: base(_key)
		{
			this.Tx = Rd;
			this.need_dispose = _need_dispose;
		}

		public override void destroy(bool destory_gameobject = true)
		{
			base.destroy(destory_gameobject);
			this.releaseTexture();
		}

		public XCameraTx releaseTexture()
		{
			if (this.need_dispose)
			{
				BLIT.nDispose(this.Tx);
			}
			this.need_dispose = false;
			this.Tx = null;
			return this;
		}

		public RenderTexture getTexture()
		{
			return this.Tx;
		}

		public XCameraTx setTexture(RenderTexture _Tx, bool _need_dispose = false)
		{
			this.releaseTexture();
			this.Tx = _Tx;
			this.need_dispose = _need_dispose;
			return this;
		}

		protected override void renderBindingsInit()
		{
			Graphics.SetRenderTarget(this.Tx);
			if (this.clear_color)
			{
				GL.Clear(true, true, this.BgCol);
			}
		}

		protected override void renderBindingsQuit()
		{
			Graphics.SetRenderTarget(null);
		}

		public Color32 BgCol;

		public bool clear_color = true;

		private RenderTexture Tx;

		public bool need_dispose;

		public ProjectionContainer JCon;
	}
}
