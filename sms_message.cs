﻿
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Execution;


namespace wp2droidMsg
{
    public class SmsMessage
    {
        private int m_protocol;
        private string m_sAddress;
        private ulong m_ulUnixDate;
        private int m_type;
        private string m_sSubject;
        private string m_sBody;
        private string m_sToa;
        private string m_sSc_Toa;
        private string m_sServiceCenter;
        private int m_nRead;
        private int m_nStatus;
        private int m_nLocked;
        private string m_sReadableDate;
        string m_sContactName;
        private string m_sDateSent;

        public SmsMessage() { }

        public string Text => m_sBody;
        public string To => m_sAddress;
        public string Date => MmsMessage.ReadableDateFromWindowsTimestamp(SmsMessage.MsecWinFromSecondsUnix(m_ulUnixDate));

        public static List<SmsMessage> ReadMessagesFromWpXml(XmlReader xr)
        {
            if (!xr.Read())
                return null;

            List<SmsMessage> smses = new List<SmsMessage>();

            bool fFoundMessageArray = false;
            bool fValidExit = false;

            while (true)
            {
                XmlNodeType nt = xr.NodeType;

                if (nt == XmlNodeType.Element)
                {
                    if (!fFoundMessageArray && xr.Name == "ArrayOfMessage")
                    {
                        xr.ReadStartElement();
                        fFoundMessageArray = true;
                        continue;
                    }

                    // only other valid element is message
                    if (xr.Name != "Message")
                        throw new Exception($"Illegal element {xr.Name} under ArrayOfMessages");

                    smses.Add(SmsMessage.CreateFromWindowsPhoneXmlReader(xr));
                    continue;
                }

                if (nt == XmlNodeType.EndElement)
                {
                    if (xr.Name == "ArrayOfMessage")
                    {
                        fValidExit = true;
                        break; // yay done
                    }

                    throw new Exception($"end element {xr.Name} unexpected");
                }

                if (nt == XmlNodeType.Attribute)
                    throw new Exception($"attribute unexpected per schema");

                // all others are just skipped
                if (!xr.Read())
                    break;
            }

            if (!fValidExit)
                throw new Exception("end of file before end message array");

            return smses;
        }
        #region Comparators
        public override bool Equals(Object obj)
        {
            return obj is SmsMessage && this == (SmsMessage) obj;
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public static bool operator !=(SmsMessage left, SmsMessage right)
        {
            return !(left == right);
        }

        public static bool operator ==(SmsMessage left, SmsMessage right)
        {
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                if (ReferenceEquals(left, right))
                    return true;
                return false;
            }

            if (left.m_protocol != right.m_protocol)
                return false;
            if (left.m_protocol != right.m_protocol)
                return false;
            if (left.m_sAddress != right.m_sAddress)
                return false;
            if (left.m_ulUnixDate != right.m_ulUnixDate)
                return false;
            if (left.m_type != right.m_type)
                return false;
            if (left.m_sSubject != right.m_sSubject)
                return false;
            if (left.m_sBody != right.m_sBody)
                return false;
            if (left.m_sToa != right.m_sToa)
                return false;
            if (left.m_sSc_Toa != right.m_sSc_Toa)
                return false;
            if (left.m_sServiceCenter != right.m_sServiceCenter)
                return false;
            if (left.m_nRead != right.m_nRead)
                return false;
            if (left.m_nStatus != right.m_nStatus)
                return false;
            if (left.m_nLocked != right.m_nLocked)
                return false;
            if (left.m_sReadableDate != right.m_sReadableDate)
                return false;
            if (left.m_sContactName != right.m_sContactName)
                return false;
            if (left.m_sDateSent != right.m_sDateSent)
                return false;

            return true;
        }
        #endregion

        #region XML I/O

