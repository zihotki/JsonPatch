using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonPatch.Model
{
	public class BatchPatchOperation<T>
	{
		public T[] ids { get; set; }
		public PatchOperation[] patch { get; set; }
	}
}
