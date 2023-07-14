using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using System.IO;

namespace EDWFileHandler
{
    class Program
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {


            string dir = "";

            StringBuilder mailBody = new StringBuilder();

            BusinessLogic businessLogic = new BusinessLogic();
            DataAccess dataAccess = new DataAccess();
            int processedStatus = -1;

            if (args.Length == 0)
            {
                Logger.Error("Could not initiate file load, input argument invalid.");
                return;
            }



            try
            {
                string processedDir = "";
                Logger.Info(String.Format("Initiating file load!"));
                PAIConst.FileTypeToLoad = FileType.DEF;
                switch (args[0].ToString())
                {
                    case "FTL":
                        {
                            dir = System.Configuration.ConfigurationManager.AppSettings["FTLPath"];
                            var dirInfo = new DirectoryInfo(dir);
                            List<FileInfo> filesInfo = dirInfo.GetFiles().OrderByDescending(x => x.LastWriteTime).ToList();
                            PAIConst.FileTypeToLoad = FileType.FTL;

                            foreach (FileInfo fileInfo in filesInfo)
                            {
                                dir = System.Configuration.ConfigurationManager.AppSettings["FTLPath"] + fileInfo.Name;
                                processedStatus = businessLogic.LoadFileAll(dir);

                                if (processedStatus > 0)//File is processed successfully
                                {
                                    processedDir = System.Configuration.ConfigurationManager.AppSettings["FTLPathProcessed"].ToString();
                                    if (Directory.Exists(processedDir))
                                    {
                                        processedDir = System.Configuration.ConfigurationManager.AppSettings["FTLPathProcessed"].ToString() + DateTime.Now.ToString("MMddyyyy_HHmmss_") + fileInfo.Name;
                                    }
                                    else
                                    {
                                        Directory.CreateDirectory(processedDir);
                                        processedDir = System.Configuration.ConfigurationManager.AppSettings["FTLPathProcessed"].ToString() + DateTime.Now.ToString("MMddyyyy_HHmmss_") + fileInfo.Name;
                                    }

                                    bool resFileReceived = true; ;
                                    //DateTime? LastInsertedOnRes = dataAccess.GetLastLoadedResDate();

                                    //if (LastInsertedOnRes.GetValueOrDefault(DateTime.MinValue).Date == DateTime.Now.Date)
                                    //{
                                    //    resFileReceived = true;
                                    //}
                                    System.Net.Mail.MailMessage messProcessed = new System.Net.Mail.MailMessage();
                                    mailBody.Clear();

                                    messProcessed.Subject = "FTL file Processed Successfully";
                                    mailBody = new StringBuilder("<HTML><BODY><p>Dear Team </p>" +
                                                   "<p> FTL File Processed successfully.</p>");


                                    if (resFileReceived)
                                        mailBody.Append("<p>Load airtrack status:<B> " + dataAccess.loadAirtrack() + "</p> </b>");
                                    else
                                        mailBody.Append("<p>Load airtrack status:<B> Reservation file not received for today. </p> </b>");

                                    mailBody.Append("<p>Time of job stop: <B>" + DateTime.Now.ToString() + "</B> <p><I> This is a system generated mail </I></p></BODY></HTML>");
                                    messProcessed.Body = mailBody.ToString();
                                    Utility.Send(messProcessed);

                                    File.Move(dir, processedDir);// Move file to processed directory
                                    Logger.Info("File Loaded successfull!");
                                   
                                }
                                else if (processedStatus == -10)
                                {
                                    System.Net.Mail.MailMessage messProcessed = new System.Net.Mail.MailMessage();
                                    mailBody.Clear();
                                    messProcessed.Subject = "FTL File not found.";
                                    mailBody = new StringBuilder("<HTML><BODY><p>Dear Team </p>" +
                                                   "<p> FTL File Not found for the job.</p>");
                                    mailBody.Append("<p>Time of job start: <B>" + DateTime.Now.ToString() + "</B><p><I> This is a system generated mail </I></p></BODY></HTML>");
                                    messProcessed.Body = mailBody.ToString();
                                    Utility.Send(messProcessed);

                                    Logger.Info("File Not Found!");
                                  
                                }
                                else if (processedStatus < 0)
                                {
                                    string errorDir = "";

                                    errorDir = System.Configuration.ConfigurationManager.AppSettings["FTLPathError"].ToString();
                                    if (Directory.Exists(errorDir))
                                    {
                                        errorDir = System.Configuration.ConfigurationManager.AppSettings["FTLPathError"].ToString() + DateTime.Now.ToString("MMddyyyy_HHmmss_") + fileInfo.Name;
                                    }
                                    else
                                    {
                                        Directory.CreateDirectory(errorDir);
                                        errorDir = System.Configuration.ConfigurationManager.AppSettings["FTLPathError"].ToString() + DateTime.Now.ToString("MMddyyyy_HHmmss_") + fileInfo.Name;
                                    }
                                    if (processedStatus != -3)
                                    {
                                        System.Net.Mail.MailMessage messProcessed = new System.Net.Mail.MailMessage();
                                        mailBody.Clear();
                                        messProcessed.Subject = "FTL File Load Failed";
                                        mailBody = new StringBuilder("<HTML><BODY><p>Dear Team </p>" +
                                                       "<p> FTL File Load Failed due to unexpected error.</p>");
                                        mailBody.Append("<p>Time <B>" + DateTime.Now.ToString() + "</B><p><I> This is a system generated mail </I></p></BODY></HTML>");
                                        messProcessed.Body = mailBody.ToString();
                                        Utility.Send(messProcessed);
                                    }

                                    File.Move(dir, errorDir);//Move file to erroe directory
                                    Logger.Info("File Loaded not successfull!");
                                }
                            }
                            break;
                        }
                    case "DCS":
                        {
                            dir = System.Configuration.ConfigurationManager.AppSettings["DCSPath"];
                            PAIConst.FileTypeToLoad = FileType.DCS;
                            var dirInfo = new DirectoryInfo(dir);
                            List<FileInfo> filesInfo = dirInfo.GetFiles().OrderByDescending(x => x.LastWriteTime).ToList();
                            foreach (FileInfo fileInfo in filesInfo)
                            {
                                dir = System.Configuration.ConfigurationManager.AppSettings["DCSPath"] + fileInfo.Name;
                                processedStatus = businessLogic.LoadFileAll(dir);
                                if (processedStatus > 0)
                                {
                                    processedDir = System.Configuration.ConfigurationManager.AppSettings["DCSPathProcessed"].ToString();
                                    if (Directory.Exists(processedDir))
                                    {
                                        processedDir = System.Configuration.ConfigurationManager.AppSettings["DCSPathProcessed"].ToString() + DateTime.Now.ToString("MMddyyyy_HHmmss_") + fileInfo.Name;
                                    }
                                    else
                                    {
                                        Directory.CreateDirectory(processedDir);
                                        processedDir = System.Configuration.ConfigurationManager.AppSettings["DCSPathProcessed"].ToString() + DateTime.Now.ToString("MMddyyyy_HHmmss_") + fileInfo.Name;
                                    }


                                    System.Net.Mail.MailMessage messProcessed = new System.Net.Mail.MailMessage();
                                    mailBody.Clear();
                                    messProcessed.Subject = "DCS file Processed Successfully";
                                    mailBody = new StringBuilder("<HTML><BODY><p>Dear Team </p>" +
                                                   "<p> DCS File Processed successfully.</p>");

                                    mailBody.Append("<p>Load airtrack status:<B> " + dataAccess.loadAirtrack() + "</p> </b>");

                                    mailBody.Append("<p>Time of job stop: <B>" + DateTime.Now.ToString() + "</B><p><I> This is a system generated mail </I></p></BODY></HTML>");
                                    messProcessed.Body = mailBody.ToString();
                                    Utility.Send(messProcessed);

                                    File.Move(dir, processedDir);
                                    Logger.Info("File Loaded successfull!");
                                   
                                }
                                else if (processedStatus == -10)
                                {
                                    System.Net.Mail.MailMessage messProcessed = new System.Net.Mail.MailMessage();
                                    mailBody.Clear();
                                    messProcessed.Subject = "DCS File not found.";
                                    mailBody = new StringBuilder("<HTML><BODY><p>Dear Team </p>" +
                                                   "<p> DCS File Not found for the job.</p>");
                                    mailBody.Append("<p>Time of job start: <B>" + DateTime.Now.ToString() + "</B><p><I> This is a system generated mail </I></p></BODY></HTML>");
                                    messProcessed.Body = mailBody.ToString();
                                    Utility.Send(messProcessed);
                                    Logger.Info("File Not Found!");
                                   
                                }
                                else if (processedStatus < 0)
                                {
                                    string errorDir = "";

                                    errorDir = System.Configuration.ConfigurationManager.AppSettings["DCSPathError"].ToString();
                                    if (Directory.Exists(errorDir))
                                    {
                                        errorDir = System.Configuration.ConfigurationManager.AppSettings["DCSPathError"].ToString() + DateTime.Now.ToString("MMddyyyy_HHmmss_") + fileInfo.Name;
                                    }
                                    else
                                    {
                                        Directory.CreateDirectory(errorDir);
                                        errorDir = System.Configuration.ConfigurationManager.AppSettings["DCSPathError"].ToString() + DateTime.Now.ToString("MMddyyyy_HHmmss_") + fileInfo.Name;
                                    }
                                    if (processedStatus != -3)
                                    {
                                        System.Net.Mail.MailMessage messProcessed = new System.Net.Mail.MailMessage();
                                        mailBody.Clear();

                                        messProcessed.Subject = "DCS File Load Failed";
                                        mailBody = new StringBuilder("<HTML><BODY><p>Dear Team </p>" +
                                                       "<p> DCS File Load Failed due to unexpected error.</p>");
                                        mailBody.Append("<p>Time <B>" + DateTime.Now.ToString() + "</B><p><I> This is a system generated mail </I></p></BODY></HTML>");
                                        messProcessed.Body = mailBody.ToString();
                                        Utility.Send(messProcessed);
                                    }



                                    File.Move(dir, errorDir);
                                    Logger.Info("File Loaded not successfull!");
                                }
                            }
                            break;
                        }
                    case "RES":
                        {
                            dir = System.Configuration.ConfigurationManager.AppSettings["RESPath"];
                            PAIConst.FileTypeToLoad = FileType.RES;

                            var dirInfo = new DirectoryInfo(dir);
                            List<FileInfo> filesInfo = dirInfo.GetFiles().OrderByDescending(x => x.LastWriteTime).ToList();
                            foreach (FileInfo fileInfo in filesInfo)
                            {
                                dir = System.Configuration.ConfigurationManager.AppSettings["RESPath"] + fileInfo.Name;
                                processedStatus = businessLogic.LoadFileAll(dir);

                                if (processedStatus > 0)
                                {
                                    processedDir = System.Configuration.ConfigurationManager.AppSettings["RESPathProcessed"].ToString();
                                    if (Directory.Exists(processedDir))
                                    {
                                        processedDir = System.Configuration.ConfigurationManager.AppSettings["RESPathProcessed"].ToString() + DateTime.Now.ToString("MMddyyyy_HHmmss_") + fileInfo.Name;
                                    }
                                    else
                                    {
                                        Directory.CreateDirectory(processedDir);
                                        processedDir = System.Configuration.ConfigurationManager.AppSettings["RESPathProcessed"].ToString() + DateTime.Now.ToString("MMddyyyy_HHmmss_") + fileInfo.Name;
                                    }

                                    bool ftlFileReceived = false;
                                    //DateTime? LastInsertedOnFTL = dataAccess.GetLastLoadedFTLDate();
                                    //if (LastInsertedOnFTL.GetValueOrDefault(DateTime.MinValue).Date == DateTime.Now.Date)
                                    //{
                                    //    ftlFileReceived = true;
                                    //}
                                    System.Net.Mail.MailMessage messProcessed = new System.Net.Mail.MailMessage();
                                    mailBody.Clear();

                                    messProcessed.Subject = "RES file Processed Successfully";
                                    mailBody = new StringBuilder("<HTML><BODY><p>Dear Team </p>" +
                                                   "<p> RES File Processed successfully.</p>");

                                    //if (ftlFileReceived)
                                    //    mailBody.Append("<p>Load airtrack status:<B> " + dataAccess.loadAirtrack() + "</p> </b>");
                                    //else
                                    //    mailBody.Append("<p>Load airtrack status:<B> FTL file not received for today. </p> </b>");

                                    mailBody.Append("<p>Time of job stop: <B>" + DateTime.Now.ToString() + "</B><p><I> This is a system generated mail </I></p></BODY></HTML>");
                                    messProcessed.Body = mailBody.ToString();
                                    Utility.Send(messProcessed);

                                    File.Move(dir, processedDir);
                                    Logger.Info("File Loaded successfull!");
                                }
                                else if (processedStatus == -10)
                                {
                                    System.Net.Mail.MailMessage messProcessed = new System.Net.Mail.MailMessage();
                                    mailBody.Clear();

                                    messProcessed.Subject = "RES File not found.";
                                    mailBody = new StringBuilder("<HTML><BODY><p>Dear Team </p>" +
                                                   "<p> RES File Not found for the job.</p>");
                                    mailBody.Append("<p>Time of job start: <B>" + DateTime.Now.ToString() + "</B><p><I> This is a system generated mail </I></p></BODY></HTML>");
                                    messProcessed.Body = mailBody.ToString();
                                    Utility.Send(messProcessed);
                                    Logger.Info("File Not Found!");
                                  
                                }
                                else if (processedStatus < 0 )
                                {
                                    string errorDir = "";

                                    errorDir = System.Configuration.ConfigurationManager.AppSettings["RESPathError"].ToString();
                                    if (Directory.Exists(errorDir))
                                    {
                                        errorDir = System.Configuration.ConfigurationManager.AppSettings["RESPathError"].ToString() + DateTime.Now.ToString("MMddyyyy_HHmmss_") + fileInfo.Name;
                                    }
                                    else
                                    {
                                        Directory.CreateDirectory(errorDir);
                                        errorDir = System.Configuration.ConfigurationManager.AppSettings["RESPathError"].ToString() + DateTime.Now.ToString("MMddyyyy_HHmmss_") + fileInfo.Name;
                                    }

                                    if (processedStatus != -3)
                                    {
                                        System.Net.Mail.MailMessage messProcessed = new System.Net.Mail.MailMessage();
                                        mailBody.Clear();

                                        messProcessed.Subject = "RES File Load Failed";
                                        mailBody = new StringBuilder("<HTML><BODY><p>Dear Team </p>" +
                                                       "<p> RES File Load Failed due to unexpected error.</p>");
                                        mailBody.Append("<p>Time <B>" + DateTime.Now.ToString() + "</B><p><I> This is a system generated mail </I></p></BODY></HTML>");
                                        messProcessed.Body = mailBody.ToString();
                                        Utility.Send(messProcessed);
                                    }

                                    File.Move(dir, errorDir);
                                    Logger.Info("File Loaded not successfull!");
                                }
                            }
                            break;
                        }

                    case "EDW":
                        {
                            //  dir = System.Configuration.ConfigurationManager.AppSettings["RESPath"];
                            PAIConst.FileTypeToLoad = FileType.DEF;
                            int recordsProcesed = businessLogic.LoadEDWViews();
                            break;
                        }
                    case "SRX":
                        {
                            dir = System.Configuration.ConfigurationManager.AppSettings["SIRAXPath"];
                            PAIConst.FileTypeToLoad = FileType.SRX;
                            var dirInfo = new DirectoryInfo(dir);
                            List<FileInfo> filesInfo = dirInfo.GetFiles().OrderByDescending(x => x.LastWriteTime).ToList();
                            foreach (FileInfo fileInfo in filesInfo)
                            {
                                dir = System.Configuration.ConfigurationManager.AppSettings["SIRAXPath"] + fileInfo.Name;
                                processedStatus = businessLogic.LoadFileAll(dir);
                                if (processedStatus > 0)
                                {
                                    processedDir = System.Configuration.ConfigurationManager.AppSettings["SIRAXPathProcessed"].ToString();
                                    if (Directory.Exists(processedDir))
                                    {
                                        processedDir = System.Configuration.ConfigurationManager.AppSettings["SIRAXPathProcessed"].ToString() + DateTime.Now.ToString("MMddyyyy_HHmmss_") + fileInfo.Name;
                                    }
                                    else
                                    {
                                        Directory.CreateDirectory(processedDir);
                                        processedDir = System.Configuration.ConfigurationManager.AppSettings["SIRAXPathProcessed"].ToString() + DateTime.Now.ToString("MMddyyyy_HHmmss_") + fileInfo.Name;
                                    }

                                    System.Net.Mail.MailMessage messProcessed = new System.Net.Mail.MailMessage();
                                    mailBody.Clear();

                                    messProcessed.Subject = "SIRAX file Processed Successfully";
                                    mailBody = new StringBuilder("<HTML><BODY><p>Dear Team </p>" +
                                                   "<p> SIRAX File Processed successfully.</p>");
                                    mailBody.Append("<p>Time of job stop: <B>" + DateTime.Now.ToString() + "</B><p><I> This is a system generated mail </I></p></BODY></HTML>");
                                    messProcessed.Body = mailBody.ToString();
                                    Utility.Send(messProcessed);
                                    File.Move(dir, processedDir);
                                    Logger.Info("File Loaded successfull!");
                                    
                                }
                                else if (processedStatus == -10)
                                {
                                    System.Net.Mail.MailMessage messProcessed = new System.Net.Mail.MailMessage();
                                    mailBody.Clear();

                                    messProcessed.Subject = "SIRAX File not found.";
                                    mailBody = new StringBuilder("<HTML><BODY><p>Dear Team </p>" +
                                                   "<p> SIRAX File Not found for the job.</p>");
                                    mailBody.Append("<p>Time of job start: <B>" + DateTime.Now.ToString() + "</B><p><I> This is a system generated mail </I></p></BODY></HTML>");
                                    messProcessed.Body = mailBody.ToString();
                                    Utility.Send(messProcessed);
                                    Logger.Info("File Not Found!");
                                    
                                }
                                else if (processedStatus < 0 )
                                {
                                    string errorDir = "";

                                    errorDir = System.Configuration.ConfigurationManager.AppSettings["SIRAXPathError"].ToString();
                                    if (Directory.Exists(errorDir))
                                    {
                                        errorDir = System.Configuration.ConfigurationManager.AppSettings["SIRAXPathError"].ToString() + DateTime.Now.ToString("MMddyyyy_HHmmss_") + fileInfo.Name;
                                    }
                                    else
                                    {
                                        Directory.CreateDirectory(errorDir);
                                        errorDir = System.Configuration.ConfigurationManager.AppSettings["SIRAXPathError"].ToString() + DateTime.Now.ToString("MMddyyyy_HHmmss_") + fileInfo.Name;
                                    }

                                    if (processedStatus != -3)
                                    {
                                        System.Net.Mail.MailMessage messProcessed = new System.Net.Mail.MailMessage();
                                        mailBody.Clear();

                                        messProcessed.Subject = "SIRAX File Load Failed";
                                        mailBody = new StringBuilder("<HTML><BODY><p>Dear Team </p>" +
                                                       "<p> SIRAX File Load Failed due to unexpected error.</p>");
                                        mailBody.Append("<p>Time <B>" + DateTime.Now.ToString() + "</B><p><I> This is a system generated mail </I></p></BODY></HTML>");
                                        messProcessed.Body = mailBody.ToString();
                                        Utility.Send(messProcessed);
                                    }

                                    File.Move(dir, errorDir);
                                    Logger.Info("File Loaded not successfull!");
                                }
                            }

                            break;
                        }
                    default:
                        {
                            Logger.Error(String.Format("Could not initiate file load, input argument invalid: '{0}' ", args[0].ToString()));
                            return;
                        }

                }


                #region commented code
                //FileInfo file = new FileInfo(dir);
                //string type = "";
                //if (PAIConst.FileTypeToLoad == FileType.DCS)
                //{
                //    type = "DCS";
                //}
                //else if (PAIConst.FileTypeToLoad == FileType.RES)
                //{
                //    type = "RES";
                //}
                //else if (PAIConst.FileTypeToLoad == FileType.FTL)
                //{
                //    type = "FTL";
                //}
                //else if (PAIConst.FileTypeToLoad == FileType.SRX)
                //{
                //    type = "SIRAX";
                //}
                //if (processedStatus > 0)
                //{

                //    switch (PAIConst.FileTypeToLoad)
                //    {
                //        case FileType.DCS:
                //            {
                //                processedDir = System.Configuration.ConfigurationManager.AppSettings["DCSPathProcessed"].ToString();
                //                if (Directory.Exists(processedDir))
                //                {
                //                    processedDir = System.Configuration.ConfigurationManager.AppSettings["DCSPathProcessed"].ToString() + "\\Processed" + DateTime.Now.ToString("_MMddyyyy_HHmmss_") + "DCS.TXT";
                //                }
                //                else
                //                {
                //                    Directory.CreateDirectory(processedDir);
                //                    processedDir = System.Configuration.ConfigurationManager.AppSettings["DCSPathProcessed"].ToString() + "\\Processed" + DateTime.Now.ToString("_MMddyyyy_HHmmss_") + "DCS.TXT";
                //                }

                //                System.Net.Mail.MailMessage messProcessed = new System.Net.Mail.MailMessage();
                //                mailBody.Clear();

                //                messProcessed.Subject = "DCS file Processed Successfully";
                //                mailBody = new StringBuilder("<HTML><BODY><p>Dear Team </p>" +
                //                               "<p>" + type + " File Processed successfully.</p>");
                //                mailBody.Append("<p>Time of job stop: <B>" + DateTime.Now.ToString() + "</B><p><I> This is a system generated mail </I></p></BODY></HTML>");
                //                messProcessed.Body = mailBody.ToString();
                //                Utility.Send(messProcessed);



                //                break;
                //            }
                //        case FileType.FTL:
                //            {


                //                break;
                //            }
                //        case FileType.RES:
                //            {
                //                processedDir = System.Configuration.ConfigurationManager.AppSettings["RESPathProcessed"].ToString();
                //                if (Directory.Exists(processedDir))
                //                {
                //                    processedDir = System.Configuration.ConfigurationManager.AppSettings["RESPathProcessed"].ToString() + "\\Processed" + DateTime.Now.ToString("_MMddyyyy_HHmmss_") + "RES.TXT";
                //                }
                //                else
                //                {
                //                    Directory.CreateDirectory(processedDir);
                //                    processedDir = System.Configuration.ConfigurationManager.AppSettings["RESPathProcessed"].ToString() + "\\Processed" + DateTime.Now.ToString("_MMddyyyy_HHmmss_") + "RES.TXT";
                //                }

                //                System.Net.Mail.MailMessage messProcessed = new System.Net.Mail.MailMessage();
                //                mailBody.Clear();

                //                messProcessed.Subject = "RES file Processed Successfully";
                //                mailBody = new StringBuilder("<HTML><BODY><p>Dear Team </p>" +
                //                               "<p>" + type + " File Processed successfully.</p>");
                //                mailBody.Append("<p>Time of Job Stop: <B>" + DateTime.Now.ToString() + "</B><p><I> This is a system generated mail </I></p></BODY></HTML>");
                //                messProcessed.Body = mailBody.ToString();
                //                Utility.Send(messProcessed);

                //                break;
                //            }

                //        case FileType.SRX:
                //            {
                //                processedDir = System.Configuration.ConfigurationManager.AppSettings["SIRAXPathProcessed"].ToString();
                //                if (Directory.Exists(processedDir))
                //                {
                //                    processedDir = System.Configuration.ConfigurationManager.AppSettings["SIRAXPathProcessed"].ToString() + "\\Processed" + DateTime.Now.ToString("_MMddyyyy_HHmmss_") + "SIRAX.TXT";
                //                }
                //                else
                //                {
                //                    Directory.CreateDirectory(processedDir);
                //                    processedDir = System.Configuration.ConfigurationManager.AppSettings["SIRAXPathProcessed"].ToString() + "\\Processed" + DateTime.Now.ToString("_MMddyyyy_HHmmss_") + "SIRAX.TXT";
                //                }

                //                System.Net.Mail.MailMessage messProcessed = new System.Net.Mail.MailMessage();
                //                mailBody.Clear();

                //                messProcessed.Subject = "SIRAX file Processed Successfully";
                //                mailBody = new StringBuilder("<HTML><BODY><p>Dear Team </p>" +
                //                               "<p>" + type + " File Processed successfully.</p>");
                //                mailBody.Append("<p>Time of Job Stop: <B>" + DateTime.Now.ToString() + "</B><p><I> This is a system generated mail </I></p></BODY></HTML>");
                //                messProcessed.Body = mailBody.ToString();
                //                Utility.Send(messProcessed);

                //                break;
                //            }

                //    }

                //    File.Move(dir, processedDir);
                //    Logger.Info("File Loaded successfull!");
                //    return;
                //}
                //else if (processedStatus == -1)
                //{
                //    System.Net.Mail.MailMessage messProcessed = new System.Net.Mail.MailMessage();
                //    mailBody.Clear();

                //    messProcessed.Subject =type+ "File not found.";
                //    mailBody = new StringBuilder("<HTML><BODY><p>Dear Team </p>" +
                //                   "<p>"+ type + " File Not found for the job.</p>");
                //    mailBody.Append("<p>Time of job start: <B>" + DateTime.Now.ToString() + "</B><p><I> This is a system generated mail </I></p></BODY></HTML>");
                //    messProcessed.Body = mailBody.ToString();
                //    Utility.Send(messProcessed);
                //    Logger.Info("File Not Found!");
                //    return;
                //}
                //else
                //{
                //    string errorDir = "";
                //    switch (PAIConst.FileTypeToLoad)
                //    {
                //        case FileType.DCS:
                //            {
                //                errorDir = System.Configuration.ConfigurationManager.AppSettings["DCSPathError"].ToString();
                //                if (Directory.Exists(errorDir))
                //                {
                //                    errorDir = System.Configuration.ConfigurationManager.AppSettings["DCSPathError"].ToString() + "\\Error" + DateTime.Now.ToString("_MMddyyyy_HHmmss_") + "DCS.TXT";
                //                }
                //                else
                //                {
                //                    Directory.CreateDirectory(errorDir);
                //                    errorDir = System.Configuration.ConfigurationManager.AppSettings["DCSPathError"].ToString() + "\\Error" + DateTime.Now.ToString("_MMddyyyy_HHmmss_") + "DCS.TXT";
                //                }

                //                System.Net.Mail.MailMessage messProcessed = new System.Net.Mail.MailMessage();
                //                mailBody.Clear();

                //                messProcessed.Subject = "DCS File Failed";
                //                mailBody = new StringBuilder("<HTML><BODY><p>Dear Team </p>" +
                //                               "<p>" + type + " File Failed due to unexpected error.</p>");
                //                mailBody.Append("<p>Time <B>" + DateTime.Now.ToString() + "</B><p><I> This is a system generated mail </I></p></BODY></HTML>");
                //                messProcessed.Body = mailBody.ToString();
                //                Utility.Send(messProcessed);



                //                break;
                //            }
                //        case FileType.FTL:
                //            {
                //                errorDir = System.Configuration.ConfigurationManager.AppSettings["FTLPathError"].ToString();
                //                if (Directory.Exists(errorDir))
                //                {
                //                    errorDir = System.Configuration.ConfigurationManager.AppSettings["FTLPathError"].ToString() + "\\Error" + DateTime.Now.ToString("_MMddyyyy_HHmmss_") + "FTL.TXT";
                //                }
                //                else
                //                {
                //                    Directory.CreateDirectory(errorDir);
                //                    errorDir = System.Configuration.ConfigurationManager.AppSettings["FTLPathError"].ToString() + "\\Error" + DateTime.Now.ToString("_MMddyyyy_HHmmss_") + "FTL.TXT";
                //                }

                //                System.Net.Mail.MailMessage messProcessed = new System.Net.Mail.MailMessage();
                //                mailBody.Clear();

                //                messProcessed.Subject = "FTL File Failed";
                //                mailBody = new StringBuilder("<HTML><BODY><p>Dear Team </p>" +
                //                               "<p>" + type + " File Failed due to unexpected error.</p>");
                //                mailBody.Append("<p>Time <B>" + DateTime.Now.ToString() + "</B><p><I> This is a system generated mail </I></p></BODY></HTML>");
                //                messProcessed.Body = mailBody.ToString();
                //                Utility.Send(messProcessed);

                //                break;
                //            }
                //        case FileType.RES:
                //            {
                //                errorDir = System.Configuration.ConfigurationManager.AppSettings["RESPathError"].ToString();
                //                if (Directory.Exists(errorDir))
                //                {
                //                    errorDir = System.Configuration.ConfigurationManager.AppSettings["RESPathError"].ToString() + "\\Error" + DateTime.Now.ToString("_MMddyyyy_HHmmss_") + "RES.TXT";
                //                }
                //                else
                //                {
                //                    Directory.CreateDirectory(errorDir);
                //                    errorDir = System.Configuration.ConfigurationManager.AppSettings["RESPathError"].ToString() + "\\Error" + DateTime.Now.ToString("_MMddyyyy_HHmmss_") + "RES.TXT";
                //                }


                //                System.Net.Mail.MailMessage messProcessed = new System.Net.Mail.MailMessage();
                //                mailBody.Clear();

                //                messProcessed.Subject = "RES File Failed";
                //                mailBody = new StringBuilder("<HTML><BODY><p>Dear Team </p>" +
                //                               "<p>" + type + " File Failed due to unexpected error.</p>");
                //                mailBody.Append("<p>Time: <B>" + DateTime.Now.ToString() + "</B><p><I> This is a system generated mail </I></p></BODY></HTML>");
                //                messProcessed.Body = mailBody.ToString();
                //                Utility.Send(messProcessed);

                //                break;
                //            }

                //    }


                //    File.Move(dir, errorDir);

                //    Logger.Info("File Loaded not successfull!");
                //}

                #endregion
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);

                System.Net.Mail.MailMessage messProcessed = new System.Net.Mail.MailMessage();
                mailBody.Clear();

                messProcessed.Subject = "Very Unexpected Error During EDW File Load";
                mailBody = new StringBuilder("<HTML><BODY><p>Dear Team </p>" +
                               "<p>A very unexpected error has been faced while loading EDW or SIRAX file.</p> <p>" + ex.Message + "</p>");
                mailBody.Append("<p>Time <B>" + DateTime.Now.ToString() + "</B><p><I> This is a system generated mail </I></p></BODY></HTML>");
                messProcessed.Body = mailBody.ToString();
                Utility.Send(messProcessed);

                return;
            }

            Logger.Info("Exiting application!");

        }
    }
}
