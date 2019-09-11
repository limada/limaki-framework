using System;
using System.IO;

namespace Limaki.UnitsOfWork.Usecases {

    public class FileVisitor
    {

        public Action<string> ProcessDirectory { get; set; }
        public Action<string> ProcessFile { get; set; }
        public Action<bool> Progress { get; set; }

        protected void OnProcessDirectory(string targetDirectory, bool recursive = false)
        {

            var fileEntries = Directory.GetFiles(targetDirectory);
            ProcessDirectory?.Invoke(targetDirectory);

            foreach (string fileName in fileEntries)
                OnProcessFile(fileName);

            if (recursive)
            {
                var subdirectoryEntries = Directory.GetDirectories(targetDirectory);
                foreach (string subdirectory in subdirectoryEntries)
                    OnProcessDirectory(subdirectory);
            }
        }


        protected void OnProgress(bool done = false)
        {
            Progress?.Invoke(done);
        }

        protected void OnProcessFile(string path)
        {
            ProcessFile?.Invoke(path);
        }

        public void Run(string targetDirectory, bool recursive)
        {
            OnProcessDirectory(targetDirectory, recursive);
            OnProgress(true);
        }
    }
}

