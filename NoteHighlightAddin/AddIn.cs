/*
 *  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 */

using Extensibility;
using GenerateHighlightContent;
using Helper;
using Infrastructure.Core;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.OneNote;
using NoteHighlightAddin.Utilities;
using NoteHighLightForm;
using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using System.Xml.Linq;
using static NoteHighlightAddin.LanguageDetectionService;
using Application = Microsoft.Office.Interop.OneNote.Application;  // Conflicts with System.Windows.Forms

#pragma warning disable CS3003 // Type is not CLS-compliant

namespace NoteHighlightAddin
{
    [ComVisible(true)]
    [Guid("4C6B0362-F139-417F-9661-3663C268B9E9"), ProgId("NoteHighlight2016.AddIn")]

    public class AddIn : IDTExtensibility2, IRibbonExtensibility
    {

        public void ContextMenuTestClicked(IRibbonControl control)
        {
            try
            {
                LanguagePreferenceProvider
                    preferenceProvider =
                        new LanguagePreferenceProvider();

                string requestedLanguage =
                    preferenceProvider.ReadLanguageTag();

                if (string.IsNullOrWhiteSpace(
                    requestedLanguage))
                {
                    MessageBox.Show(
                        "No hay un lenguaje seleccionado previamente.",
                        "NoteHighlight+",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    return;
                }

                string resolvedLanguage =
                    LanguageDefinitionResolver.Resolve(
                        requestedLanguage);

                string language =
                    string.IsNullOrWhiteSpace(
                        resolvedLanguage)
                        ? requestedLanguage
                        : resolvedLanguage;

                Thread t =
                    new Thread(
                        () => ShowForm(language, true, true, false));

                t.SetApartmentState(
                    ApartmentState.STA);

                t.Start();
            }
            catch (Exception e)
            {
                MessageBox.Show(
                    "Exception from Quick Highlight: " +
                    e,
                    "NoteHighlight+",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        public void AutoDetectHighlightClicked(
    IRibbonControl control)
        {
            try
            {
                LanguagePreferenceProvider
                    preferenceProvider =
                        new LanguagePreferenceProvider();

                string requestedLanguage =
                    preferenceProvider.ReadLanguageTag();

                if (string.IsNullOrWhiteSpace(
                    requestedLanguage))
                {
                    MessageBox.Show(
                        "No hay un lenguaje seleccionado previamente.",
                        "NoteHighlight+",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    return;
                }

                string resolvedLanguage =
                    LanguageDefinitionResolver.Resolve(
                        requestedLanguage);

                string fallbackLanguage =
                    string.IsNullOrWhiteSpace(
                        resolvedLanguage)
                        ? requestedLanguage
                        : resolvedLanguage;

                Thread t =
                    new Thread(
                        () => ShowForm(
                            fallbackLanguage,
                            true,
                            true,
                            true));

                t.SetApartmentState(
                    ApartmentState.STA);

                t.Start();
            }
            catch (Exception e)
            {
                MessageBox.Show(
                    "Exception from Auto Detect Highlight: " +
                    e,
                    "NoteHighlight+",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        protected Application OneNoteApplication
        { get; set; }

        public XNamespace ns;

        private MainForm mainForm;

        private HtmlInserter htmlInserter;

        private OneNoteService oneNoteService;

        private bool QuickStyle { get; set; }

        private bool DarkMode { get; set; }

        public AddIn()
        {
            // prueba para el instalador - saber donde estan lso dll

            ComStartupLogger.Write("AddIn.ctor ENTER");

            try
            {
                ComStartupLogger.Write(
                    "Assembly = " +
                    typeof(AddIn).Assembly.FullName);

                ComStartupLogger.Write(
                    "Location = " +
                    typeof(AddIn).Assembly.Location);

                ComStartupLogger.Write(
                    "BaseDirectory = " +
                    AppDomain.CurrentDomain.BaseDirectory);

                ComStartupLogger.Write(
                    "64-bit process = " +
                    Environment.Is64BitProcess);

                // AQUÍ permanece el código actual del constructor.

                ComStartupLogger.Write("AddIn.ctor EXIT");
            }
            catch (Exception ex)
            {
                ComStartupLogger.WriteException(
                    "AddIn.ctor",
                    ex);

                throw;
            }

            // fin de la prueba 

        }

        // Added as reference since UnitTesting was still calling for this method and it was not available in the OneNoteService class.  This is a temporary solution until the UnitTests are updated to use the OneNoteService class.
        public bool IsSelectedTextInline(string pageXml)
        {
            return OneNoteHelper.IsSelectedTextInline(pageXml, ns);
        }

        /// <summary>
        /// Returns the XML in Ribbon.xml so OneNote knows how to render our ribbon
        /// </summary>
        /// <param name="RibbonID"></param>
        /// <returns></returns>
        public string GetCustomUI(string RibbonID)
        {
            return LoadRibbon();

        }

        private string LoadRibbon()
        {
            try
            {

                //    var workingDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ribbon.xml");

                string file = File.ReadAllText(PathManager.Ribbon);

                // MessageBox.Show("Ribbon Leido Correctamente");

                return file;

            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                throw;
            }



        }

        public void OnAddInsUpdate(ref Array custom)
        {
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        /// <param name="custom"></param>
        public void OnBeginShutdown(ref Array custom)
        {
            this.mainForm?.Invoke(new Action(() =>
            {
                // close the form on the forms thread
                this.mainForm?.Close();
                this.mainForm = null;
            }));
        }

        /// <summary>
        /// Called upon startup.
        /// Keeps a reference to the current OneNote application object.
        /// </summary>
        /// <param name="application"></param>
        /// <param name="connectMode"></param>
        /// <param name="addInInst"></param>
        /// <param name="custom"></param>
        public void OnConnection(object Application, ext_ConnectMode ConnectMode, object AddInInst, ref Array custom)
        {
            // prueba dlls

            ComStartupLogger.Write("OnConnection ENTER");

            try
            {
                ComStartupLogger.Write(
                    "OnConnection - before SetOneNoteApplication");

                SetOneNoteApplication((Application)Application);

                ComStartupLogger.Write(
                    "OnConnection - after SetOneNoteApplication");

                ComStartupLogger.Write("OnConnection EXIT");
            }
            catch (Exception ex)
            {
                ComStartupLogger.WriteException(
                    "OnConnection",
                    ex);

                throw;
            }
        }


        // Updated to use OneNoteApplication from OneNoteService
        public void SetOneNoteApplication(Application application)
        {
            OneNoteApplication = application;
            oneNoteService = new OneNoteService(application);

        }

        /// <summary>
        /// Cleanup
        /// </summary>
        /// <param name="RemoveMode"></param>
        /// <param name="custom"></param>
        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.GC.Collect")]

        // Update for cleaning 
        public void OnDisconnection(ext_DisconnectMode RemoveMode, ref Array custom)
        {
            oneNoteService = null;
            OneNoteApplication = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public void OnStartupComplete(ref Array custom)
        {
        }

        public bool cbQuickStyle_GetPressed(IRibbonControl control)
        {
            this.QuickStyle = NoteHighlightForm.Properties.Settings.Default.QuickStyle;
            return this.QuickStyle;
        }

        public void cbQuickStyle_OnAction(IRibbonControl control, bool isPressed)
        {
            this.QuickStyle = isPressed;
            NoteHighlightForm.Properties.Settings.Default.QuickStyle = this.QuickStyle;
            NoteHighlightForm.Properties.Settings.Default.Save();
        }


        public bool cbDarkMode_GetPressed(IRibbonControl control)
        {
            this.DarkMode = NoteHighlightForm.Properties.Settings.Default.DarkMode;
            return this.DarkMode;
        }

        public void cbDarkMOde_OnAction(IRibbonControl control, bool isPressed)
        {
            this.DarkMode = isPressed;
            NoteHighlightForm.Properties.Settings.Default.DarkMode = this.DarkMode;
            NoteHighlightForm.Properties.Settings.Default.Save();
        }

        //public async Task AddInButtonClicked(IRibbonControl control)
        public void AddInButtonClicked(IRibbonControl control)
        {
            try
            {
                string requestedLanguage = control.Tag;

                string resolvedLanguage =
                    LanguageDefinitionResolver.Resolve(
                        requestedLanguage);

                string language =
                    string.IsNullOrWhiteSpace(
                        resolvedLanguage)
                        ? requestedLanguage
                        : resolvedLanguage;

                bool quickStyle =
                    NoteHighlightForm.Properties
                        .Settings.Default.QuickStyle;

                Thread t =
                    new Thread(
                        () => ShowForm(language, quickStyle, false, false));

                t.SetApartmentState(
                    ApartmentState.STA);

                t.Start();
            }
            catch (Exception e)
            {
                MessageBox.Show(
                    "Exception from AddInButtonClicked: " +
                    e);
            }
        }

        private void ShowForm(string language, bool quickStyle, bool requireSelection, bool autoDetectLanguage)
        {
            try
            {
                string outFileName = Guid.NewGuid().ToString();

                //try
                //{
                //ProcessHelper processHelper = new ProcessHelper("NoteHighLightForm.exe", new string[] { control.Tag, outFileName });
                //processHelper.IsWaitForInputIdle = true;
                //processHelper.ProcessStart();

                //CodeForm form = new CodeForm(tag, outFileName);
                //form.ShowDialog();

                //TestForm t = new TestForm();

                //Updated changed pageMode to ensure there is a page and avoid a null
                var pageNode = oneNoteService.GetCurrentPageNode();

                if (pageNode == null)
                {
                    MessageBox.Show(
                        "No se pudo encontrar la página activa de OneNote.");

                    return;
                }

                ns = oneNoteService.Namespace;
                htmlInserter = new HtmlInserter(ns);

                string pageId = pageNode.Attribute("ID")?.Value;

                if (string.IsNullOrWhiteSpace(pageId))
                {
                    MessageBox.Show(
                        "La página activa no contiene un identificador válido.");

                    return;
                }

                string pageXml = oneNoteService.GetPageXml(pageId);
                // end update 
                string selectedText = "";
                XElement outline = null;
                bool selectedTextFormated = false;

                if (pageNode != null)
                {
                    selectedText = OneNoteHelper.GetSelectedText(pageXml, ns, out selectedTextFormated);

                    if (selectedText.Trim() != "")
                    {
                        outline = OneNoteHelper.GetOutline(pageXml, ns);
                    }

                    if (requireSelection && string.IsNullOrWhiteSpace( selectedText))
                    {
                        MessageBox.Show(
                            "Selecciona el código que deseas resaltar.",
                            "NoteHighlight+",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        return;
                    }

                    if (autoDetectLanguage)
                    {
                        LanguageDetectionService detector =
                            new LanguageDetectionService();

                        LanguageDetectionResult detection =
                            detector.DetectDetailed(selectedText);

                        if (detection.Confidence ==
                                LanguageDetectionConfidence.High &&
                            !string.IsNullOrWhiteSpace(
                                detection.Language))
                        {
                            language = detection.Language;
                        }
                    }

                }

                MainForm form = new MainForm(language, outFileName,selectedText, quickStyle, this.DarkMode);

                System.Windows.Forms.Application.Run(form);
                //}
                //catch (Exception ex)
                //{
                //    MessageBox.Show("Error executing NoteHighLightForm.exe：" + ex.Message);
                //    return;
                //}

                string fileName = Path.Combine(Path.GetTempPath(), outFileName + ".html");

                if (File.Exists(fileName))
                {
                    InsertHighLightCodeToCurrentSide(fileName, pageXml, form.Parameters, outline, selectedTextFormated);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception from ShowForm: " + e.ToString());
            }
        }

        public void SettingsButtonClicked(IRibbonControl control)
        {
            try
            {

                Thread t = new Thread(new ThreadStart(ShowSettingsForm));
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception from SettingsButtonClicked: " + e.ToString());
            }
        }

        private void ShowSettingsForm()
        {
            try
            {

                SettingsForm form = new SettingsForm();

                System.Windows.Forms.Application.Run(form);

            }
            catch (Exception e)
            {
                MessageBox.Show("Exception from ShowForm: " + e.ToString());
            }
        }

        /// <summary>
        /// Specified in Ribbon.xml, this method returns the image to display on the ribbon button
        /// </summary>
        /// <param name="imageName"></param>
        /// <returns></returns>
        public IStream GetImage(string imageName)
        {
            MemoryStream imageStream = new MemoryStream();
            //switch (imageName)
            //{
            //    case "CSharp.png":
            //        Properties.Resources.CSharp.Save(imageStream, ImageFormat.Png);
            //        break;
            //    default:
            //        Properties.Resources.Logo.Save(imageStream, ImageFormat.Png);
            //        break;
            //}

            BindingFlags flags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            var b = typeof(Properties.Resources).GetProperty(imageName.Substring(0, imageName.IndexOf('.')), flags).GetValue(null, null) as Bitmap;
            b.Save(imageStream, ImageFormat.Png);

            return new CCOMStreamWrapper(imageStream);
        }

        /// <summary>
        /// 插入 HighLight Code 至滑鼠游標的位置
        /// Insert HighLight Code To Mouse Position  
        /// </summary>
        private void InsertHighLightCodeToCurrentSide(string fileName, string pageXml, HighLightParameter parameters, XElement outline, bool selectedTextFormated)
        {
            try
            {
                // Trace.TraceInformation(System.Reflection.MethodBase.GetCurrentMethod().Name);
                string htmlContent = File.ReadAllText(fileName, new UTF8Encoding(false));

                string byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
                htmlContent = htmlContent.Replace(byteOrderMarkUtf8, "");

                // Updated to use oneNoteService logic
                var pageNode = oneNoteService.GetCurrentPageNode();
                ns = oneNoteService.Namespace;
                htmlInserter = new HtmlInserter(ns);

                if (pageNode != null)
                {
                    var existingPageId = pageNode.Attribute("ID").Value;
                    string[] position = null;
                    if (outline == null)
                    {
                        position = OneNoteHelper.GetMousePointPosition(pageXml, ns);
                    }

                    var highlightGenerator = new GenerateHighLight();

                    var page = htmlInserter.InsertHighLightCode(
                        htmlContent,
                        position,
                        parameters,
                        outline,
                        highlightGenerator.Config,
                        selectedTextFormated,
                        OneNoteHelper.IsSelectedTextInline(pageXml, ns),
                        DarkMode,
                        NoteHighlightForm.Properties.Settings.Default.ShowTableBorder);
                    page.Root.SetAttributeValue("ID", existingPageId);

                    // Updated to use oneNoteServices 
                    oneNoteService.UpdatePageContent(page);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception from InsertHighLightCodeToCurrentSide: " + e.ToString());
            }
        }
    }
}
