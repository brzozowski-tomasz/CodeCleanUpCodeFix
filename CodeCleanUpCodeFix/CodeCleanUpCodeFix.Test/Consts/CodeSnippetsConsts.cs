namespace CodeCleanUpCodeFix.Consts
{
    public class CodeSnippetsConsts
    {
        public const string EmptyClass = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class EmptyClass
        {   
        }
    }";

        public const string ClassContainingEmptyMethod = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class EmptyClass
        {   
            private void EmptyMethod()
            {
            }
        }
    }";

        public const string ClassContainingShortMethod = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class EmptyClass
        {   
            private void ShortMethod()
            {
                Console.WriteLine(String.Empty);
            }
        }
    }";

        public const string ClassContainingLongMethod = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class ClassHavingALongMethod
        {   
            private void LongMethod()
            {
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
            }
        }
    }";

        public const string ClassContainingTwoDifferentMethods = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class EmptyClass
        {   
            private void Method1()
            {
                Console.WriteLine(String.Empty);
            }

            private void Method2()
            {
                Console.WriteLine(""Some other string"");
            }
        }
    }";

        public const string ClassContainingTwoExactSameMethods = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class EmptyClass
        {
            private void Method1()
            {
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
            }

            private void Method2()
            {
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
            }
        }
    }";
    }
}
