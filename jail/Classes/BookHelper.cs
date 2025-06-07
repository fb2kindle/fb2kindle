using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.IO.Compression;
using jail.Models;

namespace jail.Classes {
  public static class BookHelper {
    public static string Transliterate(this string str) {
      string[] latUp = {
        "A", "B", "V", "G", "D", "E", "Yo", "Zh", "Z", "I", "Y", "K", "L", "M", "N", "O", "P", "R", "S", "T", "U", "F",
        "Kh", "Ts", "Ch", "Sh", "Shch", "\"", "Y", "'", "E", "Yu", "Ya"
      };
      string[] latLow = {
        "a", "b", "v", "g", "d", "e", "yo", "zh", "z", "i", "y", "k", "l", "m", "n", "o", "p", "r", "s", "t", "u", "f",
        "kh", "ts", "ch", "sh", "shch", "\"", "y", "'", "e", "yu", "ya"
      };
      string[] rusUp = {
        "А", "Б", "В", "Г", "Д", "Е", "Ё", "Ж", "З", "И", "Й", "К", "Л", "М", "Н", "О", "П", "Р", "С", "Т", "У", "Ф",
        "Х", "Ц", "Ч", "Ш", "Щ", "Ъ", "Ы", "Ь", "Э", "Ю", "Я"
      };
      string[] rusLow = {
        "а", "б", "в", "г", "д", "е", "ё", "ж", "з", "и", "й", "к", "л", "м", "н", "о", "п", "р", "с", "т", "у", "ф",
        "х", "ц", "ч", "ш", "щ", "ъ", "ы", "ь", "э", "ю", "я"
      };
      for (var i = 0; i <= 32; i++) {
        str = str.Replace(rusUp[i], latUp[i]);
        str = str.Replace(rusLow[i], latLow[i]);
      }

      return str;
    }

    public static string TransliterateName(string file) {
      var rus = new[] {
        'а', 'б', 'в', 'г', 'д', 'е', 'ж', 'з', 'и', 'й', 'к', 'л', 'м', 'н', 'о', 'п', 'р', 'с', 'т', 'у', 'ф', 'х',
        'ц', 'ч', 'ш', 'щ', 'ы', 'э', 'ю', 'я', 'ь', '\\', ':', '/', '?', '*', ' '
      };
      var lat = new[] {
        'a', 'b', 'v', 'g', 'd', 'e', 'j', 'z', 'i', 'y', 'k', 'l', 'm', 'n', 'o', 'p', 'r', 's', 't', 'u', 'f', 'h',
        'c', 'h', 's', 's', 'i', 'e', 'u', 'a', '\'', '_', '_', '_', '_', '_', '_'
      };
      var name = "";
      foreach (var t in file) {
        var ch = Char.ToLower(t);
        if (ch == '.')
          break;
        var i = Array.FindIndex(rus, c => c == ch);
        if (i >= 0)
          name += lat[i];
        else if (ch >= '0' && ch <= 127)
          name += ch;
        if (name.Length > 31)
          break;
      }

      return name;
    }

    private static XElement LoadBookWithoutNs(string bookPath) {
      try {
        XElement book;
        //book = XDocument.Parse(File.ReadAllText(bookPath), LoadOptions.PreserveWhitespace).Root;
        //book = ReadXDocumentWithInvalidCharacters(bookPath).Root;
        using (Stream file = File.OpenRead(bookPath)) {
          book = XElement.Load(file, LoadOptions.PreserveWhitespace);
        }

        XNamespace ns = "";
        foreach (var el in book.DescendantsAndSelf()) {
          el.Name = ns.GetName(el.Name.LocalName);
          var atList = el.Attributes().ToList();
          el.Attributes().Remove();
          foreach (var at in atList)
            el.Add(new XAttribute(ns.GetName(at.Name.LocalName), at.Value));
        }

        book = new XElement("book", book.Elements("description"), book.Elements("body"), book.Elements("binary"));
        return book;
      }
      catch (Exception ex) {
        Logger.WriteError(ex, "Unknown file format: " + ex.Message);
        return null;
      }
    }

    // public static string GetApplicationPath() {
    //   return AppDomain.CurrentDomain.BaseDirectory;
    //   // var asm = Assembly.GetExecutingAssembly();
    //   // var directoryInfo = new FileInfo(asm.Location).Directory;
    //   // return directoryInfo != null ? directoryInfo.FullName : Path.GetDirectoryName(asm.Location);
    // }

