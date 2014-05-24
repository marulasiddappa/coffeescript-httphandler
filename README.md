coffeescript-httphandler
========================

Compiles *.coffee* files on the fly in *.js*. 

The HttpHandler uses the Windows Script Host [cscript.exe](http://technet.microsoft.com/en-us/library/bb490887.aspx) + [coffee-script.js](http://coffeescript.org).

To setup the handler you need to add the binary as reference to your Web application and then register as httpHandler in your web.config file:

```
...
<system.webServer>
  <handlers>
    <add name="put a name" path="*.coffee" verb="*" type="CoffeeScript.CoffeeScriptHttpHandler, CoffeeScript" resourceType="File" />
  </handlers>
</system.webServer>
...
```

The HttpHandler may be configured via config file (appConfig) and it has the following parameters:
 * *coffee-script.path* - absolute path to the coffee-script.js compiler, default one is 1.7.1
 * *coffee-script.cscriptPath* - absolute path to windows cscript.exe, default: c:\windows\system32\cscript.exe
 * *coffee-script.injectVersion* - flag to inject in header of compiled js file, the coffee-script.js version used; default true
 * *coffee-script.enableCache* - flag to make cacheable with dependency of original .coffee file, default true
