using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Drawing;
using VirtualRadar.Localisation;
using System.Windows.Forms;
using VirtualRadar.WinForms.Controls;

namespace VirtualRadar.WinForms.Options
{
    /// <summary>
    /// The base user control for all sheets that are implemented as user controls.
    /// </summary>
    public partial class SheetControl : OptionsPage, ISheet
    {
        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<ParentPage> Pages { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual string SheetTitle { get { return ""; } }
        #endregion

        #region Ctors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public SheetControl()
        {
            InitializeComponent();

            Pages = new List<ParentPage>();
        }
        #endregion

        #region ToString
        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SheetTitle;
        }
        #endregion

        #region SetInitialValues
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void SetInitialValues()
        {
            ;
        }
        #endregion

        #region Event handlers
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
            }
        }
        #endregion
    }
}