        public void WriteToDroidXml(XmlWriter xw)
        {
            xw.WriteStartElement("sms");
            xw.WriteAttributeString("protocol", m_protocol.ToString());
            xw.WriteAttributeString("address", m_sAddress ?? "");
            xw.WriteAttributeString("date", m_ulUnixDate.ToString());
            xw.WriteAttributeString("type", m_type.ToString());
            xw.WriteAttributeString("subject", m_sSubject ?? "null");
            xw.WriteAttributeString("body", m_sBody ?? "null" );
            xw.WriteAttributeString("toa", m_sToa ?? "null");
            xw.WriteAttributeString("sc_toa", m_sSc_Toa ?? "null");
            xw.WriteAttributeString("service_center", m_sServiceCenter ?? "null");
            xw.WriteAttributeString("read", m_nRead.ToString());
            xw.WriteAttributeString("status", m_nStatus.ToString());
            xw.WriteAttributeString("locked", m_nLocked.ToString());
            if (m_sReadableDate != null)
                xw.WriteAttributeString("readable_date", m_sReadableDate);
            if (m_sContactName != null)
                xw.WriteAttributeString("contact_name", m_sContactName);
            if (m_sDateSent != null)
                xw.WriteAttributeString("date_sent", m_sDateSent);
            xw.WriteEndElement();
        }

        /*----------------------------------------------------------------------------
        	%%Function: SecondsUnixFromMsecWin
        	%%Qualified: wp2droidMsg.SmsMessage.SecondsUnixFromMsecWin
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public static ulong SecondsUnixFromMsecWin(ulong nWpDate)
        {
            return (((nWpDate / (10 * 100)) - 116444736000000) + 5) / 10;
        }

        public static ulong MsecWinFromSecondsUnix(ulong msecUnix)
        {
            return 10UL * 100UL * (msecUnix * 10UL + 116444736000000UL);
        }

        public static SmsMessage CreateFromDroidXmlReader(XmlReader xr)
        {
            SmsMessage sms = new SmsMessage();

            if (xr.Name != "sms")
                throw new Exception("not at the correct node");

            bool fEmptySmsElement = xr.IsEmptyElement;

            if (!XmlIO.Read(xr))
                throw new Exception("nothing to read");

            while (true)
            {
                XmlIO.SkipNonContent(xr);
                XmlNodeType nt = xr.NodeType;

                if (nt == XmlNodeType.Element)
                    throw new Exception($"unexpected element {xr.Name} under sms element");

                if (nt == XmlNodeType.EndElement)
                {
                    if (xr.Name != "sms")
                        throw new Exception("unmatched sms element");

                    xr.ReadEndElement();
                    break;
                }

                if (xr.NodeType != XmlNodeType.Attribute)
                    throw new Exception("unexpected non attribute on <sms> element");

                while (true)
                {
                    // consume all the attributes
                    ParseDroidSmsAttribute(xr, sms);
                    if (!xr.MoveToNextAttribute())
                    {
                        if (fEmptySmsElement)
                        {
                            xr.Read();  // get past the attribute
                            return sms;
                        }

                        break; // continue till we find the end sms element
                    }

                    // otherwise just continue...
                }

                if (!XmlIO.Read(xr))
                    throw new Exception("never encountered end sms element");
            }

            return sms;
        }

        public static void ParseDroidSmsAttribute(XmlReader xr, SmsMessage sms)
        {
            switch (xr.Name)
            {
                case "protocol":
                    sms.m_protocol = XmlIO.ReadGenericIntElement(xr, "protocol") ?? 0;
                    break;
                case "address":
                    sms.m_sAddress = XmlIO.ReadGenericStringElement(xr, "address");
                    break;
                case "date":
                    sms.m_ulUnixDate = XmlIO.ReadGenericUInt64Element(xr, "date") ?? 0;
                    break;
                case "type":
                    sms.m_type = XmlIO.ReadGenericIntElement(xr, "type") ?? 2; // default is sent?
                    break;
                case "subject":
                    sms.m_sSubject = XmlIO.ReadGenericStringElement(xr, "subject");
                    break;
                case "body":
                    sms.m_sBody = XmlIO.ReadGenericStringElement(xr, "body");
                    break;
                case "toa":
                    sms.m_sToa = XmlIO.ReadGenericNullableStringElement(xr, "toa");
                    break;
                case "sc_toa":
                    sms.m_sSc_Toa = XmlIO.ReadGenericNullableStringElement(xr, "sc_toa");
                    break;
                case "service_center":
                    sms.m_sServiceCenter = XmlIO.ReadGenericNullableStringElement(xr, "service_center");
                    break;
                case "read":
                    sms.m_nRead = XmlIO.ReadGenericIntElement(xr, "read") ?? 0;
                    break;
                case "status":
                    sms.m_nStatus = XmlIO.ReadGenericIntElement(xr, "status") ?? -1;
                    break;
                case "locked":
                    sms.m_nLocked = XmlIO.ReadGenericIntElement(xr, "locked") ?? 0;
                    break;
                case "date_sent":
                    sms.m_sDateSent = XmlIO.ReadGenericStringElement(xr, "date_sent");
                    break;
                case "readable_date":
                    sms.m_sReadableDate = XmlIO.ReadGenericNullableStringElement(xr, "readable_date");
                    break;
                case "contact_name":
                    sms.m_sContactName = XmlIO.ReadGenericNullableStringElement(xr, "contact_name");
                    break;
            }
        }

        /*----------------------------------------------------------------------------
        	%%Function: CreateFromXmlReader
        	%%Qualified: wp2droidMsg.SmsMessage.CreateFromXmlReader
        	%%Contact: rlittle
        	
            given an XmlReader that is current positioned at the start of an
            <Message> element, parse until the </Message> and return a new SmsMessage.
        ----------------------------------------------------------------------------*/
        public static SmsMessage CreateFromWindowsPhoneXmlReader(XmlReader xr)
        {
            SmsMessage sms = new SmsMessage();

            sms.m_type = 2;    // absence of the IsIncoming element meants it is a sent text, hence 2
            sms.m_nStatus = -1; // always -1 as far as I can tell

            if (xr.Name != "Message")
                throw new Exception("not at the correct node");

            // finish this start element
            xr.ReadStartElement();

            while(true)
            {
                XmlNodeType nt = xr.NodeType;

                switch (nt)
                {
                    case XmlNodeType.EndElement:
                        if (xr.Name != "Message")
                            throw new Exception("encountered end node not matching <Message>");
                        xr.ReadEndElement();
                        return sms;

                    case XmlNodeType.Element:
                        ParseWPMessageElement(xr, sms);
                        // we should be advanced past the element...
                        continue;
                    case XmlNodeType.Attribute:
                        throw new Exception("there should be no attributes in this schema");
                }
                // all others just get skipped (whitespace, cdata, etc...)
                if (!xr.Read())
                    break;
            } 

            throw new Exception("hit EOF before finding end Message element");
        }

