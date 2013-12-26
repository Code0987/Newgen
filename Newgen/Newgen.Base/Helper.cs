using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Newgen.Base
{
    /// <summary>
    /// Helper
    /// </summary>
    public static partial class Helper
    {
        #region DoubleAnimation

        public static void Animate(DependencyObject dependencyobject, DependencyProperty dependencyproperty, int duration, double to, bool usedlbonly = false)
        {
            Animate(dependencyobject, dependencyproperty, duration, null, to, null, 0.0, 0.0, false, 1.0, null, FillBehavior.HoldEnd, null, null, usedlbonly);
        }

        public static void Animate(DependencyObject dependencyobject, DependencyProperty dependencyproperty, int duration, double to, double accelerationratio, double deaccelerationratio, bool usedlbonly = false)
        {
            Animate(dependencyobject, dependencyproperty, duration, null, to, null, accelerationratio, deaccelerationratio, false, 1.0, null, FillBehavior.HoldEnd, null, null, usedlbonly);
        }

        public static void Animate(DependencyObject dependencyobject, DependencyProperty dependencyproperty, int duration, double @from, double to, bool usedlbonly = false)
        {
            Animate(dependencyobject, dependencyproperty, duration, @from, to, null, 0.0, 0.0, false, 1.0, null, FillBehavior.HoldEnd, null, null, usedlbonly);
        }

        public static void Animate(DependencyObject dependencyobject, DependencyProperty dependencyproperty, int duration, double @from, double to, double accelerationratio, double deaccelerationratio, bool usedlbonly = false)
        {
            Animate(dependencyobject, dependencyproperty, duration, @from, to, null, accelerationratio, deaccelerationratio, false, 1.0, null, FillBehavior.HoldEnd, null, null, usedlbonly);
        }

        public static void Animate(DependencyObject dependencyobject, DependencyProperty dependencyproperty, int duration, double @from, double to, EventHandler callback, bool usedlbonly = false)
        {
            Animate(dependencyobject, dependencyproperty, duration, @from, to, null, 0.0, 0.0, false, 1.0, null, FillBehavior.HoldEnd, callback, null, usedlbonly);
        }

        public static void Animate(DependencyObject dependencyobject,
      DependencyProperty dependencyproperty,
      double duration,
      System.Nullable<double> @from,
      System.Nullable<double> to,
      System.Nullable<double> by,
      double accelerationratio,
      double deaccelerationratio,
      bool additive,
      double speedratio,
      IEasingFunction easing,
      FillBehavior fillbehavior,
      EventHandler callback,
      System.Nullable<int> framerate,
      bool usedlbonly = false)
        {
            DoubleAnimation doubleAnimation = new DoubleAnimation()
            {
                From = @from,
                To = to,
                Duration = new Duration(TimeSpan.FromMilliseconds(duration)),
                EasingFunction = easing,
                AccelerationRatio = accelerationratio,
                DecelerationRatio = deaccelerationratio,
                FillBehavior = fillbehavior,
                SpeedRatio = speedratio,
                IsAdditive = additive,
                By = by
            };

            if (!usedlbonly)
            {
                Storyboard storyboard = new Storyboard();
                Storyboard.SetTarget(doubleAnimation, dependencyobject);
                Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(dependencyproperty));
                storyboard.Children.Add(doubleAnimation);
                Storyboard.SetDesiredFrameRate(storyboard, framerate);
                if (callback != null) { storyboard.Completed += callback; }
                storyboard.Begin();
            }
            else
            {
                try { ((IAnimatable)dependencyobject).BeginAnimation(dependencyproperty, doubleAnimation); }
                catch { }
            }
        }

        #endregion DoubleAnimation

        #region Timer

        /// <summary>
        /// Runs specified <see cref="Action"/> for given count with timeframe as tick time (duration/delay) for each count. Use -1 for infinite. Although you can control timer manually.
        /// </summary>
        /// <param name="work">The work.</param>
        /// <param name="count">The count.</param>
        /// <param name="timeframe">The timeframe.</param>
        /// <returns></returns>
        public static DispatcherTimer RunFor(Action work, int count, double timeframe)
        {
            DispatcherTimer timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(timeframe)
            };
            int rcount = 0;
            timer.Tick += (o, e) =>
            {
                if (rcount <= count || count == -1)
                {
                    work.Invoke();
                }
                else
                {
                    timer.Stop();
                    timer = null;
                }

                rcount++;
            };

            timer.Start();
            return timer;
        }

        /// <summary>
        /// Delays the specified work.
        /// </summary>
        /// <param name="work">The work.</param>
        /// <param name="time">The time.</param>
        public static void Delay(Action work, double time)
        {
            DispatcherTimer timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(time)
            };
            timer.Tick += (o, e) =>
            {
                work.Invoke();
                timer.Stop();
                timer = null;
            };
            timer.Start();
        }

        #endregion Timer

        #region En/De

        private static bool useHashing = true;
        private static string CryptKey = "Ftware.Apps.Newgen";

        public static string Encrypt(string toEncrypt) { if (string.IsNullOrEmpty(toEncrypt)) { return ""; } byte[] keyArray; byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt); System.Configuration.AppSettingsReader settingsReader = new AppSettingsReader(); string key = CryptKey; if (useHashing) { MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider(); keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key)); hashmd5.Clear(); } else { keyArray = UTF8Encoding.UTF8.GetBytes(key); } TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider(); tdes.Key = keyArray; tdes.Mode = CipherMode.ECB; tdes.Padding = PaddingMode.PKCS7; ICryptoTransform cTransform = tdes.CreateEncryptor(); byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length); tdes.Clear(); return System.Convert.ToBase64String(resultArray, 0, resultArray.Length); }

        public static string Decrypt(string cipherString) { if (string.IsNullOrEmpty(cipherString)) { return ""; } byte[] keyArray; byte[] toEncryptArray = System.Convert.FromBase64String(cipherString); System.Configuration.AppSettingsReader settingsReader = new AppSettingsReader(); string key = CryptKey; if (useHashing) { MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider(); keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key)); hashmd5.Clear(); } else { keyArray = UTF8Encoding.UTF8.GetBytes(key); } TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider(); tdes.Key = keyArray; tdes.Mode = CipherMode.ECB; tdes.Padding = PaddingMode.PKCS7; ICryptoTransform cTransform = tdes.CreateDecryptor(); byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length); tdes.Clear(); return UTF8Encoding.UTF8.GetString(resultArray); }

        #endregion En/De
    }
}