using Amazon;
using Amazon.Comprehend;
using Amazon.Comprehend.Model;
using System.Text;

var region = RegionEndpoint.USWest2;

if (args.Length == 1)
{
    var filename = args[0];
    var analysisType = (args.Length > 1) ? args[1] : "text";

    var client = new AmazonComprehendClient(region);

    Console.WriteLine($"Processing file {filename} with Amazon Comprehend");

    var text = File.ReadAllText(filename);

    var request = new DetectSentimentRequest()
    {
        Text = text,
        LanguageCode = LanguageCode.En
    };

    Console.WriteLine("--- input text:");
    Console.WriteLine(text);
    Console.WriteLine();

    // sentiment analysis

    var response = await client.DetectSentimentAsync(request);

    Console.WriteLine($"Sentiment: {response.Sentiment} (Positive: {response.SentimentScore.Positive:N}% | Negative: {response.SentimentScore.Negative:N}% | Neutral: {response.SentimentScore.Neutral:N}% | Mixed: {response.SentimentScore.Mixed:N}%)");

    var requestEntities = new DetectEntitiesRequest()
    {
        Text = File.ReadAllText(filename),
        LanguageCode = LanguageCode.En
    };

    // detect entities

    var responseEntities = await client.DetectEntitiesAsync(requestEntities);

    foreach(var entity in responseEntities.Entities)
    {
        Console.WriteLine($"entity Type: {entity.Type.Value} | Text: {entity.Text}");
    }

    // detect personally identifiable information (PII)

    var requestPII = new DetectPiiEntitiesRequest()
    {
        Text = File.ReadAllText(filename),
        LanguageCode = LanguageCode.En
    };

    var responsePII = await client.DetectPiiEntitiesAsync(requestPII);

    if (responsePII.Entities.Count > 0)
    {
        var redactedText = new StringBuilder(requestPII.Text);

        foreach (var entity in responsePII.Entities)
        {
            Console.WriteLine($"PII entity Type: {entity.Type.Value} | Text: {entity.BeginOffset}-{entity.EndOffset}");
            for (var pos = entity.BeginOffset; pos < entity.EndOffset; pos++)
            {
                redactedText[pos] = 'X';
            }
        }
        Console.WriteLine();
        Console.WriteLine($"--- redacted text:");
        Console.WriteLine(redactedText);
    }

    Environment.Exit(0);
}

Console.WriteLine("?Invalid parameter - command line format: dotnet run -- <file>");
