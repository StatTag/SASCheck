using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SASCheck
{
    public partial class SASCheck : Form
    {
        public SASCheck()
        {
            InitializeComponent();
            cmdCopy.Enabled = false;
            lblCopied.Visible = false;
        }

        private void cmdRun_Click(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                cmdRun.Enabled = false;
                cmdCopy.Enabled = false;

                txtResults.Text = "";

                CheckSASInstallation();
                CheckTypesByID();
                CheckTypesInRegistry();
                CheckConnection();
            }
            finally
            {
                this.Cursor = Cursors.Default;
                cmdRun.Enabled = true;
                cmdCopy.Enabled = true;
            }

        }

        private void AppendResults(string text)
        {
            if (text == null)
            {
                return;
            }
            txtResults.AppendText(text + "\r\n");
        }

        private void AppendException(Exception exc)
        {
            AppendResults("Exception caught:");
            AppendResults(exc.Message);
            AppendResults(exc.StackTrace);
            if (exc.InnerException != null)
            {
                AppendResults("Inner exception:");
                AppendResults(exc.InnerException.Message);
                AppendResults(exc.InnerException.StackTrace);
            }
        }

        private void CheckTypesByID()
        {
            try
            {
                AppendResults("Checking types");
                CheckTypeByProgID("SAS.Application", "SAS Application");
                CheckTypeByCLSID("89FA3E2A-43F9-43e4-B1A2-DAC2CC90B89C", "SAS Application");
                CheckTypeByProgID("SASObjectManager.ObjectFactoryMulti2", "SAS Object Manager");
                CheckOManObjectFactory();
                CheckOManServerDef();
                CheckTypeByProgID("SAS.Workspace", "SAS Workspace");
                CheckTypeByCLSID("20001C16-05D1-4706-9BD2-1782B8575063", "SAS Workspace");
            }
            catch (Exception exc)
            {
                AppendException(exc);
            }
        }

        private void CheckTypesInRegistry()
        {
            try
            {
                AppendResults("Checking type registry keys");
                CheckSubKey(Registry.ClassesRoot, "SAS.Application", true, true);
                CheckSubKey(Registry.ClassesRoot, "SAS.Application.940", true, true);
                CheckSubKey(Registry.ClassesRoot, "SAS.Workspace", true, true);

            }
            catch (Exception exc)
            {
                AppendException(exc);
            }
        }

        private void CheckSubKey(RegistryKey rootKey, string key, bool checkCLSID = false, bool printDefaultValue = false)
        {
            try
            {
                AppendResults(string.Format("Attempting to open {0} {1}", rootKey.Name, key));
                var subKey = rootKey.OpenSubKey(key);
                if (subKey == null)
                {
                    AppendResults("...Not found");
                }
                else
                {
                    AppendResults(string.Format("...Found"));
                    if (checkCLSID)
                    {
                        CheckSubKey(subKey, "CLSID", false, true);
                    }

                    if (printDefaultValue)
                    {
                        AppendResults(string.Format("... With default value {0}", subKey.GetValue("")));
                    }
                }
            }
            catch (Exception exc)
            {
                AppendException(exc);
            }
        }

        private void CheckOManObjectFactory()
        {
            try
            {
                AppendResults("Attempting to create SAS Object Manager ObjectFactoryMulti2");
                SASObjectManager.IObjectFactory2 obObjectFactory = new SASObjectManager.ObjectFactoryMulti2();
                if (obObjectFactory == null)
                {
                    AppendResults("... NULL object");
                }
                else
                {
                    AppendResults("... created");
                }
            }
            catch (Exception exc)
            {
                AppendException(exc);
            }
        }

        private void CheckOManServerDef()
        {
            try
            {
                AppendResults("Attempting to create SAS Object Manager ServerDef");
                SASObjectManager.ServerDef obServer = new SASObjectManager.ServerDef();
                if (obServer == null)
                {
                    AppendResults("... NULL object");
                }
                else
                {
                    AppendResults("... created");
                }
            }
            catch (Exception exc)
            {
                AppendException(exc);
            }
        }

        private void CheckTypeByProgID(string progId, string description)
        {
            try
            {
                AppendResults(string.Format("Checking ProgID {0} ({1})", description, progId));
                var type = Type.GetTypeFromProgID(progId, true);
                if (type == null)
                {
                    AppendResults("...Type is NULL");
                }
                else
                {
                    AppendResults("... Found");
                }
            }
            catch (Exception exc)
            {
                AppendException(exc);
            }
        }

        private void CheckTypeByCLSID(string clsid, string description)
        {
            try
            {
                AppendResults(string.Format("Checking CLSID {0} ({1})", description, clsid));
                var type = Type.GetTypeFromCLSID(new Guid(clsid));
                if (type == null)
                {
                    AppendResults("...Type is NULL");
                }
                else
                {
                    AppendResults("... Found");
                }
            }
            catch (Exception exc)
            {
                AppendException(exc);
            }
        }

        private void CheckSASInstallation()
        {
            try
            {
                AppendResults("Checking SAS Installation");
                var sasHome = ConfigurationManager.AppSettings["SASLocation"];
                AppendResults(string.Format("Checking SAS Home: {0}", sasHome));
                if (Directory.Exists(sasHome))
                {
                    AppendResults("... Found");
                }
                else
                {
                    AppendResults(string.Format("... SAS Home not found"));
                }

                var sasIT = Path.Combine(sasHome, "Integration Technologies");
                AppendResults(string.Format("Checking: {0}", sasIT));
                if (Directory.Exists(sasIT))
                {
                    AppendResults("... Found");
                }
                else
                {
                    AppendResults(string.Format("... SAS Integration Technologies not found"));
                }

                var sasObjMan = Path.Combine(sasIT, "SASOMan.dll");
                AppendResults(string.Format("Checking: {0}", sasObjMan));
                if (File.Exists(sasObjMan))
                {
                    AppendResults("... Found");
                }
                else
                {
                    AppendResults(string.Format("... SAS Object Manager DLL not found"));
                }

                var sasEAM = Path.Combine(sasIT, "SASEAM.dll");
                AppendResults(string.Format("Checking: {0}", sasEAM));
                if (File.Exists(sasEAM))
                {
                    AppendResults("... Found");
                }
                else
                {
                    AppendResults(string.Format("... SAS EAM DLL not found"));
                }

                var sasITx86 = Path.Combine(sasHome, "x86\\Integration Technologies");
                AppendResults(string.Format("Checking: {0}", sasITx86));
                if (Directory.Exists(sasITx86))
                {
                    AppendResults("... Found");
                }
                else
                {
                    AppendResults(string.Format("... SAS Integration Technologies (x86) DLL not found"));
                }

                var sasTlb = Path.Combine(sasITx86, "sas.tlb");
                AppendResults(string.Format("Checking: {0}", sasTlb));
                if (File.Exists(sasTlb))
                {
                    AppendResults("... Found");
                }
                else
                {
                    AppendResults(string.Format("... SAS Type Library not found"));
                }

                var sasIOMCommon = Path.Combine(sasITx86, "SASIOMCommon.tlb");
                AppendResults(string.Format("Checking: {0}", sasIOMCommon));
                if (File.Exists(sasIOMCommon))
                {
                    AppendResults("... Found");
                }
                else
                {
                    AppendResults(string.Format("... SAS IOM Common Type Library not found"));
                }
            }
            catch (Exception exc)
            {
                AppendException(exc);
            }
        }

        private void CheckConnection()
        {
            try
            {
                AppendResults("Attemping full SAS connection");
                AppendResults("Creating object keeper");
                SASObjectManager.ObjectKeeper objectKeeper = new SASObjectManager.ObjectKeeper();
                // Connect using COM protocol, locally installed SAS only
                AppendResults("Creating object factory");
                SASObjectManager.IObjectFactory2 obObjectFactory = new SASObjectManager.ObjectFactoryMulti2();
                AppendResults("Creating server definition");
                SASObjectManager.ServerDef obServer = new SASObjectManager.ServerDef();
                obServer.MachineDNSName = "localhost";
                obServer.Protocol = SASObjectManager.Protocols.ProtocolCom;
                obServer.Port = 0;
                AppendResults("Creating workspace");
                var workspace = (SAS.Workspace)obObjectFactory.CreateObjectByServer(Name, true, obServer, null, null);

                AppendResults("Adding workspace to object keeper");
                objectKeeper.AddObject(1, "Test", workspace);

                AppendResults("Checking for LanguageService");
                var lang = workspace.LanguageService;
                if (lang == null)
                {
                    AppendResults("... Is NULL");
                }
                else
                {
                    AppendResults("... Found");
                }

                RunTestCommand(workspace, "");
                RunTestCommand(workspace, "%put \"testing\";");

                AppendResults("Closing workspace");
                workspace.Close();

                AppendResults("Clearing the object keeper");
                objectKeeper.RemoveAllObjects();

                workspace = null;
                AppendResults("...Success");
            }
            catch (Exception exc)
            {
                AppendException(exc);
            }
        }

        private void RunTestCommand(SAS.Workspace workspace, string command)
        {
            try
            {
                AppendResults(string.Format("Attempting to run command: {0}", command));
                Array carriageControls;
                Array lineTypeArray;
                Array logLineArray;

                AppendResults("... Submitting command");
                workspace.LanguageService.Submit(command);

                // These calls need to be made because they cause SAS to initialize internal structures that
                // are used when FlushLogLines is called.  Even though we're not really doing anything with these
                // values, don't remove these calls.
                AppendResults("... Initialization workaround");
                SAS.LanguageServiceCarriageControl carriageControlTemp = new SAS.LanguageServiceCarriageControl();
                var tmp = carriageControlTemp.ToString();
                var ccNormal = SAS.LanguageServiceCarriageControl.LanguageServiceCarriageControlNormal;
                ccNormal.ToString();
                SAS.LanguageServiceLineType lineTypeTemp = new SAS.LanguageServiceLineType();
                tmp = lineTypeTemp.ToString();
                var ltNormal = SAS.LanguageServiceLineType.LanguageServiceLineTypeNormal;
                ltNormal.ToString();

                // For all of the lines that we got back from SAS, we want to find those that are of the Normal type (meaning they
                // would contain some type of result/output), and that aren't empty.  Filtering empty lines is done because SAS
                // will dump out a bunch of extra output when we run, including blank Normal lines.
                var relevantLines = new List<string>();
                do
                {
                    AppendResults("... FlushLogLines from LanguageService");
                    workspace.LanguageService.FlushLogLines(1000, out carriageControls, out lineTypeArray, out logLineArray);
                    for (int index = 0; index < logLineArray.GetLength(0); index++)
                    {
                        var lineType = (SAS.LanguageServiceLineType)lineTypeArray.GetValue(index);
                        var line = (string)logLineArray.GetValue(index);
                        if (lineType == SAS.LanguageServiceLineType.LanguageServiceLineTypeNormal
                            && !string.IsNullOrWhiteSpace(line))
                        {
                            relevantLines.Add(line);
                        }
                    }

                }
                while (logLineArray != null && logLineArray.Length > 0);

                AppendResults(".... Finished retrieving log results");
                AppendResults(string.Join("\r\n", relevantLines));
            }
            catch (Exception exc)
            {
                AppendException(exc);
            }
        }

        private void cmdCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(txtResults.Text);
            lblCopied.Visible = true;

            var timer = new Timer();
            timer.Interval = 3000;
            timer.Tick += (source, te) => { lblCopied.Visible = false; timer.Stop(); };
            timer.Start();
        }
    }
}
