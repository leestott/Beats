﻿using Microsoft.Band;
using Microsoft.Band.Personalization;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI;

namespace MSBandAzure.Services.Fakes
{
    class ColorScale
    {
        public static Color ColorFromHSL(double h, double s, double l)
        {
            double r = 0, g = 0, b = 0;
            if (l != 0)
            {
                if (s == 0)
                    r = g = b = l;
                else
                {
                    double temp2;
                    if (l < 0.5)
                        temp2 = l * (1.0 + s);
                    else
                        temp2 = l + s - (l * s);

                    double temp1 = 2.0 * l - temp2;

                    r = GetColorComponent(temp1, temp2, h + 1.0 / 3.0);
                    g = GetColorComponent(temp1, temp2, h);
                    b = GetColorComponent(temp1, temp2, h - 1.0 / 3.0);
                }
            }
            return Color.FromArgb(255, (byte)(255 * r), (byte)(255 * g), (byte)(255 * b));

        }

        private static double GetColorComponent(double temp1, double temp2, double temp3)
        {
            if (temp3 < 0.0)
                temp3 += 1.0;
            else if (temp3 > 1.0)
                temp3 -= 1.0;

            if (temp3 < 1.0 / 6.0)
                return temp1 + (temp2 - temp1) * 6.0 * temp3;
            else if (temp3 < 0.5)
                return temp2;
            else if (temp3 < 2.0 / 3.0)
                return temp1 + ((temp2 - temp1) * ((2.0 / 3.0) - temp3) * 6.0);
            else
                return temp1;
        }
    }

    public class FakeBandPersonalizationManager : IBandPersonalizationManager
    {
        public Task<BandImage> GetMeTileImageAsync()
        {
            throw new NotImplementedException();
        }

        public Task<BandImage> GetMeTileImageAsync(CancellationToken cancel)
        {
            throw new NotImplementedException();
        }

        private BandTheme CreateBandTheme()
        {
            var rnd = new Random();
            var val = rnd.Next(0, 360) / 360.0;
            var col = ColorScale.ColorFromHSL(val, 0.5, 0.5);
            var hc = ColorScale.ColorFromHSL(val, 0.75, 0.75);
            var ll = ColorScale.ColorFromHSL(val, 0.25, 0.25);
            var hl = ColorScale.ColorFromHSL(val, 0.5, 0.75);
            var m = ColorScale.ColorFromHSL(val, 0.2, 0.4);
            var st = Color.FromArgb(255, 180, 180, 180);

            return new BandTheme
            {
                Base = new BandColor(col.R, col.G, col.B),
                HighContrast = new BandColor(hc.R, hc.G, hc.B),
                Lowlight = new BandColor(ll.R, ll.G, ll.B),
                Highlight = new BandColor(hl.R, hl.G, hl.B),
                Muted = new BandColor(m.R, m.G, m.B),
                SecondaryText = new BandColor(st.R, st.G, st.B),
            };
        }

        public Task<BandTheme> GetThemeAsync()
        {
            var tcs = new TaskCompletionSource<BandTheme>();
            Task.Run(() =>
            {
                Task.Delay(500);
                tcs.SetResult(CreateBandTheme());
            });
            
            return tcs.Task;
        }

        public Task<BandTheme> GetThemeAsync(CancellationToken cancel)
        {
            var tcs = new TaskCompletionSource<BandTheme>();
            Task.Run(() =>
            {
                Task.Delay(500, cancel);
                tcs.SetResult(CreateBandTheme());
            });

            return tcs.Task;
        }

        public Task SetMeTileImageAsync(BandImage image)
        {
            throw new NotImplementedException();
        }

        public Task SetMeTileImageAsync(BandImage image, CancellationToken cancel)
        {
            throw new NotImplementedException();
        }

        public Task SetThemeAsync(BandTheme theme)
        {
            throw new NotImplementedException();
        }

        public Task SetThemeAsync(BandTheme theme, CancellationToken cancel)
        {
            throw new NotImplementedException();
        }
    }
}
