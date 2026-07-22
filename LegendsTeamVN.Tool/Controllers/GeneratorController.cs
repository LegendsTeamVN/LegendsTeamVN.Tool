using LegendsTeamVN.Tool.Models;
using LegendsTeamVN.Tool.Services;
using Microsoft.AspNetCore.Mvc;

namespace LegendsTeamVN.Tool.Controllers
{
    public class GeneratorController : Controller
    {
        private readonly DatabaseSchemaReader _schemaReader;
        private readonly TemplateEngineService _templateEngine;
        private readonly FileGeneratorService _fileGenerator;
        private readonly IConfiguration _config;

        public GeneratorController(
            DatabaseSchemaReader schemaReader,
            TemplateEngineService templateEngine,
            FileGeneratorService fileGenerator,
            IConfiguration config)
        {
            _schemaReader = schemaReader;
            _templateEngine = templateEngine;
            _fileGenerator = fileGenerator;
            _config = config;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var connectionString = _config.GetConnectionString("DefaultConnection") ?? string.Empty;
            if (string.IsNullOrEmpty(connectionString))
            {
                return Content("Error: DefaultConnection is not configured in appsettings.json.");
            }

            try
            {
                var tables = await _schemaReader.GetTablesAsync(connectionString);
                var templates = _templateEngine.GetAvailableTemplates();

                ViewBag.Templates = templates;
                ViewBag.ConnectionString = connectionString;

                return View(tables);
            }
            catch (Exception ex)
            {
                return Content("Connection failed: " + ex.Message);
            }
        }



        [HttpGet("Generator/TableDetails")]
        public async Task<IActionResult> TableDetails([FromQuery] string schema, [FromQuery] string tableName)
        {
            var connectionString = _config.GetConnectionString("DefaultConnection") ?? string.Empty;
            if (string.IsNullOrEmpty(connectionString)) return BadRequest("No connection string configured.");

            try
            {
                var tableDetails = await _schemaReader.GetTableDetailsAsync(connectionString, schema ?? "dbo", tableName);
                return Json(tableDetails);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Generate(ScaffoldRequestModel request)
        {
            if (!request.SelectedTables.Any())
            {
                return BadRequest("Missing required fields.");
            }

            var availableTemplates = _templateEngine.GetAvailableTemplates();

            using var memoryStream = new MemoryStream();
            using (var archive = new System.IO.Compression.ZipArchive(memoryStream, System.IO.Compression.ZipArchiveMode.Create, true))
            {
                foreach (var tableName in request.SelectedTables)
                {
                    var tableInfo = await _schemaReader.GetTableDetailsAsync(request.ConnectionString, "dbo", tableName); // Using dbo as default schema for now
                    
                    foreach (var templateName in availableTemplates)
                    {
                        var fullContent = await _templateEngine.GenerateContentAsync(templateName, tableInfo, "LegendsTeamVN.BadmintonClub.Domain.Entities"); // Default namespace, can be dynamic later
                        
                        var lines = fullContent.Split('\n');
                        string filePath;
                        string actualContent;

                        if (lines.Length > 0 && lines[0].Trim().StartsWith("##PATH:"))
                        {
                            filePath = lines[0].Replace("##PATH:", "").Trim();
                            actualContent = string.Join('\n', lines.Skip(1));
                        }
                        else
                        {
                            filePath = templateName.Contains("{Entity}") 
                                ? templateName.Replace("{Entity}", tableName) + ".cs" 
                                : $"{tableName}.cs";
                            actualContent = fullContent;
                        }

                        // Sanitize path for zip
                        filePath = filePath.Replace("\\", "/").TrimStart('/');

                        var zipEntry = archive.CreateEntry(filePath);
                        using var entryStream = zipEntry.Open();
                        using var streamWriter = new StreamWriter(entryStream);
                        await streamWriter.WriteAsync(actualContent);
                    }
                }
            }

            return File(memoryStream.ToArray(), "application/zip", "GeneratedCode.zip");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return Content("An error occurred while processing your request.");
        }
    }
}
