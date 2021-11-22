# AutoUpdate libary
A simple but extensibe, general applicable, autoupdate framework.

Supports multiple version detection schemes, multiple package locations. 
Supports in-place updating.

Like ClickOnce but easier.
Like Squirrel but simpler (and .NET5 support).

## General flow 

### Get the local version
Built in localversion detection based on AssemblyVersion of the entry assembly.
A custom localversion extension point is provided by the builder.

Example:  
```
  var builder = new AutoUpdateBuilder()
      .SetLocalVersion( new Version(1,1))
```

### Get the remote version
To determine the remote available version, the library supports 2 methods : 
- filebased (UNC path / fileshare)
- http-url based

Examples:
"\\fileserver\application\version.ver" 
"http://mylocation/get-latest-version.json"
"http://soapservice/GetVersion"

Example:  
```
  var builder = new AutoUpdateBuilder()
      .SetRemoteVersion( "http://..../version.json" )
```

The returned content (file or http)	multiple content processors
	- Json based version
	- XML based version
	- simple string based 


### Check for remote>local version 

Done by your own appplication; so you have full control about
- asking end-user permission to upgrade.
- choose to upgrade blocking/non-blocking

### Update
Performs the download, extractions and (over)writes.
```
 await au.Update()
```

### Restart 
Restart the application to newest version.
```
 await au.Restart()
```

## Remarks
Self-Container exe files are currently NOT supported.

### Features (planned)

- clean-up local files not in package
- sidebyside installation
- pre/post install scripts
 -> pre/post scripts have to be registered in CAL/API.DeploymentInformation to allow buildserver -> CAL.API to store these files in standard name/version format !
- framwork/platform detection -> being able to include in version/package url providers.
  - arm/x86/x64/etc.
  - Example : package name = 1.0.4.0.x64.zip and 1.0.4.0.arm.zip
   -> fixed platform indentifiers need to be present (and may need to be implemented) in CAL/API.DeploymentInformation methods; CAL.API checks presence of these identifiers to recognize/download/publish them to BlobStorage !

### Version information in text, json and xml layouts

Text content version is supported :
- first line should contain x , x.y or x.y.z 

A json return (either file or http) should have the following structure
```
{
	...
	"version" : "1.2.3.4"
	...
}
```



## History
Jan 2021 : First idea's / implementation


## TODO 

[X] Check Restart (arguments + process wait)

[X] Assembly properties goed vullen.

[X] With Restart : the current ARGUMENTS should be copied to the new process.

[X] Bij een Restart ligt "looping (blijvende restarts)" op de loer...
	VOorbeeld: als de nieuwe zip file geen nieuwere versie blijkt te hebben, maar het mechanisme dus blijft update/restraten. 
	Hoe te voorkomen???

[ ] Library concentreert zich nu op INPLACE (vervangen) 
    Voor bootstrap projecten zou een SIDEBYSIDE versioning ook handig zijn.
	Middels de builder kunnen opgeven?

(Updated json data will not been updated ?)
[X] 
	Zou tijdens update ook "merge"  nodig zijn ? 
	Voorbeeld : dat je bestaande .json bestanden ' merged ' met update json? oorspronkelijk waardes blijven , maar additionele dingen worden samengevoegd? hm. 
	klinkt alleen niet logisch. ook lastig aan te geven in de zip file misschien?


[X] Pre/Post install scripts in de ZIP file ondersteunen. 
	pre-install.* (.bat/.cmd/.ps/.exe) bestanden
    post-install.bat / cmd / ps / exe => indien aanwezig, dan uitvoeren op de client machine?

[X] AutoUpdateBuilder.SetHttpClient( client ) 
    Extra optie in de auto builder om een eigen httpclient mee te geven. 
	Ook dieper liggende classes moet deze client dan gebruiken
	Voordeel -> je kan eventueel token-protected url's dan ook ondersteunen, doordat je een httclient geeft die correcte authentication headers heeft.
		
[X] UrlVersionProvider could auto-detect the correct VersionReader based on ContentType in HTTP response ?!
  
[ ] Xml ondersteuning... ooit.. (already done?? check!)