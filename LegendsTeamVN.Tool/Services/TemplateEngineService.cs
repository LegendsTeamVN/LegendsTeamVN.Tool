using LegendsTeamVN.Tool.Models;
using Scriban;

namespace LegendsTeamVN.Tool.Services
{
    public class TemplateEngineService
    {
        private readonly IWebHostEnvironment _env;

        public TemplateEngineService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> GenerateContentAsync(string templateName, TableInfo table, string targetNamespace)
        {
            var templatesDir = Path.Combine(_env.ContentRootPath, "Templates");
            var templatePath = Path.Combine(templatesDir, $"{templateName}.scriban");

            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Template '{templateName}.scriban' not found at {templatePath}");
            }

            var templateText = await File.ReadAllTextAsync(templatePath);
            var template = Template.Parse(templateText);

            if (template.HasErrors)
            {
                var errors = string.Join("\n", template.Messages.Select(x => x.Message));
                throw new InvalidOperationException($"Template parsing errors: \n{errors}");
            }

            var entityName = table.Name.EndsWith("s") ? table.Name.Substring(0, table.Name.Length - 1) : table.Name;
            // Simple English pluralization rule for 'ies' (e.g. Categories -> Category)
            if (table.Name.EndsWith("ies"))
            {
                entityName = table.Name.Substring(0, table.Name.Length - 3) + "y";
            }

            var result = await template.RenderAsync(new { 
                Table = table,
                EntityName = entityName,
                EntityNamePlural = table.Name,
                Namespace = targetNamespace
            });

            return result;
        }

        public List<string> GetAvailableTemplates()
        {
            var templatesDir = Path.Combine(_env.ContentRootPath, "Templates");
            if (!Directory.Exists(templatesDir))
            {
                return new List<string>();
            }

            return Directory.GetFiles(templatesDir, "*.scriban")
                            .Select(Path.GetFileNameWithoutExtension)
                            .ToList()!;
        }
    }
}
