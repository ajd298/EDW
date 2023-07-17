using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Configuration;
using System.Globalization;

namespace EDWFileHandler
{

    public class BusinessLogic
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Check if the file exists at the location
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
       // string filetype = "";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        /// My first code in develpoemt branch
        public int LoadFileAll(string dir)
        {

            int status = 0;
            try
            {

                if (File.Exists(dir))
                {
                    FileInfo fi = new FileInfo(dir);
                    Logger.Info(String.Format("Picked up file '{0}', file size {1} Bytes ", fi.Name, fi.Length));


                    status = LoadFile(dir);
                    return status;
                }
                else
                {
                    Logger.Error(String.Format("file not found. Directory: '{0}'", dir));
                    return -10;
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                return -11;
            }

        }

        public int LoadEDWViews()
        {
            System.Net.Mail.MailMessage mess = new System.Net.Mail.MailMessage();
            StringBuilder mailBody = new StringBuilder();
            mess.Subject = "EDW Views Load Started";
            mailBody = new StringBuilder("<HTML><BODY><p>Dear Team </p>" +
                           "<p>Data load from EDW views has been initiated. </p>");
            mailBody.Append("<p>Time of job start: <B>" + DateTime.Now.ToString() + "</B> <p><I> This is a system generated mail </I></p></BODY></HTML>");
            mess.Body = mailBody.ToString();
            Utility.Send(mess);

            DataAccess DT = new DataAccess();
            DT.LoadSuperView();
            return 1;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public int LoadFile(string dir)
        {

            StreamReader streamReader = null;
            string fileFormatConfigFilesPath = ConfigurationManager.AppSettings["ConfiPath"];

            //By default FTL
            string headerCfgFile = FileNames.sFTL_HEADER_CONFIG_FILE;
            string dataCfgFile = FileNames.sFTL_DATA_CONFIG_FILE;
            // change Config file 

            if (PAIConst.FileTypeToLoad == FileType.SRX)
            {
                headerCfgFile = FileNames.sSRX_HEADER_CONFIG_FILE;
                dataCfgFile = FileNames.sSRX_DATA_CONFIG_FILE;
            }
            else if (PAIConst.FileTypeToLoad == FileType.DCS)
            {
                headerCfgFile = FileNames.sDCS_HEADER_CONFIG_FILE;
                dataCfgFile = FileNames.sDCS_DATA_CONFIG_FILE;
            }
            else if (PAIConst.FileTypeToLoad == FileType.RES)
            {
                headerCfgFile = FileNames.sRES_HEADER_CONFIG_FILE;
                dataCfgFile = FileNames.sRES_DATA_CONFIG_FILE;
            }

            int status = 0;
            try
            {
                switch (PAIConst.FileTypeToLoad)
                {
                    case FileType.FTL:
                        {
                            return status = CopyToDBFTL(dir, ref streamReader, fileFormatConfigFilesPath, headerCfgFile, dataCfgFile);

                        }
                    case FileType.DCS:
                        {
                            return status = CopyToDBDCS(dir, ref streamReader, fileFormatConfigFilesPath, headerCfgFile, dataCfgFile);

                        }
                    case FileType.RES:
                        {
                            return status = CopyToDBRES(dir, ref streamReader, fileFormatConfigFilesPath, headerCfgFile, dataCfgFile);

                        }
                    case FileType.SRX:
                        {
                            return status = CopyToDBSRX(dir, ref streamReader, fileFormatConfigFilesPath, headerCfgFile, dataCfgFile);

                        }
                    default:
                        return 0;
                }


            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                throw;

            }
            finally
            {
                streamReader.Close();
            }

        }

        private int CopyToDBFTL(string fullFileName, ref StreamReader streamReader, string fileFormatConfigFilesPath, string sHeaderCfgFile, string sDataCfgFile)
        {
            int processedItems = 0;
            int recordsRejected = 0;
            int datatransferId = -1;
            DATATRANSFEREDWFLT dataTransfer = new DATATRANSFEREDWFLT();
            List<AIRTRACKFTLPFS> ftlRowList = new List<AIRTRACKFTLPFS>();
            AIRTRACKFTLPFS ftlRow;
            DataAccess dt = new DataAccess();

            int recordsToSave = 0;


            try
            {
                //   streamReader.Close();
                streamReader = new StreamReader(fullFileName, System.Text.Encoding.GetEncoding(PAIConst.PAI_FILEREAD_ENCODING));
                string headerLine = streamReader.ReadLine();
                int recordCount = 0;
                string trailerLine = "";
                if (!FormatValidator.ValidateFileAndRecCnt(fullFileName, ref recordCount, ref trailerLine))
                {
                    Logger.Info("FTL File rejected: File Header or Trailer missing.");
                    return -1;

                }

                if (!Utility.InitInputFileConfigData(FileType.FTL, fileFormatConfigFilesPath + sHeaderCfgFile, fileFormatConfigFilesPath + sDataCfgFile))
                {
                    Logger.Info("Could not load Configs");
                    return -2;
                }
                HeaderFileLinePositions fileLinePositions = Utility.oHeaderFileLinePositions;
                TrailerFileLinePositions fileLinePositionsTrailer = Utility.oTrailerFileLinePositions;
                string delivSeqno = Utility.GetStringValue(fileLinePositions.DeliverySeqNo, headerLine);

                int noOfRecFile = Utility.GetIntValueConvertEmptyStringToZero(fileLinePositionsTrailer.NoOfRecords, trailerLine);



                System.Net.Mail.MailMessage mess = new System.Net.Mail.MailMessage();
                StringBuilder mailBody = new StringBuilder();
                mess.Subject = "FTL File Load Started";
                mailBody = new StringBuilder("<HTML><BODY><p>Dear Team </p>" +
                               "<p>New FTL File is detected by the system and file processing is initiated. </p>");
                if (noOfRecFile != recordCount + 2)
                    mailBody.Append("Total records and noof records in file does not match.");
                mailBody.Append("<p>Time of job start: <B>" + DateTime.Now.ToString() + "</B> DSN :" + delivSeqno + "</p>" + "<p><I> This is a system generated mail </I></p></BODY></HTML>");
                mess.Body = mailBody.ToString();
                Utility.Send(mess);

                if (!CheckDsn(delivSeqno))
                {
                    Logger.Error(String.Format("Duplicate DSN for FTL file. received DSN {0}", delivSeqno));

                    System.Net.Mail.MailMessage messDup = new System.Net.Mail.MailMessage();
                    mailBody.Clear();
                    mess.Subject = "Duplicate DSN for FTL File";
                    mailBody = new StringBuilder("<HTML><BODY><p>Dear Team </p>" +
                                   "<p>New FTL File is rejected due to duplicate DSN.</p>");
                    mailBody.Append("<p>Time: <B>" + DateTime.Now.ToString() + "</B>, Duplicate DSN :" + delivSeqno + "</p>" + "<p><I> This is a system generated mail </I></p></BODY></HTML>");
                    mess.Body = mailBody.ToString();
                    Utility.Send(mess);
                    return -3;
                }


                if (LoadHeaderFTL(ref dataTransfer, headerLine, 1))
                {
                    dataTransfer.DATE_OF_INSERT = DateTime.UtcNow;
                    dataTransfer.STATUS = (int)DatatransferState.UNDEFINED;

                    if (!dt.insertDatatransfer(dataTransfer))
                    {
                        Logger.Error("FTL: An error occured while saving DataTransfer.");
                        return -4;
                    }
                    datatransferId = dataTransfer.DELIVNO;

                    String recType = "";
                    String fileDataLine = streamReader.ReadLine();
                    String fileTrailerLine = "";
                    int lineCounter = 1;
                    while ((fileDataLine != null) && (fileDataLine.Length > 2))
                    {
                        ftlRow = new AIRTRACKFTLPFS();

                        lineCounter++;
                        recType = fileDataLine.Substring(0, 1);
                        if (recType == PAIConst.cs_ACC_FLT_DATA_RECORD)
                        {
                            processedItems++;
                            int respCode = 0;
                            if (LoadDataFTL(dataTransfer, ref ftlRow, FileType.FTL, fileDataLine, lineCounter, ref respCode))
                            {
                                bool dayChanged = false;
                                string cityOn = ftlRow.STNON;
                                string cityOff = ftlRow.STNOFF;

                                dt.GetStationDetails(ftlRow.STNON, ftlRow.STNOFF, ref cityOn, ref cityOff);
                                dayChanged = dt.checkDayChangeFlag(ftlRow.DATOP, cityOn, cityOff);

                                if (dt.checkDuplicateAirtrackCheckIn(ftlRow.DOCNO, ftlRow.STNON, ftlRow.STNOFF, ftlRow.FLTNO, ftlRow.DATOP, ftlRow.CCID, ftlRow.CCTYPME, dayChanged))
                                {
                                    Logger.Info(String.Format("Duplicate Activity for FTL. DOCNO '{0}', STNON '{1}', STNOFF '{2}', FLTNO '{3}', DATOP '{4}', CCID '{5}', CCTYPME '{6}'", ftlRow.DOCNO, ftlRow.STNON, ftlRow.STNOFF, ftlRow.FLTNO, ftlRow.DATOP, ftlRow.CCID, ftlRow.CCTYPME));
                                    //fileDataLine = streamReader.ReadLine();
                                    respCode = PAIConst.DuplicateAirtrack;
                                    //continue;
                                }
                                recordsToSave++;
                                ftlRow.DT_EDW_FLT_ID = datatransferId;
                                ftlRow.RESP_CODE = respCode;
                                ftlRow.PROCESSED_STATUS = PAIConst.CheckInDataInitialProcessedStatus;
                                ftlRow.INSERTED_ON = DateTime.Now;
                                if (respCode != 0)
                                    recordsRejected++;

                                ftlRowList.Add(ftlRow);
                            }
                        }
                        else
                        {
                            fileTrailerLine = fileDataLine;  //Temp Varible Assignment.
                        }




                        if ((processedItems > 0) && ((processedItems % PAIConst.NO_OF_ROWS_TO_SAVE_TO_DATABASE) == 0)) // ~500
                        { // partial save.
                            try
                            {
                                if (recordsToSave > 0)
                                {
                                    dt.insertData(ftlRowList);
                                    ftlRowList.Clear();
                                    recordsToSave = 0;
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Fatal(ex);
                                throw;

                            }

                        }
                        fileDataLine = streamReader.ReadLine();
                        if (recType == PAIConst.cs_ACC_FLT_TRAILER_RECORD)
                        {
                            try
                            {
                                if (recordsToSave > 0)
                                {
                                    dt.insertData(ftlRowList);
                                    ftlRowList.Clear();
                                    dataTransfer.STATUS = (int)DatatransferState.LOADED;
                                    dataTransfer.NOOF_REC_RJCTD = recordsRejected;
                                    dataTransfer.TOTAL_REC = processedItems;
                                    dt.updateDataTransferTotRec(dataTransfer);
                                    recordsToSave = 0;
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Fatal(ex);
                                return -6;
                            }
                        }
                    }

                }
                else
                {
                    Logger.Error("Header loading  failed.");
                    return -7;
                }




                return 1;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                return -8;
            }
            finally
            {
                streamReader.Close();
            }

        }

        private int CopyToDBDCS(string fullFileName, ref StreamReader streamReader, string fileFormatConfigFilesPath, string sHeaderCfgFile, string sDataCfgFile)
        {
            int processedItems = 0;
            int datatransferId = -1;
            int rejectedRecords = 0;
            DATATRANSFEREDWDCS dataTransfer = new DATATRANSFEREDWDCS();
            List<AIRTRACKDCSPAL> ftlRowList = new List<AIRTRACKDCSPAL>();
            AIRTRACKDCSPAL dcsRow;
            DataAccess dt = new DataAccess();

            int recordsToSave = 0;


            try
            {
                //   streamReader.Close();
                streamReader = new StreamReader(fullFileName, System.Text.Encoding.GetEncoding(PAIConst.PAI_FILEREAD_ENCODING));
                string headerLine = streamReader.ReadLine();
                int recordCount = 0;
                string trailerLine = "";
                if (!FormatValidator.ValidateFileAndRecCnt(fullFileName, ref recordCount, ref trailerLine))
                {
                    Logger.Info("DCS: File rejected:File Header or Trailer missing.");
                    return -1;

                }

                if (!Utility.InitInputFileConfigData(FileType.FTL, fileFormatConfigFilesPath + sHeaderCfgFile, fileFormatConfigFilesPath + sDataCfgFile))
                {
                    Logger.Info("Could not load Configs");
                    return -2;
                }

                HeaderFileLinePositions fileLinePositions = Utility.oHeaderFileLinePositions;

                string delivSeqno = Utility.GetStringValue(fileLinePositions.DeliverySeqNo, headerLine);

                int noOfRecFile = Utility.GetIntValueConvertEmptyStringToZero(Utility.oTrailerFileLinePositions.NoOfRecords, trailerLine);

                System.Net.Mail.MailMessage mess = new System.Net.Mail.MailMessage();
                StringBuilder mailBody = new StringBuilder();
                mess.Subject = "DCS File Load Started";
                mailBody = new StringBuilder("<HTML><BODY><p>Dear Team </p>" +
                               "<p>New DCS File is detected by the system and file processing is initiated. </p>");
                if (noOfRecFile != recordCount + 2)
                    mailBody.Append("Total records and noof records in file does not match.");
                mailBody.Append("<p>Time of job start: <B>" + DateTime.Now.ToString() + "</B> DSN :" + delivSeqno + "</p>" + "<p><I> This is a system generated mail </I></p></BODY></HTML>");
                mess.Body = mailBody.ToString();
                Utility.Send(mess);

                if (!CheckDsn(delivSeqno))
                {
                    Logger.Error(String.Format("Duplicate DSN for DCS file. received DSN {0}", delivSeqno));

                    System.Net.Mail.MailMessage messDup = new System.Net.Mail.MailMessage();
                    mailBody.Clear();
                    mess.Subject = "Duplicate DSN Received FOR DCS File";
                    mailBody = new StringBuilder("<HTML><BODY><p>Dear Team </p>" +
                                   "<p>New DCS File is rejected due to duplicate DSN.</p>");
                    mailBody.Append("<p>Time: <B>" + DateTime.Now.ToString() + "</B>, Duplicate DSN :" + delivSeqno + "</p>" + "<p><I> This is a system generated mail </I></p></BODY></HTML>");
                    mess.Body = mailBody.ToString();
                    Utility.Send(mess);
                    return -3;
                }


                if (LoadHeaderDCS(ref dataTransfer, headerLine, 1))
                {
                    dataTransfer.DATE_OF_INSERT = DateTime.UtcNow;
                    dataTransfer.STATUS = (int)DatatransferState.UNDEFINED;

                    if (!dt.insertDatatransfer(dataTransfer))
                    {
                        Logger.Error("DCS: An error occured while saving DataTransfer.");
                        return -4;
                    }
                    datatransferId = dataTransfer.DELIVNO;

                    String recType = "";
                    String fileDataLine = streamReader.ReadLine();
                    String fileTrailerLine = "";
                    int lineCounter = 1;
                    while ((fileDataLine != null) && (fileDataLine.Length > 2))
                    {
                        dcsRow = new AIRTRACKDCSPAL();

                        lineCounter++;
                        recType = fileDataLine.Substring(0, 1);
                        if (recType == PAIConst.cs_ACC_FLT_DATA_RECORD)
                        {
                            processedItems++;
                            int respCode = 0;
                            if (LoadDataDCS(dataTransfer, ref dcsRow, FileType.FTL, fileDataLine, lineCounter, ref respCode))
                            {
                                bool dayChanged = false;
                                string cityOn = dcsRow.STNON;
                                string cityOff = dcsRow.STNOFF;

                                dt.GetStationDetails(dcsRow.STNON, dcsRow.STNOFF, ref cityOn, ref cityOff);
                                dayChanged = dt.checkDayChangeFlag(dcsRow.DATOP, cityOn, cityOff);

                                if (dayChanged)
                                    Logger.Info(String.Format("Day Changed STNON '{0}', STNOFF '{1}'", dcsRow.STNON, dcsRow.STNOFF));


                                if (dt.checkDuplicateAirtrackCheckIn(dcsRow.DOCNO, dcsRow.STNON, dcsRow.STNOFF, dcsRow.FLTNO, dcsRow.DATOP, dcsRow.CCID, dcsRow.CCTYPME, dayChanged))
                                {
                                    Logger.Info(String.Format("Duplicate Activity for DCS. DOCNO '{0}', STNON '{1}', STNOFF '{2}', FLTNO '{3}', DATOP '{4}', CCID '{5}', CCTYPME '{6}'", dcsRow.DOCNO, dcsRow.STNON, dcsRow.STNOFF, dcsRow.FLTNO, dcsRow.DATOP, dcsRow.CCID, dcsRow.CCTYPME));
                                    //fileDataLine = streamReader.ReadLine();
                                    //continue;
                                    respCode = PAIConst.DuplicateAirtrack;
                                }
                                recordsToSave++;
                                dcsRow.DTEDWDCSID = datatransferId;
                                dcsRow.RESP_CODE = respCode;
                                dcsRow.PROCESSED_STATUS = PAIConst.CheckInDataInitialProcessedStatus;
                                dcsRow.INSERTED_ON = DateTime.Now;
                                if (respCode != 0)
                                    rejectedRecords++;

                                ftlRowList.Add(dcsRow);
                            }
                        }
                        else
                        {
                            fileTrailerLine = fileDataLine;  //Temp Varible Assignment.
                        }




                        if ((processedItems > 0) && ((processedItems % PAIConst.NO_OF_ROWS_TO_SAVE_TO_DATABASE) == 0)) // ~500
                        { // partial save.
                            try
                            {
                                if (recordsToSave > 0)
                                {
                                    dt.insertData(ftlRowList);
                                    ftlRowList.Clear();
                                    recordsToSave = 0;
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Fatal(ex);
                                throw;

                            }

                        }
                        fileDataLine = streamReader.ReadLine();
                        if (recType == PAIConst.cs_ACC_FLT_TRAILER_RECORD)
                        {
                            try
                            {
                                if (recordsToSave > 0)
                                {
                                    dt.insertData(ftlRowList);
                                    ftlRowList.Clear();
                                    dataTransfer.STATUS = (int)DatatransferState.LOADED;
                                    dataTransfer.NOOF_REC_RJCTD = rejectedRecords;
                                    dataTransfer.TOTAL_REC = processedItems;
                                    dt.updateDataTransferTotRec(dataTransfer);
                                    recordsToSave = 0;
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Fatal(ex);
                                return -5;
                            }
                        }
                    }

                }
                else
                {
                    Logger.Error("Header loading  failed.");
                    return -6;
                }




                return 1;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                return -7;
            }
            finally
            {
                streamReader.Close();
            }

        }

        private int CopyToDBRES(string fullFileName, ref StreamReader streamReader, string fileFormatConfigFilesPath, string sHeaderCfgFile, string sDataCfgFile)
        {
            int processedItems = 0;
            int datatransferId = -1;
            DATATRANSFEREDWRES dataTransfer = new DATATRANSFEREDWRES();
            // List<AIRTRACKRES> ftlRowList = new List<AIRTRACKRES>();
            AIRTRACKRES ftlRow;
            DataAccess dt = new DataAccess();

            int recordsToSave = 0;
            int rejectedRecords = 0;


            try
            {
                //   streamReader.Close();
                streamReader = new StreamReader(fullFileName, System.Text.Encoding.GetEncoding(PAIConst.PAI_FILEREAD_ENCODING));
                string headerLine = streamReader.ReadLine();
                int recordCount = 0;
                string trailerLine = "";
                if (!FormatValidator.ValidateFileAndRecCnt(fullFileName, ref recordCount, ref trailerLine))
                {
                    Logger.Info("RES File rejected:File Header or Trailer missing.");
                    return -1;

                }

                if (!Utility.InitInputFileConfigData(FileType.FTL, fileFormatConfigFilesPath + sHeaderCfgFile, fileFormatConfigFilesPath + sDataCfgFile))
                {
                    Logger.Info("Could not load Configs");
                    return -2;
                }
                HeaderFileLinePositions fileLinePositions = Utility.oHeaderFileLinePositions;
                TrailerFileLinePositions fileLinePositionsTrailer = Utility.oTrailerFileLinePositions;

                string delivSeqno = Utility.GetStringValue(fileLinePositions.DeliverySeqNo, headerLine);
                int noOfRecFile = Utility.GetIntValueConvertEmptyStringToZero(fileLinePositionsTrailer.NoOfRecords, trailerLine);
                Logger.Info("Loaded delivery number and No of records");
                System.Net.Mail.MailMessage mess = new System.Net.Mail.MailMessage();
                StringBuilder mailBody = new StringBuilder();
                mess.Subject = "RES File Load Started";
                mailBody = new StringBuilder("<HTML><BODY><p>Dear Team </p>" +
                               "<p>New RES File is detected by the system and file processing is initiated. </p>");
                if (noOfRecFile != recordCount + 2)
                    mailBody.Append("Total records and noof records in file does not match.");
                mailBody.Append("<p>Time of job start: <B>" + DateTime.Now.ToString() + "</B> DSN :" + delivSeqno + "</p>" + "<p><I> This is a system generated mail </I></p></BODY></HTML>");
                mess.Body = mailBody.ToString();
                Utility.Send(mess);

                if (!CheckDsn(delivSeqno))
                {
                    Logger.Error(String.Format("Duplicate DSN for RES file. received DSN {0}", delivSeqno));

                    System.Net.Mail.MailMessage messDup = new System.Net.Mail.MailMessage();
                    mailBody.Clear();
                    mess.Subject = "Duplicate DSN Received for RES File";
                    mailBody = new StringBuilder("<HTML><BODY><p>Dear Team </p>" +
                                   "<p>New RES File is rejected due to duplicate DSN.</p>");
                    mailBody.Append("<p>Time <B>" + DateTime.Now.ToString() + "</B>, Duplicate DSN :" + delivSeqno + "</p>" + "<p><I> This is a system generated mail </I></p></BODY></HTML>");
                    mess.Body = mailBody.ToString();
                    Utility.Send(mess);
                    return -3;
                }


                if (LoadHeaderRES(ref dataTransfer, headerLine, 1))
                {

                    dataTransfer.DATE_OF_INSERT = DateTime.UtcNow;
                    dataTransfer.STATUS = (int)DatatransferState.UNDEFINED;

                    if (!dt.insertDatatransfer(dataTransfer))
                    {
                        Logger.Error("RES: An error occured while saving DataTransfer.");
                        return -4;
                    }
                    datatransferId = dataTransfer.DELIVNO;

                    String recType = "";
                    String fileDataLine = streamReader.ReadLine();
                    String fileTrailerLine = "";
                    int lineCounter = 1;
                    Dictionary<string, string> airtrackResIds = new Dictionary<string, string>();

                    while ((fileDataLine != null) && (fileDataLine.Length > 2))
                    {
                        ftlRow = new AIRTRACKRES();
                        AIRTRACK airtrack = null;
                        lineCounter++;
                        recType = fileDataLine.Substring(0, 1);
                        if (recType == PAIConst.cs_ACC_FLT_DATA_RECORD)
                        {
                            int respCode = 0;
                            if (LoadDataRES(dataTransfer, ref ftlRow, FileType.FTL, fileDataLine, lineCounter, ref respCode))
                            {
                                recordsToSave++;

                                ftlRow.RESP_CODE = respCode;
                                ftlRow.DTEDWRESID = datatransferId;
                                ftlRow.PROCESSED_STATUS = PAIConst.CheckInDataInitialProcessedStatus;
                                ftlRow.INSERTED_ON = DateTime.Now;

                                #region duplicity check
                                //if(respCode==0)
                                //{


                                //bool dayChanged = false;
                                //string cityOn = ftlRow.STNON;
                                //string cityOff = ftlRow.STNOFF;

                                //dt.GetStationDetails(ftlRow.STNON, ftlRow.STNOFF, ref cityOn, ref cityOff);
                                //dayChanged = dt.checkDayChangeFlag(ftlRow.DATOP, cityOn, cityOff);

                                //airtrack = dt.checkDuplicateAirtrackRes(ftlRow.DOCNO, ftlRow.STNON, ftlRow.STNOFF, ftlRow.FLTNO, ftlRow.DATOP, ftlRow.CCID, ftlRow.CCTYPME, dayChanged);

                                //if (airtrack != null)
                                //{
                                //    Logger.Info(string.Format("Reservation record already processed:  DocNo {0}, CCID {1}, STNON {2}, STNOFF {3}, DATOP {4}, Airtarck Id {5}", ftlRow.DOCNO, ftlRow.CCID, ftlRow.STNON, ftlRow.STNOFF, ftlRow.DATOP,airtrack.AIRTRACKID));
                                //    ftlRow.PROCESSED_STATUS = ResStatus.Processed;
                                //    ftlRow.RESP_CODE = PAIConst.ReservationProcessed;

                                //}
                                //}
                                #endregion
                                if (respCode != 0)
                                    rejectedRecords++;

                                processedItems++;

                            }
                        }
                        else
                        {
                            fileTrailerLine = fileDataLine;  //Temp Varible Assignment.
                        }




                        //if ((processedItems > 0) && ((processedItems % PAIConst.NO_OF_ROWS_TO_SAVE_TO_DATABASE) == 0)) // ~500
                        //{ // partial save.
                        try
                        {
                            //if (recordsToSave > 0)
                            //{
                            long ResId = dt.insertData(ftlRow);
                            if (ResId != -1 && airtrack != null)
                            {
                                dt.updateAirtrackResId(airtrack.AIRTRACKID, ResId);
                            }
                            // ftlRowList.Clear();
                            //recordsToSave = 0;
                            //}
                        }
                        catch (Exception ex)
                        {
                            Logger.Fatal(ex);


                        }

                        //}
                        fileDataLine = streamReader.ReadLine();
                        if (recType == PAIConst.cs_ACC_FLT_TRAILER_RECORD)
                        {
                            try
                            {
                                if (recordsToSave >= 0)
                                {
                                    //dt.insertData(ftlRowList);
                                    //ftlRowList.Clear();
                                    dataTransfer.STATUS = (int)DatatransferState.LOADED;
                                    dataTransfer.NOOF_REC_RJCTD = rejectedRecords;
                                    dataTransfer.TOTAL_REC = processedItems;
                                    dt.updateDataTransferTotRec(dataTransfer);
                                    //recordsToSave = 0;
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Fatal(ex);
                                return -5;
                            }
                        }
                    }

                }
                else
                {
                    Logger.Error("Header loading  failed.");
                    return -6;
                }




                return 1;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                return -7;
            }
            finally
            {
                streamReader.Close();
            }

        }

        private int CopyToDBSRX(string fullFileName, ref StreamReader streamReader, string fileFormatConfigFilesPath, string sHeaderCfgFile, string sDataCfgFile)
        {
            int processedItems = 0;
            int datatransferId = -1;
            DATATRANSFERSIRAXIN dataTransfer = new DATATRANSFERSIRAXIN();
            List<AIRTRACK> airtrackRowList = new List<AIRTRACK>();
            AIRTRACK atkRow;
            DataAccess dt = new DataAccess();

            int recordsToSave = 0;
            int airtrackRecords = 0;
            int rejectedRecords = 0;
            List<long> resIds = new List<long>();


            try
            {
                //   streamReader.Close();
                streamReader = new StreamReader(fullFileName, System.Text.Encoding.GetEncoding(PAIConst.PAI_FILEREAD_ENCODING));
                string headerLine = streamReader.ReadLine();
                //string trailerLine = streamReader.ReadLine();
                int recordCount = 0;
                string trailerLine = "";
                if (!FormatValidator.ValidateFileAndRecCnt(fullFileName, ref recordCount, ref trailerLine))
                {
                    Logger.Info("SIRAX File rejected:File Header or Trailer missing.");
                    return -1;
                }

                string delivType = "";
                if (headerLine.Length >= 18)
                {
                    delivType = headerLine.Substring(14, 3);
                }




                if (delivType == "PAX")
                {
                    sHeaderCfgFile = FileNames.sSRX_NO_DOC_HEADER_CONFIG_FILE;
                    sDataCfgFile = FileNames.sSRX_NO_DOC_DATA_CONFIG_FILE;

                }
                if (!Utility.InitInputFileConfigData(FileType.FTL, fileFormatConfigFilesPath + sHeaderCfgFile, fileFormatConfigFilesPath + sDataCfgFile, delivType))
                {
                    Logger.Info("Could not load Configs");
                    return -2;
                }
                HeaderFileLinePositions fileLinePositions = Utility.oHeaderFileLinePositions;
                TrailerFileLinePositions trailerFileLinePositions = Utility.oTrailerFileLinePositions;


                int fileRecordCount = Convert.ToInt32(Utility.GetStringValue(trailerFileLinePositions.NoOfRecords, trailerLine));
                string delivSeqno = Utility.GetStringValue(fileLinePositions.DeliverySeqNo, headerLine);

                System.Net.Mail.MailMessage mess = new System.Net.Mail.MailMessage();
                StringBuilder mailBody = new StringBuilder();
                mess.Subject = "SIRAX File Load Started";
                mailBody = new StringBuilder("<HTML><BODY><p>Dear Team </p>" +
                               "<p>New SIRAX File is detected by the system and file processing is initiated. </p>");
                if (fileRecordCount != recordCount + 2)
                    mailBody.Append("Total records and noof records in file does not match.");
                mailBody.Append("<p>Time of job start: <B>" + DateTime.Now.ToString() + "</B> DSN :" + delivSeqno + "</p>" + "<p><I> This is a system generated mail </I></p></BODY></HTML>");
                mess.Body = mailBody.ToString();
                Utility.Send(mess);

                #region commented
                if (delivType == "PAX" && !CheckDsn(delivSeqno))
                {
                    Logger.Error(String.Format("Duplicate DSN for SIRAX file file. received DSN {0}", delivSeqno));

                    System.Net.Mail.MailMessage messDup = new System.Net.Mail.MailMessage();
                    mailBody.Clear();
                    mess.Subject = "Duplicate DSN Received for SIRAX File";
                    mailBody = new StringBuilder("<HTML><BODY><p>Dear Team </p>" +
                                   "<p>New SIRAX File is rejected due to duplicate DSN.</p>");
                    mailBody.Append("<p>Time <B>" + DateTime.Now.ToString() + "</B>, Duplicate DSN :" + delivSeqno + "</p>" + "<p><I> This is a system generated mail </I></p></BODY></HTML>");
                    mess.Body = mailBody.ToString();
                    Utility.Send(mess);
                    return -3;
                }

                #endregion


                if (LoadHeaderSRX(ref dataTransfer, headerLine, 1))
                {
                    dataTransfer.DATE_OF_INSERT = DateTime.UtcNow;
                    dataTransfer.STATUS = (int)DatatransferState.UNDEFINED;

                    if (!dt.insertDatatransfer(dataTransfer, ref datatransferId))
                    {
                        Logger.Error("SIRAX: An error occured while saving DataTransfer.");
                        return -3;
                    }


                    String recType = "";
                    String fileDataLine = streamReader.ReadLine();
                    String fileTrailerLine = "";
                    int lineCounter = 1;

                    DataAccess dataAccess = new DataAccess();
                    DATATRANSFERAIRTRACK datatransferAirtrack = CreateDatatransferAirtrack();

                    if (delivType == "PAX")
                        datatransferAirtrack.SRC = "PAX";

                    int datatransferAirtrackId = 0;
                    if (dataAccess.insertDatatransfer(datatransferAirtrack))
                    {
                        datatransferAirtrackId = datatransferAirtrack.DELIVNO;
                    }
                    else
                    {
                        Logger.Error("SIRAX:Could not create datatransfer for airtrack");
                        return -4;
                    }


                    while ((fileDataLine != null) && (fileDataLine.Length > 2))
                    {
                        atkRow = new AIRTRACK();

                        lineCounter++;
                        recType = fileDataLine.Substring(0, 1);
                        if (recType == PAIConst.cs_ACC_FLT_DATA_RECORD)
                        {
                            int respCode = 0;

                            if (LoadDataSRX(datatransferAirtrackId, ref atkRow, FileType.FTL, fileDataLine, lineCounter, delivType, ref respCode, ref resIds))
                            {

                                atkRow.DTID = datatransferAirtrackId;

                                atkRow.CURR_STATUS = PAIConst.CheckInDataInitialProcessedStatus;
                                atkRow.INSERTED_ON = DateTime.Now;
                                atkRow.SRC = datatransferAirtrack.SRC;
                                if (respCode != 0)
                                    rejectedRecords++;

                                processedItems++;

                                if (respCode == 0)
                                {
                                    bool dayChange = dataAccess.checkDayChangeFlag(atkRow.DATOP, atkRow.STNON, atkRow.STNOFF);

                                    if (!dt.checkDuplicateAirtrack(atkRow, dayChange))
                                    {

                                        airtrackRecords++;
                                        airtrackRowList.Add(atkRow);
                                        recordsToSave++;

                                    }
                                    else
                                    {
                                        Logger.Info(String.Format("SIRAX: Record rejected due to duplicate entry in AIRTRACK  Doc No'{0}', Origin '{1}', Destination '{2}', Departure Date '{3}'", atkRow.DOCNO, atkRow.STNON, atkRow.STNOFF, atkRow.DATOP));
                                        rejectedRecords++;
                                    }
                                }
                                else
                                    Logger.Info(String.Format("SIRAX: Record rejected due to response code '{0}' , DOC NO '{1}', STNON '{2}', STNOFF '{3}', DATOP '{4}'", respCode, atkRow.DOCNO, atkRow.STNON, atkRow.STNOFF, atkRow.DATOP));
                            }
                            else
                            {
                                rejectedRecords++;
                                processedItems++;
                                Logger.Info(String.Format("SIRAX:Record loading Error, Line number  '{0}'  ", lineCounter));
                            }
                        }
                        else
                        {
                            fileTrailerLine = fileDataLine;  //Temp Varible Assignment.
                        }




                        if ((processedItems > 0) && ((processedItems % PAIConst.NO_OF_ROWS_TO_SAVE_TO_DATABASE) == 0)) // ~500
                        { // partial save.
                            try
                            {
                                if (recordsToSave > 0)
                                {
                                    dt.insertData(airtrackRowList);
                                    airtrackRowList.Clear();
                                    dt.updateResStatus(resIds);
                                    resIds.Clear();
                                    recordsToSave = 0;
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Fatal(ex);
                                throw;

                            }

                        }
                        fileDataLine = streamReader.ReadLine();
                        if (recType == PAIConst.cs_ACC_FLT_TRAILER_RECORD)
                        {
                            try
                            {
                                if (recordsToSave > 0)
                                {
                                    dt.insertData(airtrackRowList);
                                    airtrackRowList.Clear();

                                    dt.updateResStatus(resIds);
                                    resIds.Clear();

                                }
                                dataTransfer.STATUS = (int)DatatransferState.LOADED;
                                dataTransfer.NOOF_REC_RJCTD = rejectedRecords;
                                dataTransfer.TOTAL_REC = processedItems;
                                dt.updateDataTransferTotRec(dataTransfer);

                                datatransferAirtrack.TOTAL_REC = airtrackRecords;// update only saved in airtrack
                                datatransferAirtrack.STATUS = (int)DatatransferState.LOADED; ;
                                dt.updateDataTransferTotRec(datatransferAirtrack);
                                recordsToSave = 0;
                            }
                            catch (Exception ex)
                            {
                                Logger.Fatal(ex);
                                return -5;
                            }
                        }
                    }

                }
                else
                {
                    Logger.Error("Header loading  failed.");
                    return -6;
                }




                return 1;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                return -7;
            }
            finally
            {
                streamReader.Close();
            }

        }

        private bool LoadHeaderFTL(ref DATATRANSFEREDWFLT dataTransfer, string fileLine, int lineNo)
        {
            String fieldValue = "";
            String fieldName = "";
            HeaderFileLinePositions fileLinePositions = Utility.oHeaderFileLinePositions;
            try
            {
                fieldName = fileLinePositions.DeliverySeqNo.sName;
                dataTransfer.DELIVNO = Utility.GetIntValueConvertEmptyStringToZero(fileLinePositions.DeliverySeqNo, fileLine);

                fieldName = fileLinePositions.DeliveryDate.sName;
                dataTransfer.DELIVDAT = Utility.GetDateTimeValue(fileLinePositions.DeliveryDate, fileLine);

                fieldName = fileLinePositions.SendingSystem.sName;
                fieldValue = Utility.GetStringValue(fileLinePositions.SendingSystem, fileLine);
                dataTransfer.SENDSYS = fieldValue;

                fieldName = fileLinePositions.Supplier.sName;
                fieldValue = Utility.GetStringValue(fileLinePositions.Supplier, fileLine);
                dataTransfer.DATASUP = fieldValue;

                fieldName = fileLinePositions.TypeOfDelivery.sName;
                fieldValue = Utility.GetStringValue(fileLinePositions.TypeOfDelivery, fileLine);
                dataTransfer.TYPDELIV = fieldValue;



            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                return false;
            }
            return true;
        }

        private bool LoadDataFTL(DATATRANSFEREDWFLT dataTransfer, ref AIRTRACKFTLPFS ftlDataRow, FileType fileType, String fileLine, int lineNo, ref int respCode)
        {
            String fieldValue = "";
            String sFileType = fileType.ToString().PadLeft(1, '0');
            int requiredType = RequiredField.OPTIONAL;
            bool isValidTemp = true;
            bool isValid = true;
            DataFileLinePositions fileLinePositions = Utility.oDataFileLinePositions;
            LinePosData linePosData = fileLinePositions.RecordRowType;
            try
            {
                linePosData = fileLinePositions.FlightNumber;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.FlightNumber.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.FLTNO = fieldValue;

                linePosData = fileLinePositions.ServiceClass;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.FlightNumber.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.SERVCL = fieldValue;

                linePosData = fileLinePositions.FlightSuffix;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.FlightSuffix.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.FLTSUF = fieldValue;

                linePosData = fileLinePositions.DocumentNumber;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.DocumentNumber.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.DOCNO = fieldValue;

                linePosData = fileLinePositions.DepartureStation;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.DepartureStation.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.STNON = fieldValue;

                linePosData = fileLinePositions.ArrivalStation;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.ArrivalStation.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.STNOFF = fieldValue;


                linePosData = fileLinePositions.CardNumber;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.CardNumber.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.CCID = fieldValue;

                linePosData = fileLinePositions.CardType;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.CardType.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.CCTYPME = fieldValue;

                linePosData = fileLinePositions.BookingClass;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.BookingClass.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.BKGCLS = fieldValue;

                linePosData = fileLinePositions.DepartureDateLocal;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.DepartureDateLocal.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.DATOP = DateTime.ParseExact(fieldValue, "yyyyMMdd", null);

                linePosData = fileLinePositions.PNRNumber;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.PNRNumber.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.RECLOC = fieldValue;

                linePosData = fileLinePositions.PassengerName;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.PassengerName.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.PAXNM = fieldValue;

                linePosData = fileLinePositions.PassengerNote;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.PassengerNote.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.PSGRNT = fieldValue;

                linePosData = fileLinePositions.MarketingFlightNumber;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.MarketingFlightNumber.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.MKTFLTNO = fieldValue;

                linePosData = fileLinePositions.FareBasis;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.MarketingFlightNumber.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.FBCODE = fieldValue;

                linePosData = fileLinePositions.DocumentCouponNo;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.MarketingFlightNumber.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.COUPNO = fieldValue;

                linePosData = fileLinePositions.CompanyConsumptionType;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.MarketingFlightNumber.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.CMPCNSPNTYPE = fieldValue;

                linePosData = fileLinePositions.CompanyIdentification;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.MarketingFlightNumber.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.CMPID = fieldValue;

                linePosData = fileLinePositions.TicketlessCode;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.MarketingFlightNumber.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.TICKETLESSCODE = fieldValue;

                //linePosData = fileLinePositions.AutomaticCheckInInd;
                //isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                //if (!isValidTemp)
                //    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.MarketingFlightNumber.sName, fieldValue, lineNo, linePosData.iStart));
                //isValid &= isValidTemp;
                //ftlDataRow.AUTOCHECKIN = fieldValue;


                if (!isValid)
                    respCode = PAIConst.ResponseCodeMandFieldsMissing;
                else if (!string.IsNullOrEmpty(ftlDataRow.CCTYPME) && ftlDataRow.CCTYPME.Length >= 2)
                {
                    if ((ftlDataRow.CCTYPME.Substring(0, 2) != "SK" && ftlDataRow.CCTYPME.Substring(0, 2) != "WF" && ftlDataRow.CCTYPME.Substring(0, 2) != "EB" && ftlDataRow.FLTNO.Substring(0, 2) == "WF"))
                        respCode = PAIConst.WFOnStar;
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                throw;
            }
            return true;
        }
        private bool LoadHeaderDCS(ref DATATRANSFEREDWDCS dataTransfer, string fileLine, int lineNo)
        {
            String fieldValue = "";
            String fieldName = "";
            HeaderFileLinePositions fileLinePositions = Utility.oHeaderFileLinePositions;
            try
            {
                fieldName = fileLinePositions.DeliverySeqNo.sName;
                dataTransfer.DELIVNO = Utility.GetIntValueConvertEmptyStringToZero(fileLinePositions.DeliverySeqNo, fileLine);

                fieldName = fileLinePositions.DeliveryDate.sName;
                dataTransfer.DELIVDAT = Utility.GetDateTimeValue(fileLinePositions.DeliveryDate, fileLine);

                fieldName = fileLinePositions.SendingSystem.sName;
                fieldValue = Utility.GetStringValue(fileLinePositions.SendingSystem, fileLine);
                dataTransfer.SENDSYS = fieldValue;

                fieldName = fileLinePositions.Supplier.sName;
                fieldValue = Utility.GetStringValue(fileLinePositions.Supplier, fileLine);
                dataTransfer.DATASUP = fieldValue;

                fieldName = fileLinePositions.TypeOfDelivery.sName;
                fieldValue = Utility.GetStringValue(fileLinePositions.TypeOfDelivery, fileLine);
                dataTransfer.TYPDELIV = fieldValue;



            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                return false;
            }
            return true;
        }

        private bool LoadDataDCS(DATATRANSFEREDWDCS dataTransfer, ref AIRTRACKDCSPAL ftlDataRow, FileType fileType, String fileLine, int lineNo, ref int respCode)
        {
            String fieldValue = "";
            String sFileType = fileType.ToString().PadLeft(1, '0');
            int requiredType = RequiredField.OPTIONAL;
            bool isValidTemp = true;
            bool isValid = true;
            DataFileLinePositions fileLinePositions = Utility.oDataFileLinePositions;
            LinePosData linePosData = fileLinePositions.RecordRowType;
            try
            {
                linePosData = fileLinePositions.FlightNumber;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.FlightNumber.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.FLTNO = fieldValue;

                linePosData = fileLinePositions.ServiceClass;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.FlightNumber.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.SERVCL = fieldValue;

                linePosData = fileLinePositions.FlightSuffix;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.FlightSuffix.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.FLTSUF = fieldValue;

                linePosData = fileLinePositions.DocumentNumber;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.DocumentNumber.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.DOCNO = fieldValue;

                linePosData = fileLinePositions.DepartureStation;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.DepartureStation.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.STNON = fieldValue;

                linePosData = fileLinePositions.ArrivalStation;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.ArrivalStation.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.STNOFF = fieldValue;

                linePosData = fileLinePositions.CardNumber;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.CardNumber.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.CCID = fieldValue;

                linePosData = fileLinePositions.CardType;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.CardType.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.CCTYPME = fieldValue;

                linePosData = fileLinePositions.BookingClass;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.BookingClass.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.BKGCLS = fieldValue;

                linePosData = fileLinePositions.DepartureDateLocal;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.DepartureDateLocal.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.DATOP = DateTime.ParseExact(fieldValue, "yyyyMMdd", null);

                linePosData = fileLinePositions.PNRNumber;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.PNRNumber.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.RECLOC = fieldValue;

                linePosData = fileLinePositions.PassengerName;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.PassengerName.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.PAXNM = fieldValue;

                linePosData = fileLinePositions.PassengerNote;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.PassengerNote.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.PSGRNT = fieldValue;

                linePosData = fileLinePositions.MarketingFlightNumber;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.MarketingFlightNumber.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.MKTFLTNO = fieldValue;

                linePosData = fileLinePositions.FareBasis;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.MarketingFlightNumber.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.FBCODE = fieldValue;

                linePosData = fileLinePositions.DocumentCouponNo;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.MarketingFlightNumber.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.COUPNO = fieldValue;

                linePosData = fileLinePositions.CompanyConsumptionType;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.MarketingFlightNumber.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.CMPCNSPNTYPE = fieldValue;

                linePosData = fileLinePositions.CompanyIdentification;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.MarketingFlightNumber.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.CMPID = fieldValue;

                linePosData = fileLinePositions.AutomaticCheckInInd;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.MarketingFlightNumber.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.AUTOCHECKIN = fieldValue;


                linePosData = fileLinePositions.TicketlessCode;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.MarketingFlightNumber.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.TICKETLESSCODE = fieldValue;

                if (!isValid)
                    respCode = PAIConst.ResponseCodeMandFieldsMissing;
                else if (!string.IsNullOrEmpty(ftlDataRow.CCTYPME) && ftlDataRow.CCTYPME.Length >= 2)
                {
                    if ((ftlDataRow.CCTYPME.Substring(0, 2) != "SK" && ftlDataRow.CCTYPME.Substring(0, 2) != "WF" && ftlDataRow.CCTYPME.Substring(0, 2) != "EB" && ftlDataRow.FLTNO.Substring(0, 2) == "WF"))
                        respCode = PAIConst.WFOnStar;
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                throw;
                //return false;
            }
            return true;
        }
        private bool LoadHeaderRES(ref DATATRANSFEREDWRES dataTransfer, string fileLine, int lineNo)
        {
            Logger.Info("Starting Header Load");
            String fieldValue = "";
            String fieldName = "";
            HeaderFileLinePositions fileLinePositions = Utility.oHeaderFileLinePositions;
            try
            {
                fieldName = fileLinePositions.DeliverySeqNo.sName;
                dataTransfer.DELIVNO = Utility.GetIntValueConvertEmptyStringToZero(fileLinePositions.DeliverySeqNo, fileLine);

                fieldName = fileLinePositions.DeliveryDate.sName;
                dataTransfer.DELIVDAT = Utility.GetDateTimeValue(fileLinePositions.DeliveryDate, fileLine);

                fieldName = fileLinePositions.SendingSystem.sName;
                fieldValue = Utility.GetStringValue(fileLinePositions.SendingSystem, fileLine);
                dataTransfer.SENDSYS = fieldValue;


                fieldName = fileLinePositions.Supplier.sName;
                fieldValue = Utility.GetStringValue(fileLinePositions.Supplier, fileLine);
                dataTransfer.DATASUP = fieldValue;

                fieldName = fileLinePositions.TypeOfDelivery.sName;
                fieldValue = Utility.GetStringValue(fileLinePositions.TypeOfDelivery, fileLine);
                dataTransfer.TYPDELIV = fieldValue;
                Logger.Info("Header Loaded");


            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                return false;
            }
            return true;
        }

        private bool LoadDataRES(DATATRANSFEREDWRES dataTransfer, ref AIRTRACKRES ftlDataRow, FileType fileType, String fileLine, int lineNo, ref int respCode)
        {
            //Logger.Info("Load Data start");
            String fieldValue = "";
            String sFileType = fileType.ToString().PadLeft(1, '0');
            int requiredType = RequiredField.OPTIONAL;
            bool isValidTemp = true;
            bool isValid = true;
            DataFileLinePositions fileLinePositions = Utility.oDataFileLinePositions;
            LinePosData linePosData = fileLinePositions.RecordRowType;
            try
            {
                linePosData = fileLinePositions.FlightNumber;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.FlightNumber.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.FLTNO = fieldValue;
                //Logger.Info("Flt no");
                linePosData = fileLinePositions.ServiceClass;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.FlightNumber.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.SERVCL = fieldValue;

                linePosData = fileLinePositions.FlightSuffix;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.FlightSuffix.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.FLTSUF = fieldValue;
                linePosData = fileLinePositions.DocumentNumber;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.DocumentNumber.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.DOCNO = fieldValue;
                linePosData = fileLinePositions.DepartureStation;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.DepartureStation.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.STNON = fieldValue;
                linePosData = fileLinePositions.ArrivalStation;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.ArrivalStation.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.STNOFF = fieldValue;


                linePosData = fileLinePositions.CardNumber;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.CardNumber.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.CCID = fieldValue;

                linePosData = fileLinePositions.CardType;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.CardType.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.CCTYPME = fieldValue;

                linePosData = fileLinePositions.BookingClass;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.BookingClass.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.BKGCLS = fieldValue;

                linePosData = fileLinePositions.DepartureDateLocal;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.DepartureDateLocal.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.DATOP = DateTime.ParseExact(fieldValue, "yyyyMMdd", null);

                linePosData = fileLinePositions.PNRNumber;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.PNRNumber.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.RECLOC = fieldValue;


                linePosData = fileLinePositions.PassengerName;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.PassengerName.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.PAXNM = fieldValue;


                linePosData = fileLinePositions.PassengerNote;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.PassengerNote.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.PSGRNT = fieldValue;

                linePosData = fileLinePositions.MarketingFlightNumber;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.MarketingFlightNumber.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.MKTFLTNO = fieldValue;

                linePosData = fileLinePositions.FareBasis;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.MarketingFlightNumber.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.FBCODE = fieldValue;

                linePosData = fileLinePositions.DocumentCouponNo;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.MarketingFlightNumber.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.COUPNO = fieldValue;

                linePosData = fileLinePositions.CompanyConsumptionType;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.MarketingFlightNumber.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.CMPCNSPNTYPE = fieldValue;

                linePosData = fileLinePositions.CompanyIdentification;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.MarketingFlightNumber.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.CMPID = fieldValue;


                linePosData = fileLinePositions.TicketlessCode;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.MarketingFlightNumber.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                ftlDataRow.TICKETLESSCODE = fieldValue;

                //linePosData = fileLinePositions.AutomaticCheckInInd;
                //isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                //if (!isValidTemp)
                //    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.MarketingFlightNumber.sName, fieldValue, lineNo, linePosData.iStart));
                //isValid &= isValidTemp;
                //ftlDataRow.AUTOCHECKIN = fieldValue;


                if (!isValid)
                    respCode = PAIConst.ResponseCodeMandFieldsMissing;
                else if (!string.IsNullOrEmpty(ftlDataRow.CCTYPME) && ftlDataRow.CCTYPME.Length >= 2)
                {
                    if ((ftlDataRow.CCTYPME.Substring(0, 2) != "SK" && ftlDataRow.CCTYPME.Substring(0, 2) != "WF" && ftlDataRow.CCTYPME.Substring(0, 2) != "EB" && ftlDataRow.FLTNO.Substring(0, 2) == "WF"))
                        respCode = PAIConst.WFOnStar;
                }





            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                throw;
            }
            return true;
        }

        private bool LoadHeaderSRX(ref DATATRANSFERSIRAXIN dataTransfer, string fileLine, int lineNo)
        {
            String fieldValue = "";
            String fieldName = "";
            HeaderFileLinePositions fileLinePositions = Utility.oHeaderFileLinePositions;
            try
            {
                fieldName = fileLinePositions.DeliverySeqNo.sName;
                dataTransfer.DELIVNO = Utility.GetIntValueConvertEmptyStringToZero(fileLinePositions.DeliverySeqNo, fileLine);

                fieldName = fileLinePositions.DeliveryDate.sName;
                dataTransfer.DELIVDAT = Utility.GetDateTimeValue(fileLinePositions.DeliveryDate, fileLine);

                fieldName = fileLinePositions.SendingSystem.sName;
                fieldValue = Utility.GetStringValue(fileLinePositions.SendingSystem, fileLine);
                dataTransfer.SENDSYS = fieldValue;

                fieldName = fileLinePositions.Supplier.sName;
                fieldValue = Utility.GetStringValue(fileLinePositions.Supplier, fileLine);
                dataTransfer.DATASUP = fieldValue;

                fieldName = fileLinePositions.TypeOfDelivery.sName;
                fieldValue = Utility.GetStringValue(fileLinePositions.TypeOfDelivery, fileLine);
                dataTransfer.TYPDELIV = fieldValue;




            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                return false;
            }
            return true;
        }

        private bool LoadDataSRX(int datatransferAirtrackId, ref AIRTRACK atkDataRow, FileType fileType, String fileLine, int lineNo, string delivType, ref int respCode, ref List<long> resIds)
        {
            String fieldValue = "";
            String sFileType = fileType.ToString().PadLeft(1, '0');
            int requiredType = RequiredField.OPTIONAL;
            bool isValidTemp = true;
            bool isValid = true;
            bool missingData = false;
            SIRAXDataFileLinePositions fileLinePositions = Utility.oSIRAXDataFileLinePositions;
            LinePosData linePosData = fileLinePositions.RecordRowType;
            try
            {
                linePosData = fileLinePositions.FlightNumber;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.FlightNumber.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                atkDataRow.FLTNO = fieldValue;


                if (delivType != "PAX")
                {
                    linePosData = fileLinePositions.ServiceClass;
                    isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                    if (!isValidTemp)
                        Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.FlightNumber.sName, fieldValue, lineNo, linePosData.iStart));
                    isValid &= isValidTemp;
                    atkDataRow.SERVCL = fieldValue;
                }

                linePosData = fileLinePositions.DocumentNumber;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.DocumentNumber.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                atkDataRow.DOCNO = fieldValue;

                linePosData = fileLinePositions.FareBasis;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                atkDataRow.FBCODE = fieldValue == "-" ? fieldValue.Replace("-", "") : fieldValue;

                linePosData = fileLinePositions.DepartureStation;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.DepartureStation.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                atkDataRow.STNON = fieldValue;

                linePosData = fileLinePositions.ArrivalStation;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.ArrivalStation.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                atkDataRow.STNOFF = fieldValue;

                if (delivType != "PAX")
                {
                    linePosData = fileLinePositions.BookingClass;
                    isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                    if (!isValidTemp)
                        Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.BookingClass.sName, fieldValue, lineNo, linePosData.iStart));
                    isValid &= isValidTemp;
                    atkDataRow.BKGCLS = fieldValue;
                }

                if (delivType == "PAX")
                {
                    linePosData = fileLinePositions.PassengerName;
                    isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                    if (!isValidTemp)
                        Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.PassengerName.sName, fieldValue, lineNo, linePosData.iStart));
                    isValid &= isValidTemp;
                    atkDataRow.PAXNM = fieldValue;
                }

                linePosData = fileLinePositions.DepartureDateLocal;
                isValidTemp = FormatValidator.HandleValidateFieldFormat(sFileType, ref fieldValue, requiredType, linePosData, fileLine, lineNo);
                if (!isValidTemp)
                    Logger.Error(String.Format("Invalid detail field data detected for field: '{0}', value: '{1}', lineNo: '{2}', pos: '{3}'.", fileLinePositions.DepartureDateLocal.sName, fieldValue, lineNo, linePosData.iStart));
                isValid &= isValidTemp;
                if (delivType == "PAX")
                {
                    if (fieldValue != "-")
                        atkDataRow.DATOP = DateTime.ParseExact(fieldValue, "yyyy-MM-dd", null);
                    else
                        missingData = true;
                }
                else
                {
                    if (fieldValue != "-")
                        atkDataRow.DATOP = DateTime.ParseExact(fieldValue, "yyyyMMdd", null);
                    else
                        missingData = true;
                }



                int holdingPeriod = Convert.ToInt32(ConfigurationManager.AppSettings["HoldingPeriodForSIRAX"]);

                if (!isValid)
                {
                    respCode = PAIConst.ResponseCodeMandFieldsMissing;
                }
                //else if (atkDataRow.DATOP < DateTime.Now.AddDays(-holdingPeriod)) // SASCHG0035908: skip check for SK 
                //{
                //    respCode = PAIConst.OutsideHoldingPeriod;
                //}
                else if (delivType != "PAX")
                {
                    AIRTRACKRES matchedResRecord = new AIRTRACKRES();
                    DataAccess dataAccess = new DataAccess();

                    bool dayChange = dataAccess.checkDayChangeFlag(atkDataRow.DATOP, atkDataRow.STNON, atkDataRow.STNOFF);

                    List<AIRTRACKRES> airtrackList = dataAccess.GetResRecords(atkDataRow.DOCNO, atkDataRow.DATOP, dayChange, missingData);
                    if (airtrackList == null)
                    {
                        respCode = PAIConst.ErrorinMatching;
                    }
                    else
                    {
                        if (airtrackList.Count == 0)
                            respCode = PAIConst.ReservationNotFound;
                        else
                        {
                            bool matchFound = false;
                            foreach (AIRTRACKRES airtrackRes in airtrackList)
                            {

                                string depCity = "";
                                string arrCity = "";

                                dataAccess.GetStationDetails(airtrackRes.STNON, airtrackRes.STNOFF, ref depCity, ref arrCity);

                                if (depCity == atkDataRow.STNON && arrCity == atkDataRow.STNOFF)
                                {
                                    matchFound = true;
                                    resIds.Add(airtrackRes.AIRTRACK_FTL_PFS_ID);
                                    matchedResRecord = airtrackRes;
                                    atkDataRow.FLTSUF = airtrackRes.FLTSUF;
                                    atkDataRow.CCTYPME = airtrackRes.CCTYPME;
                                    atkDataRow.BKGCLS = airtrackRes.BKGCLS;
                                    atkDataRow.SERVCL = airtrackRes.SERVCL;
                                    atkDataRow.CCID = airtrackRes.CCID;
                                    atkDataRow.STNON = airtrackRes.STNON;
                                    atkDataRow.STNOFF = airtrackRes.STNOFF;
                                    atkDataRow.DATOP = airtrackRes.DATOP;
                                    atkDataRow.AUTOCHECKIN = airtrackRes.AUTOCHECKIN;
                                    atkDataRow.PAXNM = airtrackRes.PAXNM;
                                    atkDataRow.CMPCNSPNTYPE = airtrackRes.CMPCNSPNTYPE;
                                    atkDataRow.CMPID = airtrackRes.CMPID;
                                    atkDataRow.PSGRNT = airtrackRes.PSGRNT;
                                    atkDataRow.MKTFLTNO = airtrackRes.MKTFLTNO;
                                    atkDataRow.TICKETLESSCODE = airtrackRes.TICKETLESSCODE;
                                    atkDataRow.FBCODE = String.IsNullOrEmpty(atkDataRow.FBCODE) ? airtrackRes.FBCODE : atkDataRow.FBCODE;
                                    atkDataRow.AIRTRACKRESID = airtrackRes.AIRTRACK_FTL_PFS_ID;
                                    atkDataRow.CURR_STATUS = AirtrackStatus.LoadedState;
                                    break;
                                }

                            }

                            if (!matchFound)
                                respCode = PAIConst.ReservationNotFound;
                            else if (!string.IsNullOrEmpty(atkDataRow.CCTYPME) && atkDataRow.CCTYPME.Length >= 2)
                            {
                                if (atkDataRow.CCTYPME.Substring(0, 2) != "SK" && atkDataRow.CCTYPME.Substring(0, 2) != "WF" && atkDataRow.CCTYPME.Substring(0, 2) != "EB" && atkDataRow.FLTNO.Substring(0, 2) == "WF")
                                    respCode = PAIConst.WFOnStar;
                            }


                            // SASCHG0035908: skip check for SK                             
                            if (!String.IsNullOrEmpty(atkDataRow.CCTYPME) && (atkDataRow.CCTYPME != "SK" && !atkDataRow.CCTYPME.StartsWith("EB") && atkDataRow.CCTYPME != "WF") && atkDataRow.DATOP < DateTime.Now.AddDays(-holdingPeriod)) // SASCHG0035908: skip check for SK 
                            {
                                respCode = PAIConst.OutsideHoldingPeriod;
                            }
                        }

                    }

                }
                else
                {
                    AIRTRACKRES matchedResRecord = new AIRTRACKRES();
                    DataAccess dataAccess = new DataAccess();

                    bool dayChange = dataAccess.checkDayChangeFlag(atkDataRow.DATOP, atkDataRow.STNON, atkDataRow.STNOFF);
                    string depCity = atkDataRow.STNON;
                    string arrCity = atkDataRow.STNOFF;
                    // dataAccess.GetStationDetails(atkDataRow.STNON, atkDataRow.STNOFF, ref depCity, ref arrCity);

                    AIRTRACKRES airtrackRes = dataAccess.GetResRecords(atkDataRow.DATOP, depCity, arrCity, atkDataRow.FLTNO, atkDataRow.PAXNM, dayChange, false);
                    if (airtrackRes == null)
                    {
                        respCode = PAIConst.ReservationNotFound;
                    }
                    else
                    {
                        bool matchFound = false;

                        matchFound = true;
                        resIds.Add(airtrackRes.AIRTRACK_FTL_PFS_ID);
                        matchedResRecord = airtrackRes;
                        atkDataRow.FLTSUF = airtrackRes.FLTSUF;
                        atkDataRow.CCTYPME = airtrackRes.CCTYPME;
                        atkDataRow.SERVCL = airtrackRes.SERVCL;
                        atkDataRow.BKGCLS = airtrackRes.BKGCLS;
                        atkDataRow.CCID = airtrackRes.CCID;
                        atkDataRow.STNON = airtrackRes.STNON;
                        atkDataRow.STNOFF = airtrackRes.STNOFF;
                        atkDataRow.DATOP = airtrackRes.DATOP;
                        atkDataRow.AUTOCHECKIN = airtrackRes.AUTOCHECKIN;
                        atkDataRow.PAXNM = airtrackRes.PAXNM;
                        atkDataRow.CMPCNSPNTYPE = airtrackRes.CMPCNSPNTYPE;
                        atkDataRow.CMPID = airtrackRes.CMPID;
                        atkDataRow.PSGRNT = airtrackRes.PSGRNT;
                        atkDataRow.MKTFLTNO = airtrackRes.MKTFLTNO;
                        atkDataRow.TICKETLESSCODE = airtrackRes.TICKETLESSCODE;
                        atkDataRow.FBCODE = String.IsNullOrEmpty(atkDataRow.FBCODE) ? airtrackRes.FBCODE : atkDataRow.FBCODE;
                        atkDataRow.AIRTRACKRESID = airtrackRes.AIRTRACK_FTL_PFS_ID;
                        atkDataRow.CURR_STATUS = AirtrackStatus.LoadedState;

                        if (!matchFound)
                            respCode = PAIConst.ReservationNotFound;
                        else
                        {
                            if (atkDataRow.CCTYPME.Substring(0, 2) != "SK" && atkDataRow.CCTYPME.Substring(0, 2) != "WF" && atkDataRow.CCTYPME.Substring(0, 2) != "EB" && atkDataRow.FLTNO.Substring(0, 2) == "WF")
                            {
                                respCode = PAIConst.WFOnStar;
                            }
                        }
                        // SASCHG0035908: skip check for SK                             
                        if (!String.IsNullOrEmpty(atkDataRow.CCTYPME) && (atkDataRow.CCTYPME != "SK" && !atkDataRow.CCTYPME.StartsWith("EB") && atkDataRow.CCTYPME != "WF") && atkDataRow.DATOP < DateTime.Now.AddDays(-holdingPeriod)) // SASCHG0035908: skip check for SK 
                        {
                            respCode = PAIConst.OutsideHoldingPeriod;
                        }

                    }

                }




            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                return false;
            }
            return true;
        }

        private bool CheckDsn(string deliNo)
        {
            DataAccess dt = new DataAccess();
            try
            {

                return dt.CheckDSN(deliNo);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                throw;
            }
        }

        private DATATRANSFERAIRTRACK CreateDatatransferAirtrack()
        {
            DATATRANSFERAIRTRACK datatransfer = new DATATRANSFERAIRTRACK();
            datatransfer.DATE_OF_INSERT = DateTime.Now;
            datatransfer.STATUS = AirtrackDatatranferStatus.InitialStatus;
            datatransfer.SRC = "SRX";
            return datatransfer;
        }
    }
}
