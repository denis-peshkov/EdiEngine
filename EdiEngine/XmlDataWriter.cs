namespace EdiEngine;

public class XmlDataWriter : DataWriter
{
    private readonly XmlWriterSettings _settings;

    public XmlDataWriter(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        _settings = new XmlWriterSettings
        {
            OmitXmlDeclaration = true,
            Encoding = Encoding.UTF8,
            CloseOutput = false
        };
    }

    public XmlDataWriter(XmlWriterSettings settings, IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        _settings = settings;
    }

    public override string WriteToString(EdiBatch batch)
    {
        _serviceProvider.CheckLicense();

        using (Stream s = WriteToStream(batch))
        {
            using (StreamReader r = new StreamReader(s, _settings.Encoding))
            {
                return r.ReadToEnd();
            }
        }
    }

    public override Stream WriteToStream(EdiBatch batch)
    {
        _serviceProvider.CheckLicense();

        Stream s = new MemoryStream();

        XmlWriter w = XmlWriter.Create(s, _settings);
        w.WriteStartDocument();

        WrtiteObject(batch, w);

        w.WriteEndDocument();
        w.Flush();
        w.Close();

        s.Position = 0;
        return s;
    }

    private void WrtiteObject(object obj, XmlWriter w)
    {
        if (obj == null)
            return;

        //check element start - end tag should be omitted
        var elementAttr = obj.GetType().GetCustomAttributes().OfType<XmlElementAttribute>().FirstOrDefault();
        bool writeStratEndElement = !(elementAttr?.IgnoreElementRoot ?? false);
        var startElementName = elementAttr?.ElementName ?? obj.GetType().Name;

        if (writeStratEndElement)
            w.WriteStartElement(startElementName);

        //order properties according to Order
        var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .OrderBy(p => p.GetCustomAttributes().OfType<XmlPropertyAttribute>().FirstOrDefault()?.Order);

        foreach (PropertyInfo prop in properties)
        {
            //check property is ignored in attr
            var ignoreAttr = prop.GetCustomAttributes().OfType<XmlIgnoreAttribute>().FirstOrDefault();
            if (ignoreAttr != null)
                continue;

            //check property name redefined in attr
            var propAttr = prop.GetCustomAttributes().OfType<XmlPropertyAttribute>().FirstOrDefault();
            var elementName = propAttr?.PropertyName ?? prop.Name;

            if (prop.PropertyType.IsGenericType && (prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>)))
            {
                var listVal = prop.GetValue(obj) as System.Collections.IEnumerable;
                if (listVal == null)
                    continue;
                var children = listVal.Cast<object>().ToArray();
                if (!children.Any())
                    continue;

                w.WriteStartElement(elementName);
                foreach (object child in children)
                {
                    WrtiteObject(child, w);
                }
                w.WriteEndElement();
            }
            else if (typeof (EdiBaseEntity).IsAssignableFrom(prop.PropertyType))
            {
                var child = prop.GetValue(obj);
                if (child != null)
                    WrtiteObject(child, w);
            }
            else
            {
                var val = prop.GetValue(obj);
                if (val != null)
                {
                    string s = val is IFormattable f
                        ? f.ToString(null, CultureInfo.InvariantCulture)
                        : val.ToString();
                    w.WriteElementString(elementName, s);
                }
            }
        }

        if (writeStratEndElement)
            w.WriteEndElement();
    }
}
