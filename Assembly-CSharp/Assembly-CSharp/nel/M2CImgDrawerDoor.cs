using System;
using System.Collections.Generic;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class M2CImgDrawerDoor : M2CImgDrawer, IRunAndDestroy, IActivatable
	{
		public M2CImgDrawerDoor(MeshDrawer Md, int lay, M2Puts _Cp, string meta_key = "door", int meta_key_i = 0)
			: base(Md, lay, _Cp, true)
		{
			METACImg meta = this.Cp.Img.Meta;
			this.auto_open = meta.GetI("door", 0, meta_key_i + 2) != 0;
			this.default_door_1 = meta.GetI("door", 0, meta_key_i + 1) != 0;
			this.Dr = new DoorDrawer();
			this.Dr.open_to_behind = meta.GetI("door", 0, meta_key_i + 3) != 0;
			this.Dr.do_not_draw_behind_handle = meta.GetI("door", 0, meta_key_i + 4) != 0;
			this.door_inner_light_alpha = meta.GetNm("door_inner_light_alpha", 0.5f, 0);
			this.AMvNear = new List<M2Mover>(2);
		}

		public override bool entryMainPicToMesh(MeshDrawer Md, float meshx, float meshy, float _zmx, float _zmy, float _rotR, PxlMeshDrawer Ms)
		{
			if (!this.Cp.Img.SourceAtlas.valid)
			{
				return base.entryMainPicToMesh(Md, meshx, meshy, _zmx, _zmy, _rotR, Ms);
			}
			string text = base.Meta.GetS("door_side");
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			float num5 = 0f;
			float num6 = (float)this.Cp.iwidth * 0.38f;
			float num7 = 0f;
			float num8 = -1f;
			this.Dr.TargetMI = base.M2D.MIchip;
			if (TX.valid(text))
			{
				PxlLayer layerByName = this.Cp.Img.SourceFrm.getLayerByName(text);
				if (layerByName == null)
				{
					X.de(string.Concat(new string[]
					{
						"ドアのサイドイメージ ",
						text,
						"が ",
						this.Cp.Img.SourceFrm.ToString(),
						" に見つかりません。"
					}), null);
				}
				else
				{
					this.Dr.DoorSideImg = base.M2D.IMGS.Atlas.getAtlasRect(layerByName.Img);
					num = (float)base.Meta.GetI("door_side", 4, 1);
				}
			}
			text = base.Meta.GetS("door_handle_rail");
			if (TX.valid(text))
			{
				PxlLayer layerByName2 = this.Cp.Img.SourceFrm.getLayerByName(text);
				if (layerByName2 == null)
				{
					X.de(string.Concat(new string[]
					{
						"ドアのハンドル連結イメージ ",
						text,
						"が ",
						this.Cp.Img.SourceFrm.ToString(),
						" に見つかりません。"
					}), null);
				}
				else
				{
					this.Dr.HandleRailImg = base.M2D.IMGS.Atlas.getAtlasRect(layerByName2.Img);
					num2 = (float)base.Meta.GetI("door_handle_rail", 4, 1);
					num3 = (float)base.Meta.GetI("door_handle_rail", 0, 2);
				}
			}
			text = base.Meta.GetS("door_handle");
			if (TX.valid(text))
			{
				PxlLayer layerByName3 = this.Cp.Img.SourceFrm.getLayerByName(text);
				if (layerByName3 == null)
				{
					X.de(string.Concat(new string[]
					{
						"ドアのハンドルイメージ ",
						text,
						"が ",
						this.Cp.Img.SourceFrm.ToString(),
						" に見つかりません。"
					}), null);
				}
				else
				{
					this.Dr.HandleImg = base.M2D.IMGS.Atlas.getAtlasRect(layerByName3.Img);
					num6 = (float)base.Meta.GetI("door_handle", (int)((float)this.Cp.iwidth * 0.38f), 1);
					num7 = (float)base.Meta.GetI("door_handle", 0, 2);
					num4 = (float)base.Meta.GetI("door_handle", 4, 3);
					num5 = (float)base.Meta.GetI("door_handle", 4, 4);
					num8 = (float)base.Meta.GetI("door_handle", -1, 5);
				}
			}
			this.Dr.FrontImg = this.Cp.Img.SourceAtlas.getRect();
			this.Dr.SetByPixel((float)this.Cp.iwidth, (float)this.Cp.iheight, num, num4, num2, num5, num3, num8).HandlePosPixel(num6, num7);
			text = base.Meta.GetS("door_color");
			if (TX.valid(text))
			{
				this.Dr.FrontCol = C32.d2c(4278190080U | X.NmUI(text, 8355711U, true, true));
			}
			else
			{
				this.Dr.FrontCol = Md.Col;
			}
			text = base.Meta.GetS("door_side_color");
			if (TX.valid(text))
			{
				this.Dr.DoorSideCol = C32.d2c(4278190080U | X.NmUI(text, 8355711U, true, true));
			}
			else
			{
				this.Dr.DoorSideCol = Md.Col;
			}
			text = base.Meta.GetS("door_handle_rail_color");
			if (TX.valid(text))
			{
				this.Dr.HandleRailCol = C32.d2c(4278190080U | X.NmUI(text, 8355711U, true, true));
			}
			else
			{
				this.Dr.HandleRailCol = Md.Col;
			}
			text = base.Meta.GetS("door_handle_color");
			if (TX.valid(text))
			{
				this.Dr.HandleCol = C32.d2c(4278190080U | X.NmUI(text, 8355711U, true, true));
			}
			else
			{
				this.Dr.HandleCol = Md.Col;
			}
			string[] array = base.Meta.Get("sound");
			this.door_snd_open = (this.door_snd_close = "");
			if (array != null)
			{
				if (array.Length == 1)
				{
					this.door_snd_open = (this.door_snd_close = array[0]);
				}
				else
				{
					if (array.Length >= 1)
					{
						this.door_snd_open = array[0];
					}
					if (array.Length >= 2)
					{
						this.door_snd_close = array[1];
					}
				}
			}
			bool flag = this.Dr.isTyoutsugaiLeft() != this.Cp.flip;
			this.draw_meshx = meshx + (float)(X.MPF(!flag) * this.Cp.iwidth) * 0.5f;
			this.draw_meshy = meshy;
			base.Set(false);
			this.drawDoor(Md);
			base.Set(false);
			this.GrdCov = null;
			if (base.Lay.GRD != null && this.door_inner_light_alpha > 0f)
			{
				int length = base.Lay.GRD.Length;
				for (int i = 0; i < length; i++)
				{
					M2GradationRect m2GradationRect = base.Lay.GRD.Get(i);
					if (m2GradationRect.order == M2GradationRect.GRDORDER.SKY && m2GradationRect.isCoveringXy((float)this.Cp.drawx, (float)this.Cp.drawy, (float)this.Cp.iwidth, (float)this.Cp.iheight, 0f, -1000f))
					{
						this.GrdCov = m2GradationRect;
						break;
					}
				}
			}
			return this.redraw_flag;
		}

		private float getDoorOpenLevel()
		{
			float num = ((this.t >= 0f) ? X.ZSIN(this.t, 25f) : X.ZSINV(25f + this.t, 25f));
			if (this.default_door_1)
			{
				num = 1f - num;
			}
			return num;
		}

		private void drawDoor(MeshDrawer Md)
		{
			Color32 col = Md.Col;
			Md.Col = MTRX.ColWhite;
			this.Dr.drawTo(Md, this.draw_meshx, this.draw_meshy, this.getDoorOpenLevel(), 1.01f, 1.01f, this.Cp.draw_rotR, this.Cp.flip);
			Md.Col = col;
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			if (normal_map)
			{
				this.t = -25f;
				M2CImgDrawer drawerL = this.Cp.getDrawerL();
				if (drawerL != null && drawerL.get_Md() == base.Mp.MyDrawerL)
				{
					drawerL.setAlpha1(0f, false);
				}
				if (this.auto_open && !base.Mp.canStandArea(this.Cp))
				{
					this.auto_open = false;
				}
				if (this.auto_open)
				{
					this.setRunner(true);
				}
			}
		}

		public void setRunner(bool flag = true)
		{
			if (!flag)
			{
				this.runner_setted = false;
				base.Mp.remRunnerObject(this);
				return;
			}
			if (this.runner_setted)
			{
				return;
			}
			this.runner_setted = true;
			base.Mp.addRunnerObject(this);
		}

		public override void closeAction(bool when_map_close)
		{
			base.closeAction(when_map_close);
			this.setRunner(false);
			if (this.LigInner != null)
			{
				base.Mp.remLight(this.LigInner);
				this.LigInner = null;
			}
		}

		public bool run(float fcnt)
		{
			if (this.auto_open || this.t >= 0f)
			{
				bool flag = false;
				float num = this.xmargin_map + 0.25f;
				if (this.check_near_mover > 0)
				{
					this.AMvNear.Clear();
					for (int i = base.Mp.count_movers - 1; i >= 0; i--)
					{
						M2Mover mv = base.Mp.getMv(i);
						if (mv.getPhysic() != null && mv.isCovering(this.Cp.mleft - num, this.Cp.mright + num, this.Cp.mtop - 0.25f, this.Cp.mbottom + 0.125f, 0f))
						{
							this.AMvNear.Add(mv);
						}
					}
					if (this.AMvNear.Count > 0)
					{
						this.check_near_mover = 0;
					}
				}
				int num2 = this.AMvNear.Count;
				if (num2 == 0)
				{
					if (this.auto_open)
					{
						num2 = base.Mp.count_players;
					}
					else
					{
						flag = true;
					}
				}
				num = this.xmargin_map;
				for (int j = 0; j < num2; j++)
				{
					M2Mover m2Mover = ((this.AMvNear.Count > 0) ? this.AMvNear[j] : base.Mp.getPr(j));
					if (!m2Mover.destructed && m2Mover.getPhysic() != null && m2Mover.isCovering(this.Cp.mleft - num, this.Cp.mright + num, this.Cp.mtop - 0.25f, this.Cp.mbottom + 0.125f, 0f))
					{
						flag = true;
						if (this.t < 0f)
						{
							this.activate();
							break;
						}
					}
				}
				if (this.t >= 0f)
				{
					if (!flag)
					{
						this.deactivate();
					}
					else if (this.t < 25f)
					{
						this.need_reposit_flag = true;
						this.t = X.Mn(this.t + fcnt, 25f);
					}
				}
			}
			if (this.t < 0f && this.t > -25f)
			{
				this.t -= fcnt;
				this.need_reposit_flag = true;
				if (this.t <= -25f)
				{
					this.t = -25f;
					if (!this.auto_open)
					{
						this.runner_setted = false;
						return false;
					}
				}
			}
			return true;
		}

		public override int redraw(float fcnt)
		{
			if (this.need_reposit_flag)
			{
				float doorOpenLevel = this.getDoorOpenLevel();
				this.Cp.light_alpha = 1f - doorOpenLevel;
				int num = 0;
				int num2 = 0;
				base.revertVerAndTriIndex(ref num, ref num2);
				this.drawDoor(this.Md);
				base.revertVerAndTriIndexAfter(num, num2, false);
				M2CImgDrawer drawerL = this.Cp.getDrawerL();
				if (drawerL != null && drawerL.get_Md() == base.Mp.MyDrawerL)
				{
					drawerL.setAlpha1(doorOpenLevel, false);
				}
				this.need_reposit_flag = false;
				if (doorOpenLevel > 0f && this.GrdCov != null)
				{
					if (this.LigInner == null)
					{
						this.LigInner = new M2LightFn(base.Mp, new M2LightFn.FnDrawLight(this.fnDrawInnerLight), null, (float _vx, float _vy) => this.Cp.isinCamera(80f));
						base.Mp.addLight(this.LigInner);
					}
					this.LigInner.Pos(this.Cp.mapcx, this.Cp.mapcy);
				}
				if (this.LigInner != null)
				{
					this.LigInner.alpha_rgb = doorOpenLevel * this.door_inner_light_alpha;
				}
				return base.layer2update_flag;
			}
			return base.redraw(fcnt);
		}

		public void fnDrawInnerLight(MeshDrawer Md, M2LightFn Lt, float x, float y, float scale, float alpha)
		{
			if (alpha <= 0f || this.LigInner.alpha_rgb <= 0f)
			{
				return;
			}
			Md.ColGrd.Set(uint.MaxValue).mulA(alpha);
			Md.ColGrd.multiply(this.LigInner.alpha_rgb, false);
			Md.Col = Md.ColGrd.C;
			Md.ColGrd.mulA(0f);
			Md.Identity();
			float num = (float)(this.Cp.iwidth + 40) * 0.015625f;
			float num2 = (float)(this.Cp.iheight + 40) * 0.015625f;
			x *= 0.015625f;
			y *= 0.015625f;
			Md.initForImg(MTRX.IconWhite, 0);
			Md.uvRect(x - num * 0.5f, y - num2 * 0.5f, num, num2, Md.uv_left, Md.uv_top, Md.uv_width, Md.uv_height, true, false);
			Md.RectDoughnut(x, y, num, num2, x, y, (float)this.Cp.iwidth * 0.015625f, (float)this.Cp.iheight * 0.015625f, true, 1f, 0f, true);
		}

		public void activate()
		{
			if (this.t < 0f || base.Mp.floort < 3f)
			{
				this.t = X.Mx(25f + this.t, 0f);
				if (base.Mp.floort < 3f)
				{
					this.t = 24f;
					this.need_reposit_flag = true;
				}
				this.setRunner(true);
				if (TX.valid(this.door_snd_open))
				{
					base.Mp.M2D.Snd.playAt(this.door_snd_open, "", this.Cp.mapcx, this.Cp.mapcy, SndPlayer.SNDTYPE.SND, 1);
				}
				this.AMvNear.Clear();
				this.check_near_mover = 3;
			}
		}

		public float xmargin_map
		{
			get
			{
				if (this.t < 0f)
				{
					return 0.25f;
				}
				return 1.25f;
			}
		}

		public void deactivate()
		{
			if (this.t >= 0f)
			{
				this.t = X.Mn(-24f + this.t, -1f);
				this.setRunner(true);
				if (TX.valid(this.door_snd_close))
				{
					base.Mp.M2D.Snd.playAt(this.door_snd_close, "", this.Cp.mapcx, this.Cp.mapcy, SndPlayer.SNDTYPE.SND, 1);
				}
				this.AMvNear.Clear();
				this.check_near_mover = 0;
			}
		}

		public string getActivateKey()
		{
			return base.unique_key;
		}

		public void destruct()
		{
		}

		public bool auto_open;

		public const float maxt = 25f;

		public float t = -25f;

		public bool default_door_1;

		private bool runner_setted;

		public float draw_meshx;

		public float draw_meshy;

		public string door_snd_open;

		public string door_snd_close;

		public int check_near_mover;

		protected DoorDrawer Dr;

		protected M2LightFn LigInner;

		protected M2GradationRect GrdCov;

		public float door_inner_light_alpha;

		private readonly List<M2Mover> AMvNear;
	}
}
