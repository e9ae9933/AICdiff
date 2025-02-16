using System;
using System.Collections.Generic;
using Better;

namespace XX
{
	public sealed class TxEvalListenerContainer
	{
		public TxEvalListenerContainer(int capacity_fn = 32, bool _static_input = false)
		{
			this.OFn = new BDic<string, FnTxEval>(capacity_fn);
			this.static_input = _static_input;
		}

		public TxEvalListenerContainer Add(string key, FnTxEval Fn, params string[] Aother)
		{
			this.OFn[key] = Fn;
			for (int i = Aother.Length - 1; i >= 0; i--)
			{
				this.OFn[Aother[i]] = Fn;
			}
			return this;
		}

		public void AssignToStatic(BDic<string, FnTxEval> Ost)
		{
			foreach (KeyValuePair<string, FnTxEval> keyValuePair in this.OFn)
			{
				Ost[keyValuePair.Key] = keyValuePair.Value;
			}
		}

		public bool Get(TxEvalListenerContainer O, string key, List<string> Aargs)
		{
			FnTxEval fnTxEval = X.Get<string, FnTxEval>(this.OFn, key);
			if (fnTxEval != null)
			{
				fnTxEval(O, Aargs);
				return true;
			}
			return false;
		}

		public static TxEvalListenerContainer getDefault()
		{
			TxEvalListenerContainer txEvalListenerContainer = new TxEvalListenerContainer(12, true);
			txEvalListenerContainer.Add("SOUND_OFF", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				TX.InputE((float)((X.DEBUG && X.DEBUGNOSND) ? 1 : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("BGM_VOL", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				TX.InputE(SND.bgm_volume01);
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("DEBUG", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				TX.InputE((float)(X.DEBUG ? 1 : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("SENSITIVE", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				TX.InputE((float)(X.SENSITIVE ? 1 : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("ENG_MODE", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				TX.InputE((float)(X.ENG_MODE ? 1 : 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("randmpf", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				TX.InputE((float)X.MPF(X.xors(2) == 0));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("rand", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				TX.InputE((float)X.xors(X.GetNmI(Aargs, 1, true, 0)));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("abs", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				TX.InputE(X.Abs(X.GetNm(Aargs, 0f, true, 0)));
			}, new string[] { "Abs" });
			txEvalListenerContainer.Add("cos", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				TX.InputE(X.Cos(X.GetNm(Aargs, 0f, true, 0)));
			}, new string[] { "Cos" });
			txEvalListenerContainer.Add("sin", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				TX.InputE(X.Sin(X.GetNm(Aargs, 0f, true, 0)));
			}, new string[] { "Sin" });
			txEvalListenerContainer.Add("saturate", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				TX.InputE(X.saturate(X.GetNm(Aargs, 0f, true, 0)));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("XD", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				TX.InputE((float)CAim._XD(CAim.parseString(X.Get<string>(Aargs, 0), 0), 1));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("YD", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				TX.InputE((float)CAim._YD(CAim.parseString(X.Get<string>(Aargs, 0), 0), 1));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("GAR", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				TX.InputE(X.GAR(X.GetNm(Aargs, 0f, true, 0), X.GetNm(Aargs, 0f, true, 1), X.GetNm(Aargs, 1f, true, 2), X.GetNm(Aargs, 0f, true, 3)));
			}, Array.Empty<string>());
			txEvalListenerContainer.Add("comb_bits", delegate(TxEvalListenerContainer O, List<string> Aargs)
			{
				TX.InputE(X.comb_bits((int)X.GetNm(Aargs, 0f, true, 0), (int)X.GetNm(Aargs, 0f, true, 1)));
			}, Array.Empty<string>());
			return txEvalListenerContainer;
		}

		private BDic<string, FnTxEval> OFn;

		public readonly bool static_input;
	}
}
