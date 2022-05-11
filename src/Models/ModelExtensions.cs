using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Models;

public static class ModelExtensions
{
    readonly static JsonSerializerOptions _jsonOpts = new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.IgnoreCycles, IncludeFields = true };
    const string _defaultJson = "{\"Object\":\"null\"}";

    // general DI practice to provide fluent one-liners wrapping detailed code, and which can be called during host startup
    public static void RegisterDataContext(this IServiceCollection serviceCollection, string connString)
    {
        serviceCollection.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(connString));
        serviceCollection.AddSingleton<IDataContext, RedisDataContext>();
    }

    /// <summary>Serialize <paramref name="src"/> to a flattened json string using the built-in serializer.</summary>
    internal static string ToFlatJson<T>(this T src)
    {
        if (src.IsNullOrDefault())
            return _defaultJson;
        try
        {
            return JsonSerializer.Serialize(src, _jsonOpts);
        }
        catch (Exception ex)
        {
            return $"{{\"SerializationError\":\"{ex.Message}\"}}";
        }
    }

    /// <summary>Deserialize <paramref name="json"/> to <typeparamref name="T"/>.</summary>
    internal static T? FromJson<T>(this string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return default;
        try
        {
            return JsonSerializer.Deserialize<T>(json, _jsonOpts);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.GetType().Name}: {ex.Message}");
            return default;
        }
    }

    /// <summary>True if reference or nullable instance is null or if value instance is default.</summary>
    internal static bool IsNullOrDefault<T>([NotNullWhen(false)] this T src) => EqualityComparer<T>.Default.Equals(src, default(T));
}
