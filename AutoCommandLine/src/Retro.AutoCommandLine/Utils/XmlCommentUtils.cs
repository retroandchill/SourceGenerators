using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
namespace Retro.AutoCommandLine.Utils;

public static class XmlCommentUtils {

  private static readonly Regex Trimmer = new(@"\s\s+");

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