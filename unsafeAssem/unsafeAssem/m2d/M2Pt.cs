using System;
using System.Collections.Generic;
using XX;

namespace m2d
{
	public class M2Pt
	{
		public M2Pt(int reserve = 4, int default_cfg = 4)
		{
			this.APuts = new List<M2Puts>(reserve);
			this.cfg_ = default_cfg;
		}

		public M2Pt(M2Puts Pt, int default_cfg = 4)
		{
			this.APuts = new List<M2Puts>(1) { Pt };
			this.cfg_ = default_cfg;
		}

		public M2Pt(M2Pt Src)
		{
			this.APuts = new List<M2Puts>(Src.APuts);
			this.cfg_ = Src.cfg;
		}

		public int cfg
		{
			get
			{
				return this.cfg_ & 4095;
			}
		}

		public int count
		{
			get
			{
				return this.APuts.Count;
			}
		}

		public M2Puts Get(IM2Inputtable Ip, ulong layer_bits = 18446744073709551615UL)
		{
			for (int i = this.count - 1; i >= 0; i--)
			{
				M2Puts m2Puts = this.APuts[i];
				if ((layer_bits & (1UL << m2Puts.Lay.index)) != 0UL && Ip.isSame(m2Puts, false, -1, -1))
				{
					return m2Puts;
				}
			}
			return null;
		}

		public int countLayerPuts(ulong layer_bits)
		{
			int num = 0;
			for (int i = this.count - 1; i >= 0; i--)
			{
				M2Puts m2Puts = this.APuts[i];
				if ((layer_bits & (1UL << m2Puts.Lay.index)) != 0UL)
				{
					num++;
				}
			}
			return num;
		}

		public bool dangerous
		{
			get
			{
				return (this.cfg_ & 4096) != 0;
			}
			set
			{
				if (this.dangerous == value)
				{
					return;
				}
				if (value)
				{
					this.cfg_ |= 4096;
					return;
				}
				this.cfg_ &= -4097;
			}
		}

		private bool has_lift
		{
			get
			{
				return (this.cfg_ & 8192) != 0;
			}
			set
			{
				if (this.has_lift == value)
				{
					return;
				}
				if (value)
				{
					this.cfg_ |= 8192;
					return;
				}
				this.cfg_ &= -8193;
			}
		}

		public void resetConfig()
		{
			this.cfg_ = (this.cfg_ & -4096) + 4;
			this.has_lift = false;
		}

		public int calcConfig(int i, int j, bool consider_active_removed = true)
		{
			int num = this.cfg_ & -4096;
			int num2 = this.cfg_ & 4095;
			int num3 = ((num2 <= 4) ? 0 : (-1));
			for (int k = this.count - 1; k >= 0; k--)
			{
				M2Chip m2Chip = this.APuts[k] as M2Chip;
				int num4;
				if (this.getInnerConfigXy(m2Chip, i, j, out num4, consider_active_removed))
				{
					if (m2Chip.Img.Meta.no_make_checkpoint)
					{
						num |= 4096;
					}
					if (CCON.isLift(num4) && !CCON.isSlope(num4))
					{
						if (num3 >= 0)
						{
							CCON.calcConfig(ref num3, num4);
						}
						num |= 8192;
					}
					else if (num4 > 0)
					{
						if (num3 >= 0 && num4 > 4)
						{
							num3 = -1;
						}
						CCON.calcConfig(ref num2, num4);
					}
				}
			}
			if (!CCON.canStand(num2))
			{
				num &= -8193;
			}
			if (num3 > 0)
			{
				num2 = num3;
			}
			this.cfg_ = num2 | num;
			return num2;
		}

		public bool getInnerConfigXy(M2Chip MC, int x, int y, out int cfg, bool consider_active_removed = true)
		{
			cfg = 0;
			if (MC == null)
			{
				return false;
			}
			if ((consider_active_removed && MC.active_removed) || MC.Img.isBg() || MC.Lay.do_not_consider_config)
			{
				return false;
			}
			if (MC.config_considerable)
			{
				int mapx = MC.mapx;
				int mapy = MC.mapy;
				int num = x - mapx;
				int num2 = y - mapy;
				cfg = MC.getConfig(num, num2);
			}
			return true;
		}

		public void calcConfigManual(int apply_cfg)
		{
			int num = this.cfg_ & -4096;
			int num2 = this.cfg_ & 4095;
			CCON.calcConfig(ref num2, apply_cfg);
			this.cfg_ = num2 | num;
		}

		public M2Puts this[int i]
		{
			get
			{
				return this.APuts[i];
			}
		}

		public M2Chip GetC(int i)
		{
			return this.APuts[i] as M2Chip;
		}

