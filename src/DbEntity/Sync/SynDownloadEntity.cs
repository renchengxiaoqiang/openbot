using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DbEntity
{
	public class SynDownloadEntity
	{
		public string DbAccount { get; set; }

        public List<EntityBase> DataList { get; set; }

		public long ServerSynTick { get; set; }
	}
}
