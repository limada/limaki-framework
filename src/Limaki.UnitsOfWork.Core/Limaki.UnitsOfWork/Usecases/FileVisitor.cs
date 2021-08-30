/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2018 - 2019 Lytico
 *
 * http://www.limada.org
 * 
 */

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

