using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MSBuildCustomTasks
{
    public class CustomExec : Task
    {
        #region Protected Mehotds
        protected string tabifyStreamReader(StreamReader sr)
        {
            var result = new StringBuilder();
            while (!sr.EndOfStream)
                result.AppendLine("\t" + sr.ReadLine());

            return result.ToString();

        }

        protected bool runProcess()
        {
            using (var process = this.getProcess())
            {
                try
                {
                    process.Start();
                    if (this.WaitForExitTime <= 0)
                        process.WaitForExit();
                    else
                        process.WaitForExit(this.WaitForExitTime);

                    this.ExitCode = process.ExitCode;

                    var outputText = new StringBuilder();

                    var stdOutputText = this.tabifyStreamReader(process.StandardOutput);
                    outputText.Append(stdOutputText);
                    Log.LogMessage(stdOutputText);

                    var msgErro = this.tabifyStreamReader(process.StandardError);
                    outputText.Append(msgErro);
                    if (msgErro.Length > 0)
                        Log.LogError(msgErro);

                    this.TextResult = outputText.ToString();

                    return (this.IgnoreExitCode || this.ExitCode == 0);

                }
                catch (Exception e)
                {
                    Log.LogErrorFromException(e);
                    return false;
                }
            }
        }

        protected Process getProcess()
        {
            var result = new System.Diagnostics.Process();
            result.StartInfo.FileName = this.Command;

            if (!string.IsNullOrWhiteSpace(this.Parameters))
                result.StartInfo.Arguments = this.Parameters;

            result.StartInfo.UseShellExecute = false;

            result.StartInfo.RedirectStandardOutput = true;
            result.StartInfo.RedirectStandardError = true;
            result.StartInfo.CreateNoWindow = true;

            if (!string.IsNullOrWhiteSpace(this.WorkingDirectory))
                result.StartInfo.WorkingDirectory = this.WorkingDirectory;

            return result;

        }

        #endregion

        #region Properties

        #region Input
        [Required]
        public string Command { get; set; }
        public string Parameters { get; set; }
        public bool IgnoreExitCode { get; set; }
        public string WorkingDirectory { get; set; }
        public int WaitForExitTime { get; set; }
        #endregion

        #region Output

        [Output]
        public string TextResult { get; set; }
        [Output]
        public int ExitCode { get; set; }

        #endregion

        #endregion

        #region Public Methods

        public CustomExec()
        {
            this.IgnoreExitCode = false;
            this.WaitForExitTime = 0;
        }

        public override bool Execute()
        {
            return runProcess();
        }

        #endregion

    }
}
