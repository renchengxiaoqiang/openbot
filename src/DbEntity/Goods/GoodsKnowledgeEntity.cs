using System;

namespace DbEntity
{
	public class GoodsKnowledgeEntity : EntityBase
	{
		public long NumIid { get; set; }
		public string Title { get; set; }
		public string Content { get; set; }
		public string ImgFileName { get; set; }
	}
}
