using System;
using XX;

namespace nel
{
	public class aBtnSvd : aBtnNel
	{
		protected override void Awake()
		{
			base.Awake();
		}

		public override ButtonSkin makeButtonSkin(string key)
		{
			if (key != null && (key == "normal" || (key != null && key.Length == 0)))
			{
				this.click_snd = "tool_hand_init";
				return this.Skin = (this.RowSkin = new ButtonSkinSvdRow(this, this.w, this.h));
			}
			return base.makeButtonSkin(key);
		}

		public aBtnSvd setData(int index, SVD.sFile Svd, bool saved = false)
		{
			if (this.RowSkin == null)
			{
				this.StartBtn();
			}
			this.RowSkin.setData(index, Svd, saved);
			return this;
		}

		public SVD.sFile getSvdData()
		{
			return this.RowSkin.getSvdData();
		}

		public override bool run(float fcnt)
		{
			if (!base.run(fcnt))
			{
				return false;
			}
			SVD.sFile svdData = this.RowSkin.getSvdData();
			if (svdData != null && svdData.loadstate == SVD.sFile.STATE.NO_LOAD)
			{
				if (this.call_load_delay == 0)
				{
					bool flag = true;
					if (base.BelongScroll != null && !SVD.isLastFocusedRow(svdData))
					{
						flag = base.BelongScroll.isShowing(this, 0f, this.get_sheight_px() * 0.5f + 10f, 0f, 0f);
					}
					this.call_load_delay = (flag ? 5 : (-5));
				}
				if (this.call_load_delay > 0)
				{
					this.call_load_delay--;
					byte[] array = ((this.ContainerSVD != null) ? this.ContainerSVD.Abuffer_for_header : null);
					if (SVD.initPreparingFileHeader(svdData, base.isSelected(), ref array))
					{
						this.call_load_delay = 0;
						this.RowSkin.setData(this.getSvdIndex(), svdData, false);
					}
					if (this.ContainerSVD != null)
					{
						this.ContainerSVD.Abuffer_for_header = array;
					}
				}
				else if (this.call_load_delay < 0)
				{
					this.call_load_delay++;
				}
			}
			return true;
		}

		protected override aBtn simulateNaviTranslationInner(ref int a, aBtn Dep)
		{
			if (this.ContainerSVD != null)
			{
				return this.ContainerSVD.simulateNaviTranslationInner(ref a, Dep, this.getSvdIndex());
			}
			return Dep;
		}

		public override void hide()
		{
			base.hide();
			this.call_load_delay = 0;
		}

		public int getSvdIndex()
		{
			return this.RowSkin.index;
		}

		public void fineMarker()
		{
			this.RowSkin.fineMarker();
		}

		public ButtonSkinSvdRow RowSkin;

		public UiSVD ContainerSVD;

		public int call_load_delay;
	}
}
