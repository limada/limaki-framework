/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2017 Lytico
 *
 * http://www.limada.org
 * 
 */

using System.Diagnostics;
using System.IO;
using Limaki.Common;
using Limaki.UnitsOfWork.Reporting;

namespace Limaki.UnitsOfWork.Usecases {

    public class ReportHelper {

        IReportSettingsProvider _reportSettings = null;
        public virtual IReportSettingsProvider ReportSettings {
            get => _reportSettings ??= Registry.Pooled<IReportSettingsProvider> ();
            set => _reportSettings = value;
        }

        public virtual string OutputDir => ReportSettings.OutputDir;

        public void ShowFile (string file) {
            if (File.Exists (file)) {
                using (var _ = Process.Start( new ProcessStartInfo {FileName = file, UseShellExecute = true})) { }
            }
        }
    }
}
