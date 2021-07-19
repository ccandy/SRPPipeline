using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Frameworks
{
	public abstract class JsonCoder
	{
		public virtual void LoadFromJson(JObject json)
		{

		}

		public virtual JObject SaveAsJObject()
		{
			return null;
		}
	}
}
