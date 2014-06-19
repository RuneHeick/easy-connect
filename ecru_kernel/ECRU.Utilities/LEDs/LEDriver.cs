using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace ECRU.Utilities.LEDs
{
    public static class LEDriver
    {

        private static OutputPort LEDsouth = new OutputPort(SecretLabs.NETMF.Hardware.NetduinoPlus.Pins.GPIO_PIN_D8, true);
        private static OutputPort LEDsouthwest = new OutputPort(SecretLabs.NETMF.Hardware.NetduinoPlus.Pins.GPIO_PIN_D7, true);
        private static OutputPort LEDwest = new OutputPort(SecretLabs.NETMF.Hardware.NetduinoPlus.Pins.GPIO_PIN_D2, true);
        private static OutputPort LEDnorthwest = new OutputPort(SecretLabs.NETMF.Hardware.NetduinoPlus.Pins.GPIO_PIN_D3, true);
        private static OutputPort LEDnorth = new OutputPort(SecretLabs.NETMF.Hardware.NetduinoPlus.Pins.GPIO_PIN_D11, true);
        private static OutputPort LEDnortheast = new OutputPort(SecretLabs.NETMF.Hardware.NetduinoPlus.Pins.GPIO_PIN_D5, true);
        private static OutputPort LEDeast = new OutputPort(SecretLabs.NETMF.Hardware.NetduinoPlus.Pins.GPIO_PIN_D9, true);
        private static OutputPort LEDsoutheast = new OutputPort(SecretLabs.NETMF.Hardware.NetduinoPlus.Pins.GPIO_PIN_D6, true);

        private static OutputPort[] LEDs = { LEDsouth, LEDsouthwest, LEDwest, LEDnorthwest, LEDnorth, LEDnortheast, LEDeast, LEDsoutheast };

        private static object _defaultAnimationLock = new object();
        private static object _OneTimeLock = new object();
        private static bool _IsOneTimeActive = false;

        private static uint LEDAnimationParserIndex = 0;
        private static object LEDAnimationParserIndexLock = new object();

        private static Thread LEDThread = null;

        private static uint[][] _defaultAnimationUints =
        {
            new uint[] {1, 0, 0, 0, 0, 0, 0, 0},
            new uint[] {0, 1, 0, 0, 0, 0, 0, 0},
            new uint[] {0, 0, 1, 0, 0, 0, 0, 0},
            new uint[] {0, 0, 0, 1, 0, 0, 0, 0},
            new uint[] {0, 0, 0, 0, 1, 0, 0, 0},
            new uint[] {0, 0, 0, 0, 0, 1, 0, 0},
            new uint[] {0, 0, 0, 0, 0, 0, 1, 0},
            new uint[] {0, 0, 0, 0, 0, 0, 0, 1},
        };

        private static uint[][] _currentAnimationUints = 
        {
            new uint[] {1, 0, 0, 0, 0, 0, 0, 0},
            new uint[] {0, 1, 0, 0, 0, 0, 0, 0},
            new uint[] {0, 0, 1, 0, 0, 0, 0, 0},
            new uint[] {0, 0, 0, 1, 0, 0, 0, 0},
            new uint[] {0, 0, 0, 0, 1, 0, 0, 0},
            new uint[] {0, 0, 0, 0, 0, 1, 0, 0},
            new uint[] {0, 0, 0, 0, 0, 0, 1, 0},
            new uint[] {0, 0, 0, 0, 0, 0, 0, 1},
        };

        public static void OneTimeAnimation(uint[][] animation)
        {
            if (animation == null) return;
            lock (_OneTimeLock)
            {
                if (_IsOneTimeActive) return;

                lock (LEDAnimationParserIndexLock)
                {
                    _currentAnimationUints = animation;
                    _IsOneTimeActive = true;
                    LEDAnimationParserIndex = 0;
                }
            }
        }

        public static void startAnimation()
        {
            LEDThread = new Thread(LEDAnimationParser);
            LEDThread.Priority = ThreadPriority.Highest;
            LEDThread.Start();
        }

        public static void SetDefaultAnimation(uint[][] animation)
        {
            if (animation != null)
            {
                lock (_defaultAnimationLock)
                {
                    _defaultAnimationUints = animation;
                }
            }
        }       

        private static void LEDAnimationParser()
        {
            while (true)
            {

                lock (LEDAnimationParserIndexLock)
                {
                    lock (_OneTimeLock)
                    {
                        if (LEDAnimationParserIndex >= _currentAnimationUints.Length && _IsOneTimeActive)
                        {
                            lock (_defaultAnimationLock)
                            {
                                _currentAnimationUints = _defaultAnimationUints;
                            }

                            _IsOneTimeActive = false;
                        }
                        
                        
                    }

                    if (LEDAnimationParserIndex >= _currentAnimationUints.Length)
                    {
                        LEDAnimationParserIndex = 0;

                        lock (_defaultAnimationLock)
                        {
                            _currentAnimationUints = _defaultAnimationUints;
                        }
                    }

                    var frame = _currentAnimationUints[LEDAnimationParserIndex];

                    if (frame.Length == 8)
                    {
                        for (var i = 0; i < 8; i++)
                        {
                            LEDs[i].Write(frame[i] == 1);
                        }
                    }

                    LEDAnimationParserIndex++;
                }

                Thread.Sleep(500);
            }
        }

        public static void stopAnimation()
        {
            LEDThread.Abort();
        }

        public static void startBLEWhatever()
        {
            OneTimeAnimation(BLEwhatever);
        }

        private static uint[][] BLEwhatever = 
        {
            new uint[] {1, 0, 0, 0, 0, 0, 0, 1},
            new uint[] {0, 1, 0, 0, 0, 0, 1, 0},
            new uint[] {0, 0, 1, 0, 0, 1, 0, 0},
            new uint[] {0, 0, 0, 1, 1, 0, 0, 0},
            new uint[] {0, 0, 0, 1, 1, 0, 0, 0},
            new uint[] {0, 0, 1, 0, 0, 1, 0, 0},
            new uint[] {0, 1, 0, 0, 0, 0, 1, 0},
            new uint[] {1, 0, 0, 0, 0, 0, 0, 1},
        };

        public static void startstagetwo()
        {
            SetDefaultAnimation(stagetwo);
        }

        private static uint[][] stagetwo = 
        {
            new uint[] {0, 0, 0, 0, 0, 0, 0, 0},
            new uint[] {1, 0, 0, 0, 0, 0, 0, 0},
            new uint[] {1, 1, 0, 0, 0, 0, 0, 0},
            new uint[] {1, 1, 1, 0, 0, 0, 0, 0},
            new uint[] {1, 1, 1, 1, 0, 0, 0, 0},
            new uint[] {1, 1, 1, 1, 1, 0, 0, 0},
            new uint[] {1, 1, 1, 1, 1, 1, 0, 0},
            new uint[] {1, 1, 1, 1, 1, 1, 1, 0},
            new uint[] {1, 1, 1, 1, 1, 1, 1, 1},
            new uint[] {0, 1, 1, 1, 1, 1, 1, 1},
            new uint[] {0, 0, 1, 1, 1, 1, 1, 1},
            new uint[] {0, 0, 0, 1, 1, 1, 1, 1},
            new uint[] {0, 0, 0, 0, 1, 1, 1, 1},
            new uint[] {0, 0, 0, 0, 0, 1, 1, 1},
            new uint[] {0, 0, 0, 0, 0, 0, 1, 1},
            new uint[] {0, 0, 0, 0, 0, 0, 0, 1},
        };

        public static void startstagethree()
        {
            SetDefaultAnimation(stagethree);
        }

        private static uint[][] stagethree = 
        {
            new uint[] {1, 0, 1, 0, 1, 0, 1, 0},
            new uint[] {0, 1, 0, 1, 0, 1, 0, 1},
        };
    }
}
