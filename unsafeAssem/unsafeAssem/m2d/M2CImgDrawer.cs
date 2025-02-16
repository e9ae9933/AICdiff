using System;
using Better;
using PixelLiner;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2CImgDrawer : MdArranger
	{
		public static void CreateDrawerPreparationDefault(BDic<string, M2ImageContainer.FnCreateOneChip> Otg, BDic<string, M2CImgDrawer.FnCreateDrawer> Odr)
		{
			M2CImgDrawer.FnCreateDrawer DrDef = delegate(ref MeshDrawer Md, int lay, M2Puts Cp, METACImg Meta, M2CImgDrawer Pre_Drawer)
			{
				return new M2CImgDrawer(Md, lay, Cp, false);
			};
			Odr["_"] = DrDef;
			Odr["waterfall"] = delegate(ref MeshDrawer Md, int lay, M2Puts Cp, METACImg Meta, M2CImgDrawer Pre_Drawer)
			{
				return new M2CImgDrawerWaterFall(Md, lay, Cp, Meta);
			};
			Odr["water"] = delegate(ref MeshDrawer Md, int lay, M2Puts Cp, METACImg Meta, M2CImgDrawer Pre_Drawer)
			{
				return new M2CImgDrawerWater(Md, lay, Cp, Meta);
			};
			Odr["weed_shake"] = delegate(ref MeshDrawer Md, int lay, M2Puts Cp, METACImg Meta, M2CImgDrawer Pre_Drawer)
			{
				return new M2CImgDrawerWeedShake(Md, lay, Cp, Meta, "weed_shake");
			};
			Odr["tree_leaf"] = delegate(ref MeshDrawer Md, int lay, M2Puts Cp, METACImg Meta, M2CImgDrawer Pre_Drawer)
			{
				if (lay == 0)
				{
					return new M2CImgDrawerTreeLeaf(Md, lay, Cp, Meta, "tree_leaf");
				}
				return DrDef(ref Md, lay, Cp, Meta, Pre_Drawer);
			};
			Odr["light_mesh"] = delegate(ref MeshDrawer Md, int lay, M2Puts Cp, METACImg Meta, M2CImgDrawer Pre_Drawer)
			{
				if (Meta.Get("light_tri") != null)
				{
					return new M2CImgDrawerLight(Md, lay, Cp);
				}
				return DrDef(ref Md, lay, Cp, Meta, Pre_Drawer);
			};
			Odr["particle_loop"] = delegate(ref MeshDrawer Md, int lay, M2Puts Cp, METACImg Meta, M2CImgDrawer Pre_Drawer)
			{
				return new M2CImgDrawerParticleLooper(Md, lay, Cp, Meta);
			};
			Odr["pendulum_bell"] = delegate(ref MeshDrawer Md, int lay, M2Puts Cp, METACImg Meta, M2CImgDrawer Pre_Drawer)
			{
				return new M2CImgDrawerPendulumBell(Md, lay, Cp, Meta);
			};
			Odr["activate_ptcst"] = delegate(ref MeshDrawer Md, int lay, M2Puts Cp, METACImg Meta, M2CImgDrawer Pre_Drawer)
			{
				return new M2CImgDrawerActivateEffect(Md, lay, Cp, Meta);
			};
			Odr["morus_signal"] = delegate(ref MeshDrawer Md, int lay, M2Puts Cp, METACImg Meta, M2CImgDrawer Pre_Drawer)
			{
				int i = Meta.GetI("morus_signal", -1, 0);
				if (i >= 0 && i == lay)
				{
					return new M2CImgDrawerMorusSignal(Md, lay, Cp, Meta);
				}
				return DrDef(ref Md, lay, Cp, Meta, Pre_Drawer);
			};
			Odr["half_scale"] = delegate(ref MeshDrawer Md, int lay, M2Puts Cp, METACImg Meta, M2CImgDrawer Pre_Drawer)
			{
				return new M2CImgDrawerHalf(Md, lay, Cp, false);
			};
			Odr["animate"] = delegate(ref MeshDrawer Md, int lay, M2Puts Cp, METACImg Meta, M2CImgDrawer Pre_Drawer)
			{
				int i2 = Meta.GetI("animate", -1, 0);
				if (i2 >= 0 && i2 == lay)
				{
					return new M2CImgAnimate(Md, lay, Cp);
				}
				return DrDef(ref Md, lay, Cp, Meta, Pre_Drawer);
			};
		}

		public Map2d Mp
		{
			get
			{
				return this.Cp.Mp;
			}
		}

		public M2MapLayer Lay
		{
			get
			{
				return this.Cp.Lay;
			}
		}

		public METACImg Meta
		{
			get
			{
				return this.Cp.getMeta();
			}
		}

		public M2DBase M2D
		{
			get
			{
				return this.Cp.Mp.M2D;
			}
		}

		public float CLEN
		{
			get
			{
				return this.Mp.CLEN;
			}
		}

		public string unique_key
		{
			get
			{
				return this.Cp.unique_key;
			}
		}

		public M2CImgDrawer(MeshDrawer Md, int _layer, M2Puts _Cp, bool _redraw_flag = true)
			: base(Md)
		{
			this.MdM = Md as MdMap;
			this.layer = _layer;
			this.Cp = _Cp;
			this.redraw_flag = _redraw_flag;
		}

		public virtual void initAction(bool normal_map)
		{
		}

		public int translateBasePosition(float x, float y)
		{
			if (this.index_last <= this.index_first)
			{
				return 0;
			}
			if (this.APos != null)
			{
				int num = this.APos.Length;
				for (int i = 0; i < num; i++)
				{
					Vector3 vector = this.APos[i];
					vector.x += x;
					vector.y += y;
					this.APos[i] = vector;
				}
				this.use_translate_by_mover = true;
				this.reposition(3);
			}
			else
			{
				base.translateAll(x, y, false);
			}
			return this.layer2update_flag;
		}

		public int layer2update_flag
		{
			get
			{
				if (this.layer != 100)
				{
					return this.Mp.getLayer2UpdateFlag(this.Md);
				}
				return 8192;
			}
		}

		public virtual void closeAction(bool when_map_close)
		{
		}

		public bool hasRedrawableChipFor(MdMap _MdM)
		{
			return this.APos != null && this.index_last > this.index_first && _MdM == this.MdM;
		}

		public virtual int redraw(float fcnt)
		{
			if (this.APos == null || this.index_last <= this.index_first)
			{
				return 0;
			}
			if (this.need_reposit_flag)
			{
				this.reposition(3);
				return this.layer2update_flag;
			}
			return 0;
		}

		public virtual bool reduceDrawnAlpha(int val = 5)
		{
			Color32[] colorArray = this.Md.getColorArray();
			int num = X.Mn(colorArray.Length, this.index_last);
			if (num <= this.index_first)
			{
				return true;
			}
			bool flag = true;
			for (int i = this.index_first; i < num; i++)
			{
				byte b = (byte)X.Mx(0, (int)colorArray[i].a - val);
				colorArray[i].a = b;
				if (b > 0)
				{
					flag = false;
				}
			}
			return flag;
		}

		public int repositActiveRemoveFlag()
		{
			Color32[] colorArray = this.Md.getColorArray();
			int num = this.index_last - this.index_first;
			if (this.Cp.active_removed && !this.Cp.visible_when_removed && this.Mp.apply_chip_effect)
			{
				for (int i = 0; i < num; i++)
				{
					colorArray[i + this.index_first].a = 0;
				}
			}
			else
			{
				for (int j = 0; j < num; j++)
				{
					colorArray[j + this.index_first].a = this.Cp.Lay.LayerColor.a * (this.use_color ? this.ACol[j].a : 1);
				}
			}
			return this.layer2update_flag;
		}

		public int setColor(Color32 C)
		{
			Color32[] colorArray = this.Md.getColorArray();
			int num = this.index_last - this.index_first;
			if (this.Cp.active_removed && !this.Cp.visible_when_removed && this.Mp.apply_chip_effect)
			{
				for (int i = 0; i < num; i++)
				{
					colorArray[i + this.index_first].a = 0;
				}
			}
			else
			{
				for (int j = 0; j < num; j++)
				{
					colorArray[j + this.index_first] = C;
				}
			}
			return this.layer2update_flag;
		}

		public int setAlpha(byte a)
		{
			Color32[] colorArray = this.Md.getColorArray();
			int num = this.index_last - this.index_first;
			if (this.Cp.active_removed && !this.Cp.visible_when_removed && this.Mp.apply_chip_effect)
			{
				for (int i = 0; i < num; i++)
				{
					colorArray[i + this.index_first].a = 0;
				}
			}
			else
			{
				for (int j = 0; j < num; j++)
				{
					Color32 color = colorArray[j + this.index_first];
					color.a = a;
					colorArray[j + this.index_first] = color;
				}
			}
			return this.layer2update_flag;
		}

		protected virtual void reposition(int shift_col = 3)
		{
			this.need_reposit_flag = false;
			if ((shift_col & 1) != 0)
			{
				if (this.use_shift)
				{
					Vector3[] vertexArray = this.Md.getVertexArray();
					int num = this.index_last - this.index_first;
					float num2 = 0.015625f;
					for (int i = 0; i < num; i++)
					{
						Vector3 vector = this.ASh[i];
						Vector3 vector2 = this.APos[i];
						vector.x = vector.x * num2 + this.translate_by_mover_ux;
						vector.y = vector.y * num2 + this.translate_by_mover_uy;
						vertexArray[i + this.index_first] = vector2 * num2 + vector;
					}
				}
				else if (this.use_translate_by_mover)
				{
					Vector3[] vertexArray2 = this.Md.getVertexArray();
					int num3 = this.index_last - this.index_first;
					float num4 = 0.015625f;
					for (int j = 0; j < num3; j++)
					{
						Vector3 vector3 = this.APos[j] * num4;
						vector3.x += this.translate_by_mover_ux;
						vector3.y += this.translate_by_mover_uy;
						vertexArray2[j + this.index_first] = vector3;
					}
				}
			}
			if ((shift_col & 2) != 0 && this.use_color)
			{
				Color32[] colorArray = this.Md.getColorArray();
				int num5 = this.index_last - this.index_first;
				for (int k = 0; k < num5; k++)
				{
					Color32 color = this.ACol[k];
					if (!this.rewriteable_alpha)
					{
						color.a = colorArray[k + this.index_first].a;
					}
					if (this.Cp.active_removed && !this.Cp.visible_when_removed)
					{
						color.a = 0;
					}
					colorArray[k + this.index_first] = color;
				}
			}
		}

		public virtual bool entryMainPicToMesh(MeshDrawer Md, float meshx, float meshy, float _zmx, float _zmy, float _rotR, PxlMeshDrawer Ms)
		{
			base.Set(false);
			this.entryMainPicToMeshInner(Md, meshx, meshy, _zmx, _zmy, _rotR, Ms);
			base.Set(false);
			if ((this.use_mdarray || this.use_shift || this.use_color) && this.index_last > this.index_first)
			{
				this.initMdArray();
			}
			if (this.Cp.active_removed)
			{
				this.repositActiveRemoveFlag();
			}
			return this.redraw_flag;
		}

		protected virtual bool use_mdarray
		{
			get
			{
				return this.redraw_flag;
			}
		}

		protected virtual void entryMainPicToMeshInner(MeshDrawer Md, float meshx, float meshy, float _zmx, float _zmy, float _rotR, PxlMeshDrawer Ms)
		{
			Md.RotaMesh(meshx, meshy, _zmx, _zmy, _rotR, Ms, false, false);
		}

		protected void initMdArray()
		{
			int num = this.index_last - this.index_first;
			int num2 = 0;
			if (this.APos == null)
			{
				this.APos = new Vector3[num];
				if (this.use_shift)
				{
					this.ASh = new Vector3[num];
				}
				if (this.use_color)
				{
					this.ACol = new Color32[num];
				}
			}
			else if (this.APos.Length != num)
			{
				Array.Resize<Vector3>(ref this.APos, num);
				if (this.use_shift)
				{
					if (this.ASh == null)
					{
						this.ASh = new Vector3[num];
					}
					else
					{
						Array.Resize<Vector3>(ref this.ASh, num);
					}
				}
				if (this.use_color)
				{
					if (this.ACol == null)
					{
						this.ACol = new Color32[num];
					}
					else
					{
						num2 = this.ACol.Length;
						Array.Resize<Color32>(ref this.ACol, num);
					}
				}
			}
			this.reinputPosition();
			if (this.use_color)
			{
				Color32[] colorArray = this.Md.getColorArray();
				for (int i = num2; i < num; i++)
				{
					this.ACol[i] = colorArray[i + this.index_first];
				}
			}
		}

		protected void reinputPosition()
		{
			int num = this.index_last - this.index_first;
			Vector3[] vertexArray = this.Md.getVertexArray();
			for (int i = 0; i < num; i++)
			{
				Vector3 vector = vertexArray[i + this.index_first];
				vector.x *= 64f;
				vector.y *= 64f;
				this.APos[i] = vector;
				if (this.use_shift)
				{
					this.ASh[i].Set(0f, 0f, 0f);
				}
			}
		}

		public int reentryToArrangeableMesh()
		{
			if (this.MdM == null || this.index_last <= this.index_first)
			{
				return 0;
			}
			int num = this.index_last - this.index_first;
			MeshDrawer meshDrawer = this.MdM.checkArrangeableMeshEvacuation(this.Cp, this);
			if (meshDrawer == this.Md)
			{
				return 0;
			}
			int num2 = this.tri_last - this.tri_first;
			int index_first = this.index_first;
			int tri_first = this.tri_first;
			Vector3[] vertexArray = this.Md.getVertexArray();
			Color32[] colorArray = this.Md.getColorArray();
			Vector2[] uvArray = this.Md.getUvArray();
			int[] triangleArray = this.Md.getTriangleArray();
			this.Md = meshDrawer;
			base.Set(true);
			meshDrawer.base_x = (this.Md.base_y = 0f);
			meshDrawer.Identity();
			for (int i = 0; i < num2; i++)
			{
				meshDrawer.Tri(triangleArray[i + tri_first] - index_first);
			}
			for (int j = 0; j < num; j++)
			{
				int num3 = j + index_first;
				Vector3 vector = vertexArray[num3];
				Vector2 vector2 = uvArray[num3];
				meshDrawer.Col = colorArray[num3];
				meshDrawer.uvRectN(vector2.x, vector2.y);
				meshDrawer.base_z = vector.z;
				meshDrawer.Pos(vector.x, vector.y, null);
				colorArray[num3] = MTRX.ColTrnsp;
			}
			return this.layer2update_flag;
		}

		public void SliceRectMesh(PxlMeshDrawer PMesh, int n, int m)
		{
			n = X.Mx(1, n);
			m = X.Mx(1, m);
			Vector2[] array;
			Color32[] array2;
			Vector3[] array3;
			int[] array4;
			int num;
			int num2;
			PMesh.getRawVerticesAndTriangles(out array, out array2, out array3, out array4, out num, out num2);
			Vector3 vector = (array3[3] - array3[0]) / (float)n;
			Vector3 vector2 = (array3[1] - array3[0]) / (float)m;
			Vector2 vector3 = (array[3] - array[0]) / (float)n;
			Vector2 vector4 = (array[1] - array[0]) / (float)m;
			int num3 = (n + 1) * (m + 1);
			Vector3[] array5 = new Vector3[num3];
			int[] array6 = new int[n * m * 6];
			Vector2[] array7 = new Vector2[num3];
			Vector3 vector5 = array3[0];
			Vector2 vector6 = array[0];
			int num4 = 0;
			int num5 = 0;
			for (int i = 0; i < num3; i++)
			{
				array5[i] = vector5;
				array7[i] = vector6;
				if (++num4 > n)
				{
					num4 = 0;
					num5++;
					float sliceYCount = this.getSliceYCount((float)num5, (float)m);
					vector5 = array3[0] + vector2 * sliceYCount;
					vector6 = array[0] + vector4 * sliceYCount;
				}
				else
				{
					vector5 += vector;
					vector6 += vector3;
				}
			}
			int num6 = n + 1;
			int num7 = 0;
			for (int j = 0; j < m; j++)
			{
				int num8 = num6 * j;
				int num9 = num8 + num6;
				for (int k = 0; k < n; k++)
				{
					array6[num7++] = num8;
					array6[num7++] = num9 + 1;
					array6[num7++] = num8 + 1;
					array6[num7++] = num8;
					array6[num7++] = num9;
					array6[num7++] = num9 + 1;
					num8++;
					num9++;
				}
			}
			PMesh.setRawVerticesAndTriangles(array7, array2, array5, array6, -1, -1);
		}

		protected virtual float getSliceYCount(float ycnt, float maxy)
		{
			return ycnt;
		}

		public virtual void translateByChipMover(float ux, float uy, C32 AddCol)
		{
			int num = this.index_last - this.index_first;
			if (num <= 0)
			{
				return;
			}
			bool flag = AddCol != null;
			if (ux != this.translate_by_mover_ux || uy != this.translate_by_mover_uy)
			{
				this.use_translate_by_mover = true;
				if (this.APos == null)
				{
					this.initMdArray();
				}
				this.translate_by_mover_ux = ux;
				this.translate_by_mover_uy = uy;
				this.reposition(flag ? 3 : 1);
			}
			else
			{
				if (!flag)
				{
					return;
				}
				if (this.ACol != null)
				{
					this.reposition(2);
				}
			}
			if (flag)
			{
				Color32[] colorArray = this.Md.getColorArray();
				for (int i = 0; i < num; i++)
				{
					colorArray[i + this.index_first] = AddCol.calcAddGray((this.ACol == null) ? this.Lay.LayerColor.C : this.ACol[i], true);
				}
			}
			this.Mp.addUpdateMesh(this.layer2update_flag, false);
		}

		public bool temporaryReplaceImage(M2ImageAtlas.AtlasRect dImg, bool fix_rot = true)
		{
			if (!dImg.valid)
			{
				X.de("差し替えアトラスが不明", null);
				return false;
			}
			if (this.Cp.Img.clms != 1 || this.Cp.Img.rows != 1 || !(this.Cp is M2Chip))
			{
				return false;
			}
			float num = 1f / (float)this.M2D.MIchip.width;
			float num2 = 1f / (float)this.M2D.MIchip.height;
			float num3 = (float)dImg.x * num;
			float num4 = (float)dImg.y * num2;
			float num5 = (float)dImg.w * num;
			float num6 = (float)dImg.h * num2;
			Vector2[] uvArray = this.Md.getUvArray();
			Vector3[] vertexArray = this.Md.getVertexArray();
			int length = base.Length;
			bool flag = length == 4;
			int num7 = 0;
			float num8 = this.Mp.map2meshx((float)this.Cp.mapx);
			float num9 = this.Mp.map2meshy((float)(this.Cp.mapy + 1));
			if (flag)
			{
				num7 = this.Cp.getConfig(0, 0);
			}
			for (int i = 0; i < length; i++)
			{
				int num10 = (this.Cp.flip ? (3 - i + this.Cp.rotation) : (i + this.Cp.rotation)) % 4;
				bool flag2 = num10 == 1 || num10 == 2;
				Vector2 vector = vertexArray[this.index_first + i];
				int j = 0;
				while (j < 2)
				{
					float num11 = vector.x * 64f;
					float num12 = vector.y * 64f;
					float num13 = ((float)X.IntR(num11) - num8) / this.CLEN;
					float num14 = ((float)X.IntR(num12) - num9) / this.CLEN;
					if (j == 0 && num7 > 0 && CCON.isSlope(num7) && (flag2 ? this.Mp.canStandAndNoBlockSlope(this.Cp.mapx, this.Cp.mapy - 1) : this.Mp.canStandAndNoBlockSlope(this.Cp.mapx, this.Cp.mapy + 1)))
					{
						bool flag3 = num10 <= 1;
						float num15 = X.Mx(0.01f, 1f - CCON.getSlopeLevel(num7, !flag3));
						float num16 = num14;
						if (flag2)
						{
							num16 = X.Mn(num16, num15);
						}
						else
						{
							num16 = X.Mx(num16, num15);
						}
						if (num14 != num16)
						{
							num12 = num16 * this.CLEN + num9;
							vector.y = num12 * 0.015625f;
							vertexArray[this.index_first + i] = vector;
							j++;
							continue;
						}
					}
					uvArray[this.index_first + i] = new Vector2(num3 + num13 * num5, num4 + num14 * num6);
					break;
				}
			}
			this.Mp.addUpdateMesh(this.layer2update_flag, false);
			return true;
		}

		public int dissolveAlpha(byte a, bool showing)
		{
			if (!this.redraw_flag || this.index_first >= this.index_last)
			{
				return 0;
			}
			Color32[] colorArray = this.Md.getColorArray();
			for (int i = this.index_first; i < this.index_last; i++)
			{
				colorArray[i].a = a;
			}
			return this.layer2update_flag;
		}

		public M2CImgDrawer RotaPF(float x, float y, PxlFrame F, bool redrawing, bool consider_alpha = false, bool ignore_visible = false, uint draw_frame_bits = 4294967295U, bool fix_pxl_pos = false)
		{
			this.Md.Identity();
			return this.RotaPF(x, y, 1f, 1f, this.Cp.draw_rotR, F, this.Cp.flip, redrawing, consider_alpha, ignore_visible, draw_frame_bits);
		}

		public M2CImgDrawer RotaPF(float x, float y, float scalex, float scaley, float rotR = 0f, PxlFrame F = null, bool flip = false, bool redrawing = false, bool consider_alpha = false, bool ignore_visible = false, uint draw_frame_bits = 4294967295U)
		{
			if (F == null)
			{
				return this;
			}
			Matrix4x4 currentMatrix = this.Md.getCurrentMatrix();
			Matrix4x4 matrix4x = currentMatrix * Matrix4x4.Translate(new Vector3(x * 0.015625f, y * 0.015625f, 0f));
			if (scalex != 1f || scaley != 1f || flip || rotR != 0f)
			{
				matrix4x = matrix4x * Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, rotR * 0.31830987f * 180f)) * Matrix4x4.Scale(new Vector3(flip ? (-scalex) : scalex, scaley, 1f));
			}
			this.Md.setCurrentMatrix(matrix4x, false);
			Color32 col = this.Md.Col;
			C32 col2 = EffectItem.Col1;
			int num = F.countLayers();
			int num2 = 0;
			int num3 = 0;
			if (!redrawing)
			{
				int num4 = 0;
				for (int i = 0; i < num; i++)
				{
					PxlLayer pxlLayer = F.getLayer(i);
					if (((ulong)draw_frame_bits & (ulong)(1L << (i & 31))) != 0UL && (ignore_visible || pxlLayer.alpha > 0f))
					{
						num4++;
					}
				}
				this.Md.allocVer(this.Md.getVertexMax() + num4 * 4, 0);
				this.Md.allocTri(this.Md.getTriMax() + num4 * 6, 0);
			}
			else
			{
				num2 = this.Md.getVertexMax();
				num3 = this.Md.getTriMax();
				base.revertVerAndTriIndexFirstSaved(false);
			}
			for (int j = 0; j < num; j++)
			{
				PxlLayer pxlLayer2 = F.getLayer(j);
				if (((ulong)draw_frame_bits & (ulong)(1L << (j & 31))) != 0UL && (ignore_visible || pxlLayer2.alpha > 0f))
				{
					if (consider_alpha)
					{
						this.Md.Col = col2.Set(col).mulA(pxlLayer2.alpha / 100f).C;
					}
					M2ImageAtlas.AtlasRect atlasData = this.M2D.IMGS.Atlas.getAtlasData(pxlLayer2.Img);
					if (atlasData.valid)
					{
						atlasData.initAtlasMd(this.Md, this.M2D.MIchip);
						this.Md.RotaL(0f, 0f, pxlLayer2, true, false, 0);
					}
				}
			}
			if (consider_alpha)
			{
				this.Md.Col = col;
			}
			this.Md.setCurrentMatrix(currentMatrix, false);
			if (redrawing)
			{
				this.Md.revertVerAndTriIndex(num2, num3, false);
			}
			return this;
		}

		public void unshiftSpecificMeshDrawer(MeshDrawer Md, int ver, int tri)
		{
			if (this.index_first >= this.index_last || this.Md != Md)
			{
				return;
			}
			this.index_last += ver;
			this.index_first += ver;
			this.tri_first += tri;
			this.tri_last += tri;
		}

		public bool isinCamera()
		{
			return this.Cp.isinCamera(0f);
		}

		public Vector2 getRotatedPos(float x, float y, bool base_to_center)
		{
			if (this.Cp.rotation != 0 || this.Cp.flip)
			{
				Vector2 vector = default(Vector2);
				float num = (x - (float)this.Cp.iwidth * 0.5f) * (float)(this.Cp.flip ? (-1) : 1);
				float num2 = y - (float)this.Cp.iheight * 0.5f;
				if (this.Cp is M2Picture)
				{
					vector.x = num;
					vector.y = num2;
					vector = X.ROTV2e(vector, (float)(this.Cp.rotation / 180) * 3.1415927f);
				}
				else
				{
					switch (this.Cp.rotation)
					{
					case 1:
						vector.x = -num2;
						vector.y = num;
						break;
					case 2:
						vector.x = -num;
						vector.y = -num2;
						break;
					case 3:
						vector.x = num2;
						vector.y = -num;
						break;
					default:
						vector.x = num;
						vector.y = num2;
						break;
					}
				}
				if (!base_to_center)
				{
					vector.x += (float)this.Cp.iwidth * 0.5f;
					vector.y += (float)this.Cp.iheight * 0.5f;
				}
				return vector;
			}
			if (!base_to_center)
			{
				return new Vector2(x, y);
			}
			return new Vector2(x - (float)this.Cp.iwidth * 0.5f, y - (float)this.Cp.iheight * 0.5f);
		}

		public override string ToString()
		{
			if (this._tostring == null)
			{
				STB stb = TX.PopBld(null, 0);
				stb += "<Dr@";
				stb += this.layer;
				stb += ">";
				stb += this.Cp.ToString();
				this._tostring = stb.ToString();
				TX.ReleaseBld(stb);
			}
			return this._tostring;
		}

		protected readonly int layer;

		public readonly MdMap MdM;

		public readonly M2Puts Cp;

		public readonly bool redraw_flag;

		public bool use_shift;

		public bool use_color;

		public bool rewriteable_alpha;

		public bool need_reposit_flag;

		public bool use_translate_by_mover;

		public float translate_by_mover_ux;

		public float translate_by_mover_uy;

		protected Vector3[] APos;

		protected Vector3[] ASh;

		protected Color32[] ACol;

		public const int BLURIMAGE_LAYER = 100;

		private string _tostring;

		public delegate M2CImgDrawer FnCreateDrawer(ref MeshDrawer Md, int lay, M2Puts Cp, METACImg Meta, M2CImgDrawer Pre_Drawer);
	}
}
