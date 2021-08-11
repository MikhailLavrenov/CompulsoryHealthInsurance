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
        protected List<Stream> GetXmlFiles(Regex fileNameMatchPattern)
        {
            var allFiles = filePaths.SelectMany(x => Directory.GetFiles(x, "*.*", SearchOption.AllDirectories)).ToList();

            var xmlFiles = allFiles.Where(x=>x.EndsWith(".xml", comparer))
                .Where(x => fileNameMatchPattern.IsMatch(Path.GetFileName(x)))
                .Select(x => new FileStream(x, FileMode.Open))
                .ToList<Stream>();

            foreach (var zipFilePath in allFiles.Where(x => x.EndsWith(".zip", comparer)))
            {
                using var zipFile = new FileStream(zipFilePath, FileMode.Open);
                xmlFiles.AddRange(GetXmlFilesFromArchiveRecursive(zipFile, fileNameMatchPattern));
            }

            return xmlFiles;
        }

        protected List<Stream> GetXmlFilesFromArchiveRecursive(Stream zipFile, Regex fileNameMatchPattern)
        {
            var xmlFiles = new List<Stream>();

            using var archive = new ZipArchive(zipFile, ZipArchiveMode.Read);
            foreach (var archiveEntry in archive.Entries)
            {
                //Архиватор представляет папку и вложенные в нее файлы отдельными ZipArchiveEntry в плоском стиле, поэтому сами папки пропускаем.
                //Свойство Name - это имя файла, у папок его нет.
                if (string.IsNullOrEmpty(archiveEntry.Name))
                    continue;

                var extension = Path.GetExtension(archiveEntry.Name);

                if (extension.Equals(".xml", comparer) && fileNameMatchPattern.IsMatch(archiveEntry.Name))
                {
                    var xmlFile = new MemoryStream();
                    archiveEntry.Open().CopyTo(xmlFile);
                    xmlFiles.Add(xmlFile);
                }
                else if (extension.Equals(".zip", comparer))
                {
                    var nestedZipFile = archiveEntry.Open();                    
                    xmlFiles.AddRange(GetXmlFilesFromArchiveRecursive(nestedZipFile, fileNameMatchPattern));
                }
            }

            return xmlFiles;
        }

        protected List<T> DeserializeXmlFiles<T>(IEnumerable<Stream> files) where T : class
        {
            var result = new List<T>();

            foreach (var file in files)
                result.Add(DeserializeXmlFile<T>(file));

            return result;
        }

        protected T DeserializeXmlFile<T>(Stream file) where T : class
        {
            file.Seek(0, SeekOrigin.Begin);

            var formatter = new XmlSerializer(typeof(T));
            var obj = formatter.Deserialize(file);

            return (T)obj;
        }
    }
}
