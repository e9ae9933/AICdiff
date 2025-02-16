using System;
using System.Collections.Generic;
using PixelLiner;

namespace XX
{
	public class LoadTicketManager : MonoBehaviourAutoRun
	{
		public static void PrepareLoadManager()
		{
			if (LoadTicketManager.Instance == null)
			{
				IN._stage.gameObject.AddComponent<LoadTicketManager>();
			}
		}

		public void Awake()
		{
			this.PoolTicket = new ClsPool<LoadTicketManager.LoadTicket>(() => new LoadTicketManager.LoadTicket(), 16);
			this.ATk = new List<LoadTicketManager.LoadTicket>(16);
		}

		public override void OnEnable()
		{
		}

		public void AddTicketInner(string name, string name2, LoadTicketManager.FnLoadProgress FD_Progress, object Target = null, int priority = 0)
		{
			LoadTicketManager.LoadTicket loadTicket = this.PoolTicket.Pool();
			loadTicket.name = name;
			loadTicket.name2 = name2;
			loadTicket.FD_Progress = FD_Progress;
			loadTicket.Target = Target;
			loadTicket.priority = (byte)priority;
			int count = this.ATk.Count;
			for (int i = 0; i < count; i++)
			{
				if (this.ATk[i].priority > loadTicket.priority)
				{
					this.ATk.Insert(i, loadTicket);
					loadTicket = null;
				}
			}
			if (loadTicket != null)
			{
				this.ATk.Add(loadTicket);
			}
			base.runner_assigned = true;
		}

		public void AddTicketInner(PxlCharacter Pc, MTIOneImage MI, int priority = 0)
		{
			this.AddTicketInner("PXL", Pc.title, this.FD_Pxl_MIAssign, MI, priority);
		}

		public static void AddTicket(string name, string name2, LoadTicketManager.FnLoadProgress FD_Progress, object Target = null, int priority = 0)
		{
			LoadTicketManager.Instance.AddTicketInner(name, name2, FD_Progress, Target, priority);
		}

		public static void destructTarget(object Target)
		{
			LoadTicketManager.Instance.destructTargetInner(Target);
		}

		public void destructTargetInner(object Target)
		{
			for (int i = this.ATk.Count - 1; i >= 0; i--)
			{
				if (this.ATk[i].Target == Target)
				{
					this.ATk[i].destruct();
				}
			}
		}

		protected override bool runIRD(float fcnt)
		{
			bool flag = false;
			int num = -1;
			int num2 = this.ATk.Count;
			int i = 0;
			while (i < num2)
			{
				LoadTicketManager.LoadTicket loadTicket = this.ATk[i];
				if (!loadTicket.destructed)
				{
					if (num >= 0 && num < (int)loadTicket.priority)
					{
						break;
					}
					if (!loadTicket.FD_Progress(loadTicket))
					{
						loadTicket.destruct();
					}
				}
				if (!loadTicket.destructed)
				{
					flag = true;
					if (num == -1)
					{
						num = (int)loadTicket.priority;
					}
					i++;
				}
				else
				{
					this.ATk.RemoveAt(i);
					this.PoolTicket.Release(loadTicket);
					num2--;
				}
			}
			return flag;
		}

		public override string ToString()
		{
			return "LoadTicketManager";
		}

		public static LoadTicketManager Instance
		{
			get
			{
				return IN._stage.GetComponent<LoadTicketManager>();
			}
		}

		private ClsPool<LoadTicketManager.LoadTicket> PoolTicket;

		private List<LoadTicketManager.LoadTicket> ATk;

		private LoadTicketManager.FnLoadProgress FD_Pxl_MIAssign = delegate(LoadTicketManager.LoadTicket Tk)
		{
			MTIOneImage mtioneImage = Tk.Target as MTIOneImage;
			if (mtioneImage.MI != null && mtioneImage.MI.Tx != null)
			{
				PxlCharacter pxlCharacter = PxlsLoader.getPxlCharacter(Tk.name2);
				if (pxlCharacter != null)
				{
					mtioneImage.ReplaceExternalPngForPxl(pxlCharacter, true);
					MTRX.assignMI(pxlCharacter, mtioneImage.MI);
					return false;
				}
			}
			return true;
		};

		public delegate bool FnLoadProgress(LoadTicketManager.LoadTicket Tk);

		public class LoadTicket
		{
			public bool destructed
			{
				get
				{
					return this.FD_Progress == null;
				}
			}

			public void destruct()
			{
				this.FD_Progress = null;
				this.Target = null;
			}

			public byte priority;

			public object Target;

			public string name = "";

			public string name2 = "";

			public LoadTicketManager.FnLoadProgress FD_Progress;
		}
	}
}
