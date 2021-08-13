using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Serialization;

namespace CHI.Services
{
    public class XmlBillsLoader
    {
        static readonly StringComparison comparer = StringComparison.OrdinalIgnoreCase;
        List<string> paths;
        List<BillPart> loaded;


        /// <summary>
        ///
        /// </summary>
        /// <param name="paths">Коллекиця путей к xml файлам: папки, xml, zip, многократно упакованные zip.</param>
        public XmlBillsLoader(IEnumerable<string> paths)
        {
            this.paths = paths.ToList();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="path">Путь к xml файлам: папки, xml, zip, многократно упакованные zip.</param>
        public XmlBillsLoader(string path)
        {
            paths = new List<string>() { path };
        }


        public List<BillPart> Load()
        {
            loaded = new List<BillPart>();

            var allFiles = paths.SelectMany(x => Directory.GetFiles(x, "*.*", SearchOption.AllDirectories)).ToList();

            foreach (var xmlFilePath in allFiles.Where(x => x.EndsWith(".xml", comparer)))
            {
                using var file = new FileStream(xmlFilePath, FileMode.Open);
                var fileName = Path.GetFileName(xmlFilePath);
                AddToLoaded(fileName, file);
            }

            foreach (var zipFilePath in allFiles.Where(x => x.EndsWith(".zip", comparer)))
            {
                using var zipFile = new FileStream(zipFilePath, FileMode.Open);
                LoadFromArchiveRecursive(zipFile);
            }

            return loaded;
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

                if (extension.Equals(".xml", comparer))
                {
                    using var file = archiveEntry.Open();
                    AddToLoaded(archiveEntry.Name, file);
                }
                else if (extension.Equals(".zip", comparer))
                {
                    using var file = archiveEntry.Open();
                    LoadFromArchiveRecursive(file);
                }
            }
        }

        void AddToLoaded(string fileName, Stream file)
        {
            if (fileName.StartsWith("L", comparer))
            {
                var persList = Deserialize<PERS_LIST>(file);
                loaded.Add(persList);
            }
            else
            {
                var zlList = Deserialize<ZL_LIST>(file);
                loaded.Add(zlList);
            }
        }

        T Deserialize<T>(Stream file) where T : class
        {
            file.Seek(0, SeekOrigin.Begin);

            var formatter = new XmlSerializer(typeof(T));
            var obj = formatter.Deserialize(file);

            return (T)obj;
        }
    }
}