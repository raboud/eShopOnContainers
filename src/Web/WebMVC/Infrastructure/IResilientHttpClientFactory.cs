using Microsoft.BuildingBlocks.Resilience.Http;
using System;

namespace HMS.WebMVC.Infrastructure
{
    public interface IResilientHttpClientFactory
    {
        ResilientHttpClient CreateResilientHttpClient();
    }
}