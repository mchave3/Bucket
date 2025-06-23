using System.Globalization;
using System.Text.Json;

namespace Bucket.Tests;

public class DateParsingTest
{
    public static void TestDateParsing()
    {
        var testJson = """
        {
          "CreatedTime": "2024-09-06T06:15:51.358",
          "ModifiedTime": "2024-09-06T06:52:41.686"
        }
        """;

        using var document = JsonDocument.Parse(testJson);
        var root = document.RootElement;

        if (root.TryGetProperty("CreatedTime", out var createdTimeProperty))
        {
            var createdTimeString = createdTimeProperty.GetString();
            Console.WriteLine($"CreatedTime string: {createdTimeString}");

            if (DateTime.TryParse(createdTimeString, null, DateTimeStyles.RoundtripKind, out var createdTime))
            {
                Console.WriteLine($"Parsed CreatedTime: {createdTime}");
            }
            else
            {
                Console.WriteLine("Failed to parse CreatedTime");
            }
        }

        if (root.TryGetProperty("ModifiedTime", out var modifiedTimeProperty))
        {
            var modifiedTimeString = modifiedTimeProperty.GetString();
            Console.WriteLine($"ModifiedTime string: {modifiedTimeString}");

            if (DateTime.TryParse(modifiedTimeString, null, DateTimeStyles.RoundtripKind, out var modifiedTime))
            {
                Console.WriteLine($"Parsed ModifiedTime: {modifiedTime}");
            }
            else
            {
                Console.WriteLine("Failed to parse ModifiedTime");
            }
        }
    }
}
