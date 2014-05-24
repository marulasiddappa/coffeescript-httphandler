using System;
using System.Web;
using System.Web.Caching;
using CoffeeScript;
using System.Configuration;

namespace CoffeeScript
{
    /// <summary>
    /// Configs:
    ///  coffee-script.path - absolute path to the coffee-script.js compiler, default one is 1.7.1
    ///  coffee-script.cscriptPath - absolute path to windows cscript.exe, default: c:\windows\system32\cscript.exe
    ///  coffee-script.injectVersion - flag to inject in header of compiled js file, the coffee-script.js version used; default true
    ///  coffee-script.enableCache - flag to make cacheable with dependency of original .coffee file, default true
    /// </summary>
    public class CoffeeScriptHttpHandler : IHttpHandler
    {
        private bool _enableCache;
        CScriptEngine _engine;
        public CoffeeScriptHttpHandler()
        {
            var _scriptPath = GetConfig("coffee-script.path", null);
            _engine = new CScriptEngine(_scriptPath != null 
                ? Loader.FromFile(_scriptPath) : Loader.Default());
            
            var exepath = GetConfig("coffee-script.cscriptPath", null);
            if (exepath != null)
                _engine.ExePath = exepath;

            _engine.InjectVersion = GetConfig("coffee-script.injectVersion", true);

            _enableCache = GetConfig("coffee-script.enableCache", true);
        }

        public void ProcessRequest(HttpContext context)
        {
            var jscript = _engine.Compile(context.Request.PhysicalPath);

            context.Response.AddFileDependency(context.Request.PhysicalPath);
            var cache = context.Response.Cache;
            if (_enableCache)
            {
                cache.SetCacheability(HttpCacheability.Public);
                cache.SetValidUntilExpires(true);
                cache.SetLastModifiedFromFileDependencies();
                cache.SetETagFromFileDependencies();
            }
            else
            {
                cache.SetCacheability(HttpCacheability.NoCache);
            }

            context.Response.ContentType = "text/javascript";
            context.Response.Write(jscript);
        }

        public bool IsReusable
        {
            get { return true; }
        }

        private string GetConfig(string keyName, string defaultValue)
        {
            var result = ConfigurationManager.AppSettings[keyName];
            return (result != null) ? result : defaultValue;
        }

        public bool GetConfig(string keyName, bool defaultValue)
        {
            return bool.Parse(GetConfig(keyName, defaultValue.ToString()));
        }
    }
}