        /*----------------------------------------------------------------------------
        	%%Function: ParseWPMessageElement
        	%%Qualified: wp2droidMsg.SmsMessage.ParseWPMessageElement
        	%%Contact: rlittle
        	
            the parser should be positioned at an xml start element, ready for us
            to parse the element into the given sms
        ----------------------------------------------------------------------------*/
        static void ParseWPMessageElement(XmlReader xr, SmsMessage sms)
        {
            switch (xr.Name)
            {
                case "Recepients":
                    string[] rgsRecipients = XmlIO.RecepientsReadElement(xr);
                    if (rgsRecipients != null)
                    {
                        if (rgsRecipients.Length != 1)
                            throw new Exception("There can be only one receipient in an SMS message");

                        sms.m_sAddress = rgsRecipients[0];
                    }

                    // if null, then don't change m_sAddress...
                    break;
                case "Body":
                    sms.m_sBody = XmlIO.ReadGenericStringElement(xr, "Body");
                    break;
                case "IsIncoming":
                    bool? fIncoming = XmlIO.ReadGenericBoolElement(xr, "IsIncoming");
                    if (fIncoming == null)
                        break; // no change
                    if ((bool) fIncoming)
                        sms.m_type = 1;
                    else
                        sms.m_type = 2;
                    break;
                case "IsRead":
                    bool? fRead = XmlIO.ReadGenericBoolElement(xr, "IsRead");
                    if (fRead == null)
                        break;

                    sms.m_nRead = ((bool) fRead) ? 1 : 0;
                    break;
                case "Attachments":
                    xr.Skip();
                    // TODO TEST THIS!!!
                    break;
                case "LocalTimestamp":
                    ulong? ulRead = XmlIO.ReadGenericUInt64Element(xr, "LocalTimestamp");
                    if (ulRead == null)
                        break;

                    sms.m_ulUnixDate = SecondsUnixFromMsecWin((ulong) ulRead);
                    break;
                case "Sender":
                    string sSender = XmlIO.ReadGenericStringElement(xr, "Sender");
                    if (sSender == null)
                        break;
                    sms.m_sAddress = sSender;
                    break;
                default:
                    throw new Exception("Unknown element in Message");
            }
        }
        #endregion