		public void Add(M2Puts MC)
		{
			this.APuts.Add(MC);
		}

		public void Rem(M2Puts MC)
		{
			this.APuts.Remove(MC);
		}

		public void Rem(int i)
		{
			if (i >= 0)
			{
				this.APuts.RemoveAt(i);
			}
		}

		public void resortChips()
		{
			this.APuts.Sort((M2Puts a, M2Puts b) => METACImg.fnSortLay(a, b));
		}

		public bool canStand()
		{
			return CCON.canStand(this.cfg_ & 4095);
		}

		public bool isBlockSlope()
		{
			return CCON.isBlockSlope(this.cfg_ & 4095);
		}

		public bool isSlope()
		{
			return CCON.isSlope(this.cfg_ & 4095);
		}

		public bool isWater()
		{
			return CCON.isWater(this.cfg_ & 4095);
		}

		public bool isLift()
		{
			return CCON.isLift(this.cfg_ & 4095) || (this.has_lift && CCON.canStand(this.cfg_ & 4095));
		}

		public bool isDangerous()
		{
			return CCON.isDangerous(this.cfg_ & 4095) || (this.cfg_ & 4096) != 0;
		}

		public bool isDangerousNotWater()
		{
			return (!CCON.isWater(this.cfg_ & 4095) && CCON.isDangerous(this.cfg_ & 4095)) || (this.cfg_ & 4096) != 0;
		}

		public bool isDangerousCfg()
		{
			return CCON.isDangerous(this.cfg_ & 4095);
		}

		public int getPointPutsTo(int x, int y, bool check_dupe, List<M2Puts> APuts, int adding_max_from_one_pixel = -1, ulong layer_bits = 18446744073709551615UL)
		{
			return this.getFloatPointPutsTo((float)x, (float)y, check_dupe, APuts, adding_max_from_one_pixel, -1000f, layer_bits);
		}

		public int getFloatPointPutsTo(float x, float y, bool check_dupe, List<M2Puts> ADest, int adding_max_from_one_pixel = -1, float picture_strict_extend = -1000f, ulong layer_bits = 18446744073709551615UL)
		{
			int num = 0;
			if (adding_max_from_one_pixel != 1)
			{
				for (int i = ((adding_max_from_one_pixel < 0) ? 0 : X.Mx(0, this.APuts.Count - adding_max_from_one_pixel)); i < this.APuts.Count; i++)
				{
					M2Puts m2Puts = this.APuts[i];
					if ((picture_strict_extend == -1000f || !(m2Puts is M2Picture) || (m2Puts as M2Picture).isContainingRotatedRect(x, y, picture_strict_extend)) && (layer_bits & (1UL << m2Puts.Lay.index)) != 0UL && (!check_dupe || ADest.IndexOf(m2Puts) < 0))
					{
						ADest.Add(m2Puts);
						num++;
					}
				}
			}
			else
			{
				uint num2 = 0U;
				for (int j = this.count - 1; j >= 0; j--)
				{
					M2Puts m2Puts2 = this.APuts[j];
					if ((layer_bits & (1UL << m2Puts2.Lay.index)) != 0UL)
					{
						for (int k = 0; k <= j; k++)
						{
							if (((ulong)num2 & (ulong)(1L << (k & 31))) <= 0UL)
							{
								M2Puts m2Puts3 = this.APuts[k];
								if ((layer_bits & (1UL << m2Puts3.Lay.index)) != 0UL && (k == j || ((m2Puts2.pattern != 0U) ? (m2Puts3.pattern == m2Puts2.pattern) : (m2Puts3.Img.family == m2Puts2.Img.family))))
								{
									num2 |= 1U << k;
									if ((picture_strict_extend == -1000f || !(m2Puts3 is M2Picture) || (m2Puts3 as M2Picture).isContainingRotatedRect(x, y, picture_strict_extend)) && (!check_dupe || ADest.IndexOf(m2Puts3) < 0))
									{
										ADest.Add(m2Puts3);
										num++;
									}
								}
							}
						}
						break;
					}
				}
			}
			return num;
		}

		public int getPointMetaPutsTo(List<M2Puts> APuts, ulong layer_bits = 18446744073709551615UL, string metakey = null)
		{
			int num = 0;
			int count = this.count;
			for (int i = 0; i < count; i++)
			{
				M2Puts m2Puts = this.APuts[i];
				if ((layer_bits & (1UL << m2Puts.Lay.index)) != 0UL && (metakey == null || m2Puts.getMeta().GetS(metakey) != null) && (APuts == null || APuts.IndexOf(m2Puts) == -1))
				{
					if (APuts != null)
					{
						APuts.Add(m2Puts);
					}
					num++;
				}
			}
			return num;
		}

