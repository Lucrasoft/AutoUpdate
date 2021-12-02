[![nuget badge](https://img.shields.io/nuget/v/Lucrasoft.AutoUpdate.svg)](https://www.nuget.org/packages/Lucrasoft.AutoUpdate/)

# AutoUpdate libary
A simple but extensibe, general applicable, autoupdate framework.  
Like `ClickOnce`/`Squirrel` but easier. (and .NET5 support)  

Supports multiple version detection schemes, multiple package locations.  
Supports `in-place` and `side-by-side` installation for updating.
Supports uploading on multiple ways.

## General flow 
The returned content (file or http)	multiple content processors:
- `Json` based version
- `XML` based version
- `TEXT` based version
### <b>Get the local version</b>
Built in localversion detection based on AssemblyVersion of the entry assembly.  
A custom localversion extension point is provided by the builder.  
As well as custom file version reader extension point is provided by the builder.  

Example:  
```C#
  var builder = new AutoUpdateBuilder()
      .LocalVersion(new Version(1,1))
```

### <b>Get the remote version</b>
To determine the remote available version, the library supports following methods:  
- filebased (UNC path / fileshare)
- http-url based  

Example:  
```C#
  var builder = new AutoUpdateBuilder()
      .RemoteVersion(new Uri("http://..../version.json"))
```


### <b>Get the version package</b>
To download available version package, the library supports following methods: 
- file (UNC path / fileshare) based
- http-url (Uri / delegate) based  
- byte[] based

Example:  
```C#
  var builder = new AutoUpdateBuilder()
      .AddPackage("./1.0.0.1.zip")
```

### <b>Get Azure Blob Storage</b> (version & package)
To determine the remote available version and include possibility to download the latest release package.

Example:  
```C#
  var builder = new AutoUpdateBuilder()
		.AddBlobStorage( 
			"connection string from: `Storage Account` >> `Access keys`", 
			"container name"
		)
```

### <b>Get Github Release</b> (version & package)
To determine the remote available version and include possibility to download the latest release package.

Example:  
```C#
  var builder = new AutoUpdateBuilder()
		.AddGithub("https://github.com/user/repo")
```









### <b>Check for remote>local version</b>
Done by your own application; so you have full control about
- asking end-user permission to upgrade.
- choose to upgrade blocking/non-blocking

### <b>Update</b>
Performs the download, extractions and (over)writes.
```C#
await au.Update()
```

### <b>Restart</b>
Start the latest correct version of the application.  
Returns a process exit code as status of the restart process.  
```C#
int exitCode = await au.Restart()
```

### <b>Publish</b> 
Publish current application as newest version.
```C#
await au.Publish()
```

### <b>Run scripts before Starting new process</b>
For preparing your environment with the new release you can implement scripts.  
The location of the scripts has to be on the `root` location of the application.  
The filenames has to be named as:  
- `pre-install.(bat/cmd/ps/exe)`  
- `post-install.(bat/cmd/ps/exe)`  

### <b>Version information in file layouts</b>
Version content from either `file` or `HTTP` should have minimal the following structure:  
- XML
```xml
<?xml version="1.0" encoding="UTF-8"?>
<root>
	<version>1.2.3.4</version>
</root>
```
- JSON
```json
{
	"version" : "1.2.3.4"
}
```
- TXT
```txt
1.2.3.4
```

## History
Jan 2021 : First idea's / implementation  
Nov 2021 : Add features / update code

## Remarks
Self-Container exe files are currently NOT supported.

### Features (planned)
- clean-up local files not in package
- framwork/platform detection -> being able to include in version/package url providers.
  - arm/x86/x64/etc.
  - Example : package name = 1.0.4.0.x64.zip and 1.0.4.0.arm.zip
   -> fixed platform indentifiers need to be present (and may need to be implemented) in CAL/API.DeploymentInformation methods; CAL.API checks presence of these identifiers to recognize/download/publish them to BlobStorage !

## TODO 
<b>waarom een stabiele release omgeving met oude waardes van vorige release erin plaatsen?</b>  
[ ] Zou tijdens update ook "merge"  nodig zijn ? 
	Voorbeeld : dat je bestaande .json bestanden ' merged ' met update json? oorspronkelijk waardes blijven , maar additionele dingen worden samengevoegd? hm. 
	klinkt alleen niet logisch. ook lastig aan te geven in de zip file misschien?

[ ]  GithubVersionProvider::GitHubClient (octokit)  
// Bypass the limit calls to github.  
// Credentials = new Credentials(username, password)
		
[ ]  GithubVersionProvider Not Implemented  
// SetVersionAsync  
// SetContentAsync  
		
[ ]  PackageHelper::CurrentVersionToZip && SetVersion  
// make extracted zip files into memory (now we download the file and delete it afterwards)

[ ] TeamCityVersionProvider version:
    0.1.0-develop.29 is not correct and have to change to 0.1.29.0 [{major}.{minor}.{build}.{revision}]
