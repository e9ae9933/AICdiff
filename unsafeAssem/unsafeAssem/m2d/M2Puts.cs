using System;
using System.Collections.Generic;
using PixelLiner;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2Puts : M2DrawItem, IM2Inputtable
	{
		public M2Puts(M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, int index, M2ChipImage _Img)
			: base(_Lay.Mp)
		{
			this.Lay = _Lay;
			this.index = index;
			this.drawx = X.IntU((float)drawx);
			this.drawy = X.IntU((float)drawy);
			this.x = (float)(this.mapx = X.IntU((float)drawx / base.CLEN));
			this.y = (float)(this.mapy = X.IntU((float)drawy / base.CLEN));
			this.opacity = X.MMX(0, opacity, 255);
			this.rotation = rotation;
			this.flip = flip;
			this.Img = _Img;
			this.iwidth = this.Img.iwidth;
			this.iheight = this.Img.iheight;
		}

		public string src
		{
			get
			{
				if (this.Img == null)
				{
					return "";
				}
				return this.Img.src;
			}
		}

		public M2ImageContainer IMGS
		{
			get
			{
				return this.Mp.IMGS;
			}
		}

		public virtual M2Puts inputXy()
		{
			this.x = (float)(this.mapx = X.IntU((float)this.drawx * base.rCLEN));
			this.y = (float)(this.mapy = X.IntU((float)this.drawy * base.rCLEN));
			return this;
		}

		public virtual Vector2Int getShift()
		{
			return Vector2Int.zero;
		}

		public override bool isOnCamera(float cam_left, float cam_top, float camw, float camh)
		{
			return X.isCovering(cam_left, cam_left + camw, (float)this.drawx, (float)(this.drawx + this.rwidth), 0f) && X.isCovering(cam_top, cam_top + camh, (float)this.drawy, (float)(this.drawy + this.rheight), 0f);
		}

		public virtual int getConfig(int _x, int _y)
		{
			return 4;
		}

		public bool isinCamera(float margin_px = 0f)
		{
			if (this.Mp.SubMapData != null)
			{
				return true;
			}
			M2Camera cam = this.Mp.M2D.Cam;
			return this.isOnCamera(cam.x - cam.get_w() * 0.5f - margin_px, cam.y - cam.get_h() * 0.5f - margin_px, cam.get_w() + margin_px, cam.get_h() + margin_px);
		}

		public void releaseArranger()
		{
			this.closeAction(false, false);
		}

		public bool isActiveT()
		{
			return this.Img.MeshT != null;
		}

		public bool isActiveG()
		{
			return this.Img.MeshG != null;
		}

		public int isActiveB()
		{
			if (this.Img.MeshB == null)
			{
				return 0;
			}
			return 1;
		}

		public M2Puts setColAll(Color32 Col)
		{
			if (this.DaB != null)
			{
				this.DaB.setColAll(Col, false);
			}
			if (this.DaG != null)
			{
				this.DaG.setColAll(Col, false);
			}
			if (this.DaT != null)
			{
				this.DaT.setColAll(Col, false);
			}
			if (this.DaL != null)
			{
				this.DaL.setColAll(Col, false);
			}
			return this;
		}

		public M2Puts setAlpha1(float a)
		{
			if (this.DaB != null)
			{
				this.DaB.setAlpha1(a, false);
			}
			if (this.DaG != null)
			{
				this.DaG.setAlpha1(a, false);
			}
			if (this.DaT != null)
			{
				this.DaT.setAlpha1(a, false);
			}
			if (this.DaL != null)
			{
				this.DaL.setAlpha1(a, false);
			}
			return this;
		}

		public override float mleft
		{
			get
			{
				return (float)this.drawx * base.rCLEN;
			}
		}

		public override float mright
		{
			get
			{
				return (float)(this.drawx + this.iwidth) * base.rCLEN;
			}
		}

		public override float mtop
		{
			get
			{
				return (float)this.drawy * base.rCLEN;
			}
		}

		public override float mbottom
		{
			get
			{
				return (float)(this.drawy + this.iheight) * base.rCLEN;
			}
		}

		public M2Puts drawLocTo(int dx, int dy)
		{
			this.drawx = dx;
			this.drawy = dy;
			this.inputXy();
			return this;
		}

		public virtual float mapcx
		{
			get
			{
				return ((float)this.drawx + (float)this.iwidth * 0.5f) * base.rCLEN;
			}
		}

		public virtual float mapcy
		{
			get
			{
				return ((float)this.drawy + (float)this.iheight * 0.5f) * base.rCLEN;
			}
		}

		public static int fnSortByIndex(M2Puts a, M2Puts b)
		{
			return a.index - b.index;
		}

		public bool inputChip()
		{
			return false;
		}

		public bool removeChip()
		{
			this.Mp.removeChip(this, false, false);
			return true;
		}

		public virtual List<M2Chip> MakeChip(M2MapLayer Lay, int x, int y, int opacity, int rotation, bool flip)
		{
			return null;
		}

		public virtual List<M2Picture> MakePicture(M2MapLayer Lay, float x, float y, int opacity, int rotation, bool flip)
		{
			return null;
		}

		public bool isSame(M2Puts Cp, bool in_editor = false, int editor_curs_x = -1, int editor_curs_y = -1)
		{
			if (this.pattern == 0U)
			{
				return Cp.Img == this.Img;
			}
			return Cp.pattern == this.pattern;
		}

		public M2ChipImage getFirstImage()
		{
			return this.Img;
		}

		public virtual Vector2Int getClmsAndRows(INPUT_CR cr)
		{
			if (this.Img == null)
			{
				return new Vector2Int(1, 1);
			}
			return this.Img.getClmsAndRows(cr);
		}

		public string getTitle()
		{
			return this.src;
		}

		public uint getChipId()
		{
			if (this.pattern != 0U)
			{
				return this.pattern;
			}
			if (this.Img == null)
			{
				return 0U;
			}
			return this.Img.getChipId();
		}

		public override string getDebugString(bool get_pre = true, bool get_post = true)
		{
			string text = "";
			if (get_pre)
			{
				text = string.Concat(new string[]
				{
					text,
					":: ",
					this.src,
					" [",
					this.mapx.ToString(),
					", ",
					this.mapy.ToString(),
					"] --\nindex: ",
					this.index.ToString(),
					"\nrwidth: ",
					this.rwidth.ToString(),
					"\nrheight: ",
					this.rheight.ToString(),
					"\nopacity: ",
					this.opacity.ToString(),
					"\n"
				});
			}
			return text;
		}

		public void clearDrawer(bool close_action = false)
		{
			if (close_action)
			{
				if (this.DaB != null)
				{
					this.DaB.closeAction(false);
				}
				if (this.DaG != null)
				{
					this.DaG.closeAction(false);
				}
				if (this.DaT != null)
				{
					this.DaT.closeAction(false);
				}
				if (this.DaL != null)
				{
					this.DaL.closeAction(false);
				}
			}
			this.DaB = (this.DaG = (this.DaT = (this.DaL = null)));
		}

		public virtual int entryChipMesh(MeshDrawer MdB, MeshDrawer MdG, MeshDrawer MdT, MeshDrawer MdL, MeshDrawer MdTT, float sx, float sy, float _zm, float _rotR = 0f)
		{
			bool invisible = this.Img.Meta.invisible;
			bool flag = true;
			if (this.Img.has_remover_mesh)
			{
				flag = !this.Img.Meta.is_window || this.Lay.getLayerMeta().useWindowRemover(this.Mp);
			}
			if (invisible && !Map2d.editor_decline_lighting && !flag)
			{
				return 0;
			}
			if (this.Img.Meta.merge_to_one_layer)
			{
				if (this.Img.Meta.draw_lit_layer)
				{
					MdG = (MdB = (MdT = MdL));
				}
				else if (this.Img.Meta.draw_overtop_layer)
				{
					MdT = MdTT;
					MdG = MdTT;
					MdB = MdTT;
				}
				else if (this.Img.mesh_type == 3)
				{
					MdG = (MdB = MdT);
				}
				else if (this.Img.mesh_type == 2)
				{
					MdT = (MdB = MdG);
				}
				else
				{
					MdT = (MdG = MdB);
				}
			}
			MeshDrawer meshDrawer = null;
			PxlMeshDrawer pxlMeshDrawer = null;
			if (this.Mp.SubMapData != null && !invisible)
			{
				Dungeon dgn = this.Mp.SubMapData.getBaseMap().Dgn;
				if (dgn != null)
				{
					meshDrawer = dgn.checkBlurDrawing(this, this.Mp.SubMapData, ref pxlMeshDrawer);
				}
			}
			int num = 0;
			if (meshDrawer != null)
			{
				num = (M2Puts.entryMainPicToMesh(this, ref num, meshDrawer, sx, sy, _zm, _zm, _rotR, this.flip, 100, pxlMeshDrawer, ref this.DaT) ? 256 : 0) | 4096;
			}
			else
			{
				if (this.Img.Meta.draw_lit_layer)
				{
					MdG = (MdB = (MdT = (MdL = this.Mp.MyDrawerL)));
				}
				if (!this.Img.Meta.merge_to_one_layer && this.Img.Meta.draw_overtop_layer)
				{
					if (this.Img.mesh_type == 3)
					{
						MdT = MdTT;
					}
					else if (this.Img.mesh_type == 2)
					{
						MdG = MdTT;
					}
					else
					{
						MdB = MdTT;
					}
				}
				int num2 = 0;
				int num3 = 1;
				if (flag && Map2d.editor_decline_lighting)
				{
					num2 = 1;
					num3 = -1;
				}
				for (;;)
				{
					if (num2 == 0)
					{
						if (!invisible)
						{
							for (int i = 0; i < 5; i++)
							{
								pxlMeshDrawer = this.getSrcMeshForEntry(i);
								if (pxlMeshDrawer != null)
								{
									if (i == 0)
									{
										M2Puts.entryMainPicToMesh(this, ref num, MdB, sx, sy, _zm, _zm, _rotR, this.flip, i, pxlMeshDrawer, ref this.DaB);
									}
									if (i == 1)
									{
										M2Puts.entryMainPicToMesh(this, ref num, MdG, sx, sy, _zm, _zm, _rotR, this.flip, i, pxlMeshDrawer, ref this.DaG);
									}
									if (i == 2)
									{
										M2Puts.entryMainPicToMesh(this, ref num, MdT, sx, sy, _zm, _zm, _rotR, this.flip, i, pxlMeshDrawer, ref this.DaT);
									}
									if (i == 3)
									{
										M2Puts.entryMainPicToMesh(this, ref num, MdL, sx, sy, _zm, _zm, _rotR, this.flip, i, pxlMeshDrawer, ref this.DaL);
									}
									if (i == 4)
									{
										M2Puts.entryMainPicToMesh(this, ref num, MdTT, sx, sy, _zm, _zm, _rotR, this.flip, i, pxlMeshDrawer, ref this.DaL);
									}
								}
							}
						}
					}
					else if (flag)
					{
						for (int j = 0; j < 5; j++)
						{
							pxlMeshDrawer = this.Img.getSrcMesh(j | 32);
							if (pxlMeshDrawer != null)
							{
								MeshDrawer meshDrawer2;
								switch (j)
								{
								case 0:
									meshDrawer2 = MdB;
									break;
								case 1:
									meshDrawer2 = MdG;
									break;
								case 2:
									meshDrawer2 = MdT;
									break;
								case 3:
									meshDrawer2 = MdL;
									break;
								default:
									meshDrawer2 = MdTT;
									break;
								}
								MdMap mdMap = meshDrawer2 as MdMap;
								if (mdMap != null)
								{
									mdMap.SimplifyRemoveMesh(this, sx, sy, _zm, _rotR, pxlMeshDrawer);
								}
							}
						}
					}
					if (num3 > 0)
					{
						num2 += num3;
						if (num2 >= 2)
						{
							break;
						}
						if (!flag)
						{
							break;
						}
					}
					else if (--num2 < 0)
					{
						break;
					}
				}
				num |= this.get_update_flag();
			}
			if (this.active_closed)
			{
				this.initActiveRemoveKey();
			}
			return num;
		}

		protected virtual PxlMeshDrawer getSrcMeshForEntry(int _layer)
		{
			return this.Img.getSrcMesh(_layer);
		}

		private static bool entryMainPicToMesh(M2Puts Cp, ref int flags, MeshDrawer Md, float meshx, float meshy, float _zmx, float _zmy, float _rotation, bool flip, int _layer, PxlMeshDrawer Src, ref M2CImgDrawer Da)
		{
			if (Src == null)
			{
				return false;
			}
			if (Da == null)
			{
				if (Map2d.editor_decline_lighting)
				{
					if (Cp.Img.Meta.GetB("whole_chip", false))
					{
						Da = new M2CImgDrawerWhole(Md, _layer, Cp, Cp.Img.Meta);
					}
					else
					{
						Da = Cp.Img.CreateOneDrawer(ref Md, _layer, Cp, Cp.Img.Meta, Da, false);
					}
				}
				else
				{
					Da = Cp.CreateDrawer(ref Md, _layer, Cp.Img.Meta, null);
				}
				if (Da == null)
				{
					return false;
				}
			}
			if (Da.redraw_flag)
			{
				flags |= 256;
			}
			if (Md is MdMap)
			{
				MdMap mdMap = Md as MdMap;
				Md = mdMap.checkArrangeableMeshEvacuation(Cp, Da);
				if (Md.activation_key != "_simplify" && mdMap.allow_simplify && Cp is M2Chip)
				{
					_zmx *= 1.01f;
					_zmy *= 1.01f;
				}
			}
			Da.clear(Md);
			return Da.entryMainPicToMesh(Md, meshx, meshy, _zmx * (float)(flip ? (-1) : 1), _zmy, _rotation, Src);
		}

		protected virtual M2CImgDrawer CreateDrawer(ref MeshDrawer Md, int lay, METACImg Meta, M2CImgDrawer Pre_Drawer)
		{
			return this.Img.CreateOneDrawer(ref Md, lay, this, Meta, Pre_Drawer, false);
		}

		public virtual void initAction(bool normal_map)
		{
			if (this.DaB != null)
			{
				this.DaB.initAction(normal_map);
			}
			if (this.DaG != null)
			{
				this.DaG.initAction(normal_map);
			}
			if (this.DaT != null)
			{
				this.DaT.initAction(normal_map);
			}
			if (this.DaL != null)
			{
				this.DaL.initAction(normal_map);
			}
			if (this.Mp.apply_chip_effect && !this.Lay.no_chiplight)
			{
				if (this.AChipLight != null)
				{
					this.closeLight();
				}
				this.AChipLight = M2ChipLight.getFromMeta(this.Img, this.Mp);
				if (this.AChipLight != null)
				{
					for (int i = this.AChipLight.Count - 1; i >= 0; i--)
					{
						this.Mp.addLight(this.AChipLight[i].initPuts(this));
					}
				}
			}
		}

		public void setMatrixToChipLight(Matrix4x4 Mx)
		{
			if (this.AChipLight != null)
			{
				for (int i = this.AChipLight.Count - 1; i >= 0; i--)
				{
					this.AChipLight[i].setMatrix(Mx);
				}
			}
		}

		public void closeLight()
		{
			if (this.AChipLight != null)
			{
				for (int i = this.AChipLight.Count - 1; i >= 0; i--)
				{
					this.Mp.remLight(this.AChipLight[i]);
				}
				this.AChipLight = null;
			}
		}

		public virtual void closeAction(bool when_map_close, bool do_not_remove_drawer)
		{
			if (this.DaB != null)
			{
				this.DaB.closeAction(when_map_close);
			}
			if (this.DaG != null)
			{
				this.DaG.closeAction(when_map_close);
			}
			if (this.DaT != null)
			{
				this.DaT.closeAction(when_map_close);
			}
			if (this.DaL != null)
			{
				this.DaL.closeAction(when_map_close);
			}
			this.closeLight();
			if (!do_not_remove_drawer)
			{
				this.DaB = null;
				this.DaG = null;
				this.DaT = null;
				this.DaL = null;
			}
		}

		public virtual void activateToDrawer()
		{
			if (this.DaB != null && this.DaB is IActivatable)
			{
				(this.DaB as IActivatable).activate();
			}
			if (this.DaG != null && this.DaG is IActivatable)
			{
				(this.DaG as IActivatable).activate();
			}
			if (this.DaT != null && this.DaT is IActivatable)
			{
				(this.DaT as IActivatable).activate();
			}
			if (this.DaL != null && this.DaL is IActivatable)
			{
				(this.DaL as IActivatable).activate();
			}
		}

		public virtual void deactivateToDrawer()
		{
			if (this.DaB != null && this.DaB is IActivatable)
			{
				(this.DaB as IActivatable).deactivate();
			}
			if (this.DaG != null && this.DaG is IActivatable)
			{
				(this.DaG as IActivatable).deactivate();
			}
			if (this.DaT != null && this.DaT is IActivatable)
			{
				(this.DaT as IActivatable).deactivate();
			}
			if (this.DaL != null && this.DaL is IActivatable)
			{
				(this.DaL as IActivatable).deactivate();
			}
		}

		public int dissolveAlpha(byte a, bool showing)
		{
			int num = 0;
			if (this.DaB != null)
			{
				num |= this.DaB.dissolveAlpha(a, showing);
			}
			if (this.DaG != null)
			{
				num |= this.DaG.dissolveAlpha(a, showing);
			}
			if (this.DaT != null)
			{
				num |= this.DaT.dissolveAlpha(a, showing);
			}
			if (this.DaL != null)
			{
				num |= this.DaL.dissolveAlpha(a, showing);
			}
			if (num != 0)
			{
				this.Mp.addUpdateMesh(num, false);
			}
			return num;
		}

		public int DrawerTranslate(float pixelx, float pixely)
		{
			int num = 0;
			if (this.DaB != null)
			{
				num |= this.DaB.translateBasePosition(pixelx, pixely);
			}
			if (this.DaG != null)
			{
				num |= this.DaG.translateBasePosition(pixelx, pixely);
			}
			if (this.DaT != null)
			{
				num |= this.DaT.translateBasePosition(pixelx, pixely);
			}
			if (num != 0)
			{
				this.Mp.addUpdateMesh(num, false);
			}
			return num;
		}

		public int DrawerSetColor(Color32 C)
		{
			int num = 0;
			if (this.DaB != null)
			{
				num |= this.DaB.setColor(C);
			}
			if (this.DaG != null)
			{
				num |= this.DaG.setColor(C);
			}
			if (this.DaT != null)
			{
				num |= this.DaT.setColor(C);
			}
			if (num != 0)
			{
				this.Mp.addUpdateMesh(num, false);
			}
			return num;
		}

		public int DrawerSetAlpha(byte a)
		{
			int num = 0;
			if (this.DaB != null)
			{
				num |= this.DaB.setAlpha(a);
			}
			if (this.DaG != null)
			{
				num |= this.DaG.setAlpha(a);
			}
			if (this.DaT != null)
			{
				num |= this.DaT.setAlpha(a);
			}
			if (num != 0)
			{
				this.Mp.addUpdateMesh(num, false);
			}
			return num;
		}

		public bool temporaryDrawerReplaceImage(M2ImageAtlas.AtlasRect DestImage)
		{
			bool flag = false;
			if (this.DaB != null)
			{
				flag = this.DaB.temporaryReplaceImage(DestImage, true) || flag;
			}
			if (this.DaG != null)
			{
				flag = this.DaG.temporaryReplaceImage(DestImage, true) || flag;
			}
			if (this.DaT != null)
			{
				flag = this.DaT.temporaryReplaceImage(DestImage, true) || flag;
			}
			if (this.DaL != null)
			{
				flag = this.DaL.temporaryReplaceImage(DestImage, true) || flag;
			}
			return flag;
		}

		public bool reduceDrawnAlpha(int val = 5)
		{
			bool flag = true;
			if (this.DaB != null)
			{
				flag = this.DaB.reduceDrawnAlpha(val) && flag;
			}
			if (this.DaG != null)
			{
				flag = this.DaG.reduceDrawnAlpha(val) && flag;
			}
			if (this.DaT != null)
			{
				flag = this.DaT.reduceDrawnAlpha(val) && flag;
			}
			if (this.DaL != null)
			{
				flag = this.DaL.reduceDrawnAlpha(val) && flag;
			}
			return flag;
		}

		public int redraw(float fcnt)
		{
			int num = 0;
			if (this.DaB != null)
			{
				Bench.P(this.DaB.ToString());
				num |= this.DaB.redraw(fcnt);
				Bench.Pend(this.DaB.ToString());
			}
			if (this.DaG != null)
			{
				Bench.P(this.DaG.ToString());
				num |= this.DaG.redraw(fcnt);
				Bench.Pend(this.DaG.ToString());
			}
			if (this.DaT != null)
			{
				Bench.P(this.DaT.ToString());
				num |= this.DaT.redraw(fcnt);
				Bench.Pend(this.DaT.ToString());
			}
			if (this.DaL != null)
			{
				Bench.P(this.DaL.ToString());
				num |= this.DaL.redraw(fcnt);
				Bench.Pend(this.DaL.ToString());
			}
			return num;
		}

		public bool hasRedrawableChipFor(MdMap MdM)
		{
			bool flag = false;
			if (this.DaB != null)
			{
				flag = this.DaB.hasRedrawableChipFor(MdM) || flag;
			}
			if (this.DaG != null)
			{
				flag = this.DaG.hasRedrawableChipFor(MdM) || flag;
			}
			if (this.DaT != null)
			{
				flag = this.DaT.hasRedrawableChipFor(MdM) || flag;
			}
			if (this.DaL != null)
			{
				flag = this.DaL.hasRedrawableChipFor(MdM) || flag;
			}
			return flag;
		}

		public void unshiftSpecificMeshDrawer(MeshDrawer Md, int ver, int tri)
		{
			if (this.DaB != null)
			{
				this.DaB.unshiftSpecificMeshDrawer(Md, ver, tri);
			}
			if (this.DaG != null)
			{
				this.DaG.unshiftSpecificMeshDrawer(Md, ver, tri);
			}
			if (this.DaT != null)
			{
				this.DaT.unshiftSpecificMeshDrawer(Md, ver, tri);
			}
			if (this.DaL != null)
			{
				this.DaL.unshiftSpecificMeshDrawer(Md, ver, tri);
			}
		}

		public int get_update_flag()
		{
			int num = 0;
			if (this.DaB != null)
			{
				num |= this.DaB.layer2update_flag;
			}
			if (this.DaG != null)
			{
				num |= this.DaG.layer2update_flag;
			}
			if (this.DaT != null)
			{
				num |= this.DaT.layer2update_flag;
			}
			if (this.DaL != null)
			{
				num |= this.DaL.layer2update_flag;
			}
			return num;
		}

		public void addActiveRemoveKey(string key, bool visible = false)
		{
			if (!this.arrangeable)
			{
				X.de("arrangeable フラグのついていないチップ " + this.ToString() + " を activeRemove できない", null);
				return;
			}
			bool flag = this.active_removed_keys == null || this.visible_when_removed;
			if (!visible)
			{
				this.visible_when_removed = false;
			}
			if (this.active_removed_keys == null)
			{
				this.active_removed_keys = "\n" + key + "\n";
			}
			else if (this.active_removed_keys.IndexOf("\n" + key + "\n") == -1)
			{
				this.active_removed_keys = this.active_removed_keys + key + "\n";
			}
			if (this.visible_when_removed != flag && this.AttachCM == null)
			{
				this.initActiveRemoveKey();
			}
		}

		public bool hasActiveRemoveKey(string key)
		{
			return this.active_removed_keys != null && this.active_removed_keys.IndexOf("\n" + key + "\n") != -1;
		}

		protected virtual void initActiveRemoveKey()
		{
			int num = 0;
			if (this.DaB != null)
			{
				num |= this.DaB.repositActiveRemoveFlag();
			}
			if (this.DaG != null)
			{
				num |= this.DaG.repositActiveRemoveFlag();
			}
			if (this.DaT != null)
			{
				num |= this.DaT.repositActiveRemoveFlag();
			}
			if (this.DaL != null)
			{
				num |= this.DaL.repositActiveRemoveFlag();
			}
			if (this.DaB != null)
			{
				this.DaB.closeAction(true);
			}
			if (this.DaG != null)
			{
				this.DaG.closeAction(true);
			}
			if (this.DaT != null)
			{
				this.DaT.closeAction(true);
			}
			if (this.DaL != null)
			{
				this.DaL.closeAction(true);
			}
			this.closeLight();
			this.Mp.addUpdateMesh(num, false);
		}

		public virtual void translateByChipMover(float ux, float uy, C32 AddCol, int drawx0, int drawy0, int move_drawx = 0, int move_drawy = 0, bool stabilize_move_map = false)
		{
			if (this.DaB != null)
			{
				this.DaB.translateByChipMover(ux, uy, AddCol);
			}
			if (this.DaG != null)
			{
				this.DaG.translateByChipMover(ux, uy, AddCol);
			}
			if (this.DaT != null)
			{
				this.DaT.translateByChipMover(ux, uy, AddCol);
			}
			if (this.DaL != null)
			{
				this.DaL.translateByChipMover(ux, uy, AddCol);
			}
			if (stabilize_move_map)
			{
				int num = this.mapx;
				int num2 = this.mapy;
				this.drawx = drawx0;
				this.drawy = drawy0;
				this.inputXy();
				int num3 = this.mapx;
				int num4 = this.mapy;
				this.drawx += (int)((float)(num - num3) * base.CLEN);
				this.drawy += (int)((float)(num2 - num4) * base.CLEN);
				this.mapx = num;
				this.mapy = num2;
				if (num != num3 + move_drawx || num2 != num4 + move_drawy)
				{
					bool flag = this.Lay.connectImgLink(this, Map2d.CONNECTIMG.DELETE_TEMP, true);
					this.drawx = drawx0 + (int)((float)move_drawx * base.CLEN);
					this.drawy = drawy0 + (int)((float)move_drawy * base.CLEN);
					this.mapx = num3 + move_drawx;
					this.mapy = num4 + move_drawy;
					if (flag)
					{
						this.Lay.connectImgLink(this, Map2d.CONNECTIMG.ASSIGN, true);
						return;
					}
				}
			}
			else
			{
				this.drawx = drawx0 + move_drawx;
				this.drawy = drawy0 + move_drawy;
			}
		}

		public void remActiveRemoveKey(string key, bool do_not_initaction = false)
		{
			if (this.active_removed_keys == null)
			{
				return;
			}
			int num = this.active_removed_keys.IndexOf("\n" + key + "\n");
			if (num != -1)
			{
				this.active_removed_keys = TX.slice(this.active_removed_keys, 0, num) + TX.slice(this.active_removed_keys, num + key.Length + 1);
				if (this.active_removed_keys == "" || this.active_removed_keys == "\n")
				{
					this.active_removed_keys = null;
					if (!this.visible_when_removed)
					{
						this.visible_when_removed = true;
						int num2 = 0;
						if (this.DaB != null)
						{
							num2 |= this.DaB.repositActiveRemoveFlag();
						}
						if (this.DaG != null)
						{
							num2 |= this.DaG.repositActiveRemoveFlag();
						}
						if (this.DaT != null)
						{
							num2 |= this.DaT.repositActiveRemoveFlag();
						}
						if (this.DaL != null)
						{
							num2 |= this.DaL.repositActiveRemoveFlag();
						}
						this.Mp.addUpdateMesh(num2, false);
						if (!do_not_initaction && this.Mp.apply_chip_effect && this.AttachCM == null)
						{
							this.initAction(false);
						}
					}
				}
			}
		}

		public Vector2 PixelToMapPoint(float _x, float _y)
		{
			_x -= (float)this.Img.iwidth * 0.5f;
			_y -= (float)this.Img.iheight * 0.5f;
			Vector2 vector = new Vector2(this.flip ? (-_x) : _x, _y);
			vector = X.ROTV2e(vector, -this.draw_rotR);
			vector.x = (vector.x + (float)this.iwidth * 0.5f + (float)this.drawx) * base.rCLEN;
			vector.y = (vector.y + (float)this.iheight * 0.5f + (float)this.drawy) * base.rCLEN;
			return vector;
		}

		public virtual float draw_rotR
		{
			get
			{
				return (float)(-(float)this.rotation) * 1.5707964f;
			}
		}

		public bool arrangeable
		{
			get
			{
				return this.arrangeable_ || this.Lay.is_chip_arrangeable;
			}
			set
			{
				bool arrangeable = this.arrangeable;
				this.arrangeable_ = value;
				if (!arrangeable && this.arrangeable_)
				{
					int num = 0;
					if (this.DaB != null)
					{
						num = this.DaB.reentryToArrangeableMesh() | num;
					}
					if (this.DaG != null)
					{
						num = this.DaG.reentryToArrangeableMesh() | num;
					}
					if (this.DaT != null)
					{
						num = this.DaT.reentryToArrangeableMesh() | num;
					}
					if (this.DaL != null)
					{
						num = this.DaL.reentryToArrangeableMesh() | num;
					}
					if (num != 0)
					{
						this.Mp.addUpdateMesh(num, false);
					}
				}
			}
		}

		public bool isLinked()
		{
			return false;
		}

		public bool isFavorited()
		{
			return false;
		}

		public string getDirName()
		{
			return this.Img.dirname;
		}

		public METACImg getMeta()
		{
			return this.Img.Meta;
		}

		public M2CImgDrawer getDrawerL()
		{
			return this.DaL;
		}

		public M2CImgDrawer getAnyDrawer(uint layer_bits)
		{
			if (this.DaB != null && (layer_bits & 1U) != 0U)
			{
				return this.DaB;
			}
			if (this.DaG != null && (layer_bits & 2U) != 0U)
			{
				return this.DaG;
			}
			if (this.DaT != null && (layer_bits & 4U) != 0U)
			{
				return this.DaT;
			}
			if (this.DaL != null && (layer_bits & 8U) != 0U)
			{
				return this.DaL;
			}
			return null;
		}

		public override string ToString()
		{
			if (this._tostring == null)
			{
				STB stb = TX.PopBld(null, 0);
				stb += "<M2Puts> ";
				stb += this.mapx;
				stb += ",";
				stb += this.mapy;
				this._tostring = stb.ToString();
				TX.ReleaseBld(stb);
			}
			return this._tostring;
		}

		public bool active_removed
		{
			get
			{
				return this.active_removed_keys != null;
			}
		}

		public bool active_closed
		{
			get
			{
				return this.active_removed_keys != null && this.AttachCM == null;
			}
		}

		public bool isBg()
		{
			return false;
		}

		public virtual bool config_considerable
		{
			get
			{
				return false;
			}
		}

		public void getPutsBounds(out int l, out int t, out int r, out int b)
		{
			this.Img.getPutsBounds(this, base.CLEN, out l, out t, out r, out b);
		}

		public string getBaseName()
		{
			if (this.Img == null)
			{
				return "";
			}
			return this.Img.basename;
		}

		public string unique_key
		{
			get
			{
				if (this.unique_key_ == null)
				{
					STB stb = TX.PopBld(null, 0);
					stb.Add(this.Lay.name, "__", this.Img.dirname);
					stb.Add(this.Img.basename, ".png");
					stb.Add("__", (((float)this.drawx + (float)this.iwidth * 0.5f) / base.CLEN).ToString(), "_", (((float)this.drawy + (float)this.iheight * 0.5f) / base.CLEN).ToString());
					this.unique_key_ = stb.ToString();
					TX.ReleaseBld(stb);
				}
				return this.unique_key_;
			}
		}

		public T CastDrawer<T>() where T : class
		{
			if (this.DaT is T)
			{
				return this.DaT as T;
			}
			if (this.DaG is T)
			{
				return this.DaG as T;
			}
			if (this.DaB is T)
			{
				return this.DaB as T;
			}
			if (this.DaL is T)
			{
				return this.DaL as T;
			}
			return default(T);
		}

		public override bool isWithin(float _x, float _y, int drawer_index, bool strict = false)
		{
			if (drawer_index < 0)
			{
				return true;
			}
			bool flag;
			switch (drawer_index)
			{
			case 0:
				flag = this.DaL != null;
				break;
			case 1:
				flag = this.DaT != null;
				break;
			case 2:
				flag = this.DaG != null;
				break;
			case 3:
				flag = this.DaB != null;
				break;
			default:
				flag = false;
				break;
			}
			return flag;
		}

		public static void flagArrangeable<T>(List<T> ATargetPuts) where T : M2Puts
		{
			int count = ATargetPuts.Count;
			for (int i = 0; i < count; i++)
			{
				ATargetPuts[i].arrangeable = true;
			}
		}

		public static void flagArrangeable<T>(T[] ATargetPuts, int ARMX = -1) where T : M2Puts
		{
			if (ARMX < 0)
			{
				ARMX = ATargetPuts.Length;
			}
			for (int i = 0; i < ARMX; i++)
			{
				ATargetPuts[i].arrangeable = true;
			}
		}

		public readonly M2MapLayer Lay;

		public int rwidth;

		public int rheight;

		public int iwidth;

		public int iheight;

		public int drawx;

		public int drawy;

		public int mapx;

		public int mapy;

		public int index;

		public int opacity;

		public int rotation;

		public bool flip;

		public M2ChipImage Img;

		protected M2CImgDrawer DaB;

		protected M2CImgDrawer DaG;

		protected M2CImgDrawer DaT;

		protected M2CImgDrawer DaL;

		private List<M2ChipLight> AChipLight;

		public uint pattern;

		public float light_alpha = 1f;

		private string active_removed_keys;

		public bool visible_when_removed = true;

		private bool arrangeable_;

		public ChipMover AttachCM;

		protected string _tostring;

		private string unique_key_;
	}
}
