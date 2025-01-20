using System.Text.Json.Serialization;

namespace Custom.Engine.Agent.Models;

internal class Feedback
{
    [JsonPropertyName("feedbackText")]
    public string FeedbackText { get; set; }
}