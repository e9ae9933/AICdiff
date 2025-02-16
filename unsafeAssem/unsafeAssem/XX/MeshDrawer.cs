using System;
using System.Collections.Generic;
using PixelLiner;
using UnityEngine;
using UnityEngine.Rendering;

namespace XX
{
	public class MeshDrawer : ITeColor
	{
		public MeshDrawer(Mesh _Ms = null, int vertice_capacity = 4, int tri_capacity = 6)
		{
			if (MeshDrawer.ColBuf0 == null)
			{
				MeshDrawer.ColBuf0 = new C32();
				MeshDrawer.ColBuf1 = new C32();
			}
			this.ColGrd = new C32();
			this.AVertice = new Vector3[vertice_capacity];
			this.AMeshUv = new Vector2[vertice_capacity];
			this.Acolor = new Color32[vertice_capacity];
			this.Atriangle = new int[tri_capacity];
			this.Ms = _Ms;
		}

		public MeshDrawer MeshReplace(MeshDrawer Md, bool copy_material_only_maintex = false, bool destruct_mesh = false)
		{
			if (Md == null)
			{
				return this;
			}
			if (destruct_mesh)
			{
				this.MeshReleaseToEmpty();
			}
			this.Ms = Md.Ms;
			this.mesh_cloned = true;
			if (Md.TriMulti == null)
			{
				if (this.TriMulti != null)
				{
					this.chooseSubMesh(0, false, false);
					this.TriMulti = null;
				}
				if (copy_material_only_maintex)
				{
					this.Mtr.SetTexture("_MainTex", Md.Mtr.GetTexture("_MainTex"));
				}
			}
			else
			{
				this.TriMulti = new MeshDrawerTriMultipleManager(this, Md.TriMulti);
				if (copy_material_only_maintex)
				{
					this.TriMulti.copyMaterialOnlyMainTex(this.Mtr);
				}
			}
			return this;
		}

		public void copyMultipleTriangleAndMaterial(MeshDrawer Md, bool copy_material_only_maintex = false)
		{
			if (Md.TriMulti == null)
			{
				if (this.TriMulti != null)
				{
					this.chooseSubMesh(0, false, false);
					this.TriMulti = null;
				}
				if (copy_material_only_maintex)
				{
					this.Mtr.SetTexture("_MainTex", Md.Mtr.GetTexture("_MainTex"));
					return;
				}
				this.setMaterial(Md.Mtr, false);
				return;
			}
			else
			{
				Md.TriMulti.Save();
				this.TriMulti = new MeshDrawerTriMultipleManager(this, Md.TriMulti);
				if (copy_material_only_maintex)
				{
					this.TriMulti.copyMaterialOnlyMainTex(this.Mtr);
					return;
				}
				this.readFromTriMultiItem(Md.TriMulti.getCurrent());
				return;
			}
		}

		public virtual void destruct()
		{
			if (this.material_cloned && this.Mtr != null)
			{
				IN.DestroyOne(this.Mtr);
				this.Mtr = null;
				this.material_cloned = false;
			}
			if (this.TriMulti != null)
			{
				this.TriMulti.destruct();
			}
			this.MeshReleaseToEmpty();
			this.Ms = null;
		}

		public MeshDrawer MeshReplace(Mesh Md, bool destruct_mesh = false)
		{
			if (Md == null)
			{
				return this;
			}
			if (destruct_mesh && this.Ms != null)
			{
				this.MeshReleaseToEmpty();
			}
			this.Ms = Md;
			this.mesh_cloned = true;
			return this;
		}

		public bool draw_gl_only
		{
			get
			{
				return (this.state & MeshDrawer.STATE.DRAW_GL_ONLY) > (MeshDrawer.STATE)0;
			}
			set
			{
				if (value == this.draw_gl_only)
				{
					return;
				}
				if (value)
				{
					this.use_cache = false;
					this.state |= MeshDrawer.STATE.DRAW_GL_ONLY;
					return;
				}
				this.state &= (MeshDrawer.STATE)(-33);
			}
		}

		public bool use_z_transform
		{
			get
			{
				return (this.state & MeshDrawer.STATE.USE_Z_TRANSFORM) > (MeshDrawer.STATE)0;
			}
			set
			{
				if (value == this.use_z_transform)
				{
					return;
				}
				if (value)
				{
					this.state |= MeshDrawer.STATE.USE_Z_TRANSFORM;
					return;
				}
				this.state &= (MeshDrawer.STATE)(-129);
			}
		}

		public bool use_cache
		{
			get
			{
				return (this.state & MeshDrawer.STATE.USE_CACHE) > (MeshDrawer.STATE)0;
			}
			set
			{
				if (value == this.use_cache)
				{
					return;
				}
				if (value)
				{
					this.state |= MeshDrawer.STATE.USE_CACHE;
					return;
				}
				this.state &= (MeshDrawer.STATE)(-65);
				this.AMeshUvRendered = null;
				this.AcolorRendered = null;
				this.AVerticeRendered = null;
				this.AMeshUv2Rendered = null;
				this.AMeshUv3Rendered = null;
				this.AtriangleRendered = null;
			}
		}

		public MeshDrawer MeshReleaseToEmpty()
		{
			if (this.Ms != null && !this.mesh_cloned)
			{
				Object.DestroyImmediate(this.Ms);
			}
			this.Ms = null;
			this.mesh_cloned = false;
			return this;
		}

		public MeshDrawer BaseZ(float z)
		{
			this.base_z = z;
			return this;
		}

		public virtual MeshDrawer clear(bool no_clear_mesh = false, bool release_trimulti = false)
		{
			if (!no_clear_mesh)
			{
				if (!this.valotile && this.Ms != null)
				{
					this.Ms.Clear();
				}
				if (this.Valot != null)
				{
					this.Valot.clearCount();
				}
			}
			if (this.TriMulti != null)
			{
				this.TriMulti.clear(release_trimulti);
				if (release_trimulti)
				{
					this.TriMulti = null;
				}
			}
			this.state &= (MeshDrawer.STATE)(-29);
			this.activation_key = "";
			this.MatrixTransform = Matrix4x4.identity;
			this.resetVertexCount();
			this.transformed = false;
			this.fliped = false;
			this.gradation = false;
			this.colorassign2mtr = false;
			this.fnMeshPointColor = null;
			this.uv_settype = UV_SETTYPE.NONE;
			return this;
		}

		public MeshDrawer clearSimple()
		{
			if (this.Valot != null)
			{
				this.Valot.clearCount();
			}
			this.resetVertexCount();
			if (!this.valotile)
			{
				this.prepareMesh();
				this.Ms.Clear();
			}
			return this;
		}

		public void release()
		{
			this.clear(false, false);
			this.state = (this.state & MeshDrawer.STATE.DRAWING_OPT) | (MeshDrawer.STATE)3;
		}

		public MeshDrawer activate()
		{
			return this.activate("", MTRX.MtrMeshNormal, false, MTRX.ColWhite, null);
		}

		public MeshDrawer activate(string key, Material _Mtr, bool is_bottom, Color32 _Col, C32 _ColForGrd = null)
		{
			Bench.P("MESH activate");
			if (!this.draw_gl_only)
			{
				this.prepareMesh();
			}
			else
			{
				this.MeshReleaseToEmpty();
			}
			this.clear(true, false);
			this.activation_key = key;
			this.Col = _Col;
			if (_ColForGrd != null)
			{
				this.ColGrd.Set(_ColForGrd);
				this.gradation = true;
			}
			else
			{
				this.gradation = false;
			}
			this.colorassign2mtr = false;
			if (this.Mtr != _Mtr)
			{
				this.Mtr = _Mtr;
				this.shader_name_ = null;
			}
			this.render_queue0 = (this.render_queue_ = this.Mtr.renderQueue);
			this.material_cloned = false;
			this.state = (this.state & MeshDrawer.STATE.DRAWING_OPT) | (is_bottom ? ((MeshDrawer.STATE)0) : MeshDrawer.STATE.TOP);
			if (this.ppu == 0f)
			{
				this.ppu = 64f;
				this.ppur = 0.015625f;
			}
			Bench.Pend("MESH activate");
			return this;
		}

		public void prepareMesh()
		{
			if (!this.draw_gl_only && this.Ms == null)
			{
				this.Ms = new Mesh();
				this.mesh_cloned = false;
				this.Ms.MarkDynamic();
				this.Ms.name = this.activation_key;
			}
		}

		public MeshDrawer activate(string key, Material _Mtr, MeshFilter Mf, float _base_z)
		{
			this.prepareMesh();
			this.clear(true, false);
			this.activation_key = key;
			if (this.Mtr != _Mtr)
			{
				this.Mtr = _Mtr;
				this.shader_name_ = null;
			}
			this.material_cloned = false;
			this.state = (this.state & MeshDrawer.STATE.DRAWING_OPT) | MeshDrawer.STATE.TOP;
			this.base_z = _base_z;
			Mf.sharedMesh = this.Ms;
			this.render_queue0 = (this.render_queue_ = this.Mtr.renderQueue);
			this.colorassign2mtr = false;
			if (this.colorassign2mtr)
			{
				MaterialPropertyBlock materialPropertyBlock = this.materialPropertyBlock;
			}
			if (this.ppu == 0f)
			{
				this.ppu = 64f;
				this.ppur = 0.015625f;
			}
			return this;
		}

		public bool activateIf(string key, Material _Mtr, bool is_bottom, Color32 _Col, C32 _ColForGrd = null, bool _colorassign2mtr = false, bool do_not_activate = false)
		{
			if (!this.isActive() && !do_not_activate)
			{
				this.activate(key, _Mtr, is_bottom, _Col, _ColForGrd);
				return true;
			}
			if (!(key == this.activation_key) || !this.isSameMtr(_Mtr) || is_bottom != ((this.state & MeshDrawer.STATE.TOP) == (MeshDrawer.STATE)0))
			{
				return false;
			}
			if (!this.isActive())
			{
				this.activate(key, _Mtr, is_bottom, _Col, _ColForGrd);
				return true;
			}
			this.Identity();
			this.Col = _Col;
			if (_ColForGrd != null)
			{
				this.ColGrd.Set(_ColForGrd);
				this.gradation = true;
			}
			else
			{
				this.gradation = false;
			}
			return true;
		}

		public bool isSameMtr(Material _Mtr)
		{
			return !(_Mtr == null) && !(this.Mtr == null) && this.Mtr == _Mtr;
		}

		public MeshDrawer initForImgWhite(Sprite Spr)
		{
			this.uv_settype = UV_SETTYPE.NONE;
			Rect textureRect = Spr.textureRect;
			this.texture_width = (float)Spr.texture.width;
			this.texture_height = (float)Spr.texture.height;
			this.uv_left = (textureRect.x + textureRect.width / 2f) / this.texture_width;
			this.uv_top = (textureRect.y + textureRect.height / 2f) / this.texture_height;
			this.uv_width = textureRect.width / this.texture_width;
			this.uv_height = textureRect.height / this.texture_height;
			this.Col = MTRX.ColWhite;
			return this;
		}

		public MeshDrawer initForImg(MImage MI)
		{
			return this.initForImg(MI.Tx);
		}

		public MeshDrawer initForImg(PxlImage Img, int margin = 0)
		{
			this.uv_settype = UV_SETTYPE.IMG;
			Rect rectIUv = Img.RectIUv;
			this.texture_width = (float)Img.get_I().width;
			this.texture_height = (float)Img.get_I().height;
			this.uv_left = rectIUv.x;
			this.uv_top = rectIUv.y;
			this.uv_width = rectIUv.width;
			this.uv_height = rectIUv.height;
			if (margin != 0)
			{
				float num = 1f / this.texture_width;
				float num2 = 1f / this.texture_height;
				this.uv_left -= num * (float)margin;
				this.uv_top -= num2 * (float)margin;
				this.uv_width += num * (float)margin * 2f;
				this.uv_height += num2 * (float)margin * 2f;
			}
			return this;
		}

		public MeshDrawer initForImg(Sprite Spr)
		{
			this.uv_settype = UV_SETTYPE.IMG;
			Rect textureRect = Spr.textureRect;
			this.texture_width = (float)Spr.texture.width;
			this.texture_height = (float)Spr.texture.height;
			this.uv_left = textureRect.x / this.texture_width;
			this.uv_top = textureRect.y / this.texture_height;
			this.uv_width = textureRect.width / this.texture_width;
			this.uv_height = textureRect.height / this.texture_height;
			return this;
		}

		public virtual MeshDrawer initForImg(Texture Spr)
		{
			this.uv_settype = UV_SETTYPE.IMG;
			this.uv_left = (this.uv_top = 0f);
			this.uv_width = (this.uv_height = 1f);
			this.texture_width = (float)Spr.width;
			this.texture_height = (float)Spr.height;
			return this;
		}

		public MeshDrawer initForImg(Texture Spr, float texture_px_x, float texture_px_y, float texture_px_w, float texture_px_h)
		{
			this.uv_settype = UV_SETTYPE.IMG;
			this.texture_width = (float)Spr.width;
			this.texture_height = (float)Spr.height;
			this.uv_left = texture_px_x / this.texture_width;
			this.uv_top = texture_px_y / this.texture_height;
			this.uv_width = texture_px_w / this.texture_width;
			this.uv_height = texture_px_h / this.texture_height;
			return this;
		}

		public MeshDrawer initForImg(Texture Spr, Rect Rc, bool divide_texture_wh = true)
		{
			this.uv_settype = UV_SETTYPE.IMG;
			this.texture_width = (float)Spr.width;
			this.texture_height = (float)Spr.height;
			this.uv_left = (divide_texture_wh ? (Rc.x / this.texture_width) : Rc.x);
			this.uv_top = (divide_texture_wh ? (Rc.y / this.texture_height) : Rc.y);
			this.uv_width = (divide_texture_wh ? (Rc.width / this.texture_width) : Rc.width);
			this.uv_height = (divide_texture_wh ? (Rc.height / this.texture_height) : Rc.height);
			return this;
		}

		public MeshDrawer initForImg(Texture Spr, DRect Rc, bool divide_texture_wh = true)
		{
			this.uv_settype = UV_SETTYPE.IMG;
			this.texture_width = (float)Spr.width;
			this.texture_height = (float)Spr.height;
			this.uv_left = (divide_texture_wh ? (Rc.x / this.texture_width) : Rc.x);
			this.uv_top = (divide_texture_wh ? (Rc.y / this.texture_height) : Rc.y);
			this.uv_width = (divide_texture_wh ? (Rc.width / this.texture_width) : Rc.width);
			this.uv_height = (divide_texture_wh ? (Rc.height / this.texture_height) : Rc.height);
			return this;
		}

		public MeshDrawer initForPostEffect(Texture FinalRendered, float shift_ux, float shift_uy)
		{
			float num = (float)FinalRendered.width / IN.w;
			return this.initForPostEffect(FinalRendered, -IN.wh * num, -IN.hh * num, IN.w * num, IN.h * num, shift_ux, shift_uy, 1f);
		}

		public MeshDrawer initForPostEffect(Texture FinalRendered, float left_px, float top_px, float width_px, float height_px, float shift_ux, float shift_uy, float scale = 1f)
		{
			this.Mtr.SetTexture("_MainTex", FinalRendered);
			this.uv_settype = UV_SETTYPE.MANUAL;
			float num = (left_px + width_px / 2f) * this.ppur;
			float num2 = (top_px + height_px / 2f) * this.ppur;
			this.uv_width = width_px * scale * this.ppur;
			this.uv_height = height_px * scale * this.ppur;
			this.uv_left = num - this.uv_width / 2f + shift_ux;
			this.uv_top = num2 - this.uv_height / 2f + shift_uy;
			this.texture_width = (float)FinalRendered.width;
			this.texture_height = (float)FinalRendered.height;
			return this;
		}

		public MeshDrawer setMaterial(Material _Mtr, bool cloned = false)
		{
			if (this.Mtr != _Mtr)
			{
				if (this.Mtr != null && this.material_cloned && this.Mtr != _Mtr)
				{
					IN.DestroyOne(this.Mtr);
				}
				this.Mtr = _Mtr;
				this.shader_name_ = null;
			}
			if (this.Mtr != null)
			{
				this.render_queue_ = (this.render_queue0 = this.Mtr.renderQueue);
				this.material_cloned = cloned;
			}
			else
			{
				this.material_cloned = false;
			}
			if (this.TriMulti != null)
			{
				this.TriMulti.Save();
			}
			return this;
		}

		public int render_queue
		{
			get
			{
				return this.render_queue_;
			}
			set
			{
				if (this.render_queue_ == value)
				{
					return;
				}
				if (!this.material_cloned)
				{
					this.Mtr = MTRX.newMtr(this.Mtr);
					this.material_cloned = true;
				}
				Material mtr = this.Mtr;
				this.render_queue_ = value;
				mtr.renderQueue = value;
			}
		}

		public Material getMaterial()
		{
			return this.Mtr;
		}

		public Material[] getMaterialArray(bool force_clone = false)
		{
			if (this.TriMulti == null)
			{
				return new Material[] { this.Mtr };
			}
			return this.TriMulti.getMaterialArray(force_clone);
		}

		public MeshDrawer Tri(int tri_index = 0)
		{
			int num = this.tri_i;
			if (this.transformed && this.fliped)
			{
				int num2 = this.tri_i % 3;
				num += ((num2 == 0) ? 0 : ((num2 == 1) ? 1 : (-1)));
			}
			if (num >= this.Atriangle.Length)
			{
				Array.Resize<int>(ref this.Atriangle, X.Mx(num, 3) + 18);
			}
			this.Atriangle[num] = this.ver_i + tri_index;
			this.tri_i++;
			return this;
		}

		public MeshDrawer Tri(List<int> Atri)
		{
			int count = Atri.Count;
			this.allocTri(this.tri_i + count, 60);
			int i = 0;
			while (i < count)
			{
				int[] atriangle = this.Atriangle;
				int num = this.tri_i;
				this.tri_i = num + 1;
				atriangle[num] = this.ver_i + Atri[i++];
				if (this.transformed && this.fliped)
				{
					this.Atriangle[this.tri_i + 1] = this.ver_i + Atri[i++];
					this.Atriangle[this.tri_i] = this.ver_i + Atri[i++];
					this.tri_i += 2;
				}
				else
				{
					int[] atriangle2 = this.Atriangle;
					num = this.tri_i;
					this.tri_i = num + 1;
					atriangle2[num] = this.ver_i + Atri[i++];
					int[] atriangle3 = this.Atriangle;
					num = this.tri_i;
					this.tri_i = num + 1;
					atriangle3[num] = this.ver_i + Atri[i++];
				}
			}
			return this;
		}

		public MeshDrawer Tri(int t0, int t1, int t2, bool culloff = false)
		{
			if (!culloff)
			{
				if (this.tri_i + 2 >= this.Atriangle.Length)
				{
					Array.Resize<int>(ref this.Atriangle, this.tri_i + 18);
				}
				int[] atriangle = this.Atriangle;
				int num = this.tri_i;
				this.tri_i = num + 1;
				atriangle[num] = this.ver_i + t0;
				if (this.transformed && this.fliped)
				{
					int[] atriangle2 = this.Atriangle;
					num = this.tri_i;
					this.tri_i = num + 1;
					atriangle2[num] = this.ver_i + t2;
					int[] atriangle3 = this.Atriangle;
					num = this.tri_i;
					this.tri_i = num + 1;
					atriangle3[num] = this.ver_i + t1;
				}
				else
				{
					int[] atriangle4 = this.Atriangle;
					num = this.tri_i;
					this.tri_i = num + 1;
					atriangle4[num] = this.ver_i + t1;
					int[] atriangle5 = this.Atriangle;
					num = this.tri_i;
					this.tri_i = num + 1;
					atriangle5[num] = this.ver_i + t2;
				}
			}
			else
			{
				if (this.tri_i + 5 >= this.Atriangle.Length)
				{
					Array.Resize<int>(ref this.Atriangle, this.tri_i + 18);
				}
				int[] atriangle6 = this.Atriangle;
				int num = this.tri_i;
				this.tri_i = num + 1;
				atriangle6[num] = this.ver_i + t0;
				int[] atriangle7 = this.Atriangle;
				num = this.tri_i;
				this.tri_i = num + 1;
				atriangle7[num] = this.ver_i + t2;
				int[] atriangle8 = this.Atriangle;
				num = this.tri_i;
				this.tri_i = num + 1;
				atriangle8[num] = this.ver_i + t1;
				int[] atriangle9 = this.Atriangle;
				num = this.tri_i;
				this.tri_i = num + 1;
				atriangle9[num] = this.ver_i + t0;
				int[] atriangle10 = this.Atriangle;
				num = this.tri_i;
				this.tri_i = num + 1;
				atriangle10[num] = this.ver_i + t1;
				int[] atriangle11 = this.Atriangle;
				num = this.tri_i;
				this.tri_i = num + 1;
				atriangle11[num] = this.ver_i + t2;
			}
			return this;
		}

		public MeshDrawer Tri012()
		{
			return this.Tri(0, 1, 2, false);
		}

		public MeshDrawer TriRectBL(int add = 0)
		{
			this.Tri(add, 1 + add, 2 + add, false).Tri(add, 2 + add, 3 + add, false);
			return this;
		}

		public MeshDrawer TriRectBL(int lb, int lt, int tr, int rb)
		{
			this.Tri(lb, lt, tr, false).Tri(lb, tr, rb, false);
			return this;
		}

		public MeshDrawer TriN(int tri_index)
		{
			return this.Tri(tri_index - this.ver_i);
		}

		public MeshDrawer PosAppend(float x0, float y0, Vector3[] _AVer, Vector2[] _AUv, Color32[] _ACol, int[] _ATri, float scalex = 1f, float scaley = 1f, int ver_max = -1, int tri_max = -1)
		{
			if (ver_max < 0)
			{
				ver_max = _AVer.Length;
			}
			if (tri_max < 0)
			{
				tri_max = _ATri.Length;
			}
			this.allocTri(tri_max + this.tri_i, 60);
			for (int i = 0; i < tri_max; i++)
			{
				this.Tri(_ATri[i]);
			}
			this.allocVer(ver_max + this.ver_i, 64);
			this.uv_settype = UV_SETTYPE.NONE;
			for (int j = 0; j < ver_max; j++)
			{
				Vector3 vector = _AVer[j];
				Vector2 vector2 = _AUv[j];
				this.uv_left = vector2.x;
				this.uv_top = vector2.y;
				this.Pos(x0 + vector.x * scalex, y0 + vector.y * scaley, MeshDrawer.ColBuf0.Set(_ACol[j]).multiply(this.Col, true));
			}
			return this;
		}

		public MeshDrawer PosD(float x, float y, C32 sColor = null)
		{
			return this.Pos(x * this.ppur, y * this.ppur, sColor);
		}

		public MeshDrawer allocVT(int ver, int tri, int multiply = 1)
		{
			return this.allocVer(ver * multiply, 0).allocTri(tri * multiply, 0);
		}

		public MeshDrawer allocVer(int ver, int margin = 64)
		{
			if (ver >= this.AVertice.Length)
			{
				ver += margin;
				Array.Resize<Vector3>(ref this.AVertice, ver);
				Array.Resize<Vector2>(ref this.AMeshUv, ver);
				Array.Resize<Color32>(ref this.Acolor, ver);
				if (this.use_uv2 && this.AMeshUv2 != null && this.AMeshUv2.Length < ver)
				{
					Array.Resize<Vector2>(ref this.AMeshUv2, ver);
				}
				if (this.use_uv3 && this.AMeshUv3 != null && this.AMeshUv3.Length < ver)
				{
					Array.Resize<Vector2>(ref this.AMeshUv3, ver);
				}
			}
			return this;
		}

		public MeshDrawer allocTri(int tri, int margin = 60)
		{
			if (tri > this.Atriangle.Length)
			{
				Array.Resize<int>(ref this.Atriangle, tri + margin);
			}
			return this;
		}

		public MeshDrawer allocRevtVerTri(int count, int margin = 60)
		{
			this.allocTri(this.tri_i + count * 6, margin);
			this.allocVer(this.ver_i + count * 4, margin);
			return this;
		}

		public MeshDrawer Pos(Vector2 V, C32 sColor = null)
		{
			return this.Pos(V.x, V.y, sColor);
		}

