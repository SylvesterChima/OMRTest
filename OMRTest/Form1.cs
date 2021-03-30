using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Configuration;
using System.Collections;
using System.Web;
using System.IO;
using ImageGear.Recognition;
using ImageGear.Processing;
using ImageGear.Core;
using ImageGear.Formats;
using System.Threading;
using ImageGear.Evaluation;
using System.Drawing.Imaging;
using OMR;

namespace OMRTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ProcessAttendanceFiles();
        }

        private int UpdateStatusPanel(int total, bool justProcessedFile, string MessageToShow)
        {
            int retval = total;
            try
            {
                PrimaryTotal.Text = total.ToString();
                if (PrimaryTotal.Text == String.Empty || PrimaryTotal.Text == "0")
                    PrimaryTotal.Text = total.ToString();
                else
                    retval = Convert.ToInt32(PrimaryTotal.Text);

                if (justProcessedFile)
                {
                    //increase the rows processed number
                    PrimaryValue.Text = ((int)Convert.ToInt32(PrimaryValue.Text) + 1).ToString();
                    if (retval > 0) { PrimaryPercent.Text = ((Convert.ToInt32(PrimaryValue.Text) * 100) / retval).ToString(); }
                    double percentcomplete = Convert.ToDouble(PrimaryPercent.Text);
                    percentcomplete = percentcomplete * 4;
                    //ProgressBarImage.Width = System.Web.UI.WebControls.Unit.Pixel(Convert.ToInt32(System.Math.Floor(percentcomplete)));
                }

                CurrentOperation.Text = MessageToShow;

                if (PrimaryTotal.Text == PrimaryValue.Text)
                {
                    CurrentOperation.Text = "Process Finished!";
                }
            }
            catch
            {
                // we throw away this error because it is not important.  This is just showing the user the process.
            }

            return retval;
        }
        public void ProcessAttendanceFiles()
        {
            try
            {
                // Create a lock file to make sure no one else runs this.
                FileInfo f = new FileInfo("C:\\AttendanceRoot" + "\\AttendanceLock\\"  + "femi.lock");
                FileStream fs = f.Create();
                fs.Close();

                DirectoryInfo di = new DirectoryInfo("C:\\AttendanceRoot" + "\\input");
                FileInfo[] fi = di.GetFiles("*.tif");

                if (fi.Length > 0)
                {
                     int total = UpdateStatusPanel(fi.Length, false, "Processing...");

                    //ProgressBarImage.Width = System.Web.UI.WebControls.Unit.Pixel(0);

                    ImGearEvaluationManager.Mode = ImGearEvaluationMode.Watermark;
                    ImGearEvaluationManager.Initialize();
                    ImGearCommonFormats.Initialize();
                    //TODO:  This is a to make the foreach only run 25 times.
                    // it is not the best way to do this.  We need to make it process about 20 files per post
                    // I am not sure of the best way to chop off the array right now.   
                    if (fi.Length >= 10)
                    {
                        System.Array.Resize<FileInfo>(ref fi, 10);
                    }
                    foreach (FileInfo attendanceFile in fi)
                    {
                        //log.Info(attendanceFile.Name);
                        //make sure webparts have admin valid user
                        //if (user == null)
                        //{
                        //    user = iMISAddins.Class.Security.ReAuthiMISUser(Membership.GetUser().UserName);
                        //    Session["LoginUser"] = user;
                        //}


                        // process the input file
                        ProcessAndArchiveFile(attendanceFile);

                        //overwrite the lock file so we can see the date modified changing to see if it is still alive.
                        FileStream fs2 = f.Create();
                        fs2.Close();


                        UpdateStatusPanel(total, true, "Completed File: " + attendanceFile.Name);
                    }
                }
                else
                {
                    //timer1.Enabled = false;
                    msg.Text = "No more files to process at this time.";
                }

                // Delete the lock file so that this process can be started again.
                f.Delete();
            }
            catch (Exception ex)
            {
                //log.Error(ex);
            }
        }

        private void ProcessAndArchiveFile(FileInfo attendanceFile)
        {
            // process the file...
            ImGearRecognition rec = null;		// Represents recognition engine
            ImGearRecZone imGearRecZone;
            ImGearRecPage recPage = null;
            ImGearPage page;
            int? RosterNum = null, RosterMeetingId = null, GroupMeetingId = null, GroupId = null;
            DateTime? attDate = null;
            StringBuilder sbIdsInAttendance = new StringBuilder();
            bool DidFileSuccessfullyProcess = true;
            int rowcount = 0;
            //log.Info("File content needs to be open");
            // Open the input file
            using (FileStream content = attendanceFile.OpenRead())
            {
                try
                {
                    #region Preprocess and load the File
                    // open the content to see the number of pages



                    ImGearDocument tempDocument = null;
                    //log.Info(content);

                    tempDocument = ImGearFileFormats.LoadDocument(content);

                    int numPages = tempDocument.Pages.Count;
                    if (numPages > 1)
                    {
                        throw new ApplicationException("File has more than one page in it.");
                    }
                    // load the page from the filestream
                    page = tempDocument.Pages[0]; //ImGearFileFormats.LoadPage(content, 0);

                    // load the recognition engine
                    rec = new ImGearRecognition("C:\\Users\\Dev10\\Source\\Repos\\OMRTest\\OMRTest\\bin\\Debug");

                    #endregion


                    //set the recognition defaults and tell it to look for numbers.  It will reduce errors
                    rec.Recognition.DefaultFillingMethod = ImGearRecFillingMethod.OCRA;
                    rec.Recognition.DefaultFilter = ImGearRecFilter.NUMBERS;

                    // set the recognition seetings for the OMR.
                    ImGearRecMORSettings OmrSettings = rec.Modules.MOR;
                    // OmrSettings.FrameDetectionMode = ImGearRecFrameDetectionMode.AUTO;
                    // Use this line instead of the one above for making if find the box.  (black lines on the boxes)
                    //OmrSettings.FrameDetectionMode = ImGearRecFrameDetectionMode.YES;
                    //OmrSettings.Sensitivity = ImGearRecMarkSensitivity.NORMAL;

                    // Import the page into the recognition engine
                    recPage = rec.ImportPage((ImGearRasterPage)page);

                    // Make sure the page has acceptable resolution information
                    if (recPage.Image.XRes != 300 || recPage.Image.YRes != 300) { throw new ApplicationException("Invaild DPI.on: " + attendanceFile.Name); }

                    // Deskew the image and preprocess it
                    rec.Preprocessing.OrientationMode = ImGearRecOrientationMode.DOWN;
                    rec.Preprocessing.DeskewMode = ImGearRecDeskewMode.AUTO;
                    int slope;
                    ImGearRecOrientationMode orient;
                    recPage.Image.DetectSkew(out slope, out orient);
                    recPage.Image.Orient(orient);
                    recPage.Image.Deskew(slope);
                    recPage.Image.Despeckle();

                    // load the zones
                    recPage.Zones.LoadFromFile("C:\\AttendanceRoot" + "\\zones\\ocra.zon");
                    //log.Info("ImageGear: start implementation");
                    // Perform recognition, appending to the recognized data file
                    recPage.Recognize();

                    // We need to read out the values that were processed
                    ImGearRecLetter[] imGearRecLetters = recPage.GetLetters();
                    ArrayList alPersonIds = new ArrayList();
                    System.Text.StringBuilder sbresult = new System.Text.StringBuilder();

                    // Clear Zones used for Barcode and Setup OMR Zones
                    recPage.Zones.Clear();

                    foreach (ImGearRecLetter letter in imGearRecLetters)
                    {
                        sbresult.Append(letter.Code);

                        if ((letter.Makeup & ImGearRecMakeupInfo.ENDOFWORD) != 0)
                        {
                            //The End of a Barcode has been found
                            // so now we can figure out what it is and what to do with it.
                            if (letter.ZoneIndex == 2 || letter.ZoneIndex == 3)
                            {
                                //Setup an OMR Zone based on the Barcode zone 
                                imGearRecZone = new ImGearRecZone(ImGearRecFillingMethod.OMNIFONT, ImGearRecRecognitionModule.OMNIFONT_MOR);
                                //Define the new zone to be 100 pixels wide and 50 pixel hi from the top right edge of the barcode.
                                // Sometimes the x axis paratmer is a little off ,so we will use a certain number for the left parameter.
                                // The y parameter is the one we really care about.
                                int boxWidth = 78;
                                int topOffset = 20;
                                int boxHeight = 68;

                                int goleft = 0;

                                if (letter.ZoneIndex == 2)
                                {
                                    if (letter.Confidence >= 83 && (sbresult.Replace(".", "").Replace(",", "").Length > 3))
                                    {
                                        rowcount++;
                                    }
                                    goleft = 935;
                                    goleft = letter.Rect.Right - goleft;

                                    // we need to make sure the box is on the page.
                                    if (goleft < 0) { goleft = 0; }
                                }
                                else if (letter.ZoneIndex == 3)
                                {
                                    goleft = 935;
                                    goleft = letter.Rect.Right - goleft;
                                }
                                //imGearRecZone.Rect = new ImGearRectangle(goleft, letter.Rect.Top - topOffset, boxWidth, boxHeight);
                                recPage.Zones.Add(imGearRecZone);

                                // Add Barcode Results to the Array of Data
                                if (letter.Confidence >= 83 && (sbresult.Replace(".", "").Replace(",", "").Length > 3))
                                {
                                    alPersonIds.Add(sbresult.Replace(".", "").Replace(",", ""));
                                }
                                sbresult = new System.Text.StringBuilder();
                            }
                            else if (letter.ZoneIndex == 0) // this is the header
                            {
                                //  The date will be before the 2000 pixel point
                                if (letter.Rect.Left < 2000)
                                {
                                    // fix the special charactersby cruising the string
                                    // substring was not used here because on purpose.
                                    // REASON:  If leading zeros were ever removed, it could break things
                                    string d = String.Empty;
                                    foreach (char c in sbresult.ToString().Replace(".", "").Replace(",", "").ToCharArray())
                                    {
                                        if (char.IsDigit(c))
                                        { d = d + c; }
                                        else
                                        { d = d + "/"; }
                                    }

                                    d = d.Replace("//", "/");

                                    // ignore the error.  Due to the scan we might have gotten the words above the date.  Also we check later to make sure we have a date and roster id.
                                    try
                                    {
                                        DateTime a = Convert.ToDateTime(d.Substring(0, 1) == "/" ? d.Remove(0, 1) : d);
                                        attDate = a;
                                    }
                                    catch { }
                                }
                                else
                                {
                                    // ignore the error.  Due to the scan we might have gotten the words above the date.  Also we check later to make sure we have a date and roster id.                                        
                                    try
                                    {
                                        int b = Convert.ToInt32(sbresult.ToString().Replace(".", "").Replace(",", ""));
                                        RosterNum = b;
                                    }
                                    catch { }
                                }

                                //Clear the string builder
                                sbresult = new System.Text.StringBuilder();
                            }
                            else if (letter.ZoneIndex == 1) // this is the footer
                            {
                                //temp variable
                                int c = 0;

                                //  The date will be before the 2000 pixel point
                                if (letter.Rect.Left < 600)
                                {
                                    try
                                    {
                                        c = Convert.ToInt32(sbresult.ToString().Replace(".", ""));
                                        RosterMeetingId = c;
                                    }
                                    catch { }
                                }
                                else if (letter.Rect.Left < 1700)
                                {
                                    try
                                    {
                                        c = Convert.ToInt32(sbresult.ToString().Replace(".", ""));
                                        GroupId = c;
                                    }
                                    catch { }
                                }
                                else
                                {
                                    try
                                    {
                                        c = Convert.ToInt32(sbresult.ToString().Replace(".", ""));
                                        GroupMeetingId = c;
                                    }
                                    catch { }
                                }

                                //Clear the string builder
                                sbresult = new System.Text.StringBuilder();
                            }
                        }
                    }
                    //log.Info("ImageGer: load all person Ids in the image");
                    // make sure we have a date and a roster number or throw
                    if (!(RosterNum.HasValue)) { throw new ApplicationException("No Roster Number picked up on: " + attendanceFile.Name); }
                    if (!(attDate.HasValue)) { throw new ApplicationException("No Date picked up on: " + attendanceFile.Name); }

                    if (RosterMeetingId == 0) { RosterMeetingId = null; }
                    if (GroupId == 0) { GroupId = null; }
                    if (GroupMeetingId == 0) { GroupMeetingId = null; }
                    //log.Info("ImageGear: end implementation");
                    ArrayList alIMISIdsInAttendance = new ArrayList();
                    recPage.Dispose();
                    #region OMR Implementation

                    //log.Info("FormFix: OMR initialize");
                    string OMRResultsStringForEmail = "<br><br>Results:<br>Id - Value - (Confidence)<br>";
                    try
                    {
                        ArrayList firstIDs; ArrayList secondIDs;
                        if (alPersonIds.Count > rowcount)
                        {
                            firstIDs = alPersonIds.GetRange(0, rowcount);
                            secondIDs = alPersonIds.GetRange(rowcount, alPersonIds.Count - rowcount);
                        }
                        else
                        {
                            firstIDs = alPersonIds;
                            secondIDs = new ArrayList();
                        }
                        //log.Info(attendanceFile.Name + " - rowcount is " + rowcount);

                        var rectF = new RectangleF[(alPersonIds.Count > rowcount) ? 2 : 1];
                        var rate = 2626 / 22; var height = rate * rowcount;
                        rectF[0] = new RectangleF(95, 425, 81, height);
                        if (alPersonIds.Count > rowcount)
                        {
                            height = rate * secondIDs.Count;
                            rectF[1] = new RectangleF(1394, 425, 81, height); // TODO change height
                        }

                        var IDs = new List<string>[(alPersonIds.Count > rowcount) ? 2 : 1];
                        IDs[0] = firstIDs.Cast<object>().Select(x => x.ToString()).ToList();
                        if (alPersonIds.Count > rowcount)
                            IDs[1] = secondIDs.Cast<object>().Select(x => x.ToString()).ToList();
                        var LineNumbers = new int[(alPersonIds.Count > rowcount) ? 2 : 1];
                        LineNumbers[0] = firstIDs.Count;
                        if (alPersonIds.Count > rowcount)
                            LineNumbers[1] = secondIDs.Count;
                        System.Drawing.Rectangle Rect = new Rectangle(9, 420, 200, height);
                        System.Drawing.Image _img = System.Drawing.Image.FromStream(content);
                        OMR.OMREngine engine = new OMREngine(_img);
                        //engine = new OMREngine(filenamet.Text, sheetAddTB.Text, sheetNameTB.Text, akTitleTB.Text);
                        //engine.inDepthFeedBack = false;
                        engine.rects = rectF;
                        engine.numberofLines = LineNumbers;
                        engine.sBound = new Size(_img.Width, _img.Height);
                        engine.attendances = IDs;
                        var omrResult = engine.StartProcessOMR();
                        //log.Warn("Attended Record is " + omrResult.Count);
                        if (omrResult.Count == 0)
                        {
                            rectF = new RectangleF[(alPersonIds.Count > rowcount) ? 2 : 1];
                            rectF[0] = new RectangleF(95, 425, 81, height);
                            if (alPersonIds.Count > rowcount)
                            {
                                rectF[1] = new RectangleF(1394, 425, 81, height); // TODO change height
                            }

                            engine.rects = rectF;
                            omrResult = engine.StartProcessOMR();
                        }

                        for (int i = 0; i < omrResult.Count; i++)
                        {
                            alIMISIdsInAttendance.Add(omrResult[i]);
                            OMRResultsStringForEmail = OMRResultsStringForEmail + omrResult[i] + " - 1 (100)<br>";
                        }



                        //log.Info(OMRResultsStringForEmail.Replace("<br>", ""));
                        //log.Info("OMR: end implementation");

                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    #endregion


                    // we want to make sure that we have people there before continuing                     
                    if (alIMISIdsInAttendance.Count > 0 && RosterNum.HasValue)
                    {
                        //We need to get the meeting time based on the rostermeetingid or the groupmeetingid
                        if (RosterMeetingId.HasValue)
                        {
                            //Roster.RosterMeeting rm = Roster.RosterMeeting.FromID(RosterMeetingId.Value);
                            //if (rm != null)
                            //{
                            //    attDate = Convert.ToDateTime(attDate.Value.ToShortDateString() + " " + rm.Time);
                            //}
                        }
                        else if (GroupMeetingId.HasValue)
                        {
                            //GroupMeeting gm = GroupMeeting.FromID(GroupMeetingId.Value);
                            //if (gm != null)
                            //{
                            //    attDate = Convert.ToDateTime(attDate.Value.ToShortDateString() + " " + gm.Time);
                            //}
                        }

                        //Now cruise through array of those there to save the data.                   
                        foreach (object o in alIMISIdsInAttendance)
                        {
                            string iMISId = (o).ToString();

                            // if there is a spec on the page it may read as a decimal...we will remove it.
                            iMISId = iMISId.Replace(".", String.Empty);
                            iMISId = iMISId.Trim();
                            sbIdsInAttendance.Append(iMISId + ", ");
                            try
                            {
                                //Roster.RosterMemberAttendance.AddAttendance(iMISId, RosterNum.Value, attDate.Value, GroupId, RosterMeetingId, GroupMeetingId, (CStaffUser)user);
                                //log.Info($"Roster Attendance: {iMISId} attended roster {RosterNum.Value} on {attDate.Value}");
                            }
                            catch (Exception ex)
                            {
                                //log.Warn($"Invalid imis ID {iMISId} for roster {RosterNum.Value} on {attDate.Value}");
                            }
                        }
                        //log.Warn("Roster Attendance: Completed");
                    }

                    #region Email Confirmation File to user.
                    // Putting something in this label field turns off the emailing.
                    //if (emailmsg.Text == String.Empty)
                    //{
                    //    // remove the end comma on our string length
                    //    string s = String.Empty;
                    //    if (sbIdsInAttendance.Length >= 2)
                    //    {
                    //        s = "The following iMIS ids attended: ";
                    //        s += sbIdsInAttendance.Remove(sbIdsInAttendance.Length - 2, 2).ToString();
                    //        s += OMRResultsStringForEmail;
                    //    }

                    //    if (RosterNum.HasValue) { s = s + "<br><br>Roster Num: " + RosterNum.Value.ToString(); }
                    //    if (attDate.HasValue) { s = s + "<br>Date: " + attDate.Value.ToShortDateString() + " " + attDate.Value.ToShortTimeString(); }
                    //    if (RosterMeetingId.HasValue) { s = s + "<br>Roster Meeting: " + RosterMeetingId.Value.ToString(); }
                    //    if (GroupId.HasValue) { s = s + "<br>GroupId: " + GroupId.ToString(); }
                    //    if (GroupMeetingId.HasValue) { s = s + "<br>GroupMeetingId: " + GroupMeetingId.ToString(); }

                    //    var apiKey = ConfigurationManager.AppSettings["SGApiKey"];
                    //    var client = new SendGridClient(apiKey);
                    //    var email = ((CStaffUser)user).UserId + "@second.org";
                    //    var from = new EmailAddress(email, ((CStaffUser)user).UserName);
                    //    var to = new EmailAddress(email);
                    //    var sendGridMsg = MailHelper.CreateSingleEmail(from, to, "Attendance Confirmation", null, "<html><body>" + s + "</body></html>");
                    //    byte[] byteData = Encoding.ASCII.GetBytes(File.ReadAllText(attendanceFile.FullName));
                    //    sendGridMsg.AddAttachment(new SendGrid.Helpers.Mail.Attachment
                    //    {
                    //        Content = Convert.ToBase64String(byteData),
                    //        Disposition = "attachment",
                    //        Filename = attendanceFile.Name,
                    //        Type = MimeMapping.GetMimeMapping(attendanceFile.Name)
                    //    });
                    //    var response = client.SendEmailAsync(sendGridMsg).Result;

                    //    // if it is zero we only email the first file processed to the person running the app.
                    //    // we will put in a string to make this code not execute again.
                    //    if (ConfigurationManager.AppSettings["EmailSummaries"].ToString() == "0")

                    //        emailmsg.Text = "Emailed Confirmation File.";
                    //}
                    #endregion
                }
                catch (Exception ex)
                {
                    DidFileSuccessfullyProcess = false;
                    //log.Error(ex);
                    // update the unreadable files label
                    if (UnreadableFiles.Text == String.Empty) { UnreadableFiles.Text = "0"; }
                    UnreadableFiles.Text = ((int)Convert.ToInt32(UnreadableFiles.Text) + 1).ToString();

                    // we either continue processign or show the error depending on our run mode
                    //if (ConfigurationManager.AppSettings["StopOnError"].ToString() == "1")
                    //{
                    //    throw;
                    //}
                }
                finally
                {
                    // Free the page that was imported into the recognition engine
                    // this is throwing in certain cases so I am wrapping it.
                    try
                    {
                        rec.Dispose();

                    }
                    catch { }
                }
            }

            #region Archive the file
            try
            {
                string newloc = "";

                if (DidFileSuccessfullyProcess)
                { newloc = "C:\\AttendanceRoot" + "\\Processed\\" + attDate.Value.ToShortDateString().Replace("/", "-") + "\\" + RosterNum.Value.ToString() + "\\"; }
                else
                { newloc = "C:\\AttendanceRoot" + "\\Errors\\" + System.DateTime.Now.ToShortDateString().Replace("/", "-") + "\\"; }

                if (!(Directory.Exists(newloc)))
                {
                    Directory.CreateDirectory(newloc);
                }
                attendanceFile.MoveTo(newloc + attendanceFile.Name);
            }
            catch (System.Exception ex)
            {
                //log.Error(ex);
                if (DidFileSuccessfullyProcess)
                {
                    // since the above failed, just throw it in the processed, failed during move folder                
                    // we won't handle this error so processing will stop because we really can't do anything else.
                    string newloc = "C:\\AttendanceRoot" + "\\Processed\\FailedDuringFinalMove\\" + attendanceFile.Name;
                    attendanceFile.MoveTo(newloc);
                }
                else
                {
                    throw;
                }
            }
            #endregion
        }
    }
}
