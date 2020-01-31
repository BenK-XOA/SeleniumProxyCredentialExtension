using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;

namespace XOAProductions.Automation
{
    /// <summary>
    /// Creates a temporary firefox extension which automatically logs into a proxy by intercepting all traffic and appending a Proxy-Authentication header with credentials
    /// </summary>
    class ProxyConfigurationExtension : IDisposable
    {
        /// <summary>
        /// the script file for firefox extension
        /// </summary>
        const string scriptFile = @"browser.webRequest.onBeforeSendHeaders.addListener(handler, { urls: ['<all_urls>']}, ['requestHeaders', 'blocking']);
                                    function handler(details)
                                    {
                                        details.requestHeaders.push({ name: 'Proxy-Authorization', value: 'Basic [BASE64CREDENTIALS]' });
                                        return { requestHeaders: details.requestHeaders };
                                    }";

        /// <summary>
        /// The manifest file for firefox extension
        /// </summary>
        const string manifest = @"{
                                   'manifest_version': 2,
                                   'name': 'proxyauth',
                                   'version': '1.0',
                                   'description': 'Description',
                                   'applications': {
                                     'gecko': {
                                         'id': 'b.kuenzel@xoaproductions.com'
                                     }
                                    },
                                   'background': {
                                     'scripts': ['background.js']
                                     },
                                   'permissions': [
                                     'webRequest', 'webRequestBlocking', '<all_urls>' 
                                   ]
                                 }";

        /// <summary>
        /// The path to the temp extension created
        /// </summary>
        public string disposableExtensionPath { get; private set; }

        /// <summary>
        /// Creates the extension file. File can then be found at disposableExtensionPath.
        /// </summary>
        /// <param name="username">the username of proxy</param>
        /// <param name="password">the password of proxy</param>
        public ProxyConfigurationExtension(string username, string password)
        {
            //create temp directory for extension files
            var dir = Directory.CreateDirectory(System.Windows.Forms.Application.StartupPath + "\\" + Guid.NewGuid().ToString());
            var destinationBasePath = System.Windows.Forms.Application.StartupPath + "\\" + Guid.NewGuid().ToString();
            var zipFile = destinationBasePath + ".zip";
            var finalXPIFile = destinationBasePath + ".xpi";

            //create credentials and replace them in the script string
            string credentials = username + ":" + password;
            string b64Credentials = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(credentials));
            string scriptFileWithCredentials = scriptFile.Replace("[BASE64CREDENTIALS]", b64Credentials);

            //write script and manifest to temp directory
            File.WriteAllText(dir.FullName + "\\manifest.json", manifest.Replace("'", "\""));
            File.WriteAllText(dir.FullName + "\\background.js", scriptFileWithCredentials);

            //create zip out of directory and rename to xpi
            ZipFile.CreateFromDirectory(dir.FullName, zipFile);
            File.Move(zipFile, finalXPIFile);

            //delete tempdir
            Directory.Delete(dir.FullName, true);

            disposableExtensionPath = finalXPIFile;
        }

        /// <summary>
        /// gets rid of the temporary firefox extension.
        /// ONLY CALL AFTER FIREFOX IS CLOSED!
        /// </summary>
        public void Dispose()
        {
            try
            {
                File.Delete(disposableExtensionPath);
            }
            catch(Exception e)
            {
                Console.WriteLine("Warning! Temporary firefox extension could not be deleted at path: " + disposableExtensionPath + "\nSee error output below for more information.");
                Console.WriteLine(e.Message);
            }
        }
    }
}