        #region TESTS

        [TestCase(131271698820000000UL, 1482696282000UL)]
        [TestCase(131272856426710000UL, 1482812042671UL)]
        [TestCase(131305673581450420UL, 1486093758145UL)]
        [TestCase(131777420586331428UL, 1533268458633UL)]
        [TestCase(131777420698276081UL, 1533268469828UL)]
        [Test]
        public static void TestSecondsUnixFromMsecWin(ulong nWpDate, ulong nExpected)
        {
            Assert.AreEqual(nExpected, SecondsUnixFromMsecWin(nWpDate));
        }


        static SmsMessage SmsCreateFromString(string s)
        {
            string[] rgs = s.Split('|');
            SmsMessage sms = new SmsMessage();

            sms.m_protocol = int.Parse(rgs[0]);
            sms.m_sAddress = XmlIO.FromNullable(rgs[1]);
            sms.m_ulUnixDate = UInt64.Parse(rgs[2]);
            sms.m_type = int.Parse(rgs[3]);
            sms.m_sSubject = XmlIO.FromNullable(rgs[4]);
            sms.m_sBody = XmlIO.FromNullable(rgs[5]);
            sms.m_sToa = XmlIO.FromNullable(rgs[6]);
            sms.m_sSc_Toa = XmlIO.FromNullable(rgs[7]);
            sms.m_sServiceCenter = XmlIO.FromNullable(rgs[8]);
            sms.m_nRead = int.Parse(rgs[9]);
            sms.m_nStatus = int.Parse(rgs[10]);
            sms.m_nLocked = int.Parse(rgs[11]);
            sms.m_sReadableDate = XmlIO.FromNullable(rgs[12]);
            sms.m_sContactName = XmlIO.FromNullable(rgs[13]);

            return sms;
        }

