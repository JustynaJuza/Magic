using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Magic.Helpers
{
    public interface IFileHandler
    {
        Task<string> SaveFile(Stream fileStream, string fileName, string uploadPath = "");
        bool CheckForImageFileTypeConstraint(string fileContentType, bool allowImageOnly);
        string GetFileIconAsString(string filePath);
        int GetImageWidth(string filePath);
    }

    public class FileHandler : IFileHandler
    {
        private readonly IPathProvider _pathProvider;

        public FileHandler(IPathProvider pathProvider)
        {
            _pathProvider = pathProvider;
        }

        public string GetFileSavePath (string fileName, string uploadPath = "")
        {
            var path = "/Content" + uploadPath + "/";
            var serverPath = _pathProvider.GetServerPath(path);

            if (!PrepareFileDirectory(serverPath))
            {
                // TODO: Decide what should happen here - thowing new exception or passing exception up or else.
                return string.Empty;
            }

            return path + fileName;
        }

        public async Task<bool> SaveFile(string path, Stream fileStream)
        {
            PrepareFileDirectory(path);
            return await SaveFileInBlocksAsync(path, fileStream);
        }

        public bool CheckForImageFileTypeConstraint(string fileContentType, bool allowImageOnly)
        {
            return !allowImageOnly || Regex.IsMatch(fileContentType, "image");
        }

        public string GetFileIconAsString(string filePath)
        {
            var serverPath = _pathProvider.GetServerPath(filePath);
            var icon = Icon.ExtractAssociatedIcon(serverPath);
            if (icon == null)
            {
                ErrorHandler.Log(new Exception("A failed attempt was made to extract an icon from '" + serverPath + "'."));
                return UploaderHelpers.ErrorImage;
            }

            var image = icon.ToBitmap();
            using (var stream = new MemoryStream())
            {
                image.Save(stream, ImageFormat.Png);
                return Convert.ToBase64String(stream.ToArray());
            }
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
                ErrorHandler.Log(ex);
                return 0;
            }
        }

        private bool PrepareFileDirectory(string serverPath)
        {
            if (Directory.Exists(serverPath))
            {
                return true;
            }

            try
            {
                Directory.CreateDirectory(serverPath);
                return true;
            }
            catch (Exception ex)
            {
                ErrorHandler.Log(ex);
                return false;
            }
        }

        private async Task<bool> SaveFileInBlocksAsync(string serverPath, Stream fileStream, int blockByteSize = 1048576) // 1048576B = 1MB
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

                    await Task.WhenAll(fileSavingOperations);
                    return true;
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.Log(ex);
                return false;
            }
        }
    }
}