		public MeshDrawer Pos(float x, float y, C32 sColor = null)
		{
			this.allocVer(this.ver_i, 64);
			switch (this.uv_settype)
			{
			case UV_SETTYPE.NONE:
				this.AMeshUv[this.ver_i].Set(this.uv_left, this.uv_top);
				goto IL_0086;
			case UV_SETTYPE.MANUAL:
			case UV_SETTYPE.IMG:
				goto IL_0086;
			}
			this.AMeshUv[this.ver_i].Set((x - this.uv_left) / this.uv_width, (y - this.uv_top) / this.uv_height);
			IL_0086:
			float num = this.base_z;
			if (this.transformed)
			{
				if (this.use_z_transform)
				{
					MeshDrawer.BufTrs.Set(x, y, num);
					MeshDrawer.BufTrs = this.MatrixTransform.MultiplyPoint3x4(MeshDrawer.BufTrs);
					x = MeshDrawer.BufTrs.x;
					y = MeshDrawer.BufTrs.y;
					num = MeshDrawer.BufTrs.z;
				}
				else
				{
					MeshDrawer.BufTrs.Set(x, y, 0f);
					MeshDrawer.BufTrs = this.MatrixTransform.MultiplyPoint3x4(MeshDrawer.BufTrs);
					x = MeshDrawer.BufTrs.x;
					y = MeshDrawer.BufTrs.y;
					num += MeshDrawer.BufTrs.z;
				}
			}
			if (this.uv_settype == UV_SETTYPE.MANUAL)
			{
				this.AMeshUv[this.ver_i].Set((x + this.base_x - this.uv_left) / this.uv_width, (this.base_y + y - this.uv_top) / this.uv_height);
			}
			this.AVertice[this.ver_i].Set(this.base_x + x, this.base_y + y, num);
			if (this.fnMeshPointColor != null)
			{
				sColor = this.fnMeshPointColor(this, x, y, sColor, 0f, 0f);
			}
			Color32[] acolor = this.Acolor;
			int num2 = this.ver_i;
			this.ver_i = num2 + 1;
			acolor[num2] = ((sColor == null) ? this.Col : sColor.C);
			return this;
		}

		public MeshDrawer PosUv(float x, float y, float uvx, float uvy, C32 sColor = null)
		{
			this.Pos(x, y, sColor);
			this.AMeshUv[this.ver_i - 1] = new Vector2(uvx, uvy);
			return this;
		}

		public MeshDrawer PosReset(int pos_i, float x, float y, C32 sColor = null)
		{
			int num = this.ver_i;
			this.ver_i = pos_i;
			this.Pos(x, y, sColor);
			this.ver_i = num;
			return this;
		}

		public MeshDrawer Uv3(float x, float y, bool alloc_prefix_empty = false)
		{
			this.state |= MeshDrawer.STATE.USE_UV3;
			if (this.AMeshUv3 == null)
			{
				this.AMeshUv3 = new Vector2[(alloc_prefix_empty ? (this.ver_i + 16) : 0) + 16];
			}
			if (alloc_prefix_empty)
			{
				this.uv3_i = this.ver_i;
			}
			if (this.uv3_i >= this.AMeshUv3.Length)
			{
				Array.Resize<Vector2>(ref this.AMeshUv3, this.uv3_i + 64);
			}
			Vector2[] ameshUv = this.AMeshUv3;
			int num = this.uv3_i;
			this.uv3_i = num + 1;
			ameshUv[num].Set(x, y);
			return this;
		}

		public MeshDrawer allocUv2(int margin = 64, bool fill_last_uv = false)
		{
			this.state |= MeshDrawer.STATE.USE_UV2;
			int num = this.uv2_i;
			this.uv2_i = X.Mx(this.uv2_i, this.ver_i);
			if (this.AMeshUv2 == null)
			{
				this.AMeshUv2 = new Vector2[this.uv2_i + margin];
			}
			int num2 = this.AMeshUv2.Length;
			if (this.uv2_i >= num2)
			{
				Array.Resize<Vector2>(ref this.AMeshUv2, this.uv2_i + margin);
			}
			if (num > 0 && fill_last_uv)
			{
				Vector2 vector = this.AMeshUv2[num - 1];
				for (int i = num; i < this.uv2_i; i++)
				{
					this.AMeshUv2[i] = vector;
				}
			}
			return this;
		}

		public MeshDrawer allocUv23(int margin = 64, bool fill_last_uv = false)
		{
			return this.allocUv2(margin, fill_last_uv).allocUv3(margin, fill_last_uv);
		}

		public MeshDrawer Uv2(float x, float y, bool alloc_prefix_empty = false)
		{
			this.state |= MeshDrawer.STATE.USE_UV2;
			if (this.AMeshUv2 == null)
			{
				this.AMeshUv2 = new Vector2[alloc_prefix_empty ? (this.ver_i + 16) : 16];
			}
			if (alloc_prefix_empty)
			{
				this.uv2_i = this.ver_i;
			}
			if (this.uv2_i >= this.AMeshUv2.Length)
			{
				Array.Resize<Vector2>(ref this.AMeshUv2, this.uv2_i + 64);
			}
			Vector2[] ameshUv = this.AMeshUv2;
			int num = this.uv2_i;
			this.uv2_i = num + 1;
			ameshUv[num].Set(x, y);
			return this;
		}

		public MeshDrawer allocUv3(int margin = 64, bool fill_last_uv = false)
		{
			this.state |= MeshDrawer.STATE.USE_UV3;
			int num = this.uv3_i;
			this.uv3_i = X.Mx(this.uv3_i, this.ver_i);
			if (this.AMeshUv3 == null)
			{
				this.AMeshUv3 = new Vector2[this.uv3_i + 16];
			}
			int num2 = this.AMeshUv3.Length;
			if (this.uv3_i >= num2)
			{
				Array.Resize<Vector2>(ref this.AMeshUv3, this.uv3_i + margin);
			}
			if (num > 0 && fill_last_uv)
			{
				Vector3 vector = this.AMeshUv3[num - 1];
				for (int i = num; i < this.uv3_i; i++)
				{
					this.AMeshUv3[i] = vector;
				}
			}
			return this;
		}

		public MeshDrawer Uv23(Color32 C, bool alloc_prefix_empty = false)
		{
			float num = 0.003921569f;
			this.Uv2((float)C.r * num, (float)C.g * num, alloc_prefix_empty);
			return this.Uv3((float)C.b * num, (float)C.a * num, alloc_prefix_empty);
		}

		public MeshDrawer Uv23(float x, float y, float z, float w, bool alloc_prefix_empty = false)
		{
			this.Uv2(x, y, alloc_prefix_empty);
			return this.Uv3(z, w, alloc_prefix_empty);
		}

		public MeshDrawer Uv2(Sprite Img)
		{
			float num = 1f / (float)Img.texture.width;
			float num2 = 1f / (float)Img.texture.height;
			return this.Uv2(Img.rect.x * num, Img.rect.y * num2, false).Uv2(Img.rect.x * num, Img.rect.yMax * num2, false).Uv2(Img.rect.xMax * num, Img.rect.yMax * num2, false)
				.Uv2(Img.rect.xMax * num, Img.rect.y * num2, false);
		}

		public MeshDrawer ClipUv2(float ver_l, float ver_b, float ver_w, float ver_h, Sprite Img, bool flip = false, float alloc_margin_px = 0f)
		{
			float num = 1f / (float)Img.texture.width;
			float num2 = 1f / (float)Img.texture.height;
			float num3 = alloc_margin_px * num;
			float num4 = alloc_margin_px * num2;
			if (flip)
			{
				return this.ClipUv2(ver_l - alloc_margin_px, ver_b - alloc_margin_px, ver_w + alloc_margin_px * 2f, ver_h + alloc_margin_px * 2f, Img.rect.xMax * num + num3, Img.rect.y * num2 - num4, -Img.rect.width * num - num3 * 2f, Img.rect.height * num2 + num4 * 2f, false);
			}
			return this.ClipUv2(ver_l - alloc_margin_px, ver_b - alloc_margin_px, ver_w + alloc_margin_px * 2f, ver_h + alloc_margin_px * 2f, Img.rect.x * num - num3, Img.rect.y * num2 - num4, Img.rect.width * num + num3 * 2f, Img.rect.height * num2 + num4 * 2f, false);
		}

		public MeshDrawer ClipUv2(float ver_l, float ver_b, float ver_w, float ver_h, float uvl, float uvb, float uvw, float uvh, bool no_divide_ppu = false)
		{
			if (!no_divide_ppu && this.ppu != 1f)
			{
				ver_l *= this.ppur;
				ver_b *= this.ppur;
				ver_w *= this.ppur;
				ver_h *= this.ppur;
			}
			this.allocUv2(64, false);
			while (this.uv2_i < this.ver_i)
			{
				Vector3 vector = this.AVertice[this.uv2_i];
				this.Uv2(uvl + X.ZLINE(vector.x - this.base_x - ver_l, ver_w) * uvw, uvb + X.ZLINE(vector.y - this.base_y - ver_b, ver_h) * uvh, false);
			}
			return this;
		}

		public int PosBezier(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4, float resolution = 3f, bool no_divide_ppu = false)
		{
			if (!no_divide_ppu && this.ppu != 1f)
			{
				x1 *= this.ppur;
				y1 *= this.ppur;
				x2 *= this.ppur;
				y2 *= this.ppur;
				x3 *= this.ppur;
				y3 *= this.ppur;
				x4 *= this.ppur;
				y4 *= this.ppur;
				resolution *= this.ppur;
			}
			int num = X.Mx((int)(X.LENGTHXYS(x1, y1, x4, y4) / resolution), 2);
			float num2 = 0f;
			float num3 = 1f / (float)num;
			this.Pos(x1, y1, null);
			for (int i = 1; i <= num; i++)
			{
				float num4 = ((i == num) ? x4 : X.BEZIER_I(x1, x2, x3, x4, num2));
				float num5 = ((i == num) ? y4 : X.BEZIER_I(y1, y2, y3, y4, num2));
				this.Pos(num4, num5, null);
				num2 += num3;
			}
			return num + 1;
		}

		public MeshDrawer BezierLine(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4, float resolution, float thick_shiftx, float thick_shifty, bool shift_to_minus = false, bool no_divide_ppu = false)
		{
			if (!no_divide_ppu && this.ppu != 1f)
			{
				x1 *= this.ppur;
				y1 *= this.ppur;
				x2 *= this.ppur;
				y2 *= this.ppur;
				x3 *= this.ppur;
				y3 *= this.ppur;
				x4 *= this.ppur;
				y4 *= this.ppur;
				thick_shiftx *= this.ppur;
				thick_shifty *= this.ppur;
				resolution *= this.ppur;
			}
			int num = X.Mx((int)(X.LENGTHXYS(x1, y1, x4, y4) / resolution), 2);
			float num2 = 0f;
			float num3 = 1f / (float)num;
			for (int i = 0; i < num; i++)
			{
				int num4 = i * 2;
				if (shift_to_minus)
				{
					this.Tri(num4, num4 + 2, num4 + 1, false).Tri(num4 + 2, num4 + 3, num4 + 1, false);
				}
				else
				{
					this.Tri(num4, num4 + 1, num4 + 2, false).Tri(num4 + 2, num4 + 1, num4 + 3, false);
				}
			}
			this.Pos(x1, y1, null);
			this.Pos(x1 + thick_shiftx, y1 + thick_shifty, null);
			for (int j = 1; j <= num; j++)
			{
				float num5 = ((j == num) ? x4 : X.BEZIER_I(x1, x2, x3, x4, num2));
				float num6 = ((j == num) ? y4 : X.BEZIER_I(y1, y2, y3, y4, num2));
				this.Pos(num5, num6, null);
				this.Pos(num5 + thick_shiftx, num6 + thick_shifty, null);
				num2 += num3;
			}
			return this;
		}

		public MeshDrawer Daia(float x, float y, float w, float h, bool no_divide_ppu = false)
		{
			if (!no_divide_ppu && this.ppu != 1f)
			{
				x *= this.ppur;
				y *= this.ppur;
				w *= this.ppur;
				h *= this.ppur;
			}
			w *= 0.5f;
			h *= 0.5f;
			if (this.uv_settype == UV_SETTYPE.AUTO)
			{
				this.uvRect(x - w, y - h, w * 2f, h * 2f, false, false);
			}
			this.Tri(0, 1, 3, false).Tri(1, 2, 3, false);
			this.Pos(x - w, y, null).Pos(x, y + h, null).Pos(x + w, y, null)
				.Pos(x, y - h, null);
			return this;
		}

		public MeshDrawer Daia2(float x, float y, float w, float thick, bool no_divide_ppu = false)
		{
			return this.Poly(x, y, w, 0f, 4, thick, no_divide_ppu, 0f, 0f);
		}

		public MeshDrawer Daia3(float x, float y, float w, float h, float thickw = 0f, float thickh = 0f, bool no_divide_ppu = false)
		{
			if ((thickw <= 0f && thickh <= 0f) || thickw * 2f >= w || thickh * 2f >= h)
			{
				return this.Daia(x, y, w, h, no_divide_ppu);
			}
			if (!no_divide_ppu && this.ppu != 1f)
			{
				x *= this.ppur;
				y *= this.ppur;
				w *= this.ppur;
				h *= this.ppur;
				thickw *= this.ppur;
				thickh *= this.ppur;
			}
			w *= 0.5f;
			h *= 0.5f;
			if (this.uv_settype == UV_SETTYPE.AUTO)
			{
				this.uvRect(x - w, y - h, w * 2f, h * 2f, false, false);
			}
			this.Tri(0, 4, 1, false).Tri(4, 5, 1, false).Tri(5, 6, 1, false)
				.Tri(6, 2, 1, false)
				.Tri(3, 2, 6, false)
				.Tri(3, 6, 7, false)
				.Tri(3, 7, 4, false)
				.Tri(3, 4, 0, false);
			this.Pos(x, y - h, null).Pos(x + w, y, null).Pos(x, y + h, null)
				.Pos(x - w, y, null);
			w = X.Mx(0f, w - thickw);
			h = X.Mx(0f, h - thickh);
			this.Pos(x, y - h, null).Pos(x + w, y, null).Pos(x, y + h, null)
				.Pos(x - w, y, null);
			return this;
		}

		public MeshDrawer NelBanner(float x, float y, float w, float h, float thick, bool inner_fill = false, bool no_divide_ppu = false, float grd_level_out = 0f, float grd_level_in = 0f, float grd_level_center = 0f)
		{
			if (!no_divide_ppu && this.ppu != 1f)
			{
				x *= this.ppur;
				y *= this.ppur;
				w *= this.ppur;
				h *= this.ppur;
				thick *= this.ppur;
			}
			if (h > w)
			{
				Matrix4x4 matrixTransform = this.MatrixTransform;
				this.Rotate(3.1415927f, false).Translate(x, y, false);
				this.NelBanner(0f, 0f, h, w, thick, inner_fill, true, grd_level_out, grd_level_in, grd_level_center);
				this.MatrixTransform = matrixTransform;
				return this;
			}
			float num = w * 0.5f;
			float num2 = h * 0.5f;
			C32 c = null;
			C32 c2 = null;
			C32 c3 = null;
			if (grd_level_out > 0f)
			{
				c = ((grd_level_out == 1f) ? this.ColGrd : MeshDrawer.ColBuf0.Set(this.Col).blend(this.ColGrd, grd_level_out));
			}
			if (grd_level_in > 0f)
			{
				c2 = ((grd_level_in == 1f) ? this.ColGrd : MeshDrawer.ColBuf1.Set(this.Col).blend(this.ColGrd, grd_level_in));
			}
			if (grd_level_center > 0f)
			{
				c3 = ((grd_level_center == 1f) ? this.ColGrd : new C32().Set(this.Col).blend(this.ColGrd, grd_level_in));
			}
			bool flag = true;
			if (thick <= 0f)
			{
				inner_fill = true;
				flag = false;
			}
			bool flag2 = inner_fill && grd_level_in != grd_level_center;
			if (inner_fill)
			{
				if (flag2)
				{
					int num3 = 6 + (flag ? 6 : 0);
					this.Tri(0, num3, 1, false).Tri(5, num3, 0, false).Tri(4, num3, 5, false)
						.Tri(3, num3, 4, false)
						.Tri(2, num3, 3, false)
						.Tri(1, num3, 2, false);
				}
				else
				{
					this.Tri(0, 5, 1, false).Tri(5, 4, 1, false).Tri(1, 4, 2, false)
						.Tri(3, 2, 4, false);
				}
			}
			if (flag)
			{
				this.Tri(1, 7, 0, false).Tri(7, 6, 0, false).Tri(11, 0, 6, false)
					.Tri(5, 0, 11, false)
					.Tri(10, 4, 11, false)
					.Tri(4, 5, 11, false)
					.Tri(10, 3, 4, false)
					.Tri(3, 10, 9, false)
					.Tri(9, 8, 3, false)
					.Tri(3, 8, 2, false)
					.Tri(2, 8, 7, false)
					.Tri(2, 7, 1, false);
			}
			if (flag)
			{
				float num4 = num - thick;
				float num5 = num2 - thick;
				this.Pos(x + num4, y + 0f, c2).Pos(x + num4 - num5, y + num5, c2).Pos(x - num4 + num5, y + num5, c2)
					.Pos(x - num4, y + 0f, c2)
					.Pos(x - num4 + num5, y - num5, c2)
					.Pos(x + num4 - num5, y - num5, c2);
			}
			this.Pos(x + num, y + 0f, c).Pos(x + num - num2, y + num2, c).Pos(x - num + num2, y + num2, c)
				.Pos(x - num, y + 0f, c)
				.Pos(x - num + num2, y - num2, c)
				.Pos(x + num - num2, y - num2, c);
			if (flag2)
			{
				this.Pos(x, y, c3);
			}
			return this;
		}

		public MeshDrawer GT(float x, float y, float w, float h, float thick, bool no_divide_ppu = false, float grd_level_in = 0f, float grd_level_out = 0f)
		{
			if (!no_divide_ppu && this.ppu != 1f)
			{
				x *= this.ppur;
				y *= this.ppur;
				w *= this.ppur;
				h *= this.ppur;
				thick *= this.ppur;
			}
			C32 c = null;
			C32 c2 = null;
			if (grd_level_out > 0f)
			{
				c = ((grd_level_out == 1f) ? this.ColGrd : MeshDrawer.ColBuf0.Set(this.Col).blend(this.ColGrd, grd_level_out));
			}
			if (grd_level_in > 0f)
			{
				c2 = ((grd_level_in == 1f) ? this.ColGrd : MeshDrawer.ColBuf1.Set(this.Col).blend(this.ColGrd, grd_level_in));
			}
			if (w < 0f)
			{
				this.Tri(0, 4, 1, false).Tri(4, 0, 3, false).Tri(0, 5, 3, false)
					.Tri(2, 5, 0, false);
			}
			else
			{
				this.Tri(4, 0, 1, false).Tri(0, 4, 3, false).Tri(5, 0, 3, false)
					.Tri(5, 2, 0, false);
			}
			float num = h / 2f;
			thick = X.Abs(thick) * (float)((w >= 0f) ? 1 : (-1));
			this.Pos(x + w, y + 0f, c2).Pos(x + 0f, y + num, c2).Pos(x + 0f, y - num, c2);
			this.Pos(x + w + thick, y + 0f, c).Pos(x + thick, y + num, c).Pos(x + thick, y - num, c);
			return this;
		}

		public MeshDrawer CheckMark(float x, float y, float w, float thick, bool no_divide_ppu = false)
		{
			if (!no_divide_ppu && this.ppu != 1f)
			{
				x *= this.ppur;
				y *= this.ppur;
				w *= this.ppur;
				thick *= this.ppur;
			}
			float num = thick / 2f;
			float num2 = w / 2f;
			float num3 = w / 4f;
			this.Tri(4, 0, 1, false).Tri(4, 3, 0, false).Tri(3, 5, 0, false)
				.Tri(5, 2, 0, false);
			this.Pos(x - num2 + num3, y - num2, null).Pos(x - num2, y - num2 + num3, null).Pos(x + num2, y + num2 - num3, null);
			this.Pos(x - num2 + num3, y - num2 + thick, null).Pos(x - num2 + num, y - num2 + num3 + thick - num, null).Pos(x + num2 - num, y + num2 - num3 + thick - num, null);
			return this;
		}

		public MeshDrawer Drip(float x, float y, float w, float h, float thick, float grd_level_in = 0f, float grd_level_out = 0f)
		{
			if (thick >= w)
			{
				thick = 0f;
			}
			int num = this.ver_i;
			this.Arc2(x, y, w, w, -3.1415927f, 0f, thick, grd_level_in, grd_level_out);
			int num2 = num - this.ver_i;
			C32 c = null;
			C32 c2 = null;
			if (grd_level_out > 0f)
			{
				c = ((grd_level_out == 1f) ? this.ColGrd : MeshDrawer.ColBuf0.Set(this.Col).blend(this.ColGrd, grd_level_out));
			}
			if (thick <= 0f)
			{
				if (grd_level_in == grd_level_out)
				{
					this.Tri(num2 + 1, 0, -1, false);
				}
				else
				{
					this.Tri(num2 + 1, 0, num2, false).Tri(num2, 0, -1, false);
				}
				this.PosD(x, y + h, c);
			}
			else
			{
				if (grd_level_in > 0f)
				{
					c2 = ((grd_level_in == 1f) ? this.ColGrd : MeshDrawer.ColBuf1.Set(this.Col).blend(this.ColGrd, grd_level_in));
				}
				this.Tri(num2, 0, num2 + 1, false).Tri(num2 + 1, 0, 1, false);
				this.Tri(-1, -2, 0, false).Tri(-2, 1, 0, false);
				this.PosD(x, y + h, c);
				this.PosD(x, y + h - thick, c2);
			}
			return this;
		}

		public MeshDrawer BoxBL(float x, float y, float w, float h, float thick = 0f, bool no_divide_ppu = false)
		{
			if (thick <= 0f)
			{
				return this.RectBL(x, y, w, h, no_divide_ppu);
			}
			if (!no_divide_ppu && this.ppu != 1f)
			{
				x *= this.ppur;
				y *= this.ppur;
				w *= this.ppur;
				h *= this.ppur;
				thick *= this.ppur;
			}
			float num = y + h;
			float num2 = x + w;
			this.Tri(0, 1, 4, false).Tri(1, 5, 4, false).Tri(1, 2, 5, false)
				.Tri(5, 2, 6, false)
				.Tri(6, 2, 7, false)
				.Tri(7, 2, 3, false)
				.Tri(0, 4, 7, false)
				.Tri(0, 7, 3, false);
			this.Pos(x, y, null).Pos(x, num, null).Pos(num2, num, null)
				.Pos(num2, y, null);
			this.Pos(x + thick, y + thick, null).Pos(x + thick, num - thick, null).Pos(num2 - thick, num - thick, null)
				.Pos(num2 - thick, y + thick, null);
			return this;
		}

		public MeshDrawer Box(float x, float y, float w, float h, float thick = 0f, bool no_divide_ppu = false)
		{
			return this.BoxBL(x - w * 0.5f, y - h * 0.5f, w, h, thick, no_divide_ppu);
		}

		public MeshDrawer Rect(float x, float y, float w, float h, bool no_divide_ppu = false)
		{
			return this.RectBL(x - w * 0.5f, y - h * 0.5f, w, h, no_divide_ppu);
		}

		public MeshDrawer RectC(float x, float y, float w, float h, bool no_divide_ppu = false, float grd_level_in = 1f, float grd_level_out = 0f)
		{
			return this.RectCBL(x - w * 0.5f, y - h * 0.5f, w, h, no_divide_ppu, grd_level_in, grd_level_out);
		}

