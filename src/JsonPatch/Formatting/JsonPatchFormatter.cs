using JsonPatch.Model;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;

namespace JsonPatch.Formatting
{
    public class JsonPatchFormatter : BufferedMediaTypeFormatter
    {
        internal static JsonPatchSettings Settings { get; private set; }
        public JsonPatchFormatter() : this(JsonPatchSettings.DefaultPatchSettings())
        {
            
        }

        public JsonPatchFormatter(JsonPatchSettings settings)
        {
            Settings = settings;
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json-patch+json"));
        }

        public override bool CanWriteType(Type type)
        {
            return false;
        }

        public override bool CanReadType(Type type)
        {
	        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(JsonPatchDocument<>))
            {
                return true;
            }

			return false;
        }

        public override object ReadFromStream(Type type, Stream readStream, System.Net.Http.HttpContent content, IFormatterLogger formatterLogger)
        {
            var entityType = type.GetGenericArguments()[0];

	        using (var reader = new StreamReader(readStream))
	        {
		        var deserializeMethod = GetType().GetMethod(nameof(Deserialize), BindingFlags.NonPublic | BindingFlags.Instance)
			        .MakeGenericMethod(entityType);

		        var jsonPatchDocument = deserializeMethod.Invoke(this, new object[] {reader});

		        return jsonPatchDocument;
	        }
        }

	    private static IJsonPatchDocument Deserialize<TEntity>(StreamReader reader) where TEntity : class, new()
		{
		    var jsonPatchDocument = new JsonPatchDocument<TEntity>(); 

		    var jsonString = reader.ReadToEnd();
		    var operations = JsonConvert.DeserializeObject<PatchOperation[]>(jsonString);

		    foreach (var operation in operations)
		    {
			    if (operation.op == Constants.Operations.ADD)
			    {
				    jsonPatchDocument.Add(operation.path, operation.value);
			    }
			    else if (operation.op == Constants.Operations.REMOVE)
			    {
				    jsonPatchDocument.Remove(operation.path);
			    }
			    else if (operation.op == Constants.Operations.REPLACE)
			    {
				    jsonPatchDocument.Replace(operation.path, operation.value);
			    }
			    else if (operation.op == Constants.Operations.MOVE)
			    {
				    jsonPatchDocument.Move(operation.from, operation.path);
			    }
			    else
			    {
				    throw new JsonPatchParseException($"The operation '{operation.op}' is not supported.");
			    }
		    }
		    return jsonPatchDocument;
	    }
    }
}
