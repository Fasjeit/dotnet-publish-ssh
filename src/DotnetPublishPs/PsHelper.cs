//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Xml.Linq;

//using System;
//using System.Collections.Generic;
//using System.Management.Automation;
//using System.Management.Automation.Runspaces;
//using System.Threading.Tasks;

//namespace DotnetPublishPs
//{
//    internal class PsHelper
//    {
//        // see here - https://learn.microsoft.com/en-us/powershell/scripting/developer/hosting/creating-remote-runspaces?view=powershell-7.3

//        internal static void Exec()
//        {
//            // create Powershell runspace
//            using (var runspace = RunspaceFactory.CreateRunspace())
//            {
//                // open it
//                runspace.Open();

//                // create a pipeline and feed it the script text
//                Pipeline pipeline = runspace.CreatePipeline();

//                try
//                {
//                    if (!File.Exists(this.scriptPath))
//                    {
//                        throw new Exception(
//                            string.Format(Properties.Resources.FileNotExists, this.scriptPath));
//                    }

//                    pipeline.Commands.AddScript(File.ReadAllText(this.scriptPath));

//                    // specify the parameters to pass into the script.
//                    // ps.AddParameters(scriptParameters);

//                    // adding pipleline to string output
//                    pipeline.Commands.Add("Out-String");

//                    // execute the script
//                    var results = pipeline.Invoke();
//                }
//                finally
//                {
//                    // close the runspace
//                    runspace.Close();
//                }

//                // convert the script result into a single string
//                StringBuilder stringBuilder = new StringBuilder();
//                foreach (PSObject obj in results)
//                {
//                    stringBuilder.AppendLine(obj.ToString());
//                }

//                var stringResult = stringBuilder.ToString();

//                this.Result.Completed = true;
//            }
//        }
//    }
//}
