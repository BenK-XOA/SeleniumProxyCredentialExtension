# SeleniumProxyCredentialExtension
A C# extension for Selenium that makes it easy to work with proxies that require username:password authentication.
Creates a temporary extension for the webbrowser which can be loaded with selenium. The extension automatically adds the proxy authorization header to any outgoing messages.

# Usage

1. Import ProxyConfigurationextension.cs into your project.
2. Create extension and add to selenium firefox profile:
```csharp
public ProxyConfigurationExtension tempProxyLoginExtension { get; private set; }

 private void AddProxyToOptions(ref FirefoxOptions options, string ip, string port, string username, string password)
        {
              var Proxy = new Proxy();
                          Proxy.Kind = ProxyKind.Manual;
                          Proxy.HttpProxy =  ip + ":" + port;
                          Proxy.SslProxy  =  ip + ":" + port;
                          
               options.Proxy = Proxy;
               options.SetPreference("xpinstall.signatures.required", false);
               
               tempProxyLoginExtension = new ProxyConfigurationExtension(username, password);

              options.Profile.AddExtension(tempProxyLoginExtension.disposableExtensionPath);
              
          }
                          
```

As you can see, you create the proxy object as normal, pass in your ip and port. You then create a new instance of the ProxyConfigurationExtension class and pass in username and password. Make sure not to lose this reference, you'll need it later after quitting selenium and firefox to dispose of the temporary extension file that gets created by the class.
Then load in the disposableExtensionPath as a new extension into your FirefoxOptions.

3. dispose when you're finished:

```csharp
driver.Quit();

//wait til firefox really shutdown, then dispose of temp extension
System.Threading.Thread.Sleep(4000);

if (tempProxyLoginExtension != null)
    tempProxyLoginExtension.Dispose();
```       
