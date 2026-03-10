# Cross.EdiEngine [![Nuget](https://img.shields.io/nuget/v/Cross.EdiEngine.svg)](https://nuget.org/packages/Cross.EdiEngine/) [![Documentation](https://img.shields.io/badge/docs-wiki-yellow.svg)](https://github.com/denis-peshkov/Cross.EdiEngine/wiki)

Simple .NET EDI Reader, Writer and Validator.
Read, Write and Validate X12 EDI files with EDI Parser written on C#.

Main Features:
* **EDI to JSON and JSON to EDI conversion**. Uses System.Text.Json (replaced Newtonsoft.Json) for serialization and a custom JSON reader for deserialization. JSON is a handy extension for the library. Imagine you can parse your EDI object directly in Angular or jQuery app.
* **EDI to XML and XML to EDI conversion**. EdiEngine does not use XML as intermediate format, as many other engines do. It uses POCO objects and XML is just an extension.
* **Deterministic output**: control segments (ISA, GS, ST, SE, IEA, GE) and XML serialization use InvariantCulture for numbers and dates, so EDI/XML/JSON do not contain locale-dependent characters (e.g. NNBSP).
* **Configurable EDI X12 997 - Functional Acknowledgment** generation.
  You can setup whether to accept all messages, accept but say errors were noted or reject depending on your needs.
* **HL Loop Hierarchical parsing** - Create a real tree structure based on HL segment hierarchy. No need to map every HL to map, this means one map can serve multiple needs. Say for ASN it can be S-O-P-I or S-O-I hierarchy in one map.
* **Syntax Notes**. All types of [EDI Syntax notes](https://github.com/denis-peshkov/Cross.EdiEngine/wiki/Syntax-Notes) are supported. P Paired, R Required, E Exclusion, C Conditional, L List Conditional
* **Composite Data Elements** are supported, which is really important for HIPAA and sometimes for other transactions even in retail.
* **X12 Maps** Current repository contains all 004010 maps, including Purchase Order, Invoice, Shipment and many others.
You can easily craft yours on their basis.
* **.NET Standard 2.1, .NET 6, .NET 8 and Source Linking**. Multi-targeting support. Source linking enabled and symbol package is published to NuGet symbols server, making debugging easier.

## Install NuGet package

Install the package _Cross.EdiEngine_ [NuGet package](https://www.nuget.org/packages/Cross.EdiEngine/) into your ASP.NET Core project:

```powershell
Install-Package Cross.EdiEngine
```
or
```bash
dotnet add package Cross.EdiEngine
```

## Licensing

Для production требуется лицензия. Ключ лицензии задаётся одним из способов (приоритет: 1 → 2 → 3):

1. **appsettings.json** — вызвать `EdiEngineConfiguration.ConfigureLicensing(configuration)` при старте приложения (namespace EdiEngine.Licensing):

```json
{
  "EdiEngine": {
    "LicenseKey": "ваш_jwt_ключ_лицензии"
  }
}
```

```csharp
// ASP.NET Core
EdiEngineConfiguration.ConfigureLicensing(builder.Configuration);

// Консольное приложение
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();
EdiEngineConfiguration.ConfigureLicensing(configuration);
```

2. **Переменная окружения** `CROSS_EDIENGINE_LICENSE_KEY`

3. Без ключа — разрешено для разработки и тестирования; в production требуется лицензия. Подробнее: https://peshkov.biz

## How To's

Please use [Wiki](https://github.com/denis-peshkov/Cross.EdiEngine/wiki) for documentation and usage examples.

### Usage examples

**EdiEngine.Samples** — runnable examples for 210 (Freight Invoice), 322 (Terminal), 810 (Invoice), 850 (Purchase Order), 997 (Functional Acknowledgment):

```bash
dotnet run --project EdiEngine.Samples
```

**EdiEngine.Tests** — unit tests with additional examples. Clone the repository to explore.

## Roadmap:
 - ~~Json Serialization and Deserialization~~
 - ~~Xml Serialization and Deserialization~~
 - ~~Craft more maps (Added all 004010 maps)~~
 - ~~997 generation~~
 - ~~HL loop hierarchical parsing~~
 - ~~Syntax Notes~~
 - ~~Composite data elements~~
 - ~~.NET Core support~~
 - HIPAA support
