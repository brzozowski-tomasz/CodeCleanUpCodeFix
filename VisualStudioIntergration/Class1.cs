using System;

namespace VisualStudioIntergration
{
    public class Class1
    {
        public void Method()
        {
            System.Type dteType = Type.GetTypeFromProgID("VisualStudio.DTE.14.0", true);
            EnvDTE.DTE dte = (EnvDTE.DTE) System.Activator.CreateInstance(dteType);
        }
    }
}
