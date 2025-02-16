using System;
using System.Collections.Generic;
using Better;
using PixelLiner;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2ImageAtlas
	{
		private RenderTexture Tx
		{
			get
			{
				return this.CalcAtlas.Tx;
			}
		}

		public M2ImageAtlas(M2DBase _M2D, M2ImageContainer _IMGS)
		{
			this.M2D = _M2D;
			this.IMGS = _IMGS;
			this.ORc = new BDic<PxlImage, M2ImageAtlas.AtlasRect>();
			this.Aloaded_dir = new List<string>();
			this.Ainit_load_pxl = new List<string>();
			this.OFnForDropObject = new BDic<PxlImage, M2DropObject.FnDropObjectDraw>(8);
			this.OABlurImg = new BDic<M2ChipImage, List<M2BlurImage>>(4);
			this.APxcEntry = new List<M2ImageAtlas.AtlasPxcEntry>();
			this.ALayForLoading = new List<M2ImageAtlas.LayerEntry>();
			this.CalcAtlas = new RectAtlasTexture(0, 0, "M2ImageAtlas", false, 0, RenderTextureFormat.ARGB32);
			this.FD_FnSortFotAtlasAssign = new Comparison<M2ImageAtlas.LayerEntry>(this.FnSortFotAtlasAssign);
		}

		public void initAsyncLoad()
		{
			if (this.Acurrent_async_loading == null)
			{
				this.Acurrent_async_loading = new List<string>(2);
			}
		}

		public void quitAsyncLoad()
		{
			this.Acurrent_async_loading = null;
		}

		public bool pxl_already_loaded(string title)
		{
			for (int i = this.APxcEntry.Count - 1; i >= 0; i--)
			{
				if (this.APxcEntry[i].Pc.title == title)
				{
					return true;
				}
			}
			return false;
		}

		private bool is_initialize_need_load_pose(PxlPose P)
		{
			string title = P.title;
			return TX.isStart(title, "_anim", 0) || X.isinStr(M2DBase.Achip_pxlpose_necessary, title, -1) >= 0;
		}

		private void initLoadCharacter(PxlCharacter Pc)
		{
			int num = Pc.countPoses();
			for (int i = 0; i < num; i++)
			{
				PxlPose pose = Pc.getPose(i);
				if (this.is_initialize_need_load_pose(pose))
				{
					this.prepareChipImageDirectory(pose.title + "/", false);
				}
			}
		}

		public void initCspAtlas(string[] Apxl_key)
		{
			int num = Apxl_key.Length;
			for (int i = 0; i < num; i++)
			{
				string text = Apxl_key[i];
				this.initCspAtlas(text);
			}
		}

		public void initCspAtlas(string title)
		{
			if (TX.noe(title))
			{
				return;
			}
			PxlCharacter pxlCharacter = PxlsLoader.getPxlCharacter(title);
			string text = "MapChips/" + title + ".pxls";
			string text2 = text;
			string text3 = text;
			if (pxlCharacter == null)
			{
				if (this.pxl_already_loaded(title))
				{
					return;
				}
				byte[] array = null;
				if (array == null)
				{
					using (MTI mti = new MTI(text, "_"))
					{
						array = mti.LoadBytes(title + ".pxls");
					}
				}
				if (array != null)
				{
					pxlCharacter = PxlsLoader.loadCharacterASync(title, array, null, 64f, false);
					pxlCharacter.external_png_header = "MapChips/";
					pxlCharacter.no_load_external_texture_on_first = true;
				}
			}
			if (!this.pxl_already_loaded(title))
			{
				string text4 = text + ".bytes.texture_0";
				MImage mimage = null;
				MTIOneImage mtioneImage = null;
				if (mimage == null)
				{
					mtioneImage = MTI.LoadContainerOneImage(text4, null, null);
					if (mtioneImage.addLoadKey("M2D", !M2ImageAtlas.decline_async) && M2ImageAtlas.decline_async)
					{
						mimage = mtioneImage.MI;
					}
				}
				this.APxcEntry.Add(new M2ImageAtlas.AtlasPxcEntry(pxlCharacter, text3, text2, mimage, mtioneImage));
			}
		}

		public void destruct()
		{
			this.CalcAtlas.destruct();
			IN.DestroyOne(this.MtrMd);
			this.MtrMd = null;
		}

		public bool flushPxl()
		{
			if (Map2d.editor_decline_lighting)
			{
				return false;
			}
			int num = M2DBase.Achip_pxl_key.Length;
			for (int i = 0; i < num; i++)
			{
				string text = M2DBase.Achip_pxl_key[i];
				for (int j = this.APxcEntry.Count - 1; j >= 0; j--)
				{
					if (this.APxcEntry[j].Pc.title == text)
					{
						this.flushPxl(PxlsLoader.getPxlCharacter(text), j, false);
					}
				}
			}
			return true;
		}

		private void flushPxl(PxlCharacter Pxc, int pxl_i, bool set_dirty_flag = false)
		{
			if (Pxc == null)
			{
				return;
			}
			this.pre_dirname = null;
			int num = Pxc.countPoses();
			int count = this.Aloaded_dir.Count;
			bool flag = Pxc.title == M2DBase.Achip_pxl_key[0];
			for (int i = 0; i < num; i++)
			{
				string text = Pxc.getPose(i).title + "/";
				if (text == "obj/")
				{
					flag = true;
				}
				else if (this.Aloaded_dir.IndexOf(text) != -1)
				{
					if (this.Acurrent_async_loading != null && this.Acurrent_async_loading.IndexOf(text) != -1)
					{
						flag = true;
					}
					else
					{
						this.Aloaded_dir.Remove(text);
					}
				}
			}
			if (!flag)
			{
				Pxc.releaseLoadedExternalTexture(set_dirty_flag, null);
				MTI.ReleaseContainer("MapChips/" + Pxc.title + ".pxls" + ".bytes.texture_0", "M2D");
				if (set_dirty_flag)
				{
					PxlsLoader.disposeCharacter(Pxc.title, true);
				}
				MTRX.releaseMI(Pxc, true);
				this.APxcEntry.RemoveAt(pxl_i);
			}
			this.Ainit_load_pxl.Remove(Pxc.title);
			if (count > this.Aloaded_dir.Count)
			{
				this.ORc.Clear();
				if (this.loaded_index >= 0)
				{
					this.loaded_index = -1;
					this.fined_imgs_index = 0;
				}
			}
		}

		public MImage MIForPcr(PxlCharacter Char)
		{
			for (int i = this.APxcEntry.Count - 1; i >= 0; i--)
			{
				M2ImageAtlas.AtlasPxcEntry atlasPxcEntry = this.APxcEntry[i];
				if (atlasPxcEntry.Pc == Char)
				{
					return atlasPxcEntry.MI;
				}
			}
			return null;
		}

		public bool hasLoadingPxc()
		{
			for (int i = this.APxcEntry.Count - 1; i >= 0; i--)
			{
				M2ImageAtlas.AtlasPxcEntry atlasPxcEntry = this.APxcEntry[i];
				if (!atlasPxcEntry.Pc.isLoadCompleted() || !atlasPxcEntry.prepareImage(false))
				{
					return true;
				}
			}
			return false;
		}

		public bool prepareChipImageDirectory(M2ChipImage I, bool stop_m2d = false)
		{
			return this.prepareChipImageDirectory(I.dirname, stop_m2d);
		}

		public bool prepareChipImageDirectory(string dirname, bool stop_m2d = false)
		{
			if (dirname == this.pre_dirname)
			{
				return false;
			}
			this.pre_dirname = dirname;
			if (this.Acurrent_async_loading != null && this.Acurrent_async_loading.IndexOf(dirname) == -1)
			{
				this.Acurrent_async_loading.Add(dirname);
			}
			if (this.Aloaded_dir.IndexOf(dirname) != -1)
			{
				return false;
			}
			this.Aloaded_dir.Add(dirname);
			if (stop_m2d)
			{
				this.M2D.transferring_game_stopping = true;
			}
			return true;
		}

		public bool prepareAtlasProgress(int progress_count = 900)
		{
			bool flag;
			return this.prepareAtlasProgress(out flag, progress_count);
		}

		public bool prepareAtlasProgress(out bool executed, int progress_count = 900)
		{
			executed = false;
			if (this.loaded_index == -1 || this.Tx == null)
			{
				if (!this.fine_pre_tx)
				{
					this.Recreated_Pre_Texture = this.Tx;
					this.fine_pre_tx = true;
				}
				this.loaded_index = (this.fined_imgs_index = (this.first_fined_imgs_index = 0));
				this.atlas_rescale_x = (this.atlas_rescale_y = 0f);
				this.first_step_on_fine_image = true;
				this.ORc.Clear();
				this.ALayForLoading.Clear();
				this.CurEntry = null;
				this.CalcAtlas.Clear(2048, 2048);
				this.CalcAtlas.Tx.name = "M2ImageAtalas(" + this.CalcAtlas.width.ToString() + "," + this.CalcAtlas.height.ToString();
				this.CalcAtlas.copy_previous_image = false;
			}
			if (this.MdBuf == null)
			{
				this.MdBuf = new MeshDrawer(null, 4, 6)
				{
					draw_gl_only = true
				};
				this.MtrMd = this.MtrMd ?? MTRX.newMtr(MTRX.ShaderGDT);
				this.MdBuf.activate("", this.MtrMd, true, MTRX.ColWhite, null);
				this.MtrMd.EnableKeyword("NO_PIXELSNAP");
			}
			int count = this.APxcEntry.Count;
			bool flag = false;
			for (int i = count - 1; i >= 0; i--)
			{
				PxlCharacter pc = this.APxcEntry[i].Pc;
				if (!pc.isLoadCompleted())
				{
					flag = true;
				}
				else if (this.Ainit_load_pxl.IndexOf(pc.title) < 0)
				{
					this.Ainit_load_pxl.Add(pc.title);
					this.initLoadCharacter(pc);
				}
			}
			if (flag && this.alloc_m2d_loader_inactive)
			{
				executed = true;
				return true;
			}
			if (!this.isLoading())
			{
				return false;
			}
			executed = true;
			if (!Caching.ready)
			{
				return true;
			}
			this.fined_imgs_index = X.Abs(this.fined_imgs_index);
			for (int j = 0; j < 2; j++)
			{
				for (;;)
				{
					int num = ((j == 0) ? this.loaded_index : this.fined_imgs_index);
					if (num < this.Aloaded_dir.Count)
					{
						string text = this.Aloaded_dir[num];
						if (this.CurEntry == null)
						{
							string text2 = TX.slice(text, 0, text.Length - 1);
							this.CurEntryPose = null;
							if (j == 0)
							{
								this.calced_atlas_index = -1;
							}
							else
							{
								this.calced_layer_index = -1;
								this.ALayForLoading.Clear();
							}
							bool flag2 = false;
							int k = 0;
							while (k < count)
							{
								M2ImageAtlas.AtlasPxcEntry atlasPxcEntry = this.APxcEntry[k];
								PxlCharacter pc2 = atlasPxcEntry.Pc;
								this.CurEntryPose = pc2.getPoseByName(text2);
								bool flag3 = !pc2.isLoadCompleted();
								flag2 = flag2 || flag3;
								if (this.CurEntryPose != null)
								{
									if (flag3 || !atlasPxcEntry.prepareImage(true))
									{
										return true;
									}
									this.CurEntry = atlasPxcEntry;
									break;
								}
								else
								{
									k++;
								}
							}
							if (this.CurEntry == null)
							{
								if (flag2)
								{
									return true;
								}
								X.de(string.Concat(new string[]
								{
									"準備された",
									this.APxcEntry.Count.ToString(),
									" 個のPxlCharacter内にポーズ",
									text,
									" が見つかりませんでした。"
								}), null);
							}
						}
						if (this.CurEntry != null && (j == 0 || this.calced_layer_index == -1))
						{
							if (j == 0)
							{
								PxlCharacter pc3 = this.CurEntry.Pc;
							}
							if (j == 1)
							{
								if (TX.isStart(this.CurEntryPose.title, "_", 0))
								{
									this.CurEntry = null;
									this.CurEntryPose = null;
								}
								else
								{
									if (this.IMGS.initializeChipsDirectory(text, ref progress_count, false) || progress_count == 0)
									{
										return true;
									}
									if (this.first_step_on_fine_image)
									{
										this.fineMeshToTexture(null, false);
										this.first_step_on_fine_image = false;
										this.IMGS.assignPxlLayerInitialize();
										this.IMGS.MIchip.Tx = this.Tx;
										if (this.atlas_rescale_x == 0f)
										{
											this.IMGS.releaseAtlas(false, null);
										}
									}
								}
								this.calced_layer_index = 0;
							}
							if (this.CurEntryPose != null)
							{
								int num2 = 0;
								while ((long)num2 < 8L)
								{
									if (!this.CurEntryPose.isValidAim(num2) || this.CurEntryPose.isFlipped(num2))
									{
										num2++;
									}
									else
									{
										bool flag4 = TX.isStart(this.CurEntryPose.title, "_", 0);
										PxlSequence sequence = this.CurEntryPose.getSequence(num2);
										int num3 = sequence.countFrames();
										for (int l = 0; l < num3; l++)
										{
											PxlFrame frame = sequence.getFrame(l);
											int num4 = frame.countLayers();
											while (--num4 >= 0)
											{
												PxlLayer layer = frame.getLayer(num4);
												if (layer.isGroup())
												{
													if (TX.isStart(layer.name, "stamp_", 0))
													{
														num4 -= layer.Group.countLayersWhole();
													}
												}
												else
												{
													M2ImageAtlas.AtlasRect atlasRect;
													if (j == 0)
													{
														if (this.ORc.ContainsKey(layer.Img))
														{
															continue;
														}
														this.ORc[layer.Img] = default(M2ImageAtlas.AtlasRect);
													}
													else if (flag4 || !this.ORc.TryGetValue(layer.Img, out atlasRect) || !atlasRect.valid)
													{
														continue;
													}
													this.ALayForLoading.Add(new M2ImageAtlas.LayerEntry(layer, text));
												}
											}
										}
									}
									num2++;
								}
							}
						}
						if (j == 1)
						{
							while (this.CurEntry != null && this.ALayForLoading.Count > this.calced_layer_index)
							{
								List<M2ImageAtlas.LayerEntry> alayForLoading = this.ALayForLoading;
								int num5 = this.calced_layer_index;
								this.calced_layer_index = num5 + 1;
								PxlLayer lay = alayForLoading[num5].Lay;
								if (!lay.isGroup())
								{
									M2ImageAtlas.AtlasRect atlasRect2;
									if (!this.ORc.TryGetValue(lay.Img, out atlasRect2) || !atlasRect2.valid)
									{
										continue;
									}
									bool flag5;
									M2ChipImage m2ChipImage = this.IMGS.assignPxlLayerToImage(out flag5, lay, text, atlasRect2, this.atlas_rescale_x, this.atlas_rescale_y, this.first_fined_imgs_index <= this.fined_imgs_index);
									if (m2ChipImage != null && !m2ChipImage.loaded_additional_material)
									{
										this.M2D.loadAdditionalMaterialForChip(m2ChipImage);
									}
								}
								if (--progress_count == 0)
								{
									return true;
								}
							}
						}
						this.CurEntry = null;
						this.CurEntryPose = null;
						if (j == 0)
						{
							this.loaded_index++;
						}
						else
						{
							this.fined_imgs_index++;
							this.IMGS.assignPxlLayerFinalizeOnPose(true);
						}
					}
					else
					{
						if (j == 1 || this.calced_atlas_index == -3)
						{
							break;
						}
						if (this.calced_atlas_index < 0)
						{
							this.ALayForLoading.Sort(this.FD_FnSortFotAtlasAssign);
							this.calced_atlas_index = 0;
						}
						while (this.ALayForLoading.Count > this.calced_atlas_index)
						{
							List<M2ImageAtlas.LayerEntry> alayForLoading2 = this.ALayForLoading;
							int num5 = this.calced_atlas_index;
							this.calced_atlas_index = num5 + 1;
							M2ImageAtlas.LayerEntry layerEntry = alayForLoading2[num5];
							PxlLayer lay2 = layerEntry.Lay;
							this.fineMeshToTexture(lay2.Img.get_I(), false);
							string dirname = layerEntry.dirname;
							int num6;
							M2ImageAtlas.AtlasRect atlasRect3 = this.createRect(lay2, out num6);
							progress_count = ((progress_count > 0) ? X.Mx(1, progress_count - num6 / 32) : progress_count);
							if (dirname == "obj/" && lay2.name == "DUMMY_CHIP_IMAGE")
							{
								this.RectWhite = atlasRect3;
							}
							if (--progress_count == 0)
							{
								break;
							}
						}
						if (this.ALayForLoading.Count <= this.calced_atlas_index)
						{
							this.calced_atlas_index = -3;
							this.ALayForLoading.Clear();
						}
						if (progress_count == 0)
						{
							return true;
						}
						if (this.calced_atlas_index == -3)
						{
							break;
						}
					}
				}
			}
			GL.Flush();
			this.fineMeshToTexture(null, false);
			this.MdBuf = null;
			this.CurEntry = null;
			this.CurEntryPose = null;
			this.pre_dirname = null;
			this.ALayForLoading.Clear();
			this.IMGS.assignPxlLayerFinalize();
			this.atlas_rescale_x = (this.atlas_rescale_y = 1f);
			this.first_fined_imgs_index = this.fined_imgs_index;
			this.first_step_on_fine_image = true;
			this.alloc_m2d_loader_inactive = false;
			if (this.Recreated_Pre_Texture != this.Tx)
			{
				this.M2D.reloadTexture(this.Recreated_Pre_Texture, this.Tx);
				this.Recreated_Pre_Texture = this.Tx;
			}
			this.fine_pre_tx = false;
			this.M2D.prepareImageAtlasFinalize();
			if (this.pxlFullLoaded)
			{
				this.check_duplicate = false;
			}
			if (!Map2d.editor_decline_lighting)
			{
				for (int m = 0; m < count; m++)
				{
				}
			}
			return false;
		}

		private int FnSortFotAtlasAssign(M2ImageAtlas.LayerEntry Lea, M2ImageAtlas.LayerEntry Leb)
		{
			PxlLayer lay = Lea.Lay;
			PxlLayer lay2 = Leb.Lay;
			bool flag = lay.isGroup();
			bool flag2 = lay2.isGroup();
			if (flag || flag2)
			{
				if (flag && flag2)
				{
					return string.Compare(lay.name, lay2.name);
				}
				if (!flag)
				{
					return -1;
				}
				return 1;
			}
			else
			{
				PxlImage img = lay.Img;
				PxlImage img2 = lay2.Img;
				int num = ((img.height == img2.height) ? (img2.width - img.width) : (img2.height - img.height));
				if (num != 0)
				{
					return num;
				}
				return string.Compare(lay.pChar.title, lay2.pChar.title);
			}
		}

		private void fnExtendAtlas(int w, int h)
		{
		}

		private M2ImageAtlas.AtlasRect createRect(PxlLayer Lay, out int cost)
		{
			M2ImageAtlas.AtlasRect atlasRect = this.createRect(Lay.Img.width, Lay.Img.height);
			this.MdBuf.initForImg(Lay.Img, 0);
			this.MdBuf.RectBL((float)atlasRect.x, (float)atlasRect.y, (float)atlasRect.w, (float)atlasRect.h, true);
			this.ORc[Lay.Img] = atlasRect;
			cost = 1;
			return atlasRect;
		}

		private M2ImageAtlas.AtlasRect createRect(int w, int h)
		{
			int num = w + 1;
			int num2 = h + 1;
			int num3 = X.IntC(0.5f);
			int width = this.Tx.width;
			int height = this.Tx.height;
			int num4;
			RenderTexture renderTexture;
			RectInt rectInt = this.CalcAtlas.createRect(num, num2, out num4, out renderTexture, true);
			if (width < this.Tx.width || height < this.Tx.height)
			{
				if (!this.fine_pre_tx)
				{
					this.Recreated_Pre_Texture = renderTexture;
					this.fine_pre_tx = true;
					this.fined_imgs_index = 0;
				}
				this.atlas_rescale_x *= (float)width / (float)this.Tx.width;
				this.atlas_rescale_y *= (float)height / (float)this.Tx.height;
			}
			int x = rectInt.x;
			int y = rectInt.y;
			return new M2ImageAtlas.AtlasRect(x + num3, y + num3, w, h);
		}

		private void fineMeshToTexture(Texture T, bool force = false)
		{
			if (T != this.TxSource)
			{
				if (this.TxSource != null && this.MdBuf.getTriMax() > 0)
				{
					this.MdBuf.initForImgAndTexture("_MainTex", this.TxSource);
					Graphics.SetRenderTarget(this.Tx);
					GL.LoadProjectionMatrix(Matrix4x4.Ortho(0f, (float)this.Tx.width, 0f, (float)this.Tx.height, -1f, 100f));
					this.MtrMd.SetPass(0);
					BLIT.RenderToGLImmediate(this.MdBuf, this.MdBuf.getTriMax());
					Graphics.SetRenderTarget(null);
					this.MdBuf.clear(false, false);
					this.CalcAtlas.copy_previous_image = true;
				}
				this.TxSource = T;
			}
		}

		public Rect getAtlasRect(PxlImage I)
		{
			M2ImageAtlas.AtlasRect atlasRect;
			if (this.ORc.TryGetValue(I, out atlasRect))
			{
				return atlasRect.getRect();
			}
			return Rect.zero;
		}

		public M2ImageAtlas.AtlasRect getAtlasData(PxlImage I)
		{
			M2ImageAtlas.AtlasRect atlasRect;
			if (!this.ORc.TryGetValue(I, out atlasRect))
			{
				return default(M2ImageAtlas.AtlasRect);
			}
			return atlasRect;
		}

		public RenderTexture getTexture()
		{
			return this.Tx;
		}

		public bool pxlFullLoaded
		{
			get
			{
				return this.APxcEntry.Count >= M2DBase.Achip_pxl_key.Length;
			}
		}

		public bool isLoading()
		{
			return (this.M2D.isLoaderLoading() || this.alloc_m2d_loader_inactive) && this.fined_imgs_index < this.Aloaded_dir.Count;
		}

		public bool isImageLoading()
		{
			return this.fined_imgs_index < this.Aloaded_dir.Count;
		}

		public bool hasDir(string dir)
		{
			return this.Aloaded_dir.IndexOf(dir) >= 0;
		}

		public void getTextureWH(out float w, out float h)
		{
			w = (float)this.Tx.width;
			h = (float)this.Tx.height;
		}

		public void initForRectWhite(MeshDrawer Md)
		{
			this.RectWhite.initAtlasMd(Md, this.Tx);
		}

		public void initForRectWhite(MeshDrawer Md, float meshx, float meshy, float meshw, float meshh, bool no_divide_ppu = false, bool auto_mode = false)
		{
			float num = 1f / (float)this.Tx.width;
			float num2 = 1f / (float)this.Tx.height;
			if (!no_divide_ppu)
			{
				meshx *= 0.015625f;
				meshy *= 0.015625f;
				meshw *= 0.015625f;
				meshh *= 0.015625f;
			}
			Md.uvRect(meshx, meshy, meshw, meshh, (float)this.RectWhite.x * num, (float)this.RectWhite.y * num2, (float)this.RectWhite.w * num, (float)this.RectWhite.h * num2, !auto_mode, auto_mode);
		}

		public void releaseFnDrawForDropObject()
		{
			this.OFnForDropObject.Clear();
		}

		public M2DropObject.FnDropObjectDraw getFnDrawForDropObject(PxlImage I)
		{
			M2DropObject.FnDropObjectDraw fnDropObjectDraw;
			if (!this.OFnForDropObject.TryGetValue(I, out fnDropObjectDraw))
			{
				fnDropObjectDraw = delegate(M2DropObject Dro, EffectItem Ef, M2DrawBinder Ed)
				{
					M2ImageAtlas.AtlasRect atlasData = this.getAtlasData(I);
					return atlasData.valid && M2DropObject.fnDrawAtlasForDropObject(Dro, Ef, Ed, atlasData, this.IMGS.MIchip);
				};
				this.OFnForDropObject[I] = fnDropObjectDraw;
			}
			return fnDropObjectDraw;
		}

		public void releaseBlurImages()
		{
			foreach (KeyValuePair<M2ChipImage, List<M2BlurImage>> keyValuePair in this.OABlurImg)
			{
				List<M2BlurImage> value = keyValuePair.Value;
				int count = value.Count;
				for (int i = 0; i < count; i++)
				{
					value[i].Dispose();
				}
			}
			this.OABlurImg.Clear();
		}

		public M2BlurImage getBluredImage(M2ChipImage Img, int _level, Dungeon Dgn)
		{
			List<M2BlurImage> list;
			if (this.OABlurImg.TryGetValue(Img, out list))
			{
				for (int i = list.Count - 1; i >= 0; i--)
				{
					M2BlurImage m2BlurImage = list[i];
					if (m2BlurImage.level == _level)
					{
						return m2BlurImage;
					}
				}
			}
			else
			{
				list = (this.OABlurImg[Img] = new List<M2BlurImage>(1));
			}
			M2BlurImage m2BlurImage2 = new M2BlurImage(_level, Img, C32.c2d(Dgn.ColBlurTransparent));
			list.Add(m2BlurImage2);
			return m2BlurImage2;
		}

		public readonly M2DBase M2D;

		public readonly M2ImageContainer IMGS;

		private readonly List<string> Ainit_load_pxl;

		private readonly BDic<PxlImage, M2ImageAtlas.AtlasRect> ORc;

		private readonly List<string> Aloaded_dir;

		private readonly List<M2ImageAtlas.AtlasPxcEntry> APxcEntry;

		private int loaded_index;

		private int fined_imgs_index;

		private int first_fined_imgs_index;

		private int calced_layer_index;

		private int calced_atlas_index;

		private bool first_step_on_fine_image = true;

		private RenderTexture Recreated_Pre_Texture;

		public const int margin = 1;

		private float atlas_rescale_x;

		private float atlas_rescale_y;

		public List<string> Acurrent_async_loading;

		private M2ImageAtlas.AtlasPxcEntry CurEntry;

		private List<M2ImageAtlas.LayerEntry> ALayForLoading;

		private readonly BDic<PxlImage, M2DropObject.FnDropObjectDraw> OFnForDropObject;

		private readonly BDic<M2ChipImage, List<M2BlurImage>> OABlurImg;

		public bool alloc_m2d_loader_inactive;

		private bool check_duplicate;

		public static bool decline_async;

		public M2ImageAtlas.AtlasRect RectWhite;

		private string pre_dirname;

		private const int WHITE_WH = 4;

		private Comparison<M2ImageAtlas.LayerEntry> FD_FnSortFotAtlasAssign;

		internal const string LAYNAME_GROUP_HEADER_STAMP = "stamp_";

		private PxlPose CurEntryPose;

		private MeshDrawer MdBuf;

		private Material MtrMd;

		private Texture TxSource;

		private bool fine_pre_tx;

		private RectAtlasTexture CalcAtlas;

		public readonly struct AtlasRect
		{
			public AtlasRect(int _x, int _y, int _w, int _h)
			{
				this.x = _x;
				this.y = _y;
				this.w = _w;
				this.h = _h;
			}

			public bool valid
			{
				get
				{
					return this.w > 0;
				}
			}

			public int r
			{
				get
				{
					return this.x + this.w;
				}
			}

			public int b
			{
				get
				{
					return this.y + this.h;
				}
			}

			public float cx
			{
				get
				{
					return (float)this.x + (float)this.w * 0.5f;
				}
			}

			public float cy
			{
				get
				{
					return (float)this.y + (float)this.h * 0.5f;
				}
			}

			public void initAtlasMd(MeshDrawer Md, RenderTexture Tx)
			{
				Md.initForImg(Tx, (float)this.x, (float)this.y, (float)this.w, (float)this.h);
			}

			public void initAtlasMd(MeshDrawer Md, MImage MI, Rect ClipArea)
			{
				Md.initForImg(MI.Tx, (float)this.x + ClipArea.x, (float)this.y + ClipArea.y, ClipArea.width, ClipArea.height);
			}

			public void initAtlasMd(MeshDrawer Md, MImage MI)
			{
				Md.initForImg(MI.Tx, (float)this.x, (float)this.y, (float)this.w, (float)this.h);
			}

			public PxlMeshDrawer makeMesh(MImage MI, float shift_pixel_x, float shift_pixel_y, PxlMeshDrawer Src = null)
			{
				return this.makeMesh(MI.Tx, shift_pixel_x, shift_pixel_y, 1f, 1f, 0f, Src);
			}

			public PxlMeshDrawer makeMesh(MImage MI, float shift_pixel_x, float shift_pixel_y, float zmx, float zmy, float agR, PxlMeshDrawer Src = null)
			{
				return this.makeMesh(MI.Tx, shift_pixel_x, shift_pixel_y, zmx, zmy, agR, Src);
			}

			public PxlMeshDrawer makeMesh(Texture Tx, float shift_pixel_x, float shift_pixel_y, float zmx, float zmy, float agR, PxlMeshDrawer Src = null)
			{
				float num = 1f / (float)Tx.width;
				float num2 = 1f / (float)Tx.height;
				Vector2 vector = new Vector2((float)this.x * num, (float)this.y * num2);
				Vector2 vector2 = new Vector2((float)this.x * num, (float)(this.y + this.h) * num2);
				Vector2 vector3 = new Vector2((float)(this.x + this.w) * num, (float)(this.y + this.h) * num2);
				Vector2 vector4 = new Vector2((float)(this.x + this.w) * num, (float)this.y * num2);
				if (Src != null)
				{
					int num3;
					Vector2[] rawUvArray = Src.getRawUvArray(out num3);
					if (num3 == 4)
					{
						ref Vector2 ptr = ref rawUvArray[0];
						ref Vector2 ptr2 = ref rawUvArray[1];
						ref Vector2 ptr3 = ref rawUvArray[2];
						Vector2[] array = rawUvArray;
						int num4 = 3;
						Vector2 vector5 = vector;
						Vector2 vector6 = vector2;
						Vector2 vector7 = vector3;
						Vector2 vector8 = vector4;
						ptr = vector5;
						ptr2 = vector6;
						ptr3 = vector7;
						array[num4] = vector8;
						return Src;
					}
				}
				float num5 = shift_pixel_x * 0.015625f;
				float num6 = shift_pixel_y * 0.015625f;
				float num7 = (float)this.w / 2f * 0.015625f;
				float num8 = (float)this.h / 2f * 0.015625f;
				int[] array2 = ((zmx * zmy >= 0f) ? PxlMeshDrawer.Asimple_rect_tri : PxlMeshDrawer.Asimple_rect_tri_fliped);
				Vector3[] array3 = new Vector3[4];
				AIM aim = AIM.BL;
				for (int i = 0; i < 4; i++)
				{
					Vector2 vector9 = new Vector2(num7 * (float)CAim._XD(aim, 1) * zmx, num8 * (float)CAim._YD(aim, 1) * zmy);
					if (agR != 0f)
					{
						vector9 = X.ROTV2e(vector9, agR);
					}
					array3[i] = new Vector3(vector9.x + num5, vector9.y + num6, 0f);
					aim = CAim.get_clockwise2(aim, false);
				}
				PxlMeshDrawer pxlMeshDrawer = new PxlMeshDrawer(true);
				pxlMeshDrawer.setRawVerticesAndTriangles(new Vector2[] { vector, vector2, vector3, vector4 }, null, array3, array2, -1, -1);
				return pxlMeshDrawer;
			}

			public Rect getRect()
			{
				return new Rect((float)this.x, (float)this.y, (float)this.w, (float)this.h);
			}

			public readonly int x;

			public readonly int y;

			public readonly int w;

			public readonly int h;
		}

		private class AtlasPxcEntry
		{
			public AtlasPxcEntry(PxlCharacter _Pc, string entry_path, string _data_path, MImage _MI, MTIOneImage _MtiLoader)
			{
				this.Pc = _Pc;
				this.MI = _MI;
				this.MtiLoader = _MtiLoader;
				if (this.MI != null)
				{
					this.prepareImage(this.MI);
				}
			}

			public bool isModified()
			{
				return false;
			}

			public bool prepareImage(bool load_execute = true)
			{
				if (this.MI != null)
				{
					return true;
				}
				if (!this.MtiLoader.isAsyncLoadFinished())
				{
					return false;
				}
				if (load_execute)
				{
					this.prepareImage(this.MtiLoader.MI);
				}
				return true;
			}

			private void prepareImage(MImage _MI)
			{
				this.MI = _MI;
				this.Pc.ReplaceExternalPng(new Texture[] { this.MI.Tx }, true);
				MTRX.assignMI(this.Pc, this.MI);
			}

			public PxlCharacter Pc;

			public MImage MI;

			public MTIOneImage MtiLoader;
		}

		private struct LayerEntry
		{
			public LayerEntry(PxlLayer _Lay, string _dirname)
			{
				this.Lay = _Lay;
				this.dirname = _dirname;
			}

			public PxlLayer Lay;

			public string dirname;
		}
	}
}
