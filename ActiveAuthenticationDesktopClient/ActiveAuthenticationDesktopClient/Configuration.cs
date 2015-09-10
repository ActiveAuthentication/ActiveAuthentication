using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ActiveAuthenticationDesktopClient
{
    /// <summary>
    /// Class that contains variables used throughout the application
    /// </summary>
    public static class Configuration
    {
        public static DataSet dsImportedConfig;

        public static string configPath
        {
            get
            {
                return System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Louisiana Tech University\Active Authentication";
            }
        }

        public static string configFileName
        {
            get
            {
                return configPath + @"\config.xml";
            }
        }

        public static string profilePath
        {
            get
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments)+@"\..\..\"+ Environment.UserName + @"\AppData\Roaming\Louisiana Tech University\Active Authentication\Profiles";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            }
        }

        public static string verifierThresholdsFile
        {
            get
            {
                return configPath + @"\VerifierThresholds.xml";
            }
        }

        public static string fuserThresholdsFile
        {
            get
            {
                return configPath + @"\FuserThreshold.xml";
            }
        }

        public static string messagingAddressFile
        {
            get
            {
                return configPath + @"\MessagingAddresses.xml";
            }
        }

        public static string validKeyboardsFile
        {
            get
            {
                return configPath + @"\ValidKeyboards.xml";
            }
        }

        public static string ownerFile
        {
            get
            {
                return configPath + @"\Owner.xml";
            }
        }

        public static int keyEventsPerSample
        {
            get
            {
                return 1100;
            }
        }

        public static int requiredTrainingSamples
        {
            get
            {
                return 6;
            }
        }

        public static int slidingWindowJump
        {
            get
            {
                return 110;
            }
        }

        public static double numSkippedSamples
        {
            get
            {
                return keyEventsPerSample / slidingWindowJump;
            }
        }

        public static bool useSlidingWindow
        {
            get
            {
                return true;
            }
        }

        public static int absoluteMinMatchingPairs
        {
            get
            {
                return 0;
            }
        }
        public static double absoluteMaxRatioForValidMatch
        {
            get
            {
                return 1.45;
            }
        }

        public static int relativeMinMatchingPairs
        {
            get
            {
                return 0;
            }
        }

        public static int similarityMinMatchingPairs
        {
            get
            {
                return 10;
            }
        }

        public static double similarityMaxDifferenceForValidMatch
        {
            get
            {
                return 200;
            }
        }

        public static int scaledEuclideanMinMatchingPairs
        {
            get
            {
                return 15;
            }
        }

        public static int scaledManhattanMinMatchingPairs
        {
            get
            {
                return 15;
            }
        }

        public static double outlierDetectionRadius
        {
            get
            {
                return 100;
            }
        }

        public static double outlierDetectionRatio
        {
            get
            {
                return 0.68;
            }
        }

        public static int templateMinFeatureCount
        {
            get
            {
                return 4;
            }
        }

        public static int featureGroupingSize
        {
            get
            {
                return 1;
            }
        }

        public static void Initialize()
        {

        }

       

        static Configuration()
        {
            Initialize();
            
        }
    }
}
