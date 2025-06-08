using System;

namespace DbEntity
{
	public class UpdateDownloadEntity : EntityBase
	{
        public bool IsForceUpdate { get; set; }
        public int PatchVersion { get; set; }
        public string PatchUrl { get; set; }
        public string PatchFileName { get; set; }
        public int PatchSize { get; set; }
        public string Tip { get; set; }
        //public int MinBaseVersion;
        //public string BaseVersionDownloadUrl;
        //public int BaseSize;
        //public int[] DeleteVersions;
        //public int DeleteVersionLessThan = 0;
	}
}
