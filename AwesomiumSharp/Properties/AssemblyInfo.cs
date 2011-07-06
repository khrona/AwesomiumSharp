using System.Windows;
using System.Reflection;
using System.Windows.Markup;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("AwesomiumSharp v1.6.0")]
[assembly: AssemblyDescription(".NET Wrapper for Awesomium")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Khrona LLC")]
[assembly: AssemblyProduct("AwesomiumSharp")]
[assembly: AssemblyCopyright("Copyright © Khrona LLC 2011")]
[assembly: AssemblyTrademark("Awesomium is a trademark of Khrona LLC")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("1a1d92a3-87be-403a-aa3f-eab63c95e1f2")]

// The ThemeInfo attribute describes where any theme specific and generic resource dictionaries can be found.
// 1st parameter: where theme specific resource dictionaries are located
// (used if a resource is not found in the page, 
// or application resource dictionaries)

// 2nd parameter: where the generic resource dictionary is located
// (used if a resource is not found in the page, 
// app, and any theme specific resource dictionaries)
[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]

// AmaDeuS (07/03/11): Added these definitions.
[assembly: XmlnsDefinition( "http://schemas.awesomium.com/sharp", "AwesomiumSharp" )]
[assembly: XmlnsDefinition( "http://schemas.awesomium.com/winfx", "AwesomiumSharp.Windows.Controls" )]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.6.0.6")]
[assembly: AssemblyFileVersion("1.6.0.6")]
