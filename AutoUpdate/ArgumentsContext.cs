using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdate
{
    public class ArgumentsContext
    {
        public List<string> Values { get; private set; } = new();

        public ArgumentsContext(Func<List<string>> extraArguments = null)
        {
            //starts the (hopefully correcly updated) process using the original executable name startup arguments.
            var arguments = Environment.GetCommandLineArgs();

            //1st argument is always the executable path (see AppCore from MSDN  reference).
            for (int i = 1; i < arguments.Length; i++)
            {
                Values.Add(arguments[i]);
            }

            var extraArgs = extraArguments?.Invoke();
            if (extraArgs != null)
            {
                //keep it clean.
                foreach (var extraArg in extraArgs)
                {
                    if (!Values.Contains(extraArg)) Values.Add(extraArg);
                }
            }
        }

        public void SetAsCollection(Collection<string> list)
        {
            foreach (var arg in Values)
            {
                list.Add(arg);
            }
        }

    }
}
