namespace LegendsTeamVN.Tool.Services
{
    public class FileGeneratorService
    {
        public async Task GenerateFileAsync(string targetPath, string folderName, string fileName, string content)
        {
            var fullDirectoryPath = Path.Combine(targetPath, folderName);
            if (!Directory.Exists(fullDirectoryPath))
            {
                Directory.CreateDirectory(fullDirectoryPath);
            }

            var fullFilePath = Path.Combine(fullDirectoryPath, fileName);
            await File.WriteAllTextAsync(fullFilePath, content);
        }
    }
}
