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
using VirtualRadar.Interface.View;

namespace VirtualRadar.WinForms
{
    /// <summary>
    /// A wizard that collects information from the user about a receiver that they
    /// want to connect to.
    /// </summary>
    public partial class ReceiverConfigurationWizard : BaseForm, IReceiverConfigurationWizard
    {
        #region Fields
        /// <summary>
        /// The stack of pages that the user has gone through. Used by the Back button.
        /// </summary>
        private Stack<WizardPage> _PageHistory = new Stack<WizardPage>();

        /// <summary>
        /// A generic name for the receiver (e.g. 'the SDR decoder'). Used in some headings.
        /// </summary>
        private string _SourceName;
        
        /// <summary>
        /// True if the receiver is a program, in which case it could be running on the same
        /// machine as VRS. False if it's dedicated hardware.
        /// </summary>
        private bool _SourceIsProgram;
        #endregion

        #region Properties
        private ReceiverConfigurationWizardAnswers _Answers;
        /// <summary>
        /// Gets or sets the user's answers to the questions.
        /// </summary>
        public IReceiverConfigurationWizardAnswers Answers
        {
            get { return _Answers; }
            set
            {
                _Answers = (ReceiverConfigurationWizardAnswers)value;
                BindAnswers();
            }
        }
        #endregion

        #region Ctors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ReceiverConfigurationWizard()
        {
            InitializeComponent();
        }
        #endregion

        #region OnLoad
        /// <summary>
        /// Called after the form has been loaded but before it is shown to the user.
        /// </summary>
        /// <param name="e"></param>
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
        #endregion

        #region BindAnswers
        /// <summary>
        /// Binds controls to the properties of the Answers object.
        /// </summary>
        private void BindAnswers()
        {
            var answers = (ReceiverConfigurationWizardAnswers)Answers;

            // ReceiverClass
            radioButtonReceiverSdr.Tag =        ReceiverClass.SoftwareDefinedRadio;
            radioButtonReceiverDedicated.Tag =  ReceiverClass.DedicatedHardware;
            radioPanelReceiverClass.ValueMember = PropertyHelper.ExtractName(answers, r => r.ReceiverClass);
            radioPanelReceiverClass.DataSource = Answers;

            // SdrDecoder
            radioButtonSdrAdsbSharp.Tag =   SdrDecoder.AdsbSharp;
            radioButtonSdrDump1090.Tag =    SdrDecoder.Dump1090;
            radioButtonSdrGrAirModes.Tag =  SdrDecoder.GrAirModes;
            radioButtonSdrModesdeco.Tag =   SdrDecoder.Modesdeco;
            radioButtonSdrOther.Tag =       SdrDecoder.Other;
            radioButtonSdrRtl1090.Tag =     SdrDecoder.Rtl1090;
            radioButtonSdrFR24Feeder.Tag =  SdrDecoder.FR24Feeder;
            radioPanelSdrDecoder.ValueMember = PropertyHelper.ExtractName(answers, r => r.SdrDecoder);
            radioPanelSdrDecoder.DataSource = Answers;

            // DedicatedReceiver
            radioButtonDedicatedBeast.Tag =     DedicatedReceiver.Beast;
            radioButtonDedicatedKinetics.Tag =  DedicatedReceiver.KineticAvionicsAll;
            radioButtonDedicatedMicroAdsb.Tag = DedicatedReceiver.MicroAdsb;
            radioButtonDedicatedOther.Tag =     DedicatedReceiver.Other;
            radioButtonDedicatedRadarBox.Tag =  DedicatedReceiver.RadarBox;
            radioPanelDedicatedReceiver.ValueMember = PropertyHelper.ExtractName(answers, r => r.DedicatedReceiver);
            radioPanelDedicatedReceiver.DataSource = Answers;

            // ConnectionType
            radioButtonConnectionTypeNetwork.Tag =  ConnectionType.TCP;
            radioButtonConnectionTypeUsb.Tag =      ConnectionType.COM;
            radioPanelConnectionType.ValueMember = PropertyHelper.ExtractName(answers, r => r.ConnectionType);
            radioPanelConnectionType.DataSource = Answers;

            // KineticConnection
            radioButtonKineticConnectBaseStation.Tag =  KineticConnection.BaseStation;
            radioButtonKineticConnectHardware.Tag =     KineticConnection.DirectToHardware;
            radioPanelKineticConnection.ValueMember = PropertyHelper.ExtractName(answers, r => r.KineticConnection);
            radioPanelKineticConnection.DataSource = Answers;

            // Loopback
            radioButtonLoopbackYes.Tag =    YesNo.Yes;
            radioButtonLoopbackNo.Tag =     YesNo.No;
            radioPanelLoopback.ValueMember = PropertyHelper.ExtractName(answers, r => r.UseLoopbackAddress);
            radioPanelLoopback.DataSource = Answers;

            // Network address
            textBoxNetworkAddress.DataBindings.Add("Text", answers, PropertyHelper.ExtractName(Answers, r => r.NetworkAddress));
        }
        #endregion

