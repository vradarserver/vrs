namespace VirtualRadar.WinForms.OptionPage
{
    partial class Page
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if(disposing) {
                foreach(var observable in _HookedObservableChangeds) {
                    observable.Changed -= Observable_Changed;
                }
                _HookedObservableChangeds.Clear();

                foreach(var control in _HookedInlineHelpControls) {
                    control.Enter -= Control_Enter;
                    control.Leave -= Control_Leave;
                }
                _HookedInlineHelpControls.Clear();

                foreach(var page in _HookedPagePropertyChangeds) {
                    page.PropertyValueChanged -= Page_PropertyChangedValue;
                }
                _HookedPagePropertyChangeds.Clear();

                foreach(var binder in _Binders) {
                    binder.Dispose();
                }
                _Binders.Clear();

                if(components != null) components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        }

        #endregion
    }
}