		public MeshDrawer RectCBL(float x, float y, float w, float h, bool no_divide_ppu = false, float grd_level_in = 1f, float grd_level_out = 0f)
		{
			if (!no_divide_ppu && this.ppu != 1f)
			{
				x *= this.ppur;
				y *= this.ppur;
				w *= this.ppur;
				h *= this.ppur;
			}
			if (this.uv_settype == UV_SETTYPE.AUTO)
			{
				this.uvRect(x, y, w, h, false, false);
			}
			this.Tri(1, 2, 0, false).Tri(0, 2, 3, false).Tri(0, 3, 4, false)
				.Tri(0, 4, 1, false);
			C32 c = null;
			C32 c2 = null;
			if (grd_level_in > 0f)
			{
				c = ((grd_level_in == 1f) ? this.ColGrd : MeshDrawer.ColBuf0.Set(this.Col).blend(this.ColGrd, grd_level_in));
			}
			if (grd_level_out > 0f)
			{
				c2 = ((grd_level_out == 1f) ? this.ColGrd : MeshDrawer.ColBuf1.Set(this.Col).blend(this.ColGrd, grd_level_out));
			}
			this.Pos(x + w * 0.5f, y + h * 0.5f, c);
			if (this.uv_settype == UV_SETTYPE.IMG)
			{
				this.AMeshUv[this.getVertexMax() - 1].Set(this.uv_left + this.uv_width * 0.5f, this.uv_top + this.uv_height * 0.5f);
			}
			float num = y + h;
			float num2 = x + w;
			this.Pos(x, y, c2).Pos(x, num, c2).Pos(num2, num, c2)
				.Pos(num2, y, c2)
				.InputImageUv();
			return this;
		}

		public MeshDrawer RectBL(float x, float y, float w, float h, bool no_divide_ppu = false)
		{
			if (!no_divide_ppu && this.ppu != 1f)
			{
				x *= this.ppur;
				y *= this.ppur;
				w *= this.ppur;
				h *= this.ppur;
			}
			if (this.uv_settype == UV_SETTYPE.AUTO)
			{
				this.uvRect(x, y, w, h, false, false);
			}
			this.TriRectBL(0);
			float num = y + h;
			float num2 = x + w;
			this.Pos(x, y, null).Pos(x, num, null).Pos(num2, num, null)
				.Pos(num2, y, null)
				.InputImageUv();
			return this;
		}

		public MeshDrawer InputImageUv()
		{
			if (this.uv_settype == UV_SETTYPE.IMG)
			{
				int num = this.getVertexMax() - 4;
				for (int i = 0; i < 4; i++)
				{
					this.AMeshUv[num++].Set((i == 0 || i == 1) ? this.uv_left : (this.uv_left + this.uv_width), (i == 0 || i == 3) ? this.uv_top : (this.uv_top + this.uv_height));
				}
			}
			return this;
		}

		public MeshDrawer InputImageUv(float cl, float ct, float cw = 1f, float ch = 1f)
		{
			if (this.uv_settype == UV_SETTYPE.IMG)
			{
				int num = this.getVertexMax() - 4;
				for (int i = 0; i < 4; i++)
				{
					this.AMeshUv[num++].Set((i == 0 || i == 1) ? (this.uv_left + this.uv_width * cl) : (this.uv_left + this.uv_width * (cl + cw)), (i == 0 || i == 3) ? (this.uv_top + this.uv_height * ct) : (this.uv_top + this.uv_height * (ct + ch)));
				}
			}
			return this;
		}

		public MeshDrawer Triangle(float x, float y, float x2, float y2, float x3, float y3, bool no_divide_ppu = false)
		{
			if (!no_divide_ppu && this.ppu != 1f)
			{
				x *= this.ppur;
				y *= this.ppur;
				x2 *= this.ppur;
				y2 *= this.ppur;
				x3 *= this.ppur;
				y3 *= this.ppur;
			}
			this.Tri(0, 1, 2, false);
			this.Pos(x, y, null).Pos(x2, y2, null).Pos(x3, y3, null);
			return this;
		}

		public MeshDrawer RectBLGradation(float x, float y, float w, float h, GRD grd, bool no_divide_ppu = false)
		{
			if (!no_divide_ppu && this.ppu != 1f)
			{
				x *= this.ppur;
				y *= this.ppur;
				w *= this.ppur;
				h *= this.ppur;
			}
			if (this.uv_settype == UV_SETTYPE.AUTO)
			{
				this.uvRect(x, y, w, h, false, false);
			}
			this.Tri(0, 1, 2, false).Tri(0, 2, 3, false);
			float num = y + h;
			float num2 = x + w;
			if (grd == GRD.LEFT2RIGHT)
			{
				this.Pos(x, y, null).Pos(x, num, null).Pos(num2, num, this.ColGrd)
					.Pos(num2, y, this.ColGrd);
			}
			else if (grd == GRD.RIGHT2LEFT)
			{
				this.Pos(x, y, this.ColGrd).Pos(x, num, this.ColGrd).Pos(num2, num, null)
					.Pos(num2, y, null);
			}
			else if (grd == GRD.TOP2BOTTOM)
			{
				this.Pos(x, y, this.ColGrd).Pos(x, num, null).Pos(num2, num, null)
					.Pos(num2, y, this.ColGrd);
			}
			else if (grd == GRD.BOTTOM2TOP)
			{
				this.Pos(x, y, null).Pos(x, num, this.ColGrd).Pos(num2, num, this.ColGrd)
					.Pos(num2, y, null);
			}
			this.InputImageUv();
			return this;
		}

		public MeshDrawer RectGradation(float x, float y, float w, float h, GRD grd, bool no_divide_ppu = false)
		{
			return this.RectBLGradation(x - w * 0.5f, y - h * 0.5f, w, h, grd, no_divide_ppu);
		}

		public MeshDrawer RectDoughnut(float x, float y, float w, float h, float ix, float iy, float iw, float ih, bool no_divide_ppu = false, float grd_level_out = 0f, float grd_level_in = 0f, bool fill_inner = false)
		{
			if ((iw == 0f || ih == 0f) && grd_level_in == grd_level_out)
			{
				return this.RectBL(x - w * 0.5f, y - h * 0.5f, w, h, no_divide_ppu);
			}
			if (!no_divide_ppu && this.ppu != 1f)
			{
				x *= this.ppur;
				y *= this.ppur;
				w *= this.ppur;
				h *= this.ppur;
				ix *= this.ppur;
				iy *= this.ppur;
				iw *= this.ppur;
				ih *= this.ppur;
			}
			iw = ((iw < 0f) ? (-iw) : iw);
			ih = ((ih < 0f) ? (-ih) : ih);
			x -= w / 2f;
			y -= h / 2f;
			ix -= iw / 2f;
			iy -= ih / 2f;
			C32 c = null;
			C32 c2 = null;
			if (grd_level_out > 0f)
			{
				c = ((grd_level_out == 1f) ? this.ColGrd : MeshDrawer.ColBuf0.Set(this.Col).blend(this.ColGrd, grd_level_out));
			}
			if (grd_level_in > 0f)
			{
				c2 = ((grd_level_in == 1f) ? this.ColGrd : MeshDrawer.ColBuf1.Set(this.Col).blend(this.ColGrd, grd_level_in));
			}
			if (iw == 0f || ih == 0f)
			{
				this.Tri(0, 1, 4, false).Tri(1, 2, 4, false).Tri(2, 3, 4, false)
					.Tri(3, 0, 4, false);
				this.Pos(x, y, c).Pos(x, y + h, c).Pos(x + w, y + h, c)
					.Pos(x + w, y, c);
				this.Pos(ix, iy, c2);
			}
			else
			{
				this.Tri(1, 5, 0, false).Tri(1, 2, 5, false).Tri(2, 6, 5, false)
					.Tri(2, 3, 6, false);
				this.Tri(6, 3, 7, false).Tri(7, 3, 0, false).Tri(4, 7, 0, false)
					.Tri(5, 4, 0, false);
				if (fill_inner)
				{
					this.Tri(4, 5, 6, false).Tri(4, 6, 7, false);
				}
				this.Pos(x, y, c).Pos(x, y + h, c).Pos(x + w, y + h, c)
					.Pos(x + w, y, c);
				this.Pos(ix, iy, c2).Pos(ix, iy + ih, c2).Pos(ix + iw, iy + ih, c2)
					.Pos(ix + iw, iy, c2);
			}
			return this;
		}

		public MeshDrawer LineDif(float x, float y, float deltax, float deltay, float thick = 1f, bool no_divide_ppu = false, float grd_level0 = 0f, float grd_level1 = 0f)
		{
			return this.Line(x, y, x + deltax, y + deltay, thick, no_divide_ppu, grd_level0, grd_level1);
		}

		public MeshDrawer Line(float x, float y, float dx, float dy, float thick = 1f, bool no_divide_ppu = false, float grd_level0 = 0f, float grd_level1 = 0f)
		{
			if (!no_divide_ppu && this.ppu != 1f)
			{
				x *= this.ppur;
				y *= this.ppur;
				dx *= this.ppur;
				dy *= this.ppur;
				thick *= this.ppur;
			}
			thick *= 0.5f;
			C32 c = null;
			C32 c2 = null;
			if (grd_level0 > 0f)
			{
				c = ((grd_level0 == 1f) ? this.ColGrd : MeshDrawer.ColBuf0.Set(this.Col).blend(this.ColGrd, grd_level0));
			}
			if (grd_level1 > 0f)
			{
				c2 = ((grd_level1 == 1f) ? this.ColGrd : MeshDrawer.ColBuf1.Set(this.Col).blend(this.ColGrd, grd_level1));
			}
			float num;
			if (dx == x)
			{
				num = ((y < dy) ? 1.5707964f : (-1.5707964f)) + 1.5707964f;
			}
			else if (dy == y)
			{
				num = ((x < dx) ? 0f : (-3.1415927f)) + 1.5707964f;
			}
			else
			{
				num = X.GAR2(x, y, dx, dy) + 1.5707964f;
			}
			float num2 = thick * X.Cos(num);
			float num3 = thick * X.Sin(num);
			this.Tri(0, 2, 1, false).Tri(0, 3, 2, false);
			this.Pos(x - num2, y - num3, c).Pos(dx - num2, dy - num3, c2).Pos(dx + num2, dy + num3, c2)
				.Pos(x + num2, y + num3, c);
			return this.InputImageUv();
		}

		public MeshDrawer MathLine(float x, float y, float dx, float dy, float thick, MeshDrawer.FnMathLineHeight Fn, float resolution = 2f, bool no_divide_ppu = false, float grd_level0 = 0f, float grd_level1 = 0f)
		{
			if (!no_divide_ppu && this.ppu != 1f)
			{
				x *= this.ppur;
				y *= this.ppur;
				dx *= this.ppur;
				dy *= this.ppur;
				thick *= this.ppur;
			}
			thick *= 0.5f;
			C32 c = null;
			if (grd_level0 > 0f)
			{
				c = ((grd_level0 == 1f) ? this.ColGrd : MeshDrawer.ColBuf0.Set(this.Col).blend(this.ColGrd, grd_level0));
			}
			if (grd_level1 > 0f)
			{
				if (grd_level1 != 1f)
				{
					MeshDrawer.ColBuf1.Set(this.Col).blend(this.ColGrd, grd_level1);
				}
				else
				{
					C32 colGrd = this.ColGrd;
				}
			}
			float num;
			if (dx == x)
			{
				num = X.Abs(dy - y);
			}
			else if (dy == y)
			{
				num = X.Abs(dx - x);
			}
			else
			{
				num = X.LENGTHXY(x, y, dx, dy);
			}
			int num2 = X.IntC(num * this.ppu / resolution);
			float num3 = 1f / (float)num2;
			float num4 = 0f;
			float num5 = num * num3;
			float num6;
			float num7;
			float num8;
			float num9;
			if (dx == x)
			{
				num6 = (float)X.MPF(y < dy) * num5;
				num7 = 0f;
				num8 = 0f;
				num9 = (float)(-(float)X.MPF(y < dy));
			}
			else if (dy == y)
			{
				num6 = 0f;
				num7 = (float)X.MPF(x < dx) * num5;
				num8 = (float)X.MPF(x < dx);
				num9 = 0f;
			}
			else
			{
				float num10 = X.GAR2(x, y, dx, dy);
				num9 = X.Cos(num10 + 1.5707964f);
				num8 = X.Sin(num10 + 1.5707964f);
				num7 = num5 * X.Cos(num10);
				num6 = num5 * X.Sin(num10);
			}
			float num11 = num9 * thick;
			float num12 = num8 * thick;
			this.Pos(x - num11, y - num12, c).Pos(x + num11, y + num12, c);
			float num14;
			float num15;
			for (int i = 1; i < num2; i++)
			{
				this.Tri(-2, -1, 1, false).Tri(-2, 1, 0, false);
				x += num7;
				y += num6;
				float num13 = Fn(num4 += num3);
				num14 = num13 * this.ppur * num9;
				num15 = num13 * this.ppur * num8;
				this.Pos(x - num11 + num14, y - num12 - num15, c).Pos(x + num11 + num14, y + num12 + num15, c);
			}
			this.Tri(-2, -1, 1, false).Tri(-2, 1, 0, false);
			float num16 = Fn(1f);
			num14 = num16 * num9;
			num15 = num16 * num8;
			this.Pos(dx - num11 + num14, dy - num12 - num14, c).Pos(dx + num11 + num14, dy + num12 + num15, c);
			return this;
		}

		public MeshDrawer BoxPattern(float x, float y, float w, float h, float basepos_x, float basepos_y, float uv_scale_x, float uv_scale_y, PxlImage Img, bool no_divide_ppu = false)
		{
			return this.BoxPatternBL(x - w / 2f, y - h / 2f, w, h, basepos_x, basepos_y, uv_scale_x, uv_scale_y, Img, no_divide_ppu);
		}

		public MeshDrawer BoxPatternBL(float x, float y, float w, float h, float basepos_x, float basepos_y, float uv_scale_x, float uv_scale_y, PxlImage Img, bool no_divide_ppu = false)
		{
			if (no_divide_ppu)
			{
				x *= 64f;
				y *= 64f;
				w *= 64f;
				h *= 64f;
			}
			this.LoopTextureBL(x, y, w, h, Img, basepos_x, basepos_y, uv_scale_x);
			return this;
		}

		public MeshDrawer Poly(float x, float y, float r, float dagR, int kaku, float thick = 0f, bool no_divide_ppu = false, float grd_level_out = 0f, float grd_level_in = 0f)
		{
			if (!no_divide_ppu && this.ppu != 1f)
			{
				x *= this.ppur;
				y *= this.ppur;
				r *= this.ppur;
				thick *= this.ppur;
			}
			MeshDrawer.FnMeshPointColor fnMeshPointColor = this.fnMeshPointColor;
			this.fnMeshPointColor = null;
			if (this.uv_settype == UV_SETTYPE.AUTO)
			{
				this.uvRect(x - r - thick, y - r - thick, (r + thick) * 2f, (r + thick) * 2f, false, false);
			}
			C32 c = null;
			C32 c2 = null;
			if (grd_level_out > 0f)
			{
				c = ((grd_level_out == 1f) ? this.ColGrd : MeshDrawer.ColBuf0.Set(this.Col).blend(this.ColGrd, grd_level_out));
			}
			if (grd_level_in > 0f)
			{
				c2 = ((grd_level_in == 1f) ? this.ColGrd : MeshDrawer.ColBuf1.Set(this.Col).blend(this.ColGrd, grd_level_in));
			}
			float num;
			if (kaku <= 4 && thick <= 0f && grd_level_out == grd_level_in)
			{
				if (kaku == 4)
				{
					this.Tri(0, 2, 1, false).Tri(0, 3, 2, false);
					num = 1.5707964f;
				}
				else
				{
					kaku = 3;
					this.Tri012();
					num = -2.0943952f;
				}
				for (int i = 0; i < kaku; i++)
				{
					float num2 = x + r * X.Cos(dagR);
					float num3 = y + r * X.Sin(dagR);
					this.Pos(num2, num3, (fnMeshPointColor != null) ? fnMeshPointColor(this, num2, num3, c, r, dagR) : c);
					dagR += num;
				}
				this.fnMeshPointColor = fnMeshPointColor;
				return this;
			}
			num = 6.2831855f / (float)kaku;
			if (thick <= 0f || thick >= r)
			{
				for (int j = 1; j < kaku; j++)
				{
					this.Tri(0, j + 1, j, false);
				}
				this.Tri(0, 1, kaku, false);
				this.Pos(x, y, (fnMeshPointColor != null) ? fnMeshPointColor(this, x, y, c2, 0f, 0f) : c2);
				for (int k = 0; k < kaku; k++)
				{
					float num2 = x + r * X.Cos(dagR);
					float num3 = y + r * X.Sin(dagR);
					this.Pos(num2, num3, (fnMeshPointColor != null) ? fnMeshPointColor(this, x, y, c, r, dagR) : c);
					dagR += num;
				}
			}
			else
			{
				int num4 = kaku * 2;
				r += thick * 0.5f;
				float num5 = r - thick;
				for (int l = 0; l < num4; l += 2)
				{
					int num6 = (l + 2) % num4;
					int num7 = (l + 3) % num4;
					this.Tri(l, l + 1, num6, false);
					this.Tri(l + 1, num7, num6, false);
				}
				for (int m = 0; m < kaku; m++)
				{
					float num8 = X.Cos(dagR);
					float num9 = X.Sin(dagR);
					float num2 = x + r * num8;
					float num3 = y + r * num9;
					this.Pos(num2, num3, (fnMeshPointColor != null) ? fnMeshPointColor(this, x, y, c, r, dagR) : c);
					num2 = x + num5 * num8;
					num3 = y + num5 * num9;
					this.Pos(num2, num3, (fnMeshPointColor != null) ? fnMeshPointColor(this, x, y, c2, num5, dagR) : c2);
					dagR += num;
				}
			}
			this.fnMeshPointColor = fnMeshPointColor;
			return this;
		}

		public MeshDrawer Star(float x, float y, float r, float dagR, int kaku, float dent = 0.5f, float thick = 0f, bool no_divide_ppu = false, float grd_level_out = 0f, float grd_level_in = 0f)
		{
			if (dent == 1f)
			{
				return this.Poly(x, y, r, dagR, kaku, thick, no_divide_ppu, grd_level_out, grd_level_in);
			}
			if (!no_divide_ppu && this.ppu != 1f)
			{
				x *= this.ppur;
				y *= this.ppur;
				r *= this.ppur;
				thick *= this.ppur;
			}
			if (this.uv_settype == UV_SETTYPE.AUTO)
			{
				this.uvRect(x - r - thick, y - r - thick, (r + thick) * 2f, (r + thick) * 2f, false, false);
			}
			kaku *= 2;
			float num = 6.2831855f / (float)kaku;
			C32 c = null;
			C32 c2 = null;
			if (grd_level_out > 0f)
			{
				c = ((grd_level_out == 1f) ? this.ColGrd : MeshDrawer.ColBuf0.Set(this.Col).blend(this.ColGrd, grd_level_out));
			}
			if (grd_level_in > 0f)
			{
				c2 = ((grd_level_in == 1f) ? this.ColGrd : MeshDrawer.ColBuf1.Set(this.Col).blend(this.ColGrd, grd_level_in));
			}
			if (thick <= 0f || thick >= r)
			{
				float num2 = r * dent;
				for (int i = 1; i < kaku; i++)
				{
					this.Tri(0, i + 1, i, false);
				}
				this.Tri(0, 1, kaku, false);
				this.Pos(x, y, c2);
				for (int j = 0; j < kaku; j++)
				{
					float num3 = ((j % 2 == 1) ? num2 : r);
					this.Pos(x + num3 * X.Cos(dagR), y + num3 * X.Sin(dagR), c);
					dagR += num;
				}
			}
			else
			{
				int num4 = kaku * 2;
				r += thick * 0.5f;
				float num5 = r * dent;
				float num6 = r - thick;
				float num7 = num6 * dent;
				for (int k = 0; k < num4; k += 2)
				{
					int num8 = (k + 2) % num4;
					int num9 = (k + 3) % num4;
					this.Tri(k, k + 1, num8, false);
					this.Tri(k + 1, num9, num8, false);
				}
				for (int l = 0; l < kaku; l++)
				{
					float num10;
					float num11;
					if (l % 2 == 1)
					{
						num10 = r;
						num11 = num6;
					}
					else
					{
						num10 = num5;
						num11 = num7;
					}
					this.Pos(x + num10 * X.Cos(dagR), y + num10 * X.Sin(dagR), c);
					this.Pos(x + num11 * X.Cos(dagR), y + num11 * X.Sin(dagR), c2);
					dagR += num;
				}
			}
			return this;
		}

		public MeshDrawer Circle(float x, float y, float r, float thick = 0f, bool no_divide_ppu = false, float grd_level_out = 0f, float grd_level_in = 0f)
		{
			return this.Poly(x, y, r, 0f, X.MMX(6, X.IntC(r * (no_divide_ppu ? this.ppu : 1f) * 2f * 3.1415927f / 10f), 32), thick, no_divide_ppu, grd_level_out, grd_level_in);
		}

		public MeshDrawer ArcLen(float x, float y, float r, float sagR, float dlen, float thick = 0f)
		{
			float num = r * 2f * 3.1415927f;
			return this.Arc(x, y, r, sagR, sagR + dlen / num * 6.2831855f, thick);
		}

		public MeshDrawer Arc(float x, float y, float r, float sagR, float dagR, float thick = 0f)
		{
			return this.Arc2(x, y, r, r, sagR, dagR, thick, 0f, 0f);
		}

		public MeshDrawer Arc2(float x, float y, float rw, float rh, float sagR, float dagR, float thick = 0f, float grd_level_out = 0f, float grd_level_in = 0f)
		{
			if (dagR < sagR)
			{
				float num = sagR;
				float num2 = dagR;
				dagR = num;
				sagR = num2;
			}
			float num3 = dagR - sagR;
			if (num3 >= 6.2831855f)
			{
				return this.Circle(x, y, rw, thick, false, 0f, 0f);
			}
			if (thick > 0f && (rw <= thick || rh <= thick))
			{
				return this.Arc2(x, y, rw, rh, sagR, dagR, 0f, grd_level_out, grd_level_in);
			}
			if (this.ppu != 1f)
			{
				x *= this.ppur;
				y *= this.ppur;
				rw *= this.ppur;
				rh *= this.ppur;
				thick *= this.ppur;
			}
			int num4 = X.MMX(3, X.IntC(num3 * X.Mx(rw, rh) * 2f / (9f * this.ppur)) + 1, 20);
			float num5 = num3 / (float)num4;
			C32 c = null;
			C32 c2 = null;
			if (grd_level_out > 0f)
			{
				c = ((grd_level_out == 1f) ? this.ColGrd : MeshDrawer.ColBuf0.Set(this.Col).blend(this.ColGrd, grd_level_out));
			}
			if (grd_level_in > 0f)
			{
				c2 = ((grd_level_in == 1f) ? this.ColGrd : MeshDrawer.ColBuf1.Set(this.Col).blend(this.ColGrd, grd_level_in));
			}
			if (thick <= 0f)
			{
				for (int i = 1; i <= num4; i++)
				{
					this.Tri(0, i + 1, i, false);
				}
				this.Pos(x, y, c2);
				for (int j = 0; j < num4; j++)
				{
					this.Pos(x + rw * X.Cos(sagR), y + rh * X.Sin(sagR), c);
					sagR += num5;
				}
				this.Pos(x + rw * X.Cos(dagR), y + rh * X.Sin(dagR), c);
			}
			else
			{
				float num6 = rw - thick;
				float num7 = rh - thick;
				int num8 = 0;
				for (int k = 0; k < num4; k++)
				{
					this.Tri(num8, num8 + 1, num8 + 2, false).Tri(num8 + 1, num8 + 3, num8 + 2, false);
					num8 += 2;
				}
				for (int l = 0; l <= num4; l++)
				{
					float num9 = X.Cos(sagR);
					float num10 = X.Sin(sagR);
					this.Pos(x + rw * num9, y + rh * num10, c);
					this.Pos(x + num6 * num9, y + num7 * num10, c2);
					sagR += num5;
				}
			}
			return this;
		}

