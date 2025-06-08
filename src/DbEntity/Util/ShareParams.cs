using System;

namespace DbEntity
{
	public class ShareParams
	{
		public const int LoginReportMinute = 60;
		public const int UnLimitAccountCount = 1000000;
		public const int MaxUploadRecordCount = 3000;
		public const int MaxDownloadRecordCount = 3000;

		public static string Password
		{
			get
			{
				return "0xFF";
			}
		}
    }
}
