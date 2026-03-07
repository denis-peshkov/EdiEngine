namespace EdiEngine.Samples;

internal static class Program
{
    private static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        Sample210.SaveFreightInvoiceEdi();
        Sample210.ReadFreightInvoiceEdiToJson();

        Sample322.SaveTerminalEdi();
        Sample322.ReadTerminalEdiToJson();

        Sample810.SaveInvoiceEdi();
        Sample810.ReadInvoiceEdiToJson();

        Sample850.SavePurchaseOrderEdi();
        Sample850.ReadPurchaseOrderEdiToJson();

        Sample997.SaveAckEdi();
        Sample997.ReadAckEdiToJson();
    }
}
