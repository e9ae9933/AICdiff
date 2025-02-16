using System;
using XX;

namespace nel
{
	public sealed class EffectHandlerPE : EffectHandler<PostEffectItem>
	{
		public EffectHandlerPE(int capacity = 2)
			: base(capacity, null)
		{
		}

		public EffectHandlerPE deactivate(bool delete_instance = false)
		{
			int count = this.AEf.Count;
			for (int i = 0; i < count; i++)
			{
				EffectHandler<PostEffectItem>.HandledItem handledItem = this.AEf[i];
				if (handledItem.Ef != null && handledItem.Ef.FnDef != null && handledItem.Ef.index == handledItem.s_index)
				{
					handledItem.Ef.deactivate(false);
				}
			}
			if (delete_instance)
			{
				this.AEf.Clear();
			}
			return this;
		}

		public bool has(POSTM pe_type)
		{
			for (int i = this.AEf.Count - 1; i >= 0; i--)
			{
				EffectHandler<PostEffectItem>.HandledItem handledItem = this.AEf[i];
				if (handledItem.Ef != null && handledItem.Ef.FnDef != null && handledItem.Ef.index == handledItem.s_index && handledItem.Ef.time == (int)pe_type)
				{
					return true;
				}
			}
			return false;
		}

		public EffectHandlerPE deactivateSpecific(POSTM pe_type)
		{
			for (int i = this.AEf.Count - 1; i >= 0; i--)
			{
				EffectHandler<PostEffectItem>.HandledItem handledItem = this.AEf[i];
				if (handledItem.Ef != null && handledItem.Ef.FnDef != null && handledItem.Ef.index == handledItem.s_index && handledItem.Ef.time == (int)pe_type)
				{
					handledItem.Ef.deactivate(false);
					this.AEf.RemoveAt(i);
				}
			}
			return this;
		}

		public EffectHandlerPE deactivateSpecific(EffectItem Ef)
		{
			for (int i = this.AEf.Count - 1; i >= 0; i--)
			{
				EffectHandler<PostEffectItem>.HandledItem handledItem = this.AEf[i];
				if (handledItem.Ef != null && handledItem.Ef.FnDef != null && handledItem.Ef.index == handledItem.s_index && handledItem.Ef == Ef)
				{
					handledItem.Ef.deactivate(false);
					this.AEf.RemoveAt(i);
				}
			}
			return this;
		}

		public EffectHandlerPE fine(int f = 100)
		{
			int count = this.AEf.Count;
			for (int i = 0; i < count; i++)
			{
				EffectHandler<PostEffectItem>.HandledItem handledItem = this.AEf[i];
				if (handledItem.Ef != null && handledItem.Ef.FnDef != null && handledItem.Ef.index == handledItem.s_index)
				{
					handledItem.Ef.fine(f);
				}
			}
			return this;
		}

		public EffectHandlerPE setXLevel(POSTM pe_type, float level = 1f)
		{
			int count = this.AEf.Count;
			for (int i = 0; i < count; i++)
			{
				EffectHandler<PostEffectItem>.HandledItem handledItem = this.AEf[i];
				if (handledItem.Ef != null && handledItem.Ef.FnDef != null && handledItem.Ef.index == handledItem.s_index && handledItem.Ef.time == (int)pe_type)
				{
					handledItem.Ef.x = level;
					break;
				}
			}
			return this;
		}
	}
}