        #region DoValidation
        /// <summary>
        /// Performs the validation on a single page within the wizard.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        private bool DoValidation(WizardPage page)
        {
            string message = null;

            if(page == wizardPageNetworkAddress) {
                Answers.NetworkAddress = (Answers.NetworkAddress ?? "").Trim();
                if(Answers.NetworkAddress == "") message = Strings.DataSourceNetworkAddressMissing;
                else {
                    var currentCursor = Cursor.Current;
                    Cursor = Cursors.WaitCursor;
                    try {
                        try {
                            var dnsLookup = Dns.GetHostEntry(Answers.NetworkAddress);
                            if(dnsLookup == null || dnsLookup.AddressList == null || dnsLookup.AddressList.Length == 0) {
                                message = String.Format(Strings.CannotResolveAddress, Answers.NetworkAddress);
                            }
                        } catch {
                            message = Strings.CidrInvalid;
                        }
                    } finally {
                        Cursor = currentCursor;
                    }
                }
                errorProvider.SetError(textBoxNetworkAddress, message);
            }

            return message == null;
        }
        #endregion

        #region Event handlers
        /// <summary>
        /// Called when the user closes a page as a result of clicking the Back button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Page_CloseFromBack(object sender, PageEventArgs args)
        {
            if(_PageHistory.Count > 0) args.Page = _PageHistory.Pop();
        }

        /// <summary>
        /// Called when the wizard is closing a page as the result of the user clicking Next.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Page_CloseFromNext(object sender, PageEventArgs args)
        {
            var page = wizard.Page;
            var answers = (ReceiverConfigurationWizardAnswers)Answers;

            if(!DoValidation(page)) {
                args.Page = page;
            } else {
                if(page == wizardPageSdrOrDedicated) {
                    switch(answers.ReceiverClass) {
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
                    switch(answers.DedicatedReceiver) {
                        case DedicatedReceiver.Beast:               args.Page = wizardPageConnectionType; break;
                        case DedicatedReceiver.KineticAvionicsAll:  args.Page = wizardPageKineticConnection; break;
                        case DedicatedReceiver.MicroAdsb:           args.Page = wizardPageFinish; break;
                        default:                                    args.Page = wizardPageNetworkAddress; break;
                    }
                } else if(page == wizardPageConnectionType) {
                    switch(answers.ConnectionType) {
                        case ConnectionType.COM:    args.Page = wizardPageFinish; break;
                        case ConnectionType.TCP:    args.Page = wizardPageNetworkAddress; break;
                        default:                    throw new NotImplementedException();
                    }
                } else if(page == wizardPageKineticConnection) {
                    switch(answers.KineticConnection) {
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
                    if(answers.UseLoopbackAddress == YesNo.No) args.Page = wizardPageNetworkAddress;
                    else                                       args.Page = wizardPageFinish;
                } else if(page == wizardPageNetworkAddress) {
                    args.Page = wizardPageFinish;
                }

                labelLoopbackTitle.Text = String.Format(Strings.RecConWizLoopbackTitle, _SourceName);
                if(_SourceIsProgram) labelNetworkAddressTitle.Text = String.Format(Strings.RecConWizNetworkProgramAddressTitle, _SourceName);
                else                 labelNetworkAddressTitle.Text = String.Format(Strings.RecConWizNetworkHardwareAddressTitle, _SourceName);

                _PageHistory.Push(page);
            }
        }
        #endregion
    }
}
