﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18046
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Reflection;

namespace System.Composition.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("System.Composition.Properties.Resources", typeof(Resources).GetTypeInfo().Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Missing dependency &apos;{0}&apos; on &apos;{1}&apos;..
        /// </summary>
        internal static string CompositionContextExtensions_MissingDependency {
            get {
                return ResourceManager.GetString("CompositionContextExtensions_MissingDependency", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The default conventions for the container configuration have already been set..
        /// </summary>
        internal static string ContainerConfiguration_DefaultConventionSet {
            get {
                return ResourceManager.GetString("ContainerConfiguration_DefaultConventionSet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Multiple imports have been configured for &apos;{0}&apos;. At most one import can be applied to a single site..
        /// </summary>
        internal static string ContractHelpers_TooManyImports {
            get {
                return ResourceManager.GetString("ContractHelpers_TooManyImports", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Multiple importing constructors were found on type &apos;{0}&apos;..
        /// </summary>
        internal static string DiscoveredPart_MultipleImportingConstructorsFound {
            get {
                return ResourceManager.GetString("DiscoveredPart_MultipleImportingConstructorsFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No importing constructor was found on type &apos;{0}&apos;..
        /// </summary>
        internal static string DiscoveredPart_NoImportingConstructorsFound {
            get {
                return ResourceManager.GetString("DiscoveredPart_NoImportingConstructorsFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The method {0}.{1} has the OnImportsSatisfied attribute applied, but is not a public or internal parameterless instance method returning void..
        /// </summary>
        internal static string OnImportsSatisfiedFeature_AttributeError {
            get {
                return ResourceManager.GetString("OnImportsSatisfiedFeature_AttributeError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Exported contract &apos;{0}&apos; of open generic part &apos;{1}&apos; does not match the generic arguments of the class..
        /// </summary>
        internal static string TypeInspector_ArgumentMissmatch {
            get {
                return ResourceManager.GetString("TypeInspector_ArgumentMissmatch", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Exported contract type &apos;{0}&apos; is not assignable from part &apos;{1}&apos;..
        /// </summary>
        internal static string TypeInspector_ContractNotAssignable {
            get {
                return ResourceManager.GetString("TypeInspector_ContractNotAssignable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Exported contract type &apos;{0}&apos; is not assignable from property &apos;{1}&apos; of part &apos;{2}&apos;..
        /// </summary>
        internal static string TypeInspector_ExportedContractTypeNotAssignable {
            get {
                return ResourceManager.GetString("TypeInspector_ExportedContractTypeNotAssignable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The open generic export &apos;{0}&apos; on part &apos;{1}&apos; is not compatible with the contract &apos;{2}&apos;..
        /// </summary>
        internal static string TypeInspector_ExportNotCompatible {
            get {
                return ResourceManager.GetString("TypeInspector_ExportNotCompatible", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Open generic part &apos;{0}&apos; cannot export non-generic contract &apos;{1}&apos;..
        /// </summary>
        internal static string TypeInspector_NoExportNonGenericContract {
            get {
                return ResourceManager.GetString("TypeInspector_NoExportNonGenericContract", resourceCulture);
            }
        }
    }
}