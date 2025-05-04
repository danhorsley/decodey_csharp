using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace Decodey.Services
{
    /// <summary>
    /// Interface for share service
    /// </summary>
    public interface IShareService
    {
        /// <summary>
        /// Share text with platform share dialog
        /// </summary>
        Task ShareTextAsync(string text, string title = null);

        /// <summary>
        /// Share a file with platform share dialog
        /// </summary>
        Task ShareFileAsync(string filePath, string title = null, string contentType = null);
    }

    /// <summary>
    /// Service for sharing content using platform share functionality
    /// </summary>
    public class ShareService : IShareService
    {
        /// <summary>
        /// Share text with platform share dialog
        /// </summary>
        public async Task ShareTextAsync(string text, string title = null)
        {
            try
            {
                // Create share request
                var request = new ShareTextRequest
                {
                    Text = text,
                    Title = title ?? "Share"
                };

                // Use platform share
                await Share.RequestAsync(request);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sharing text: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Share a file with platform share dialog
        /// </summary>
        public async Task ShareFileAsync(string filePath, string title = null, string contentType = null)
        {
            try
            {
                // Create share request
                var request = new ShareFileRequest
                {
                    Title = title ?? "Share File",
                    File = new ShareFile(filePath)
                    {
                        ContentType = contentType
                    }
                };

                // Use platform share
                await Share.RequestAsync(request);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sharing file: {ex.Message}");
                throw;
            }
        }
    }
}