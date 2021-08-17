using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Serialization;

namespace CHI.Services
{
    /// <summary>
    /// Загружает реестры-счетов из xml файлов.
    /// </summary>
    public class XmlBillsLoader
    {
        static readonly StringComparison comparer = StringComparison.OrdinalIgnoreCase;
        public List<PERS_LIST> PersonsBills { get; private set; }
        public List<ZL_LIST> CasesBills { get; private set; }
        public List<string> XmlFileNameStartsWithFilter { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="paths">Пути к xml файлам реестров-счетов. (может быть папками, xml файлами и/или zip архивами)</param>
        public void Load(IEnumerable<string> paths)
        {
            PersonsBills = new List<PERS_LIST>();
            CasesBills = new List<ZL_LIST>();

            var allFiles = paths.Where(x => File.GetAttributes(x).HasFlag(FileAttributes.Directory))
                .SelectMany(x => Directory.GetFiles(x, "*.*", SearchOption.AllDirectories))
                .Union(paths.Where(x => !File.GetAttributes(x).HasFlag(FileAttributes.Directory)))
                .ToList();

            foreach (var xmlFilePath in allFiles.Where(x => x.EndsWith(".xml", comparer) && CheckFileNameWithFilter(x)))
            {
                using var file = new FileStream(xmlFilePath, FileMode.Open);
                var fileName = Path.GetFileName(xmlFilePath);
                AddToBillsLists(fileName, file);
            }

            foreach (var zipFilePath in allFiles.Where(x => x.EndsWith(".zip", comparer)))
            {
                using var zipFile = new FileStream(zipFilePath, FileMode.Open);
                LoadFromArchiveRecursive(zipFile);
            }
        }

        void LoadFromArchiveRecursive(Stream zipFile)
        {
            using var archive = new ZipArchive(zipFile, ZipArchiveMode.Read);
            foreach (var archiveEntry in archive.Entries)
            {
                //Архиватор представляет папку и вложенные в нее файлы отдельными ZipArchiveEntry в плоском стиле, поэтому сами папки пропускаем.
                //Свойство Name - это имя файла, у папок его нет.
                if (string.IsNullOrEmpty(archiveEntry.Name))
                    continue;

                var extension = Path.GetExtension(archiveEntry.Name);

                if (extension.Equals(".xml", comparer) && CheckFileNameWithFilter(archiveEntry.Name))
                {
                    using var file = archiveEntry.Open();
                    AddToBillsLists(archiveEntry.Name, file);
                }
                else if (extension.Equals(".zip", comparer))
                {
                    using var file = archiveEntry.Open();

                    LoadFromArchiveRecursive(file);
                }
            }
        }

        void AddToBillsLists(string fileName, Stream file)
        {
            if (fileName.StartsWith("L", comparer))
            {
                var persList = Deserialize<PERS_LIST>(file);
                PersonsBills.Add(persList);
            }
            else
            {
                var zlList = Deserialize<ZL_LIST>(file);
                CasesBills.Add(zlList);
            }
        }

        T Deserialize<T>(Stream file) where T : class
        {
            if (file.CanSeek)
                file.Seek(0, SeekOrigin.Begin);

            var formatter = new XmlSerializer(typeof(T));
            var obj = formatter.Deserialize(file);

            return (T)obj;
        }

        bool CheckFileNameWithFilter(string fileNameOrPath)
        {
            if ((XmlFileNameStartsWithFilter?.Count ?? 0) == 0)
                return true;

            var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileNameOrPath);

            return XmlFileNameStartsWithFilter.Any(x => nameWithoutExtension.StartsWith(x, comparer));
        }
    }
}