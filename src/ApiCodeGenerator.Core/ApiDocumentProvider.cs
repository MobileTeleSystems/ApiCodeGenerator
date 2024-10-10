using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using ApiCodeGenerator.Core.NswagDocument;
using static ApiCodeGenerator.Core.GetDocumentReaderResult;

namespace ApiCodeGenerator.Core
{
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1649:FileNameMustMatchTypeName", Justification = "Reviewed.")]
    internal interface IApiDocumentProvider
    {
        Task<GetDocumentReaderResult?> GetDocumentReaderAsync(DocumentGenerator documentSource);
    }

    internal class ApiDocumentProvider : IApiDocumentProvider
    {
        private readonly IFileProvider _fileProvider;
        private readonly HttpClient _httpClient;

        public ApiDocumentProvider()
            : this(new PhysicalFileProvider(), new())
        {
        }

        public ApiDocumentProvider(IFileProvider fileProvider, HttpClient httpClient)
        {
            _fileProvider = fileProvider;
            _httpClient = httpClient;
        }

        public async Task<GetDocumentReaderResult?> GetDocumentReaderAsync(DocumentGenerator documentSource)
        {
            if (documentSource.FromDocument != null)
            {
                if (string.IsNullOrEmpty(documentSource.FromDocument.Json))
                {
                    if (string.IsNullOrEmpty(documentSource.FromDocument.Url))
                    {
                        return null;
                    }
                    else
                    {
                        if (Uri.TryCreate(documentSource.FromDocument.Url, UriKind.Absolute, out var url))
                        {
                            return await FromUrlAsync(url);
                        }
                        else
                        {
                            return Failed("Invalid url format", null);
                        }
                    }
                }
                else
                {
                    var json = documentSource.FromDocument.Json!;
                    if (IsPath(json))
                    {
                        return await FromFileAsync(json);
                    }
                    else
                    {
                        return FromContent(json, null);
                    }
                }
            }

            if (documentSource.JsonSchemaToOpenApi != null)
            {
                var data = documentSource.JsonSchemaToOpenApi;
                if (string.IsNullOrEmpty(data.Name))
                {
                    return Failed("jsonSchemaToOpenApi.name must be not null or empty", null);
                }

                if (string.IsNullOrEmpty(data.Schema))
                {
                    return Failed("jsonSchemaToOpenApi.schema must be not null or empty", null);
                }

                if (IsPath(data.Schema))
                {
                    var result = await FromFileAsync(data.Schema);
                    if (result.Reader is not null)
                    {
                        data.Schema = result.Reader.ReadToEnd();
                        result.Reader.Close();
                    }
                    else
                    {
                        return result;
                    }
                }
                else if (Uri.TryCreate(data.Schema, UriKind.Absolute, out var uri))
                {
                    var result = await FromUrlAsync(uri);
                    if (result.Reader is not null)
                    {
                        data.Schema = await result.Reader.ReadToEndAsync();
                        result.Reader.Close();
                    }
                    else
                    {
                        return result;
                    }
                }

                var content = $"{{\"openapi\":\"3.0.0\",\"components\":{{\"schemas\":{{\"{data.Name}\":{data.Schema}}}}}}}";
                return FromContent(content, null);
            }

            return null;
        }

        private static GetDocumentReaderResult FromContent(string content, string? filePath)
        {
            return Success(new StringReader(content), filePath);
        }

        private async Task<GetDocumentReaderResult> FromFileAsync(string path)
        {
            try
            {
                using var stream = _fileProvider.OpenRead(path);
                using var reader = new StreamReader(stream);
                var content = await reader.ReadToEndAsync();
                return FromContent(content, path);
            }
            catch (Exception ex)
            {
                return Failed(ex.Message, path);
            }
        }

        private async Task<GetDocumentReaderResult> FromUrlAsync(Uri url)
        {
            var strUrl = url.ToString();
            if (url.Scheme == "file")
            {
                return await FromFileAsync(strUrl);
            }

            string? content;
            try
            {
                var response = await _httpClient.GetAsync(strUrl);
                response.EnsureSuccessStatusCode();
                content = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                return Failed(ex.Message, strUrl);
            }

            return FromContent(content, strUrl);
        }

        private bool IsPath(string json)
        {
            return json.IndexOf('\n') == -1 && json.IndexOf(':', 2) == -1 && json.IndexOf('{') == -1;
        }
    }
}
