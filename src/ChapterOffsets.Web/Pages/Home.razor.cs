using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;

namespace ChapterOffsets.Web.Pages;

public sealed partial class Home : ComponentBase
{
    private string input = "";
    private string output = "";
    private bool showSeconds = false;

    private void Convert()
    {
        try
        {
            var chapters = input.Split(["\r\n", "\n"], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(InputChapter.Parse)
                .ToArray();

            var resultingChapters = new List<OutputChapter>(capacity: chapters.Length);
            var offset = TimeSpan.Zero;

            foreach (var chapter in chapters)
            {
                resultingChapters.Add(new OutputChapter(chapter.Name, offset) { ShowSeconds = showSeconds });
                offset += chapter.Duration;
            }

            output = string.Join("\n", resultingChapters);
        }
        catch (Exception e)
        {
            output = e.Message;
        }
    }

    [GeneratedRegex(@"^(?<Name>.+)\s+((?<Hours>\d{1,2}):)?((?<Minutes>\d{1,2}):)(?<Seconds>\d{1,2})$", RegexOptions.Compiled)]
    private static partial Regex InputChapterRegex { get; }

    private readonly record struct InputChapter(string Name, TimeSpan Duration)
    {
        public static InputChapter Parse(string input)
        {
            var match = InputChapterRegex.Match(input);
            if (!match.Success)
            {
                throw new FormatException($"Cannot parse line '{input}' as chapter.");
            }

            var chapterName = match.Groups["Name"].Value;
            var hours = match.Groups["Hours"].Success ? match.Groups["Hours"].Value : "0";
            var minutes = match.Groups["Minutes"].Value;
            var seconds = match.Groups["Seconds"].Value;

            return new InputChapter(chapterName, new TimeSpan(int.Parse(hours), int.Parse(minutes), int.Parse(seconds)));
        }
    }

    private readonly record struct OutputChapter(string Name, TimeSpan Offset)
    {
        public bool ShowSeconds { get; init; }

        public override string ToString()
        {
            return ShowSeconds ? $"{Name} {Offset.TotalSeconds}" : $"{Name} {Offset.Hours:00}:{Offset.Minutes:00}:{Offset.Seconds:00}";
        }
    }
}
