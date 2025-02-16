using System;

namespace XX
{
	public sealed class BtnMenuRunner : HideScreen
	{
		protected override bool runIRD(float fcnt)
		{
			if (!base.runIRD(fcnt))
			{
				return false;
			}
			if (this.BCon == null)
			{
				return false;
			}
			this.t += fcnt;
			this.BCon.runCarrier(this.t, null);
			bool flag;
			this.Mn.runMenu(out this.submitted, out flag);
			if (this.bt == 0)
			{
				if (!IN.isMouseOn())
				{
					if (this.t >= 25f)
					{
						this.submitted = true;
						this.deactivate(true);
						return true;
					}
					this.bt = 1;
				}
			}
			else if (this.bt == 1 && this.t >= 6f && IN.isMousePushDown(1))
			{
				int length = this.BCon.Length;
				for (int i = 0; i < length; i++)
				{
					if (this.BCon.GetButton(i).isMouseOver())
					{
						this.submitted = true;
						break;
					}
				}
				if (!this.submitted)
				{
					flag = true;
				}
			}
			if (this.submitted || flag)
			{
				this.submitted = !flag;
				this.deactivate(true);
			}
			return true;
		}

		public override void deactivate(bool immediate = false)
		{
			base.deactivate(immediate);
			this.BCon.Pause();
		}

		public void resetTimer()
		{
			this.t = (float)(this.bt = 0);
			this.submitted = false;
			base.enabled = true;
			base.gameObject.SetActive(true);
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			if (this.BCon != null)
			{
				BtnContainerBasic bcon = this.BCon;
				this.BCon = null;
				bcon.destruct();
			}
		}

		public BtnContainerBasic BCon;

		public BtnMenuRunner.IMenu Mn;

		public float t;

		public int bt;

		public bool submitted;

		public interface IMenu
		{
			void runMenu(out bool submitted, out bool canceled);
		}
	}
}
