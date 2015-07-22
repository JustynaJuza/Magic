using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Magic.Models.Extensions;

namespace Magic.Helpers
{
    public interface IFileHandler
    {
        Task<bool> SaveFile(Stream fileStream, string fileName, string uploadPath = "");
        string GetAppRelativeFilePath(string fileName, string uploadPath = "");
        bool PassImageFileTypeConstraint(string fileContentType, bool allowImageOnly);
        bool PassImageSizeConstraint(Stream fileStream, int width, int height);
        int GetImageWidth(string filePath);
        string GetFileIconAsString(string filePath);
    }

    public class FileHandler : IFileHandler
    {
        private readonly IPathProvider _pathProvider;

        public FileHandler(IPathProvider pathProvider)
        {
            _pathProvider = pathProvider;
        }

        public async Task<bool> SaveFile(Stream fileStream, string fileName, string uploadPath = "")
        {
            try
            {
                var path = GetServerFilePath(fileName, uploadPath);
                PrepareFileDirectory(path);
                await SaveFileInBlocksAsync(path, fileStream);
                return true;
            }
            catch (Exception ex)
            {
                if (ex.HandleException(typeof(IOException), typeof(SecurityException), typeof(UnauthorizedAccessException)))
                {
                    return false;
                }
                throw;
            }
        }

        public string GetAppRelativeFilePath(string fileName, string uploadPath = "")
        {
            var path = "/Content" + uploadPath + "/";
            var serverPath = _pathProvider.GetServerPath(path);

            var filePath = serverPath + fileName;
            try
            {
                // check if full path is accessible
                Path.GetFullPath(filePath);
                return path + fileName;
            }
            catch (Exception ex)
            {
                if (ex.HandleException(typeof(PathTooLongException)))
                {
                    return string.Empty;
                }
                throw;
            }
        }

        public bool PassImageFileTypeConstraint(string fileContentType, bool allowImageOnly)
        {
            return !allowImageOnly || Regex.IsMatch(fileContentType, "image");
        }

        public int GetImageWidth(string filePath)
        {
            var serverPath = _pathProvider.GetServerPath("~" + filePath);
            try
            {
                using (var image = Image.FromFile(serverPath))
                {
                    return image.Size.Width;
                }
            }
            catch (Exception ex)
            {
                if (ex.HandleException(typeof(IOException)))
                {
                    return 0;
                }
                throw;
            }
        }

        public bool PassImageSizeConstraint(Stream fileStream, int width = int.MaxValue, int height = int.MaxValue)
        {
            var image = Image.FromStream(fileStream, true, false);
            return !(image.Size.Width > width || image.Size.Height > height);
        }

        public string GetFileIconAsString(string filePath)
        {
            var serverPath = _pathProvider.GetServerPath(filePath);
            var icon = Icon.ExtractAssociatedIcon(serverPath);
            if (icon == null)
            {
                new Exception("A failed attempt was made to extract an icon from '" + serverPath + "'.").LogException();
                return UploaderHelpers.ErrorImage;
            }

            var image = icon.ToBitmap();
            using (var stream = new MemoryStream())
            {
                image.Save(stream, ImageFormat.Png);
                return Convert.ToBase64String(stream.ToArray());
            }
        }

        private string GetServerFilePath(string fileName, string uploadPath)
        {
            var path = "/Content" + uploadPath + "/";
            var serverPath = _pathProvider.GetServerPath(path);
            var filePath = serverPath + fileName;
            try
            {
                // check if full path is accessible
                Path.GetFullPath(filePath);
                return filePath;
            }
            catch (Exception ex)
            {
                ex.LogException();
                throw;
            }
        }

        private void PrepareFileDirectory(string serverPath)
        {
            if (!Directory.Exists(serverPath))
            {
                try
                {
                    Directory.CreateDirectory(serverPath);
                }
                catch (Exception ex)
                {
                    ex.LogException();
                    throw;
                }
            }
        }

        private Task SaveFileInBlocksAsync(string serverPath, Stream fileStream, int blockByteSize = 1048576) // 1048576B = 1MB
        {
            var bytesTransferred = 0;
            var buffer = new byte[blockByteSize];
            var fileSavingOperations = new List<Task>();
            fileStream.Seek(0, SeekOrigin.Begin);

            try
            {
                using (var file = new FileStream(serverPath, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, FileOptions.Asynchronous))
                // 4096 is default, async prevents the file from breaking when writing from the middle of the file
                {
                    int currentByteBlockSize;
                    do
                    {
                        currentByteBlockSize = fileStream.Read(buffer, 0, buffer.Length);
                        file.Seek(bytesTransferred, SeekOrigin.Begin);
                        var fileSaving = file.WriteAsync(buffer, 0, currentByteBlockSize);

                        bytesTransferred += currentByteBlockSize;

                        fileSavingOperations.Add(fileSaving);
                    } while (currentByteBlockSize != 0);

                    return Task.WhenAll(fileSavingOperations);
                }
            }
            catch (Exception ex)
            {
                ex.LogException();
                throw;
            }
        }
    }

    public class FileSavingException : Exception
    {
        public FileSavingException()
        {
            // Add implementation.
        }
        public FileSavingException(string message)
        {
            // Add implementation.
        }
        public FileSavingException(string message, Exception inner)
        {
            // Add implementation.
        }
    }
}