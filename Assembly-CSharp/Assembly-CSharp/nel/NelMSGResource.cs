using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Better;
using evt;
using UnityEngine;
using XX;

namespace nel
{
	public class NelMSGResource
	{
		public NelMSGResource touch()
		{
			try
			{
				FileInfo fileInfo = new FileInfo(Path.Combine(NelMSGResource.lang2dir, this.key));
				this.modified = fileInfo.LastWriteTime;
			}
			catch
			{
			}
			return this;
		}

		public bool reload()
		{
			string text = null;
			try
			{
				using (StreamReader streamReader = new StreamReader(Path.Combine(NelMSGResource.lang2dir, this.key)))
				{
					text = streamReader.ReadToEnd();
					streamReader.Close();
				}
			}
			catch (Exception ex)
			{
				X.de(ex.Message, null);
			}
			if (text == null)
			{
				return false;
			}
			this.recheck_flag = false;
			this.Adata = TX.split(text, "\n");
			return true;
		}

		public NelMSGResource reindex()
		{
			List<NelMSGResource.LabelPos> list = new List<NelMSGResource.LabelPos>(20);
			int num = this.Adata.Length;
			string text = "";
			for (int i = 0; i < num; i++)
			{
				string text2 = this.Adata[i];
				if (REG.match(text2, NelMSGResource.RegComment))
				{
					text2 = (this.Adata[i] = REG.leftContext);
				}
				if (TX.isStart(text2, "*", 0) && !REG.match(text2, NelMSGResource.RegClickPos))
				{
					if (REG.match(text2, NelMSGResource.RegEventAndLabel))
					{
						text = REG.R1;
						list.Add(new NelMSGResource.LabelPos
						{
							index = i,
							label = text + " " + REG.R2
						});
					}
					else if (REG.match(text2, NelMSGResource.RegOnlyLabel))
					{
						list.Add(new NelMSGResource.LabelPos
						{
							index = i,
							label = text + " " + REG.R1
						});
					}
				}
			}
			this.ALabelPos = list.ToArray();
			return this;
		}

		private static bool checklabel(NelMSGResource.LabelPos L)
		{
			return L.label == NelMSGResource.checklabel_content;
		}

		private bool getContentInner(string label, List<string> Adest)
		{
			NelMSGResource.checklabel_content = label;
			int num = X.isina<NelMSGResource.LabelPos>(this.ALabelPos, (NelMSGResource.LabelPos __L) => NelMSGResource.checklabel(__L), -1);
			if (num < 0)
			{
				return false;
			}
			NelMSGResource.LabelPos labelPos = this.ALabelPos[num];
			if (this.recheck_flag)
			{
				this.recheck_flag = false;
				bool flag = false;
				try
				{
					DateTime lastWriteTime = new FileInfo(Path.Combine(NelMSGResource.lang2dir, this.key)).LastWriteTime;
					if (lastWriteTime > this.modified)
					{
						flag = true;
						this.modified = lastWriteTime;
					}
				}
				catch
				{
				}
				if (flag && this.reload() && (this.Adata.Length <= labelPos.index || this.Adata[labelPos.index] != "*" + labelPos.label))
				{
					this.reindex();
					return this.getContentInner(label, Adest);
				}
			}
			int num2 = ((num == this.ALabelPos.Length - 1) ? this.Adata.Length : this.ALabelPos[num + 1].index);
			using (STB stb = TX.PopBld(null, 0))
			{
				int num3;
				int num4;
				for (int i = labelPos.index + 1; i < num2; i++)
				{
					string text = this.Adata[i];
					if (TX.isStart(text, '*') && REG.match(text, NelMSGResource.RegClickPos))
					{
						stb.TrimSpace(0, out num3, out num4, -1);
						if (num3 < num4)
						{
							Adest.Add(stb.ToString(num3, num4 - num3));
						}
						stb.Clear();
					}
					else
					{
						stb.AR(this.Adata[i]);
					}
				}
				stb.TrimSpace(0, out num3, out num4, -1);
				if (num3 < num4)
				{
					Adest.Add(stb.ToString(num3, num4 - num3));
				}
				stb.Clear();
			}
			return true;
		}

		private static string lang2dir
		{
			get
			{
				return Path.Combine(Path.Combine(Application.streamingAssetsPath, "localization"), NelMSGResource.load_lang);
			}
		}

