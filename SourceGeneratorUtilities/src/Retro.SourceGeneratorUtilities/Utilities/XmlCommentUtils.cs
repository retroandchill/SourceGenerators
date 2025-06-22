using System.Text.RegularExpressions;
using System.Xml;
#if SOURCE_UTILS_GENERATOR
using RhoMicro.CodeAnalysis;
#endif

namespace Retro.SourceGeneratorUtilities.Utilities;

/// <summary>
/// Provides utility methods for processing and extracting data from XML-based documentation comments.
/// </summary>
#if SOURCE_UTILS_GENERATOR
[IncludeFile]
#endif
internal static class XmlCommentUtils {

  private static readonly Regex Trimmer = new(@"\s\s+", RegexOptions.None, TimeSpan.FromSeconds(1));

  /// <summary>
  /// Extracts and trims the content of the &lt;summary> tag from an XML comment string.
  /// The method removes excessive whitespace, normalizing it to single spaces.
  /// </summary>
  /// <param name="xmlComment">The XML comment as a string. Can be null, empty, or contain whitespace.</param>
  /// <returns>
  /// A string containing the trimmed content of the &lt;summary> tag if it exists; null if the tag is absent,
  /// the input is null, only whitespace, or improperly formatted.
  /// </returns>
  public static string? GetSummaryTag(this string? xmlComment) {
    if (string.IsNullOrWhiteSpace(xmlComment)) {
      return null;
    }
    
    var doc = new XmlDocument();
    doc.LoadXml(xmlComment);

    if (doc.DocumentElement is null) {
      return null;
    }

    if (doc.DocumentElement.FirstChild.NodeType == XmlNodeType.Text) {
      return Trimmer.Replace(doc.DocumentElement.InnerText.Trim(), " ");
    }

    var summaryElements = doc.DocumentElement.GetElementsByTagName("summary");
    return summaryElements.Count > 0 ? Trimmer.Replace(summaryElements[0].InnerText.Trim(), " ") : null;
  }
  
}