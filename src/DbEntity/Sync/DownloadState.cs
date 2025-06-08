using System;

namespace DbEntity
{
	public class DownloadState
	{
		public bool IsOk()
		{
			return string.IsNullOrEmpty(this.Error);
		}

		public string Error;
	}
}
