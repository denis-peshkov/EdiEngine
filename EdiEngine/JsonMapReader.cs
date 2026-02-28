namespace EdiEngine;

public class JsonMapReader : InetermediateFormatReader
{
    public JsonMapReader(MapLoop map) : base(map)
    {
    }

    protected override EdiIntermediateEntity ReadIntermediateTree(string rawData)
    {
        var reader = new Utf8JsonReader(
            Encoding.UTF8.GetBytes(rawData),
            new JsonReaderOptions
            {
                CommentHandling = JsonCommentHandling.Skip
            });

        var root = new EdiIntermediateEntity(null);
        var context = root;
        PropertyInfo prop = null;

        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.StartObject:
                    var ent = new EdiIntermediateEntity(context);
                    context?.Children.Add(ent);
                    context = ent;
                    break;

                case JsonTokenType.PropertyName:
                    var propertyName = reader.GetString();
                    if (!string.IsNullOrEmpty(propertyName))
                    {
                        prop = typeof(EdiIntermediateEntity).GetProperty(propertyName);
                    }
                    break;

                case JsonTokenType.String:
                    var value = reader.GetString();
                    if (prop != null)
                    {
                        prop.SetValue(context, value);
                    }
                    break;

                case JsonTokenType.EndObject:
                    context = context?.Parent;
                    break;
            }
        }

        return root.Children.Count > 0 ? root.Children[0] : null;
    }
}