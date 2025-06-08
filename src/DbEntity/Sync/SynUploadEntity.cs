using System;
using System.Collections.Generic;

namespace DbEntity
{
	public class SynUploadEntity
	{
		public string DbAccount { get; set; }

		public List<EntityBase> DataList { get; set; }

		public long ServerSynTick { get; set; }
	}
}
