using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace CoffeeScript
{
    public class CScriptEngine
    {
        public CScriptEngine(string coffeScript)
        {
            ExePath = @"c:\Windows\System32\cscript.exe";
            CoffeeScript = coffeScript;
            InjectVersion = true;
        }

        public string CoffeeScript { get; private set; }
        public string ExePath { get; set; }
        public bool InjectVersion { get; set; }

        private string _computedVersion;
        public string Version
        {
            get
            {
                if (_computedVersion == null)
                {
                    var coffeeVersion = new Regex("VERSION=\"(?<vv>.+?)\"").Match(CoffeeScript).Groups["vv"].Value;

                    _computedVersion = string.Format("{{NET:'{0}', CoffeScript:'{1}', cscript:'{2}'}}",
                        Environment.Version + "-" + (IntPtr.Size == 4 ? "x86" : "x64"),
                        coffeeVersion,
                        ExePath != null ? FileVersionInfo.GetVersionInfo(ExePath).FileVersion : "N/A");
                }
                return _computedVersion;
            }
        }

        public string Compile(string scriptPath)
        {
            var writer = new StringWriter();
            writer.Write(CoffeeScript);
            writer.WriteLine();
            writer.WriteLine();
            writer.Write(@"

function loadFile(scriptPath) {
    // use adodb.stream to handle utf-8 files. 
    // see: http://stackoverflow.com/questions/13851473/read-utf-8-text-file-in-vbscript
    //var fso = new ActiveXObject('Scripting.FileSystemObject');
    //return fso.OpenTextFile(script_path, 1).ReadAll();

    var stream = new ActiveXObject('ADODB.Stream');
    stream.Charset = 'utf-8'
    stream.Open
    stream.LoadFromFile(scriptPath);
    return stream.ReadText();
}

function compileFile(scriptPath) {
    try{
        return CoffeeScript.compile(loadFile(scriptPath), {no_wrap: true});
    }catch(err){
        return err;
    }
    return result;
}

if (<inject-version>){
    WScript.Echo(
        '/**'
    + '\n * CoffeeScript Compiler: ' + CoffeeScript.VERSION 
    + '\n * Compiled at: ' + new Date() 
    + '\n */');
}

WScript.Echo(compileFile('<script-path>'));
".Replace("<script-path>", scriptPath.Replace("\\", "/"))
 .Replace("<inject-version>", InjectVersion.ToString().ToLowerInvariant())
 );
            writer.WriteLine();

            return RunEngine(writer.ToString());
        }

        private string RunEngine(string code)
        {
            using (var tmpFile = new TmpJsFile())
            {
                File.AppendAllText(tmpFile.FilePath, code);

                var cmd = string.Format("\"{0}\" //NoLogo", tmpFile.FilePath);
                var info = new ProcessStartInfo(ExePath, cmd);
                info.UseShellExecute = false;
                info.RedirectStandardOutput = true;
                using (var process = Process.Start(info))
                {
                    var stdOut = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    return stdOut;
                }
            }
        }




        class TmpJsFile : IDisposable
        {
            public TmpJsFile()
            {
                var tmpDir = Path.Combine(Path.GetTempPath(), "coffee-script");
                Directory.CreateDirectory(tmpDir);

                FilePath = Path.Combine(tmpDir, Path.GetRandomFileName() + ".js");
            }
            public string FilePath { get; private set; }

            public void Dispose()
            {
                File.Delete(FilePath);
            }
        }

       
    }
}
