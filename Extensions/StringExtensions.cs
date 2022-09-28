using System.Text.RegularExpressions;
using System.Web;

namespace httpDownloader.Extensions
{
    public static class StringExtensions
    {
        public static bool UrlContainsFile(this string input, out string result)
        {
            var decoded = HttpUtility.UrlDecode(input);

            //Handling url with parameters
            if (decoded.IndexOf("?") is { } queryIndex && queryIndex != -1)
            {
                decoded = decoded.Substring(0, queryIndex);
            }
            var fileName = Path.GetFileName(decoded);

            if (HasValidFileExtension(fileName))
            {
                result = fileName;
                return true;
            }
            result = string.Empty;
            return false;
        }

        public static bool HasValidFileExtension(this string fileName)
        {
            var validFileExtensions = @"^.*\.(jpg|JPG|png|PNG|gif|GIF|doc|DOC|docx|DOCX|pdf|PDF|txt|TXT)$";

            return Regex.IsMatch(
                fileName,
                validFileExtensions);
        }

        public static bool IsValidUrl(this string webSiteUrl)
        {
            return Uri.TryCreate(webSiteUrl, UriKind.Absolute, out Uri uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp
                  || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}
