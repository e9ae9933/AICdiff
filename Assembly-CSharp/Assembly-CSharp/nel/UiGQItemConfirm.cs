using System;

namespace nel
{
	public class UiGQItemConfirm : UiItemConfirm
	{
		public void InitItemConfirm(NelM2DBase _M2D, GuildManager.GQEntry _GQ)
		{
			if (this.FD_fnItemConfirmFinished == null)
			{
				this.FD_fnItemConfirmFinished = (NelItemEntry IE, bool item_delivered) => this.FD_fnItemConfirmFinishedGQ == null || this.FD_fnItemConfirmFinishedGQ(this.GQ, item_delivered);
			}
			this.ATarget.Clear();
			this.GQ = _GQ;
			if (!(_GQ.TargetItem is NelItem))
			{
				return;
			}
			this.ATarget.Add(new NelItemEntry(_GQ.TargetItem as NelItem, (int)_GQ.value1, (byte)_GQ.value2));
			base.InitItemConfirm(_M2D, this.ATarget);
		}

		protected override void StorageCheckAfter(NelItemEntry CurIE)
		{
			if (this.GQ.categ == GuildManager.CATEG.COLLECTNEW)
			{
				NelItem data = CurIE.Data;
				int grade = (int)CurIE.grade;
				ItemStorage.ObtainInfo info = this.M2D.GUILD.InvForCollectNew.getInfo(data);
				if (info == null)
				{
					this.AStorage[0].Reduce(data, this.AStorage[0].getCount(data, -1), -1, true);
					return;
				}
				for (int i = grade; i < 5; i++)
				{
					int count = info.getCount(i);
					int count2 = this.AStorage[0].getCount(data, i);
					if (count2 > count)
					{
						this.AStorage[0].Reduce(data, count2 - count, i, true);
					}
				}
			}
		}

		public UiGQItemConfirm.FnItemConfirmFinishedGQ FD_fnItemConfirmFinishedGQ;

		private GuildManager.GQEntry GQ;

		public delegate bool FnItemConfirmFinishedGQ(GuildManager.GQEntry GQ, bool item_delivered);
	}
}
