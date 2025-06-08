using System;
using System.Collections.Generic;

namespace DbEntity
{
	public class LoginUploadEntity
	{
		public UploadNickInfo[] NickDatas;

		public HashSet<string> FirstLoginMainNicks;

		public string InstanceGuid;
	}
}
