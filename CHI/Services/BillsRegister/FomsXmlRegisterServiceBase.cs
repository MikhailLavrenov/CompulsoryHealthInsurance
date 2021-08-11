using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace CHI.Services
{
    public abstract class FomsXmlRegisterServiceBase
    {
        protected static readonly StringComparison comparer = StringComparison.OrdinalIgnoreCase;
        protected List<string> filePaths;


        /// <summary>
        ///
        /// </summary>
        /// <param name="paths">Коллекиця путей к xml файлам: папки, xml, zip, многократно упакованные zip.</param>
        public FomsXmlRegisterServiceBase(IEnumerable<string> paths)
        {
            this.filePaths = paths.ToList();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="filePath">Путь к xml файлам: папки, xml, zip, многократно упакованные zip.</param>
        public FomsXmlRegisterServiceBase(string filePath)
        {
            filePaths = new List<string>() { filePath };
        }


        /// <summary>
        /// Получает список потоков  на файлы из указанных расположений  файла/файлов, начинающихся с заданных имен.
        /// </summary>
        /// <param name="fileNamesStartsWith">Коллекция начала имен файлов.</param>
        /// <returns>Список потоков файлов.</returns>
        protected List<Stream> GetFiles(Regex fileNameMatchPattern)
        {
            var xmlFiles = filePaths.SelectMany(x => Directory.GetFiles(x, "*.xml", SearchOption.AllDirectories))
                .Where(x => fileNameMatchPattern.IsMatch(Path.GetFileName(x)))
                .Select(x => new FileStream(x, FileMode.Open))
                .ToList<Stream>();

            var archivePaths = filePaths.SelectMany(x => Directory.GetFiles(x, "*.zip", SearchOption.AllDirectories));
            foreach (var archivePath in archivePaths)
            {
                using (var archive = ZipFile.OpenRead(archivePath))
                {
                    var xmlFilesFromArchvie = archive.Entries.SelectMany(x => ArchiveEntryGetXmlFilesRecursive(x, fileNameMatchPattern));
                    xmlFiles.AddRange(xmlFilesFromArchvie);
                }
            }

            return xmlFiles;
        }

        /// <summary>
        /// Получает список потоков на файлы в архиве, имена которых начинаются с опеределенных строк.
        /// </summary>
        /// <param name="archiveEntry">Файл внутри zip архива.</param>
        /// <param name="fileNamesStartsWith">Коллекция начала имен файлов.</param>
        /// <returns>Список потоков файлов.</returns>
        protected List<Stream> ArchiveEntryGetXmlFilesRecursive(ZipArchiveEntry archiveEntry, Regex fileNameMatchPattern)
        {
            var xmlFiles = new List<Stream>();

            if (string.IsNullOrEmpty(archiveEntry.Name))
                return xmlFiles;

            var extension = Path.GetExtension(archiveEntry.Name);

            if (extension.Equals(".xml", comparer) && fileNameMatchPattern.IsMatch(archiveEntry.Name))
            {
                var extractedEntry = new MemoryStream();
                archiveEntry.Open().CopyTo(extractedEntry);
                xmlFiles.Add(extractedEntry);
            }
            else if (extension.Equals(".zip", comparer))
            {
                var extractedEntry = new MemoryStream();
                archiveEntry.Open().CopyTo(extractedEntry);

                using (var archive = new ZipArchive(extractedEntry))
                {
                    foreach (var entry in archive.Entries)
                        xmlFiles.AddRange(ArchiveEntryGetXmlFilesRecursive(entry, fileNameMatchPattern));
                }
            }

            return xmlFiles;
        }

        protected List<T> DeserializeXmlCollection<T>(IEnumerable<Stream> files) where T : class
        {
            var result = new List<T>();

            foreach (var file in files)
                result.Add(DeserializeXml<T>(file));

            return result;
        }

        protected T DeserializeXml<T>(Stream file) where T : class
        {
            file.Seek(0, SeekOrigin.Begin);

            var formatter = new XmlSerializer(typeof(T));
            var obj = formatter.Deserialize(file);

            return (T)obj;
        }
    }
}
