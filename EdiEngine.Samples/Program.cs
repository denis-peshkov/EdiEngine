namespace EdiEngine.Samples;

internal static class Program
{
    private static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        Sample210.Save210Edi();
        Sample210.Read210EdiToJson();
        Sample210.Read210JsonToXml();
        Sample210.Read210XmlToJson();

        Sample322.Save322Edi();
        Sample322.Read322EdiToJson();
        Sample322.Read322JsonToXml();
        Sample322.Read322XmlToJson();

        Sample810.Save810Edi();
        Sample810.Read810EdiToJson();
        Sample810.Read810JsonToXml();
        Sample810.Read810XmlToJson();

        Sample850.Save850Edi();
        Sample850.Read850EdiToJson();
        Sample850.Read850JsonToXml();
        Sample850.Read850XmlToJson();

        Sample997.Save997Edi();
        Sample997.Read997EdiToJson();
        Sample997.Read997JsonToXml();
        Sample997.Read997XmlToJson();
    }
}
