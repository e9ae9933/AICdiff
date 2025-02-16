using System;
using System.Collections.Generic;
using Better;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2PressChecker
	{
		public M2PressChecker(M2Attackable Target)
		{
			this.Mv = Target;
			this.OPublish = new BDic<IPresserBehaviour, M2PressChecker.BhvData>(1);
			this.close_delay = 20f;
			this.ABuf = new List<M2BlockColliderContainer.BCCLine>();
		}

		public void destruct()
		{
			this.OPublish.Clear();
			this.force_crouch_delay = 0f;
		}

		public void addPresser(IPresserBehaviour P, List<M2BlockColliderContainer.BCCLine> Target = null)
		{
			int pressAim = P.getPressAim(this.Mv);
			Map2d mp = this.Mv.Mp;
			M2PressChecker.BhvData bhvData = null;
			if (this.OPublish.TryGetValue(P, out bhvData))
			{
				if (mp.floort - bhvData.lock_t < 15f)
				{
					return;
				}
			}
			else
			{
				bhvData = (this.OPublish[P] = new M2PressChecker.BhvData());
			}
			bhvData.lock_t = mp.floort;
			if (pressAim >= 0)
			{
				int num = CAim._XD(pressAim, 1);
				int num2 = CAim._YD(pressAim, 1);
				float num3 = this.Mv.x - this.Mv.sizex * (float)num;
				float num4 = this.Mv.y + this.Mv.sizey * (float)num2;
				float num5 = ((num != 0) ? 0.5f : (0.8f * this.Mv.sizex));
				float num6 = ((num2 != 0) ? 0.5f : (0.8f * this.Mv.sizey));
				M2PressChecker.ALineBuf.Clear();
				if (Target != null)
				{
					M2PressChecker.ALineBuf.AddRange(Target);
				}
				else
				{
					P.getPressableBccLineNear(num3, num4, num5, num6, CAim.get_opposite((AIM)pressAim), M2PressChecker.ALineBuf);
				}
				if (M2PressChecker.ALineBuf.Count > 0)
				{
					int count = M2PressChecker.ALineBuf.Count;
					for (int i = 0; i < count; i++)
					{
						M2BlockColliderContainer.BCCLine bccline = M2PressChecker.ALineBuf[i];
						if (bhvData.IndexOf(bccline) == -1)
						{
							bhvData.ALine.Add(bccline);
						}
					}
					this.close_delay = 15f;
					return;
				}
				bhvData.lock_t -= 10f;
			}
		}

		public bool addForceCrouchDelay(float t)
		{
			bool flag = this.force_crouch_delay <= 0f;
			this.force_crouch_delay = X.Mx(t, this.force_crouch_delay);
			return flag;
		}

		public bool run()
		{
			if (this.OPublish.Count == 0)
			{
				return false;
			}
			if (this.force_crouch_delay > 0f)
			{
				this.force_crouch_delay = X.Mx(this.force_crouch_delay - Map2d.TS, 0f);
			}
			if (this.Mv.isNoDamageActive(NDMG.PRESSDAMAGE) || !this.Mv.pressdamage_applyable)
			{
				return true;
			}
			Map2d mp = this.Mv.Mp;
			bool flag = this.is_force_crouch;
			foreach (KeyValuePair<IPresserBehaviour, M2PressChecker.BhvData> keyValuePair in this.OPublish)
			{
				M2PressChecker.BhvData value = keyValuePair.Value;
				value.updateDelta(keyValuePair.Key.getTranslatedDelta());
				List<M2BlockColliderContainer.BCCLine> aline = value.ALine;
				for (int i = aline.Count - 1; i >= 0; i--)
				{
					M2BlockColliderContainer.BCCLine bccline = aline[i];
					AIM aim = CAim.get_opposite(bccline.aim);
					int num = CAim._XD(bccline.aim, 1);
					int num2 = CAim._YD(bccline.aim, 1);
					float num3 = 1f + value.inside_len_add[(int)bccline.aim];
					if (!bccline.isNearInside(this.Mv.x + this.Mv.sizex * (float)num, this.Mv.y - this.Mv.sizey * (float)num2, (num != 0) ? 0.24f : (this.Mv.sizex * 0.6f), (num2 != 0) ? 0.14f : (this.Mv.sizey * 0.6f), num3, -1, false))
					{
						aline.RemoveAt(i);
					}
					else
					{
						flag = true;
						float num4 = this.Mv.x - this.Mv.sizex * (float)num;
						float num5 = this.Mv.y + this.Mv.sizey * (float)num2;
						float num6 = ((num != 0) ? 0.14f : X.Mx(this.Mv.sizex - 0.2f, this.Mv.sizex * 0.5f));
						float num7 = ((num2 != 0) ? 0.14f : X.Mx(this.Mv.sizey - 0.2f, this.Mv.sizey * 0.5f));
						M2FootManager footManager = this.Mv.getFootManager();
						if (footManager != null)
						{
							List<M2BlockColliderContainer.BCCLine> cachedBccVector = footManager.getCachedBccVector();
							int count = cachedBccVector.Count;
							int j = 0;
							while (j < count)
							{
								M2BlockColliderContainer.BCCLine bccline2 = cachedBccVector[j];
								if (!bccline2.is_lift && bccline2.isNearInside(num4, num5, num6, num7, num3, (int)aim, false))
								{
									if (!value.pressExecute(this.Mv, keyValuePair.Key, bccline))
									{
										return true;
									}
									this.destruct();
									return false;
								}
								else
								{
									j++;
								}
							}
							for (int k = mp.count_carryable_bcc - 1; k >= 0; k--)
							{
								M2BlockColliderContainer carryableBCCByIndex = mp.getCarryableBCCByIndex(k);
								if (((num != 0) ? X.Abs(carryableBCCByIndex.BelongTo.vx - bccline.BCC.BelongTo.vx) : X.Abs(carryableBCCByIndex.BelongTo.vy - bccline.BCC.BelongTo.vy)) >= 0.04f && carryableBCCByIndex != bccline.BCC && (!(carryableBCCByIndex.BelongTo is IPresserBehaviour) || !this.OPublish.ContainsKey(carryableBCCByIndex.BelongTo as IPresserBehaviour)))
								{
									this.ABuf.Clear();
									carryableBCCByIndex.getNear(num4, num5, num6, num7, (int)aim, this.ABuf, false, true, num3);
									if (this.ABuf.Count > 0)
									{
										M2PressChecker.BhvData bhvData = null;
										IPresserBehaviour presserBehaviour = null;
										if (carryableBCCByIndex.BelongTo is IPresserBehaviour)
										{
											presserBehaviour = carryableBCCByIndex.BelongTo as IPresserBehaviour;
											this.addPresser(presserBehaviour, this.ABuf);
											bhvData = X.Get<IPresserBehaviour, M2PressChecker.BhvData>(this.OPublish, presserBehaviour);
										}
										if (value.pressExecute(this.Mv, keyValuePair.Key, bccline) || bhvData == null || bhvData.pressExecute(this.Mv, presserBehaviour, this.ABuf))
										{
											this.destruct();
											return false;
										}
										return true;
									}
								}
							}
						}
					}
				}
				if (aline.Count == 0)
				{
					value.lock_t = 0f;
				}
			}
			if (this.close_delay > 0f)
			{
				this.close_delay = X.VALWALK(this.close_delay, 0f, Map2d.TS);
			}
			if (!flag)
			{
				this.destruct();
				return false;
			}
			return true;
		}

		public bool is_force_crouch
		{
			get
			{
				return this.force_crouch_delay > 0f;
			}
		}

		public readonly M2Attackable Mv;

		public static List<M2BlockColliderContainer.BCCLine> ALineBuf = new List<M2BlockColliderContainer.BCCLine>(1);

		public static List<M2BlockColliderContainer.BCCLine> ALineBufNC = new List<M2BlockColliderContainer.BCCLine>(1);

		private BDic<IPresserBehaviour, M2PressChecker.BhvData> OPublish;

		public float close_delay = 15f;

		private const float assign_delay = 15f;

		private float force_crouch_delay;

		private List<M2BlockColliderContainer.BCCLine> ABuf;

		private class BhvData
		{
			public BhvData()
			{
				this.ALine = new List<M2BlockColliderContainer.BCCLine>(1);
				this.inside_len_add = new float[4];
			}

			public int IndexOf(M2BlockColliderContainer.BCCLine Bcc)
			{
				for (int i = this.ALine.Count - 1; i >= 0; i--)
				{
					if (this.ALine[i] == Bcc)
					{
						return i;
					}
				}
				return -1;
			}

			public void updateDelta(Vector2 D)
			{
				if (D.x == 0f)
				{
					this.inside_len_add[0] = (this.inside_len_add[2] = 0f);
				}
				else if (D.x > 0f)
				{
					this.inside_len_add[0] += D.x;
					this.inside_len_add[2] = 0f;
				}
				else
				{
					this.inside_len_add[2] -= D.x;
					this.inside_len_add[0] = 0f;
				}
				if (D.y == 0f)
				{
					this.inside_len_add[1] = (this.inside_len_add[3] = 0f);
					return;
				}
				if (D.y > 0f)
				{
					this.inside_len_add[1] += D.y;
					this.inside_len_add[3] = 0f;
					return;
				}
				this.inside_len_add[3] -= D.y;
				this.inside_len_add[1] = 0f;
			}

			public bool pressExecute(M2Attackable Mv, IPresserBehaviour Pa, M2BlockColliderContainer.BCCLine Ba)
			{
				M2PressChecker.BhvData.ALinePa.Clear();
				M2PressChecker.BhvData.ALinePa.Add(Ba);
				bool flag;
				return Pa.publishPress(Mv, M2PressChecker.BhvData.ALinePa, out flag);
			}

			public bool pressExecute(M2Attackable Mv, IPresserBehaviour Pa, List<M2BlockColliderContainer.BCCLine> ABa)
			{
				bool flag;
				return Pa.publishPress(Mv, ABa, out flag);
			}

			public List<M2BlockColliderContainer.BCCLine> ALine;

			private static List<M2BlockColliderContainer.BCCLine> ALinePa = new List<M2BlockColliderContainer.BCCLine>(1);

			public float lock_t;

			public float[] inside_len_add;

			public bool damage_applied;
		}
	}
}
