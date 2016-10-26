using System.Collections.Generic;

namespace JsonPatch
{
    public class JsonBatchPatchDocument<TEntity, TId> : JsonPatchDocument<TEntity> where TEntity : class, new()
    {
	   public List<TId> Ids { get; set; }
    }
}
