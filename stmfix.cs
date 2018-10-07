
using System;
using System.IO;
using System.Text;
using TCore.StreamEx;

namespace TCore.StreamFix
{
    public class StreamFix
    {
        public static string FixBrokenUTF8Surrogates(string sInFile)
        {
            using (FileStream stm = new FileStream(sInFile, FileMode.Open, FileAccess.Read))
            {
                return FixBrokenUTF8Surrogates(stm);
            }
            
        }

        public static string FixBrokenUTF8Surrogates(Stream stmIn)
        {
            string sTempFile = TCore.Util.Filename.SBuildTempFilename("stmfix", "xml");

            BufferedStreamEx stmx = new BufferedStreamEx(stmIn, 0, stmIn.Length);
            Stream stmOut = new FileStream(sTempFile, FileMode.Create);

            byte b;
            bool fSkipNextNCRStart = false; // this means we have tried this &, and its not an NCR
            string sNCR;
            string sLeadNCR = null;

            while (stmx.ReadByte(out b) != SwapBuffer.ReadByteBufferState.SourceDataExhausted)
            {
                if (fSkipNextNCRStart || b != '&')
                {
                    stmOut.WriteByte(b);
                    fSkipNextNCRStart = false;
                    sLeadNCR = null;
                    continue;
                }

                if (!stmx.ReadNCR(out sNCR))
                {
                    fSkipNextNCRStart = true;
                    sLeadNCR = null;
                    continue;
                }

                if (sLeadNCR == null)
                {
                    int nNcr = UInt16.Parse(sNCR.Substring(2, sNCR.Length - 3));
                    if (nNcr < 32)
                    {
                        // don't touch control words
                        byte[] rgb = System.Text.Encoding.UTF8.GetBytes(sNCR);
                        stmOut.Write(rgb, 0, rgb.Length);
                        continue;
                    }

                    sLeadNCR = sNCR;
                    continue;
                }

                // YAY, encode
                // at this point we have 2 unicode characters and are ready to encode
                ushort[] rgw = new ushort[2];

                rgw[0] = UInt16.Parse(sLeadNCR.Substring(2, sLeadNCR.Length - 3));
                rgw[1] = UInt16.Parse(sNCR.Substring(2, sLeadNCR.Length - 3));
                string s = Convert.ToString(rgw);

                byte[] rgbUnicode = new byte[4];

                rgbUnicode[0] = (byte) (rgw[0] & 0x00ff);
                rgbUnicode[1] = (byte) ((rgw[0] & 0xff00) >> 8);
                rgbUnicode[2] = (byte)(rgw[1] & 0x00ff);
                rgbUnicode[3] = (byte)((rgw[1] & 0xff00) >> 8);

                byte[] rgbOut = System.Text.Encoding.Convert(Encoding.Unicode, Encoding.UTF8, rgbUnicode);
                stmOut.Write(rgbOut, 0, rgbOut.Length);
                
            }

            stmOut.Flush();
            stmOut.Close();
            return sTempFile;
        }
    }
}