using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VStudioIntegration
{
    public class Class1 
    {
        public void Method()
        {
            System.Type dteType = Type.GetTypeFromProgID("VisualStudio.DTE.15.0", true);
            EnvDTE.DTE dte = (EnvDTE.DTE)System.Activator.CreateInstance(dteType);
            Console.WriteLine(dte.ActiveWindow.Height + " x " + dte.ActiveWindow.Width);

            //var tfsExt = (TeamFoundationServerExt)dte.GetObject("Microsoft.VisualStudio.TeamFoundation.TeamFoundationServerExt");
            //var tfs = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(tfsExt.ActiveProjectContext.DomainUri));

            Console.WriteLine(dte.SourceControl.DTE.MainWindow.Caption);
            Console.WriteLine(dte.ActiveWindow.Caption);


            //EnvDTE.Window window = dte.ActiveWindow.LinkedWindows.Item(0);
            //Console.WriteLine(window.Width);

            //ErrorList myErrorList = CType(window.Object, EnvDTE80.ErrorList);



            //var teamExplorer = serviceProvider.GetService(typeof(ITeamExplorer)) as ITeamExplorer;

            //var teamExplorer = (ITeamExplorer) dte.SourceControl.GetService(typeof(ITeamExplorer));
            //var pendingChangesPage = (TeamExplorerPageBase)teamExplorer.NavigateToPage(new Guid(TeamExplorerPageIds.PendingChanges), null);

            //var workItemStore = tfs.GetService<WorkItemStore>();
            //var workItem = workItemStore.GetWorkItem(24065); // workItem is not null!

            //var model = (IPendingCheckin)pendingChangesPage.Model;
            //model.PendingChanges.Comment = "Hello, World!"; // Comment saved
            //model.WorkItems.CheckedWorkItems = new[]
            //{
            //    new WorkItemCheckinInfo(workItem, WorkItemCheckinAction.Associate),
            //}; // CheckedWorkItems not saved =(

        }
    }
}
