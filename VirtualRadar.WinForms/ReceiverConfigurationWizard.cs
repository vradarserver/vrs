// Copyright © 2014 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows.Forms;
using Gui.Wizard;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;
using System.Net.Sockets;
using System.Net;
using VirtualRadar.Resources;
using VirtualRadar.Localisation;

namespace VirtualRadar.WinForms
{
    public partial class ReceiverConfigurationWizard : Form
    {
        private Stack<WizardPage> _PageHistory = new Stack<WizardPage>();
        private string _SourceName;
        private bool _SourceIsProgram;

        private ReceiverConfigurationWizardAnswers _Answers;
        public ReceiverConfigurationWizardAnswers Answers
        {
            get { return _Answers; }
            set
            {
                _Answers = value;
                BindAnswers();
            }
        }

        public ReceiverConfigurationWizard()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                Localise.Form(this);

                pictureBoxLogo.Image = Images.Radio48x48;

                Answers = new ReceiverConfigurationWizardAnswers() {
                    ConnectionType =        ConnectionType.TCP,
                    DedicatedReceiver =     DedicatedReceiver.KineticAvionicsAll,
                    KineticConnection =     KineticConnection.BaseStation,
                    ReceiverClass =         ReceiverClass.SoftwareDefinedRadio,
                    SdrDecoder =            SdrDecoder.Rtl1090,
                    UseLoopbackAddress =    YesNo.Yes,
                };

