﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace banhang24.VMGAPI {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="VMGAPI.VMGAPISoap")]
    public interface VMGAPISoap {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/AdsSendSms", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        banhang24.VMGAPI.ApiAdsReturn AdsSendSms(string[] msisdns, string alias, string message, string sendTime, string authenticateUser, string authenticatePass);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/AdsSendSms", ReplyAction="*")]
        System.Threading.Tasks.Task<banhang24.VMGAPI.ApiAdsReturn> AdsSendSmsAsync(string[] msisdns, string alias, string message, string sendTime, string authenticateUser, string authenticatePass);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/AdsGPCSendSms", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        banhang24.VMGAPI.ApiBulkReturn AdsGPCSendSms(string msisdn, string alias, string message, string sendTime, string authenticateUser, string authenticatePass);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/AdsGPCSendSms", ReplyAction="*")]
        System.Threading.Tasks.Task<banhang24.VMGAPI.ApiBulkReturn> AdsGPCSendSmsAsync(string msisdn, string alias, string message, string sendTime, string authenticateUser, string authenticatePass);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/BulkSendSms", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        banhang24.VMGAPI.ApiBulkReturn BulkSendSms(string msisdn, string alias, string message, string sendTime, string authenticateUser, string authenticatePass);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/BulkSendSms", ReplyAction="*")]
        System.Threading.Tasks.Task<banhang24.VMGAPI.ApiBulkReturn> BulkSendSmsAsync(string msisdn, string alias, string message, string sendTime, string authenticateUser, string authenticatePass);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/BulkSendSmsWithRequestId", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        banhang24.VMGAPI.ApiBulkReturn BulkSendSmsWithRequestId(string requestId, string msisdn, string alias, string message, string sendTime, string authenticateUser, string authenticatePass);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/BulkSendSmsWithRequestId", ReplyAction="*")]
        System.Threading.Tasks.Task<banhang24.VMGAPI.ApiBulkReturn> BulkSendSmsWithRequestIdAsync(string requestId, string msisdn, string alias, string message, string sendTime, string authenticateUser, string authenticatePass);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/BulkMessageBlockReciver", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        banhang24.VMGAPI.ApiBulkBlockReturn BulkMessageBlockReciver(string[] msisdns, string alias, string message, string sendTime, string authenticateUser, string authenticatePass);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/BulkMessageBlockReciver", ReplyAction="*")]
        System.Threading.Tasks.Task<banhang24.VMGAPI.ApiBulkBlockReturn> BulkMessageBlockReciverAsync(string[] msisdns, string alias, string message, string sendTime, string authenticateUser, string authenticatePass);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/getBalance", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        banhang24.VMGAPI.BalanceInfo getBalance(string authenticateUser, string authenticatePass);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/getBalance", ReplyAction="*")]
        System.Threading.Tasks.Task<banhang24.VMGAPI.BalanceInfo> getBalanceAsync(string authenticateUser, string authenticatePass);
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.4084.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
    public partial class ApiAdsReturn : object, System.ComponentModel.INotifyPropertyChanged {
        
        private int error_codeField;
        
        private string error_detailField;
        
        private string prog_codeField;
        
        private APIAdsSendMT[] detailField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public int error_code {
            get {
                return this.error_codeField;
            }
            set {
                this.error_codeField = value;
                this.RaisePropertyChanged("error_code");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=1)]
        public string error_detail {
            get {
                return this.error_detailField;
            }
            set {
                this.error_detailField = value;
                this.RaisePropertyChanged("error_detail");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=2)]
        public string prog_code {
            get {
                return this.prog_codeField;
            }
            set {
                this.prog_codeField = value;
                this.RaisePropertyChanged("prog_code");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(Order=3)]
        public APIAdsSendMT[] detail {
            get {
                return this.detailField;
            }
            set {
                this.detailField = value;
                this.RaisePropertyChanged("detail");
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
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.4084.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
    public partial class APIAdsSendMT : object, System.ComponentModel.INotifyPropertyChanged {
        
        private string telcoField;
        
        private int error_codeField;
        
        private string error_detailField;
        
        private string prog_codeField;
        
        private int total_subField;
        
        private int total_smsField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public string telco {
            get {
                return this.telcoField;
            }
            set {
                this.telcoField = value;
                this.RaisePropertyChanged("telco");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=1)]
        public int error_code {
            get {
                return this.error_codeField;
            }
            set {
                this.error_codeField = value;
                this.RaisePropertyChanged("error_code");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=2)]
        public string error_detail {
            get {
                return this.error_detailField;
            }
            set {
                this.error_detailField = value;
                this.RaisePropertyChanged("error_detail");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=3)]
        public string prog_code {
            get {
                return this.prog_codeField;
            }
            set {
                this.prog_codeField = value;
                this.RaisePropertyChanged("prog_code");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=4)]
        public int total_sub {
            get {
                return this.total_subField;
            }
            set {
                this.total_subField = value;
                this.RaisePropertyChanged("total_sub");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=5)]
        public int total_sms {
            get {
                return this.total_smsField;
            }
            set {
                this.total_smsField = value;
                this.RaisePropertyChanged("total_sms");
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
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.4084.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
    public partial class BalanceInfo : object, System.ComponentModel.INotifyPropertyChanged {
        
        private int error_codeField;
        
        private string error_detailField;
        
        private string accountNameField;
        
        private string statusField;
        
        private double balanceField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public int error_code {
            get {
                return this.error_codeField;
            }
            set {
                this.error_codeField = value;
                this.RaisePropertyChanged("error_code");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=1)]
        public string error_detail {
            get {
                return this.error_detailField;
            }
            set {
                this.error_detailField = value;
                this.RaisePropertyChanged("error_detail");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=2)]
        public string accountName {
            get {
                return this.accountNameField;
            }
            set {
                this.accountNameField = value;
                this.RaisePropertyChanged("accountName");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=3)]
        public string status {
            get {
                return this.statusField;
            }
            set {
                this.statusField = value;
                this.RaisePropertyChanged("status");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=4)]
        public double balance {
            get {
                return this.balanceField;
            }
            set {
                this.balanceField = value;
                this.RaisePropertyChanged("balance");
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
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.4084.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
    public partial class msisdnInfo : object, System.ComponentModel.INotifyPropertyChanged {
        
        private string msisdnField;
        
        private int isSuccessField;
        
        private string msgField;
        
        private string messageidField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public string msisdn {
            get {
                return this.msisdnField;
            }
            set {
                this.msisdnField = value;
                this.RaisePropertyChanged("msisdn");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=1)]
        public int isSuccess {
            get {
                return this.isSuccessField;
            }
            set {
                this.isSuccessField = value;
                this.RaisePropertyChanged("isSuccess");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=2)]
        public string msg {
            get {
                return this.msgField;
            }
            set {
                this.msgField = value;
                this.RaisePropertyChanged("msg");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=3)]
        public string messageid {
            get {
                return this.messageidField;
            }
            set {
                this.messageidField = value;
                this.RaisePropertyChanged("messageid");
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
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.4084.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
    public partial class ApiBulkBlockReturn : object, System.ComponentModel.INotifyPropertyChanged {
        
        private int error_codeField;
        
        private string error_detailField;
        
        private string progCodeField;
        
        private string blackListField;
        
        private int doublicNumbersField;
        
        private int wrongNumbersField;
        
        private msisdnInfo[] detailField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public int error_code {
            get {
                return this.error_codeField;
            }
            set {
                this.error_codeField = value;
                this.RaisePropertyChanged("error_code");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=1)]
        public string error_detail {
            get {
                return this.error_detailField;
            }
            set {
                this.error_detailField = value;
                this.RaisePropertyChanged("error_detail");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=2)]
        public string progCode {
            get {
                return this.progCodeField;
            }
            set {
                this.progCodeField = value;
                this.RaisePropertyChanged("progCode");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=3)]
        public string blackList {
            get {
                return this.blackListField;
            }
            set {
                this.blackListField = value;
                this.RaisePropertyChanged("blackList");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=4)]
        public int doublicNumbers {
            get {
                return this.doublicNumbersField;
            }
            set {
                this.doublicNumbersField = value;
                this.RaisePropertyChanged("doublicNumbers");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=5)]
        public int wrongNumbers {
            get {
                return this.wrongNumbersField;
            }
            set {
                this.wrongNumbersField = value;
                this.RaisePropertyChanged("wrongNumbers");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(Order=6)]
        public msisdnInfo[] detail {
            get {
                return this.detailField;
            }
            set {
                this.detailField = value;
                this.RaisePropertyChanged("detail");
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
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.4084.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
    public partial class ApiBulkReturn : object, System.ComponentModel.INotifyPropertyChanged {
        
        private int error_codeField;
        
        private string error_detailField;
        
        private int messageIdField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public int error_code {
            get {
                return this.error_codeField;
            }
            set {
                this.error_codeField = value;
                this.RaisePropertyChanged("error_code");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=1)]
        public string error_detail {
            get {
                return this.error_detailField;
            }
            set {
                this.error_detailField = value;
                this.RaisePropertyChanged("error_detail");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=2)]
        public int messageId {
            get {
                return this.messageIdField;
            }
            set {
                this.messageIdField = value;
                this.RaisePropertyChanged("messageId");
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
    public interface VMGAPISoapChannel : banhang24.VMGAPI.VMGAPISoap, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class VMGAPISoapClient : System.ServiceModel.ClientBase<banhang24.VMGAPI.VMGAPISoap>, banhang24.VMGAPI.VMGAPISoap {
        
        public VMGAPISoapClient() {
        }
        
        public VMGAPISoapClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public VMGAPISoapClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public VMGAPISoapClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public VMGAPISoapClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public banhang24.VMGAPI.ApiAdsReturn AdsSendSms(string[] msisdns, string alias, string message, string sendTime, string authenticateUser, string authenticatePass) {
            return base.Channel.AdsSendSms(msisdns, alias, message, sendTime, authenticateUser, authenticatePass);
        }
        
        public System.Threading.Tasks.Task<banhang24.VMGAPI.ApiAdsReturn> AdsSendSmsAsync(string[] msisdns, string alias, string message, string sendTime, string authenticateUser, string authenticatePass) {
            return base.Channel.AdsSendSmsAsync(msisdns, alias, message, sendTime, authenticateUser, authenticatePass);
        }
        
        public banhang24.VMGAPI.ApiBulkReturn AdsGPCSendSms(string msisdn, string alias, string message, string sendTime, string authenticateUser, string authenticatePass) {
            return base.Channel.AdsGPCSendSms(msisdn, alias, message, sendTime, authenticateUser, authenticatePass);
        }
        
        public System.Threading.Tasks.Task<banhang24.VMGAPI.ApiBulkReturn> AdsGPCSendSmsAsync(string msisdn, string alias, string message, string sendTime, string authenticateUser, string authenticatePass) {
            return base.Channel.AdsGPCSendSmsAsync(msisdn, alias, message, sendTime, authenticateUser, authenticatePass);
        }
        
        public banhang24.VMGAPI.ApiBulkReturn BulkSendSms(string msisdn, string alias, string message, string sendTime, string authenticateUser, string authenticatePass) {
            return base.Channel.BulkSendSms(msisdn, alias, message, sendTime, authenticateUser, authenticatePass);
        }
        
        public System.Threading.Tasks.Task<banhang24.VMGAPI.ApiBulkReturn> BulkSendSmsAsync(string msisdn, string alias, string message, string sendTime, string authenticateUser, string authenticatePass) {
            return base.Channel.BulkSendSmsAsync(msisdn, alias, message, sendTime, authenticateUser, authenticatePass);
        }
        
        public banhang24.VMGAPI.ApiBulkReturn BulkSendSmsWithRequestId(string requestId, string msisdn, string alias, string message, string sendTime, string authenticateUser, string authenticatePass) {
            return base.Channel.BulkSendSmsWithRequestId(requestId, msisdn, alias, message, sendTime, authenticateUser, authenticatePass);
        }
        
        public System.Threading.Tasks.Task<banhang24.VMGAPI.ApiBulkReturn> BulkSendSmsWithRequestIdAsync(string requestId, string msisdn, string alias, string message, string sendTime, string authenticateUser, string authenticatePass) {
            return base.Channel.BulkSendSmsWithRequestIdAsync(requestId, msisdn, alias, message, sendTime, authenticateUser, authenticatePass);
        }
        
        public banhang24.VMGAPI.ApiBulkBlockReturn BulkMessageBlockReciver(string[] msisdns, string alias, string message, string sendTime, string authenticateUser, string authenticatePass) {
            return base.Channel.BulkMessageBlockReciver(msisdns, alias, message, sendTime, authenticateUser, authenticatePass);
        }
        
        public System.Threading.Tasks.Task<banhang24.VMGAPI.ApiBulkBlockReturn> BulkMessageBlockReciverAsync(string[] msisdns, string alias, string message, string sendTime, string authenticateUser, string authenticatePass) {
            return base.Channel.BulkMessageBlockReciverAsync(msisdns, alias, message, sendTime, authenticateUser, authenticatePass);
        }
        
        public banhang24.VMGAPI.BalanceInfo getBalance(string authenticateUser, string authenticatePass) {
            return base.Channel.getBalance(authenticateUser, authenticatePass);
        }
        
        public System.Threading.Tasks.Task<banhang24.VMGAPI.BalanceInfo> getBalanceAsync(string authenticateUser, string authenticatePass) {
            return base.Channel.getBalanceAsync(authenticateUser, authenticatePass);
        }
    }
}
