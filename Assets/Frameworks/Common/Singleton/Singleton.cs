using System;

namespace Frameworks.Common
{
	public abstract class SingletonBase : IDisposable
	{
		~SingletonBase()
		{
			Dispose(false);
		}

		public virtual void Reset()
		{

		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected SingletonBase()
		{ 

		}

		protected virtual void Dispose(bool disposing)
		{

		}
	}

	public class SingletonBehaviour<T> : SingletonBase where T : class, IDisposable, new()
	{
		public static T Instance
		{ 
			get
			{
				return ms_Instance;
			}
		}

		static T ms_Instance = null;

		public static T Create()
		{
			if (ms_Instance == null)
				ms_Instance = new T();

			return ms_Instance;
		}

		public static void Del()
		{
			if (ms_Instance == null)
				return;

			ms_Instance.Dispose();
		}
	}
}
