using System;
using System.IO;
using evt;
using PixelLiner;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2EvDebugListener : IRunAndDestroy
	{
		public M2EvDebugListener(M2DBase _M2D)
		{
			this.M2D = _M2D;
			this.Dbg = EV.getDebugger();
			if (this.Dbg != null)
			{
				EvDebugger dbg = this.Dbg;
				dbg.ActionTabInit = (Func<string, Designer, Designer, Designer, bool>)Delegate.Combine(dbg.ActionTabInit, new Func<string, Designer, Designer, Designer, bool>(this.categoryInit));
				EvDebugger dbg2 = this.Dbg;
				dbg2.ActionAwake = (Action<Designer>)Delegate.Combine(dbg2.ActionAwake, new Action<Designer>(this.tabLTInit));
				EvDebugger dbg3 = this.Dbg;
				dbg3.ActionCommand = (EvDebugger.FnDebugConsole)Delegate.Combine(dbg3.ActionCommand, new EvDebugger.FnDebugConsole(this.debugCommandLine));
				this.Dbg.addIgnoreEventHeader("%M2EVENTCOMMAND__@");
			}
			this.fnReturnNothing = delegate(LabeledInputField Fld)
			{
				if (this.Dbg != null)
				{
					this.Dbg.lock_return_execute = true;
				}
				return true;
			};
		}

		public bool isActive()
		{
			return this.Dbg != null && this.Dbg.isActive();
		}

		public bool isAwaken()
		{
			return this.Dbg != null;
		}

		public void ListenerInitialize()
		{
			if (this.Dbg != null)
			{
				this.Dbg.initEvDebugger();
				IN.addRunner(this);
			}
		}

		protected void modeRecreate()
		{
			if (this.Dbg != null)
			{
				this.Dbg.fnTriggerSub(null);
			}
		}

		protected void changeMode(string t)
		{
			if (this.Dbg != null)
			{
				this.Dbg.changeMode(t);
			}
		}

		protected bool changeMode(aBtn B)
		{
			this.changeMode(B.title);
			return true;
		}

		public void changeActivate(bool flag = false)
		{
			if (this.Dbg != null)
			{
				this.Dbg.changeActivate(flag);
				if (flag)
				{
					this.M2D.FlagValotStabilize.Add("EVDEBUGGER");
					return;
				}
				this.M2D.FlagValotStabilize.Rem("EVDEBUGGER");
			}
		}

		protected virtual void activateChanged(EvDebugger Dbg, bool flag = false)
		{
		}

		public virtual bool initS(Map2d Map)
		{
			return Map != null && this.Dbg != null;
		}

		protected void addHr(Designer Ds, int margin_t_ = 12, int margin_b_ = 18)
		{
			Ds.addHr(new DsnDataHr
			{
				margin_t = (float)margin_t_,
				margin_b = (float)margin_b_,
				line_height = 1f,
				Col = C32.d2c(2298478591U)
			});
		}

		public virtual void tabLTInit(Designer DsLT)
		{
			EvDebugger dbg = this.Dbg;
			dbg.ActionActivate = (Action<EvDebugger, bool>)Delegate.Combine(dbg.ActionActivate, new Action<EvDebugger, bool>(this.activateChanged));
		}

		public virtual bool categoryInit(string category, Designer DsT, Designer DsL, Designer DsR)
		{
			return false;
		}

		public virtual void destruct()
		{
			if (this.Dbg != null)
			{
				try
				{
					EvDebugger dbg = this.Dbg;
					dbg.ActionTabInit = (Func<string, Designer, Designer, Designer, bool>)Delegate.Remove(dbg.ActionTabInit, new Func<string, Designer, Designer, Designer, bool>(this.categoryInit));
					EvDebugger dbg2 = this.Dbg;
					dbg2.ActionAwake = (Action<Designer>)Delegate.Remove(dbg2.ActionAwake, new Action<Designer>(this.tabLTInit));
					EvDebugger dbg3 = this.Dbg;
					dbg3.ActionCommand = (EvDebugger.FnDebugConsole)Delegate.Remove(dbg3.ActionCommand, new EvDebugger.FnDebugConsole(this.debugCommandLine));
				}
				catch
				{
				}
			}
			IN.remRunner(this);
		}

		public virtual bool run(float fcnt)
		{
			return true;
		}

		public virtual bool debugCommandLine(string[] Acmd)
		{
			string text = Acmd[0].ToUpper();
			if (text != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(text);
				if (num <= 1343120305U)
				{
					if (num <= 731006985U)
					{
						if (num != 609853201U)
						{
							if (num == 731006985U)
							{
								if (text == "LOAD_SND")
								{
									if (Acmd.Length >= 2)
									{
										this.M2D.loadMaterialSnd(Acmd[1]);
									}
								}
							}
						}
						else if (text == "MAP")
						{
							if (Acmd.Length >= 2 && this.M2D.Get(Acmd[1], false) != null && this.M2D.curMap != null && this.M2D.curMap.key != Acmd[1])
							{
								EV.stack("_DEBUG_GOTO_MAP", 0, -1, new string[] { Acmd[1] }, null);
							}
							return false;
						}
					}
					else if (num != 1138742780U)
					{
						if (num == 1343120305U)
						{
							if (text == "EXPORT_IMAGE")
							{
								if (Acmd.Length >= 2)
								{
									int num2 = X.NmI(Acmd[1], 0, false, false);
									M2EvDebugListener.exportImage(this.M2D, "MdMap_" + this.M2D.curMap.key + "_" + num2.ToString(), this.M2D.curMap.get_MMRD().getUnsitabilizeMesh(num2).TxSimplified);
								}
								else
								{
									M2EvDebugListener.saveAtlasToPng(this.M2D);
								}
							}
						}
					}
					else if (text == "MAXFPS")
					{
						QualitySettings.vSyncCount = 0;
						if (Acmd.Length >= 2)
						{
							Application.targetFrameRate = X.NmI(Acmd[1], 144, false, false);
						}
						else
						{
							Application.targetFrameRate = 144;
						}
						if (!IN.enable_vsync)
						{
							IN.enable_vsync = true;
						}
					}
				}
				else if (num <= 3288762887U)
				{
					if (num != 2190495519U)
					{
						if (num == 3288762887U)
						{
							if (text == "PXL_PROGRESS")
							{
								if (Acmd.Length >= 2)
								{
									PxlsLoader.loadSpeed = X.Nm(Acmd[1], 1f, false);
								}
								X.dl("現在の Pxls LoadSpeed: " + PxlsLoader.loadSpeed.ToString(), null, false, false);
							}
						}
					}
					else if (text == "REMAKE_MAP")
					{
						this.M2D.curMap.reloadWholePxls(true);
					}
				}
				else if (num != 3727667089U)
				{
					if (num == 4217973635U)
					{
						if (text == "TXCHECK")
						{
							if (Acmd.Length >= 3)
							{
								TX.TXFamily familyByName = TX.getFamilyByName(Acmd[1]);
								TX.TXFamily familyByName2 = TX.getFamilyByName(Acmd[2]);
								if (familyByName == null)
								{
									X.dl("Tx " + Acmd[1] + " が存在しません", null, false, false);
								}
								else if (familyByName2 == null)
								{
									X.dl("Tx " + Acmd[2] + " が存在しません", null, false, false);
								}
								else
								{
									X.dl(string.Concat(new string[]
									{
										"Tx ",
										Acmd[1],
										" にTx ",
										Acmd[2],
										" の内容が存在するかチェックします。"
									}), null, false, false);
									familyByName2.checkDebugExting(familyByName);
								}
							}
						}
					}
				}
				else if (text == "EXPORT_CAM_IMAGE")
				{
					RenderTexture renderTexture = null;
					if (Acmd.Length >= 2 && Acmd[1] == "MV")
					{
						renderTexture = this.M2D.Cam.getMoverTexture();
					}
					if (renderTexture != null)
					{
						M2EvDebugListener.exportImage(this.M2D, "CAM_" + Acmd[1], renderTexture);
					}
				}
			}
			return true;
		}

		private static void exportImage(M2DBase M2D, string name, RenderTexture Tx)
		{
			string text = Application.dataPath + "/../Capture";
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			string text2 = string.Concat(new string[]
			{
				text,
				"/",
				name,
				"_",
				DateTime.Now.ToString("yyMMdd_HHmmss"),
				".png"
			});
			byte[] pngBytesS = EvPerson.getPngBytesS(Tx);
			File.WriteAllBytes(text2, pngBytesS);
			X.dl(text2 + " にマップ画像キャプチャイメージを保存", null, false, true);
		}

		public static bool saveAtlasToPng(M2DBase M2D)
		{
			byte[] pngBytesS = EvPerson.getPngBytesS(M2D.IMGS.Atlas.getTexture());
			if (pngBytesS == null)
			{
				return false;
			}
			string text = Application.dataPath + "/../Capture";
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			string text2 = text + "/m2d_atlas_" + DateTime.Now.ToString("yyMMdd_HHmmss") + ".png";
			File.WriteAllBytes(text2, pngBytesS);
			X.dl(text2 + " にキャプチャイメージを保存", null, false, true);
			return true;
		}

		public override string ToString()
		{
			return "<M2EvDebugListener>";
		}

		public readonly M2DBase M2D;

		public EvDebugger Dbg;

		private bool runner_assined_;

		private bool need_remake_ds;

		protected FnFldBindings fnReturnNothing;
	}
}
