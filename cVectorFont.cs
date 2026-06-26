namespace CSiNET8PluginExample1
{
    using System;

    public class cVectorFont
    {
        public enum TextAlignment
        {
            kTA_HLeft = 0,
            kTA_HCenter,
            kTA_HRight,
            kTA_VBottom,
            kTA_VCenter,
            kTA_VTop
        }

        protected const int VF_CHARACTERS = 96;
        protected const int VF_MOVES_PER_CHARACTER = 20;
        protected const double VF_ASPECT_RATIO = 0.5;

        struct VFData
        {
            public double XI;
            public double YI;
            public int pen;
        }

        VFData[,] vf;

        public cVectorFont()
        {
            vf = new VFData[VF_CHARACTERS + 1, VF_CHARACTERS + 1];
            Initialize();
        }

        protected void Initialize()
        {
            string[] VectorChar = new string[97];
            string tmpString;
            int i, j, k;

            VectorChar[1] = "923 000";
            VectorChar[2] = "323 422 343 442 462 572 482 382 272 362 342 923 000";
            VectorChar[3] = "383 272 262 162 172 272 683 572 562 462 472 572 923 000";
            VectorChar[4] = "133 172 573 532 643 042 063 662 923 000";
            VectorChar[5] = "133 532 642 552 152 062 172 572 383 322 923 000";
            VectorChar[6] = "163 262 372 282 182 072 162 573 132 423 522 632 542 442 332 422 923 000";
            VectorChar[7] = "623 172 282 482 472 052 032 122 422 532 632 923 000";
            VectorChar[8] = "483 372 362 262 272 372 923 000";
            VectorChar[9] = "683 472 252 232 412 602 923 000";
            VectorChar[10] = "283 472 652 632 412 203 923 000";
            VectorChar[11] = "333 372 563 142 053 652 543 162 923 000";
            VectorChar[12] = "333 372 053 652 923 000";
            VectorChar[13] = "213 322 332 232 222 322 923 000";
            VectorChar[14] = "053 652 923 000";
            VectorChar[15] = "223 322 332 232 222 923 000";
            VectorChar[16] = "682 923 000";
            VectorChar[17] = "123 522 632 672 582 182 072 032 122 133 572 923 000";
            VectorChar[18] = "323 522 423 482 372 923 000";
            VectorChar[19] = "173 282 582 672 662 032 022 622 923 000";
            VectorChar[20] = "033 122 522 632 642 552 352 553 662 672 582 282 172 923 000";
            VectorChar[21] = "423 622 523 582 042 642 923 000";
            VectorChar[22] = "033 122 522 632 642 552 152 182 582 923 000";
            VectorChar[23] = "583 262 152 042 032 122 522 632 642 552 152 923 000";
            VectorChar[24] = "023 682 182 072 923 000";
            VectorChar[25] = "123 522 632 642 452 252 162 172 282 482 572 562 452 253 042 032 122 923 000";
            VectorChar[26] = "023 442 662 672 582 182 072 062 152 552 923 000";
            VectorChar[27] = "223 322 332 232 222 253 352 362 262 252 923 000";
            VectorChar[28] = "213 322 332 232 222 322 253 352 362 262 252 923 000";
            VectorChar[29] = "533 152 572 923 000";
            VectorChar[30] = "143 542 563 162 923 000";
            VectorChar[31] = "043 452 062 352 042 923 000";
            VectorChar[32] = "223 322 243 342 352 562 572 482 182 072 923 000";
            VectorChar[33] = "553 542 342 352 552 662 572 272 062 032 222 522 632 923 000";
            VectorChar[34] = "052 382 652 622 043 642 923 000";
            VectorChar[35] = "082 482 572 562 452 552 642 632 522 022 053 452 923 000";
            VectorChar[36] = "573 482 182 072 032 122 522 632 923 000";
            VectorChar[37] = "082 582 672 632 522 022 923 000";
            VectorChar[38] = "082 582 353 052 023 622 923 000";
            VectorChar[39] = "082 682 453 052 923 000";
            VectorChar[40] = "353 552 642 632 522 122 032 072 182 482 572 923 000";
            VectorChar[41] = "082 053 652 683 622 923 000";
            VectorChar[42] = "123 522 323 382 183 582 923 000";
            VectorChar[43] = "033 122 422 532 582 383 682 923 000";
            VectorChar[44] = "082 053 352 683 352 622 923 000";
            VectorChar[45] = "083 022 622 923 000";
            VectorChar[46] = "082 352 682 622 923 000";
            VectorChar[47] = "082 622 682 582 923 000";
            VectorChar[48] = "033 072 182 582 672 632 522 122 032 923 000";
            VectorChar[49] = "082 582 672 662 552 052 923 000";
            VectorChar[50] = "033 072 182 582 672 632 522 122 032 343 432 622 923 000";
            VectorChar[51] = "082 582 672 662 552 052 453 442 622 923 000";
            VectorChar[52] = "033 122 522 632 642 552 152 062 072 182 482 572 923 000";
            VectorChar[53] = "323 382 083 682 923 000";
            VectorChar[54] = "083 032 122 522 632 682 923 000";
            VectorChar[55] = "083 052 322 652 682 923 000";
            VectorChar[56] = "083 022 352 622 682 923 000";
            VectorChar[57] = "682 083 622 923 000";
            VectorChar[58] = "323 352 082 683 352 923 000";
            VectorChar[59] = "083 682 022 622 253 452 923 000";
            VectorChar[60] = "423 222 282 482 923 000";
            VectorChar[61] = "083 622 923 000";
            VectorChar[62] = "223 422 482 282 923 000";
            VectorChar[63] = "153 372 552 923 000";
            VectorChar[64] = "013 612 923 000";
            VectorChar[65] = "453 362 372 272 262 362 923 000";
            VectorChar[66] = "623 532 422 122 032 042 152 452 542 652 543 532 923 000";
            VectorChar[67] = "072 043 252 552 642 632 522 122 032 923 000";
            VectorChar[68] = "553 152 042 032 122 522 632 923 000";
            VectorChar[69] = "623 672 643 452 152 042 032 122 522 632 923 000";
            VectorChar[70] = "623 122 032 042 262 362 353 352 923 000";
            VectorChar[71] = "623 532 522 122 032 252 662 923 000";
            VectorChar[72] = "153 672 643 452 152 042 032 122 522 632 552 923 000";
            VectorChar[73] = "623 532 042 032 352 923 000";
            VectorChar[74] = "623 532 642 552 153 043 032 122 923 000";
            VectorChar[75] = "623 532 042 032 172 462 642 662 923 000";
            VectorChar[76] = "623 032 042 353 923 000";
            VectorChar[77] = "623 042 032 332 442 362 572 482 562 662 652 552 923 000";
            VectorChar[78] = "623 042 032 332 442 362 572 482 562 662 923 000";
            VectorChar[79] = "123 522 632 672 643 452 152 042 032 122 923 000";
            VectorChar[80] = "623 122 032 042 152 452 543 562 552 923 000";
            VectorChar[81] = "123 522 632 672 643 452 152 042 032 122 253 152 162 662 923 000";
            VectorChar[82] = "623 122 032 042 152 452 543 532 572 482 562 652 923 000";
            VectorChar[83] = "143 672 643 452 152 042 032 122 522 632 552 352 923 000";
            VectorChar[84] = "623 032 042 232 352 923 000";
            VectorChar[85] = "623 432 442 353 252 552 643 662 923 000";
            VectorChar[86] = "623 352 342 142 152 372 552 923 000";
            VectorChar[87] = "623 352 342 142 152 372 562 652 572 482 923 000";
            VectorChar[88] = "623 342 352 372 582 662 642 562 342 923 000";
            VectorChar[89] = "623 342 352 372 582 662 552 342 352 562 652 923 000";
            VectorChar[90] = "623 032 042 352 623 923 000";
            VectorChar[91] = "272 662 282 372 482 572 923 000";
            VectorChar[92] = "223 622 232 332 442 542 923 000";
            VectorChar[93] = "473 523 532 633 923 000";
            VectorChar[94] = "243 632 293 243 142 082 093 193 293 433 532 622 652 923 000";
            VectorChar[95] = "253 172 282 582 672 632 522 122 032 923 000";
            VectorChar[96] = "233 242 443 442 442 623 342 923 000";

            for (i = 1; i <= VF_CHARACTERS; i++)
            {
                for (j = 1; j <= VF_MOVES_PER_CHARACTER; j++)
                {
                    k = (4 * j) - 4; // k = (4 * j) - 3;
                    tmpString = VectorChar[i].Substring(k + 2, 1);
                    vf[i, j].pen = int.Parse(tmpString);
                    if (vf[i, j].pen == 0)
                        break;
                    tmpString = VectorChar[i].Substring(k, 1);
                    vf[i, j].XI = double.Parse(tmpString);
                    vf[i, j].XI *= VF_ASPECT_RATIO;
                    tmpString = VectorChar[i].Substring(k + 1, 1);
                    vf[i, j].YI = double.Parse(tmpString);
                }
            }
        }

        public void FillTextVertices(string inStr, double CharHeight, TextAlignment HAlignment, TextAlignment VAlignment, ref double[] tX, ref double[] tY)
        {
            int i, j, NumChars, NumPts, pos;
            const double CharWidth = VF_ASPECT_RATIO * 9; // 9 is initial height

            NumPts = 0;
            NumChars = inStr.Length;

            for (pos = 0; pos <= NumChars - 1; pos++)
            {
                if (Convert.ToByte(inStr.Substring(pos, 1).FirstOrDefault()) == 13)
                {
                    //do nothing
                }
                else if (Convert.ToByte(inStr.Substring(pos, 1).FirstOrDefault()) == 10)
                {
                    //do nothing
                }
                else
                {
                    i = Convert.ToByte(inStr.Substring(pos, 1).FirstOrDefault()) - 31;
                    for (j = 1; j <= VF_MOVES_PER_CHARACTER; j++)
                    {
                        if (vf[i, j].pen == 2)
                            NumPts += 2;
                    }
                }
            }

            tX = new double[NumPts + 1];
            tY = new double[NumPts + 1];
            int LineStart = 0;
            int LineEnd = 1;
            double YOffset = 0.0;
            double CharStartX = 0.0;
            double XCurrent = 0.0;
            double YCurrent = 0.0;
            double XStart = 0.0;
            double YStart = 0.0;

            for (pos = 0; pos <= NumChars - 1; pos++)
            {
                if (Convert.ToByte(inStr.Substring(pos, 1).FirstOrDefault()) == 13)
                {
                    YOffset -= 9.0 + 2.0; // 9.0 is initial char height, 2.0 is spacing
                    CharStartX = 0.0;
                }
                else if (Convert.ToByte(inStr.Substring(pos, 1).FirstOrDefault()) == 10)
                {
                }
                else
                {
                    i = Convert.ToByte(inStr.Substring(pos, 1).FirstOrDefault()) - 31; // ASCII 32 is VectorFont(1)   

                    if (vf[i, 1].pen == 2)
                    {
                        XStart = CharStartX;
                        YStart = 2.0 + YOffset;
                    }

                    for (j = 1; j <= VF_MOVES_PER_CHARACTER; j++)
                    {
                        if (vf[i, j].pen == 0)
                            break;

                        if (vf[i, j].pen == 2)
                        {
                            XCurrent = vf[i, j].XI + CharStartX;
                            YCurrent = vf[i, j].YI + YOffset;

                            tX[LineStart] = XStart; tX[LineEnd] = XCurrent;
                            tY[LineStart] = YStart; tY[LineEnd] = YCurrent;

                            LineStart += 2; LineEnd += 2;

                            XStart = XCurrent;
                            YStart = YCurrent;
                        }
                        else if (vf[i, j].pen == 3)
                        {
                            XStart = vf[i, j].XI + CharStartX;
                            YStart = vf[i, j].YI + YOffset;
                        }
                    }
                    CharStartX += CharWidth;
                }
            }

            double ScaleFactor = CharHeight / 9.0;
            double OffsetX = 0.0;
            var OffsetY = 0.0;

            switch (HAlignment)
            {
                case TextAlignment.kTA_HCenter:
                    {
                        OffsetX = -NumChars * CharWidth / 2.0;
                        break;
                    }

                case TextAlignment.kTA_HRight:
                    {
                        OffsetX = -NumChars * CharWidth;
                        break;
                    }
            }

            switch (VAlignment)
            {
                case TextAlignment.kTA_VCenter:
                    {
                        OffsetY = -9 / 2.0;
                        break;
                    }

                case TextAlignment.kTA_VTop:
                    {
                        OffsetY = 0.0;
                        break;
                    }

                case TextAlignment.kTA_VBottom:
                    {
                        OffsetY = -9.0;
                        break;
                    }
            }

            for (i = 0; i <= NumPts - 1; i++)
            {
                tX[i] += OffsetX; tY[i] += OffsetY;
                tX[i] *= ScaleFactor; tY[i] *= ScaleFactor;
            }
        }
    }
}

