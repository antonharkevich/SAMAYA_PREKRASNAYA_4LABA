using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using ConfigurationProvider;
using Options;
using Logger;
using Database;


namespace FileWatcher
{
    public class ManagerFiles
    {
        private readonly ImportantOptions options;
        private bool isEnabled = true;

        public ManagerOfLogging logger;


        public ManagerFiles()
        {
            var optionsManager = new ManagerOptions(AppDomain.CurrentDomain.BaseDirectory);
            options = optionsManager.GetConfigurations<ImportantOptions>();
            DatabaseProvider dataManager = new DatabaseProvider(options);

            logger = new ManagerOfLogging(dataManager.databaseManager);
            logger.Setup();
            dataManager.CreateXMLFiles();
        }


        public void EncryptFile(string inputFile, string outputFile, string Key)
        {
            var encod = new UnicodeEncoding();
            var key = encod.GetBytes(Key);
            var File = System.IO.File.ReadAllBytes(inputFile);

            try
            {
                using (var fileStream = new FileStream(outputFile, FileMode.Create))
                {
                    var rijndaelManaged = new RijndaelManaged();

                    using (var cryptoStream = new CryptoStream(fileStream,
                        rijndaelManaged.CreateEncryptor(key, key), CryptoStreamMode.Write))
                    {
                        foreach (var symbol in File)
                        {
                            cryptoStream.WriteByte(symbol);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (logger.setuped == true)
                {
                    logger.LoggerMessage("Encryption error: ");
                    logger.LoggerDatabaseMessage("Encryption error: ");
                }
            }
        }

        public void DecryptFile(string inputFile, string outputFile, string Key)
        {
            try
            {
                var encod = new UnicodeEncoding();
                var key = encod.GetBytes(Key);
                var File = new List<byte>();

                using (var fileStream = new FileStream(inputFile, FileMode.Open))
                {
                    var rijndaelManaged = new RijndaelManaged();

                    using (var cryptoStream = new CryptoStream(fileStream,
                        rijndaelManaged.CreateDecryptor(key, key), CryptoStreamMode.Read))
                    {
                        int data;
                        while ((data = cryptoStream.ReadByte()) != -1)
                        {
                            File.Add((byte)data);
                        }
                    }
                }
                using (var fileStreamOut = new FileStream(outputFile, FileMode.Create))
                {
                    foreach (var symbol in File)
                    {
                        fileStreamOut.WriteByte(symbol);
                    }
                }
            }
            catch (Exception ex)
            {
                if (logger.setuped == true)
                {
                    logger.LoggerMessage("Decryption error: ");
                    logger.LoggerDatabaseMessage("Decryption error: ");
                }
            }
        }

        public void Compress(string sourceFile, string compressedFile)
        {
            try
            {
                using (var sourceStream = new FileStream(sourceFile, FileMode.OpenOrCreate))
                {
                    using (var targetStream = File.Create(compressedFile))
                    {
                        using (var compressionStream = new GZipStream(targetStream, CompressionMode.Compress))
                        {
                            sourceStream.CopyTo(compressionStream);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (logger.setuped == true)
                {
                    logger.LoggerMessage("Compression error: ");
                    logger.LoggerDatabaseMessage("Compression error: ");
                }
            }
        }

        public void Decompress(string compressedFile, string targetFile)
        {
            try
            {
                using (var sourceStream = new FileStream(compressedFile, FileMode.OpenOrCreate))
                {
                    using (var targetStream = File.Create(targetFile))
                    {
                        using (var decompressionStream = new GZipStream(sourceStream, CompressionMode.Decompress))
                        {
                            decompressionStream.CopyTo(targetStream);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                if (logger.setuped == true)
                {
                    logger.LoggerMessage("Decompression error: ");
                    logger.LoggerDatabaseMessage("Decompression error: ");
                }
            }
        }

        public void Archivation(string filePath, string archive)
        {
            try
            {
                using (ZipArchive zipArchive = ZipFile.Open(archive, ZipArchiveMode.Update))
                {
                    zipArchive.CreateEntryFromFile(filePath, Path.GetFileName(filePath));
                }
            }
            catch (Exception ex)
            {

                if (logger.setuped == true)
                {
                    logger.LoggerMessage("Arhivation error: ");
                    logger.LoggerDatabaseMessage("Arhivation error: ");
                }
            }
        }

        public void Start()
        {
            if (options != null)
            {
                WorkWithFile();
            }

            while (isEnabled)
            {
                Thread.Sleep(1000);
            }
        }

        public void Stop()
        {
            isEnabled = false;
        }

        private void WorkWithFile()
        {
            var control = new object();

            lock (control)
            {
                try
                {

                    var date = DateTime.Now;
                    var subPath = $"{date.ToString("yyyy", DateTimeFormatInfo.InvariantInfo)}\\" +
                                  $"{date.ToString("MM", DateTimeFormatInfo.InvariantInfo)}\\" +
                                  $"{date.ToString("dd", DateTimeFormatInfo.InvariantInfo)}";
                    var path = options.DirectoryOptions.SourceDirectory;
                    var dirInfo = new DirectoryInfo(path);
                    var fileEntries = from file in Directory.GetFiles(path)
                                      where
                                            Path.GetExtension(file) == ".txt"
                                      select file;

                    var filepath = fileEntries.Count() != 0 ? fileEntries.First() : null;
                    var fileName = Path.GetFileName(filepath);
                    var newPath = path +
                       $"\\{date.ToString("yyyy", DateTimeFormatInfo.InvariantInfo)}\\" +
                       $"{date.ToString("MM", DateTimeFormatInfo.InvariantInfo)}\\" +
                       $"{date.ToString("dd", DateTimeFormatInfo.InvariantInfo)}\\" +
                       $"{Path.GetFileNameWithoutExtension(fileName)}_" +
                       $"{date.ToString(@"yyyy_MM_dd_HH_mm_ss", DateTimeFormatInfo.InvariantInfo)}" +
                       $"{Path.GetExtension(fileName)}";
                    var compressedPath = Path.ChangeExtension(newPath, "gz");
                    var newCompressedPath = Path.Combine(options.DirectoryOptions.TargetDirectory,
                        Path.GetFileName(compressedPath));
                    var decompressedPath = Path.ChangeExtension(newCompressedPath, "txt");

                    if (!dirInfo.Exists)
                    {
                        dirInfo.Create();
                    }

                    dirInfo.CreateSubdirectory(subPath);
                    File.Move(filepath, newPath);
                    EncryptFile(newPath, newPath,
                        options.CryptOptions.Key);
                    Compress(newPath, compressedPath);
                    File.Move(compressedPath, newCompressedPath);
                    Decompress(newCompressedPath, decompressedPath);
                    DecryptFile(decompressedPath, decompressedPath,
                        options.CryptOptions.Key);
                    Archivation(decompressedPath,
                        options.ArchivationOptions.ZipName);
                    File.Delete(newPath);
                    File.Delete(newCompressedPath);
                    File.Delete(decompressedPath);
                    if (logger.setuped == true)
                    {
                        logger.LoggerMessage("Success in work with file: ");
                        logger.LoggerDatabaseMessage("Success in work with file:  ");

                    }
                }
                catch
                {
                    if (logger.setuped == true)
                    {
                        logger.LoggerMessage("Error in work with file: ");
                        logger.LoggerDatabaseMessage("Error in work with file:  ");

                    }
                }
            }
        }




    }
}
