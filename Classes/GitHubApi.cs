using LibGit2Sharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace QuickSearchHelper.Classes
{
    internal class GitHubApi
    {
        public static string DefaultDirectory = AppDomain.CurrentDomain.BaseDirectory;
        public static readonly string TempSettingsFileSaveLocation = Path.Combine(DefaultDirectory, @"Versions\TempVersion");
        public static readonly string AppSettingsFilePath = Path.Combine(DefaultDirectory, @"AppDependancies\AppSettings.txt");

        public static readonly string GitHubRawBaseUrl = "https://raw.githubusercontent.com";
        public static readonly string GitHubRepositoryOwner = "JanArnoCoetzer";
        public static readonly string GitHubRepositoryName = "QuickSearchBeta";
        public static readonly string FilePathInRepository = "AppDependancies/AppSettings.txt";

        public static bool CheckUpdate()
        {
            try
            {
                // Get the version from the web (GitHub)
                string webVersion = GetVersionString();

                // Get the current version from local file
                string currentVersion = GetFieldInFile(AppSettingsFilePath, "Version");

                Debug.WriteLine($"Web Version: {webVersion}, Local Version: {currentVersion}");

                // Compare versions and return true if they are different
                return !string.Equals(webVersion, currentVersion, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking for update: {ex.Message}");
                return false; // Return false in case of any exception
            }
        }

        public static string GetVersionString()
        {
            string rawUrl = $"{GitHubRawBaseUrl}/{GitHubRepositoryOwner}/{GitHubRepositoryName}/master/{FilePathInRepository}";

            using (WebClient webClient = new WebClient())
            {
                try
                {
                    string content = webClient.DownloadString(rawUrl);
                    return ParseVersionFromText(content);
                }
                catch (WebException ex)
                {
                    throw new IOException($"Error accessing file from GitHub: {ex.Message}", ex);
                }
            }
        }

        public static string ParseVersionFromText(string text)
        {
            // Regular expression pattern to match version inside curly braces
            string pattern = @"Version\{([^}]*)\}";

            // Match the pattern in the text
            Match match = Regex.Match(text, pattern);

            // If a match is found, return the version string (group 1 of the match)
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                throw new FormatException("Version string not found in the provided text.");
            }
        }

        public static string GetFieldInFile(string path, string field)
        {
            if (!File.Exists(path))
            {
                Debug.WriteLine($"FilePathError: {path} not found");
                return "";
            }

            string[] lines = File.ReadAllLines(path);

            string pattern = @"\{(.*?)\}";

            foreach (string line in lines)
            {
                Match match = Regex.Match(line, pattern);

                if (match.Success)
                {
                    string content = match.Groups[1].Value.Trim();

                    if (line.Contains(field))
                    {
                        return content;
                    }
                }
            }

            Debug.WriteLine($"Content Error: Field '{field}' not found in {path}");
            return "";
        }

        public static bool DownloadCloneRepository()
        {
            try
            {
                Repository.Clone($"https://github.com/{GitHubRepositoryOwner}/{GitHubRepositoryName}.git", TempSettingsFileSaveLocation);                 
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error cloning repository: {ex.Message}");
                return false;
            }
        }

        public static void MoveFiles()
        {
            string tempDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Versions", "TempVersion");
            string defaultDirectory = AppDomain.CurrentDomain.BaseDirectory;

            try
            {
                // Get all files in the source directory
                string[] files = Directory.GetFiles(tempDirectory);

                // Move each file to the destination directory
                foreach (string file in files)
                {
                    // Get the file name only
                    string fileName = Path.GetFileName(file);

                    // Construct the destination file path
                    string destFile = Path.Combine(defaultDirectory, fileName);

                    // Move the file, overwriting if it already exists
                    File.Copy(file, destFile, true);
                    File.Delete(file); // Optionally delete the source file after copying

                    Debug.WriteLine($"Moved and replaced {fileName} in {defaultDirectory}");
                }

                // Optionally, delete the source directory after moving files
                Directory.Delete(tempDirectory, true); // Delete recursively
                Debug.WriteLine($"Deleted {tempDirectory}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error moving files: {ex.Message}");
            }
        }
    }
}
