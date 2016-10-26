using JsonPatch.Model;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;

namespace JsonPatch.Formatting
{
    public class JsonBatchPatchFormatter : BufferedMediaTypeFormatter
    {
        internal static JsonPatchSettings Settings { get; private set; }

        public JsonBatchPatchFormatter() : this(JsonPatchSettings.DefaultPatchSettings())
        {
            
        }

        public JsonBatchPatchFormatter(JsonPatchSettings settings)
        {
            Settings = settings;
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json-batch-patch+json"));
        }

        public override bool CanWriteType(Type type)
        {
            return false;
        }

        public override bool CanReadType(Type type)
        {
	        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(JsonBatchPatchDocument<,>))
            {
                return true;
            }

			return false;
        }

        public override object ReadFromStream(Type type, Stream readStream, System.Net.Http.HttpContent content, IFormatterLogger formatterLogger)
        {
            var entityType = type.GetGenericArguments()[0];
			var entityIdType = type.GetGenericArguments()[1];

			using (var reader = new StreamReader(readStream))
			{
				var deserializeMethod = GetType().GetMethod(nameof(Deserialize), BindingFlags.NonPublic | BindingFlags.Instance)
					.MakeGenericMethod(entityType, entityIdType);

				var jsonBatchPatchDocument = deserializeMethod.Invoke(this, new object[] {reader});

                return jsonBatchPatchDocument;
            }
        }

	    private object Deserialize<TEntity, TEntityId>(StreamReader reader) where TEntity : class, new()
	    {
		    var jsonBatchPatchDocument = new JsonBatchPatchDocument<TEntity, TEntityId>();
			
			var jsonString = reader.ReadToEnd();
		    var batchOperation = JsonConvert.DeserializeObject<BatchPatchOperation<TEntityId>>(jsonString);

		    jsonBatchPatchDocument.Ids = (batchOperation.ids ?? Enumerable.Empty<TEntityId>()).ToList();
			
			foreach (var operation in batchOperation.patch)
			{
				switch (operation.op)
				{
					case Constants.Operations.ADD:
						jsonBatchPatchDocument.Add(operation.path, operation.value);
						break;
					case Constants.Operations.REMOVE:
						jsonBatchPatchDocument.Remove(operation.path);
						break;
					case Constants.Operations.REPLACE:
						jsonBatchPatchDocument.Replace(operation.path, operation.value);
						break;
					case Constants.Operations.MOVE:
						jsonBatchPatchDocument.Move(operation.from, operation.path);
						break;
					default:
						throw new JsonPatchParseException($"The operation '{operation.op}' is not supported.");
				}
			}

		    return jsonBatchPatchDocument;
	    }
    }
}
