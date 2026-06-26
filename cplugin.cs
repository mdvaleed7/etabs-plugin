using ETABSv1;
using System;
using System.Windows.Forms;

namespace CSiNET8PluginExample1
{
    // Implementing the cPluginContract interface is not required, however
    // it is recommended to ensure that the required cPlugin methods are created correctly.
    // Do not implement the Info or Main methods explicitly,
    // i.e. their method signatures are correct as is
    public class cPlugin : cPluginContract
    {
        private static string _modality = "Non-Modal";
        private static string _versionString = "2.0";
        private int errorCode = 0; // default return code is no error

        public int Info(ref string Text)
        {
            try
            {
                Text = "This plugin is supplied by Computers and Structures, Inc., " +
                       "as a simple example for developers of new plugins for CSI products. " +
                       "It starts a new blank model, then converts a line of text into " +
                       "frame objects and adds them to the model. It trivially uses the popular " +
                       "Newtonsoft.Json library to demonstrate proper dependency management. " +
                       "If you enter the " +
                       "text \"crash\", an error will be generated for testing purposes. " +
                       "Version " + _versionString;
            }
            catch (Exception)
            {
            }

            return 0;
        }

        public void Main(ref cSapModel sapModel, ref cPluginCallback pluginCallback)
        {
            var aForm = new Form1();

            try
            {
                aForm.SetSapModel(ref sapModel, ref pluginCallback);

                if (string.Compare(_modality, "Non-Modal", true) == 0)
                {
                    // Non-modal form, allows graphics refresh operations in CSI program, 
                    // but Main will return to CSI program before the form is closed.
                    aForm.Show();
                }
                else
                {
                    // Modal form, will not return to CSI program until form is closed,
                    // but may cause errors when refreshing the view.
                    aForm.ShowDialog();
                }

                // It is very important to call pluginCallback.Finish(errorCode) when the form closes, !!!
                // otherwise, the CSI program will wait and be hung !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                // This must be done inside the closing event for the form itself, not here !!!!!!!!!!!!!!

                // If you have only algorithmic code here without any forms, 
                // then call pluginCallback.Finish(errorCode) here before returning to the CSI program

                // errorCode = 0 indicates that the plugin completed successfully
                // ie pluginCallback.Finish(0)
                // errorCode = (Any non-zero integer) indicates that the plugin closed with an error
                // ie pluginCallback.Finish(1)
                // If an error occurs, the errorCode value will be displayed
                // to the plugin end-user in a message box, for debugging purposes.

                // If your code will run for more than a few seconds, you should exercise
                // the Windows messaging loop to keep the program responsive. You may 
                // also want to provide an opportunity for the user to cancel operations.

            }
            catch (Exception ex)
            {
                errorCode = 1;
                MessageBox.Show("The following error terminated the plugin:" + Environment.NewLine + ex.Message);

                // call Finish to inform the CSI program that the plugin has terminated
                try
                {
                    pluginCallback.Finish(errorCode); // error code 1 will be visible to plugin end-user for debugging purposes
                }
                catch (Exception)
                {
                }
            }
        }
    }
}

