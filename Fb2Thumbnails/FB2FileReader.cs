using System;
using System.Xml;
using System.IO;

namespace Fb2Kindle {

  public class Fb2FileReader {
    private string fData;

    public Fb2FileReader() {
      fData = null;
    }

    /// <summary>
    /// In case if there are no covers in current file just returning a 0 to prevent
    /// performing of a further work
    /// </summary>
    /// <returns></returns>
    public int Read(byte[] bytes) {
      fData = GetImageContent(bytes);
      return fData?.Length ?? 0;
    }

    /// <summary>
    /// Reading a list of "coverpage" XML entries, after that trying to find 
    /// the content of a first entry from this list.
    /// </summary>
    /// <returns></returns>
    private string GetImageContent(byte[] bytes) {
      var vStream = new MemoryStream(bytes);
      var vFb2 = new XmlDocument();
      vFb2.Load(vStream);
      var vCovers = vFb2.GetElementsByTagName("coverpage");
      var vContent = vFb2.GetElementsByTagName("binary");
      if (vCovers.Count <= 0) return null;
      var vImage = vCovers.Item(0)?.FirstChild;
      if (vImage == null) return null;
      if (!vImage.Name.Equals("image")) return null;
      var vAttributes = vImage.Attributes;
      var vHref = vAttributes?.GetNamedItem("l:href")?.Value?.Substring(1);
      if (vHref == null) return null;
      return GetContent(vHref, vContent);
    }

    private string GetContent(string name, XmlNodeList data) {
      string vResult = null;
      for (var i = 0; i < data.Count; i++) {
        var vAttributes = data[i].Attributes;
        var vName = vAttributes?.GetNamedItem("id")?.Value;
        if (!string.Equals(vName, name, StringComparison.OrdinalIgnoreCase)) continue;
        vResult = data[i].InnerText;
        break;
      }
      return vResult;
    }

    /// <summary>
    /// Images in FB2 are stored in base64 encoding, decoding stored data
    /// </summary>
    /// <returns></returns>
    public byte[] GetBuffer() {
      byte[] vData;
      try {
        vData = Convert.FromBase64String(fData);
      }
      catch {
        vData = null;
      }
      finally {
        fData = null;
      }
      return vData;
    }
  }
}