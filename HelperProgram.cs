using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using QuickSearchHelper.Classes;

namespace QuickSearchHelper
{
    internal static class HelperProgram
    {
        public static string DefaultDirectory = AppDomain.CurrentDomain.BaseDirectory;
        
        [STAThread]
        static async Task Main(string[] args)
        {
            if (QuickSearchHelper.Classes.GitHubApi.CheckUpdate())
            {
                QuickSearchHelper.Classes.GitHubApi.DownloadCloneRepository();
                CloseQuickSearchApplication();
                QuickSearchHelper.Classes.GitHubApi.MoveFiles();
                ReopenQuickSearchApplication();
            }
            else { }

            Debug.WriteLine("No update needed. Exiting QuickSearchHelper.");           
            await Task.Delay(3000); 
            Environment.Exit(0); 
        }
        public static void CloseQuickSearchApplication()
        {
            // Find all processes with name "QuickSearch"
            Process[] processes = Process.GetProcessesByName("QuickSearch");

            // Iterate through each process and close it
            foreach (Process process in processes)
            {
                try
                {
                    // Try to close the main window of the process gracefully
                    process.CloseMainWindow();
                    process.WaitForExit(5000); // Wait for up to 5 seconds for the process to exit gracefully
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that occur during closing
                    Debug.WriteLine($"Error closing process: {ex.Message}");
                }

                if (!process.HasExited)
                {
                    // If the process hasn't exited, kill it forcefully
                    try
                    {
                        process.Kill();
                    }
                    catch (Exception ex)
                    {
                        // Handle any exceptions that occur during killing
                        Debug.WriteLine($"Error killing process: {ex.Message}");
                    }
                }
            }
        }

        public static void ReopenQuickSearchApplication()
        {
            try
            {
                // Specify the path to QuickSearch.exe
                string quickSearchPath = DefaultDirectory+ "QuickSearch.exe"; // Replace with your actual path

                // Start QuickSearch.exe
                Process.Start(quickSearchPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reopening QuickSearch.exe: {ex.Message}");
            }
        }
    }
}