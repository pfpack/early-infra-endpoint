using System;
using Microsoft.OpenApi.Models;

namespace PrimeFuncPack;

public interface ISwaggerBuilder
{
    ISwaggerBuilder Use(Action<OpenApiDocument> configurator);

    OpenApiDocument Build();
}