using System;

namespace DbEntity
{
	public class UploadEntity<T>
	{
		public int Version = 1;

		public string PcId;

		public long Timestamp;

		public T Data;
	}
}