                foreach(WizardPage page in wizard.Pages) {
                    page.CloseFromNext += Page_CloseFromNext;
                    page.CloseFromBack += Page_CloseFromBack;
                }
            }
        }

        private void BindAnswers()
        {
            // ReceiverClass
            radioButtonReceiverSdr.Tag =        ReceiverClass.SoftwareDefinedRadio;
            radioButtonReceiverDedicated.Tag =  ReceiverClass.DedicatedHardware;
            radioPanelReceiverClass.ValueMember = PropertyHelper.ExtractName(Answers, r => r.ReceiverClass);
            radioPanelReceiverClass.DataSource = Answers;

            // SdrDecoder
            radioButtonSdrAdsbSharp.Tag =   SdrDecoder.AdsbSharp;
            radioButtonSdrCocoa1090.Tag =   SdrDecoder.Cocoa1090;
            radioButtonSdrDump1090.Tag =    SdrDecoder.Dump1090;
            radioButtonSdrGrAirModes.Tag =  SdrDecoder.GrAirModes;
            radioButtonSdrModesdeco.Tag =   SdrDecoder.Modesdeco;
            radioButtonSdrOther.Tag =       SdrDecoder.Other;
            radioButtonSdrRtl1090.Tag =     SdrDecoder.Rtl1090;
            radioPanelSdrDecoder.ValueMember = PropertyHelper.ExtractName(Answers, r => r.SdrDecoder);
            radioPanelSdrDecoder.DataSource = Answers;

            // DedicatedReceiver
            radioButtonDedicatedBeast.Tag =     DedicatedReceiver.Beast;
            radioButtonDedicatedKinetics.Tag =  DedicatedReceiver.KineticAvionicsAll;
            radioButtonDedicatedMicroAdsb.Tag = DedicatedReceiver.MicroAdsb;
            radioButtonDedicatedOther.Tag =     DedicatedReceiver.Other;
            radioButtonDedicatedRadarBox.Tag =  DedicatedReceiver.RadarBox;
            radioPanelDedicatedReceiver.ValueMember = PropertyHelper.ExtractName(Answers, r => r.DedicatedReceiver);
            radioPanelDedicatedReceiver.DataSource = Answers;

            // ConnectionType
            radioButtonConnectionTypeNetwork.Tag =  ConnectionType.TCP;
            radioButtonConnectionTypeUsb.Tag =      ConnectionType.COM;
            radioPanelConnectionType.ValueMember = PropertyHelper.ExtractName(Answers, r => r.ConnectionType);
            radioPanelConnectionType.DataSource = Answers;

            // KineticConnection
            radioButtonKineticConnectBaseStation.Tag =  KineticConnection.BaseStation;
            radioButtonKineticConnectHardware.Tag =     KineticConnection.DirectToHardware;
            radioPanelKineticConnection.ValueMember = PropertyHelper.ExtractName(Answers, r => r.KineticConnection);
            radioPanelKineticConnection.DataSource = Answers;

            // Loopback
            radioButtonLoopbackYes.Tag =    YesNo.Yes;
            radioButtonLoopbackNo.Tag =     YesNo.No;
            radioPanelLoopback.ValueMember = PropertyHelper.ExtractName(Answers, r => r.UseLoopbackAddress);
            radioPanelLoopback.DataSource = Answers;

            // Network address
            textBoxNetworkAddress.DataBindings.Add("Text", Answers, PropertyHelper.ExtractName(Answers, r => r.NetworkAddress));
        }

        private bool DoValidation(WizardPage page)
        {
            string message = null;

            if(page == wizardPageNetworkAddress) {
                Answers.NetworkAddress = (Answers.NetworkAddress ?? "").Trim();
                if(Answers.NetworkAddress == "") message = "Please enter a network address";
                else {
                    var currentCursor = Cursor.Current;
                    Cursor = Cursors.WaitCursor;
                    try {
                        try {
                            var dnsLookup = Dns.GetHostEntry(Answers.NetworkAddress);
                            if(dnsLookup == null || dnsLookup.AddressList == null || dnsLookup.AddressList.Length == 0) {
                                message = "This address cannot be resolved";
                            }
                        } catch {
                            message = "Please enter a valid network address";
                        }
                    } finally {
                        Cursor = currentCursor;
                    }
                }
                errorProvider.SetError(textBoxNetworkAddress, message);
            }

            return message == null;
        }

        private void Page_CloseFromNext(object sender, PageEventArgs args)
        {
            var page = wizard.Page;

            if(!DoValidation(page)) {
                args.Page = page;
            } else {
                if(page == wizardPageSdrOrDedicated) {
                    switch(Answers.ReceiverClass) {
                        case ReceiverClass.SoftwareDefinedRadio:
                            _SourceName = Strings.RecConWizSourceTheSdr;
                            _SourceIsProgram = true;
                            args.Page = wizardPageSdrDecoder;
                            break;
                        case ReceiverClass.DedicatedHardware:
                            _SourceName = Strings.RecConWizSourceTheReceiver;
                            _SourceIsProgram = false;
                            args.Page = wizardPageDedicatedReceiver;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                } else if(page == wizardPageSdrDecoder) {
                    args.Page = wizardPageLoopback;
                } else if(page == wizardPageDedicatedReceiver) {
                    switch(Answers.DedicatedReceiver) {
                        case DedicatedReceiver.Beast:               args.Page = wizardPageConnectionType; break;
                        case DedicatedReceiver.KineticAvionicsAll:  args.Page = wizardPageKineticConnection; break;
                        default:                                    args.Page = wizardPageNetworkAddress; break;
                    }
                } else if(page == wizardPageConnectionType) {
                    switch(Answers.ConnectionType) {
                        case ConnectionType.COM:    args.Page = wizardPageFinish; break;
                        case ConnectionType.TCP:    args.Page = wizardPageNetworkAddress; break;
                        default:                    throw new NotImplementedException();
                    }
                } else if(page == wizardPageKineticConnection) {
                    switch(Answers.KineticConnection) {
                        case KineticConnection.BaseStation:
                            _SourceName = Strings.BaseStation;
                            _SourceIsProgram = true;
                            args.Page = wizardPageLoopback;
                            break;
                        case KineticConnection.DirectToHardware:
                            _SourceName = Strings.RecConWizSourceTheReceiver;
                            _SourceIsProgram = false;
                            args.Page = wizardPageNetworkAddress;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                } else if(page == wizardPageLoopback) {
                    if(Answers.UseLoopbackAddress == YesNo.No) args.Page = wizardPageNetworkAddress;
                    else                                       args.Page = wizardPageFinish;
                } else if(page == wizardPageNetworkAddress) {
                    args.Page = wizardPageFinish;
                }

                labelLoopbackTitle.Text = String.Format(Strings.RecConWizLoopbackTitle, _SourceName);
                labelNetworkAddressTitle.Text = String.Format(Strings.RecConWizNetworkAddressTitle, _SourceIsProgram ? "the computer that is running " : "", _SourceName);

                _PageHistory.Push(page);
            }
        }

        private void Page_CloseFromBack(object sender, PageEventArgs args)
        {
            if(_PageHistory.Count > 0) args.Page = _PageHistory.Pop();
        }
    }

    public enum YesNo
    {
        Yes,
        No,
    }

    public enum ReceiverClass
    {
        SoftwareDefinedRadio,
        DedicatedHardware,
    }

    public enum SdrDecoder
    {
        AdsbSharp,
        Cocoa1090,
        Dump1090,
        GrAirModes,
        Modesdeco,
        Rtl1090,
        Other,
    }

    public enum DedicatedReceiver
    {
        RadarBox,
        Beast,
        KineticAvionicsAll,
        MicroAdsb,
        Other,
    }

    public enum KineticConnection
    {
        BaseStation,
        DirectToHardware,
    }

    public class ReceiverConfigurationWizardAnswers : INotifyPropertyChanged
    {
        private ReceiverClass _ReceiverClass;
        public ReceiverClass ReceiverClass
        {
            get { return _ReceiverClass; }
            set { SetField(ref _ReceiverClass, value, () => ReceiverClass); }
        }

        private SdrDecoder _SdrDecoder;
        public SdrDecoder SdrDecoder
        {
            get { return _SdrDecoder; }
            set { SetField(ref _SdrDecoder, value, () => SdrDecoder); }
        }

        private DedicatedReceiver _DedicatedReceiver;
        public DedicatedReceiver DedicatedReceiver
        {
            get { return _DedicatedReceiver; }
            set { SetField(ref _DedicatedReceiver, value, () => DedicatedReceiver); }
        }

        private ConnectionType _ConnectionType;
        public ConnectionType ConnectionType
        {
            get { return _ConnectionType; }
            set { SetField(ref _ConnectionType, value, () => ConnectionType); }
        }

        private KineticConnection _KineticConnection;
        public KineticConnection KineticConnection
        {
            get { return _KineticConnection; }
            set { SetField(ref _KineticConnection, value, () => KineticConnection); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            if(PropertyChanged != null) PropertyChanged(this, args);
        }

        private YesNo _UseLoopbackAddress;
        public YesNo UseLoopbackAddress
        {
            get { return _UseLoopbackAddress; }
            set { SetField(ref _UseLoopbackAddress, value, () => UseLoopbackAddress); }
        }

        private string _NetworkAddress;
        public string NetworkAddress
        {
            get { return _NetworkAddress; }
            set { SetField(ref _NetworkAddress, value, () => NetworkAddress); }
        }


        protected bool SetField<T>(ref T field, T value, Expression<Func<T>> selectorExpression)
        {
            if(EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;

            if(selectorExpression == null) throw new ArgumentNullException("selectorExpression");
            MemberExpression body = selectorExpression.Body as MemberExpression;
            if(body == null) throw new ArgumentException("The body must be a member expression");
            OnPropertyChanged(new PropertyChangedEventArgs(body.Member.Name));

            return true;
        }
    }
}
