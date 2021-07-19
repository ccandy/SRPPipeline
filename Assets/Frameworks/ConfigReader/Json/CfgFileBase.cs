using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace Config
{
    public class CfgFileBase
    {
        public delegate void OnFinishInitCallback();

        protected OnFinishInitCallback finishInitCallback;

        public virtual bool Init(JToken jsonObj)
        {
            //finishInitCallback?.Invoke();
            return true;
        }

		public void DoConfig()
		{
			finishInitCallback?.Invoke();
		}
    }
}