        // Order is:    nProtocol|sAddress|nUnixDate|nType|sSubject|sBody|sToa|sSc_toa|sServiceCenter|nRead|nStatus|nLocked|nDateSent|sReadableDate|sContactName
        [TestCase(null, "0|<null>|0|0|<null>|<null>|<null>|<null>|<null>|0|0|0|<null>|<null>", null)]
        [TestCase("<Message><Recepients><string>+1234</string></Recepients></Message>", "0|+1234|0|2|<null>|<null>|<null>|<null>|<null>|0|-1|0|<null>|<null>", null)]
        [TestCase("<Message><Body>testing</Body></Message>", "0|<null>|0|2|<null>|testing|<null>|<null>|<null>|0|-1|0|<null>|<null>", null)]
        [TestCase("<Message><IsIncoming>true</IsIncoming></Message>", "0|<null>|0|1|<null>|<null>|<null>|<null>|<null>|0|-1|0|<null>|<null>", null)]
        [TestCase("<Message><IsRead>1</IsRead></Message>", "0|<null>|0|2|<null>|<null>|<null>|<null>|<null>|1|-1|0|<null>|<null>", null)]
        [TestCase("<Message><LocalTimestamp>131777420698276081</LocalTimestamp></Message>", "0|<null>|1533268469828|2|<null>|<null>|<null>|<null>|<null>|0|-1|0|<null>|<null>", null)]
        [TestCase("<Message><Sender>+4321</Sender></Message>", "0|+4321|0|2|<null>|<null>|<null>|<null>|<null>|0|-1|0|<null>|<null>", null)]
        [TestCase("<Message><Sender>+4321</Sender><LocalTimestamp>131777420698276081</LocalTimestamp></Message>", "0|+4321|1533268469828|2|<null>|<null>|<null>|<null>|<null>|0|-1|0|<null>|<null>", null)]
        [TestCase("<Message><Sender>+4321</Sender> <LocalTimestamp>131777420698276081</LocalTimestamp></Message>", "0|+4321|1533268469828|2|<null>|<null>|<null>|<null>|<null>|0|-1|0|<null>|<null>", null)]
        [TestCase("<Message><Sender>+4321</Sender>\r\n <LocalTimestamp>131777420698276081</LocalTimestamp></Message>", "0|+4321|1533268469828|2|<null>|<null>|<null>|<null>|<null>|0|-1|0|<null>|<null>", null)]
        [TestCase("<Message><Sender>+4321</Sender><!-- comment here --> <LocalTimestamp>131777420698276081</LocalTimestamp></Message>", "0|+4321|1533268469828|2|<null>|<null>|<null>|<null>|<null>|0|-1|0|<null>|<null>", null)]
        [TestCase("<Message><Body><![CDATA[testing]]></Body></Message>", "0|<null>|0|2|<null>|testing|<null>|<null>|<null>|0|-1|0|<null>|<null>", null)]
        [TestCase("<Message><Body>testing\nnewline</Body></Message>", "0|<null>|0|2|<null>|testing\nnewline|<null>|<null>|<null>|0|-1|0|<null>|<null>", null)]
        [TestCase("<Message><Recepients><string>+1234</string></Recepients><Body>foo&amp;bar</Body></Message>", "0|+1234|0|2|<null>|foo&bar|<null>|<null>|<null>|0|-1|0|<null>|<null>", null)]
        [TestCase("<Message><Body>testing</Message>", "0|<null>|0|2|<null>|testing|<null>|<null>|<null>|0|-1|0|<null>|<null>", "System.Xml.XmlException")]
        [TestCase("<Message><Recepients><string>+14255551212</string></Recepients><Body>:-)</Body><IsIncoming>false</IsIncoming><IsRead>true</IsRead><Attachments /><LocalTimestamp>131777420698276081</LocalTimestamp><Sender /></Message>", "0|+14255551212|1533268469828|2|<null>|:-)|<null>|<null>|<null>|1|-1|0|<null>|<null>", null)]
        [TestCase("<Message></Message>", "0|<null>|0|2|<null>|<null>|<null>|<null>|<null>|0|-1|0|<null>|<null>", null)]
        [TestCase("<Message></Message>", "0|<null>|0|2|<null>|<null>|<null>|<null>|<null>|0|-1|0|<null>|<null>", null)]
        [TestCase("<Message><Unknown>foobar</Unknown></Message>", "0|<null>|0|2|<null>|<null>|<null>|<null>|<null>|0|-1|0|<null>|<null>", "System.Exception")]
        [Test]
        public static void TestCreateFromWindowsPhoneXmlReader(string sIn, string sSmsExpected,
            string sExpectedException)
        {
            SmsMessage smsExpected = SmsCreateFromString(sSmsExpected);

            if (sIn == null)
            {
                Assert.AreEqual(smsExpected, new SmsMessage());
                return;
            }

            XmlReader xr = XmlIO.SetupXmlReaderForTest(sIn);

            try
            {
                XmlIO.AdvanceReaderToTestContent(xr, "Message");
            }
            catch (Exception e)
            {
                if (sExpectedException != null)
                    return;

                throw e;
            }

            if (sExpectedException == null)
                Assert.AreEqual(smsExpected, CreateFromWindowsPhoneXmlReader(xr));
            if (sExpectedException != null)
                XmlIO.RunTestExpectingException(() => CreateFromWindowsPhoneXmlReader(xr), sExpectedException);
        }

        [Test]
        public static void TestXmlReaderFull()
        {
            string sTest =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><ArrayOfMessage xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><Message><Recepients><string>+14254956002</string></Recepients><Body>:-)</Body><IsIncoming>false</IsIncoming><IsRead>true</IsRead><Attachments /><LocalTimestamp>131777420698276081</LocalTimestamp><Sender /></Message></ArrayOfMessage>";

            XmlReader xr = XmlIO.SetupXmlReaderForTest(sTest);
            XmlIO.AdvanceReaderToTestContent(xr, "Message");
        }

        [TestCase(131271698820000000UL)]
        [TestCase(231271698820000000UL)]
        [TestCase(2312716988200000000UL)]
        [Test]
        public static void TestDateConversion(ulong ul)
        {
            ulong ulUnix = SecondsUnixFromMsecWin(ul);
            Assert.AreEqual(ul, MsecWinFromSecondsUnix(ulUnix));
        }
        #endregion
    }
}
