/*
 *  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 */

using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Extensibility;
using Microsoft.Office.Core;
using NoteHighlightAddin.Utilities;
using Application = Microsoft.Office.Interop.OneNote.Application;  // Conflicts with System.Windows.Forms
using System.Reflection;
using System.Drawing;
using Microsoft.Office.Interop.OneNote;
using NoteHighLightForm;
using System.Text;
using System.Linq;
using Helper;
using System.Threading;
using System.Web;
using GenerateHighlightContent;
using System.Configuration;
using System.Globalization;
using Infrastructure.Core;

#pragma warning disable CS3003 // Type is not CLS-compliant

namespace NoteHighlightAddin
{
	[ComVisible(true)]
	[Guid("4C6B0362-F139-417F-9661-3663C268B9E9"), ProgId("NoteHighlight2016.AddIn")]

	public class AddIn : IDTExtensibility2, IRibbonExtensibility
	{
		protected Application OneNoteApplication
		{ get; set; }

        public XNamespace ns;

        private MainForm mainForm;

        private HtmlInserter htmlInserter;

        private OneNoteService oneNoteService;

        string tag;

        private bool QuickStyle { get; set; }

        private bool DarkMode { get; set; }

        public AddIn()
		{
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

                // Mensajes te prueba - ya se soluciono - los voy a comentar 
                /*
                MessageBox.Show("Root: " + PathManager.Root);
                MessageBox.Show("Ribbon: " + PathManager.Ribbon);
                MessageBox.Show("Exist: " + File.Exists(PathManager.Ribbon));
                */

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
			SetOneNoteApplication((Application)Application);
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
                tag = control.Tag;

                Thread t = new Thread(new ThreadStart(ShowForm));
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception from AddInButtonClicked: "+ e.ToString());
            }

            //t.Join(5000);

            //ShowForm();
        }

