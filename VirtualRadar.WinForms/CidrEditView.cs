using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using InterfaceFactory;
using VirtualRadar.Interface.Presenter;
using VirtualRadar.Interface.View;
using VirtualRadar.Localisation;

namespace VirtualRadar.WinForms
{
    /// <summary>
    /// The WinForms implementation of ICidrEditView
    /// </summary>
    public partial class CidrEditView : BaseForm, ICidrEditView
    {
        private ICidrEditPresenter _Presenter;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Cidr
        {
            get { return textBoxCidr.Text.Trim(); }
            set { textBoxCidr.Text = value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool CidrIsValid
        {
            get { return _Presenter == null ? false : _Presenter.CidrIsValid; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string FirstMatchingAddress
        {
            get { return labelFirstMatchingAddress.Text; }
            set { labelFirstMatchingAddress.Text = value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string LastMatchingAddress
        {
            get { return labelLastMatchingAddress.Text; }
            set { labelLastMatchingAddress.Text = value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler CidrChanged;

        /// <summary>
        /// Raises <see cref="CidrChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnCidrChanged(EventArgs args)
        {
            if(CidrChanged != null) CidrChanged(this, args);
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public CidrEditView() : base()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Called after the view has loaded but before it is shown to the user.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                Localise.Form(this);

                _Presenter = Factory.Singleton.Resolve<ICidrEditPresenter>();
                _Presenter.Initialise(this);
            }
        }

        /// <summary>
        /// Called whenever the CIDR changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxCidr_TextChanged(object sender, EventArgs e)
        {
            OnCidrChanged(e);
        }

        /// <summary>
        /// Called whenever the user presses a key in the CIDR text control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxCidr_KeyDown(object sender, KeyEventArgs e)
        {
            if(!e.Handled && e.Modifiers == Keys.None) {
                var handled = true;
                switch(e.KeyCode) {
                    case Keys.Escape:
                        Close();
                        Cidr = "";
                        break;
                    case Keys.Return:
                        Close();
                        break;
                    default:
                        handled = false;
                        break;
                }

                e.Handled = handled;
                if(handled) e.SuppressKeyPress = true;
            }
        }
    }
}
