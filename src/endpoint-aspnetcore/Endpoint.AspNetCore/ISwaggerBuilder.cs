using System;
using Microsoft.OpenApi;

namespace PrimeFuncPack;

public interface ISwaggerBuilder
{
    ISwaggerBuilder Use(Action<OpenApiDocument> configurator);

    OpenApiDocument Build();
}