    // private static string Value(IEnumerable<XElement> source, string defaultResult = null) {
    //   var value = source.Select(element => element.Value).FirstOrDefault();
    //   if (value == null || String.IsNullOrEmpty(value.Trim()))
    //     return defaultResult;
    //   return value.Trim();
    // }
    
    // public class Fb2FileInfo {
    //   public string Title { get; set; }
    //   public string Authors { get; set; }
    //   public string Annotation { get; set; }
    // }
    
    public static string GetBookDetails(string inputFilePath, string annotationsFilePath, string coverFilePath) {

      XElement book = null;

      if (!string.IsNullOrEmpty(coverFilePath) && !File.Exists(coverFilePath)) {
        var coverSaved = false;
        book = LoadBookWithoutNs(inputFilePath);
          
        var coverImage = book.Descendants("coverpage").Elements("image").FirstOrDefault();
        if (coverImage != null) {
          var coverPage = (string) coverImage.Attribute("href");
          if (!string.IsNullOrWhiteSpace(coverPage)) {
            var node = book.XPathSelectElement($"descendant::binary[@id='{coverPage.Replace("#", "")}']");
            if (node != null) {
              File.WriteAllBytes(coverFilePath, Convert.FromBase64String(node.Value));
              coverSaved = true;
            }
          }
        }
        if (!coverSaved) {
          foreach (var binEl in book.Elements("binary")) {
            File.WriteAllBytes(coverFilePath, Convert.FromBase64String(binEl.Value));
            break;
          }
        }
      }

      string annotation = null;
      if (!string.IsNullOrEmpty(annotationsFilePath) && File.Exists(annotationsFilePath)) {
        annotation = File.ReadAllText(annotationsFilePath);
      }
      else {
        if (book == null)
          book = LoadBookWithoutNs(inputFilePath);
        var desc = book.XPathSelectElement("descendant::annotation");
        if (desc != null) {
          annotation = desc.Value;
        }
        else {
          // no annotation - get short part from body
          var body = book.XPathSelectElement("descendant::body");
          if (body != null) {
            annotation = Regex.Replace(body.Value.Trim().Shorten(1024).Replace("\n", "<br/>"), @"\s+", " ")
              .Replace("<br/> <br/> ", "<br/>");
          }
        }

        if (!string.IsNullOrEmpty(annotationsFilePath))
          File.WriteAllText(annotationsFilePath, annotation);
      }

      // if (book == null)
      //   book = LoadBookWithoutNs(inputFilePath);
      // var result = new Fb2FileInfo {
      //   Annotation = annotation,
      //   Title = Value(book.Elements("description").Elements("title-info").Elements("book-title"), ""),
      //   Authors = string.Empty
      // };
      // var authors = book.Elements("description").Elements("title-info").Elements("author");
      // foreach (var ai in authors) {
      //   var author = $"{Value(ai.Elements("last-name"))} {Value(ai.Elements("first-name"))} {Value(ai.Elements("middle-name"))}";
      //   if (!string.IsNullOrWhiteSpace(author)) {
      //     result.Authors += author.Trim();
      //   }
      // }

      return annotation;
    }

    public static void Transform(string inputFile, string outputFile, string xsl) {
      using (var reader = new XmlTextReader(inputFile)) {
        var xslt = new XslCompiledTransform();
        xslt.Load(xsl);
        using (var writer = new XmlTextWriter(outputFile, null)) {
          writer.Formatting = Formatting.Indented;
          xslt.Transform(reader, null, writer, null);
          writer.Close();
        }
      }
    }

    public static void ExtractZipFile(string archivePath, string fileName, string outputFileName) {
      using (var zip = ZipFile.OpenRead(archivePath)) {
        var zipEntry = zip.GetEntry(fileName); 
        if (zipEntry == null)
          throw new FileNotFoundException("Book file not found in archive");
        zipEntry.ExtractToFile(outputFileName);
      }
    }

    public static string GetBookDownloadFileName(BookInfo book, string ext = ".fb2") {
      var fileName = Regex.Replace(Regex.Replace(
        $"{book.Authors.First().FullName.ToLower().Transliterate()}_{book.Title.ToLower().Transliterate()}",
        @"[!@#$%_,. …\[\]\-']", "_"), @"(\p{P})(?<=\1\p{P}+)", "").Trim('_') + ext;
      return fileName;
    }

    public static string GetCorrectedFileName(string filename) {
      var fileName = Regex.Replace(filename.ToLower().Transliterate(), @"[!@#$%_ ']", "_");
      return fileName;
    }
  }
}