using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class WeakReference<T> where T : class
{
	WeakReference r;
	public T Target
	{
		get
		{
			return r.IsAlive ? (T)r.Target : null;
		}
		set
		{
			r = new WeakReference(value);
		}
	}
	
	public static implicit operator T(WeakReference<T> re)
	{
		return re.Target;
	}
	
	public static implicit operator WeakReference<T>(T value)
	{
		return new WeakReference<T>() { Target = value};
	}
	
}


public class Lookup<TK, TR> : Dictionary<TK, TR> where TR : class
{
	public new virtual TR this[TK index]
	{
		get
		{
			if (ContainsKey(index))
			{
				return base[index];
			}
			return null;
			
		}
		set
		{
			base[index] = value;
		}
	}
	
	public T Get<T>(TK index) where T : class
	{
		return this[index] as T;
	}
	
	
	
}

public interface IChanged
{
	void Changed(object index);
}

public class Index<TK, TR> : Lookup<TK,TR>, IChanged  where TR : class, new()
{

	public event Action<TK,TR, TR> Setting;
	public event Action<TK,TR> Getting = delegate {};
	
	public void Changed(object index)
	{
		if (Setting != null)
		{
			TR current = null;
			if (base.ContainsKey((TK)index))
			{
				current = base[(TK)index];
			}
			Setting((TK)index, current, current);
		}
	}
	
	public override TR this[TK index]
	{
		get
		{
			if (ContainsKey(index))
			{
				return base[index];
			}
			var ret = new TR();
			base[index] = ret;
			Getting(index, ret);
			return ret;
		}
		set
		{
			if (Setting != null)
			{
				TR current = null;
				if (base.ContainsKey(index))
				{
					current = base[index];
				}
				Setting(index, current, value);
			}
			base[index] = value;
		}
	}
}