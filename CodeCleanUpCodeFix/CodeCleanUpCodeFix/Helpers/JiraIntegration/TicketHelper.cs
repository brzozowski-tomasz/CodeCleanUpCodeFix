using System;
using System.Collections.Generic;
using CodeCleanUpCodeFix.Helpers.JiraIntegration.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;

namespace CodeCleanUpCodeFix.Helpers.JiraIntegration
{
    public class TicketHelper : ITicketHelper
    {
        public string GetDuplicateTicketSummary(Document document)
        {
            var fileNamePath = document.FilePath.Replace(GlobalSettingsConsts.RepositoryDiskLocation, String.Empty);
            fileNamePath = fileNamePath.Replace("\\", ".");
            return string.Format("{0} :: {1}", JiraConsts.DuplicateCodeTicketType, fileNamePath);
        }

        public string GetDuplicateTicketDescription(Document document, List<Location> locations, SourceText sourceText)
        {
            var descriptionLocations = new TicketDescriptionLocations
            {
                Locations = new List<TicketDescriptionLocation>()
            };

            foreach (var location in locations)
            {
                descriptionLocations.Locations.Add(GetSingleLocationDescription(document, location));
            }

            var description = JsonConvert.SerializeObject(descriptionLocations);

            description += "\\\\\\\\ \\\\\\\\ Code {code} " + sourceText + " {code}";

            description = description.Replace(Environment.NewLine, "\\r\\n");
            description = description.Replace("\"", "\\\"");

            return description;
        }

        private TicketDescriptionLocation GetSingleLocationDescription(Document document, Location location)
        {
            var locationLineSpan = location.GetLineSpan();
            return new TicketDescriptionLocation
            {
                BeginLine = locationLineSpan.StartLinePosition.Line,
                EndLine = locationLineSpan.EndLinePosition.Line,
                CountLineCode = locationLineSpan.EndLinePosition.Line - locationLineSpan.StartLinePosition.Line,
                Pkg = document.Project.Name,
                RelFile = document.Name
            };
        }
    }
}
