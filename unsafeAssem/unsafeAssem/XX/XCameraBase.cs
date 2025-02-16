using System;
using System.Collections.Generic;
using UnityEngine;

namespace XX
{
	public abstract class XCameraBase
	{
		public XCameraBase(string _key)
		{
			this.key = _key;
		}

		public virtual void destroy(bool destory_gameobject = true)
		{
			this.releaseAllBinder();
		}

		public void releaseAllBinder()
		{
			this.ABind = null;
			this.need_sort_binds = false;
		}

		public void assignRenderFunc(XCameraBase DC)
		{
			if (DC.ABind != null)
			{
				if (this.ABind == null)
				{
					this.ABind = new List<ICameraRenderBinder>(DC.ABind.Count);
				}
				this.ABind.AddRange(DC.ABind);
				this.need_sort_binds = true;
			}
		}

		public virtual void assignRenderFunc(ICameraRenderBinder Fn)
		{
			if (CameraBidingsBehaviour.assignRenderFunc(Fn, ref this.ABind))
			{
				this.need_sort_binds = true;
			}
		}

		public virtual void deassignRenderFunc(ICameraRenderBinder Fn)
		{
			CameraBidingsBehaviour.deassignRenderFunc(Fn, ref this.ABind);
		}

		public virtual void Render(ProjectionContainer JCon, bool no_static_render)
		{
			if (this.ABind != null)
			{
				if (this.need_sort_binds)
				{
					this.need_sort_binds = false;
					this.ABind.Sort((ICameraRenderBinder a, ICameraRenderBinder b) => CameraBidingsBehaviour.fnSortICameraRenderBinder(a, b));
				}
				int num = this.ABind.Count;
				int i = 0;
				Camera targetCam = this.getTargetCam();
				this.renderBindingsInit();
				while (i < num)
				{
					ICameraRenderBinder cameraRenderBinder = this.ABind[i];
					string text = Bench.P(cameraRenderBinder.ToString());
					bool flag = cameraRenderBinder.RenderToCam(this, JCon, targetCam);
					Bench.Pend(text);
					if (!flag)
					{
						this.ABind.RemoveAt(i);
						if (--num == 0)
						{
							this.ABind = null;
							break;
						}
					}
					else
					{
						i++;
					}
				}
				this.renderBindingsQuit();
				GL.Flush();
			}
		}

		protected abstract void renderBindingsInit();

		protected abstract void renderBindingsQuit();

		public virtual Camera getTargetCam()
		{
			return null;
		}

		public string key;

		protected List<ICameraRenderBinder> ABind;

		public bool need_sort_binds;
	}
}
