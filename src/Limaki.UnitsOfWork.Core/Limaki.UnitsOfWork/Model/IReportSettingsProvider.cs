/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2016 - 2019 Lytico
 *
 * http://www.limada.org
 * 
 */

namespace Limaki.UnitsOfWork.Reporting {

    public interface IReportSettingsProvider {

        string TemplateDir { get; set; }
        string OutputDir { get; set; }
    }

    /// <summary>
    /// a default reportsettings implementation
    /// </summary>
    public class ReportSettings : IReportSettingsProvider {

        public string TemplateDir { get; set; }
        public string OutputDir { get; set; }
    }
}