		public MeshDrawer CircleB(float x, float y, float r, float in_x, float in_y, float in_r, float grd_level_out = 0f, float grd_level_in = 0f)
		{
			int num = X.MMX(6, X.IntC(r * 2f * 3.1415927f / 10f), 32);
			if (in_r <= 0f)
			{
				return this.Poly(x, y, r, 0f, num, 0f, false, 0f, 0f);
			}
			if (in_r >= 1f)
			{
				return this;
			}
			if (this.uv_settype == UV_SETTYPE.AUTO)
			{
				this.uvRect(x - r, y - r, r * 2f, r * 2f, false, false);
			}
			if (this.ppu != 1f)
			{
				x *= this.ppur;
				y *= this.ppur;
				in_x *= this.ppur;
				in_y *= this.ppur;
				r *= this.ppur;
			}
			C32 c = null;
			C32 c2 = null;
			if (grd_level_out > 0f)
			{
				c = ((grd_level_out == 1f) ? this.ColGrd : MeshDrawer.ColBuf0.Set(this.Col).blend(this.ColGrd, grd_level_out));
			}
			if (grd_level_in > 0f)
			{
				c2 = ((grd_level_in == 1f) ? this.ColGrd : MeshDrawer.ColBuf1.Set(this.Col).blend(this.ColGrd, grd_level_in));
			}
			float num2 = 6.2831855f / (float)num;
			float num3 = 0f;
			float num4 = num2 * 0.5f;
			int num5 = num * 2;
			for (int i = 0; i < num5; i += 2)
			{
				int num6 = (i + 2) % num5;
				int num7 = (i + 3) % num5;
				this.Tri(i, i + 1, num6, false);
				this.Tri(i + 1, num7, num6, false);
			}
			for (int j = 0; j < num; j++)
			{
				this.Pos(x + r * X.Cos(num3), y + r * X.Sin(num3), c);
				this.Pos(X.NAIBUN_I(x + in_x, x + r * X.Cos(num4), in_r), X.NAIBUN_I(y + in_y, y + r * X.Sin(num4), in_r), c2);
				num3 += num2;
				num4 += num2;
			}
			return this;
		}

		public MeshDrawer BlurLineW(float x, float y, float dx, float dy, float thick, float blur_thick = 0f, int tale = 3, float tale_len = 20f, float center_s_grd_lvl = 0f, float center_d_grd_lvl = 1f, C32 BlurCenterCol = null, C32 BlurOutColS = null, C32 BlurOutColD = null)
		{
			this.Line(x, y, dx, dy, thick, false, center_s_grd_lvl, center_d_grd_lvl);
			if (blur_thick > 0f)
			{
				blur_thick *= 0.5f;
				Color32 col = this.Col;
				if (BlurCenterCol != null)
				{
					this.Col = BlurCenterCol.C;
				}
				this.BlurLine3(x, y, dx, dy, blur_thick, blur_thick, blur_thick, blur_thick, BlurOutColS, BlurOutColS, BlurOutColD, BlurOutColD, tale, tale_len, false);
				if (BlurCenterCol != null)
				{
					this.Col = col;
				}
			}
			return this;
		}

		public MeshDrawer BlurLine(float x, float y, float dx, float dy, float blur_thick = 0f, int tale = 3, float tale_len = 20f, bool no_divide_ppu = false)
		{
			if (blur_thick == 0f)
			{
				return this.Line(x, y, dx, dy, 1f, false, 0f, 0f);
			}
			blur_thick *= 0.5f;
			return this.BlurLine3(x, y, dx, dy, blur_thick, blur_thick, blur_thick, blur_thick, this.ColGrd, this.ColGrd, this.ColGrd, this.ColGrd, tale, tale_len, no_divide_ppu);
		}

		public MeshDrawer BlurLine2(float x, float y, float dx, float dy, float in_thick, float out_thick, C32 InCol = null, C32 OutCol = null, int tale = 3, float tale_len = 20f)
		{
			in_thick *= 0.5f;
			out_thick *= 0.5f;
			return this.BlurLine3(x, y, dx, dy, in_thick, out_thick, in_thick, out_thick, InCol, OutCol, InCol, OutCol, tale, tale_len, false);
		}

		public MeshDrawer BlurLine3(float x, float y, float dx, float dy, float in_thick_s, float out_thick_s, float in_thick_d, float out_thick_d, C32 InColS = null, C32 OutColS = null, C32 InColD = null, C32 OutColD = null, int tale = 3, float tale_len = 20f, bool no_divide_ppu = false)
		{
			if (x == dx && dy == y)
			{
				return this;
			}
			if (x == dx && dy == y)
			{
				return this;
			}
			int num = 0;
			if (this.ppu != 1f && !no_divide_ppu)
			{
				x *= this.ppur;
				y *= this.ppur;
				dx *= this.ppur;
				dy *= this.ppur;
				in_thick_s *= this.ppur;
				out_thick_s *= this.ppur;
				in_thick_d *= this.ppur;
				out_thick_d *= this.ppur;
				tale_len *= this.ppur;
			}
			if (OutColS == null)
			{
				OutColS = this.ColGrd;
			}
			if (OutColD == null)
			{
				OutColD = this.ColGrd;
			}
			if (tale_len <= 0f)
			{
				tale = 0;
			}
			if ((tale & 1) > 0)
			{
				this.Tri(0, 6, 1, false).Tri(0, 2, 6, false);
				num++;
			}
			if ((tale & 2) > 0)
			{
				this.Tri(3, 4, 6 + num, false).Tri(5, 3, 6 + num, false);
			}
			this.Tri(0, 1, 3, false).Tri(3, 1, 4, false).Tri(3, 2, 0, false)
				.Tri(3, 5, 2, false);
			float num2;
			float num3;
			if (dx == x)
			{
				num2 = (float)X.MPF(y < dy);
				num3 = 0f;
			}
			else if (dy == y)
			{
				num2 = 0f;
				num3 = (float)X.MPF(x < dx);
			}
			else
			{
				float num4 = X.GAR2(x, y, dx, dy);
				num3 = X.Cos(num4);
				num2 = X.Sin(num4);
			}
			float num5 = -num2;
			float num6 = num3;
			this.Pos(x, y, null).Pos(x + num5 * out_thick_s, y + num6 * out_thick_s, OutColS).Pos(x - num5 * in_thick_s, y - num6 * in_thick_s, InColS);
			this.Pos(dx, dy, null).Pos(dx + num5 * out_thick_d, dy + num6 * out_thick_d, OutColD).Pos(dx - num5 * in_thick_d, dy - num6 * in_thick_d, InColD);
			if ((tale & 1) > 0)
			{
				this.Pos(x - num3 * tale_len, y - num2 * tale_len, OutColS);
			}
			if ((tale & 2) > 0)
			{
				this.Pos(dx + num3 * tale_len, dy + num2 * tale_len, OutColD);
			}
			return this;
		}

		public MeshDrawer BluredBanner(float x, float y, float dx, float dy, float in_thick, float out_thick, C32 OutCol = null, bool no_divide_ppu = false)
		{
			if (!no_divide_ppu && this.ppu != 1f)
			{
				x *= this.ppur;
				y *= this.ppur;
				dx *= this.ppur;
				dy *= this.ppur;
				in_thick *= this.ppur;
				out_thick *= this.ppur;
			}
			in_thick *= 0.5f;
			out_thick *= 0.5f;
			float num;
			float num2;
			if (dx == x)
			{
				num = (float)X.MPF(y < dy);
				num2 = 0f;
			}
			else if (dy == y)
			{
				num = 0f;
				num2 = (float)X.MPF(x < dx);
			}
			else
			{
				float num3 = X.GAR2(x, y, dx, dy);
				num2 = X.Cos(num3);
				num = X.Sin(num3);
			}
			float num4 = -num;
			float num5 = num2;
			this.Tri(0, 4, 6, false).Tri(0, 6, 2, false).Tri(1, 4, 0, false)
				.Tri(1, 5, 4, false)
				.Tri(2, 6, 7, false)
				.Tri(2, 7, 3, false);
			this.Pos(x + num4 * in_thick, y + num5 * in_thick, null).Pos(x + num4 * out_thick, y + num5 * out_thick, OutCol).Pos(x - num4 * in_thick, y - num5 * in_thick, null)
				.Pos(x - num4 * out_thick, y - num5 * out_thick, OutCol);
			this.Pos(dx + num4 * in_thick, dy + num5 * in_thick, null).Pos(dx + num4 * out_thick, dy + num5 * out_thick, OutCol).Pos(dx - num4 * in_thick, dy - num5 * in_thick, null)
				.Pos(dx - num4 * out_thick, dy - num5 * out_thick, OutCol);
			return this;
		}

		public MeshDrawer BlurArc(float x, float y, float r, float sagR, float dagR, float blur_thick = 0f, int tale = 3)
		{
			if (blur_thick == 0f)
			{
				return this.Arc(x, y, r, sagR, dagR, 1f);
			}
			return this.BlurArc2(x, y, r, sagR, dagR, blur_thick, blur_thick, this.ColGrd, this.ColGrd, tale, 0.22439948f);
		}

		public MeshDrawer BlurArc2(float x, float y, float r, float sagR, float dagR, float in_thick, float out_thick, C32 InCol = null, C32 OutCol = null, int tale = 3, float taleagR = -1000f)
		{
			if (dagR < sagR)
			{
				float num = sagR;
				float num2 = dagR;
				dagR = num;
				sagR = num2;
			}
			float num3 = dagR - sagR;
			if (num3 >= 6.2831855f)
			{
				return this.BlurCircle2(x, y, r, in_thick, out_thick, InCol, OutCol);
			}
			if (this.ppu != 1f)
			{
				x *= this.ppur;
				y *= this.ppur;
				r *= this.ppur;
				in_thick *= this.ppur;
				out_thick *= this.ppur;
			}
			if (InCol == null)
			{
				InCol = this.ColGrd;
			}
			if (OutCol == null)
			{
				OutCol = this.ColGrd;
			}
			if (taleagR == -1000f)
			{
				taleagR = 0.22439948f;
			}
			if (taleagR < 0f)
			{
				tale = 0;
			}
			int num4 = X.IntC(X.Abs(num3) * r * this.ppu * 2f / 6f) + 2;
			int num5 = num4 - 1;
			float num6 = num3 / (float)num5;
			float num7 = X.Mx(0f, r - in_thick);
			float num8 = r + out_thick;
			int num9 = 0;
			if ((tale & 1) > 0)
			{
				this.Tri(0, 2, 3, false).Tri(2, 0, 4, false);
			}
			else
			{
				num9--;
			}
			if ((tale & 2) > 0)
			{
				int num10 = num5 * 3 + num9;
				this.Tri(2 + num10, 1, 3 + num10, false).Tri(1, 2 + num10, 4 + num10, false);
			}
			else
			{
				num9--;
			}
			for (int i = 0; i < num5; i++)
			{
				int num11 = i * 3 + num9;
				this.Tri(2 + num11, 5 + num11, 3 + num11, false).Tri(6 + num11, 3 + num11, 5 + num11, false).Tri(5 + num11, 2 + num11, 4 + num11, false)
					.Tri(5 + num11, 4 + num11, 7 + num11, false);
			}
			float num12 = (dagR + sagR) / 2f;
			float num13 = ((tale == 0) ? 1f : 1.1f);
			float num14 = num12 - num3 * 0.5f * num13;
			float num15 = num6 * num13;
			float num16 = num14 - taleagR;
			float num17 = num12 + (num12 - num16);
			if ((tale & 1) > 0)
			{
				this.Pos(x + r * X.Cos(num16), y + r * X.Sin(num16), InCol);
			}
			if ((tale & 2) > 0)
			{
				this.Pos(x + r * X.Cos(num17), y + r * X.Sin(num17), InCol);
			}
			for (int j = 0; j < num4; j++)
			{
				this.Pos(x + r * X.Cos(sagR), y + r * X.Sin(sagR), null);
				this.Pos(x + num8 * X.Cos(num14), y + num8 * X.Sin(num14), OutCol);
				this.Pos(x + num7 * X.Cos(num14), y + num7 * X.Sin(num14), InCol);
				sagR += num6;
				num14 += num15;
			}
			return this;
		}

		public MeshDrawer BlurCircle(float x, float y, float r, float blur_thick = 0f)
		{
			if (blur_thick == 0f)
			{
				return this.Circle(x, y, r, 1f, false, 0f, 0f);
			}
			return this.BlurCircle2(x, y, r, blur_thick, blur_thick, this.ColGrd, this.ColGrd);
		}

		public MeshDrawer BlurCircle2(float x, float y, float r, float in_thick, float out_thick, C32 InCol = null, C32 OutCol = null)
		{
			return this.BlurPoly2(x, y, r, 0f, X.Mn(X.IntC(3.1415927f * X.Mx(out_thick, r) / 16f) + 2, 28), in_thick, out_thick, InCol, OutCol);
		}

		public MeshDrawer BlurCircle2R(float x, float y, float r, float resolution, float in_thick, float out_thick, C32 InCol, C32 OutCol)
		{
			return this.BlurPoly2(x, y, r, 0f, X.Mn(X.IntC(3.1415927f * X.Mx(out_thick, r) / resolution) + 2, 28), in_thick, out_thick, InCol, OutCol);
		}

		public MeshDrawer BlurPoly(float x, float y, float r, float dagR, int kaku, float blur_thick)
		{
			if (blur_thick == 0f)
			{
				return this.Poly(x, y, r, dagR, kaku, 1f, false, 0f, 0f);
			}
			return this.BlurPoly2(x, y, r, dagR, kaku, blur_thick, blur_thick, this.ColGrd, this.ColGrd);
		}

		public MeshDrawer BlurPoly2(float x, float y, float r, float sagR, int kaku, float in_thick, float out_thick, C32 InCol = null, C32 OutCol = null)
		{
			if (this.ppu != 1f)
			{
				x *= this.ppur;
				y *= this.ppur;
				r *= this.ppur;
				in_thick *= this.ppur;
				out_thick *= this.ppur;
			}
			MeshDrawer.FnMeshPointColor fnMeshPointColor = this.fnMeshPointColor;
			this.fnMeshPointColor = null;
			if (InCol == null)
			{
				InCol = this.ColGrd;
			}
			if (OutCol == null)
			{
				OutCol = this.ColGrd;
			}
			kaku = X.Mx(kaku, 3);
			float num = 6.2831855f / (float)kaku;
			float num2 = X.Mx(0f, r - in_thick);
			float num3 = r + out_thick;
			if (num2 == 0f)
			{
				for (int i = 0; i < kaku; i++)
				{
					int num4 = i * 2;
					int num5 = (i + 1) % kaku * 2;
					this.Tri(0, num5 + 1, num4 + 1, false).Tri(num5 + 1, num5 + 2, num4 + 2, false).Tri(num5 + 1, num4 + 2, num4 + 1, false);
				}
				this.Pos(x, y, (fnMeshPointColor != null) ? fnMeshPointColor(this, x, y, InCol, num2, 0f) : InCol);
			}
			else
			{
				for (int j = 0; j < kaku; j++)
				{
					int num6 = j * 3;
					int num7 = (j + 1) % kaku * 3;
					this.Tri(num6, num7, num6 + 1, false).Tri(num7, num7 + 1, num6 + 1, false).Tri(num7, num6, num6 + 2, false)
						.Tri(num7, num6 + 2, num7 + 2, false);
				}
			}
			for (int k = 0; k < kaku; k++)
			{
				float num8 = x + r * X.Cos(sagR);
				float num9 = y + r * X.Sin(sagR);
				this.Pos(num8, num9, (fnMeshPointColor != null) ? fnMeshPointColor(this, num8, num9, null, r, sagR) : null);
				num8 = x + num3 * X.Cos(sagR);
				num9 = y + num3 * X.Sin(sagR);
				this.Pos(num8, num9, (fnMeshPointColor != null) ? fnMeshPointColor(this, num8, num9, OutCol, num3, sagR) : OutCol);
				if (num2 > 0f)
				{
					num8 = x + num2 * X.Cos(sagR);
					num9 = y + num2 * X.Sin(sagR);
					this.Pos(num8, num9, (fnMeshPointColor != null) ? fnMeshPointColor(this, num8, num9, InCol, num2, sagR) : InCol);
				}
				sagR += num;
			}
			this.fnMeshPointColor = fnMeshPointColor;
			return this;
		}

		public MeshDrawer InnerCircle(float x, float y, float w, float h, float cirx, float ciry, float cirrx, float cirry, float cir_in_rx, float cir_in_ry, bool fill_inner = false, bool no_divide_ppu = false, float grd_level_out = 0f, float grd_level_in = 1f)
		{
			if (!no_divide_ppu && this.ppu != 1f)
			{
				x *= this.ppur;
				y *= this.ppur;
				w *= this.ppur;
				h *= this.ppur;
				cirx *= this.ppur;
				ciry *= this.ppur;
				cirrx *= this.ppur;
				cirry *= this.ppur;
				cir_in_rx *= this.ppur;
				cir_in_ry *= this.ppur;
			}
			int num = X.IntC(X.Mx(cirrx, cirry) * this.ppu / 24f / 4f) * 4;
			return this.InnerPoly(x, y, w, h, cirx, ciry, cirrx, cirry, cir_in_rx, cir_in_ry, num, 0f, fill_inner, true, grd_level_out, grd_level_in);
		}