		public List<M2Puts> getPointMetaPutsCheckingTo(List<M2Puts> APuts, string metakey)
		{
			int count = this.count;
			for (int i = 0; i < count; i++)
			{
				M2Puts m2Puts = this.APuts[i];
				if ((metakey == null || m2Puts.getMeta().GetS(metakey) != null) && (APuts == null || APuts.IndexOf(m2Puts) == -1))
				{
					if (APuts == null)
					{
						APuts = new List<M2Puts>(8);
					}
					APuts.Add(m2Puts);
				}
			}
			return APuts;
		}

		public List<M2Puts> getPointMetaPutsCheckingTo(List<M2Puts> APuts, string metakey0, string metakey1)
		{
			int count = this.count;
			for (int i = 0; i < count; i++)
			{
				M2Puts m2Puts = this.APuts[i];
				if (m2Puts.getMeta().GetS(metakey0, metakey1) != null && (APuts == null || APuts.IndexOf(m2Puts) == -1))
				{
					if (APuts == null)
					{
						APuts = new List<M2Puts>(8);
					}
					APuts.Add(m2Puts);
				}
			}
			return APuts;
		}

		public List<M2Puts> findPuts(ChipMover.FnAttractChip FnAttract, List<M2Puts> APt = null)
		{
			int count = this.count;
			for (int i = 0; i < count; i++)
			{
				M2Puts m2Puts = this.APuts[i];
				if ((APt == null || APt.IndexOf(m2Puts) == -1) && FnAttract(m2Puts, APt))
				{
					if (APt == null)
					{
						APt = new List<M2Puts>(1);
					}
					APt.Add(m2Puts);
				}
			}
			return APt;
		}

		public M2Chip findChip(ulong layer_bits, string metakey)
		{
			int count = this.count;
			for (int i = 0; i < count; i++)
			{
				M2Puts m2Puts = this.APuts[i];
				if ((layer_bits & (1UL << m2Puts.Lay.index)) != 0UL && m2Puts is M2Chip && m2Puts.getMeta().GetS(metakey) != null)
				{
					return m2Puts as M2Chip;
				}
			}
			return null;
		}

		public void clearBccSideCache()
		{
			if (this.ABcc != null && (this.bcc_gen_id_tb != 0 || this.bcc_gen_id_lr != 0))
			{
				X.ALLN<M2BlockColliderContainer.BCCLine>(this.ABcc);
				this.bcc_gen_id_tb = (this.bcc_gen_id_lr = 0);
			}
		}

		public void writeBccWall(M2BlockColliderContainer.BCCLine B)
		{
			if (this.bcc_line_cfg)
			{
				AIM foot_aim = B.foot_aim;
				if (this.ABcc == null)
				{
					this.ABcc = new M2BlockColliderContainer.BCCLine[4];
				}
				this.ABcc[(int)foot_aim] = B;
				if (B.is_naname)
				{
					this.ABcc[(int)CAim.get_aim_tetra(0, 0, B._xd, 0)] = B;
				}
				this.bcc_gen_id_tb = (this.bcc_gen_id_lr = -1);
			}
		}

		public void writeBcc(AIM aim, M2BlockColliderContainer.BCCLine B)
		{
			if (this.ABcc == null)
			{
				this.ABcc = new M2BlockColliderContainer.BCCLine[4];
			}
			this.ABcc[(int)aim] = B;
		}

		public M2BlockColliderContainer.BCCLine[] getBccArray(Map2d Mp, bool xd)
		{
			if (this.ABcc == null)
			{
				this.ABcc = new M2BlockColliderContainer.BCCLine[4];
			}
			int num = (xd ? this.bcc_gen_id_lr : this.bcc_gen_id_tb);
			if (num < 0)
			{
				return this.ABcc;
			}
			if (Mp.BCC.gen_id != num)
			{
				if (xd)
				{
					this.ABcc[0] = (this.ABcc[2] = null);
					this.bcc_gen_id_lr = Mp.BCC.gen_id;
				}
				else
				{
					this.ABcc[1] = (this.ABcc[3] = null);
					this.bcc_gen_id_tb = Mp.BCC.gen_id;
				}
			}
			return this.ABcc;
		}