        private void ShowForm()
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
                }

                MainForm form = new MainForm(tag, outFileName, selectedText, this.QuickStyle, this.DarkMode);

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

                    var page = InsertHighLightCode(htmlContent, position, parameters, outline, (new GenerateHighLight()).Config, selectedTextFormated, OneNoteHelper.IsSelectedTextInline(pageXml,ns));
                    page.Root.SetAttributeValue("ID", existingPageId);

                    //Bug fix - remove overflow value for Indents
                    foreach (var el in page.Descendants(ns + "Indent").Where(n => double.Parse(n.Attribute("indent").Value, new CultureInfo(page.Root.Attribute("lang").Value)) > 1000000))
                    {
                        el.Attribute("indent").Value = "0";
                    }

                    // Updated to use oneNoteServices 
                    oneNoteService.UpdatePageContent(page);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception from InsertHighLightCodeToCurrentSide: "+e.ToString());
            }
        }

        XElement GetPageNode()
        {
            string notebookXml;
            try
            {
                OneNoteApplication.GetHierarchy(null, HierarchyScope.hsPages, out notebookXml, XMLSchema.xs2013);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception from onApp.GetHierarchy:" + ex.Message);
                return null; ;
            }

            var doc = XDocument.Parse(notebookXml);
            ns = doc.Root.Name.Namespace;

            var pageNode = doc.Descendants(ns + "Page")
                              .Where(n => n.Attribute("isCurrentlyViewed") != null && n.Attribute("isCurrentlyViewed").Value == "true")
                              .FirstOrDefault();
            return pageNode;
        }

        /// <summary>
        /// 取得滑鼠所在的點
        /// Get Mouse Point
        /// </summary>
       
        private string GetPageXml(string pageID)
        {
            string pageXml;
            OneNoteApplication.GetPageContent(pageID, out pageXml, PageInfo.piSelection);

            return pageXml;
        }

        public string GetSelectedText(string pageXml, out bool selectedTextFormated)
        {
            return OneNoteHelper.GetSelectedText(pageXml, ns, out selectedTextFormated);
        }

        /// <summary>
        /// 產生 XML 插入至 OneNote
        /// Generate XML Insert To OneNote
        /// </summary>
        public XDocument InsertHighLightCode(string htmlContent, string[] position, HighLightParameter parameters, XElement outline, HighLightSection config, bool selectedTextFormated, bool isInline)
        {
            XElement children = PrepareFormatedContent(htmlContent, parameters, config, isInline);

            bool update = false;
            // this has been updated to include htmlinserter
            if (outline == null)
            {
                outline = htmlInserter.CreateOutline(position, children);
            }
            else // Update exiting outline
            {
                update = true;

                //Change outline width
                outline.Element(ns + "Size").Attribute("width").Value = "1600";

                if (selectedTextFormated)
                {
                    outline.Descendants(ns + "Table").Where(n => n.Attribute("selected") != null &&
                                        (n.Attribute("selected").Value == "all" || n.Attribute("selected").Value == "partial")).FirstOrDefault().ReplaceWith(children.Descendants(ns + "Table").FirstOrDefault());
                    //outline.Descendants().Where(n => n.Attribute("selected") != null && n.Attribute("selected").Value == "all").Remove();
                }
                else
                {
                    if (isInline)
                    {
                        int j = 0;
                        for(int i = 0; i < outline.Descendants(ns + "OE").Count(); i++)
                        {
                            XElement oeNode = outline.Descendants(ns + "OE").ElementAt(i);

                            if (oeNode.Descendants(ns + "T").Where(n => n.Attribute("selected") != null && n.Attribute("selected").Value == "all").Count() > 0)
                            {
                                oeNode.Descendants(ns + "T").Where(n => n.Attribute("selected") != null && n.Attribute("selected").Value == "all").FirstOrDefault().ReplaceWith(children.Descendants(ns + "Table").Descendants(ns + "OEChildren").Descendants(ns + "OE").ElementAt(j).Descendants(ns + "T"));
                                j++;
                            }

                        }
                        outline.Descendants(ns + "OE").Where(t => t.Elements(ns + "T").Any(n => n.Attribute("selected") != null && n.Attribute("selected").Value == "all")).Remove();
                    }
                    else
                    {
                        outline.Descendants(ns + "T").Where(n => n.Attribute("selected") != null && n.Attribute("selected").Value == "all").FirstOrDefault().ReplaceWith(children.Descendants(ns + "Table").FirstOrDefault());
                        outline.Descendants(ns + "OE").Where(t => t.Elements(ns + "T").Any(n => n.Attribute("selected") != null && n.Attribute("selected").Value == "all")).Remove();
                        outline.Descendants(ns + "OEChildren").Where(n => n.HasElements == false && n.Attribute("selected") != null && (n.Attribute("selected").Value == "partial")).Remove();
                    }
                }
            }

            if (update)
            {
                return outline.Parent.Document;
            }
            else
            {
                XElement page = new XElement(ns + "Page");
                page.Add(outline);

                XDocument doc = new XDocument();
                doc.Add(page);
                return doc;
            }


        }

        // Added as reference to the bridge
        private void EnsureHtmlInserter()
        {
            if (htmlInserter == null)
            {
                htmlInserter = new HtmlInserter(ns);
            }
        }

        // updated function to work as a bridge for UnitTesting
        public XElement CreateOutline(string[] position, XElement children)
        {
            EnsureHtmlInserter();

            return htmlInserter.CreateOutline(position, children);
        }

        // converted PrepareFormatedContent to Bridge 

        public XElement PrepareFormatedContent(
    string htmlContent,
    HighLightParameter parameters,
    HighLightSection config,
    bool isInline)
        {
            EnsureHtmlInserter();

            return htmlInserter.PrepareFormatedContent(
                htmlContent,
                parameters,
                config,
                isInline,
                DarkMode,
                NoteHighlightForm.Properties.Settings.Default.ShowTableBorder);
        }

        // Updated bridge }

        public bool ContainsAsianCharacter(string itemLine)
        {
            EnsureHtmlInserter();

            return htmlInserter.ContainsAsianCharacter(itemLine);
        }
    }
}
