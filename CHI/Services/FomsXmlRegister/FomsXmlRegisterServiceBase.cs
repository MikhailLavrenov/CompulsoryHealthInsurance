using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Serialization;

namespace CHI.Services
{
    public abstract class FomsXmlRegisterServiceBase
    {
        protected static readonly StringComparison comparer = StringComparison.OrdinalIgnoreCase;
        protected List<string> paths;
        protected List<PERS_LIST> persLists;
        protected List<ZL_LIST> zlLists;


        /// <summary>
        ///
        /// </summary>
        /// <param name="paths">Коллекиця путей к xml файлам: папки, xml, zip, многократно упакованные zip.</param>
        public FomsXmlRegisterServiceBase(IEnumerable<string> paths)
        {
            this.paths = paths.ToList();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="path">Путь к xml файлам: папки, xml, zip, многократно упакованные zip.</param>
        public FomsXmlRegisterServiceBase(string path)
        {
            paths = new List<string>() { path };
        }


        protected void LoadXmlFiles()
        {
            persLists = new List<PERS_LIST>();
            zlLists = new List<ZL_LIST>();

            var allFiles = paths.SelectMany(x => Directory.GetFiles(x, "*.*", SearchOption.AllDirectories)).ToList();

            foreach (var xmlFilePath in allFiles.Where(x => x.EndsWith(".xml", comparer)))
            {
                using var file = new FileStream(xmlFilePath, FileMode.Open);
                var fileName = Path.GetFileName(xmlFilePath);
                DeserializeXmlFile(fileName, file);
            }

            foreach (var zipFilePath in allFiles.Where(x => x.EndsWith(".zip", comparer)))
            {
                using var zipFile = new FileStream(zipFilePath, FileMode.Open);
                LoadXmlFilesFromArchiveRecursive(zipFile);
            }
        }

        void LoadXmlFilesFromArchiveRecursive(Stream zipFile)
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
                    DeserializeXmlFile(archiveEntry.Name, file);
                }
                else if (extension.Equals(".zip", comparer))
                {
                    using var file = archiveEntry.Open();
                    LoadXmlFilesFromArchiveRecursive(file);
                }
            }
        }

        void DeserializeXmlFile(string fileName, Stream file)
        {
            if (fileName.StartsWith("L", comparer))
            {
                var persList = DeserializeXmlFile<PERS_LIST>(file);
                persLists.Add(persList);
            }
            else
            {
                var zlList=DeserializeXmlFile<ZL_LIST>(file);
                zlLists.Add(zlList);
            }
        }

        T DeserializeXmlFile<T>(Stream file) where T : class
        {
            file.Seek(0, SeekOrigin.Begin);

            var formatter = new XmlSerializer(typeof(T));
            var obj = formatter.Deserialize(file);

            return (T)obj;
        }
    }
}