		public M2BlockColliderContainer.BCCLine getSideBcc(Map2d Mp, int mapx, int mapy, AIM a)
		{
			int num = CAim._XD(a, 1);
			int num2 = -CAim._YD(a, 1);
			if (this.ABcc != null && Mp.BCC.gen_id == ((num != 0) ? this.bcc_gen_id_lr : this.bcc_gen_id_tb))
			{
				return this.ABcc[(int)a];
			}
			this.getBccArray(Mp, num != 0);
			int num3 = ((num != 0) ? Mp.clms : Mp.rows);
			int i = 0;
			AIM aim = CAim.get_opposite(a);
			bool bcc_line_cfg = this.bcc_line_cfg;
			if (bcc_line_cfg)
			{
				num = -num;
				num2 = -num2;
			}
			int num4 = mapx;
			int num5 = mapy;
			M2Pt m2Pt = this;
			M2BlockColliderContainer.BCCLine bccline = null;
			M2BlockColliderContainer.BCCLine bccline2 = null;
			while (i < num3)
			{
				m2Pt.getBccArray(Mp, num != 0);
				if ((bccline = m2Pt.ABcc[(int)a]) != null)
				{
					break;
				}
				num4 += num;
				num5 += num2;
				i++;
				m2Pt = Mp.getPointPuts(num4, num5, true, false);
				if (m2Pt == null)
				{
					if (bcc_line_cfg)
					{
						i--;
						num4 -= num;
						num5 -= num2;
						break;
					}
					break;
				}
			}
			m2Pt = this;
			while (i < num3)
			{
				m2Pt.getBccArray(Mp, num != 0);
				if ((bccline2 = m2Pt.ABcc[(int)aim]) != null)
				{
					break;
				}
				mapx -= num;
				mapy -= num2;
				i++;
				m2Pt = Mp.getPointPuts(mapx, mapy, true, false);
				if (m2Pt == null)
				{
					if (bcc_line_cfg)
					{
						i--;
						break;
					}
					break;
				}
			}
			if (i > 0)
			{
				if (!bcc_line_cfg)
				{
					i -= 2;
					num4 -= num;
					num5 -= num2;
				}
				while (i-- >= 0)
				{
					m2Pt = Mp.getPointPuts(num4, num5, true, false);
					if (m2Pt != null)
					{
						M2BlockColliderContainer.BCCLine[] bccArray = m2Pt.getBccArray(Mp, num != 0);
						bccArray[(int)aim] = bccline2;
						bccArray[(int)a] = bccline;
						if (num != 0)
						{
							m2Pt.bcc_gen_id_lr = Mp.BCC.gen_id;
						}
						else
						{
							m2Pt.bcc_gen_id_tb = Mp.BCC.gen_id;
						}
					}
					num4 -= num;
					num5 -= num2;
				}
			}
			return this.ABcc[(int)a];
		}

		public IM2Inputtable getSmartChipFamily(M2SmartChipImage Smi, M2MapLayer Lay)
		{
			for (int i = this.APuts.Count - 1; i >= 0; i--)
			{
				M2Chip m2Chip = this.APuts[i] as M2Chip;
				if (m2Chip != null && m2Chip.Lay == Lay)
				{
					if (m2Chip.pattern == Smi.chip_id)
					{
						return Smi;
					}
					if (!(Smi.family == ""))
					{
						if (m2Chip.pattern != 0U)
						{
							IM2CLItem byId = Smi.IMGS.GetById(m2Chip.pattern);
							if (byId != null)
							{
								if (byId is M2SmartChipImage && (byId as M2SmartChipImage).family == Smi.family)
								{
									return byId;
								}
								if (byId is M2ChipPattern && (byId as M2ChipPattern).family == Smi.family)
								{
									return byId;
								}
							}
						}
						if (Smi.family == m2Chip.Img.family)
						{
							return m2Chip;
						}
					}
				}
			}
			return null;
		}

		public void activateToChip(bool activating, ulong layer_bits = 18446744073709551615UL, string metakey = null)
		{
			int count = this.count;
			for (int i = 0; i < count; i++)
			{
				M2Puts m2Puts = this.APuts[i];
				if ((layer_bits & (1UL << m2Puts.Lay.index)) != 0UL && (metakey == null || m2Puts.getMeta().GetS(metakey) != null))
				{
					if (activating)
					{
						if (m2Puts is IActivatable)
						{
							(m2Puts as IActivatable).activate();
						}
						m2Puts.activateToDrawer();
					}
					else
					{
						if (m2Puts is IActivatable)
						{
							(m2Puts as IActivatable).deactivate();
						}
						m2Puts.deactivateToDrawer();
					}
				}
			}
		}

		public bool bcc_line_cfg
		{
			get
			{
				return !CCON.canStandAndNoBlockSlope(this.cfg);
			}
		}

		private const int C__CHIP_CFG = 4095;

		private const int C_DANGEROUS = 4096;

		public const int C_HAS_LIFT = 8192;

		private readonly List<M2Puts> APuts;

		private int cfg_;

		private int bcc_gen_id_lr = -1;

		private int bcc_gen_id_tb = -1;

		private M2BlockColliderContainer.BCCLine[] ABcc;
	}
}
