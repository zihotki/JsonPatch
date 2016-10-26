using System;
using System.Collections.Generic;
using JsonPatch.Paths;

namespace JsonPatch
{
    public class JsonPatchDocument<TEntity> : IJsonPatchDocument where TEntity : class, new()
    {
	    public List<JsonPatchOperation> Operations { get; } = new List<JsonPatchOperation>();

	    public bool HasOperations { get { return Operations.Count > 0; } }

        public void Add(string path, object value)
        {
            Operations.Add(new JsonPatchOperation
            {
                Operation = JsonPatchOperationType.add,
                Path = path,
                ParsedPath = PathHelper.ParsePath(path, typeof(TEntity)),
                Value = value
            });
        }

        public void Replace(string path, object value)
        {
            Operations.Add(new JsonPatchOperation
            {
                Operation = JsonPatchOperationType.replace,
                Path = path,
                ParsedPath = PathHelper.ParsePath(path, typeof(TEntity)),
                Value = value
            });
        }

        public void Remove(string path)
        {
            Operations.Add(new JsonPatchOperation
            {
                Operation = JsonPatchOperationType.remove,
                Path = path,
                ParsedPath = PathHelper.ParsePath(path, typeof(TEntity))
            });
        }

        public void Move(string from, string path)
        {
            Operations.Add(new JsonPatchOperation
            {
                Operation = JsonPatchOperationType.move,
                FromPath = from,
                ParsedFromPath = PathHelper.ParsePath(from, typeof(TEntity)),
                Path = path,
                ParsedPath = PathHelper.ParsePath(path, typeof(TEntity)),
            });
        }

        public void ApplyUpdatesTo(TEntity entity)
        {
            foreach (var operation in Operations)
            {
                switch (operation.Operation)
                {
                    case JsonPatchOperationType.remove:
                        PathHelper.SetValueFromPath(typeof(TEntity), operation.ParsedPath, entity, null, JsonPatchOperationType.remove);
                        break;
                    case JsonPatchOperationType.replace:
                        PathHelper.SetValueFromPath(typeof(TEntity), operation.ParsedPath, entity, operation.Value, JsonPatchOperationType.replace);
                        break;
                    case JsonPatchOperationType.add:
                        PathHelper.SetValueFromPath(typeof(TEntity), operation.ParsedPath, entity, operation.Value, JsonPatchOperationType.add);
                        break;
                    case JsonPatchOperationType.move:
                        var value = PathHelper.GetValueFromPath(typeof(TEntity), operation.ParsedFromPath, entity);
                        PathHelper.SetValueFromPath(typeof(TEntity), operation.ParsedFromPath, entity, null, JsonPatchOperationType.remove);
                        PathHelper.SetValueFromPath(typeof(TEntity), operation.ParsedPath, entity, value, JsonPatchOperationType.add);
                        break;
                    default:
                        throw new NotSupportedException("Operation not supported: " + operation.Operation);
                }
            }
        }
    }
}
