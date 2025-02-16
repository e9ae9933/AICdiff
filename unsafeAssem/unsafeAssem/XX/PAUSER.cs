using System;
using System.Collections.Generic;
using Better;
using UnityEngine;

namespace XX
{
	public sealed class PAUSER : IPauseable
	{
		public PAUSER()
		{
			this.Astr_lock_key = new string[4];
			this.OPauseable = new BDic<object, PauseMemItem>();
			this.OPauseableP = new BDic<IPauseable, PauseMemItem>();
		}

		public PAUSER AddM(GameObject Gob)
		{
			MeshRenderer meshRenderer;
			if (Gob.TryGetComponent<MeshRenderer>(out meshRenderer))
			{
				this.OPauseable[meshRenderer] = new PauseMemItemMeshRenderer(meshRenderer);
			}
			return this;
		}

		public PAUSER AddM(MultiMeshRenderer MMRD)
		{
			int length = MMRD.getLength();
			for (int i = 0; i < length; i++)
			{
				this.AddM(MMRD.GetMeshRenderer(i));
			}
			return this;
		}

		public PAUSER AddM(MeshRenderer Mr)
		{
			if (Mr != null)
			{
				this.OPauseable[Mr] = new PauseMemItemMeshRenderer(Mr);
			}
			return this;
		}

		public void AddLock(string s)
		{
			X.pushToEmptyS<string>(ref this.Astr_lock_key, s, ref this.str_i, 4);
		}

		public void Assign(MonoBehaviour Mb)
		{
			if (Mb == null)
			{
				return;
			}
			this.OPauseable[Mb] = new PauseMemItemMonoBehaviour(Mb);
		}

		public void Assign(IPauseable Mb)
		{
			if (Mb == null)
			{
				return;
			}
			this.OPauseableP[Mb] = new PauseMemItemPauser(Mb);
		}

		public void AddP(IPauseable Mb)
		{
			if (Mb == null)
			{
				return;
			}
			this.OPauseableP[Mb] = new PauseMemItemPauser(Mb);
		}

		public PauseMemItemRigidbody Assign(Rigidbody2D R2D)
		{
			if (R2D == null)
			{
				return null;
			}
			PauseMemItemRigidbody pauseMemItemRigidbody = new PauseMemItemRigidbody(R2D);
			this.OPauseable[R2D] = pauseMemItemRigidbody;
			return pauseMemItemRigidbody;
		}

		public PauseMemItemRigidbody Assign(PauseMemItemRigidbody PR2D)
		{
			if (PR2D == null)
			{
				return null;
			}
			this.OPauseable[PR2D] = PR2D;
			return PR2D;
		}

		public void Deassign(object R2D)
		{
			if (R2D == null)
			{
				return;
			}
			this.OPauseable.Remove(R2D);
			if (R2D is IPauseable)
			{
				this.OPauseableP.Remove(R2D as IPauseable);
			}
		}

		public void Clear()
		{
			this.OPauseable.Clear();
			this.OPauseableP.Clear();
			this.str_i = 0;
		}

		public void Pause()
		{
			List<object> list = null;
			foreach (KeyValuePair<object, PauseMemItem> keyValuePair in this.OPauseable)
			{
				try
				{
					keyValuePair.Value.Pause();
				}
				catch
				{
					if (list == null)
					{
						list = new List<object>();
					}
					list.Add(keyValuePair.Key);
				}
			}
			if (list != null)
			{
				int count = list.Count;
				for (int i = 0; i < count; i++)
				{
					this.OPauseable.Remove(list[i]);
				}
			}
			List<IPauseable> list2 = null;
			foreach (KeyValuePair<IPauseable, PauseMemItem> keyValuePair2 in this.OPauseableP)
			{
				try
				{
					keyValuePair2.Value.Pause();
				}
				catch
				{
					if (list2 == null)
					{
						list2 = new List<IPauseable>();
					}
					list2.Add(keyValuePair2.Key);
				}
			}
			if (list2 != null)
			{
				int count2 = list2.Count;
				for (int j = 0; j < count2; j++)
				{
					this.OPauseableP.Remove(list2[j]);
				}
			}
			if (this.resume_to_screen_lock)
			{
				for (int k = 0; k < this.str_i; k++)
				{
					X.REMLOCK(this.Astr_lock_key[k]);
				}
				return;
			}
			for (int l = 0; l < this.str_i; l++)
			{
				X.SCLOCK(this.Astr_lock_key[l]);
			}
		}

		public void Resume()
		{
			List<object> list = null;
			foreach (KeyValuePair<object, PauseMemItem> keyValuePair in this.OPauseable)
			{
				try
				{
					keyValuePair.Value.Resume();
				}
				catch
				{
					if (list == null)
					{
						list = new List<object>();
					}
					list.Add(keyValuePair.Key);
				}
			}
			if (list != null)
			{
				int count = list.Count;
				for (int i = 0; i < count; i++)
				{
					this.OPauseable.Remove(list[i]);
				}
			}
			List<IPauseable> list2 = null;
			foreach (KeyValuePair<IPauseable, PauseMemItem> keyValuePair2 in this.OPauseableP)
			{
				try
				{
					keyValuePair2.Value.Resume();
				}
				catch
				{
					if (list2 == null)
					{
						list2 = new List<IPauseable>();
					}
					list2.Add(keyValuePair2.Key);
				}
			}
			if (list2 != null)
			{
				int count2 = list2.Count;
				for (int j = 0; j < count2; j++)
				{
					this.OPauseableP.Remove(list2[j]);
				}
			}
			if (this.resume_to_screen_lock)
			{
				for (int k = 0; k < this.str_i; k++)
				{
					X.SCLOCK(this.Astr_lock_key[k]);
				}
				return;
			}
			for (int l = 0; l < this.str_i; l++)
			{
				X.REMLOCK(this.Astr_lock_key[l]);
			}
		}

		public override string ToString()
		{
			return "Pauser(" + this.OPauseable.Count.ToString() + ")";
		}

		private BDic<object, PauseMemItem> OPauseable;

		private BDic<IPauseable, PauseMemItem> OPauseableP;

		private string[] Astr_lock_key;

		private int str_i;

		public bool resume_to_screen_lock = true;
	}
}
