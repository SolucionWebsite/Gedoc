//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Gedoc.WebApp.ServiceReferenceEtlReporte {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="LogEtl", Namespace="http://schemas.datacontract.org/2004/07/Gedoc.Etl.Winsrv.Entidades")]
    [System.SerializableAttribute()]
    public partial class LogEtl : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string DescripcionField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.DateTime FechaField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int IdField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int ParentLogIdField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string TipoField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Descripcion {
            get {
                return this.DescripcionField;
            }
            set {
                if ((object.ReferenceEquals(this.DescripcionField, value) != true)) {
                    this.DescripcionField = value;
                    this.RaisePropertyChanged("Descripcion");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.DateTime Fecha {
            get {
                return this.FechaField;
            }
            set {
                if ((this.FechaField.Equals(value) != true)) {
                    this.FechaField = value;
                    this.RaisePropertyChanged("Fecha");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int Id {
            get {
                return this.IdField;
            }
            set {
                if ((this.IdField.Equals(value) != true)) {
                    this.IdField = value;
                    this.RaisePropertyChanged("Id");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int ParentLogId {
            get {
                return this.ParentLogIdField;
            }
            set {
                if ((this.ParentLogIdField.Equals(value) != true)) {
                    this.ParentLogIdField = value;
                    this.RaisePropertyChanged("ParentLogId");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Tipo {
            get {
                return this.TipoField;
            }
            set {
                if ((object.ReferenceEquals(this.TipoField, value) != true)) {
                    this.TipoField = value;
                    this.RaisePropertyChanged("Tipo");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="ServiceReferenceEtlReporte.IServiceInteract")]
    public interface IServiceInteract {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServiceInteract/ExecuteEtl", ReplyAction="http://tempuri.org/IServiceInteract/ExecuteEtlResponse")]
        string ExecuteEtl();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServiceInteract/ExecuteEtl", ReplyAction="http://tempuri.org/IServiceInteract/ExecuteEtlResponse")]
        System.Threading.Tasks.Task<string> ExecuteEtlAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServiceInteract/ExecuteEtlSelectivo", ReplyAction="http://tempuri.org/IServiceInteract/ExecuteEtlSelectivoResponse")]
        string ExecuteEtlSelectivo(string[] destinos);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServiceInteract/ExecuteEtlSelectivo", ReplyAction="http://tempuri.org/IServiceInteract/ExecuteEtlSelectivoResponse")]
        System.Threading.Tasks.Task<string> ExecuteEtlSelectivoAsync(string[] destinos);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServiceInteract/GetLastLogs", ReplyAction="http://tempuri.org/IServiceInteract/GetLastLogsResponse")]
        Gedoc.WebApp.ServiceReferenceEtlReporte.LogEtl[] GetLastLogs();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServiceInteract/GetLastLogs", ReplyAction="http://tempuri.org/IServiceInteract/GetLastLogsResponse")]
        System.Threading.Tasks.Task<Gedoc.WebApp.ServiceReferenceEtlReporte.LogEtl[]> GetLastLogsAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServiceInteract/GetEstadoEjecucion", ReplyAction="http://tempuri.org/IServiceInteract/GetEstadoEjecucionResponse")]
        string GetEstadoEjecucion();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServiceInteract/GetEstadoEjecucion", ReplyAction="http://tempuri.org/IServiceInteract/GetEstadoEjecucionResponse")]
        System.Threading.Tasks.Task<string> GetEstadoEjecucionAsync();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IServiceInteractChannel : Gedoc.WebApp.ServiceReferenceEtlReporte.IServiceInteract, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class ServiceInteractClient : System.ServiceModel.ClientBase<Gedoc.WebApp.ServiceReferenceEtlReporte.IServiceInteract>, Gedoc.WebApp.ServiceReferenceEtlReporte.IServiceInteract {
        
        public ServiceInteractClient() {
        }
        
        public ServiceInteractClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public ServiceInteractClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ServiceInteractClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ServiceInteractClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public string ExecuteEtl() {
            return base.Channel.ExecuteEtl();
        }
        
        public System.Threading.Tasks.Task<string> ExecuteEtlAsync() {
            return base.Channel.ExecuteEtlAsync();
        }
        
        public string ExecuteEtlSelectivo(string[] destinos) {
            return base.Channel.ExecuteEtlSelectivo(destinos);
        }
        
        public System.Threading.Tasks.Task<string> ExecuteEtlSelectivoAsync(string[] destinos) {
            return base.Channel.ExecuteEtlSelectivoAsync(destinos);
        }
        
        public Gedoc.WebApp.ServiceReferenceEtlReporte.LogEtl[] GetLastLogs() {
            return base.Channel.GetLastLogs();
        }
        
        public System.Threading.Tasks.Task<Gedoc.WebApp.ServiceReferenceEtlReporte.LogEtl[]> GetLastLogsAsync() {
            return base.Channel.GetLastLogsAsync();
        }
        
        public string GetEstadoEjecucion() {
            return base.Channel.GetEstadoEjecucion();
        }
        
        public System.Threading.Tasks.Task<string> GetEstadoEjecucionAsync() {
            return base.Channel.GetEstadoEjecucionAsync();
        }
    }
}