		public static void initResource(bool force = false)
		{
			string text = TX.getCurrentFamily().key;
			if (!force && NelMSGResource.load_lang == text)
			{
				return;
			}
			X.dl("Load Event Lang: " + text, null, false, true);
			NelMSGResource.load_lang = text;
			string[] files = Directory.GetFiles(NelMSGResource.lang2dir, "ev*.txt");
			string text2 = "Count_EvResource_" + NelMSGResource.load_lang;
			int num = files.Length;
			List<NelMSGResource> list = new List<NelMSGResource>((NelMSGResource.AResource != null) ? NelMSGResource.AResource.Length : PlayerPrefs.GetInt(text2));
			for (int i = 0; i < num; i++)
			{
				string text3 = files[i];
				string fileName = Path.GetFileName(text3);
				if (!TX.isEnd(fileName, ".meta") && (File.GetAttributes(text3) & FileAttributes.Directory) == (FileAttributes)0)
				{
					NelMSGResource nelMSGResource = new NelMSGResource
					{
						key = fileName
					};
					if (nelMSGResource.reload())
					{
						list.Add(nelMSGResource.touch().reindex());
					}
				}
			}
			NelMSGResource.AResource = list.ToArray();
			PlayerPrefs.SetInt(text2, NelMSGResource.AResource.Length);
			IN.save_prefs = true;
			EvPerson.fineLocalizedName();
		}

		public static bool getContent(string label, List<string> Adest, bool merging = false, bool no_error = false, bool alloc_empty = false)
		{
			if (!merging)
			{
				Adest.Clear();
			}
			int count = Adest.Count;
			if (NelMSGResource.OResourceAdditional != null && label.IndexOf("<<<") >= 0)
			{
				string[] array;
				if (NelMSGResource.OResourceAdditional.TryGetValue(label, out array))
				{
					Adest.AddRange(array);
					return true;
				}
				return false;
			}
			else
			{
				for (int i = NelMSGResource.AResource.Length - 1; i >= 0; i--)
				{
					if (NelMSGResource.AResource[i].getContentInner(label, Adest))
					{
						if (Adest.Count == count)
						{
							if (!alloc_empty)
							{
								X.de("No Content in the Label :" + label, null);
								return false;
							}
							Adest.Add("\u3000");
						}
						return true;
					}
				}
				if (X.DEBUGRELOADMTR && NelMSGResource.reload_source_if_undefined)
				{
					NelMSGResource.reload_source_if_undefined = false;
					NelMSGResource.load_lang = "";
					NelMSGResource.initResource(false);
					return NelMSGResource.getContent(label, Adest, true, false, false);
				}
				if (!no_error)
				{
					X.de("Unknown Event Text Label:" + label, null);
				}
				if (X.DEBUG)
				{
					Adest.Add("[Unknown Label] " + label);
					return true;
				}
				return false;
			}
		}

		public static void addAdditionalLabel(string _s, string[] Astr)
		{
			if (NelMSGResource.OResourceAdditional == null)
			{
				NelMSGResource.OResourceAdditional = new BDic<string, string[]>(4);
			}
			NelMSGResource.OResourceAdditional[_s] = Astr;
		}

		public static void setRecheckContentFlag()
		{
			for (int i = NelMSGResource.AResource.Length - 1; i >= 0; i--)
			{
				NelMSGResource.AResource[i].recheck_flag = true;
			}
			NelMSGResource.reload_source_if_undefined = true;
		}

		public static void replaceTxInjection(List<string> Asrc, List<string> Arpl)
		{
			using (STB stb = TX.PopBld(null, 0))
			{
				int count = Asrc.Count;
				int count2 = Arpl.Count;
				for (int i = 0; i < count; i++)
				{
					if (Asrc[i] != null)
					{
						stb.Set(Asrc[i]);
						stb.prepareTxReplace(0, 1);
						for (int j = 0; j < count2; j++)
						{
							stb.TxRpl(TX.ReplaceTX(Arpl[j], false));
						}
						Asrc[i] = stb.ToString();
					}
				}
			}
		}

		public static void replaceTxInjection(List<string> Asrc, string[] Arpl_)
		{
			using (BList<string> blist = ListBuffer<string>.Pop(Arpl_.Length))
			{
				blist.AddRange(Arpl_);
				NelMSGResource.replaceTxInjection(Asrc, blist);
			}
		}

		public bool recheck_flag;

		private string key = "";

		public string[] Adata;

		public DateTime modified = X.TimeEpoch;

		public NelMSGResource.LabelPos[] ALabelPos;

		private static string checklabel_content;

		private static readonly Regex RegClickPos = new Regex("^\\*[ \\s\\t]*$");

		private static readonly Regex RegEventAndLabel = new Regex("^\\*([\\w\\/\\.]+)[ \\s\\t]+([\\w\\/\\.]+)[ \\s\\t]*$");

		private static readonly Regex RegOnlyLabel = new Regex("^\\*([\\w\\/\\.]+)[ \\s\\t]*$");

		private static readonly Regex RegComment = new Regex("[\\s\\t]*\\/\\/");

		public static string load_lang = "";

		public static NelMSGResource[] AResource;

		public static BDic<string, string[]> OResourceAdditional;

		public static bool reload_source_if_undefined = true;

		public class LabelPos
		{
			public string label;

			public int index;
		}
	}
}