		public MeshDrawer InnerPoly(float x, float y, float w, float h, float cirx, float ciry, float cirrx, float cirry, float cir_in_rx, float cir_in_ry, int kaku4, float agR, bool fill_inner = false, bool no_divide_ppu = false, float grd_level_out = 0f, float grd_level_in = 1f)
		{
			if (!no_divide_ppu && this.ppu != 1f)
			{
				x *= this.ppur;
				y *= this.ppur;
				w *= this.ppur;
				h *= this.ppur;
				cirx *= this.ppur;
				ciry *= this.ppur;
				cirrx *= this.ppur;
				cirry *= this.ppur;
				cir_in_rx *= this.ppur;
				cir_in_ry *= this.ppur;
			}
			int num = X.Mx(1, kaku4 / 4);
			int num2 = num * 4;
			int num3 = num2 * 2;
			int num4 = 4 + num3;
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j <= num; j++)
				{
					if (j == 0)
					{
						this.Tri(i).Tri(4 + 2 * num * i).Tri((i + 3) % 4);
					}
					else
					{
						this.Tri(i).Tri(4 + 2 * (num * i + j) % num3).Tri(4 + 2 * (num * i + j - 1));
					}
				}
				for (int k = 0; k < num; k++)
				{
					int num5 = 2 * (num * i + k);
					this.Tri(4 + num5).Tri(4 + (num5 + 2) % num3).Tri(4 + (num5 + 1) % num3)
						.Tri(4 + (num5 + 2) % num3)
						.Tri(4 + (num5 + 3) % num3)
						.Tri(4 + (num5 + 1) % num3);
				}
				if (fill_inner)
				{
					for (int l = 0; l < num; l++)
					{
						int num6 = 4 + 2 * (num * i + l);
						this.Tri(4 + (num6 + 3) % num3).Tri(num4).Tri(4 + (num6 + 1) % num3);
					}
				}
			}
			x -= w * 0.5f;
			y -= h * 0.5f;
			float num7 = y + h;
			float num8 = x + w;
			C32 c = null;
			C32 c2 = null;
			if (grd_level_out > 0f)
			{
				c = ((grd_level_out == 1f) ? this.ColGrd : MeshDrawer.ColBuf0.Set(this.Col).blend(this.ColGrd, grd_level_out));
			}
			if (grd_level_in > 0f)
			{
				c2 = ((grd_level_in == 1f) ? this.ColGrd : MeshDrawer.ColBuf1.Set(this.Col).blend(this.ColGrd, grd_level_in));
			}
			this.Pos(x, y, c).Pos(x, num7, c).Pos(num8, num7, c)
				.Pos(num8, y, c);
			agR += 4.712389f;
			float num9 = 6.2831855f / (float)num2;
			for (int m = 0; m < num2; m++)
			{
				float num10 = X.Cos(agR);
				float num11 = X.Sin(agR);
				this.Pos(cirx + cirrx * num10, ciry + cirry * num11, c);
				this.Pos(cirx + cir_in_rx * num10, ciry + cir_in_ry * num11, c2);
				agR -= num9;
			}
			if (fill_inner)
			{
				this.Pos(cirx, ciry, c2);
			}
			return this;
		}

		public MeshDrawer KadomaruRect(float x, float y, float w, float h, float r, float t, bool no_divide_ppu = false, float grd_level_out = 0f, float grd_level_in = 0f, bool fill_inner_when_thick_drawing = false)
		{
			r = X.Mn(X.Mn(r, w * 0.5f), h * 0.5f);
			int num = X.MMX(1, X.IntR(r * 1.5707964f / (8f * (no_divide_ppu ? this.ppur : 1f))), 12);
			return this.KadoPolyRect(x, y, w, h, r, num, t, no_divide_ppu, grd_level_out, grd_level_in, fill_inner_when_thick_drawing);
		}

		public MeshDrawer KadoPolyRect(float x, float y, float w, float h, float r, int kaku, float t, bool no_divide_ppu = false, float grd_level_out = 0f, float grd_level_in = 0f, bool fill_inner_when_thick_drawing = false)
		{
			r = X.Mn(X.Mn(r, w * 0.5f), h * 0.5f);
			if (!no_divide_ppu && this.ppu != 1f)
			{
				x *= this.ppur;
				y *= this.ppur;
				w *= this.ppur;
				h *= this.ppur;
				r *= this.ppur;
				t *= this.ppur;
			}
			float num = x + w * 0.5f - r;
			float num2 = y + h * 0.5f - r;
			int num3 = 0;
			int num4 = 0;
			float num5 = ((t > 0f) ? X.Mx(0f, r - t) : r);
			float num6 = w - r * 2f;
			float num7 = h - r * 2f;
			int num8 = (kaku + 1) * 4;
			int num9 = num8 - ((num6 <= 0f) ? 2 : 0) - ((num7 <= 0f) ? 2 : 0);
			float num10 = 0f;
			C32 c = null;
			C32 c2 = null;
			if (grd_level_out > 0f)
			{
				c = ((grd_level_out == 1f) ? this.ColGrd : MeshDrawer.ColBuf0.Set(this.Col).blend(this.ColGrd, grd_level_out));
			}
			if (grd_level_in > 0f)
			{
				c2 = ((grd_level_in == 1f) ? this.ColGrd : MeshDrawer.ColBuf1.Set(this.Col).blend(this.ColGrd, grd_level_in));
			}
			if (t <= 0f)
			{
				this.Pos(x, y, c2);
				for (int i = 0; i < num9; i++)
				{
					this.Tri(-1).Tri((i + 1) % num9).Tri(i);
				}
			}
			else
			{
				int num11 = num9 * 2;
				if (fill_inner_when_thick_drawing)
				{
					this.Pos(x, y, c2);
					for (int j = 0; j < num11; j += 2)
					{
						this.Tri(-1).Tri((j + 3) % num11).Tri((j + 1) % num11);
					}
				}
				for (int k = 0; k < num11; k += 2)
				{
					this.Tri((k + 2) % num11).Tri(k).Tri((k + 1) % num11);
					this.Tri((k + 2) % num11).Tri((k + 1) % num11).Tri((k + 3) % num11);
				}
			}
			float num12 = ((kaku == 0) ? 1.5707964f : (1.5707964f / (float)kaku));
			float num13 = X.Cos(num10);
			float num14 = X.Sin(num10);
			int l = 0;
			while (l < num8)
			{
				this.Pos(num + r * num13, num2 + r * num14, c);
				if (t > 0f)
				{
					this.Pos(num + num5 * num13, num2 + num5 * num14, c2);
				}
				if (num3++ >= kaku)
				{
					num3 = 0;
					num4++;
					if (num4 % 2 == 1)
					{
						if (num6 > 0f)
						{
							num += (float)X.MPF(num4 % 4 == 3) * num6;
							goto IL_032C;
						}
						l++;
					}
					else
					{
						if (num7 > 0f)
						{
							num2 += (float)X.MPF(num4 % 4 == 0) * num7;
							goto IL_032C;
						}
						l++;
					}
					num3 = 1;
					goto IL_0313;
				}
				goto IL_0313;
				IL_032C:
				l++;
				continue;
				IL_0313:
				num10 += num12;
				num13 = X.Cos(num10);
				num14 = X.Sin(num10);
				goto IL_032C;
			}
			return this;
		}

		public MeshDrawer ButtonKadomaruDashedM(float x, float y, float w, float h, float r, int line_count, float t, bool no_divide_ppu = false, float fill_ratio = 0.5f, int kaku = -1)
		{
			if (w <= 0f || h <= 0f)
			{
				return this;
			}
			r = X.Mn(X.Mn(r, w * 0.5f), h * 0.5f);
			if (r <= 0f)
			{
				return this.RectDashedM(x, y, w, h, line_count, t, fill_ratio, no_divide_ppu, false);
			}
			if (!no_divide_ppu && this.ppu != 1f)
			{
				x *= this.ppur;
				y *= this.ppur;
				w *= this.ppur;
				h *= this.ppur;
				r *= this.ppur;
				t *= this.ppur;
			}
			if (kaku < 0)
			{
				kaku = X.MMX(1, X.IntR(r * 1.5707964f / (4f * this.ppur)), 12);
			}
			float num = ((t > 0f) ? X.Mx(0f, r - t) : r);
			float num2 = w - r * 2f;
			float num3 = h - r * 2f;
			int num4 = (kaku + 1) * 4;
			int num5 = (num4 - ((num2 <= 0f) ? 2 : 0) - ((num3 <= 0f) ? 2 : 0)) * 2;
			this.allocTri(num5 * 3, 60);
			for (int i = 0; i < num5; i += 2)
			{
				this.Tri(i + 2).Tri(i).Tri(i + 1);
				this.Tri(i + 2).Tri(i + 1).Tri(i + 3);
			}
			int num6 = 0;
			int num7 = 0;
			float num8 = 0f;
			float num9 = 1.5707964f / (float)kaku;
			float num10 = X.Cos(num8);
			float num11 = X.Sin(num8);
			float num12 = num2 * 2f + num3 * 2f + r * 6.2831855f;
			float num13 = 0f;
			UV_SETTYPE uv_SETTYPE = this.uv_settype;
			float num14 = x + w * 0.5f - r;
			float num15 = y + h * 0.5f - r;
			this.allocVer((num4 + 1) * 2, 64);
			int j = 0;
			while (j <= num4)
			{
				this.uvRectN((float)line_count * (1f - num13 / num12), fill_ratio);
				this.Pos(num14 + r * num10, num15 + r * num11, null);
				this.Pos(num14 + num * num10, num15 + num * num11, null);
				if (num6++ >= kaku)
				{
					num6 = 0;
					num7++;
					if (num7 % 2 == 1)
					{
						if (num2 > 0f)
						{
							num13 += num2;
							num14 += (float)X.MPF(num7 % 4 == 3) * num2;
							goto IL_02DA;
						}
						j++;
					}
					else
					{
						if (num3 > 0f)
						{
							num13 += num3;
							num15 += (float)X.MPF(num7 % 4 == 0) * num3;
							goto IL_02DA;
						}
						j++;
					}
					num6 = 1;
					goto IL_02B7;
				}
				goto IL_02B7;
				IL_02DA:
				j++;
				continue;
				IL_02B7:
				num8 += num9;
				num13 += num9 * r;
				num10 = X.Cos(num8);
				num11 = X.Sin(num8);
				goto IL_02DA;
			}
			this.uv_settype = uv_SETTYPE;
			return this;
		}

		public MeshDrawer KadomaruDaia(float x, float y, float wh, float r, float t, bool no_divide_ppu = false, float grd_level_out = 0f, float grd_level_in = 0f, bool fill_inner_when_thick_drawing = false)
		{
			if (!no_divide_ppu && this.ppu != 1f)
			{
				x *= this.ppur;
				y *= this.ppur;
				wh *= this.ppur;
				r *= this.ppur;
				t *= this.ppur;
			}
			Matrix4x4 matrixTransform = this.MatrixTransform;
			this.Rotate(0.7853982f, false).Translate(x, y, false);
			this.KadomaruRect(0f, 0f, wh * 2f, wh * 2f, r, t, true, grd_level_out, grd_level_in, fill_inner_when_thick_drawing);
			this.MatrixTransform = matrixTransform;
			return this;
		}

		public MeshDrawer Arrow(float x, float y, float wh, float agR, float t, bool no_divide_ppu = false)
		{
			if (wh == 0f)
			{
				return this;
			}
			if (!no_divide_ppu && this.ppu != 1f)
			{
				x *= this.ppur;
				y *= this.ppur;
				t *= this.ppur;
				wh *= this.ppur;
			}
			float num = t / 1.4142135f;
			float num2 = wh - num - t * 0.5f;
			Matrix4x4 currentMatrix = this.getCurrentMatrix();
			this.Translate(x, y, true);
			this.Rotate(agR, true);
			this.Tri(2, 1, 0, false).Tri(2, 0, 8, false).Tri(8, 0, 7, false)
				.Tri(7, 0, 4, false)
				.Tri(4, 0, 3, false)
				.Tri(5, 8, 7, false)
				.Tri(5, 7, 6, false);
			this.Pos(wh, 0f, null).Pos(0f, wh, null).Pos(-num, wh - num, null);
			this.Pos(0f, -wh, null).Pos(-num, -wh + num, null);
			this.Pos(-wh, t * 0.5f, null).Pos(-wh, -t * 0.5f, null);
			this.Pos(-num + num2, -t * 0.5f, null).Pos(-num + num2, t * 0.5f, null);
			this.setCurrentMatrix(currentMatrix, false);
			return this;
		}

		public MeshDrawer ArrowMush(float x, float y, float wh, float agR, bool no_divide_ppu = false)
		{
			if (wh == 0f)
			{
				return this;
			}
			if (!no_divide_ppu && this.ppu != 1f)
			{
				x *= this.ppur;
				y *= this.ppur;
				wh *= this.ppur;
			}
			Matrix4x4 currentMatrix = this.getCurrentMatrix();
			float num = wh * 0.2f;
			float num2 = num;
			float num3 = wh * 0.5f;
			float num4 = wh - num3;
			this.Translate(x, y, true);
			this.Rotate(agR, true);
			this.TriRectBL(0, 1, 2, 6).Tri(3, 4, 5, false);
			this.Pos(-num4 - num2, -num3, null).Pos(-num4 - num2, num3, null).Pos(num - num2, num3, null)
				.Pos(num - num2, wh - num, null)
				.Pos(wh - num2, 0f, null)
				.Pos(num - num2, -wh + num, null)
				.Pos(num - num2, -num3, null);
			this.setCurrentMatrix(currentMatrix, false);
			return this;
		}

		public MeshDrawer Boko(float x, float y, float w, float h, float thick, bool no_divide_ppu = false)
		{
			if (!no_divide_ppu && this.ppu != 1f)
			{
				x *= this.ppur;
				y *= this.ppur;
				w *= this.ppur;
				h *= this.ppur;
				thick *= this.ppur;
			}
			float num = w * 0.5f;
			float num2 = h * 0.5f;
			float num3 = h - thick;
			Matrix4x4 currentMatrix = this.getCurrentMatrix();
			this.Translate(x, y, true);
			this.TriRectBL(0).TriRectBL(0, 3, 4, 7).TriRectBL(4, 5, 6, 7);
			this.Pos(-num, -num2, null).Pos(-num, num2, null).Pos(-num + thick, num2, null)
				.Pos(-num + thick, num2 - num3, null);
			this.Pos(num - thick, num2 - num3, null).Pos(num - thick, num2, null).Pos(num, num2, null)
				.Pos(num, -num2, null);
			this.setCurrentMatrix(currentMatrix, false);
			return this;
		}

		public MeshDrawer CuteFrame(float x, float y, float w, float h, float daia_wh, float t, bool no_divide_ppu = false)
		{
			if (!no_divide_ppu && this.ppu != 1f)
			{
				x *= this.ppur;
				y *= this.ppur;
				w *= this.ppur;
				h *= this.ppur;
				t *= this.ppur;
				daia_wh *= this.ppur;
			}
			float num = w / 2f;
			float num2 = h / 2f;
			float num3 = daia_wh * 2f;
			this.KadomaruRect(x - num, y, t, (num2 - num3) * 2f, t, 0f, true, 0f, 0f, false);
			this.KadomaruRect(x, y + num2, (num - num3) * 2f, t, t, 0f, true, 0f, 0f, false);
			this.KadomaruRect(x + num, y, t, (num2 - num3) * 2f, t, 0f, true, 0f, 0f, false);
			this.KadomaruRect(x, y - num2, (num - num3) * 2f, t, t, 0f, true, 0f, 0f, false);
			for (int i = 4; i < 8; i++)
			{
				this.KadomaruDaia(x + num * (float)CAim._XD(i, 1), y + num2 * (float)CAim._YD(i, 1), daia_wh, daia_wh / 2.5f, t, true, 0f, 0f, false);
			}
			return this;
		}

		public MeshDrawer ButtonBanner(float x, float y, float w, float h, bool no_divide_ppu = false)
		{
			if (!no_divide_ppu && this.ppu != 1f)
			{
				x *= this.ppur;
				y *= this.ppur;
				w *= this.ppur;
				h *= this.ppur;
			}
			int num = X.MMX(6, X.IntC(h * 3.1415927f / (6f * this.ppur)), 10);
			float num2 = h * 0.5f;
			float num3 = num2 * 0.82f;
			float num4 = x - w * 0.5f + num2;
			float num5 = y;
			float num6 = 1.5707964f;
			float num7 = 1.5707964f;
			int num8 = num + 1;
			int num9 = num8 * 2;
			int num10 = num9 * 2;
			float num11 = 3.1415927f / (float)num;
			float num12 = 3.1415927f / (float)num8;
			x += -w * 0.5f + num2;
			for (int i = 0; i < num10; i += 2)
			{
				int num13 = (i + 2) % num10;
				int num14 = (i + 3) % num10;
				this.Tri(i, i + 1, num13, false);
				this.Tri(i + 1, num14, num13, false);
			}
			for (int j = 1; j <= num9; j++)
			{
				this.Pos(x + num2 * X.Cos(num6), y + num2 * X.Sin(num6), null);
				this.Pos(num4 + num3 * X.Cos(num7), num5 + num3 * X.Sin(num7), null);
				if (j == num8)
				{
					x += w - h;
				}
				else
				{
					num6 += num11;
				}
				num7 += num12;
			}
			return this;
		}

		public MeshDrawer StripedCircle(float cx, float cy, float radius, float moverate = 0f, float fillrate = 0.5f, float base_line_thick = 8f, bool no_divide_ppu = false)
		{
			if (!no_divide_ppu && this.ppu != 1f)
			{
				cx *= this.ppur;
				cy *= this.ppur;
				radius *= this.ppur;
				base_line_thick *= this.ppur;
			}
			int num = X.IntC(radius / base_line_thick) + 2;
			if (num <= 0 || fillrate <= 0f)
			{
				return this;
			}
			float num2 = radius;
			float num3 = -(num2 + base_line_thick * 2f * (1f - moverate % 1f)) * 1.4142135f;
			float num4 = num2 * num2;
			if (fillrate >= 1f)
			{
				this.Circle(cx, cy, radius, 0f, true, 0f, 0f);
			}
			else
			{
				for (int i = 0; i < num; i++)
				{
					float num5 = num3 + base_line_thick * 1.4142135f * 2f * fillrate;
					float num6 = num4 * 2f - num3 * num3;
					float num7 = num4 * 2f - num5 * num5;
					if (num6 >= 0f || num7 >= 0f)
					{
						float num8 = 0f;
						float num9 = 0f;
						if (num6 >= 0f && num7 >= 0f)
						{
							float num10 = Mathf.Sqrt(num6);
							float num11 = Mathf.Sqrt(num7);
							float num12 = (num3 - num10) / 2f;
							float num13 = (num3 + num10) / 2f;
							float num14 = (num5 - num11) / 2f;
							num8 = (num5 + num11) / 2f;
							float num15 = -num12 + num3;
							float num16 = -num13 + num3;
							float num17 = -num14 + num5;
							num9 = -num8 + num5;
							this.Tri(0, 2, 1, false).Tri(0, 3, 2, false);
							this.Pos(cx + num12, cy + num15, null).Pos(cx + num13, cy + num16, null).Pos(cx + num8, cy + num9, null)
								.Pos(cx + num14, cy + num17, null);
						}
						else
						{
							float num18 = 0f;
							int num19 = 0;
							if (num7 >= 0f && num6 < 0f)
							{
								if (num7 != 0f)
								{
									num6 = num7;
									float num14;
									num8 = (num14 = -num2 / 1.4142135f);
									num9 = num14;
									num18 = num5;
									num19 = 2;
								}
							}
							else if (num6 != 0f)
							{
								float num14;
								num8 = (num14 = num2 / 1.4142135f);
								num9 = num14;
								num18 = num3;
								num19 = 1;
							}
							if (num19 > 0)
							{
								float num10 = Mathf.Sqrt(num6);
								float num12 = (num18 - num10) / 2f;
								float num13 = (num18 + num10) / 2f;
								float num15 = -num12 + num18;
								float num16 = -num13 + num18;
								if (num19 == 1)
								{
									this.Tri(0, 2, 1, false);
								}
								else
								{
									this.Tri(0, 1, 2, false);
								}
								this.Pos(cx + num12, cy + num15, null).Pos(cx + num13, cy + num16, null).Pos(cx + num8, cy + num9, null);
							}
						}
					}
					num3 += base_line_thick * 2f * 1.4142135f;
				}
			}
			return this;
		}

		public MeshDrawer StripedM(float scroll_agR = 0.7853982f, float line_intv = 20f, float fill_ratio = 0.5f, int alloc_ver = 4)
		{
			this.uvRectN(X.Cos(scroll_agR), X.Sin(-scroll_agR));
			this.allocUv2(alloc_ver, false).Uv2(line_intv - 20f, fill_ratio, false);
			return this;
		}

		public MeshDrawer StripedRect(float cx, float cy, float wd, float hg, float moverate = 0f, float fillrate = 0.5f, float base_line_thick = 8f, bool no_divide_ppu = false)
		{
			return this.StripedRectBL(cx - wd / 2f, cy - hg / 2f, wd, hg, moverate, fillrate, base_line_thick, no_divide_ppu);
		}

		public MeshDrawer StripedRectBL(float cx, float cy, float wd, float hg, float moverate = 0f, float fillrate = 0.5f, float base_line_thick = 8f, bool no_divide_ppu = false)
		{
			if (!no_divide_ppu && this.ppu != 1f)
			{
				cx *= this.ppur;
				cy *= this.ppur;
				wd *= this.ppur;
				hg *= this.ppur;
				base_line_thick *= this.ppur;
			}
			if (fillrate >= 1f)
			{
				this.RectBL(cx, cy, wd, hg, true);
			}
			else
			{
				Vector2[] astrpPts = MeshDrawer.AStrpPts;
				X.ALL0(MeshDrawer.AStrpPts);
				int num = X.IntC(X.Mx(wd, hg) / base_line_thick) + 2;
				Vector2 vector = new Vector2(-(base_line_thick * 2f * (1f - moverate)) * 1.4142135f, 0f);
				Vector2Int vector2Int = default(Vector2Int);
				for (int i = 0; i < num; i++)
				{
					vector[1] = vector[0] + base_line_thick * 1.4142135f * 2f * fillrate;
					for (int j = 0; j < 2; j++)
					{
						float num2 = vector[j];
						vector2Int[j] = 0;
						if (num2 <= 0f)
						{
							astrpPts[2 + j].x = 0f;
							astrpPts[2 + j].y = 0f;
							astrpPts[j].x = 0f;
							astrpPts[j].y = 0f;
						}
						else if (num2 >= hg + wd)
						{
							astrpPts[2 + j].x = wd;
							astrpPts[2 + j].y = hg;
							astrpPts[j].x = wd;
							astrpPts[j].y = hg;
						}
						else
						{
							if (num2 >= wd)
							{
								ref Vector2Int ptr = ref vector2Int;
								int num3 = j;
								ptr[num3] |= 1;
								astrpPts[2 + j].x = wd;
								astrpPts[2 + j].y = -wd + num2;
							}
							else
							{
								astrpPts[2 + j].x = num2;
								astrpPts[2 + j].y = 0f;
							}
							if (num2 <= 0f)
							{
								astrpPts[j].x = 0f;
								astrpPts[j].y = 0f;
							}
							else if (num2 >= hg)
							{
								ref Vector2Int ptr = ref vector2Int;
								int num3 = j;
								ptr[num3] |= 2;
								astrpPts[j].x = num2 - hg;
								astrpPts[j].y = hg;
							}
							else
							{
								astrpPts[j].x = 0f;
								astrpPts[j].y = num2;
							}
						}
					}
					int num4 = this.ver_i;
					this.Tri(0, 1, 2, false).Tri(1, 3, 2, false);
					this.Pos(cx + astrpPts[0].x, cy + astrpPts[0].y, null).Pos(cx + astrpPts[1].x, cy + astrpPts[1].y, null).Pos(cx + astrpPts[2].x, cy + astrpPts[2].y, null)
						.Pos(cx + astrpPts[3].x, cy + astrpPts[3].y, null);
					if ((vector2Int[0] & 1) != (vector2Int[1] & 1))
					{
						this.TriN(num4 + 2).TriN(num4 + 3).Tri(0);
						this.Pos(cx + wd, cy, null);
					}
					if ((vector2Int[0] & 2) != (vector2Int[1] & 2))
					{
						this.TriN(num4).Tri(0).TriN(num4 + 1);
						this.Pos(cx, cy + hg, null);
					}
					vector[0] = vector[0] + base_line_thick * 2f * 1.4142135f;
				}
			}
			return this;
		}

		public MeshDrawer StripedDaia(float cx, float cy, float wd, float moverate = 0f, float fillrate = 0.5f, float base_line_thick = 8f, bool no_divide_ppu = false)
		{
			if (!no_divide_ppu && this.ppu != 1f)
			{
				cx *= this.ppur;
				cy *= this.ppur;
				wd *= this.ppur;
				base_line_thick *= this.ppur;
			}
			Matrix4x4 currentMatrix = this.getCurrentMatrix();
			this.Rotate(-0.7853982f, false).Translate(cx, cy, false);
			float num = wd * 0.25f * 1.4142135f;
			float num2 = num * 2f;
			this.LineDashed(-num, 0f, num, 0f, moverate, X.IntR(num2 / base_line_thick), wd * 0.5f * 1.4142135f, true, fillrate);
			this.setCurrentMatrix(currentMatrix, false);
			return this;
		}

		public MeshDrawer MIcon(float cx, float cy, float wd, string name, float val1 = 0f)
		{
			MTRX.DrawMeshIcon(this, cx, cy, wd, name, val1);
			return this;
		}

		public MeshDrawer PatternFill(float lx, float by, float wd, float hg, Sprite Sp = null, float pat_w = 0f, float pat_h = 0f, float shiftx = 0f, float shifty = 0f)
		{
			float num;
			float num2;
			float num3;
			float num4;
			if (Sp != null)
			{
				this.initForImg(Sp);
				num = ((pat_w > 0f) ? pat_w : Sp.rect.width);
				num2 = ((pat_h > 0f) ? pat_h : Sp.rect.height);
				num3 = Sp.rect.width / num;
				num4 = Sp.rect.height / num2;
			}
			else
			{
				if (this.texture_width == 0f)
				{
					return this;
				}
				num = ((pat_w > 0f) ? pat_w : (this.texture_width * this.uv_width));
				num2 = ((pat_h > 0f) ? pat_h : (this.texture_height * this.uv_height));
				num3 = this.texture_width * this.uv_width / num;
				num4 = this.texture_height * this.uv_height / num2;
			}
			float num5 = this.uv_left;
			float num6 = this.uv_top;
			float num7 = this.uv_width;
			float num8 = this.uv_height;
			float num9 = (num - shiftx % num) % num;
			float num10 = (num2 - shifty % num2) % num2;
			float num11 = num9;
			float num12 = 0f;
			while (num12 < hg)
			{
				float num13 = X.Mn(hg, num12 + (num2 - num10));
				float num14 = num13 - num12;
				this.uv_top = num6 + num10 / this.texture_height * num4;
				this.uv_height = num14 / this.texture_height * num4;
				float num15 = 0f;
				while (num15 < wd)
				{
					float num16 = X.Mn(wd, num15 + (num - num9));
					float num17 = num16 - num15;
					this.uv_left = num5 + num9 / this.texture_width * num3;
					this.uv_width = num17 / this.texture_width * num3;
					this.RectBL(lx + num15, by + num12, num17, num14, false);
					num15 = num16;
					num9 = 0f;
				}
				num9 = num11;
				num12 = num13;
				num10 = 0f;
			}
			return this;
		}

		public MeshDrawer PatternFill4GDTPat(float lx, float by, float wd, float hg, PxlImage Img = null, float pattern_scale = 1f, float shiftx_level = 0f, float shifty_level = 0f)
		{
			Rect rectIUv = Img.RectIUv;
			this.Mtr.SetTextureOffset("_MainTex", new Vector2(rectIUv.x, rectIUv.y));
			this.Mtr.SetTextureScale("_MainTex", new Vector2(rectIUv.width, rectIUv.height));
			Texture i = Img.get_I();
			X.IntR(rectIUv.width * (float)i.width);
			X.IntR(rectIUv.height * (float)i.height);
			shiftx_level = X.frac(shiftx_level) * rectIUv.width * pattern_scale;
			shifty_level = X.frac(shifty_level) * rectIUv.height * pattern_scale;
			float num = shiftx_level + wd / (float)i.width / pattern_scale;
			float num2 = shifty_level + hg / (float)i.height / pattern_scale;
			this.TriRectBL(0);
			lx *= this.ppur;
			by *= this.ppur;
			wd *= this.ppur;
			hg *= this.ppur;
			this.uvRectN(shiftx_level, shifty_level).Pos(lx, by, null);
			this.uvRectN(shiftx_level, num2).Pos(lx, by + hg, null);
			this.uvRectN(num, num2).Pos(lx + wd, by + hg, null);
			this.uvRectN(num, shifty_level).Pos(lx + wd, by, null);
			return this;
		}

		public MeshDrawer LineDashed(float x, float y, float dx, float dy, float pos_level, int line_count, float thick, bool no_divide_ppu = false, float fillrate = 0.5f)
		{
			if (!no_divide_ppu && this.ppu != 1f)
			{
				x *= this.ppur;
				y *= this.ppur;
				dx *= this.ppur;
				dy *= this.ppur;
				thick *= this.ppur;
			}
			float num;
			float num2;
			if (dx == x)
			{
				num = ((y < dy) ? 1.5707964f : (-1.5707964f));
				num2 = X.Abs(y - dy);
			}
			else if (dy == y)
			{
				num = ((x < dx) ? 0f : (-3.1415927f));
				num2 = X.Abs(x - dx);
			}
			else
			{
				num = X.GAR2(x, y, dx, dy);
				num2 = X.LENGTHXY(x, y, dx, dy);
			}
			num2 /= (float)line_count * 2f;
			float num3 = thick / 2f * X.Cos(num + 1.5707964f);
			float num4 = thick / 2f * X.Sin(num + 1.5707964f);
			float num5 = num2 * X.Cos(num) * 2f;
			float num6 = num2 * X.Sin(num) * 2f;
			float num7 = num5 * fillrate;
			float num8 = num6 * fillrate;
			this.allocTri(this.tri_i + (line_count + 2) * 6, 60);
			this.allocVer(this.ver_i + (line_count + 2) * 4, 64);
			if (pos_level < fillrate)
			{
				float num9 = num5 * pos_level;
				float num10 = num6 * pos_level;
				this.TriRectBL(0).Pos(x + num3, y + num4, null).Pos(x + num3 + num9, y + num4 + num10, null)
					.Pos(x - num3 + num9, y - num4 + num10, null)
					.Pos(x - num3, y - num4, null);
				x += num9 + num5 * (1f - fillrate);
				y += num10 + num6 * (1f - fillrate);
			}
			else
			{
				x += num5 * (pos_level - fillrate);
				y += num6 * (pos_level - fillrate);
			}
			for (int i = ((pos_level < 0.5f) ? 1 : 0); i < line_count; i++)
			{
				this.TriRectBL(0).Pos(x + num3, y + num4, null).Pos(x + num3 + num7, y + num4 + num8, null)
					.Pos(x - num3 + num7, y - num4 + num8, null)
					.Pos(x - num3, y - num4, null);
				x += num5;
				y += num6;
			}
			if (pos_level < 0.5f)
			{
				this.TriRectBL(0).Pos(x + num3, y + num4, null).Pos(dx + num3, dy + num4, null)
					.Pos(dx - num3, dy - num4, null)
					.Pos(x - num3, y - num4, null);
			}
			return this;
		}

		public MeshDrawer ButtonKadomaruDashed(float x, float y, float w, float h, float pos_level, int line_count, float thick)
		{
			float num = h / 2f;
			float num2 = num * 3.1415927f;
			float num3 = w - num * 2f;
			float num4 = num3 / 2f;
			float num5 = num3 * 2f + num2 * 2f;
			float num6 = num5 / (float)line_count / 2f;
			float num7 = pos_level * num6 * 2f;
			float num8 = num3;
			float num9 = num3 + num2;
			float num10 = num3 * 2f + num2;
			float num11 = num * 2f * 3.1415927f;
			for (int i = 0; i < line_count; i++)
			{
				float num12 = num7 + num6;
				if (num7 < num8)
				{
					this.Line(x - num4 + num7, y + num, x - num4 + X.Mn(num12, num8), y + num, thick, false, 0f, 0f);
					if (num12 >= num8)
					{
						this.ArcLen(x + num4, y, num, 1.5707964f, -(num12 - num8), thick);
					}
				}
				else if (num7 < num9)
				{
					float num13 = 1.5707964f - (num7 - num8) / num11 * 6.2831855f;
					this.ArcLen(x + num4, y, num, num13, -(X.Mn(num12, num9) - num7), thick);
					if (num12 >= num9)
					{
						this.Line(x + num4, y - num, x + num4 - (num12 - num9), y - num, thick, false, 0f, 0f);
					}
				}
				else if (num7 < num10)
				{
					this.Line(x + num4 - (num7 - num9), y - num, x + num4 - (X.Mn(num12, num10) - num9), y - num, thick, false, 0f, 0f);
					if (num12 >= num10)
					{
						this.ArcLen(x - num4, y, num, -1.5707964f, -(num12 - num10), thick);
					}
				}
				else
				{
					float num14 = -1.5707964f - (num7 - num10) / num11 * 6.2831855f;
					this.ArcLen(x - num4, y, num, num14, -(X.Mn(num12, num5) - num7), thick);
					if (num12 >= num5)
					{
						this.Line(x - num4, y + num, x - num4 + (num12 - num5), y + num, thick, false, 0f, 0f);
					}
				}
				num7 = num12 + num6;
			}
			return this;
		}

		public MeshDrawer RectDashedBL(float x, float y, float w, float h, float pos_level, int line_count, float thick, bool no_divide_ppu = false)
		{
			return this.RectDashed(x + w / 2f, y + h / 2f, w, h, pos_level, line_count, thick, no_divide_ppu, false);
		}

		public MeshDrawer RectDashed(float x, float y, float w, float h, float pos_level, int line_count, float thick, bool no_divide_ppu = false, bool expand_head_and_foot = false)
		{
			if (!no_divide_ppu && this.ppu != 1f)
			{
				x *= this.ppur;
				y *= this.ppur;
				w *= this.ppur;
				h *= this.ppur;
				thick *= this.ppur;
			}
			float num = w * 2f + h * 2f;
			float num2 = num / (float)line_count / 2f;
			float num3 = pos_level * num2 * 2f;
			float num4 = w / 2f;
			float num5 = h / 2f;
			float num6 = (expand_head_and_foot ? thick : 0f);
			float num7 = w;
			float num8 = w + h;
			float num9 = w * 2f + h;
			for (int i = 0; i < line_count; i++)
			{
				float num10 = num3 + num2;
				for (int j = 0; j < 2; j++)
				{
					if (num3 < num7)
					{
						this.Line(x - num4 + num3 - num6, y + num5, x - num4 + X.Mn(num10 + num6, num7), y + num5, thick, true, 0f, 0f);
						if (num10 < num7)
						{
							break;
						}
						num3 = num7;
					}
					else if (num3 < num8)
					{
						this.Line(x + num4, y + num5 - X.Mx(0f, num3 - num7 - num6), x + num4, y + num5 - (X.Mn(num10 + num6, num8) - num7), thick, true, 0f, 0f);
						if (num10 < num8)
						{
							break;
						}
						num3 = num8;
					}
					else if (num3 < num9)
					{
						this.Line(x + num4 - X.Mx(0f, num3 - num8 - num6), y - num5, x + num4 - (X.Mn(num10 + num6, num9) - num8), y - num5, thick, true, 0f, 0f);
						if (num10 < num9)
						{
							break;
						}
						num3 = num9;
					}
					else
					{
						if (num3 >= num)
						{
							break;
						}
						this.Line(x - num4, y - num5 + X.Mx(0f, num3 - num9 - num6), x - num4, y - num5 + (X.Mn(num10 + num6, num) - num9), thick, true, 0f, 0f);
						if (num10 >= num)
						{
							this.Line(x - num4, y + num5, x - num4 + X.Mn(num10 - num + num6, num7), y + num5, thick, true, 0f, 0f);
							break;
						}
						break;
					}
				}
				num3 = num10 + num2;
			}
			return this;
		}

		public MeshDrawer RectDashedM(float x, float y, float w, float h, int line_count, float thick, float fill_ratio = 0.5f, bool no_divide_ppu = false, bool expand_head_and_foot = false)
		{
			return this.RectDashedMBL(x - w / 2f, y - h / 2f, w, h, line_count, thick, fill_ratio, no_divide_ppu, false);
		}

		public MeshDrawer RectDashedMBL(float x, float y, float w, float h, int line_count, float thick, float fill_ratio = 0.5f, bool no_divide_ppu = false, bool expand_head_and_foot = false)
		{
			if (w == 0f || h == 0f)
			{
				return this;
			}
			if (!no_divide_ppu && this.ppu != 1f)
			{
				x *= this.ppur;
				y *= this.ppur;
				w *= this.ppur;
				h *= this.ppur;
				thick *= this.ppur;
			}
			float num = (float)line_count / ((w + h) * 2f);
			float num2 = y + h;
			float num3 = x + w;
			this.Tri(0, 2, 1, false).Tri(1, 2, 3, false).Tri(2, 4, 3, false)
				.Tri(3, 4, 5, false)
				.Tri(5, 4, 7, false)
				.Tri(7, 4, 6, false)
				.Tri(9, 7, 8, false)
				.Tri(8, 7, 6, false);
			UV_SETTYPE uv_SETTYPE = this.uv_settype;
			this.uvRectN(0f, fill_ratio);
			this.Pos(x, y, null).Pos(x + thick, y + thick, null);
			this.uvRectN(h * num, fill_ratio);
			this.Pos(x, num2, null).Pos(x + thick, num2 - thick, null);
			this.uvRectN((w + h) * num, fill_ratio);
			this.Pos(num3, num2, null).Pos(num3 - thick, num2 - thick, null);
			this.uvRectN((w + h + h) * num, fill_ratio);
			this.Pos(num3, y, null).Pos(num3 - thick, y + thick, null);
			this.uvRectN((float)line_count, fill_ratio);
			this.Pos(x, y, null).Pos(x + thick, y + thick, null);
			this.uv_settype = uv_SETTYPE;
			return this;
		}

		public MeshDrawer BluredCurtain(float x, float y, float w, float h, float sidewh, bool vertical, bool no_divide_ppu = false)
		{
			if (!no_divide_ppu && this.ppu != 1f)
			{
				x *= this.ppur;
				y *= this.ppur;
				w *= this.ppur;
				h *= this.ppur;
				sidewh *= this.ppur;
			}
			this.Rect(x, y, w, h, true);
			float num = x - w * 0.5f;
			float num2 = y - h * 0.5f;
			float num3 = num + w;
			float num4 = num2 + h;
			this.Tri(0, 1, 2, false).Tri(0, 2, 3, false);
			this.Tri(4, 5, 6, false).Tri(4, 6, 7, false);
			if (vertical)
			{
				this.Pos(num, num4, null).Pos(num, num4 + sidewh, this.ColGrd).Pos(num3, num4 + sidewh, this.ColGrd)
					.Pos(num3, num4, null);
				this.Pos(num, num2 - sidewh, this.ColGrd).Pos(num, num2, null).Pos(num3, num2, null)
					.Pos(num3, num2 - sidewh, this.ColGrd);
			}
			else
			{
				this.Pos(num - sidewh, num2, null).Pos(num - sidewh, num4, null).Pos(num, num4, null)
					.Pos(num, num2, null);
				this.Pos(num3, num2, null).Pos(num3, num4, null).Pos(num3 + sidewh, num4, null)
					.Pos(num3 + sidewh, num2, null);
			}
			return this;
		}

		public MeshDrawer BoxCharacterInfo(float x, float y, float zmx, float zmy, char chr, FontStorage Storage, CharacterInfo Ch, bool no_divide_ppu = false, TextRenderStyle Style = null)
		{
			if (!no_divide_ppu && this.ppu != 1f)
			{
				x *= this.ppur;
				y *= this.ppur;
			}
			x += (float)Ch.bearing * zmx * this.ppur;
			y += (float)Storage.margin * zmy * this.ppur;
			this.Tri(0, 1, 2, false).Tri(0, 2, 3, false);
			this.uv_settype = UV_SETTYPE.NONE;
			float num = (float)(Ch.maxX - Ch.minX) * zmx * this.ppur;
			float num2 = (float)(Ch.maxY - Ch.minY) * zmy * this.ppur;
			bool flag = false;
			float num3 = 0f;
			if (Style != null)
			{
				if (Style.consider_leftshift)
				{
					x += (float)Ch.minX * zmx * this.ppur;
				}
				flag = Style.bold;
				num3 = (Style.italic ? (zmx * 4f * this.ppur) : 0f);
			}
			y += (float)Ch.minY * zmy * this.ppur;
			for (int i = (flag ? 1 : 0); i >= 0; i--)
			{
				float num4 = x + (float)i * zmx * 2f * this.ppur;
				this.uv_left = Ch.uvBottomLeft.x;
				this.uv_top = Ch.uvBottomLeft.y;
				this.Pos(num4, y, null);
				this.uv_left = Ch.uvTopLeft.x;
				this.uv_top = Ch.uvTopLeft.y;
				this.Pos(num4 + num3, y + num2, null);
				this.uv_left = Ch.uvTopRight.x;
				this.uv_top = Ch.uvTopRight.y;
				this.Pos(num4 + num3 + num, y + num2, null);
				this.uv_left = Ch.uvBottomRight.x;
				this.uv_top = Ch.uvBottomRight.y;
				this.Pos(num4 + num, y, null);
			}
			return this;
		}

		public MeshDrawer Identity()
		{
			this.MatrixTransform = Matrix4x4.identity;
			this.transformed = false;
			this.fliped = false;
			return this;
		}

		public MeshDrawer Scale(float scalex, float scaley, bool pre_transform = false)
		{
			MeshDrawer.BufTrs.Set(scalex, scaley, 1f);
			this.MatrixTransform = (pre_transform ? (this.MatrixTransform * Matrix4x4.Scale(MeshDrawer.BufTrs)) : (Matrix4x4.Scale(MeshDrawer.BufTrs) * this.MatrixTransform));
			this.transformed = true;
			this.fliped = ((scalex * scaley < 0f) ? (!this.fliped) : this.fliped);
			return this;
		}

		public MeshDrawer Rotate(float rotR, bool pre_transform = false)
		{
			Matrix4x4 matrix4x = Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, rotR * 0.31830987f * 180f));
			this.MatrixTransform = (pre_transform ? (this.MatrixTransform * matrix4x) : (matrix4x * this.MatrixTransform));
			this.transformed = true;
			return this;
		}

		public MeshDrawer Rotate01(float rot01, bool pre_transform = false)
		{
			return this.Rotate(rot01 * 6.2831855f, pre_transform);
		}

		public MeshDrawer Translate(float translatex, float translatey, bool pre_transform = false)
		{
			MeshDrawer.BufTrs.Set(translatex, translatey, 0f);
			this.MatrixTransform = (pre_transform ? (this.MatrixTransform * Matrix4x4.Translate(MeshDrawer.BufTrs)) : (Matrix4x4.Translate(MeshDrawer.BufTrs) * this.MatrixTransform));
			this.transformed = true;
			return this;
		}

		public MeshDrawer TranslateP(float translatex, float translatey, bool pre_transform = false)
		{
			translatex *= this.ppur;
			translatey *= this.ppur;
			MeshDrawer.BufTrs.Set(translatex, translatey, 0f);
			this.MatrixTransform = (pre_transform ? (this.MatrixTransform * Matrix4x4.Translate(MeshDrawer.BufTrs)) : (Matrix4x4.Translate(MeshDrawer.BufTrs) * this.MatrixTransform));
			this.transformed = true;
			return this;
		}

		public MeshDrawer Skew(float tan_x, float tan_y, bool pre_transform = false)
		{
			Matrix4x4 identity = Matrix4x4.identity;
			identity[0, 1] = tan_x;
			identity[1, 0] = tan_y;
			this.MatrixTransform = (pre_transform ? (this.MatrixTransform * identity) : (identity * this.MatrixTransform));
			this.transformed = true;
			return this;
		}

		public Matrix4x4 getCurrentMatrix()
		{
			return this.MatrixTransform;
		}

		public MeshDrawer setCurrentMatrix(Matrix4x4 Mtrx, bool mul = false)
		{
			if (mul)
			{
				this.MatrixTransform = Mtrx * this.MatrixTransform;
			}
			else
			{
				this.MatrixTransform = Mtrx;
			}
			this.transformed = true;
			Vector3 lossyScale = this.MatrixTransform.lossyScale;
			this.fliped = lossyScale.x * lossyScale.y < 0f;
			return this;
		}

		public MeshDrawer White()
		{
			this.Col.r = (this.Col.g = (this.Col.b = (this.Col.a = byte.MaxValue)));
			return this;
		}

		public MeshDrawer DrawDiffuseImage(float x, float y, float scale, float level, Sprite Spr, AIM aim_start = AIM.LT)
		{
			if (Spr != null)
			{
				this.initForImg(Spr);
			}
			if (this.texture_width == 0f)
			{
				return this;
			}
			float num = scale * this.uv_width * this.texture_width * this.ppur;
			float num2 = scale * this.uv_height * this.texture_height * this.ppur;
			x = x * this.ppur - num * 0.5f;
			y = y * this.ppur - num2 * 0.5f;
			this.uvRect(x, y, num, num2, this.uv_left, this.uv_top, this.uv_width, this.uv_height, false, true);
			if (level < 0.5f)
			{
				if (aim_start == AIM.LT)
				{
					y += num2;
					this.Tri(0, 1, 2, false);
					this.Pos(x, y, null).Pos(x + num * level * 2f, y, null).Pos(x, y - num2 * level * 2f, null);
				}
			}
			else
			{
				float num3 = (level - 0.5f) * 2f;
				if (aim_start == AIM.LT)
				{
					y += num2;
					this.Tri(0, 1, 2, false).Tri(1, 3, 2, false).Tri(2, 3, 4, false);
					this.Pos(x, y, null).Pos(x + num, y, null).Pos(x, y - num2, null);
					this.Pos(x + num, y - num2 * num3, null).Pos(x + num * num3, y - num2, null);
				}
			}
			return this;
		}

		public MeshDrawer DrawScaleMesh(float x, float y, float scalex = 1f, float scaley = 1f, PxlMeshDrawer PM = null)
		{
			Vector2[] array;
			Color32[] array2;
			Vector3[] array3;
			int[] array4;
			int num;
			int num2;
			if (!PM.getRawVerticesAndTriangles(out array, out array2, out array3, out array4, out num, out num2))
			{
				Mesh mesh = PM.getMesh(false);
				this.PosAppend(x * this.ppur, y * this.ppur, mesh.vertices, mesh.uv, mesh.colors32, mesh.triangles, scalex, scaley, -1, -1);
			}
			else
			{
				this.PosAppend(x * this.ppur, y * this.ppur, array3, array, array2, array4, scalex, scaley, num, num2);
			}
			return this;
		}

		public MeshDrawer DrawCen(float x, float y, Sprite Spr = null)
		{
			return this.DrawScaleGraph2(x, y, 0.5f, 0.5f, 1f, 1f, Spr);
		}

		public MeshDrawer DrawScaleGraph(float x, float y, float scalex = 1f, float scaley = 1f, Sprite Spr = null)
		{
			return this.DrawScaleGraph2(x, y, 0.5f, 0.5f, scalex, scaley, Spr);
		}

		public MeshDrawer DrawScaleGraph2(float x, float y, float pivot_x = 0.5f, float pivot_y = 0.5f, float scalex = 1f, float scaley = 1f, Sprite Spr = null)
		{
			if (Spr != null)
			{
				this.initForImg(Spr);
			}
			if (this.texture_width == 0f)
			{
				return this;
			}
			float num = this.texture_width * this.uv_width * scalex;
			float num2 = this.texture_height * this.uv_height * scaley;
			return this.RectBL(x - num * pivot_x, y - num2 * pivot_y, num, num2, false);
		}

		public MeshDrawer DrawFlipGraph(float x, float y, float pivot_x = 0.5f, float pivot_y = 0.5f, float scalex = 1f, float scaley = 1f, Sprite Spr = null, bool flipx = false, bool flipy = false)
		{
			if (Spr != null)
			{
				this.initForImg(Spr);
			}
			if (this.texture_width == 0f)
			{
				return this;
			}
			float num = this.uv_left;
			float num2 = this.uv_top;
			float num3 = this.uv_width;
			float num4 = this.uv_height;
			float num5 = this.texture_width * this.uv_width * scalex;
			float num6 = this.texture_height * this.uv_height * scaley;
			if (flipx)
			{
				this.uv_left += this.uv_width;
				this.uv_width *= -1f;
			}
			if (flipy)
			{
				this.uv_top += this.uv_height;
				this.uv_height *= -1f;
			}
			this.RectBL(x - num5 * pivot_x, y - num6 * pivot_y, num5, num6, false);
			this.uv_left = num;
			this.uv_top = num2;
			this.uv_width = num3;
			this.uv_height = num4;
			return this;
		}

		public MeshDrawer DrawExtendGraphBL(float x, float y, float dw, float dh, Sprite Spr = null, bool flipx = false, bool flipy = false)
		{
			if (Spr != null)
			{
				this.initForImg(Spr);
			}
			if (this.texture_width == 0f)
			{
				return this;
			}
			float num = this.uv_left;
			float num2 = this.uv_top;
			float num3 = this.uv_width;
			float num4 = this.uv_height;
			if (flipx)
			{
				this.uv_left += this.uv_width;
				this.uv_width *= -1f;
			}
			if (flipy)
			{
				this.uv_top += this.uv_height;
				this.uv_height *= -1f;
			}
			this.RectBL(x, y, dw, dh, false);
			this.uv_left = num;
			this.uv_top = num2;
			this.uv_width = num3;
			this.uv_height = num4;
			return this;
		}

		public MeshDrawer DrawGraph(float x, float y, Sprite Spr = null)
		{
			return this.DrawScaleGraph2(x, y, 0f, 0f, 1f, 1f, Spr);
		}

		public MeshDrawer RotaGraph(float x, float y, float scale = 1f, float rotR = 0f, Sprite Spr = null, bool flip = false)
		{
			return this.RotaGraph3(x, y, 0.5f, 0.5f, scale, scale, rotR, Spr, flip);
		}

		public MeshDrawer RotaGraphSP(float x, float y, float scale = 1f, float rotR = 0f, Sprite Spr = null, bool flip = false)
		{
			return this.RotaGraph3(x, y, Spr.pivot.x / Spr.rect.width, Spr.pivot.y / Spr.rect.height, scale, scale, rotR, Spr, flip);
		}

		public MeshDrawer RotaGraph2(float x, float y, float pivot_x = 0.5f, float pivot_y = 0.5f, float scale = 1f, float rotR = 0f, Sprite Spr = null, bool flip = false)
		{
			return this.RotaGraph3(x, y, pivot_x, pivot_y, scale, scale, rotR, Spr, flip);
		}

		public MeshDrawer RotaGraph3(float x, float y, float pivot_x = 0.5f, float pivot_y = 0.5f, float scalex = 1f, float scaley = 1f, float rotR = 0f, Sprite Spr = null, bool flip = false)
		{
			if (Spr != null)
			{
				this.initForImg(Spr);
			}
			if (scalex == 1f && scaley == 1f && rotR == 0f && !flip)
			{
				return this.DrawScaleGraph2(x, y, pivot_x, pivot_y, 1f, 1f, null);
			}
			bool flag = this.transformed;
			Matrix4x4 matrixTransform = this.MatrixTransform;
			this.Translate(x * this.ppur, y * this.ppur, true);
			if (rotR != 0f)
			{
				this.Rotate(rotR, true);
			}
			this.Scale(flip ? (-scalex) : scalex, scaley, true);
			this.DrawScaleGraph2(0f, 0f, pivot_x, pivot_y, 1f, 1f, null);
			if (flag)
			{
				this.setCurrentMatrix(matrixTransform, false);
			}
			else
			{
				this.Identity();
			}
			return this;
		}

		public MeshDrawer RotaL(float x, float y, PxlLayer L, bool no_initimg = false, bool fix_pxl_pos = false, int draw_margin_px = 0)
		{
			if (!no_initimg)
			{
				this.initForImg(L.Img, draw_margin_px);
			}
			int width = L.Img.width;
			int height = L.Img.height;
			if (fix_pxl_pos)
			{
				if (width % 2 == 1)
				{
					x += 0.5f;
				}
				if (height % 2 == 1)
				{
					y -= 0.5f;
				}
			}
			if (L.zmx == 1f && L.zmy == 1f && L.rotR == 0f)
			{
				return this.Rect(x + L.x, y - L.y, (float)(width + draw_margin_px * 2), (float)(height + draw_margin_px * 2), false);
			}
			bool flag = this.transformed;
			Matrix4x4 currentMatrix = this.getCurrentMatrix();
			this.Translate((x + L.x) * this.ppur, (y - L.y) * this.ppur, true);
			if (L.rotR != 0f)
			{
				this.Rotate(-L.rotR, true);
			}
			this.Scale(L.zmx, L.zmy, true);
			this.Rect(0f, 0f, (float)(width + draw_margin_px * 2), (float)(height + draw_margin_px * 2), false);
			if (flag)
			{
				this.setCurrentMatrix(currentMatrix, false);
			}
			else
			{
				this.Identity();
			}
			return this;
		}

		public MeshDrawer RotaMesh(float x, float y, float scalex = 1f, float scaley = 1f, float rotR = 0f, PxlMeshDrawer Ms = null, bool flip = false, bool get_color = false)
		{
			if (Ms == null)
			{
				return this;
			}
			Vector2[] array;
			Color32[] array2;
			Vector3[] array3;
			int[] array4;
			int num;
			int num2;
			if (!Ms.getRawVerticesAndTriangles(out array, out array2, out array3, out array4, out num, out num2))
			{
				return this.RotaMesh(x, y, scalex, scaley, rotR, Ms.getMesh(false), flip, get_color, 0, -1, 0, -1);
			}
			return this.RotaMesh(x, y, scalex, scaley, rotR, array3, get_color ? array2 : null, array, null, null, array4, flip, 0, num2, 0, num);
		}

		public MeshDrawer RotaMesh(float x, float y, float scalex = 1f, float scaley = 1f, float rotR = 0f, Mesh Ms = null, bool flip = false, bool get_color = false, int tri_start_i = 0, int tri_end_i = -1, int ver_start_i = 0, int ver_end_i = -1)
		{
			if (Ms == null)
			{
				return this;
			}
			Vector3[] vertices = Ms.vertices;
			Vector2[] uv = Ms.uv;
			Color32[] array = (get_color ? Ms.colors32 : null);
			return this.RotaMesh(x, y, scalex, scaley, rotR, vertices, array, uv, Ms.uv2, null, Ms.triangles, flip, tri_start_i, tri_end_i, ver_start_i, ver_end_i);
		}

		public MeshDrawer RotaTempMeshDrawer(float x, float y, float scalex = 1f, float scaley = 1f, float rotR = 0f, MeshDrawer Ms = null, bool flip = false, bool get_color = false, int tri_start_i = 0, int tri_end_i = -1, int ver_start_i = 0, int ver_end_i = -1)
		{
			Vector3[] vertexArray = Ms.getVertexArray();
			Vector2[] uvArray = Ms.getUvArray();
			Color32[] array = (get_color ? Ms.getColorArray() : null);
			return this.RotaMesh(x, y, scalex, scaley, rotR, vertexArray, array, uvArray, Ms.use_uv2 ? Ms.getUv2Array(false) : null, null, Ms.getTriangleArray(), flip, tri_start_i, (tri_end_i < 0) ? Ms.getTriMax() : tri_end_i, ver_start_i, (ver_end_i < 0) ? Ms.getVertexMax() : ver_end_i);
		}

		public MeshDrawer RotaMesh(float x, float y, float scalex, float scaley, float rotR, Vector3[] _AVer, Color32[] _Acol, Vector2[] _Auv, Vector2[] _Auv2, Vector2[] _Auv3, int[] _Atri, bool flip = false, int tri_start_i = 0, int tri_end_i = -1, int ver_start_i = 0, int ver_end_i = -1)
		{
			bool flag = this.transformed;
			Matrix4x4 matrixTransform = this.MatrixTransform;
			Matrix4x4 matrix4x = matrixTransform * Matrix4x4.Translate(new Vector3(x * this.ppur, y * this.ppur, 0f));
			if (scalex != 1f || scaley != 1f || flip || rotR != 0f)
			{
				matrix4x = matrix4x * Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, rotR * 0.31830987f * 180f)) * Matrix4x4.Scale(new Vector3(flip ? (-scalex) : scalex, scaley, 1f));
			}
			bool flag2 = this.fliped;
			this.Identity();
			this.fliped = flag2 != flip;
			this.fliped = scalex * scaley < 0f != this.fliped;
			this.transformed = this.fliped;
			Color32 col = this.Col;
			bool flag3 = _Auv == null || _Auv.Length == 0;
			if (tri_end_i < 0)
			{
				tri_end_i = _Atri.Length;
			}
			this.allocVer(this.ver_i + ver_end_i - ver_start_i, 64);
			this.allocTri(this.tri_i + tri_end_i - tri_start_i, 60);
			for (int i = tri_start_i; i < tri_end_i; i++)
			{
				this.Tri(_Atri[i] - ver_start_i);
			}
			if (_Auv2 != null && _Auv2.Length == 0)
			{
				_Auv2 = null;
			}
			this.fliped = (this.transformed = false);
			if (ver_end_i < 0)
			{
				ver_end_i = _AVer.Length;
			}
			if (!flag3)
			{
				this.uv_settype = UV_SETTYPE.NONE;
			}
			for (int j = ver_start_i; j < ver_end_i; j++)
			{
				if (_Acol != null)
				{
					this.Col = MeshDrawer.ColBuf1.Set(col).multiply(_Acol[j], true).C;
				}
				Vector3 vector = _AVer[j];
				if (!flag3)
				{
					Vector2 vector2 = _Auv[j];
					this.uv_left = vector2.x;
					this.uv_top = vector2.y;
					if (_Auv2 != null)
					{
						this.Uv2(vector2.x, vector2.y, true);
					}
				}
				vector = matrix4x.MultiplyPoint3x4(vector);
				this.Pos(vector.x, vector.y, null);
			}
			if (_Acol != null)
			{
				this.Col = col;
			}
			if (flag)
			{
				this.setCurrentMatrix(matrixTransform, false);
			}
			else
			{
				this.Identity();
			}
			return this;
		}

		public MeshDrawer RotaPF(float x, float y, float scalex = 1f, float scaley = 1f, float rotR = 0f, PxlFrame F = null, bool flip = false, bool consider_alpha = false, bool ignore_visible = false, uint draw_frame_bits = 4294967295U, bool fix_pxl_pos = false, int draw_margin_px = 0)
		{
			return this.RotaPF(x, y, false, scalex, scaley, rotR, F, flip, consider_alpha, ignore_visible, draw_frame_bits, draw_margin_px);
		}

		public MeshDrawer RotaPF(float x, float y, bool fix_pxl_pos, float scalex, float scaley, float rotR = 0f, PxlFrame F = null, bool flip = false, bool consider_alpha = false, bool ignore_visible = false, uint draw_frame_bits = 4294967295U, int draw_margin_px = 0)
		{
			if (F == null)
			{
				return this;
			}
			bool flag = this.transformed;
			Matrix4x4 matrixTransform = this.MatrixTransform;
			Matrix4x4 matrix4x = matrixTransform * Matrix4x4.Translate(new Vector3(x * this.ppur, y * this.ppur, 0f));
			if (scalex != 1f || scaley != 1f || flip || rotR != 0f)
			{
				matrix4x = matrix4x * Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, rotR * 0.31830987f * 180f)) * Matrix4x4.Scale(new Vector3(flip ? (-scalex) : scalex, scaley, 1f));
			}
			this.setCurrentMatrix(matrix4x, false);
			Color32 col = this.Col;
			C32 colBuf = MeshDrawer.ColBuf0;
			int num = F.countLayers();
			int num2 = 0;
			for (int i = 0; i < num; i++)
			{
				PxlLayer layer = F.getLayer(i);
				if (((ulong)draw_frame_bits & (ulong)(1L << (i & 31))) != 0UL && (ignore_visible || layer.alpha > 0f))
				{
					num2++;
				}
			}
			this.allocVer(this.ver_i + num2 * 4, 0);
			this.allocTri(this.tri_i + num2 * 6, 0);
			for (int j = 0; j < num; j++)
			{
				PxlLayer layer2 = F.getLayer(j);
				if (((ulong)draw_frame_bits & (ulong)(1L << (j & 31))) != 0UL && (ignore_visible || layer2.alpha > 0f))
				{
					if (consider_alpha)
					{
						this.Col = MeshDrawer.ColBuf0.Set(col).mulA(X.Mx(0f, layer2.alpha * 0.01f)).C;
					}
					this.RotaL(0f, 0f, layer2, false, fix_pxl_pos, draw_margin_px);
				}
			}
			if (consider_alpha)
			{
				this.Col = col;
			}
			if (flag)
			{
				this.setCurrentMatrix(matrixTransform, false);
			}
			else
			{
				this.Identity();
			}
			return this;
		}

		public MeshDrawer LoopTextureBL(float bl_x, float bl_y, float w, float h, PxlImage Img, float l_scroll_x, float b_scroll_y, float texture_scale = 1f)
		{
			return this.LoopTexture(bl_x, bl_y, bl_x, bl_y + h, bl_x + w, bl_y, Img, l_scroll_x, b_scroll_y, texture_scale * ((float)Img.width / w), texture_scale * ((float)Img.height / h));
		}

		public MeshDrawer LoopTexture(float bl_x, float bl_y, float lt_x, float lt_y, float rb_x, float rb_y, PxlImage Img, float l_scroll_x, float b_scroll_y, float appear_block_ratio_x, float appear_block_ratio_y)
		{
			if (appear_block_ratio_x <= 0f || appear_block_ratio_y <= 0f)
			{
				return this;
			}
			if (this.ppu != 1f)
			{
				bl_x *= this.ppur;
				bl_y *= this.ppur;
				lt_x *= this.ppur;
				lt_y *= this.ppur;
				rb_x *= this.ppur;
				rb_y *= this.ppur;
			}
			this.initForImgAndTexture(Img.get_I());
			float num = (float)Img.width;
			float num2 = (float)Img.height;
			while (l_scroll_x < 0f)
			{
				l_scroll_x += num;
			}
			while (b_scroll_y < 0f)
			{
				b_scroll_y += num2;
			}
			l_scroll_x -= (float)((int)(l_scroll_x / num)) * num;
			b_scroll_y -= (float)((int)(b_scroll_y / num2)) * num2;
			float num3 = rb_x - bl_x;
			float num4 = rb_y - bl_y;
			float num5 = lt_x - bl_x;
			float num6 = lt_y - bl_y;
			float num7 = b_scroll_y;
			float num8 = bl_x;
			float num9 = bl_y;
			float num10 = 1f / this.texture_width;
			float num11 = 1f / this.texture_height;
			float x = Img.RectIUv.x;
			float y = Img.RectIUv.y;
			float num12 = 0f;
			while (num12 < 1f)
			{
				float num13 = l_scroll_x + num;
				float num14 = appear_block_ratio_x;
				if (num13 > num)
				{
					num13 = num;
					num14 *= (num13 - l_scroll_x) / num;
					if (num14 <= 0f)
					{
						l_scroll_x = 0f;
						continue;
					}
				}
				if (num12 + num14 > 1f)
				{
					num14 = 1f - num12;
					if (num14 <= 0f)
					{
						break;
					}
					num13 = l_scroll_x + num * num14 / appear_block_ratio_x;
					num12 = 1f;
				}
				else
				{
					num12 += num14;
				}
				float num15 = num8;
				float num16 = num9;
				float num17 = num8 + num3 * num14;
				float num18 = num9 + num4 * num14;
				float num19 = 0f;
				num8 = num17;
				num9 = num18;
				b_scroll_y = num7;
				while (num19 < 1f)
				{
					float num20 = b_scroll_y + num2;
					float num21 = appear_block_ratio_y;
					if (num20 > num2)
					{
						num20 = num2;
						num21 *= (num20 - b_scroll_y) / num2;
						if (num21 <= 0f)
						{
							b_scroll_y = 0f;
							continue;
						}
					}
					if (num19 + num21 > 1f)
					{
						num21 = 1f - num19;
						if (num21 <= 0f)
						{
							break;
						}
						num20 = b_scroll_y + num2 * num21 / appear_block_ratio_y;
						num19 = 1f;
					}
					else
					{
						num19 += num21;
					}
					float num22 = num15 + num5 * num21;
					float num23 = num16 + num6 * num21;
					float num24 = num17 + num5 * num21;
					float num25 = num18 + num6 * num21;
					this.TriRectBL(0).uvRectN(x + l_scroll_x * num10, y + b_scroll_y * num11).Pos(num15, num16, null)
						.uvRectN(x + l_scroll_x * num10, y + num20 * num11)
						.Pos(num22, num23, null)
						.uvRectN(x + num13 * num10, y + num20 * num11)
						.Pos(num24, num25, null)
						.uvRectN(x + num13 * num10, y + b_scroll_y * num11)
						.Pos(num17, num18, null);
					num15 = num22;
					num16 = num23;
					num17 = num24;
					num18 = num25;
					b_scroll_y = ((num20 >= num2) ? 0f : num20);
				}
				l_scroll_x = ((num13 >= num) ? 0f : num13);
			}
			return this;
		}

		public MeshDrawer uvRect(float _uv_left, float _uv_top, float _uv_width, float _uv_height, bool manual_mode = true, bool auto_mode = false)
		{
			return this.uvRect(_uv_left, _uv_top, _uv_width, _uv_height, 0f, 0f, 1f, 1f, manual_mode, auto_mode);
		}

		public MeshDrawer uvRect(float _uv_left, float _uv_top, float _uv_width, float _uv_height, PxlImage PImgArea, bool manual_mode = true, bool auto_mode = false)
		{
			return this.uvRect(_uv_left, _uv_top, _uv_width, _uv_height, PImgArea.RectIUv.x, PImgArea.RectIUv.y, PImgArea.RectIUv.width, PImgArea.RectIUv.height, manual_mode, auto_mode);
		}

		public MeshDrawer uvRect(float _uv_left, float _uv_top, float _uv_width, float _uv_height, float area_left, float area_bottom, float area_width, float area_height, bool manual_mode = true, bool auto_mode = false)
		{
			this.uv_width = _uv_width / area_width;
			this.uv_height = _uv_height / area_height;
			this.uv_left = _uv_left - this.uv_width * area_left;
			this.uv_top = _uv_top - this.uv_height * area_bottom;
			if (manual_mode)
			{
				this.uv_settype = UV_SETTYPE.MANUAL;
			}
			else if (auto_mode)
			{
				this.uv_settype = UV_SETTYPE.AUTO;
			}
			return this;
		}

		public MeshDrawer uvRectN(float _uv_left, float _uv_top)
		{
			this.uv_left = _uv_left;
			this.uv_top = _uv_top;
			this.uv_settype = UV_SETTYPE.NONE;
			return this;
		}

		public void Draw(int top_layer, int bottom_layer, Matrix4x4 DestMatrix, bool temporary = false, Camera CameraForMesh = null)
		{
			if (this.isOffLineState())
			{
				return;
			}
			int num = (((this.state & MeshDrawer.STATE.TOP) > (MeshDrawer.STATE)0) ? top_layer : bottom_layer);
			if (this.ver_i == 0)
			{
				this.state = (this.state & (MeshDrawer.STATE)226) | MeshDrawer.STATE.OFFLINE_ALWAYS;
				return;
			}
			if (!temporary)
			{
				this.state |= MeshDrawer.STATE.OFFLINE;
			}
			MaterialPropertyBlock materialPropertyBlock = null;
			if (this.colorassign2mtr)
			{
				materialPropertyBlock = this.materialPropertyBlock;
				materialPropertyBlock.Clear();
				materialPropertyBlock.SetColor("_MainColor", this.Col);
				materialPropertyBlock.SetColor("_TintColor", this.Col);
				materialPropertyBlock.SetColor("_Color", this.Col);
				if (this.gradation)
				{
					materialPropertyBlock.SetColor("_Color2", this.ColGrd.C);
				}
			}
			try
			{
				this.arraysSubtraction();
				if (num >= 0)
				{
					if (this.valotile)
					{
						BLIT.RenderToGLMtr(this, 0f, 0f, 1f, this.Mtr, DestMatrix, this.tri_i, false, false);
					}
					else
					{
						this.Mtr.renderQueue = this.render_queue_;
						Graphics.DrawMesh(this.Ms, DestMatrix, this.Mtr, num, CameraForMesh, 0, materialPropertyBlock, false, false, false);
						this.Mtr.renderQueue = this.render_queue0;
					}
				}
			}
			catch
			{
				X.dl("mesh draw error", null, false, false);
			}
			if (!temporary && !this.valotile)
			{
				this.resetVertexCount();
				if (this.TriMulti != null)
				{
					this.TriMulti.resetVertexCount();
				}
			}
		}

		public void RedrawSameMesh(int top_layer, int bottom_layer, Matrix4x4 DestMatrix, Camera CameraForMesh = null, bool no_z_scale2zero = false)
		{
			if ((this.state & MeshDrawer.STATE.OFFLINE_ALWAYS) > (MeshDrawer.STATE)0)
			{
				return;
			}
			int num = (((this.state & MeshDrawer.STATE.TOP) > (MeshDrawer.STATE)0) ? top_layer : bottom_layer);
			if (num < 0)
			{
				return;
			}
			if (!this.valotile)
			{
				this.Mtr.renderQueue = this.render_queue_;
				Graphics.DrawMesh(this.Ms, DestMatrix, this.Mtr, num, CameraForMesh, 0, this.materialPropertyBlock, false, false, false);
				this.Mtr.renderQueue = this.render_queue0;
				return;
			}
			if (this.tri_i <= 0)
			{
				return;
			}
			BLIT.RenderToGLMtr(this, 0f, 0f, 1f, this.Mtr, DestMatrix, this.tri_i, false, no_z_scale2zero);
		}

		public void updateForMeshRenderer(bool temporary = false)
		{
			this.arraysSubtraction();
			if (this.Valot != null)
			{
				this.Valot.arraySubtraction(temporary, this.TriMulti);
			}
			if (!temporary && !this.draw_gl_only)
			{
				this.resetVertexCount();
				if (this.TriMulti != null)
				{
					this.TriMulti.resetVertexCount();
				}
			}
		}

		public MeshDrawer Offline()
		{
			if ((this.state & MeshDrawer.STATE.OFFLINE_ALWAYS) == (MeshDrawer.STATE)0)
			{
				this.state |= MeshDrawer.STATE.OFFLINE;
			}
			return this;
		}

		public void resetVertexCount()
		{
			this.ver_i = (this.uv2_i = (this.uv3_i = 0));
			this.tri_i = 0;
		}

		public MeshDrawer setPointsAlpha(float alpha01)
		{
			int num = this.Acolor.Length;
			byte b = (byte)(alpha01 * 255f);
			for (int i = num - 1; i >= 0; i--)
			{
				this.Acolor[i].a = b;
			}
			if (!this.draw_gl_only && this.Ms != null)
			{
				this.Ms.colors32 = this.Acolor;
			}
			return this;
		}

		public MeshDrawer InitSubMeshContainer(int first_i = 0)
		{
			if (this.TriMulti == null)
			{
				this.TriMulti = new MeshDrawerTriMultipleManager(this, first_i);
			}
			return this;
		}

		public MeshDrawer chooseSubMesh(int i, bool create_mesh_arranger = false, bool clearing = false)
		{
			this.InitSubMeshContainer(0);
			MeshDrawerTriMultipleManager.TMItem tmitem;
			if (i != this.TriMulti.get_cur())
			{
				this.TriMulti.Save();
				tmitem = this.TriMulti.Choose(i, clearing);
				if (clearing)
				{
					tmitem.clearIndex();
				}
				this.readFromTriMultiItem(tmitem);
			}
			else
			{
				tmitem = this.TriMulti.Get(i);
				if (tmitem == null)
				{
					tmitem = this.TriMulti.Choose(i, clearing);
				}
				if (clearing)
				{
					tmitem.clearIndex();
					this.tri_i = 0;
				}
			}
			if (create_mesh_arranger)
			{
				if (tmitem.Ma == null)
				{
					tmitem.Ma = new MdArranger(this);
				}
				tmitem.Ma.Set(true);
			}
			return this;
		}

		public MdArranger chooseSubMeshArranger(int i)
		{
			if (this.TriMulti == null)
			{
				return null;
			}
			MeshDrawerTriMultipleManager.TMItem tmitem = this.TriMulti.Get(i);
			if (tmitem == null)
			{
				return null;
			}
			return tmitem.Ma;
		}

		public int getCurrentSubMeshIndex()
		{
			if (this.TriMulti == null)
			{
				return 0;
			}
			return this.TriMulti.get_cur();
		}

		public int[] getSubMeshData(int i, ref int t_max)
		{
			t_max = 0;
			if (this.TriMulti == null)
			{
				return null;
			}
			MeshDrawerTriMultipleManager.TMItem tmitem = this.TriMulti.Get(i);
			if (tmitem != null)
			{
				t_max = tmitem.tri_max;
				return tmitem.Atri;
			}
			return null;
		}

		public bool getSubMeshMaterialActive(int i)
		{
			if (this.TriMulti == null)
			{
				return i == 0;
			}
			return this.TriMulti.isMaterialActive(i);
		}

		public Material getSubMeshMaterial(int i)
		{
			if (this.TriMulti == null)
			{
				return this.Mtr;
			}
			MeshDrawerTriMultipleManager.TMItem tmitem = this.TriMulti.Get(i);
			if (tmitem != null)
			{
				return tmitem.Mtr;
			}
			return this.Mtr;
		}

		public int getSubMeshAssignedIndex(Material Mtr)
		{
			if (this.TriMulti == null)
			{
				if (!(this.Mtr == Mtr))
				{
					return -1;
				}
				return 0;
			}
			else
			{
				Material[] attachedMaterialArray = this.TriMulti.getAttachedMaterialArray();
				if (attachedMaterialArray != null)
				{
					int num = attachedMaterialArray.Length;
					for (int i = 0; i < num; i++)
					{
						if (attachedMaterialArray[i] == Mtr)
						{
							return i;
						}
					}
					return -1;
				}
				if (!(this.Mtr == Mtr))
				{
					return -1;
				}
				return 0;
			}
		}

		public MdArranger finalizeSubMeshArraTrnger(bool secure_tri_ver_length = false)
		{
			if (this.TriMulti == null)
			{
				return null;
			}
			MeshDrawerTriMultipleManager.TMItem current = this.TriMulti.getCurrent();
			if (current == null)
			{
				return null;
			}
			if (!secure_tri_ver_length)
			{
				if (current.Ma != null)
				{
					if (!current.secure_tri_ver_length)
					{
						current.Ma.Set(false);
					}
					else
					{
						current.Ma.allocateAfterIndex();
					}
				}
			}
			else
			{
				if (current.Ma == null)
				{
					current.Ma = new MdArranger(this);
					current.Ma.Set(true);
				}
				current.Ma.Set(false);
				current.tri_max = this.tri_i;
				current.secure_tri_ver_length = true;
			}
			return current.Ma;
		}

		public MeshDrawer MergeSubMeshAll(MeshDrawer Src, float _dxu = 0f, float _dyu = 0f)
		{
			if (Src.TriMulti == null)
			{
				return this.RotaTempMeshDrawer(0f, 0f, 1f, 1f, 0f, Src, false, true, 0, -1, 0, -1);
			}
			if (this.TriMulti == null)
			{
				this.readFromTriMultiItem(Src.TriMulti.getCurrent());
				this.InitSubMeshContainer(Src.getCurrentSubMeshIndex());
			}
			else
			{
				this.TriMulti.Save();
			}
			int num = this.ver_i;
			this.MergeVerticeList(Src);
			if (_dxu != 0f || _dyu != 0f)
			{
				Vector3 vector = new Vector3(_dxu, _dyu, 0f);
				for (int i = num; i < this.ver_i; i++)
				{
					this.AVertice[i] += vector;
				}
			}
			int count = Src.TriMulti.Count;
			for (int j = 0; j < count; j++)
			{
				MeshDrawerTriMultipleManager.TMItem tmitem = Src.TriMulti.Get(j);
				if (tmitem != null)
				{
					MeshDrawerTriMultipleManager.TMItem tmitem2 = this.TriMulti.Add(j, tmitem, num);
					if (j == this.TriMulti.get_cur())
					{
						this.readFromTriMultiItem(tmitem2);
					}
				}
			}
			return this;
		}

		public MeshDrawer MergeVerticeList(MeshDrawer Src)
		{
			int num = Src.ver_i;
			X.pushA<Vector3>(ref this.AVertice, Src.AVertice, this.ver_i, num);
			X.pushA<Color32>(ref this.Acolor, Src.Acolor, this.ver_i, num);
			X.pushA<Vector2>(ref this.AMeshUv, Src.AMeshUv, this.ver_i, num);
			if (Src.use_uv2)
			{
				this.allocUv2(Src.uv2_i, false);
				X.pushA<Vector2>(ref this.AMeshUv2, Src.AMeshUv2, this.uv2_i, Src.uv2_i);
				this.uv2_i += Src.uv2_i;
			}
			if (Src.use_uv3)
			{
				this.allocUv3(Src.uv3_i, false);
				X.pushA<Vector2>(ref this.AMeshUv3, Src.AMeshUv3, this.uv3_i, Src.uv3_i);
				this.uv3_i += Src.uv3_i;
			}
			this.ver_i += num;
			return this;
		}

		public void connectRendererToTriMulti(MeshRenderer Mrd)
		{
			if (this.TriMulti != null)
			{
				this.TriMulti.connectRendererToTriMulti(Mrd);
				return;
			}
			Mrd.sharedMaterials = this.getMaterialArray(false);
		}

		public MeshDrawer reverVerAndTriIndexFromCSM()
		{
			if (this.TriMulti == null)
			{
				return this;
			}
			MeshDrawerTriMultipleManager.TMItem current = this.TriMulti.getCurrent();
			if (current.Ma == null)
			{
				return this;
			}
			int num = 0;
			int num2 = 0;
			current.Ma.revertVerAndTriIndex(ref num, ref num2);
			return this;
		}

		public void writeToTriMultiItem(MeshDrawerTriMultipleManager.TMItem Tmi, bool replace_tri_array = false)
		{
			Tmi.Mtr = this.Mtr;
			if (!Tmi.secure_tri_ver_length)
			{
				Tmi.tri_max = this.tri_i;
			}
			if (replace_tri_array)
			{
				Tmi.Atri = (this.Atriangle = new int[60]);
			}
			else
			{
				Tmi.Atri = this.Atriangle;
			}
			Tmi.base_z = this.base_z;
			Tmi.gradation = this.gradation;
			Tmi.render_queue = this.render_queue;
			Tmi.render_queue0 = this.render_queue0;
			Tmi.mtr_cloned = this.material_cloned;
		}

		public void readFromTriMultiItem(MeshDrawerTriMultipleManager.TMItem Tmi)
		{
			this.Mtr = Tmi.Mtr;
			this.shader_name_ = null;
			this.Atriangle = Tmi.Atri;
			this.tri_i = Tmi.tri_max;
			this.gradation = Tmi.gradation;
			this.render_queue = Tmi.render_queue;
			this.render_queue0 = Tmi.render_queue0;
			this.material_cloned = Tmi.mtr_cloned;
			this.base_z = Tmi.base_z;
		}

		public int getSubMeshCount(bool only_if_material_active = false)
		{
			if (this.TriMulti == null)
			{
				return 1;
			}
			if (only_if_material_active)
			{
				int use_count = this.TriMulti.use_count;
				int num = 0;
				for (int i = 0; i < use_count; i++)
				{
					if (this.TriMulti.isMaterialActive(i))
					{
						num++;
					}
				}
				return num;
			}
			return this.TriMulti.Count;
		}

		public MdArranger finalizeSubMeshArranger(bool secure_tri_ver_length = false)
		{
			if (this.TriMulti == null)
			{
				return null;
			}
			MeshDrawerTriMultipleManager.TMItem current = this.TriMulti.getCurrent();
			if (current == null)
			{
				return null;
			}
			if (!secure_tri_ver_length)
			{
				if (current.Ma != null)
				{
					if (!current.secure_tri_ver_length)
					{
						current.Ma.Set(false);
					}
					else
					{
						current.Ma.allocateAfterIndex();
					}
				}
			}
			else
			{
				if (current.Ma == null)
				{
					current.Ma = new MdArranger(this);
					current.Ma.Set(true);
				}
				current.Ma.Set(false);
				current.tri_max = this.tri_i;
				current.secure_tri_ver_length = true;
			}
			return current.Ma;
		}

		public void arraysSubtraction()
		{
			if (this.valotile)
			{
				if (this.TriMulti != null)
				{
					this.TriMulti.Save();
				}
				return;
			}
			this.prepareMesh();
			string text = null;
			if (this.Valot != null)
			{
				text = Bench.P(this.Valot.ToString());
			}
			this.Ms.Clear();
			if (this.TriMulti == null)
			{
				int[] array = (this.use_cache ? this.AtriangleRendered : this.Atriangle);
				if (array == null)
				{
					array = (this.AtriangleRendered = new int[this.tri_i]);
				}
				else if (array.Length != this.tri_i)
				{
					if (this.use_cache)
					{
						array = (this.AtriangleRendered = new int[this.tri_i]);
					}
					else
					{
						Array.Resize<int>(ref array, this.tri_i);
						this.Atriangle = array;
					}
				}
				if (this.use_cache)
				{
					Array.Copy(this.Atriangle, array, this.tri_i);
				}
			}
			if (this.ver_i != 0)
			{
				Vector3[] array2 = (this.use_cache ? this.AVerticeRendered : this.AVertice);
				Vector2[] array3 = (this.use_cache ? this.AMeshUvRendered : this.AMeshUv);
				Color32[] array4 = (this.use_cache ? this.AcolorRendered : this.Acolor);
				if (array2 == null)
				{
					array2 = (this.AVerticeRendered = new Vector3[this.ver_i]);
					array3 = (this.AMeshUvRendered = new Vector2[this.ver_i]);
					array4 = (this.AcolorRendered = new Color32[this.ver_i]);
				}
				else if (array2.Length != this.ver_i)
				{
					if (this.use_cache)
					{
						array2 = (this.AVerticeRendered = new Vector3[this.ver_i]);
						array3 = (this.AMeshUvRendered = new Vector2[this.ver_i]);
						array4 = (this.AcolorRendered = new Color32[this.ver_i]);
					}
					else
					{
						Array.Resize<Vector3>(ref array2, this.ver_i);
						Array.Resize<Vector2>(ref array3, this.ver_i);
						Array.Resize<Color32>(ref array4, this.ver_i);
						this.AVertice = array2;
						this.AMeshUv = array3;
						this.Acolor = array4;
					}
				}
				if (this.use_cache)
				{
					Array.Copy(this.AMeshUv, array3, this.ver_i);
					Array.Copy(this.AVertice, array2, this.ver_i);
					Array.Copy(this.Acolor, array4, this.ver_i);
				}
				this.Ms.vertices = array2;
				this.Ms.uv = array3;
				if ((this.state & MeshDrawer.STATE.USE_UV2) > (MeshDrawer.STATE)0)
				{
					Vector2[] array5 = (this.use_cache ? this.AMeshUv2Rendered : this.AMeshUv2);
					if (array5 == null)
					{
						array5 = (this.AMeshUv2Rendered = new Vector2[this.ver_i]);
					}
					else if (array5.Length != this.ver_i)
					{
						if (this.use_cache)
						{
							array5 = (this.AMeshUv2Rendered = new Vector2[this.ver_i]);
						}
						else
						{
							Array.Resize<Vector2>(ref array5, this.ver_i);
							this.AMeshUv2 = array5;
						}
					}
					this.uv2_i = X.Mn(this.uv2_i, this.ver_i);
					if (this.use_cache)
					{
						Array.Copy(this.AMeshUv2, array5, X.Mn(this.AMeshUv2.Length, this.ver_i));
					}
					this.Ms.uv2 = array5;
				}
				if ((this.state & MeshDrawer.STATE.USE_UV3) > (MeshDrawer.STATE)0)
				{
					Vector2[] array6 = (this.use_cache ? this.AMeshUv3Rendered : this.AMeshUv3);
					if (array6 == null)
					{
						array6 = (this.AMeshUv3Rendered = new Vector2[this.ver_i]);
					}
					else if (array6.Length != this.ver_i)
					{
						if (this.use_cache)
						{
							array6 = (this.AMeshUv3Rendered = new Vector2[this.ver_i]);
						}
						else
						{
							Array.Resize<Vector2>(ref array6, this.ver_i);
							this.AMeshUv3 = array6;
						}
					}
					this.uv3_i = X.Mn(this.uv3_i, this.ver_i);
					if (this.use_cache)
					{
						Array.Copy(this.AMeshUv3, array6, X.Mn(this.AMeshUv3.Length, this.ver_i));
					}
					this.Ms.uv3 = array6;
				}
				this.Ms.colors32 = array4;
				if (this.TriMulti != null)
				{
					this.TriMulti.arraySubtraction(this.Ms, this.use_cache ? this.AtriangleRendered : null);
					this.Atriangle = this.TriMulti.getCurrent().Atri;
				}
				else
				{
					this.Ms.triangles = (this.use_cache ? this.AtriangleRendered : this.Atriangle);
				}
				this.Ms.RecalculateBounds();
			}
			if (this.Valot != null)
			{
				Bench.Pend(text);
			}
		}

		public bool use_uv2
		{
			get
			{
				return (this.state & MeshDrawer.STATE.USE_UV2) > (MeshDrawer.STATE)0;
			}
		}

		public bool use_uv3
		{
			get
			{
				return (this.state & MeshDrawer.STATE.USE_UV3) > (MeshDrawer.STATE)0;
			}
		}

		public DRect calcBounds(DRect R = null, int ver_max = -1)
		{
			if (R == null)
			{
				R = new DRect(this.activation_key, 0f, 0f, 0f, 0f, 0f);
			}
			else
			{
				R.Set(0f, 0f, 0f, 0f);
			}
			int num = ((ver_max < 0) ? this.ver_i : ver_max);
			for (int i = 0; i < num; i++)
			{
				Vector3 vector = this.AVertice[i];
				R.Expand(vector.x, vector.y, 0f, 0f, i >= 1);
			}
			return R;
		}

		public void allocateAfterIndex(int v, int t)
		{
			if (this.ver_i < v)
			{
				this.ver_i = v;
			}
			if (this.tri_i < t)
			{
				this.tri_i = t;
			}
		}

		public void setMaterialCloneFlag()
		{
			this.material_cloned = true;
		}

		public static MeshDrawer prepareMeshRenderer(GameObject gameObject, Material Mtr, float base_z = 0f, int queue = -1, MeshDrawer Md = null, bool use_valotile = false, bool draw_gl_only = false)
		{
			bool flag = false;
			if (Mtr == null)
			{
				Mtr = MTRX.getMtr(BLEND.NORMAL, -1);
				flag = false;
			}
			if (queue >= 0)
			{
				if (!flag)
				{
					Mtr = MTRX.newMtr(Mtr);
					flag = true;
				}
				Mtr.renderQueue = queue;
			}
			MeshRenderer meshRenderer;
			MeshFilter meshFilter = MeshDrawer.makeNormalMeshRendererAndFilter(gameObject, Mtr, out meshRenderer);
			if (Md == null)
			{
				Md = new MeshDrawer(null, 4, 6);
			}
			Md.use_cache = false;
			Md.draw_gl_only = draw_gl_only;
			if (use_valotile)
			{
				IN.GetOrAdd<ValotileRenderer>(gameObject).Init(Md, meshRenderer, true);
			}
			Md.activate(gameObject.name, Mtr, meshFilter, base_z);
			if (flag)
			{
				Md.setMaterial(Mtr, true);
			}
			return Md;
		}

		public static MeshFilter makeNormalMeshRendererAndFilter(GameObject gameObject, Material Mtr, out MeshRenderer MRD)
		{
			MRD = IN.GetOrAdd<MeshRenderer>(gameObject);
			MRD == null;
			if (MRD != null)
			{
				MeshDrawer.initMrd(MRD);
			}
			if (Mtr != null)
			{
				MRD.sharedMaterials = new Material[] { Mtr };
			}
			return IN.GetOrAdd<MeshFilter>(gameObject);
		}

		public static MeshRenderer initMrd(MeshRenderer MRD)
		{
			MRD.allowOcclusionWhenDynamic = false;
			MRD.receiveShadows = false;
			MRD.lightProbeUsage = LightProbeUsage.Off;
			MRD.reflectionProbeUsage = ReflectionProbeUsage.Off;
			MRD.shadowCastingMode = ShadowCastingMode.Off;
			MRD.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
			return MRD;
		}

		public static PxlMeshDrawer makeBasicSpriteMesh(Sprite Sp, uint col = 4294967295U, bool calc_normal_and_bounds = true, float ppu = 0f)
		{
			Rect rect = Sp.rect;
			float num = 1f / (float)Sp.texture.width;
			float num2 = 1f / (float)Sp.texture.height;
			return MeshDrawer.makeBasicSpriteMesh(Sp.texture, rect.x * num, rect.y * num2, rect.width * num, rect.height * num2, col, calc_normal_and_bounds, ppu);
		}

		public static PxlMeshDrawer makeBasicSpriteMesh(Texture Tx, float uvx, float uvy, float uvw, float uvh, uint col = 4294967295U, bool calc_normal_and_bounds = true, float ppu = 0f)
		{
			float num;
			if (ppu == 0f)
			{
				num = 0.015625f;
			}
			else
			{
				num = 1f / ppu;
			}
			Color32 color = C32.d2c(col);
			PxlMeshDrawer pxlMeshDrawer = new PxlMeshDrawer(true);
			Vector3[] array = new Vector3[4];
			float num2 = (float)Tx.width * uvw;
			float num3 = (float)Tx.height * uvh;
			float num4 = num2 / 2f;
			float num5 = num3 / 2f;
			array[0] = new Vector3(-num4 * num, -num5 * num);
			array[1] = new Vector3(-num4 * num, num5 * num);
			array[2] = new Vector3(num4 * num, num5 * num);
			array[3] = new Vector3(num4 * num, -num5 * num);
			pxlMeshDrawer.setRawVerticesAndTriangles(new Vector2[]
			{
				new Vector2(uvx, uvy),
				new Vector2(uvx, uvy + uvh),
				new Vector2(uvx + uvw, uvy + uvh),
				new Vector2(uvx + uvw, uvy)
			}, new Color32[] { color, color, color, color }, array, PxlMeshDrawer.Asimple_rect_tri, -1, -1);
			if (calc_normal_and_bounds)
			{
				Mesh mesh = pxlMeshDrawer.getMesh(false);
				if (mesh != null)
				{
					mesh.RecalculateNormals();
					mesh.RecalculateBounds();
				}
			}
			return pxlMeshDrawer;
		}

		public static Mesh mergeMesh(Mesh Dest, Mesh Src)
		{
			Mesh mesh = new Mesh();
			mesh.MarkDynamic();
			int vertexCount = Dest.vertexCount;
			int[] triangles = Dest.triangles;
			int[] triangles2 = Src.triangles;
			int num = triangles2.Length;
			int num2 = triangles.Length;
			num += num2;
			Array.Resize<int>(ref triangles, num);
			int num3 = 0;
			for (int i = num2; i < num; i++)
			{
				triangles[i] = vertexCount + triangles2[num3++];
			}
			int[] array = triangles;
			Vector3[] vertices = Dest.vertices;
			X.pushA<Vector3>(ref vertices, Src.vertices);
			mesh.vertices = vertices;
			Color32[] colors = Dest.colors32;
			X.pushA<Color32>(ref colors, Src.colors32);
			mesh.colors32 = colors;
			Vector2[] uv = Dest.uv;
			X.pushA<Vector2>(ref uv, Src.uv);
			mesh.uv = uv;
			mesh.triangles = array;
			return mesh;
		}

		public Mesh getMesh()
		{
			return this.Ms;
		}

		public PxlMeshDrawer createSimplePxlMesh(PxlMeshDrawer PMesh = null, bool uv_copy = true, bool color_copy = true, bool array_copy = true)
		{
			if (PMesh == null)
			{
				PMesh = new PxlMeshDrawer(false);
			}
			Vector3[] array = this.AVertice;
			Vector2[] array2 = (uv_copy ? this.AMeshUv : null);
			Color32[] array3 = (color_copy ? this.Acolor : null);
			int[] array4 = this.Atriangle;
			int num = (this.draw_gl_only ? this.getVertexMax() : array.Length);
			int draw_triangle_count = this.draw_triangle_count;
			if (array_copy)
			{
				array = new Vector3[num];
				Array.Copy(this.AVertice, array, num);
				if (uv_copy)
				{
					array2 = new Vector2[num];
					Array.Copy(this.AMeshUv, array2, num);
				}
				if (color_copy)
				{
					array3 = new Color32[num];
					Array.Copy(this.Acolor, array3, num);
				}
				array4 = new int[draw_triangle_count];
				Array.Copy(this.Atriangle, array4, draw_triangle_count);
			}
			else
			{
				if (array.Length != num)
				{
					Array.Resize<Vector3>(ref array, num);
					if (uv_copy)
					{
						Array.Resize<Vector2>(ref array2, num);
					}
					if (color_copy)
					{
						Array.Resize<Color32>(ref array3, num);
					}
				}
				if (array4.Length != draw_triangle_count)
				{
					Array.Resize<int>(ref array4, draw_triangle_count);
				}
			}
			PMesh.setRawVerticesAndTriangles(array2, array3, array, array4, num, draw_triangle_count);
			return PMesh;
		}

		public Color32 getColorTe()
		{
			if (this.Mtr.HasProperty("_Color"))
			{
				return MeshDrawer.ColBuf1.Set(this.Mtr.GetColor("_Color")).C;
			}
			return MTRX.ColWhite;
		}

		public void setColorTe(C32 Col, C32 CMul, C32 CAdd)
		{
			this.Mtr.SetColor("_Color", CMul.C);
			this.Mtr.SetColor("_AddColor", CAdd.C);
			this.Mtr.SetFloat("_UseAddColor", 1f);
		}

		public MeshDrawer setMtrColor(string property_title, Color32 _Col)
		{
			this.Mtr.SetColor(property_title, _Col);
			return this;
		}

		public MeshDrawer setMtrFloat(string property_title, float _val)
		{
			this.Mtr.SetFloat(property_title, _val);
			return this;
		}

		public MeshDrawer setMtrTexture(Texture Img)
		{
			this.Mtr.SetTexture("_MainTex", Img);
			return this;
		}

		public MeshDrawer setMtrTexture(string property_title, Texture Img)
		{
			this.Mtr.SetTexture(property_title, Img);
			return this;
		}

		public MeshDrawer initForImgAndTexture(Texture Img)
		{
			this.initForImg(Img);
			this.Mtr.SetTexture("_MainTex", Img);
			return this;
		}

		public MeshDrawer initForImgAndTexture(MImage Img)
		{
			this.initForImg(Img);
			this.Mtr.SetTexture("_MainTex", Img.Tx);
			return this;
		}

		public MeshDrawer initForImgAndTexture(string property_title, Texture Img)
		{
			this.initForImg(Img);
			this.Mtr.SetTexture(property_title, Img);
			return this;
		}

		public MeshDrawer setMtrTextureAndOffset(string property_title, Sprite Spr)
		{
			this.Mtr.SetTexture(property_title, Spr.texture);
			float num = 1f / (float)Spr.texture.width;
			float num2 = 1f / (float)Spr.texture.height;
			Rect rect = Spr.rect;
			this.Mtr.SetTextureOffset(property_title, new Vector2(rect.x * num, rect.y * num2));
			this.Mtr.SetTextureScale(property_title, new Vector2(rect.width * num, rect.height * num2));
			return this;
		}

		public MeshDrawer setMtrTextureAndOffset(string property_title, PxlImage Spr)
		{
			Texture i = Spr.get_I();
			this.Mtr.SetTexture(property_title, i);
			Rect rectIUv = Spr.RectIUv;
			this.Mtr.SetTextureOffset(property_title, new Vector2(rectIUv.x, rectIUv.y));
			this.Mtr.SetTextureScale(property_title, new Vector2(rectIUv.width, rectIUv.height));
			return this;
		}

		public bool isActive()
		{
			return !this.isOffLineState();
		}

		public int getVertexMax()
		{
			return this.ver_i;
		}

		public int getUv2Max()
		{
			return this.uv2_i;
		}

		public int getUv3Max()
		{
			return this.uv3_i;
		}

		public int getTriMax()
		{
			return this.tri_i;
		}

		public virtual bool isEmpty()
		{
			return this.ver_i <= 0 && (!(this.Ms != null) || this.Ms.vertexCount <= 0);
		}

		public void revertVerAndTriIndex(int v, int t, bool apply_to_uv23 = false)
		{
			this.ver_i = v;
			this.tri_i = t;
			if (apply_to_uv23)
			{
				if ((this.state & MeshDrawer.STATE.USE_UV2) > (MeshDrawer.STATE)0)
				{
					this.uv2_i = v;
				}
				if ((this.state & MeshDrawer.STATE.USE_UV3) > (MeshDrawer.STATE)0)
				{
					this.uv3_i = v;
				}
			}
		}

		public float get_ppu()
		{
			return this.ppu;
		}

		public bool isOffLineState()
		{
			return (this.state & MeshDrawer.STATE.OFFLINE) > (MeshDrawer.STATE)0 || (this.state & MeshDrawer.STATE.OFFLINE_ALWAYS) > (MeshDrawer.STATE)0;
		}

		public bool is_bottom
		{
			get
			{
				return (this.state & MeshDrawer.STATE.TOP) == (MeshDrawer.STATE)0;
			}
		}

		public Vector3[] getVertexArray()
		{
			return this.AVertice;
		}

		public Vector2[] getUvArray()
		{
			return this.AMeshUv;
		}

		public Vector2[] getUv2Array(bool if_only_using = false)
		{
			if (if_only_using && (this.state & MeshDrawer.STATE.USE_UV2) == (MeshDrawer.STATE)0)
			{
				return null;
			}
			return this.AMeshUv2;
		}

		public Vector2[] getUv3Array(bool if_only_using = false)
		{
			if (if_only_using && (this.state & MeshDrawer.STATE.USE_UV3) == (MeshDrawer.STATE)0)
			{
				return null;
			}
			return this.AMeshUv3;
		}

		public int[] getTriangleArray()
		{
			return this.Atriangle;
		}

		public Color32[] getColorArray()
		{
			return this.Acolor;
		}

		public static int fnSortMeshDrawer(MeshDrawer A, MeshDrawer B)
		{
			int num = A.render_queue_;
			int num2 = B.render_queue_;
			if (num != num2)
			{
				if (num <= num2)
				{
					return -1;
				}
				return 1;
			}
			else
			{
				if (A.base_z < B.base_z)
				{
					return 1;
				}
				if (A.base_z != B.base_z)
				{
					return -1;
				}
				return 0;
			}
		}

		public static int fnSortMeshDrawerExistsNull(MeshDrawer A, MeshDrawer B)
		{
			if (A != null && B != null)
			{
				return MeshDrawer.fnSortMeshDrawer(A, B);
			}
			if (A == null && B != null)
			{
				return 1;
			}
			if (A == null || B != null)
			{
				return 0;
			}
			return -1;
		}

		public int draw_triangle_count
		{
			get
			{
				if (this.draw_gl_only)
				{
					return this.getTriMax();
				}
				if (!this.use_cache)
				{
					return this.Atriangle.Length;
				}
				if (!(this.Ms == null))
				{
					return (int)this.Ms.GetIndexCount(0);
				}
				return 0;
			}
		}

		public bool valotile
		{
			get
			{
				return this.draw_gl_only || (this.Valot != null && this.Valot.valotile);
			}
		}

		public bool hasMultipleTriangle()
		{
			return this.TriMulti != null && this.TriMulti.Count > 0;
		}

		public float base_px_x
		{
			get
			{
				return this.base_x * this.ppu;
			}
			set
			{
				this.base_x = value * this.ppur;
			}
		}

		public float base_px_y
		{
			get
			{
				return this.base_y * this.ppu;
			}
			set
			{
				this.base_y = value * this.ppur;
			}
		}

		public float ppu_rev
		{
			get
			{
				return this.ppur;
			}
		}

		public string shader_name
		{
			get
			{
				if (this.Mtr == null)
				{
					return null;
				}
				if (this.shader_name_ == null)
				{
					this.shader_name_ = this.Mtr.shader.name;
				}
				return this.shader_name_;
			}
		}

		public override string ToString()
		{
			STB stb = TX.PopBld(null, 0);
			stb.Add(this.activation_key, "-<", this.shader_name, ">");
			stb.Add("... vert:");
			STB stb2 = stb + ((this.ver_i == 0 && !this.valotile) ? this.Ms.vertexCount : this.ver_i) + " tri:" + (int)((this.tri_i == 0 && !this.valotile) ? this.Ms.GetIndexCount(0) : ((uint)this.tri_i));
			string text = stb2.ToString();
			TX.ReleaseBld(stb2);
			return text;
		}

		public float pixelsPerUnit
		{
			get
			{
				return this.ppu;
			}
			set
			{
				if (this.ppu == value)
				{
					return;
				}
				this.ppu = value;
				this.ppur = 1f / this.ppu;
			}
		}

		public float base_x;

		public float base_y;

		public float base_z;

		private float ppu = 64f;

		private float ppur = 0.015625f;

		public Color32 Col = MTRX.ColWhite;

		public C32 ColGrd;

		public static C32 ColBuf0;

		public static C32 ColBuf1;

		protected int ver_i;

		protected int tri_i;

		private int uv2_i;

		private int uv3_i;

		protected Vector2[] AMeshUv;

		protected Color32[] Acolor;

		protected Vector3[] AVertice;

		protected Vector2[] AMeshUv2;

		protected Vector2[] AMeshUv3;

		protected int[] Atriangle;

		private Vector2[] AMeshUvRendered;

		private Color32[] AcolorRendered;

		private Vector3[] AVerticeRendered;

		private Vector2[] AMeshUv2Rendered;

		private Vector2[] AMeshUv3Rendered;

		private int[] AtriangleRendered;

		protected Mesh Ms;

		private Matrix4x4 MatrixTransform = Matrix4x4.identity;

		private bool transformed;

		public bool fliped;

		public string activation_key = "";

		private string shader_name_;

		public static Vector3 BufTrs;

		public MeshDrawer.FnMeshPointColor fnMeshPointColor;

		protected MeshDrawerTriMultipleManager TriMulti;

		public bool gradation;

		public bool colorassign2mtr;

		protected Material Mtr;

		private bool material_cloned;

		private bool mesh_cloned;

		private int render_queue0 = 3000;

		private int render_queue_ = 3000;

		public UV_SETTYPE uv_settype;

		public float uv_left;

		public float uv_top;

		public float uv_width;

		public float uv_height;

		public float texture_width;

		public float texture_height;

		public ValotileRenderer Valot;

		public MaterialPropertyBlock materialPropertyBlock;

		private MeshDrawer.STATE state = (MeshDrawer.STATE)3;

		public static Vector2[] AStrpPts = new Vector2[4];

		public delegate C32 FnMeshPointColor(MeshDrawer Md, float pixel_x, float pixel_y, C32 DefCol, float v0, float v1);

		public delegate bool FnGeneralDraw(MeshDrawer Md, float alpha);

		public delegate float FnMathLineHeight(float level01);

		private enum STATE
		{
			OFFLINE = 1,
			TOP,
			USE_UV2 = 4,
			USE_UV3 = 8,
			USE_UV4 = 16,
			DRAW_GL_ONLY = 32,
			USE_CACHE = 64,
			USE_Z_TRANSFORM = 128,
			DRAWING_OPT = 224,
			OFFLINE_ALWAYS = 1024
		}
	}
}
