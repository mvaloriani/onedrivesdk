﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OneDrive.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")]
    internal sealed partial class ODSettings : global::System.Configuration.ApplicationSettingsBase {
        
        private static ODSettings defaultInstance = ((ODSettings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new ODSettings())));
        
        public static ODSettings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string RefreshToken {
            get {
                return ((string)(this["RefreshToken"]));
            }
            set {
                this["RefreshToken"] = value;
            }
        }
    }
}