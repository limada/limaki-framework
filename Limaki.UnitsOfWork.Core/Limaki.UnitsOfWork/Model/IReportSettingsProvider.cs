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